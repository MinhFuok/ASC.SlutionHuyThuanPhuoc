using System.ComponentModel.DataAnnotations;

namespace ASC.WebHuyThuanPhuoc.Areas.Promotions.Models
{
    public class PromotionViewModel
    {
        public string? RowKey { get; set; }

        public bool IsDeleted { get; set; }

        [Required]
        [Display(Name = "Type")]
        public string PartitionKey { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Header")]
        public string Header { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Content")]
        public string Content { get; set; } = string.Empty;
    }
}