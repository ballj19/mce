using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mods
{
    public class Content
    {
        List<string> content;
        string[,] imap,omap;
        public string[,] inputs,outputs;

        public Content(string file)
        {
            this.content = Get_Content(file);

            if (file.StartsWith("MP2COC"))
            {
                this.imap = Build_Imap(file);
                this.inputs = Inputs(this.imap);
                this.omap = Build_Omap(file);
                this.outputs = Outputs(this.omap);
            }
            if (file.StartsWith("MP2OGM"))
            {
                this.imap = Build_Group_Imap(file);
                this.inputs = Group_Inputs(this.imap);
                this.omap = Build_Group_Omap(file);
                this.outputs = Group_Outputs(this.omap);
            }
        }

        private List<string> Get_Content(string file)
        {
            string path = "G:\\Software\\Product\\" + file;
            string[] lines = System.IO.File.ReadAllLines(@path);
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

            return content;
        }

        private string[,] Build_Imap(string file)
        {
            string path = "G:\\Software\\Product\\" + file;
            string[] linesArray = System.IO.File.ReadAllLines(@path);
            List<string> lines = linesArray.ToList();

            string[,] imap = new string[32, 8];

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

        private string[,] Build_Omap(string file)
        {
            string path = "G:\\Software\\Product\\" + file;
            string[] linesArray = System.IO.File.ReadAllLines(@path);
            List<string> lines = linesArray.ToList();

            string[,] omap = new string[32, 8];

            int output_x = 0;
            int output_y = 0;

            if (lines.FindIndex(x => x.StartsWith("IOOUTE:")) != -1)
            {
                int output_index = lines.FindIndex(x => x.StartsWith("IOOUTE:"));
                for (int x = 0; x < 8; x++)
                {
                    int omap_index = output_index + x + 1;

                    int comment_index = lines[omap_index].IndexOf(';');
                    string comment_string = lines[omap_index].Substring(comment_index, lines[omap_index].Length - comment_index);
                    bool building = false;
                    StringBuilder outCode = new StringBuilder();

                    for (int c = 0; c < comment_string.Length; c++)
                    {
                        if (building)
                        {
                            if (Char.IsLetterOrDigit(comment_string[c]) || comment_string[c] == '/')
                            {
                                outCode.Append(comment_string[c]);
                            }
                            else
                            {
                                omap[output_x, output_y] = outCode.ToString();
                                outCode.Clear();
                                output_y++;
                                if (output_y == 8)
                                {
                                    output_y = 0;
                                    output_x++;
                                }
                                building = false;
                            }
                        }
                        else
                        {
                            if (Char.IsLetterOrDigit(comment_string[c]) || comment_string[c] == '/')
                            {
                                outCode.Append(comment_string[c]);
                                building = true;
                            }
                            else
                            {
                                //do nothing
                            }
                        }
                    }
                    //This block of code is to write the last outCode of the line
                    omap[output_x, output_y] = outCode.ToString();  //{
                    outCode.Clear();
                    output_y++;
                    if (output_y == 8)
                    {
                        output_y = 0;
                        output_x++;
                    }
                    building = false;                            //}

                }
            }

            if (lines.FindIndex(x => x.StartsWith("IOOUTE2:")) != -1)
            {
                int output2_index = lines.FindIndex(x => x.StartsWith("IOOUTE2:"));
                for (int x = 0; x < 8; x++)
                {
                    int omap_index = output2_index + x + 1;

                    int comment_index = lines[omap_index].IndexOf(';');
                    string comment_string = lines[omap_index].Substring(comment_index, lines[omap_index].Length - comment_index);
                    bool building = false;
                    StringBuilder outCode = new StringBuilder();

                    for (int c = 0; c < comment_string.Length; c++)
                    {
                        if (building)
                        {
                            if (Char.IsLetterOrDigit(comment_string[c]) || comment_string[c] == '/')
                            {
                                outCode.Append(comment_string[c]);
                            }
                            else
                            {
                                omap[output_x, output_y] = outCode.ToString();
                                outCode.Clear();
                                output_y++;
                                if (output_y == 8)
                                {
                                    output_y = 0;
                                    output_x++;
                                }
                                building = false;
                            }
                        }
                        else
                        {
                            if (Char.IsLetterOrDigit(comment_string[c]) || comment_string[c] == '/')
                            {
                                outCode.Append(comment_string[c]);
                                building = true;
                            }
                            else
                            {
                                //do nothing
                            }
                        }
                    }
                    //This block of code is to write the last outCode of the line
                    omap[output_x, output_y] = outCode.ToString();  //{
                    outCode.Clear();
                    output_y++;
                    if (output_y == 8)
                    {
                        output_y = 0;
                        output_x++;
                    }
                    building = false;                            //}

                }
            }

            if (lines.FindIndex(x => x.StartsWith("IOOUTE3:")) != -1)
            {
                int output3_index = lines.FindIndex(x => x.StartsWith("IOOUTE3:"));
                for (int x = 0; x < 8; x++)
                {
                    int omap_index = output3_index + x + 1;

                    int comment_index = lines[omap_index].IndexOf(';');
                    string comment_string = lines[omap_index].Substring(comment_index, lines[omap_index].Length - comment_index);
                    bool building = false;
                    StringBuilder outCode = new StringBuilder();

                    for (int c = 0; c < comment_string.Length; c++)
                    {
                        if (building)
                        {
                            if (Char.IsLetterOrDigit(comment_string[c]) || comment_string[c] == '/')
                            {
                                outCode.Append(comment_string[c]);
                            }
                            else
                            {
                                omap[output_x, output_y] = outCode.ToString();
                                outCode.Clear();
                                output_y++;
                                if (output_y == 8)
                                {
                                    output_y = 0;
                                    output_x++;
                                }
                                building = false;
                            }
                        }
                        else
                        {
                            if (Char.IsLetterOrDigit(comment_string[c]) || comment_string[c] == '/')
                            {
                                outCode.Append(comment_string[c]);
                                building = true;
                            }
                            else
                            {
                                //do nothing
                            }
                        }
                    }
                    //This block of code is to write the last outCode of the line
                    omap[output_x, output_y] = outCode.ToString();  //{
                    outCode.Clear();
                    output_y++;
                    if (output_y == 8)
                    {
                        output_y = 0;
                        output_x++;
                    }
                    building = false;                            //}

                }
            }

            if (lines.FindIndex(x => x.StartsWith("IOOUTE4:")) != -1)
            {
                int output4_index = lines.FindIndex(x => x.StartsWith("IOOUTE4:"));
                for (int x = 0; x < 8; x++)
                {
                    int omap_index = output4_index + x + 1;

                    int comment_index = lines[omap_index].IndexOf(';');
                    string comment_string = lines[omap_index].Substring(comment_index, lines[omap_index].Length - comment_index);
                    bool building = false;
                    StringBuilder outCode = new StringBuilder();

                    for (int c = 0; c < comment_string.Length; c++)
                    {
                        if (building)
                        {
                            if (Char.IsLetterOrDigit(comment_string[c]) || comment_string[c] == '/')
                            {
                                outCode.Append(comment_string[c]);
                            }
                            else
                            {
                                omap[output_x, output_y] = outCode.ToString();
                                outCode.Clear();
                                output_y++;
                                if (output_y == 8)
                                {
                                    output_y = 0;
                                    output_x++;
                                }
                                building = false;
                            }
                        }
                        else
                        {
                            if (Char.IsLetterOrDigit(comment_string[c]) || comment_string[c] == '/')
                            {
                                outCode.Append(comment_string[c]);
                                building = true;
                            }
                            else
                            {
                                //do nothing
                            }
                        }
                    }
                    //This block of code is to write the last outCode of the line
                    omap[output_x, output_y] = outCode.ToString();  //{
                    outCode.Clear();
                    output_y++;
                    if (output_y == 8)
                    {
                        output_y = 0;
                        output_x++;
                    }
                    building = false;                            //}

                }
            }

            return omap;
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

        private string[,] Build_Group_Imap(string file)
        {
            string path = "G:\\Software\\Product\\" + file;
            string[] linesArray = System.IO.File.ReadAllLines(@path);
            List<string> lines = linesArray.ToList();

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

        private string[,] Build_Group_Omap(string file)
        {
            string path = "G:\\Software\\Product\\" + file;
            string[] linesArray = System.IO.File.ReadAllLines(@path);
            List<string> lines = linesArray.ToList();

            string[,] omap = new string[16, 8];

            int output_x = 0;
            int output_y = 0;

            if (lines.FindIndex(x => x.StartsWith("IOXOUTE:")) != -1)
            {
                int output_index = lines.FindIndex(x => x.StartsWith("IOXOUTE:"));
                for (int x = 0; x < 8; x++)
                {
                    int omap_index = output_index + x + 1;

                    int comment_index = lines[omap_index].IndexOf(';');
                    string comment_string = lines[omap_index].Substring(comment_index, lines[omap_index].Length - comment_index);
                    bool building = false;
                    StringBuilder outCode = new StringBuilder();

                    for (int c = 0; c < comment_string.Length; c++)
                    {
                        if (building)
                        {
                            if (Char.IsLetterOrDigit(comment_string[c]) || comment_string[c] == '/')
                            {
                                outCode.Append(comment_string[c]);
                            }
                            else
                            {
                                omap[output_x, output_y] = outCode.ToString();
                                outCode.Clear();
                                output_y++;
                                if (output_y == 8)
                                {
                                    output_y = 0;
                                    output_x++;
                                }
                                building = false;
                            }
                        }
                        else
                        {
                            if (Char.IsLetterOrDigit(comment_string[c]) || comment_string[c] == '/')
                            {
                                outCode.Append(comment_string[c]);
                                building = true;
                            }
                            else
                            {
                                //do nothing
                            }
                        }
                    }
                    //This block of code is to write the last outCode of the line
                    omap[output_x, output_y] = outCode.ToString();  //{
                    outCode.Clear();
                    output_y++;
                    if (output_y == 8)
                    {
                        output_y = 0;
                        output_x++;
                    }
                    building = false;                            //}

                }
            }

            return omap;
        }

        private string Hex_To_Bin(string hex)
        {
            string strippedHex = Remove_Suffix(Remove_Prefix(hex, "DB"), "H").Trim();
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
            string cleanString = Remove_Suffix(Remove_Prefix(this.content[index], "DB"), "H").Trim();

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
            string cleanString = Remove_Suffix(Remove_Prefix(this.content[index], "DB"), "H").Trim();

            return cleanString.Substring(cleanString.Length - nibble - 1, 1);
        }

        public string Get_Byte(string label, int offset)
        {
            int index = this.content.FindIndex(x => x.StartsWith(label)) + offset;
            string cleanString = Remove_Suffix(Remove_Prefix(this.content[index], "DB"), "H").Trim();

            return cleanString.Substring(cleanString.Length - 2, 2);
        }

        public string Get_String(string label, int offset)
        {
            int index = this.content.FindIndex(x => x.StartsWith(label)) + offset;
            string cleanString = Remove_Suffix(Remove_Prefix(this.content[index], "DB"), "H").Trim();

            return cleanString.Substring(1, cleanString.Length - 2);
        }

        public string[,] Inputs(string[,] imap)
        {
            int input_x = 0;
            int input_y = 7;

            string[,] inputs = new string[8, 8];

            int input_index = this.content.FindIndex(x => x.StartsWith("IOINPE:"));
            int input2_index = this.content.FindIndex(x => x.StartsWith("IOINPE2:"));

            for (int x = 0; x < 8; x++)
            {
                int imap_index = input_index + x + 1;
                string imap_binary = Hex_To_Bin(this.content[imap_index]);

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

            for (int x = 0; x < 8; x++)
            {
                int imap_index = input2_index + x + 1;
                string imap_binary = Hex_To_Bin(this.content[imap_index]);

                for (int y = 0; y < 8; y++)
                {
                    if (imap_binary[7 - y] == '1')
                    {
                        inputs[input_x, input_y] = imap[x + 8, 7 - y];
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

        public string[,] Outputs(string[,] omap)
        {
            int output_x = 0;
            int output_y = 7;

            string[,] outputs = new string[8, 8];

            int output_index = this.content.FindIndex(x => x.StartsWith("IOOUTE:"));
            int output2_index = this.content.FindIndex(x => x.StartsWith("IOOUTE2:"));

            for (int x = 0; x < 8; x++)
            {
                int omap_index = output_index + x + 1;
                string omap_binary = Hex_To_Bin(this.content[omap_index]);

                for (int y = 0; y < 8; y++)
                {
                    if (omap_binary[7 - y] == '1')
                    {
                        outputs[output_x, output_y] = omap[x, 7 - y];
                        output_y--;

                        if (output_y == -1)
                        {
                            output_x++;
                            output_y = 7;
                        }
                    }
                }
            }

            for (int x = 0; x < 8; x++)
            {
                int omap_index = output2_index + x + 1;
                string omap_binary = Hex_To_Bin(this.content[omap_index]);

                for (int y = 0; y < 8; y++)
                {
                    if (omap_binary[7 - y] == '1')
                    {
                        outputs[output_x, output_y] = omap[x + 8, 7 - y];
                        output_y--;

                        if (output_y == -1)
                        {
                            output_x++;
                            output_y = 7;
                        }
                    }
                }
            }

            return outputs;
        }

        public string[,] Group_Inputs(string[,] imap)
        {
            int input_x = 0;
            int input_y = 7;

            string[,] inputs = new string[16, 8];

            int input_index = this.content.FindIndex(x => x.StartsWith("IOXINE:"));

            for (int x = 0; x < 16; x++)
            {
                int imap_index = input_index + x + 1;
                string imap_binary = Hex_To_Bin(this.content[imap_index]);

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

        public string[,] Group_Outputs(string[,] imap)
        {
            int output_x = 0;
            int output_y = 7;

            string[,] outputs = new string[16, 8];

            int output_index = this.content.FindIndex(x => x.StartsWith("IOXOUTE:"));

            for (int x = 0; x < 16; x++)
            {
                int omap_index = output_index + x + 1;
                string omap_binary = Hex_To_Bin(this.content[omap_index]);

                for (int y = 0; y < 8; y++)
                {
                    if (omap_binary[7 - y] == '1')
                    {
                        outputs[output_x, output_y] = omap[x, 7 - y];
                        output_y--;

                        if (output_y == -1)
                        {
                            output_x++;
                            output_y = 7;
                        }
                    }
                }
            }

            return outputs;
        }
    }
}
