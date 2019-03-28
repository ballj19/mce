using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Windows.Threading;
using System.Threading;
using System.Diagnostics;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Data;

namespace mods
{
    public partial class AdvancedSearch : Window
    {
        bool stopSearching = false;
        int searchProgress = 0;
        string G_DRIVE = @"\\10.113.32.45\shared\";
        MainWindow window;
        DateTime database_last_updated;
        List<Thread> threads = new List<Thread>();

        public AdvancedSearch()
        {
            InitializeComponent();

            window = Application.Current.Windows.OfType<MainWindow>().First();
        }
        
        private void SearchFiles(List<Filter> filters)
        {
            if (Filters.Children.Count < 1)
            {
                MessageBox.Show("Please add at least 1 filter");
                return;
            }

            SqlConnection conn = new SqlConnection(@"Data Source = (LocalDB)\MSSQLLocalDB; AttachDbFilename = C:\Users\Jacob.Ball\source\repos\mce\mods\mods\Controllers.mdf; Integrated Security = True");

            try
            {
                conn.Open();

                string select_sql = "SELECT * FROM controller WHERE ";

                foreach(Filter filter in filters)
                {
                    select_sql += Filter.Filter_To_SQL(filter);
                    if(!filters.Last().Equals(filter))
                    {
                        select_sql += " AND ";
                    }
                }
                
                SqlCommand cmd = new SqlCommand(select_sql, conn);

                foreach(Filter filter in filters)
                {
                    cmd.Parameters.Add("@" + Filter.filtersList[filter.name], filter.datatype).Value = filter.value;
                }

                SqlDataReader reader = cmd.ExecuteReader();

                while(reader.Read())
                {
                    string file = reader.GetString(reader.GetOrdinal("filepath"));
                    int locationIndex = file.IndexOf("Software") + 9;
                    string jobFile = file.Substring(locationIndex, file.Length - locationIndex);
                    window.FilesListBox.Dispatcher.Invoke(() => window.FilesListBox.Items.Add(jobFile), DispatcherPriority.Background);
                }
            }
            catch
            {

            }
            finally
            {
                conn.Close();
            }
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            window.FilesListBox.Items.Clear();

            List<StackPanel> spList = Filters.Children.OfType<StackPanel>().ToList();
            List<Filter> filters = new List<Filter>();
            foreach (StackPanel sp in spList)
            {
                filters.Add(new Filter(sp));
            }

            SearchFiles(filters);
        }

        private void Loop_Files(IEnumerable<string> files, Action<string> action)
        {
            foreach (string file in files)
            {
                try
                {
                        action(Path.GetFileName(file));
                }
                catch
                {

                }
                searchProgress++;
                SearchProgress.Dispatcher.Invoke(() => SearchProgress.Value = searchProgress, DispatcherPriority.Background);
            }
        }

        private void Get_Database_Last_Updated()
        {
            database_last_updated = DateTime.Parse(System.IO.File.ReadAllLines(@"\\10.112.10.28\MCE-Rancho\Jake Ball\DatabaseLastUpdated.txt")[0]);
        }
        
        private void Update_Controller_In_Database(Controller controller)
        {
            SqlConnection conn = new SqlConnection(@"Data Source = (LocalDB)\MSSQLLocalDB; AttachDbFilename = C:\Users\Jacob.Ball\source\repos\mce\mods\mods\Controllers.mdf; Integrated Security = True");

            try
            {
                conn.Open();
                //SqlCommand cmd = new SqlCommand("UPDATE controller SET jobname = @jobname, version = @version, firecode = @firecode WHERE filepath LIKE @filepath", conn);
                SqlCommand cmd = new SqlCommand("UPDATE controller SET firecode = @firecode, fileVersionInt = @fileVersionInt WHERE filepath LIKE @filepath", conn);
                cmd.Parameters.AddWithValue("@filepath", controller.file);
                //cmd.Parameters.AddWithValue("@jobname", controller.jobName);
                //cmd.Parameters.AddWithValue("@version", controller.fileVersion);
                cmd.Parameters.AddWithValue("@firecode", controller.firecode);
                cmd.Parameters.AddWithValue("@fileVersionInt", controller.fileVersionInt);
                
                cmd.ExecuteNonQuery();
            }
            catch
            {

            }
            finally
            {
                conn.Close();
            }

        }

        private Controller Get_Controller(string file)
        {
            Controller controller;

            Content content = new Content(file, false);

            if(content.content.IndexOf("END") == -1)
            {
                return null;
            }

            if (file.Contains("MP2OGM") || file.Contains("MPOGM") || file.Contains("MPOGD"))
            {
                controller = new Group(file, content);
            }
            else if (file.Contains("MPODT") || file.Contains("MPODH"))
            {
                controller = new Simplex(file, content);
            }
            else
            {
                controller = new Local(file, content);
            }

            return controller;
        }

        private void Add_Controller_To_Database(Controller controller)
        {
            SqlConnection conn = new SqlConnection(@"Data Source = (LocalDB)\MSSQLLocalDB; AttachDbFilename = C:\Users\Jacob.Ball\source\repos\mce\mods\mods\Controllers.mdf; Integrated Security = True");

            try
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("INSERT INTO controller (filepath, jobname, version) VALUES (@filepath, @jobname, @version)", conn);
                cmd.Parameters.AddWithValue("@filepath", controller.file);
                cmd.Parameters.AddWithValue("@jobname", controller.jobName);
                cmd.Parameters.AddWithValue("@version", controller.fileVersion);
                cmd.ExecuteNonQuery();
            }
            catch
            {
                
            }
            finally
            {
                conn.Close();
            }
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
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Height = 25,
                Margin = new Thickness(0, 0, 0, 0),
            };

