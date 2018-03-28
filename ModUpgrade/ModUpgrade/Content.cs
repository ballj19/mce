using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModUpgrade
{
    public class Content
    {
        public List<string> lines = new List<string>();
        public string file;
        public List<string> labels = new List<string>();
        public List<int> labelsInt = new List<int>();


        public Content(string file)
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

                if (line == "")
                {
                    lines.Add("");
                }
                else
                {
                    //Need this logic because sometimes the first byte is defined on the same line as the label - inconsistently
                    //So we force the defined byte onto the next line always
                    if (Value(line).Contains(":") && Value(line).Trim().EndsWith(":") == false)
                    {
                        int commentIndex = line.IndexOf(";");
                        int colonIndex = line.IndexOf(":");

                        if(commentIndex == -1)
                        {
                            lines.Add(line.Substring(0, colonIndex + 1).Trim());
                            lines.Add(line.Substring(colonIndex + 1, line.Length - colonIndex - 1));
                        }
                        else
                        {
                            if(commentIndex > colonIndex)
                            {
                                lines.Add(line.Substring(0, colonIndex + 1).Trim());
                                lines.Add(line.Substring(colonIndex + 1, line.Length - colonIndex - 1));
                            }
                            else
                            {
                                lines.Add(line);
                            }
                        }
                    }
                    else
                    {
                        if(line.Trim().EndsWith(":"))
                        {
                            lines.Add(line.Trim());
                        }
                        else if(line.Trim() == "END")
                        {
                            lines.Add(line.Trim());
                        }
                        else
                        {
                            lines.Add(line);
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
                if(line.StartsWith("PUBLIC"))
                {
                    string label = line.Substring(6, line.Length - 6);
                    labels.Add(label.Trim());
                    labelsInt.Add(l);
                }
                l++;
            }
        }

        private string Value(string line)
        {
            if (line.IndexOf(";") == -1) //indexOf returns -1 if string not found
            {
                return line;
            }
            else
            {
                int commentIndex = line.IndexOf(";");
                return line.Substring(0, commentIndex).Trim();
            }
        }

        private string Comment(string line)
        {
            if (line.IndexOf(";") == -1) //indexOf returns -1 if string not found
            {
                return "";
            }
            else
            {
                int commentIndex = line.IndexOf(";");
                return line.Substring(commentIndex, line.Length - commentIndex).Trim();
            }
        }
    }
}
