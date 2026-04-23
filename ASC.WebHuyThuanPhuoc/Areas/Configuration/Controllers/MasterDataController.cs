using AutoMapper;
using ASC.Business.Interfaces;
using ASC.Model.Models;
using ASC.Utilities;
using ASC.WebHuyThuanPhuoc.Areas.Configuration.Models;
using ASC.WebHuyThuanPhuoc.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
            return View(await BuildMasterKeysViewModelAsync(new MasterDataKeyViewModel
            {
                IsActive = true
            }));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MasterKeys(MasterKeysViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(await BuildMasterKeysViewModelAsync(
                    model.MasterKeyInContext,
                    model.IsEdit));
            }

            var masterKey = _mapper.Map<MasterDataKey>(model.MasterKeyInContext);
            masterKey.UpdatedBy = CurrentUserName();

            bool result;
            if (model.IsEdit)
            {
                result = await _masterData.UpdateMasterKeyAsync(
                    model.MasterKeyInContext.PartitionKey ?? string.Empty,
                    masterKey);
            }
            else
            {
                masterKey.RowKey = Guid.NewGuid().ToString();
                masterKey.PartitionKey = model.MasterKeyInContext.Name;
                masterKey.CreatedBy = CurrentUserName();
                result = await _masterData.InsertMasterKeyAsync(masterKey);
            }

            if (!result)
            {
                ModelState.AddModelError(string.Empty, "Master key could not be saved. Check for duplicates.");
                return View(await BuildMasterKeysViewModelAsync(
                    model.MasterKeyInContext,
                    model.IsEdit));
            }

            return RedirectToAction(nameof(MasterKeys));
        }

        [HttpGet]
        public async Task<IActionResult> MasterValues()
        {
            return View(await BuildMasterValuesViewModelAsync(new MasterDataValueViewModel
            {
                IsActive = true
            }));
        }

        [HttpGet]
        public async Task<IActionResult> MasterValuesByKey(string? key)
        {
            var masterValues = string.IsNullOrWhiteSpace(key)
                ? await _masterData.GetAllMasterValuesAsync()
                : await _masterData.GetAllMasterValuesByKeyAsync(key);

            return Json(new
            {
                data = _mapper.Map<List<MasterDataValueViewModel>>(masterValues)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MasterValues(MasterValuesViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(await BuildMasterValuesViewModelAsync(
                    model.MasterValueInContext,
                    model.IsEdit,
                    model.OriginalPartitionKey,
                    model.OriginalRowKey));
            }

            var masterValue = _mapper.Map<MasterDataValue>(model.MasterValueInContext);
            masterValue.UpdatedBy = CurrentUserName();

            bool result;
            if (model.IsEdit)
            {
                result = await _masterData.UpdateMasterValueAsync(
                    model.OriginalPartitionKey ?? string.Empty,
                    model.OriginalRowKey ?? string.Empty,
                    masterValue);
            }
            else
            {
                masterValue.RowKey = Guid.NewGuid().ToString();
                masterValue.CreatedBy = CurrentUserName();
                result = await _masterData.InsertMasterValueAsync(masterValue);
            }

            if (!result)
            {
                ModelState.AddModelError(string.Empty, "Master value could not be saved. Check for duplicates.");
                return View(await BuildMasterValuesViewModelAsync(
                    model.MasterValueInContext,
                    model.IsEdit,
                    model.OriginalPartitionKey,
                    model.OriginalRowKey));
            }

            return RedirectToAction(nameof(MasterValues));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadExcel()
        {
            var excelFile = Request.Form.Files.FirstOrDefault();
            if (excelFile == null || excelFile.Length == 0)
            {
                return Json(new { success = false, text = "Upload a file." });
            }

            var masterData = await ParseMasterDataExcelAsync(excelFile);
            if (masterData.Count == 0)
            {
                return Json(new { success = false, text = "No master data rows were found." });
            }

            foreach (var value in masterData)
            {
                value.CreatedBy = CurrentUserName();
                value.UpdatedBy = CurrentUserName();
            }

            var result = await _masterData.UploadAllMasterDataAsync(masterData);
            return Json(new { success = result, text = result ? "Upload completed." : "Upload failed." });
        }

        private async Task<MasterKeysViewModel> BuildMasterKeysViewModelAsync(
            MasterDataKeyViewModel masterKeyInContext,
            bool isEdit = false)
        {
            var masterKeys = await _masterData.GetAllMasterKeysAsync();

            return new MasterKeysViewModel
            {
                MasterKeys = _mapper.Map<List<MasterDataKeyViewModel>>(masterKeys),
                MasterKeyInContext = masterKeyInContext,
                IsEdit = isEdit
            };
        }

        private async Task<MasterValuesViewModel> BuildMasterValuesViewModelAsync(
            MasterDataValueViewModel masterValueInContext,
            bool isEdit = false,
            string? originalPartitionKey = null,
            string? originalRowKey = null)
        {
            var masterValues = await _masterData.GetAllMasterValuesAsync();

            return new MasterValuesViewModel
            {
                MasterValues = _mapper.Map<List<MasterDataValueViewModel>>(masterValues),
                MasterValueInContext = masterValueInContext,
                MasterKeys = await BuildMasterKeySelectListAsync(),
                OriginalPartitionKey = originalPartitionKey ?? masterValueInContext.PartitionKey,
                OriginalRowKey = originalRowKey ?? masterValueInContext.RowKey,
                IsEdit = isEdit
            };
        }

        private async Task<List<SelectListItem>> BuildMasterKeySelectListAsync()
        {
            var masterKeys = await _masterData.GetAllMasterKeysAsync();

            return masterKeys
                .Where(key => key.IsActive)
                .OrderBy(key => key.Name)
                .Select(key => new SelectListItem
                {
                    Text = key.Name,
                    Value = key.PartitionKey
                })
                .ToList();
        }

        private async Task<List<MasterDataValue>> ParseMasterDataExcelAsync(IFormFile excelFile)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            await using var memoryStream = new MemoryStream();
            await excelFile.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            using var package = new ExcelPackage(memoryStream);
            var worksheet = package.Workbook.Worksheets.FirstOrDefault();
            var masterDataValues = new List<MasterDataValue>();

            if (worksheet?.Dimension == null)
            {
                return masterDataValues;
            }

            for (var row = 2; row <= worksheet.Dimension.End.Row; row++)
            {
                var partitionKey = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                var name = worksheet.Cells[row, 2].Value?.ToString()?.Trim();

                if (string.IsNullOrWhiteSpace(partitionKey) ||
                    string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                masterDataValues.Add(new MasterDataValue
                {
                    PartitionKey = partitionKey,
                    Name = name,
                    IsActive = ParseBoolean(worksheet.Cells[row, 3].Value)
                });
            }

            return masterDataValues;
        }

        private string CurrentUserName()
        {
            return User.GetCurrentUser().UserName ??
                User.GetCurrentUser().Email ??
                "system";
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
