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

namespace mods
{
    /// <summary>
    /// Interaction logic for Procedure.xaml
    /// </summary>
    public partial class Procedure : Window
    {
        public string filepath;

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

            this.content = new Content(filepath);
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
            {"DOBL", new int[] {424,366} }
        };

        private static readonly Dictionary<string, int[]> Colors = new Dictionary<string, int[]>
        {
            {"DoorOpen", new int[]{128,128,0} },
            {"DoorClosed", new int[] {192,192,192} }
        };

        private void Simulate_Click(object sender, RoutedEventArgs e)
        {
            Thread t = new System.Threading.Thread(() => DoTheLoop());
            t.Start();
        }

        private void DoTheLoop()
        {
            CarCallDictionary();
            //CarCalls();
            FireService();
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

        private void Log(string log)
        {
            ProcedureLog.Dispatcher.BeginInvoke(new Action(() => { ProcedureLog.Text += log; })); 
        }

        private void waitEvent(string button, string color, string log = "")
        {
            while (!Enumerable.SequenceEqual(getColor(Buttons[button]), Colors[color]))
            {
                // Wait
            }

            Log(log);
        }

        private void CarCalls()
        {
            int top_landing = content.HexStringToDecimal(content.Get_Byte("BOTTOM:", 2)) + 1;

            for (int x = 1; x <= top_landing; x++)
            {
                if (content.Get_Bit("ELIGIV:", x, 0, 3) == "YES")
                {
                    LeftMouseClick(Buttons["FCC" + x][0], Buttons["FCC" + x][1]);
                    Log("Front Car Call " + x + "\t\t");
                    waitEvent("FDO", "DoorOpen", "Door Opened\t\t");
                    waitEvent("FDC", "DoorClosed", "Door Closed\n");
                }
            }

            for (int x = 1; x <= top_landing; x++)
            {
                if (content.Get_Bit("ELIGIV:", x, 0, 2) == "YES")
                {
                    LeftMouseClick(Buttons["RCC" + x][0], Buttons["RCC" + x][1]);

                    Log("Rear Car Call " + x + "\t");
                    waitEvent("RDO", "DoorOpen", "Door Opened\t");
                    waitEvent("RDC", "DoorClosed", "Door Closed\n");
                }
            }
        }

        private void FireService()
        {
            LeftMouseClick(Buttons["FRSL"][0], Buttons["FRSL"][1]);
            Log("Initiate Fire Phase 1 - Main\t\t");
            waitEvent("FDO", "DoorOpen", "Finished Recall\tDoor Opened\n");

            LeftMouseClick(Buttons["FCSH"][0], Buttons["FCSH"][1]);
            Log("Initiate Fire Phase 2\t\t");
            Thread.Sleep(1000);
            Log("DBC\t");
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
                        Log("Front Car Call " + x + "\t\t");
                        Thread.Sleep(1000);
                        LeftMouseClick(Buttons["DOBH"][0], Buttons["DOBH"][1]);
                        waitEvent("FDO", "DoorOpen", "Door Opened\t\t");
                        LeftMouseClick(Buttons["DOBH"][0], Buttons["DOBH"][1]);
                        Thread.Sleep(1000);
                        LeftMouseClick(Buttons["DBCH"][0], Buttons["DBCH"][1]);
                        waitEvent("FDC", "DoorClosed", "Door Closed\n");
                        LeftMouseClick(Buttons["DBCH"][0], Buttons["DBCH"][1]);
                        Thread.Sleep(1000);
                    }
                    skip++;
                    if(skip == 3)
                    {
                        skip = 0;
                    }
                }
            }

            LeftMouseClick(Buttons["FCSH"][0], Buttons["FCSH"][1]);
            Thread.Sleep(1000);
            LeftMouseClick(Buttons["FCOFH"][0], Buttons["FCOFH"][1]);
            Thread.Sleep(1000);
            LeftMouseClick(Buttons["FCOFH"][0], Buttons["FCOFH"][1]);
            Log("End Fire Phase 2\n");

            Thread.Sleep(1000);
            LeftMouseClick(Buttons["FRSH"][0], Buttons["FRSH"][1]);
            Log("End Fire Phase 1\n");
        }
    }
}
