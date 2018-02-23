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
using System.Diagnostics;
using System.IO;

namespace mods
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ListBox1.SelectionMode = SelectionMode.Extended;
            FileExtension.Items.Add(".asm");
            FileExtension.Items.Add(".old");
            FileExtension.Items.Add("All Files");
            FileExtension.SelectedIndex = 0;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SearchFiles();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            foreach(var item in ListBox1.SelectedItems)
            {
                Console.WriteLine(item.ToString());
                Process.Start("C:\\Program Files\\Notepad++\\notepad++.exe", "G:\\Software\\Product\\" + item.ToString());
                System.Threading.Thread.Sleep(1000);
                Process.Start("C:\\CW32\\cw32.exe", "G:\\Software\\Product\\" + item.ToString());
            }
        }

        private void Archive_Click(object sender, RoutedEventArgs e)
        {
            string createdFileString = "";
            foreach (var item in ListBox1.SelectedItems)
            {
                string itemString = item.ToString();
                int extensionIndex = itemString.IndexOf(".");
                bool oldCheck = false;
                int oldNum = 0;
                string newFileName = "";
                while (!oldCheck)
                {
                    try
                    {
                        if (oldNum == 0)
                        {
                            newFileName = itemString.Substring(0, extensionIndex) + ".OLD";
                        }
                        else
                        {
                            newFileName = itemString.Substring(0, extensionIndex) + ".OLD" + oldNum;
                        }
                        File.Copy("G:\\Software\\Product\\" + itemString, "G:\\Software\\Product\\" + newFileName);
                        oldCheck = true;
                    }
                    catch
                    {
                        oldNum++;
                    }
                }
                createdFileString += newFileName + " was created.\n";
            }
            MessageBox.Show(createdFileString);
            SearchFiles();
        }

        private void ModDocs_Click(object sender, RoutedEventArgs e)
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

            string[] locations = new string[] { "MP2COC", "MP2OGM", "MPODH", "MPODT", "MPOGD", "MPOGM", "MPOLHD", "MPOLHM", "MPOLOM", "MPOLTD", "MPOLTM" };
            string[] custom_locations = new string[] { "MC-MP", "MC-MP2" };

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
                string jobNumber = "*" + TextBox1.Text + fileExtension;
                string folder = "G:\\Software\\Product\\" + location;
                string[] files = Directory.GetFiles(@folder, jobNumber);
                foreach (string file in files)
                {
                    int locationIndex = file.IndexOf(location);
                    string jobFile = file.Substring(locationIndex, file.Length - locationIndex);
                    ListBox1.Items.Add(jobFile);
                }
            }

            foreach (string location in custom_locations)
            {
                string jobNumber = "*" + TextBox1.Text + fileExtension;
                string folder = "G:\\Software\\Source\\" + location;
                string[] files = Directory.GetFiles(@folder, jobNumber, SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    int locationIndex = file.IndexOf(location);
                    string jobFile = file.Substring(locationIndex, file.Length - locationIndex);
                    ListBox1.Items.Add(jobFile);
                }
            }
        }

        private void ShowPrints_Click(object sender, RoutedEventArgs e)
        {
            string topFolder = TextBox1.Text.Substring(0, TextBox1.Text.Length - 3) + "000";
            string jobFolder = TextBox1.Text;
            string path = "\\\\ranusnmcvpfs01\\Jobfiles\\" + topFolder + "\\" + jobFolder;
            string cmd = "C:\\Windows\\explorer.exe";
            string arg = path;
            Process.Start(cmd, arg);
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

            string jobName = content.Get_String("JBNAME:", 1);
            string topFloor = content.Get_Byte("BOTTOM:", 2) + 'H';
            string topFloorDecimal = (content.HexStringToDecimal(topFloor)+1).ToString();
            string botFloor = content.Get_Byte("BOTTOM:", 1) + 'H';
            string botFloorDecimal = (content.HexStringToDecimal(botFloor)+1).ToString();
            string i4o = content.Get_Nibble("LOBBY:", 40, 1);
            string iox = content.Get_Nibble("LOBBY:", 40, 0);
            string aiox = content.Get_Nibble("LOBBY:", 52, 0);
            string rearDoor = content.Get_Bit("LOBBY:", 12, 0, 3);
            string ceBoard = content.Get_Bit("BOTTOM:", 6, 1, 1);
            string ncBoard = content.Get_Bit("LOBBY:", 33, 1, 2);
            string ftBoard = content.Get_Bit("BOTTOM:", 6, 1, 3);
            string[,] inputs = content.inputs;
            string[,] outputs = content.outputs;

            JobInfo.Text = "";
            JobInfo.Text += file + "\n\n";
            JobInfo.Text += jobName + "\n\n";
            JobInfo.Text += "Top Floor: " + topFloor + "\t\t(" + topFloorDecimal + ")" + "\n";
            JobInfo.Text += "Bottom Floor: " + botFloor + "\t(" + botFloorDecimal + ")" + "\n\n";
            JobInfo.Text += "Independed Rear Doors: " + rearDoor + "\n\n";
            JobInfo.Text += "# of IOX Boards: " + iox + "\n";
            JobInfo.Text += "# of I4O Boards: " + i4o + "\n";
            JobInfo.Text += "# of AIOX Boards: " + aiox + "\n\n";
            JobInfo.Text += "CE Board: " + ceBoard + "\n";
            JobInfo.Text += "NC Board: " + ncBoard + "\n";
            JobInfo.Text += "FT Board: " + ftBoard + "\n\n";

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
                    if (outputs[x, 7-y] == null)
                    {
                        JobInfo.Text += "XXXX" + hyphen;
                    }
                    else
                    {
                        JobInfo.Text += outputs[x, 7-y] + hyphen;
                    }
                }
            }
        }

        private void MP2OGM_JobInfo(string file)
        {
            Content content = new Content(file);

            string jobName = content.Get_String("JBNAME:", 1);
            string iox = content.Get_Nibble("LOBBY:", 6, 0);
            string i4o = content.Get_Nibble("LOBBY:", 6, 1);
            string[,] inputs = content.inputs;
            string[,] outputs = content.outputs;

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
                if (item.ToString().StartsWith("MP2OGM"))
                {
                    message += "A simulator file could not be made for the group\n";
                }
                else
                {
                    Simulator sim = new Simulator(item.ToString());
                    message += "File Created: " + sim.Write_File() + "\n";
                }
                
            }

            MessageBox.Show(message);
            
        }
    }
}
