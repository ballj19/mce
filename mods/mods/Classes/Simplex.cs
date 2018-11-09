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


namespace mods
{
    class Simplex : Controller
    {
        public Simplex(string file)
        {
            content = new Content(file);
            Initialize_Controller(file);
        }

        protected override void Set_Variables()
        {
            if (content.content.IndexOf("END") != -1)
            {
                lastModified = System.IO.File.GetLastWriteTime(@"\\10.113.32.45\shared\Software\" + file);
                jobName = content.Get_String("JBNAME:", 1);
                topFloor = content.Get_Byte("BOTTOM:", 2) + 'H';
                topFloorDecimal = (General.HexStringToDecimal(topFloor) + 1).ToString();
                botFloor = content.Get_Byte("BOTTOM:", 1) + 'H';
                botFloorDecimal = (General.HexStringToDecimal(botFloor) + 1).ToString();
                falseFloors = content.Get_Bit("LOBBY", 30, 0, 3);
                nudging = content.Get_Bit("LOBBY", 27, 0, 3);
                i4o = General.HexStringToDecimal(content.Get_Nibble("LOBBY:", 55, 0));
                iox = General.HexStringToDecimal(content.Get_Nibble("LOBBY:", 40, 0));
                aiox = General.HexStringToDecimal(content.Get_Nibble("LOBBY:", 54, 0));
                callbnu = General.HexStringToDecimal(content.Get_Nibble("LOBBY:", 36, 0));
                rearDoor = content.Get_Bit("LOBBY:", 12, 0, 3);
                ceBoard = content.Get_Bit("BOTTOM:", 12, 1, 1);
                ncBoard = content.Get_Bit("LOBBY:", 38, 1, 3);
                ftBoard = content.Get_Bit("BOTTOM:", 8, 1, 3);
                versionTop = content.Get_Comma_Separated_Byte("MPVERNUM:", 1, 0);
                versionMid = content.Get_Comma_Separated_Byte("MPVERNUM:", 1, 1);
                versionBot = content.Get_String("CUSTOM:", 1);
                if (versionTop[0] == '0' && versionTop.Length > 1)
                {
                    versionTop = versionTop.Substring(1, 1);
                }
                if (versionBot[0] == '0' && versionBot.Length > 1 && versionBot[1] != ' ')
                {
                    versionBot = versionBot.Substring(1, 1);
                }
                if (versionTop == "N/A")
                {
                    fileVersion = "N/A";
                }
                else
                {
                    fileVersion = versionTop + "." + versionMid + "." + versionBot;
                }
                drivebit2 = content.Get_Bit("LOBBY", 26, 0, 1);
                drivebit3 = content.Get_Bit("LOBBY", 26, 0, 0);
                driveType = "";
                if (drivebit2 == "YES" && drivebit3 == "YES")
                {
                    driveType = "IMC-AC";
                }
                else if (drivebit2 == "YES")
                {
                    driveType = "IMC-MG";
                }
                else if (drivebit3 == "YES")
                {
                    driveType = "IMC-SCR";
                }
                else
                {
                    driveType = "NONE";
                }
            }
        }

        public override void Job_Info()
        {
            //Job Info
            window.JobInfo.Text = "";
            window.JobInfo.Text += file + "\n";
            window.JobInfo.Text += "Last Modified: " + lastModified.ToString("MM/dd/yy HH:mm:ss") + "\n\n";
            window.JobInfo.Text += jobName + "\n";
            window.JobInfo.Text += "Version: " + fileVersion + "\n\n";
            window.JobInfo.Text += "Top Floor: " + topFloorDecimal + "\n";
            window.JobInfo.Text += "Bottom Floor: " + botFloorDecimal + "\n\n";
            window.JobInfo.Text += "Independent Rear Doors: " + rearDoor + "\n";
            window.JobInfo.Text += "False Floors: " + falseFloors + "\n";
            window.JobInfo.Text += "Nudging: " + nudging + "\n";
            window.JobInfo.Text += "Drive Type: " + driveType + "\n";

            //Hardware
            window.JobInfo.Text += "\n";
            window.JobInfo.Text += "# of CALL Boards: " + callbnu + "\n";
            window.JobInfo.Text += "# of IOX Boards: " + iox + "\n";
            window.JobInfo.Text += "# of I4O Boards: " + i4o + "\n";
            window.JobInfo.Text += "# of AIOX Boards: " + aiox + "\n\n";
            window.JobInfo.Text += "CE Board: " + ceBoard + "\n";
            window.JobInfo.Text += "NC Board: " + ncBoard + "\n";
            window.JobInfo.Text += "FT Board: " + ftBoard + "\n";
        }

        public override void Options()
        {
            window.LobbyOptionsBlock.Text = "This feature is not yet supported for Simplex files";
            window.BottomOptionsBlock.Text = "This feature is not yet supported for Simplex files";
        }

        public override void Generate_Headers()
        {
            window.HeaderSP.Children.Clear();

            List<string> calls = new List<string>();

            if (ncBoard == "NO") //Exclude ELIGI: Car Calls if NC board is set
            {
                //ELIGI: Front Car Calls
                for (int x = 0; x < 4; x++)
                {
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("ELIGI:", x + 1, 0, b) == "YES")
                        {
                            int callNum = 100 + x * 8 + (3 - b) + 1;
                            calls.Add(callNum.ToString());
                        }
                    }
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("ELIGI:", x + 1, 1, b) == "YES")
                        {
                            int callNum = 100 + x * 8 + (3 - b) + 5;
                            calls.Add(callNum.ToString());
                        }
                    }
                }

