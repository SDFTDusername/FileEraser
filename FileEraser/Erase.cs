namespace FileEraser
{
    public static class Erase
    {
        private static readonly byte[] bytes8 = { 0, 0, 0, 0, 0, 0, 0, 0 };

        public static bool FillFileWithZeros(FileInfo fileInfo, bool changeSize)
        {
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

                return true;
            }

            return false;
        }

        public static void RenameFileName(FileInfo fileInfo, bool changeLength)
        {
            int attempt = 0;
            while (true)
            {
                string suffix = (attempt == 0) ? "" : attempt.ToString();

                string newPath = Path.Join(fileInfo.DirectoryName, new string('a', fileInfo.Name.Length) + suffix);
                if (!Path.Exists(newPath))
                {
                    fileInfo.MoveTo(newPath);
                    if (!changeLength) break;
                }
                else
                {
                    ++attempt;
                    continue;
                }

                if (changeLength)
                {
                    newPath = Path.Join(fileInfo.DirectoryName, "a" + suffix);

                    if (!Path.Exists(newPath))
                    {
                        fileInfo.MoveTo(newPath);
                        break;
                    }
                    else
                        ++attempt;
                }
            }
        }

        public static void RenameFolderName(DirectoryInfo folderInfo, bool changeLength)
        {
            int attempt = 0;
            while (true)
            {
                string suffix = (attempt == 0) ? "" : attempt.ToString();

                string newPath = Path.Join(folderInfo.Parent?.FullName, new string('a', folderInfo.Name.Length) + suffix);
                if (!Path.Exists(newPath))
                {
                    folderInfo.MoveTo(newPath);
                    if (!changeLength) break;
                }
                else
                {
                    ++attempt;
                    continue;
                }

                if (changeLength)
                {
                    newPath = Path.Join(folderInfo.Parent?.FullName, "a" + suffix);

                    if (!Path.Exists(newPath))
                    {
                        folderInfo.MoveTo(newPath);
                        break;
                    }
                    else
                        ++attempt;
                }
            }
        }
    }
}
