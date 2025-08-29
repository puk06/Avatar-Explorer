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
            AvatarList = new Panel();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font(_mainForm.GuiFont, 12F, FontStyle.Bold);
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(424, 48);
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
            // AvatarList
            // 
            AvatarList.BackColor = Color.FromArgb(249, 249, 249);
            AvatarList.Location = new Point(16, 89);
            AvatarList.Name = "AvatarList";
            AvatarList.Size = new Size(1011, 467);
            AvatarList.TabIndex = 4;
            // 
            // SelectSupportedAvatarForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1043, 615);
            Controls.Add(AvatarList);
            Controls.Add(ConfirmButton);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "SelectSupportedAvatarForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "SelectSupportedAvatar";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Label label1;
        private Button ConfirmButton;
        private Panel AvatarList;
    }
}