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
using System.IO;

namespace mods
{
    /// <summary>
    /// Interaction logic for ArchiveWindow.xaml
    /// </summary>
    public partial class ArchiveWindow : Window
    {
        private string jobNumber;
        private string filePath;
        private string folder;

        public ArchiveWindow(string filepath)
        {
            InitializeComponent();

            this.filePath = filepath;
            FilePathBlock.Text = filePath;

            FileNameList.Text = "File Names\n";
            FileLastModifiedList.Text = "Last Modified\n";
            FileCreatedList.Text = "Created\n";
            
            this.folder = General.Get_Folder_From_Path(filePath);

            string jobFile = General.Get_File_From_Path(filePath);
            int dotIndex = jobFile.IndexOf(".");
            if(dotIndex == -1)
            {
                jobNumber = jobFile;
            }
            else
            {
                this.jobNumber = jobFile.Substring(0, dotIndex);
            }

            FileNameBlock.Text = jobNumber + ".";
            FileExtensionBox.Text = "OLD";

            Search_Files();
        }

        private void Search_Files()
        {
            string jobNumber = this.jobNumber + ".*";
            List<string> paths = Directory.GetFiles(@folder, jobNumber).ToList();

            foreach(string path in paths)
            {
                string fileName = General.Get_File_From_Path(path);
                DateTime lastModified = File.GetLastWriteTime(path);
                DateTime created = File.GetCreationTime(path);

                FileNameList.Text += fileName + "\n";
                FileLastModifiedList.Text += lastModified.ToString("MM/dd/yy HH:mm:ss") + "\n";
                FileCreatedList.Text += created.ToString("MM/dd/yy HH:mm:ss") + "\n";
            }

            Modify_Window_Height(paths.Count);
        }

        private void Modify_Window_Height(int numOfFiles)
        {
            this.Height = 200 + 16 * numOfFiles;
        }

        private void ArchiveButton_Click(object sender, RoutedEventArgs e)
        {
            string newFile = folder + FileNameBlock.Text + FileExtensionBox.Text;

            if(File.Exists(newFile))
            {
                if(System.Windows.Forms.MessageBox.Show("This File already exists, are you sure you want to overwrite?", "Archive?", System.Windows.Forms.MessageBoxButtons.YesNo) != System.Windows.Forms.DialogResult.Yes)
                {
                    return;
                }
            }

            File.Move(filePath, newFile);
            File.Copy(newFile, folder + jobNumber + ".ASM");

            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
