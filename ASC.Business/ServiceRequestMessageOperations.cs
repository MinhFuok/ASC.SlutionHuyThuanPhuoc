using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASC.Business.Interfaces;
using ASC.DataAccess;
using ASC.Model.Models;

namespace ASC.Business
{
    public class ServiceRequestMessageOperations : IServiceRequestMessageOperations
    {
        private readonly IUnitOfWork _unitOfWork;

        public ServiceRequestMessageOperations(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task CreateServiceRequestMessageAsync(ServiceRequestMessage message)
        {
            await _unitOfWork.Repository<ServiceRequestMessage>().AddAsync(message);
            _unitOfWork.CommitTransaction();
        }

        public async Task<List<ServiceRequestMessage>> GetServiceRequestMessagesAsync(string serviceRequestId)
        {
            var messages = await _unitOfWork.Repository<ServiceRequestMessage>()
                .FindAllByPartitionKeyAsync(serviceRequestId);

            return messages
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.MessageDate)
                .ToList();
        }
    }
}