                //ELIGI: Rear Car Calls
                for (int x = 0; x < 4; x++)
                {
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("ELIGI:", x + 5, 0, b) == "YES")
                        {
                            int callNum = 100 + x * 8 + (3 - b) + 1;
                            calls.Add(callNum.ToString() + "R");
                        }
                    }
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("ELIGI:", x + 5, 1, b) == "YES")
                        {
                            int callNum = 100 + x * 8 + (3 - b) + 5;
                            calls.Add(callNum.ToString() + "R");
                        }
                    }
                }
            }

            //ELIGI: Front Down Hall Calls
            for (int x = 0; x < 4; x++)
            {
                for (int b = 3; b >= 0; b--)
                {
                    if (content.Get_Bit("ELIGI:", x + 17, 0, b) == "YES")
                    {
                        int callNum = 500 + x * 8 + (3 - b) + 1;
                        calls.Add(callNum.ToString());
                    }
                }
                for (int b = 3; b >= 0; b--)
                {
                    if (content.Get_Bit("ELIGI:", x + 17, 1, b) == "YES")
                    {
                        int callNum = 500 + x * 8 + (3 - b) + 5;
                        calls.Add(callNum.ToString());
                    }
                }
            }

            //ELIGI: Rear Down Hall Calls
            for (int x = 0; x < 4; x++)
            {
                for (int b = 3; b >= 0; b--)
                {
                    if (content.Get_Bit("ELIGI:", x + 21, 0, b) == "YES")
                    {
                        int callNum = 500 + x * 8 + (3 - b) + 1;
                        calls.Add(callNum.ToString() + "R");
                    }
                }
                for (int b = 3; b >= 0; b--)
                {
                    if (content.Get_Bit("ELIGI:", x + 21, 1, b) == "YES")
                    {
                        int callNum = 500 + x * 8 + (3 - b) + 5;
                        calls.Add(callNum.ToString() + "R");
                    }
                }
            }

            //ELIGI: Front Up Hall Calls
            for (int x = 0; x < 4; x++)
            {
                for (int b = 3; b >= 0; b--)
                {
                    if (content.Get_Bit("ELIGI:", x + 25, 0, b) == "YES")
                    {
                        int callNum = 600 + x * 8 + (3 - b) + 1;
                        calls.Add(callNum.ToString());
                    }
                }
                for (int b = 3; b >= 0; b--)
                {
                    if (content.Get_Bit("ELIGI:", x + 25, 1, b) == "YES")
                    {
                        int callNum = 600 + x * 8 + (3 - b) + 5;
                        calls.Add(callNum.ToString());
                    }
                }
            }

            //ELIGI: Rear Up Hall Calls
            for (int x = 0; x < 4; x++)
            {
                for (int b = 3; b >= 0; b--)
                {
                    if (content.Get_Bit("ELIGI:", x + 29, 0, b) == "YES")
                    {
                        int callNum = 600 + x * 8 + (3 - b) + 1;
                        calls.Add(callNum.ToString() + "R");
                    }
                }
                for (int b = 3; b >= 0; b--)
                {
                    if (content.Get_Bit("ELIGI:", x + 29, 1, b) == "YES")
                    {
                        int callNum = 600 + x * 8 + (3 - b) + 5;
                        calls.Add(callNum.ToString() + "R");
                    }
                }
            }

            if (content.content.IndexOf("XELIGI:") != -1)
            {
                //XELIGI: Front Down Hall Calls
                for (int x = 0; x < 4; x++)
                {
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("XELIGI:", x + 1, 0, b) == "YES")
                        {
                            int callNum = 500 + x * 8 + (3 - b) + 1;
                            calls.Add(callNum.ToString() + "X");
                        }
                    }
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("XELIGI:", x + 1, 1, b) == "YES")
                        {
                            int callNum = 500 + x * 8 + (3 - b) + 5;
                            calls.Add(callNum.ToString() + "X");
                        }
                    }
                }

                //XELIGI: Rear Down Hall Calls
                for (int x = 0; x < 4; x++)
                {
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("XELIGI:", x + 5, 0, b) == "YES")
                        {
                            int callNum = 500 + x * 8 + (3 - b) + 1;
                            calls.Add(callNum.ToString() + "R" + "X");
                        }
                    }
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("XELIGI:", x + 5, 1, b) == "YES")
                        {
                            int callNum = 500 + x * 8 + (3 - b) + 5;
                            calls.Add(callNum.ToString() + "R" + "X");
                        }
                    }
                }

                //XELIGI: Front Up Hall Calls
                for (int x = 0; x < 4; x++)
                {
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("XELIGI:", x + 9, 0, b) == "YES")
                        {
                            int callNum = 600 + x * 8 + (3 - b) + 1;
                            calls.Add(callNum.ToString() + "X");
                        }
                    }
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("XELIGI:", x + 9, 1, b) == "YES")
                        {
                            int callNum = 600 + x * 8 + (3 - b) + 5;
                            calls.Add(callNum.ToString() + "X");
                        }
                    }
                }

                //XELIGI: Rear Up Hall Calls
                for (int x = 0; x < 4; x++)
                {
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("XELIGI:", x + 13, 0, b) == "YES")
                        {
                            int callNum = 600 + x * 8 + (3 - b) + 1;
                            calls.Add(callNum.ToString() + "R" + "X");
                        }
                    }
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("XELIGI:", x + 13, 1, b) == "YES")
                        {
                            int callNum = 600 + x * 8 + (3 - b) + 5;
                            calls.Add(callNum.ToString() + "R" + "X");
                        }
                    }
                }
            }

            if (content.content.IndexOf("HELIGI:") != -1)
            {
                //HELIGI: Front Car Calls
                for (int x = 0; x < 4; x++)
                {
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("HELIGI:", x + 1, 0, b) == "YES")
                        {
                            int callNum = x * 8 + (3 - b) + 1;
                            calls.Add("EC" + callNum.ToString());
                        }
                    }
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("HELIGI:", x + 1, 1, b) == "YES")
                        {
                            int callNum = x * 8 + (3 - b) + 5;
                            calls.Add("EC" + callNum.ToString());
                        }
                    }
                }

                //HELIGI: Rear Car Calls
                for (int x = 0; x < 4; x++)
                {
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("HELIGI:", x + 5, 0, b) == "YES")
                        {
                            int callNum = x * 8 + (3 - b) + 1;
                            calls.Add("EC" + callNum.ToString() + "R");
                        }
                    }
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("HELIGI:", x + 5, 1, b) == "YES")
                        {
                            int callNum = x * 8 + (3 - b) + 5;
                            calls.Add("EC" + callNum.ToString() + "R");
                        }
                    }
                }
            }

            if (content.content.IndexOf("INELIG:") != -1)
            {
                //INELIG: System Input Eligibility Map
                List<string> inelig = content.IO(new List<string> { "INELIG" });
                foreach (string input in inelig)
                {
                    calls.Add(input);
                }
            }

            if (content.content.IndexOf("FSECURA:") != -1)
            {
                //FSECUR: Per Opening Security Input Eligibility Map
                for (int x = 0; x < 8; x++)
                {
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("FSECURA:", x + 1, 0, b) == "YES")
                        {
                            int callNum = x * 8 + (3 - b) + 1;
                            calls.Add("S" + callNum.ToString());
                        }
                    }
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("FSECURA:", x + 1, 1, b) == "YES")
                        {
                            int callNum = x * 8 + (3 - b) + 5;
                            calls.Add("S" + callNum.ToString());
                        }
                    }
                }
            }

            if (content.content.IndexOf("RSECURA:") != -1)
            {
                //RSECUR: Per Opening Security Input Eligibility Map
                for (int x = 0; x < 8; x++)
                {
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("RSECURA:", x + 1, 0, b) == "YES")
                        {
                            int callNum = x * 8 + (3 - b) + 1;
                            calls.Add("S" + callNum.ToString() + "R");
                        }
                    }
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("RSECURA:", x + 1, 1, b) == "YES")
                        {
                            int callNum = x * 8 + (3 - b) + 5;
                            calls.Add("S" + callNum.ToString() + "R");
                        }
                    }
                }
            }

            //Add to Headers Tab
            int numOfCalls = calls.Count;
            int column = 0;
            do
            {
                if (numOfCalls < 16)
                {
                    for (int x = 16 - numOfCalls; x > 0; x--)
                    {
                        calls.Add("N/C");
                    }
                }

                StackPanel sp = new StackPanel { Orientation = Orientation.Vertical, Name = ("Column" + column), Margin = new Thickness(10, 15, 10, 0) };
                for (int x = 15; x >= 0; x--)
                {
                    Thickness margin = new Thickness(0, -2, 0, 0);
                    if (x == 7)
                    {
                        margin = new Thickness(0, 0, 0, 0);
                    }
                    sp.Children.Add(
                        new TextBox
                        {
                            Text = calls[column * 16 + x],
                            Width = 50,
                            Height = 25,
                            BorderThickness = new Thickness(2),
                            BorderBrush = System.Windows.Media.Brushes.Black,
                            IsReadOnly = true,
                            Background = System.Windows.Media.Brushes.Transparent,
                            TextAlignment = TextAlignment.Center,
                            Margin = margin
                        });
                    numOfCalls--;
                }
                column++;

                window.HeaderSP.Children.Add(sp);
            } while (numOfCalls > 0);
        }
    }
}
