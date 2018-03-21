using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace mods
{
    /// <summary>
    /// Interaction logic for PrintPreview.xaml
    /// </summary>
    public partial class PrintPreview : Window
    {
        private MainWindow mainWindow;

        public PrintPreview(MainWindow mainWindow)
        {
            InitializeComponent();

            this.mainWindow = mainWindow;
            Landing_Config();
            
        }

        private void Print_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog printDialog = new PrintDialog();
            if (printDialog.ShowDialog().GetValueOrDefault(false))
            {
                printDialog.PrintVisual(this, this.Title);
            }
        }

        private void TogglePIs_Click(object sender, RoutedEventArgs e)
        {
            if (LandingPIs.Visibility == Visibility.Visible)
            {
                LandingPIs.Visibility = Visibility.Hidden;
                LandingLevels.Visibility = Visibility.Visible;
                TogglePIs.Content = "PI";
            }
            else
            {
                LandingPIs.Visibility = Visibility.Visible;
                LandingLevels.Visibility = Visibility.Hidden;
                TogglePIs.Content = "#";
            }
        }

        private void Landing_Config()
        {
            LandingConfig = mainWindow.LandingConfig;
            LandingNormalConfig.Text = mainWindow.LandingNormalConfig.Text;
            LandingAltConfig.Text = mainWindow.LandingAltConfig.Text;

            LandingLevels.BorderThickness = new System.Windows.Thickness(2);
            LandingPIs.BorderThickness = new System.Windows.Thickness(2);
            LandingNormalConfig.BorderThickness = new System.Windows.Thickness(2);
            LandingAltConfig.BorderThickness = new System.Windows.Thickness(2);

            LandingNormalHeader.Width = 96;
            LandingNormalConfig.Width = 96;
            LandingAltHeader.Width = 96;
            LandingAltConfig.Width = 96;

            LandingNormalConfig.Height = mainWindow.LandingNormalConfig.Height;
            LandingAltConfig.Height = mainWindow.LandingAltConfig.Height;
        }
    }
}
