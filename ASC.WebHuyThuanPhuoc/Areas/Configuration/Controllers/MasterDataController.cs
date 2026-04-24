using AutoMapper;
using ASC.Business.Interfaces;
using ASC.Model.Models;
using ASC.Utilities;
using ASC.WebHuyThuanPhuoc.Areas.Configuration.Models;
using ASC.WebHuyThuanPhuoc.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;

namespace ASC.WebHuyThuanPhuoc.Areas.Configuration.Controllers
{
    [Area("Configuration")]
    [Authorize(Roles = "Admin")]
    public class MasterDataController : BaseController
    {
        private readonly IMasterDataOperations _masterData;
        private readonly IMapper _mapper;

        public MasterDataController(IMasterDataOperations masterData, IMapper mapper)
        {
            _masterData = masterData;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> MasterKeys()
        {
            var masterkeys = await _masterData.GetAllMasterKeysAsync();
            var masterkeysViewModel = _mapper.Map<List<MasterDataKey>, List<MasterDataKeyViewModel>>(masterkeys);

            HttpContext.Session.SetSession("Masterkeys", masterkeysViewModel);

            return View(new MasterKeysViewModel
            {
                MasterKeys = masterkeysViewModel ?? new List<MasterDataKeyViewModel>(),
                MasterKeyInContext = new MasterDataKeyViewModel(),
                IsEdit = false
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MasterKeys(MasterKeysViewModel model)
        {
            model.MasterKeys =
                HttpContext.Session.GetSession<List<MasterDataKeyViewModel>>("Masterkeys") ??
                new List<MasterDataKeyViewModel>();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var masterkey = _mapper.Map<MasterDataKeyViewModel, MasterDataKey>(model.MasterKeyInContext);
            if (model.IsEdit)
            {
                masterkey.UpdatedBy = CurrentUserName();
                await _masterData.UpdateMasterKeyAsync(
                    model.MasterKeyInContext.PartitionKey ?? string.Empty,
                    masterkey);
            }
            else
            {
                masterkey.RowKey = Guid.NewGuid().ToString();
                masterkey.PartitionKey = masterkey.Name;
                masterkey.CreatedBy = CurrentUserName();
                masterkey.UpdatedBy = CurrentUserName();
                await _masterData.InsertMasterKeyAsync(masterkey);
            }

            return RedirectToAction(nameof(MasterKeys));
        }

        [HttpGet]
        public async Task<IActionResult> MasterValues()
        {
            ViewBag.MasterKeys = await _masterData.GetAllMasterKeysAsync();

            return View(new MasterValuesViewModel
            {
                MasterValues = new List<MasterDataValueViewModel>(),
                MasterValueInContext = new MasterDataValueViewModel(),
                IsEdit = false
            });
        }

        [HttpGet]
        public async Task<IActionResult> MasterValuesByKey(string? key)
        {
            var mastervalues = await _masterData.GetAllMasterValuesByKeyAsync(key ?? string.Empty);

            return Json(new
            {
                data = _mapper.Map<List<MasterDataValue>, List<MasterDataValueViewModel>>(mastervalues)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MasterValues(MasterValuesViewModel model)
        {
            ViewBag.MasterKeys = await _masterData.GetAllMasterKeysAsync();

            if (!ModelState.IsValid)
            {
                model.MasterValues = _mapper.Map<List<MasterDataValue>, List<MasterDataValueViewModel>>(
                    await _masterData.GetAllMasterValuesAsync());
                return View(model);
            }

            var masterDataValue = _mapper.Map<MasterDataValueViewModel, MasterDataValue>(model.MasterValueInContext);
            if (model.IsEdit)
            {
                masterDataValue.UpdatedBy = CurrentUserName();
                await _masterData.UpdateMasterValueAsync(
                    masterDataValue.PartitionKey,
                    masterDataValue.RowKey ?? string.Empty,
                    masterDataValue);
            }
            else
            {
                masterDataValue.RowKey = Guid.NewGuid().ToString();
                masterDataValue.CreatedBy = CurrentUserName();
                masterDataValue.UpdatedBy = CurrentUserName();
                await _masterData.InsertMasterValueAsync(masterDataValue);
            }

            return RedirectToAction(nameof(MasterValues));
        }

        private async Task<List<MasterDataValue>> ParseMasterDataExcelAsync(IFormFile excelFile)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            await using var memoryStream = new MemoryStream();
            await excelFile.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            using var package = new ExcelPackage(memoryStream);
            var worksheet = package.Workbook.Worksheets.FirstOrDefault();
            var masterValueList = new List<MasterDataValue>();

            if (worksheet?.Dimension == null)
            {
                return masterValueList;
            }

            for (var row = 2; row <= worksheet.Dimension.End.Row; row++)
            {
                masterValueList.Add(new MasterDataValue
                {
                    RowKey = Guid.NewGuid().ToString(),
                    PartitionKey = worksheet.Cells[row, 1].Value?.ToString() ?? string.Empty,
                    Name = worksheet.Cells[row, 2].Value?.ToString() ?? string.Empty,
                    IsActive = ParseBoolean(worksheet.Cells[row, 3].Value)
                });
            }

            return masterValueList;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadExcel()
        {
            var files = Request.Form.Files;
            if (!files.Any())
            {
                return Json(new { Error = true, Text = "Upload a file" });
            }

            var excelFile = files.First();
            if (excelFile.Length <= 0)
            {
                return Json(new { Error = true, Text = "Upload a file" });
            }

            var masterData = await ParseMasterDataExcelAsync(excelFile);
            foreach (var masterDataValue in masterData)
            {
                masterDataValue.CreatedBy = CurrentUserName();
                masterDataValue.UpdatedBy = CurrentUserName();
            }

            var result = await _masterData.UploadAllMasterDataAsync(masterData);
            return Json(new { Success = result });
        }

        private string CurrentUserName()
        {
            var currentUser = User.GetCurrentUserDetails();

            if (!string.IsNullOrWhiteSpace(currentUser.Name))
            {
                return currentUser.Name;
            }

            if (!string.IsNullOrWhiteSpace(currentUser.Email))
            {
                return currentUser.Email;
            }

            return "system";
        }

        private static bool ParseBoolean(object? value)
        {
            var text = value?.ToString()?.Trim();
            if (bool.TryParse(text, out var result))
            {
                return result;
            }

            return string.IsNullOrWhiteSpace(text) ||
                text == "1" ||
                text.Equals("yes", StringComparison.OrdinalIgnoreCase) ||
                text.Equals("active", StringComparison.OrdinalIgnoreCase);
        }
    }
}
