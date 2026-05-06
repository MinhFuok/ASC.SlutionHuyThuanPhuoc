using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASC.Model.BaseTypes;

namespace ASC.Model.Models
{
    public class ServiceRequestMessage : BaseEntity
    {
        public ServiceRequestMessage()
        {
        }

        public ServiceRequestMessage(string serviceRequestId)
        {
            RowKey = Guid.NewGuid().ToString();
            PartitionKey = serviceRequestId;
        }

        public string FromDisplayName { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime? MessageDate { get; set; }
    }
}