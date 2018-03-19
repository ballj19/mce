using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace mods
{
    class Simulator
    {
        List<string> sim_base;
        List<string> sim_content = new List<string>();
        string filename;
        string filepath;
        int index;
        Content content;

        public Simulator(string filepath)
        {
            this.index = 0;

            string[] lines = System.IO.File.ReadAllLines(@"C:\\Simulator\\default.sdf");
            this.sim_base = lines.ToList();

            this.filepath = filepath;

            content = new Content(filepath);
            
            while(filepath.IndexOf("\\") != -1)
            {
                int slashindex = filepath.IndexOf('\\') + 1;
                int length = filepath.Length - slashindex;
                string tempfilepath = filepath.Substring(slashindex, length);
                filepath = tempfilepath;
            }
            this.filename = filepath.Substring(0,filepath.Length-4);
        }

        public void Write_Intermediate(int end_index)
        {
            for(int x = index; x < end_index; x++)
            {
                Write_Line(sim_base[this.index]);
            }
        }

        public void Write_Line(string text)
        {
            sim_content.Add(text);
            this.index++;
        }

        public string Write_File()
        {
            int number_of_landings_index = 661;
            int landing_config_index = 664;
            int ccelig_index = 858;
            int fhcelig_index = 988;
            int rhcelig_index = 1118;
            int hospelig_index = 1248;
            int iox_index = 1555;
            int i4o_index = 1558;
            int aiox_index = 1561;
            int ce_index = 1564;
            int flex_index = 1567;
            int dlm_index = 1591;
            int spare1_index = 7878;
            int eof_index = 8753;

            string jobName = content.Get_String("JBNAME:", 1);
            string topFloor = content.Get_Byte("BOTTOM:", 2);
            string botFloor = content.Get_Byte("BOTTOM:", 1);
            string i4o = content.Get_Nibble("LOBBY:", 40, 1);
            string iox = content.Get_Nibble("LOBBY:", 40, 0);
            string aiox = content.Get_Nibble("LOBBY:", 52, 0);
            string rearDoor = content.Get_Bit("LOBBY:", 12, 0, 3);
            string ceBoard = content.Get_Bit("BOTTOM:", 6, 1, 1);
            string ncBoard = content.Get_Bit("LOBBY:", 33, 1, 2);
            string ftBoard = content.Get_Bit("BOTTOM:", 6, 1, 3);
            string dlmBoard = content.Get_Bit("LOBBY:", 39, 0, 2);
            string[,] inputs = content.inputs;

            Write_Intermediate(number_of_landings_index);
            Write_Line("Value = " + (HexStringToDecimal(topFloor)+1));
            Write_Intermediate(landing_config_index);
            Write_Landing_Config();
            Write_Intermediate(ccelig_index);
            Write_CC_Elig();
            Write_Intermediate(fhcelig_index);
            Write_Front_Hall();
            Write_Intermediate(rhcelig_index);
            Write_Rear_Hall();
            Write_Intermediate(hospelig_index);
            Write_Hosp_Elig();
            Write_Intermediate(iox_index);
            Write_Line("Value = " + iox);
            Write_Intermediate(i4o_index);
            Write_Line("Value = " + i4o);
            Write_Intermediate(aiox_index);
            Write_Line("Value = " + aiox);
            Write_Intermediate(ce_index);
            Write_CE_Board();
            Write_Intermediate(flex_index);
            Write_FLEX_Board();
            Write_Intermediate(dlm_index);
            Write_DLM_Board();
            Write_Intermediate(spare1_index);
            Write_Inputs(inputs);
            Write_Intermediate(eof_index);


            System.IO.File.WriteAllLines(@"C:\\Simulator\\" + filename + ".sdf", sim_content.ToArray());

            return "C:\\Simulator\\" + filename + ".sdf";
        }

        public void Open_File()
        {
            Process.Start("C:\\Program Files\\SoftSim\\HoistwaySim.exe", "C:\\Simulator\\" + this.filename + ".sdf");
        }

        private void Write_Landing_Config()
        {
            int top_landing = HexStringToDecimal(content.Get_Byte("BOTTOM:", 2)) + 1;
            string front = "False";
            string rear = "False";
            string isFalseFloor = content.Get_Bit("CPVAR", 3, 0, 3);

            if(isFalseFloor == "NO")
            {
                for (int x = 1; x <= top_landing; x++)
                {
                    if (content.Get_Bit("ELIGIV:", x, 0, 3) == "YES" || content.Get_Bit("ALTMP:", x, 0, 3) == "YES")
                    {
                        front = "True";
                    }
                    else
                    {
                        front = "False";
                    }

                    if (content.Get_Bit("ELIGIV:", x, 0, 2) == "YES" || content.Get_Bit("ALTMP:", x, 0, 2) == "YES")
                    {
                        rear = "True";
                    }
                    else
                    {
                        rear = "False";
                    }

                    Write_Line("Value Height " + x + " = 10");
                    Write_Line("Value " + x + " F = " + front);
                    Write_Line("Value " + x + " R = " + rear);
                }
            }
            else
            {
                int pix_tableIndex = content.content.IndexOf("PIX_TABLE:");
                int x = 1;
                List<int> falseFloors = new List<int>();
                List<int> nonFalseFloors = new List<int>();
                while(content.content[pix_tableIndex + x].StartsWith("DB") && content.Get_Byte("PIX_TABLE:", x) != "7F")
                {
                    string floorHex = content.Get_Byte("PIX_TABLE:", x);
                    string floorBinary = content.HexStringToBinary(floorHex);
                    int floorDec = content.HexStringToDecimal(floorHex) + 1;
                    if(floorBinary[0] == '0') //If False Floor
                    {
                        falseFloors.Add(floorDec);
                    }
                    else //Non False Floor
                    {
                        nonFalseFloors.Add(floorDec - 128);
                    }
                    x++;
                }

                for (int f = 1; f <= top_landing; f++)
                {
                    if(nonFalseFloors.Contains(f))
                    {
                        if (content.Get_Bit("ELIGIV:", f, 0, 3) == "YES" || content.Get_Bit("ALTMP:", f, 0, 3) == "YES")
                        {
                            front = "True";
                        }
                        else
                        {
                            front = "False";
                        }

                        if (content.Get_Bit("ELIGIV:", f, 0, 2) == "YES" || content.Get_Bit("ALTMP:", f, 0, 2) == "YES")
                        {
                            rear = "True";
                        }
                        else
                        {
                            rear = "False";
                        }
                    }
                    else if(falseFloors.Contains(f))
                    {
                        int falseFloorIndex = falseFloors.IndexOf(f);
                        int falseFloorNum = f;

                        while(falseFloorIndex < falseFloors.Count - 1 && falseFloors[falseFloorIndex + 1] == falseFloorNum)
                        {
                            front = "True";
                            rear = "False";

                            Write_Line("Value Height " + f + " = 10");
                            Write_Line("Value " + f + " F = " + front);
                            Write_Line("Value " + f + " R = " + rear);

                            f++;
                            falseFloorIndex++;
                        }

                        front = "True";
                        rear = "False";

                    }
                    else
                    {
                        front = "False";
                        rear = "False";
                    }

                    Write_Line("Value Height " + f + " = 10");
                    Write_Line("Value " + f + " F = " + front);
                    Write_Line("Value " + f + " R = " + rear);
                }
            }
        }

        private void Write_CC_Elig()
        {
            string front = "False";
            string rear = "False";
            int floor;

            for (int x = 0; x < 8; x++)
            {
                for (int b = 3; b >= 0; b--)
                {
                    if (content.Get_Bit("ELIGI:", x + 1, 0, b) == "YES")
                    {
                        front = "True";
                    }
                    else
                    {
                        front = "False";
                    }

                    if (content.Get_Bit("ELIGI:", x + 9, 0, b) == "YES")
                    {
                        rear = "True";
                    }
                    else
                    {
                        rear = "False";
                    }

                    floor = x * 8 + (3 - b) + 1;

                    Write_Line("Value " + floor + " F = " + front);
                    Write_Line("Value " + floor + " R = " + rear);
                }

                for (int b = 3; b >= 0; b--)
                {
                    if (content.Get_Bit("ELIGI:", x + 1, 1, b) == "YES")
                    {
                        front = "True";
                    }
                    else
                    {
                        front = "False";
                    }

                    if (content.Get_Bit("ELIGI:", x + 9, 1, b) == "YES")
                    {
                        rear = "True";
                    }
                    else
                    {
                        rear = "False";
                    }

                    floor = x * 8 + (3 - b) + 5;

                    Write_Line("Value " + floor + " F = " + front);
                    Write_Line("Value " + floor + " R = " + rear);
                }
            }
        }

        private void Write_Inputs(string[,] inputs)
        {
            int spare_number = 1;

            for(int x = 0; x < 8; x++)
            {
                for(int y = 0; y < 8; y++)
                {
                    if(inputs[x, 7 - y] == null)
                    {
                        return;
                    }
                    else
                    {
                        Write_Line("[SpareSwComboBox" + spare_number + "]");
                        Write_Line("Value = " + inputs[x, 7 - y]);
                        Write_Line("");
                        spare_number++;
                    }
                }
            }
        }

        private void Write_Front_Hall()
        {
            string up = "False";
            string down = "False";
            int floor = 0;

            for (int x = 0; x < 8; x++)
            {
                for (int b = 3; b >= 0; b--)
                {
                    if (content.Get_Bit("ELIGI:", x + 33, 0, b) == "YES")
                    {
                        up = "True";
                    }
                    else
                    {
                        up = "False";
                    }

                    if (content.Get_Bit("ELIGI:", x + 17, 0, b) == "YES")
                    {
                        down = "True";
                    }
                    else
                    {
                        down = "False";
                    }

                    floor = x * 8 + (3 - b) + 1;

                    Write_Line("Value " + floor + " U = " + up);
                    Write_Line("Value " + floor + " D = " + down);
                }

                for (int b = 3; b >= 0; b--)
                {
                    if (content.Get_Bit("ELIGI:", x + 33, 1, b) == "YES")
                    {
                        up = "True";
                    }
                    else
                    {
                        up = "False";
                    }

                    if (content.Get_Bit("ELIGI:", x + 17, 1, b) == "YES")
                    {
                        down = "True";
                    }
                    else
                    {
                        down = "False";
                    }

                    floor = x * 8 + (3 - b) + 5;

                    Write_Line("Value " + floor + " U = " + up);
                    Write_Line("Value " + floor + " D = " + down);
                }
            }
        }

        private void Write_Rear_Hall()
        {
            string up = "False";
            string down = "False";
            int floor = 0;

            for (int x = 0; x < 8; x++)
            {
                for (int b = 3; b >= 0; b--)
                {
                    if (content.Get_Bit("ELIGI:", x + 41, 0, b) == "YES")
                    {
                        up = "True";
                    }
                    else
                    {
                        up = "False";
                    }

                    if (content.Get_Bit("ELIGI:", x + 25, 0, b) == "YES")
                    {
                        down = "True";
                    }
                    else
                    {
                        down = "False";
                    }

                    floor = x * 8 + (3 - b) + 1;

                    Write_Line("Value " + floor + " U = " + up);
                    Write_Line("Value " + floor + " D = " + down);
                }

                for (int b = 3; b >= 0; b--)
                {
                    if (content.Get_Bit("ELIGI:", x + 41, 1, b) == "YES")
                    {
                        up = "True";
                    }
                    else
                    {
                        up = "False";
                    }

                    if (content.Get_Bit("ELIGI:", x + 25, 1, b) == "YES")
                    {
                        down = "True";
                    }
                    else
                    {
                        down = "False";
                    }

                    floor = x * 8 + (3 - b) + 5;

                    Write_Line("Value " + floor + " U = " + up);
                    Write_Line("Value " + floor + " D = " + down);
                }
            }
        }

        private void Write_Hosp_Elig()
        {
            string front = "False";
            string rear = "False";
            int floor;

            for (int x = 0; x < 8; x++)
            {
                for (int b = 3; b >= 0; b--)
                {
                    if (content.Get_Bit("HELIGI:", x + 1, 0, b) == "YES" || content.Get_Bit("CARDRF:", x + 1, 0, b) ==  "YES")
                    {
                        front = "True";
                    }
                    else
                    {
                        front = "False";
                    }

                    if (content.Get_Bit("HELIGI:", x + 9, 0, b) == "YES" || content.Get_Bit("CARDRR:", x + 1, 0, b) == "YES")
                    {
                        rear = "True";
                    }
                    else
                    {
                        rear = "False";
                    }

                    floor = x * 8 + (3 - b) + 1;

                    Write_Line("Value " + floor + " F = " + front);
                    Write_Line("Value " + floor + " R = " + rear);
                }

                for (int b = 3; b >= 0; b--)
                {
                    if (content.Get_Bit("HELIGI:", x + 1, 1, b) == "YES" || content.Get_Bit("CARDRF:", x + 1, 1, b) == "YES")
                    {
                        front = "True";
                    }
                    else
                    {
                        front = "False";
                    }

                    if (content.Get_Bit("HELIGI:", x + 9, 1, b) == "YES" || content.Get_Bit("CARDRR:", x + 1, 1, b) == "YES")
                    {
                        rear = "True";
                    }
                    else
                    {
                        rear = "False";
                    }

                    floor = x * 8 + (3 - b) + 5;

                    Write_Line("Value " + floor + " F = " + front);
                    Write_Line("Value " + floor + " R = " + rear);
                }
            }
        }

        private void Write_CE_Board()
        {
            if(content.Get_Bit("BOTTOM:", 6, 1, 1) == "YES")
            {
                Write_Line("Value = Yes");
            }
            else
            {
                Write_Line("Value = No");
            }
        }

        private void Write_FLEX_Board()
        {
            if (content.Get_Bit("BOTTOM:", 6, 1, 3) == "YES")
            {
                Write_Line("Value = Yes");
            }
            else
            {
                Write_Line("Value = No");
            }
        }

        private void Write_DLM_Board()
        {
            if (content.Get_Bit("LOBBY:", 39, 0, 2) == "YES")
            {
                Write_Line("Value = Yes");
            }
            else
            {
                Write_Line("Value = No");
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
    }
}
