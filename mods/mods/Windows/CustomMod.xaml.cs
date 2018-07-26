using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.IO;

namespace mods
{
    /// <summary>
    /// Interaction logic for CustomMod.xaml
    /// </summary>
    public partial class CustomMod : Window
    {
        string jobNumber = "";
        string grepString = "";
        string subFolder = "";

        public CustomMod(string jobNumber)
        {
            InitializeComponent();

            this.jobNumber = jobNumber;
            Find_Custom_Folder();
        }

        private void Find_Custom_Folder()
        {
            string custom1or2 = Microsoft.VisualBasic.Interaction.InputBox("Custom or Custom2?", "Custom Folder", "Custom");
            string folder = @"G:\Software\";
            if (custom1or2 == "Custom")
            {
                folder += @"Custom\MC-MP\";
                subFolder = Microsoft.VisualBasic.Interaction.InputBox("Subfolder? (MPOLTM...etc)", "SubFolder", "MPOLTM");
                folder += subFolder;
            }
            else
            {
                folder += @"Custom2\";
            }
            List<string> customFolders = Directory.GetDirectories(folder, jobNumber, SearchOption.TopDirectoryOnly).ToList();

            foreach(string customFolder in customFolders)
            {
                string custFolder = customFolder;

                if (custom1or2 == "Custom2")
                {
                    List<string> localOrGroup = Directory.GetDirectories(custFolder, "*", SearchOption.TopDirectoryOnly).ToList();
                    foreach (string localGroup in localOrGroup)
                    {
                        List<string> folders = Directory.GetDirectories(localGroup, "*", SearchOption.TopDirectoryOnly).ToList();

                        foreach (string fold in folders)
                        {
                            OldComboBox.Items.Add(fold);
                            NewComboBox.Items.Add(fold);
                        }
                    }
                }
                else
                {
                    List<string> folders = Directory.GetDirectories(customFolder, "*", SearchOption.TopDirectoryOnly).ToList();

                    foreach (string fold in folders)
                    {
                        OldComboBox.Items.Add(fold);
                        NewComboBox.Items.Add(fold);
                    }
                }
                

                if (System.Windows.Forms.MessageBox.Show("Open custom folder in VS Code?", "Open Folder?", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    var startInfo = new ProcessStartInfo();
                    startInfo.WorkingDirectory = custFolder;
                    startInfo.Arguments = "-r .";
                    startInfo.FileName = "code";

                    Process proc = Process.Start(startInfo);
                }
            }
        }

        private void Browse_Old_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".sdf";

            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();


            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
                string filename = dlg.FileName;
                OldFolder.Text = filename;
            }
        }

        private void Browse_New_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".sdf";

            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();


            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
                string filename = dlg.FileName;
                NewFolder.Text = filename;
            }
        }

        private void OldComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OldFolder.Text = OldComboBox.SelectedValue.ToString();
        }

        private void NewComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            NewFolder.Text = NewComboBox.SelectedValue.ToString();
        }

        private void FileGrep_Click(object sender, RoutedEventArgs e)
        {
            FilesListBox.Items.Clear();

            List<string> files = Directory.GetFiles(OldFolder.Text).ToList();

            grepString  = Microsoft.VisualBasic.Interaction.InputBox("Grep String?", "Grep String", jobNumber);

            foreach (string file in files)
            {
                if(ValidFile(file))
                {
                    FilesListBox.Items.Add(General.Get_File_From_Path(file));
                }
            }
        }

        private bool ValidFile(string file)
        {
            List<string> lines = File.ReadLines(file).ToList();

            bool foundString = false;

            foreach(string line in lines)
            {
                if(line.Contains(grepString))
                {
                    foundString = true;
                    break;
                }
            }

            return foundString;
        }

        private void OpenFiles_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in FilesListBox.SelectedItems)
            {
                string cmd = "C:\\Windows\\explorer.exe";
                string arg = OldFolder.Text + "\\" + FilesListBox.SelectedItem.ToString();
                Process.Start(cmd, arg);

                arg = NewFolder.Text + "\\" + FilesListBox.SelectedItem.ToString();
                Process.Start(cmd, arg);
            }
        }

        private void FilesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MatchedLinesSP.Children.Clear();

            if (FilesListBox.Items.Count > 0)
            {

                List<string> lines = File.ReadLines(OldFolder.Text + "\\" + FilesListBox.SelectedItem.ToString()).ToList();

                foreach (string line in lines)
                {
                    if (line.Contains(grepString))
                    {
                        string lineNumber = "Line " + lines.IndexOf(line) + ":";
                        lineNumber = lineNumber.PadRight(11, ' ');
                        TextBlock tb = new TextBlock
                        {
                            Text = lineNumber + "\t" + line.Trim(';').Trim()
                        };

                        if (line.ToUpper().Contains("END"))
                        {
                            tb.Text += "\n";
                        }

                        MatchedLinesSP.Children.Add(tb);
                    }
                }
            }
        }

        private void JBuild_Click(object sender, RoutedEventArgs e)
        {
            var startInfo = new ProcessStartInfo();
            startInfo.WorkingDirectory = NewFolder.Text;
            if (NewFolder.Text.Contains("Custom2"))
            {
                startInfo.Arguments = "all";
                startInfo.FileName = "jbuild";
            }
            else
            {
                startInfo.Arguments = subFolder;
                startInfo.FileName = "asmbl";
            }

            Process proc = Process.Start(startInfo);
        }

        private void EPRLNK_Click(object sender, RoutedEventArgs e)
        {
            var startInfo = new ProcessStartInfo();
            startInfo.WorkingDirectory = NewFolder.Text;
            startInfo.Arguments = Microsoft.VisualBasic.Interaction.InputBox("Arguments?", "Arguments", jobNumber + " " + subFolder);
            startInfo.FileName = "eprlnk";

            string[] args = startInfo.Arguments.Split(' ');

            Process proc = Process.Start(startInfo);
            proc.WaitForExit();

            if (args[0].ToUpper().StartsWith("G"))
            {
                System.IO.File.Copy(@"C:\EMULATION\TMPMPGRP.BIN", @"C:\EMULATION\" + args[0] + ".BIN", true);
            }
            else
            {
                System.IO.File.Copy(@"C:\EMULATION\TMPMPLCL.BIN", @"C:\EMULATION\" + args[0] + ".BIN", true);
            }
        }
    }
}
