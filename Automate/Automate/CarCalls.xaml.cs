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

namespace Automate
{
    /// <summary>
    /// Interaction logic for CarCalls.xaml
    /// </summary>
    public partial class CarCalls : Window
    {
        List<int> frontCalls = new List<int>();
        List<int> rearCalls = new List<int>();
        public List<string> car_calls = new List<string>();

        public CarCalls(List<string> carCalls, int topLanding)
        {
            InitializeComponent();

            Get_Front_and_Rear_Calls(carCalls);
            Create_Call_Boxes(topLanding);
        }

        private void Get_Front_and_Rear_Calls(List<string> carCalls)
        {
            foreach(string call in carCalls)
            {
                int landingNum = Int32.Parse(call.Substring(3, call.Length - 3));
                if(call.Contains("CCF"))
                {
                    frontCalls.Add(landingNum);
                }
                else
                {
                    rearCalls.Add(landingNum);
                }
            }
        }

        private void Create_Call_Boxes(int topLanding)
        {
            for(int landing = topLanding; landing > 0; landing--)
            {
                StackPanel sp = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                Label landingLabel = new Label
                {
                    Content = landing.ToString(),
                    HorizontalContentAlignment = HorizontalAlignment.Right
                };

                CheckBox frontCB = new CheckBox
                {
                    Margin = new Thickness(10, 10, 0, 0),
                    Name = "CCF" + landing,
                };

                CheckBox rearCB = new CheckBox
                {
                    Margin = new Thickness(10, 10, 0, 0),
                    Name = "CCR" + landing,
                };

                if(frontCalls.Contains(landing))
                {
                    frontCB.IsChecked = true;
                }
                if(rearCalls.Contains(landing))
                {
                    rearCB.IsChecked = true;
                }

                sp.Children.Add(landingLabel);
                sp.Children.Add(frontCB);
                sp.Children.Add(rearCB);
                LandingsSP.Children.Add(sp);
            }
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            foreach(StackPanel sp in LandingsSP.Children)
            {
                foreach(var child in sp.Children)
                {
                    if(child.GetType() == typeof(CheckBox))
                    {
                        CheckBox cb = child as CheckBox;

                        if(cb.IsChecked == true)
                        {
                            car_calls.Add(cb.Name);
                        }
                    }
                }
            }

            this.Close();
        }
    }
}
