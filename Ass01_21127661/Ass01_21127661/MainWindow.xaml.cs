using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ass01_21127661
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
    }

    //public partial class MainWindow : Window
    //{
    //    public class FileStructure
    //    {
    //        public string Name { get; set; }
    //        public DateTime Date { get; set; }
    //        public string Type { get; set; }
    //        public string Size { get; set; }
    //    }
    //    DirectoryInfo curDir;
    //    List<FileSystemInfo> curItems;
    //    public MainWindow()
    //    {
    //        InitializeComponent();
    //        loadDriver();
    //    }

    //    private void loadDriver()
    //    {
    //        var drives = DriveInfo.GetDrives();
    //        foreach (var drive in drives)
    //        {
    //            drivesList.Items.Add(drive.Name);
    //        }
    //    }
    //    private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    //    {
    //        var control = sender as ComboBox;
    //        var index = control.SelectedIndex;
    //        String value = control.SelectedItem.ToString();

    //        Debug.WriteLine("Combo box change");
    //        Debug.WriteLine(value);
    //        entryView.ItemsSource = null;
    //        path.Text = value;
    //        readDrive(value);

    //    }
    //    private void readDrive(string directoryPath)
    //    {
    //        try
    //        {
    //            // Clear previous items
    //            curItems = new List<FileSystemInfo>();

    //            // Check if the directory exists
    //            if (Directory.Exists(directoryPath))
    //            {
    //                // Read and print files in the current directory
    //                curDir = new DirectoryInfo(directoryPath);
    //                Debug.WriteLine("kaka: " + curDir.FullName);

    //                string[] entries = Directory.GetFileSystemEntries(directoryPath);

    //                foreach (string entry in entries)
    //                {
    //                    if (File.Exists(entry))
    //                    {
    //                        curItems.Add(new FileInfo(entry));
    //                    }
    //                    else if (Directory.Exists(entry))
    //                    {
    //                        curItems.Add(new DirectoryInfo(entry));
    //                    }
    //                }
    //                foreach (FileSystemInfo item in curItems)
    //                {
    //                    Debug.WriteLine(item.FullName);
    //                }
    //                generateListView();

    //            }
    //            else
    //            {
    //                Debug.WriteLine($"Directory does not exist: {directoryPath}");
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            Debug.WriteLine($"An error occurred: {ex.Message}");
    //        }

    //    }

    //    private void generateListView()
    //    {
    //        var fileSystemItems = curItems.Select(item => new FileStructure
    //        {
    //            Name = item.Name,
    //            Date = item.LastWriteTime,
    //            Type = item is FileInfo ? "File" : "Folder",
    //            Size = (item is FileInfo) ? ((FileInfo)item).Length.ToString() : string.Empty
    //        }).ToList();

    //        // Bind the ListView to the list of FileSystemItem objects
    //        entryView.ItemsSource = fileSystemItems;
    //    }

    //    private void productsListView_MouseDoub1eC1ick(object sender, MouseEventArgs e)
    //    {
    //        var control = sender as ListViewItem;
    //        var value = control.Content as FileStructure;

    //        if (value.Type == "Folder")
    //        {
    //            string newPath = path.Text.Trim() + value.Name + "\\";

    //            path.Text = newPath;
    //            curItems.Clear();
    //            readDrive(newPath);

    //        }
    //        else
    //        {
    //            string newPath = path.Text.Trim() + value.Name;
    //            ProcessStartInfo processStartInfo = new ProcessStartInfo
    //            {
    //                FileName = newPath,
    //                UseShellExecute = true,
    //            };
    //            Process.Start(processStartInfo);

    //        }
    //    }
    //    private void ListView_SizeChanged(object sender, SizeChangedEventArgs e)
    //    {
    //        ListView listView = sender as ListView;

    //        if (listView.View is GridView grid)
    //        {
    //            grid.Columns[0].Width = listView.ActualWidth / 4;
    //            grid.Columns[1].Width = listView.ActualWidth / 4;
    //            grid.Columns[2].Width = listView.ActualWidth / 4;
    //            grid.Columns[3].Width = listView.ActualWidth / 4;
    //        }

    //    }

    //}

}