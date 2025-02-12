using Avatar_Explorer.Classes;

namespace Avatar_Explorer.Forms
{
    partial class Main
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
            UndoButton = new Button();
            PathTextBox = new TextBox();
            AvatarSearchFilterList = new TabControl();
            AvatarPage = new TabPage();
            AvatarAuthorPage = new TabPage();
            CategoryPage = new TabPage();
            AddItemButton = new Button();
            ExplorerList = new TabControl();
            AvatarItemExplorer = new TabPage();
            StartLabel = new Label();
            SearchBox = new TextBox();
            label1 = new Label();
            SearchResultLabel = new Label();
            ExportButton = new Button();
            MakeBackupButton = new Button();
            label2 = new Label();
            LanguageBox = new ComboBox();
            ManageCommonAvatarButton = new Button();
            LoadData = new Button();
            SortingBox = new ComboBox();
            SortingLabel = new Label();
            AvatarSearchFilterList.SuspendLayout();
            ExplorerList.SuspendLayout();
            AvatarItemExplorer.SuspendLayout();
            SuspendLayout();
            // 
            // UndoButton
            // 
            UndoButton.Location = new Point(23, 15);
            UndoButton.Name = "UndoButton";
            UndoButton.Size = new Size(97, 23);
            UndoButton.TabIndex = 0;
            UndoButton.Text = "戻る";
            UndoButton.UseVisualStyleBackColor = true;
            UndoButton.Click += UndoButton_Click;
            // 
            // PathTextBox
            // 
            PathTextBox.Font = new Font("Yu Gothic UI", 10F);
            PathTextBox.Location = new Point(148, 16);
            PathTextBox.Name = "PathTextBox";
            PathTextBox.Size = new Size(733, 25);
            PathTextBox.TabIndex = 2;
            PathTextBox.Text = "ここには現在のパスが表示されます";
            // 
            // AvatarSearchFilterList
            // 
            AvatarSearchFilterList.Controls.Add(AvatarPage);
            AvatarSearchFilterList.Controls.Add(AvatarAuthorPage);
            AvatarSearchFilterList.Controls.Add(CategoryPage);
            AvatarSearchFilterList.Location = new Point(16, 45);
            AvatarSearchFilterList.Name = "AvatarSearchFilterList";
            AvatarSearchFilterList.SelectedIndex = 0;
            AvatarSearchFilterList.Size = new Size(333, 590);
            AvatarSearchFilterList.TabIndex = 3;
            // 
            // AvatarPage
            // 
            AvatarPage.AllowDrop = true;
            AvatarPage.AutoScroll = true;
            AvatarPage.Location = new Point(4, 24);
            AvatarPage.Name = "AvatarPage";
            AvatarPage.Padding = new Padding(3);
            AvatarPage.Size = new Size(325, 562);
            AvatarPage.TabIndex = 0;
            AvatarPage.Text = "アバター";
            AvatarPage.UseVisualStyleBackColor = true;
            AvatarPage.DragDrop += AvatarPage_DragDrop;
            AvatarPage.DragEnter += Helper.DragEnter;
            // 
            // AvatarAuthorPage
            // 
            AvatarAuthorPage.AutoScroll = true;
            AvatarAuthorPage.Location = new Point(4, 24);
            AvatarAuthorPage.Name = "AvatarAuthorPage";
            AvatarAuthorPage.Padding = new Padding(3);
            AvatarAuthorPage.Size = new Size(325, 562);
            AvatarAuthorPage.TabIndex = 1;
            AvatarAuthorPage.Text = "作者";
            AvatarAuthorPage.UseVisualStyleBackColor = true;
            // 
            // CategoryPage
            // 
            CategoryPage.AutoScroll = true;
            CategoryPage.Location = new Point(4, 24);
            CategoryPage.Name = "CategoryPage";
            CategoryPage.Size = new Size(325, 562);
            CategoryPage.TabIndex = 2;
            CategoryPage.Text = "カテゴリ別";
            CategoryPage.UseVisualStyleBackColor = true;
            // 
            // AddItemButton
            // 
            AddItemButton.Location = new Point(1093, 641);
            AddItemButton.Name = "AddItemButton";
            AddItemButton.Size = new Size(161, 46);
            AddItemButton.TabIndex = 5;
            AddItemButton.Text = "アイテムの追加";
            AddItemButton.UseVisualStyleBackColor = true;
            AddItemButton.Click += AddItemButton_Click;
            // 
            // ExplorerList
            // 
            ExplorerList.Controls.Add(AvatarItemExplorer);
            ExplorerList.Location = new Point(355, 45);
            ExplorerList.Name = "ExplorerList";
            ExplorerList.SelectedIndex = 0;
            ExplorerList.Size = new Size(899, 590);
            ExplorerList.TabIndex = 6;
            // 
            // AvatarItemExplorer
            // 
            AvatarItemExplorer.AllowDrop = true;
            AvatarItemExplorer.AutoScroll = true;
            AvatarItemExplorer.Controls.Add(StartLabel);
            AvatarItemExplorer.Location = new Point(4, 24);
            AvatarItemExplorer.Name = "AvatarItemExplorer";
            AvatarItemExplorer.Padding = new Padding(3);
            AvatarItemExplorer.Size = new Size(891, 562);
            AvatarItemExplorer.TabIndex = 0;
            AvatarItemExplorer.Text = "アイテム";
            AvatarItemExplorer.UseVisualStyleBackColor = true;
            AvatarItemExplorer.DragDrop += AvatarItemExplorer_DragDrop;
            AvatarItemExplorer.DragEnter += Helper.DragEnter;
            // 
            // StartLabel
            // 
            StartLabel.AutoSize = true;
            StartLabel.Font = new Font(GuiFont, 15F);
            StartLabel.Location = new Point(150, 252);
            StartLabel.Name = "StartLabel";
            StartLabel.Size = new Size(591, 58);
            StartLabel.TabIndex = 7;
            StartLabel.Text = "左のメニューからアイテムを選択してください\r\nそれか、右下の\"アイテムの追加\"でアイテムを追加してください";
            StartLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // SearchBox
            // 
            SearchBox.Font = new Font("Yu Gothic UI", 11F);
            SearchBox.Location = new Point(980, 16);
            SearchBox.Name = "SearchBox";
            SearchBox.Size = new Size(270, 27);
            SearchBox.TabIndex = 7;
            SearchBox.KeyDown += SearchBox_KeyDown;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font(GuiFont, 12F);
            label1.Location = new Point(917, 20);
            label1.Name = "label1";
            label1.Size = new Size(42, 23);
            label1.TabIndex = 8;
            label1.Text = "検索";
            // 
            // SearchResultLabel
            // 
            SearchResultLabel.AutoSize = true;
            SearchResultLabel.Font = new Font(GuiFont, 10F);
            SearchResultLabel.Location = new Point(917, 45);
            SearchResultLabel.Name = "SearchResultLabel";
            SearchResultLabel.Size = new Size(0, 20);
            SearchResultLabel.TabIndex = 9;
            // 
            // ExportButton
            // 
            ExportButton.Font = new Font("Yu Gothic UI", 10F);
            ExportButton.Location = new Point(20, 641);
            ExportButton.Name = "ExportButton";
            ExportButton.Size = new Size(161, 46);
            ExportButton.TabIndex = 10;
            ExportButton.Text = "CSVに出力";
            ExportButton.UseVisualStyleBackColor = true;
            ExportButton.Click += ExportButton_Click;
            // 
            // MakeBackupButton
            // 
            MakeBackupButton.Font = new Font("Yu Gothic UI", 10F);
            MakeBackupButton.Location = new Point(209, 641);
            MakeBackupButton.Name = "MakeBackupButton";
            MakeBackupButton.Size = new Size(161, 46);
            MakeBackupButton.TabIndex = 11;
            MakeBackupButton.Text = "バックアップの作成";
            MakeBackupButton.UseVisualStyleBackColor = true;
            MakeBackupButton.Click += MakeBackupButton_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Yu Gothic UI", 13F);
            label2.Location = new Point(775, 634);
            label2.Name = "label2";
            label2.Size = new Size(142, 25);
            label2.TabIndex = 12;
            label2.Text = "言語 / Language";
            // 
            // LanguageBox
            // 
            LanguageBox.DropDownStyle = ComboBoxStyle.DropDownList;
            LanguageBox.Font = new Font("Yu Gothic UI", 12F);
            LanguageBox.FormattingEnabled = true;
            LanguageBox.Items.AddRange(new object[] { "日本語", "한국어", "English" });
            LanguageBox.Location = new Point(785, 662);
            LanguageBox.Name = "LanguageBox";
            LanguageBox.Size = new Size(121, 29);
            LanguageBox.TabIndex = 13;
            LanguageBox.SelectedIndex = 0;
            LanguageBox.SelectedIndexChanged += LanguageBox_SelectedIndexChanged;
            // 
            // ManageCommonAvatarButton
            // 
            ManageCommonAvatarButton.Font = new Font("Yu Gothic UI", 10F);
            ManageCommonAvatarButton.Location = new Point(398, 641);
            ManageCommonAvatarButton.Name = "ManageCommonAvatarButton";
            ManageCommonAvatarButton.Size = new Size(161, 46);
            ManageCommonAvatarButton.TabIndex = 14;
            ManageCommonAvatarButton.Text = "共通素体の管理";
            ManageCommonAvatarButton.UseVisualStyleBackColor = true;
            ManageCommonAvatarButton.Click += ManageCommonAvatarButton_Click;
            // 
            // LoadData
            // 
            LoadData.Font = new Font("Yu Gothic UI", 10F);
            LoadData.Location = new Point(587, 641);
            LoadData.Name = "LoadData";
            LoadData.Size = new Size(161, 46);
            LoadData.TabIndex = 15;
            LoadData.Text = "データを読み込む";
            LoadData.UseVisualStyleBackColor = true;
            LoadData.Click += LoadData_Click;
            // 
            // SortingBox
            // 
            SortingBox.DropDownStyle = ComboBoxStyle.DropDownList;
            SortingBox.Font = new Font("Yu Gothic UI", 12F);
            SortingBox.FormattingEnabled = true;
            SortingBox.Items.AddRange(new object[] { "タイトル", "作者" });
            SortingBox.Location = new Point(943, 662);
            SortingBox.Name = "SortingBox";
            SortingBox.Size = new Size(121, 29);
            SortingBox.TabIndex = 17;
            SortingBox.SelectedIndex = 0;
            SortingBox.SelectedIndexChanged += SortingBox_SelectedIndexChanged;
            // 
            // SortingLabel
            // 
            SortingLabel.AutoSize = true;
            SortingLabel.Font = new Font("Yu Gothic UI", 13F);
            SortingLabel.Location = new Point(955, 634);
            SortingLabel.Name = "SortingLabel";
            SortingLabel.Size = new Size(77, 25);
            SortingLabel.TabIndex = 16;
            SortingLabel.Text = "並び替え順";
            // 
            // Main
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1266, 699);
            Controls.Add(SortingBox);
            Controls.Add(SortingLabel);
            Controls.Add(LoadData);
            Controls.Add(ManageCommonAvatarButton);
            Controls.Add(LanguageBox);
            Controls.Add(label2);
            Controls.Add(MakeBackupButton);
            Controls.Add(ExportButton);
            Controls.Add(SearchResultLabel);
            Controls.Add(label1);
            Controls.Add(SearchBox);
            Controls.Add(ExplorerList);
            Controls.Add(AddItemButton);
            Controls.Add(AvatarSearchFilterList);
            Controls.Add(PathTextBox);
            Controls.Add(UndoButton);
            Name = "Main";
            Text = "Avatar Explorer";
            FormClosing += Main_FormClosing;
            Resize += Main_Resize;
            AvatarSearchFilterList.ResumeLayout(false);
            ExplorerList.ResumeLayout(false);
            AvatarItemExplorer.ResumeLayout(false);
            AvatarItemExplorer.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button UndoButton;
        private TextBox PathTextBox;
        private TabControl AvatarSearchFilterList;
        private TabPage AvatarPage;
        private TabPage AvatarAuthorPage;
        private Button AddItemButton;
        private TabControl ExplorerList;
        private TabPage AvatarItemExplorer;
        private Label StartLabel;
        private TextBox SearchBox;
        private Label label1;
        private Label SearchResultLabel;
        private Button ExportButton;
        private TabPage CategoryPage;
        private Button MakeBackupButton;
        private Label label2;
        private ComboBox LanguageBox;
        private Button ManageCommonAvatarButton;
        private Button LoadData;
        private ComboBox SortingBox;
        private Label SortingLabel;
    }
}
