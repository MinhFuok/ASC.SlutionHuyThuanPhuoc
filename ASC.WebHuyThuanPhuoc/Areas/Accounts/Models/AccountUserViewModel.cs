namespace ASC.WebHuyThuanPhuoc.Areas.Accounts.Models
{
    public class AccountUserViewModel
    {
        public string Email { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }
}
