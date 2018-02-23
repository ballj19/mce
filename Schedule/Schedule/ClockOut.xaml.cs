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
using Excel = Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;

namespace Schedule
{
    /// <summary>
    /// Interaction logic for ClockOut.xaml
    /// </summary>
    public partial class ClockOut : Window
    {
        public ClockOut()
        {
            InitializeComponent();

            for (int i = 1; i <= 12; i++)
            {
                Hour.Items.Add(i.ToString());
            }

            Minute.Items.Add("00");
            Minute.Items.Add("15");
            Minute.Items.Add("30");
            Minute.Items.Add("45");

            AMPM.Items.Add("AM");
            AMPM.Items.Add("PM");

            Hour.SelectedIndex = 3;
            Minute.SelectedIndex = 0;
            AMPM.SelectedIndex = 1;
        }

        private void ClockOut_Click(object sender, RoutedEventArgs e)
        {
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

            for (int row = 2; row < 200; row++)
            {
                var rowDate = xlRange.Cells[row, 2].Value2;

                if (rowDate != null)
                {
                    if (xlRange.Cells[row, 1].Value2 == "Arrival")
                    {
                        if (compareDates(date, xlRange.Cells[row, 2].Value2.ToString()) > 0)
                        {
                            if (compareDates(date, xlRange.Cells[row + 1, 2].Value2.ToString()) < 2)
                            {
                                xlRange.Cells[row + 1, day].Value = timeString;
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

            //close and release
            xlWorkbook.Save();
            xlWorkbook.Close(true);
            Marshal.ReleaseComObject(xlWorkbook);

            //quit and release
            xlApp.Quit();
            Marshal.ReleaseComObject(xlApp);
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
    }
}
