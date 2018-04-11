using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace ModHubUpdater
{
    class Program
    {
        static void Main(string[] args)
        {
            string versionPath = "";

            //To get the location the assembly normally resides on disk or the install directory
            string path = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;

            //once you have the path you get the directory with:
            string modsPath = System.IO.Path.GetDirectoryName(path) + "\\mods.exe";
            modsPath = modsPath.Substring(6, modsPath.Length - 6);

            Console.Write("Fetching Update");
            Thread.Sleep(4000);

            List<string> versions = new List<string>();
            versions = System.IO.File.ReadAllLines(@"\\\\amrappfil01\\MCE-Rancho\\Jake Ball\\Versions.txt").ToList();
            
            foreach(string version in versions)
            {
                if(version.StartsWith("ModHub"))
                {
                    int colonIndex = version.IndexOf(":");
                    versionPath = version.Substring(colonIndex + 1, version.Length - colonIndex - 1);
					int semicolonIndex = versionPath.IndexOf(";");
					versionPath = versionPath.Substring(0,semicolonIndex);
                }
            }

            File.Copy(@versionPath, @modsPath,true);

            string cmd = "C:\\Windows\\explorer.exe";
            string arg = modsPath;
            Process.Start(cmd, arg);
        }
    }
}
