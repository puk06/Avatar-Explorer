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
    };
    private readonly Dictionary<string, string> _defaultKeysDescriptions = new()
    {
        { "ItemsPerPage", "# 1ページあたりに表示するアイテムの量を指定できます。デフォルトは30です。" },
        { "PreviewScale", "# アイテムのサムネイルプレビューサイズを何倍にするかを決めることが出来ます。" },
        { "DefaultLanguage", "# デフォルトの言語を指定できます。1: 日本語, 2: 한국어, 3: English" },
        { "DefaultSortOrder", "# デフォルトの並び替え順を変更できます。1: タイトル, 2: 作者, 3: 作成日時, 4: 更新日時, 5: 実装済み, 6: 未実装" }
    };

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
        foreach (var pair in _defaultKeys)
        {
            if (!_data.ContainsKey(pair.Key))
            {
                _data[pair.Key] = pair.Value;
                updated = true;
            }
        }

        if (!File.Exists(path) || updated)
        {
            Save(path);
        }
    }

    private void Save(string path)
    {
        using StreamWriter writer = new(path);
        foreach (var kvp in _data)
        {
            writer.WriteLine(_defaultKeysDescriptions[kvp.Key]);
            writer.WriteLine($"{kvp.Key}={kvp.Value}");
            writer.WriteLine();
        }
    }
}
