namespace Avatar_Explorer.Forms
{
    partial class AddMemoForm
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
            AddMemoLabel = new Label();
            MemoTextBox = new TextBox();
            EditButton = new Button();
            SuspendLayout();
            // 
            // AddMemoLabel
            // 
            AddMemoLabel.AutoSize = true;
            AddMemoLabel.Font = new Font(_mainForm.GuiFont, 20F, FontStyle.Bold);
            AddMemoLabel.Location = new Point(2, 6);
            AddMemoLabel.Name = "AddMemoLabel";
            AddMemoLabel.Size = new Size(152, 39);
            AddMemoLabel.TabIndex = 0;
            AddMemoLabel.Text = "メモの追加";
            // 
            // MemoTextBox
            // 
            MemoTextBox.Font = new Font("Yu Gothic UI", 12F);
            MemoTextBox.BorderStyle = BorderStyle.FixedSingle;
            MemoTextBox.Location = new Point(12, 51);
            MemoTextBox.Multiline = true;
            MemoTextBox.Name = "MemoTextBox";
            MemoTextBox.Size = new Size(645, 214);
            MemoTextBox.TabIndex = 1;
            // 
            // EditButton
            // 
            EditButton.Font = new Font("Yu Gothic UI", 12F);
            EditButton.Location = new Point(549, 271);
            EditButton.Name = "EditButton";
            EditButton.Size = new Size(108, 38);
            EditButton.TabIndex = 2;
            EditButton.Text = "完了";
            EditButton.UseVisualStyleBackColor = true;
            EditButton.Click += EditButton_Click;
            // 
            // AddMemo
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(671, 317);
            Controls.Add(EditButton);
            Controls.Add(MemoTextBox);
            Controls.Add(AddMemoLabel);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "AddMemo";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "メモの追加";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label AddMemoLabel;
        private TextBox MemoTextBox;
        private Button EditButton;
    }
}