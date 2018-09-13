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
using System.Globalization;

namespace mods
{
    /// <summary>
    /// Interaction logic for ModDoc.xaml
    /// </summary>
    public partial class ModDoc : Window
    {
        private ListBox jobList;
        List<byte> export_mod_base;

        public ModDoc(ListBox jobList)
        {
            InitializeComponent();

            this.jobList = jobList;

            Populate_JobFile_ComboBox();

            JobFile.SelectedIndex = 0;
        }

        private void Populate_JobFile_ComboBox()
        {
            foreach(var item in jobList.Items)
            {
                JobFile.Items.Add(item.ToString());
            }
        }
        
        private List<string> Generate_Track_Mod_List(string jobNumber)
        {
            List<string> track_mod_list = new List<string>();

            Excel.Application xlApp = new Excel.Application();
            Excel.Workbook xlWorkbook = xlApp.Workbooks.Open("F:\\Software\\Product\\Trac_Mod.xlsm", 0, true);
            Excel._Worksheet xlWorksheet = xlWorkbook.Sheets[1];
            Excel._Worksheet dlmWorksheet = xlWorkbook.Sheets[2];
            Excel.Range xlRange = xlWorksheet.UsedRange;
            Excel.Range dlmRange = dlmWorksheet.UsedRange;

            List<string> Usernames = new List<string>();

            string userName = "jacob;jake";

            while (userName.Contains(";"))
            {
                int colonIndex = userName.IndexOf(';');
                Usernames.Add(userName.Substring(0, colonIndex));
                userName = userName.Substring(colonIndex + 1, userName.Length - colonIndex - 1);
            }

            Usernames.Add(userName);

            for (int row = 4; row < 100; row++)
            {
                if (xlRange.Cells[row, 8].Value2 != null && xlRange.Cells[row, 5].Value2 != null && xlRange.Cells[row, 4].Value2 != null)
                {
                    string engineer = xlRange.Cells[row, 8].Value2.ToString();
                    string jobNum = xlRange.Cells[row, 5].Value2.ToString();
                    string notifNumber = xlRange.Cells[row, 4].Value2.ToString();
                    foreach (string username in Usernames)
                    {
                        if (engineer.ToLower().Contains(username))
                        {
                            if (jobNum.Contains(jobNumber))
                            {
                                track_mod_list.Add(xlRange.Cells[row, 1].Value2.ToString());
                                track_mod_list.Add(xlRange.Cells[row, 2].Value2.ToString());
                                track_mod_list.Add(xlRange.Cells[row, 4].Value2.ToString());
                                track_mod_list.Add(xlRange.Cells[row, 5].Value2.ToString());
                                track_mod_list.Add(xlRange.Cells[row, 8].Value2.ToString());
                            }
                        }
                    }
                }
            }

            for (int row = 4; row < 100; row++)
            {
                if (dlmRange.Cells[row, 8].Value2 != null && dlmRange.Cells[row, 5].Value2 != null && dlmRange.Cells[row, 4].Value2 != null)
                {
                    string jobNum = dlmRange.Cells[row, 5].Value2.ToString();
                    string engineer = dlmRange.Cells[row, 8].Value2.ToString();
                    foreach (string username in Usernames)
                    {
                        if (engineer.ToLower().Contains(username))
                        {
                            if (jobNum.Contains(jobNumber))
                            {
                                track_mod_list.Add(dlmRange.Cells[row, 1].Value2.ToString());
                                track_mod_list.Add(dlmRange.Cells[row, 2].Value2.ToString());
                                track_mod_list.Add(dlmRange.Cells[row, 4].Value2.ToString());
                                track_mod_list.Add(dlmRange.Cells[row, 5].Value2.ToString());
                                track_mod_list.Add(dlmRange.Cells[row, 8].Value2.ToString());
                            }
                        }
                    }
                }
            }

            //cleanup
            GC.Collect();
            GC.WaitForPendingFinalizers();

            //rule of thumb for releasing com objects:
            //  never use two dots, all COM objects must be referenced and released individually
            //  ex: [somthing].[something].[something] is bad

            //release com objects to fully kill excel process from running in the background
            Marshal.ReleaseComObject(xlRange);
            Marshal.ReleaseComObject(xlWorksheet);
            Marshal.ReleaseComObject(dlmRange);
            Marshal.ReleaseComObject(dlmWorksheet);

            //close and release
            xlWorkbook.Close(false);
            Marshal.ReleaseComObject(xlWorkbook);

            //quit and release
            xlApp.Quit();
            Marshal.ReleaseComObject(xlApp);

            return track_mod_list;
        }

