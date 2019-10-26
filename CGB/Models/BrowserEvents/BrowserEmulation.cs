using Microsoft.Win32;
using System;

namespace CGB.Models.BrowserEvents
{
    public class BrowserEmulation
    {
        public static void EnsureBrowserEmulationEnabled(int emulationMode = 11001, bool uninstall = false)
        {
            try
            {
                String appname = System.Diagnostics.Process.GetCurrentProcess().ProcessName + ".exe";

                using (
                    var rk = Registry.CurrentUser.OpenSubKey(
                            @"SOFTWARE\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION", true)
                )
                {
                    if (!uninstall)
                    {
                        rk.SetValue(appname, (uint)emulationMode, RegistryValueKind.DWord);
                    }
                    else
                        rk.DeleteValue(appname);
                }
            }
            catch
            {
            }
        }

        public static void SetWebBrowserEmulation(int emulationMode = 9999)
        {
            const string BROWSER_EMULATION_KEY =
                @"Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION";

            // app.exe and app.vshost.exe
            String appname = System.Diagnostics.Process.GetCurrentProcess().ProcessName + ".exe";

            // Webpages are displayed in IE9 Standards mode, regardless of the !DOCTYPE directive.
            RegistryKey browserEmulationKey =
                Registry.CurrentUser.OpenSubKey(BROWSER_EMULATION_KEY, RegistryKeyPermissionCheck.ReadWriteSubTree) ??
                Registry.CurrentUser.CreateSubKey(BROWSER_EMULATION_KEY);

            if (browserEmulationKey != null)
            {
                browserEmulationKey.SetValue(appname, emulationMode, RegistryValueKind.DWord);
                browserEmulationKey.Close();
            }
        }
    }
}
