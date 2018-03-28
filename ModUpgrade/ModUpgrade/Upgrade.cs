using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Windows;

namespace ModUpgrade
{
    class Upgrade
    {
        string filename;
        string filepath;
        Content upgrade_content;
        Content original_content;
        List<string> new_lines = new List<string>();
        int tableCount = 0;

        public Upgrade(string filepath)
        {
            original_content = new Content(filepath);

            this.filepath = filepath;
            
            while(filepath.IndexOf("\\") != -1)
            {
                int slashindex = filepath.IndexOf('\\') + 1;
                int length = filepath.Length - slashindex;
                string tempfilepath = filepath.Substring(slashindex, length);
                filepath = tempfilepath;
            }
            int dotIndex = filepath.IndexOf(".");
            this.filename = filepath.Substring(0,dotIndex);
        }

        public void Write_Line(string text)
        {
            new_lines.Add(text);
        }

        private void Write_Intermediate(int startIndex, int endIndex)
        {
            for(int x = startIndex; x < endIndex; x ++)
            {
                Write_Line(original_content.lines[x]);
            }
        }

        public void Version_Upgrade(string sourcepath)
        {
            upgrade_content = new Content(sourcepath);

            List<string> combine_labels = new List<string>
            {   "IOINPE",
                "IOINPE2",
                "IOINPE3",
                "IOINPE4",
                "IOOUTE",
                "IOOUTE2",
                "BOTTOM",
                "LOBBY",
                "NIOINS",
                "NIOOUTS",
                "CCLOCKBYP",
                "INELIG",
                "CARDRR",
                "CARDRF",
                "CPVAR",
                "CPVAR2",
                "ELIGI",
                "XELIGI",
                "HELIGI",
                "CCLOCKM",
            };

            List<string> add_to_end_labels = new List<string>
            {
                "CVTROM",
                "L_TABLE",
                "ETMRA",
                "ESYSTM",
            };

            List<string> replace_labels = new List<string>
            {
                "MPVERNUM",
                "CUSTOM",
                "NUMOFTMRS",
                "NUMOFABTIM",
                "NUMOFSTIM",
            };

            List<string> load_labels = new List<string>
            {
                "DEF_LOAD",
                "TDEF_LOAD",
            };

            List<string> select_labels = new List<string>
            {
                "DEF_SELECT",
                "TDEF_SELECT",
            };

            Write_Intermediate(0, original_content.labelsInt[0]); //Writes the header at the beginning of file

            foreach (string label in upgrade_content.labels)
            {
                int labelIndex = upgrade_content.labels.IndexOf(label);

                if (!original_content.labels.Contains(label)) //If original_content did not contain the label, insert it
                {
                    if (labelIndex != 1) //The V013 Label messes things up because its always different, so skip it
                    {
                        original_content.labels.Insert(labelIndex, label);
                        original_content.labelsInt.Insert(labelIndex, 0); //Dont know what the int is
                        Insert_Upgraded_Lines(label + ":");
                    }
                    else
                    {
                        original_content.labels.RemoveAt(1);
                        original_content.labels.Insert(1, upgrade_content.labels[1]);
                        Replace_Label(label + ":");
                    }
                }
                else if (combine_labels.Contains(label))
                {
                    Combine_Label(label + ":");
                }
                else if (replace_labels.Contains(label))
                {
                    Replace_Label(label + ":");
                }
                else if (add_to_end_labels.Contains(label))
                {
                    Add_To_End_Label(label + ":");
                }
                else if (load_labels.Contains(label))
                {
                    Load_Label(label + ":");
                }
                else if (select_labels.Contains(label))
                {
                    Select_Label(label + ":");
                }
                else
                {
                    int labelNumber = upgrade_content.labels.IndexOf(label);
                    if (labelNumber + 1 == original_content.labels.Count)
                    {
                        int EOFindex = original_content.lines.IndexOf("END");
                        Write_Intermediate(original_content.labelsInt[labelNumber], EOFindex + 1);
                    }
                    else
                    {
                        Write_Intermediate(original_content.labelsInt[labelNumber], original_content.labelsInt[labelNumber + 1]);
                    }
                }
            }
        }

        public void No_Version_Upgrade()
        {
            new_lines = original_content.lines;
        }

        public string Write_File()
        {
            System.IO.File.WriteAllLines(@"C:\\ModUpgrades\\" + filename + ".ASM", new_lines.ToArray());

            return "C:\\ModUpgrades\\" + filename + ".ASM";
        }

        private void Combine_Label(string label)
        {
             Replace_Values(label);
        }