            cb.SelectionChanged += Filter_Changed;
            
            foreach (string f in Filter.filtersList.Keys)
            {
                cb.Items.Add(f);
            }

            filter.Children.Add(cb);

            Filters.Children.Add(filter);

            double topmargin = AddFilter.Margin.Top + 30;

            AddFilter.Margin = new Thickness(356, topmargin, 0, 0);
        }

        private void Filter_Changed(object sender, RoutedEventArgs e)
        {
            ComboBox cb = (ComboBox)sender;
            StackPanel sp = (StackPanel)cb.Parent;

            Filter.Add_Filter_To_SP(cb, sp);
        }

        private void ClearFitlers_Click(object sender, RoutedEventArgs e)
        {
            Filters.Children.Clear();
            AddFilter.Margin = new Thickness(356, 100, 0, 0);
        }
        
        private async void UpdateDB_Click(object sender, RoutedEventArgs e)
        {
            Get_Database_Last_Updated();
            
            searchProgress = 0;
            SearchProgress.Maximum = 135;
            SearchProgress.Dispatcher.Invoke(() => SearchProgress.Value = searchProgress, DispatcherPriority.Background);
            
            Create_ExistingFiles_Set();
                
            Generate_Threads(G_DRIVE + "Software\\Product\\");
            Generate_Threads(G_DRIVE + "Software\\Source\\MC-MP\\");
            Generate_Threads(G_DRIVE + "Software\\Custom\\MC-MP\\");
            
            foreach(Thread t in threads)
            {
                await Task.Run(() => t.Join());
            }

            System.IO.File.Delete(@"\\10.112.10.28\MCE-Rancho\Jake Ball\DatabaseLastUpdated.txt");
            System.IO.File.WriteAllLines(@"\\10.112.10.28\MCE-Rancho\Jake Ball\DatabaseLastUpdated.txt", new string[] { DateTime.Now.ToString() });

        }

        private void Generate_Threads(string topdir)
        {
            foreach (string directory in Directory.GetDirectories(topdir))
            {
                if(directory.Contains("MP"))
                {
                    Thread t = new Thread(() => Loop_Directory(directory));
                    threads.Add(t);
                    t.Start();
                }
            }
        }

        private void Loop_Directory(string directory)
        {
            var files = Get_Assembly_Files(directory);

            var files_need_adding = Get_Files_Need_Adding(files);

            var files_need_updating = Get_Files_Need_Updating(files);

            Add_Files(files_need_adding);

            Update_Files(files_need_updating);
        }

        private IEnumerable<FileInfo> Get_Assembly_Files(string directory)
        {
            DirectoryInfo dir = new DirectoryInfo(directory);
            var files = dir.GetFiles("*", SearchOption.AllDirectories).Where(file => file.Extension == ".ASM" || file.Extension == ".asm" || file.Extension == "");

            searchProgress++;
            SearchProgress.Dispatcher.Invoke(() => SearchProgress.Value = searchProgress, DispatcherPriority.Background);

            return files;
        }

        private IEnumerable<FileInfo> Get_Files_Need_Adding(IEnumerable<FileInfo> files)
        {
            var files_need_adding = files.Where(file => !Controller_Exists(file.FullName));

            searchProgress++;
            SearchProgress.Dispatcher.Invoke(() => SearchProgress.Value = searchProgress, DispatcherPriority.Background);

            return files_need_adding;
        }

        private IEnumerable<FileInfo> Get_Files_Need_Updating(IEnumerable<FileInfo> files)
        {
            searchProgress++;
            SearchProgress.Dispatcher.Invoke(() => SearchProgress.Value = searchProgress, DispatcherPriority.Background);

            //return files; //Uncomment only when all files need updating

            var files_need_updating = files.Where(file => file.LastWriteTime > database_last_updated && Controller_Exists(file.FullName));

            searchProgress++;
            SearchProgress.Dispatcher.Invoke(() => SearchProgress.Value = searchProgress, DispatcherPriority.Background);
            
            return files_need_updating;
        }

        private void Add_Files(IEnumerable<FileInfo> files)
        {
            foreach (FileInfo file in files)
            {
                Controller controller = Get_Controller(file.FullName);
                if (controller != null)
                    Add_Controller_To_Database(controller);
            }

            searchProgress++;
            SearchProgress.Dispatcher.Invoke(() => SearchProgress.Value = searchProgress, DispatcherPriority.Background);
        }

        private void Update_Files(IEnumerable<FileInfo> files)
        {
            foreach (FileInfo file in files)
            {
                Controller controller = Get_Controller(file.FullName);
                if (controller != null)
                    Update_Controller_In_Database(controller);
            }
            searchProgress++;
            SearchProgress.Dispatcher.Invoke(() => SearchProgress.Value = searchProgress, DispatcherPriority.Background);
        }

        private void Create_ExistingFiles_Set()
        {
            SqlConnection conn = new SqlConnection(@"Data Source = (LocalDB)\MSSQLLocalDB; AttachDbFilename = C:\Users\Jacob.Ball\source\repos\mce\mods\mods\Controllers.mdf; Integrated Security = True");

            try
            {
                conn.Open();

                string select_sql = "SELECT filepath FROM controller";
                
                SqlCommand cmd = new SqlCommand(select_sql, conn);

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    string file = reader.GetString(reader.GetOrdinal("filepath"));
                    existing_files.Add(file);
                }
            }
            catch
            {

            }
            finally
            {
                conn.Close();
            }
        }

        private bool Controller_Exists(string file)
        {
            return existing_files.Contains(file);
        }

        HashSet<string> existing_files = new HashSet<string>();
    }
}
