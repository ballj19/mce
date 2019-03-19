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
using IWshRuntimeLibrary;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace mods
{
    public class Motion : Controller
    {
        public override int topLandingHeight
        {
            get
            {
               return 16 * Int32.Parse(Get_Value("3. Top Landing Served (This car)")) + 10;
            }
        }

        public Motion(string file)
        {
            this.file = file;
            motioncontent = new MotionContent(file);
        }

        public override void Generate_Headers()
        {
            window.HeaderSP.Children.Clear();

            int top_landing = Int32.Parse(Get_Value("3. Top Landing Served (This car)"));
            List<string> cc_f = new List<string>();
            List<string> cc_r = new List<string>();
            List<string> hc_df = new List<string>();
            List<string> hc_uf = new List<string>();
            List<string> hc_dr = new List<string>();
            List<string> hc_ur = new List<string>();
            List<string> calls = new List<string>();

            for (int f = 1; f <= top_landing; f++)
            {
                string frontlevelValue = Get_Value("Serves Front1_BOX" + f);
                string front2levelValue = Get_Value("Serves Front2_BOX" + f);

                string rearlevelValue = Get_Value("Serves Rear1_BOX" + f);
                string rear2levelValue = Get_Value("Serves Rear2_BOX" + f);

                if(frontlevelValue == "1" || front2levelValue == "1")
                {
                    cc_f.Add("1" + f.ToString().PadLeft(2,'0'));

                    if(f != 1)
                    {
                        hc_df.Add("5" + f.ToString().PadLeft(2, '0'));
                    }
                    if (f != top_landing)
                    {
                        hc_uf.Add("6" + f.ToString().PadLeft(2, '0'));
                    }
                }

                if (rearlevelValue == "1" || rear2levelValue == "1")
                {
                    cc_r.Add("10" + f + "R");

                    if (f != 1)
                    {
                        hc_dr.Add("5" + f.ToString().PadLeft(2, '0') + "R");
                    }
                    if (f != top_landing)
                    {
                        hc_ur.Add("6" + f.ToString().PadLeft(2, '0') + "R");
                    }
                }
            }

            calls.AddRange(cc_f);
            calls.AddRange(cc_r);
            calls.AddRange(hc_df);
            calls.AddRange(hc_uf);
            calls.AddRange(hc_dr);
            calls.AddRange(hc_ur);


        }

        public override void Job_Info()
        {
            DateTime lastModified = System.IO.File.GetLastWriteTime(file);
            string jobName = Get_Value("Job Name:");
            string jobNumber = Get_Value("SalesOdr");
            string topFloor = Get_Value("3. Top Landing Served (This car)");
            string version = Get_Value("PCA_Ver#06.03.0000");
            string m2000 = Get_Value("M2000");
            string simplex = Get_Value("simplex");
            string duplex = Get_Value("Duplex");
            string rearDoors = "NO";
            string type = "";
            string subtype = "";
            if (Get_Value("PAGE1_YES1") == "1")
            {
                rearDoors = "YES";
            }
            if (m2000 == "1")
            {
                type = "M2000";
            }
            else
            {
                type = "M4000";
            }
            if (simplex == "1")
            {
                subtype = "Simplex";
            }
            else if (duplex == "1")
            {
                subtype = "Duplex";
            }
            else
            {
                subtype = "Local";
            }

            //Job Info
            window.JobInfo.Text = "";
            window.JobInfo.Text += file + "\n";
            window.JobInfo.Text += "Last Modified: " + lastModified.ToString("MM/dd/yy HH:mm:ss") + "\n\n";

            window.JobInfo.Text += jobName + "\n";
            window.JobInfo.Text += "Controller Type: " + type + " - " + subtype + "\n";
            window.JobInfo.Text += "Version: " + version + "\n\n";

            window.JobInfo.Text += "Top Floor: " + topFloor + "\n";
            window.JobInfo.Text += "Independent Rear Doors: " + rearDoors + "\n";
        }

        public override void Options()
        {
            throw new NotImplementedException();
        }

        protected override string Get_Fire_Code()
        {
            throw new NotImplementedException();
        }

        protected override void Set_Variables()
        {
            throw new NotImplementedException();
        }

        public override void Draw_Landing_Preview()
        {
            int top_landing = Int32.Parse(Get_Value("3. Top Landing Served (This car)"));

            window.LandingNormalHeader.Width = 96;
            window.LandingNormalConfig.Width = 96;
            window.LandingAltHeader.Width = 96;
            window.LandingAltConfig.Width = 96;

            window.LandingNormalConfig.Text = "";
            window.LandingNormalConfig.Height = 0;
            window.LandingNormalConfig.BorderThickness = new Thickness(0);
            window.LandingAltConfig.Text = "";
            window.LandingAltConfig.Height = 0;
            window.LandingAltConfig.BorderThickness = new Thickness(0);

            window.LandingLevels.Text = "";
            window.LandingLevels.Height = 16 * top_landing + 10;
            window.LandingLevels.BorderThickness = new Thickness(2);
            window.LandingPIs.Text = "";
            window.LandingPIs.Height = 16 * top_landing + 10;
            window.LandingPIs.BorderThickness = new Thickness(2);

            window.LandingNormalConfig.Text = "";
            window.LandingNormalConfig.Height = 16 * top_landing + 10;
            window.LandingNormalConfig.BorderThickness = new Thickness(2);

            for (int f = top_landing; f >= 1; f--)
            {
                window.LandingLevels.Text += f + "\n";

                string frontlevelValue = Get_Value("Serves Front1_BOX" + f);
                string rearlevelValue = Get_Value("Serves Rear1_BOX" + f);

                string front = ".";
                string rear = ".";

                if (frontlevelValue == "1")
                {
                    front = "F";
                }
                if (rearlevelValue == "1")
                {
                    rear = "R";
                }

                window.LandingNormalConfig.Text += front + " " + rear + "\n";
            }

            window.LandingLevels.Text = window.LandingLevels.Text.Substring(0, window.LandingLevels.Text.Length - 1); //Remove final \n
            window.LandingNormalConfig.Text = window.LandingNormalConfig.Text.Substring(0, window.LandingNormalConfig.Text.Length - 1); //Remove final \n
        }

        private string Get_Value(string option)
        {
            return motioncontent.values[option];
        }
    }
}
