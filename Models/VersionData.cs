namespace Avatar_Explorer.Models;

public class VersionData
{
    public string LatestVersion { get; set; } = string.Empty;
    public string[] ChangeLog { get; set; } = Array.Empty<string>();
}
