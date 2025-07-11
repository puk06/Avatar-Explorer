using Avatar_Explorer.Models;
using Avatar_Explorer.Utils;

namespace Avatar_Explorer.Forms
{
    sealed partial class SelectSupportedAvatarForm
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
            label1 = new Label();
            ConfirmButton = new Button();
            tabControl1 = new TabControl();
            AvatarList = new TabPage();
            tabControl1.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font(_mainForm.GuiFont, 12F, FontStyle.Bold);
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(359, 42);
            label1.TabIndex = 1;
            label1.Text = "対応アバターを選択してください。\r\n選択されてなければ、全アバター対応として扱われます！\r\n";
            // 
            // ConfirmButton
            // 
            ConfirmButton.Location = new Point(900, 562);
            ConfirmButton.Name = "ConfirmButton";
            ConfirmButton.Size = new Size(131, 41);
            ConfirmButton.TabIndex = 2;
            ConfirmButton.Text = "確定";
            ConfirmButton.UseVisualStyleBackColor = true;
            ConfirmButton.Click += ConfirmButton_Click;
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(AvatarList);
            tabControl1.Location = new Point(12, 65);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(1019, 495);
            tabControl1.TabIndex = 3;
            // 
            // AvatarList
            // 
            AvatarList.AutoScroll = true;
            AvatarList.Location = new Point(4, 24);
            AvatarList.Name = "AvatarList";
            AvatarList.Padding = new Padding(3);
            AvatarList.Size = new Size(1011, 474);
            AvatarList.TabIndex = 0;
            AvatarList.Text = "アバターリスト";
            AvatarList.UseVisualStyleBackColor = true;
            AvatarList.MouseWheel += AEUtils.OnScroll;
            AvatarList.Scroll += AEUtils.OnScroll;
            // 
            // SelectSupportedAvatar
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1043, 615);
            Controls.Add(tabControl1);
            Controls.Add(ConfirmButton);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "SelectSupportedAvatar";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "SelectSupportedAvatar";
            tabControl1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Label label1;
        private Button ConfirmButton;
        private TabControl tabControl1;
        private TabPage AvatarList;
    }
}