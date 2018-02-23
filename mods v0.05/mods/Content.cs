using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mods
{
    public class Content
    {
        List<string> content = new List<string>();
        string[,] imap,omap;
        public string[,] inputs,outputs;
        List<string> inputLabels = new List<string>{ "IOINPE", "IOXINE", "IOIA", "IOELIG" };
        List<string> outputLabels = new List<string>{ "IOOUTE", "IOXOUTE", "IOOA" };
        List<string> filepaths = new List<string> { "G:\\Software\\" };


        public Content(string file)
        {
            this.content = Get_Content(file);
            this.imap = Build_IOmap(file,'I');
            this.inputs = IO(this.imap,'I');
            this.omap = Build_IOmap(file,'O');
            this.outputs = IO(this.omap,'O');
        }

        private List<string> Get_Content(string file)
        {
            List<string> lines = new List<string>();

            foreach (string filepath in filepaths)
            {
                try
                {
                    string path = filepath + file;
                    lines = System.IO.File.ReadAllLines(@path).ToList();
                }
                catch
                {
                    
                }
            }
            int x = 0;
            string[] uncommentedLines = new string[lines.Count];
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

            return content;
        }

        private string Remove_Prefix(string text, string prefix)
        {
            if (text.StartsWith(prefix))
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
            if (text.IndexOf("H") == -1)
            {
                return false;
            }
            return true;
        }

        private string Hex_To_Bin(string hex)
        {
            string strippedHex = Remove_Suffix(Remove_Prefix(hex, "DB"), "H").Trim();
            strippedHex = strippedHex.Substring(strippedHex.Length - 2, 2);
            return HexStringToBinary(strippedHex);
        }

        private string Dec_To_Hex(string deci)
        {
            int dec = Int32.Parse(deci);
            return dec.ToString("X").PadLeft(3,'0');
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

        private static readonly Dictionary<string, string> ioConverter = new Dictionary<string, string> {
            { "OFF/FRBYP", "FRBYP" }
        };

        private string HexStringToBinary(string hex)
        {
            StringBuilder result = new StringBuilder();
            foreach (char c in hex)
            {
                // This will crash for non-hex characters. You might want to handle that differently.
                result.Append(hexCharacterToBinary[char.ToLower(c)]);
            }
            return result.ToString();
        }

        public string Get_Bit(string label, int offset, int nibble, int bit)
        {
            int index = this.content.FindIndex(x => x.StartsWith(label)) + offset;

            string value = Remove_Prefix(this.content[index],"DB").Trim();

            if(!Is_Hex(value))
            {
                value = Dec_To_Hex(value);
            }

            string cleanString = Remove_Suffix(value, "H").Trim();

            cleanString = cleanString.Substring(cleanString.Length - nibble - 1, 1); //High nibble = 1; Low nibble = 0

            string binary = HexStringToBinary(cleanString);

            if (binary[bit] == '1')
            {
                return "YES";
            }
            else
            {
                return "NO";
            }
        }

        public string Get_Nibble(string label, int offset, int nibble)
        {
            int index = this.content.FindIndex(x => x.StartsWith(label)) + offset;

            string value = Remove_Prefix(this.content[index], "DB").Trim();

            if (!Is_Hex(value))
            {
                value = Dec_To_Hex(value);
            }

            string cleanString = Remove_Suffix(value,"H").Trim();

            return cleanString.Substring(cleanString.Length - nibble - 1, 1);
        }

        public string Get_Byte(string label, int offset)
        {
            int index = this.content.FindIndex(x => x.StartsWith(label)) + offset;

            string value = Remove_Prefix(this.content[index], "DB").Trim();

            if (!Is_Hex(value))
            {
                value = Dec_To_Hex(value);
            }

            string cleanString = Remove_Suffix(value, "H").Trim();

            return cleanString.Substring(cleanString.Length - 2, 2);
        }

        public string Get_String(string label, int offset)
        {
            int index = this.content.FindIndex(x => x.StartsWith(label)) + offset;
            string cleanString = Remove_Suffix(Remove_Prefix(this.content[index], "DB"), "H").Trim();

            return cleanString.Substring(1, cleanString.Length - 2);
        }

        private static readonly Dictionary<char, int> hexCharacterToDecimal = new Dictionary<char, int> {
            { '0', 0 },
            { '1', 1 },
            { '2', 2 },
            { '3', 3 },
            { '4', 4 },
            { '5', 5 },
            { '6', 6 },
            { '7', 7 },
            { '8', 8 },
            { '9', 9 },
            { 'a', 10 },
            { 'b', 11 },
            { 'c', 12 },
            { 'd', 13 },
            { 'e', 14 },
            { 'f', 15 }
        };

        public int HexStringToDecimal(string hex)
        {
            //Need to reverse the hex string for the math to work out better
            hex = Remove_Suffix(hex, "H");

            char[] charArray = hex.ToCharArray();
            Array.Reverse(charArray);
            hex = new string(charArray);

            int dec_value = 0;
            int x = 0;
            foreach (char c in hex)
            {
                if (x == 0)
                {
                    dec_value += hexCharacterToDecimal[char.ToLower(c)];
                }
                else
                {
                    dec_value += 16 * x * hexCharacterToDecimal[char.ToLower(c)];
                }
                x++;
            }
            return dec_value;
        }

        private string[,] Build_IOmap(string file, char io)
        {
            List<string> lines = new List<string>();

            foreach (string filepath in filepaths)
            {
                try
                {
                    string path = filepath + file;
                    lines = System.IO.File.ReadAllLines(@path).ToList();
                }
                catch
                {

                }
            }

            string[,] iomap = new string[32, 8];

            List<string> ioLabels = new List<string>();

            int io_x = 0;
            int io_y = 0;

            string[] labelNumbers = { "", "2", "3", "4" };

            if (io == 'I')
            {
                ioLabels = this.inputLabels;
            }

            if (io == 'O')
            {
                ioLabels = this.outputLabels;
            }

            foreach (string ioLabel in ioLabels)
            {
                foreach (string labelNumber in labelNumbers)
                {
                    if (lines.FindIndex(x => x.StartsWith(ioLabel + labelNumber + ":")) != -1)
                    {
                        int io_index = lines.FindIndex(i => i.StartsWith(ioLabel + labelNumber + ":"));
                        int x = 0;
                        int iomap_index = io_index + x + 1;

                        while (lines[iomap_index].Trim().StartsWith("DB"))
                        {
                            int comment_index = lines[iomap_index].IndexOf(';');
                            string comment_string = lines[iomap_index].Substring(comment_index, lines[iomap_index].Length - comment_index).Trim();
                            bool building = false;
                            StringBuilder ioCode = new StringBuilder();

                            for (int c = 0; c < comment_string.Length; c++)
                            {
                                if (building)
                                {
                                    if (Char.IsLetterOrDigit(comment_string[c]) || comment_string[c] == '/')
                                    {
                                        ioCode.Append(comment_string[c]);
                                    }
                                    else
                                    {
                                        if (ioConverter.ContainsKey(ioCode.ToString()))
                                        {
                                            iomap[io_x, io_y] = ioConverter[ioCode.ToString()];
                                        }
                                        else
                                        {
                                            iomap[io_x, io_y] = ioCode.ToString();
                                        }
                                        ioCode.Clear();
                                        io_y++;
                                        if (io_y == 8)
                                        {
                                            io_y = 0;
                                            io_x++;
                                        }
                                        building = false;
                                    }
                                }
                                else
                                {
                                    if (Char.IsLetterOrDigit(comment_string[c]) || comment_string[c] == '/')
                                    {
                                        ioCode.Append(comment_string[c]);
                                        building = true;
                                    }
                                    else
                                    {
                                        //do nothing
                                    }
                                }
                            }
                            //This block of code is to write the last ioCode of the line
                            if (ioConverter.ContainsKey(ioCode.ToString()))
                            {
                                iomap[io_x, io_y] = ioConverter[ioCode.ToString()];
                            }
                            else
                            {
                                iomap[io_x, io_y] = ioCode.ToString();
                            }
                            ioCode.Clear();
                            io_y++;
                            if (io_y == 8)
                            {
                                io_y = 0;
                                io_x++;
                            }
                            building = false;
                            x++;
                            iomap_index = io_index + x + 1;
                        }
                    }
                }
            }
            return iomap;
        }

        public string[,] IO(string[,] iomap, char io)
        {
            int io_x = 0;
            int io_y = 7;

            List<string> ioLabels = new List<string>();

            string[,] ios = new string[8, 8];

            if(io == 'I')
            {
                ioLabels = this.inputLabels;
            }

            if (io == 'O')
            {
                ioLabels = this.outputLabels;
            }

            foreach (string ioLabel in ioLabels)
            {
                if (this.content.FindIndex(x => x.StartsWith(ioLabel + ":")) != -1)
                {
                    int io_index = this.content.FindIndex(x => x.StartsWith(ioLabel + ":"));
                    for (int x = 0; x < 8; x++)
                    {
                        int iomap_index = io_index + x + 1;
                        string iomap_binary = Hex_To_Bin(this.content[iomap_index]);

                        for (int y = 0; y < 8; y++)
                        {
                            if (io == 'I')
                            {
                                if (iomap_binary[7 - y] == '1')
                                {
                                    ios[io_x, io_y] = iomap[x, 7 - y];
                                    io_y--;

                                    if (io_y == -1)
                                    {
                                        io_x++;
                                        io_y = 7;
                                    }
                                }
                            }
                            else //This is needed because outputs get added in reverse
                            {
                                if (iomap_binary[y] == '1')
                                {
                                    ios[io_x, io_y] = iomap[x, y];
                                    io_y--;

                                    if (io_y == -1)
                                    {
                                        io_x++;
                                        io_y = 7;
                                    }
                                }
                            }
                        }
                    }
                }

                if (this.content.FindIndex(x => x.StartsWith(ioLabel + "2:")) != -1)
                {
                    int io2_index = this.content.FindIndex(x => x.StartsWith(ioLabel + "2:"));
                    for (int x = 0; x < 8; x++)
                    {
                        int iomap_index = io2_index + x + 1;
                        string iomap_binary = Hex_To_Bin(this.content[iomap_index]);

                        for (int y = 0; y < 8; y++)
                        {
                            if (io == 'I')
                            {
                                if (iomap_binary[7 - y] == '1')
                                {
                                    ios[io_x, io_y] = iomap[x+8, 7 - y];
                                    io_y--;

                                    if (io_y == -1)
                                    {
                                        io_x++;
                                        io_y = 7;
                                    }
                                }
                            }
                            else //This is needed because outputs get added in reverse
                            {
                                if (iomap_binary[y] == '1')
                                {
                                    ios[io_x, io_y] = iomap[x+8, y];
                                    io_y--;

                                    if (io_y == -1)
                                    {
                                        io_x++;
                                        io_y = 7;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return ios;
        }
    }
}
