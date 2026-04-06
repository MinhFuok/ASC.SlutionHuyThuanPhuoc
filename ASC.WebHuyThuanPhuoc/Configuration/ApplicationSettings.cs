namespace ASC.WebHuyThuanPhuoc.Configuration
{
    public class ApplicationSettings
    {
        public string ApplicationTitle { get; set; } = string.Empty;

        public string AdminEmail { get; set; } = string.Empty;
        public string AdminName { get; set; } = string.Empty;
        public string AdminPassword { get; set; } = string.Empty;

        public string Roles { get; set; } = string.Empty;

        public string EngineerEmail { get; set; } = string.Empty;
        public string EngineerName { get; set; } = string.Empty;
        public string EngineerPassword { get; set; } = string.Empty;

        public string SMTPServer { get; set; } = string.Empty;
        public int SMTPPort { get; set; }
        public string SMTPAccount { get; set; } = string.Empty;
        public string SMTPPassword { get; set; } = string.Empty;
    }
}