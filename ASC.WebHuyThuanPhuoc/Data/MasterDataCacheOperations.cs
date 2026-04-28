using ASC.Business.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace ASC.WebHuyThuanPhuoc.Data
{
    public class MasterDataCacheOperations : IMasterDataCacheOperations
    {
        private readonly IDistributedCache _cache;
        private readonly IMasterDataOperations _masterData;
        private readonly string _masterDataCacheName = "MasterDataCache";

        public MasterDataCacheOperations(
            IDistributedCache cache,
            IMasterDataOperations masterData)
        {
            _cache = cache;
            _masterData = masterData;
        }

        public async Task CreateMasterDataCacheAsync()
        {
            var masterDataCache = new MasterDataCache
            {
                Keys = (await _masterData.GetAllMasterKeysAsync())
                    .Where(p => !p.IsDeleted)
                    .ToList(),

                Values = (await _masterData.GetAllMasterValuesAsync())
                    .Where(p => !p.IsDeleted)
                    .ToList()
            };

            var value = JsonConvert.SerializeObject(masterDataCache);

            await _cache.SetStringAsync(_masterDataCacheName, value);
        }

        public async Task<MasterDataCache> GetMasterDataCacheAsync()
        {
            var json = await _cache.GetStringAsync(_masterDataCacheName);

            if (string.IsNullOrWhiteSpace(json))
            {
                await CreateMasterDataCacheAsync();
                json = await _cache.GetStringAsync(_masterDataCacheName);
            }

            return JsonConvert.DeserializeObject<MasterDataCache>(json);
        }
    }
}