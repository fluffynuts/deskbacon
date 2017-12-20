using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using PeanutButter.TrayIcon;
using PeanutButter.Utils;

namespace SysMenu
{
    public class Program
    {
        private static IntPtr _hookId;

        public static void Main()
        {
            var icon = new TrayIcon(Resources.pig_face_192x192);
            icon.AddMenuItem("&Exit", () => Exit(icon), null);
            icon.Show();

            HookMenus();
            Application.Run();
        }

        private static void HookMenus()
        {
            Process.GetProcesses()
                .ForEach(proc =>
                {
                    try
                    {
                        if ((proc?.MainModule?.FileName ?? "").ToLower().Contains("notepad.exe"))
                        {
                            var systemMenu = GetSystemMenu(proc.MainWindowHandle, false);
                            AppendMenu(systemMenu, MF_SEPARATOR, SYSMENU_SEPARATOR, string.Empty);
                            AppendMenu(systemMenu, MF_STRING, SYSMENU_ABOUT_ID, "&About...");
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine($"FAIL: {ex}");
                    }
                });

            _hookId = SetWindowsHookEx(HOOK_MOUSE, MouseHook,
                Marshal.GetHINSTANCE(Assembly.GetExecutingAssembly().GetModules()[0]), 0);
        }

        private static int MouseHook(int ncode, IntPtr wparam, IntPtr lparam)
        {
            Trace.WriteLine($"MouseHook: {ncode} {wparam} {lparam}");
            return CallNextHookEx(IntPtr.Zero, ncode, wparam, lparam);
        }

        private static void Exit(TrayIcon icon)
        {
            icon.Hide();
            UnhookWindowsHookEx(_hookId);
            Process.GetProcesses()
                .ForEach(proc =>
                {
                    try
                    {
                        if ((proc?.MainModule?.FileName ?? "").ToLower().Contains("notepad.exe"))
                        {
                            var systemMenu = GetSystemMenu(proc.MainWindowHandle, false);
                            RemoveMenu(systemMenu, SYSMENU_SEPARATOR, 0);
                            RemoveMenu(systemMenu, SYSMENU_ABOUT_ID, 0);
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine($"FAIL: {ex}");
                    }
                });
            Application.Exit();
        }

        private delegate int HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool AppendMenu(IntPtr hMenu, int uFlags, int uIDNewItem, string lpNewItem);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool InsertMenu(IntPtr hMenu, int uPosition, int uFlags, int uIDNewItem,
            string lpNewItem);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool RemoveMenu(IntPtr hMenu, int uPosition, int uFlags);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall,
            SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, int dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall,
            SetLastError = true)]
        private static extern int UnhookWindowsHookEx(IntPtr idHook);

        [DllImport("user32.dll")]
        protected static extern int CallNextHookEx(IntPtr hhook, int code, IntPtr wParam, IntPtr lParam);


        private const int WM_SYSCOMMAND = 0x112;
        private const int MF_STRING = 0x0;
        private const int MF_SEPARATOR = 0x800;
        private const long MF_BYCOMMAND = 0x00000000L;
        private const long MF_BYPOSITION = 0x00000400L;

        private const int HOOK_MOUSE = 14;
        private const int HOOK_KEYBOARD = 13;


        private const int SYSMENU_ABOUT_ID = 0x2;
        private const int SYSMENU_SEPARATOR = 0x1;
    }
}