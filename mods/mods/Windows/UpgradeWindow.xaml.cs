﻿using System;
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
using Excel = Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;

namespace mods
{
    /// <summary>
    /// Interaction logic for UpgradeWindow.xaml
    /// </summary>
    public partial class UpgradeWindow : Window
    {
        Upgrade upgrade;

        public UpgradeWindow()
        {
            InitializeComponent();

            JobComboBox.Items.Add("");

            Trac_Mod();

            if (JobComboBox.Items.Count > 1)
            {
                JobComboBox.Items.RemoveAt(0);
            }

            JobComboBox.SelectedIndex = 0;

            SourceFile.Text = "F:\\Software\\Source\\MC-MP2\\MP2COC\\V8_06\\V8_06_1\\Mp2cocvar.ASM";

            CarType.Items.Add("Simplex");
            CarType.Items.Add("Local");
            CarType.Items.Add("Group");
            CarType.SelectedIndex = 0;
        }

        private void Upgrade_Click(object sender, RoutedEventArgs e)
        {
            ArchiveWindow aw = new ArchiveWindow(JobFile.Text);
            aw.ShowDialog();

            this.upgrade = new Upgrade(JobFile.Text);
            
            if (VersionUpgrade.IsChecked == true)
            {
                Version_Upgrade();
            }
            else
            {
                upgrade.No_Version_Upgrade();
            }
            if (DLMUpgrade.IsChecked == true)
            {
                NYC_DLM();
            }
            if (CRTLOCK.IsChecked == true)
            {
                if (CarType.SelectedItem.ToString() == "Group")
                {
                    Group_CRTLOCK();
                }
                else
                {
                    Local_CRTLOCK();
                }
            }
            if (PCHCSUpgrade.IsChecked == true)
            {
                if (CarType.SelectedItem.ToString() == "Group")
                {
                    Group_PerCarHallCallSecurity();
                }
                else
                {
                    Local_PerCarHallCallSecurity();
                }
            }
            if(Chicago.IsChecked == true)
            {
                if(CarType.SelectedItem.ToString() != "Group")
                {
                    Chicago_Fire();
                }
            }
            if (ANSI2K.IsChecked == true)
            {
                if (CarType.SelectedItem.ToString() != "Group")
                {
                    ANSI2K_Fire();
                }
                else
                {
                    ANSI2K_Fire_Group();
                }
            }
            string newFile = upgrade.Write_File();
            upgrade.Open_Files(JobFile.Text, newFile);
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
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
                JobFile.Text = filename;
            }
        }

        private void CopyComment_Click(object sender, RoutedEventArgs e)
        {
            string commentText = Generate_Comment();

            Clipboard.SetText(commentText);
        }

        private void Trac_Mod()
        {
            Excel.Application xlApp = new Excel.Application();
            Excel.Workbook xlWorkbook = xlApp.Workbooks.Open("F:\\Software\\Product\\Trac_Mod.xlsm", 0, true);
            Excel._Worksheet xlWorksheet = xlWorkbook.Sheets[1];
            Excel._Worksheet dlmWorksheet = xlWorkbook.Sheets[2];
            Excel.Range xlRange = xlWorksheet.UsedRange;
            Excel.Range dlmRange = dlmWorksheet.UsedRange;

            List<string> Usernames = new List<string>();

            string userName = "Jacob;Jake";

            while (userName.Contains(";"))
            {
                int colonIndex = userName.IndexOf(';');
                Usernames.Add(userName.Substring(0, colonIndex));
                userName = userName.Substring(colonIndex + 1, userName.Length - colonIndex - 1);
            }

            Usernames.Add(userName);

            for (int row = 4; row < 100; row++)
            {
                if (xlRange.Cells[row, 8].Value2 != null && xlRange.Cells[row, 5].Value2 != null && xlRange.Cells[row, 4].Value2 != null)
                {
                    string engineer = xlRange.Cells[row, 8].Value2.ToString();
                    string jobNumber = xlRange.Cells[row, 5].Value2.ToString();
                    string notifNumber = xlRange.Cells[row, 4].Value2.ToString();
                    foreach (string username in Usernames)
                    {
                        if (engineer.Contains(username))
                        {
                            if (jobNumber.Contains("-"))
                            {
                                int dashIndex = jobNumber.IndexOf("-");
                                jobNumber = jobNumber.Substring(dashIndex + 1, jobNumber.Length - dashIndex - 1);
                            }
                            JobComboBox.Items.Add("Job #: " + jobNumber + "\tNotification #: " + notifNumber);
                        }
                    }
                }
            }

            for (int row = 4; row < 100; row++)
            {
                if (dlmRange.Cells[row, 8].Value2 != null && dlmRange.Cells[row, 5].Value2 != null && dlmRange.Cells[row, 4].Value2 != null)
                {
                    string engineer = dlmRange.Cells[row, 8].Value2.ToString();
                    string jobNumber = dlmRange.Cells[row, 5].Value2.ToString();
                    string notifNumber = dlmRange.Cells[row, 4].Value2.ToString();
                    foreach (string username in Usernames)
                    {
                        if (engineer.Contains(username))
                        {
                            if (jobNumber.Contains("-"))
                            {
                                int dashIndex = jobNumber.IndexOf("-");
                                jobNumber = jobNumber.Substring(dashIndex + 1, jobNumber.Length - dashIndex - 1);
                            }
                            JobComboBox.Items.Add("Job #: " + jobNumber + "\tNotification #: " + notifNumber);
                        }
                    }
                }
            }

            //cleanup
            GC.Collect();
            GC.WaitForPendingFinalizers();

            //rule of thumb for releasing com objects:
            //  never use two dots, all COM objects must be referenced and released individually
            //  ex: [somthing].[something].[something] is bad

            //release com objects to fully kill excel process from running in the background
            Marshal.ReleaseComObject(xlRange);
            Marshal.ReleaseComObject(xlWorksheet);

            //close and release
            xlWorkbook.Close(false);
            Marshal.ReleaseComObject(xlWorkbook);

            //quit and release
            xlApp.Quit();
            Marshal.ReleaseComObject(xlApp);
        }

