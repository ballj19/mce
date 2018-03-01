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

namespace mods
{
    /// <summary>
    /// Interaction logic for Procedure.xaml
    /// </summary>
    public partial class Procedure : Window
    {
        public string filepath;
        public string filename;
        public string filelog = "";
        public List<int> landings;
        public int position;
        public int MainRecallFloor;
        public int FRBYP;

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

        public Content content;

        public Procedure(string filepath)
        {
            InitializeComponent();

            CarCallsBox.IsChecked = true;
            FireServiceBox.IsChecked = true;
            DLMBox.IsChecked = false;

            this.content = new Content(filepath);

            while (filepath.IndexOf("\\") != -1)
            {
                int slashindex = filepath.IndexOf('\\') + 1;
                int length = filepath.Length - slashindex;
                string tempfilepath = filepath.Substring(slashindex, length);
                filepath = tempfilepath;
            }
            this.filename = filepath.Substring(0, filepath.Length - 4);

            string[,] inputs = content.inputs;
            int inputcount = 0;

            /*foreach(string input in inputs)
            {
                if(input.Contains("FRBYP"))
                {
                    this.FRBYP = inputcount;
                }
                inputcount++;
            }*/
            CarCallDictionary();
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
            {"DBCH", new int[] {424,414} },
            {"DBCL", new int[] {424,431} },
            {"DOBH", new int[] {425,351} },
            {"DOBL", new int[] {424,366} },
            {"FCDH", new int[] {328,351} },
            {"FCDL", new int[] {328,367} },
            {"FHDH", new int[] {360,351} },
            {"FHDL", new int[] {360,367} },
            {"INH", new int[] {232,158} },
            {"INL", new int[] {232,174} }
        };

        private static readonly Dictionary<string, int[]> SpareInputs = new Dictionary<string, int[]> {};

        private static readonly Dictionary<string, int[]> Colors = new Dictionary<string, int[]>
        {
            {"DoorOpen", new int[]{128,128,0} },
            {"DoorClosed", new int[] {192,192,192} }
        };

        private void Simulate_Click(object sender, RoutedEventArgs e)
        {
            bool cc = (bool)CarCallsBox.IsChecked;
            bool fs = (bool)FireServiceBox.IsChecked;
            bool dlm = (bool)DLMBox.IsChecked;
            Thread t = new System.Threading.Thread(() => DoTheLoop(cc,fs,dlm));
            t.Start();
        }

        private void DoTheLoop(bool cc, bool fs, bool dlm)
        {
            if (cc)
            {
                CarCalls();
            }
            if(dlm)
            {
                DLMNormal();
            }
            if (fs)
            {
                FireService();
            }
            if(dlm)
            {
                DLMFireService();
            }

            WriteToFile();
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
        
        //This simulates a left mouse click
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
                    Buttons.Add("FCC" + cc, new int[] { x * 32 + frontstart[0], y * 32 + frontstart[1] });
                    cc++;
                }
            }

            cc = 1;

