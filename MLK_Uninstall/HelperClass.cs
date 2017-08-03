using System;
using System.Text;
using System.IO;
using System.Security.Principal;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace MLK_Uninstall
{
    class HelperClass
    {
        [DllImport("shell32.dll")]
        static extern bool SHGetSpecialFolderPath(IntPtr hwndOwner, [Out] StringBuilder lpszPath, int nFolder, bool fCreate);
        const int CSIDL_COMMON_STARTMENU = 0x16;  // All Users\Start Menu

        public void ShowErrorMessageDialog(string sErrorBasic, string sErrorStacktrace, string sLocation)
        {
            MessageBox.Show("Derpy is awfully sorry, but this MyLittleKaraoke Installer just encountered an error.\n" +
                "For help, see www.mylittlekaraoke.com/forum\n\n" +
                "Error description: " + sErrorBasic + "\n\nError location: " + sLocation + "\n\nExtended information:\n" + sErrorStacktrace, "Error in " + sLocation, MessageBoxButtons.OK, MessageBoxIcon.Error);

            if (Application.MessageLoop)
            {
                Application.Exit();
            }
            else
            {
                Environment.Exit(1);
            }
        }


        public Boolean SetInstallLocationInRegistryKey()
        {
            try
            {
                try { Registry.LocalMachine.DeleteSubKey(@"SOFTWARE\Classes\.mlk"); }
                catch (Exception) { }
                try { Registry.LocalMachine.DeleteSubKey(@"SOFTWARE\\DerpyMuffinsFactory"); }
                catch (Exception) { }
                try { Registry.LocalMachine.DeleteSubKey(@"SOFTWARE\Classes\derpymuffinsfactory.mlk.v1\shell\open\command"); }
                catch (Exception) { }
                try { Registry.LocalMachine.DeleteSubKey(@"SOFTWARE\Classes\derpymuffinsfactory.mlk.v1\shell\open"); }
                catch (Exception) { }
                try { Registry.LocalMachine.DeleteSubKey("SOFTWARE\\DerpyMuffinsFactory"); }
                catch (Exception) { }
                try { Registry.LocalMachine.DeleteSubKey(@"SOFTWARE\Classes\derpymuffinsfactory.mlk.v1\shell"); }
                catch (Exception) { }
                try { Registry.LocalMachine.DeleteSubKey(@"SOFTWARE\Classes\derpymuffinsfactory.mlk.v1"); }
                catch (Exception) { }
                return true;
            }
            catch (Exception ex)
            {
                //ShowErrorMessageDialog(ex.Message, ex.StackTrace, "HelperClass.SetInstallLocationInRegistryKey(string InstallPath)");
                return false;
            }

        }

        public string GetInstallLocationfromRegistryKey()
        {
            try
            {
                return (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\DerpyMuffinsFactory", "MLK Path", null);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public void RemoveStartmenuShortcut()
        {

            try
            {
                StringBuilder path = new StringBuilder(260);
                SHGetSpecialFolderPath(IntPtr.Zero, path, CSIDL_COMMON_STARTMENU, false);
                string commonStartMenuPath = path.ToString();
                string appStartMenuPath = Path.Combine(Path.Combine(commonStartMenuPath, "Programs"), "Derpy Muffins Factory");

                if (Directory.Exists(appStartMenuPath))
                    Directory.Delete(appStartMenuPath, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Was not able to remove shortcuts in Windows startmenu. " + Environment.NewLine + ex.Message, "Error removing shortcuts.", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
