using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Windows.Threading;
using System.Threading;

namespace mods
{
    public partial class AdvancedSearch : Window
    {
        bool stopSearching = false;
        private static Semaphore _search;
        int searchProgress = 0;
        string G_DRIVE = @"\\10.113.32.45\shared\";
        MainWindow window;
        List<string> filtersList;

        public AdvancedSearch()
        {
            InitializeComponent();

            window = Application.Current.Windows.OfType<MainWindow>().First();
        }

        private void Filters_List()
        {
            filtersList = new List<string>
            {

            };
        }

        private void SearchFiles()
        {
            _search = new Semaphore(0, 100);

            if (Filters.Children.Count < 1)
            {
                MessageBox.Show("Please add at least 1 filter");
                return;
            }

            searchProgress = 0;
            SearchProgress.Dispatcher.Invoke(() => SearchProgress.Value = searchProgress, DispatcherPriority.Background);

            string[] locations = new string[] { "MP2COC", "MP2OGM", "MPODH", "MPODT", "MPOGD", "MPOGM", "MPOLHD", "MPOLHM", "MPOLOM", "MPOLTD", "MPOLTM" };
            string[] source_locations = new string[] { "MC-MP\\MPODH", "MC-MP\\MPODT", "MC-MP\\MPOGM", "MC-MP\\MPOLHM", "MC-MP\\MPOLOM", "MC-MP\\MPOLTM", "MC-MP2\\MP2COC", "MC-MP2\\MP2OGM" };

            string[] custom_locations = new string[] { "MC-MP\\MPODH\\", "MC-MP\\MPODT\\", "MC-MP\\MPOGD\\", "MC-MP\\MPOGM\\", "MC-MP\\MPOLHD\\", "MC-MP\\MPOLHM\\", "MC-MP\\MPOLOM\\", "MC-MP\\MPOLTD\\", "MC-MP\\MPOLTM\\" };
            string[] custom2_locations = new string[] { "" };

            SearchProgress.Maximum = 10; //Start at 10 because its PUBLIK directory weight
            SearchProgress.Maximum += Directory.GetDirectories(G_DRIVE + "Software\\Product\\").Length;
            SearchProgress.Maximum += Directory.GetDirectories(G_DRIVE + "Software\\Source\\MC-MP\\").Length;
            SearchProgress.Maximum += Directory.GetDirectories(G_DRIVE + "Software\\Source\\MC-MP2\\").Length;
            SearchProgress.Maximum += Directory.GetDirectories(G_DRIVE + "Software\\Custom\\MC-MP\\").Length * 100;
            SearchProgress.Maximum += Directory.GetDirectories(G_DRIVE + "Software\\Custom2\\").Length;

            Thread publik = new Thread(() => SearchLocation(G_DRIVE + "Software\\Publik\\", 10));
            publik.Start();

            if(CustomFoldersCheckBox.IsChecked == true)
            {
                foreach (string directory in Directory.GetDirectories(G_DRIVE + "Software\\Source\\MC-MP\\"))
                {
                    Thread t = new Thread(() => SearchLocation(directory));
                    t.Start();
                }

                foreach (string directory in Directory.GetDirectories(G_DRIVE + "Software\\Source\\MC-MP2\\"))
                {
                    Thread t = new Thread(() => SearchLocation(directory));
                    t.Start();
                }

                foreach (string directory in Directory.GetDirectories(G_DRIVE + "Software\\Custom2\\"))
                {
                    Thread t = new Thread(() => SearchLocation(directory));
                    t.Start();
                }

                foreach (string directory in Directory.GetDirectories(G_DRIVE + "Software\\Custom\\MC-MP\\"))
                {
                    Thread t = new Thread(() => SearchLocation(directory, 100));
                    t.Start();
                }
            }

            foreach (string directory in Directory.GetDirectories(G_DRIVE + "Software\\Product\\"))
            {
                if (!directory.Contains("MASTER.BIN"))
                {
                    Thread t = new Thread(() => SearchLocation(directory));
                    t.Start();
                }
                else
                {
                    searchProgress++;
                    SearchProgress.Dispatcher.Invoke(() => SearchProgress.Value = searchProgress, DispatcherPriority.Background);
                }
            }

            _search.Release(100);
        
        }

        private void SearchLocation(string directory, int weight = 1)
        {
            int validNum = 0;

            _search.WaitOne();

            if (Directory.Exists(directory))
            {
                try
                {
                    string[] files = Directory.GetFiles(directory, "*", SearchOption.AllDirectories);
                    foreach (string file in files)
                    {
                        if(stopSearching)
                        {
                            return;
                        }
                        else
                        {
                            bool validFile = false;
                            string fileExtension = General.Get_FileExtension_From_Path(file).ToLower();

                            if (fileExtension == ".asm" || fileExtension == "")
                            {
                                validFile = Validate_File(file);
                            }

                            if (validFile)
                            {
                                int locationIndex = directory.IndexOf("Software") + 9;
                                string jobFile = file.Substring(locationIndex, file.Length - locationIndex);
                                window.FilesListBox.Dispatcher.Invoke(() => window.FilesListBox.Items.Add(jobFile), DispatcherPriority.Background);
                                validNum++;
                            }
                        }
                    }
                }
                catch
                {

                }
            }

            searchProgress += weight;
            SearchProgress.Dispatcher.Invoke(() => SearchProgress.Value = searchProgress, DispatcherPriority.Background);

            _search.Release();
        }

        private bool Validate_File(string file)
        {
            Controller controller;

            if (file.Contains("MP2OGM") || file.Contains("MPOGM") || file.Contains("MPOGD"))
            {
                controller = new Group(file);
            }
            else if (file.Contains("MPODT"))
            {
                controller = new Simplex(file);
            }
            else
            {
                controller = new Local(file);
            }

            List<ComboBox> cblist = Filters.Children.OfType<ComboBox>().ToList();
            List<TextBox> tblist = Filters.Children.OfType<TextBox>().ToList();

            for(int i = 0; i < cblist.Count; i++)
            {
                string filter = cblist[i].SelectedItem.ToString();
                string value = tblist[i].Text;
            }

            return false;
        }

        private void Close(object sender, EventArgs e)
        {
            stopSearching = true;
        }

        private void AddFilter_Click(object sender, RoutedEventArgs e)
        {
            StackPanel filter = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(0, 5, 0, 0),
            };

            ComboBox cb = new ComboBox
            {
                Width = 150,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                Height = 25,
                Margin = new Thickness(227, 0, 0, 0),
            };
            TextBox tb = new TextBox
            {
                Width = 150,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                Height = 25,
                Margin = new Thickness(0, 0, 227, 0),
            };

            filter.Children.Add(cb);
            filter.Children.Add(tb);

            Filters.Children.Add(filter);

            double topmargin = AddFilter.Margin.Top + 30;

            AddFilter.Margin = new Thickness(356, topmargin, 0, 0);
        }

        private void ClearFitlers_Click(object sender, RoutedEventArgs e)
        {
            Filters.Children.Clear();
            AddFilter.Margin = new Thickness(356, 100, 0, 0);
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
