namespace ASC.WebHuyThuanPhuoc.Areas.Accounts.Models
{
    public class CustomerViewModel
    {
        public List<AccountUserViewModel> Customers { get; set; } = new();

        public CustomerRegistrationViewModel Registration { get; set; } = new();
    }
}
