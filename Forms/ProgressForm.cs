namespace Avatar_Explorer.Forms;

internal sealed class ProgressForm : Form
{
    private readonly ProgressBar _progressBar;
    private readonly Label _progressLabel;
    private readonly string _formTitle;

    internal ProgressForm(string progressFormTitle)
    {
        _formTitle = progressFormTitle;

        Text = _formTitle;
        Size = new Size(400, 90);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterScreen;
        MaximizeBox = false;
        MinimizeBox = false;

        _progressBar = new ProgressBar
        {
            Dock = DockStyle.Top,
            Style = ProgressBarStyle.Continuous,
            Minimum = 0,
            Maximum = 100
        };

        _progressLabel = new Label
        {
            Dock = DockStyle.Top,
            TextAlign = ContentAlignment.MiddleCenter,
            Text = "0%",
            AutoSize = false,
            Height = 20
        };

        Controls.Add(_progressBar);
        Controls.Add(_progressLabel);
    }

    internal void UpdateProgress(int percentage, string message = "")
    {
        if (InvokeRequired)
        {
            Invoke(() => UpdateProgress(percentage, message));
            return;
        }

        _progressBar.Value = percentage;
        _progressLabel.Text = $"{percentage}% {message}";
        Text = $"{_formTitle} - {percentage}%";
    }
}
