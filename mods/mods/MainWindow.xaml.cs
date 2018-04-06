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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.IO;
using System.ComponentModel;
using System.Windows.Threading;
using Excel = Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media;
using System.Printing;

namespace mods
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool blockSearchHistoryChange = false;
        string version = "V1.02.4";
        int permission = 1000;
        int searchProgress = 0;
        List<string> Trac_Mod_Jobs = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
            
            Set_Permissions();
            
            this.Title = "Modification Hub by Jake Ball " + version;

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

            Username.Text = Properties.Settings.Default.Username;
            CustomFoldersCheckBox.IsChecked = true;
            FilesListBox.SelectionMode = SelectionMode.Extended;
            FileExtension.Items.Add(".asm");
            if(permission < 2)
            {
                FileExtension.Items.Add(".old");
                FileExtension.Items.Add("All Files");
            }
            FileExtension.SelectedIndex = 0;
            Make_Controls_Invisible();
            Update_Search_History();
            if(permission <= 1)
            {
                Track_Mod();
            }
                
            try
            {
                if(SearchHistory.Items[1].ToString().StartsWith("-"))
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
            blockSearchHistoryChange = true;
            Update_Search_History();
            SearchFiles();
            blockSearchHistoryChange = false;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (FilesListBox.SelectedItems.Count > 0)
            {
                foreach (var item in FilesListBox.SelectedItems)
                {
                    string cmd = "C:\\Windows\\explorer.exe";
                    string arg = "G:\\Software\\" + FilesListBox.SelectedItem.ToString();
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
            if(TextBox1.Text == "")
            {
                MessageBox.Show("Please enter a valid job number into the search bar");
                return;
            }
            try
            {
                string jobNumber = "*" + TextBox1.Text + "*.afm";
                string folder = "G:\\Software\\Modification_docs";
                string[] files = Directory.GetFiles(@folder, jobNumber);
                foreach (string file in files)
                {
                    Console.WriteLine(file);
                    Process.Start("C:\\Program Files\\Acro Software\\FormMax Filler\\AcroFill.exe", file);
                }
            }
            catch
            {
                MessageBox.Show("File could not be found");
            }
        }

        private void Emulink_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("Y:\\Emulink.exe");
        }

        private void Mp2link_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("Y:\\MP2Link.exe");
        }

        private void SearchFiles()
        {
            Make_Controls_Invisible();

            FilesListBox.Items.Clear();

            if(TextBox1.Text == "")
            {
                MessageBox.Show("Please enter a job number into the search bar");
                return;
            }

            searchProgress = 0;
            if(CustomFoldersCheckBox.IsChecked == true)
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
                new System.IO.StreamWriter(@"K:\\Jake Ball\\test.txt", true))
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

            if(FilesListBox.Items.Count < 1)
            {
                JobInfo.Visibility = Visibility.Visible;
                JobInfo.Text = "No preview available for this job";
            }
        }

        private void SearchLocation(string[] locations, string subfolder)
        {
            string fileExtension = "";
            if (FileExtension.SelectedIndex == 0)
            {
                fileExtension = "*.asm";
            }
            else if (FileExtension.SelectedIndex == 1)
            {
                fileExtension = "*.ol*";
            }
            else
            {
                fileExtension = "*";
            }

            foreach (string location in locations)
            {
                try
                {
                    string jobNumber = "*" + TextBox1.Text + fileExtension;
                    string folder = "G:\\Software\\" + subfolder + "\\" + location;
                    string[] files = Directory.GetFiles(@folder, jobNumber,SearchOption.AllDirectories);
                    foreach (string file in files)
                    {
                        int locationIndex = 12;
                        string jobFile = file.Substring(locationIndex, file.Length - locationIndex);
                        if (permission < 2)
                        {
                            FilesListBox.Items.Add(jobFile);
                        }
                        else
                        {
                            if(Generate_JobInfo(jobFile))
                            {
                                FilesListBox.Items.Add(jobFile);
                            }
                        }
                    }
                }
                catch
                {

                }
                searchProgress++;
                SearchProgress.Dispatcher.Invoke(() => SearchProgress.Value = searchProgress, DispatcherPriority.Background);
            }
        }

        private void ShowPrints_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string topFolder = TextBox1.Text.Substring(0, TextBox1.Text.Length - 3) + "000";
                string jobFolder = TextBox1.Text;
                string path = "\\\\ranusnmcvpfs01\\Jobfiles\\" + topFolder + "\\" + jobFolder;
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

        private void MP2COC_JobInfo(string file)
        {
            Content content = new mods.Content(file);

            DateTime lastModified = System.IO.File.GetLastWriteTime("G:\\Software\\" + file);
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
            if(versionTop[0] == '0' && versionTop.Length > 1)
            {
                versionTop = versionTop.Substring(1, 1);
            }
            if(versionBot[0] == '0' && versionBot.Length > 1 && versionBot[1] != ' ')
            {
                versionBot = versionBot.Substring(1, 1);
            }

            //Job Info
            JobInfo.Text = "";
            JobInfo.Text += file + "\n";
            JobInfo.Text += "Last Modified: " + lastModified.ToString("MM/dd/yy HH:mm:ss") + "\n\n";
            JobInfo.Text += jobName + "\n";
            JobInfo.Text += "Version: " + versionTop + "." + versionMid + "." + versionBot + "\n\n";
            JobInfo.Text += "Top Floor: " + topFloorDecimal + "\n";
            JobInfo.Text += "Bottom Floor: " + botFloorDecimal + "\n\n";
            JobInfo.Text += "Independent Rear Doors: " + rearDoor + "\n";
            JobInfo.Text += "Security: " + Security(content) + "\n";
            JobInfo.Text += "False Floors: " + falseFloors + "\n";
            JobInfo.Text += "Nudging: " + nudging + "\n";

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

            //Landings
            Draw_Landing_Preview(content);

            //Inputs and Outputs
            Generate_IO(content);

            //Headers
            Generate_Headers(content);
        }

        private void MP2OGM_JobInfo(string file)
        {
            Content content = new Content(file);
            
            DateTime lastModified = System.IO.File.GetLastWriteTime("G:\\Software\\" + file);
            string jobName = content.Get_String("JBNAME:", 1);
            int iox = General.HexStringToDecimal(content.Get_Nibble("LOBBY:", 6, 0));
            int i4o = General.HexStringToDecimal(content.Get_Nibble("LOBBY:", 6, 1));
            int callbnu = General.HexStringToDecimal(content.Get_Nibble("LOBBY:", 7, 0));
            string[,] inputs = content.inputs;
            string[,] outputs = content.outputs;

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

            Draw_Group_Landing_Preview(content);

            JobInfo.Text = "";
            JobInfo.Text += file + "\n";
            JobInfo.Text += "Last Modified: " + lastModified.ToString("MM/dd/yy HH:mm:ss") + "\n\n";
            JobInfo.Text += jobName + "\n\n";
            JobInfo.Text += "# of Call Boards: " + callbnu + "\n";
            JobInfo.Text += "# of IOX Boards: " + iox + "\n";
            JobInfo.Text += "# of I4O Boards: " + i4o + "\n\n";

            HeaderSP.Children.Clear();
            Generate_IO(content,true);
        }

        private bool Generate_JobInfo(string file)
        {
            if (file.Contains("MP2OGM") || file.Contains("MPOGM") || file.Contains("MPOGD"))
            {
                try
                {
                    MP2OGM_JobInfo(file);
                    return true;
                }
                catch (Exception ex)
                {
                    using (System.IO.StreamWriter writefile =
                    new System.IO.StreamWriter(@"K:\\Jake Ball\\Error_Log.txt", true))
                    {
                        DateTime now = DateTime.Now;
                        writefile.WriteLine("[" + now.ToString() + "] " + Environment.UserName);
                        writefile.WriteLine(file);
                        writefile.WriteLine(ex.ToString() + "\n");
                    }

                    JobInfo.Text = "Job Info could not be generated for this file.";
                    return false;
                }
            }
            else
            {
                try
                {
                    MP2COC_JobInfo(file);
                    return true;
                }
                catch (Exception ex)
                {
                    using (System.IO.StreamWriter writefile =
                    new System.IO.StreamWriter(@"K:\\Jake Ball\\Error_Log.txt", true))
                    {
                        DateTime now = DateTime.Now;
                        writefile.WriteLine("[" + now.ToString() + "] " + Environment.UserName);
                        writefile.WriteLine(file);
                        writefile.WriteLine(ex.ToString() + "\n");
                    }
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

                if(Generate_JobInfo(file))
                {
                    Make_Controls_Visible();
                }
                else
                {
                    Make_Controls_Invisible();
                    JobInfo.Visibility = Visibility.Visible;
                }
            }
        }

        private void OpenSim_Click(object sender, RoutedEventArgs e)
        {
            string message = "";

            foreach (var item in FilesListBox.SelectedItems)
            {
                if (item.ToString().Contains("MP2OGM")|| item.ToString().Contains("MPOGD")||item.ToString().Contains("MPOGM"))
                {
                    message += "Group files not supported\n";
                }
                else
                {
                    Simulator sim = new Simulator(item.ToString());
                    message += "File Created: " + sim.Write_File() + "\n";
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
                if(i == 3)
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

            foreach(string job in Trac_Mod_Jobs)
            {
                SearchHistory.Items.Add(job);
            }
        }

        private void SearchHistory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!blockSearchHistoryChange)
            {
                if(SearchHistory.SelectedValue.ToString().StartsWith("-"))
                {
                    TextBox1.Text = ""; //The user selected a title instead of a job number
                }
                else
                {
                    TextBox1.Text = SearchHistory.SelectedValue.ToString();
                }
            }
        }

        private string Security(Content content)
        {
            string security = "";

            bool BSI = false;
            bool SECRTY = false;
            bool CRTLOK = false;
            bool SECUR = false;
            bool NEWSECRTY = false;

            for (int i = 0; i < 8; i++)
            {
                for (int i2 = 0; i2 < 8; i2++)
                {
                    if (content.inputs[i, i2] == "BSI")
                    {
                        BSI = true;
                    }
                }
            }

            if(content.Get_Bit("LOBBY:",31,0,0) == "YES")
            {
                NEWSECRTY = true;
            }

            if(content.Get_Bit("LOBBY:",31,0,1) == "YES")
            {
                CRTLOK = true;
            }

            if(content.Get_Bit("LOBBY:",31,0,3) == "YES")
            {
                SECRTY = true;
            }

            if(content.Get_Bit("CPVAR",7,1,0) == "YES" )
            {
                SECUR = true;
            }

            if(BSI)
            {
                security += "BSI";
            }

            if(SECRTY && CRTLOK && SECUR)
            {
                if(security != "")
                {
                    security += ", ";
                }

                security += "CRTLOCK";
            }

            if(NEWSECRTY)
            {
                if (security != "")
                {
                    security += ", ";
                }

                security += "ACE";
            }

            if(security == "")
            {
                return "NO";
            }
            else
            {
                security = "YES - " + security;
            }

            return security;
        }

        private void Draw_Landing_Preview(Content content)
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
                    if(falseFloors.Contains(f))
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

            for (int i = 0; i < 8; i++)
            {
                for (int i2 = 0; i2 < 8; i2++)
                {
                    if (content.inputs[i, i2] == "ALT")
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
                }
            }
        }

        private void Draw_Group_Landing_Preview(Content content)
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

            int number_of_cars = content.Get_Group_Num_Of_Cars();

            LandingNormalHeader.Width = 48 + 48 * number_of_cars;
            LandingNormalConfig.Width = 48 + 48 * number_of_cars;

            LandingLevels.Text += "Car\n";
            LandingPIs.Text += "Car\n";

            for(int c = 0; c < number_of_cars; c++)
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
                    if( c < number_of_cars - 1)
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
        }

        private void Generate_Headers(Content content)
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
                List<string> inelig = content.INELIG_Inputs(file);
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
                if(numOfCalls < 16)
                {
                    for(int x = 16 - numOfCalls; x > 0; x --)
                    {
                        calls.Add("N/C");
                    }
                }

                StackPanel sp = new StackPanel { Orientation = Orientation.Vertical, Name = ("Column" + column), Margin=new Thickness(10,15,10,0) };
                for (int x = 15; x >= 0; x--)
                {
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
                            Margin = new Thickness(0,-2,0,0)
                        });
                    numOfCalls--;
                }
                column++;

                HeaderSP.Children.Add(sp);
            } while (numOfCalls > 0);
        }

        private void Generate_IO(Content content, bool group = false)
        {
            IOInfoSP.Children.Clear();

            string[,] inputs = content.inputs;
            string[,] outputs = content.outputs;

            Label inputLabel = new Label
            {
                Content = "Spare Inputs",
            };

            IOInfoSP.Children.Add(inputLabel);

            for(int row = 0; row < 8; row++)
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

                for (int column = 0; column < 8; column ++)
                {
                    string ioText = inputs[row, column];

                    if(ioText == null)
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
                    else if(column < 7) // dont want to add hyphen for last column
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

                foreach(var child in rowSP.Children)
                {
                    if(child.GetType() == typeof(TextBox))
                    {
                        TextBox tb = child as TextBox;

                        if(tb.Text != "XXXX" && tb.Tag.ToString() == "IO")
                        {
                            rowIsEmpty = false;
                        }
                    }
                }

                if(!rowIsEmpty)
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
                    string ioText = outputs[row, 7 - column];

                    if (ioText == null)
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
                    List<string> ncinputs = content.NC_Inputs(content.file);
                    List<string> ncoutputs = content.NC_Outputs(content.file);

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

            }
            else
            {
                i4o = General.HexStringToDecimal(content.Get_Nibble("LOBBY:", 40, 1));
                iox = General.HexStringToDecimal(content.Get_Nibble("LOBBY:", 40, 0));
                aiox = General.HexStringToDecimal(content.Get_Nibble("LOBBY:", 52, 0));
            }

            string[,] inputs = content.inputs;
            string[,] outputs = content.outputs;

            int inputRow = 0;
            int outputRow = 0;
            int inputCol = 0;
            int outputCol = 0;

            //IOX
            for(int b = 0; b < iox; b++)
            {

                Border border = new Border {
                    BorderBrush = System.Windows.Media.Brushes.Black,
                    BorderThickness = new Thickness(2),
                    Background = System.Windows.Media.Brushes.Transparent,
                    Margin = new Thickness(0, 0, 0, 10),
                    Width = bWidth,
                    HorizontalAlignment = HorizontalAlignment.Left
                };

                StackPanel ioxsp = new StackPanel {
                    Name = "ioxsp" + b,
                    Orientation = Orientation.Vertical,
                    Width = spWidth,
                    Margin = new Thickness(5,0,0,0)
                };

                Label boardLabel = new Label { Content = "IOX Board # " + (b + 1) };
                Label inputLabel = new Label { Content = "Inputs:" };
                Label outputLabel = new Label { Content = "Outputs:", Margin = new Thickness(0,23,0,0) };

                StackPanel inputsp1 = new StackPanel { Orientation = Orientation.Horizontal };
                StackPanel outputsp1 = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0,0,0,10)};

                for(int i = 0; i < 8; i++)
                {
                    string input = inputs[inputRow, inputCol];
                    inputsp1.Children.Add(
                        new TextBox
                        {
                            Text = input,
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

                for(int o = 0; o < 8; o ++)
                {
                    string output = outputs[outputRow, 7 - outputCol];
                    outputsp1.Children.Add(
                        new TextBox
                        {
                            Text = output,
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
                StackPanel inputsp2 = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0,-2,0,0) };
                StackPanel outputsp1 = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 10) };

                for (int i = 0; i < 8; i++)
                {
                    string input = inputs[inputRow, inputCol];
                    inputsp1.Children.Add(
                        new TextBox
                        {
                            Text = input,
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

                    if(inputCol == 8)
                    {
                        inputCol = 0;
                        inputRow++;
                    }
                }

                for (int i = 0; i < 8; i++)
                {
                    string input = inputs[inputRow, inputCol];
                    inputsp2.Children.Add(
                        new TextBox
                        {
                            Text = input,
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
                    string output = outputs[outputRow, 7 - outputCol];
                    outputsp1.Children.Add(
                        new TextBox
                        {
                            Text = output,
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

                    if(outputCol == 8)
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
                    string input = inputs[inputRow, inputCol];
                    inputsp1.Children.Add(
                        new TextBox
                        {
                            Text = input,
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
                    string output = outputs[outputRow, 7 - outputCol];
                    outputsp1.Children.Add(
                        new TextBox
                        {
                            Text = output,
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
            Excel.Workbook xlWorkbook = xlApp.Workbooks.Open("F:\\Software\\Product\\Trac_Mod.xlsm",0,true);
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
                string dateReceived         =  "";
                string shipDate             =  "";
                string notificationNumber   =  "";
                string jobNumber            =  "";
                string type                 =  "";
                string custom               =  "";
                string engineer             =  "";
                string notes                =  "";

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

            //close and release
            xlWorkbook.Close(false);
            Marshal.ReleaseComObject(xlWorkbook);

            //quit and release
            xlApp.Quit();
            Marshal.ReleaseComObject(xlApp);
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
                string arg = "G:\\Software\\" + path;
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
            Properties.Settings.Default.Username = Username.Text;
            Update_Search_History();
        }

        private void ToggleIOView_Click(object sender, RoutedEventArgs e)
        {
            if(IOInfoSP.Visibility == Visibility.Visible)
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
        }

        private bool Version_Check()
        {
            string versionPath = "";
            string newVersion = "";

            List<string> versions = new List<string>();
            versions = System.IO.File.ReadAllLines(@"K:\\Jake Ball\\Versions.txt").ToList();

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
            if(Version_Check())
            {
                if(System.Windows.Forms.MessageBox.Show("There is a new version available, do you want to update?", "Update ModHub?", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
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
            users = System.IO.File.ReadAllLines(@"K:\\Jake Ball\\Permissions.txt").ToList();
            string environmentName = Environment.UserName;

            foreach (string user in users)
            {
                int equalIndex = user.IndexOf("=");
                string userName = user.Substring(0, equalIndex);
                if(userName == environmentName)
                {
                    this.permission = Int32.Parse(user.Substring(equalIndex + 1, user.Length - equalIndex - 1));
                }
            }

            if(permission > 1)
            {
                OpenFile.Visibility = Visibility.Hidden;
                OpenFolder.Visibility = Visibility.Hidden;
                ModDocs.Visibility = Visibility.Hidden;
                OpenSim.Visibility = Visibility.Hidden;
                Mp2link.Visibility = Visibility.Hidden;
                Emulink.Visibility = Visibility.Hidden;
                SettingsTab.Visibility = Visibility.Hidden;
                TracModTab.Visibility = Visibility.Hidden;

                ShowPrints.Margin = new Thickness(ShowPrints.Margin.Left, ShowPrints.Margin.Top - 51, ShowPrints.Margin.Right, ShowPrints.Margin.Bottom);
                PrintPage.Margin = new Thickness(PrintPage.Margin.Left, PrintPage.Margin.Top - 51, PrintPage.Margin.Right, PrintPage.Margin.Bottom);
            }

            if(permission > 0)
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

                foreach(var grandchild in child.Children)
                {
                    if(grandchild.GetType() == typeof(TextBox))
                    {
                        TextBox grandchildTB = grandchild as TextBox;
                        if(grandchildTB.Text.ToLower().Contains(TracModFilter.Text.ToLower()))
                        {
                            foundText = true;
                        }
                    }
                }

                if(foundText)
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
                uw.JobFile.Text =  "G:\\Software\\" + FilesListBox.SelectedItem.ToString();
            }
            catch
            {

            }
            uw.ShowDialog();            
        }

        private void InfoTabControl_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if(InfoTabControl.SelectedIndex == 3)
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
            ArchiveWindow aw = new ArchiveWindow("G:\\Software\\" + FilesListBox.SelectedItem.ToString());
            aw.ShowDialog();
        }

        private void CreatePersonalFile_Click(object sender, RoutedEventArgs e)
        {
            string selectedPath = "G:\\Software\\" + FilesListBox.SelectedItem.ToString();
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

            File.Copy(selectedPath, selectedFolder + newFileName, true);
        }
    }
}
