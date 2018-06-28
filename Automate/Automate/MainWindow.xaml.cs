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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Threading;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;

namespace Automate
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<string> content = new List<string>();
        public string filelog = "";
        public string filename = "";
        public int position;
        public List<string> car_calls = new List<string>();
        public int top_landing;
        public int MainRecallFloor;
        public int FRBYP;
        private string ForR_Recall;
        

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int BitBlt(IntPtr hDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, int dwRop);

        Bitmap screenPixel = new Bitmap(1, 1, PixelFormat.Format32bppArgb);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool SetCursorPos(int x, int y);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        public const int MOUSEEVENTF_LEFTDOWN = 0x02;
        public const int MOUSEEVENTF_LEFTUP = 0x04;

        public MainWindow()
        {
            InitializeComponent();
            DLMNormalBox.IsChecked = true;
            DLMFSPH1.IsChecked = true;
            DLMFSPH2.IsChecked = true;
                        
            CarCallDictionary();
        }

        private void SetVariables()
        {
            //Top Landing
            int topLandingIndex = content.IndexOf("[TotalNumberOfLandings]");
            string topLanding = content[topLandingIndex + 1].Substring(content[topLandingIndex + 1].Length - 2, 2);
            this.top_landing = Int32.Parse(topLanding);

            //Car Calls
            int CCElig = content.IndexOf("[CCElig]");
            int cc = 1;

            for(int x = CCElig+1; x <= CCElig + 126; x+=2)
            {
                if(content[x].Contains("True"))
                {
                    car_calls.Add("CCF" + cc);
                }
                if(content[x+1].Contains("True"))
                {
                    car_calls.Add("CCR" + cc);
                }
                cc++;
            }

            CarCalls calls = new CarCalls(car_calls, this.top_landing);
            calls.ShowDialog();
            car_calls = calls.car_calls;

            if(RearFR.IsChecked == true)
            {
                ForR_Recall = "RDO";
            }
            else
            {
                ForR_Recall = "FDO";
            }
        }

        private void Get_Content()
        {
            string path = FileBox.Text;
            content = System.IO.File.ReadAllLines(@path).ToList();

            string filepath = FileBox.Text;

            while (filepath.IndexOf("\\") != -1)
            {
                int slashindex = filepath.IndexOf('\\') + 1;
                int length = filepath.Length - slashindex;
                string tempfilepath = filepath.Substring(slashindex, length);
                filepath = tempfilepath;
            }
            filename = filepath.Substring(0, filepath.Length - 4);
        }

        private static readonly Dictionary<string, int[]> Buttons = new Dictionary<string, int[]> {
            {"FDC", new int[] {331,541} },
            {"FDO", new int[] {278,541} },
            {"RDC", new int[] {610,541} },
            {"RDO", new int[] {670,541} },
            {"FRSH", new int[] {828,441} },
            {"FRSL", new int[] {828,457} },
            {"FRAH", new int[] {860,441} },
            {"FRAL", new int[] {860,457} },
            {"FCSH", new int[] {949,441} },
            {"FCSL", new int[] {949,457} },
            {"FCOFH", new int[] {949,504} },
            {"FCOFL", new int[] {949,521} },
            {"FDBCH", new int[] {424,414} },
            {"FDBCL", new int[] {424,431} },
            {"RDBCH", new int[] {704,414} },
            {"RDBCL", new int[] {704,431} },
            {"FDOBH", new int[] {425,351} },
            {"FDOBL", new int[] {424,366} },
            {"RDOBH", new int[] {704,350} },
            {"RDOBL", new int[] {704,367} },
            {"FCDH", new int[] {328,351} },
            {"FCDL", new int[] {328,367} },
            {"RCDH", new int[] {608,351} },
            {"RCDL", new int[] {608,367} },
            {"FHDH", new int[] {360,351} },
            {"FHDL", new int[] {360,367} },
            {"RHDH", new int[] {640,351} },
            {"RHDL", new int[] {640,367} },
            {"INH", new int[] {232,158} },
            {"INL", new int[] {232,174} }
        };

        private static readonly Dictionary<string, int[]> SpareInputs = new Dictionary<string, int[]> { };

        private static readonly Dictionary<string, int[]> Colors = new Dictionary<string, int[]>
        {
            {"DoorOpen", new int[]{128,128,0} },
            {"DoorClosed", new int[] {192,192,192} }
        };

        private void Simulate_Click(object sender, RoutedEventArgs e)
        {
            bool ccb = (bool)CarCallsBox.IsChecked;
            bool fs = (bool)FSPH2Box.IsChecked;
            bool dlmN = (bool)DLMNormalBox.IsChecked;
            bool dlmFS1 = (bool)DLMFSPH1.IsChecked;
            bool dlmFS2 = (bool)DLMFSPH2.IsChecked;
            Get_Content();
            SetVariables();
            Thread t = new System.Threading.Thread(() => DoTheLoop(ccb,fs,dlmN,dlmFS1,dlmFS2));
            t.Start();
        }

        private void DoTheLoop(bool ccb, bool fs, bool dlmN, bool dlmFS1, bool dlmFS2)
        {
            DateTime now = DateTime.Now;
            filelog = "[" + now.ToString() + "]";
            WriteToFile();

            if (ccb == true)
            {
                CarCalls();
                WriteToFile();
            }
            if (fs == true)
            {
                FireService();
                WriteToFile();
            }
            if (dlmN == true)
            {
                DLMNormal();
                WriteToFile();
            }
            if (dlmFS1 == true)
            {
                DLMFireServicePhase1();
                WriteToFile();
            }
            if(dlmFS2 == true)
            { 
                DLMFireServicePhase2();
                WriteToFile();
            }
        }

        private void CursorPos_Click(object sender, RoutedEventArgs e)
        {
            while (true)
            {
                Win32Point w32Mouse = new Win32Point();
                GetCursorPos(ref w32Mouse);

                using (Graphics gdest = Graphics.FromImage(screenPixel))
                {
                    using (Graphics gsrc = Graphics.FromHwnd(IntPtr.Zero))
                    {
                        IntPtr hSrcDC = gsrc.GetHdc();
                        IntPtr hDC = gdest.GetHdc();
                        int retval = BitBlt(hDC, 0, 0, 1, 1, hSrcDC, w32Mouse.X, w32Mouse.Y, (int)CopyPixelOperation.SourceCopy);
                        gdest.ReleaseHdc();
                        gsrc.ReleaseHdc();
                    }
                }

                string position = w32Mouse.X.ToString() + "," + w32Mouse.Y.ToString() + " - " + screenPixel.GetPixel(0, 0).R + "," + screenPixel.GetPixel(0, 0).G + "," + screenPixel.GetPixel(0, 0).B;
                CursorPosition.Dispatcher.Invoke(() => CursorPosition.Text = position, DispatcherPriority.Background);
            }
        }
        
        private static void LeftMouseClick(int xpos, int ypos)
        {
            SetCursorPos(xpos, ypos);
            mouse_event(MOUSEEVENTF_LEFTDOWN, xpos, ypos, 0, 0);
            Thread.Sleep(200);
            mouse_event(MOUSEEVENTF_LEFTUP, xpos, ypos, 0, 0);
        }

        private int[] getColor(int[] point)
        {
            using (Graphics gdest = Graphics.FromImage(screenPixel))
            {
                using (Graphics gsrc = Graphics.FromHwnd(IntPtr.Zero))
                {
                    IntPtr hSrcDC = gsrc.GetHdc();
                    IntPtr hDC = gdest.GetHdc();
                    int retval = BitBlt(hDC, 0, 0, 1, 1, hSrcDC, point[0], point[1], (int)CopyPixelOperation.SourceCopy);
                    gdest.ReleaseHdc();
                    gsrc.ReleaseHdc();
                }
            }

            return new int[] { screenPixel.GetPixel(0, 0).R, screenPixel.GetPixel(0, 0).G, screenPixel.GetPixel(0, 0).B };
        }

        private void CarCallDictionary()
        {
            int[] frontstart = { 833, 65 };
            int[] rearstart = { 833, 222 };
            int cc = 1;

            for (int y = 0; y < 4; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    Buttons.Add("CCF" + cc, new int[] { x * 32 + frontstart[0], y * 32 + frontstart[1] });
                    cc++;
                }
            }

            cc = 1;

            for (int y = 0; y < 4; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    Buttons.Add("CCR" + cc, new int[] { x * 32 + rearstart[0], y * 32 + rearstart[1] });
                    cc++;
                }
            }
        }

        private void SpareInputsDictionary()
        {
            int[] start = { 28, 742 };
            int si = 0;

            for (int y = 0; y < 7; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    Buttons.Add("SI" + si, new int[] { x * 104 + start[0], y * 40 + start[1] });
                    si++;
                }
            }
        }

        private void Log(string log)
        {
            ProcedureLog.Dispatcher.BeginInvoke(new Action(() => { ProcedureLog.Text += log; }));
            filelog += log;
        }

        private void ClearLog()
        {
            ProcedureLog.Dispatcher.BeginInvoke(new Action(() => { ProcedureLog.Text = ""; }));
        }

        private void WriteToFile()
        {
            using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(@"C:\\Simulator\\Log\\" + filename + ".log", true))
            {
                file.WriteLine(filelog);
            }
            filelog = "";
        }

        private bool waitEvent(string button, string color, string log, int tab, double wait = 120)
        {
            Stopwatch watch = new Stopwatch();
            string tabs = "";
            watch.Start();
            while (!Enumerable.SequenceEqual(getColor(Buttons[button]), Colors[color]))
            {
                if (watch.Elapsed.TotalSeconds > wait)
                {
                    
                    for(int t = 0; t < tab; t++)
                    {
                        tabs += "\t";
                    }
                    Log(tabs + "Failed: " + log);
                    return false;
                }
            }

            for (int t = 0; t < tab; t++)
            {
                tabs += "\t";
            }
            Log(tabs + log);

            return true;
        }

        private void CarCalls()
        {
            ClearLog();
            Log("Initiate Car Calls Test\n");

            int skip = 0;

            foreach(string car_call in car_calls)
            {
                if (skip == 0)
                {
                    string call_num = car_call.Substring(3, car_call.Length - 3);
                    LeftMouseClick(Buttons[car_call][0], Buttons[car_call][1]);
                    char d = car_call[2];
                    Log(d + " Car Call " + call_num + "\n");
                    waitEvent(d + "DO", "DoorOpen", "Door Opened\n",1);
                    waitEvent(d + "DC", "DoorClosed", "Door Closed\n",1);
                }
                skip++;
                if(skip == 5)
                {
                    skip = 0;
                }
            }
        }

        private void FireService()
        {
            ClearLog();
            Log("Initiate Fire Service Test\n");

            LeftMouseClick(Buttons["FRSL"][0], Buttons["FRSL"][1]);
            Log("Initiate Fire Phase 1 - Main\n");
            waitEvent(ForR_Recall, "DoorOpen", "Door Opened: Finished Recall\n",1);
            UpdatePosition();
            MainRecallFloor = this.position;

            LeftMouseClick(Buttons["FCSH"][0], Buttons["FCSH"][1]);
            Log("\tInitiate Fire Phase 2\n");
            Thread.Sleep(1000);
            LeftMouseClick(Buttons["FDBCH"][0], Buttons["FDBCH"][1]);
            waitEvent("FDC", "DoorClosed", "Door Closed\n",2);
            LeftMouseClick(Buttons["FDBCH"][0], Buttons["FDBCH"][1]);
            Thread.Sleep(1000);

            int skip = 0;

            foreach (string car_call in car_calls)
            {
                if (skip == 0)
                {
                    LeftMouseClick(Buttons[car_call][0], Buttons[car_call][1]);
                    string call_num = car_call.Substring(3, car_call.Length - 3);
                    char d = car_call[2];

                    Log("\t\t" + d + " Car Call " + call_num + "\n");
                    Thread.Sleep(1000);
                    LeftMouseClick(Buttons[d + "DOBH"][0], Buttons[d + "DOBH"][1]);
                    waitEvent(d + "DO", "DoorOpen", "Door Opened\n",3);
                    LeftMouseClick(Buttons[d + "DOBH"][0], Buttons[d + "DOBH"][1]);
                    Thread.Sleep(1000);
                    LeftMouseClick(Buttons[d + "DBCH"][0], Buttons[d + "DBCH"][1]);
                    waitEvent(d + "DC", "DoorClosed", "Door Closed\n",3);
                    LeftMouseClick(Buttons[d + "DBCH"][0], Buttons[d + "DBCH"][1]);
                    UpdatePosition();
                    Thread.Sleep(1000);
                }
                skip++;
                if (skip == 5)
                {
                    skip = 0;
                }
            }

            LeftMouseClick(Buttons["CCF" + MainRecallFloor][0], Buttons["CCF" + MainRecallFloor][1]);
            Log("\tReturning to Main Recall Floor\n");
            Thread.Sleep(1000);
            LeftMouseClick(Buttons["FDOBH"][0], Buttons["FDOBH"][1]);
            waitEvent("FDO", "DoorOpen", "Door Opened\n",1);
            LeftMouseClick(Buttons["FDOBH"][0], Buttons["FDOBH"][1]);
            Thread.Sleep(1000);
            UpdatePosition();

            LeftMouseClick(Buttons["FCSH"][0], Buttons["FCSH"][1]);
            Thread.Sleep(1000);
            LeftMouseClick(Buttons["FCOFH"][0], Buttons["FCOFH"][1]);
            Thread.Sleep(1000);
            LeftMouseClick(Buttons["FCOFH"][0], Buttons["FCOFH"][1]);
            Log("\tEnd Fire Phase 2\n");

            Thread.Sleep(1000);
            LeftMouseClick(Buttons["FRSL"][0], Buttons["FRSL"][1]);
            Log("End Fire Phase 1\n");
            waitEvent("FDC", "DoorClosed", "Door Closed\n",0);
            Thread.Sleep(3000);
        }

        private void DLMNormal()
        {
            Random rand = new Random();

            int randCall = rand.Next(0, car_calls.Count-1);
            int call_num = 0;

            ClearLog();
            Log("Initiate Normal Operation DLM Test\n");

            //CD Jumpered Test
            LeftMouseClick(Buttons[car_calls[randCall]][0], Buttons[car_calls[randCall]][1]);
            call_num = Int32.Parse(car_calls[randCall].Substring(3, car_calls[randCall].Length - 3));
            char d = car_calls[randCall][2];

            LeftMouseClick(Buttons[d + "CDH"][0], Buttons[d + "CDH"][1]);
            Log("CD Jumpered\n");
            Thread.Sleep(1000);
            Log("\t" + d + " Car Call " + call_num + "\n");
            waitEvent(d + "DO", "DoorOpen", "Door Opened\n",2);
            waitEvent(d + "DC", "DoorClosed", "Door Closed\n", 2, 5.0);
            Thread.Sleep(1000);

            LeftMouseClick(Buttons[d + "DBCH"][0], Buttons[d + "DBCH"][1]);
            Log("\t\tActivated Door Close Button\n");
            waitEvent(d + "DC", "DoorClosed", "Door Closed\n", 2, 5.0);
            LeftMouseClick(Buttons[d + "DBCH"][0], Buttons[d + "DBCH"][1]);
            Thread.Sleep(1000);

            LeftMouseClick(Buttons[d + "CDH"][0], Buttons[d + "CDH"][1]);
            Log("\t\tCD Jumper Removed\n");
            Thread.Sleep(1000);
            if (!waitEvent(d + "DC", "DoorClosed", "Door Closed\n", 2, 5))
            {
                Log("\t\tToggle Inspection Switch to clear Fault\n");
                LeftMouseClick(Buttons["INL"][0], Buttons["INL"][1]);
                Thread.Sleep(1000);
                LeftMouseClick(Buttons["INL"][0], Buttons["INL"][1]);
                waitEvent(d + "DC", "DoorClosed", "Door Closed\n",2);
            }
            Thread.Sleep(1000);

            //HD Jumpered Test
            randCall = rand.Next(0, car_calls.Count - 1);
            call_num = Int32.Parse(car_calls[randCall].Substring(3, car_calls[randCall].Length - 3));
            d = car_calls[randCall][2];

            LeftMouseClick(Buttons[d + "HDH"][0], Buttons[d + "HDH"][1]);
            Log("HD Jumpered\n");
            Thread.Sleep(1000);

            LeftMouseClick(Buttons[car_calls[randCall]][0], Buttons[car_calls[randCall]][1]);
            
            Log("\t" + d + " Car Call " + call_num + "\n");
            waitEvent(d + "DO", "DoorOpen", "Door Opened\n",2);
            waitEvent(d + "DC", "DoorClosed", "Door Closed\n", 2, 5.0);
            Thread.Sleep(1000);

            LeftMouseClick(Buttons[d + "DBCH"][0], Buttons[d + "DBCH"][1]);
            Log("\t\tActivated Door Close Button\n");
            waitEvent(d + "DC", "DoorClosed", "Door Closed\n", 2, 5.0);
            LeftMouseClick(Buttons[d + "DBCH"][0], Buttons[d + "DBCH"][1]);
            Thread.Sleep(1000);

            LeftMouseClick(Buttons[d + "HDH"][0], Buttons[d + "HDH"][1]);
            Log("\t\tHD Jumper Removed\n");
            Thread.Sleep(1000);
            if (!waitEvent(d + "DC", "DoorClosed", "Door Closed\n", 2, 5))
            {
                Log("\t\tToggle Inspection Switch to clear Fault\n");
                LeftMouseClick(Buttons["INL"][0], Buttons["INL"][1]);
                Thread.Sleep(1000);
                LeftMouseClick(Buttons["INL"][0], Buttons["INL"][1]);
                waitEvent(d + "DC", "DoorClosed", "Door Closed", 2);
            }
            Thread.Sleep(1000);

            //CD and HD Jumpered Test
            randCall = rand.Next(0, car_calls.Count - 1);
            call_num = Int32.Parse(car_calls[randCall].Substring(3, car_calls[randCall].Length - 3));
            d = car_calls[randCall][2];

            LeftMouseClick(Buttons[d + "CDH"][0], Buttons[d + "CDH"][1]);
            Log("CD Jumpered\n");
            Thread.Sleep(1000);
            LeftMouseClick(Buttons[d + "HDH"][0], Buttons[d + "HDH"][1]);
            Log("HD Jumpered\n");
            Thread.Sleep(1000);

            LeftMouseClick(Buttons[car_calls[randCall]][0], Buttons[car_calls[randCall]][1]);
            
            Log("\t" + d + " Car Call " + call_num + "\n");
            waitEvent(d + "DO", "DoorOpen", "Door Opened\n", 2);
            waitEvent(d + "DC", "DoorClosed", "Door Closed\n", 2, 5.0);
            Thread.Sleep(1000);

            LeftMouseClick(Buttons[d + "DBCH"][0], Buttons[d + "DBCH"][1]);
            Log("\t\tActivated Door Close Button\n");
            waitEvent(d + "DC", "DoorClosed", "Door Closed\n", 2, 5.0);
            LeftMouseClick(Buttons[d + "DBCH"][0], Buttons[d + "DBCH"][1]);
            Thread.Sleep(1000);

            LeftMouseClick(Buttons[d + "CDH"][0], Buttons[d + "CDH"][1]);
            Log("\t\tCD Jumper Removed\n");
            Thread.Sleep(1000);
            LeftMouseClick(Buttons[d + "HDH"][0], Buttons[d + "HDH"][1]);
            Log("\t\tHD Jumper Removed\n");
            Thread.Sleep(1000);
            if (!waitEvent(d + "DC", "DoorClosed", "Door Closed\n", 2, 5))
            {
                Log("\t\tToggle Inspection Switch to clear Fault\n");
                LeftMouseClick(Buttons["INL"][0], Buttons["INL"][1]);
                Thread.Sleep(1000);
                LeftMouseClick(Buttons["INL"][0], Buttons["INL"][1]);
                waitEvent(d + "DC", "DoorClosed", "Door Closed\n", 2);
            }
            Thread.Sleep(1000);
        }

        private void DLMFireServicePhase1()
        {
            ClearLog();
            Log("\nInitiate Fire Service Phase 1 DLM Test\n");

            //Get Fire Service Phase 1 Main Recall Floor
            LeftMouseClick(Buttons["FRSL"][0], Buttons["FRSL"][1]);
            Log("\nInitiate Fire Phase 1 - Main\n");
            waitEvent(ForR_Recall, "DoorOpen", "Door Opened: Finished Recall\n",1);
            UpdatePosition();
            MainRecallFloor = this.position;
            Thread.Sleep(1000);
            LeftMouseClick(Buttons["FRSL"][0], Buttons["FRSL"][1]);
            Log("End Fire Phase 1");
            Thread.Sleep(3000);
            
            Random rand = new Random();
            int randCall = 0;
            int call_num = 0;

            do
            {
                randCall = rand.Next(0, car_calls.Count - 1);
                call_num = Int32.Parse(car_calls[randCall].Substring(3, car_calls[randCall].Length - 3));
            } while (call_num == MainRecallFloor);

            char d = car_calls[randCall][2];

            //CD Jumpered Test
            LeftMouseClick(Buttons[d + "CDH"][0], Buttons[d + "CDH"][1]);
            Log("\n\tCD Jumpered\n");
            Thread.Sleep(1000);
            
            LeftMouseClick(Buttons[car_calls[randCall]][0], Buttons[car_calls[randCall]][1]);
            Log("\t\t" + d + " Car Call " + call_num + "\n");
            waitEvent(d + "DO", "DoorOpen", "Door Opened\n",3);
            waitEvent(d + "DC", "DoorClosed", "Door Closed\n", 3, 5.0);
            Thread.Sleep(1000);
            LeftMouseClick(Buttons["FRSL"][0], Buttons["FRSL"][1]);
            Log("\t\t\tInitiate Fire Phase 1 - Main\n");
            waitEvent(d + "DC", "DoorClosed", "Door Closed\n", 3, 5.0);

            LeftMouseClick(Buttons[d + "CDH"][0], Buttons[d + "CDH"][1]);
            Log("\t\t\tCD Jumper Removed\n");
            if (!waitEvent(d + "DC", "DoorClosed", "Door Closed: Begin Recall\n", 4, 5))
            {
                Log("\t\t\t\tToggle Inspection Switch to clear Fault\n");
                LeftMouseClick(Buttons["INL"][0], Buttons["INL"][1]);
                Thread.Sleep(1000);
                LeftMouseClick(Buttons["INL"][0], Buttons["INL"][1]);
                waitEvent(d + "DC", "DoorClosed", "Door Closed: Begin Recall\n", 4);
            }
            waitEvent(ForR_Recall, "DoorOpen", "Door Opened: Finished Recall\n", 4);
            Thread.Sleep(3000);
            LeftMouseClick(Buttons["FRSL"][0], Buttons["FRSL"][1]);
            Log("\t\t\tEnd Fire Phase 1\n");
            Thread.Sleep(1000);

            //HD Jumpered Test
            do
            {
                randCall = rand.Next(0, car_calls.Count - 1);
                call_num = Int32.Parse(car_calls[randCall].Substring(3, car_calls[randCall].Length - 3));
            } while (call_num == MainRecallFloor);

            d = car_calls[randCall][2];

            LeftMouseClick(Buttons[d + "HDH"][0], Buttons[d + "HDH"][1]);
            Log("\n\tHD Jumpered\n");
            Thread.Sleep(1000);

            LeftMouseClick(Buttons[car_calls[randCall]][0], Buttons[car_calls[randCall]][1]);
            Log("\t\t" + d + " Car Call " + call_num + "\n");
            waitEvent(d + "DO", "DoorOpen", "Door Opened\n", 3);
            waitEvent(d + "DC", "DoorClosed", "Door Closed\n", 3, 5.0);
            Thread.Sleep(1000);
            LeftMouseClick(Buttons["FRSL"][0], Buttons["FRSL"][1]);
            Log("\t\t\tInitiate Fire Phase 1 - Main\n");
            waitEvent(d + "DC", "DoorClosed", "Door Closed\n", 3, 5.0);
            
            LeftMouseClick(Buttons[d + "HDH"][0], Buttons[d + "HDH"][1]);
            Log("\t\t\tHD Jumper Removed\n");
            if (!waitEvent(d + "DC", "DoorClosed", "Door Closed: Begin Recall\n", 4, 5))
            {
                Log("\t\t\t\tToggle Inspection Switch to clear Fault\n");
                LeftMouseClick(Buttons["INL"][0], Buttons["INL"][1]);
                Thread.Sleep(1000);
                LeftMouseClick(Buttons["INL"][0], Buttons["INL"][1]);
                waitEvent(d + "DC", "DoorClosed", "Door Closed: Begin Recall\n",4);
            }
            waitEvent(ForR_Recall, "DoorOpen", "Door Opened: Finished Recall\n",4);
            Thread.Sleep(3000);
            LeftMouseClick(Buttons["FRSL"][0], Buttons["FRSL"][1]);
            Log("\t\t\tEnd Fire Phase 1\n");
            Thread.Sleep(1000);

            //CD & HD Jumpered Test
            do
            {
                randCall = rand.Next(0, car_calls.Count - 1);
                call_num = Int32.Parse(car_calls[randCall].Substring(3, car_calls[randCall].Length - 3));
            } while (call_num == MainRecallFloor);

            d = car_calls[randCall][2];

            LeftMouseClick(Buttons[d + "CDH"][0], Buttons[d + "CDH"][1]);
            Log("\n\tCD Jumpered\n");
            Thread.Sleep(1000);
            LeftMouseClick(Buttons[d + "HDH"][0], Buttons[d + "HDH"][1]);
            Log("\tHD Jumpered\n");
            Thread.Sleep(1000);

            LeftMouseClick(Buttons[car_calls[randCall]][0], Buttons[car_calls[randCall]][1]);
            Log("\t\t" + d + " Car Call " + call_num + "\n");
            waitEvent(d + "DO", "DoorOpen", "Door Opened\n",3);
            waitEvent(d + "DC", "DoorClosed", "Door Closed\n", 3, 5.0);
            Thread.Sleep(1000);
            LeftMouseClick(Buttons["FRSL"][0], Buttons["FRSL"][1]);
            Log("\t\t\tInitiate Fire Phase 1 - Main\n");
            waitEvent(d + "DC", "DoorClosed", "Door Closed\n", 3, 5.0);

            LeftMouseClick(Buttons[d + "CDH"][0], Buttons[d + "CDH"][1]);
            Log("\t\t\tCD Jumper Removed\n");
            Thread.Sleep(1000);
            LeftMouseClick(Buttons[d + "HDH"][0], Buttons[d + "HDH"][1]);
            Log("\t\t\tHD Jumper Removed\n");
            if (!waitEvent(d + "DC", "DoorClosed", "Door Closed: Begin Recall\n", 4, 5))
            {
                Log("\t\t\t\tToggle Inspection Switch to clear Fault\n");
                LeftMouseClick(Buttons["INL"][0], Buttons["INL"][1]);
                Thread.Sleep(1000);
                LeftMouseClick(Buttons["INL"][0], Buttons["INL"][1]);
                waitEvent(d + "DC", "DoorClosed", "\t\t\t\tDoor Closed: Begin Recall\n",4);
            }
            waitEvent(ForR_Recall, "DoorOpen", "Door Opened: Finished Recall\n",4);
            Thread.Sleep(3000);
            LeftMouseClick(Buttons["FRSL"][0], Buttons["FRSL"][1]);
            Log("\t\t\tEnd Fire Phase 1\n");
            Thread.Sleep(1000);
        }

        private void DLMFireServicePhase2()
        {
            ClearLog();
            Log("Initiate Fire Service Phase 2 DLM Test\n");

            Random rand = new Random();
            int randCall = 0;
            int call_num = 0;

            LeftMouseClick(Buttons["FRSL"][0], Buttons["FRSL"][1]);
            Log("Initiate Fire Phase 1 - Main\n");
            waitEvent(ForR_Recall, "DoorOpen", "Door Opened: Finished Recall\n",1);
            UpdatePosition();
            MainRecallFloor = this.position;

            if(ForR_Recall.StartsWith("R"))
            {
                //Part 1
                LeftMouseClick(Buttons["FCSH"][0], Buttons["FCSH"][1]);
                Log("\tInitiate Fire Phase 2\n");
                Thread.Sleep(1000);
                LeftMouseClick(Buttons["RDBCH"][0], Buttons["RDBCH"][1]);
                waitEvent("RDC", "DoorClosed", "Door Closed\n", 2);
                LeftMouseClick(Buttons["RDBCH"][0], Buttons["RDBCH"][1]);
                Thread.Sleep(1000);
            }
            else
            {
                //Part 1
                LeftMouseClick(Buttons["FCSH"][0], Buttons["FCSH"][1]);
                Log("\tInitiate Fire Phase 2\n");
                Thread.Sleep(1000);
                LeftMouseClick(Buttons["FDBCH"][0], Buttons["FDBCH"][1]);
                waitEvent("FDC", "DoorClosed", "Door Closed\n", 2);
                LeftMouseClick(Buttons["FDBCH"][0], Buttons["FDBCH"][1]);
                Thread.Sleep(1000);
            }

            do
            {
                randCall = rand.Next(0, car_calls.Count - 1);
                call_num = Int32.Parse(car_calls[randCall].Substring(3, car_calls[randCall].Length - 3));
            } while (call_num == MainRecallFloor);

            char d = car_calls[randCall][2];
            
            LeftMouseClick(Buttons[car_calls[randCall]][0], Buttons[car_calls[randCall]][1]);
            Log("\t\t" + d + " Car Call " + call_num + "\n");
            Thread.Sleep(1000);


            //Part 2
            LeftMouseClick(Buttons[d + "CDH"][0], Buttons[d + "CDH"][1]);
            Log("\t\t\tCD Jumpered\n");
            Thread.Sleep(1000);

            //Part 3
            LeftMouseClick(Buttons[d + "DOBH"][0], Buttons[d + "DOBH"][1]);
            waitEvent(d + "DO", "DoorOpen", "Door Opened\n",4);
            LeftMouseClick(Buttons[d + "DOBH"][0], Buttons[d + "DOBH"][1]);
            Thread.Sleep(1000);

            //Part 4
            LeftMouseClick(Buttons[d + "DBCH"][0], Buttons[d + "DBCH"][1]);
            waitEvent(d + "DC", "DoorClosed", "Door Closed\n",4,5);
            LeftMouseClick(Buttons[d + "DBCH"][0], Buttons[d + "DBCH"][1]);
            Thread.Sleep(1000);


            //Part 5
            LeftMouseClick(Buttons[d + "HDH"][0], Buttons[d + "HDH"][1]);
            Log("\t\t\tHD Jumpered\n");
            Thread.Sleep(1000);

            //Part 6
            LeftMouseClick(Buttons[d + "DBCH"][0], Buttons[d + "DBCH"][1]);
            waitEvent(d + "DC", "DoorClosed", "Door Closed\n",4, 5);
            LeftMouseClick(Buttons[d + "DBCH"][0], Buttons[d + "DBCH"][1]);
            Thread.Sleep(1000);

            //Part 7
            LeftMouseClick(Buttons[d + "CDH"][0], Buttons[d + "CDH"][1]);
            Log("\t\t\tCD Jumper Removed\n");
            Thread.Sleep(1000);

            //Part 8
            LeftMouseClick(Buttons[d + "DBCH"][0], Buttons[d + "DBCH"][1]);
            waitEvent(d + "DC", "DoorClosed", "Door Closed\n",4,5);
            LeftMouseClick(Buttons[d + "DBCH"][0], Buttons[d + "DBCH"][1]);
            UpdatePosition();
            LeftMouseClick(Buttons[d + "HDH"][0], Buttons[d + "HDH"][1]);
            Log("\t\t\tHD Jumper Removed\n");

            //Part 9
            do
            {
                randCall = rand.Next(0, car_calls.Count - 1);
                call_num = Int32.Parse(car_calls[randCall].Substring(3, car_calls[randCall].Length - 3));
            } while (call_num == position);

            d = car_calls[randCall][2];

            LeftMouseClick(Buttons[car_calls[randCall]][0], Buttons[car_calls[randCall]][1]);
            Log("\t\t" + d + " Car Call " + call_num + "\n");
            Thread.Sleep(1000);
            LeftMouseClick(Buttons[d + "HDH"][0], Buttons[d + "HDH"][1]);
            Log("\t\t\tHD Jumpered\n");

            //Part 10
            LeftMouseClick(Buttons[d + "DOBH"][0], Buttons[d + "DOBH"][1]);
            waitEvent(d + "DO", "DoorOpen", "Door Opened\n",4);
            LeftMouseClick(Buttons[d + "DOBH"][0], Buttons[d + "DOBH"][1]);
            Thread.Sleep(1000);

            //Part 11
            LeftMouseClick(Buttons["FCSH"][0], Buttons["FCSH"][1]);
            Thread.Sleep(1000);
            LeftMouseClick(Buttons["FCOFH"][0], Buttons["FCOFH"][1]);
            Thread.Sleep(1000);
            LeftMouseClick(Buttons["FCOFH"][0], Buttons["FCOFH"][1]);
            Log("\t\t\tEnd Fire Phase 2\n");

            //Part 12
            waitEvent(d + "DC", "DoorClosed", "Door Closed: Begin Recall\n",3, 5);
            waitEvent(ForR_Recall, "DoorOpen", "Door Opened: Finished Recall\n",3);
            LeftMouseClick(Buttons[d + "HDH"][0], Buttons[d + "HDH"][1]);
            Log("\t\t\tHD Jumper Removed\n");
            Thread.Sleep(1000);

            //Part 13
            LeftMouseClick(Buttons["FCSH"][0], Buttons["FCSH"][1]);
            Log("Initiate Fire Phase 2\n");
            Thread.Sleep(1000);
            LeftMouseClick(Buttons["FDBCH"][0], Buttons["FDBCH"][1]);
            waitEvent("FDC", "DoorClosed", "Door Closed\n",1);
            LeftMouseClick(Buttons["FDBCH"][0], Buttons["FDBCH"][1]);
            Thread.Sleep(1000);

            do
            {
                randCall = rand.Next(0, car_calls.Count - 1);
                call_num = Int32.Parse(car_calls[randCall].Substring(3, car_calls[randCall].Length - 3));
            } while (call_num == MainRecallFloor);

            d = car_calls[randCall][2];

            LeftMouseClick(Buttons[car_calls[randCall]][0], Buttons[car_calls[randCall]][1]);
            Log("\t" + d + " Car Call " + call_num + "\n");
            Thread.Sleep(1000);
            LeftMouseClick(Buttons[d + "CDH"][0], Buttons[d + "CDH"][1]);
            Log("\t\tCD Jumpered\n");
            Thread.Sleep(1000);
            LeftMouseClick(Buttons[d + "DOBH"][0], Buttons[d + "DOBH"][1]);
            waitEvent(d + "DO", "DoorOpen", "Door Opened\n",3);
            LeftMouseClick(Buttons[d + "DOBH"][0], Buttons[d + "DOBH"][1]);
            Thread.Sleep(1000);
            LeftMouseClick(Buttons[d + "DBCH"][0], Buttons[d + "DBCH"][1]);
            waitEvent(d + "DC", "DoorClosed", "Door Closed\n", 3,5);
            LeftMouseClick(Buttons[d + "DBCH"][0], Buttons[d + "DBCH"][1]);
            Thread.Sleep(1000);

            LeftMouseClick(Buttons["FCSH"][0], Buttons["FCSH"][1]);
            Thread.Sleep(1000);
            LeftMouseClick(Buttons["FCOFH"][0], Buttons["FCOFH"][1]);
            Thread.Sleep(1000);
            LeftMouseClick(Buttons["FCOFH"][0], Buttons["FCOFH"][1]);
            Log("\t\t\tEnd Fire Phase 2\n");
            waitEvent(d + "DC", "DoorClosed", "Door Closed: Begin Recall\n",3, 5);

            //Part 14
            Thread.Sleep(1000);
            LeftMouseClick(Buttons[d + "CDH"][0], Buttons[d + "CDH"][1]);
            Log("\t\tCD Jumper Removed\n");
            if(!waitEvent(d + "DC", "DoorClosed", "Door Closed: Begin Recall\n",3, 5))
            {
                Log("\t\t\tToggle Inspection Switch to clear Fault\n");
                LeftMouseClick(Buttons["INL"][0], Buttons["INL"][1]);
                Thread.Sleep(1000);
                LeftMouseClick(Buttons["INL"][0], Buttons["INL"][1]);
                waitEvent(d + "DC", "DoorClosed", "Door Closed: Begin Recall\n",3);
            }
            waitEvent(ForR_Recall, "DoorOpen", "Door Opened: Finished Recall\n",3);
                
            Thread.Sleep(1000);
            LeftMouseClick(Buttons["FRSL"][0], Buttons["FRSL"][1]);
            Log("End Fire Phase 1\n");
            Thread.Sleep(3000);
        }

        private void UpdatePosition()
        {
            int[] On = new int[] { 182, 215, 83 };
            int[] Off = new int[] { 0, 64, 0 };
            int yPos = 440;
            int[] encodings = new int[] { 1270, 1246, 1222, 1198, 1174, 1150, 1126, 1102 };
            int total = 0;
            int bin = 0;

            foreach (int encoding in encodings)
            {
                int[] color = getColor(new int[] { encoding, yPos });
                if (Enumerable.SequenceEqual(color, On))
                {
                    total += (int)Math.Pow(2, bin);
                }
                bin++;
            }

            this.position = total;
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            
            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".sdf";

            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();


            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
                string filename = dlg.FileName;
                FileBox.Text = filename;
            }
        }
        
        private void Window_Closed(object sender, EventArgs args)
        {
            System.Windows.Application.Current.Shutdown();
        }
    }
}
