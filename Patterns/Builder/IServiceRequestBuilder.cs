using ST10448420_TechMove_GLMS.Models;

namespace ST10448420_TechMove_GLMS.Patterns.Builder
{
    public interface IServiceRequestBuilder
    {
        //we basically defining the process of step needed to build a service request, and the  builder will implement these steps
        ServiceRequestBuilder SetContract(int contractId);
        ServiceRequestBuilder SetDescription(string description);
        ServiceRequestBuilder SetCostUSD(decimal cost);
        ServiceRequestBuilder SetCostZAR(decimal cost);
        ServiceRequestBuilder SetFilePath(string path);

        ServiceRequest Build();
    }
}
