using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.IO;
using System.ComponentModel;
using System.Windows.Threading;
using Excel = Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;
using IWshRuntimeLibrary;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace mods
{
    public partial class MainWindow : Window
    {
        bool blockSearchHistoryChange = false;
        string version = "V1.04.0";
        int permission = 1000;
        int searchProgress = 0;
        string selectedFileVersion = "";
        List<string> Trac_Mod_Jobs = new List<string>();
        List<string> Motion_Values = new List<string>();
        List<string> Motion_Options = new List<string>();
        string G_DRIVE = @"G:\";
        string file = "";
        Content content;

        public MainWindow()
        {
            InitializeComponent();

            Set_Permissions();

            this.Title = "Modification Hub by Jake Ball " + version;

            Update_Auto_Updater();

            if (Version_Check())
            {
                if (System.Windows.Forms.MessageBox.Show("There is a new version available, do you want to update?", "Update ModHub?", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    //To get the location the assembly normally resides on disk or the install directory
                    string path = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;

                    //once you have the path you get the directory with:
                    string directory = System.IO.Path.GetDirectoryName(path);

                    string updatepath = directory + "\\ModHubUpdater.exe";
                    string cmd = "C:\\Windows\\explorer.exe";
                    Process.Start(cmd, updatepath);
                    System.Windows.Application.Current.Shutdown();
                }
            }

            CustomFoldersCheckBox.IsChecked = true;
            FilesListBox.SelectionMode = SelectionMode.Extended;
            FileExtension.SelectedIndex = 0;
            Make_Controls_Invisible();
            Update_Search_History();
            Legacy_Controls_Visible();

            try
            {
                if (SearchHistory.Items[1].ToString().StartsWith("-"))
                {
                    TextBox1.Text = "";
                }
                else
                {
                    TextBox1.Text = SearchHistory.Items[1].ToString();
                }
            }
            catch
            {

            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SearchButton.IsEnabled = false;
            blockSearchHistoryChange = true;
            Update_Search_History();
            SearchFiles();
            blockSearchHistoryChange = false;
            SearchButton.IsEnabled = true;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (FilesListBox.SelectedItems.Count > 0)
            {
                foreach (var item in FilesListBox.SelectedItems)
                {
                    string cmd = "C:\\Windows\\explorer.exe";
                    string arg = "";

                    if (FilesListBox.SelectedItem.ToString().Contains(".afm"))
                    {
                        arg = FilesListBox.SelectedItem.ToString();
                    }
                    else
                    {
                        arg = G_DRIVE + "Software\\" + FilesListBox.SelectedItem.ToString();
                    }
                    Process.Start(cmd, arg);
                }
            }
            else
            {
                MessageBox.Show("Please Select an Item from the List");
            }
        }

        private void ModDocs_Click(object sender, RoutedEventArgs e)
        {
            if (TextBox1.Text == "")
            {
                MessageBox.Show("Please enter a valid job number into the search bar");
                return;
            }
            try
            {
                if (System.Windows.Forms.MessageBox.Show("Would you like to generate a file to import?", "Generate File to Import?", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    ModDoc md = new ModDoc(this.FilesListBox);
                    md.ShowDialog();
                }
                string jobNumber = "*" + TextBox1.Text + "*.afm";
                string folder = G_DRIVE + "Software\\Modification_docs";
                List<string> files = Directory.GetFiles(@folder, jobNumber).ToList();
                foreach (string file in files)
                {
                    Process.Start("C:\\Program Files\\Acro Software\\FormMax Filler\\AcroFill.exe", file);
                }

                if (files.Count < 1)
                {
                    if (System.Windows.Forms.MessageBox.Show("There is no existing modification doc. Would you like to create one?", "Create Mod Doc?", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                    {
                        string file = folder + "\\MOD_" + TextBox1.Text + ".afm";
                        System.IO.File.Copy(folder + "\\" + "Mod_Base.afm", file);
                        Process.Start("C:\\Program Files\\Acro Software\\FormMax Filler\\AcroFill.exe", file);
                    }
                }
            }
            catch
            {
                MessageBox.Show("File could not be found");
            }
        }

        private void Emulink_Click(object sender, RoutedEventArgs e)
        {
            Process proc = Process.Start("Y:\\Emulink.exe");

            proc.WaitForExit();

            string response = Microsoft.VisualBasic.Interaction.InputBox("Rename File?", "Rename File", TextBox1.Text + ".BIN");

            if (response != "")
            {
                System.IO.File.Copy(@"C:\EMULATION\TEMP.BIN", @"C:\EMULATION\" + response, true);
            }
        }

        private void Mp2link_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("Y:\\MP2Link.exe");
        }

        private void SearchFiles()
        {
            Make_Controls_Invisible();

            FilesListBox.Items.Clear();

            if (TextBox1.Text == "")
            {
                MessageBox.Show("Please enter a job number into the search bar");
                return;
            }

            searchProgress = 0;

            if (FileExtension.SelectedItem.ToString() != "Motion") //Legacy Job
            {
                if (CustomFoldersCheckBox.IsChecked == true)
                {
                    SearchProgress.Maximum = 29;
                }
                else
                {
                    SearchProgress.Maximum = 11;
                }

                if (Environment.UserName != "jacob.ball")
                {
                    using (System.IO.StreamWriter file =
                    new System.IO.StreamWriter(@"\\10.112.10.28\MCE-Rancho\Jake Ball\test.txt", true))
                    {
                        DateTime now = DateTime.Now;
                        file.WriteLine("[" + now.ToString() + "] " + this.version + " " + Environment.UserName + " " + TextBox1.Text);
                    }
                }

                string[] locations = new string[] { "MP2COC", "MP2OGM", "MPODH", "MPODT", "MPOGD", "MPOGM", "MPOLHD", "MPOLHM", "MPOLOM", "MPOLTD", "MPOLTM" };
                string[] source_locations = new string[] { "MC-MP\\MPODH", "MC-MP\\MPODT", "MC-MP\\MPOGM", "MC-MP\\MPOLHM", "MC-MP\\MPOLOM", "MC-MP\\MPOLTM", "MC-MP2\\MP2COC", "MC-MP2\\MP2OGM" };
                string[] custom_locations = new string[] { "MC-MP\\MPODH\\" + TextBox1.Text, "MC-MP\\MPODT\\" + TextBox1.Text, "MC-MP\\MPOGD\\" + TextBox1.Text, "MC-MP\\MPOGM\\" + TextBox1.Text, "MC-MP\\MPOLHD\\" + TextBox1.Text, "MC-MP\\MPOLHM\\" + TextBox1.Text, "MC-MP\\MPOLOM\\" + TextBox1.Text, "MC-MP\\MPOLTD\\" + TextBox1.Text, "MC-MP\\MPOLTM\\" + TextBox1.Text };
                string[] custom2_locations = new string[] { TextBox1.Text };

                SearchLocation(locations, "Product");
                if (CustomFoldersCheckBox.IsChecked == true)
                {
                    SearchLocation(source_locations, "Source");
                    SearchLocation(custom_locations, "Custom");
                    SearchLocation(custom2_locations, "Custom2");
                }

                if (FilesListBox.Items.Count < 1)
                {
                    JobInfo.Visibility = Visibility.Visible;
                    JobInfo.Text = "No preview available for this job.\n";
                    JobInfo.Text += "This job may be custom and under a different Job Number.\n";
                    JobInfo.Text += "Please consult the Software Department for more info on this job.";
                }

                Legacy_Controls_Visible();
            }
            else //Motion Job
            {

                string fullNumber = "";
                string jobNumber = "";
                string jobYear = "";

                if (TextBox1.Text.Contains("-"))
                {
                    jobNumber = TextBox1.Text.Substring(3, 5);
                    jobYear = "20" + TextBox1.Text.Substring(0, 2);
                    fullNumber = jobYear + "0" + jobNumber;
                }
                else
                {
                    jobNumber = TextBox1.Text.Substring(5, 5);
                    jobYear = TextBox1.Text.Substring(0, 4);
                    fullNumber = TextBox1.Text;
                }

                string searchFolder = G_DRIVE + @"Test Dept\Controller Data\" + jobYear + "\\" + fullNumber;
                string kdmSearchFolder = G_DRIVE + @"Test Dept\Controller Data\KDM\" + jobYear + "\\" + fullNumber;
                string[] files = new string[1];
                string[] kdmfiles = new string[1];

                try
                {
                    files = Directory.GetFiles(@searchFolder, "*.afm", SearchOption.AllDirectories);
                }
                catch { }
                try
                {
                    kdmfiles = Directory.GetFiles(@kdmSearchFolder, "*.afm", SearchOption.AllDirectories);
                }
                catch { }

                foreach (string file in files)
                {
                    FilesListBox.Items.Add(file);
                }
                foreach (string file in kdmfiles)
                {
                    FilesListBox.Items.Add(file);
                }

                if (permission < 2)
                {
                    Motion_Controls_Visible();
                }
            }
        }

        private int SearchLocation(string[] locations, string subfolder)
        {
            int validNum = 0;

            foreach (string location in locations)
            {
                try
                {
                    string jobNumber = "*" + TextBox1.Text + "*";

                    string folder = G_DRIVE + "Software\\" + subfolder + "\\" + location;
                    string[] files = Directory.GetFiles(@folder, jobNumber, SearchOption.AllDirectories);
                    foreach (string file in files)
                    {
                        bool validFile = false;
                        string fileExtension = General.Get_FileExtension_From_Path(file).ToLower();

                        if (FileExtension.SelectedIndex == 0)
                        {
                            if (fileExtension == ".asm" || fileExtension == "")
                            {
                                validFile = true;
                            }
                        }
                        else if (FileExtension.SelectedIndex == 1)
                        {
                            if (fileExtension.Contains(".ol"))
                            {
                                validFile = true;
                            }
                        }
                        else
                        {
                            validFile = true;
                        }
                        if (TextBox1.Text.ToUpper() != General.Get_Job_Number_From_Path(file))
                        {
                            validFile = false;
                        }

                        if (validFile)
                        {
                            int locationIndex = folder.IndexOf(subfolder);
                            string jobFile = file.Substring(locationIndex, file.Length - locationIndex);
                            FilesListBox.Items.Add(jobFile);
                            validNum++;
                        }
                    }
                }
                catch
                {

                }
                searchProgress++;
                SearchProgress.Dispatcher.Invoke(() => SearchProgress.Value = searchProgress, DispatcherPriority.Background);
            }

            return validNum;
        }

        private void ShowPrints_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string topFolder = TextBox1.Text.Substring(0, TextBox1.Text.Length - 3) + "000";
                string jobFolder = TextBox1.Text;
                string path = @"\\10.113.32.203\Jobfiles\" + topFolder + "\\" + jobFolder;
                string cmd = "C:\\Windows\\explorer.exe";
                string arg = path;
                Process.Start(cmd, arg);
            }
            catch
            {
                MessageBox.Show("Please enter a job number into the search bar");
            }
        }

        private void run_cmd(string cmd, string args)
        {
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = cmd;
            start.Arguments = args;
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            using (Process process = Process.Start(start))
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    string result = reader.ReadToEnd();
                    Console.Write(result);
                }
            }
        }

        private bool MP2COC_JobInfo(string file)
        {
            //Job Summary
            try
            {
                content = new Content(file);
                JobSummary.Text = "";
                List<string> jobSummary = content.Get_Job_Summary();
                foreach (string line in jobSummary)
                {
                    if (line.IndexOf(";") != -1)
                    {
                        JobSummary.Text += line.Substring(line.IndexOf(";") + 1, line.Length - line.IndexOf(";") - 1) + "\n";
                    }
                    else
                    {
                        JobSummary.Text += line + "\n";
                    }
                }
            }
            catch (Exception ex)
            {
                Write_Error_To_Log(file, ex);
                JobSummary.Text = "Job Summary could not be created for this file";
                Dispatcher.BeginInvoke((Action)(() => InfoTabControl.SelectedIndex = 3));
            }

            try
            {
                if (content.content.IndexOf("END") == -1)
                {
                    JobInfo.Text = "This variable file is incomplete - this job may be located in a custom folder\nor under another job number";
                    Dispatcher.BeginInvoke((Action)(() => InfoTabControl.SelectedIndex = 3));
                    DifferentJobNumber.Visibility = Visibility.Visible;
                    return false;
                }
                else
                {
                    DifferentJobNumber.Visibility = Visibility.Hidden;
                }

                DateTime lastModified = System.IO.File.GetLastWriteTime(G_DRIVE + "Software\\" + file);
                string jobName = content.Get_String("JBNAME:", 1);
                string topFloor = content.Get_Byte("BOTTOM:", 2) + 'H';
                string topFloorDecimal = (General.HexStringToDecimal(topFloor) + 1).ToString();
                string botFloor = content.Get_Byte("BOTTOM:", 1) + 'H';
                string botFloorDecimal = (General.HexStringToDecimal(botFloor) + 1).ToString();
                string falseFloors = content.Get_Bit("CPVAR", 3, 0, 3);
                string nudging = content.Get_Bit("CPVAR", 7, 0, 3);
                int i4o = General.HexStringToDecimal(content.Get_Nibble("LOBBY:", 40, 1));
                int iox = General.HexStringToDecimal(content.Get_Nibble("LOBBY:", 40, 0));
                int aiox = General.HexStringToDecimal(content.Get_Nibble("LOBBY:", 52, 0));
                int callbnu = General.HexStringToDecimal(content.Get_Nibble("LOBBY:", 41, 1));
                string rearDoor = content.Get_Bit("LOBBY:", 12, 0, 3);
                string ceBoard = content.Get_Bit("BOTTOM:", 6, 1, 1);
                string ncBoard = content.Get_Bit("LOBBY:", 38, 1, 3);
                string ftBoard = content.Get_Bit("BOTTOM:", 6, 1, 3);
                string dlmBoard = content.Get_Bit("LOBBY:", 39, 0, 1);
                string versionTop = content.Get_Comma_Separated_Byte("MPVERNUM:", 1, 0);
                string versionMid = content.Get_Comma_Separated_Byte("MPVERNUM:", 1, 1);
                string versionBot = content.Get_String("CUSTOM:", 1);
                if (versionTop[0] == '0' && versionTop.Length > 1)
                {
                    versionTop = versionTop.Substring(1, 1);
                }
                if (versionBot[0] == '0' && versionBot.Length > 1 && versionBot[1] != ' ')
                {
                    versionBot = versionBot.Substring(1, 1);
                }
                if (versionTop == "N/A")
                {
                    selectedFileVersion = "N/A";
                }
                else
                {
                    this.selectedFileVersion = versionTop + "." + versionMid + "." + versionBot;
                }
                string drivebit2 = content.Get_Bit("CPVAR", 2, 0, 1);
                string drivebit3 = content.Get_Bit("CPVAR", 2, 0, 0);
                string driveType = "";
                if (drivebit2 == "YES" && drivebit3 == "YES")
                {
                    driveType = "IMC-AC";
                }
                else if (drivebit2 == "YES")
                {
                    driveType = "IMC-MG";
                }
                else if (drivebit3 == "YES")
                {
                    driveType = "IMC-SCR";
                }
                else
                {
                    driveType = "NONE";
                }


                //Job Info
                JobInfo.Text = "";
                JobInfo.Text += file + "\n";
                JobInfo.Text += "Last Modified: " + lastModified.ToString("MM/dd/yy HH:mm:ss") + "\n\n";
                JobInfo.Text += jobName + "\n";
                JobInfo.Text += "Version: " + selectedFileVersion + "\n\n";
                JobInfo.Text += "Top Floor: " + topFloorDecimal + "\n";
                JobInfo.Text += "Bottom Floor: " + botFloorDecimal + "\n\n";
                JobInfo.Text += "Independent Rear Doors: " + rearDoor + "\n";
                JobInfo.Text += "Security: " + Security() + "\n";
                JobInfo.Text += "False Floors: " + falseFloors + "\n";
                JobInfo.Text += "Nudging: " + nudging + "\n";
                JobInfo.Text += "Drive Type: " + driveType + "\n";

                //Hardware
                JobInfo.Text += "\n";
                JobInfo.Text += "# of CALL Boards: " + callbnu + "\n";
                JobInfo.Text += "# of IOX Boards: " + iox + "\n";
                JobInfo.Text += "# of I4O Boards: " + i4o + "\n";
                JobInfo.Text += "# of AIOX Boards: " + aiox + "\n\n";
                JobInfo.Text += "CE Board: " + ceBoard + "\n";
                JobInfo.Text += "NC Board: " + ncBoard + "\n";
                JobInfo.Text += "FT Board: " + ftBoard + "\n";
                JobInfo.Text += "DLM Board: " + dlmBoard + "\n\n";
            }
            catch (Exception ex)
            {
                Write_Error_To_Log(file, ex);
                JobInfo.Text = "Job Info could not be created for this file";
            }

            //Options
            try
            {
                LobbyOptionsBlock.Text = content.Build_OptionsMap("LOBBY:");
            }
            catch (Exception ex)
            {
                Write_Error_To_Log(file, ex);
                LobbyOptionsBlock.Text = "There was an issue generating options for this file";
            }

            try
            {
                BottomOptionsBlock.Text = content.Build_OptionsMap("BOTTOM:");
            }
            catch (Exception ex)
            {
                Write_Error_To_Log(file, ex);
                BottomOptionsBlock.Text = "There was an issue generating options for this file";
            }

            //Landings
            try
            {
                Draw_Landing_Preview();
            }
            catch (Exception ex)
            {
                Write_Error_To_Log(file, ex);
            }

            //Inputs and Outputs
            try
            {
                Generate_IO();
            }
            catch (Exception ex)
            {
                Write_Error_To_Log(file, ex);
            }

            //Headers
            try
            {
                Generate_Headers();
            }
            catch (Exception ex)
            {
                Write_Error_To_Log(file, ex);
            }

            return true;
        }

        private bool MP2OGM_JobInfo(string file)
        {
            content = new Content(file);

            //Job Summary
            try
            {
                JobSummary.Text = "";
                List<string> jobSummary = content.Get_Job_Summary();
                foreach (string line in jobSummary)
                {
                    if (line.IndexOf(";") != -1)
                    {
                        JobSummary.Text += line.Substring(line.IndexOf(";") + 1, line.Length - line.IndexOf(";") - 1) + "\n";
                    }
                    else
                    {
                        JobSummary.Text += line + "\n";
                    }
                }
            }
            catch (Exception ex)
            {
                Write_Error_To_Log(file, ex);
                JobSummary.Text = "Job Summary could not be created for this file";
            }

            try
            {
                if (content.content.IndexOf("END") == -1)
                {
                    JobInfo.Text = "This variable file is incomplete - this job may be located in a custom folder\nor under another job number";
                    Dispatcher.BeginInvoke((Action)(() => InfoTabControl.SelectedIndex = 3));
                    DifferentJobNumber.Visibility = Visibility.Visible;
                    return false;
                }
                else
                {
                    DifferentJobNumber.Visibility = Visibility.Hidden;
                }

                DateTime lastModified = System.IO.File.GetLastWriteTime(G_DRIVE + "Software\\" + file);
                string jobName = content.Get_String("JBNAME:", 1);
                int iox = General.HexStringToDecimal(content.Get_Nibble("LOBBY:", 6, 0));
                int i4o = General.HexStringToDecimal(content.Get_Nibble("LOBBY:", 6, 1));
                int aiox = General.HexStringToDecimal(content.Get_Nibble("LOBBY:", 8, 0));
                int callbnu = General.HexStringToDecimal(content.Get_Nibble("LOBBY:", 7, 0));
                List<string> inputs = content.inputs;
                List<string> outputs = content.outputs;
                string versionTop = content.Get_Comma_Separated_Byte("MPVERNUM:", 1, 0);
                string versionMid = content.Get_Comma_Separated_Byte("MPVERNUM:", 1, 1);
                string versionBot = content.Get_String("CUSTOM:", 1);
                if (versionTop[0] == '0' && versionTop.Length > 1)
                {
                    versionTop = versionTop.Substring(1, 1);
                }
                if (versionBot[0] == '0' && versionBot.Length > 1 && versionBot[1] != ' ')
                {
                    versionBot = versionBot.Substring(1, 1);
                }
                if (versionTop == "N/A")
                {
                    selectedFileVersion = "N/A";
                }
                else
                {
                    this.selectedFileVersion = versionTop + "." + versionMid + "." + versionBot;
                }

                LandingLevels.Text = "";
                LandingLevels.Height = 0;
                LandingLevels.BorderThickness = new System.Windows.Thickness(0);

                LandingNormalConfig.Text = "";
                LandingNormalConfig.Height = 0;
                LandingNormalConfig.BorderThickness = new System.Windows.Thickness(0);
                LandingAltConfig.Text = "";
                LandingAltConfig.Height = 0;
                LandingAltConfig.BorderThickness = new System.Windows.Thickness(0);

                LandingNormalHeader.Visibility = Visibility.Hidden;
                LandingAltHeader.Visibility = Visibility.Hidden;

                JobInfo.Text = "";
                JobInfo.Text += file + "\n";
                JobInfo.Text += "Last Modified: " + lastModified.ToString("MM/dd/yy HH:mm:ss") + "\n\n";
                JobInfo.Text += jobName + "\n";
                JobInfo.Text += "Version: " + selectedFileVersion + "\n\n";
                JobInfo.Text += "# of Call Boards: " + callbnu + "\n";
                JobInfo.Text += "# of IOX Boards: " + iox + "\n";
                JobInfo.Text += "# of I4O Boards: " + i4o + "\n";
                JobInfo.Text += "# of AIOX Boards: " + aiox + "\n\n";

            }
            catch (Exception ex)
            {
                Write_Error_To_Log(file, ex);
                JobInfo.Text = "Job Info could not be created for this file";
            }

            //Headers
            try
            {
                Generate_Headers_Group();
            }
            catch (Exception ex)
            {
                Write_Error_To_Log(file, ex);
            }

            //IO
            try
            {
                Generate_IO(true);
            }
            catch (Exception ex)
            {
                Write_Error_To_Log(file, ex);
            }

            //Landings
            try
            {
                Draw_Group_Landing_Preview();
            }
            catch (Exception ex)
            {
                Write_Error_To_Log(file, ex);
            }

            //Options
            try
            {
                LobbyOptionsBlock.Text = content.Build_OptionsMap("LOBBY:");
            }
            catch (Exception ex)
            {
                Write_Error_To_Log(file, ex);
                LobbyOptionsBlock.Text = "There was an issue generating options for this file";
            }

            try
            {
                BottomOptionsBlock.Text = content.Build_OptionsMap("BOTTOM:");
            }
            catch (Exception ex)
            {
                Write_Error_To_Log(file, ex);
                BottomOptionsBlock.Text = "There was an issue generating options for this file";
            }

            return true;
        }

        private bool Motion_JobInfo()
        {
            MotionContent content = new MotionContent(file);

            content.Draw_Landing_Preview();
            content.Generate_Job_Info();

            return true;
        }

        private bool Generate_JobInfo(string file)
        {
            this.file = file;
            if (file.Contains(".afm"))
            {
                try
                {
                    return Motion_JobInfo();
                }
                catch (Exception ex)
                {
                    Write_Error_To_Log(file, ex);
                    JobInfo.Text = "Job Info could not be generated for this file.";
                    return false;
                }
            }
            else if (file.Contains("MP2OGM") || file.Contains("MPOGM") || file.Contains("MPOGD"))
            {

                try
                {
                    return MP2OGM_JobInfo(file);
                }
                catch (Exception ex)
                {
                    Write_Error_To_Log(file, ex);
                    JobInfo.Text = "Job Info could not be generated for this file.";
                    return false;
                }
            }
            else
            {
                try
                {
                    return MP2COC_JobInfo(file);
                }
                catch (Exception ex)
                {
                    Write_Error_To_Log(file, ex);
                    JobInfo.Text = "Job Info could not be generated for this file.";
                    return false;
                }
            }
        }

        private void FilesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var addedItem = e.AddedItems;

            if (addedItem.Count == 1)
            {
                string file = addedItem[0].ToString();

                if (Generate_JobInfo(file))
                {
                    Make_Controls_Visible();
                }
                else
                {
                    Make_Controls_Invisible();
                    JobInfo.Visibility = Visibility.Visible;
                }
            }

            ViewVersionIO.Dispatcher.Invoke(() => ViewVersionIO.Content = "Version I/O", DispatcherPriority.Background);
        }

        private void OpenSim_Click(object sender, RoutedEventArgs e)
        {
            string message = "";

            foreach (var item in FilesListBox.SelectedItems)
            {
                if (item.ToString().Contains("MP2OGM") || item.ToString().Contains("MPOGD") || item.ToString().Contains("MPOGM"))
                {
                    message += "Group files not supported\n";
                }
                else
                {
                    MessageBox.Show("Please select a template file for your simulator");

                    // Create OpenFileDialog 
                    Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

                    dlg.InitialDirectory = @"C:\Simulator\";

                    // Set filter for file extension and default file extension 
                    dlg.DefaultExt = ".sdf";

                    // Display OpenFileDialog by calling ShowDialog method 
                    Nullable<bool> result = dlg.ShowDialog();

                    string templateFile = "";

                    // Get the selected file name and display in a TextBox 
                    if (result == true)
                    {
                        // Open document 
                        string filename = dlg.FileName;
                        templateFile = filename;

                        Simulator sim = new Simulator(item.ToString(), templateFile);
                        message += "File Created: " + sim.Write_File() + "\n";
                    }
                }
            }

            MessageBox.Show(message);
        }

        public void Update_Search_History()
        {
            List<string> tempSearchHistory = new List<string>();

            int i = 0;
            foreach (string search in Properties.Settings.Default.SearchHistory)
            {
                tempSearchHistory.Add(search);
                i++;
                if (i == 3)
                {
                    break;
                }
            }

            tempSearchHistory.Reverse();  //We need to reverse the list so the Add/Remove functions
                                          //behave as we want. We then reverse it back at the end.
            if (TextBox1.Text != "")
            {
                if (tempSearchHistory.Contains(TextBox1.Text))
                {

                    tempSearchHistory.Remove(TextBox1.Text);
                    tempSearchHistory.Add(TextBox1.Text);
                }
                else
                {
                    if (tempSearchHistory.Count >= 5)
                    {
                        tempSearchHistory.Remove(tempSearchHistory[0]);
                    }
                    tempSearchHistory.Add(TextBox1.Text);
                }
            }

            tempSearchHistory.Reverse();

            Properties.Settings.Default.SearchHistory.Clear();
            SearchHistory.Items.Clear();
            SearchHistory.Items.Add("----- Recents -----");

            foreach (string search in tempSearchHistory)
            {
                Properties.Settings.Default.SearchHistory.Add(search);
                SearchHistory.Items.Add(search);
            }

            foreach (string job in Trac_Mod_Jobs)
            {
                SearchHistory.Items.Add(job);
            }
        }

        private void SearchHistory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!blockSearchHistoryChange)
            {
                if (SearchHistory.SelectedValue.ToString().StartsWith("-"))
                {
                    TextBox1.Text = ""; //The user selected a title instead of a job number
                }
                else
                {
                    TextBox1.Text = SearchHistory.SelectedValue.ToString();
                }
            }
        }

        private string Security()
        {
            string security = "";

            bool BSI = false;
            bool SECRTY = false;
            bool CRTLOK = false;
            bool SECUR = false;
            bool NEWSECRTY = false;

            foreach (string input in content.inputs)
            {
                if (input == "BSI")
                {
                    BSI = true;
                }
            }

            if (content.Get_Bit("LOBBY:", 31, 0, 0) == "YES")
            {
                NEWSECRTY = true;
            }

            if (content.Get_Bit("LOBBY:", 31, 0, 1) == "YES")
            {
                CRTLOK = true;
            }

            if (content.Get_Bit("LOBBY:", 31, 0, 3) == "YES")
            {
                SECRTY = true;
            }

            if (content.Get_Bit("CPVAR", 7, 1, 0) == "YES")
            {
                SECUR = true;
            }

            if (BSI)
            {
                security += "BSI";
            }

            if (SECRTY && CRTLOK && SECUR)
            {
                if (security != "")
                {
                    security += ", ";
                }

                security += "CRTLOCK";
            }

            if (NEWSECRTY)
            {
                if (security != "")
                {
                    security += ", ";
                }

                security += "ACE";
            }

            if (security == "")
            {
                return "NO";
            }
            else
            {
                security = "YES - " + security;
            }

            return security;
        }

        private void Draw_Landing_Preview()
        {
            LandingNormalHeader.Width = 96;
            LandingNormalConfig.Width = 96;
            LandingAltHeader.Width = 96;
            LandingAltConfig.Width = 96;

            LandingNormalConfig.Text = "";
            LandingNormalConfig.Height = 0;
            LandingNormalConfig.BorderThickness = new System.Windows.Thickness(0);
            LandingAltConfig.Text = "";
            LandingAltConfig.Height = 0;
            LandingAltConfig.BorderThickness = new System.Windows.Thickness(0);

            LandingAltHeader.Visibility = Visibility.Hidden;

            int top_landing = General.HexStringToDecimal(content.Get_Byte("BOTTOM:", 2)) + 1;
            string isFalseFloors = content.Get_Bit("CPVAR", 3, 0, 3);

            List<string> piLabels = content.Get_PILabels();

            string front = "False";
            string rear = "False";

            LandingLevels.Text = "";
            LandingLevels.Height = 16 * top_landing + 10;
            LandingLevels.BorderThickness = new System.Windows.Thickness(2);
            LandingPIs.Text = "";
            LandingPIs.Height = 16 * top_landing + 10;
            LandingPIs.BorderThickness = new System.Windows.Thickness(2);

            LandingNormalConfig.Text = "";
            LandingNormalConfig.Height = 16 * top_landing + 10;
            LandingNormalConfig.BorderThickness = new System.Windows.Thickness(2);

            List<int> falseFloors = new List<int>();
            List<int> nonFalseFloors = new List<int>();

            if (isFalseFloors == "YES")
            {
                int pix_tableIndex = content.content.IndexOf("PIX_TABLE:");
                int x = 1;

                while (content.content[pix_tableIndex + x].StartsWith("DB") && content.Get_Byte("PIX_TABLE:", x) != "7F")
                {
                    string floorHex = content.Get_Byte("PIX_TABLE:", x);
                    string floorBinary = General.HexStringToBinary(floorHex);
                    int floorDec = General.HexStringToDecimal(floorHex) + 1;
                    if (floorBinary[0] == '0') //If False Floor
                    {
                        falseFloors.Add(floorDec);
                    }
                    else //Non False Floor
                    {
                        nonFalseFloors.Add(floorDec - 128);
                    }
                    x++;
                }
            }

            for (int f = top_landing; f >= 1; f--)
            {
                if (content.Get_Bit("ELIGIV:", f, 0, 3) == "YES")
                {
                    front = "F";
                }
                else
                {
                    if (falseFloors.Contains(f))
                    {
                        front = " X";
                    }
                    else
                    {
                        front = ".";
                    }
                }

                if (content.Get_Bit("ELIGIV:", f, 0, 2) == "YES")
                {
                    rear = "R";
                }
                else
                {
                    if (falseFloors.Contains(f))
                    {
                        rear = "";
                    }
                    else
                    {
                        rear = ".";
                    }
                }

                LandingPIs.Text += piLabels[f - 1] + "\n";
                LandingLevels.Text += f + "\n";
                LandingNormalConfig.Text += front + " " + rear + "\n";
            }

            bool isAltInput = false;
            if (content.content.IndexOf("INELIG:") != -1)
            {
                //INELIG: System Input Eligibility Map
                List<string> inelig = content.IO(new List<string> { "INELIG" });
                foreach (string input in inelig)
                {
                    if (input == "ALT")
                    {
                        isAltInput = true;
                    }
                }
            }

            foreach (string input in content.inputs)
            {
                if (input == "ALT")
                {
                    isAltInput = true;
                }
            }


            if (isAltInput)
            {
                if (FilesListBox.SelectedItems.Count > 0) //This is to prevent this from being visible before a file is selected
                {
                    LandingAltHeader.Visibility = Visibility.Visible;
                    LandingAltConfig.Visibility = Visibility.Visible;
                }

                LandingAltConfig.Text = "";
                LandingAltConfig.Height = 16 * top_landing + 10;
                LandingAltConfig.BorderThickness = new System.Windows.Thickness(2);

                for (int f = top_landing; f >= 1; f--)
                {

                    if (content.Get_Bit("ALTMP:", f, 0, 3) == "YES")
                    {
                        front = "F";
                    }
                    else
                    {
                        if (falseFloors.Contains(f))
                        {
                            front = " X";
                        }
                        else
                        {
                            front = ".";
                        }
                    }

                    if (content.Get_Bit("ALTMP:", f, 0, 2) == "YES")
                    {
                        rear = "R";
                    }
                    else
                    {
                        if (falseFloors.Contains(f))
                        {
                            rear = "";
                        }
                        else
                        {
                            rear = ".";
                        }
                    }
                    LandingAltConfig.Text += front + " " + rear + "\n";
                }
            }

            //Remove Last new line character from each column
            LandingPIs.Text = LandingPIs.Text.Substring(0, LandingPIs.Text.Length - 1);
            LandingLevels.Text = LandingLevels.Text.Substring(0, LandingLevels.Text.Length - 1);
            LandingNormalConfig.Text = LandingNormalConfig.Text.Substring(0, LandingNormalConfig.Text.Length - 1);
            LandingAltConfig.Text = LandingAltConfig.Text.Substring(0, LandingAltConfig.Text.Length - 1);
        }

        private void Draw_Group_Landing_Preview()
        {
            int group_top_landing = content.Get_Group_Top_Level();
            List<string> piLabels = content.Get_PILabels();
            string front = "False";
            string rear = "False";
            string tab = "";

            LandingLevels.Text = "";
            LandingLevels.Height = 16 * group_top_landing + 26;
            LandingLevels.BorderThickness = new System.Windows.Thickness(2);
            LandingPIs.Text = "";
            LandingPIs.Height = 16 * group_top_landing + 26;
            LandingPIs.BorderThickness = new System.Windows.Thickness(2);

            LandingNormalConfig.Text = "";
            LandingNormalConfig.Height = 16 * group_top_landing + 26;
            LandingNormalConfig.BorderThickness = new System.Windows.Thickness(2);

            LandingAltConfig.Text = "";
            LandingAltConfig.Height = 16 * group_top_landing + 26;
            LandingAltConfig.BorderThickness = new System.Windows.Thickness(2);

            LandingAltHeader.Visibility = Visibility.Hidden;
            LandingAltConfig.Visibility = Visibility.Hidden;

            string[] cars = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L" };

            int number_of_cars = Int32.Parse(content.Get_Byte("LOBBY:", 18));

            LandingNormalHeader.Width = 48 + 48 * number_of_cars;
            LandingNormalConfig.Width = 48 + 48 * number_of_cars;

            LandingLevels.Text += "Car\n";
            LandingPIs.Text += "Car\n";

            for (int c = 0; c < number_of_cars; c++)
            {
                if (c < number_of_cars - 1)
                {
                    tab = "\t";
                }
                else
                {
                    tab = "";
                }
                LandingNormalConfig.Text += cars[c] + tab;
            }

            LandingNormalConfig.Text += "\n";

            for (int x = group_top_landing; x >= 1; x--)
            {
                for (int c = 0; c < number_of_cars; c++)
                {
                    if (content.Get_Bit("ELIGIV" + cars[c], x, 1, 1) == "YES" || content.Get_Bit("ELIGIV" + cars[c], x, 1, 3) == "YES" || content.Get_Bit("ELIGIV" + cars[c], x, 0, 1) == "YES" || content.Get_Bit("ELIGIV" + cars[c], x, 0, 3) == "YES")
                    {
                        front = "F";
                    }
                    else
                    {
                        front = ".";
                    }

                    if (content.Get_Bit("ELIGIV" + cars[c], x, 1, 0) == "YES" || content.Get_Bit("ELIGIV" + cars[c], x, 1, 2) == "YES" || content.Get_Bit("ELIGIV" + cars[c], x, 0, 0) == "YES" || content.Get_Bit("ELIGIV" + cars[c], x, 0, 2) == "YES")
                    {
                        rear = "R";
                    }
                    else
                    {
                        rear = ".";
                    }
                    if (c < number_of_cars - 1)
                    {
                        tab = "\t";
                    }
                    else
                    {
                        tab = "";
                    }
                    LandingNormalConfig.Text += front + " " + rear + tab;
                }
                LandingNormalConfig.Text += "\n";
                LandingLevels.Text += x + "\n";
                LandingPIs.Text += piLabels[x - 1] + "\n";
            }

            //Remove Last new line character from each column
            LandingPIs.Text = LandingPIs.Text.Substring(0, LandingPIs.Text.Length - 1);
            LandingLevels.Text = LandingLevels.Text.Substring(0, LandingLevels.Text.Length - 1);
            LandingNormalConfig.Text = LandingNormalConfig.Text.Substring(0, LandingNormalConfig.Text.Length - 1);
        }

        private void Generate_Headers()
        {
            HeaderSP.Children.Clear();

            List<string> calls = new List<string>();

            string file = content.file;
            string ncBoard = content.Get_Bit("LOBBY:", 38, 1, 3);

            if (ncBoard == "NO") //Exclude ELIGI: if NC board is set
            {
                //ELIGI: Front Car Calls
                for (int x = 0; x < 8; x++)
                {
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("ELIGI:", x + 1, 0, b) == "YES")
                        {
                            int callNum = 100 + x * 8 + (3 - b) + 1;
                            calls.Add(callNum.ToString());
                        }
                    }
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("ELIGI:", x + 1, 1, b) == "YES")
                        {
                            int callNum = 100 + x * 8 + (3 - b) + 5;
                            calls.Add(callNum.ToString());
                        }
                    }
                }

                //ELIGI: Rear Car Calls
                for (int x = 0; x < 8; x++)
                {
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("ELIGI:", x + 9, 0, b) == "YES")
                        {
                            int callNum = 100 + x * 8 + (3 - b) + 1;
                            calls.Add(callNum.ToString() + "R");
                        }
                    }
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("ELIGI:", x + 9, 1, b) == "YES")
                        {
                            int callNum = 100 + x * 8 + (3 - b) + 5;
                            calls.Add(callNum.ToString() + "R");
                        }
                    }
                }

                //ELIGI: Front Down Hall Calls
                for (int x = 0; x < 8; x++)
                {
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("ELIGI:", x + 17, 0, b) == "YES")
                        {
                            int callNum = 500 + x * 8 + (3 - b) + 1;
                            calls.Add(callNum.ToString());
                        }
                    }
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("ELIGI:", x + 17, 1, b) == "YES")
                        {
                            int callNum = 500 + x * 8 + (3 - b) + 5;
                            calls.Add(callNum.ToString());
                        }
                    }
                }

                //ELIGI: Rear Down Hall Calls
                for (int x = 0; x < 8; x++)
                {
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("ELIGI:", x + 25, 0, b) == "YES")
                        {
                            int callNum = 500 + x * 8 + (3 - b) + 1;
                            calls.Add(callNum.ToString() + "R");
                        }
                    }
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("ELIGI:", x + 25, 1, b) == "YES")
                        {
                            int callNum = 500 + x * 8 + (3 - b) + 5;
                            calls.Add(callNum.ToString() + "R");
                        }
                    }
                }

                //ELIGI: Front Up Hall Calls
                for (int x = 0; x < 8; x++)
                {
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("ELIGI:", x + 33, 0, b) == "YES")
                        {
                            int callNum = 600 + x * 8 + (3 - b) + 1;
                            calls.Add(callNum.ToString());
                        }
                    }
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("ELIGI:", x + 33, 1, b) == "YES")
                        {
                            int callNum = 600 + x * 8 + (3 - b) + 5;
                            calls.Add(callNum.ToString());
                        }
                    }
                }

                //ELIGI: Rear Up Hall Calls
                for (int x = 0; x < 8; x++)
                {
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("ELIGI:", x + 41, 0, b) == "YES")
                        {
                            int callNum = 600 + x * 8 + (3 - b) + 1;
                            calls.Add(callNum.ToString() + "R");
                        }
                    }
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("ELIGI:", x + 41, 1, b) == "YES")
                        {
                            int callNum = 600 + x * 8 + (3 - b) + 5;
                            calls.Add(callNum.ToString() + "R");
                        }
                    }
                }
            }

            if (content.content.IndexOf("XELIGI:") != -1)
            {
                //XELIGI: Front Car Calls
                for (int x = 0; x < 8; x++)
                {
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("XELIGI:", x + 1, 0, b) == "YES")
                        {
                            int callNum = 100 + x * 8 + (3 - b) + 1;
                            calls.Add(callNum.ToString() + "X");
                        }
                    }
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("XELIGI:", x + 1, 1, b) == "YES")
                        {
                            int callNum = 100 + x * 8 + (3 - b) + 5;
                            calls.Add(callNum.ToString() + "X");
                        }
                    }
                }

                //XELIGI: Rear Car Calls
                for (int x = 0; x < 8; x++)
                {
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("XELIGI:", x + 9, 0, b) == "YES")
                        {
                            int callNum = 100 + x * 8 + (3 - b) + 1;
                            calls.Add(callNum.ToString() + "R" + "X");
                        }
                    }
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("XELIGI:", x + 9, 1, b) == "YES")
                        {
                            int callNum = 100 + x * 8 + (3 - b) + 5;
                            calls.Add(callNum.ToString() + "R" + "X");
                        }
                    }
                }

                //XELIGI: Front Down Hall Calls
                for (int x = 0; x < 8; x++)
                {
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("XELIGI:", x + 17, 0, b) == "YES")
                        {
                            int callNum = 500 + x * 8 + (3 - b) + 1;
                            calls.Add(callNum.ToString() + "X");
                        }
                    }
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("XELIGI:", x + 17, 1, b) == "YES")
                        {
                            int callNum = 500 + x * 8 + (3 - b) + 5;
                            calls.Add(callNum.ToString() + "X");
                        }
                    }
                }

                //XELIGI: Rear Down Hall Calls
                for (int x = 0; x < 8; x++)
                {
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("XELIGI:", x + 25, 0, b) == "YES")
                        {
                            int callNum = 500 + x * 8 + (3 - b) + 1;
                            calls.Add(callNum.ToString() + "R" + "X");
                        }
                    }
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("XELIGI:", x + 25, 1, b) == "YES")
                        {
                            int callNum = 500 + x * 8 + (3 - b) + 5;
                            calls.Add(callNum.ToString() + "R" + "X");
                        }
                    }
                }

                //XELIGI: Front Up Hall Calls
                for (int x = 0; x < 8; x++)
                {
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("XELIGI:", x + 33, 0, b) == "YES")
                        {
                            int callNum = 600 + x * 8 + (3 - b) + 1;
                            calls.Add(callNum.ToString() + "X");
                        }
                    }
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("XELIGI:", x + 33, 1, b) == "YES")
                        {
                            int callNum = 600 + x * 8 + (3 - b) + 5;
                            calls.Add(callNum.ToString() + "X");
                        }
                    }
                }

                //XELIGI: Rear Up Hall Calls
                for (int x = 0; x < 8; x++)
                {
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("XELIGI:", x + 41, 0, b) == "YES")
                        {
                            int callNum = 600 + x * 8 + (3 - b) + 1;
                            calls.Add(callNum.ToString() + "R" + "X");
                        }
                    }
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("XELIGI:", x + 41, 1, b) == "YES")
                        {
                            int callNum = 600 + x * 8 + (3 - b) + 5;
                            calls.Add(callNum.ToString() + "R" + "X");
                        }
                    }
                }
            }

            if (content.content.IndexOf("HELIGI:") != -1)
            {
                //HELIGI: Front Car Calls
                for (int x = 0; x < 8; x++)
                {
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("HELIGI:", x + 1, 0, b) == "YES")
                        {
                            int callNum = x * 8 + (3 - b) + 1;
                            calls.Add("EC" + callNum.ToString());
                        }
                    }
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("HELIGI:", x + 1, 1, b) == "YES")
                        {
                            int callNum = x * 8 + (3 - b) + 5;
                            calls.Add("EC" + callNum.ToString());
                        }
                    }
                }

                //HELIGI: Rear Car Calls
                for (int x = 0; x < 8; x++)
                {
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("HELIGI:", x + 9, 0, b) == "YES")
                        {
                            int callNum = x * 8 + (3 - b) + 1;
                            calls.Add("EC" + callNum.ToString() + "R");
                        }
                    }
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("HELIGI:", x + 9, 1, b) == "YES")
                        {
                            int callNum = x * 8 + (3 - b) + 5;
                            calls.Add("EC" + callNum.ToString() + "R");
                        }
                    }
                }
            }

            if (content.content.IndexOf("INELIG:") != -1)
            {
                //INELIG: System Input Eligibility Map
                List<string> inelig = content.IO(new List<string> { "INELIG" });
                foreach (string input in inelig)
                {
                    calls.Add(input);
                }
            }

            if (content.content.IndexOf("FSECUR") != -1)
            {


                //FSECUR: Per Opening Security Input Eligibility Map
                for (int x = 0; x < 8; x++)
                {
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("FSECUR:", x + 1, 0, b) == "YES")
                        {
                            int callNum = x * 8 + (3 - b) + 1;
                            calls.Add("S" + callNum.ToString());
                        }
                    }
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("FSECUR:", x + 1, 1, b) == "YES")
                        {
                            int callNum = x * 8 + (3 - b) + 5;
                            calls.Add("S" + callNum.ToString());
                        }
                    }
                }
            }

            if (content.content.IndexOf("RSECUR:") != -1)
            {
                //RSECUR: Per Opening Security Input Eligibility Map
                for (int x = 0; x < 8; x++)
                {
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("RSECUR:", x + 1, 0, b) == "YES")
                        {
                            int callNum = x * 8 + (3 - b) + 1;
                            calls.Add("S" + callNum.ToString() + "R");
                        }
                    }
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("RSECUR:", x + 1, 1, b) == "YES")
                        {
                            int callNum = x * 8 + (3 - b) + 5;
                            calls.Add("S" + callNum.ToString() + "R");
                        }
                    }
                }
            }

            if (content.content.IndexOf("CARDRF:") != -1)
            {
                //CARDRF: Front Card Reader Calls
                for (int x = 0; x < 8; x++)
                {
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("CARDRF:", x + 1, 0, b) == "YES")
                        {
                            int callNum = x * 8 + (3 - b) + 1;
                            calls.Add("CR" + callNum.ToString());
                        }
                    }
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("CARDRF:", x + 1, 1, b) == "YES")
                        {
                            int callNum = x * 8 + (3 - b) + 5;
                            calls.Add("CR" + callNum.ToString());
                        }
                    }
                }
            }

            if (content.content.IndexOf("CARDRR:") != -1)
            {
                //CARDRR: Rear Card Reader Calls
                for (int x = 0; x < 8; x++)
                {
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("CARDRR:", x + 1, 0, b) == "YES")
                        {
                            int callNum = x * 8 + (3 - b) + 1;
                            calls.Add("CR" + callNum.ToString() + "R");
                        }
                    }
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("CARDRR:", x + 1, 1, b) == "YES")
                        {
                            int callNum = x * 8 + (3 - b) + 5;
                            calls.Add("CR" + callNum.ToString() + "R");
                        }
                    }
                }
            }

            //Add to Headers Tab
            int numOfCalls = calls.Count;
            int column = 0;
            do
            {
                if (numOfCalls < 16)
                {
                    for (int x = 16 - numOfCalls; x > 0; x--)
                    {
                        calls.Add("N/C");
                    }
                }

                StackPanel sp = new StackPanel { Orientation = Orientation.Vertical, Name = ("Column" + column), Margin = new Thickness(10, 15, 10, 0) };
                for (int x = 15; x >= 0; x--)
                {
                    Thickness margin = new Thickness(0, -2, 0, 0);
                    if (x == 7)
                    {
                        margin = new Thickness(0, 0, 0, 0);
                    }
                    sp.Children.Add(
                        new TextBox
                        {
                            Text = calls[column * 16 + x],
                            Width = 50,
                            Height = 25,
                            BorderThickness = new Thickness(2),
                            BorderBrush = System.Windows.Media.Brushes.Black,
                            IsReadOnly = true,
                            Background = System.Windows.Media.Brushes.Transparent,
                            TextAlignment = TextAlignment.Center,
                            Margin = margin
                        });
                    numOfCalls--;
                }
                column++;

                HeaderSP.Children.Add(sp);
            } while (numOfCalls > 0);
        }

        private void Generate_Headers_Group()
        {
            HeaderSP.Children.Clear();

            List<string> calls = new List<string>();

            //ELIGI: Down Hall Calls Front
            for (int x = 0; x < 8; x++)
            {
                for (int n = 0; n < 2; n++)
                {
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("ELIGI:", x + 1, n, b) == "YES")
                        {
                            int callNum = 500 + x * 8 + (3 - b) + 1 + n * 4;
                            calls.Add(callNum.ToString());
                        }
                    }
                }
            }

            //ELIGI: Down Hall Calls Rear
            for (int x = 0; x < 8; x++)
            {
                for (int n = 0; n < 2; n++)
                {
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("ELIGI:", x + 9, n, b) == "YES")
                        {
                            int callNum = 500 + x * 8 + (3 - b) + 1 + n * 4;
                            calls.Add(callNum.ToString() + "R");
                        }
                    }
                }
            }

            //ELIGI: Up Hall Calls Front
            for (int x = 0; x < 8; x++)
            {
                for (int n = 0; n < 2; n++)
                {
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("ELIGI:", x + 17, n, b) == "YES")
                        {
                            int callNum = 600 + x * 8 + (3 - b) + 1 + n * 4;
                            calls.Add(callNum.ToString());
                        }
                    }
                }
            }

            //ELIGI: Up Hall Calls Rear
            for (int x = 0; x < 8; x++)
            {
                for (int n = 0; n < 2; n++)
                {
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("ELIGI:", x + 25, n, b) == "YES")
                        {
                            int callNum = 600 + x * 8 + (3 - b) + 1 + n * 4;
                            calls.Add(callNum.ToString() + "R");
                        }
                    }
                }
            }

            //AELIGI: Aux Down Hall Calls Front
            for (int x = 0; x < 8; x++)
            {
                for (int n = 0; n < 2; n++)
                {
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("AELIGI:", x + 1, n, b) == "YES")
                        {
                            int callNum = 500 + x * 8 + (3 - b) + 1 + n * 4;
                            calls.Add(callNum.ToString() + "X");
                        }
                    }
                }
            }

            //AELIGI: Aux Down Hall Calls Rear
            for (int x = 0; x < 8; x++)
            {
                for (int n = 0; n < 2; n++)
                {
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("AELIGI:", x + 9, n, b) == "YES")
                        {
                            int callNum = 500 + x * 8 + (3 - b) + 1 + n * 4;
                            calls.Add(callNum.ToString() + "RX");
                        }
                    }
                }
            }

            //AELIGI: Aux Up Hall Calls Front
            for (int x = 0; x < 8; x++)
            {
                for (int n = 0; n < 2; n++)
                {
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("AELIGI:", x + 17, n, b) == "YES")
                        {
                            int callNum = 600 + x * 8 + (3 - b) + 1 + n * 4;
                            calls.Add(callNum.ToString() + "X");
                        }
                    }
                }
            }

            //AELIGI: Aux Up Hall Calls Rear
            for (int x = 0; x < 8; x++)
            {
                for (int n = 0; n < 2; n++)
                {
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("AELIGI:", x + 25, n, b) == "YES")
                        {
                            int callNum = 600 + x * 8 + (3 - b) + 1 + n * 4;
                            calls.Add(callNum.ToString() + "RX");
                        }
                    }
                }
            }

            //HELIGI: Hospital Calls Front
            for (int x = 0; x < 8; x++)
            {
                for (int n = 0; n < 2; n++)
                {
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("HELIGI:", x + 1, n, b) == "YES")
                        {
                            int callNum = x * 8 + (3 - b) + 1 + n * 4;
                            calls.Add("EC" + callNum.ToString());
                        }
                    }
                }
            }

            //HELIGI: Hospital Calls Rear
            for (int x = 0; x < 8; x++)
            {
                for (int n = 0; n < 2; n++)
                {
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("HELIGI:", x + 9, n, b) == "YES")
                        {
                            int callNum = x * 8 + (3 - b) + 1 + n * 4;
                            calls.Add("EC" + callNum.ToString() + "R");
                        }
                    }
                }
            }

            //AHELIGI: Hospital Calls Front
            for (int x = 0; x < 8; x++)
            {
                for (int n = 0; n < 2; n++)
                {
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("AHELIGI:", x + 1, n, b) == "YES")
                        {
                            int callNum = x * 8 + (3 - b) + 1 + n * 4;
                            calls.Add("EC" + callNum.ToString() + "X");
                        }
                    }
                }
            }

            //AHELIGI: Hospital Calls Rear
            for (int x = 0; x < 8; x++)
            {
                for (int n = 0; n < 2; n++)
                {
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("AHELIGI:", x + 9, n, b) == "YES")
                        {
                            int callNum = x * 8 + (3 - b) + 1 + n * 4;
                            calls.Add("EC" + callNum.ToString() + "RX");
                        }
                    }
                }
            }

            if (content.content.IndexOf("CIOINE:") != -1)
            {
                List<string> ioLabels = new List<string> { "CIOINE" };
                List<string> cioine = content.IO(ioLabels);
                foreach (string input in cioine)
                {
                    calls.Add(input);
                }
            }

            //Add to Headers Tab
            int numOfCalls = calls.Count;
            int column = 0;
            do
            {
                if (numOfCalls < 16)
                {
                    for (int x = 16 - numOfCalls; x > 0; x--)
                    {
                        calls.Add("N/C");
                    }
                }

                StackPanel sp = new StackPanel { Orientation = Orientation.Vertical, Name = ("Column" + column), Margin = new Thickness(10, 15, 10, 0) };
                for (int x = 15; x >= 0; x--)
                {
                    Thickness margin = new Thickness(0, -2, 0, 0);
                    if (x == 7)
                    {
                        margin = new Thickness(0, 0, 0, 0);
                    }
                    sp.Children.Add(
                        new TextBox
                        {
                            Text = calls[column * 16 + x],
                            Width = 50,
                            Height = 25,
                            BorderThickness = new Thickness(2),
                            BorderBrush = System.Windows.Media.Brushes.Black,
                            IsReadOnly = true,
                            Background = System.Windows.Media.Brushes.Transparent,
                            TextAlignment = TextAlignment.Center,
                            Margin = margin
                        });
                    numOfCalls--;
                }
                column++;

                HeaderSP.Children.Add(sp);
            } while (numOfCalls > 0);
        }

        private void Generate_IO(bool group = false)
        {
            IOInfoSP.Children.Clear();

            List<string> inputs = content.inputs;
            List<string> outputs = content.outputs;

            Label inputLabel = new Label
            {
                Content = "Spare Inputs",
            };

            IOInfoSP.Children.Add(inputLabel);

            for (int row = 0; row < 8; row++)
            {
                StackPanel rowSP = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(20, 0, 0, 0),
                };

                if (row == 3)
                {
                    rowSP.Margin = new Thickness(20, 0, 0, 20);
                }

                for (int column = 0; column < 8; column++)
                {
                    string ioText = "";
                    if (row * 8 + (7 - column) < inputs.Count)
                    {
                        ioText = inputs[row * 8 + (7 - column)];
                    }
                    else
                    {
                        ioText = "XXXX";
                    }

                    TextBox io = new TextBox
                    {
                        Text = ioText,
                        Width = 48,
                        Height = 25,
                        BorderThickness = new Thickness(0),
                        IsReadOnly = true,
                        Background = System.Windows.Media.Brushes.Transparent,
                        TextAlignment = TextAlignment.Center,
                        Tag = "IO",
                    };

                    rowSP.Children.Add(io);

                    if (column == 3)
                    {
                        TextBox hyphen = new TextBox
                        {
                            Text = "---",
                            Width = 20,
                            Height = 25,
                            BorderThickness = new Thickness(0),
                            IsReadOnly = true,
                            Background = System.Windows.Media.Brushes.Transparent,
                            TextAlignment = TextAlignment.Center,
                            Tag = "hyphen",
                        };

                        rowSP.Children.Add(hyphen);
                    }
                    else if (column < 7) // dont want to add hyphen for last column
                    {
                        TextBox hyphen = new TextBox
                        {
                            Text = "-",
                            Width = 7,
                            Height = 25,
                            BorderThickness = new Thickness(0),
                            IsReadOnly = true,
                            Background = System.Windows.Media.Brushes.Transparent,
                            TextAlignment = TextAlignment.Center,
                            Tag = "hyphen",
                        };

                        rowSP.Children.Add(hyphen);
                    }
                }

                bool rowIsEmpty = true;

                foreach (var child in rowSP.Children)
                {
                    if (child.GetType() == typeof(TextBox))
                    {
                        TextBox tb = child as TextBox;

                        if (tb.Text != "XXXX" && tb.Tag.ToString() == "IO")
                        {
                            rowIsEmpty = false;
                        }
                    }
                }

                if (!rowIsEmpty)
                {
                    IOInfoSP.Children.Add(rowSP);
                }
            }

            Label outputLabel = new Label
            {
                Content = "Spare Outputs",
            };

            IOInfoSP.Children.Add(outputLabel);

            for (int row = 0; row < 8; row++)
            {
                StackPanel rowSP = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(20, 0, 0, 0),
                };

                for (int column = 0; column < 8; column++)
                {
                    string ioText = "";
                    if (row * 8 + column < outputs.Count)
                    {
                        ioText = outputs[row * 8 + column];
                    }
                    else
                    {
                        ioText = "XXXX";
                    }

                    TextBox io = new TextBox
                    {
                        Text = ioText,
                        Width = 48,
                        Height = 25,
                        BorderThickness = new Thickness(0),
                        IsReadOnly = true,
                        Background = System.Windows.Media.Brushes.Transparent,
                        TextAlignment = TextAlignment.Center,
                        Tag = "IO",
                    };

                    rowSP.Children.Add(io);

                    if (column == 3)
                    {
                        TextBox hyphen = new TextBox
                        {
                            Text = "---",
                            Width = 20,
                            Height = 25,
                            BorderThickness = new Thickness(0),
                            IsReadOnly = true,
                            Background = System.Windows.Media.Brushes.Transparent,
                            TextAlignment = TextAlignment.Center,
                            Tag = "hyphen",
                        };

                        rowSP.Children.Add(hyphen);
                    }
                    else if (column < 7) // dont want to add hyphen for last column
                    {
                        TextBox hyphen = new TextBox
                        {
                            Text = "-",
                            Width = 7,
                            Height = 25,
                            BorderThickness = new Thickness(0),
                            IsReadOnly = true,
                            Background = System.Windows.Media.Brushes.Transparent,
                            TextAlignment = TextAlignment.Center,
                            Tag = "hyphen",
                        };

                        rowSP.Children.Add(hyphen);
                    }
                }

                bool rowIsEmpty = true;

                foreach (var child in rowSP.Children)
                {
                    if (child.GetType() == typeof(TextBox))
                    {
                        TextBox tb = child as TextBox;

                        if (tb.Text != "XXXX" && tb.Tag.ToString() == "IO")
                        {
                            rowIsEmpty = false;
                        }
                    }
                }

                if (!rowIsEmpty)
                {
                    IOInfoSP.Children.Add(rowSP);
                }
            }

            if (!group)
            {
                string ncBoard = content.Get_Bit("LOBBY:", 38, 1, 3);

                if (ncBoard == "YES")
                {
                    List<string> ncinputs = content.IO(new List<string> { "NIOINS" });
                    List<string> ncoutputs = content.IO(new List<string> { "NIOOUTS" }, 'O');

                    Label ncInputLabel = new Label
                    {
                        Content = "NC Inputs",
                    };

                    IOInfoSP.Children.Add(ncInputLabel);

                    for (int row = 0; row < 8; row++)
                    {
                        StackPanel rowSP = new StackPanel
                        {
                            Orientation = Orientation.Horizontal,
                            Margin = new Thickness(20, 0, 0, 0),
                        };

                        for (int column = 0; column < 8; column++)
                        {
                            string ioText = "";
                            if (row * 8 + (7 - column) >= ncinputs.Count)
                            {
                                ioText = "XXXX";
                            }
                            else
                            {
                                ioText = ncinputs[row * 8 + (7 - column)];
                            }

                            TextBox io = new TextBox
                            {
                                Text = ioText,
                                Width = 48,
                                Height = 25,
                                BorderThickness = new Thickness(0),
                                IsReadOnly = true,
                                Background = System.Windows.Media.Brushes.Transparent,
                                TextAlignment = TextAlignment.Center,
                                Tag = "IO",
                            };

                            rowSP.Children.Add(io);

                            if (column == 3)
                            {
                                TextBox hyphen = new TextBox
                                {
                                    Text = "---",
                                    Width = 20,
                                    Height = 25,
                                    BorderThickness = new Thickness(0),
                                    IsReadOnly = true,
                                    Background = System.Windows.Media.Brushes.Transparent,
                                    TextAlignment = TextAlignment.Center,
                                    Tag = "hyphen",
                                };

                                rowSP.Children.Add(hyphen);
                            }
                            else if (column < 7) // dont want to add hyphen for last column
                            {
                                TextBox hyphen = new TextBox
                                {
                                    Text = "-",
                                    Width = 7,
                                    Height = 25,
                                    BorderThickness = new Thickness(0),
                                    IsReadOnly = true,
                                    Background = System.Windows.Media.Brushes.Transparent,
                                    TextAlignment = TextAlignment.Center,
                                    Tag = "hyphen",
                                };

                                rowSP.Children.Add(hyphen);
                            }
                        }

                        bool rowIsEmpty = true;

                        foreach (var child in rowSP.Children)
                        {
                            if (child.GetType() == typeof(TextBox))
                            {
                                TextBox tb = child as TextBox;

                                if (tb.Text != "XXXX" && tb.Tag.ToString() == "IO")
                                {
                                    rowIsEmpty = false;
                                }
                            }
                        }

                        if (!rowIsEmpty)
                        {
                            IOInfoSP.Children.Add(rowSP);
                        }
                    }

                    Label ncOutputLabel = new Label
                    {
                        Content = "NC Outputs",
                    };

                    IOInfoSP.Children.Add(ncOutputLabel);

                    for (int row = 0; row < 8; row++)
                    {
                        StackPanel rowSP = new StackPanel
                        {
                            Orientation = Orientation.Horizontal,
                            Margin = new Thickness(20, 0, 0, 0),
                        };

                        for (int column = 0; column < 8; column++)
                        {
                            string ioText = "";
                            if (row * 8 + column >= ncoutputs.Count)
                            {
                                ioText = "XXXX";
                            }
                            else
                            {
                                ioText = ncoutputs[row * 8 + column];
                            }

                            TextBox io = new TextBox
                            {
                                Text = ioText,
                                Width = 48,
                                Height = 25,
                                BorderThickness = new Thickness(0),
                                IsReadOnly = true,
                                Background = System.Windows.Media.Brushes.Transparent,
                                TextAlignment = TextAlignment.Center,
                                Tag = "IO",
                            };

                            rowSP.Children.Add(io);

                            if (column == 3)
                            {
                                TextBox hyphen = new TextBox
                                {
                                    Text = "---",
                                    Width = 20,
                                    Height = 25,
                                    BorderThickness = new Thickness(0),
                                    IsReadOnly = true,
                                    Background = System.Windows.Media.Brushes.Transparent,
                                    TextAlignment = TextAlignment.Center,
                                    Tag = "hyphen",
                                };

                                rowSP.Children.Add(hyphen);
                            }
                            else if (column < 7) // dont want to add hyphen for last column
                            {
                                TextBox hyphen = new TextBox
                                {
                                    Text = "-",
                                    Width = 7,
                                    Height = 25,
                                    BorderThickness = new Thickness(0),
                                    IsReadOnly = true,
                                    Background = System.Windows.Media.Brushes.Transparent,
                                    TextAlignment = TextAlignment.Center,
                                    Tag = "hyphen",
                                };

                                rowSP.Children.Add(hyphen);
                            }
                        }

                        bool rowIsEmpty = true;

                        foreach (var child in rowSP.Children)
                        {
                            if (child.GetType() == typeof(TextBox))
                            {
                                TextBox tb = child as TextBox;

                                if (tb.Text != "XXXX" && tb.Tag.ToString() == "IO")
                                {
                                    rowIsEmpty = false;
                                }
                            }
                        }

                        if (!rowIsEmpty)
                        {
                            IOInfoSP.Children.Add(rowSP);
                        }
                    }
                }
            }

            Draw_Boards(content, group);
        }

        private void Draw_Boards(Content content, bool group = false)
        {
            BoardSP.Children.Clear();

            int bWidth = 384;
            int spWidth = 379;
            int tbWidth = 48;

            int iox = 0;
            int i4o = 0;
            int aiox = 0;

            if (group)
            {
                iox = General.HexStringToDecimal(content.Get_Nibble("LOBBY:", 6, 0));
                i4o = General.HexStringToDecimal(content.Get_Nibble("LOBBY:", 6, 1));
                aiox = General.HexStringToDecimal(content.Get_Nibble("LOBBY:", 8, 0));

            }
            else
            {
                i4o = General.HexStringToDecimal(content.Get_Nibble("LOBBY:", 40, 1));
                iox = General.HexStringToDecimal(content.Get_Nibble("LOBBY:", 40, 0));
                aiox = General.HexStringToDecimal(content.Get_Nibble("LOBBY:", 52, 0));
            }

            List<string> inputs = content.inputs;
            List<string> outputs = content.outputs;

            int inputRow = 0;
            int outputRow = 0;
            int inputCol = 0;
            int outputCol = 0;

            //IOX
            for (int b = 0; b < iox; b++)
            {

                Border border = new Border
                {
                    BorderBrush = System.Windows.Media.Brushes.Black,
                    BorderThickness = new Thickness(2),
                    Background = System.Windows.Media.Brushes.Transparent,
                    Margin = new Thickness(0, 0, 0, 10),
                    Width = bWidth,
                    HorizontalAlignment = HorizontalAlignment.Left
                };

                StackPanel ioxsp = new StackPanel
                {
                    Name = "ioxsp" + b,
                    Orientation = Orientation.Vertical,
                    Width = spWidth,
                    Margin = new Thickness(5, 0, 0, 0)
                };

                Label boardLabel = new Label { Content = "IOX Board # " + (b + 1) };
                Label inputLabel = new Label { Content = "Inputs:" };
                Label outputLabel = new Label { Content = "Outputs:", Margin = new Thickness(0, 23, 0, 0) };

                StackPanel inputsp1 = new StackPanel { Orientation = Orientation.Horizontal };
                StackPanel outputsp1 = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 10) };

                for (int i = 0; i < 8; i++)
                {
                    string ioText = "";
                    if (inputRow * 8 + (7 - inputCol) < inputs.Count)
                    {
                        ioText = inputs[inputRow * 8 + (7 - inputCol)];
                    }
                    else
                    {
                        ioText = "";
                    }
                    inputsp1.Children.Add(
                        new TextBox
                        {
                            Text = ioText,
                            Width = tbWidth,
                            Height = 25,
                            BorderThickness = new Thickness(2),
                            BorderBrush = System.Windows.Media.Brushes.Black,
                            IsReadOnly = true,
                            Background = System.Windows.Media.Brushes.Transparent,
                            TextAlignment = TextAlignment.Center,
                            Margin = new Thickness(0, 0, -2, 0)
                        });
                    inputCol++;
                }

                inputCol = 0;
                inputRow++;

                for (int o = 0; o < 8; o++)
                {
                    string ioText = "";
                    if (outputRow * 8 + outputCol < outputs.Count)
                    {
                        ioText = outputs[outputRow * 8 + outputCol];
                    }
                    else
                    {
                        ioText = "";
                    }
                    outputsp1.Children.Add(
                        new TextBox
                        {
                            Text = ioText,
                            Width = tbWidth,
                            Height = 25,
                            BorderThickness = new Thickness(2),
                            BorderBrush = System.Windows.Media.Brushes.Black,
                            IsReadOnly = true,
                            Background = System.Windows.Media.Brushes.Transparent,
                            TextAlignment = TextAlignment.Center,
                            Margin = new Thickness(0, 0, -2, 0)
                        });
                    outputCol++;
                }

                outputCol = 0;
                outputRow++;

                ioxsp.Children.Add(boardLabel);

                ioxsp.Children.Add(inputLabel);
                ioxsp.Children.Add(inputsp1);

                ioxsp.Children.Add(outputLabel);
                ioxsp.Children.Add(outputsp1);

                border.Child = ioxsp;
                BoardSP.Children.Add(border);
            }

            //I4O
            for (int b = 0; b < i4o; b++)
            {
                Border border = new Border
                {
                    BorderBrush = System.Windows.Media.Brushes.Black,
                    BorderThickness = new Thickness(2),
                    Background = System.Windows.Media.Brushes.Transparent,
                    Margin = new Thickness(0, 0, 0, 10),
                    Width = bWidth,
                    HorizontalAlignment = HorizontalAlignment.Left
                };

                StackPanel i4osp = new StackPanel
                {
                    Name = "i4osp" + b,
                    Orientation = Orientation.Vertical,
                    Width = spWidth,
                    Margin = new Thickness(5, 0, 0, 0)
                };

                Label boardLabel = new Label { Content = "I4O Board # " + (b + 1) };
                Label inputLabel = new Label { Content = "Inputs:" };
                Label outputLabel = new Label { Content = "Outputs:" };

                StackPanel inputsp1 = new StackPanel { Orientation = Orientation.Horizontal };
                StackPanel inputsp2 = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, -2, 0, 0) };
                StackPanel outputsp1 = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 10) };

                for (int i = 0; i < 8; i++)
                {
                    string ioText = "";
                    if (inputRow * 8 + (7 - inputCol) < inputs.Count)
                    {
                        ioText = inputs[inputRow * 8 + (7 - inputCol)];
                    }
                    else
                    {
                        ioText = "";
                    }
                    inputsp1.Children.Add(
                        new TextBox
                        {
                            Text = ioText,
                            Width = tbWidth,
                            Height = 25,
                            BorderThickness = new Thickness(2),
                            BorderBrush = System.Windows.Media.Brushes.Black,
                            IsReadOnly = true,
                            Background = System.Windows.Media.Brushes.Transparent,
                            TextAlignment = TextAlignment.Center,
                            Margin = new Thickness(0, 0, -2, 0)
                        });

                    inputCol++;

                    if (inputCol == 8)
                    {
                        inputCol = 0;
                        inputRow++;
                    }
                }

                for (int i = 0; i < 8; i++)
                {
                    string ioText = "";
                    if (inputRow * 8 + (7 - inputCol) < inputs.Count)
                    {
                        ioText = inputs[inputRow * 8 + (7 - inputCol)];
                    }
                    else
                    {
                        ioText = "";
                    }
                    inputsp2.Children.Add(
                        new TextBox
                        {
                            Text = ioText,
                            Width = tbWidth,
                            Height = 25,
                            BorderThickness = new Thickness(2),
                            BorderBrush = System.Windows.Media.Brushes.Black,
                            IsReadOnly = true,
                            Background = System.Windows.Media.Brushes.Transparent,
                            TextAlignment = TextAlignment.Center,
                            Margin = new Thickness(0, 0, -2, 0)
                        });

                    inputCol++;

                    if (inputCol == 8)
                    {
                        inputCol = 0;
                        inputRow++;
                    }
                }

                for (int o = 0; o < 4; o++)
                {
                    string ioText = "";
                    if (outputRow * 8 + outputCol < outputs.Count)
                    {
                        ioText = outputs[outputRow * 8 + outputCol];
                    }
                    else
                    {
                        ioText = "";
                    }
                    outputsp1.Children.Add(
                        new TextBox
                        {
                            Text = ioText,
                            Width = tbWidth,
                            Height = 25,
                            BorderThickness = new Thickness(2),
                            BorderBrush = System.Windows.Media.Brushes.Black,
                            IsReadOnly = true,
                            Background = System.Windows.Media.Brushes.Transparent,
                            TextAlignment = TextAlignment.Center,
                            Margin = new Thickness(0, 0, -2, 0)
                        });

                    outputCol++;

                    if (outputCol == 8)
                    {
                        outputCol = 0;
                        outputRow++;
                    }
                }

                i4osp.Children.Add(boardLabel);

                i4osp.Children.Add(inputLabel);
                i4osp.Children.Add(inputsp1);
                i4osp.Children.Add(inputsp2);

                i4osp.Children.Add(outputLabel);
                i4osp.Children.Add(outputsp1);

                border.Child = i4osp;
                BoardSP.Children.Add(border);
            }

            //AIOX
            for (int b = 0; b < aiox; b++)
            {

                Border border = new Border
                {
                    BorderBrush = System.Windows.Media.Brushes.Black,
                    BorderThickness = new Thickness(2),
                    Background = System.Windows.Media.Brushes.Transparent,
                    Margin = new Thickness(0, 0, 0, 10),
                    Width = bWidth,
                    HorizontalAlignment = HorizontalAlignment.Left
                };

                StackPanel aioxsp = new StackPanel
                {
                    Name = "aioxsp" + b,
                    Orientation = Orientation.Vertical,
                    Width = spWidth,
                    Margin = new Thickness(5, 0, 0, 0)
                };

                Label boardLabel = new Label { Content = "AIOX Board # " + (b + 1) };
                Label inputLabel = new Label { Content = "Inputs:" };
                Label outputLabel = new Label { Content = "Outputs:", Margin = new Thickness(0, 23, 0, 0) };

                StackPanel inputsp1 = new StackPanel { Orientation = Orientation.Horizontal };
                StackPanel outputsp1 = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 10) };

                for (int i = 0; i < 8; i++)
                {
                    string ioText = "";
                    if (inputRow * 8 + (7 - inputCol) < inputs.Count)
                    {
                        ioText = inputs[inputRow * 8 + (7 - inputCol)];
                    }
                    else
                    {
                        ioText = "";
                    }
                    inputsp1.Children.Add(
                        new TextBox
                        {
                            Text = ioText,
                            Width = tbWidth,
                            Height = 25,
                            BorderThickness = new Thickness(2),
                            BorderBrush = System.Windows.Media.Brushes.Black,
                            IsReadOnly = true,
                            Background = System.Windows.Media.Brushes.Transparent,
                            TextAlignment = TextAlignment.Center,
                            Margin = new Thickness(0, 0, -2, 0)
                        });
                    inputCol++;
                }

                inputCol = 0;
                inputRow++;

                for (int o = 0; o < 8; o++)
                {
                    string ioText = "";
                    if (outputRow * 8 + outputCol < outputs.Count)
                    {
                        ioText = outputs[outputRow * 8 + outputCol];
                    }
                    else
                    {
                        ioText = "";
                    }
                    outputsp1.Children.Add(
                        new TextBox
                        {
                            Text = ioText,
                            Width = tbWidth,
                            Height = 25,
                            BorderThickness = new Thickness(2),
                            BorderBrush = System.Windows.Media.Brushes.Black,
                            IsReadOnly = true,
                            Background = System.Windows.Media.Brushes.Transparent,
                            TextAlignment = TextAlignment.Center,
                            Margin = new Thickness(0, 0, -2, 0)
                        });
                    outputCol++;

                    if (outputCol == 8)
                    {
                        outputCol = 0;
                        outputRow++;
                    }
                }

                outputRow++;

                aioxsp.Children.Add(boardLabel);

                aioxsp.Children.Add(inputLabel);
                aioxsp.Children.Add(inputsp1);

                aioxsp.Children.Add(outputLabel);
                aioxsp.Children.Add(outputsp1);

                border.Child = aioxsp;
                BoardSP.Children.Add(border);
            }
        }

        private void Track_Mod()
        {
            //OPEN EXCEL
            Excel.Application xlApp = new Excel.Application();
            xlApp.Visible = false;
            Excel.Workbook xlWorkbook = xlApp.Workbooks.Open("F:\\Software\\Product\\Trac_Mod.xlsm", 0, true);
            Excel._Worksheet xlWorksheet = xlWorkbook.Sheets[1];
            Excel._Worksheet dlmWorksheet = xlWorkbook.Sheets[2];
            Excel.Range xlRange = xlWorksheet.UsedRange;
            Excel.Range dlmRange = dlmWorksheet.UsedRange;

            //GENERATE TRAC_MOD TAB
            StackPanel labelPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
            };

            Label dateReceivedLabel = new Label
            {
                Content = "Received",
                Margin = new Thickness(0, 0, 5, 10),
                HorizontalContentAlignment = HorizontalAlignment.Center,
                Width = 75,
            };

            Label shipedDateLabel = new Label
            {
                Margin = new Thickness(0, 0, 5, 10),
                HorizontalContentAlignment = HorizontalAlignment.Center,
                Content = "Ship Date",
                Width = 75
            };

            Label notificationLabel = new Label
            {
                Content = "Notification #",
                Width = 75,
                Margin = new Thickness(0, 0, 5, 10),
                HorizontalContentAlignment = HorizontalAlignment.Center,
            };

            Label jobNumberLabel = new Label
            {
                Content = "Job Number",
                Width = 75,
                Margin = new Thickness(0, 0, 5, 10),
                HorizontalContentAlignment = HorizontalAlignment.Center,
            };

            Label typeLabel = new Label
            {
                Content = "Type",
                Width = 50,
                Margin = new Thickness(0, 0, 5, 10),
                HorizontalContentAlignment = HorizontalAlignment.Center,
            };

            Label customLabel = new Label
            {
                Content = "Custom",
                Width = 50,
                Margin = new Thickness(0, 0, 5, 10),
                HorizontalContentAlignment = HorizontalAlignment.Center,
            };

            Label engineerLabel = new Label
            {
                Content = "Engineer",
                Width = 60,
                Margin = new Thickness(0, 0, 5, 10),
                HorizontalContentAlignment = HorizontalAlignment.Center,
            };

            Label notesLabel = new Label
            {
                Content = "Notes",
                Width = 400,
                Margin = new Thickness(0, 0, 5, 10),
                HorizontalContentAlignment = HorizontalAlignment.Center,
            };

            labelPanel.Children.Add(dateReceivedLabel);
            labelPanel.Children.Add(shipedDateLabel);
            labelPanel.Children.Add(notificationLabel);
            labelPanel.Children.Add(jobNumberLabel);
            labelPanel.Children.Add(typeLabel);
            labelPanel.Children.Add(customLabel);
            labelPanel.Children.Add(engineerLabel);
            labelPanel.Children.Add(notesLabel);

            TracModLabelSP.Children.Add(labelPanel);

            for (int row = 4; row < 100; row++)
            {
                string dateReceived = "";
                string shipDate = "";
                string notificationNumber = "";
                string jobNumber = "";
                string type = "";
                string custom = "";
                string engineer = "";
                string notes = "";

                if (xlRange.Cells[row, 5].Value2 != null)
                {
                    StackPanel sp = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Height = 70,
                    };

                    try
                    {
                        string rawDate = xlRange.Cells[row, 1].Value2.ToString();
                        double d = double.Parse(rawDate);
                        dateReceived = DateTime.FromOADate(d).ToString("MM/dd/yy");

                    }
                    catch
                    {

                    }
                    try
                    {
                        string rawDate = xlRange.Cells[row, 2].Value2.ToString();
                        double d = double.Parse(rawDate);
                        shipDate = DateTime.FromOADate(d).ToString("MM/dd/yy");
                    }
                    catch
                    {

                    }
                    try
                    {
                        notificationNumber = xlRange.Cells[row, 4].Value2.ToString();
                    }
                    catch
                    {

                    }
                    try
                    {
                        jobNumber = xlRange.Cells[row, 5].Value2.ToString();
                    }
                    catch
                    {

                    }
                    try
                    {
                        type = xlRange.Cells[row, 6].Value2.ToString();
                    }
                    catch
                    {

                    }
                    try
                    {
                        custom = xlRange.Cells[row, 7].Value2.ToString();
                    }
                    catch
                    {

                    }
                    try
                    {
                        engineer = xlRange.Cells[row, 8].Value2.ToString();
                    }
                    catch
                    {

                    }
                    try
                    {
                        notes = xlRange.Cells[row, 9].Value2.ToString();
                    }
                    catch
                    {

                    }

                    TextBox receivedDateTB = new TextBox
                    {
                        Height = 50,
                        Background = System.Windows.Media.Brushes.Transparent,
                        IsReadOnly = true,
                        Margin = new Thickness(0, 0, 5, 10),
                        Text = dateReceived,
                        Width = 75,
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        VerticalContentAlignment = VerticalAlignment.Center
                    };

                    TextBox shipDateTB = new TextBox
                    {
                        Height = 50,
                        Background = System.Windows.Media.Brushes.Transparent,
                        IsReadOnly = true,
                        Margin = new Thickness(0, 0, 5, 10),
                        Text = shipDate,
                        Width = 75,
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        VerticalContentAlignment = VerticalAlignment.Center
                    };

                    TextBox notificationTB = new TextBox
                    {
                        Height = 50,
                        Background = System.Windows.Media.Brushes.Transparent,
                        IsReadOnly = true,
                        Margin = new Thickness(0, 0, 5, 10),
                        Text = notificationNumber,
                        Width = 75,
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        VerticalContentAlignment = VerticalAlignment.Center
                    };

                    TextBox jobNumberTB = new TextBox
                    {
                        Height = 50,
                        Background = System.Windows.Media.Brushes.Transparent,
                        IsReadOnly = true,
                        Margin = new Thickness(0, 0, 5, 10),
                        Text = jobNumber,
                        Width = 75,
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        VerticalContentAlignment = VerticalAlignment.Center
                    };

                    TextBox typeTB = new TextBox
                    {
                        Height = 50,
                        Background = System.Windows.Media.Brushes.Transparent,
                        IsReadOnly = true,
                        Margin = new Thickness(0, 0, 5, 10),
                        Text = type,
                        Width = 50,
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        VerticalContentAlignment = VerticalAlignment.Center
                    };

                    TextBox customTB = new TextBox
                    {
                        Height = 50,
                        Background = System.Windows.Media.Brushes.Transparent,
                        IsReadOnly = true,
                        Margin = new Thickness(0, 0, 5, 10),
                        Text = custom,
                        Width = 50,
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        VerticalContentAlignment = VerticalAlignment.Center
                    };

                    TextBox engineerTB = new TextBox
                    {
                        Height = 50,
                        Background = System.Windows.Media.Brushes.Transparent,
                        IsReadOnly = true,
                        Margin = new Thickness(0, 0, 5, 10),
                        Text = engineer,
                        Width = 60,
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        VerticalContentAlignment = VerticalAlignment.Center
                    };

                    TextBox notesTB = new TextBox
                    {
                        Height = 50,
                        Background = System.Windows.Media.Brushes.Transparent,
                        IsReadOnly = true,
                        Margin = new Thickness(0, 0, 5, 10),
                        Text = notes,
                        Width = 400,
                        VerticalScrollBarVisibility = ScrollBarVisibility.Visible,
                        TextWrapping = TextWrapping.Wrap,
                        VerticalContentAlignment = VerticalAlignment.Center,
                    };



                    Button tracModSearchButton = new Button
                    {
                        Content = "Search",
                        Tag = jobNumberTB.Text,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Center,
                        Width = 80,
                    };

                    receivedDateTB.MouseDoubleClick += TracModInfoClipboard_Click;
                    shipDateTB.MouseDoubleClick += TracModInfoClipboard_Click;
                    notificationTB.MouseDoubleClick += TracModInfoClipboard_Click;
                    jobNumberTB.MouseDoubleClick += TracModInfoClipboard_Click;
                    typeTB.MouseDoubleClick += TracModInfoClipboard_Click;
                    customTB.MouseDoubleClick += TracModInfoClipboard_Click;
                    engineerTB.MouseDoubleClick += TracModInfoClipboard_Click;
                    notesTB.MouseDoubleClick += TracModInfoClipboard_Click;
                    tracModSearchButton.Click += TracModSearch_Click;

                    sp.Children.Add(receivedDateTB);
                    sp.Children.Add(shipDateTB);
                    sp.Children.Add(notificationTB);
                    sp.Children.Add(jobNumberTB);
                    sp.Children.Add(typeTB);
                    sp.Children.Add(customTB);
                    sp.Children.Add(engineerTB);
                    sp.Children.Add(notesTB);
                    sp.Children.Add(tracModSearchButton);

                    TracModContentSP.Children.Add(sp);
                }
            }

            for (int row = 4; row < 100; row++)
            {
                string dateReceived = "";
                string shipDate = "";
                string notificationNumber = "";
                string jobNumber = "";
                string type = "";
                string custom = "";
                string engineer = "";
                string notes = "";

                if (dlmRange.Cells[row, 5].Value2 != null)
                {
                    StackPanel sp = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Height = 70,
                    };

                    try
                    {
                        string rawDate = dlmRange.Cells[row, 1].Value2.ToString();
                        double d = double.Parse(rawDate);
                        dateReceived = DateTime.FromOADate(d).ToString("MM/dd/yy");

                    }
                    catch
                    {

                    }
                    try
                    {
                        string rawDate = dlmRange.Cells[row, 2].Value2.ToString();
                        double d = double.Parse(rawDate);
                        shipDate = DateTime.FromOADate(d).ToString("MM/dd/yy");
                    }
                    catch
                    {

                    }
                    try
                    {
                        notificationNumber = dlmRange.Cells[row, 4].Value2.ToString();
                    }
                    catch
                    {

                    }
                    try
                    {
                        jobNumber = dlmRange.Cells[row, 5].Value2.ToString();
                    }
                    catch
                    {

                    }
                    try
                    {
                        type = dlmRange.Cells[row, 6].Value2.ToString();
                    }
                    catch
                    {

                    }
                    try
                    {
                        custom = dlmRange.Cells[row, 7].Value2.ToString();
                    }
                    catch
                    {

                    }
                    try
                    {
                        engineer = dlmRange.Cells[row, 8].Value2.ToString();
                    }
                    catch
                    {

                    }
                    try
                    {
                        notes = dlmRange.Cells[row, 9].Value2.ToString();
                    }
                    catch
                    {

                    }

                    TextBox receivedDateTB = new TextBox
                    {
                        Height = 50,
                        Background = System.Windows.Media.Brushes.Transparent,
                        IsReadOnly = true,
                        Margin = new Thickness(0, 0, 5, 10),
                        Text = dateReceived,
                        Width = 75,
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        VerticalContentAlignment = VerticalAlignment.Center
                    };

                    TextBox shipDateTB = new TextBox
                    {
                        Height = 50,
                        Background = System.Windows.Media.Brushes.Transparent,
                        IsReadOnly = true,
                        Margin = new Thickness(0, 0, 5, 10),
                        Text = shipDate,
                        Width = 75,
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        VerticalContentAlignment = VerticalAlignment.Center
                    };

                    TextBox notificationTB = new TextBox
                    {
                        Height = 50,
                        Background = System.Windows.Media.Brushes.Transparent,
                        IsReadOnly = true,
                        Margin = new Thickness(0, 0, 5, 10),
                        Text = notificationNumber,
                        Width = 75,
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        VerticalContentAlignment = VerticalAlignment.Center
                    };

                    TextBox jobNumberTB = new TextBox
                    {
                        Height = 50,
                        Background = System.Windows.Media.Brushes.Transparent,
                        IsReadOnly = true,
                        Margin = new Thickness(0, 0, 5, 10),
                        Text = jobNumber,
                        Width = 75,
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        VerticalContentAlignment = VerticalAlignment.Center
                    };

                    TextBox typeTB = new TextBox
                    {
                        Height = 50,
                        Background = System.Windows.Media.Brushes.Transparent,
                        IsReadOnly = true,
                        Margin = new Thickness(0, 0, 5, 10),
                        Text = type,
                        Width = 50,
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        VerticalContentAlignment = VerticalAlignment.Center
                    };

                    TextBox customTB = new TextBox
                    {
                        Height = 50,
                        Background = System.Windows.Media.Brushes.Transparent,
                        IsReadOnly = true,
                        Margin = new Thickness(0, 0, 5, 10),
                        Text = custom,
                        Width = 50,
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        VerticalContentAlignment = VerticalAlignment.Center
                    };

                    TextBox engineerTB = new TextBox
                    {
                        Height = 50,
                        Background = System.Windows.Media.Brushes.Transparent,
                        IsReadOnly = true,
                        Margin = new Thickness(0, 0, 5, 10),
                        Text = engineer,
                        Width = 60,
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        VerticalContentAlignment = VerticalAlignment.Center
                    };

                    TextBox notesTB = new TextBox
                    {
                        Height = 50,
                        IsReadOnly = true,
                        Margin = new Thickness(0, 0, 5, 10),
                        Text = notes,
                        Width = 400,
                        VerticalScrollBarVisibility = ScrollBarVisibility.Visible,
                        TextWrapping = TextWrapping.Wrap,
                        VerticalContentAlignment = VerticalAlignment.Center,
                        Background = System.Windows.Media.Brushes.Transparent,
                    };

                    Button tracModSearchButton = new Button
                    {
                        Content = "Search",
                        Tag = jobNumberTB.Text,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Center,
                        Width = 80
                    };

                    receivedDateTB.MouseDoubleClick += TracModInfoClipboard_Click;
                    shipDateTB.MouseDoubleClick += TracModInfoClipboard_Click;
                    notificationTB.MouseDoubleClick += TracModInfoClipboard_Click;
                    jobNumberTB.MouseDoubleClick += TracModInfoClipboard_Click;
                    typeTB.MouseDoubleClick += TracModInfoClipboard_Click;
                    customTB.MouseDoubleClick += TracModInfoClipboard_Click;
                    engineerTB.MouseDoubleClick += TracModInfoClipboard_Click;
                    notesTB.MouseDoubleClick += TracModInfoClipboard_Click;
                    tracModSearchButton.Click += TracModSearch_Click;

                    sp.Children.Add(receivedDateTB);
                    sp.Children.Add(shipDateTB);
                    sp.Children.Add(notificationTB);
                    sp.Children.Add(jobNumberTB);
                    sp.Children.Add(typeTB);
                    sp.Children.Add(customTB);
                    sp.Children.Add(engineerTB);
                    sp.Children.Add(notesTB);
                    sp.Children.Add(tracModSearchButton);

                    TracModContentSP.Children.Add(sp);
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
            Marshal.ReleaseComObject(dlmRange);
            Marshal.ReleaseComObject(dlmWorksheet);

            //close and release
            xlWorkbook.Close(false);
            Marshal.ReleaseComObject(xlWorkbook);

            //quit and release
            xlApp.Quit();
            Marshal.ReleaseComObject(xlApp);
        }

        private void Write_Error_To_Log(string file, Exception ex)
        {
            using (System.IO.StreamWriter writefile =
                    new System.IO.StreamWriter(@"\\10.112.10.28\MCE-Rancho\Jake Ball\Error_Log.txt", true))
            {
                DateTime now = DateTime.Now;
                writefile.WriteLine("[" + now.ToString() + "] " + Environment.UserName);
                writefile.WriteLine(file);
                writefile.WriteLine(ex.ToString() + "\n");
            }
        }

        private void TracModSearch_Click(object sender, RoutedEventArgs e)
        {
            InfoTabControl.SelectedIndex = 0;

            string jobNumber = ((Button)sender).Tag.ToString();

            if (jobNumber.Contains("-"))
            {
                int dashIndex = jobNumber.IndexOf("-");
                jobNumber = jobNumber.Substring(dashIndex + 1, jobNumber.Length - dashIndex - 1);
            }

            TextBox1.Text = jobNumber;

            blockSearchHistoryChange = true;
            Update_Search_History();
            SearchFiles();
            blockSearchHistoryChange = false;
        }

        private void TracModInfoClipboard_Click(object sender, RoutedEventArgs e)
        {
            string content = ((TextBox)sender).Text;

            Clipboard.SetText(content);
        }

        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string path = "";
                string deconstructedPath = FilesListBox.SelectedItem.ToString();
                while (deconstructedPath.Contains("\\"))
                {
                    int slashIndex = deconstructedPath.IndexOf("\\");
                    path += deconstructedPath.Substring(0, slashIndex) + "\\";
                    deconstructedPath = deconstructedPath.Substring(slashIndex + 1, deconstructedPath.Length - slashIndex - 1);
                }

                string cmd = "C:\\Windows\\explorer.exe";
                string arg = "";


                if (FilesListBox.SelectedItem.ToString().Contains(".afm"))
                {
                    arg = General.Get_Folder_From_Path(FilesListBox.SelectedItem.ToString());
                }
                else
                {
                    arg = G_DRIVE + "Software\\" + path;
                }
                Process.Start(cmd, arg);
            }
            catch
            {
                MessageBox.Show("Please Select an Item from the List");
            }
        }

        private void AdvancedSearch_Click(object sender, RoutedEventArgs e)
        {
            AdvancedSearch advsearch = new AdvancedSearch();
            advsearch.Show();
        }

        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            Update_Search_History();
        }

        private void ToggleIOView_Click(object sender, RoutedEventArgs e)
        {
            if (IOInfoSP.Visibility == Visibility.Visible)
            {
                IOInfoSP.Visibility = Visibility.Hidden;
                BoardSP.Visibility = Visibility.Visible;
            }
            else
            {
                IOInfoSP.Visibility = Visibility.Visible;
                BoardSP.Visibility = Visibility.Hidden;
            }
        }

        private void TogglePIs_Click(object sender, RoutedEventArgs e)
        {
            if (LandingPIs.Visibility == Visibility.Visible)
            {
                LandingPIs.Visibility = Visibility.Hidden;
                LandingLevels.Visibility = Visibility.Visible;
                TogglePIs.Content = "PI";
            }
            else
            {
                LandingPIs.Visibility = Visibility.Visible;
                LandingLevels.Visibility = Visibility.Hidden;
                TogglePIs.Content = "#";
            }
        }

        private void Make_Controls_Visible()
        {
            LandingNormalConfig.Visibility = Visibility.Visible;
            LandingNormalHeader.Visibility = Visibility.Visible;
            LandingPIs.Visibility = Visibility.Hidden;
            LandingLevels.Visibility = Visibility.Visible;
            TogglePIs.Content = "PI";
            TogglePIs.Visibility = Visibility.Visible;
            ToggleIOView.Visibility = Visibility.Visible;
            IOInfoSP.Visibility = Visibility.Visible;
            BoardSP.Visibility = Visibility.Hidden;
            JobInfo.Visibility = Visibility.Visible;
            LobbyOptionsBlock.Visibility = Visibility.Visible;
            BottomOptionsBlock.Visibility = Visibility.Visible;
            ViewVersionIO.Visibility = Visibility.Visible;
        }

        private void Make_Controls_Invisible()
        {
            IOInfoSP.Children.Clear();
            HeaderSP.Children.Clear();
            LandingAltConfig.Visibility = Visibility.Hidden;
            LandingAltHeader.Visibility = Visibility.Hidden;
            LandingNormalConfig.Visibility = Visibility.Hidden;
            LandingNormalHeader.Visibility = Visibility.Hidden;
            LandingLevels.Visibility = Visibility.Hidden;
            BoardSP.Children.Clear();
            LandingPIs.Visibility = Visibility.Hidden;
            TogglePIs.Visibility = Visibility.Hidden;
            ToggleIOView.Visibility = Visibility.Hidden;
            JobInfo.Visibility = Visibility.Hidden;
            LobbyOptionsBlock.Visibility = Visibility.Hidden;
            BottomOptionsBlock.Visibility = Visibility.Hidden;
            ViewVersionIO.Visibility = Visibility.Hidden;
        }

        private void Motion_Controls_Visible()
        {
            KDMFolder.Visibility = Visibility.Visible;
            MotionDummyFolder.Visibility = Visibility.Visible;
            KDMEmail.Visibility = Visibility.Visible;

            ExportExcel.Visibility = Visibility.Hidden;
            AdvancedSearch.Visibility = Visibility.Hidden;
            ShowPrints.Visibility = Visibility.Hidden;
        }

        private void Legacy_Controls_Visible()
        {
            KDMFolder.Visibility = Visibility.Hidden;
            MotionDummyFolder.Visibility = Visibility.Hidden;
            KDMEmail.Visibility = Visibility.Hidden;

            ExportExcel.Visibility = Visibility.Visible;
            AdvancedSearch.Visibility = Visibility.Visible;
            ShowPrints.Visibility = Visibility.Visible;
        }

        private bool Version_Check()
        {
            string versionPath = "";
            string newVersion = "";

            List<string> versions = new List<string>();
            versions = System.IO.File.ReadAllLines(@"\\10.112.10.28\MCE-Rancho\Jake Ball\Versions.txt").ToList();

            foreach (string version in versions)
            {
                if (version.StartsWith("ModHub"))
                {
                    int colonIndex = version.IndexOf(":");
                    versionPath = version.Substring(colonIndex + 1, version.Length - colonIndex - 1);
                    int semicolonIndex = versionPath.IndexOf(";");
                    newVersion = versionPath.Substring(semicolonIndex + 1, versionPath.Length - semicolonIndex - 1);
                    versionPath = versionPath.Substring(0, semicolonIndex);
                }
            }

            if (newVersion != this.version)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            if (Version_Check())
            {
                if (System.Windows.Forms.MessageBox.Show("There is a new version available, do you want to update?", "Update ModHub?", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    //To get the location the assembly normally resides on disk or the install directory
                    string path = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;

                    //once you have the path you get the directory with:
                    string directory = System.IO.Path.GetDirectoryName(path);

                    string updatepath = directory + "\\ModHubUpdater.exe";
                    string cmd = "C:\\Windows\\explorer.exe";
                    Process.Start(cmd, updatepath);
                    System.Windows.Application.Current.Shutdown();
                }
            }
            else
            {
                MessageBox.Show("Your version is up to date");
            }
        }

        private void Set_Permissions()
        {
            List<string> users = new List<string>();
            users = System.IO.File.ReadAllLines(@"\\10.112.10.28\MCE-Rancho\Jake Ball\Permissions.txt").ToList();
            string environmentName = Environment.UserName;

            foreach (string user in users)
            {
                int equalIndex = user.IndexOf("=");
                string userName = user.Substring(0, equalIndex);
                if (userName.ToLower() == environmentName.ToLower())
                {
                    this.permission = Int32.Parse(user.Substring(equalIndex + 1, user.Length - equalIndex - 1));
                }
            }

            FileExtension.Items.Add(".asm");
            if (permission < 2)
            {
                FileExtension.Items.Add(".old");
                FileExtension.Items.Add("All Files");
                FileExtension.Items.Add("Motion");
            }

            if (permission > 1)
            {
                OpenFile.Visibility = Visibility.Hidden;
                OpenFolder.Visibility = Visibility.Hidden;
                ModDocs.Visibility = Visibility.Hidden;
                OpenSim.Visibility = Visibility.Hidden;
                Mp2link.Visibility = Visibility.Hidden;
                Emulink.Visibility = Visibility.Hidden;
                BrowseFile.Visibility = Visibility.Hidden;
                UtilityTab.Visibility = Visibility.Hidden;
                TracModTab.Visibility = Visibility.Hidden;
                OptionsTab.Visibility = Visibility.Hidden;

                ShowPrints.Margin = new Thickness(ShowPrints.Margin.Left, ShowPrints.Margin.Top - 51, ShowPrints.Margin.Right, ShowPrints.Margin.Bottom);
                ExportExcel.Margin = new Thickness(ExportExcel.Margin.Left, ExportExcel.Margin.Top - 51, ExportExcel.Margin.Right, ExportExcel.Margin.Bottom);
                AdvancedSearch.Margin = new Thickness(AdvancedSearch.Margin.Left, AdvancedSearch.Margin.Top - 51, AdvancedSearch.Margin.Right, AdvancedSearch.Margin.Bottom);

                G_DRIVE = @"\\10.113.32.45\shared\";
            }

            if (permission > 0)
            {
                AdminTab.Visibility = Visibility.Hidden;
            }
        }

        private void PrintPage_Click(object sender, RoutedEventArgs e)
        {
            PrintPreview printPreview = new PrintPreview(this);
            printPreview.Show();
        }

        private void TracModFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            foreach (StackPanel child in TracModContentSP.Children)
            {
                bool foundText = false;

                foreach (var grandchild in child.Children)
                {
                    if (grandchild.GetType() == typeof(TextBox))
                    {
                        TextBox grandchildTB = grandchild as TextBox;
                        if (grandchildTB.Text.ToLower().Contains(TracModFilter.Text.ToLower()))
                        {
                            foundText = true;
                        }
                    }
                }

                if (foundText)
                {
                    child.Height = 70;
                }
                else
                {
                    child.Height = 0;
                }
            }
        }

        private void TracModRefresh_Click(object sender, RoutedEventArgs e)
        {
            TracModContentSP.Children.Clear();
            TracModLabelSP.Children.Clear();
            Track_Mod();
        }

        private void ModUpgrade_Click(object sender, RoutedEventArgs e)
        {
            UpgradeWindow uw = new UpgradeWindow();

            try
            {
                uw.JobFile.Text = G_DRIVE + "Software\\" + FilesListBox.SelectedItem.ToString();
                uw.ShowDialog();
            }
            catch (Exception ex)
            {
                Write_Error_To_Log("MODUPGRADE", ex);
            }

        }

        private void InfoTabControl_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (InfoTabControl.SelectedIndex == 5)
            {
                InfoTabControl.Margin = new Thickness(0, 18, 0, 0);

                CustomFoldersCheckBox.Visibility = Visibility.Hidden;
            }
            else
            {
                InfoTabControl.Margin = new Thickness(413, 18, 0, 0);
                CustomFoldersCheckBox.Visibility = Visibility.Visible;
            }
        }

        private void ArchiveButton_Click(object sender, RoutedEventArgs e)
        {
            ArchiveWindow aw = new ArchiveWindow(G_DRIVE + "Software\\" + FilesListBox.SelectedItem.ToString(), ".ASM");
            aw.ShowDialog();
        }

        private void CreatePersonalFile_Click(object sender, RoutedEventArgs e)
        {
            string selectedPath = G_DRIVE + "Software\\" + FilesListBox.SelectedItem.ToString();
            string selectedFolder = General.Get_Folder_From_Path(selectedPath);
            string selectedFile = General.Get_File_From_Path(selectedPath);

            StringBuilder newFileName = new StringBuilder();

            int c = 0;
            while (char.IsLetter(selectedFile[c]))
            {
                newFileName.Append(selectedFile[c]);
                c++;
            }
            newFileName.Append("JAKE");
            while (char.IsNumber(selectedFile[c]))
            {
                //do nothing while looping through the numbers
                c++;
            }
            while (char.IsLetter(selectedFile[c]))
            {
                newFileName.Append(selectedFile[c]);
                c++;
            }
            newFileName.Append(".ASM");

            System.IO.File.Copy(selectedPath, selectedFolder + newFileName, true);

            if (System.Windows.Forms.MessageBox.Show("Remove Drive Option?", "Remove Drive Option", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                Upgrade upgrade = new Upgrade(selectedFolder + newFileName);
                upgrade.No_Version_Upgrade();
                upgrade.Modify_Value("LOBBY:", "21", "AND", "5F");
                upgrade.Write_File(selectedFolder + newFileName);
            }

            string cmd = "C:\\Windows\\explorer.exe";
            string arg = selectedFolder + newFileName;
            Process.Start(cmd, arg);
        }

        private void N_EPLNK_Click(object sender, RoutedEventArgs e)
        {
            string args = "";
            string cmd = @"Y:\N_EPLNK.bat";

            string folder = General.Get_Folder_From_Path(FilesListBox.SelectedItem.ToString());

            string subfolder = folder.Substring(8, folder.Length - 8);
            int slashIndex = subfolder.IndexOf("\\");
            subfolder = subfolder.Substring(0, slashIndex);

            string file = General.Get_File_From_Path(FilesListBox.SelectedItem.ToString());
            int dotIndex = file.IndexOf(".");
            file = file.Substring(0, dotIndex);

            string topVersion = selectedFileVersion.Substring(0, 1);
            string midVersion = selectedFileVersion.Substring(2, 2);
            string botVersion = selectedFileVersion.Substring(5, 1);

            string version = topVersion + "_" + midVersion + " " + botVersion;

            args = Microsoft.VisualBasic.Interaction.InputBox("N_EPLNK args", "N_EPLNK", file + " " + subfolder + " " + version);

            Process proc = Process.Start(cmd, args);
            proc.WaitForExit();

            if (file.ToUpper().StartsWith("G"))
            {
                System.IO.File.Copy(@"C:\EMULATION\TMPMPGRP.BIN", @"C:\EMULATION\" + file + ".BIN", true);
            }
            else
            {
                System.IO.File.Copy(@"C:\EMULATION\TMPMPLCL.BIN", @"C:\EMULATION\" + file + ".BIN", true);
            }
        }

        private void ViewVersionIO_Click(object sender, RoutedEventArgs e)
        {
            List<string> inputLabels = new List<string> { "IOINPE", "IOXINE", "IOIA", "IOELIG" };
            List<string> outputLabels = new List<string> { "IOOUTE", "IOXOUTE", "IOOA" };

            List<string> inputs = content.Build_IOmap(inputLabels);
            List<string> outputs = content.Build_IOmap(outputLabels);

            VersionIO vio = new VersionIO(content.inputs, content.outputs);
            vio.PopulateIO(inputs, "inputs");
            vio.PopulateIO(outputs, "outputs");
            vio.Title = "V" + selectedFileVersion + " Spare Inputs and Outputs";
            vio.Show();
        }

        private void Update_Auto_Updater()
        {
            try
            {
                //To get the location the assembly normally resides on disk or the install directory
                string path = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;

                //once you have the path you get the directory with:
                string modsPath = System.IO.Path.GetDirectoryName(path) + @"\ModHubUpdater.exe";
                modsPath = modsPath.Substring(6, modsPath.Length - 6);

                string updaterPath = @"\\10.113.32.45\shared\Software\Utility\Software Programs and shortcuts\ModHub\ModHubAutoUpgrader\ModHubUpdater.exe";

                System.IO.File.Copy(updaterPath, modsPath, true);
            }
            catch (Exception ex)
            {
                using (System.IO.StreamWriter writefile =
                    new System.IO.StreamWriter(@"\\10.112.10.28\MCE-Rancho\Jake Ball\Error_Log.txt", true))
                {
                    DateTime now = DateTime.Now;
                    writefile.WriteLine("[" + now.ToString() + "] " + Environment.UserName);
                    writefile.WriteLine(ex.ToString() + "\n");
                }
            }
        }

        private void MotionDummyFolder_Click(object sender, RoutedEventArgs e)
        {
            string path = @"G:\Software\MOTION_LINE\";
            string yearSubFolder = TextBox1.Text.Substring(0, 4);
            string jobPath = path + yearSubFolder + "\\" + TextBox1.Text;

            if (!Directory.Exists(jobPath))
            {
                string response = Microsoft.VisualBasic.Interaction.InputBox("What job number can this job Reference?", "Rename File", "");

                if (System.Windows.Forms.MessageBox.Show("There is no existing folder for this job. Would you like to create one?", "Create Job Folder?", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    Directory.CreateDirectory(jobPath);

                    if (response != "")
                    {
                        string referenceYearSubFolder = response.Substring(0, 4);
                        string referenceJobPath = path + referenceYearSubFolder + "\\" + response;
                        CreateShortcut(response + " - Shortcut", jobPath + "\\", referenceJobPath);


                        // Create a file to write to.
                        using (StreamWriter sw = System.IO.File.CreateText(jobPath + @"\Readme.txt"))
                        {
                            DateTime now = DateTime.Now;
                            sw.WriteLine("Date: " + now.ToString("MM-dd-yy"));
                            sw.WriteLine("");
                            sw.WriteLine("The Custom is the same as job " + response.Substring(0, 4) + "-" + response.Substring(4, response.Length - 4));
                        }
                    }
                }
            }

            string cmd = "C:\\Windows\\explorer.exe";
            string arg = jobPath;
            Process.Start(cmd, arg);
        }

        private void KDMFolder_Click(object sender, RoutedEventArgs e)
        {
            //KDM FOLDER
            string kdmpath = @"\\10.113.0.38\mce-public\MCE\Test\Motion Software\Custom Software";
            string kdmjobPath = kdmpath + "\\" + TextBox1.Text;

            if (!Directory.Exists(kdmjobPath))
            {
                if (System.Windows.Forms.MessageBox.Show("There is no existing folder for this job. Would you like to create one?", "Create Job Folder?", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    Directory.CreateDirectory(kdmjobPath);
                }
            }

            string cmd = "C:\\Windows\\explorer.exe";
            string kdmarg = kdmjobPath;
            Process.Start(cmd, kdmarg);
        }

        public static void CreateShortcut(string shortcutName, string shortcutPath, string targetFileLocation)
        {
            string shortcutLocation = System.IO.Path.Combine(shortcutPath, shortcutName + ".lnk");
            WshShell shell = new WshShell();
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutLocation);

            shortcut.TargetPath = targetFileLocation;   // The path of the file that will launch when the shortcut is run
            shortcut.Save();                            // Save the shortcut
        }

        private void Custom_Mod_Click(object sender, RoutedEventArgs e)
        {
            CustomMod cm = new CustomMod(TextBox1.Text);
            cm.Show();
        }

        private void ProgramMotion_Click(object sender, RoutedEventArgs e)
        {
            ProgramMotion pm = new ProgramMotion();
            pm.Show();
        }

        private void BrowseFile_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Multiselect = true;

            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".sdf";

            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();


            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                string[] files = dlg.FileNames;

                foreach(string file in files)
                {
                    string folder = General.Get_Folder_From_Path(file);

                    int locationIndex = folder.IndexOf("Software") + 9;
                    string jobFile = file.Substring(locationIndex, file.Length - locationIndex);

                    FilesListBox.Items.Add(jobFile);

                    string jobNumber = General.Get_Job_Number_From_Path(file);
                    TextBox1.Text = jobNumber;
                }
            }

        }

        private void KDMEmail_Click(object sender, RoutedEventArgs e)
        {
            string jobNumber = TextBox1.Text.Substring(5, 5);
            string jobYear = TextBox1.Text.Substring(2, 2);
            string kdmpath = @"\\10.113.0.38\mce-public\MCE\Test\Motion Software\Custom Software";
            string kdmjobPath = kdmpath + "\\" + TextBox1.Text;

            Outlook.Application app = new Outlook.Application();
            string body = "Alan and Eliud,\n\n" +
                "Custom logic for the subject job is ready and the files are in the Custom Software folder on the KdM drive.\n\n";
            body += kdmjobPath + "\n";
            body += "Thanks,\nJake";
            string subject = "Job " + jobYear + "-" + jobNumber;
            string to = "alan.aranda@nidec-mce.com;eliud.jimenez@nidec-mce.com";
            string cc = "emilio.garza@nidec-mce.com;bart.lewalski@nidec-mce.com;jim.stuart@nidec-mce.com";
            Outlook.MAPIFolder sentContacts = (Outlook.MAPIFolder)
                 app.ActiveExplorer().Session.GetDefaultFolder
                 (Outlook.OlDefaultFolders.olFolderContacts);


            CreateEmailItem(subject, to, cc, body, app);
        }

        private void CreateEmailItem(string subjectEmail, string toEmail, string ccEmail, string bodyEmail, Outlook.Application app)
        {
            Outlook.MailItem eMail = (Outlook.MailItem)
                app.CreateItem(Outlook.OlItemType.olMailItem);
            eMail.Subject = subjectEmail;
            eMail.To = toEmail;
            eMail.CC = ccEmail;
            eMail.Body = bodyEmail;
            eMail.Importance = Outlook.OlImportance.olImportanceNormal;
            ((Outlook._MailItem)eMail).Display();
        }

        private void Export_Excel_Click(object sender, RoutedEventArgs e)
        {

            System.Windows.Forms.SaveFileDialog saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();

            saveFileDialog1.Filter = "Excel (*.xlsx)|*.xlsx|All files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.RestoreDirectory = true;
            saveFileDialog1.FileName = General.Get_Job_Number_From_Path(FilesListBox.SelectedItem.ToString(), true);

            if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Excel.Application excel;
                Excel.Workbook workbook;
                Excel.Worksheet worksheet;

                excel = new Excel.Application();
                excel.Visible = false;
                excel.DisplayAlerts = false;
                workbook = excel.Workbooks.Add(Type.Missing);

                //LANDINGS WORKSHEET
                worksheet = (Excel.Worksheet)workbook.ActiveSheet;
                worksheet.Name = "Job Info";

                Excel.Range jobinforange = worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[47, 6]];
                jobinforange.Merge();
                jobinforange.Cells.VerticalAlignment = Excel.XlVAlign.xlVAlignTop;
                worksheet.Cells[1, 1] = JobInfo.Text;
                worksheet.Cells.Font.Size = 11;

                string[] levels;

                if (LandingPIs.Visibility == Visibility.Visible)
                {
                    levels = LandingPIs.Text.Split('\n');
                }
                else
                {
                    levels = LandingLevels.Text.Split('\n');
                }
                string[] normalconfig = LandingNormalConfig.Text.Split('\n');
                string[] altconfig = LandingAltConfig.Text.Split('\n');


                worksheet.Cells[1, 7] = "Landing";
                worksheet.Cells[1, 8] = "Normal";

                if (LandingAltConfig.Visibility == Visibility.Visible)
                {
                    worksheet.Cells[1, 9] = "Alternate";
                }

                Excel.Range LandingRange = worksheet.Range[worksheet.Cells[1, 7], worksheet.Cells[levels.Length + 1, 9]];

                LandingRange.Cells.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

                for (int level = 0; level < levels.Length; level++)
                {
                    worksheet.Cells[2 + level, 7] = levels[level];
                    if (level < normalconfig.Length)
                    {
                        worksheet.Cells[2 + level, 8] = normalconfig[level].Replace("\t", "     ");
                    }
                    if (level < altconfig.Length)
                    {
                        worksheet.Cells[2 + level, 9] = altconfig[level].Replace("\t", "     ");
                    }
                }

                //INPUTS & OUTPUTS WORKSHEET
                Excel.Worksheet ioworksheet;
                ioworksheet = workbook.Sheets.Add(After: workbook.Sheets[workbook.Sheets.Count]);
                ioworksheet.Name = "Inputs & Outputs";
                ioworksheet.Cells.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                List<string> inputs = content.inputs;
                List<string> outputs = content.outputs;

                Excel.Range inputheader = ioworksheet.Range[ioworksheet.Cells[1, 1], ioworksheet.Cells[1, 8]];
                inputheader.Merge();
                ioworksheet.Cells[1, 1] = "SPARE INPUTS";

                int inputCounter = 0;
                int rowCounter = 2;
                while (inputCounter < inputs.Count)
                {
                    for (int c = 8; c > 0; c--)
                    {
                        if (inputCounter < inputs.Count)
                        {
                            ioworksheet.Cells[rowCounter, c] = inputs[inputCounter];
                            inputCounter++;
                        }
                    }
                    rowCounter++;
                }


                Excel.Range outputheader = ioworksheet.Range[ioworksheet.Cells[rowCounter, 1], ioworksheet.Cells[rowCounter, 8]];
                outputheader.Merge();
                ioworksheet.Cells[rowCounter, 1] = "SPARE OUTPUTS";

                rowCounter++;

                int outputCounter = 0;
                while (outputCounter < outputs.Count)
                {
                    for (int c = 1; c < 9; c++)
                    {
                        if (outputCounter < outputs.Count)
                        {
                            ioworksheet.Cells[rowCounter, c] = outputs[outputCounter];
                            outputCounter++;
                        }
                    }
                    rowCounter++;
                }

                //I40 & IOX WORKSHEET
                Excel.Worksheet i4oioxworksheet;
                i4oioxworksheet = workbook.Sheets.Add(After: workbook.Sheets[workbook.Sheets.Count]);
                i4oioxworksheet.Name = "I40 & IOX";
                i4oioxworksheet.Cells.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;


                Excel.Range farleftColumn = i4oioxworksheet.Range[i4oioxworksheet.Cells[1, 1], i4oioxworksheet.Cells[1, 1]];
                Excel.Range leftpaddingRange = i4oioxworksheet.Range[i4oioxworksheet.Cells[1, 2], i4oioxworksheet.Cells[1, 2]];
                Excel.Range rightpaddingRange = i4oioxworksheet.Range[i4oioxworksheet.Cells[1, 11], i4oioxworksheet.Cells[1, 11]];
                farleftColumn.ColumnWidth = 2.14;
                leftpaddingRange.ColumnWidth = 2.14;
                rightpaddingRange.ColumnWidth = 2.14;

                rowCounter = 2;
                int columnCounter = 3;

                foreach (Border border in BoardSP.Children)
                {
                    int startingRow = rowCounter;

                    StackPanel boardsp = border.Child as StackPanel;
                    foreach (var child in boardsp.Children)
                    {
                        if (child.GetType() == typeof(Label))
                        {
                            Label label = child as Label;

                            i4oioxworksheet.Cells[rowCounter, 3] = label.Content;
                            Excel.Range labelRange = i4oioxworksheet.Range[i4oioxworksheet.Cells[rowCounter, 3], i4oioxworksheet.Cells[rowCounter, 10]];
                            labelRange.Merge();
                            labelRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignLeft;
                            labelRange.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                            labelRange.RowHeight = 24;
                            rowCounter++;
                        }
                        if (child.GetType() == typeof(StackPanel))
                        {
                            StackPanel sp = child as StackPanel;
                            columnCounter = 3;
                            foreach (TextBox tb in sp.Children)
                            {
                                i4oioxworksheet.Cells[rowCounter, columnCounter] = tb.Text;
                                Excel.Range ioRange = i4oioxworksheet.Range[i4oioxworksheet.Cells[rowCounter, columnCounter], i4oioxworksheet.Cells[rowCounter, columnCounter]];
                                ioRange.BorderAround2(Excel.XlLineStyle.xlContinuous, Excel.XlBorderWeight.xlMedium);
                                columnCounter++;
                            }
                            rowCounter++;
                        }
                    }

                    Excel.Range botpaddingRange = i4oioxworksheet.Range[i4oioxworksheet.Cells[rowCounter, 2], i4oioxworksheet.Cells[rowCounter, 11]];
                    botpaddingRange.Merge();
                    botpaddingRange.RowHeight = 15;

                    rowCounter++;

                    Excel.Range boardRange = i4oioxworksheet.Range[i4oioxworksheet.Cells[startingRow, 2], i4oioxworksheet.Cells[rowCounter - 1, 11]];
                    boardRange.BorderAround2(Excel.XlLineStyle.xlContinuous, Excel.XlBorderWeight.xlMedium);

                    rowCounter++;
                }

                //HEADERS WORKSHEET
                Excel.Worksheet headersworksheet;
                headersworksheet = workbook.Sheets.Add(After: workbook.Sheets[workbook.Sheets.Count]);
                headersworksheet.Name = "Headers";
                headersworksheet.Cells.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

                rowCounter = 2;
                columnCounter = 2;
                foreach (StackPanel childSP in HeaderSP.Children)
                {
                    foreach (TextBox tb in childSP.Children)
                    {
                        headersworksheet.Cells[rowCounter, columnCounter] = tb.Text;
                        Excel.Range headerRange = headersworksheet.Range[headersworksheet.Cells[rowCounter, columnCounter], headersworksheet.Cells[rowCounter, columnCounter]];
                        headerRange.BorderAround2(Excel.XlLineStyle.xlContinuous, Excel.XlBorderWeight.xlMedium);
                        rowCounter++;
                    }
                    rowCounter = 2;
                    columnCounter++;

                    Excel.Range botpaddingRange = headersworksheet.Range[headersworksheet.Cells[1, columnCounter], headersworksheet.Cells[1, columnCounter]];
                    botpaddingRange.ColumnWidth = 2.14;

                    columnCounter++;
                }


                workbook.Sheets[1].Select();
                workbook.SaveAs(saveFileDialog1.FileName);
                workbook.Close();
                excel.Quit();

                string cmd = "C:\\Windows\\explorer.exe";
                string arg = saveFileDialog1.FileName;
                Process.Start(cmd, arg);
            }
        }

        private void Populate_PTHC()
        {
            List<string> prfiles = Directory.GetFiles(@"G:\Software\Programming_Record_Templetes\PTHC\PRECORD", " *.afm").ToList();

            foreach (string file in prfiles)
            {
                PTHCProgRecordComboBox.Items.Add(file);
            }
        }

        private void PTHC_Prog_Record(object sender, RoutedEventArgs e)
        {
            string fullNumber = TextBox1.Text;
            string jobNumber = TextBox1.Text.Substring(5, 5);
            string jobYear = TextBox1.Text.Substring(0, 4);

            string jobFolder = G_DRIVE + @"Test Dept\Controller Data\" + jobYear + "\\" + fullNumber;
            string kdmFolder = G_DRIVE + @"Test Dept\Controller Data\KDM\" + jobYear + "\\" + fullNumber;

            if (!Directory.Exists(jobFolder) && !Directory.Exists(kdmFolder))
            {
                if (System.Windows.Forms.MessageBox.Show("There is no folder, would you like to create one?", "Create Folder?", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    if (System.Windows.Forms.MessageBox.Show("KDM Job?", "KDM Job?", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                    {
                        Directory.CreateDirectory(jobFolder);
                    }
                    else
                    {
                        Directory.CreateDirectory(kdmFolder);
                    }
                }
                else
                {
                    return;
                }
            }

            if (Directory.Exists(jobFolder))
            {
                System.IO.File.Copy(@"G:\Software\Programming_Record_Templetes\PTHC\PRECORD\" + PTHCProgRecordComboBox.SelectedItem.ToString(), jobFolder + "\\" + fullNumber + ".afm");
            }

            if (Directory.Exists(kdmFolder))
            {

            }

        }

        private void PTHC_Mod_Doc(object sender, RoutedEventArgs e)
        {

        }

        private void DifferentJobNumber_Click(object sender, RoutedEventArgs e)
        {
            string response = Microsoft.VisualBasic.Interaction.InputBox("What Job Directory would you like to search?\nPlease look at the 'Summary' tab for information on where this job could be saved\n\nPlease enter a job number", "Which Job Directory", "");

            if(response != "")
            {
                string[] custom_locations = new string[] { "MC-MP\\MPODH\\" + response, "MC-MP\\MPODT\\" + response, "MC-MP\\MPOGD\\" + response, "MC-MP\\MPOGM\\" + response, "MC-MP\\MPOLHD\\" + response, "MC-MP\\MPOLHM\\" + response, "MC-MP\\MPOLOM\\" + response, "MC-MP\\MPOLTD\\" + response, "MC-MP\\MPOLTM\\" + response };
                string[] custom2_locations = new string[] { response };

                int cust1Num = SearchLocation(custom_locations, "Custom");
                int cust2Num = SearchLocation(custom2_locations, "Custom2");

                MessageBox.Show((cust1Num + cust2Num) + " file(s) found");
            }
        }
    }
}
