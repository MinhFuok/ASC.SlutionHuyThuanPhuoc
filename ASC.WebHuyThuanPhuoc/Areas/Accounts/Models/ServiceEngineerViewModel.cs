namespace ASC.WebHuyThuanPhuoc.Areas.Accounts.Models
{
    public class ServiceEngineerViewModel
    {
        public List<AccountUserViewModel> ServiceEngineers { get; set; } = new();

        public ServiceEngineerRegistrationViewModel Registration { get; set; } = new();
    }
}
