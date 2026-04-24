using ASC.WebHuyThuanPhuoc.Models;

namespace ASC.WebHuyThuanPhuoc.Data
{
    public interface INavigationCacheOperations
    {
        Task SetMenuItemsToCacheAsync();
        NavigationModel GetMenuItemsFromCache();
    }
}