        private void Generate_File_Click(object sender, RoutedEventArgs e)
        {
            export_mod_base = File.ReadAllBytes(@"K:\Jake Ball\export_mod_base.afd").ToList();

            int initial_length = export_mod_base.Count;

            Content content = new Content(JobFile.SelectedItem.ToString());
            string jobNumber = General.Get_Job_Number_From_Path(JobFile.SelectedItem.ToString());

            string jobName = content.Get_String("JBNAME:", 1);
            jobName = jobName.Substring(0, jobName.IndexOf("("));
            string drivebit2 = content.Get_Bit("CPVAR", 2, 0, 1);
            string drivebit3 = content.Get_Bit("CPVAR", 2, 0, 0);
            string driveType = "";
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
            string selectedFileVersion = versionTop + "." + versionMid + "." + versionBot;


            List<string> track_mod_list = Generate_Track_Mod_List(jobNumber);
            string dateRcvd = Convert_Date_String(track_mod_list[0]);
            string duedate = Convert_Date_String(track_mod_list[1]);
            string notif = track_mod_list[2];
            string jobNum = track_mod_list[3];
            string engineer = track_mod_list[4];
            string todayDate = DateTime.Now.ToString("yyyyMMdd");

            /*Replace_Data("importJobNumber", jobNum);
            Replace_Data("importJobName", jobName);
            Replace_Data("importEngineer", engineer);
            Replace_Data("importNotif", notif);
            Replace_Data("20100605", duedate);
            Replace_Data("importDriveType", driveType);
            Replace_Data("20110605", dateRcvd);
            Replace_Data("20120605", todayDate);
            Replace_Data("importCarMP", selectedFileVersion);

            Handle_Data_Length_Bytes(initial_length);

            File.WriteAllBytes(@"C:\Users\jacob.ball\Desktop\Experimental\export_mod_base.afd", export_mod_base.ToArray());*/

            MemoryEditor me = new MemoryEditor("AcroFill", "ASCII");

            //me.Scan_Range((int)0x19AA268, (int)0x19AE47D);

            me.Replace_String("importJobNumber", jobNum);
            /*me.Replace_String("importJobName", jobName);
            me.Replace_String("importEngineer", engineer);
            me.Replace_String("importNotif", notif);
            me.Replace_String("20100605", duedate);
            me.Replace_String("importDriveType", driveType);
            me.Replace_String("20110605", dateRcvd);
            me.Replace_String("20120605", todayDate);
            me.Replace_String("importCarMP", selectedFileVersion);*/

            this.Close();
        }

        private string Convert_Date_String(string date)
        {
            double d = double.Parse(date);
            date = DateTime.FromOADate(d).ToString("MM/dd/yyyy");


            int slashIndex1 = date.IndexOf("/");
            int slashIndex2 = date.IndexOf("/", slashIndex1 + 1);

            string year = date.Substring(slashIndex2 + 1, 4);
            string month = date.Substring(0, slashIndex1);
            string day = date.Substring(slashIndex1 + 1, slashIndex2 - slashIndex1 - 1);

            return year + month + day;
        }

        private void Handle_Data_Length_Bytes(int initial_length)
        {
            int data_count = initial_length - 4891; //4901 bytes without data.
            int data_256_num = data_count / 256;
            int data_remainder = data_count % 256;

            byte[] bytes = new byte[] { 8, 0, 0, 0, Convert.ToByte(data_remainder), Convert.ToByte(data_256_num) };

            int data_length_int = Find_Byte_Array(bytes);

            int export_length = export_mod_base.Count;
            int new_data_count = export_length - 4891; //4901 bytes without data.
            int new_data_256_num = new_data_count / 256;
            int new_data_remainder = new_data_count % 256;

            for (int c = 0; c < 6; c++)
            {
                export_mod_base.RemoveAt(data_length_int);
            }

            byte[] new_bytes = new byte[] { 8, 0, 0, 0, Convert.ToByte(new_data_remainder), Convert.ToByte(new_data_256_num) };

            export_mod_base.InsertRange(data_length_int, new_bytes);
        }

        private void Replace_Data(string parameter, string data)
        {
            int parameterInt = Find_Byte_String(parameter);

            byte[] dataBytes = Encoding.ASCII.GetBytes(data);

            for (int c = 0; c < parameter.Length; c++)
            {
                export_mod_base.RemoveAt(parameterInt);
            }

            export_mod_base.InsertRange(parameterInt, dataBytes);
        }

        private int Find_Byte_Array(byte[] bytes)
        {
            int inc = 0;
            bool found = false;
            while (!found)
            {
                if (export_mod_base[inc] == bytes[0])
                {
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        if (export_mod_base[inc + i] == bytes[i])
                        {
                            found = true;
                        }
                        else
                        {
                            found = false;
                            break;
                        }
                    }
                }
                inc++;
            }

            return inc - 1;
        }

        private int Find_Byte_String(string findString)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(findString);

            int inc = 0;
            bool found = false;
            while(!found)
            {
                if(export_mod_base[inc] == findString[0])
                {
                    for(int i = 0; i < findString.Length; i++)
                    {
                        if(export_mod_base[inc + i] == findString[i])
                        {
                            found = true;
                        }
                        else
                        {
                            found = false;
                            break;
                        }
                    }
                }
                inc++;
            }

            return inc - 1;
        }

        public static byte[] ConvertHexStringToByteArray(string hexString)
        {
            if (hexString.Length % 2 != 0)
            {
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "The binary key cannot have an odd number of digits: {0}", hexString));
            }

            byte[] HexAsBytes = new byte[hexString.Length / 2];
            for (int index = 0; index < HexAsBytes.Length; index++)
            {
                string byteValue = hexString.Substring(index * 2, 2);
                HexAsBytes[index] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }

            return HexAsBytes;
        }
    }
}