        private string Generate_Comment()
        {
            string job = JobComboBox.SelectedItem.ToString();
            int notificationIndex = job.IndexOf("Notification #");
            string notificationNumber = "";
            if (notificationIndex != -1)
            {
                notificationNumber = job.Substring(notificationIndex + 16, job.Length - notificationIndex - 16);
            }
            else
            {
                notificationNumber = "";
            }

            string commentText = "";
            DateTime date = DateTime.Now;

            //Need to make commentbox text work for multiple lines
            string boxText = CommentBox.Text;
            boxText = boxText.Replace("\r\n", "\r\n;\t\t");

            commentText = "";
            commentText += ";***************************************************************************************\n";
            commentText += "; UPDATE: " + date.ToString("MM/dd/yyyy") + "\t\tNOTIFICATION #: " + notificationNumber + "\n";
            commentText += boxText + "\n";
            commentText += ";\t\t\t\t\t\t\t\t...............Jake\n";
            commentText += ";***************************************************************************************";

            return commentText;
        }

        private void BrowseSource_Click(object sender, RoutedEventArgs e)
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
                SourceFile.Text = filename;
            }
        }

        private void Version_Upgrade()
        {
            upgrade.Version_Upgrade(SourceFile.Text,CarType.SelectedItem.ToString());

            CommentBox.Text += ";\t\t";
            CommentBox.Text += "Upgraded Software to Version \n";
        }

        public void NYC_DLM()
        {
            upgrade.Modify_Value("BOTTOM:", "10", "OR", "40H");
            upgrade.Modify_Value("LOBBY:", "17", "OR", "30H");
            upgrade.Modify_Value("LOBBY:", "26", "OR", "03H");
            upgrade.Modify_Value("LOBBY:", "31", "REPLACE", "001H");

            CommentBox.Text += ";\t\t";
            CommentBox.Text += "Enabled Options for NYC DLM\n";
        }

        private void Chicago_Fire()
        {
            upgrade.Modify_Value("LOBBY:", "14", "OR", "01H");
            upgrade.Modify_Value("LOBBY:", "18", "OR", "04H");
            upgrade.Modify_Value("LOBBY:", "15", "OR", "02H");

            if(CarType.SelectedItem.ToString() == "Local")
            {
                upgrade.Modify_Value("LOBBY:", "0A", "OR", "08H");
            }

            CommentBox.Text += ";\t\t";
            CommentBox.Text += "Enabled Options for Chicago Fire Code 2001";
        }

        private void ANSI2K_Fire()
        {
            upgrade.Modify_Value("LOBBY:", "0B", "OR", "10H");
            upgrade.Modify_Value("LOBBY:", "07", "OR", "05H");
            upgrade.Modify_Value("LOBBY:", "18", "OR", "04H");
            upgrade.Modify_Value("LOBBY:", "15", "OR", "30H");

            if (CarType.SelectedItem.ToString() == "Local")
            {
                upgrade.Modify_Value("LOBBY:", "0A", "OR", "08H");
            }

            SoftwareOption so = new SoftwareOption("ANSI 2K Options", 450, 210);

            List<string> ansi2k = new List<string>
            {
                "ANSI 2K5 Fire",
                "ANSI 2K7 Fire",
                "None of the above - Old ANSI 2K Fire"
            };

            so.Radio_Option(ansi2k);

            so.ShowDialog();

            int result = so.result;

            if (result == 0)
            {
                upgrade.Modify_Value("L_TABLE:", "1A", "REPLACE", "'A2K5',  000H,  001H,  004H,  001H");
                upgrade.Modify_Value("LOBBY:", "0A", "AND", "0BH");
            }
            else if (result == 1)
            {
                upgrade.Modify_Value("L_TABLE:", "1B", "REPLACE", "'A2K7',  000H,  001H,  004H,  001H");
                upgrade.Modify_Value("LOBBY:", "0A", "AND", "0BH");
            }
            else
            {
                //do nothing
            }

            CommentBox.Text += ";\t\t";
            CommentBox.Text += "Enabled Options for ANSI 2K Fire";
        }

        private void ANSI2K_Fire_Group()
        {
            upgrade.Modify_Value("LOBBY:", "10", "OR", "10H");
            upgrade.Modify_Value("LOBBY:", "04", "OR", "18H");

            SoftwareOption so = new SoftwareOption("ANSI 2K Options", 450, 210);

            List<string> ansi2k = new List<string>
            {
                "ANSI 2K5 Fire",
                "ANSI 2K7 Fire",
                "None of the above - Old ANSI 2K Fire"
            };

            so.Radio_Option(ansi2k);

            so.ShowDialog();

            int result = so.result;

            if (result == 0)
            {
                upgrade.Modify_Value("LOBBY:", "01", "AND", "B0H");
            }
            else if (result == 1)
            {
                upgrade.Modify_Value("LOBBY:", "01", "AND", "B0H");
            }
            else
            {
                //do nothing
            }

            CommentBox.Text += ";\t\t";
            CommentBox.Text += "Enabled Options for ANSI 2K Fire";
        }

        public void Group_CRTLOCK()
        {
            upgrade.Modify_Value("CPVAR:", "06", "OR", "01H");
            upgrade.Modify_Value("CPVAR:", "05", "OR", "80H");

            CommentBox.Text += ";\t\t";
            CommentBox.Text += "Enabled CRTLOCK Security Options\n";
        }

        public void Group_PerCarHallCallSecurity()
        {
            upgrade.Modify_Value("CPVAR:", "06", "OR", "08H");

            CommentBox.Text += ";\t\t";
            CommentBox.Text += "Enabled options for Per Car Hall Call Security\n";
        }

        public void Local_CRTLOCK()
        {
            upgrade.Modify_Value("LOBBY:", "1E", "OR", "05H");
            upgrade.Modify_Value("CPVAR:", "06", "OR", "80H");

            SoftwareOption so = new SoftwareOption("Parking Options", 450, 210);

            List<string> parkingOptions = new List<string>
            {
                "NOPKSEC - Park the car at lobby, if the parking floor is secure",
                "NOPKSEC1 - No parking, if the parking floor is secure",
                "NOPKSEC2 - No parking, when security is turned on",
                "NORMPKSEC - Park all cars as normally assigned from the group",
                "None of the above - Park all cars at the lobby"
            };

            so.Radio_Option(parkingOptions);

            so.ShowDialog();

            int result = so.result;

            if (result == 0)
            {
                upgrade.Modify_Value("LOBBY:", "1E", "OR", "02H");
            }
            else if (result == 1)
            {
                upgrade.Modify_Value("LOBBY:", "1D", "OR", "01H");
            }
            else if (result == 2)
            {
                upgrade.Modify_Value("LOBBY:", "1D", "OR", "04H");
            }
            else if (result == 3)
            {
                upgrade.Modify_Value("LOBBY:", "1D", "OR", "10H");
            }

            CommentBox.Text += ";\t\t";
            CommentBox.Text += "Enabled CRTLOCK Security Options\n";
        }

        public void Local_PerCarHallCallSecurity()
        {
            CommentBox.Text += ";\t\t";
            CommentBox.Text += "Enabled options for Per Car Hall Call Security\n";
        }

        private void Checkbox_Validator_Check(object sender, RoutedEventArgs e)
        {
            bool? security = Security.IsChecked;
            bool? crtlock = CRTLOCK.IsChecked;
            bool? pchcs = PCHCSUpgrade.IsChecked;

            if (crtlock == true)
            {
                Security.IsChecked = true;
            }

            if (pchcs == true)
            {
                Security.IsChecked = true;
                CRTLOCK.IsChecked = true;
            }
        }

        private void Checkbox_Validator_UnCheck(object sender, RoutedEventArgs e)
        {
            bool? security = Security.IsChecked;
            bool? crtlock = CRTLOCK.IsChecked;
            bool? pchcs = PCHCSUpgrade.IsChecked;

            if (crtlock == false)
            {
                PCHCSUpgrade.IsChecked = false;
            }

            if (security == false)
            {
                CRTLOCK.IsChecked = false;
                PCHCSUpgrade.IsChecked = false;
            }
        }
    }
}