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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Flowchart
{
    /// <summary>
    /// Interaction logic for Chart.xaml
    /// </summary>
    public partial class Chart : Window
    {
        public List<string> content = new List<string>();
        public List<string> instructions = new List<string>();
        public List<string> labels = new List<string>();
        public List<string[]> operands = new List<string[]>();
        public double chartHeight;
        public List<ChartBox> chartboxes = new List<ChartBox>();
        public List<JumpBox> jumpboxes = new List<JumpBox>();

        public Chart(List<string> content, List<string> labels, List<string> instructions, List<string[]> operands)
        {
            InitializeComponent();

            this.content = content;
            this.instructions = instructions;
            this.labels = labels;
            this.operands = operands;

            Identify_Boxes();
        }

        private void Identify_Boxes()
        {
            int i = 0;

            string[] stopInstructions = { "SJMP", "JB", "JNB", "JMP", "JC", "JNC", "JBC", "JZ", "JNZ" };

            while (i < content.Count)
            {
                if(labels[i] != "")
                {
                    chartboxes.Add(new ChartBox(i, labels[i]));
                }
                if(stopInstructions.Contains(instructions[i]))
                {
                    string label = "";

                    if(operands[2][i] != "")
                    {
                        label = operands[2][i];
                    }
                    else if (operands[1][i] != "")
                    {
                        label = operands[1][i];
                    }
                    else
                    {
                        label = operands[0][i];
                    }

                    jumpboxes.Add(new JumpBox(i, label, content[i]));
                }
            }
        }

        private void Draw_Lanes()
        {
            List<Lane> lanes = new List<Lane>();

            List<SolidColorBrush> colors = new List<SolidColorBrush>();
            colors.Add(Brushes.Red);
            colors.Add(Brushes.Orange);
            colors.Add(Brushes.Yellow);
            colors.Add(Brushes.Green);
            colors.Add(Brushes.Blue);
            colors.Add(Brushes.Indigo);
            colors.Add(Brushes.Violet);

            List<char> sides = new List<char>();
            sides.Add('r');
            sides.Add('l');

            List<int> offsets = new List<int>();
            offsets.Add(30);
            offsets.Add(60);
            offsets.Add(90);
            offsets.Add(120);
            offsets.Add(150);
            offsets.Add(180);

            foreach (SolidColorBrush color in colors)
            {
                foreach(int offset in offsets)
                {
                    foreach (char side in sides)
                    {
                        lanes.Add(new Lane(side, offset, color));
                    }
                }
            }
        }

        public void Draw_Branches()
        {
            Line branch = new Line();

            branch.X1 = 525;
            branch.X2 = 525;
            branch.Y2 = branch.Y1 + 25;

            branch.Stroke = Brushes.Black;
            branch.StrokeThickness = 2;

            sp1.Children.Add(branch);
            
        }
    }

    public class Lane
    {
        char side;
        int offset;
        System.Windows.Media.SolidColorBrush color;

        public Lane(char side,int offset, System.Windows.Media.SolidColorBrush color)
        {
            this.side = side;
            this.offset = offset;
            this.color = color;
        }
    }

    public class ChartBox
    {
        public int index;
        public string label;
        public string content;

        public ChartBox(int index, string label)
        {
            this.index = index;
            this.label = label;
        }
    }

    public class JumpBox
    {
        public int startIndex;
        public string label;
        public string content;

        public JumpBox(int startIndex, string label, string content)
        {
            this.startIndex = startIndex;
            this.label = label;
            this.content = content;
        }
    }
}
