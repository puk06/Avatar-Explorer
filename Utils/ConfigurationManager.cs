using System.Text;

namespace Avatar_Explorer.Utils;

internal class ConfigurationManager
{
    private static readonly Dictionary<string, string> _data = new();

    private readonly Dictionary<string, string> _defaultKeys = new()
    {
        { "ItemsPerPage", "30" },
        { "PreviewScale", "1" },
        { "DefaultLanguage", "1" },
        { "DefaultSortOrder", "1" },
        { "ThumbnailUpdateTimeout", "200" },
        { "BackupInterval", "5" },
        { "RemoveBrackets", "false" },
        { "ButtonSize", "64" },
        { "DarkMode", "false" }
    };
    private readonly Dictionary<string, string> _defaultKeysDescriptions = new()
    {
        { "ItemsPerPage", "# 1ページあたりに表示するアイテムの量を指定できます。デフォルトは30です。範囲: 1 - 1000" },
        { "PreviewScale", "# アイテムのサムネイルプレビューサイズを何倍にするかを決めることが出来ます。範囲: 0.1 - 10" },
        { "DefaultLanguage", "# デフォルトの言語を指定できます。1: 日本語, 2: 한국어, 3: English" },
        { "DefaultSortOrder", "# デフォルトの並び替え順を変更できます。1: タイトル, 2: 作者, 3: 作成日時, 4: 更新日時, 5: 実装済み, 6: 未実装" },
        { "ThumbnailUpdateTimeout", "# スクロール終了後、何ms後にサムネイルを描画し直すかを変えることが出来ます。デフォルトは200msです。範囲: 1 - 10000" },
        { "BackupInterval", "# 自動バックアップの間隔を変更することが出来ます。単位は分で、デフォルトは5分です。範囲: 1 - 1000" },
        { "RemoveBrackets", "# 商品タイトルに含まれる【】のような括弧を非表示にします。表示上のみ影響され、データベースに影響はありません。trueで括弧の非表示、falseで表示です。" },
        { "ButtonSize", "# ボタンのサイズ(高さ)を変更できます。変更されるのはボタンの画像サイズです。デフォルトは64です。範囲: 1 - 500" },
        { "DarkMode", "# ソフトのダークモードを有効にします。trueで有効、falseで無効です。" }
    };
    private const string _configulationDescription = "# このファイルはAvatar Explorerの起動時に読み込まれる設定ファイルです。書き換えた際はAvatar Explorerを再起動してください。";

    internal string? this[string key]
    {
        get => _data.TryGetValue(key, out var value) ? value : null;
        set
        {
            if (value != null)
                _data[key] = value;
        }
    }

    internal void Load(string path)
    {
        _data.Clear();

        if (File.Exists(path))
        {
            foreach (var line in File.ReadLines(path))
            {
                var trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith('#') || !trimmed.Contains('=')) continue;

                var parts = trimmed.Split('=', 2);
                if (parts.Length == 2)
                {
                    string key = parts[0].Trim();
                    string value = parts[1].Trim();
                    _data[key] = value;
                }
            }
        }

        bool updated = false;
        foreach (var pair in _defaultKeys.Where(pair => !_data.ContainsKey(pair.Key)))
        {
            _data[pair.Key] = pair.Value;
            updated = true;
        }

        if (!File.Exists(path) || updated)
        {
            Save(path);
        }
    }

    private void Save(string path)
    {
        try
        {
            using StreamWriter writer = new(path, false, Encoding.UTF8);

            writer.WriteLine(_configulationDescription);
            writer.WriteLine();

            foreach (var kvp in _data)
            {
                if (_defaultKeysDescriptions.TryGetValue(kvp.Key, out var description))
                {
                    writer.WriteLine(description);
                }

                writer.WriteLine($"{kvp.Key} = {kvp.Value}");
                writer.WriteLine();
            }
        }
        catch (Exception ex)
        {
            FormUtils.ShowMessageBox("設定ファイルの保存に失敗しました。詳細はErrorLog.txtをご覧ください。", "エラー", true);
            LogUtils.ErrorLogger("設定ファイルの保存に失敗しました", ex);
        }
    }
}
