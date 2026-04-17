using AutoMapper;
using ASC.Model.Models;

namespace ASC.WebHuyThuanPhuoc.Areas.Configuration.Models
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<MasterDataKey, MasterDataKeyViewModel>().ReverseMap();
            CreateMap<MasterDataValue, MasterDataValueViewModel>().ReverseMap();
        }
    }
}
