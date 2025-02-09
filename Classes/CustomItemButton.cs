namespace Avatar_Explorer.Classes
{
    internal class CustomItemButton : Button
    {
        private readonly PictureBox _pictureBox;
        private readonly Label _title;
        private readonly Label _authorName;
        private readonly ToolTip _toolTip;
        private string _toolTipText;
        private Form? _previewForm;
        private PictureBox? _previewPictureBox;

        public Image Picture
        {
            get => _pictureBox.Image;
            set => _pictureBox.Image = value;
        }

        public string? ImagePath { get; set; }

        public string TitleText
        {
            get => _title.Text;
            set => _title.Text = value;
        }

        public string AuthorName
        {
            get => _authorName.Text;
            set => _authorName.Text = value;
        }

        public string ToolTipText
        {
            get => _toolTipText;
            set
            {
                _toolTipText = value;
                _toolTip.SetToolTip(this, _toolTipText);
                foreach (Control control in Controls)
                {
                    _toolTip.SetToolTip(control, _toolTipText);
                }
            }
        }

        public CustomItemButton(int buttonWidth)
        {
            Size = new Size(buttonWidth, 64);
            SizeChanged += (_, _) =>
            {
                if (_title == null || _authorName == null) return;
                _title.Size = new Size(Size.Width - 60 - 5, 24);
                _authorName.Size = new Size(Size.Width - 60 - 5, 40);
            };

            _pictureBox = new PictureBox
            {
                Location = new Point(4, 4),
                Size = new Size(56, 56),
                SizeMode = PictureBoxSizeMode.StretchImage
            };
            Controls.Add(_pictureBox);

            // タイトルが長すぎる場合にボタンの幅を超えないようにする
            // ボタンの幅 - ラベルのX位置 - 余裕を持たせて数px引く
            var labelWidth = buttonWidth - 60 - 5;

            _title = new Label
            {
                Location = new Point(60, 3),
                Size = new Size(labelWidth, 24),
                Font = new Font("Yu Gothic UI", 12F)
            };
            Controls.Add(_title);

            _authorName = new Label
            {
                Location = new Point(60, 25),
                Size = new Size(labelWidth, 40)
            };
            Controls.Add(_authorName);

            _toolTipText = "";
            _toolTip = new ToolTip();

            // 画像とラベルのイベントが発生した際にボタンのイベントを呼び出す
            foreach (Control control in Controls)
            {
                control.MouseEnter += (_, e) => OnMouseEnter(e);
                control.MouseLeave += (_, e) => OnMouseLeave(e);
                control.MouseMove += (_, e) => OnMouseMove(e);
                control.MouseDown += (_, e) => OnMouseDown(e);
                control.MouseClick += (_, e) => OnMouseClick(e);
            }

            _pictureBox.MouseEnter += PictureBox_MouseEnter;
            _pictureBox.MouseLeave += PictureBox_MouseLeave;
        }

        private void PictureBox_MouseEnter(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(ImagePath) || Path.GetInvalidPathChars().Any(c => ImagePath.Contains(c)) || !File.Exists(ImagePath)) return;

            try
            {
                _previewForm = new Form
                {
                    FormBorderStyle = FormBorderStyle.None,
                    StartPosition = FormStartPosition.Manual,
                    Size = new Size(200, 200),
                    BackColor = Color.Black,
                    ShowInTaskbar = false,
                    TopMost = true
                };

                _previewPictureBox = new PictureBox
                {
                    Dock = DockStyle.Fill,
                    Image = Image.FromFile(ImagePath),
                    SizeMode = PictureBoxSizeMode.StretchImage
                };

                if (_previewPictureBox.Image is { Width: > 0, Height: > 0 })
                {
                    if (_previewPictureBox.Image.Width > _previewPictureBox.Image.Height)
                    {
                        var aspectRatio = (double)_previewPictureBox.Image.Width / _previewPictureBox.Image.Height;
                        _previewForm.Width = (int)(_previewForm.Height * aspectRatio);
                    }
                    else
                    {
                        var aspectRatio = (double)_previewPictureBox.Image.Height / _previewPictureBox.Image.Width;
                        _previewForm.Height = (int)(_previewForm.Width * aspectRatio);
                    }
                }

                _previewForm.Controls.Add(_previewPictureBox);

                var cursorPosition = Cursor.Position;
                var screenBounds = Screen.FromPoint(cursorPosition).WorkingArea;

                int formX = Math.Max(0, Math.Min(cursorPosition.X + 10, screenBounds.Right - _previewForm.Width));
                int formY = Math.Max(0, Math.Min(cursorPosition.Y + 10, screenBounds.Bottom - _previewForm.Height));

                _previewForm.Location = new Point(formX, formY);
                _previewForm.Show();
            }
            catch (Exception ex) when (ex is FileNotFoundException or OutOfMemoryException)
            {
                Console.WriteLine($"Failed to load image: {ex.Message}");
            }
        }

        private void PictureBox_MouseLeave(object? sender, EventArgs e)
        {
            if (_previewForm == null) return;

            _previewForm.Close();
            _previewForm.Dispose();
            _previewForm = null;

            // 画像のメモリ開放
            if (_previewPictureBox?.Image != null && !SharedImages.IsSharedImage(_previewPictureBox.Image))
            {
                _previewPictureBox.Image.Dispose();
            }

            _previewPictureBox?.Dispose();
            _previewPictureBox = null;
        }

        protected override void OnClick(EventArgs e)
        {
            // ここではクリックされたボタンの種類を取得できないため、何もしない（OnMouseClickで行う）
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            // 左クリックされた場合
            if (e.Button == MouseButtons.Left)
                ProcessClick();
            base.OnMouseClick(e);
        }

        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            // エンターキーまたはスペースキーを押下された場合
            if (e.KeyCode is Keys.Enter or Keys.Space)
                ProcessClick();
            base.OnPreviewKeyDown(e);
        }

        private void ProcessClick()
        {
            // 画像・ラベルをクリックされた場合に備えてフォーカスをボタンに移動させる
            Focus();
            // button.Click += で追加されたイベントを実行する
            base.OnClick(EventArgs.Empty);
        }

        protected override void Dispose(bool disposing)
        {
            // リソースの解放
            if (disposing)
            {
                if (_pictureBox.Image != null && !SharedImages.IsSharedImage(_pictureBox.Image))
                {
                    _pictureBox.Image.Dispose();
                }
                _pictureBox.Image = null;

                _pictureBox.Dispose();
                _title.Dispose();
                _authorName.Dispose();
                _toolTip.Dispose();
                _previewForm?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}