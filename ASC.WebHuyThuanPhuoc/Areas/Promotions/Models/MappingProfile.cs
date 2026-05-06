using ASC.Model.Models;
using AutoMapper;

namespace ASC.WebHuyThuanPhuoc.Areas.Promotions.Models
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Promotion, PromotionViewModel>();
            CreateMap<PromotionViewModel, Promotion>();
        }
    }
}