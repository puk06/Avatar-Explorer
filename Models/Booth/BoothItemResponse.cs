namespace Avatar_Explorer.Models.Booth;

public class BoothItemResponse
{
    public string Name { get; set; } = string.Empty;
    public ShopInfo Shop { get; set; } = new ShopInfo();
    public List<ImageInfo> Images { get; set; } = new List<ImageInfo>();
    public CategoryInfo Category { get; set; } = new CategoryInfo();
}
