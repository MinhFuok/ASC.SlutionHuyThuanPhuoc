using ASC.WebHuyThuanPhuoc.Models;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace ASC.WebHuyThuanPhuoc.Operations
{
    public class NavigationCacheOperations : INavigationCacheOperations
    {
        private const string NavigationCacheKey = "NAVIGATION_CACHE_KEY";
        private readonly IWebHostEnvironment _environment;
        private readonly IMemoryCache _memoryCache;

        public NavigationCacheOperations(
            IWebHostEnvironment environment,
            IMemoryCache memoryCache)
        {
            _environment = environment;
            _memoryCache = memoryCache;
        }

        public async Task SetMenuItemsToCacheAsync()
        {
            var filePath = Path.Combine(_environment.ContentRootPath, "Navigation.json");

            if (!File.Exists(filePath))
            {
                _memoryCache.Set(NavigationCacheKey, new NavigationModel());
                return;
            }

            var json = await File.ReadAllTextAsync(filePath);
            var navigationModel = JsonConvert.DeserializeObject<NavigationModel>(json) ?? new NavigationModel();

            _memoryCache.Set(NavigationCacheKey, navigationModel);
        }

        public NavigationModel GetMenuItemsFromCache()
        {
            if (_memoryCache.TryGetValue(NavigationCacheKey, out NavigationModel? navigationModel) &&
                navigationModel != null)
            {
                return navigationModel;
            }

            return new NavigationModel();
        }
    }
}