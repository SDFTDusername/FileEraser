using System.ComponentModel;
using System.Diagnostics;
using System.Security;

namespace FileEraser
{
    public partial class EraseForm : Form
    {
        public bool Cancelled = false;

        private long fileCount = 0;
        private long totalSize = 0;

        private Queue<long> size = new Queue<long>();

        private long erasedFileCount = 0;

        private string[] fileNames;
        private Queue<FileSystemInfo> queue = new Queue<FileSystemInfo>();

        private EnumerationOptions enumOptions = new EnumerationOptions { IgnoreInaccessible = true };

        private bool changeSize = true;
        private bool changeName = true;
        private bool changeNameLength = true;

        public EraseForm(string[] fileNames)
        {
            this.fileNames = fileNames;

            if (MessageBox.Show("Once the file data is erased, you cannot recover it. Do you want to continue?\nFile names, file sizes, and metadata may still be recoverable.\nInaccesible or readonly files and folders are ignored.", "Erase files?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                Cancelled = true;
                return;
            }

            InitializeComponent();
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
                        return;
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    return;
                }
                catch (IOException)
                {
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
                try {
                    if (Directory.Exists(path))
                        addFolder(new DirectoryInfo(path));
                    else if (File.Exists(path))
                        addFile(new FileInfo(path));
                } catch (SecurityException) { }
                catch (UnauthorizedAccessException) { }
            }
        }

        private void discoverBgWorker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            statusLabel.Text = $"Discovered {fileCount:n0} file{(fileCount == 1 ? "" : "s")}... ({Format.ByteSize(totalSize)})";
        }

        private void discoverBgWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            statusLabel.Text = $"Erased 0 out of {fileCount:n0} file{(fileCount == 1 ? "" : "s")}... ({Format.ByteSize(totalSize)} left)";
            progressBar.Style = ProgressBarStyle.Blocks;

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

            byte[] bytes8 = { 0, 0, 0, 0, 0, 0, 0, 0 };

            while (queue.Count > 0)
            {
                FileSystemInfo fileSystemInfo = queue.Dequeue();

                if (fileSystemInfo is FileInfo)
                {
                    //try
                    //{
                        FileInfo fileInfo = (FileInfo)fileSystemInfo;
                        FileStream fileStream = File.Open(fileInfo.FullName, FileMode.Open);

                        if (fileStream.CanWrite)
                        {
                            long bytes8Count = fileInfo.Length / 8;
                            byte byteCount = (byte)(fileInfo.Length % 8);

                            for (long i = 0; i < bytes8Count; ++i)
                                fileStream.Write(bytes8, 0, 8);

                            for (byte i = 0; i < byteCount; ++i)
                                fileStream.WriteByte(0);
                            
                            fileStream.Close();

                            if (changeSize)
                            {
                                fileStream = File.Open(fileInfo.FullName, FileMode.Truncate);
                                fileStream.WriteByte(0);
                                fileStream.Close();
                            }

                            if (changeName)
                            {
                                int attempt = 0;
                                while (true)
                                {
                                    string suffix = (attempt == 0) ? "" : attempt.ToString();

                                    string newPath = Path.Join(fileInfo.DirectoryName, new string('a', fileInfo.Name.Length) + suffix);
                                    if (!File.Exists(newPath))
                                    {
                                        fileInfo.MoveTo(newPath);
                                        if (!changeNameLength) break;
                                    }
                                    else
                                    {
                                        ++attempt;
                                        continue;
                                    }

                                    if (changeNameLength)
                                    {
                                        newPath = Path.Join(fileInfo.DirectoryName, "a" + suffix);

                                        if (!File.Exists(newPath))
                                        {
                                            fileInfo.MoveTo(newPath);
                                            break;
                                        }
                                        else
                                            ++attempt;
                                    }
                                }
                            }

                            //fileInfo.Delete();

                            ++erasedFileCount;
                            totalSize -= size.Dequeue();
                            bgWorker.ReportProgress(0);
                        }
                    //}
                    //catch (UnauthorizedAccessException) { }
                    //catch (IOException) { }
                }
                else if (fileSystemInfo is DirectoryInfo)
                {
                    DirectoryInfo directoryInfo = (DirectoryInfo)fileSystemInfo;

                    if (changeName)
                    {
                        int attempt = 0;
                        while (true)
                        {
                            string suffix = (attempt == 0) ? "" : attempt.ToString();

                            string newPath = Path.Join(directoryInfo.Parent?.FullName, new string('a', directoryInfo.Name.Length) + suffix);
                            if (!Directory.Exists(newPath))
                            {
                                directoryInfo.MoveTo(newPath);
                                if (!changeNameLength) break;
                            }
                            else
                            {
                                ++attempt;
                                continue;
                            }

                            if (changeNameLength)
                            {
                                newPath = Path.Join(directoryInfo.Parent?.FullName, "a" + suffix);

                                if (!Directory.Exists(newPath))
                                {
                                    Debug.WriteLine(directoryInfo.FullName, newPath);
                                    directoryInfo.MoveTo(newPath);
                                    break;
                                }
                                else
                                    ++attempt;
                            }
                        }
                    }

                    //if (!directoryInfo.EnumerateFileSystemInfos().Any())
                    //    directoryInfo.Delete();
                }
            }
        }

        private void eraseBgWorker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            statusLabel.Text = $"Erased {erasedFileCount:n0} out of {fileCount:n0} file{(fileCount == 1 ? "" : "s")}... ({Format.ByteSize(totalSize)} left)";
            progressBar.Value = (int)((double)erasedFileCount / (double)fileCount * 1000d);
        }

        private void eraseBgWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            Close();
        }
    }
}
