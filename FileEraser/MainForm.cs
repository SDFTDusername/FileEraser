using Microsoft.WindowsAPICodePack.Dialogs;

namespace FileEraser
{
    public partial class MainForm : Form
    {
        private CommonOpenFileDialog dialog = new CommonOpenFileDialog();
        private ImageList iconList = new ImageList();

        private List<string> fileNames = new List<string>();

        private int folderCount = 0;

        public MainForm()
        {
            InitializeComponent();

            dialog.Multiselect = true;

            addDefaultIcons();
            itemsTreeView.ImageList = iconList;
        }

        private void addDefaultIcons()
        {
            iconList.Images.Add(Properties.Resources.unknown);
            iconList.Images.Add(Properties.Resources.folder);
        }

        private int getIcon(string fileName)
        {
            Icon? fileIcon = Icon.ExtractAssociatedIcon(fileName);

            if (fileIcon != null)
            {
                iconList.Images.Add(fileIcon);
                return iconList.Images.Count - 1;
            }

            return 0;
        }

        private void addFile(string fileName)
        {
            if (fileNames.Contains(fileName))
                return;

            TreeNode node = new TreeNode();
            node.Text = fileName;

            node.ImageIndex = getIcon(fileName);
            node.SelectedImageIndex = node.ImageIndex;

            itemsTreeView.Nodes.Add(node);
            fileNames.Add(fileName);

            eraseAllButton.Enabled = true;
        }

        private void addFolder(string folderName)
        {
            if (fileNames.Contains(folderName))
                return;

            TreeNode node = new TreeNode();
            node.Text = folderName;

            node.ImageIndex = 1;
            node.SelectedImageIndex = 1;

            itemsTreeView.Nodes.Insert(folderCount, node);
            fileNames.Insert(folderCount, folderName);

            ++folderCount;
            eraseAllButton.Enabled = true;
        }

        private void addFilesButton_Click(object sender, EventArgs e)
        {
            dialog.IsFolderPicker = false;

            if (dialog.ShowDialog() != CommonFileDialogResult.Ok)
                return;

            foreach (string fileName in dialog.FileNames)
                addFile(fileName);
        }

        private void addFoldersButton_Click(object sender, EventArgs e)
        {
            dialog.IsFolderPicker = true;

            if (dialog.ShowDialog() != CommonFileDialogResult.Ok)
                return;

            foreach (string folderName in dialog.FileNames)
                addFolder(folderName);
        }

        private void itemsTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            removeItemButton.Enabled = itemsTreeView.SelectedNode != null;
        }

        private void removeItemButton_Click(object sender, EventArgs e)
        {
            if (itemsTreeView.SelectedNode != null)
            {
                if (Directory.Exists(itemsTreeView.SelectedNode.Text))
                    --folderCount;

                fileNames.RemoveAt(itemsTreeView.SelectedNode.Index);
                itemsTreeView.Nodes.RemoveAt(itemsTreeView.SelectedNode.Index);

                itemsTreeView.SelectedNode = null;
                removeItemButton.Enabled = false;

                if (fileNames.Count == 0)
                    eraseAllButton.Enabled = false;
            }
        }

        private void itemsTreeView_DragDrop(object sender, DragEventArgs e)
        {
            string[]? fileNames = (string[]?)e.Data?.GetData(DataFormats.FileDrop);

            if (fileNames == null)
                return;

            foreach (string fileName in fileNames)
            {
                if (Directory.Exists(fileName))
                    addFolder(fileName);
                else if (File.Exists(fileName))
                    addFile(fileName);
            }
        }

        private void itemsTreeView_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Link;
        }

        private void eraseAllButton_Click(object sender, EventArgs e)
        {
            EraseForm eraseForm = new EraseForm(fileNames.ToArray());
            eraseForm.StartPosition = FormStartPosition.CenterParent;

            if (!eraseForm.Cancelled)
            {
                eraseForm.ShowDialog();

                fileNames.Clear();
                itemsTreeView.Nodes.Clear();

                iconList.Images.Clear();
                addDefaultIcons();

                folderCount = 0;
                eraseAllButton.Enabled = false;
            }
        }
    }
}
