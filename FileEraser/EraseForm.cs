using System.ComponentModel;
using System.Media;
using System.Security;

namespace FileEraser
{
    public partial class EraseForm : Form
    {
        public bool Cancelled = false;

        private long fileCount = 0;
        private long totalSize = 0;

        private long failedFiles = 0;
        private long failedFolders = 0;

        private Queue<long> size = new Queue<long>();

        private long erasedFileCount = 0;

        private string[] fileNames;
        private Queue<FileSystemInfo> queue = new Queue<FileSystemInfo>();

        private EnumerationOptions enumOptions = new EnumerationOptions { IgnoreInaccessible = true };

        private bool changeSize = true;
        private bool changeName = true;
        private bool changeNameLength = true;
        private bool delete = false;

        public EraseForm(string[] fileNames)
        {
            this.fileNames = fileNames;

            if (MessageBox.Show("Once the file(s) are permanently deleted, you can no longer recover them. Metadata may still be recoverable.\r\nDo you want to continue?", "Erase file(s)?", MessageBoxButtons.YesNo, MessageBoxIcon.None) != DialogResult.Yes)
            {
                Cancelled = true;
                Close();
                return;
            }

            InitializeComponent();

            updateDiscoverStatus();
            Text = $"Discovering file(s)...";
            progressBar.Style = ProgressBarStyle.Marquee;
        }

        private void EraseForm_Load(object sender, EventArgs e)
        {
            BackgroundWorker bgWorker = new BackgroundWorker();
            bgWorker.WorkerReportsProgress = true;

            bgWorker.DoWork += discoverBgWorker_DoWork;
            bgWorker.ProgressChanged += discoverBgWorker_ProgressChanged;
            bgWorker.RunWorkerCompleted += discoverBgWorker_RunWorkerCompleted;

            bgWorker.RunWorkerAsync();
        }

        private void checkFailed(string action, string pastTenseWord)
        {
            List<string> items = new List<string>();

            if (failedFiles > 0) items.Add($"{failedFiles:n0} file{(failedFiles == 1 ? "" : "s")}");
            if (failedFolders > 0) items.Add($"{failedFolders:n0} folder{(failedFolders == 1 ? "" : "s")}");

            if (items.Count > 0)
                MessageBox.Show($"Couldn't discover {action} {string.Join(" and ", fileNames)}", $"{pastTenseWord} file{(fileNames.Length == 1 ? "" : "s")}", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void updateDiscoverStatus()
        {
            statusLabel.Text = $"Discovered {fileCount:n0} file{(fileCount == 1 ? "" : "s")}... ({Format.ByteSize(totalSize)})";
        }

        private void discoverBgWorker_DoWork(object? sender, DoWorkEventArgs e)
        {
            BackgroundWorker? bgWorker = sender as BackgroundWorker;

            if (bgWorker == null)
                return;

            fileCount = 0;

            void addFolder(DirectoryInfo folder)
            {
                foreach (DirectoryInfo folder2 in folder.EnumerateDirectories("*", enumOptions))
                    addFolder(folder2);

                foreach (FileInfo file in folder.EnumerateFiles("*", enumOptions))
                    addFile(file);

                queue.Enqueue(folder);
            }

            void addFile(FileInfo file)
            {
                FileStream fileStream;

                try
                {
                    fileStream = File.Open(file.FullName, FileMode.Open);

                    if (!fileStream.CanWrite)
                    {
                        fileStream.Close();
                        ++failedFiles;
                        return;
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    ++failedFiles;
                    return;
                }
                catch (IOException)
                {
                    ++failedFiles;
                    return;
                }

                fileStream.Close();

                queue.Enqueue(file);

                ++fileCount;
                totalSize += file.Length;
                size.Enqueue(file.Length);

                bgWorker.ReportProgress(0);
            }

            foreach (string path in fileNames)
            {
                if (File.Exists(path))
                {
                    try
                    {
                        addFile(new FileInfo(path));
                    }
                    catch (SecurityException) { ++failedFiles; }
                    catch (UnauthorizedAccessException) { ++failedFiles; }
                }
                else if (Directory.Exists(path))
                {
                    try
                    {
                        addFolder(new DirectoryInfo(path));
                    }
                    catch (SecurityException) { ++failedFolders; }
                    catch (UnauthorizedAccessException) { ++failedFolders; }
                }
            }
        }

        private void discoverBgWorker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            updateDiscoverStatus();
        }

        private void updateEraseStatus()
        {
            statusLabel.Text = $"Erased {erasedFileCount:n0} out of {fileCount:n0} file{(fileCount == 1 ? "" : "s")}... ({Format.ByteSize(totalSize)} left)";
            progressBar.Value = (int)((double)erasedFileCount / fileCount * 1000d);
        }

        private void discoverBgWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            checkFailed("discover", "Discovered");

            failedFiles = 0;
            failedFolders = 0;

            updateEraseStatus();
            progressBar.Style = ProgressBarStyle.Blocks;
            Text = $"Erasing file{(fileCount == 1 ? "" : "s")}...";

            BackgroundWorker bgWorker = new BackgroundWorker();
            bgWorker.WorkerReportsProgress = true;

            bgWorker.DoWork += eraseBgWorker_DoWork;
            bgWorker.ProgressChanged += eraseBgWorker_ProgressChanged;
            bgWorker.RunWorkerCompleted += eraseBgWorker_RunWorkerCompleted;

            bgWorker.RunWorkerAsync();
        }

        private void eraseBgWorker_DoWork(object? sender, DoWorkEventArgs e)
        {
            BackgroundWorker? bgWorker = sender as BackgroundWorker;

            if (bgWorker == null)
                return;

            while (queue.Count > 0)
            {
                FileSystemInfo fileSystemInfo = queue.Dequeue();

                if (fileSystemInfo is FileInfo)
                {
                    FileInfo fileInfo = (FileInfo)fileSystemInfo;

                    bool success = Erase.FillFileWithZeros(fileInfo, changeSize);

                    if (changeName)
                        Erase.RenameFileName(fileInfo, changeNameLength);

                    if (success)
                    {
                        if (delete)
                            fileInfo.Delete();

                        ++erasedFileCount;
                        totalSize -= size.Dequeue();
                        bgWorker.ReportProgress(0);
                    }
                    else
                        ++failedFiles;
                }
                else if (fileSystemInfo is DirectoryInfo)
                {
                    DirectoryInfo folderInfo = (DirectoryInfo)fileSystemInfo;
                    
                    if (changeName)
                        Erase.RenameFolderName(folderInfo, changeNameLength);

                    if (delete && !folderInfo.EnumerateFileSystemInfos().Any())
                        folderInfo.Delete();
                }
            }
        }

        private void eraseBgWorker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            updateEraseStatus();
        }

        private void eraseBgWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            checkFailed("erase", "Erased");

            SystemSounds.Beep.Play();
            Close();
        }
    }
}
