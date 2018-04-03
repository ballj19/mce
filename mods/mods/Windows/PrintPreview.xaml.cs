using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Markup;
using System.IO;
using System.Xml;

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
            Job_Info();
            IO_Info();
            Header_Info();

            IOInfoSP.Visibility = Visibility.Hidden;
            BoardSP1.Visibility = Visibility.Hidden;
            BoardSP2.Visibility = Visibility.Hidden;
            JobInfo.Visibility = Visibility.Hidden;
        }

        private void Print_Click(object sender, RoutedEventArgs e)
        {
            Hide_Buttons();

            PrintDialog printDialog = new PrintDialog();
            if (printDialog.ShowDialog().GetValueOrDefault(false))
            {
                printDialog.PrintVisual(this, this.Title);

                Print_Section2();

                PrintDialog pd2 = printDialog;

                pd2.PrintVisual(this, this.Title);
            }

            this.Close();
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

        private void ToggleIOView_Click(object sender, RoutedEventArgs e)
        {
            if (IOInfoSPDummy.Visibility == Visibility.Visible)
            {
                IOInfoSPDummy.Visibility = Visibility.Hidden;
                BoardSPDummy1.Visibility = Visibility.Visible;
                BoardSPDummy2.Visibility = Visibility.Visible;
            }
            else
            {
                IOInfoSPDummy.Visibility = Visibility.Visible;
                BoardSPDummy1.Visibility = Visibility.Hidden;
                BoardSPDummy2.Visibility = Visibility.Hidden;
            }
        }

        private void Landing_Config()
        {
            LandingConfig = mainWindow.LandingConfig;
            LandingNormalConfig.Text = mainWindow.LandingNormalConfig.Text;
            LandingAltConfig.Text = mainWindow.LandingAltConfig.Text;
            LandingLevels.Text = mainWindow.LandingLevels.Text;
            LandingPIs.Text = mainWindow.LandingPIs.Text;

            LandingLevels.BorderThickness = new System.Windows.Thickness(2);
            LandingPIs.BorderThickness = new System.Windows.Thickness(2);
            LandingNormalConfig.BorderThickness = new System.Windows.Thickness(2);
            LandingAltConfig.BorderThickness = new System.Windows.Thickness(2);

            LandingNormalHeader.Width = 80;
            LandingNormalConfig.Width = 80;
            LandingAltHeader.Width = 80;
            LandingAltConfig.Width = 80;

            LandingNormalConfig.Height = mainWindow.LandingNormalConfig.Height;
            LandingAltConfig.Height = mainWindow.LandingAltConfig.Height;
            LandingPIs.Height = mainWindow.LandingPIs.Height;
            LandingLevels.Height = mainWindow.LandingLevels.Height;

            LandingAltHeader.Visibility = mainWindow.LandingAltHeader.Visibility;
            LandingAltConfig.Visibility = mainWindow.LandingAltConfig.Visibility;
            LandingPIs.Visibility = mainWindow.LandingPIs.Visibility;
            LandingLevels.Visibility = mainWindow.LandingLevels.Visibility;
        }

        private void Job_Info()
        {
            JobInfo.Text = mainWindow.JobInfo.Text;
            JobInfoDummy.Text = JobInfo.Text;
        }

        private void IO_Info()
        {
            bool alternate = true;
            foreach(System.Windows.UIElement child in mainWindow.BoardSP.Children)
            {
                string childXaml = XamlWriter.Save(child);

                StringReader stringReader = new StringReader(childXaml);
                XmlReader xmlReader = XmlReader.Create(stringReader);
                Border border = (Border)XamlReader.Load(xmlReader);
                
                string childXamlDummy = XamlWriter.Save(child);

                StringReader stringReaderDummy = new StringReader(childXamlDummy);
                XmlReader xmlReaderDummy = XmlReader.Create(stringReaderDummy);
                Border borderDummy = (Border)XamlReader.Load(xmlReaderDummy);
                
                if(alternate)
                {
                    BoardSPDummy1.Children.Add(borderDummy);
                    BoardSP1.Children.Add(border);
                }
                else
                {
                    BoardSPDummy2.Children.Add(borderDummy);
                    BoardSP2.Children.Add(border);
                }
                alternate = !alternate;
            }

            foreach (System.Windows.UIElement child in mainWindow.IOInfoSP.Children)
            {
                string childXaml = XamlWriter.Save(child);
                string childXamlDummy = XamlWriter.Save(child);

                if(child.GetType() == typeof(StackPanel))
                {
                    StringReader stringReader = new StringReader(childXaml);
                    XmlReader xmlReader = XmlReader.Create(stringReader);
                    StackPanel sp = (StackPanel)XamlReader.Load(xmlReader);

                    StringReader stringReaderDummy = new StringReader(childXamlDummy);
                    XmlReader xmlReaderDummy = XmlReader.Create(stringReaderDummy);
                    StackPanel spDummy = (StackPanel)XamlReader.Load(xmlReaderDummy);

                    IOInfoSP.Children.Add(sp);
                    IOInfoSPDummy.Children.Add(spDummy);
                }
                else if(child.GetType() == typeof(Label))
                {
                    StringReader stringReader = new StringReader(childXaml);
                    XmlReader xmlReader = XmlReader.Create(stringReader);
                    Label lb = (Label)XamlReader.Load(xmlReader);

                    StringReader stringReaderDummy = new StringReader(childXamlDummy);
                    XmlReader xmlReaderDummy = XmlReader.Create(stringReaderDummy);
                    Label lbDummy = (Label)XamlReader.Load(xmlReaderDummy);

                    IOInfoSP.Children.Add(lb);
                    IOInfoSPDummy.Children.Add(lbDummy);
                }                
            }
        }

        private void Header_Info()
        {
            int headerNum = 0;
            foreach (System.Windows.UIElement child in mainWindow.HeaderSP.Children)
            {
                string childXaml = XamlWriter.Save(child);

                StringReader stringReader = new StringReader(childXaml);
                XmlReader xmlReader = XmlReader.Create(stringReader);
                StackPanel sp = (StackPanel)XamlReader.Load(xmlReader);

                string childXamlDummy = XamlWriter.Save(child);

                StringReader stringReaderDummy = new StringReader(childXamlDummy);
                XmlReader xmlReaderDummy = XmlReader.Create(stringReaderDummy);
                StackPanel spDummy = (StackPanel)XamlReader.Load(xmlReaderDummy);

                if (headerNum < 8)
                {
                    HeaderSP1.Children.Add(sp);
                    headerNum++;
                }
                else
                {
                    HeaderSP2.Children.Add(spDummy);
                    headerNum++;
                }
            }
        }

        private void Hide_Buttons()
        {
            ToggleIOView.Visibility = Visibility.Hidden;
            Print.Visibility = Visibility.Hidden;
        }

        private void Reveal_Buttons()
        {
            ToggleIOView.Visibility = Visibility.Visible;
            Print.Visibility = Visibility.Visible;
        }

        private void Print_Section2()
        {
            BoardSP1.Visibility = BoardSPDummy1.Visibility;
            BoardSP2.Visibility = BoardSPDummy2.Visibility;
            IOInfoSP.Visibility = IOInfoSPDummy.Visibility;
            JobInfo.Visibility = Visibility.Visible;

            LandingNormalConfig.Visibility = Visibility.Hidden;
            LandingAltConfig.Visibility = Visibility.Hidden;
            LandingNormalHeader.Visibility = Visibility.Hidden;
            LandingAltHeader.Visibility = Visibility.Hidden;
            LandingLevels.Visibility = Visibility.Hidden;
            LandingPIs.Visibility = Visibility.Hidden;
            TogglePIs.Visibility = Visibility.Hidden;
            HeaderSP1.Visibility = Visibility.Hidden;
            HeaderSP2.Visibility = Visibility.Hidden;
        }
    }
}
