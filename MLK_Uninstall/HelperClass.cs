using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Threading;
using System.Security.Permissions;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace MLK_Uninstall
{
    class HelperClass
    {
        public void ShowErrorMessageDialog(string sErrorBasic, string sErrorStacktrace, string sLocation)
        {
            MessageBox.Show("Derpy is awfully sorry, but this MyLittleKaraoke Installer just encountered an error.\n" +
                "For help, see www.mylittlekaraoke.com/forum\n\n" +
                "Error description: " + sErrorBasic + "\n\nError location: " + sLocation + "\n\nExtended information:\n" + sErrorStacktrace, "Error in " + sLocation, MessageBoxButtons.OK, MessageBoxIcon.Error);

            if (System.Windows.Forms.Application.MessageLoop)
            {
                System.Windows.Forms.Application.Exit();
            }
            else
            {
                System.Environment.Exit(1);
            };
        }


        public Boolean SetInstallLocationInRegistryKey()
        {
            try
            {
                try { Registry.LocalMachine.DeleteSubKey(@"SOFTWARE\Classes\.mlk"); }
                catch (Exception) { ;}
                try { Registry.LocalMachine.DeleteSubKey(@"SOFTWARE\\DerpyMuffinsFactory"); }
                catch (Exception) { ;}
                try { Registry.LocalMachine.DeleteSubKey(@"SOFTWARE\Classes\derpymuffinsfactory.mlk.v1\shell\open\command"); }
                catch (Exception) { ;}
                try { Registry.LocalMachine.DeleteSubKey(@"SOFTWARE\Classes\derpymuffinsfactory.mlk.v1\shell\open"); }
                catch (Exception) { ;}
                try { Registry.LocalMachine.DeleteSubKey("SOFTWARE\\DerpyMuffinsFactory"); }
                catch (Exception) { ;}
                try { Registry.LocalMachine.DeleteSubKey(@"SOFTWARE\Classes\derpymuffinsfactory.mlk.v1\shell"); }
                catch (Exception) { ;}
                try { Registry.LocalMachine.DeleteSubKey(@"SOFTWARE\Classes\derpymuffinsfactory.mlk.v1"); }
                catch (Exception) { ;}
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

        public void SetWritePermissionForLoggedInUsers(string FolderPath)
        {
            try
            {
                // This gets the "Authenticated Users" group, no matter what it's called
                IdentityReference everybodyIdentity = new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null);

                FileSystemAccessRule writerule = new FileSystemAccessRule(
                    everybodyIdentity,
                    FileSystemRights.Modify,
                    InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                    PropagationFlags.None,
                    AccessControlType.Allow);

                if (!string.IsNullOrEmpty(FolderPath) && Directory.Exists(FolderPath))
                {
                    // Get your file's ACL
                    DirectorySecurity fsecurity = Directory.GetAccessControl(FolderPath);

                    // Add the new rule to the ACL
                    fsecurity.AddAccessRule(writerule);

                    // Set the ACL back to the file
                    Directory.SetAccessControl(FolderPath, fsecurity);

                }
            }
            catch (Exception ex)
            {
                ShowErrorMessageDialog(ex.Message, ex.StackTrace, "SetWritePermissionForLoggedInUsers(string FolderPath)");
            }
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
                MessageBox.Show("Was not able to create shortcuts in Windows startmenu. " + Environment.NewLine + ex.Message, "Error creating shortcuts.", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        [DllImport("shell32.dll")]
        static extern bool SHGetSpecialFolderPath(IntPtr hwndOwner,
           [Out] StringBuilder lpszPath, int nFolder, bool fCreate);
        const int CSIDL_COMMON_STARTMENU = 0x16;  // All Users\Start Menu

        public bool IsDVDInstallation()
        {
            try
            {
                string location = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                var info = new DriveInfo(Path.GetPathRoot(location));
                return info.DriveType == DriveType.CDRom;
            }
            catch
            {
                return false;
            }
        }

        public void Run_Old_Uninstaller()
        {
            try
            {
                System.Diagnostics.Process.Start("cmd.exe", "/Q /C MsiExec.exe /x{590FE3A5-47DB-42C0-B868-D5E43F46DCBC} /passive /norestart");
                if (MessageBox.Show("Please press yes when uninstall finished successfully.", "Confirm when uninstall completed without error messages", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
                { throw new InvalidOperationException("User did not confirm successfull uninstall of old MLK"); };
            }
            catch (Exception ex)
            {
                ShowErrorMessageDialog("Was not able to correctly uninstall MLK SIM: " + ex.Message, ex.StackTrace, "Run_Old_Uninstaller");
            }
        }

        [ComImport]
        [Guid("00021401-0000-0000-C000-000000000046")]
        internal class ShellLink
        {
        }

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("000214F9-0000-0000-C000-000000000046")]
        internal interface IShellLink
        {
            void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, out IntPtr pfd, int fFlags);
            void GetIDList(out IntPtr ppidl);
            void SetIDList(IntPtr pidl);
            void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cchMaxName);
            void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
            void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);
            void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
            void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);
            void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
            void GetHotkey(out short pwHotkey);
            void SetHotkey(short wHotkey);
            void GetShowCmd(out int piShowCmd);
            void SetShowCmd(int iShowCmd);
            void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);
            void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
            void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, int dwReserved);
            void Resolve(IntPtr hwnd, int fFlags);
            void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
        }

        public static void RemoveAssociation(string Extension, string KeyName, string OpenWith, string FileDescription)
        {
            RegistryKey BaseKey;
            RegistryKey OpenMethod;
            RegistryKey Shell;
            RegistryKey CurrentUser;

            BaseKey = Registry.ClassesRoot.CreateSubKey(Extension);
            BaseKey.SetValue("", KeyName);

            OpenMethod = Registry.ClassesRoot.CreateSubKey(KeyName);
            OpenMethod.SetValue("", FileDescription);
            OpenMethod.CreateSubKey("DefaultIcon").SetValue("", "\"" + OpenWith + "\",0");
            Shell = OpenMethod.CreateSubKey("Shell");
            Shell.CreateSubKey("edit").CreateSubKey("command").SetValue("", "\"" + OpenWith + "\"" + " \"%1\"");
            Shell.CreateSubKey("open").CreateSubKey("command").SetValue("", "\"" + OpenWith + "\"" + " \"%1\"");
            BaseKey.Close();
            OpenMethod.Close();
            Shell.Close();

            CurrentUser = Registry.CurrentUser.CreateSubKey(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\.ucs");
            CurrentUser = CurrentUser.OpenSubKey("UserChoice", RegistryKeyPermissionCheck.ReadWriteSubTree, System.Security.AccessControl.RegistryRights.FullControl);
            CurrentUser.SetValue("Progid", KeyName, RegistryValueKind.String);
            CurrentUser.Close();
        }
    }
}
