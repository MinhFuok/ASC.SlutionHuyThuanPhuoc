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
            var masterKeys = await _unitOfWork.Repository<MasterDataKey>().FindAllAsync();

            return masterKeys
                .Where(key => !key.IsDeleted)
                .OrderBy(key => key.Name)
                .ToList();
        }

        public async Task<List<MasterDataKey>> GetMasterKeyByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return new List<MasterDataKey>();
            }

            var masterKeys = await _unitOfWork.Repository<MasterDataKey>().FindAllByPartitionKeyAsync(name.Trim());

            return masterKeys
                .Where(key => !key.IsDeleted)
                .OrderBy(key => key.Name)
                .ToList();
        }

        public async Task<bool> InsertMasterKeyAsync(MasterDataKey key)
        {
            key.PartitionKey = Normalize(key.PartitionKey, key.Name);
            key.RowKey = Normalize(key.RowKey, Guid.NewGuid().ToString());
            key.Name = Normalize(key.Name, key.PartitionKey);
            EnsureAuditValues(key);

            if ((await GetMasterKeyByNameAsync(key.PartitionKey)).Any())
            {
                return false;
            }

            await _unitOfWork.Repository<MasterDataKey>().AddAsync(key);
            _unitOfWork.CommitTransaction();

            return true;
        }

        public async Task<bool> UpdateMasterKeyAsync(string originalPartitionKey, MasterDataKey key)
        {
            var masterKey = await _unitOfWork
                .Repository<MasterDataKey>()
                .FindAsync(originalPartitionKey, key.RowKey);

            if (masterKey == null)
            {
                return false;
            }

            masterKey.Name = Normalize(key.Name, masterKey.Name);
            masterKey.IsActive = key.IsActive;
            masterKey.UpdatedBy = Normalize(key.UpdatedBy, masterKey.UpdatedBy);

            _unitOfWork.Repository<MasterDataKey>().Update(masterKey);
            _unitOfWork.CommitTransaction();

            return true;
        }

        public async Task<List<MasterDataValue>> GetAllMasterValuesByKeyAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return await GetAllMasterValuesAsync();
            }

            var masterValues = await _unitOfWork
                .Repository<MasterDataValue>()
                .FindAllByPartitionKeyAsync(key.Trim());

            return masterValues
                .Where(value => !value.IsDeleted)
                .OrderBy(value => value.PartitionKey)
                .ThenBy(value => value.Name)
                .ToList();
        }

        public async Task<List<MasterDataValue>> GetAllMasterValuesAsync()
        {
            var masterValues = await _unitOfWork.Repository<MasterDataValue>().FindAllAsync();

            return masterValues
                .Where(value => !value.IsDeleted)
                .OrderBy(value => value.PartitionKey)
                .ThenBy(value => value.Name)
                .ToList();
        }

        public async Task<MasterDataValue?> GetMasterValueByNameAsync(string key, string name)
        {
            var masterValues = await GetAllMasterValuesByKeyAsync(key);

            return masterValues.FirstOrDefault(value =>
                string.Equals(value.Name, name?.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        public async Task<bool> InsertMasterValueAsync(MasterDataValue value)
        {
            value.PartitionKey = Normalize(value.PartitionKey, string.Empty);
            value.RowKey = Normalize(value.RowKey, Guid.NewGuid().ToString());
            value.Name = Normalize(value.Name, string.Empty);
            EnsureAuditValues(value);

            if (string.IsNullOrWhiteSpace(value.PartitionKey) ||
                string.IsNullOrWhiteSpace(value.Name) ||
                await GetMasterValueByNameAsync(value.PartitionKey, value.Name) != null)
            {
                return false;
            }

            await _unitOfWork.Repository<MasterDataValue>().AddAsync(value);
            _unitOfWork.CommitTransaction();

            return true;
        }

        public async Task<bool> UpdateMasterValueAsync(
            string originalPartitionKey,
            string originalRowKey,
            MasterDataValue value)
        {
            var masterValue = await _unitOfWork
                .Repository<MasterDataValue>()
                .FindAsync(originalPartitionKey, originalRowKey);

            if (masterValue == null)
            {
                return false;
            }

            var newPartitionKey = Normalize(value.PartitionKey, masterValue.PartitionKey);
            var newName = Normalize(value.Name, masterValue.Name);
            var duplicate = await GetMasterValueByNameAsync(newPartitionKey, newName);
            if (duplicate != null &&
                (!string.Equals(duplicate.PartitionKey, masterValue.PartitionKey, StringComparison.Ordinal) ||
                 !string.Equals(duplicate.RowKey, masterValue.RowKey, StringComparison.Ordinal)))
            {
                return false;
            }

            if (!string.Equals(masterValue.PartitionKey, newPartitionKey, StringComparison.Ordinal))
            {
                _unitOfWork.Repository<MasterDataValue>().Delete(masterValue);

                value.PartitionKey = newPartitionKey;
                value.RowKey = Normalize(value.RowKey, originalRowKey);
                value.Name = newName;
                value.CreatedBy = Normalize(masterValue.CreatedBy, value.UpdatedBy);
                value.UpdatedBy = Normalize(value.UpdatedBy, masterValue.UpdatedBy);

                await _unitOfWork.Repository<MasterDataValue>().AddAsync(value);
            }
            else
            {
                masterValue.Name = newName;
                masterValue.IsActive = value.IsActive;
                masterValue.UpdatedBy = Normalize(value.UpdatedBy, masterValue.UpdatedBy);
                _unitOfWork.Repository<MasterDataValue>().Update(masterValue);
            }

            _unitOfWork.CommitTransaction();

            return true;
        }

        public async Task<bool> UploadAllMasterDataAsync(List<MasterDataValue> values)
        {
            foreach (var value in values.Where(value =>
                         !string.IsNullOrWhiteSpace(value.PartitionKey) &&
                         !string.IsNullOrWhiteSpace(value.Name)))
            {
                value.PartitionKey = value.PartitionKey.Trim();
                value.Name = value.Name.Trim();
                EnsureAuditValues(value);

                if (!(await GetMasterKeyByNameAsync(value.PartitionKey)).Any())
                {
                    await InsertMasterKeyAsync(new MasterDataKey
                    {
                        PartitionKey = value.PartitionKey,
                        RowKey = Guid.NewGuid().ToString(),
                        Name = value.PartitionKey,
                        IsActive = true,
                        CreatedBy = value.CreatedBy,
                        UpdatedBy = value.UpdatedBy
                    });
                }

                var existingValue = await GetMasterValueByNameAsync(value.PartitionKey, value.Name);
                if (existingValue == null)
                {
                    value.RowKey = Guid.NewGuid().ToString();
                    await InsertMasterValueAsync(value);
                }
                else
                {
                    existingValue.IsActive = value.IsActive;
                    existingValue.UpdatedBy = value.UpdatedBy;
                    await UpdateMasterValueAsync(existingValue.PartitionKey, existingValue.RowKey, existingValue);
                }
            }

            return true;
        }

        private static void EnsureAuditValues(ASC.Model.BaseTypes.BaseEntity entity)
        {
            entity.CreatedBy = Normalize(entity.CreatedBy, "system");
            entity.UpdatedBy = Normalize(entity.UpdatedBy, entity.CreatedBy);
        }

        private static string Normalize(string? value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        }
    }
}
