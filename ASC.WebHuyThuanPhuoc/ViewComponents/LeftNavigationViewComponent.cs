using ASC.WebHuyThuanPhuoc.Models;
using ASC.WebHuyThuanPhuoc.Operations;
using Microsoft.AspNetCore.Mvc;

namespace ASC.WebHuyThuanPhuoc.ViewComponents
{
    public class LeftNavigationViewComponent : ViewComponent
    {
        private readonly INavigationCacheOperations _navigationCacheOperations;

        public LeftNavigationViewComponent(INavigationCacheOperations navigationCacheOperations)
        {
            _navigationCacheOperations = navigationCacheOperations;
        }

        public IViewComponentResult Invoke()
        {
            var navigationModel = _navigationCacheOperations.GetMenuItemsFromCache();
            var userRoles = UserClaimsPrincipal.Claims
                .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            var filteredMenu = new NavigationModel
            {
                MenuItems = navigationModel.MenuItems
                    .Where(m => m.UserRoles.Any(r => userRoles.Contains(r)))
                    .OrderBy(m => m.Sequence)
                    .Select(m => new NavigationItem
                    {
                        DisplayName = m.DisplayName,
                        MaterialIcon = m.MaterialIcon,
                        Link = m.Link,
                        IsNested = m.IsNested,
                        Sequence = m.Sequence,
                        UserRoles = m.UserRoles,
                        NestedItems = m.NestedItems
                            .Where(n => n.UserRoles.Any(r => userRoles.Contains(r)))
                            .OrderBy(n => n.Sequence)
                            .ToList()
                    })
                    .ToList()
            };

            return View(filteredMenu);
        }
    }
}