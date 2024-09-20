namespace FileEraser
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            itemsTreeView = new TreeView();
            noteLabel = new Label();
            eraseAllButton = new Button();
            addFilesButton = new Button();
            removeItemButton = new Button();
            addFoldersButton = new Button();
            SuspendLayout();
            // 
            // itemsTreeView
            // 
            itemsTreeView.AllowDrop = true;
            itemsTreeView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            itemsTreeView.Location = new Point(12, 43);
            itemsTreeView.Name = "itemsTreeView";
            itemsTreeView.ShowPlusMinus = false;
            itemsTreeView.ShowRootLines = false;
            itemsTreeView.Size = new Size(440, 211);
            itemsTreeView.TabIndex = 3;
            itemsTreeView.AfterSelect += itemsTreeView_AfterSelect;
            itemsTreeView.DragDrop += itemsTreeView_DragDrop;
            itemsTreeView.DragEnter += itemsTreeView_DragEnter;
            // 
            // noteLabel
            // 
            noteLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            noteLabel.AutoSize = true;
            noteLabel.ForeColor = SystemColors.ControlDark;
            noteLabel.Location = new Point(12, 257);
            noteLabel.Name = "noteLabel";
            noteLabel.Size = new Size(187, 15);
            noteLabel.TabIndex = 4;
            noteLabel.Text = "Metadata may still be recoverable.";
            // 
            // eraseAllButton
            // 
            eraseAllButton.Anchor = AnchorStyles.Bottom;
            eraseAllButton.AutoSize = true;
            eraseAllButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            eraseAllButton.Enabled = false;
            eraseAllButton.Location = new Point(202, 284);
            eraseAllButton.Name = "eraseAllButton";
            eraseAllButton.Size = new Size(59, 25);
            eraseAllButton.TabIndex = 5;
            eraseAllButton.Text = "Erase all";
            eraseAllButton.UseVisualStyleBackColor = true;
            eraseAllButton.Click += eraseAllButton_Click;
            // 
            // addFilesButton
            // 
            addFilesButton.AutoSize = true;
            addFilesButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            addFilesButton.Location = new Point(12, 12);
            addFilesButton.Name = "addFilesButton";
            addFilesButton.Size = new Size(63, 25);
            addFilesButton.TabIndex = 0;
            addFilesButton.Text = "Add files";
            addFilesButton.UseVisualStyleBackColor = true;
            addFilesButton.Click += addFilesButton_Click;
            // 
            // removeItemButton
            // 
            removeItemButton.AutoSize = true;
            removeItemButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            removeItemButton.Enabled = false;
            removeItemButton.Location = new Point(165, 12);
            removeItemButton.Name = "removeItemButton";
            removeItemButton.Size = new Size(87, 25);
            removeItemButton.TabIndex = 2;
            removeItemButton.Text = "Remove item";
            removeItemButton.UseVisualStyleBackColor = true;
            removeItemButton.Click += removeItemButton_Click;
            // 
            // addFoldersButton
            // 
            addFoldersButton.AutoSize = true;
            addFoldersButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            addFoldersButton.Location = new Point(81, 12);
            addFoldersButton.Name = "addFoldersButton";
            addFoldersButton.Size = new Size(78, 25);
            addFoldersButton.TabIndex = 1;
            addFoldersButton.Text = "Add folders";
            addFoldersButton.UseVisualStyleBackColor = true;
            addFoldersButton.Click += addFoldersButton_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(464, 321);
            Controls.Add(addFoldersButton);
            Controls.Add(removeItemButton);
            Controls.Add(addFilesButton);
            Controls.Add(eraseAllButton);
            Controls.Add(noteLabel);
            Controls.Add(itemsTreeView);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MinimumSize = new Size(280, 189);
            Name = "MainForm";
            Text = "File Eraser";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TreeView itemsTreeView;
        private Label noteLabel;
        private Button eraseAllButton;
        private Button addFilesButton;
        private Button removeItemButton;
        private Button addFoldersButton;
    }
}
