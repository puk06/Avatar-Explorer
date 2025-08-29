using Avatar_Explorer.Utils;

namespace Avatar_Explorer.Forms
{
    sealed partial class AddItemForm
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
            label2 = new Label();
            label3 = new Label();
            FolderTextBox = new TextBox();
            BoothURLTextBox = new TextBox();
            GetButton = new Button();
            label4 = new Label();
            TypeComboBox = new ComboBox();
            label5 = new Label();
            SelectAvatar = new Button();
            AuthorTextBox = new TextBox();
            TitleTextBox = new TextBox();
            label6 = new Label();
            label7 = new Label();
            label8 = new Label();
            ErrorLabel = new Label();
            AddButton = new Button();
            CustomButton = new Button();
            openFolderButton = new Button();
            label9 = new Label();
            OpenMaterialFolderButton = new Button();
            MaterialTextBox = new TextBox();
            label10 = new Label();
            OtherFolderCount = new Label();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font(_mainForm.GuiFont, 12F);
            label1.Location = new Point(17, 85);
            label1.Name = "label1";
            label1.Size = new Size(74, 23);
            label1.TabIndex = 0;
            label1.Text = "フォルダ";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font(_mainForm.GuiFont, 12F);
            label2.Location = new Point(18, 162);
            label2.Name = "label2";
            label2.Size = new Size(94, 23);
            label2.TabIndex = 1;
            label2.Text = "Booth URL";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font(_mainForm.GuiFont, 20F, FontStyle.Bold);
            label3.Location = new Point(12, 9);
            label3.Name = "label3";
            label3.Size = new Size(203, 39);
            label3.TabIndex = 2;
            label3.Text = "アイテムの追加";
            // 
            // FolderTextBox
            // 
            FolderTextBox.AllowDrop = true;
            FolderTextBox.Font = new Font(_mainForm.GuiFont, 12F);
            FolderTextBox.Location = new Point(121, 82);
            FolderTextBox.Name = "FolderTextBox";
            FolderTextBox.Size = new Size(227, 31);
            FolderTextBox.TabIndex = 3;
            FolderTextBox.TextChanged += CheckText;
            FolderTextBox.DragDrop += FolderTextBox_DragDrop;
            FolderTextBox.DragEnter += FormUtils.DragEnter;
            FolderTextBox.KeyDown += AddItem_KeyDown;
            // 
            // BoothURLTextBox
            // 
            BoothURLTextBox.Font = new Font(_mainForm.GuiFont, 12F);
            BoothURLTextBox.Location = new Point(120, 159);
            BoothURLTextBox.Name = "BoothURLTextBox";
            BoothURLTextBox.Size = new Size(415, 31);
            BoothURLTextBox.TabIndex = 4;
            BoothURLTextBox.PlaceholderText = "https://booth.pm/ja/items/123456";
            BoothURLTextBox.KeyDown += BoothURLTextBox_KeyDown;
            BoothURLTextBox.KeyDown += AddItem_KeyDown;
            // 
            // GetButton
            // 
            GetButton.Location = new Point(408, 385);
            GetButton.Name = "GetButton";
            GetButton.Size = new Size(122, 36);
            GetButton.TabIndex = 5;
            GetButton.Text = "情報を取得";
            GetButton.UseVisualStyleBackColor = true;
            GetButton.Click += GetButton_Click;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font(_mainForm.GuiFont, 12F);
            label4.Location = new Point(17, 332);
            label4.Name = "label4";
            label4.Size = new Size(58, 23);
            label4.TabIndex = 7;
            label4.Text = "タイプ";
            // 
            // TypeComboBox
            // 
            TypeComboBox.DropDownStyle = ComboBoxStyle.DropDown;
            TypeComboBox.FormattingEnabled = true;
            TypeComboBox.Items.AddRange(new object[] { "アバター", "衣装", "テクスチャ", "ギミック", "アクセサリー", "髪型", "アニメーション", "ツール", "シェーダー" });
            TypeComboBox.Location = new Point(128, 335);
            TypeComboBox.Name = "TypeComboBox";
            TypeComboBox.Size = new Size(186, 23);
            TypeComboBox.TabIndex = 8;
            TypeComboBox.TextChanged += TypeComboBox_TextChanged;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font(_mainForm.GuiFont, 12F);
            label5.Location = new Point(16, 378);
            label5.Name = "label5";
            label5.Size = new Size(105, 23);
            label5.TabIndex = 9;
            label5.Text = "対応アバター";
            // 
            // SelectAvatar
            // 
            SelectAvatar.Location = new Point(174, 372);
            SelectAvatar.Name = "SelectAvatar";
            SelectAvatar.Size = new Size(140, 38);
            SelectAvatar.TabIndex = 10;
            SelectAvatar.Text = "選択";
            SelectAvatar.UseVisualStyleBackColor = true;
            SelectAvatar.Click += SelectAvatar_Click;
            // 
            // AuthorTextBox
            // 
            AuthorTextBox.Enabled = false;
            AuthorTextBox.Font = new Font(_mainForm.GuiFont, 10F);
            AuthorTextBox.Location = new Point(128, 294);
            AuthorTextBox.Name = "AuthorTextBox";
            AuthorTextBox.Size = new Size(407, 27);
            AuthorTextBox.TabIndex = 14;
            AuthorTextBox.PlaceholderText = "作者未取得";
            AuthorTextBox.TextChanged += CheckText;
            AuthorTextBox.KeyDown += AddItem_KeyDown;
            // 
            // TitleTextBox
            // 
            TitleTextBox.Enabled = false;
            TitleTextBox.Font = new Font(_mainForm.GuiFont, 10F);
            TitleTextBox.Location = new Point(128, 258);
            TitleTextBox.Name = "TitleTextBox";
            TitleTextBox.Size = new Size(407, 27);
            TitleTextBox.TabIndex = 13;
            TitleTextBox.PlaceholderText = "タイトル未取得";
            TitleTextBox.TextChanged += CheckText;
            TitleTextBox.KeyDown += AddItem_KeyDown;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font(_mainForm.GuiFont, 12F);
            label6.Location = new Point(17, 294);
            label6.Name = "label6";
            label6.Size = new Size(42, 23);
            label6.TabIndex = 12;
            label6.Text = "作者";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Font = new Font(_mainForm.GuiFont, 12F);
            label7.Location = new Point(17, 258);
            label7.Name = "label7";
            label7.Size = new Size(74, 23);
            label7.TabIndex = 11;
            label7.Text = "タイトル";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Font = new Font(_mainForm.GuiFont, 18F, FontStyle.Bold);
            label8.Location = new Point(12, 213);
            label8.Name = "label8";
            label8.Size = new Size(232, 35);
            label8.TabIndex = 15;
            label8.Text = "Booth アイテム情報";
            // 
            // ErrorLabel
            // 
            ErrorLabel.AutoSize = true;
            ErrorLabel.Font = new Font(_mainForm.GuiFont, 11F);
            ErrorLabel.ForeColor = Color.Red;
            ErrorLabel.Location = new Point(17, 435);
            ErrorLabel.Name = "ErrorLabel";
            ErrorLabel.Size = new Size(0, 22);
            ErrorLabel.TabIndex = 16;
            ErrorLabel.TextAlign = ContentAlignment.BottomLeft;
            // 
            // AddButton
            // 
            AddButton.Enabled = false;
            AddButton.Location = new Point(408, 430);
            AddButton.Name = "AddButton";
            AddButton.Size = new Size(122, 36);
            AddButton.TabIndex = 17;
            AddButton.Text = "追加";
            AddButton.UseVisualStyleBackColor = true;
            AddButton.Click += AddButton_Click;
            // 
            // CustomButton
            // 
            CustomButton.Location = new Point(408, 343);
            CustomButton.Name = "CustomButton";
            CustomButton.Size = new Size(122, 36);
            CustomButton.TabIndex = 18;
            CustomButton.Text = "カスタムで追加";
            CustomButton.UseVisualStyleBackColor = true;
            CustomButton.Click += CustomButton_Click;
            // 
            // openFolderButton
            // 
            openFolderButton.Location = new Point(453, 82);
            openFolderButton.Name = "openFolderButton";
            openFolderButton.Size = new Size(85, 31);
            openFolderButton.TabIndex = 19;
            openFolderButton.Text = "フォルダを選択";
            openFolderButton.UseVisualStyleBackColor = true;
            openFolderButton.Click += OpenFolderButton_Click;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Font = new Font(_mainForm.GuiFont, 9.3F);
            label9.Location = new Point(19, 52);
            label9.Name = "label9";
            label9.Size = new Size(464, 19);
            label9.TabIndex = 20;
            label9.Text = "Tip: フォルダを下の欄にドラッグアンドドロップすることでも入力できます！";
            // 
            // OpenMaterialFolderButton
            // 
            OpenMaterialFolderButton.Location = new Point(452, 119);
            OpenMaterialFolderButton.Name = "openMaterialFolderButton";
            OpenMaterialFolderButton.Size = new Size(85, 31);
            OpenMaterialFolderButton.TabIndex = 23;
            OpenMaterialFolderButton.Text = "フォルダを選択";
            OpenMaterialFolderButton.UseVisualStyleBackColor = true;
            OpenMaterialFolderButton.Click += OpenMaterialFolderButton_Click;
            // 
            // MaterialTextBox
            // 
            MaterialTextBox.AllowDrop = true;
            MaterialTextBox.Font = new Font(_mainForm.GuiFont, 12F);
            MaterialTextBox.Location = new Point(120, 119);
            MaterialTextBox.Name = "MaterialTextBox";
            MaterialTextBox.Size = new Size(326, 31);
            MaterialTextBox.TabIndex = 22;
            MaterialTextBox.TextChanged += CheckText;
            MaterialTextBox.DragDrop += MaterialTextBox_DragDrop;
            MaterialTextBox.DragEnter += FormUtils.DragEnter;
            MaterialTextBox.KeyDown += AddItem_KeyDown;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Font = new Font(_mainForm.GuiFont, 12F);
            label10.Location = new Point(17, 122);
            label10.Name = "label10";
            label10.Size = new Size(90, 23);
            label10.TabIndex = 21;
            label10.Text = "マテリアル";
            // 
            // OtherFolderCount
            // 
            OtherFolderCount.AutoSize = true;
            OtherFolderCount.Font = new Font(_mainForm.GuiFont, 12F);
            OtherFolderCount.Location = new Point(350, 85);
            OtherFolderCount.Name = "otherFolderCount";
            OtherFolderCount.Size = new Size(0, 23);
            OtherFolderCount.TabIndex = 24;
            // 
            // AddItem
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(550, 490);
            Controls.Add(OtherFolderCount);
            Controls.Add(OpenMaterialFolderButton);
            Controls.Add(MaterialTextBox);
            Controls.Add(label10);
            Controls.Add(label9);
            Controls.Add(openFolderButton);
            Controls.Add(CustomButton);
            Controls.Add(AddButton);
            Controls.Add(ErrorLabel);
            Controls.Add(label8);
            Controls.Add(AuthorTextBox);
            Controls.Add(TitleTextBox);
            Controls.Add(label6);
            Controls.Add(label7);
            Controls.Add(SelectAvatar);
            Controls.Add(label5);
            Controls.Add(TypeComboBox);
            Controls.Add(label4);
            Controls.Add(GetButton);
            Controls.Add(BoothURLTextBox);
            Controls.Add(FolderTextBox);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "AddItem";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "AddItem";
            KeyDown += AddItem_KeyDown;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Label label2;
        private Label label3;
        private TextBox FolderTextBox;
        private TextBox BoothURLTextBox;
        private Button GetButton;
        private Label label4;
        private ComboBox TypeComboBox;
        private Label label5;
        private Button SelectAvatar;
        private TextBox AuthorTextBox;
        private TextBox TitleTextBox;
        private Label label6;
        private Label label7;
        private Label label8;
        private Label ErrorLabel;
        private Button AddButton;
        private Button CustomButton;
        private Button openFolderButton;
        private Label label9;
        private Button OpenMaterialFolderButton;
        private TextBox MaterialTextBox;
        private Label label10;
        private Label OtherFolderCount;
    }
}