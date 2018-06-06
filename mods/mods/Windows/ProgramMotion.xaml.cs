using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CanUsbInterface;
using CanUsbComponent;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.IO;
using System.Windows.Threading;

namespace mods
{
    /// <summary>
    /// Interaction logic for ProgramMotion.xaml
    /// </summary>
    public partial class ProgramMotion : Window
    {
        CanUsbComponentClass canPort;
        Content content;
        List<string> sorted_inputs = new List<string>();
        List<string> options = new List<string>();
        List<string> values = new List<string>();

        private Dictionary<string, int> CanLength = new Dictionary<string, int>
        {
            {"115", 6},
            {"116", 6},
            {"11B", 8},

            {"121", 5},
            {"127", 5},
            {"128", 6},
            {"12D", 5},

            {"141", 4},
            {"142", 3},
            {"146", 4},
            {"147", 3},

            {"151", 2},

            {"191", 2},
            {"193", 2},
            {"195", 2},
            {"197", 2},
            {"199", 2},
            {"19B", 2},

            {"1FD", 2 },
        };

        private Dictionary<string, string> CanData = new Dictionary<string, string>
        {
            {"115", "00 00 D0 05 00 00" },
            {"116", "00 00 00 00 C6 03"},
            {"11B", "00 00 00 00 00 00 2F 01"},

            {"121", "00 00 7C 00 00"},
            {"127", "00 00 00 0B 00"},
            {"128", "00 00 00 00 00 00"},
            {"12D", "00 00 00 0B 00"},

            {"141", "00 00 34 01"},
            {"142", "00 00 30"},
            {"146", "00 00 90 02"},
            {"147", "00 00 00"},

            {"151", "00 00"},

            {"191", "00 00"},
            {"193", "00 00"},
            {"195", "00 00"},
            {"197", "00 00"},
            {"199", "00 00"},
            {"19B", "00 00"},

            {"1FD", "00 00"},
        };        

        public ProgramMotion()
        {
            InitializeComponent();
            Window_Load();
        }

        private void Window_Load()
        {
            canPort = new CanUsbComponentClass();

            SoftwareSource.Text = Properties.Settings.Default.PMSoftwareSource;

            if (SoftwareSource.Text != "")
            {
                content = new Content(SoftwareSource.Text + @"\MPU\MPU_B\DEBUG1.ASM");
            }

            // Populate USB Ports
            fillSerialNumberComboBox();
            Basic_Features_Default();
            Setup_Spare_Input_SP();
            Setup_Spare_Output_SP();

            Available_Spare_Inputs();
        }

        private void Browse_Source_Click(object sender, RoutedEventArgs e)
        {
            using (var fbd = new System.Windows.Forms.FolderBrowserDialog())
            {
                fbd.SelectedPath = @"\\mceshared\Shared\Software\MOTION_LINE";

                System.Windows.Forms.DialogResult result = fbd.ShowDialog();

                if(result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    SoftwareSource.Text = fbd.SelectedPath;
                    Properties.Settings.Default.PMSoftwareSource = fbd.SelectedPath;
                    Properties.Settings.Default.Save();
                    content = new Content(fbd.SelectedPath + @"\MPU\MPU_B\DEBUG1.ASM");
                }
            }
        }

        private string Get_LCD_String()
        {
            string[] ID600 = new string[] { "", "", "", "", "", "" };
            string LCD_String = "";

            if (canPort.State == AdaptorState.CanBusOpen)
            {
                // We are open. Get the messages
                CanMessage msgcan;
                while (Empty_Cell(ID600))
                {
                    canPort.GetCanMessage(out msgcan, 0);
                    string rawData = "";
                    try
                    {
                        rawData = msgcan.ToString();
                    }
                    catch {}

                    if(rawData.Contains("ID:600"))
                    {
                        int dataIndex = rawData.IndexOf("Data:");
                        string data = rawData.Substring(dataIndex + 5, rawData.Length - dataIndex - 5);

                        int byteIndex = Int32.Parse(data.Substring(3, 1));
                        string ascii = "";

                        for (int i = 4; i < data.Length; i += 2)
                        {
                            int charInt = Int16.Parse(data.Substring(i, 2), NumberStyles.AllowHexSpecifier);
                            ascii += (char)charInt;
                        }

                        ID600[byteIndex] = ascii;
                    }
                }

                for (int s = 0; s < 5; s++)
                {
                    LCD_String += ID600[s];
                }
            }

            return LCD_String;
        }

