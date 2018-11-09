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
using System.Threading;

namespace mods
{
    public partial class MainWindow : Window
    {
        bool blockSearchHistoryChange = false;
        string version = "V1.04.2";
        int permission = 1000;
        int searchProgress = 0;
        List<string> Trac_Mod_Jobs = new List<string>();
        List<string> Motion_Values = new List<string>();
        List<string> Motion_Options = new List<string>();
        string G_DRIVE = @"G:\";
        string file = "";
        Controller controller;
        string jobNumber = "";
        string fileExtension = "";
        private static Semaphore _search;

        public MainWindow()
        {
            try
            {
                InitializeComponent();

                Set_Permissions();            

                this.Title = "Modification Hub by Jake Ball " + version;
            

                if (Version_Check())
                {
                    Update_Auto_Updater();
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

                try
                {
                    FilesListBox.SelectionMode = SelectionMode.Extended;
                    FileExtension.SelectedIndex = 0;
                    Make_Controls_Invisible();
                    Update_Search_History();
                    Legacy_Controls_Visible();
                }
                catch(Exception ex)
                {
                    Write_Error_To_Log("Visibility Setup", ex);
                }

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
            catch (Exception ex)
            {
                Write_Error_To_Log("Initialize Component", ex);
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SearchButton.IsEnabled = false;

            if(TextBox1.Text.Contains("-"))
            {
                int dashindex = TextBox1.Text.IndexOf("-");

                TextBox1.Dispatcher.Invoke(() => TextBox1.Text = TextBox1.Text.Substring(dashindex + 1, TextBox1.Text.Length - dashindex - 1), DispatcherPriority.Background);
            }

            SearchFiles();
            blockSearchHistoryChange = true;
            Update_Search_History();
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
                        arg = item.ToString();
                    }
                    else
                    {
                        arg = G_DRIVE + "Software\\" + item.ToString();
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
                /*if (System.Windows.Forms.MessageBox.Show("Would you like to generate a file to import?", "Generate File to Import?", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    ModDoc md = new ModDoc(this.FilesListBox);
                    md.ShowDialog();
                }
                */
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
            string cmd = @"Y:\EPRLNK7";
            string args = "";

            Controller temp_controller;
            string jobNum = "";

            if(controller == null)
            {

                jobNum =  Microsoft.VisualBasic.Interaction.InputBox("Job Number?", "Job Number?", "").ToUpper();
                if(jobNum.StartsWith("C"))
                {
                    if(System.IO.File.Exists(G_DRIVE + @"Software\Product\MP2COC\" + jobNum + ".asm"))
                    {
                        temp_controller = new Local(@"Product\MP2COC\" + jobNum + ".asm");
                    }
                    else
                    {
                        MessageBox.Show("File not found");
                        return;
                    }
                }
                else if(jobNum.StartsWith("G"))
                {
                    if (System.IO.File.Exists(G_DRIVE + @"Software\Product\MP2OGM\" + jobNum + ".asm"))
                    {
                        temp_controller = new Local(@"Product\MP2OGM\" + jobNum + ".asm");
                    }
                    else
                    {
                        MessageBox.Show("File not found");
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("File not found");
                    return;
                }
            }
            else
            {
                jobNum = General.Get_Job_Number_From_Path(FilesListBox.SelectedItem.ToString());

                temp_controller = controller;
            }

            string version = temp_controller.versionTop + "_" + temp_controller.versionMid + " " + temp_controller.versionBot;

            if (jobNum.StartsWith("C"))
            {
                args = file + " " + "MP2COC" + " " + version;
            }
            else if (jobNum.StartsWith("G"))
            {
                args = file + " " + "MP2OGM" + " " + version;
            }
            else
            {
                MessageBox.Show("File not found");
                return;
            }

            var startInfo = new ProcessStartInfo();
            startInfo.WorkingDirectory = G_DRIVE + "Software\\Product";
            startInfo.Arguments = args;
            startInfo.FileName = cmd;
            
            Process proc = Process.Start(startInfo);
            proc.WaitForExit();

            //Process.Start("Y:\\MP2Link.exe");
        }

        private void SearchFiles()
        {
            Make_Controls_Invisible();

            FilesListBox.Items.Clear();

            _search = new Semaphore(0, 100);

            if (TextBox1.Text == "")
            {
                MessageBox.Show("Please enter a job number into the search bar");
                return;
            }
            else
            {
                jobNumber = TextBox1.Text;
                fileExtension = FileExtension.SelectedItem.ToString();
            }

            searchProgress = 0;
            SearchProgress.Dispatcher.Invoke(() => SearchProgress.Value = searchProgress, DispatcherPriority.Background);

            if (FileExtension.SelectedItem.ToString() == "DDP")
            {
                FilesListBox.Items.Clear();
                string jobNumber = "*" + TextBox1.Text + "*";

                string folder = G_DRIVE + "Software\\Product\\MASTER.BIN\\DDP";
                string[] files = Directory.GetFiles(@folder, jobNumber, SearchOption.AllDirectories);

                foreach(string file in files)
                {
                    FilesListBox.Items.Add(file);
                }
            }
            else if (FileExtension.SelectedItem.ToString() != "Motion") //Legacy Job
            {
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

                string[] custom_locations = new string[] { "MC-MP\\MPODH\\", "MC-MP\\MPODT\\", "MC-MP\\MPOGD\\", "MC-MP\\MPOGM\\", "MC-MP\\MPOLHD\\", "MC-MP\\MPOLHM\\", "MC-MP\\MPOLOM\\", "MC-MP\\MPOLTD\\", "MC-MP\\MPOLTM\\" };
                string[] custom2_locations = new string[] { "" };

                if(AllJobNumbersCheckBox.IsChecked == false)
                {
                    for (int i = 0; i < custom_locations.Length; i++)
                    {
                        custom_locations[i] += jobNumber;
                    }
                    for (int i = 0; i < custom2_locations.Length; i++)
                    {
                        custom2_locations[i] += jobNumber;
                    }
                }
                SearchProgress.Maximum = 0;
                SearchProgress.Maximum += Directory.GetDirectories(G_DRIVE + "Software\\Product\\").Length;


                SearchProgress.Maximum += Directory.GetDirectories(G_DRIVE + "Software\\Source\\MC-MP\\").Length;
                SearchProgress.Maximum += Directory.GetDirectories(G_DRIVE + "Software\\Source\\MC-MP2\\").Length;
                if(AllJobNumbersCheckBox.IsChecked == true)
                {
                    SearchProgress.Maximum += Directory.GetDirectories(G_DRIVE + "Software\\Custom\\MC-MP\\").Length * 100;
                    SearchProgress.Maximum += Directory.GetDirectories(G_DRIVE + "Software\\Custom2\\").Length;
                    //SearchProgress.Maximum += 1;
                }
                else
                {
                    SearchProgress.Maximum += 9; //9 Custom Locations
                    SearchProgress.Maximum += 1; //1 Custom2 Location
                    SearchProgress.Maximum += 10; //PUBLIK Locations
                }

                Thread publik = new Thread(() => SearchLocation(G_DRIVE + "Software\\Publik\\", 10));
                publik.Start();

                foreach (string directory in Directory.GetDirectories(G_DRIVE + "Software\\Source\\MC-MP\\"))
                {
                    Thread t = new Thread(() => SearchLocation(directory));
                    t.Start();
                }

                foreach (string directory in Directory.GetDirectories(G_DRIVE + "Software\\Source\\MC-MP2\\"))
                {
                    Thread t = new Thread(() => SearchLocation(directory));
                    t.Start();
                }

                if (AllJobNumbersCheckBox.IsChecked == false)
                {
                    Thread t = new Thread(() => SearchLocation(G_DRIVE + "Software\\Custom2\\" + jobNumber));
                    t.Start();
                }
                else
                {
                    foreach (string directory in Directory.GetDirectories(G_DRIVE + "Software\\Custom2\\"))
                    {
                        Thread t = new Thread(() => SearchLocation(directory));
                        t.Start();
                    }
                }

                foreach (string directory in Directory.GetDirectories(G_DRIVE + "Software\\Custom\\MC-MP\\"))
                {
                    if (AllJobNumbersCheckBox.IsChecked == false)
                    {
                        Thread t = new Thread(() => SearchLocation(directory + "\\" + jobNumber));
                        t.Start();
                    }
                    else
                    {
                        Thread t = new Thread(() => SearchLocation(directory, 100));
                        t.Start();
                    }
                }

                foreach (string directory in Directory.GetDirectories(G_DRIVE + "Software\\Product\\"))
                {
                    if (!directory.Contains("MASTER.BIN"))
                    {
                        Thread t = new Thread(() => SearchLocation(directory));
                        t.Start();
                    }
                    else
                    {
                        searchProgress++;
                        SearchProgress.Dispatcher.Invoke(() => SearchProgress.Value = searchProgress, DispatcherPriority.Background);
                    }
                }

                _search.Release(100);

                /*if (FilesListBox.Items.Count < 1)
                {
                    JobInfo.Visibility = Visibility.Visible;
                    JobInfo.Text = "No preview available for this job.\n";
                    JobInfo.Text += "This job may be custom and under a different Job Number.\n";
                    JobInfo.Text += "Please consult the Software Department for more info on this job.";
                }*/

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

        private void Remove_From_Files_List(object sender, RoutedEventArgs e)
        {
            if(FilesListBox.SelectedIndex != -1)
            {
                FilesListBox.Items.Remove(FilesListBox.SelectedItem);
            }
        }

        private void Clear_Files_List(object sender, RoutedEventArgs e)
        {
            FilesListBox.Items.Clear();
        }

        private void Reload_File(object sender, RoutedEventArgs e)
        {
            Generate_JobInfo(FilesListBox.SelectedItem.ToString());
        }

        private int SearchLocation(string folder, int weight = 1)
        {
            int validNum = 0;

            _search.WaitOne();

            if(Directory.Exists(folder))
            {
                try
                {
                    string jobNumber = "*" + this.jobNumber + "*";
                    string[] files = Directory.GetFiles(@folder, jobNumber, SearchOption.AllDirectories);
                    foreach (string file in files)
                    {
                        bool validFile = false;
                        string fileExtension = General.Get_FileExtension_From_Path(file).ToLower();

                        if (this.fileExtension == ".asm")
                        {
                            if (fileExtension == ".asm" || fileExtension == "")
                            {
                                validFile = true;
                            }
                        }
                        else if (this.fileExtension == ".old")
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
                        if (this.jobNumber.ToUpper() != General.Get_Job_Number_From_Path(file))
                        {
                            validFile = false;
                        }

                        if (validFile)
                        {
                            int locationIndex = folder.IndexOf("Software") + 9;
                            string jobFile = file.Substring(locationIndex, file.Length - locationIndex);
                            FilesListBox.Dispatcher.Invoke(() => FilesListBox.Items.Add(jobFile), DispatcherPriority.Background);
                            validNum++;
                        }
                    }
                }
                catch
                {

                }
            }

            searchProgress += weight;
            SearchProgress.Dispatcher.Invoke(() => SearchProgress.Value = searchProgress, DispatcherPriority.Background);

            _search.Release();

            return validNum;
        }

        private void LDrive_Click(object sender, RoutedEventArgs e)
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

        private bool Job_Info()
        {
            //Job Summary
            try
            {
                controller.Job_Summary();
            }
            catch (Exception ex)
            {
                Write_Error_To_Log(file, ex);
                JobSummary.Text = "Job Summary could not be created for this file";
                Dispatcher.BeginInvoke((Action)(() => InfoTabControl.SelectedIndex = 3));
            }

            try
            {
                if (controller.content.content.IndexOf("END") == -1)
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

                controller.Job_Info();
            }
            catch (Exception ex)
            {
                Write_Error_To_Log(file, ex);
                JobInfo.Text = "Job Info could not be created for this file";
            }

            //Options
            try
            {
                controller.Options();
            }
            catch (Exception ex)
            {
                Write_Error_To_Log(file, ex);
                LobbyOptionsBlock.Text = "There was an issue generating options for this file";
            }

            //Landings
            try
            {
                controller.Draw_Landing_Preview();
            }
            catch (Exception ex)
            {
                Write_Error_To_Log(file, ex);
            }

            //Inputs and Outputs
            try
            {
                controller.Generate_IO();
            }
            catch (Exception ex)
            {
                Write_Error_To_Log(file, ex);
            }

            //Headers
            try
            {
                controller.Generate_Headers();
            }
            catch (Exception ex)
            {
                Write_Error_To_Log(file, ex);
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
                controller = new Group(file);
            }
            else if(file.Contains("MPODT"))
            {
                controller = new Simplex(file);
            }
            else
            {
                controller = new Local(file);
            }

            try
            {
                Job_Info();
                return true;
            }
            catch (Exception ex)
            {
                Write_Error_To_Log(file, ex);
                JobInfo.Text = "Job Info could not be generated for this file.";
                return false;
            }

        }

        private void FilesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(FileExtension.SelectedItem.ToString() != "DDP")
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

        private void TracJobRefresh_Click(object sender, RoutedEventArgs e)
        {
            //OPEN EXCEL
            Excel.Application xlApp = new Excel.Application();
            xlApp.Visible = false;
            Excel.Workbook xlWorkbook = xlApp.Workbooks.Open("F:\\Software\\Product\\Trac_Mod.xlsm", 0, true);
            Excel._Worksheet xlWorksheet = xlWorkbook.Sheets[1];
            Excel._Worksheet dlmWorksheet = xlWorkbook.Sheets[2];
            Excel.Range xlRange = xlWorksheet.UsedRange;
            Excel.Range dlmRange = dlmWorksheet.UsedRange;


            for (int row = 4; row < 100; row++)
            {
                string notificationNumber = "";
                string jobNumber = "";

                if (xlRange.Cells[row, 5].Value2 != null)
                {
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

                    Trac_Job(jobNumber, notificationNumber);
                }

            }

            for (int row = 4; row < 100; row++)
            {
                string notificationNumber = "";
                string jobNumber = "";

                if (dlmRange.Cells[row, 5].Value2 != null)
                {
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

                    Trac_Job(jobNumber, notificationNumber);
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

        private void Trac_Job(string jobNumber, string notificationNumber)
        {
            List<string> files = Broad_Job_Search(jobNumber);

            //OPEN EXCEL
            Excel.Application xlApp = new Excel.Application();
            xlApp.Visible = false;
            Excel.Workbook xlWorkbook = xlApp.Workbooks.Open("F:\\Software\\Product\\Trac_Job.xlsx", 0, true);
            Excel._Worksheet xlWorksheet = xlWorkbook.Sheets[1];
            Excel.Range xlRange = xlWorksheet.UsedRange;

            bool jobFound = false;

            int row = 2;

            while(xlRange.Cells[row, 1].Value2 != null)
            {
                if (xlRange.Cells[row, 1].Value2 == jobNumber)
                {
                    jobFound = true;
                }

                row++;
            }

            if(!jobFound)
            {
                files = Broad_Job_Search(jobNumber);

                xlWorksheet.Cells[row, 1] = JobInfo.Text;
            }


            xlWorkbook.Save();


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

        private List<string> Broad_Job_Search(string jobNumber)
        {
            List<string> locations = new List<string> { "Software\\Publik\\", "Software\\Source\\MC-MP\\", "Software\\Source\\MC-MP2\\", "Software\\Custom2\\", "Software\\Custom\\MC-MP\\", "Software\\Product\\" };

            List<Thread> threads = new List<Thread>();

            List<string> files = new List<string>();

            foreach(string location in locations)
            {
                foreach(string directory in Directory.GetDirectories(G_DRIVE + location))
                {
                    if(!directory.Contains("MASTER.BIN"))
                    {
                        Thread t = new Thread(
                            () =>
                            {
                                files.AddRange(Find_Job(directory, jobNumber));
                            });
                        t.Start();
                        threads.Add(t);
                    }
                }
            }

            foreach(var thread in threads)
            {
                thread.Join();
            }

            return files;
        }

        private List<string> Find_Job(string location, string jobNumber)
        {
            List<string> files_list = new List<string>();

            try
            {
                string jobNum = jobNumber.Substring(3, jobNumber.Length - 3);
                string[] files = Directory.GetFiles(@location, "*" + jobNum + "*", SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    bool validFile = false;

                    string fileExtension = General.Get_FileExtension_From_Path(file);

                    if (jobNum.ToUpper() == General.Get_Job_Number_From_Path(file))
                    {
                        if (fileExtension.ToUpper() == ".ASM" || fileExtension == "")
                        {
                            validFile = true;
                        }
                    }
                    
                    if (validFile)
                    {
                        int locationIndex = location.IndexOf("Software") + 9;
                        string jobFile = file.Substring(locationIndex, file.Length - locationIndex);
                        files_list.Add(jobFile);
                    }
                }
            }
            catch
            {

            }

            return files_list;
        }

        private void Write_Error_To_Log(string file, Exception ex)
        {
            try
            {
                using (System.IO.StreamWriter writefile =
                    new System.IO.StreamWriter(@"\\10.112.10.28\MCE-Rancho\Jake Ball\Error_Log.txt", true))
                {
                    DateTime now = DateTime.Now;
                    writefile.WriteLine("Modhub[" + now.ToString() + "] " + Environment.UserName);
                    writefile.WriteLine(file);
                    writefile.WriteLine(ex.ToString() + "\n");
                }
            }
            catch
            {

            }

            try
            {
                //To get the location the assembly normally resides on disk or the install directory
                string path = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;

                //once you have the path you get the directory with:
                string directory = System.IO.Path.GetDirectoryName(path);

                string logpath = directory.Substring(6, directory.Length - 6) + "\\Error_Log.txt";

                using (System.IO.StreamWriter writefile =
                        new System.IO.StreamWriter(logpath, true))
                {
                    DateTime now = DateTime.Now;
                    writefile.WriteLine("[" + now.ToString() + "] " + Environment.UserName);
                    writefile.WriteLine(file);
                    writefile.WriteLine(ex.ToString() + "\n");
                }
            }
            catch
            {

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


                if (FileExtension.SelectedItem.ToString() == "Motion" || FileExtension.SelectedItem.ToString() == "DDP")
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
            //AdvancedSearch.Visibility = Visibility.Hidden;
            LDrive.Visibility = Visibility.Hidden;
        }

        private void Legacy_Controls_Visible()
        {
            KDMFolder.Visibility = Visibility.Hidden;
            MotionDummyFolder.Visibility = Visibility.Hidden;
            KDMEmail.Visibility = Visibility.Hidden;

            ExportExcel.Visibility = Visibility.Visible;
            //AdvancedSearch.Visibility = Visibility.Visible;
            LDrive.Visibility = Visibility.Visible;
        }

        private bool Version_Check()
        {
            try
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
            catch(Exception ex)
            {
                Write_Error_To_Log("Version Check", ex);
            }
            return false;
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
            try
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
                    FileExtension.Items.Add("DDP");
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
                    AdvancedSearch.Visibility = Visibility.Hidden;

                    LDrive.Margin = new Thickness(LDrive.Margin.Left, LDrive.Margin.Top - 51, LDrive.Margin.Right, LDrive.Margin.Bottom);
                    ExportExcel.Margin = new Thickness(ExportExcel.Margin.Left, ExportExcel.Margin.Top - 51, ExportExcel.Margin.Right, ExportExcel.Margin.Bottom);
                    //AdvancedSearch.Margin = new Thickness(AdvancedSearch.Margin.Left, AdvancedSearch.Margin.Top - 51, AdvancedSearch.Margin.Right, AdvancedSearch.Margin.Bottom);

                    G_DRIVE = @"\\10.113.32.45\shared\";
                }

                if (permission > 0)
                {
                    AdminTab.Visibility = Visibility.Hidden;
                }
            }
            catch(Exception ex)
            {
                Write_Error_To_Log("Set Permissions", ex);
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
            TracModFilter_TextChanged(null, null);
        }

        private void ModUpgrade_Click(object sender, RoutedEventArgs e)
        {
            string jobnum = General.Get_Job_Number_From_Path(FilesListBox.SelectedItem.ToString());

            UpgradeWindow uw = new UpgradeWindow(jobnum);

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
            }
            else
            {
                InfoTabControl.Margin = new Thickness(413, 18, 0, 0);
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

            string topVersion = controller.fileVersion.Substring(0, 1);
            string midVersion = controller.fileVersion.Substring(2, 2);
            string botVersion = controller.fileVersion.Substring(5, 1);

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
            List<string> inputs = controller.content.Build_IOmap(controller.content.inputLabels);
            List<string> outputs = controller.content.Build_IOmap(controller.content.outputLabels);

            VersionIO vio = new VersionIO(controller.content.inputs, controller.content.outputs);
            vio.PopulateIO(inputs, "inputs");
            vio.PopulateIO(outputs, "outputs");
            vio.Title = "V" + controller.fileVersion + " Spare Inputs and Outputs";
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
                List<string> inputs = controller.content.inputs;
                List<string> outputs = controller.content.outputs;

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
            AllJobNumbersCheckBox.IsChecked = true;
            SearchFiles();
        }

        private void Job_Prints_Click(object sender, RoutedEventArgs e)
        {
            ModPrintsBox.Items.Clear();
            string notif = Microsoft.VisualBasic.Interaction.InputBox("Notif #?", "Notification #", "");
            string jobNumber = Microsoft.VisualBasic.Interaction.InputBox("Job #?", "Job #", "");
            string engineer = Microsoft.VisualBasic.Interaction.InputBox("Engineer?", "Engineer", "");

            if (notif != "")
            {
                PrintsFolder.IsEnabled = false;
                Find_Mod_Notif_Folder(@"F:\!!Mods Cabinet\Review Pending (Do not move to L)", notif, jobNumber, engineer);
                //test(@"F:\!!Mods Cabinet\Review Pending (Do not move to L)", notif, jobNumber, engineer);
            }
            PrintsFolder.IsEnabled = true;
        }

        private void test(string dir, string notif, string jobNumber, string engineer)
        {
            foreach(string engineerDir in Directory.GetDirectories(dir))
            {
                if(engineer == "" || engineerDir.Contains(engineer))
                {
                    try
                    {
                        foreach (string jobDir in Directory.GetDirectories(engineerDir))
                        {
                            if (jobNumber == "" || jobDir.Contains(jobNumber))
                            {
                                try
                                {
                                    if (Directory.Exists(jobDir + "\\Documents"))
                                    {
                                        foreach (string notifDir in Directory.GetDirectories(jobDir + "\\Documents"))
                                        {
                                            try
                                            {
                                                if (notif == "" || notifDir.Contains(notif))
                                                {
                                                    ModPrintsBox.Items.Add(notifDir);
                                                }
                                            }catch{}
                                        }
                                    }
                                }catch{}
                            }
                        }
                    }catch{}
                }
            }
        }

        private void Find_Mod_Notif_Folder(string dir, string notif, string jobNumber, string engineer)
        {
            try
            {
                foreach (string subDir in Directory.GetDirectories(dir))
                {
                    Console.WriteLine(subDir);
                    if (!subDir.Substring(30, subDir.Length - 30).Contains("!"))
                    {
                        if (engineer != "")
                        {
                            if (subDir.Contains(engineer))
                            {
                                if (jobNumber != "")
                                {
                                    if (subDir.Contains(jobNumber))
                                    {
                                        if (subDir.Contains(notif))
                                        {
                                            ModPrintsBox.Items.Add(subDir);
                                        }
                                    }
                                }
                                else
                                {
                                    if (subDir.Contains(notif))
                                    {
                                        ModPrintsBox.Items.Add(subDir);
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (jobNumber != "")
                            {
                                if (subDir.Contains(jobNumber))
                                {
                                    if (subDir.Contains(notif))
                                    {
                                        ModPrintsBox.Items.Add(subDir);
                                    }
                                }
                            }
                            else
                            {
                                if (subDir.Contains(notif))
                                {
                                    ModPrintsBox.Items.Add(subDir);
                                }
                            }
                        }
                        Find_Mod_Notif_Folder(subDir, notif, jobNumber, engineer);
                    }
                }
            }
            catch
            {

            }
        }

        private void Open_Mod_Folder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string cmd = "C:\\Windows\\explorer.exe";
                string arg = ModPrintsBox.SelectedItem.ToString();
                Process.Start(cmd, arg);
            }
            catch
            {
                MessageBox.Show("Please Select an Item from the List");
            }
        }
    }
}
