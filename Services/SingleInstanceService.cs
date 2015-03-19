using CodeIDX.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace CodeIDX.Services
{
    public static class SingleInstanceService
    {
        private static readonly string ApplicationGuid = "1ED22D8A-4392-476A-A091-58398ABC3976";
        public static readonly int WM_SHOWFIRSTINSTANCE = Win32Helper.RegisterWindowMessage(string.Format("WM_SHOWFIRSTINSTANCE|{0}", ApplicationGuid));

        private static Mutex _Mutex;

        public static bool Start()
        {
            string mutexName = String.Format("Local\\{0}", ApplicationGuid);

            bool instanceCreated = false;
            _Mutex = new Mutex(true, mutexName, out instanceCreated);
            if (!instanceCreated)
            {
                Win32Helper.BroadcastMessage(WM_SHOWFIRSTINSTANCE);
                _Mutex = null;
            }

            return instanceCreated;
        }

        public static void Stop()
        {
            if (_Mutex != null)
                _Mutex.ReleaseMutex();
        }

        internal static void InitWndProc(MainWindow window)
        {
            //Attach to window messages
            HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(window).Handle);
            source.AddHook(new HwndSourceHook(WndProc));
        }

        private static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == SingleInstanceService.WM_SHOWFIRSTINSTANCE)
            {
                var window = App.Current.MainWindow;
                window.ShowInTaskbar = true;
                if (window.WindowState == WindowState.Minimized)
                    window.WindowState = WindowState.Normal;

                window.Activate();

                handled = true;
            }

            return IntPtr.Zero;
        }

    }
}
