using Avatar_Explorer.Utils;

namespace Avatar_Explorer.Forms
{
    partial class ManageCommonAvatarsForm
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
            CommonAvatarsCombobox = new ComboBox();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            DeleteSelectedGroupButton = new Button();
            AddButton = new Button();
            NewLabel = new Label();
            AvatarList = new Panel();
            SuspendLayout();
            // 
            // CommonAvatarsCombobox
            // 
            CommonAvatarsCombobox.Font = new Font("Yu Gothic UI", 13F);
            CommonAvatarsCombobox.FormattingEnabled = true;
            CommonAvatarsCombobox.Location = new Point(237, 53);
            CommonAvatarsCombobox.Name = "CommonAvatarsCombobox";
            CommonAvatarsCombobox.Size = new Size(232, 31);
            CommonAvatarsCombobox.TabIndex = 0;
            CommonAvatarsCombobox.TextChanged += CommonAvatarsCombobox_TextChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font(_mainForm.GuiFont, 16F, FontStyle.Bold);
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(212, 32);
            label1.TabIndex = 1;
            label1.Text = "共通素体の管理画面";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font(_mainForm.GuiFont, 13F);
            label2.Location = new Point(16, 53);
            label2.Name = "label2";
            label2.Size = new Size(111, 26);
            label2.TabIndex = 2;
            label2.Text = "共通素体名: ";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font(_mainForm.GuiFont, 11F);
            label3.Location = new Point(475, 9);
            label3.Name = "label3";
            label3.Size = new Size(414, 42);
            label3.TabIndex = 3;
            label3.Text = "新しく入力すると新しい共通素体グループが作成されます！\r\n選ぶと編集画面になります！";
            // 
            // DeleteSelectedGroupButton
            // 
            DeleteSelectedGroupButton.Font = new Font("Yu Gothic UI", 13F);
            DeleteSelectedGroupButton.Location = new Point(12, 504);
            DeleteSelectedGroupButton.Name = "DeleteSelectedGroupButton";
            DeleteSelectedGroupButton.Size = new Size(227, 40);
            DeleteSelectedGroupButton.TabIndex = 4;
            DeleteSelectedGroupButton.Text = "選択中のグループを削除";
            DeleteSelectedGroupButton.UseVisualStyleBackColor = true;
            DeleteSelectedGroupButton.Click += DeleteSelectedGroupButton_Click;
            // 
            // AddButton
            // 
            AddButton.Font = new Font("Yu Gothic UI", 13F);
            AddButton.Location = new Point(792, 504);
            AddButton.Name = "AddButton";
            AddButton.Size = new Size(124, 40);
            AddButton.TabIndex = 6;
            AddButton.Text = "追加";
            AddButton.UseVisualStyleBackColor = true;
            AddButton.Click += AddButton_Click;
            // 
            // NewLabel
            // 
            NewLabel.AutoSize = true;
            NewLabel.Font = new Font("Yu Gothic UI", 12F);
            NewLabel.ForeColor = Color.ForestGreen;
            NewLabel.Location = new Point(475, 58);
            NewLabel.Name = "NewLabel";
            NewLabel.Size = new Size(253, 21);
            NewLabel.TabIndex = 0;
            NewLabel.Text = "新しく共通素体グループが作成されます";
            NewLabel.Visible = false;
            // 
            // AvatarList
            // 
            AvatarList.BackColor = Color.FromArgb(249, 249, 249);
            AvatarList.Location = new Point(16, 114);
            AvatarList.Name = "AvatarList";
            AvatarList.Size = new Size(900, 380);
            AvatarList.TabIndex = 7;
            // 
            // ManageCommonAvatarsForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(932, 553);
            Controls.Add(AvatarList);
            Controls.Add(NewLabel);
            Controls.Add(AddButton);
            Controls.Add(DeleteSelectedGroupButton);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(CommonAvatarsCombobox);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ManageCommonAvatarsForm";
            Text = "共通素体の管理";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ComboBox CommonAvatarsCombobox;
        private Label label1;
        private Label label2;
        private Label label3;
        private Button DeleteSelectedGroupButton;
        private Button AddButton;
        private Label NewLabel;
        private Panel AvatarList;
    }
}
