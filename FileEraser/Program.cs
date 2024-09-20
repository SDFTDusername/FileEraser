namespace FileEraser
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            if (args.Length > 0)
            {
                if (args.All(Path.Exists))
                    Application.Run(new EraseForm(args));
                else
                    MessageBox.Show("Invalid path(s)", "File Eraser", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
                Application.Run(new MainForm());
        }
    }
}