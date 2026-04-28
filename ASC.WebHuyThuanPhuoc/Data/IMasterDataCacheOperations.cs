namespace ASC.WebHuyThuanPhuoc.Data
{
    public interface IMasterDataCacheOperations
    {
        Task<MasterDataCache> GetMasterDataCacheAsync();
        Task CreateMasterDataCacheAsync();
    }
}