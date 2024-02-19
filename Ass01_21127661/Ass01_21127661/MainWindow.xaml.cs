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
using System.Windows.Threading;
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

        private bool IsDirectory => Directory.Exists(Path);
        private TextBlock statusMessage;


        public FileItem(string path, string name, ImageSource icon, string type, DateTime date, string size)
        {
            this.Path = path;
            this.Name = name;
            this.Icon = icon;
            this.Type = type;
            this.Date = date;
            this.Size = size;
        }

        public FileItem(string path, ImageSource icon, string name, DateTime date, string type, string size)
            : this(path,name, icon, type, date, size)
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

        public void CopyTo(string destinationPath)
        {
            try
            {
                if (File.Exists(Path))
                {
                    // Copy file
                    string newFilePath = GenerateNewPath(destinationPath, Name);
                    File.Copy(Path, newFilePath, true);

                }
                else if (Directory.Exists(Path))
                {
                    // Copy folder
                    string newFolderPath = GenerateNewPath(destinationPath, Name);
                    CopyFolder(Path, newFolderPath);
                }
                // Handle other cases if needed
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error copying item: {ex.Message}");
                // Handle the exception according to your application's requirements
            }
        }


        private void CopyFolder(string sourceFolder, string destinationFolder)
        {
            if (!Directory.Exists(destinationFolder))
            {
                Directory.CreateDirectory(destinationFolder);
            }

            string[] files = Directory.GetFiles(sourceFolder);
            foreach (string file in files)
            {
                string name = System.IO.Path.GetFileName(file);
                string dest = System.IO.Path.Combine(destinationFolder, name);
                File.Copy(file, dest, true);
            }

            string[] folders = Directory.GetDirectories(sourceFolder);
            foreach (string folder in folders)
            {
                string name = System.IO.Path.GetFileName(folder);
                string dest = System.IO.Path.Combine(destinationFolder, name);
                CopyFolder(folder, dest);
            }
        }

        public void MoveTo(string destinationPath)
        {
            try
            {
               
                string parentPath = System.IO.Path.GetDirectoryName(Path);
                if (parentPath == destinationPath)
                {
                    // Show a user-friendly message that the paths are the same
                    MessageBoxResult result = MessageBox.Show("Source and destination paths are the same. Do you want to replace the existing item or create a copy?", "Move Operation", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                    switch (result)
                    {
                        case MessageBoxResult.Yes:
                            // Replace the existing item
                            if (File.Exists(Path))
                            {
                                File.Replace(Path, destinationPath, null);
                            }
                            else if (Directory.Exists(Path))
                            {
                                // Handle the case when the source and destination paths are the same for a directory
                                MessageBox.Show("Source and destination paths are the same for a directory. No action taken.", "Move Operation", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            break;

                        case MessageBoxResult.No:
                            // Create a copy
                            if (File.Exists(Path))
                            {
                                string newFilePath = GenerateNewPath(destinationPath, Name);
                                File.Copy(Path, newFilePath);
                            }
                            else if (Directory.Exists(Path))
                            {
                                string newFolderPath = GenerateNewPath(destinationPath, Name);
                                CopyFolder(Path, newFolderPath);
                            }
                            break;

                        case MessageBoxResult.Cancel:
                            // User canceled the operation
                            return;
                    }
                }
                else
                {
                    if (File.Exists(Path))
                    {
                        // Move file
                        string newFilePath = GenerateNewPath(destinationPath, Name);
                        File.Move(Path, newFilePath);
                    }
                    else if (Directory.Exists(Path))
                    {
                        // Move folder
                        string newFolderPath = GenerateNewPath(destinationPath, Name);
                        Directory.Move(Path, newFolderPath);
                    }
                }
                // Handle other cases if needed
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error moving item: {ex.Message}");
                // Handle the exception according to your application's requirements
            }
        }

        private string GenerateNewPath(string destinationPath, string itemName)
        {
            string newPath = System.IO.Path.Combine(destinationPath, itemName);

            // Check if the destination already contains an item with the same name
            int counter = 1;
            while (File.Exists(newPath) || Directory.Exists(newPath))
            {
                string newName = $"{System.IO.Path.GetFileNameWithoutExtension(itemName)}_{counter}{System.IO.Path.GetExtension(itemName)}";
                newPath = System.IO.Path.Combine(destinationPath, newName);
                counter++;
            }

            return newPath;
        }

        public bool Delete()
        {
            try
            {
                string itemType = IsDirectory ? "folder" : "file";
                MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete this {itemType}?\n\nPath: {Path}",
                                                           "Delete Confirmation",
                                                           MessageBoxButton.YesNo,
                                                           MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    if (IsDirectory)
                    {
                        // Delete folder
                        Directory.Delete(Path, true); // Set to true for recursive deletion
                    }
                    else
                    {
                        // Delete file
                        File.Delete(Path);
                    }
                    //ShowStatusMessage("Deletion successful!", TimeSpan.FromSeconds(5));
                }
                // No need to handle "No" case as the deletion is canceled
                 return true;
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error deleting item: {ex.Message}");
                return false;
            }
        }



        private void ShowSuccessMessage(string message)
        {
            MessageBox.Show(message, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            Debug.WriteLine("READ DRIVE CALLED");
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
            Debug.WriteLine("GenerateListView called");
            var fileSystemItems = itemList.Select(item => new FileItem(item.Path,item.Name, item.Icon, item.Type, item.Date, item.Size)
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

        public void listEntryView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var listView = sender as ListView;
            var listViewItem = GetListViewItem(e.OriginalSource as DependencyObject);

            if (listViewItem == null)
            {
                return;
            }

            var fileItem = listViewItem.DataContext as FileItem;

            if (fileItem != null)
            {
                // Access fileItem properties here
                Debug.WriteLine($"Single click on item: {fileItem.Name}, Path: {fileItem.Path}");
            }
        }

        private ListViewItem GetListViewItem(DependencyObject obj)
        {
            while (obj != null && obj.GetType() != typeof(ListViewItem))
            {
                obj = VisualTreeHelper.GetParent(obj);
            }

            return obj as ListViewItem;
        }


        public void PerformOperationAndRefreshUI(FileItem selectedFile, string destinationPath, Action<string, string> operation)
        {
            try
            {
                if (selectedFile != null)
                {
                    // Perform the operation (e.g., copy or delete)
                    operation(selectedFile.Path, System.IO.Path.Combine(destinationPath, selectedFile.Name));
                    Debug.WriteLine($"File operation completed: {operation}");

                    // Update the UI
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        // Refresh the data source
                        ReadDrive(destinationPath);
                    });
                }
                // Handle other cases if needed
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error performing operation: {ex.Message}");
                // Handle the exception according to your application's requirements
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
            if (curIndex == 9)
            {
                pathArr.RemoveAt(0);
                pathArr.Add(newPath);
            }
            else
            {
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
            Debug.WriteLine(pathArr.Count);
            foreach (var path in pathArr)
            {
                Debug.WriteLine(path);
            }
            Debug.WriteLine("CUR", curIndex.ToString());


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


        private void listEntryView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (isDockPanel1Focused) {
                Debug.WriteLine("dockPanel1 has a single click");

                fileExplorer1.listEntryView_PreviewMouseLeftButtonDown(sender, e);
            }
            else if (isDockPanel2Focused) 
            {
                Debug.WriteLine("dockPanel2 has a single click");

                fileExplorer2.listEntryView_PreviewMouseLeftButtonDown(sender, e);

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

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            // Check if an item is selected in the ListView
            Debug.WriteLine("COPY");
            FileItem selectedFile = null;
            string destinationPath = null;
            if (isDockPanel1Focused)
            {
                selectedFile = listEntryView1.SelectedItem as FileItem;
                destinationPath = fileExplorer2.driveList.pathTextBlock.Text;
                if(selectedFile != null && destinationPath != null) {
                    selectedFile.CopyTo(destinationPath);
                    if (fileExplorer1.driveList.pathTextBlock.Text == fileExplorer1.driveList.pathTextBlock.Text)
                    {
                        RefreshUI(fileExplorer1, fileExplorer1.driveList.pathTextBlock.Text);
                        RefreshUI(fileExplorer2, fileExplorer2.driveList.pathTextBlock.Text);


                    }
                    else
                    {

                        RefreshUI(fileExplorer2, destinationPath);
                    }    
                    //fileExplorer1.PerformOperationAndRefreshUI(selectedFile, destinationPath, (source, destination) => File.Copy(source, destination, true));

                }

            }
            else if(isDockPanel2Focused)
            {
                selectedFile = listEntryView2.SelectedItem as FileItem;
                destinationPath = fileExplorer1.driveList.pathTextBlock.Text;
                if (selectedFile != null && destinationPath != null)
                {
                    selectedFile.CopyTo(destinationPath);
                    if (fileExplorer1.driveList.pathTextBlock.Text == fileExplorer1.driveList.pathTextBlock.Text)
                    {
                        RefreshUI(fileExplorer1, fileExplorer1.driveList.pathTextBlock.Text);
                        RefreshUI(fileExplorer2, fileExplorer2.driveList.pathTextBlock.Text);


                    }
                    else
                    {
                        RefreshUI(fileExplorer1, destinationPath);

                    }    
                    //fileExplorer1.PerformOperationAndRefreshUI(selectedFile, destinationPath, (source, destination) => File.Copy(source, destination, true));
                }

            }
            else
            {
                // Handle the case where no item is selected
                MessageBox.Show("Please select an item to copy.", "No Item Selected", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private void MoveButton_Click(object sender, RoutedEventArgs e)
        {
            // Check if an item is selected in the ListView
            Debug.WriteLine("MOVE");
            FileItem selectedFile = null;
            string destinationPath = null;
            if (isDockPanel1Focused)
            {
                selectedFile = listEntryView1.SelectedItem as FileItem;
                destinationPath = fileExplorer2.driveList.pathTextBlock.Text;
                if (selectedFile != null && destinationPath != null)
                {
                    selectedFile.MoveTo(destinationPath);
                    RefreshUI(fileExplorer2, destinationPath);
                    RefreshUI(fileExplorer1, fileExplorer1.driveList.pathTextBlock.Text);
                }
            }
            else if (isDockPanel2Focused)
            {
                selectedFile = listEntryView2.SelectedItem as FileItem;
                destinationPath = fileExplorer1.driveList.pathTextBlock.Text;
                if (selectedFile != null && destinationPath != null)
                {
                    selectedFile.MoveTo(destinationPath);
                    RefreshUI(fileExplorer1, destinationPath);
                    RefreshUI(fileExplorer2, fileExplorer2.driveList.pathTextBlock.Text);

                }
            }
            else
            {
                // Handle the case where no item is selected
                MessageBox.Show("Please select an item to move.", "No Item Selected", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            FileItem selectedFile = null;

            if (isDockPanel1Focused)
            {
                selectedFile = listEntryView1.SelectedItem as FileItem;
            }
            else if (isDockPanel2Focused)
            {
                selectedFile = listEntryView2.SelectedItem as FileItem;
            }

            if (selectedFile != null)
            {
                bool isDelete = selectedFile.Delete();
                if (isDelete)
                {

                    if (fileExplorer1.driveList.pathTextBlock.Text == fileExplorer1.driveList.pathTextBlock.Text)
                    {
                        RefreshUI(fileExplorer1, fileExplorer1.driveList.pathTextBlock.Text);
                        RefreshUI(fileExplorer2, fileExplorer2.driveList.pathTextBlock.Text);


                    }
                    else
                    {
                        if (isDockPanel1Focused)
                        {
                            RefreshUI(fileExplorer1, fileExplorer1.driveList.pathTextBlock.Text);

                        }
                        else if (isDockPanel2Focused)
                        {
                            RefreshUI(fileExplorer2, fileExplorer2.driveList.pathTextBlock.Text);

                        }
                    }
                    ShowStatusMessage("Delete SuccessFully!", TimeSpan.FromSeconds(5));

                }


                // Make sure to refresh the UI after deletion
            }
            else
            {
                // Handle the case where no item is selected
                MessageBox.Show("Please select an item to delete.", "No Item Selected", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }


        private void RefreshUI(FileExplorer fileExplorer, string destinationPath)
        {
            Debug.WriteLine("REFresh");
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Refresh the data source
                fileExplorer.ReadDrive(destinationPath);
            });
        }

        private void ShowStatusMessage(string message, TimeSpan duration)
        {
            statusMessage.Text = message;
            statusPopup.IsOpen = true;

            var timer = new DispatcherTimer { Interval = duration };
            timer.Tick += (sender, args) =>
            {
                statusPopup.IsOpen = false;
                timer.Stop();
            };

            timer.Start();
        }


    }
    


}