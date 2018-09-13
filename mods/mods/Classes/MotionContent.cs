using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.IO;
using System.ComponentModel;
using System.Windows.Threading;
using Excel = Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;
using IWshRuntimeLibrary;
using Outlook = Microsoft.Office.Interop.Outlook;
using System.Globalization;

namespace mods
{
    class MotionContent
    {
        public List<string> values = new List<string>();
        public List<string> options = new List<string>();
        private string file = "";
        private MainWindow window = Application.Current.MainWindow as MainWindow;

        public MotionContent(string file)
        {
            this.file = file;

            int endIndex = Get_Motion_Options();
            Get_Motion_Values(endIndex);
        }

        private int Get_Motion_Options()
        {
            options.Clear();

            byte[] fileBytes = System.IO.File.ReadAllBytes(file);
            string hex = BitConverter.ToString(fileBytes).Replace("-", string.Empty);

            int index = hex.IndexOf("094A6F62204E616D653A");

            while (hex.Substring(index + 2, 2) != "00") //Means option name would start with null character ==> no more options.
            {
                index = Decode_String(hex, index);
            }

            return index;
        }

        private int Decode_String(string decode, int startIndex)
        {
            int index = startIndex + 2;

            int nameLength = General.HexStringToDecimal(decode.Substring(startIndex, 2));

            options.Add(EnglishString(decode.Substring(index, nameLength * 2)));

            index += nameLength * 2;

            index += 16;

            index += 8;

            index += 24;

            int thirdnum = General.HexStringToDecimal(decode.Substring(index, 2));

            index += 8;

            int defaultCount = General.HexStringToDecimal(decode.Substring(index, 2));

            index += 2;

            index += 2 * defaultCount;

            return startIndex + 2 * (41 + nameLength + defaultCount + thirdnum);
        }

        private string EnglishString(string hex)
        {
            string english = "";

            for (int i = 0; i < hex.Length; i += 2)
            {
                int charInt = Int16.Parse(hex.Substring(i, 2), NumberStyles.AllowHexSpecifier);
                english += (char)charInt;
            }

            return english;
        }

        private void Get_Motion_Values(int beginIndex)
        {
            byte[] fileBytes = System.IO.File.ReadAllBytes(file);
            string hex = BitConverter.ToString(fileBytes).Replace("-", string.Empty);

            int index = beginIndex + 48;

            List<string> values_raw = new List<string>();
            string valueString = "";
            for (int i = 0; i < options.Count; i++)
            {
                while (hex.Substring(index, 2) != "00")
                {
                    valueString += hex.Substring(index, 2);
                    index += 2;
                }
                values_raw.Add(valueString);
                valueString = "";
                index += 2;
            }


            List<string> valuesEnglish = new List<string>();
            foreach (string value in values_raw)
            {
                string english = "";
                for (int i = 0; i < value.Length; i += 2)
                {
                    int charInt = Int16.Parse(value.Substring(i, 2), NumberStyles.AllowHexSpecifier);
                    english += (char)charInt;
                }

                valuesEnglish.Add(english);
            }

            values = valuesEnglish;
        }

        public string Get_Value(string option)
        {
            int optionIndex = options.IndexOf(option);

            return values[optionIndex];
        }

