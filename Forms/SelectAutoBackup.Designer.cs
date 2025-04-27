namespace Avatar_Explorer.Forms
{
    partial class SelectAutoBackup
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            RestoreAutoBackup = new Label();
            RestoreDateLabel = new Label();
            BackupInfo = new Label();
            SelectBackup = new ComboBox();
            SelectButton = new Button();
            BackupDataInfo = new Label();
            SuspendLayout();
            // 
            // RestoreAutoBackup
            // 
            RestoreAutoBackup.AutoSize = true;
            RestoreAutoBackup.Font = new Font(_mainForm.GuiFont, 16F);
            RestoreAutoBackup.Location = new Point(12, 9);
            RestoreAutoBackup.Name = "RestoreAutoBackup";
            RestoreAutoBackup.Size = new Size(278, 32);
            RestoreAutoBackup.TabIndex = 0;
            RestoreAutoBackup.Text = "自動バックアップから復元";
            // 
            // RestoreDateLabel
            // 
            RestoreDateLabel.AutoSize = true;
            RestoreDateLabel.Font = new Font(_mainForm.GuiFont, 13F);
            RestoreDateLabel.Location = new Point(12, 56);
            RestoreDateLabel.Name = "RestoreDateLabel";
            RestoreDateLabel.Size = new Size(84, 26);
            RestoreDateLabel.TabIndex = 1;
            RestoreDateLabel.Text = "復元日時";
            // 
            // BackupInfo
            // 
            BackupInfo.AutoSize = true;
            BackupInfo.Font = new Font("Yu Gothic UI", 12F);
            BackupInfo.Location = new Point(12, 158);
            BackupInfo.Name = "BackupInfo";
            BackupInfo.Size = new Size(136, 63);
            BackupInfo.TabIndex = 2;
            // 
            // SelectBackup
            // 
            SelectBackup.Font = new Font(_mainForm.GuiFont, 11F);
            SelectBackup.FormattingEnabled = true;
            SelectBackup.Location = new Point(12, 85);
            SelectBackup.Name = "SelectBackup";
            SelectBackup.Size = new Size(332, 30);
            SelectBackup.TabIndex = 3;
            SelectBackup.Text = "選択されていません";
            SelectBackup.SelectedIndexChanged += SelectBackup_SelectedIndexChanged;
            // 
            // SelectButton
            // 
            SelectButton.Font = new Font(_mainForm.GuiFont, 10F);
            SelectButton.Location = new Point(84, 236);
            SelectButton.Name = "SelectButton";
            SelectButton.Size = new Size(184, 53);
            SelectButton.TabIndex = 4;
            SelectButton.Text = "このバックアップを復元";
            SelectButton.UseVisualStyleBackColor = true;
            SelectButton.Click += SelectButton_Click;
            // 
            // BackupDataInfo
            // 
            BackupDataInfo.AutoSize = true;
            BackupDataInfo.Font = new Font(_mainForm.GuiFont, 13F);
            BackupDataInfo.Location = new Point(12, 132);
            BackupDataInfo.Name = "BackupDataInfo";
            BackupDataInfo.Size = new Size(156, 26);
            BackupDataInfo.TabIndex = 5;
            BackupDataInfo.Text = "バックアップ情報";
            // 
            // SelectAutoBackup
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(361, 303);
            Controls.Add(BackupDataInfo);
            Controls.Add(SelectButton);
            Controls.Add(SelectBackup);
            Controls.Add(BackupInfo);
            Controls.Add(RestoreDateLabel);
            Controls.Add(RestoreAutoBackup);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "SelectAutoBackup";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "自動バックアップから復元";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label RestoreAutoBackup;
        private Label RestoreDateLabel;
        private Label BackupInfo;
        private ComboBox SelectBackup;
        private Button SelectButton;
        private Label BackupDataInfo;
    }
}