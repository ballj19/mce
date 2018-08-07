using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;

namespace TrackModAlert
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                DateTime now = DateTime.Now;

                Console.WriteLine("Scanning " + now.ToString("HH:mm") + ".....");
                
                List<string> jobs = File.ReadAllLines(@"\\amrappfil01\MCE-Rancho\Jake Ball\jobs.txt").ToList();
                
                //OPEN EXCEL
                Excel.Application xlApp = new Excel.Application();
                xlApp.Visible = false;
                Excel.Workbook xlWorkbook = xlApp.Workbooks.Open("F:\\Software\\Product\\Trac_Mod.xlsm", 0, true);
                Excel._Worksheet xlWorksheet = xlWorkbook.Sheets[1];
                Excel._Worksheet dlmWorksheet = xlWorkbook.Sheets[2];
                Excel.Range xlRange = xlWorksheet.UsedRange;
                Excel.Range dlmRange = dlmWorksheet.UsedRange;

                for (int row = 4; row < 100; row++)
                {
                    if (xlRange.Cells[row, 5].Value2 != null)
                    {
                        string engineer = "";
                        string jobNumber = xlRange.Cells[row, 5].Value2.ToString();

                        try
                        {
                            engineer = xlRange.Cells[row, 8].Value2.ToString();
                        }
                        catch
                        {

                        }

                        if (engineer.ToLower() == "jacob" || engineer.ToLower() == "jake")
                        {
                            if (!jobs.Contains(jobNumber))
                            {
                                using (System.IO.StreamWriter file =
                                            new System.IO.StreamWriter(@"\\amrappfil01\MCE-Rancho\Jake Ball\jobs.txt", true))
                                {
                                    file.WriteLine(jobNumber);
                                    Console.WriteLine("Found New Job " + jobNumber);
                                }

                                System.Windows.Forms.MessageBox.Show("A new job is available");
                            }
                        }
                    }
                }

                for (int row = 4; row < 100; row++)
                {
                    if (dlmRange.Cells[row, 5].Value2 != null)
                    {
                        string engineer = "";
                        string jobNumber = dlmRange.Cells[row, 5].Value2.ToString();

                        try
                        {
                            engineer = dlmRange.Cells[row, 8].Value2.ToString();
                        }
                        catch
                        {

                        }

                        if (engineer.ToLower() == "jacob" || engineer.ToLower() == "jake")
                        {
                            if (!jobs.Contains(jobNumber))
                            {
                                using (System.IO.StreamWriter file =
                                            new System.IO.StreamWriter(@"\\amrappfil01\MCE-Rancho\Jake Ball\jobs.txt", true))
                                {
                                    file.WriteLine(jobNumber);
                                }

                                System.Windows.Forms.MessageBox.Show("A new job is available");
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

                Thread.Sleep(60000);
            }
        }
    }
}
