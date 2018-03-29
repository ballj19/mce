using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mods
{
    public class Content
    {
        public List<string> content = new List<string>();
        string[,] imap, omap;
        public string[,] inputs, outputs;
        List<string> inputLabels = new List<string> { "IOINPE", "IOXINE", "IOIA", "IOELIG" };
        List<string> outputLabels = new List<string> { "IOOUTE", "IOXOUTE", "IOOA" };
        List<string> filepaths = new List<string> { "G:\\Software\\" };
        public string file;


        public Content(string file)
        {
            this.content = Get_Content(file);
            this.imap = Build_IOmap(file, 'I');
            this.omap = Build_IOmap(file, 'O');
            this.inputs = IO(this.imap, 'I');
            this.outputs = IO(this.omap, 'O');
            this.file = file;
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
                        content.Add(line);
                    }
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
            return dec.ToString("X").PadLeft(3, '0');
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

        private List<string> Get_Bytes_List(string label)
        {
            List<string> bytes = new List<string>();

            bytes.Add(label);

            int index = this.content.FindIndex(x => x.StartsWith(label));
            int offset = 1;

            while(content[index + offset].StartsWith("DB"))
            {
                string value = Remove_Prefix(this.content[index + offset], "DB").Trim();
                
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
            return bytes;
        }

        public string Get_Bit(string label, int offset, int nibble, int bit)
        {
            try
            {
                List<string> bytes = Get_Bytes_List(label);

                string value = bytes[offset];

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
            catch
            {
                return "N/A";
            }
            
        }

        public string Get_Nibble(string label, int offset, int nibble)
        {
            try
            {
                List<string> bytes = Get_Bytes_List(label);

                string value = bytes[offset];

                string cleanString = Remove_Suffix(value, "H").Trim();

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
                List<string> bytes = Get_Bytes_List(label);

                string value = bytes[offset];

                string cleanString = Remove_Suffix(value, "H").Trim();

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
                string cleanString = Remove_Suffix(Remove_Prefix(this.content[index], "DB"), "H").Trim();

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

                string value = Remove_Prefix(this.content[index], "DB").Trim();

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

                if (!Is_Hex(value))
                {
                    value = Dec_To_Hex(value);
                }

                string cleanString = Remove_Suffix(value, "H").Trim();

                return cleanString.Substring(cleanString.Length - 2, 2);
            }
            catch
            {
                return "N/A";
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

        public int HexStringToDecimal(string hex)
        {
            try
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
            catch
            {
                return 0;
            }
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
                                    if (Char.IsLetterOrDigit(comment_string[c]) || comment_string[c] == '/' || comment_string[c] == '_' || comment_string[c] == '(' || comment_string[c] == ')')
                                    {
                                        ioCode.Append(comment_string[c]);
                                    }
                                    else
                                    {
                                        iomap[io_x, io_y] = ioCode.ToString();
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
                            iomap[io_x, io_y] = ioCode.ToString();
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

            string[] labelNumbers = { "", "2", "3", "4" };
            

            foreach (string ioLabel in ioLabels)
            {
                int labelNumberint = -1;
                foreach (string labelNumber in labelNumbers)
                {
                    labelNumberint++;
                    if (this.content.FindIndex(i => i.StartsWith(ioLabel + labelNumber + ":")) != -1)
                    {
                        int io_index = this.content.FindIndex(i => i.StartsWith(ioLabel + labelNumber + ":"));
                        int x = 0;
                        while (this.content[io_index + x + 1].Trim().StartsWith("DB"))
                        {
                            int iomap_index = io_index + x + 1;
                            string iomap_binary = Hex_To_Bin(this.content[iomap_index]);

                            for (int y = 0; y < 8; y++)
                            {
                                if (io == 'I')
                                {
                                    if (iomap_binary[7 - y] == '1')
                                    {
                                        ios[io_x, io_y] = iomap[labelNumberint * 8 + x, 7 - y];
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
                                        ios[io_x, io_y] = iomap[labelNumberint * 8 + x, y];
                                        io_y--;

                                        if (io_y == -1)
                                        {
                                            io_x++;
                                            io_y = 7;
                                        }
                                    }
                                }
                            }
                            x++;
                        }
                    }
                }
            }

            return ios;
        }

        public int Get_Group_Top_Level()
        {
            string[] cars = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L" };

            int number_of_cars = this.Get_Group_Num_Of_Cars();

            List<int> top_landings = new List<int>();

            for(int c = 0; c < number_of_cars; c++)
            {
                int eligiv_index = this.content.FindIndex(x => x.StartsWith("ELIGIV" + cars[c]));
                int top_landing = 0;
                int l = 1;

                while(content[eligiv_index + l].StartsWith("DB"))
                {
                    if(this.Get_Byte("ELIGIV" + cars[c],l) != "00")
                    {
                        top_landing = l;
                    }

                    l++;
                }

                top_landings.Add(top_landing);
            }

            return top_landings.Max(); 
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

        private void Build_LobbyMap(string file)
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

            if (lines.FindIndex(x => x.StartsWith("LOBBY:")) != -1)
            {
                int lobbyIndex = lines.FindIndex(x => x.StartsWith("LOBBY:"));

                //Configuration A:  XXXX-XXXX-XXXX-XXXX
                //Configuration B:  Bits 0-7 = X
                //Configuration C:  Bits 0-3 = X or Bits 4-7 = X
                //Configuration D:  Should Always Be X
                //Configuration E:  000H = X
                //Configuration F:  XXXX-XXXX-XXXX Bits 5-0
                //Configuration Z:  Not Used
            }
        }

        private List<string> NC_Input_Map(string file)
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

            List<string> ncinputs = new List<string>();

            if (lines.FindIndex(x => x.StartsWith("NIOINS:")) != -1)
            {
                int io_index = lines.FindIndex(i => i.StartsWith("NIOINS:"));
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
                                ncinputs.Add(ioCode.ToString());
                                ioCode.Clear();
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
                    ncinputs.Add(ioCode.ToString());
                    ioCode.Clear();
                    building = false;
                    x++;
                    iomap_index = io_index + x + 1;
                }
            }

            return ncinputs;
        }

        private List<string> NC_Output_Map(string file)
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

            List<string> ncoutputs = new List<string>();

            if (lines.FindIndex(x => x.StartsWith("NIOOUTS:")) != -1)
            {
                int io_index = lines.FindIndex(i => i.StartsWith("NIOOUTS:"));
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
                                ncoutputs.Add(ioCode.ToString());
                                ioCode.Clear();
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
                    ncoutputs.Add(ioCode.ToString());
                    ioCode.Clear();
                    building = false;
                    x++;
                    iomap_index = io_index + x + 1;
                }
            }

            return ncoutputs;
        }
        
        public List<string> NC_Inputs(string file)
        {
            List<string> ncinputs = new List<string>();
            List<string> ncinputsmap = NC_Input_Map(file);

            if (this.content.FindIndex(x => x.StartsWith("NIOINS:")) != -1)
            {
                int io_index = this.content.FindIndex(x => x.StartsWith("NIOINS:"));
                for (int x = 0; x < ncinputsmap.Count/8; x++)
                {
                    int iomap_index = io_index + x + 1;
                    string iomap_binary = Hex_To_Bin(this.content[iomap_index]);

                    for (int y = 0; y < 8; y++)
                    {
                        if (iomap_binary[7 - y] == '1')
                        {
                            ncinputs.Add(ncinputsmap[x * 8 + 7 - y]);
                        }
                    }
                }
            }

            return ncinputs;
        }

        public List<string> NC_Outputs(string file)
        {
            List<string> ncoutputs = new List<string>();
            List<string> ncoutputsmap = NC_Output_Map(file);

            if (this.content.FindIndex(x => x.StartsWith("NIOOUTS:")) != -1)
            {
                int io_index = this.content.FindIndex(x => x.StartsWith("NIOOUTS:"));
                for (int x = 0; x < ncoutputsmap.Count / 8; x++)
                {
                    int iomap_index = io_index + x + 1;
                    string iomap_binary = Hex_To_Bin(this.content[iomap_index]);

                    for (int y = 0; y < 8; y++)
                    {
                        if (iomap_binary[7 - y] == '1')
                        {
                            ncoutputs.Add(ncoutputsmap[x * 8 + 7 - y]);
                        }
                    }
                }
            }

            return ncoutputs;
        }

        private List<string> INELIG_Input_Map(string file)
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

            List<string> inputs = new List<string>();

            if (lines.FindIndex(x => x.StartsWith("INELIG:")) != -1)
            {
                int io_index = lines.FindIndex(i => i.StartsWith("INELIG:"));
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
                                inputs.Add(ioCode.ToString());
                                ioCode.Clear();
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
                    inputs.Add(ioCode.ToString());
                    ioCode.Clear();
                    building = false;
                    x++;
                    iomap_index = io_index + x + 1;
                }
            }

            return inputs;
        }

        public List<string> INELIG_Inputs(string file)
        {
            List<string> inputs = new List<string>();
            List<string> inputsmap = INELIG_Input_Map(file);

            if (this.content.FindIndex(x => x.StartsWith("INELIG:")) != -1)
            {
                int io_index = this.content.FindIndex(x => x.StartsWith("INELIG:"));
                for (int x = 0; x < inputsmap.Count / 8; x++)
                {
                    int iomap_index = io_index + x + 1;
                    string iomap_binary = Hex_To_Bin(this.content[iomap_index]);

                    for (int y = 0; y < 8; y++)
                    {
                        if (iomap_binary[7 - y] == '1')
                        {
                            inputs.Add(inputsmap[x * 8 + 7 - y]);
                        }
                    }
                }
            }

            return inputs;
        }

        public List<string> Get_PILabels()
        {
            int index = this.content.FindIndex(x => x.StartsWith("PILAB"));
            string pilabelstring = "";
            List<string> piLabels = new List<string>();
            int c = 1;

            while(this.content[index + c].StartsWith("DB"))
            {
                pilabelstring += this.Get_String("PILAB", c);
                c++;
            }

            for (int i = 0; i < pilabelstring.Length / 2; i++)
            {
                string pilabel = pilabelstring.Substring(i * 2, 2);
                piLabels.Add(pilabel);
            }

            return piLabels;
        }

        private static readonly Dictionary<string, string> LobbyConfig = new Dictionary<string, string> {
            {"00","BB" },
            {"01","AC" },
            {"02","AC" },
            {"03","DD" },
            {"04","CC" },
            {"05","DD" },
            {"06","AA" },
            {"07","AA" },
            {"08","AA" },
            {"09","AC" },
            {"0A","AA" },
            {"0B","AA" },
            {"0C","AA" },
            {"0D","AA" },
            {"0E","AA" },
            {"0F","AC" },
            {"10","AA" },
            {"11","AA" },
            {"12","AA" },
            {"13","AA" },
            {"14","AA" },
            {"15","AA" },
            {"16","AA" },
            {"17","AA" },
            {"18","AA" },
            {"19","AA" },
            {"1A","AC" },
            {"1B","AA" },
            {"1C","AA" },
            {"1D","AA" },
            {"1E","AA" },
            {"1F","AC" },
            {"20","EE" },
            {"21","AA" },
            {"22","AA" },
            {"23","AA" },
            {"24","AA" },
            {"25","AA" },
            {"26","AA" },
            {"27","CC" },
            {"28","CC" },
            {"29","AC" },
            {"2A","FF" },
            {"2B","AC" },
            {"2C","DD" },
            {"2D","BB" },
            {"2E","BB" },
            {"2F","AA" },
            {"30","AC" },
            {"31","CC" },
            {"32","FF" },
            {"33","ZC" },
            {"34","BB" },
            {"35","ZC" },
            {"36","EE" },
            {"37","CC" },
            {"38","" },
            {"39","" },
            {"3A","" },
            {"3B","" },
            {"3C","" },
            {"3D","" },
        };
        
    }
}
