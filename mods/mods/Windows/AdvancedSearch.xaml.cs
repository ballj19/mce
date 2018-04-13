﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.IO;
using System.ComponentModel;
using System.Windows.Threading;
using Excel = Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;

namespace mods
{
    public partial class AdvancedSearch : Window
    {
        bool stopSearching = false;
        int progress = 0;

        public AdvancedSearch()
        {
            InitializeComponent();
            Default_Values();
        }

        private void Default_Values()
        {
            CustomFoldersCheckBox.IsChecked = false;

            ControllerCB.Items.Add("");
            ControllerCB.Items.Add("MP");
            ControllerCB.Items.Add("MP2");
            ControllerCB.SelectedIndex = 0;

            TypeTB.Items.Add("Local");
            TypeTB.Items.Add("Simplex");
            TypeTB.Items.Add("Group");
            TypeTB.SelectedIndex = 0;

            TopFloorCB.Items.Add("");
            TopFloorCB.Items.Add("=");
            TopFloorCB.Items.Add(">");
            TopFloorCB.Items.Add("<");
            TopFloorCB.SelectedIndex = 0;

            BottomFloorCB.Items.Add("");
            BottomFloorCB.Items.Add("=");
            BottomFloorCB.Items.Add(">");
            BottomFloorCB.Items.Add("<");
            BottomFloorCB.SelectedIndex = 0;

            RearDoorsCB.Items.Add("");
            RearDoorsCB.Items.Add("YES");
            RearDoorsCB.Items.Add("NO");
            RearDoorsCB.SelectedIndex = 0;

            SecurityCB.Items.Add("");
            SecurityCB.Items.Add("YES");
            SecurityCB.Items.Add("NO");
            SecurityCB.SelectedIndex = 0;

            BSICB.Items.Add("");
            BSICB.Items.Add("YES");
            BSICB.Items.Add("NO");
            BSICB.SelectedIndex = 0;

            CRTLOCKCB.Items.Add("");
            CRTLOCKCB.Items.Add("YES");
            CRTLOCKCB.Items.Add("NO");
            CRTLOCKCB.SelectedIndex = 0;

            ACECB.Items.Add("");
            ACECB.Items.Add("YES");
            ACECB.Items.Add("NO");
            ACECB.SelectedIndex = 0;

            FalseFloorsCB.Items.Add("");
            FalseFloorsCB.Items.Add("YES");
            FalseFloorsCB.Items.Add("NO");
            FalseFloorsCB.SelectedIndex = 0;

            NudgingCB.Items.Add("");
            NudgingCB.Items.Add("YES");
            NudgingCB.Items.Add("NO");
            NudgingCB.SelectedIndex = 0;

            CallBoardsCB.Items.Add("");
            CallBoardsCB.Items.Add("=");
            CallBoardsCB.Items.Add(">");
            CallBoardsCB.Items.Add("<");
            CallBoardsCB.SelectedIndex = 0;

            IOXBoardsCB.Items.Add("");
            IOXBoardsCB.Items.Add("=");
            IOXBoardsCB.Items.Add(">");
            IOXBoardsCB.Items.Add("<");
            IOXBoardsCB.SelectedIndex = 0;

            I4OBoardsCB.Items.Add("");
            I4OBoardsCB.Items.Add("=");
            I4OBoardsCB.Items.Add(">");
            I4OBoardsCB.Items.Add("<");
            I4OBoardsCB.SelectedIndex = 0;

            AIOXBoardsCB.Items.Add("");
            AIOXBoardsCB.Items.Add("=");
            AIOXBoardsCB.Items.Add(">");
            AIOXBoardsCB.Items.Add("<");
            AIOXBoardsCB.SelectedIndex = 0;

            CEBoardCB.Items.Add("");
            CEBoardCB.Items.Add("YES");
            CEBoardCB.Items.Add("NO");
            CEBoardCB.SelectedIndex = 0;

            NCBoardCB.Items.Add("");
            NCBoardCB.Items.Add("YES");
            NCBoardCB.Items.Add("NO");
            NCBoardCB.SelectedIndex = 0;

            FTBoardCB.Items.Add("");
            FTBoardCB.Items.Add("YES");
            FTBoardCB.Items.Add("NO");
            FTBoardCB.SelectedIndex = 0;

            DLMBoardCB.Items.Add("");
            DLMBoardCB.Items.Add("YES");
            DLMBoardCB.Items.Add("NO");
            DLMBoardCB.SelectedIndex = 0;
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.GetType() == typeof(MainWindow))
                {
                    (window as MainWindow).FilesListBox.Items.Clear();
                }
            }

