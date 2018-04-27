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
        public VersionIO(List<string> inputs, List<string> outputs)
        {
            InitializeComponent();

            PopulateIO(inputs,"inputs");
            PopulateIO(outputs,"outputs");
        }

        private void PopulateIO(List<string> ios, string type)
        {
            foreach(string io in ios)
            {
                TextBox ioBox = new TextBox
                {
                    Height = 50,
                    Background = System.Windows.Media.Brushes.Transparent,
                    IsReadOnly = true,
                    Margin = new Thickness(0, 0, 5, 10),
                    Text = io,
                    Width = 75,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Center
                };

                if (!"XXXXX".Contains(io))
                {
                    if (type == "inputs")
                    {
                        InputsSP.Children.Add(ioBox);
                    }
                    else
                    {
                        OutputsSP.Children.Add(ioBox);
                    }
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

    }
}
