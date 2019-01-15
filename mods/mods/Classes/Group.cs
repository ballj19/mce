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
    class Group : Controller
    {        
        public Group(string file)
        {
            Initialize_Controller(file);
        }

        public Group(string file, Content content)
        {
            Initialize_Controller(file, content);
        }

        protected override string Get_Fire_Code()
        {
                return "NONE";
        }

        protected override void Set_Variables()
        {
            if (content.content.IndexOf("END") != -1)
            {
                //lastModified = System.IO.File.GetLastWriteTime(@"\\10.113.32.45\shared\Software\" + file);
                //jobName = content.Get_String("JBNAME:", 1);
                iox = General.HexStringToDecimal(content.Get_Nibble("LOBBY:", 6, 0));
                i4o = General.HexStringToDecimal(content.Get_Nibble("LOBBY:", 6, 1));
                aiox = General.HexStringToDecimal(content.Get_Nibble("LOBBY:", 8, 0));
                callbnu = General.HexStringToDecimal(content.Get_Nibble("LOBBY:", 7, 0));
                referenceJob = content.Get_String("JOB_REF", 1).Substring(1, 5);
                /*versionTop = content.Get_Comma_Separated_Byte("MPVERNUM:", 1, 0);
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
                }*/
            }
        }

        public override void Job_Info()
        {
            window = Application.Current.Windows.OfType<MainWindow>().First();
            window.JobInfo.Text = "";
            window.JobInfo.Text += file + "\n";
            window.JobInfo.Text += "Last Modified: " + lastModified.ToString("MM/dd/yy HH:mm:ss") + "\n\n";
            window.JobInfo.Text += jobName + "\n";
            window.JobInfo.Text += "Version: " + fileVersion + "\n\n";
            window.JobInfo.Text += "# of Call Boards: " + callbnu + "\n";
            window.JobInfo.Text += "# of IOX Boards: " + iox + "\n";
            window.JobInfo.Text += "# of I4O Boards: " + i4o + "\n";
            window.JobInfo.Text += "# of AIOX Boards: " + aiox + "\n\n";
        }

        public override void Options()
        {
            window.LobbyOptionsBlock.Text = "This feature is not yet supported for Group files";
            window.BottomOptionsBlock.Text = "This feature is not yet supported for Group files";
        }

        public override void Generate_Headers()
        {
            window.HeaderSP.Children.Clear();

            List<string> calls = new List<string>();

            //ELIGI: Down Hall Calls Front
            for (int x = 0; x < 8; x++)
            {
                for (int n = 0; n < 2; n++)
                {
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("ELIGI:", x + 1, n, b) == "YES")
                        {
                            int callNum = 500 + x * 8 + (3 - b) + 1 + n * 4;
                            calls.Add(callNum.ToString());
                        }
                    }
                }
            }

            //ELIGI: Down Hall Calls Rear
            for (int x = 0; x < 8; x++)
            {
                for (int n = 0; n < 2; n++)
                {
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("ELIGI:", x + 9, n, b) == "YES")
                        {
                            int callNum = 500 + x * 8 + (3 - b) + 1 + n * 4;
                            calls.Add(callNum.ToString() + "R");
                        }
                    }
                }
            }

            //ELIGI: Up Hall Calls Front
            for (int x = 0; x < 8; x++)
            {
                for (int n = 0; n < 2; n++)
                {
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("ELIGI:", x + 17, n, b) == "YES")
                        {
                            int callNum = 600 + x * 8 + (3 - b) + 1 + n * 4;
                            calls.Add(callNum.ToString());
                        }
                    }
                }
            }

            //ELIGI: Up Hall Calls Rear
            for (int x = 0; x < 8; x++)
            {
                for (int n = 0; n < 2; n++)
                {
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("ELIGI:", x + 25, n, b) == "YES")
                        {
                            int callNum = 600 + x * 8 + (3 - b) + 1 + n * 4;
                            calls.Add(callNum.ToString() + "R");
                        }
                    }
                }
            }

            //AELIGI: Aux Down Hall Calls Front
            for (int x = 0; x < 8; x++)
            {
                for (int n = 0; n < 2; n++)
                {
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("AELIGI:", x + 1, n, b) == "YES")
                        {
                            int callNum = 500 + x * 8 + (3 - b) + 1 + n * 4;
                            calls.Add(callNum.ToString() + "X");
                        }
                    }
                }
            }

            //AELIGI: Aux Down Hall Calls Rear
            for (int x = 0; x < 8; x++)
            {
                for (int n = 0; n < 2; n++)
                {
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("AELIGI:", x + 9, n, b) == "YES")
                        {
                            int callNum = 500 + x * 8 + (3 - b) + 1 + n * 4;
                            calls.Add(callNum.ToString() + "RX");
                        }
                    }
                }
            }

            //AELIGI: Aux Up Hall Calls Front
            for (int x = 0; x < 8; x++)
            {
                for (int n = 0; n < 2; n++)
                {
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("AELIGI:", x + 17, n, b) == "YES")
                        {
                            int callNum = 600 + x * 8 + (3 - b) + 1 + n * 4;
                            calls.Add(callNum.ToString() + "X");
                        }
                    }
                }
            }

            //AELIGI: Aux Up Hall Calls Rear
            for (int x = 0; x < 8; x++)
            {
                for (int n = 0; n < 2; n++)
                {
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("AELIGI:", x + 25, n, b) == "YES")
                        {
                            int callNum = 600 + x * 8 + (3 - b) + 1 + n * 4;
                            calls.Add(callNum.ToString() + "RX");
                        }
                    }
                }
            }

            //HELIGI: Hospital Calls Front
            for (int x = 0; x < 8; x++)
            {
                for (int n = 0; n < 2; n++)
                {
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("HELIGI:", x + 1, n, b) == "YES")
                        {
                            int callNum = x * 8 + (3 - b) + 1 + n * 4;
                            calls.Add("EC" + callNum.ToString());
                        }
                    }
                }
            }

            //HELIGI: Hospital Calls Rear
            for (int x = 0; x < 8; x++)
            {
                for (int n = 0; n < 2; n++)
                {
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("HELIGI:", x + 9, n, b) == "YES")
                        {
                            int callNum = x * 8 + (3 - b) + 1 + n * 4;
                            calls.Add("EC" + callNum.ToString() + "R");
                        }
                    }
                }
            }

            //AHELIGI: Hospital Calls Front
            for (int x = 0; x < 8; x++)
            {
                for (int n = 0; n < 2; n++)
                {
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("AHELIGI:", x + 1, n, b) == "YES")
                        {
                            int callNum = x * 8 + (3 - b) + 1 + n * 4;
                            calls.Add("EC" + callNum.ToString() + "X");
                        }
                    }
                }
            }

            //AHELIGI: Hospital Calls Rear
            for (int x = 0; x < 8; x++)
            {
                for (int n = 0; n < 2; n++)
                {
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("AHELIGI:", x + 9, n, b) == "YES")
                        {
                            int callNum = x * 8 + (3 - b) + 1 + n * 4;
                            calls.Add("EC" + callNum.ToString() + "RX");
                        }
                    }
                }
            }

            if (content.content.IndexOf("CIOINE:") != -1)
            {
                List<string> ioLabels = new List<string> { "CIOINE" };
                List<string> cioine = content.IO(ioLabels);
                foreach (string input in cioine)
                {
                    calls.Add(input);
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

        public override void Draw_Landing_Preview()
        {
            int group_top_landing = content.Get_Group_Top_Level();
            List<string> piLabels = content.Get_PILabels();
            string front = "False";
            string rear = "False";
            string tab = "";

            window.LandingLevels.Text = "";
            window.LandingLevels.Height = 16 * group_top_landing + 26;
            window.LandingLevels.BorderThickness = new System.Windows.Thickness(2);
            window.LandingPIs.Text = "";
            window.LandingPIs.Height = 16 * group_top_landing + 26;
            window.LandingPIs.BorderThickness = new System.Windows.Thickness(2);

            window.LandingNormalConfig.Text = "";
            window.LandingNormalConfig.Height = 16 * group_top_landing + 26;
            window.LandingNormalConfig.BorderThickness = new System.Windows.Thickness(2);

            window.LandingAltConfig.Text = "";
            window.LandingAltConfig.Height = 16 * group_top_landing + 26;
            window.LandingAltConfig.BorderThickness = new System.Windows.Thickness(2);

            window.LandingAltHeader.Visibility = Visibility.Hidden;
            window.LandingAltConfig.Visibility = Visibility.Hidden;

            string[] cars = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L" };

            int number_of_cars = Int32.Parse(content.Get_Byte("LOBBY:", 18));

            window.LandingNormalHeader.Width = 48 + 48 * number_of_cars;
            window.LandingNormalConfig.Width = 48 + 48 * number_of_cars;

            window.LandingLevels.Text += "Car\n";
            window.LandingPIs.Text += "Car\n";

            for (int c = 0; c < number_of_cars; c++)
            {
                if (c < number_of_cars - 1)
                {
                    tab = "\t";
                }
                else
                {
                    tab = "";
                }
                window.LandingNormalConfig.Text += cars[c] + tab;
            }

            window.LandingNormalConfig.Text += "\n";

            for (int x = group_top_landing; x >= 1; x--)
            {
                for (int c = 0; c < number_of_cars; c++)
                {
                    if (content.Get_Bit("ELIGIV" + cars[c], x, 1, 1) == "YES" || content.Get_Bit("ELIGIV" + cars[c], x, 1, 3) == "YES" || content.Get_Bit("ELIGIV" + cars[c], x, 0, 1) == "YES" || content.Get_Bit("ELIGIV" + cars[c], x, 0, 3) == "YES")
                    {
                        front = "F";
                    }
                    else
                    {
                        front = ".";
                    }

                    if (content.Get_Bit("ELIGIV" + cars[c], x, 1, 0) == "YES" || content.Get_Bit("ELIGIV" + cars[c], x, 1, 2) == "YES" || content.Get_Bit("ELIGIV" + cars[c], x, 0, 0) == "YES" || content.Get_Bit("ELIGIV" + cars[c], x, 0, 2) == "YES")
                    {
                        rear = "R";
                    }
                    else
                    {
                        rear = ".";
                    }
                    if (c < number_of_cars - 1)
                    {
                        tab = "\t";
                    }
                    else
                    {
                        tab = "";
                    }
                    window.LandingNormalConfig.Text += front + " " + rear + tab;
                }
                window.LandingNormalConfig.Text += "\n";
                window.LandingLevels.Text += x + "\n";
                window.LandingPIs.Text += piLabels[x - 1] + "\n";
            }

            //Remove Last new line character from each column
            window.LandingPIs.Text = window.LandingPIs.Text.Substring(0, window.LandingPIs.Text.Length - 1);
            window.LandingLevels.Text = window.LandingLevels.Text.Substring(0, window.LandingLevels.Text.Length - 1);
            window.LandingNormalConfig.Text = window.LandingNormalConfig.Text.Substring(0, window.LandingNormalConfig.Text.Length - 1);
        }
    }
}
