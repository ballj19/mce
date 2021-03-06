﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace SoftwareTrackerUpdater
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string versionPath = "";

                //To get the location the assembly normally resides on disk or the install directory
                string path = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;

                //once you have the path you get the directory with:
                string modsPath = System.IO.Path.GetDirectoryName(path) + @"\SoftwareTracker.exe";
                modsPath = modsPath.Substring(6, modsPath.Length - 6);

                Console.Write("Fetching Update");
                Thread.Sleep(15000);

                List<string> versions = new List<string>();
                versions = System.IO.File.ReadAllLines(@"\\10.112.10.28\MCE-Rancho\Jake Ball\Versions.txt").ToList();

                foreach (string version in versions)
                {
                    if (version.StartsWith("SoftwareTracker"))
                    {
                        int colonIndex = version.IndexOf(":");
                        versionPath = version.Substring(colonIndex + 1, version.Length - colonIndex - 1);
                        int semicolonIndex = versionPath.IndexOf(";");
                        versionPath = versionPath.Substring(0, semicolonIndex);
                    }
                }

                File.Copy(versionPath, modsPath, true);

                string cmd = @"C:\Windows\explorer.exe";
                string arg = modsPath;
                Process.Start(cmd, arg);
            }
            catch (Exception ex)
            {
                using (System.IO.StreamWriter writefile =
                    new System.IO.StreamWriter(@"\\10.112.10.28\MCE-Rancho\Jake Ball\Error_Log.txt", true))
                {
                    DateTime now = DateTime.Now;
                    writefile.WriteLine("SoftwareTrackerUpdater[" + now.ToString() + "] " + Environment.UserName);
                    writefile.WriteLine(ex.ToString() + "\n");
                }
            }
        }
    }
}
