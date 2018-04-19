using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flowchart
{
    public class Content
    {
        public List<string> content = new List<string>();
        public List<string> instructions = new List<string>();
        public List<string> labels = new List<string>();
        public List<string[]> operands = new List<string[]>();

        public Content()
        {
            string path = @"C:\Users\jacob.ball\Desktop\Docs\test.asm";
            Get_Content(General.Get_Clean_Lines_From_Path(path));
            Parse_Lines();
        }

        private void Get_Content(List<string> lines)
        {
            int x = 0;
            string[] uncommentedLines = new string[lines.Count];
            foreach (string line in lines)
            {
                if (lines[x].IndexOf(";") == -1) //indexOf returns -1 if string not found
                {
                    uncommentedLines[x] = lines[x];  //do nothing
                }
                else
                {
                    int commentIndex = lines[x].IndexOf(";");
                    uncommentedLines[x] = lines[x].Substring(0, lines[x].IndexOf(";"));
                }
                uncommentedLines[x] = uncommentedLines[x].Trim();
                x++;
            }

            foreach (string line in uncommentedLines)
            {
                if (line == "")
                {
                    //empty line, do noting
                }
                else
                {
                    content.Add(line.ToUpper());
                }
            }
        }

        private void Parse_Lines()
        {
            foreach (string line in content)
            {
                if (line.Contains(':'))
                {
                    int colonIndex = line.IndexOf(':');

                    string label = line.Substring(0, colonIndex);
                    labels.Add(label);

                    if (line.Length > colonIndex + 1) //This block will not run if the label is by itself on a line with no instruction
                    {

                        string instructionString = line.Substring(colonIndex + 1, line.Length - colonIndex - 1).Trim();
                        StringBuilder instruction = new StringBuilder();

                        int x = 0;
                        while (x < line.Length && Char.IsLetter(instructionString[x]))
                        {
                            instruction.Append(instructionString[x]);
                            x++;
                        }

                        instructions.Add(instruction.ToString());

                        string[] operand = { "", "", "" };

                        string operandString = instructionString.Substring(x, instructionString.Length - x).Trim();

                        int i = 0;

                        while (operandString.Contains(','))
                        {
                            int commaIndex = operandString.IndexOf(',');
                            operand[i] = operandString.Substring(0, commaIndex).Trim();
                            operandString = operandString.Substring(commaIndex + 1, operandString.Length - commaIndex - 1).Trim();
                            i++;
                        }

                        operand[i] = operandString;

                        operands.Add(operand);
                    }
                    else
                    {
                        instructions.Add("");
                        string[] operand = { "", "", "" };
                        operands.Add(operand);
                    }
                }
                else
                {
                    labels.Add("");

                    string instructionString = line;

                    StringBuilder instruction = new StringBuilder();

                    int x = 0;
                    while (x < line.Length && Char.IsLetter(instructionString[x]))
                    {
                        instruction.Append(instructionString[x]);
                        x++;
                    }

                    instructions.Add(instruction.ToString());

                    string[] operand = { "", "", "" };

                    string operandString = instructionString.Substring(x, instructionString.Length - x).Trim();

                    int i = 0;

                    while(operandString.Contains(','))
                    {
                        int commaIndex = operandString.IndexOf(',');
                        operand[i] = operandString.Substring(0, commaIndex).Trim();
                        operandString = operandString.Substring(commaIndex + 1, operandString.Length - commaIndex - 1).Trim();
                        i++;
                    }

                    operand[i] = operandString;

                    operands.Add(operand);
                }
            }
        }
    }
}
