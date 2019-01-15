using System;
using System.Windows;
using Excel = Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;

namespace Schedule
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            for(int i = 1; i <= 12; i++)
            {
                Hour.Items.Add(i);
            }

            for(int i = 0; i < 10; i++)
            {
                Minute.Items.Add("0" + i);
            }

            for(int i = 10; i < 60; i++)
            {
                Minute.Items.Add(i);
            }

            AMPM.Items.Add("AM");
            AMPM.Items.Add("PM");

            Hour.SelectedIndex = 5;
            Minute.SelectedIndex = 30;
            AMPM.SelectedIndex = 0;

            System.Windows.Forms.NotifyIcon ni = new System.Windows.Forms.NotifyIcon();
            ni.Icon = new System.Drawing.Icon("Main.ico");
            ni.Visible = true;
            ni.DoubleClick +=
                delegate (object sender, EventArgs args)
                {
                    this.Show();
                    this.WindowState = WindowState.Normal;
                };
        }
        
        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == System.Windows.WindowState.Minimized)
                this.Hide();

            base.OnStateChanged(e);
        }

        private void Enter_Data(int clock_int_or_out)
        {
            //Clock in = 0
            //Clock out = 1

            Excel.Application xlApp = new Excel.Application();
            Excel.Workbook xlWorkbook = xlApp.Workbooks.Open("C:\\Users\\Jacob.Ball\\Desktop\\Schedule.xlsx");
            Excel._Worksheet xlWorksheet = xlWorkbook.Sheets[1];
            Excel.Range xlRange = xlWorksheet.UsedRange;

            string hour = Hour.SelectedItem.ToString();
            string minute = Minute.SelectedItem.ToString();
            string ampm = AMPM.SelectedItem.ToString();

            string timeString = hour + ":" + minute + " " + ampm;

            DateTime today = DateTime.Today;
            string date = today.ToString("d");
            string weekday = today.ToString("D").Substring(0, today.ToString("D").IndexOf(','));

            int day = 4;

            if (weekday == "Saturday")
            {
                day = 4;
            }
            if (weekday == "Sunday")
            {
                day = 5;
            }
            if (weekday == "Monday")
            {
                day = 6;
            }
            if (weekday == "Tuesday")
            {
                day = 7;
            }
            if (weekday == "Wednesday")
            {
                day = 8;
            }
            if (weekday == "Thursday")
            {
                day = 9;
            }
            if (weekday == "Friday")
            {
                day = 10;
            }
                        
            int row = Convert_Todays_Date_To_Row_Number();
            xlRange.Cells[row + clock_int_or_out, day].Value = timeString;

            //cleanup
            GC.Collect();
            GC.WaitForPendingFinalizers();

            //rule of thumb for releasing com objects:
            //  never use two dots, all COM objects must be referenced and released individually
            //  ex: [somthing].[something].[something] is bad

            //release com objects to fully kill excel process from running in the background
            Marshal.ReleaseComObject(xlRange);
            Marshal.ReleaseComObject(xlWorksheet);

            //close and release
            xlWorkbook.Save();
            xlWorkbook.Close(true);
            Marshal.ReleaseComObject(xlWorkbook);

            //quit and release
            xlApp.Quit();
            Marshal.ReleaseComObject(xlApp);
        }

        private void ClockIn_Click(object sender, RoutedEventArgs e)
        {
            Enter_Data(0);

            Hour.SelectedIndex = 2;
            Minute.SelectedIndex = 0;
            AMPM.SelectedIndex = 1;
        }

        private void ClockOut_Click(object sender, RoutedEventArgs e)
        {
            Enter_Data(1);

            this.Close();
        }

        private int compareDates(string date1, string date2)
        {
            int slashIndex1 = date1.IndexOf('/');
            int month1 = Int32.Parse(date1.Substring(0, slashIndex1));
            date1 = date1.Substring(slashIndex1 + 1, date1.Length - slashIndex1 - 1);
            slashIndex1 = date1.IndexOf('/');
            int day1 = Int32.Parse(date1.Substring(0, slashIndex1));
            int year1 = Int32.Parse(date1.Substring(slashIndex1 + 1, date1.Length - slashIndex1 - 1));

            int slashIndex2 = date2.IndexOf('/');
            int month2 = Int32.Parse(date2.Substring(0, slashIndex2));
            date2 = date2.Substring(slashIndex2 + 1, date2.Length - slashIndex2 - 1);
            slashIndex2 = date2.IndexOf('/');
            int day2 = Int32.Parse(date2.Substring(0, slashIndex2));
            int year2 = Int32.Parse(date2.Substring(slashIndex2 + 1, date2.Length - slashIndex2 - 1));


            if (year1 == year2)
            {
                if (month1 == month2)
                {
                    if (day1 == day2)
                    {
                        return 1;
                    }
                    if (day1 > day2)
                    {
                        return 2;
                    }
                }
                if (month1 > month2)
                {
                    return 2;
                }
            }
            if (year1 > year2)
            {
                return 2;
            }

            return 0;
        }

        private int Convert_Todays_Date_To_Row_Number()
        {
            DateTime today = DateTime.Now;

            int day = today.DayOfYear + 2;  //We need to add 2 because The first friday is the 5th and we need it to be 7 for the offset to work out

            return (int)(2 * Math.Floor(day / 7.0)) + 2;  //2 is the row offset for the first week
        }
    }
}