            string[] MP_locations = new string[] { "MPOLHM", "MPOLOM", "MPOLTD", "MPOLTM", "MPODH", "MPODT", "MPOLHD" };
            string[] MP_locations_Group = new string[] { "MPOGD", "MPOGM" };
            string[] MP2_locations = new string[] { "MP2COC" };
            string[] MP2_locations_Group = new string[] { "MP2OGM" };
            string[] source_locations = new string[] { "MC-MP\\MPODH", "MC-MP\\MPODT", "MC-MP\\MPOLHM", "MC-MP\\MPOLOM", "MC-MP\\MPOLTM"};
            string[] source_locations_Group = new string[] { "MC-MP\\MPOGM" };
            string[] source2_locations = new string[] { "MC-MP2\\MP2COC" };
            string[] source2_locations_Group = new string[] {"MC-MP2\\MP2OGM" };
            string[] custom_locations = new string[] { "MC-MP\\MPODH", "MC-MP\\MPODT", "MC-MP\\MPOLHD", "MC-MP\\MPOLHM", "MC-MP\\MPOLOM", "MC-MP\\MPOLTD", "MC-MP\\MPOLTM" };
            string[] custom_locations_Group = new string[] { "MC-MP\\MPOGD", "MC-MP\\MPOGM" };

            if (ControllerCB.SelectedItem.ToString() != "MP")
            {
                if(TypeTB.SelectedItem.ToString() != "Group")
                {
                    foreach (string location in MP2_locations)
                    {
                        Search_Location(location, "Product", false);
                    }
                }
                else
                {
                    foreach (string location in MP2_locations_Group)
                    {
                        Search_Location(location, "Product", true);
                    }
                }

                if (CustomFoldersCheckBox.IsChecked == true)
                {
                    if(TypeTB.SelectedItem.ToString() != "Group")
                    {
                        foreach (string location in source2_locations)
                        {
                            Search_Location(location, "Source", false);
                        }
                        Search_Custom2();
                    }
                    else
                    {
                        foreach (string location in source2_locations_Group)
                        {
                            Search_Location(location, "Source", true);
                        }
                    }
                }
            }

            if(ControllerCB.SelectedItem.ToString() != "MP2")
            {
                if(TypeTB.SelectedItem.ToString() != "Group")
                {
                    foreach (string location in MP_locations)
                    {
                        Search_Location(location, "Product", false);
                    }
                }
                else
                {
                    foreach(string location in MP_locations_Group)
                    {
                        Search_Location(location, "Product", true);
                    }
                }

                if(CustomFoldersCheckBox.IsChecked == true)
                {
                    if (TypeTB.SelectedItem.ToString() != "Group")
                    {
                        foreach (string location in source_locations)
                        {
                            Search_Location(location, "Source", false);
                        }
                    }
                    else
                    {
                        foreach(string location in source_locations_Group)
                        {
                            Search_Location(location, "Source", true);
                        }
                    }

                    if (TypeTB.SelectedItem.ToString() != "Group")
                    {
                        foreach (string location in custom_locations)
                        {
                            Search_Location(location, "Custom", false);
                        }
                    }
                    else
                    {
                        foreach (string location in custom_locations_Group)
                        {
                            Search_Location(location, "Custom", true);
                        }
                    }
                }
            }

