using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Excel = Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;
using System.IO;

namespace Schedule
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        void App_SessionEnding(object sender, SessionEndingCancelEventArgs e)
        {
            try
            {
                string msg = string.Format("{0}. End session?", e.ReasonSessionEnding);
                MessageBoxResult result = MessageBox.Show(msg, "Session Ending", MessageBoxButton.YesNo);

                if(result == MessageBoxResult.No)
                {
                    e.Cancel = true;
                    Window main = MainWindow;
                    main.Show();
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private bool Check_Clocked_Out()
        {
            bool clocked_out = false;

            Excel.Application xlApp = new Excel.Application();
            Excel.Workbook xlWorkbook = xlApp.Workbooks.Open("C:\\Users\\Jacob.Ball\\Desktop\\Schedule.xlsx");
            Excel._Worksheet xlWorksheet = xlWorkbook.Sheets[1];
            Excel.Range xlRange = xlWorksheet.UsedRange;

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
            if(Convert.ToString(xlRange.Cells[row + 1, day].Value2) != "")
            {
                clocked_out = true;
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
            xlWorkbook.Close(true);
            Marshal.ReleaseComObject(xlWorkbook);

            //quit and release
            xlApp.Quit();
            Marshal.ReleaseComObject(xlApp);

            return clocked_out;
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
