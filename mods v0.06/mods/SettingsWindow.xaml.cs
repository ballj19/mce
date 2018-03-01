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
using System.ComponentModel;

namespace mods
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        MainWindow mainWindow;

        public SettingsWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            Username.Text = Properties.Settings.Default.Username;
            this.mainWindow = mainWindow;
        }

        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Username = Username.Text;
            this.Close();
        }

        private void Settings_Closed(object sender, CancelEventArgs e)
        {
            mainWindow.Update_Search_History();
        }
    }
}
