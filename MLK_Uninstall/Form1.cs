using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace MLK_Uninstall
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                button1.Enabled = false;
                button1.Text = "Please wait.";
                Application.DoEvents();
                HelperClass cHelper = new HelperClass();
                String InstallFolderPath = "";
                if (cHelper.IsAdministrator() == false)
                {
                    // Restart program and run as admin
                    var exeName = Process.GetCurrentProcess().MainModule.FileName;
                    ProcessStartInfo startInfo = new ProcessStartInfo(exeName);
                    startInfo.Verb = "runas";
                    Process.Start(startInfo);
                    Application.Exit();
                    return;
                }

                String InstLocation = cHelper.GetInstallLocationfromRegistryKey();
                if (InstLocation != null && !InstLocation.Equals(""))
                {
                    InstallFolderPath = InstLocation;
                }
                else if (8 == IntPtr.Size || (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432"))))
                {
                    InstallFolderPath = Path.Combine(Environment.GetEnvironmentVariable("ProgramFiles(x86)"), "MyLittleKaraoke");
                }
                else
                {
                    InstallFolderPath = Path.Combine(Environment.GetEnvironmentVariable("ProgramFiles"), "MyLittleKaraoke");
                }

                if (Directory.Exists(InstallFolderPath) == false)
                    InstallFolderPath = AppDomain.CurrentDomain.BaseDirectory;

                DialogResult dialogResult = MessageBox.Show("Confirm to delete all files in " + InstallFolderPath +".\nPlease note that this will also remove all of your installed songs.", "Confirm uninstalling ponies? - Singing is Magic", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                if (dialogResult != DialogResult.OK)
                {
                    MessageBox.Show("Please simply manually delete the files and especially the \"songs\" folder (contains most of the data).", "My Little Karaoke Uninstaller");
                    Application.Exit();
                    return;
                }

                try
                {
                    Directory.Delete(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SIM4toNew"), true);
                }
                catch (Exception ex)
                { }

                try
                {
                    Directory.Delete(InstallFolderPath, true);
                }
                catch (Exception ex)
                { }

                cHelper.SetInstallLocationInRegistryKey();
                cHelper.RemoveStartmenuShortcut();
                MessageBox.Show("Uninstalling MyLittleKaraoke - Singing is Magic finished successfully.", "My Little Karaoke Uninstaller");
                Application.Exit();
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error during uninstall of MLK SIM: " + ex.Message + Environment.NewLine + Environment.NewLine + ex.StackTrace, "My Little Karaoke Uninstaller");
            }
        }
    }
}
