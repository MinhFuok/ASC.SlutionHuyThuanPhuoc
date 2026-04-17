using Microsoft.AspNetCore.Mvc.Rendering;

namespace ASC.WebHuyThuanPhuoc.Areas.Configuration.Models
{
    public class MasterValuesViewModel
    {
        public List<MasterDataValueViewModel> MasterValues { get; set; } = new();

        public MasterDataValueViewModel MasterValueInContext { get; set; } = new();

        public List<SelectListItem> MasterKeys { get; set; } = new();

        public string? OriginalPartitionKey { get; set; }

        public string? OriginalRowKey { get; set; }

        public bool IsEdit { get; set; }
    }
}
