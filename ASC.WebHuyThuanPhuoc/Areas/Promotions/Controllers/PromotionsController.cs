using ASC.Business.Interfaces;
using ASC.Model.BaseTypes;
using ASC.Model.Models;
using ASC.Utilities;
using ASC.WebHuyThuanPhuoc.Areas.Promotions.Models;
using ASC.WebHuyThuanPhuoc.Controllers;
using ASC.WebHuyThuanPhuoc.Data;
using ASC.WebHuyThuanPhuoc.ServiceHub;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace ASC.WebHuyThuanPhuoc.Areas.Promotions.Controllers
{
    [Area("Promotions")]
    public class PromotionsController : BaseController
    {
        private readonly IPromotionOperations _promotionOperations;
        private readonly IMapper _mapper;
        private readonly IMasterDataCacheOperations _masterData;
        private readonly IHubContext<ServiceMessagesHub> _hubContext;

        public PromotionsController(
            IPromotionOperations promotionOperations,
            IMapper mapper,
            IMasterDataCacheOperations masterData,
            IHubContext<ServiceMessagesHub> hubContext)
        {
            _promotionOperations = promotionOperations;
            _mapper = mapper;
            _masterData = masterData;
            _hubContext = hubContext;
        }

        [HttpGet]
        public async Task<IActionResult> Promotion()
        {
            await LoadPromotionTypesAsync();

            var promotions = await _promotionOperations.GetAllPromotionsAsync();
            var promotionsViewModel = _mapper.Map<List<Promotion>, List<PromotionViewModel>>(promotions);

            HttpContext.Session.SetSession("Promotions", promotionsViewModel);

            return View(new PromotionsViewModel
            {
                Promotions = promotionsViewModel,
                IsEdit = false,
                PromotionInContext = new PromotionViewModel()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Promotion(PromotionsViewModel model)
        {
            ModelState.Remove("PromotionInContext.RowKey");

            model.Promotions = HttpContext.Session.GetSession<List<PromotionViewModel>>("Promotions")
                ?? new List<PromotionViewModel>();

            if (!ModelState.IsValid)
            {
                await LoadPromotionTypesAsync();
                return View(model);
            }

            var currentUser = HttpContext.User.GetCurrentUserDetails();
            var promotion = _mapper.Map<PromotionViewModel, Promotion>(model.PromotionInContext);

            if (model.IsEdit)
            {
                promotion.UpdatedBy = currentUser.Email;
                promotion.UpdatedDate = DateTime.UtcNow;

                await _promotionOperations.UpdatePromotionAsync(
                    model.PromotionInContext.RowKey!,
                    promotion);
            }
            else
            {
                promotion.RowKey = Guid.NewGuid().ToString();
                promotion.CreatedBy = currentUser.Email;
                promotion.UpdatedBy = currentUser.Email;
                promotion.CreatedDate = DateTime.UtcNow;
                promotion.UpdatedDate = DateTime.UtcNow;

                await _promotionOperations.CreatePromotionAsync(promotion);

                if (!promotion.IsDeleted)
                {
                    await _hubContext.Clients.All.SendAsync("publishPromotion", promotion);
                }
            }

            return RedirectToAction(nameof(Promotion));
        }

        [HttpGet]
        public async Task<IActionResult> Promotions()
        {
            var promotions = await _promotionOperations.GetAllPromotionsAsync();

            var promotionsViewModel = _mapper
                .Map<List<Promotion>, List<PromotionViewModel>>(promotions)
                .Where(p => !p.IsDeleted)
                .ToList();

            return View(promotionsViewModel);
        }

        private async Task LoadPromotionTypesAsync()
        {
            var masterData = await _masterData.GetMasterDataCacheAsync();

            ViewBag.PromotionTypes = masterData.Values
                .Where(p => p.PartitionKey == MasterKeys.PromotionType.ToString() && !p.IsDeleted)
                .ToList();
        }
    }
}