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

namespace mods
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool blockSearchHistoryChange = false;

        public MainWindow()
        {
            InitializeComponent();
            Username.Text = Properties.Settings.Default.Username;
            ListBox1.SelectionMode = SelectionMode.Extended;
            FileExtension.Items.Add(".asm");
            FileExtension.Items.Add(".old");
            FileExtension.Items.Add("All Files");
            FileExtension.SelectedIndex = 0;
            Update_Search_History();
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
            if (ListBox1.SelectedItems.Count > 0)
            {
                foreach (var item in ListBox1.SelectedItems)
                {
                    string cmd = "C:\\Windows\\explorer.exe";
                    string arg = "G:\\Software\\" + ListBox1.SelectedItem.ToString();
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
            ListBox1.Items.Clear();

            if(TextBox1.Text == "")
            {
                MessageBox.Show("Please enter a job number into the search bar");
                return;
            }

            int progress = 0;

            string[] locations = new string[] { "MP2COC", "MP2OGM", "MPODH", "MPODT", "MPOGD", "MPOGM", "MPOLHD", "MPOLHM", "MPOLOM", "MPOLTD", "MPOLTM" };
            string[] source_locations = new string[] { "MC-MP\\MPODH", "MC-MP\\MPODT", "MC-MP\\MPOGM", "MC-MP\\MPOLHM", "MC-MP\\MPOLOM", "MC-MP\\MPOLTM", "MC-MP2\\MP2COC", "MC-MP2\\MP2OGM" };
            string[] custom_locations = new string[] { "MC-MP\\MPODH\\" + TextBox1.Text, "MC-MP\\MPODT\\" + TextBox1.Text, "MC-MP\\MPOGD\\" + TextBox1.Text, "MC-MP\\MPOGM\\" + TextBox1.Text, "MC-MP\\MPOLHD\\" + TextBox1.Text, "MC-MP\\MPOLHM\\" + TextBox1.Text, "MC-MP\\MPOLOM\\" + TextBox1.Text, "MC-MP\\MPOLTD\\" + TextBox1.Text, "MC-MP\\MPOLTM\\" + TextBox1.Text };
            string[] custom2_locations = new string[] { TextBox1.Text };

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
                    string folder = "G:\\Software\\Product\\" + location;
                    string[] files = Directory.GetFiles(@folder, jobNumber);
                    foreach (string file in files)
                    {
                        //int locationIndex = file.IndexOf(location);
                        int locationIndex = 12;
                        string jobFile = file.Substring(locationIndex, file.Length - locationIndex);
                        ListBox1.Items.Add(jobFile);
                    }
                }
                catch
                {

                }
                progress++;
                SearchProgress.Dispatcher.Invoke(() => SearchProgress.Value = progress, DispatcherPriority.Background);
            }

            foreach (string location in source_locations)
            {
                try
                {
                    string jobNumber = "*" + TextBox1.Text + fileExtension;
                    string folder = "G:\\Software\\Source\\" + location;
                    string[] files = Directory.GetFiles(@folder, jobNumber, SearchOption.AllDirectories);
                    foreach (string file in files)
                    {
                        //int locationIndex = file.IndexOf(location);
                        int locationIndex = 12; //index after G:\Software
                        string jobFile = file.Substring(locationIndex, file.Length - locationIndex);
                        ListBox1.Items.Add(jobFile);
                    }
                    
                }
                catch
                {

                }
                progress++;
                SearchProgress.Dispatcher.Invoke(() => SearchProgress.Value = progress, DispatcherPriority.Background);
            }

            foreach (string location in custom_locations)
            {
                try
                {
                    string jobNumber = "*" + TextBox1.Text + fileExtension;
                    string folder = "G:\\Software\\Custom\\" + location;
                    string[] files = Directory.GetFiles(@folder, jobNumber, SearchOption.AllDirectories);
                    foreach (string file in files)
                    {
                        //int locationIndex = file.IndexOf(location);
                        int locationIndex = 12;
                        string jobFile = file.Substring(locationIndex, file.Length - locationIndex);
                        ListBox1.Items.Add(jobFile);
                    }
                }
                catch
                {

                }
                progress++;
                SearchProgress.Dispatcher.Invoke(() => SearchProgress.Value = progress, DispatcherPriority.Background);
            }

            foreach (string location in custom2_locations)
            {
                try
                {
                    string jobNumber = "*" + TextBox1.Text + fileExtension;
                    string folder = "G:\\Software\\Custom2\\" + location;
                    string[] files = Directory.GetFiles(@folder, jobNumber, SearchOption.AllDirectories);
                    foreach (string file in files)
                    {
                        //int locationIndex = file.IndexOf(location);
                        int locationIndex = 12;
                        string jobFile = file.Substring(locationIndex, file.Length - locationIndex);
                        ListBox1.Items.Add(jobFile);
                    }
                    
                }
                catch
                {

                }
                progress++;
                SearchProgress.Dispatcher.Invoke(() => SearchProgress.Value = progress, DispatcherPriority.Background);
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
            string topFloorDecimal = (content.HexStringToDecimal(topFloor) + 1).ToString();
            string botFloor = content.Get_Byte("BOTTOM:", 1) + 'H';
            string botFloorDecimal = (content.HexStringToDecimal(botFloor) + 1).ToString();
            string falseFloors = content.Get_Bit("CPVAR", 3, 0, 3);
            string nudging = content.Get_Bit("CPVAR", 7, 0, 3);
            string i4o = content.Get_Nibble("LOBBY:", 40, 1);
            string iox = content.Get_Nibble("LOBBY:", 40, 0);
            string aiox = content.Get_Nibble("LOBBY:", 52, 0);
            string callbnu = content.Get_Nibble("LOBBY:", 41, 1);
            string rearDoor = content.Get_Bit("LOBBY:", 12, 0, 3);
            string ceBoard = content.Get_Bit("BOTTOM:", 6, 1, 1);
            string ncBoard = content.Get_Bit("LOBBY:", 38, 1, 2);
            string ftBoard = content.Get_Bit("BOTTOM:", 6, 1, 3);

            //Job Info
            JobInfo.Text = "";
            JobInfo.Text += file + "\n";
            JobInfo.Text += "Last Modified: " + lastModified.ToString("MM/dd/yy HH:mm:ss") + "\n\n";
            JobInfo.Text += jobName + "\n\n";
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
            JobInfo.Text += "FT Board: " + ftBoard + "\n\n";

            //Landings
            Draw_Landing_Preview(content);

            //Inputs and Outputs
            Generate_IO(content,file);

            //Headers
            Generate_Headers(content);
        }

        private void MP2OGM_JobInfo(string file)
        {
            Content content = new Content(file);

            string jobName = content.Get_String("JBNAME:", 1);
            string iox = content.Get_Nibble("LOBBY:", 6, 0);
            string i4o = content.Get_Nibble("LOBBY:", 6, 1);
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

            //Draw_Group_Landing_Preview(content);

            JobInfo.Text = "";
            JobInfo.Text += file + "\n\n";
            JobInfo.Text += jobName + "\n\n";
            JobInfo.Text += "# of IOX Boards: " + iox + "\n";
            JobInfo.Text += "# of I4O Boards: " + i4o + "\n\n";

            JobInfo.Text += "Spare Inputs:\n";

            for (int x = 0; x < 8; x++)
            {
                string inputLine = "";

                for (int y = 0; y < 8; y++)
                {
                    string hyphen = "";
                    if (y == 7)
                    {
                        if (x == 3)
                        {
                            hyphen = "\n\n";
                        }
                        else
                        {
                            hyphen = "\n";
                        }
                    }
                    else if (y == 3)
                    {
                        hyphen = "-||-";
                    }
                    else
                    {
                        hyphen = "-";
                    }
                    if (inputs[x, y] == null)
                    {
                        inputLine += "XXXX" + hyphen;
                    }
                    else
                    {
                        inputLine += inputs[x, y] + hyphen;
                    }
                }

                if (inputLine.StartsWith("XXXX-XXXX-XXXX-XXXX-||-XXXX-XXXX-XXXX-XXXX"))
                {
                    if (x == 4)
                    {
                        break;
                    }
                    else
                    {
                        JobInfo.Text += inputLine;
                    }
                }
                else
                {
                    JobInfo.Text += inputLine;
                }
            }

            JobInfo.Text += "Spare Outputs:\n";

            for (int x = 0; x < 4; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    string hyphen = "";
                    if (y == 7)
                    {
                        if (x == 3)
                        {
                            hyphen = "\n\n";
                        }
                        else
                        {
                            hyphen = "\n";
                        }
                    }
                    else if (y == 3)
                    {
                        hyphen = "-||-";
                    }
                    else
                    {
                        hyphen = "-";
                    }
                    if (outputs[x, 7 - y] == null)
                    {
                        JobInfo.Text += "XXXX" + hyphen;
                    }
                    else
                    {
                        JobInfo.Text += outputs[x, 7 - y] + hyphen;
                    }
                }
            }
        }

        private void ListBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var addedItem = e.AddedItems;

            if (addedItem.Count == 1)
            {
                string file = addedItem[0].ToString();

                if (file.Contains("MP2OGM") || file.Contains("MPOGM") || file.Contains("MPOGD"))
                {
                    try
                    {
                        MP2OGM_JobInfo(file);
                    }
                    catch
                    {
                        JobInfo.Text = "Job Info could not be generated for this file.";
                    }
                }
                else
                {
                    try
                    {
                        MP2COC_JobInfo(file);
                    }
                    catch
                    {
                        JobInfo.Text = "Job Info could not be generated for this file.";
                    }
                }
            }
        }

        private void OpenSim_Click(object sender, RoutedEventArgs e)
        {
            string message = "";

            foreach (var item in ListBox1.SelectedItems)
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
            foreach (string search in Properties.Settings.Default.SearchHistory)
            {
                SearchHistory.Items.Add(search);
            }

            List<string> tempSearchHistory = new List<string>();

            foreach (string search in Properties.Settings.Default.SearchHistory)
            {
                tempSearchHistory.Add(search);
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
                    if (tempSearchHistory.Count >= 3)
                    {
                        tempSearchHistory.Remove(tempSearchHistory[1]);
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

            Track_Mod();
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

            return security;
        }

        private void Draw_Landing_Preview(Content content)
        {
            LandingNormalConfig.Text = "";
            LandingNormalConfig.Height = 0;
            LandingNormalConfig.BorderThickness = new System.Windows.Thickness(0);
            LandingAltConfig.Text = "";
            LandingAltConfig.Height = 0;
            LandingAltConfig.BorderThickness = new System.Windows.Thickness(0);

            LandingNormalHeader.Visibility = Visibility.Hidden;
            LandingAltHeader.Visibility = Visibility.Hidden;

            int top_landing = content.HexStringToDecimal(content.Get_Byte("BOTTOM:", 2)) + 1;
            string front = "False";
            string rear = "False";

            LandingLevels.Text = "";
            LandingLevels.Height = 16 * top_landing + 10;
            LandingLevels.BorderThickness = new System.Windows.Thickness(2);

            LandingNormalConfig.Text = "";
            LandingNormalConfig.Height = 16 * top_landing + 10;
            LandingNormalConfig.BorderThickness = new System.Windows.Thickness(2);

            LandingNormalHeader.Visibility = Visibility.Visible;

            for (int x = top_landing; x >= 1; x--)
            {
                if (content.Get_Bit("ELIGIV:", x, 0, 3) == "YES")
                {
                    front = "F";
                }
                else
                {
                    front = ".";
                }

                if (content.Get_Bit("ELIGIV:", x, 0, 2) == "YES")
                {
                    rear = "R";
                }
                else
                {
                    rear = ".";
                }

                LandingLevels.Text += x + "\n";
                LandingNormalConfig.Text += front + " " + rear + "\n";
            }

            for(int i = 0; i < 8; i++)
            {
                for (int i2 = 0; i2 < 8; i2++)
                {
                    if (content.inputs[i,i2] == "ALT")
                    {
                        LandingAltHeader.Visibility = Visibility.Visible;

                        LandingAltConfig.Text = "";
                        LandingAltConfig.Height = 16 * top_landing + 10;
                        LandingAltConfig.BorderThickness = new System.Windows.Thickness(2);

                        for (int x = top_landing; x >= 1; x--)
                        {

                            if (content.Get_Bit("ALTMP:", x, 0, 3) == "YES")
                            {
                                front = "F";
                            }
                            else
                            {
                                front = ".";
                            }

                            if (content.Get_Bit("ALTMP:", x, 0, 2) == "YES")
                            {
                                rear = "R";
                            }
                            else
                            {
                                rear = ".";
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
            string front = "False";
            string rear = "False";

            LandingLevels.Text = "";
            LandingLevels.Height = 16 * group_top_landing + 26;
            LandingLevels.BorderThickness = new System.Windows.Thickness(2);

            LandingNormalConfig.Text = "";
            LandingNormalConfig.Height = 16 * group_top_landing + 26;
            LandingNormalConfig.BorderThickness = new System.Windows.Thickness(2);

            LandingNormalHeader.Visibility = Visibility.Visible;

            string[] cars = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L" };

            int number_of_cars = content.Get_Group_Num_Of_Cars();

            for (int x = group_top_landing; x >= 1; x--)
            {
                for (int c = 0; c < number_of_cars; c++)
                {
                    if (content.Get_Bit("ELIGIV" + cars[c], x, 1, 1) == "YES" || content.Get_Bit("ELIGIV" + cars[c], x, 1, 3) == "YES")
                    {
                        front = "F";
                    }
                    else
                    {
                        front = ".";
                    }

                    if (content.Get_Bit("ELIGIV" + cars[c], x, 1, 0) == "YES" || content.Get_Bit("ELIGIV" + cars[c], x, 1, 2) == "YES")
                    {
                        rear = "R\t";
                    }
                    else
                    {
                        rear = ".\t";
                    }
                    LandingNormalConfig.Text += front + " " + rear;
                }
                LandingNormalConfig.Text += "\n";
                LandingLevels.Text += x + "\n";
            }
        }

        private void Generate_Headers(Content content)
        {
            HeaderSP.Children.Clear();

            List<string> calls = new List<string>();

            //ELIGI: Front Car Calls
            for(int x = 0; x < 8; x++)
            {
                for(int b = 3; b >= 0; b--)
                {
                    if (content.Get_Bit("ELIGI:", x + 1, 0, b) == "YES")
                    {
                        int callNum = 100 + x * 8 + (3 - b) + 1;
                        calls.Add(callNum.ToString());
                    }
                }
                for(int b = 3; b >= 0; b--)
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

                StackPanel sp = new StackPanel { Orientation = Orientation.Vertical, Name = ("Column" + column), Margin=new Thickness(30,50,0,0) };
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

        private void Generate_IO(Content content, string file)
        {
            IOInfo.Text = "";

            string[,] inputs = content.inputs;
            string[,] outputs = content.outputs;
            string ncBoard = content.Get_Bit("LOBBY:", 38, 1, 2);

            IOInfo.Text += "Spare Inputs:\n";

            for (int x = 0; x < 8; x++)
            {
                string inputLine = "";

                for (int y = 0; y < 8; y++)
                {
                    string hyphen = "";
                    if (y == 7)
                    {
                        if (x == 3)
                        {
                            hyphen = "\n\n";
                        }
                        else
                        {
                            hyphen = "\n";
                        }
                    }
                    else if (y == 3)
                    {
                        hyphen = "-||-";
                    }
                    else
                    {
                        hyphen = "-";
                    }
                    if (inputs[x, y] == null)
                    {
                        inputLine += "XXXX" + hyphen;
                    }
                    else
                    {
                        inputLine += inputs[x, y] + hyphen;
                    }
                }

                if (inputLine.StartsWith("XXXX-XXXX-XXXX-XXXX-||-XXXX-XXXX-XXXX-XXXX"))
                {
                    break;
                }
                else
                {
                    IOInfo.Text += inputLine;
                }
            }

            IOInfo.Text += "\nSpare Outputs:\n";

            for (int x = 0; x < 4; x++)
            {
                string outputLine = "";

                for (int y = 0; y < 8; y++)
                {
                    string hyphen = "";
                    if (y == 7)
                    {
                        if (x == 3)
                        {
                            hyphen = "\n\n";
                        }
                        else
                        {
                            hyphen = "\n";
                        }
                    }
                    else if (y == 3)
                    {
                        hyphen = "-||-";
                    }
                    else
                    {
                        hyphen = "-";
                    }
                    if (outputs[x, 7 - y] == null)
                    {
                        outputLine += "XXXX" + hyphen;
                    }
                    else
                    {
                        outputLine += outputs[x, 7 - y] + hyphen;
                    }
                }

                if (outputLine.StartsWith("XXXX-XXXX-XXXX-XXXX-||-XXXX-XXXX-XXXX-XXXX"))
                {
                    break;
                }
                else
                {
                    IOInfo.Text += outputLine;
                }
            }

            if(ncBoard == "YES")
            {
                List<string> ncinputs = content.NC_Inputs(file);
                List<string> ncoutputs = content.NC_Outputs(file);

                IOInfo.Text += "\nNC Board Inputs:\n";

                for (int x = 0; x < 8; x++)
                {
                    string inputLine = "";

                    for (int y = 0; y < 8; y++)
                    {
                        string hyphen = "";
                        if (y == 0)
                        {
                            if (x == 3)
                            {
                                hyphen = "\n\n";
                            }
                            else
                            {
                                hyphen = "\n";
                            }
                        }
                        else if (y == 4)
                        {
                            hyphen = "-||-";
                        }
                        else
                        {
                            hyphen = "-";
                        }
                        if (x*8 + y >= ncinputs.Count || ncinputs[x*8 + y] == null)
                        {
                            inputLine = "XXXX" + hyphen + inputLine;
                        }
                        else
                        {
                            inputLine = ncinputs[x*8 + y] + hyphen + inputLine;
                        }
                    }

                    if (inputLine.StartsWith("XXXX-XXXX-XXXX-XXXX-||-XXXX-XXXX-XXXX-XXXX"))
                    {
                        break;
                    }
                    else
                    {
                        IOInfo.Text += inputLine;
                    }
                }

                IOInfo.Text += "\nNC Board Outputs:\n";

                for (int x = 0; x < 8; x++)
                {
                    string outputLine = "";

                    for (int y = 0; y < 8; y++)
                    {
                        string hyphen = "";
                        if (y == 0)
                        {
                            if (x == 3)
                            {
                                hyphen = "\n\n";
                            }
                            else
                            {
                                hyphen = "\n";
                            }
                        }
                        else if (y == 4)
                        {
                            hyphen = "-||-";
                        }
                        else
                        {
                            hyphen = "-";
                        }
                        if (x * 8 + y >= ncoutputs.Count || ncoutputs[x * 8 + y] == null)
                        {
                            outputLine = "XXXX" + hyphen + outputLine;
                        }
                        else
                        {
                            outputLine = ncoutputs[x * 8 + y] + hyphen + outputLine;
                        }
                    }

                    if (outputLine.StartsWith("XXXX-XXXX-XXXX-XXXX-||-XXXX-XXXX-XXXX-XXXX"))
                    {
                        break;
                    }
                    else
                    {
                        IOInfo.Text += outputLine;
                    }
                }
            }
        }

        private void Track_Mod()
        {
            Excel.Application xlApp = new Excel.Application();
            Excel.Workbook xlWorkbook = xlApp.Workbooks.Open("F:\\Software\\Product\\Trac_Mod.xlsm",0,true);
            Excel._Worksheet xlWorksheet = xlWorkbook.Sheets[1];
            Excel.Range xlRange = xlWorksheet.UsedRange;

            SearchHistory.Items.Add("---Active Mods---");

            List<string> Usernames = new List<string>();

            string userName = Properties.Settings.Default.Username;

            while(userName.Contains(";"))
            {
                int colonIndex = userName.IndexOf(';');
                Usernames.Add(userName.Substring(0, colonIndex));
                userName = userName.Substring(colonIndex + 1, userName.Length - colonIndex - 1);
            }

            Usernames.Add(userName);

            for (int row = 4; row<100;row++)
            {
                if (xlRange.Cells[row, 8].Value2 != null && xlRange.Cells[row, 5].Value2 != null)
                {
                    string engineer = xlRange.Cells[row, 8].Value2.ToString();
                    string jobNumber = xlRange.Cells[row, 5].Value2.ToString();
                    foreach (string username in Usernames)
                    {
                        if (engineer.Contains(username))
                        {
                            if (jobNumber.Contains("-"))
                            {
                                int dashIndex = jobNumber.IndexOf("-");
                                jobNumber = jobNumber.Substring(dashIndex + 1, jobNumber.Length - dashIndex - 1);
                            }
                            SearchHistory.Items.Add(jobNumber);
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
        
        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string path = "";
                string deconstructedPath = ListBox1.SelectedItem.ToString();
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
    }
}
