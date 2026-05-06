using ASC.Business.Interfaces;
using ASC.DataAccess;
using ASC.Model.Models;

namespace ASC.Business
{
    public class OnlineUsersOperations : IOnlineUsersOperations
    {
        private readonly IUnitOfWork _unitOfWork;

        public OnlineUsersOperations(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task CreateOnlineUserAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            var users = await _unitOfWork.Repository<OnlineUser>()
                .FindAllByPartitionKeyAsync(name);

            var onlineUser = users.FirstOrDefault();

            if (onlineUser == null)
            {
                onlineUser = new OnlineUser(name)
                {
                    IsDeleted = false,
                    CreatedBy = name,
                    UpdatedBy = name
                };

                await _unitOfWork.Repository<OnlineUser>().AddAsync(onlineUser);
            }
            else
            {
                onlineUser.IsDeleted = false;
                onlineUser.UpdatedBy = name;

                _unitOfWork.Repository<OnlineUser>().Update(onlineUser);
            }

            _unitOfWork.CommitTransaction();
        }

        public async Task DeleteOnlineUserAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            var users = await _unitOfWork.Repository<OnlineUser>()
                .FindAllByPartitionKeyAsync(name);

            var onlineUser = users.FirstOrDefault();

            if (onlineUser == null)
            {
                return;
            }

            onlineUser.IsDeleted = true;
            onlineUser.UpdatedBy = name;

            _unitOfWork.Repository<OnlineUser>().Update(onlineUser);
            _unitOfWork.CommitTransaction();
        }

        public async Task<bool> GetOnlineUserAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            var users = await _unitOfWork.Repository<OnlineUser>()
                .FindAllByPartitionKeyAsync(name);

            var onlineUser = users.FirstOrDefault();

            return onlineUser != null && !onlineUser.IsDeleted;
        }
    }
}