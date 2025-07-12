using System.Text.Json.Serialization;

namespace Avatar_Explorer.Models.Booth;

public class ShopInfo
{
    public string Name { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("thumbnail_url")]
    public string ThumbnailUrl { get; set; } = string.Empty;
}