        private void Add_To_End_Label(string label)
        {
            int oCount = 0;
            int uCount = 0;
            tableCount = 0;

            int oLabelIndex = original_content.lines.IndexOf(label);
            int uLabelIndex = upgrade_content.lines.IndexOf(label);

            Write_Line(original_content.lines[oLabelIndex - 1]); //Write PUBLIC "LABEL"
            
            int o = oLabelIndex + 1;
            while (!Value(original_content.lines[o]).StartsWith("PUBLIC") && !Value(original_content.lines[o]).Contains("'{'"))
            {
                if (Value(original_content.lines[o]).StartsWith("DB"))
                {
                    tableCount++;
                }
                oCount++;
                o++;
            }

            int u = uLabelIndex + 1;
            while (!Value(upgrade_content.lines[u]).StartsWith("PUBLIC"))
            {
                uCount++;
                u++;
            }

            for (int x = 0; x <= uCount; x++)
            {
                if(x < oCount)
                {
                    Write_Line(original_content.lines[oLabelIndex + x]);
                }
                else
                {
                    Write_Line(upgrade_content.lines[uLabelIndex + x]);
                }                
            }
        }

        private void Replace_Label(string label)
        {
            int oLabelIndex = original_content.lines.IndexOf(label);
            int uLabelIndex = upgrade_content.lines.IndexOf(label);

            int uCount = 0;

            Write_Line(upgrade_content.lines[uLabelIndex - 1]); //Write PUBLIC "LABEL"

            int u = uLabelIndex;
            while (!Value(upgrade_content.lines[u]).StartsWith("PUBLIC"))
            {
                uCount++;
                u++;
            }

            for (int x = 0; x < uCount; x++)
            {
                Write_Line(upgrade_content.lines[uLabelIndex + x]);
            }
        }

        private void Load_Label(string label)
        {
            int uload_labelIndex = upgrade_content.lines.IndexOf(label);

            Write_Line(upgrade_content.lines[uload_labelIndex - 1]); //Write PUBLIC "LABEL"

            int u = 0;
            while (!Value(upgrade_content.lines[uload_labelIndex + u]).StartsWith("PUBLIC"))
            {
                if(u == 1)
                {
                    string value = "\tDB\t055H";
                    string comment = Comment(upgrade_content.lines[uload_labelIndex + u]);
                    Write_Line(value + "\t" + comment);
                }
                else
                {
                    Write_Line(upgrade_content.lines[uload_labelIndex + u]);
                }
                u++;
            }
        }