        public void Generate_Job_Info()
        {
            DateTime lastModified = System.IO.File.GetLastWriteTime(file);
            string jobName = Get_Value("Job Name:");
            string jobNumber = Get_Value("SalesOdr");
            string topFloor = Get_Value("3. Top Landing Served (This car)");
            string version = Get_Value("PCA_Ver#06.03.0000");
            string m2000 = Get_Value("M2000");
            string simplex = Get_Value("simplex");
            string duplex = Get_Value("Duplex");
            string rearDoors = "NO";
            string type = "";
            string subtype = "";
            if(Get_Value("PAGE1_YES1") == "1")
            {
                rearDoors = "YES";
            }
            if(m2000 == "1")
            {
                type = "M2000";
            }
            else
            {
                type = "M4000";
            }
            if(simplex == "1")
            {
                subtype = "Simplex";
            }
            else if(duplex == "1")
            {
                subtype = "Duplex";
            }
            else
            {
                subtype = "Local";
            }

            //Job Info
            window.JobInfo.Text = "";
            window.JobInfo.Text += file + "\n";
            window.JobInfo.Text += "Last Modified: " + lastModified.ToString("MM/dd/yy HH:mm:ss") + "\n\n";

            window.JobInfo.Text += jobName + "\n";
            window.JobInfo.Text += "Controller Type: " + type + " - " + subtype + "\n";
            window.JobInfo.Text += "Version: " + version + "\n\n";

            window.JobInfo.Text += "Top Floor: " + topFloor + "\n";
            window.JobInfo.Text += "Independent Rear Doors: " + rearDoors+ "\n";

            /*
                DateTime lastModified = System.IO.File.GetLastWriteTime(G_DRIVE + "Software\\" + file);
                string jobName = content.Get_String("JBNAME:", 1);
                string topFloor = content.Get_Byte("BOTTOM:", 2) + 'H';
                string topFloorDecimal = (General.HexStringToDecimal(topFloor) + 1).ToString();
                string botFloor = content.Get_Byte("BOTTOM:", 1) + 'H';
                string botFloorDecimal = (General.HexStringToDecimal(botFloor) + 1).ToString();
                string falseFloors = content.Get_Bit("CPVAR", 3, 0, 3);
                string nudging = content.Get_Bit("CPVAR", 7, 0, 3);
                int i4o = General.HexStringToDecimal(content.Get_Nibble("LOBBY:", 40, 1));
                int iox = General.HexStringToDecimal(content.Get_Nibble("LOBBY:", 40, 0));
                int aiox = General.HexStringToDecimal(content.Get_Nibble("LOBBY:", 52, 0));
                int callbnu = General.HexStringToDecimal(content.Get_Nibble("LOBBY:", 41, 1));
                string rearDoor = content.Get_Bit("LOBBY:", 12, 0, 3);
                string ceBoard = content.Get_Bit("BOTTOM:", 6, 1, 1);
                string ncBoard = content.Get_Bit("LOBBY:", 38, 1, 3);
                string ftBoard = content.Get_Bit("BOTTOM:", 6, 1, 3);
                string dlmBoard = content.Get_Bit("LOBBY:", 39, 0, 1);
                string versionTop = content.Get_Comma_Separated_Byte("MPVERNUM:", 1, 0);
                string versionMid = content.Get_Comma_Separated_Byte("MPVERNUM:", 1, 1);
                string versionBot = content.Get_String("CUSTOM:", 1);
                if (versionTop[0] == '0' && versionTop.Length > 1)
                {
                    versionTop = versionTop.Substring(1, 1);
                }
                if (versionBot[0] == '0' && versionBot.Length > 1 && versionBot[1] != ' ')
                {
                    versionBot = versionBot.Substring(1, 1);
                }
                if(versionTop == "N/A")
                {
                    selectedFileVersion = "N/A";
                }
                else
                {
                    this.selectedFileVersion = versionTop + "." + versionMid + "." + versionBot;
                }
                string drivebit2 = content.Get_Bit("CPVAR", 2, 0, 1);
                string drivebit3 = content.Get_Bit("CPVAR", 2, 0, 0);
                string driveType = "";
                if(drivebit2 == "YES" && drivebit3 == "YES")
                {
                    driveType = "IMC-AC";
                }
                else if(drivebit2 == "YES")
                {
                    driveType = "IMC-MG";
                }
                else if(drivebit3 == "YES")
                {
                    driveType = "IMC-SCR";
                }
                else
                {
                    driveType = "NONE";
                }


                //Job Info
                JobInfo.Text = "";
                JobInfo.Text += file + "\n";
                JobInfo.Text += "Last Modified: " + lastModified.ToString("MM/dd/yy HH:mm:ss") + "\n\n";
                JobInfo.Text += jobName + "\n";
                JobInfo.Text += "Version: " + selectedFileVersion + "\n\n";
                JobInfo.Text += "Top Floor: " + topFloorDecimal + "\n";
                JobInfo.Text += "Bottom Floor: " + botFloorDecimal + "\n\n";
                JobInfo.Text += "Independent Rear Doors: " + rearDoor + "\n";
                JobInfo.Text += "Security: " + Security() + "\n";
                JobInfo.Text += "False Floors: " + falseFloors + "\n";
                JobInfo.Text += "Nudging: " + nudging + "\n";
                JobInfo.Text += "Drive Type: " + driveType + "\n";

                //Hardware
                JobInfo.Text += "\n";
                JobInfo.Text += "# of CALL Boards: " + callbnu + "\n";
                JobInfo.Text += "# of IOX Boards: " + iox + "\n";
                JobInfo.Text += "# of I4O Boards: " + i4o + "\n";
                JobInfo.Text += "# of AIOX Boards: " + aiox + "\n\n";
                JobInfo.Text += "CE Board: " + ceBoard + "\n";
                JobInfo.Text += "NC Board: " + ncBoard + "\n";
                JobInfo.Text += "FT Board: " + ftBoard + "\n";
                JobInfo.Text += "DLM Board: " + dlmBoard + "\n\n";
                */
        }

        public void Draw_Landing_Preview()
        {
            int top_landing = Int32.Parse(Get_Value("3. Top Landing Served (This car)"));

            window.LandingNormalHeader.Width = 96;
            window.LandingNormalConfig.Width = 96;
            window.LandingAltHeader.Width = 96;
            window.LandingAltConfig.Width = 96;

            window.LandingNormalConfig.Text = "";
            window.LandingNormalConfig.Height = 0;
            window.LandingNormalConfig.BorderThickness = new Thickness(0);
            window.LandingAltConfig.Text = "";
            window.LandingAltConfig.Height = 0;
            window.LandingAltConfig.BorderThickness = new Thickness(0);

            window.LandingLevels.Text = "";
            window.LandingLevels.Height = 16 * top_landing + 10;
            window.LandingLevels.BorderThickness = new Thickness(2);
            window.LandingPIs.Text = "";
            window.LandingPIs.Height = 16 * top_landing + 10;
            window.LandingPIs.BorderThickness = new Thickness(2);

            window.LandingNormalConfig.Text = "";
            window.LandingNormalConfig.Height = 16 * top_landing + 10;
            window.LandingNormalConfig.BorderThickness = new Thickness(2);

            for(int f = top_landing; f >= 1; f--)
            {
                window.LandingLevels.Text += f + "\n";

                string frontlevelValue = Get_Value("Serves Front1_BOX" + f);
                string rearlevelValue = Get_Value("Serves Rear1_BOX" + f);

                string front = ".";
                string rear = ".";

                if(frontlevelValue == "1")
                {
                    front = "F";
                }
                if(rearlevelValue == "1")
                {
                    rear = "R";
                }

                window.LandingNormalConfig.Text += front + " " + rear + "\n";
            }

            window.LandingLevels.Text = window.LandingLevels.Text.Substring(0, window.LandingLevels.Text.Length - 1); //Remove final \n
            window.LandingNormalConfig.Text = window.LandingNormalConfig.Text.Substring(0, window.LandingNormalConfig.Text.Length - 1); //Remove final \n
        }
    }
}