            for (int y = 0; y < 4; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    Buttons.Add("RCC" + cc, new int[] { x * 32 + rearstart[0], y * 32 + rearstart[1] });
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
            new System.IO.StreamWriter(@"C:\\Simulator\\Log\\" + filename + ".txt", true))
            {
                file.WriteLine(filelog);
            }
        }

        private bool waitEvent(string button, string color, string log = "", double wait = 120)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            while (!Enumerable.SequenceEqual(getColor(Buttons[button]), Colors[color]))
            {
                if(watch.Elapsed.TotalSeconds > wait)
                {
                    Log("Failed: " + log);
                    return false;
                }
            }

            Log(log);

            return true;
        }

        private void CarCalls()
        {
            int top_landing = content.HexStringToDecimal(content.Get_Byte("BOTTOM:", 2)) + 1;

            ClearLog();
            Log("Initiate Car Calls Test\n");

            for (int x = 1; x <= top_landing; x++)
            {
                if (content.Get_Bit("ELIGIV:", x, 0, 3) == "YES")
                {
                    LeftMouseClick(Buttons["FCC" + x][0], Buttons["FCC" + x][1]);
                    Log("Front Car Call " + x + "\n");
                    waitEvent("FDO", "DoorOpen", "Door Opened\n");
                    waitEvent("FDC", "DoorClosed", "Door Closed\n");
                }
            }

            for (int x = 1; x <= top_landing; x++)
            {
                if (content.Get_Bit("ELIGIV:", x, 0, 2) == "YES")
                {
                    LeftMouseClick(Buttons["RCC" + x][0], Buttons["RCC" + x][1]);
                    Log("Rear Car Call " + x + "\n");
                    waitEvent("RDO", "DoorOpen", "Door Opened\n");
                    waitEvent("RDC", "DoorClosed", "Door Closed\n");
                }
            }
        }

        private void FireService()
        {
            ClearLog();
            Log("Initiate Fire Service Test\n");

            LeftMouseClick(Buttons["FRSL"][0], Buttons["FRSL"][1]);
            Log("Initiate Fire Phase 1 - Main\n");
            waitEvent("FDO", "DoorOpen", "Finished Recall\nDoor Opened\n");
            UpdatePosition();
            MainRecallFloor = this.position;

            LeftMouseClick(Buttons["FCSH"][0], Buttons["FCSH"][1]);
            Log("Initiate Fire Phase 2\n");
            Thread.Sleep(1000);
            LeftMouseClick(Buttons["DBCH"][0], Buttons["DBCH"][1]);
            waitEvent("FDC", "DoorClosed", "Door Closed\n");
            LeftMouseClick(Buttons["DBCH"][0], Buttons["DBCH"][1]);
            Thread.Sleep(1000);

            int top_landing = content.HexStringToDecimal(content.Get_Byte("BOTTOM:", 2)) + 1;
            int skip = 0;
            for (int x = 1; x <= top_landing; x++)
            {
                if (content.Get_Bit("ELIGIV:", x, 0, 3) == "YES")
                {
                    if (skip == 0)
                    {
                        LeftMouseClick(Buttons["FCC" + x][0], Buttons["FCC" + x][1]);
                        Log("Front Car Call " + x + "\n");
                        Thread.Sleep(1000);
                        LeftMouseClick(Buttons["DOBH"][0], Buttons["DOBH"][1]);
                        waitEvent("FDO", "DoorOpen", "Door Opened\n");
                        LeftMouseClick(Buttons["DOBH"][0], Buttons["DOBH"][1]);
                        Thread.Sleep(1000);
                        LeftMouseClick(Buttons["DBCH"][0], Buttons["DBCH"][1]);
                        waitEvent("FDC", "DoorClosed", "Door Closed\n");
                        LeftMouseClick(Buttons["DBCH"][0], Buttons["DBCH"][1]);
                        UpdatePosition();
                        Thread.Sleep(1000);
                    }
                    skip++;
                    if(skip == 3)
                    {
                        skip = 0;
                    }
                }
            }

            LeftMouseClick(Buttons["FCC" + MainRecallFloor][0], Buttons["FCC" + MainRecallFloor][1]);
            Log("Returning to Main Recall Floor\n");
            Thread.Sleep(1000);
            LeftMouseClick(Buttons["DOBH"][0], Buttons["DOBH"][1]);
            waitEvent("FDO", "DoorOpen", "Door Opened\n");
            LeftMouseClick(Buttons["DOBH"][0], Buttons["DOBH"][1]);
            Thread.Sleep(1000);
            UpdatePosition();

            LeftMouseClick(Buttons["FCSH"][0], Buttons["FCSH"][1]);
            Thread.Sleep(1000);
            LeftMouseClick(Buttons["FCOFH"][0], Buttons["FCOFH"][1]);
            Thread.Sleep(1000);
            LeftMouseClick(Buttons["FCOFH"][0], Buttons["FCOFH"][1]);
            Log("End Fire Phase 2\n");

            Thread.Sleep(1000);
            LeftMouseClick(Buttons["FRSL"][0], Buttons["FRSL"][1]);
            Log("End Fire Phase 1\n");
        }

        private void DLMNormal()
        {
            int top_landing = content.HexStringToDecimal(content.Get_Byte("BOTTOM:", 2)) + 1;
            Random rand = new Random();

            int randCall = rand.Next(1, top_landing);

            ClearLog();
            Log("Initiate Normal Operation DLM Test\n");

            //CD Jumpered Test
            LeftMouseClick(Buttons["FCDH"][0], Buttons["FCDH"][1]);
            Log("CD Jumpered\n");
            Thread.Sleep(1000);

            LeftMouseClick(Buttons["FCC" + randCall][0], Buttons["FCC" + randCall][1]);
            Log("Front Car Call " + randCall + "\n");
            waitEvent("FDO", "DoorOpen", "Door Opened\n");
            waitEvent("FDC", "DoorClosed", "Door Closed\n",5.0);
            Thread.Sleep(1000);

            LeftMouseClick(Buttons["DBCH"][0], Buttons["DBCH"][1]);
            Log("Activated Door Close Button\n");
            waitEvent("FDC", "DoorClosed", "Door Closed\n",5.0);
            LeftMouseClick(Buttons["DBCH"][0], Buttons["DBCH"][1]);
            Thread.Sleep(1000);

            LeftMouseClick(Buttons["FCDH"][0], Buttons["FCDH"][1]);
            Log("CD Jumper Removed\n");
            Thread.Sleep(1000);
            if (!waitEvent("FDO", "DoorClosed", "Door Closed\n", 5))
            {
                Log("Toggle Inspection Switch to clear Fault\n");
                LeftMouseClick(Buttons["INL"][0], Buttons["INL"][1]);
                Thread.Sleep(1000);
                LeftMouseClick(Buttons["INL"][0], Buttons["INL"][1]);
                waitEvent("FDO", "DoorClosed", "Door Closed\n");
            }
            Thread.Sleep(5000);

            //HD Jumpered Test
            randCall = rand.Next(1, top_landing);
            LeftMouseClick(Buttons["FHDH"][0], Buttons["FHDH"][1]);
            Log("HD Jumpered\n");
            Thread.Sleep(1000);

            LeftMouseClick(Buttons["FCC" + randCall][0], Buttons["FCC" + randCall][1]);
            Log("Front Car Call " + randCall + "\n");
            waitEvent("FDO", "DoorOpen", "Door Opened\n");
            waitEvent("FDC", "DoorClosed", "Door Closed\n", 5.0);
            Thread.Sleep(1000);

            LeftMouseClick(Buttons["DBCH"][0], Buttons["DBCH"][1]);
            Log("Activated Door Close Button\n");
            waitEvent("FDC", "DoorClosed", "Door Closed\n", 5.0);
            LeftMouseClick(Buttons["DBCH"][0], Buttons["DBCH"][1]);
            Thread.Sleep(1000);

            LeftMouseClick(Buttons["FHDH"][0], Buttons["FHDH"][1]);
            Log("HD Jumper Removed\n");
            Thread.Sleep(1000);
            if (!waitEvent("FDO", "DoorClosed", "Door Closed\n", 5))
            {
                Log("Toggle Inspection Switch to clear Fault\n");
                LeftMouseClick(Buttons["INL"][0], Buttons["INL"][1]);
                Thread.Sleep(1000);
                LeftMouseClick(Buttons["INL"][0], Buttons["INL"][1]);
                waitEvent("FDO", "DoorClosed", "Door Closed");
            }
            Thread.Sleep(5000);

            //CD and HD Jumpered Test
            randCall = rand.Next(1, top_landing);
            LeftMouseClick(Buttons["FCDH"][0], Buttons["FCDH"][1]);
            Log("CD Jumpered\n");
            Thread.Sleep(1000);
            LeftMouseClick(Buttons["FHDH"][0], Buttons["FHDH"][1]);
            Log("HD Jumpered\n");
            Thread.Sleep(1000);

            LeftMouseClick(Buttons["FCC" + randCall][0], Buttons["FCC" + randCall][1]);
            Log("Front Car Call " + randCall + "\n");
            waitEvent("FDO", "DoorOpen", "Door Opened\n");
            waitEvent("FDC", "DoorClosed", "Door Closed\n", 5.0);
            Thread.Sleep(1000);

            LeftMouseClick(Buttons["DBCH"][0], Buttons["DBCH"][1]);
            Log("Activated Door Close Button\n");
            waitEvent("FDC", "DoorClosed", "Door Closed\n", 5.0);
            LeftMouseClick(Buttons["DBCH"][0], Buttons["DBCH"][1]);
            Thread.Sleep(1000);

            LeftMouseClick(Buttons["FCDH"][0], Buttons["FCDH"][1]);
            Log("CD Jumper Removed\n");
            Thread.Sleep(1000);
            LeftMouseClick(Buttons["FHDH"][0], Buttons["FHDH"][1]);
            Log("HD Jumper Removed\n");
            Thread.Sleep(1000);
            if (!waitEvent("FDO", "DoorClosed", "Door Closed\n", 5))
            {
                Log("Toggle Inspection Switch to clear Fault\n");
                LeftMouseClick(Buttons["INL"][0], Buttons["INL"][1]);
                Thread.Sleep(1000);
                LeftMouseClick(Buttons["INL"][0], Buttons["INL"][1]);
                waitEvent("FDO", "DoorClosed", "Door Closed\n");
            }
            Thread.Sleep(5000);
        }

        private void DLMFireServicePhase1()
        {
            ClearLog();
            Log("\nInitiate Fire Service DLM Test\n");

            //Get Fire Service Phase 1 Main Recall Floor
            LeftMouseClick(Buttons["FRSL"][0], Buttons["FRSL"][1]);
            Log("\nInitiate Fire Phase 1 - Main\n");
            waitEvent("FDO", "DoorOpen", "Finished Recall\nDoor Opened\n");
            UpdatePosition();
            MainRecallFloor = this.position;
            Thread.Sleep(1000);
            LeftMouseClick(Buttons["FRSL"][0], Buttons["FRSL"][1]);
            Thread.Sleep(3000);

            int top_landing = content.HexStringToDecimal(content.Get_Byte("BOTTOM:", 2)) + 1;

            Random rand = new Random();
            int randCall = 0;

            //CD Jumpered Test
            LeftMouseClick(Buttons["FCDH"][0], Buttons["FCDH"][1]);
            Log("\nCD Jumpered\n");
            Thread.Sleep(1000);

            do
            {
                randCall = rand.Next(1, top_landing);
            } while (randCall == MainRecallFloor);

            LeftMouseClick(Buttons["FCC" + randCall][0], Buttons["FCC" + randCall][1]);
            Log("Front Car Call " + randCall + "\n");
            waitEvent("FDO", "DoorOpen", "Door Opened\n");
            waitEvent("FDC", "DoorClosed", "Door Closed\n", 5.0);
            Thread.Sleep(1000);
            LeftMouseClick(Buttons["FRSL"][0], Buttons["FRSL"][1]);
            Log("Initiate Fire Phase 1 - Main\n");
            waitEvent("FDC", "DoorClosed", "Door Closed\n", 5.0);


            LeftMouseClick(Buttons["FCDH"][0], Buttons["FCDH"][1]);
            Log("CD Jumper Removed\n");
            if (!waitEvent("FDO", "DoorClosed", "Door Closed: Begin Recall\n", 5))
            {
                Log("Toggle Inspection Switch to clear Fault\n");
                LeftMouseClick(Buttons["INL"][0], Buttons["INL"][1]);
                Thread.Sleep(1000);
                LeftMouseClick(Buttons["INL"][0], Buttons["INL"][1]);
                waitEvent("FDO", "DoorClosed", "Door Closed: Begin Recall\n");
            }
            waitEvent("FDO", "DoorOpen", "Finished Recall\nDoor Opened\n");
            Thread.Sleep(3000);
            LeftMouseClick(Buttons["FRSL"][0], Buttons["FRSL"][1]);
            Log("Deactivate Fire Phase 1\n");
            Thread.Sleep(1000);

            //HD Jumpered Test
            LeftMouseClick(Buttons["FHDH"][0], Buttons["FHDH"][1]);
            Log("\nHD Jumpered\n");
            Thread.Sleep(1000);

            do
            {
                randCall = rand.Next(1, top_landing);
            } while (randCall == MainRecallFloor);

            LeftMouseClick(Buttons["FCC" + randCall][0], Buttons["FCC" + randCall][1]);
            Log("Front Car Call " + randCall + "\n");
            waitEvent("FDO", "DoorOpen", "Door Opened\n");
            waitEvent("FDC", "DoorClosed", "Door Closed\n", 5.0);
            Thread.Sleep(1000);
            LeftMouseClick(Buttons["FRSL"][0], Buttons["FRSL"][1]);
            Log("Initiate Fire Phase 1 - Main\n");
            waitEvent("FDC", "DoorClosed", "Door Closed\n", 5.0);


            LeftMouseClick(Buttons["FHDH"][0], Buttons["FHDH"][1]);
            Log("HD Jumper Removed\n");
            if (!waitEvent("FDO", "DoorClosed", "Door Closed: Begin Recall", 5))
            {
                Log("Toggle Inspection Switch to clear Fault\n");
                LeftMouseClick(Buttons["INL"][0], Buttons["INL"][1]);
                Thread.Sleep(1000);
                LeftMouseClick(Buttons["INL"][0], Buttons["INL"][1]);
                waitEvent("FDO", "DoorClosed", "Door Closed: Begin Recall\n");
            }
            waitEvent("FDO", "DoorOpen", "Finished Recall\nDoor Opened\n");
            Thread.Sleep(3000);
            LeftMouseClick(Buttons["FRSL"][0], Buttons["FRSL"][1]);
            Log("Deactivate Fire Phase 1\n");
            Thread.Sleep(1000);

            //CD & HD Jumpered Test
            LeftMouseClick(Buttons["FCDH"][0], Buttons["FCDH"][1]);
            Log("\nCD Jumpered\n");
            Thread.Sleep(1000);
            LeftMouseClick(Buttons["FHDH"][0], Buttons["FHDH"][1]);
            Log("HD Jumpered\n");
            Thread.Sleep(1000);

            do
            {
                randCall = rand.Next(1, top_landing);
            } while (randCall == MainRecallFloor);

            LeftMouseClick(Buttons["FCC" + randCall][0], Buttons["FCC" + randCall][1]);
            Log("Front Car Call " + randCall + "\n");
            waitEvent("FDO", "DoorOpen", "Door Opened\n");
            waitEvent("FDC", "DoorClosed", "Door Closed\n", 5.0);
            Thread.Sleep(1000);
            LeftMouseClick(Buttons["FRSL"][0], Buttons["FRSL"][1]);
            Log("Initiate Fire Phase 1 - Main\n");
            waitEvent("FDC", "DoorClosed", "Door Closed\n", 5.0);


            LeftMouseClick(Buttons["FCDH"][0], Buttons["FCDH"][1]);
            Log("CD Jumper Removed\n");
            Thread.Sleep(1000);
            LeftMouseClick(Buttons["FHDH"][0], Buttons["FHDH"][1]);
            Log("HD Jumper Removed\n");
            if(!waitEvent("FDO", "DoorClosed", "Door Closed: Begin Recall\n",5))
            {
                Log("Toggle Inspection Switch to clear Fault\n");
                LeftMouseClick(Buttons["INL"][0], Buttons["INL"][1]);
                Thread.Sleep(1000);
                LeftMouseClick(Buttons["INL"][0], Buttons["INL"][1]);
                waitEvent("FDO", "DoorClosed", "Door Closed: Begin Recall\n");
            }
            waitEvent("FDO", "DoorOpen", "Finished Recall\nDoor Opened\n");
            Thread.Sleep(3000);
            LeftMouseClick(Buttons["FRSL"][0], Buttons["FRSL"][1]);
            Log("Deactivate Fire Phase 1\n");
            Thread.Sleep(1000);

        }

        private void DLMFireServicePhase2()
        {

        }

        private List<int> Landings()
        {
            List<int> landings = new List<int>();

            int startPixel = 601;
            int endPixel = 183;
            int[] black = new int[] { 0, 0, 0 };

            for(int y = startPixel; y > endPixel; y--)
            {
                int[] color = getColor(new int[] { 133, y });
                if (Enumerable.SequenceEqual(color,black))
                {
                    landings.Add(y);
                }
            }

            return landings;
        }

        private void UpdatePosition()
        {
            int[] On = new int[] { 182, 215, 83 };
            int[] Off = new int[] { 0, 64, 0 };
            int yPos = 440;
            int[] encodings = new int[] { 1270, 1246, 1222, 1198, 1174, 1150, 1126, 1102 };
            int total = 0;
            int bin = 0;

            foreach(int encoding in encodings)
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
    }
}
