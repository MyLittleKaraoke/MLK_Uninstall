using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
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
                    var exeName = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                    ProcessStartInfo startInfo = new ProcessStartInfo(exeName);
                    startInfo.Verb = "runas";
                    System.Diagnostics.Process.Start(startInfo);
                    Application.Exit();
                    return;
                }
                String InstLocation = cHelper.GetInstallLocationfromRegistryKey();
                if (InstLocation != null && InstLocation.Equals("") != true)
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
                };
                if (Directory.Exists(InstallFolderPath) == false)
                    InstallFolderPath = AppDomain.CurrentDomain.BaseDirectory;

                
                try { Directory.Delete(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SIM4toNew"), true); }
                catch (Exception) { ;}
                try { Directory.Delete(InstallFolderPath, true); }
                catch (Exception) { ;}
                cHelper.SetInstallLocationInRegistryKey();
                cHelper.RemoveStartmenuShortcut();
                MessageBox.Show("Uninstalling MyLittleKaraoke - Singing is Magic finished successfully.");
                Application.Exit();
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error during uninstall of MLK SIM: " + ex.Message + Environment.NewLine + Environment.NewLine + ex.StackTrace);
            }

        }
    }
}
