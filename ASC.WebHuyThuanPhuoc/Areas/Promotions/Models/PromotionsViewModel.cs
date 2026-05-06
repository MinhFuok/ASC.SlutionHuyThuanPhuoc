namespace ASC.WebHuyThuanPhuoc.Areas.Promotions.Models
{
    public class PromotionsViewModel
    {
        public List<PromotionViewModel> Promotions { get; set; } = new();

        public PromotionViewModel PromotionInContext { get; set; } = new();

        public bool IsEdit { get; set; }
    }
}