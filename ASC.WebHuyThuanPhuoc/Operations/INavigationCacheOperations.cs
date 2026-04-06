using ASC.WebHuyThuanPhuoc.Models;

namespace ASC.WebHuyThuanPhuoc.Operations
{
    public interface INavigationCacheOperations
    {
        Task SetMenuItemsToCacheAsync();
        NavigationModel GetMenuItemsFromCache();
    }
}