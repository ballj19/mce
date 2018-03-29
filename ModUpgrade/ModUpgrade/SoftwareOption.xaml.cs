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

namespace ModUpgrade
{
    /// <summary>
    /// Interaction logic for SoftwareOption.xaml
    /// </summary>
    public partial class SoftwareOption : Window
    {
        public int result = -1;

        public string optionType;

        public SoftwareOption(string title, int width, int height)
        {
            InitializeComponent();

            this.Width = width;
            this.Height = height;
            this.Title = title;
            SubmitButton.Margin = new Thickness(Width - SubmitButton.Width - 25, Height - SubmitButton.ActualHeight - 70, 0, 0);

        }

        public void Radio_Option(List<string> options)
        {
            foreach(string option in options)
            {
                RadioButton rb = new RadioButton
                {
                    Content = options[options.IndexOf(option)],
                    Margin = new Thickness(20, 0, 0, 10),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    GroupName = "G1",
                };

                optionSP.Children.Add(rb);
            }

            optionType = "Radio";
        }

        private void Close(object sender, System.ComponentModel.CancelEventArgs e)
        {
            
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            if (optionType == "Radio")
            {
                int c = 0;
                foreach (var child in optionSP.Children)
                {
                    RadioButton rb = child as RadioButton;

                    if (rb.IsChecked == true)
                    {
                        result = c;
                    }

                    c++;
                }
            }

            this.Close();
        }
    }
}
