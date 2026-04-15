using System.ComponentModel.DataAnnotations;

namespace ASC.WebHuyThuanPhuoc.Areas.Accounts.Models
{
    public class ProfileModel
    {
        [Required]
        [Display(Name = "User Name")]
        public string UserName { get; set; } = string.Empty;
    }
}
