using ASC.Model.Models;

namespace ASC.WebHuyThuanPhuoc.Areas.ServiceRequests.Models
{
    public class ServiceRequestDetailViewModel
    {
        public UpdateServiceRequestViewModel ServiceRequest { get; set; } = new();

        public List<ServiceRequest> ServiceRequestAudit { get; set; } = new();
    }
}