namespace Avatar_Explorer.Classes
{
    public class SearchFilter
    {
        public string[] Author { get; set; } = Array.Empty<string>();
        public string[] Title { get; set; } = Array.Empty<string>();
        public string[] BoothId { get; set; } = Array.Empty<string>();
        public string[] Avatar { get; set; } = Array.Empty<string>();
        public string[] Category { get; set; } = Array.Empty<string>();
        public string[] ItemMemo { get; set; } = Array.Empty<string>();
        public string[] SearchWords { get; set; } = Array.Empty<string>();
    }
}
