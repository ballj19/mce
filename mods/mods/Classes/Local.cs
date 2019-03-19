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
    public class Local : Controller
    {
        public override int topLandingHeight
        {
            get
            {
                return 16 * (General.HexStringToDecimal(content.Get_Byte("BOTTOM:", 2)) + 1) + 10;
            }
        }

    public Local(string file)
        {
            Initialize_Controller(file);
        }

        public Local(string file, Content content)
        {
            Initialize_Controller(file, content);
        }

        protected override void Set_Variables()
        {
            if (content.content.IndexOf("END") != -1)
            {
                //lastModified = System.IO.File.GetLastWriteTime(@"\\10.113.32.45\shared\Software\" + file);
                //jobName = content.Get_String("JBNAME:", 1);
                topFloor = content.Get_Byte("BOTTOM:", 2) + 'H';
                topFloorDecimal = (General.HexStringToDecimal(topFloor) + 1).ToString();
                botFloor = content.Get_Byte("BOTTOM:", 1) + 'H';
                botFloorDecimal = (General.HexStringToDecimal(botFloor) + 1).ToString();
                falseFloors = content.Get_Bit("CPVAR", 3, 0, 3);
                nudging = content.Get_Bit("CPVAR", 7, 0, 3);
                i4o = General.HexStringToDecimal(content.Get_Nibble("LOBBY:", 40, 1));
                iox = General.HexStringToDecimal(content.Get_Nibble("LOBBY:", 40, 0));
                aiox = General.HexStringToDecimal(content.Get_Nibble("LOBBY:", 52, 0));
                callbnu = General.HexStringToDecimal(content.Get_Nibble("LOBBY:", 41, 1));
                rearDoor = content.Get_Bit("LOBBY:", 12, 0, 3);
                ceBoard = content.Get_Bit("BOTTOM:", 6, 1, 1);
                ncBoard = content.Get_Bit("LOBBY:", 38, 1, 3);
                ftBoard = content.Get_Bit("BOTTOM:", 6, 0, 3);
                dlmBoard = content.Get_Bit("LOBBY:", 39, 0, 1);
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
                drivebit2 = content.Get_Bit("CPVAR", 2, 0, 1);
                drivebit3 = content.Get_Bit("CPVAR", 2, 0, 0);
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

        protected override string Get_Fire_Code()
        {
            string CHIOPT = content.Get_Bit("LOBBY:", 21, 0, 3);
            string CHIOPT01 = content.Get_Bit("LOBBY:", 22, 0, 2);
            string ANSI85 = content.Get_Bit("LOBBY:", 8, 0, 3);
            string ANSI89 = content.Get_Bit("LOBBY:", 8, 0, 1);
            string ANSI2K = content.Get_Bit("LOBBY:", 12, 1, 3);
            string AUST = content.Get_Bit("LOBBY:", 8, 1, 1);
            string DETROIT = content.Get_Bit("LOBBY:", 43, 1, 0);
            string HAWAII = content.Get_Bit("LOBBY:", 21, 0, 1);
            string MASS2K = content.Get_Bit("LOBBY:", 12, 1, 0);
            string NYOPT91 = content.Get_Bit("LOBBY:", 8, 0, 0);
            string NYOPT = content.Get_Bit("LOBBY:", 21, 1, 0);

            if (CHIOPT == "YES")
            {
                if(CHIOPT01 == "YES")
                    return "Chicago 2001";
                else
                    return "Chicago";
            }
            else if(AUST == "YES")
            {
                return "Australia";
            }
            else if(DETROIT == "YES")
            {
                return "Detroit";
            }
            else if(HAWAII == "YES")
            {
                return "Hawaii";
            }
            else if(MASS2K == "YES")
            {
                return "Massachusets 2K";
            }
            else if(NYOPT == "YES" || NYOPT91 == "YES")
            {
                return "New York City";
            }
            else if(ANSI85 == "YES" && ANSI89 == "YES")
            {
                if (ANSI2K == "YES")
                    return "ANSI 2K";
                else
                    return "ANSI 85-89 or 96";
            }
            return "NONE";
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
            window.JobInfo.Text += "Security: " + Security() + "\n";
            window.JobInfo.Text += "False Floors: " + falseFloors + "\n";
            window.JobInfo.Text += "Nudging: " + nudging + "\n";
            window.JobInfo.Text += "Drive Type: " + driveType + "\n";
            window.JobInfo.Text += "Fire Code: " + firecode + "\n";
            //window.JobInfo.Text += "VersionInt: " + fileVersionInt + "\n";

            //Hardware
            window.JobInfo.Text += "\n";
            window.JobInfo.Text += "# of CALL Boards: " + callbnu + "\n";
            window.JobInfo.Text += "# of IOX Boards: " + iox + "\n";
            window.JobInfo.Text += "# of I4O Boards: " + i4o + "\n";
            window.JobInfo.Text += "# of AIOX Boards: " + aiox + "\n\n";
            window.JobInfo.Text += "CE Board: " + ceBoard + "\n";
            window.JobInfo.Text += "NC Board: " + ncBoard + "\n";
            window.JobInfo.Text += "FT Board: " + ftBoard + "\n";
            window.JobInfo.Text += "DLM Board: " + dlmBoard + "\n\n";
        }

        private string Security()
        {
            string security = "";

            bool BSI = false;
            bool SECRTY = false;
            bool CRTLOK = false;
            bool SECUR = false;
            bool NEWSECRTY = false;

            foreach (string input in content.inputs)
            {
                if (input == "BSI")
                {
                    BSI = true;
                }
            }

            if (content.Get_Bit("LOBBY:", 31, 0, 0) == "YES")
            {
                NEWSECRTY = true;
            }

            if (content.Get_Bit("LOBBY:", 31, 0, 1) == "YES")
            {
                CRTLOK = true;
            }

            if (content.Get_Bit("LOBBY:", 31, 0, 3) == "YES")
            {
                SECRTY = true;
            }

            if (content.Get_Bit("CPVAR", 7, 1, 0) == "YES")
            {
                SECUR = true;
            }

            if (BSI)
            {
                security += "BSI";
            }

            if (SECRTY && CRTLOK && SECUR)
            {
                if (security != "")
                {
                    security += ", ";
                }

                security += "CRTLOCK";
            }

            if (NEWSECRTY)
            {
                if (security != "")
                {
                    security += ", ";
                }

                security += "ACE";
            }

            if (security == "")
            {
                return "NO";
            }
            else
            {
                security = "YES - " + security;
            }

            return security;
        }

        public override void Options()
        {
            window.LobbyOptionsBlock.Text = "";
            window.BottomOptionsBlock.Text = "";

            window.LobbyOptionsBlock.Text = content.Build_OptionsMap("LOBBY:");
            window.BottomOptionsBlock.Text = content.Build_OptionsMap("BOTTOM:");
        }

        public override void Generate_IO()
        {
            base.Generate_IO();

            string ncBoard = content.Get_Bit("LOBBY:", 38, 1, 3);

            if (ncBoard == "YES")
            {
                List<string> ncinputs = content.IO(new List<string> { "NIOINS" });
                List<string> ncoutputs = content.IO(new List<string> { "NIOOUTS" }, 'O');

                Label ncInputLabel = new Label
                {
                    Content = "NC Inputs",
                };

                window.IOInfoSP.Children.Add(ncInputLabel);

                for (int row = 0; row < 8; row++)
                {
                    StackPanel rowSP = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Margin = new Thickness(20, 0, 0, 0),
                    };

                    for (int column = 0; column < 8; column++)
                    {
                        string ioText = "";
                        if (row * 8 + (7 - column) >= ncinputs.Count)
                        {
                            ioText = "XXXX";
                        }
                        else
                        {
                            ioText = ncinputs[row * 8 + (7 - column)];
                        }

                        TextBox io = new TextBox
                        {
                            Text = ioText,
                            Width = 48,
                            Height = 25,
                            BorderThickness = new Thickness(0),
                            IsReadOnly = true,
                            Background = System.Windows.Media.Brushes.Transparent,
                            TextAlignment = TextAlignment.Center,
                            Tag = "IO",
                        };

                        rowSP.Children.Add(io);

                        if (column == 3)
                        {
                            TextBox hyphen = new TextBox
                            {
                                Text = "---",
                                Width = 20,
                                Height = 25,
                                BorderThickness = new Thickness(0),
                                IsReadOnly = true,
                                Background = System.Windows.Media.Brushes.Transparent,
                                TextAlignment = TextAlignment.Center,
                                Tag = "hyphen",
                            };

                            rowSP.Children.Add(hyphen);
                        }
                        else if (column < 7) // dont want to add hyphen for last column
                        {
                            TextBox hyphen = new TextBox
                            {
                                Text = "-",
                                Width = 7,
                                Height = 25,
                                BorderThickness = new Thickness(0),
                                IsReadOnly = true,
                                Background = System.Windows.Media.Brushes.Transparent,
                                TextAlignment = TextAlignment.Center,
                                Tag = "hyphen",
                            };

                            rowSP.Children.Add(hyphen);
                        }
                    }

                    bool rowIsEmpty = true;

                    foreach (var child in rowSP.Children)
                    {
                        if (child.GetType() == typeof(TextBox))
                        {
                            TextBox tb = child as TextBox;

                            if (tb.Text != "XXXX" && tb.Tag.ToString() == "IO")
                            {
                                rowIsEmpty = false;
                            }
                        }
                    }

                    if (!rowIsEmpty)
                    {
                        window.IOInfoSP.Children.Add(rowSP);
                    }
                }

                Label ncOutputLabel = new Label
                {
                    Content = "NC Outputs",
                };

                window.IOInfoSP.Children.Add(ncOutputLabel);

                for (int row = 0; row < 8; row++)
                {
                    StackPanel rowSP = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Margin = new Thickness(20, 0, 0, 0),
                    };

                    for (int column = 0; column < 8; column++)
                    {
                        string ioText = "";
                        if (row * 8 + column >= ncoutputs.Count)
                        {
                            ioText = "XXXX";
                        }
                        else
                        {
                            ioText = ncoutputs[row * 8 + column];
                        }

                        TextBox io = new TextBox
                        {
                            Text = ioText,
                            Width = 48,
                            Height = 25,
                            BorderThickness = new Thickness(0),
                            IsReadOnly = true,
                            Background = System.Windows.Media.Brushes.Transparent,
                            TextAlignment = TextAlignment.Center,
                            Tag = "IO",
                        };

                        rowSP.Children.Add(io);

                        if (column == 3)
                        {
                            TextBox hyphen = new TextBox
                            {
                                Text = "---",
                                Width = 20,
                                Height = 25,
                                BorderThickness = new Thickness(0),
                                IsReadOnly = true,
                                Background = System.Windows.Media.Brushes.Transparent,
                                TextAlignment = TextAlignment.Center,
                                Tag = "hyphen",
                            };

                            rowSP.Children.Add(hyphen);
                        }
                        else if (column < 7) // dont want to add hyphen for last column
                        {
                            TextBox hyphen = new TextBox
                            {
                                Text = "-",
                                Width = 7,
                                Height = 25,
                                BorderThickness = new Thickness(0),
                                IsReadOnly = true,
                                Background = System.Windows.Media.Brushes.Transparent,
                                TextAlignment = TextAlignment.Center,
                                Tag = "hyphen",
                            };

                            rowSP.Children.Add(hyphen);
                        }
                    }

                    bool rowIsEmpty = true;

                    foreach (var child in rowSP.Children)
                    {
                        if (child.GetType() == typeof(TextBox))
                        {
                            TextBox tb = child as TextBox;

                            if (tb.Text != "XXXX" && tb.Tag.ToString() == "IO")
                            {
                                rowIsEmpty = false;
                            }
                        }
                    }

                    if (!rowIsEmpty)
                    {
                        window.IOInfoSP.Children.Add(rowSP);
                    }
                }
            }
        }

        public override void Generate_Headers()
        {
            window.HeaderSP.Children.Clear();

            List<string> calls = new List<string>();

            string file = content.file;

            if (ncBoard == "NO") //Exclude ELIGI: if NC board is set
            {
                //ELIGI: Front Car Calls
                for (int x = 0; x < 8; x++)
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
                for (int x = 0; x < 8; x++)
                {
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("ELIGI:", x + 9, 0, b) == "YES")
                        {
                            int callNum = 100 + x * 8 + (3 - b) + 1;
                            calls.Add(callNum.ToString() + "R");
                        }
                    }
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("ELIGI:", x + 9, 1, b) == "YES")
                        {
                            int callNum = 100 + x * 8 + (3 - b) + 5;
                            calls.Add(callNum.ToString() + "R");
                        }
                    }
                }
            }

            //ELIGI: Front Down Hall Calls
            for (int x = 0; x < 8; x++)
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
            for (int x = 0; x < 8; x++)
            {
                for (int b = 3; b >= 0; b--)
                {
                    if (content.Get_Bit("ELIGI:", x + 25, 0, b) == "YES")
                    {
                        int callNum = 500 + x * 8 + (3 - b) + 1;
                        calls.Add(callNum.ToString() + "R");
                    }
                }
                for (int b = 3; b >= 0; b--)
                {
                    if (content.Get_Bit("ELIGI:", x + 25, 1, b) == "YES")
                    {
                        int callNum = 500 + x * 8 + (3 - b) + 5;
                        calls.Add(callNum.ToString() + "R");
                    }
                }
            }

            //ELIGI: Front Up Hall Calls
            for (int x = 0; x < 8; x++)
            {
                for (int b = 3; b >= 0; b--)
                {
                    if (content.Get_Bit("ELIGI:", x + 33, 0, b) == "YES")
                    {
                        int callNum = 600 + x * 8 + (3 - b) + 1;
                        calls.Add(callNum.ToString());
                    }
                }
                for (int b = 3; b >= 0; b--)
                {
                    if (content.Get_Bit("ELIGI:", x + 33, 1, b) == "YES")
                    {
                        int callNum = 600 + x * 8 + (3 - b) + 5;
                        calls.Add(callNum.ToString());
                    }
                }
            }

            //ELIGI: Rear Up Hall Calls
            for (int x = 0; x < 8; x++)
            {
                for (int b = 3; b >= 0; b--)
                {
                    if (content.Get_Bit("ELIGI:", x + 41, 0, b) == "YES")
                    {
                        int callNum = 600 + x * 8 + (3 - b) + 1;
                        calls.Add(callNum.ToString() + "R");
                    }
                }
                for (int b = 3; b >= 0; b--)
                {
                    if (content.Get_Bit("ELIGI:", x + 41, 1, b) == "YES")
                    {
                        int callNum = 600 + x * 8 + (3 - b) + 5;
                        calls.Add(callNum.ToString() + "R");
                    }
                }
            }

            if (content.content.IndexOf("XELIGI:") != -1)
            {
                //XELIGI: Front Car Calls
                for (int x = 0; x < 8; x++)
                {
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("XELIGI:", x + 1, 0, b) == "YES")
                        {
                            int callNum = 100 + x * 8 + (3 - b) + 1;
                            calls.Add(callNum.ToString() + "X");
                        }
                    }
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("XELIGI:", x + 1, 1, b) == "YES")
                        {
                            int callNum = 100 + x * 8 + (3 - b) + 5;
                            calls.Add(callNum.ToString() + "X");
                        }
                    }
                }

                //XELIGI: Rear Car Calls
                for (int x = 0; x < 8; x++)
                {
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("XELIGI:", x + 9, 0, b) == "YES")
                        {
                            int callNum = 100 + x * 8 + (3 - b) + 1;
                            calls.Add(callNum.ToString() + "R" + "X");
                        }
                    }
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("XELIGI:", x + 9, 1, b) == "YES")
                        {
                            int callNum = 100 + x * 8 + (3 - b) + 5;
                            calls.Add(callNum.ToString() + "R" + "X");
                        }
                    }
                }

                //XELIGI: Front Down Hall Calls
                for (int x = 0; x < 8; x++)
                {
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("XELIGI:", x + 17, 0, b) == "YES")
                        {
                            int callNum = 500 + x * 8 + (3 - b) + 1;
                            calls.Add(callNum.ToString() + "X");
                        }
                    }
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("XELIGI:", x + 17, 1, b) == "YES")
                        {
                            int callNum = 500 + x * 8 + (3 - b) + 5;
                            calls.Add(callNum.ToString() + "X");
                        }
                    }
                }

                //XELIGI: Rear Down Hall Calls
                for (int x = 0; x < 8; x++)
                {
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("XELIGI:", x + 25, 0, b) == "YES")
                        {
                            int callNum = 500 + x * 8 + (3 - b) + 1;
                            calls.Add(callNum.ToString() + "R" + "X");
                        }
                    }
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("XELIGI:", x + 25, 1, b) == "YES")
                        {
                            int callNum = 500 + x * 8 + (3 - b) + 5;
                            calls.Add(callNum.ToString() + "R" + "X");
                        }
                    }
                }

                //XELIGI: Front Up Hall Calls
                for (int x = 0; x < 8; x++)
                {
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("XELIGI:", x + 33, 0, b) == "YES")
                        {
                            int callNum = 600 + x * 8 + (3 - b) + 1;
                            calls.Add(callNum.ToString() + "X");
                        }
                    }
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("XELIGI:", x + 33, 1, b) == "YES")
                        {
                            int callNum = 600 + x * 8 + (3 - b) + 5;
                            calls.Add(callNum.ToString() + "X");
                        }
                    }
                }

                //XELIGI: Rear Up Hall Calls
                for (int x = 0; x < 8; x++)
                {
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("XELIGI:", x + 41, 0, b) == "YES")
                        {
                            int callNum = 600 + x * 8 + (3 - b) + 1;
                            calls.Add(callNum.ToString() + "R" + "X");
                        }
                    }
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("XELIGI:", x + 41, 1, b) == "YES")
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
                for (int x = 0; x < 8; x++)
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
                for (int x = 0; x < 8; x++)
                {
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("HELIGI:", x + 9, 0, b) == "YES")
                        {
                            int callNum = x * 8 + (3 - b) + 1;
                            calls.Add("EC" + callNum.ToString() + "R");
                        }
                    }
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("HELIGI:", x + 9, 1, b) == "YES")
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

            if (content.content.IndexOf("FSECUR") != -1)
            {


                //FSECUR: Per Opening Security Input Eligibility Map
                for (int x = 0; x < 8; x++)
                {
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("FSECUR:", x + 1, 0, b) == "YES")
                        {
                            int callNum = x * 8 + (3 - b) + 1;
                            calls.Add("S" + callNum.ToString());
                        }
                    }
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("FSECUR:", x + 1, 1, b) == "YES")
                        {
                            int callNum = x * 8 + (3 - b) + 5;
                            calls.Add("S" + callNum.ToString());
                        }
                    }
                }
            }

            if (content.content.IndexOf("RSECUR:") != -1)
            {
                //RSECUR: Per Opening Security Input Eligibility Map
                for (int x = 0; x < 8; x++)
                {
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("RSECUR:", x + 1, 0, b) == "YES")
                        {
                            int callNum = x * 8 + (3 - b) + 1;
                            calls.Add("S" + callNum.ToString() + "R");
                        }
                    }
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("RSECUR:", x + 1, 1, b) == "YES")
                        {
                            int callNum = x * 8 + (3 - b) + 5;
                            calls.Add("S" + callNum.ToString() + "R");
                        }
                    }
                }
            }

            if (content.content.IndexOf("CARDRF:") != -1)
            {
                //CARDRF: Front Card Reader Calls
                for (int x = 0; x < 8; x++)
                {
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("CARDRF:", x + 1, 0, b) == "YES")
                        {
                            int callNum = x * 8 + (3 - b) + 1;
                            calls.Add("CR" + callNum.ToString());
                        }
                    }
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("CARDRF:", x + 1, 1, b) == "YES")
                        {
                            int callNum = x * 8 + (3 - b) + 5;
                            calls.Add("CR" + callNum.ToString());
                        }
                    }
                }
            }

            if (content.content.IndexOf("CARDRR:") != -1)
            {
                //CARDRR: Rear Card Reader Calls
                for (int x = 0; x < 8; x++)
                {
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("CARDRR:", x + 1, 0, b) == "YES")
                        {
                            int callNum = x * 8 + (3 - b) + 1;
                            calls.Add("CR" + callNum.ToString() + "R");
                        }
                    }
                    for (int b = 3; b >= 0; b--)
                    {
                        if (content.Get_Bit("CARDRR:", x + 1, 1, b) == "YES")
                        {
                            int callNum = x * 8 + (3 - b) + 5;
                            calls.Add("CR" + callNum.ToString() + "R");
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
