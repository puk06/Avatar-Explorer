using Avatar_Explorer.Utils;

namespace Avatar_Explorer.Models;

internal class CustomItemButton : Button
{
    private readonly int _buttonHeight = 64;
    private readonly PictureBox _pictureBox;
    private readonly Label _title;
    private readonly Label _authorName;
    private readonly ToolTip _toolTip;
    private string _toolTipText;
    private Form? _previewForm;
    private PictureBox? _previewPictureBox;
    private Image? _loadedPicture;

    internal float PreviewScale { get; set; } = 1.0f;

    internal string? ImagePath { get; set; }

    internal string TitleText
    {
        get => _title.Text;
        set => _title.Text = value;
    }

    internal string AuthorName
    {
        get => _authorName.Text;
        set => _authorName.Text = value;
    }

    internal string ToolTipText
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

    internal CustomItemButton(int buttonWidth, int buttonHeight, bool darkMode)
    {
        _buttonHeight = buttonHeight;

        if (darkMode) DarkModeUtils.SetDarkMode(this);

        UseVisualStyleBackColor = true;
        Size = new Size(buttonWidth, buttonHeight);
        SizeChanged += (_, _) =>
        {
            if (_title == null || _authorName == null) return;
            _title.Size = new Size(Size.Width - buttonHeight - 4 - 5, 24);
            _authorName.Size = new Size(Size.Width - 60 - 5, 40);
        };

        _pictureBox = new PictureBox
        {
            Location = new Point(4, 4),
            Size = new Size(buttonHeight - 8, buttonHeight - 8),
            SizeMode = PictureBoxSizeMode.StretchImage,
            BackColor = Color.Transparent
        };
        Controls.Add(_pictureBox);

        // タイトルが長すぎる場合にボタンの幅を超えないようにする
        // ボタンの幅 - ラベルのX位置 - 余裕を持たせて数px引く
        var labelWidth = buttonWidth - buttonHeight - 4 - 5;

        _title = new Label
        {
            Location = new Point(buttonHeight - 4, 3),
            Size = new Size(labelWidth, 24),
            Font = new Font("Yu Gothic UI", 12F),
            BackColor = Color.Transparent
        };
        Controls.Add(_title);

        _authorName = new Label
        {
            Location = new Point(buttonHeight - 4, 25),
            Size = new Size(labelWidth, 40),
            BackColor = Color.Transparent
        };
        Controls.Add(_authorName);

        _toolTipText = string.Empty;
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

    internal void CheckThmbnail(Point location, Size size, Rectangle scrollArea)
    {
        if ((location.Y >= scrollArea.Y && location.Y <= scrollArea.Y + scrollArea.Height) || (location.Y + size.Height >= scrollArea.Y && location.Y + size.Height <= scrollArea.Y + scrollArea.Height))
        {
            if (_pictureBox.Image != null) return;

            if (ImagePath == null)
            {
                _pictureBox.Image = SharedImages.GetImage(SharedImages.Images.FolderIcon);
            }
            else
            {
                if (_loadedPicture == null && File.Exists(ImagePath))
                {
                    try
                    {
                        var image = Image.FromFile(ImagePath);
                        _loadedPicture = new Bitmap(image, new Size(_buttonHeight - 8, _buttonHeight - 8));
                        image.Dispose();
                        image = null;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to load image: {ex.Message}");
                    }
                }

                _pictureBox.Image = _loadedPicture ?? SharedImages.GetImage(SharedImages.Images.FileIcon);
            }
        }
        else if (_pictureBox.Image != null)
        {
            _pictureBox.Image = null;
        }
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
                Size = new Size((int)(200 * PreviewScale), (int)(200 * PreviewScale)),
                BackColor = Color.Black,
                ShowInTaskbar = false,
                TopMost = true
            };

            _previewPictureBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.StretchImage
            };

            var image = Image.FromFile(ImagePath);
            var imageHeight = image.Height;
            var imageWidth = image.Width;

            if (imageHeight > 0 && imageWidth > 0)
            {
                if (imageWidth > imageHeight)
                {
                    var aspectRatio = (double)imageWidth / imageHeight;
                    _previewForm.Width = (int)(_previewForm.Height * aspectRatio);
                }
                else
                {
                    var aspectRatio = (double)imageHeight / imageWidth;
                    _previewForm.Height = (int)(_previewForm.Width * aspectRatio);
                }
            }

            _previewPictureBox.Image = new Bitmap(image, new Size(_previewForm.Width, _previewForm.Height));
            image.Dispose();
            image = null;

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

            foreach (Control control in Controls)
            {
                control.MouseEnter -= (_, e) => OnMouseEnter(e);
                control.MouseLeave -= (_, e) => OnMouseLeave(e);
                control.MouseMove -= (_, e) => OnMouseMove(e);
                control.MouseDown -= (_, e) => OnMouseDown(e);
                control.MouseClick -= (_, e) => OnMouseClick(e);
            }

            _pictureBox.Dispose();
            _title.Dispose();
            _authorName.Dispose();
            _toolTip.Dispose();
            _previewForm?.Dispose();
            _loadedPicture?.Dispose();
        }

        base.Dispose(disposing);
    }
}