            this.Close();
        }

        private void Search_Location(string location, string subfolder, bool isGroup)
        {
            try
            {
                int progress = 0;
                string jobNumber = "*.asm";
                string folder = "\\" + "\\" + "mceshared\\shared\\Software\\" + subfolder + "\\" + location;
                SearchFolderTB.Text = "Searching through " + folder + "...";
                string[] files = Directory.GetFiles(@folder, jobNumber, SearchOption.AllDirectories);
                SearchProgress.Maximum = files.Count();
                foreach (string file in files)
                {
                    if (stopSearching)
                    {
                        return;
                    }

                    try
                    {
                        int locationIndex = 28;
                        string jobFile = file.Substring(locationIndex, file.Length - locationIndex);
                        Validate_File(jobFile, isGroup);
                    }
                    catch
                    {

                    }
                    progress++;
                    SearchProgress.Dispatcher.Invoke(() => SearchProgress.Value = progress, DispatcherPriority.Background);
                }
            }
            catch
            {

            }
        }

        private void Search_Custom2()
        {
            try
            {
                int progress = 0;
                string jobNumber = "*.asm";
                string folder = "\\" + "\\" + "mceshared\\shared\\Software\\Custom2";
                SearchFolderTB.Text = "Searching through " + folder + "...";
                string[] files = Directory.GetFiles(@folder, jobNumber, SearchOption.AllDirectories);
                SearchProgress.Maximum = files.Count();
                foreach (string file in files)
                {
                    if (stopSearching)
                    {
                        return;
                    }

                    try
                    {
                        int locationIndex = 28;
                        string jobFile = file.Substring(locationIndex, file.Length - locationIndex);
                        if(General.Get_File_From_Path(jobFile).StartsWith("g") || General.Get_File_From_Path(jobFile).StartsWith("G"))
                        {
                            Validate_File(jobFile, true);
                        }
                        else
                        {
                            Validate_File(jobFile, false);
                        }
                        
                    }
                    catch
                    {

                    }
                    progress++;
                    SearchProgress.Dispatcher.Invoke(() => SearchProgress.Value = progress, DispatcherPriority.Background);
                }
            }
            catch
            {

            }
        }

        private void Validate_File(string file, bool isGroup = false)
        {
            Content content = new Content(file);

            string jobName = "";
            string topFloor = "";
            string topFloorDecimal = "";
            string botFloor = "";
            string botFloorDecimal = "";
            string falseFloors = "";
            string nudging = "";
            string iox = "";
            string i4o = "";
            string aiox = "";
            string callbnu = "";
            string rearDoor = "";
            string ceBoard = "";
            string ncBoard = "";
            string ftBoard = "";
            string dlmBoard = "";
            string bsi = "";
            string secrty = "";
            string crtlok = "";
            string secur = "";
            string ace = "";
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
            string FileVersion = versionTop + "." + versionMid + "." + versionBot;

            if (isGroup)
            {
                jobName = content.Get_String("JBNAME:", 1);
                iox = General.HexStringToDecimal(content.Get_Nibble("LOBBY:", 6, 0)).ToString();
                i4o = General.HexStringToDecimal(content.Get_Nibble("LOBBY:", 6, 1)).ToString();
                aiox = General.HexStringToDecimal(content.Get_Nibble("LOBBY:", 8, 0)).ToString();
                callbnu = General.HexStringToDecimal(content.Get_Nibble("LOBBY:", 7, 0)).ToString();
            }
            else
            {
                topFloor = content.Get_Byte("BOTTOM:", 2) + 'H';
                topFloorDecimal = (General.HexStringToDecimal(topFloor) + 1).ToString();
                botFloor = content.Get_Byte("BOTTOM:", 1) + 'H';
                botFloorDecimal = (General.HexStringToDecimal(botFloor) + 1).ToString();
                falseFloors = content.Get_Bit("CPVAR", 3, 0, 3);
                nudging = content.Get_Bit("CPVAR", 7, 0, 3);
                i4o = content.Get_Nibble("LOBBY:", 40, 1);
                iox = content.Get_Nibble("LOBBY:", 40, 0);
                aiox = content.Get_Nibble("LOBBY:", 52, 0);
                callbnu = content.Get_Nibble("LOBBY:", 41, 1);
                rearDoor = content.Get_Bit("LOBBY:", 12, 0, 3);
                ceBoard = content.Get_Bit("BOTTOM:", 6, 1, 1);
                ncBoard = content.Get_Bit("LOBBY:", 38, 1, 3);
                ftBoard = content.Get_Bit("BOTTOM:", 6, 1, 3);
                dlmBoard = content.Get_Bit("LOBBY:", 39, 0, 1);
                bsi = "NO";
                secrty = content.Get_Bit("LOBBY:", 31, 0, 3);
                crtlok = content.Get_Bit("LOBBY:", 31, 0, 1);
                secur = content.Get_Bit("CPVAR", 7, 1, 0);
                ace = content.Get_Bit("LOBBY:", 31, 0, 0);
            }
            
            foreach(string input in content.inputs)
            {
                if (input == "BSI")
                {
                    bsi = "YES";
                }
            }

            //Top Floor
            if (TopFloorTB.Text != "" && TopFloorCB.SelectedItem.ToString() != "")
            {
                if(TopFloorCB.SelectedItem.ToString() == "=")
                {
                    if(TopFloorTB.Text != topFloorDecimal)
                    {
                        return;
                    }
                }

                if(TopFloorCB.SelectedItem.ToString() == ">")
                {
                    if(Int32.Parse(TopFloorTB.Text) >= Int32.Parse(topFloorDecimal))
                    {
                        return;
                    }
                }

                if (TopFloorCB.SelectedItem.ToString() == "<")
                {
                    if (Int32.Parse(TopFloorTB.Text) <= Int32.Parse(topFloorDecimal))
                    {
                        return;
                    }
                }
            }

            //Bottom Floor
            if (BottomFloorTB.Text != "" && BottomFloorCB.SelectedItem.ToString() != "")
            {
                if (BottomFloorCB.SelectedItem.ToString() == "=")
                {
                    if (BottomFloorTB.Text != botFloorDecimal)
                    {
                        return;
                    }
                }

                if (BottomFloorCB.SelectedItem.ToString() == ">")
                {
                    if (Int32.Parse(BottomFloorTB.Text) >= Int32.Parse(botFloorDecimal))
                    {
                        return;
                    }
                }

                if (BottomFloorCB.SelectedItem.ToString() == "<")
                {
                    if (Int32.Parse(BottomFloorTB.Text) <= Int32.Parse(botFloorDecimal))
                    {
                        return;
                    }
                }
            }

            //Rear Doors
            if(RearDoorsCB.SelectedItem.ToString() != "")
            {
                if(RearDoorsCB.SelectedItem.ToString() != rearDoor)
                {
                    return;
                }
            }

            //Security
            if(SecurityCB.SelectedItem.ToString() != "")
            {
                if(SecurityCB.SelectedItem.ToString() == "YES")
                {
                    if (secur == "NO" && bsi == "NO" && crtlok == "NO" && ace == "NO")
                    {
                        return;
                    }
                }

                if (SecurityCB.SelectedItem.ToString() == "NO")
                {
                    if (secur == "YES" || bsi == "YES" || crtlok == "YES" || ace == "YES")
                    {
                        return;
                    }
                }
            }

            //BSI
            if(BSICB.SelectedItem.ToString() != "")
            {
                if(BSICB.SelectedItem.ToString() != bsi)
                {
                    return;
                }
            }

            //CRTLOCK
            if(CRTLOCKCB.SelectedItem.ToString() != "")
            {
                if(CRTLOCKCB.SelectedItem.ToString() != crtlok)
                {
                    return;
                }
            }

            //ACE
            if(ACECB.SelectedItem.ToString() != "")
            {
                if(ACECB.SelectedItem.ToString() != ace)
                {
                    return;
                }
            }

            //False Floors
            if (FalseFloorsCB.SelectedItem.ToString() != "")
            {
                if (FalseFloorsCB.SelectedItem.ToString() != falseFloors)
                {
                    return;
                }
            }

            //Nudging
            if (NudgingCB.SelectedItem.ToString() != "")
            {
                if (NudgingCB.SelectedItem.ToString() != nudging)
                {
                    return;
                }
            }

            //Call Boards
            if (CallBoardsTB.Text != "" && CallBoardsCB.SelectedItem.ToString() != "")
            {
                if (CallBoardsCB.SelectedItem.ToString() == "=")
                {
                    if (CallBoardsTB.Text != callbnu)
                    {
                        return;
                    }
                }

                if (CallBoardsCB.SelectedItem.ToString() == ">")
                {
                    if (Int32.Parse(CallBoardsTB.Text) >= Int32.Parse(callbnu))
                    {
                        return;
                    }
                }

                if (CallBoardsCB.SelectedItem.ToString() == "<")
                {
                    if (Int32.Parse(CallBoardsTB.Text) <= Int32.Parse(callbnu))
                    {
                        return;
                    }
                }
            }

            //IOX Boards
            if (IOXBoardsTB.Text != "" && IOXBoardsCB.SelectedItem.ToString() != "")
            {
                if (IOXBoardsCB.SelectedItem.ToString() == "=")
                {
                    if (IOXBoardsTB.Text != iox)
                    {
                        return;
                    }
                }

                if (IOXBoardsCB.SelectedItem.ToString() == ">")
                {
                    if (Int32.Parse(IOXBoardsTB.Text) >= Int32.Parse(iox))
                    {
                        return;
                    }
                }

                if (IOXBoardsCB.SelectedItem.ToString() == "<")
                {
                    if (Int32.Parse(IOXBoardsTB.Text) <= Int32.Parse(iox))
                    {
                        return;
                    }
                }
            }

            //I4O Boards
            if (I4OBoardsTB.Text != "" && I4OBoardsCB.SelectedItem.ToString() != "")
            {
                if (I4OBoardsCB.SelectedItem.ToString() == "=")
                {
                    if (I4OBoardsTB.Text != i4o)
                    {
                        return;
                    }
                }

                if (I4OBoardsCB.SelectedItem.ToString() == ">")
                {
                    if (Int32.Parse(I4OBoardsTB.Text) >= Int32.Parse(i4o))
                    {
                        return;
                    }
                }

                if (I4OBoardsCB.SelectedItem.ToString() == "<")
                {
                    if (Int32.Parse(I4OBoardsTB.Text) <= Int32.Parse(i4o))
                    {
                        return;
                    }
                }
            }

            //AIOXBoards
            if (AIOXBoardsTB.Text != "" && AIOXBoardsCB.SelectedItem.ToString() != "")
            {
                if (AIOXBoardsCB.SelectedItem.ToString() == "=")
                {
                    if (AIOXBoardsTB.Text != aiox)
                    {
                        return;
                    }
                }

                if (AIOXBoardsCB.SelectedItem.ToString() == ">")
                {
                    if (Int32.Parse(AIOXBoardsTB.Text) >= Int32.Parse(aiox))
                    {
                        return;
                    }
                }

                if (AIOXBoardsCB.SelectedItem.ToString() == "<")
                {
                    if (Int32.Parse(AIOXBoardsTB.Text) <= Int32.Parse(aiox))
                    {
                        return;
                    }
                }
            }

            //CE Board
            if (CEBoardCB.SelectedItem.ToString() != "")
            {
                if (CEBoardCB.SelectedItem.ToString() != ceBoard)
                {
                    return;
                }
            }

            //NC Board
            if (NCBoardCB.SelectedItem.ToString() != "")
            {
                if (NCBoardCB.SelectedItem.ToString() != ncBoard)
                {
                    return;
                }
            }

            //FT Board
            if (FTBoardCB.SelectedItem.ToString() != "")
            {
                if (FTBoardCB.SelectedItem.ToString() != ftBoard)
                {
                    return;
                }
            }

            //DLM Board
            if (DLMBoardCB.SelectedItem.ToString() != "")
            {
                if (DLMBoardCB.SelectedItem.ToString() != dlmBoard)
                {
                    return;
                }
            }

            //Spare Inputs
            foreach (var child in SpareInputSP.Children)
            {
                if(child.GetType() == typeof(TextBox))
                {
                    TextBox tb = (TextBox)child;
                    bool found = false;

                    if (tb.Text != null && tb.Text != "")
                    {
                        foreach (string input in content.inputs)
                        {
                            if (input == tb.Text)
                            {
                                found = true;
                            }
                        }
                    }

                    if(found == false)
                    {
                        return;
                    }
                }
            }

            //Spare Outputs
            foreach (var child in SpareOutputSP.Children)
            {
                if (child.GetType() == typeof(TextBox))
                {
                    TextBox tb = (TextBox)child;
                    bool found = false;

                    if (tb.Text != null && tb.Text != "")
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            for (int i2 = 0; i2 < 8; i2++)
                            {
                                if (content.outputs[i * 8 + i2] == tb.Text)
                                {
                                    found = true;
                                }
                            }
                        }

                        if (found == false)
                        {
                            return;
                        }
                    }
                }
            }

            //File Version
            if(VersionTB.Text.Length > 0)
            {
                if(FileVersion.Trim() != VersionTB.Text)
                {
                    return;
                }
            }

            //If the file made it this far, add it to the list box
            foreach (Window window in Application.Current.Windows)
            {
                if(window.GetType() == typeof(MainWindow))
                {
                    (window as MainWindow).FilesListBox.Items.Add(file);
                }
            }
        }

        private void Close(object sender, EventArgs e)
        {
            stopSearching = true;
        }

        private void AddSpareInput_Click(object sender, RoutedEventArgs e)
        {
            TextBox tb = new TextBox();
            tb.Width = 50;
            tb.Margin = new Thickness(419, 2, 0, 0);
            tb.HorizontalAlignment = HorizontalAlignment.Left;
            SpareInputSP.Children.Add(tb);
        }

        private void AddSpareOutput_Click(object sender, RoutedEventArgs e)
        {
            TextBox tb = new TextBox();
            tb.Width = 50;
            tb.Margin = new Thickness(419, 2, 0, 0);
            tb.HorizontalAlignment = HorizontalAlignment.Left;
            SpareOutputSP.Children.Add(tb);
        }
    }
}