        private bool Empty_Cell(string[] array)
        {
            bool emptyCell = false;
            for (int s = 0; s < 5; s++)
            {
                if (array[s] == "")
                {
                    emptyCell = true;
                }
            }

            return emptyCell;
        }

        private void Setup_Spare_Input_SP()
        {
            int spinNum = 1;

            StackPanel ctl = new StackPanel { Orientation = Orientation.Vertical };

            StackPanel ctlTitleSP = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 10, 0, 10),
                Tag = "Title",
            };

            Label ctlLabel = new Label
            {
                Width = 60,
                Content = "Type:",
                HorizontalContentAlignment = HorizontalAlignment.Right,
            };

            ComboBox ctlCB = new ComboBox
            {
                Width = 75,
            };

            ctlCB.Items.Add("CTL");
            ctlCB.SelectedIndex = 0;

            ctlTitleSP.Children.Add(ctlLabel);
            ctlTitleSP.Children.Add(ctlCB);
            ctl.Children.Add(ctlTitleSP);

            for (int i = 1; i <= 10; i++)
            {
                StackPanel sp = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                };

                Label label = new Label
                {
                    Content = "SPIN" + i,
                    Width = 60,
                    HorizontalContentAlignment = HorizontalAlignment.Right,
                };

                TextBox tb = new TextBox
                {
                    Width = 75,
                    Text = "NOT USED",
                    Tag = "spare_in" + spinNum,
                };

                spinNum++;

                sp.Children.Add(label);
                sp.Children.Add(tb);

