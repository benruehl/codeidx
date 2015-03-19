using System;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Diagnostics;
using CodeIDX.Services;

namespace CodeIDX.Helpers
{
    public static class Win32Helper
    {
        public const int HWND_BROADCAST = 0xffff;

        [DllImport("user32")]
        public static extern int RegisterWindowMessage(string message);

        [DllImport("User32.dll")]
        public static extern Int32 SendMessage(int hWnd, int Msg, int wParam, int lParam);

        public static void BroadcastMessage(int message)
        {
            SendMessage(HWND_BROADCAST, message, 0, 0);
        }

        public static bool SetStartOnStartup(bool start)
        {
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            string executingPath = Assembly.GetExecutingAssembly().Location;
            string appName = FileVersionInfo.GetVersionInfo(executingPath).ProductName;

            try
            {
                if (start)
                {
                    if (registryKey.GetValue(appName) == null)
                        registryKey.SetValue(appName, string.Format("{0} /startMinimized", executingPath));
                }
                else
                {
                    registryKey.DeleteValue(appName, false);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool OpenInVisualStudio(string file, int line)
        {
            try
            {
                var dte2 = (EnvDTE80.DTE2)System.Runtime.InteropServices.Marshal.GetActiveObject("VisualStudio.DTE");
                dte2.MainWindow.Activate();
                dte2.ItemOperations.OpenFile(file, Constants.EnvDTEConstantVsViewKindTextView);
                ((EnvDTE.TextSelection)dte2.ActiveDocument.Selection).GotoLine(line, true);

                return true;
            }
            catch (Exception ex)
            {
                ErrorProvider.Instance.LogError(ex.Message, ex);
                return false;
            }
        }
    }
}
