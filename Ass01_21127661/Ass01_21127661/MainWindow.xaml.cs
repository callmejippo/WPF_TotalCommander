using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static Ass01_21127661.MainWindow;

namespace Ass01_21127661
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    //public partial class MainWindow : Window
    //{
    //    public MainWindow()
    //    {
    //        InitializeComponent();
    //    }
    //}



    public class FileItem
    {

        public string Path { get; set; }
        public ImageSource Icon { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; }
        public string Size { get; set; }


        public FileItem(string name, ImageSource icon, string type, DateTime date, string size)
        {
            this.Name = name;
            this.Icon = icon;
            this.Type = type;
            this.Date = date;
            this.Size = size;
        }

        public FileItem(string path, ImageSource icon, string name, DateTime date, string type, string size)
            : this(name, icon, type, date, size)
        {
            this.Path = path;
        }

        public ImageSource GetFileIcon(string filePath)
        {
            if (File.Exists(filePath))
            {
                Icon fileIcon = ShellIcon.GetSmallIcon(filePath);
                return ToImageSource(fileIcon);
            }
            else if (Directory.Exists(filePath))
            {
                Icon folderIcon = ShellIcon.GetSmallFolderIcon();
                return ToImageSource(folderIcon);
            }
            else
            {
                return null; // Handle the case where the file or folder doesn't exist
            }
        }

        public static ImageSource ToImageSource(Icon icon)
        {
            if (icon == null || icon.Handle == IntPtr.Zero)
                return null;
            try
            {
                ImageSource imageSource = Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
                
                return imageSource;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error creating ImageSource: {ex.Message}");
                return null;
            }
        }






    }

    public class DriveList
    {
        public ComboBox comboBox;
        public TextBlock pathTextBlock;


        public DriveList(ComboBox comboBox, TextBlock pathTextBlock)
        {
            this.comboBox = comboBox;
            this.pathTextBlock = pathTextBlock;
            LoadDrives();
        }

        private void LoadDrives()
        {
            Debug.WriteLine("Loading drives...");
            var drives = DriveInfo.GetDrives();
            foreach (var drive in drives)
            {
                comboBox.Items.Add(drive.Name);
            }
        }




    }

    public class FileExplorer
    {
        public DriveList driveList { get; set; }
        public List<FileItem> itemList { get; set;}
        public FileItem selectingItem { get; set; }
        public List<FileSystemInfo> curItems { get; set; }
        public ListView entryView { get; set; }
        public History history { get; set; }
        private bool comboBoxSelectionHandled = false;


        public FileExplorer(DriveList driveList, ListView entryView, History history)
        {
            this.driveList = driveList;
            this.itemList = new List<FileItem>();
            curItems = new List<FileSystemInfo>();
            this.entryView = entryView;
            this.history = history;

            //// Attach ComboBox SelectionChanged event in the constructor
            //driveList.comboBox.SelectionChanged += ComboBox_SelectionChanged;

            //// Subscribe to the MouseDoubleClick event
            //entryView.MouseDoubleClick += ProductsListView_MouseDoubleClick;
        }

        public void ReadDrive(string directoryPath)
        {
            try
            {
                // Clear previous items
                curItems.Clear();
                itemList.Clear();

                // Check if the directory exists
                if (Directory.Exists(directoryPath))
                {
                    // Read and print files in the current directory
                    DirectoryInfo curDir = new DirectoryInfo(directoryPath);
                    //Debug.WriteLine("kaka: " + curDir.FullName);

                    string[] entries = Directory.GetFileSystemEntries(directoryPath).Where(entry => IsValidEntry(entry)).ToArray();

                    foreach (string entry in entries)
                    {
                        if (File.Exists(entry))
                        {
                            curItems.Add(new FileInfo(entry));
                        }
                        else if (Directory.Exists(entry))
                        {
                            curItems.Add(new DirectoryInfo(entry));
                        }
                    }

                    foreach (FileSystemInfo item in curItems)
                    {
                        FileItem fileItem = new FileItem(item.FullName, null, item.Name, item.LastWriteTime,
                                      item is FileInfo ? "File" : "Folder",
                                      (item is FileInfo) ? ((FileInfo)item).Length.ToString() : string.Empty);

                        fileItem.Icon = fileItem.GetFileIcon(item.FullName);

                        itemList.Add(fileItem);
                        //Debug.WriteLine("Itemlist hear: ", fileItem.Name);
                    }
                    GenerateListView();


                }
                else
                {
                    Debug.WriteLine($"Directory does not exist: {directoryPath}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        public void GenerateListView()
        {
            var fileSystemItems = itemList.Select(item => new FileItem(item.Name, item.Icon, item.Type, item.Date, item.Size)
            {
            }).ToList();

            // Assuming entryView is a WPF ListView
            entryView.ItemsSource = fileSystemItems;
        }

        private bool IsValidEntry(string entry)
        {
            try
            {
                // Attempt to get file/directory info without throwing an exception
                var fileInfo = new FileInfo(entry);
                var directoryInfo = new DirectoryInfo(entry);

                // Add additional checks as needed
                if ((fileInfo.Attributes & FileAttributes.System) == FileAttributes.System ||
                    (directoryInfo.Attributes & FileAttributes.System) == FileAttributes.System ||
                    (fileInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden ||
                    (directoryInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                {
                    return false; // Skip system and hidden files/directories
                }

                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false; // Skip unauthorized access
            }
        }
        public void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //var control = sender as ComboBox;
            //var index = control.SelectedIndex;
            //string value = control.SelectedItem.ToString();
            //driveList.pathTextBlock.Text = value;

            if (comboBoxSelectionHandled)
            {
                // Event has already been handled, exit early to avoid duplicate processing
                return;
            }

            var control = sender as ComboBox;
            var index = control.SelectedIndex;
            string value = control.SelectedItem.ToString();
            driveList.pathTextBlock.Text = value;

            // Call ReadDrive in the current FileExplorer instance when ComboBox selection changes
            history.addPath(value);
            ReadDrive(value);

            // Set the flag to indicate that the event has been handled
            comboBoxSelectionHandled = true;

            //foreach (var path in history.pathArr)
            //{
            //    Debug.WriteLine(path);
            //}
            //Debug.WriteLine(history.Count.ToString());

            // Reset the flag after a short delay to allow subsequent events to be processed
            Task.Delay(100).ContinueWith(_ => comboBoxSelectionHandled = false);


        }

        public void ProductsListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var control = sender as ListViewItem;


            // Check if control is null
            if (control == null)
            {
                Debug.WriteLine("null control");
                // Handle the case where control is null (log an error, show a message, etc.)
                return;
            }
            //Debug.WriteLine("Value Path:", control.Content);

            var value = control.DataContext as FileItem;
            // Check if value is null
            if (value == null)
            {
                Debug.WriteLine("value control");

                // Handle the case where value is null (log an error, show a message, etc.)
                return;
            }

            string newPath = System.IO.Path.Combine(driveList.pathTextBlock.Text.Trim(), value.Name);

            Debug.WriteLine("path:", newPath);
            if (value.Type == "Folder")
            {

                // Use Dispatcher to update UI on the UI thread
                Application.Current.Dispatcher.Invoke(() =>
                {
                    driveList.pathTextBlock.Text = newPath;
                    curItems.Clear();
                    history.addPath(newPath);
                    
                    ReadDrive(newPath);
                });


            }
            else
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    FileName = newPath,
                    UseShellExecute = true,
                };
                Process.Start(processStartInfo);

            }


        }
    }

    public class History
    {
        public List<string> pathArr { get; set; }
        public int curIndex { get; set; }
        public int Count
        {
            get { return pathArr.Count; }
        }

        public History(int capacity)
        {
            pathArr = new List<string>(capacity);
            curIndex = -1;
        }

        public void Backward() {
            if (curIndex > 0)
            {
                curIndex--;
            }
        }
        public void Forward() {
            if (curIndex < pathArr.Count - 1)
            {
                curIndex++;
            }
        }
        public void addPath(string newPath) {

            // Remove paths after the current index
            if (pathArr.Count == 0 || curIndex == pathArr.Count - 1)
            {
                // If the list is empty or the current index is at the end, add the new path
                pathArr.Add(newPath);
                curIndex = pathArr.Count - 1;
            }
            else
            {
                // If the current index is not at the end, replace the current path and remove subsequent paths
                pathArr[curIndex + 1] = newPath;
                pathArr.RemoveRange(curIndex + 2, pathArr.Count - curIndex - 2);
                curIndex = pathArr.Count - 1;
            }
            

        }
        public void removePath(string path) {
            pathArr.Remove(path);
            if (curIndex >= pathArr.Count)
            {
                curIndex = pathArr.Count - 1;
            }
        }



    }


    public partial class MainWindow : Window
    {


        public FileExplorer fileExplorer1 { get; private set; }
        public FileExplorer fileExplorer2 { get; private set; }
        private bool isDockPanel1Focused = false;
        private bool isDockPanel2Focused = false;
        History history1, history2;
        public MainWindow()
        {
            InitializeComponent();
            history1 = new History(10); 
            history2 = new History(10);
            fileExplorer1 = InitializeFileExplorer(drivesList1, path1, listEntryView1, history1);
            fileExplorer2 = InitializeFileExplorer(drivesList2, path2, listEntryView2, history2);


        //loadDriver();
        }
        private FileExplorer InitializeFileExplorer(ComboBox driveComboBox, TextBlock pathTextBlock, ListView entryView, History history)
        {
            var fileExplorer = new FileExplorer(new DriveList(driveComboBox, pathTextBlock), entryView, history);

            // Attach event handlers
            driveComboBox.SelectionChanged += fileExplorer.ComboBox_SelectionChanged;
            entryView.MouseDoubleClick += fileExplorer.ProductsListView_MouseDoubleClick;
            return fileExplorer;

        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            // Forward the event to FileExplorer
            if (comboBox == drivesList1)
            {
                // Forward the event to FileExplorer1
                //Debug.WriteLine("ComboBox1 Selection Changed");
                fileExplorer1.ComboBox_SelectionChanged(sender, e);
            }
            else if (comboBox == drivesList2)
            {
                // Forward the event to FileExplorer2
                //Debug.WriteLine("ComboBox2 Selection Changed");
                fileExplorer2.ComboBox_SelectionChanged(sender, e);
            }

        }

        private void ProductsListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListViewItem item = sender as ListViewItem;

            if (item != null)
            {
                // Get the parent ListView
                ListView listView = ItemsControl.ItemsControlFromItemContainer(item) as ListView;

                if (listView != null)
                {
                    ComboBox comboBox = null;

                    // Check which ListView triggered the event
                    if (listView == listEntryView1)
                    {
                        comboBox = drivesList1;
                    }
                    else if (listView == listEntryView2)
                    {
                        comboBox = drivesList2;
                    }

                    // Forward the event to the corresponding FileExplorer
                    if (comboBox != null)
                    {
                        //Debug.WriteLine($"{comboBox.Name} ListView Double-Click");
                        if (comboBox == drivesList1)
                        {
                            fileExplorer1.ProductsListView_MouseDoubleClick(sender, e);
                        }
                        else if (comboBox == drivesList2)
                        {
                            fileExplorer2.ProductsListView_MouseDoubleClick(sender, e);
                        }
                    }
                }
            }

        }
        private void ListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ListView listView = sender as ListView;

            if (listView.View is GridView grid)
            {
                grid.Columns[0].Width = listView.ActualWidth / 4;
                grid.Columns[1].Width = listView.ActualWidth / 4;
                grid.Columns[2].Width = listView.ActualWidth / 4;
                grid.Columns[3].Width = listView.ActualWidth / 4;
            }

        }

        private void DockPanel_GotFocus(object sender, RoutedEventArgs e)
        {
            DockPanel dockPanel = sender as DockPanel;

            if (dockPanel != null)
            {
                if (dockPanel == dockPanel1)
                {
                    // dockPanel1 got focus
                    //Debug.WriteLine("dockPanel1 got focus");
                    isDockPanel1Focused = true;
                    isDockPanel2Focused = false;
                }
                else if (dockPanel == dockPanel2)
                {
                    // dockPanel2 got focusy
                    //Debug.WriteLine("dockPanel2 got focus");
                    isDockPanel1Focused = false;
                    isDockPanel2Focused = true;
                }
            }
        }

        private void DockPanel_LostFocus(object sender, RoutedEventArgs e)
        {
            DockPanel dockPanel = sender as DockPanel;

            if (dockPanel != null)
            {
                if (dockPanel == dockPanel1)
                {
                    // dockPanel1 lost focus
                    Debug.WriteLine("dockPanel1 lost focus");
                }
                else if (dockPanel == dockPanel2)
                {
                    // dockPanel2 lost focus
                    Debug.WriteLine("dockPanel2 lost focus");
                }
            }
        }


        public void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (isDockPanel1Focused)
            {
                // Back button clicked in DockPanel1
                history1.Backward();
                //Debug.WriteLine("BACK");
                //Debug.WriteLine(history1.Count);
                //foreach (var path in history1.pathArr)
                //{
                //    Debug.WriteLine(path);
                //}
                //Debug.WriteLine("CUR", history1.curIndex.ToString());
                if (history1.Count > 0)
                {

                    backBtn.IsEnabled = true;
                    fileExplorer1.driveList.pathTextBlock.Text = history1.pathArr[history1.curIndex];
                    fileExplorer1.ReadDrive(history1.pathArr[history1.curIndex]);
                }
                else
                {
                    backBtn.IsEnabled = false;

                }
            }
            else if (isDockPanel2Focused)
            {
                // Back button clicked in DockPanel2
                history2.Backward();
                if (history2.Count > 0)
                {
                    backBtn.IsEnabled = true;
                    fileExplorer2.driveList.pathTextBlock.Text = history2.pathArr[history2.curIndex];
                    fileExplorer2.ReadDrive(history2.pathArr[history2.curIndex]);
                }
                else
                {
                    backBtn.IsEnabled = false;

                }

            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (isDockPanel1Focused)
            {
                // Next button clicked in DockPanel1
                fileExplorer1.history.Forward();
                if (history1.Count >= 1)
                {
                    fileExplorer1.driveList.pathTextBlock.Text = history1.pathArr[history1.curIndex];
                    fileExplorer1.ReadDrive(history1.pathArr[history1.curIndex]);

                }
                else
                {
                    backBtn.IsEnabled = false;

                }
            }
            else if (isDockPanel2Focused)
            {
                // Next button clicked in DockPanel2
                fileExplorer2.history.Forward();
                if (history2.Count <= 10)
                {
                    backBtn.IsEnabled = true;
                    fileExplorer2.driveList.pathTextBlock.Text = history2.pathArr[history2.curIndex];
                    fileExplorer2.ReadDrive(history2.pathArr[history2.curIndex]);
                }
                else
                {
                    backBtn.IsEnabled = false;

                }

            }
        }


        public void UpButton_Click(object sender, RoutedEventArgs e)
        {
            if (isDockPanel1Focused)
            {
                // Up button clicked in DockPanel1
                string currentPath = fileExplorer1.driveList.pathTextBlock.Text;
                string parentPath = System.IO.Path.GetDirectoryName(currentPath);

                if (!string.IsNullOrEmpty(parentPath))
                {
                    // Navigate up if there is a parent directory
                    fileExplorer1.driveList.pathTextBlock.Text = parentPath;
                    fileExplorer1.ReadDrive(parentPath);
                }
            }
            else if (isDockPanel2Focused)
            {
                // Up button clicked in DockPanel2
                string currentPath = fileExplorer2.driveList.pathTextBlock.Text;
                string parentPath = System.IO.Path.GetDirectoryName(currentPath);

                if (!string.IsNullOrEmpty(parentPath))
                {
                    // Navigate up if there is a parent directory
                    fileExplorer2.driveList.pathTextBlock.Text = parentPath;
                    fileExplorer2.ReadDrive(parentPath);
                }
            }
        }

    }

}