                ctl.Children.Add(sp);
            }

            SpareInputsSP.Children.Add(ctl);

            for (int b = 0; b < 5; b++)
            {
                StackPanel uio = new StackPanel { Orientation = Orientation.Vertical };

                StackPanel uioTitleSP = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 10, 0, 10),
                    Tag = "Title",
                };

                Label uioLabel = new Label
                {
                    Width = 50,
                    Content = "Type:",
                    HorizontalContentAlignment = HorizontalAlignment.Right,
                };

                ComboBox uioCB = new ComboBox
                {
                    Width = 75,
                };

                uioCB.Items.Add("UIO");
                uioCB.Items.Add("CPI F");
                uioCB.Items.Add("CPI FX");
                uioCB.Items.Add("CPI R");
                uioCB.Items.Add("COP2 F");
                uioCB.Items.Add("COP2 R");
                uioCB.SelectedIndex = 0;

                uioCB.SelectionChanged += Board_Type_Selection_Changed;

                uioTitleSP.Children.Add(uioLabel);
                uioTitleSP.Children.Add(uioCB);
                uio.Children.Add(uioTitleSP);

                for (int i = 1; i <= 8; i++)
                {
                    StackPanel sp = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                    };

                    Label label = new Label
                    {
                        Content = "IO " + i,
                        Width = 50,
                        HorizontalContentAlignment = HorizontalAlignment.Right,
                    };

                    TextBox tb = new TextBox
                    {
                        Width = 75,
                        Text = "NOT USED",
                        Tag = "spare_in" + spinNum,
                    };

                    spinNum++;

                    sp.Children.Add(label);
                    sp.Children.Add(tb);

                    uio.Children.Add(sp);
                }

                SpareInputsSP.Children.Add(uio);
            }
        }

        private void Setup_Spare_Output_SP()
        {
            int spoutNum = 1;

            StackPanel ctl = new StackPanel { Orientation = Orientation.Vertical };

            StackPanel ctlTitleSP = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 10, 0, 10),
                Tag = "Title",
            };

            Label ctlLabel = new Label
            {
                Width = 60,
                Content = "Type:",
                HorizontalContentAlignment = HorizontalAlignment.Right,
            };

            ComboBox ctlCB = new ComboBox
            {
                Width = 75,
            };

            ctlCB.Items.Add("CTL");
            ctlCB.SelectedIndex = 0;

            ctlTitleSP.Children.Add(ctlLabel);
            ctlTitleSP.Children.Add(ctlCB);
            ctl.Children.Add(ctlTitleSP);

            for (int i = 1; i <= 4; i++)
            {
                StackPanel sp = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                };

                Label label = new Label
                {
                    Content = "SPOUT" + i,
                    Width = 60,
                    HorizontalContentAlignment = HorizontalAlignment.Right,
                };

                TextBox tb = new TextBox
                {
                    Width = 75,
                    Text = "NOT USED",
                    Tag = "spare_ou" + spoutNum,
                };

                spoutNum++;

                sp.Children.Add(label);
                sp.Children.Add(tb);

                ctl.Children.Add(sp);
            }

            SpareOutputSP.Children.Add(ctl);

            for (int b = 0; b < 5; b++)
            {
                StackPanel uio = new StackPanel { Orientation = Orientation.Vertical };

                StackPanel uioTitleSP = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 10, 0, 10),
                    Tag = "Title",
                };

                Label uioLabel = new Label
                {
                    Width = 50,
                    Content = "Type:",
                    HorizontalContentAlignment = HorizontalAlignment.Right,
                };

                ComboBox uioCB = new ComboBox
                {
                    Width = 75,
                };

                uioCB.Items.Add("UIO");
                uioCB.Items.Add("CPI F");
                uioCB.Items.Add("CPI FX");
                uioCB.Items.Add("CPI R");
                uioCB.Items.Add("COP2 F");
                uioCB.Items.Add("COP2 R");
                uioCB.SelectedIndex = 0;

                uioCB.SelectionChanged += Board_Type_Selection_Changed;

                uioTitleSP.Children.Add(uioLabel);
                uioTitleSP.Children.Add(uioCB);
                uio.Children.Add(uioTitleSP);

                for (int i = 9; i <= 16; i++)
                {
                    StackPanel sp = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                    };

                    Label label = new Label
                    {
                        Content = "IO " + i,
                        Width = 50,
                        HorizontalContentAlignment = HorizontalAlignment.Right,
                    };

                    TextBox tb = new TextBox
                    {
                        Width = 75,
                        Text = "NOT USED",
                        Tag = "spare_ou" + spoutNum,
                    };

                    spoutNum++;

                    sp.Children.Add(label);
                    sp.Children.Add(tb);

                    uio.Children.Add(sp);
                }

                SpareOutputSP.Children.Add(uio);
            }
        }

        private void Board_Type_Selection_Changed(object sender, RoutedEventArgs e)
        {
            ComboBox senderCB = sender as ComboBox;

            StackPanel parentSP = senderCB.Parent as StackPanel;

            StackPanel grandParentSP = parentSP.Parent as StackPanel;
            
            int ioNum = 0;
            string ioText = "";

            if(grandParentSP.Name == "SpareInputSP")
            {
                if (senderCB.SelectedIndex == 0 || senderCB.SelectedIndex == 4 || senderCB.SelectedIndex == 5)
                {
                    ioNum = 1;
                    ioText = "IO ";
                }
                else if (senderCB.SelectedIndex == 1 || senderCB.SelectedIndex == 3)
                {
                    ioNum = 10;
                    ioText = "I";
                }
                else if (senderCB.SelectedIndex == 2)
                {
                    ioNum = 1;
                    ioText = "I ";
                }
            }
            else
            {
                if (senderCB.SelectedIndex == 0)
                {
                    ioNum = 9;
                    ioText = "IO ";
                }
                else if (senderCB.SelectedIndex == 1 || senderCB.SelectedIndex == 3)
                {
                    ioNum = 10;
                    ioText = "O";
                }
                else if (senderCB.SelectedIndex == 2)
                {
                    ioNum = 1;
                    ioText = "O";
                }
                else if(senderCB.SelectedIndex == 4 || senderCB.SelectedIndex == 5)
                {
                    ioNum = 13;
                    ioText = "IO ";
                }
            }
            foreach (StackPanel spChild in grandParentSP.Children)
            {
                if(spChild.Tag == null)
                {
                    foreach (var child in spChild.Children)
                    {
                        if (child.GetType() == typeof(Label))
                        {
                            Label label = child as Label;
                            label.Content = ioText + ioNum;
                            ioNum++;
                        }
                    }
                }
            }
        }

        private void Available_Spare_Inputs()
        {
            List<string> alpha_inputs = Get_IO_String_List("ALPHA_INPUTS:", content.content);
            List<string> available_inputs = Get_IO_String_List("SPDISPLAY:", content.content);
            this.sorted_inputs = Alpha_Sort_List(available_inputs, alpha_inputs);
        }

        private List<string> Get_IO_String_List(string label, List<string> content)
        {
            List<string> bytes = new List<string>();

            int index = content.FindIndex(x => x.StartsWith(label));

            if (index != -1)
            {
                int offset = 1;

                while (!content[index + offset].EndsWith(":"))
                {
                    if (content[index + offset].StartsWith("DB"))
                    {
                        string value = General.Remove_Prefix(content[index + offset], "DB").Trim();

                        int startString = value.IndexOf('\'');
                        int endString = value.IndexOf('\'', startString + 1);

                        if (startString == -1)
                        {
                            bytes.Add(value);
                        }
                        else
                        {
                            value = value.Substring(startString + 1, endString - startString - 1).Trim();

                            bytes.Add(value);
                        }
                    }

                    offset++;
                }
            }

            return bytes;
        }

        private List<string> Alpha_Sort_List(List<string> list, List<string> alphaList)
        {
            List<string> sorted_list = new List<string>();
            int offset = 0;

            sorted_list.Add(list[0]);

            for(int item = 1; item < alphaList.Count; item++)
            {
                int alphaOffset = Int32.Parse(alphaList[item]);

                offset += alphaOffset;

                sorted_list.Add(list[offset]);
            }

            return sorted_list;
        }

        private void Basic_Features_Default()
        {
            ControllerType.Items.Add("TRACTION (M4000)");
            ControllerType.Items.Add("HYDRO (M2000)");
            ControllerType.SelectedIndex = 0;

            Duplex.Items.Add("DUPLEX");
            Duplex.Items.Add("LOCAL");
            Duplex.Items.Add("SIMPLEX");
            Duplex.SelectedIndex = 0;

            for (int i = 2; i <= 32; i++)
            {
                TopLanding.Items.Add(i.ToString());
            }
            TopLanding.SelectedIndex = 0;

            HCRDR.Items.Add("NO");
            HCRDR.Items.Add("YES");
            HCRDR.SelectedIndex = 0;
        }

        private void fillSerialNumberComboBox()
        {
            CanUSB.Items.Clear();
            // Populate USB Ports
            string[] adaptors = canPort.AdaptorSerialNumbers;
            if (adaptors.Length >= 1)
            {
                // Then we have found CANUSB adaptors. Add them to the dropdown and Select the first one
                foreach (string serialnumber in adaptors)
                {
                    CanUSB.Items.Add(serialnumber);
                }
                CanUSB.SelectedIndex = 0;
            }
            else
            {
                // No adaptors have been found
                CanUSB.Items.Add("NoneFound");
            }
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (canPort.State == AdaptorState.Closed)
            {
                while(canPort.State == AdaptorState.Closed)
                {
                    // The connection is closed. Set the controls accordingly
                    ConnectButton.Content = "Open";
                    // Now extract the CAN settings
                    ECanBps baud = ECanBps.Baud500kBps;
                    // Serial number
                    string serialnum = CanUSB.SelectedItem.ToString();
                    // Open the port..
                    try
                    {
                        if (canPort.OpenCanBus(serialnum, baud))
                        //if (canPort.OpenCanBus(serialnum, baud, 0xF000, 0x0FFF))
                        //if (canPort.OpenCanBus("", baud))
                        {
                            // It has been opened
                            ConnectButton.Content = "Close";
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                }
            }
            else
            {
                // The port is open, try to close it.
                ConnectButton.Content = "Close";
                try
                {
                    if (canPort.CloseCanBus())
                    {
                        ConnectButton.Content = "Open";
                    }
                }
                catch (Exception ex) { MessageBox.Show(ex.ToString()); }
            }
        }

        private void Send_Can_Data()
        {
            foreach (KeyValuePair<string, string> entry in CanData)
            {
                string str_id = entry.Key;
                string str_data = entry.Value;
                int length = CanLength[str_id];

                UInt32 id;
                if (str_id != "")
                {
                    id = Convert.ToUInt32(str_id, 16);
                }
                else
                {
                    id = 0;
                }

                UInt64 data;
                if (str_data != "")
                {
                    str_data = str_data.Replace(" ", "");
                    data = Convert.ToUInt64(str_data, 16);
                }
                else
                {
                    data = 0;
                }
                CanMessage canMsg = new CanMessage(CanMode.CAN2A, false, id, length, data);
                if (canPort.State == AdaptorState.CanBusOpen)
                {
                    canPort.SendCanMessage(canMsg);
                }
            }
        }

        private void Default_Parameters()
        {
            //Default Controller Parameters
            CanData["1FD"] = "55 00";
            Send_Can_Data();

            Thread.Sleep(1000);

            CanData["1FD"] = "55 0F";
            Send_Can_Data();

            Thread.Sleep(1000);
        }

        private void Program_Click(object sender, RoutedEventArgs e)
        {
            if(canPort.State == AdaptorState.Closed)
            {
                MessageBox.Show("Please Open a CAN Port");
            }
            else if(SoftwareSource.Text == "")
            {
                MessageBox.Show("Please select a software source directory");
            }
            else if(!Verify_IO())
            {
                MessageBox.Show("One or more Spare Inputs/Outputs are not available in this software version");
            }
            else
            {
                //Default Datastream
                CanData["116"] = "00 00 00 00 C7 03";
                CanData["11B"] = "00 00 20 00 00 00 2F 01";
                CanData["127"] = "00 00 EC 7B 00";
                CanData["128"] = "00 00 00 00 01 71";
                CanData["12D"] = "00 00 EC 0B 00";

                //Enter Inspection
                CanData["116"] = "00 00 00 00 C7 02";
                Send_Can_Data();

                Thread.Sleep(1000);

                //Default Parameters
                if (System.Windows.Forms.MessageBox.Show("Default Parameters?", "Default?", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    Default_Parameters();
                }

                //F1 Switch To enter Program Mode
                CanData["1FD"] = "01 00";
                Send_Can_Data();

                Thread.Sleep(1000);

                //Press N to Begin
                Button_Press("N");

                Wait_For_Top_Menu("SPARE INPUTS");

                Button_Press("S");

                Spare_Inputs();

                Wait_For_Top_Menu("SAVE CHANGES");

                //Press S to Save
                Button_Press("S");
            }
        }

        private void Double_Check_IO(string io)
        {
            bool match = false;

            while (!match)
            {
                string lcd = "";

                Stopwatch stopwatch = new Stopwatch();

                stopwatch.Start();

                while (stopwatch.Elapsed.Seconds < 1)
                {
                    lcd = Get_LCD_String();
                }

                int colonIndex = lcd.IndexOf(":");

                string spareIOstring = lcd.Substring(colonIndex + 1, lcd.Length - colonIndex - 1).Trim();

                if (spareIOstring == io)
                {
                    match = true;
                }
                else
                {
                    Button_Press("S", true);
                }
            }
        }

        private void Double_Check_IO_Slot(string slot)
        {
            bool match = false;

            while (!match)
            {
                string lcd = "";

                Stopwatch stopwatch = new Stopwatch();

                stopwatch.Start();

                while (stopwatch.Elapsed.Seconds < 1)
                {
                    lcd = Get_LCD_String();
                }

                if (lcd.Contains(slot))
                {
                    match = true;
                }
                else
                {
                    int onIndex = lcd.IndexOf("ON");

                    string lcd_slot = lcd.Substring(0, onIndex).Trim();

                    if(Get_IO_Slot_Int(lcd_slot) > Get_IO_Slot_Int(slot))
                    {
                        Button_Press("N", true);
                    }
                    else
                    {
                        Button_Press("N");
                    }
                }
            }
        }

        private int Get_IO_Slot_Int(string io)
        {
            string intBuilder = "";
            
            for(int i = 0; i < io.Length; i++)
            {
                if(Char.IsDigit(io[i]))
                {
                    intBuilder += io[i];
                }
            }

            return Int32.Parse(intBuilder);
        }

        private void Wait_For_Top_Menu(string menuString)
        {
            bool match = false;

            while (!match)
            {
                string lcd = "";

                Stopwatch stopwatch = new Stopwatch();

                stopwatch.Start();

                while (stopwatch.Elapsed.Seconds < 1)
                {
                    lcd = Get_LCD_String();
                }
                

                if (lcd.Contains(menuString))
                {
                    match = true;
                }
                else
                {
                    Button_Press("N");
                }
            }
        }

        private void Wait_For_IO_Rapid(string waitString, string button)
        {
            string buttonData = "";

            if (button == "S")
            {
                buttonData = "01 04";
            }
            else if(button == "N")
            {
                buttonData = "01 08";
            }
            
            CanData["1FD"] = buttonData;

            bool match = false;

            while (!match)
            {
                string lcd = Get_LCD_String();

                int colonIndex = lcd.IndexOf(":");

                string spareIOstring = lcd.Substring(colonIndex + 1, lcd.Length - colonIndex - 1).Trim();

                if (spareIOstring == waitString)
                {
                    match = true;
                }
                else
                {
                    Send_Can_Data();
                }
            }

            CanData["1FD"] = "01 00";
            Send_Can_Data();

            Thread.Sleep(1000);
        }

        private void Button_Press(string button, bool reverse = false)
        {
            string buttonData = "";

            if (button == "S")
            {
                buttonData = "01 04";
            }
            else if (button == "N")
            {
                buttonData = "01 08";
            }

            if(reverse)
            {
                if (button == "S")
                {
                    buttonData = "01 05";
                }
                else if (button == "N")
                {
                    buttonData = "01 09";
                }
            }
            
            CanData["1FD"] = buttonData;
            Send_Can_Data();

            Thread.Sleep(200);

            CanData["1FD"] = "01 00";
            Send_Can_Data();

            Thread.Sleep(600);
        }

        private void Spare_Inputs()
        {
            foreach(StackPanel sp in SpareInputsSP.Children)
            {
                foreach(StackPanel spChild in sp.Children)
                {
                    if (spChild.Tag == null)
                    {
                        foreach (var child in spChild.Children)
                        {
                            string labelContent = "";

                            if (child.GetType() == typeof(Label))
                            {
                                Label label = child as Label;
                                labelContent = label.Content.ToString();
                            }

                            if (child.GetType() == typeof(TextBox))
                            {
                                Double_Check_IO_Slot(labelContent);

                                TextBox tb = child as TextBox;

                                string spareInput = tb.Text;

                                if (spareInput == "")
                                {
                                    spareInput = "NOT USED";
                                }

                                if (spareInput != "NOT USED")
                                {
                                    Wait_For_IO_Rapid(spareInput,"S");
                                    Double_Check_IO(spareInput);
                                }

                                //Press N to go to next option
                                CanData["1FD"] = "01 08";
                                Send_Can_Data();

                                Thread.Sleep(200);

                                //Toggle Off N
                                CanData["1FD"] = "01 00";
                                Send_Can_Data();

                                Thread.Sleep(200);
                            }
                        }
                    }
                }
            }
        }

        private void Spare_Outputs()
        {
            foreach (StackPanel sp in SpareOutputSP.Children)
            {
                foreach (StackPanel spChild in sp.Children)
                {
                    if (spChild.Tag == null)
                    {
                        foreach (var child in spChild.Children)
                        {
                            string labelContent = "";

                            if (child.GetType() == typeof(Label))
                            {
                                Label label = child as Label;
                                labelContent = label.Content.ToString();
                            }

                            if (child.GetType() == typeof(TextBox))
                            {
                                Double_Check_IO_Slot(labelContent);

                                TextBox tb = child as TextBox;

                                string spareInput = tb.Text;

                                if (spareInput == "")
                                {
                                    spareInput = "NOT USED";
                                }

                                if (spareInput != "NOT USED")
                                {
                                    Wait_For_IO_Rapid(spareInput, "S");
                                    Double_Check_IO(spareInput);
                                }

                                //Press N to go to next option
                                CanData["1FD"] = "01 08";
                                Send_Can_Data();

                                Thread.Sleep(200);

                                //Toggle Off N
                                CanData["1FD"] = "01 00";
                                Send_Can_Data();

                                Thread.Sleep(200);
                            }
                        }
                    }
                }
            }
        }

        private void Close(object sender, System.ComponentModel.CancelEventArgs e)
        {
            canPort.CloseCanBus();
        }

        private void Option_Select(ComboBox cb)
        {
            for (int c = 0; c < cb.SelectedIndex; c++)
            {
                //Press S to go to next choice
                CanData["1FD"] = "01 04";
                Send_Can_Data();

                //Toggle Off S
                CanData["1FD"] = "01 00";
                Send_Can_Data();
            }

            //Press N to go to next option
            CanData["1FD"] = "01 08";
            Send_Can_Data();

            //Toggle Off N
            CanData["1FD"] = "01 00";
            Send_Can_Data();
        }

        private void ImportPRButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".txt";

            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();


            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                byte[] fileBytes = File.ReadAllBytes(dlg.FileName);
                string hex = BitConverter.ToString(fileBytes).Replace("-", string.Empty);

                List<string> optionList = new List<string>();
                List<string> englishOptions = new List<string>();

                int inc = 0;
                int findIndex = 0;

                //Gather Options
                while (hex.IndexOf("28F412", inc) != -1)
                {
                    findIndex = hex.IndexOf("28F412", inc);

                    bool build = true;
                    int index = findIndex - 26;
                    string optionString = "";
                    int characterCount = 0;

                    while (build)
                    {
                        string asciiCode = hex.Substring(index, 2);
                        int asciiDec = General.HexStringToDecimal(asciiCode);

                        if (asciiDec >= 32 && asciiDec <= 122)
                        {
                            characterCount++;
                            index -= 2;
                        }
                        else
                        {
                            build = false;
                            index += 2;
                        }
                    }

                    for (int c = 0; c < characterCount; c++)
                    {
                        optionString += hex.Substring(index, 2);
                        index += 2;
                    }

                    optionList.Add(optionString);

                    inc = findIndex + 2;
                }


                foreach (string option in optionList)
                {
                    string english = "";
                    for (int i = 0; i < option.Length; i += 2)
                    {
                        int charInt = Int16.Parse(option.Substring(i, 2), NumberStyles.AllowHexSpecifier);
                        english += (char)charInt;
                    }

                    englishOptions.Add(english);
                }

                //Data Values come 104 characters after last option
                List<string> values = new List<string>();
                string valueHex = hex.Substring(findIndex + 104, hex.Length - findIndex - 104);

                string valueString = "";
                for (int i = 0; i < valueHex.Length; i = i + 2)
                {
                    string hexChar = valueHex.Substring(i, 2);

                    if (hexChar == "00")
                    {
                        values.Add(valueString);
                        valueString = "";
                    }
                    else
                    {
                        valueString += hexChar;
                    }
                }


                List<string> valuesEnglish = new List<string>();
                foreach (string value in values)
                {
                    string english = "";
                    for (int i = 0; i < value.Length; i += 2)
                    {
                        int charInt = Int16.Parse(value.Substring(i, 2), NumberStyles.AllowHexSpecifier);
                        english += (char)charInt;
                    }

                    valuesEnglish.Add(english);
                }

                this.options = englishOptions;
                this.values = valuesEnglish;

                //POPULATE FIELDS
                Populate_Spare_Inputs();
                Populate_Spare_Outputs();
            }
        }

        private void Populate_Spare_Inputs()
        {
            foreach (StackPanel sp in SpareInputsSP.Children)
            {
                foreach (StackPanel spChild in sp.Children)
                {
                    if (spChild.Tag == null)
                    {
                        foreach (var child in spChild.Children)
                        {
                            if (child.GetType() == typeof(TextBox))
                            {
                                TextBox tb = child as TextBox;

                                int option_index = options.IndexOf(tb.Tag.ToString());

                                tb.Text = values[option_index];
                            }
                        }
                    }
                }
            }
        }

        private void Populate_Spare_Outputs()
        {
            foreach (StackPanel sp in SpareOutputSP.Children)
            {
                foreach (StackPanel spChild in sp.Children)
                {
                    if (spChild.Tag == null)
                    {
                        foreach (var child in spChild.Children)
                        {
                            if (child.GetType() == typeof(TextBox))
                            {
                                TextBox tb = child as TextBox;

                                int option_index = options.IndexOf(tb.Tag.ToString());

                                tb.Text = values[option_index];
                            }
                        }
                    }
                }
            }
        }

        private bool Verify_IO()
        {
            bool pass = true;

            foreach (StackPanel sp in SpareInputsSP.Children)
            {
                foreach (StackPanel spChild in sp.Children)
                {
                    if (spChild.Tag == null)
                    {
                        foreach (var child in spChild.Children)
                        {
                            if (child.GetType() == typeof(TextBox))
                            {
                                TextBox tb = child as TextBox;
                                if(tb.Text.Trim() == "")
                                {
                                    tb.Text = "NOT USED";
                                }
                                if (!sorted_inputs.Contains(tb.Text))
                                {
                                    pass = false;
                                    tb.BorderBrush = System.Windows.Media.Brushes.Red;
                                }
                                else
                                {
                                    tb.BorderBrush = System.Windows.Media.Brushes.Black;
                                }
                            }
                        }
                    }
                }
            }

            return pass;
        }
    }
}
