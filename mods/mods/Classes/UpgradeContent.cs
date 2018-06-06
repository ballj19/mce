using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mods
{
    public class UpgradeContent
    {
        public List<string> lines = new List<string>();
        public string file;
        public List<string> labels = new List<string>();
        public List<int> labelsInt = new List<int>();


        public UpgradeContent(string file)
        {
            this.file = file;
            Get_Content(file);
            Get_Labels();
        }

        private void Get_Content(string file)
        {
            List<string> rawLines = new List<string>();
            List<string> lines = new List<string>();
            List<string> values = new List<string>();
            List<string> comments = new List<string>();
            try
            {
                string path = file;
                rawLines = System.IO.File.ReadAllLines(@path).ToList();
            }
            catch
            {
                    
            }

            foreach (string line in rawLines)
            {
                if(line.Contains("ELIGI:"))
                {
                    //debug
                }

                if (line == "")
                {
                    lines.Add("");
                }
                else
                {
                    //Need this logic because sometimes the first byte is defined on the same line as the label - inconsistently
                    //So we force the defined byte onto the next line always
                    if (General.Value(line).Contains(":") && (!General.Value(line).EndsWith(":") || !General.Comment(line).Equals("")))
                    {
                        int colonIndex = line.IndexOf(":");
                        lines.Add(line.Substring(0, colonIndex + 1).Trim());
                        lines.Add("\t" + line.Substring(colonIndex + 1, line.Length - colonIndex - 1).Trim());
                    }
                    else
                    {
                        if(line.Trim().EndsWith(":"))
                        {
                            lines.Add(line.Trim());
                        }
                        else
                        {
                            char[] trim = { '\u001a' };
                            string trimmedLine = line.Trim(trim);
                            trimmedLine = trimmedLine.Trim();
                            if (trimmedLine == "END")
                            {
                                lines.Add('\t' + trimmedLine);
                            }
                            else
                            {
                                lines.Add(line);
                            }
                        }
                    }
                }
            }
            this.lines = lines;
        }

        private void Get_Labels()
        {
            int l = 0;
            foreach (string line in lines)
            {
                if(General.Value(line).EndsWith(":"))
                {
                    string label = line.Substring(0, line.Length - 1);
                    labels.Add(label.Trim());
                    labelsInt.Add(l);
                }
                l++;
            }
        }
    }
}
