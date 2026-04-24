using ASC.Business.Interfaces;
using ASC.DataAccess;
using ASC.Model.Models;

namespace ASC.Business
{
    public class MasterDataOperations : IMasterDataOperations
    {
        private readonly IUnitOfWork _unitOfWork;

        public MasterDataOperations(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<MasterDataKey>> GetAllMasterKeysAsync()
        {
            var masterkeys = await _unitOfWork.Repository<MasterDataKey>().FindAllAsync();
            return masterkeys.ToList();
        }

        public async Task<List<MasterDataKey>> GetMasterKeysByNameAsync(string name)
        {
            var masterkeys = await _unitOfWork.Repository<MasterDataKey>().FindAllByPartitionKeyAsync(name);
            return masterkeys.ToList();
        }

        public async Task<bool> InsertMasterKeyAsync(MasterDataKey key)
        {
            using (_unitOfWork)
            {
                await _unitOfWork.Repository<MasterDataKey>().AddAsync(key);
                _unitOfWork.CommitTransaction();
                return true;
            }
        }

        public async Task<bool> UpdateMasterKeyAsync(string originalPartitionKey, MasterDataKey key)
        {
            using (_unitOfWork)
            {
                var masterkey = await _unitOfWork.Repository<MasterDataKey>().FindAsync(originalPartitionKey, key.RowKey);
                if (masterkey == null)
                {
                    return false;
                }

                masterkey.PartitionKey = key.PartitionKey;
                masterkey.Name = key.Name;
                masterkey.IsActive = key.IsActive;
                masterkey.UpdatedBy = key.UpdatedBy;
                _unitOfWork.Repository<MasterDataKey>().Update(masterkey);
                _unitOfWork.CommitTransaction();
                return true;
            }
        }

        public async Task<List<MasterDataValue>> GetAllMasterValuesByKeyAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return await GetAllMasterValuesAsync();
            }

            var mastervalues = await _unitOfWork.Repository<MasterDataValue>().FindAllByPartitionKeyAsync(key);
            return mastervalues.ToList();
        }

        public async Task<List<MasterDataValue>> GetAllMasterValuesAsync()
        {
            var mastervalues = await _unitOfWork.Repository<MasterDataValue>().FindAllAsync();
            return mastervalues.ToList();
        }

        public async Task<MasterDataValue?> GetMasterValueByNameAsync(string key, string name)
        {
            var mastervalues = await _unitOfWork.Repository<MasterDataValue>().FindAllByPartitionKeyAsync(key);
            return mastervalues.SingleOrDefault(p => p.Name == name);
        }

        public async Task<bool> InsertMasterValueAsync(MasterDataValue value)
        {
            using (_unitOfWork)
            {
                await _unitOfWork.Repository<MasterDataValue>().AddAsync(value);
                _unitOfWork.CommitTransaction();
                return true;
            }
        }

        public async Task<bool> UpdateMasterValueAsync(
            string originalPartitionKey,
            string originalRowKey,
            MasterDataValue value)
        {
            using (_unitOfWork)
            {
                var mastervalue = await _unitOfWork.Repository<MasterDataValue>().FindAsync(originalPartitionKey, originalRowKey);
                if (mastervalue == null)
                {
                    return false;
                }

                mastervalue.PartitionKey = value.PartitionKey;
                mastervalue.RowKey = value.RowKey;
                mastervalue.Name = value.Name;
                mastervalue.IsActive = value.IsActive;
                mastervalue.UpdatedBy = value.UpdatedBy;
                _unitOfWork.Repository<MasterDataValue>().Update(mastervalue);
                _unitOfWork.CommitTransaction();
                return true;
            }
        }

        public async Task<bool> UploadAllMasterDataAsync(List<MasterDataValue> values)
        {
            using (_unitOfWork)
            {
                foreach (var value in values)
                {
                    var masterkey = await GetMasterKeysByNameAsync(value.PartitionKey);
                    if (!masterkey.Any())
                    {
                        await _unitOfWork.Repository<MasterDataKey>().AddAsync(new MasterDataKey
                        {
                            RowKey = Guid.NewGuid().ToString(),
                            PartitionKey = value.PartitionKey,
                            Name = value.PartitionKey,
                            IsActive = true,
                            CreatedBy = value.CreatedBy,
                            UpdatedBy = value.UpdatedBy
                        });
                    }

                    var mastervalue = await GetMasterValueByNameAsync(value.PartitionKey, value.Name);
                    if (mastervalue == null)
                    {
                        await _unitOfWork.Repository<MasterDataValue>().AddAsync(value);
                    }
                    else
                    {
                        mastervalue.Name = value.Name;
                        mastervalue.IsActive = value.IsActive;
                        mastervalue.UpdatedBy = value.UpdatedBy;
                        _unitOfWork.Repository<MasterDataValue>().Update(mastervalue);
                    }
                }

                _unitOfWork.CommitTransaction();
                return true;
            }
        }
    }
}