        private void Select_Label(string label)
        {
            int uSelect_labelIndex = upgrade_content.lines.IndexOf(label);

            Write_Line(upgrade_content.lines[uSelect_labelIndex - 1]); //Write PUBLIC "LABEL"
            Write_Line(upgrade_content.lines[uSelect_labelIndex]); //Write "LABEL:"

            int byteCount = tableCount;

            if(label == "L_TABLE:")
            {
                byteCount--; //We have to subtract 1 because of the End of Table Marker '{'
            }

            int b = 1;
            while(!Value(upgrade_content.lines[uSelect_labelIndex + b]).StartsWith("PUBLIC"))
            {
                if(Value(upgrade_content.lines[uSelect_labelIndex + b]).StartsWith("DB"))
                {
                    string comment = Comment(upgrade_content.lines[uSelect_labelIndex + b]);
                    string value = "";

                    if (byteCount >= 8)
                    {
                        value = "000H";
                        byteCount = byteCount - 8;
                    }
                    else if (byteCount == 7)
                    {
                        value = "080H";
                        byteCount = 0;
                    }
                    else if (byteCount == 6)
                    {
                        value = "0C0H";
                        byteCount = 0;
                    }
                    else if (byteCount == 5)
                    {
                        value = "0E0H";
                        byteCount = 0;
                    }
                    else if (byteCount == 4)
                    {
                        value = "0F0H";
                        byteCount = 0;
                    }
                    else if (byteCount == 3)
                    {
                        value = "0F8H";
                        byteCount = 0;
                    }
                    else if (byteCount == 2)
                    {
                        value = "0FCH";
                        byteCount = 0;
                    }
                    else if (byteCount == 1)
                    {
                        value = "0FEH";
                        byteCount = 0;
                    }
                    else
                    {
                        value = "0FFH";
                        byteCount = 0;
                    }
                    Write_Line("\tDB\t" + value + "\t" + comment);
                }
                else
                {
                    Write_Line(Comment(upgrade_content.lines[uSelect_labelIndex + b]));
                }
                b++;
            }
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

        private static readonly Dictionary<string, char> binaryToHexCharacter = new Dictionary<string, char> {
            { "0000", '0' },
            { "0001", '1' },
            { "0010", '2' },
            { "0011", '3' },
            { "0100", '4' },
            { "0101", '5' },
            { "0110", '6' },
            { "0111", '7' },
            { "1000", '8' },
            { "1001", '9' },
            { "1010", 'a' },
            { "1011", 'b' },
            { "1100", 'c' },
            { "1101", 'd' },
            { "1110", 'e' },
            { "1111", 'f' }
        };

        public int HexStringToDecimal(string hex)
        {         
            //Need to reverse the hex string for the math to work out better
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
        
        private void Replace_Comments(int oIndex, int uIndex, int length = 1)
        {
            //oIndex is the original index, uIndex is the upgrade index
            for (int x = 0; x < length; x++)
            {
                string value = Value(original_content.lines[x + oIndex]);
                string comment = Comment(upgrade_content.lines[x + uIndex]);

                Write_Line(value + comment);
            }
        }

        private void Insert_Upgraded_Lines(string label)
        {
            int uIndex = upgrade_content.lines.IndexOf(label);
            Write_Line(upgrade_content.lines[uIndex - 1]); //Write PUBLIC "LABEL"
            do
            {
                Write_Line(upgrade_content.lines[uIndex]);
                uIndex++;
            } while (!Value(upgrade_content.lines[uIndex]).StartsWith("PUBLIC"));
        }

        private void Replace_Values(string label)
        {
            List<string> oValues = new List<string>();
            List<string> uValues = new List<string>();
            List<int> oValuesIndex = new List<int>();
            List<int> uValuesIndex = new List<int>();

            int oLabelIndex = original_content.lines.IndexOf(label);
            int uLabelIndex = upgrade_content.lines.IndexOf(label);

            Write_Line(original_content.lines[oLabelIndex - 1]); //Write PUBLIC "LABEL"
            Write_Line(original_content.lines[oLabelIndex]); //Write "LABEL:"

            int oEndIndex;
            int uEndIndex;

            int o = oLabelIndex + 1;
            while (!Value(original_content.lines[o]).StartsWith("PUBLIC"))
            {
                if (Value(original_content.lines[o]).StartsWith("DB"))
                {
                    oValues.Add(Value(original_content.lines[o]));
                    oValuesIndex.Add(o);
                }
                o++;
            } 
            oEndIndex = o;

            int u = uLabelIndex + 1;
            while (!Value(upgrade_content.lines[u]).StartsWith("PUBLIC"))
            {
                if (Value(upgrade_content.lines[u]).StartsWith("DB"))
                {
                    uValues.Add(Value(upgrade_content.lines[u]));
                    uValuesIndex.Add(u);
                }
                u++;
            }
            uEndIndex = u;

            for(int x = uLabelIndex + 1; x < uEndIndex; x++)
            {
                if(uValuesIndex.Contains(x))
                {
                    int valueNumber = uValuesIndex.IndexOf(x);
                    string value = "";
                    if(valueNumber >= oValues.Count)
                    {
                        value = Value(uValues[valueNumber]); //This is for when there is a new option added in the upgraded version
                    }
                    else
                    {
                        value = Value(oValues[valueNumber]);
                    }
                    string comment = Comment(upgrade_content.lines[x]);
                    Write_Line("\t" + value + "\t" + comment);
                }
                else
                {
                    Write_Line(upgrade_content.lines[x]);
                }
            }
        }

        private string Value(string line)
        {
            if (line.IndexOf(";") == -1) //indexOf returns -1 if string not found
            {
                return line;
            }
            else
            {
                int commentIndex = line.IndexOf(";");
                return line.Substring(0, commentIndex).Trim();
            }
        }

        private string Comment(string line)
        {
            if (line.IndexOf(";") == -1) //indexOf returns -1 if string not found
            {
                return "";
            }
            else
            {
                int commentIndex = line.IndexOf(";");
                return line.Substring(commentIndex, line.Length - commentIndex).Trim();
            }
        }

        public void Archive()
        {
            string jobNumber = filename + ".*";
            string folder = filepath.Substring(0, filepath.IndexOf(filename));
            List<string> filesAll = Directory.GetFiles(@folder, jobNumber, SearchOption.AllDirectories).ToList();

            bool asmFileExists = false;
            string asmFile = "";

            foreach(string file in filesAll)
            {
                int dotIndex = file.IndexOf(".");
                string lowercasefile = file.ToLower();
                if (lowercasefile.Substring(dotIndex, file.Length - dotIndex).Contains("asm"))
                {
                    asmFileExists = true;
                    asmFile = file;
                }
            }

            if(!asmFileExists)
            {
                MessageBox.Show(".ASM file not found");
                return;
            }

            List<string> files = new List<string>();

            foreach(string file in filesAll)
            {
                int dotIndex = file.IndexOf(".");
                string lowercasefile = file.ToLower();
                if(lowercasefile.Substring(dotIndex,file.Length - dotIndex).Contains("ol"))
                {
                    files.Add(file);
                }
            }

            try
            {
                if(files.Count > 0)
                {
                    int dotIndex = asmFile.IndexOf(".");
                    string filepath = asmFile.Substring(0, dotIndex);
                    string extension = ".OLD" + (files.Count + 1);
                    File.Move(asmFile, filepath + extension);
                    File.Copy(filepath + extension, filepath + ".ASM");
                }

                else
                {
                    int dotIndex = asmFile.IndexOf(".");
                    string filepath = asmFile.Substring(0, dotIndex);
                    string extension = ".OLD";
                    File.Move(asmFile, filepath + extension);
                    File.Copy(filepath + extension, filepath + ".ASM");
                }
            }
            catch
            {
                
            }
        }

        public void Open_Files(string originalFile, string newFile)
        {
            string cmd = "C:\\Windows\\explorer.exe";
            Process.Start(cmd, originalFile);
            Process.Start(cmd, newFile);
        }

        public void Modify_Value(string label, string db, string operation, string operand)
        {
            int labelIndex = new_lines.IndexOf(label);

            int byteNum = HexStringToDecimal(db);

            int b = -1;
            while(b != byteNum)
            {
                labelIndex++;
                if (Value(new_lines[labelIndex]).StartsWith("DB"))
                {
                    b++;
                }
            }

            string sourceValue = Value(new_lines[labelIndex]);
            string sourceComment = Comment(new_lines[labelIndex]);

            string modifiedValue = "";

            if (operation == "OR")
            {
                modifiedValue = OR_Byte(sourceValue, operand);
            }
            else if(operation == "AND")
            {
                modifiedValue = AND_Byte(sourceValue, operand);
            }
            else if(operation == "REPLACE")
            {
                modifiedValue = operand;
            }
            new_lines[labelIndex] = "\tDB\t" + modifiedValue + "\t" + sourceComment; 
        }

        private string OR_Byte(string source, string operand)
        {
            source = Remove_Prefix(source, "DB").Trim();
            source = Remove_Suffix(source, "H").Trim();
            operand = Remove_Suffix(operand,"H").Trim();

            //Make sure source is only 1 byte
            source = source.Substring(source.Length - 2, 2);

            string sourceBinary = "";
            source = source.ToLower();
            operand = operand.ToLower();

            foreach(char nibble in source)
            {
                sourceBinary += hexCharacterToBinary[nibble];
            }

            string operandBinary = "";

            foreach (char nibble in operand)
            {
                operandBinary += hexCharacterToBinary[nibble];
            }

            string resultBinary = "";

            for(int b = 0; b < 8; b++)
            {
                if(sourceBinary[b] == '1' || operandBinary[b] == '1')
                {
                    resultBinary += '1';
                }
                else
                {
                    resultBinary += '0';
                }
            }

            char upperNibble = binaryToHexCharacter[resultBinary.Substring(0, 4)];
            char lowerNibble = binaryToHexCharacter[resultBinary.Substring(4, 4)];

            return ("0" + upperNibble + lowerNibble + "H").ToUpper();
        }

        private string AND_Byte(string source, string operand)
        {
            source = Remove_Prefix(source, "DB").Trim();
            source = Remove_Suffix(source, "H").Trim();
            operand = Remove_Suffix(operand, "H").Trim();

            //Make sure source is only 1 byte
            source = source.Substring(source.Length - 2, 2);

            string sourceBinary = "";
            source = source.ToLower();
            operand = operand.ToLower();

            foreach (char nibble in source)
            {
                sourceBinary += hexCharacterToBinary[nibble];
            }

            string operandBinary = "";

            foreach (char nibble in operand)
            {
                operandBinary += hexCharacterToBinary[nibble];
            }

            string resultBinary = "";

            for (int b = 0; b < 8; b++)
            {
                if (sourceBinary[b] == '1' && operandBinary[b] == '1')
                {
                    resultBinary += '1';
                }
                else
                {
                    resultBinary += '0';
                }
            }

            char upperNibble = binaryToHexCharacter[resultBinary.Substring(0, 4)];
            char lowerNibble = binaryToHexCharacter[resultBinary.Substring(4, 4)];

            return ("0" + upperNibble + lowerNibble + "H").ToUpper();
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
    }
}
