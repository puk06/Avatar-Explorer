﻿namespace Avatar_Explorer.Classes
{
    public class SearchFilter
    {
        public string[] Author { get; set; } = Array.Empty<string>();
        public string[] Title { get; set; } = Array.Empty<string>();
        public string[] BoothId { get; set; } = Array.Empty<string>();
        public string[] SearchWords { get; set; } = Array.Empty<string>();
    }
}
