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
    public abstract class Controller
    {
        public string file;
        public Content content;
        public MotionContent motioncontent;
        public MainWindow window
        {
            get
            {
                return Application.Current.Windows.OfType<MainWindow>().First();
            }
        }
        public DateTime lastModified
        {
            get
            {
                return System.IO.File.GetLastWriteTime(file);
            }
        }
        public string jobName
        {
            get
            {
                return content.Get_String("JBNAME:", 1);
            }
        }
        protected string topFloor;
        protected string topFloorDecimal;
        protected string botFloor;
        protected string botFloorDecimal;
        protected string falseFloors;
        protected string nudging;
        protected int i4o;
        protected int iox;
        protected int aiox;
        protected int callbnu;
        protected string rearDoor;
        protected string ceBoard;
        protected string ncBoard;
        protected string ftBoard;
        protected string dlmBoard;
        public abstract int topLandingHeight { get; }
        public string versionBot
        {
            get
            {
                string vb = content.Get_String("CUSTOM:", 1);
                if (vb[0] == '0' && vb.Length > 1 && vb[1] != ' ')
                {
                    vb = vb.Substring(1, 1);
                }
                return vb;
            }
        }

        public string versionTop
        {
            get
            {
                string vt = content.Get_Comma_Separated_Byte("MPVERNUM:", 1, 0);
                if (vt[0] == '0' && vt.Length > 1)
                {
                    vt = vt.Substring(1, 1);
                }
                return vt;
            }
        }

        public string versionMid
        {
            get
            {
                return content.Get_Comma_Separated_Byte("MPVERNUM:", 1, 1);
            }
        }

        public string fileVersion
        {
            get
            {
                return versionTop + "." + versionMid + "." + versionBot;
            }
        }
        public int fileVersionInt
        {
            get
            {
                try
                {
                    bool vbot_passed = Int32.TryParse(versionBot, out int vbot);
                    bool vmid_passed = Int32.TryParse(versionMid, out int vmid);
                    bool vtop_passed = Int32.TryParse(versionTop, out int vtop);

                    if (vbot_passed && vmid_passed && vtop_passed)
                    {
                        return vtop * 10000 + vmid * 100 + vbot;
                    }
                    else
                    {
                        return 0;
                    }
                }
                catch
                {
                    return 0;
                }

            }
        }
        protected string drivebit2;
        protected string drivebit3;
        protected string driveType;
        public string referenceJob;
        public string firecode
        {
            get
            {
                return Get_Fire_Code();
            }
        }

        protected abstract string Get_Fire_Code();

        protected void Initialize_Controller(string file)
        {
            if(file.ToUpper().Contains("SOFTWARE"))
            {
                this.file = file;
            }
            else
            {
                this.file = @"\\10.113.32.45\shared\Software\" + file;
            }
            content = new Content(file);
            Set_Variables();
        }

        protected void Initialize_Controller(string file, Content content)
        {
            if (file.ToUpper().Contains("SOFTWARE"))
            {
                this.file = file;
            }
            else
            {
                this.file = @"\\10.113.32.45\shared\Software\" + file;
            }
            this.content = content;
        }

        protected abstract void Set_Variables();

        public abstract void Job_Info();

        public abstract void Options();

        public void Job_Summary()
        {
            window.JobSummary.Text = "";
            List<string> jobSummary = content.Get_Job_Summary();
            foreach (string line in jobSummary)
            {
                if (line.IndexOf(";") != -1)
                {
                    window.JobSummary.Text += line.Substring(line.IndexOf(";") + 1, line.Length - line.IndexOf(";") - 1) + "\n";
                }
                else
                {
                    window.JobSummary.Text += line + "\n";
                }
            }
        }

        public virtual void Draw_Landing_Preview()
        {
            window.LandingNormalHeader.Width = 96;
            window.LandingNormalConfig.Width = 96;
            window.LandingAltHeader.Width = 96;
            window.LandingAltConfig.Width = 96;

            window.LandingNormalConfig.Text = "";
            window.LandingNormalConfig.Height = 0;
            window.LandingNormalConfig.BorderThickness = new System.Windows.Thickness(0);
            window.LandingAltConfig.Text = "";
            window.LandingAltConfig.Height = 0;
            window.LandingAltConfig.BorderThickness = new System.Windows.Thickness(0);

            window.LandingAltHeader.Visibility = Visibility.Hidden;

            int top_Landing = General.HexStringToDecimal(content.Get_Byte("BOTTOM:", 2)) + 1;
            string isFalseFloors = content.Get_Bit("CPVAR", 3, 0, 3);

            List<string> piLabels = content.Get_PILabels();

            string front = "False";
            string rear = "False";
            
            window.LandingLevels.Text = "";
            window.LandingLevels.Height = 16 * top_Landing + 10;
            window.LandingLevels.BorderThickness = new System.Windows.Thickness(2);
            window.LandingPIs.Text = "";
            window.LandingPIs.Height = 16 * top_Landing + 10;
            window.LandingPIs.BorderThickness = new System.Windows.Thickness(2);

            window.LandingNormalConfig.Text = "";
            window.LandingNormalConfig.Height = 16 * top_Landing + 10;
            window.LandingNormalConfig.BorderThickness = new System.Windows.Thickness(2);

            List<int> falseFloors = new List<int>();
            List<int> nonFalseFloors = new List<int>();

            if (isFalseFloors == "YES")
            {
                int pix_tableIndex = content.content.IndexOf("PIX_TABLE:");
                int x = 1;

                while (content.content[pix_tableIndex + x].StartsWith("DB") && content.Get_Byte("PIX_TABLE:", x) != "7F")
                {
                    string floorHex = content.Get_Byte("PIX_TABLE:", x);
                    string floorBinary = General.HexStringToBinary(floorHex);
                    int floorDec = General.HexStringToDecimal(floorHex) + 1;
                    if (floorBinary[0] == '0') //If False Floor
                    {
                        falseFloors.Add(floorDec);
                    }
                    else //Non False Floor
                    {
                        nonFalseFloors.Add(floorDec - 128);
                    }
                    x++;
                }
            }

            for (int f = top_Landing; f >= 1; f--)
            {
                if (content.Get_Bit("ELIGIV:", f, 0, 3) == "YES")
                {
                    front = "F";
                }
                else
                {
                    if (falseFloors.Contains(f))
                    {
                        front = " X";
                    }
                    else
                    {
                        front = ".";
                    }
                }

                if (content.Get_Bit("ELIGIV:", f, 0, 2) == "YES")
                {
                    rear = "R";
                }
                else
                {
                    if (falseFloors.Contains(f))
                    {
                        rear = "";
                    }
                    else
                    {
                        rear = ".";
                    }
                }

                window.LandingPIs.Text += piLabels[f - 1] + "\n";
                window.LandingLevels.Text += f + "\n";
                window.LandingNormalConfig.Text += front + " " + rear + "\n";
            }

            bool isAltInput = false;
            if (content.content.IndexOf("INELIG:") != -1)
            {
                //INELIG: System Input Eligibility Map
                List<string> inelig = content.IO(new List<string> { "INELIG" });
                foreach (string input in inelig)
                {
                    if (input == "ALT")
                    {
                        isAltInput = true;
                    }
                }
            }

            foreach (string input in content.inputs)
            {
                if (input == "ALT")
                {
                    isAltInput = true;
                }
            }


            if (isAltInput)
            {
                if (window.FilesListBox.SelectedItems.Count > 0) //This is to prevent this from being visible before a file is selected
                {
                    window.LandingAltHeader.Visibility = Visibility.Visible;
                    window.LandingAltConfig.Visibility = Visibility.Visible;
                }

                window.LandingAltConfig.Text = "";
                window.LandingAltConfig.Height = 16 * top_Landing + 10;
                window.LandingAltConfig.BorderThickness = new System.Windows.Thickness(2);

                for (int f = top_Landing; f >= 1; f--)
                {

                    if (content.Get_Bit("ALTMP:", f, 0, 3) == "YES")
                    {
                        front = "F";
                    }
                    else
                    {
                        if (falseFloors.Contains(f))
                        {
                            front = " X";
                        }
                        else
                        {
                            front = ".";
                        }
                    }

                    if (content.Get_Bit("ALTMP:", f, 0, 2) == "YES")
                    {
                        rear = "R";
                    }
                    else
                    {
                        if (falseFloors.Contains(f))
                        {
                            rear = "";
                        }
                        else
                        {
                            rear = ".";
                        }
                    }
                    window.LandingAltConfig.Text += front + " " + rear + "\n";
                }
                window.LandingAltConfig.Text = window.LandingAltConfig.Text.Substring(0, window.LandingAltConfig.Text.Length - 1);
            }

            //Remove Last new line character from each column
            window.LandingPIs.Text = window.LandingPIs.Text.Substring(0, window.LandingPIs.Text.Length - 1);
            window.LandingLevels.Text = window.LandingLevels.Text.Substring(0, window.LandingLevels.Text.Length - 1);
            window.LandingNormalConfig.Text = window.LandingNormalConfig.Text.Substring(0, window.LandingNormalConfig.Text.Length - 1);
        }

        public virtual void Generate_IO()
        {
            window.IOInfoSP.Children.Clear();

            List<string> inputs = content.inputs;
            List<string> outputs = content.outputs;

            Label inputLabel = new Label
            {
                Content = "Spare Inputs",
            };

            window.IOInfoSP.Children.Add(inputLabel);

            for (int row = 0; row < 8; row++)
            {
                StackPanel rowSP = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(20, 0, 0, 0),
                };

                if (row == 3)
                {
                    rowSP.Margin = new Thickness(20, 0, 0, 20);
                }

                for (int column = 0; column < 8; column++)
                {
                    string ioText = "";
                    if (row * 8 + (7 - column) < inputs.Count)
                    {
                        ioText = inputs[row * 8 + (7 - column)];
                    }
                    else
                    {
                        ioText = "XXXX";
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

            Label outputLabel = new Label
            {
                Content = "Spare Outputs",
            };

            window.IOInfoSP.Children.Add(outputLabel);

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
                    if (row * 8 + column < outputs.Count)
                    {
                        ioText = outputs[row * 8 + column];
                    }
                    else
                    {
                        ioText = "XXXX";
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

            Draw_Boards();
        }

        private void Draw_Boards()
        {
            window.BoardSP.Children.Clear();

            int bWidth = 384;
            int spWidth = 379;
            int tbWidth = 48;
            
            List<string> inputs = content.inputs;
            List<string> outputs = content.outputs;

            int inputRow = 0;
            int outputRow = 0;
            int inputCol = 0;
            int outputCol = 0;

            //IOX
            for (int b = 0; b < iox; b++)
            {

                Border border = new Border
                {
                    BorderBrush = System.Windows.Media.Brushes.Black,
                    BorderThickness = new Thickness(2),
                    Background = System.Windows.Media.Brushes.Transparent,
                    Margin = new Thickness(0, 0, 0, 10),
                    Width = bWidth,
                    HorizontalAlignment = HorizontalAlignment.Left
                };

                StackPanel ioxsp = new StackPanel
                {
                    Name = "ioxsp" + b,
                    Orientation = Orientation.Vertical,
                    Width = spWidth,
                    Margin = new Thickness(5, 0, 0, 0)
                };

                Label boardLabel = new Label { Content = "IOX Board # " + (b + 1) };
                Label inputLabel = new Label { Content = "Inputs:" };
                Label outputLabel = new Label { Content = "Outputs:", Margin = new Thickness(0, 23, 0, 0) };

                StackPanel inputsp1 = new StackPanel { Orientation = Orientation.Horizontal };
                StackPanel outputsp1 = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 10) };

                for (int i = 1; i <= 8; i++)
                {
                    string ioText = "";
                    if (inputRow * 8 + (7 - inputCol) < inputs.Count)
                    {
                        ioText = inputs[inputRow * 8 + (7 - inputCol)];
                    }
                    else
                    {
                        ioText = "";
                    }
                    inputsp1.Children.Add(
                        new TextBox
                        {
                            Text = ioText,
                            Width = tbWidth,
                            Height = 25,
                            BorderThickness = new Thickness(2),
                            BorderBrush = System.Windows.Media.Brushes.Black,
                            IsReadOnly = true,
                            Background = System.Windows.Media.Brushes.Transparent,
                            TextAlignment = TextAlignment.Center,
                            Margin = new Thickness(0, 0, -2, 0),
                            ToolTip = "IN" + (9 - i)
                        });
                    inputCol++;
                }

                inputCol = 0;
                inputRow++;

                for (int o = 1; o <= 8; o++)
                {
                    string ioText = "";
                    if (outputRow * 8 + outputCol < outputs.Count)
                    {
                        ioText = outputs[outputRow * 8 + outputCol];
                    }
                    else
                    {
                        ioText = "";
                    }
                    outputsp1.Children.Add(
                        new TextBox
                        {
                            Text = ioText,
                            Width = tbWidth,
                            Height = 25,
                            BorderThickness = new Thickness(2),
                            BorderBrush = System.Windows.Media.Brushes.Black,
                            IsReadOnly = true,
                            Background = System.Windows.Media.Brushes.Transparent,
                            TextAlignment = TextAlignment.Center,
                            Margin = new Thickness(0, 0, -2, 0),
                            ToolTip = "OUT" + o
                        });
                    outputCol++;
                }

                outputCol = 0;
                outputRow++;

                ioxsp.Children.Add(boardLabel);

                ioxsp.Children.Add(inputLabel);
                ioxsp.Children.Add(inputsp1);

                ioxsp.Children.Add(outputLabel);
                ioxsp.Children.Add(outputsp1);

                border.Child = ioxsp;
                window.BoardSP.Children.Add(border);
            }

            //I4O
            for (int b = 0; b < i4o; b++)
            {
                Border border = new Border
                {
                    BorderBrush = System.Windows.Media.Brushes.Black,
                    BorderThickness = new Thickness(2),
                    Background = System.Windows.Media.Brushes.Transparent,
                    Margin = new Thickness(0, 0, 0, 10),
                    Width = bWidth,
                    HorizontalAlignment = HorizontalAlignment.Left
                };

                StackPanel i4osp = new StackPanel
                {
                    Name = "i4osp" + b,
                    Orientation = Orientation.Vertical,
                    Width = spWidth,
                    Margin = new Thickness(5, 0, 0, 0)
                };

                Label boardLabel = new Label { Content = "I4O Board # " + (b + 1) };
                Label inputLabel = new Label { Content = "Inputs:" };
                Label outputLabel = new Label { Content = "Outputs:" };

                StackPanel inputsp1 = new StackPanel { Orientation = Orientation.Horizontal };
                StackPanel inputsp2 = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, -2, 0, 0) };
                StackPanel outputsp1 = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 10) };

                for (int i = 1; i <= 8; i++)
                {
                    string ioText = "";
                    if (inputRow * 8 + (7 - inputCol) < inputs.Count)
                    {
                        ioText = inputs[inputRow * 8 + (7 - inputCol)];
                    }
                    else
                    {
                        ioText = "";
                    }
                    inputsp1.Children.Add(
                        new TextBox
                        {
                            Text = ioText,
                            Width = tbWidth,
                            Height = 25,
                            BorderThickness = new Thickness(2),
                            BorderBrush = System.Windows.Media.Brushes.Black,
                            IsReadOnly = true,
                            Background = System.Windows.Media.Brushes.Transparent,
                            TextAlignment = TextAlignment.Center,
                            Margin = new Thickness(0, 0, -2, 0),
                            ToolTip = "IN" + (9 - i)
                        });

                    inputCol++;

                    if (inputCol == 8)
                    {
                        inputCol = 0;
                        inputRow++;
                    }
                }

                for (int i = 9; i <= 16; i++)
                {
                    string ioText = "";
                    if (inputRow * 8 + (7 - inputCol) < inputs.Count)
                    {
                        ioText = inputs[inputRow * 8 + (7 - inputCol)];
                    }
                    else
                    {
                        ioText = "";
                    }
                    inputsp2.Children.Add(
                        new TextBox
                        {
                            Text = ioText,
                            Width = tbWidth,
                            Height = 25,
                            BorderThickness = new Thickness(2),
                            BorderBrush = System.Windows.Media.Brushes.Black,
                            IsReadOnly = true,
                            Background = System.Windows.Media.Brushes.Transparent,
                            TextAlignment = TextAlignment.Center,
                            Margin = new Thickness(0, 0, -2, 0),
                            ToolTip = "IN" + (25 - i),
                        });

                    inputCol++;

                    if (inputCol == 8)
                    {
                        inputCol = 0;
                        inputRow++;
                    }
                }

                for (int o = 1; o <= 4; o++)
                {
                    string ioText = "";
                    if (outputRow * 8 + outputCol < outputs.Count)
                    {
                        ioText = outputs[outputRow * 8 + outputCol];
                    }
                    else
                    {
                        ioText = "";
                    }
                    outputsp1.Children.Add(
                        new TextBox
                        {
                            Text = ioText,
                            Width = tbWidth,
                            Height = 25,
                            BorderThickness = new Thickness(2),
                            BorderBrush = System.Windows.Media.Brushes.Black,
                            IsReadOnly = true,
                            Background = System.Windows.Media.Brushes.Transparent,
                            TextAlignment = TextAlignment.Center,
                            Margin = new Thickness(0, 0, -2, 0),
                            ToolTip = "OUT" + o
                        });

                    outputCol++;

                    if (outputCol == 8)
                    {
                        outputCol = 0;
                        outputRow++;
                    }
                }

                i4osp.Children.Add(boardLabel);

                i4osp.Children.Add(inputLabel);
                i4osp.Children.Add(inputsp1);
                i4osp.Children.Add(inputsp2);

                i4osp.Children.Add(outputLabel);
                i4osp.Children.Add(outputsp1);

                border.Child = i4osp;
                window.BoardSP.Children.Add(border);
            }

            //AIOX
            for (int b = 0; b < aiox; b++)
            {

                Border border = new Border
                {
                    BorderBrush = System.Windows.Media.Brushes.Black,
                    BorderThickness = new Thickness(2),
                    Background = System.Windows.Media.Brushes.Transparent,
                    Margin = new Thickness(0, 0, 0, 10),
                    Width = bWidth,
                    HorizontalAlignment = HorizontalAlignment.Left
                };

                StackPanel aioxsp = new StackPanel
                {
                    Name = "aioxsp" + b,
                    Orientation = Orientation.Vertical,
                    Width = spWidth,
                    Margin = new Thickness(5, 0, 0, 0)
                };

                Label boardLabel = new Label { Content = "AIOX Board # " + (b + 1) };
                Label inputLabel = new Label { Content = "Inputs:" };
                Label outputLabel = new Label { Content = "Outputs:", Margin = new Thickness(0, 23, 0, 0) };

                StackPanel inputsp1 = new StackPanel { Orientation = Orientation.Horizontal };
                StackPanel outputsp1 = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 10) };

                for (int i = 1; i <= 8; i++)
                {
                    string ioText = "";
                    if (inputRow * 8 + (7 - inputCol) < inputs.Count)
                    {
                        ioText = inputs[inputRow * 8 + (7 - inputCol)];
                    }
                    else
                    {
                        ioText = "";
                    }
                    inputsp1.Children.Add(
                        new TextBox
                        {
                            Text = ioText,
                            Width = tbWidth,
                            Height = 25,
                            BorderThickness = new Thickness(2),
                            BorderBrush = System.Windows.Media.Brushes.Black,
                            IsReadOnly = true,
                            Background = System.Windows.Media.Brushes.Transparent,
                            TextAlignment = TextAlignment.Center,
                            Margin = new Thickness(0, 0, -2, 0),
                            ToolTip = "IN" + (9 - i)
                        });
                    inputCol++;
                }

                inputCol = 0;
                inputRow++;

                for (int o = 1; o <= 8; o++)
                {
                    string ioText = "";
                    if (outputRow * 8 + outputCol < outputs.Count)
                    {
                        ioText = outputs[outputRow * 8 + outputCol];
                    }
                    else
                    {
                        ioText = "";
                    }
                    outputsp1.Children.Add(
                        new TextBox
                        {
                            Text = ioText,
                            Width = tbWidth,
                            Height = 25,
                            BorderThickness = new Thickness(2),
                            BorderBrush = System.Windows.Media.Brushes.Black,
                            IsReadOnly = true,
                            Background = System.Windows.Media.Brushes.Transparent,
                            TextAlignment = TextAlignment.Center,
                            Margin = new Thickness(0, 0, -2, 0),
                            ToolTip = "OUT" + o
                        });
                    outputCol++;

                    if (outputCol == 8)
                    {
                        outputCol = 0;
                        outputRow++;
                    }
                }

                outputRow++;

                aioxsp.Children.Add(boardLabel);

                aioxsp.Children.Add(inputLabel);
                aioxsp.Children.Add(inputsp1);

                aioxsp.Children.Add(outputLabel);
                aioxsp.Children.Add(outputsp1);

                border.Child = aioxsp;
                window.BoardSP.Children.Add(border);
            }
        }

        public abstract void Generate_Headers();
    }
}
