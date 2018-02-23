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

        private string Remove_Prefix(string text, string prefix)
        {
            if(text.StartsWith(prefix))
            {
                return text.Substring(prefix.Length, text.Length - prefix.Length);
            }
            else
            {
                return text;
            }
        }

        private string Remove_Suffix(string text, string suffix)
        {
            if (text.EndsWith(suffix))
            {
                return text.Substring(0, text.Length - suffix.Length);
            }
            else
            {
                return text;
            }
        }

        private bool Is_Hex(string text)
        {
            if(text.IndexOf("H") == -1)
            {
                return false;
            }
            return true;
        }

        private string[,] Build_Imap(List<string> lines)
        {
            string[,] imap = new string[32,8];
            
            int input_x = 0;
            int input_y = 0;

            if (lines.FindIndex(x => x.StartsWith("IOINPE:")) != -1)
            {
                int input_index = lines.FindIndex(x => x.StartsWith("IOINPE:"));
                for (int x = 0; x < 8; x++)
                {
                    int imap_index = input_index + x + 1;

                    int comment_index = lines[imap_index].IndexOf(';');
                    string comment_string = lines[imap_index].Substring(comment_index, lines[imap_index].Length - comment_index);
                    bool building = false;
                    StringBuilder inCode = new StringBuilder();

                    for (int c = 0; c < comment_string.Length; c++)
                    {
                        if (building)
                        {
                            if (Char.IsLetterOrDigit(comment_string[c]) || comment_string[c] == '/')
                            {
                                inCode.Append(comment_string[c]);
                            }
                            else
                            {
                                imap[input_x, input_y] = inCode.ToString();
                                inCode.Clear();
                                input_y++;
                                if (input_y == 8)
                                {
                                    input_y = 0;
                                    input_x++;
                                }
                                building = false;
                            }
                        }
                        else
                        {
                            if (Char.IsLetterOrDigit(comment_string[c]) || comment_string[c] == '/')
                            {
                                inCode.Append(comment_string[c]);
                                building = true;
                            }
                            else
                            {
                                //do nothing
                            }
                        }
                    }
                    //This block of code is to write the last inCode of the line
                    imap[input_x, input_y] = inCode.ToString();  //{
                    inCode.Clear();
                    input_y++;
                    if (input_y == 8)
                    {
                        input_y = 0;
                        input_x++;
                    }
                    building = false;                            //}

                }
            }

            if (lines.FindIndex(x => x.StartsWith("IOINPE2:")) != -1)
            {
                int input2_index = lines.FindIndex(x => x.StartsWith("IOINPE2:"));
                for (int x = 0; x < 8; x++)
                {
                    int imap_index = input2_index + x + 1;

                    int comment_index = lines[imap_index].IndexOf(';');
                    string comment_string = lines[imap_index].Substring(comment_index, lines[imap_index].Length - comment_index);
                    bool building = false;
                    StringBuilder inCode = new StringBuilder();

                    for (int c = 0; c < comment_string.Length; c++)
                    {
                        if (building)
                        {
                            if (Char.IsLetterOrDigit(comment_string[c]) || comment_string[c] == '/')
                            {
                                inCode.Append(comment_string[c]);
                            }
                            else
                            {
                                imap[input_x, input_y] = inCode.ToString();
                                inCode.Clear();
                                input_y++;
                                if (input_y == 8)
                                {
                                    input_y = 0;
                                    input_x++;
                                }
                                building = false;
                            }
                        }
                        else
                        {
                            if (Char.IsLetterOrDigit(comment_string[c]) || comment_string[c] == '/')
                            {
                                inCode.Append(comment_string[c]);
                                building = true;
                            }
                            else
                            {
                                //do nothing
                            }
                        }
                    }
                    //This block of code is to write the last inCode of the line
                    imap[input_x, input_y] = inCode.ToString();  //{
                    inCode.Clear();
                    input_y++;
                    if (input_y == 8)
                    {
                        input_y = 0;
                        input_x++;
                    }
                    building = false;                            //}

                }
            }

            if (lines.FindIndex(x => x.StartsWith("IOINPE3:")) != -1)
            {
                int input3_index = lines.FindIndex(x => x.StartsWith("IOINPE3:"));
                for (int x = 0; x < 8; x++)
                {
                    int imap_index = input3_index + x + 1;

                    int comment_index = lines[imap_index].IndexOf(';');
                    string comment_string = lines[imap_index].Substring(comment_index, lines[imap_index].Length - comment_index);
                    bool building = false;
                    StringBuilder inCode = new StringBuilder();

                    for (int c = 0; c < comment_string.Length; c++)
                    {
                        if (building)
                        {
                            if (Char.IsLetterOrDigit(comment_string[c]) || comment_string[c] == '/')
                            {
                                inCode.Append(comment_string[c]);
                            }
                            else
                            {
                                imap[input_x, input_y] = inCode.ToString();
                                inCode.Clear();
                                input_y++;
                                if (input_y == 8)
                                {
                                    input_y = 0;
                                    input_x++;
                                }
                                building = false;
                            }
                        }
                        else
                        {
                            if (Char.IsLetterOrDigit(comment_string[c]) || comment_string[c] == '/')
                            {
                                inCode.Append(comment_string[c]);
                                building = true;
                            }
                            else
                            {
                                //do nothing
                            }
                        }
                    }
                    //This block of code is to write the last inCode of the line
                    imap[input_x, input_y] = inCode.ToString();  //{
                    inCode.Clear();
                    input_y++;
                    if (input_y == 8)
                    {
                        input_y = 0;
                        input_x++;
                    }
                    building = false;                            //}

                }
            }

            if (lines.FindIndex(x => x.StartsWith("IOINPE4:")) != -1)
            {
                int input4_index = lines.FindIndex(x => x.StartsWith("IOINPE4:"));
                for (int x = 0; x < 8; x++)
                {
                    int imap_index = input4_index + x + 1;

                    int comment_index = lines[imap_index].IndexOf(';');
                    string comment_string = lines[imap_index].Substring(comment_index, lines[imap_index].Length - comment_index);
                    bool building = false;
                    StringBuilder inCode = new StringBuilder();

                    for (int c = 0; c < comment_string.Length; c++)
                    {
                        if (building)
                        {
                            if (Char.IsLetterOrDigit(comment_string[c]) || comment_string[c] == '/')
                            {
                                inCode.Append(comment_string[c]);
                            }
                            else
                            {
                                imap[input_x, input_y] = inCode.ToString();
                                inCode.Clear();
                                input_y++;
                                if (input_y == 8)
                                {
                                    input_y = 0;
                                    input_x++;
                                }
                                building = false;
                            }
                        }
                        else
                        {
                            if (Char.IsLetterOrDigit(comment_string[c]) || comment_string[c] == '/')
                            {
                                inCode.Append(comment_string[c]);
                                building = true;
                            }
                            else
                            {
                                //do nothing
                            }
                        }
                    }
                    //This block of code is to write the last inCode of the line
                    imap[input_x, input_y] = inCode.ToString();  //{
                    inCode.Clear();
                    input_y++;
                    if (input_y == 8)
                    {
                        input_y = 0;
                        input_x++;
                    }
                    building = false;                            //}

                }
            }

            return imap;
        }

        private string[,] Build_Group_Imap(List<string> lines)
        {
            string[,] imap = new string[16, 8];

            int input_x = 0;
            int input_y = 0;

            if (lines.FindIndex(x => x.StartsWith("IOXINE:")) != -1)
            {
                int input_index = lines.FindIndex(x => x.StartsWith("IOXINE:"));
                for (int x = 0; x < 8; x++)
                {
                    int imap_index = input_index + x + 1;

                    int comment_index = lines[imap_index].IndexOf(';');
                    string comment_string = lines[imap_index].Substring(comment_index, lines[imap_index].Length - comment_index);
                    bool building = false;
                    StringBuilder inCode = new StringBuilder();

                    for (int c = 0; c < comment_string.Length; c++)
                    {
                        if (building)
                        {
                            if (Char.IsLetterOrDigit(comment_string[c]) || comment_string[c] == '/')
                            {
                                inCode.Append(comment_string[c]);
                            }
                            else
                            {
                                imap[input_x, input_y] = inCode.ToString();
                                inCode.Clear();
                                input_y++;
                                if (input_y == 8)
                                {
                                    input_y = 0;
                                    input_x++;
                                }
                                building = false;
                            }
                        }
                        else
                        {
                            if (Char.IsLetterOrDigit(comment_string[c]) || comment_string[c] == '/')
                            {
                                inCode.Append(comment_string[c]);
                                building = true;
                            }
                            else
                            {
                                //do nothing
                            }
                        }
                    }
                    //This block of code is to write the last inCode of the line
                    imap[input_x, input_y] = inCode.ToString();  //{
                    inCode.Clear();
                    input_y++;
                    if (input_y == 8)
                    {
                        input_y = 0;
                        input_x++;
                    }
                    building = false;                            //}

                }
            }

            return imap;
        }

        private string Hex_To_Bin(string hex)
        {
            string strippedHex = Remove_Suffix(Remove_Prefix(hex,"DB"),"H").Trim();
            strippedHex = strippedHex.Substring(strippedHex.Length - 2, 2);
            return HexStringToBinary(strippedHex);
        }

        private static readonly Dictionary<char, string> hexCharacterToBinary = new Dictionary<char, string> {
            { '0', "0000" },
            { '1', "0001" },
            { '2', "0010" },
            { '3', "0011" },
            { '4', "0100" },
            { '5', "0101" },
            { '6', "0110" },
            { '7', "0111" },
            { '8', "1000" },
            { '9', "1001" },
            { 'a', "1010" },
            { 'b', "1011" },
            { 'c', "1100" },
            { 'd', "1101" },
            { 'e', "1110" },
            { 'f', "1111" }
        };

        public string HexStringToBinary(string hex)
        {
            StringBuilder result = new StringBuilder();
            foreach (char c in hex)
            {
                // This will crash for non-hex characters. You might want to handle that differently.
                result.Append(hexCharacterToBinary[char.ToLower(c)]);
            }
            return result.ToString();
        }

        private string Get_Bit(List<string> content, string label, int offset, int nibble, int bit)
        {
            int index = content.FindIndex(x => x.StartsWith(label)) + offset;
            string cleanString = Remove_Suffix(Remove_Prefix(content[index], "DB"), "H").Trim();
            
            cleanString = cleanString.Substring(cleanString.Length - nibble - 1, 1); //High nibble = 1; Low nibble = 0

            string binary = HexStringToBinary(cleanString);

            if(binary[bit] == '1')
            {
                return "YES";
            }
            else
            {
                return "NO";
            }
        }

        private string Get_Nibble(List<string> content, string label, int offset, int nibble)
        {
            int index = content.FindIndex(x => x.StartsWith(label)) + offset;
            string cleanString = Remove_Suffix(Remove_Prefix(content[index], "DB"), "H").Trim();

            return cleanString.Substring(cleanString.Length - nibble - 1, 1);
        }

        private string Get_Byte(List<string> content, string label, int offset)
        {
            int index = content.FindIndex(x => x.StartsWith(label)) + offset;
            string cleanString = Remove_Suffix(Remove_Prefix(content[index], "DB"), "H").Trim();

            return cleanString.Substring(cleanString.Length - 2, 2);
        }

        private string[,] Inputs(List<string> content, string[,] imap)
        {
            int input_x = 0;
            int input_y = 7;

            string[,] inputs = new string[8,8];

            int input_index = content.FindIndex(x => x.StartsWith("IOINPE:"));
            int input2_index = content.FindIndex(x => x.StartsWith("IOINPE2:"));

            for(int x = 0;x<8;x++)
            {
                int imap_index = input_index + x + 1;
                string imap_binary = Hex_To_Bin(content[imap_index]);

                for(int y = 0;y<8;y++)
                {
                    if(imap_binary[7-y] == '1')
                    {
                        inputs[input_x, input_y] = imap[x, 7 - y];
                        input_y--;

                        if(input_y == -1)
                        {
                            input_x++;
                            input_y = 7;
                        }
                    }
                }
            }

            for (int x = 0; x < 8; x++)
            {
                int imap_index = input2_index + x + 1;
                string imap_binary = Hex_To_Bin(content[imap_index]);

                for (int y = 0; y < 8; y++)
                {
                    if (imap_binary[7 - y] == '1')
                    {
                        inputs[input_x, input_y] = imap[x+8, 7 - y];
                        input_y--;

                        if (input_y == -1)
                        {
                            input_x++;
                            input_y = 7;
                        }
                    }
                }
            }

            return inputs;
        }

        private string[,] Group_Inputs(List<string> content, string[,] imap)
        {
            int input_x = 0;
            int input_y = 7;

            string[,] inputs = new string[16, 8];

            int input_index = content.FindIndex(x => x.StartsWith("IOXINE:"));

            for (int x = 0; x < 16; x++)
            {
                int imap_index = input_index + x + 1;
                string imap_binary = Hex_To_Bin(content[imap_index]);

                for (int y = 0; y < 8; y++)
                {
                    if (imap_binary[7 - y] == '1')
                    {
                        inputs[input_x, input_y] = imap[x, 7 - y];
                        input_y--;

                        if (input_y == -1)
                        {
                            input_x++;
                            input_y = 7;
                        }
                    }
                }
            }

            return inputs;
        }

        private void MP2COC_JobInfo(string file)
        {
            string path = "G:\\Software\\Product\\" + file;
            string[] lines = System.IO.File.ReadAllLines(@path);
            List<string> linesList = lines.ToList();
            string[,] imap = Build_Imap(linesList);
            List<string> content = new List<string>();
            int x = 0;
            string[] uncommentedLines = new string[lines.Length];
            foreach (string line in lines)
            {
                if (lines[x].IndexOf(";") == -1) //indexOf returns -1 if string not found
                {
                    uncommentedLines[x] = lines[x];  //do nothing
                }
                else
                {
                    int commentIndex = lines[x].IndexOf(";");
                    uncommentedLines[x] = lines[x].Substring(0, lines[x].IndexOf(";"));
                }
                uncommentedLines[x] = uncommentedLines[x].Trim();
                x++;
            }

            foreach (string line in uncommentedLines)
            {
                if (line == "")
                {
                    //empty line, do noting
                }
                else
                {
                    content.Add(line);
                }
            }

            string topFloor = Get_Byte(content, "BOTTOM:", 2) + 'H';
            string botFloor = Get_Byte(content, "BOTTOM:", 1) + 'H';
            string i4o = Get_Nibble(content, "LOBBY:", 40, 1);
            string iox = Get_Nibble(content, "LOBBY:", 40, 0);
            string aiox = Get_Nibble(content, "LOBBY:", 52, 0);
            string rearDoor = Get_Bit(content, "LOBBY:", 12, 0, 3);
            string ceBoard = Get_Bit(content, "BOTTOM:", 6, 1, 1);  
            string ncBoard = Get_Bit(content, "LOBBY:", 33, 1, 2);
            string ftBoard = Get_Bit(content, "BOTTOM:", 6, 1, 3);
            string[,] inputs = Inputs(content, imap);
            JobInfo.Text = "";
            JobInfo.Text = file + "\n\n";
            JobInfo.Text += "Top Floor: " + topFloor + "\n";
            JobInfo.Text += "Bottom Floor: " + botFloor + "\n\n";
            JobInfo.Text += "Independed Rear Doors: " + rearDoor + "\n\n";
            JobInfo.Text += "# of IOX Boards: " + iox + "\n";
            JobInfo.Text += "# of I4O Boards: " + i4o + "\n";
            JobInfo.Text += "# of AIOX Boards: " + aiox + "\n\n";
            JobInfo.Text += "CE Board: " + ceBoard + "\n";
            JobInfo.Text += "NC Board: " + ncBoard + "\n";
            JobInfo.Text += "FT Board: " + ftBoard + "\n\n";

            for (x = 0; x < 8; x++)
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
                    if (inputs[x, y] == null)
                    {
                        JobInfo.Text += "XXXX" + hyphen;
                    }
                    else
                    {
                        JobInfo.Text += inputs[x, y] + hyphen;
                    }
                }
            }
        }

        private void MP2OGM_JobInfo(string file)
        {
            string path = "G:\\Software\\Product\\" + file;
            string[] lines = System.IO.File.ReadAllLines(@path);
            List<string> linesList = lines.ToList();
            string[,] imap = Build_Group_Imap(linesList);
            List<string> content = new List<string>();
            int x = 0;
            string[] uncommentedLines = new string[lines.Length];
            foreach (string line in lines)
            {
                if (lines[x].IndexOf(";") == -1) //indexOf returns -1 if string not found
                {
                    uncommentedLines[x] = lines[x];  //do nothing
                }
                else
                {
                    int commentIndex = lines[x].IndexOf(";");
                    uncommentedLines[x] = lines[x].Substring(0, lines[x].IndexOf(";"));
                }
                uncommentedLines[x] = uncommentedLines[x].Trim();
                x++;
            }

            foreach (string line in uncommentedLines)
            {
                if (line == "")
                {
                    //empty line, do noting
                }
                else
                {
                    content.Add(line);
                }
            }

            string iox = Get_Nibble(content, "LOBBY:", 6, 0);
            string i4o = Get_Nibble(content, "LOBBY:", 6, 1);
            string[,] inputs = Group_Inputs(content, imap);

            JobInfo.Text = "";
            JobInfo.Text = file + "\n\n";
            JobInfo.Text += "# of IOX Boards: " + iox + "\n";
            JobInfo.Text += "# of I4O Boards: " + i4o + "\n\n";

            for (x = 0; x < 8; x++)
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
                    if (inputs[x, y] == null)
                    {
                        JobInfo.Text += "XXXX" + hyphen;
                    }
                    else
                    {
                        JobInfo.Text += inputs[x, y] + hyphen;
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
                if (file.StartsWith("MP2COC"))
                {
                    MP2COC_JobInfo(file);
                }
                if (file.StartsWith("MP2OGM"))
                {
                    MP2OGM_JobInfo(file);
                }
            }
        }
        
    }
}
