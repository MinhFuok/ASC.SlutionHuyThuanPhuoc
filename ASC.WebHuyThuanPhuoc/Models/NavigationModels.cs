namespace ASC.WebHuyThuanPhuoc.Models
{
    public class NavigationModel
    {
        public List<NavigationItem> MenuItems { get; set; } = new();
    }

    public class NavigationItem
    {
        public string DisplayName { get; set; } = string.Empty;
        public string MaterialIcon { get; set; } = string.Empty;
        public string Link { get; set; } = string.Empty;
        public bool IsNested { get; set; }
        public int Sequence { get; set; }
        public List<string> UserRoles { get; set; } = new();
        public List<NavigationItem> NestedItems { get; set; } = new();
    }
}