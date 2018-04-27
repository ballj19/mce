using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Forms;
using IWshRuntimeLibrary;

namespace ModHubInstaller
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            PathBox.Text = @"C:\ModHub";

            DesktopShortcutButton.IsChecked = true;
        }

        private void BrowseFolderButton_Click(object sender, RoutedEventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    PathBox.Text = fbd.SelectedPath + @"\ModHub";
                }
            }
        }

        private void InstallButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string versionPath = "";
                string updaterPath = @"\\mceshared\shared\Software\Utility\Software Programs and shortcuts\ModHub\ModHubAutoUpgrader\ModHubUpdater.exe";

                List<string> versions = new List<string>();
                versions = System.IO.File.ReadAllLines(@"\\amrappfil01\MCE-Rancho\Jake Ball\Versions.txt").ToList();

                foreach (string version in versions)
                {
                    if (version.StartsWith("ModHub"))
                    {
                        int colonIndex = version.IndexOf(":");
                        versionPath = version.Substring(colonIndex + 1, version.Length - colonIndex - 1);
                        int semicolonIndex = versionPath.IndexOf(";");
                        versionPath = versionPath.Substring(0, semicolonIndex);
                    }
                }

                if (Directory.Exists(PathBox.Text))
                {

                }
                else
                {
                    // Try to create the directory.
                    DirectoryInfo di = Directory.CreateDirectory(PathBox.Text);
                }

                System.IO.File.Copy(versionPath, PathBox.Text + @"\mods.exe", true);
                System.IO.File.Copy(updaterPath, PathBox.Text + @"\ModHubUpdater.exe", true);

                if (DesktopShortcutButton.IsChecked == true)
                {
                    object shDesktop = (object)"Desktop";
                    WshShell shell = new WshShell();
                    string shortcutAddress = (string)shell.SpecialFolders.Item(ref shDesktop) + @"\ModHub.lnk";
                    IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutAddress);
                    shortcut.Description = "Shortcut for ModHub";
                    shortcut.TargetPath = PathBox.Text + @"\mods.exe";
                    shortcut.Save();
                }

                System.Windows.Forms.MessageBox.Show("ModHub installed successfully");

                this.Close();
            }
            catch (Exception ex)
            {
                using (System.IO.StreamWriter writefile =
                    new System.IO.StreamWriter(@"\\amrappfil01\MCE-Rancho\Jake Ball\Error_Log.txt", true))
                {
                    DateTime now = DateTime.Now;
                    writefile.WriteLine("[" + now.ToString() + "] " + Environment.UserName);
                    writefile.WriteLine(ex.ToString() + "\n");
                }
            }
        }
    }
}
