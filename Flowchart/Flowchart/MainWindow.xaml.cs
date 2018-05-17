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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Flowchart
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<string> content = new List<string>();
        public List<string> instructions = new List<string>();
        public List<string> labels = new List<string>();
        public List<string[]> operands = new List<string[]>();
        public List<string> windows = new List<string>();
        public int currentPosition = 0;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void CreateFlowchart_Click(object sender, RoutedEventArgs e)
        {
            Content content = new Content();

            this.content = content.content;
            this.instructions = content.instructions;
            this.labels = content.labels;
            this.operands = content.operands;

            currentPosition = Starting_Label();
            Generate_Window();
        }

        private int Starting_Label()
        {
            foreach(string label in labels)
            {
                if(label != "")
                {
                    return labels.IndexOf(label);
                }
            }
            return 0;
        }

        private void Generate_Window()
        {
            List<string> stopInstructions = new List<string> { "SJMP", "JB", "JNB", "JMP", "JC", "JNC", "JBC", "JZ", "JNZ" };

            ContentWindowSP.Children.Clear();

            for(int line = 0; line < content.Count; line++)
            {
                StackPanel lineSP = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                };

                TextBox label = new TextBox
                {
                    Name = labels[line],
                    Text = labels[line],
                };
                TextBox instruction = new TextBox
                {
                    Text = instructions[line],
                    Margin = new Thickness(40,0,0,0),
                };
                TextBox operand0 = new TextBox
                {
                    Text = operands[line][0],
                    Margin = new Thickness(0, 0, 0, 0),
                    
                };
                TextBox operand1 = new TextBox
                {
                    Text = operands[line][1],
                    Margin = new Thickness(5, 0, 0, 0),
                };
                TextBox operand2 = new TextBox
                {
                    Text = operands[line][2],
                    Margin = new Thickness(5, 0, 0, 0),
                };

                operand0.MouseDoubleClick += Label_Focus;
                operand1.MouseDoubleClick += Label_Focus;
                operand2.MouseDoubleClick += Label_Focus;

                if(label.Text != "")
                {
                    lineSP.Children.Add(label);
                    lineSP.Children.Add(new TextBox { Text = ":" });
                }
                if (instruction.Text != "")
                {
                    lineSP.Children.Add(instruction);
                    lineSP.Children.Add(new TextBox { Text = "\t\t" });
                }
                if (operand0.Text != "")
                {
                    lineSP.Children.Add(operand0);
                }
                if (operand1.Text != "")
                {
                    lineSP.Children.Add(new TextBox { Text = "," });
                    lineSP.Children.Add(operand1);
                }
                if (operand2.Text != "")
                {
                    lineSP.Children.Add(new TextBox { Text = "," });
                    lineSP.Children.Add(operand2);
                }

                ContentWindowSP.Children.Add(lineSP);
            }
        }

        private void Label_Focus(object sender, RoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;

            foreach(StackPanel childsp in ContentWindowSP.Children)
            {
                foreach(TextBox childtb in childsp.Children)
                {
                    if(childtb.Name == tb.Text)
                    {
                        var point = GetPosition(childtb);
                        SV.ScrollToVerticalOffset(point.Y);
                    }
                }
            }
        }

        private Point GetPosition(Visual element)
        {
            var positionTransform = element.TransformToAncestor(SV);
            var areaPosition = positionTransform.Transform(new Point(0, 0));

            return areaPosition;
        }
    }
}
