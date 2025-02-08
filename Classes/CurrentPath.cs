namespace Avatar_Explorer.Classes
{
    public class CurrentPath
    {
        public string? CurrentSelectedAvatar;
        public string? CurrentSelectedAvatarPath;
        public Author? CurrentSelectedAuthor;
        public ItemType CurrentSelectedCategory = ItemType.Unknown;
        public string CurrentSelectedCustomCategory = "";
        public Item? CurrentSelectedItem;
        public string? CurrentSelectedItemCategory;
        public ItemFolderInfo CurrentSelectedItemFolderInfo = new();

        public bool IsEmpty()
        {
            return CurrentSelectedAvatar == null &&
                   CurrentSelectedAvatarPath == null &&
                   CurrentSelectedAuthor == null &&
                   CurrentSelectedCategory == ItemType.Unknown &&
                   CurrentSelectedItem == null &&
                   CurrentSelectedItemCategory == null;
        }
    }
}
