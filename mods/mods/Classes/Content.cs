﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace mods
{
    public class Content
    {
        public List<string> content = new List<string>();
        public List<string> inputs, outputs;
        //List<string> inputLabels = new List<string> { "IOINPE", "IOXINE", "IOIA", "IOELIG" };
        //List<string> outputLabels = new List<string> { "IOOUTE", "IOXOUTE", "IOOA" };
        public List<string> inputLabels = new List<string>();
        public List<string> outputLabels = new List<string>();
        string filepath = @"\\10.113.32.45\shared\Software\";
        public string file;

        public Content(string file, bool GetIO = true)
        {
            string path = "";

            if (file.Contains(@"\Software\"))
            {
                path = file;
            }
            else
            {
                path = filepath + file;
            }
            this.file = General.Get_File_From_Path(path);
            this.filepath = General.Get_Folder_From_Path(path);
            this.content = Get_Content();
            if(GetIO)
            {
                Get_IO_Labels();
                this.inputs = IO(inputLabels, 'I');
                this.outputs = IO(outputLabels, 'O');
            }
        }

        private List<string> Get_Content()
        {
            IEnumerable<string> lines;

            try
            {
                lines = System.IO.File.ReadLines(filepath + file);
            }
            catch
            {
                lines = new string[0];
            }
            int x = 0;
            string[] uncommentedLines = new string[lines.Count()];
            foreach (string line in lines)
            {
                if (line.IndexOf(";") == -1) //indexOf returns -1 if string not found
                {
                    uncommentedLines[x] = line;  //do nothing
                }
                else
                {
                    int commentIndex = line.IndexOf(";");
                    uncommentedLines[x] = line.Substring(0, line.IndexOf(";"));
                }
                uncommentedLines[x] = uncommentedLines[x].Trim();
                x++;
            }

            foreach (string line in uncommentedLines)
            {
                if (line == "")
                {
                    //empty line, do nothing
                }
                else
                {
                    //Need this logic because sometimes the first byte is defined on the same line as the label - inconsistently
                    //So we force the defined byte onto the next line always
                    if (line.Contains(":") && !line.EndsWith(":"))
                    {
                        int colonIndex = line.IndexOf(":");
                        content.Add(line.Substring(0, colonIndex + 1).Trim());
                        content.Add(line.Substring(colonIndex + 1, line.Length - colonIndex - 1).Trim());
                    }
                    else
                    {
                        char[] trim = { '\u001a' };
                        string trimmedLine = line.Trim(trim);
                        trimmedLine = trimmedLine.Trim();
                        if(trimmedLine == "END")
                        {
                            content.Add(trimmedLine);
                        }
                        else
                        {
                            content.Add(line);
                        }
                    }
                }
            }
            return content;
        }
        
        private void Get_IO_Labels()
        {
            foreach(string line in content)
            {
                string value = General.Value(line);
                if (value.EndsWith(":"))
                {
                    if (value.StartsWith("IOXI") || value.StartsWith("IOI") || value.StartsWith("IOELIG"))
                    {
                        inputLabels.Add(value.Substring(0, value.Length - 1)); //Add without ending :
                    }
                    else if(value.StartsWith("IOXO") || value.StartsWith("IOO"))
                    {
                        outputLabels.Add(value.Substring(0, value.Length - 1)); //Add without ending :
                    }
                }
            }
        }

        public string Get_Bit(string label, int offset, int nibble, int bit)
        {
            try
            {
                List<string> bytes = General.Get_Bytes_List(label,content);

                string value = bytes[offset];

                string cleanString = General.Remove_Suffix(value, "H").Trim();

                cleanString = cleanString.Substring(cleanString.Length - nibble - 1, 1); //High nibble = 1; Low nibble = 0

                string binary = General.HexStringToBinary(cleanString);

                if (binary[bit] == '1')
                {
                    return "YES";
                }
                else
                {
                    return "NO";
                }
            }
            catch
            {
                return "N/A";
            }
            
        }

        public string Get_Nibble(string label, int offset, int nibble)
        {
            try
            {
                List<string> bytes = General.Get_Bytes_List(label,content);

                string value = bytes[offset];

                string cleanString = General.Remove_Suffix(value, "H").Trim();

                return cleanString.Substring(cleanString.Length - nibble - 1, 1);
            }
            catch
            {
                return "N/A";
            }
            
        }

        public string Get_Byte(string label, int offset)
        {
            try
            {
                List<string> bytes = General.Get_Bytes_List(label,content);

                string value = bytes[offset];

                string cleanString = General.Remove_Suffix(value, "H").Trim();

                return cleanString.Substring(cleanString.Length - 2, 2);
            }
            catch
            {
                return "N/A";
            }
            
        }

        public string Get_String(string label, int offset)
        {
            try
            {
                int index = this.content.FindIndex(x => x.StartsWith(label)) + offset;
                string cleanString = General.Remove_Suffix(General.Remove_Prefix(this.content[index], "DB"), "H").Trim();

                return cleanString.Substring(1, cleanString.Length - 2);
            }
            catch
            {
                return "N/A";
            }
        }

        public string Get_Comma_Separated_Byte(string label, int offset, int byteNum)
        {
            try
            {
                int index = this.content.FindIndex(x => x.StartsWith(label)) + offset;

                string value = General.Remove_Prefix(this.content[index], "DB").Trim();

                for (int b = 0; b < byteNum; b++)
                {
                    int commaIndex = value.IndexOf(",");
                    value = value.Substring(commaIndex + 1, value.Length - commaIndex - 1);
                }

                if (value.IndexOf(",") == -1)
                {
                    //do nothing
                }
                else
                {
                    int commaIndex = value.IndexOf(",");
                    value = value.Substring(0, commaIndex);
                }

                if (!General.Is_Hex(value))
                {
                    value = General.Dec_To_Hex(value);
                }

                string cleanString = General.Remove_Suffix(value, "H").Trim();

                return cleanString.Substring(cleanString.Length - 2, 2);
            }
            catch
            {
                return "N/A";
            }
        }

        public List<string> Build_IOmap(List<string> ioLabels)
        {
            List<string> iomap = new List<string>();

            List<string> lines = General.Get_Clean_Lines_From_Path(filepath + file);

            foreach (string ioLabel in ioLabels)
            {
                if (lines.FindIndex(x => x.StartsWith(ioLabel + ":")) != -1)
                {
                    int io_index = lines.FindIndex(i => i.StartsWith(ioLabel + ":"));
                    int x = 0;
                    int iomap_index = io_index + x + 1;

                    while (!lines[iomap_index].Trim().EndsWith(":"))
                    {
                        if(General.Value(lines[iomap_index]).StartsWith("DB"))
                        {
                            if (General.Value(lines[iomap_index]).Contains("0,"))
                            {
                                //Fix for comma separated zeros bugging IO
                            }
                            else
                            {
                                iomap.AddRange(Crawl_Options(lines, iomap_index, 8));
                            }
                        }
                        x++;
                        iomap_index = io_index + x + 1;
                    }
                }
            }
            return iomap;
        }

        public List<string> IO(List<string> ioLabels, char io = 'I')
        {
            List<string> ios = new List<string>();
            List<string> ioValues = new List<string>();
            List<string> iomap = Build_IOmap(ioLabels);
            
            foreach(string ioLabel in ioLabels)
            {
                if (this.content.FindIndex(i => i.StartsWith(ioLabel + ":")) != -1)
                {
                    int io_index = this.content.FindIndex(i => i.StartsWith(ioLabel + ":")) + 1;

                    while(content[io_index].StartsWith("DB"))
                    {
                        if(content[io_index].Contains("0,"))
                        {
                            //Fix for comma separated zeros bugging IO
                        }
                        else
                        {
                            ioValues.Add(General.Value(content[io_index]));
                        }
                        io_index++;
                    }
                }
            }

            if(io == 'I')
            {
                for (int x = 0; x < iomap.Count / 8; x++)
                {
                    string iomap_binary = General.Hex_To_Bin(ioValues[x]);

                    for (int y = 0; y < 8; y++)
                    {
                        if (iomap_binary[7 - y] == '1')
                        {
                            ios.Add(iomap[x * 8 + 7 - y]);
                        }
                    }
                }
            }
            else
            {
                for (int x = 0; x < iomap.Count / 8; x++)
                {
                    string iomap_binary = General.Hex_To_Bin(ioValues[x]);

                    for (int y = 0; y < 8; y++)
                    {
                        if (iomap_binary[y] == '1')
                        {
                            ios.Add(iomap[x * 8 + y]);
                        }
                    }
                }
            }

            return ios;
        }

        public int Get_Group_Top_Level()
        {
            string top_string = Get_Byte("LOBBY:", 20);
            return General.HexStringToDecimal(top_string);
        }

        public int Get_Group_Num_Of_Cars()
        {
            int number_of_cars = 0;

            for (int i = 1; i < 36; i += 3)
            {
                if (this.Get_Byte("CARVAR", i) != "00")
                {
                    number_of_cars++;
                }
            }

            return number_of_cars;
        }

        public string Build_OptionsMap(string label)
        {
            string OptionsBlock = "";

            List<string> optionsConfig = new List<string>();

            if(label == "LOBBY:")
            {
                optionsConfig = LobbyConfig;
                OptionsBlock += "LOBBY\n";
            }
            else if(label == "BOTTOM:")
            {
                optionsConfig = BottomConfig;
                OptionsBlock += "BOTTOM\n";
            }

            List<string> lines = General.Get_Clean_Lines_From_Path(filepath + file);            

            if (lines.FindIndex(x => x.StartsWith(label)) != -1)
            {
                int lobbyIndex = lines.FindIndex(x => x.StartsWith(label));

                //Configuration A:  XXXX-XXXX-XXXX-XXXX
                //Configuration B:  Bits 0-7 = X
                //Configuration C:  Bits 0-3 = X or Bits 4-7 = X
                //Configuration D:  Should Always Be X
                //Configuration E:  000H = X
                //Configuration F:  XXXX-XXXX Bits 5-0
                //Configuration G:  XXXX-XXXX-XXXX-XXXX--XXXX-XXXX Bits 2-0
                //Configuration Z:  Not Used

                int index = lobbyIndex + 1;
                int byteIndex = 0;

                while(!lines[index].EndsWith(":"))
                {
                    if(General.Value(lines[index]).StartsWith("DB"))
                    {
                        string byteString = "";
                        string titleString = "";

                        if (byteIndex < 16)
                        {
                            string comment = General.Comment(lines[index]);
                            titleString += comment.Substring(1,2) + " - ";
                        }
                        else
                        {
                            string comment = General.Comment(lines[index]);
                            titleString += comment.Substring(1, 3) + " - ";
                        }

                        for (int n = 0; n < optionsConfig[byteIndex].Length; n++)
                        {
                            if (optionsConfig[byteIndex][n] == 'A')
                            {
                                int numOfOptions = 4;
                                string binary = General.Hex_To_Bin(General.Value(lines[index]));
                                string nibbleBinary = binary.Substring(n * 4, 4);
                                List<string> options = Crawl_Options(lines, index + 1 + n,numOfOptions);
                                
                                for (int b = 0; b < numOfOptions; b++)
                                {
                                    if (nibbleBinary[b] == '1')
                                    {
                                        byteString += options[b] + ", ";
                                    }
                                }
                            }
                            else if(optionsConfig[byteIndex][n] == 'F')
                            {
                                int numOfOptions = 2;
                                string binary = General.Hex_To_Bin(General.Value(lines[index]));
                                string nibbleBinary = binary.Substring(n * 4, 4);
                                List<string> options = Crawl_Options(lines, index + 1 + n, numOfOptions);

                                for (int b = 0; b < numOfOptions; b++)
                                {
                                    if (nibbleBinary[b] == '1')
                                    {
                                        byteString += options[b] + ", ";
                                    }
                                }
                            }
                            else if(optionsConfig[byteIndex][n] == 'G')
                            {
                                int numOfOptions = 6;
                                string binary = General.Hex_To_Bin(General.Value(lines[index]));
                                List<string> options = Crawl_Options(lines, index + 1 + n, numOfOptions);

                                for (int b = 0; b < numOfOptions; b++)
                                {
                                    if (binary[b] == '1')
                                    {
                                        byteString += options[b] + ", ";
                                    }
                                }
                            }
                        }

                        if(byteString.Length > 0)
                        {
                            byteString = byteString.Substring(0, byteString.Length - 2);
                        }
                        OptionsBlock += titleString + byteString + "\n";
                        byteIndex++;
                    }
                    index++;
                }
            }

            return OptionsBlock;
        }

        private List<string> Crawl_Options(List<string> lines, int offset, int numOfOptions = 8)
        {
            List<char> Acceptable_Chars = new List<char>
            {
                '/',
                '_',
                ' ',
                '(',
                ')',
            };

            List<string> options = new List<string>();
            int comment_index = lines[offset].IndexOf(';');
            if (comment_index != -1)
            {
                string comment_string = lines[offset].Substring(comment_index, lines[offset].Length - comment_index).Trim();
                bool building = false;
                StringBuilder opCode = new StringBuilder();

                for (int c = 0; c < comment_string.Length; c++)
                {
                    if (building)
                    {
                        if (Char.IsLetterOrDigit(comment_string[c]) || Acceptable_Chars.Contains(comment_string[c]))
                        {
                            opCode.Append(comment_string[c]);
                        }
                        else
                        {
                            if(opCode.ToString().Trim() != "")
                            {
                                options.Add(opCode.ToString().Trim());
                            }
                            if(options.Count == numOfOptions)
                            {
                                return options;
                            }
                            opCode.Clear();
                            building = false;
                        }
                    }
                    else
                    {
                        if (Char.IsLetterOrDigit(comment_string[c]) || Acceptable_Chars.Contains(comment_string[c]))
                        {
                            opCode.Append(comment_string[c]);
                            building = true;
                        }
                        else
                        {
                            //do nothing
                        }
                    }
                }
                if (opCode.ToString().Trim() != "")
                {
                    options.Add(opCode.ToString().Trim());
                }
            }
            else //This is for the case where there is no comment to go with the byte
            {
                for (int i = 0; i < numOfOptions; i++)
                {
                    options.Add("XXXX");
                }
            }

            return options;
        }

        public List<string> Get_PILabels()
        {
            string pilabelstring = "";
            List<string> piLabels = new List<string>();
            int index = this.content.FindIndex(x => x.StartsWith("PILAB"));
            if (index != -1)
            {
                int c = 1;

                while (this.content[index + c].StartsWith("DB"))
                {
                    pilabelstring += this.Get_String("PILAB", c);
                    c++;
                }

                for (int i = 0; i < pilabelstring.Length / 2; i++)
                {
                    string pilabel = pilabelstring.Substring(i * 2, 2);
                    piLabels.Add(pilabel);
                }
            }
            else
            {
                index = this.content.FindIndex(x => x.StartsWith("CRTVAR:"));

                int c = 9;
                while(this.content[index + c].StartsWith("DB"))
                {
                    pilabelstring += this.Get_String("CRTVAR", c);
                    c++;
                }

                for (int i = 0; i < pilabelstring.Length / 2; i++)
                {
                    string pilabel = pilabelstring.Substring(i * 2, 2);
                    piLabels.Add(pilabel);
                }
            }

            return piLabels;
        }

        public List<string> Get_Job_Summary()
        {
            List<string> lines = General.Get_Clean_Lines_From_Path(filepath + file);
            List<string> jobSummary = new List<string>();

            int endIndex = 0;

            foreach(string line in lines)
            {
                if(General.Comment(line).Contains(";BOARD TYPE:"))
                {
                    endIndex = lines.IndexOf(line);
                    break;
                }
            }

            for(int l = 0; l < endIndex; l++)
            {
                jobSummary.Add(lines[l]);
            }

            return jobSummary;
        }

        private List<string> LobbyConfig = new List<string>
        {
            //Configuration A:  XXXX-XXXX-XXXX-XXXX
            //Configuration B:  Bits 0-7 = X
            //Configuration C:  Bits 0-3 = X or Bits 4-7 = X
            //Configuration D:  Should Always Be X
            //Configuration E:  000H = X
            //Configuration F:  XXXX-XXXX Bits 5-0
            //Configuration G:  XXXX-XXXX-XXXX-XXXX--XXXX-XXXX Bits 2-0
            //Configuration Z:  Not Used
            "B", //00
            "AC",
            "AC",
            "D",
            "CC",
            "D",
            "AA",
            "AA",
            "AA", //08
            "AC",
            "AA",
            "AA",
            "AA",
            "AA",
            "AA",
            "AC",
            "AA", //10
            "AA",
            "AA",
            "AA",
            "AA",
            "AA",
            "AA",
            "AA",
            "AA", //18
            "AA",
            "AC",
            "AA",
            "AA",
            "AA",
            "AA",
            "AC",
            "E", //20
            "AA",
            "AA",
            "AA",
            "AA",
            "AA",
            "AA",
            "CC",
            "CC", //28
            "AC",
            "F",
            "ZZ",
            "DD",
            "B",
            "B",
            "AA",
            "AC", //30
            "CC",
            "F",
            "AC",
            "B",
            "ZC",
            "E",
            "CC",
            "ZZ", //38
            "ZZ",
            "ZZ",
            "ZZ",
            "ZZ",
            "ZZ",
        };

        private List<string> BottomConfig = new List<string>
        {
            //Configuration A:  XXXX-XXXX-XXXX-XXXX
            //Configuration B:  Bits 0-7 = X
            //Configuration C:  Bits 0-3 = X or Bits 4-7 = X
            //Configuration D:  Should Always Be X
            //Configuration E:  000H = X
            //Configuration F:  XXXX-XXXX Bits 5-0
            //Configuration G:  XXXX-XXXX-XXXX-XXXX--XXXX-XXXX Bits 2-0
            //Configuration Z:  Not Used
            "B", //00
            "B",
            "AA",
            "F",
            "F",
            "AA",
            "F",
            "AA",
            "F", //08
            "AA",
            "AA",
            "AA",
            "G",
            "B",
            "AA",
            "AA",
            "AA", //10
            "ZZ",
            "ZZ",
            "ZZ",
            "ZZ",
            "ZZ",
            "ZZ",
            "ZZ",
            "ZZ", //18
            "ZZ",
            "ZZ",
            "ZZ",
            "ZZ",
        };
    }
}
