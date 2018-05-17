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

namespace mods
{
    /// <summary>
    /// Interaction logic for VersionIO.xaml
    /// </summary>
    public partial class VersionIO : Window
    {
        private List<string> activeInputs;
        private List<string> activeOutputs;
        public List<string> addedInputs = new List<string>();
        public List<string> removedInputs = new List<string>();
        public List<string> addedOutputs = new List<string>();
        public List<string> removedOutputs = new List<string>();
        public List<string> finalInputs = new List<string>();
        public List<string> finalOutputs = new List<string>();
        private List<string> endInputs = new List<string>();
        private List<string> endOutputs = new List<string>();
        public bool allowToggleActiveIO = false;

        public VersionIO(List<string> activeInputs, List<string> activeOutputs)
        {
            InitializeComponent();

            this.activeInputs = activeInputs;
            this.activeOutputs = activeOutputs;
        }

        public void PopulateIO(List<string> ios, string type)
        {
            if(allowToggleActiveIO)
            {
                SubmitButton.Visibility = Visibility.Visible;
            }
            else
            {
                SubmitButton.Visibility = Visibility.Hidden;
            }

            foreach (string io in ios)
            {
                TextBox ioBox = new TextBox
                {
                    Height = 50,
                    IsReadOnly = true,
                    Margin = new Thickness(0, 0, 5, 10),
                    Text = io,
                    Width = 75,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Center,
                };

                if(allowToggleActiveIO)
                {
                    ioBox.MouseDoubleClick += Toggle_Active_IO;
                }

                if (type == "inputs")
                {
                    if (activeInputs.Contains(io))
                    {
                        ioBox.Background = System.Windows.Media.Brushes.LightGreen;
                    }
                    else
                    {
                        ioBox.Background = System.Windows.Media.Brushes.Transparent;
                    }

                    InputsSP.Children.Add(ioBox);

                }
                else
                {
                    if (activeOutputs.Contains(io))
                    {
                        ioBox.Background = System.Windows.Media.Brushes.LightGreen;
                    }
                    else
                    {
                        ioBox.Background = System.Windows.Media.Brushes.Transparent;
                    }

                    OutputsSP.Children.Add(ioBox);
                }             
            }            
        }

        private void Toggle_Active_IO(object sender, RoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            WrapPanel ioPanel = tb.Parent as WrapPanel;

            if(ioPanel.Name == "InputsSP")
            {
                if(tb.Background == System.Windows.Media.Brushes.LightGreen)
                {
                    tb.Background = System.Windows.Media.Brushes.Transparent;
                }
                else
                {
                    tb.Background = System.Windows.Media.Brushes.LightGreen;
                }
            }
            else if(ioPanel.Name == "OutputsSP")
            {
                if (tb.Background == System.Windows.Media.Brushes.LightGreen)
                {
                    tb.Background = System.Windows.Media.Brushes.Transparent;
                }
                else
                {
                    tb.Background = System.Windows.Media.Brushes.LightGreen;
                }
            }
        }

        private void Filter_TextChanged(object sender, TextChangedEventArgs e)
        {
            foreach (TextBox child in InputsSP.Children)
            {
                bool foundText = false;
                if (child.Text.ToLower().Contains(Filter.Text.ToLower()))
                {
                    foundText = true;
                }

                if (foundText)
                {
                    child.Height = 50;
                    child.Width = 75;
                    child.Margin = new Thickness(0, 0, 5, 10);
                }
                else
                {
                    child.Height = 0;
                    child.Width = 0;
                    child.Margin = new Thickness(0, 0, 0, 0);
                }
            }

            foreach (TextBox child in OutputsSP.Children)
            {
                bool foundText = false;
                if (child.Text.ToLower().Contains(Filter.Text.ToLower()))
                {
                    foundText = true;
                }

                if (foundText)
                {
                    child.Height = 50;
                    child.Width = 75;
                    child.Margin = new Thickness(0, 0, 5, 10);
                }
                else
                {
                    child.Height = 0;
                    child.Width = 0;
                    child.Margin = new Thickness(0, 0, 0, 0);
                }
            }
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            foreach(TextBox tb in InputsSP.Children)
            {
                if(tb.Background == System.Windows.Media.Brushes.Transparent)
                {
                    finalInputs.Add("0");
                }
                else
                {
                    finalInputs.Add("1");
                    endInputs.Add(tb.Text);
                }
            }
            foreach (TextBox tb in OutputsSP.Children)
            {
                if (tb.Background == System.Windows.Media.Brushes.Transparent)
                {
                    finalOutputs.Add("0");
                }
                else
                {
                    finalOutputs.Add("1");
                    endOutputs.Add(tb.Text);
                }
            }

            Calculate_Add_Rem_IO();

            this.Close();
        }

        private void Calculate_Add_Rem_IO()
        {
            foreach(string activeInput in activeInputs)
            {
                if(!endInputs.Contains(activeInput))
                {
                    removedInputs.Add(activeInput);
                }
            }
            foreach (string endInput in endInputs)
            {
                if (!activeInputs.Contains(endInput))
                {
                    addedInputs.Add(endInput);
                }
            }

            foreach (string activeoutput in activeOutputs)
            {
                if (!endOutputs.Contains(activeoutput))
                {
                    removedOutputs.Add(activeoutput);
                }
            }
            foreach (string endoutput in endOutputs)
            {
                if (!activeOutputs.Contains(endoutput))
                {
                    addedOutputs.Add(endoutput);
                }
            }
        }
    }
}
