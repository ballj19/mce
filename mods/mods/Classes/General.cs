﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mods
{
    class General
    {
        public static string Get_File_From_Path(string path)
        {
            string deconstructedPath = path;
            while (deconstructedPath.Contains("\\"))
            {
                int slashIndex = deconstructedPath.IndexOf("\\");
                deconstructedPath = deconstructedPath.Substring(slashIndex + 1, deconstructedPath.Length - slashIndex - 1);
            }

            return deconstructedPath;
        }

        public static string Get_Folder_From_Path(string path)
        {
            string deconstructedPath = path;
            string folder = "";
            while (deconstructedPath.Contains("\\"))
            {
                int slashIndex = deconstructedPath.IndexOf("\\");
                folder += deconstructedPath.Substring(0, slashIndex) + "\\";
                deconstructedPath = deconstructedPath.Substring(slashIndex + 1, deconstructedPath.Length - slashIndex - 1);
            }

            return folder;
        }

        public static string Get_FileExtension_From_Path(string path)
        {
            string file = Get_File_From_Path(path);

            int dotIndex = file.IndexOf(".");

            if(dotIndex == -1)
            {
                return "";
            }
            else
            {
                return file.Substring(dotIndex, file.Length - dotIndex);
            }
        }

        public static string Get_Job_Number_From_Path(string path, bool fullJobNumber = false)
        {
            string file = Get_File_From_Path(path);

            int c = 0;
            string jobNumber = "";
            string onlyLettersJobNumber = "";

            if(fullJobNumber)
            {
                while (c < file.Length && file[c] != '.')
                {
                    jobNumber += file[c];
                    c++;
                }
            }
            else
            {
                while (char.IsLetter(file[c]))
                {
                    onlyLettersJobNumber += file[c];
                    c++;
                }
                while (c < file.Length && char.IsNumber(file[c]))
                {
                    jobNumber += file[c];
                    c++;
                }
            }

            if(jobNumber.Length < 1)
            {
                return onlyLettersJobNumber.ToUpper();
            }
            else
            {
                return jobNumber;
            }
        }

        public static bool Is_Hex(string text)
        {
            if (text.IndexOf("H") == -1)
            {
                return false;
            }
            return true;
        }

        public static string Hex_To_Bin(string hex)
        {
            if(!Is_Hex(hex))
            {
                hex = Dec_To_Hex(hex);
            }

            string strippedHex = Remove_Suffix(Remove_Prefix(hex, "DB"), "H").Trim();
            strippedHex = strippedHex.Substring(strippedHex.Length - 2, 2);
            return HexStringToBinary(strippedHex);
        }

        public static string Dec_To_Hex(string deci, int padding = 3)
        {
            string strippedDeci = Remove_Prefix(deci, "DB");
            int dec = Int32.Parse(strippedDeci);
            return dec.ToString("X").PadLeft(padding, '0');
        }

        public static int HexStringToDecimal(string hex)
        {
            int dec_value = 0;
            try
            {
                hex = Remove_Suffix(hex, "H").Trim();

                //Need to reverse the hex string for the math to work out better
                char[] charArray = hex.ToCharArray();
                Array.Reverse(charArray);
                hex = new string(charArray);

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
            }
            catch
            {
                dec_value = 0;
            }
            
            return dec_value;
        }

        public static string HexStringToBinary(string hex)
        {
            hex = Remove_Suffix(hex,"H");
            StringBuilder result = new StringBuilder();
            foreach (char c in hex)
            {
                // This will crash for non-hex characters. You might want to handle that differently.
                result.Append(hexCharacterToBinary[char.ToLower(c)]);
            }
            return result.ToString();
        }

        public static readonly Dictionary<char, int> hexCharacterToDecimal = new Dictionary<char, int> {
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

        public static readonly Dictionary<char, string> hexCharacterToBinary = new Dictionary<char, string> {
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

        public static readonly Dictionary<string, char> binaryToHexCharacter = new Dictionary<string, char> {
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

        public static string Remove_Prefix(string text, string prefix)
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

        public static string Remove_Suffix(string text, string suffix)
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

        public static string Value(string line)
        {
            if (line.IndexOf(";") == -1) //indexOf returns -1 if string not found
            {
                return line.Trim();
            }
            else
            {
                int commentIndex = line.IndexOf(";");
                return line.Substring(0, commentIndex).Trim();
            }
        }

        public static string Comment(string line)
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

        public static List<string> Get_Clean_Lines_From_Path(string path)
        {
            List<string> lines = new List<string>();
            List<string> rawLines = System.IO.File.ReadAllLines(@path).ToList();

            foreach (string line in rawLines)
            {
                if (line == "")
                {
                    lines.Add("");
                }
                else
                {
                    //Need this logic because sometimes the first byte is defined on the same line as the label - inconsistently
                    //So we force the defined byte onto the next line always
                    if (General.Value(line).Contains(":") && (!General.Value(line).EndsWith(":") || !General.Comment(line).Equals("")))
                    {
                        int colonIndex = line.IndexOf(":");
                        lines.Add(line.Substring(0, colonIndex + 1).Trim());
                        lines.Add("\t" + line.Substring(colonIndex + 1, line.Length - colonIndex - 1).Trim());
                    }
                    else
                    {
                        if (line.Trim().EndsWith(":"))
                        {
                            lines.Add(line.Trim());
                        }
                        else if (line.Trim() == "END")
                        {
                            lines.Add(line.Trim());
                        }
                        else
                        {
                            lines.Add(line);
                        }
                    }
                }
            }
            return lines;
        }

        public static List<string> Get_Bytes_List(string label, List<string> content)
        {
            List<string> bytes = new List<string>();
            
            int index = content.FindIndex(x => x.StartsWith(label));

            if (index != -1)
            {
                bytes.Add(label);

                int offset = 1;

                while (content[index + offset].StartsWith("DB"))
                {
                    string value = Remove_Prefix(content[index + offset], "DB").Trim();

                    while (value.IndexOf(',') != -1)
                    {
                        int commaIndex = value.IndexOf(',');
                        string commaValue = value.Substring(0, commaIndex);

                        if (!Is_Hex(commaValue))
                        {
                            commaValue = Dec_To_Hex(commaValue);
                        }

                        bytes.Add(commaValue);
                        value = value.Substring(commaIndex + 1, value.Length - commaIndex - 1);
                    }

                    if (!Is_Hex(value))
                    {
                        value = Dec_To_Hex(value);
                    }

                    bytes.Add(value);

                    offset++;
                }
            }
            return bytes;
        }

        public static string BinaryStringToHex(string binary)
        {
            string hex = "";

            for(int h = 0; h < binary.Length / 4; h++)
            {
                hex += binaryToHexCharacter[binary.Substring(h * 4, 4)];
            }

            return hex.ToUpper();
        }
    }
}
