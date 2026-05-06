using System.ComponentModel.DataAnnotations;

namespace ASC.WebHuyThuanPhuoc.Areas.ServiceRequests.Models
{
    public class UpdateServiceRequestViewModel : NewServiceRequestViewModel
    {
        public string RowKey { get; set; } = string.Empty;

        public string PartitionKey { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Service Engineer")]
        public string ServiceEngineer { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Status")]
        public string Status { get; set; } = string.Empty;
    }
}