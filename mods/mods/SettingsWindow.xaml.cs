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
            TextEditorBox.Text = Properties.Settings.Default.TextEditor;
            this.mainWindow = mainWindow;
        }

        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Username = Username.Text;
            Properties.Settings.Default.TextEditor = TextEditorBox.Text;
            this.Close();
        }

        private void Settings_Closed(object sender, CancelEventArgs e)
        {
            mainWindow.Update_Search_History();
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            
            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".exe";
            
            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
                string filename = dlg.FileName;
                TextEditorBox.Text = filename;
            }
        }
    }
}
