using ST10448420_TechMove_GLMS.Models;

namespace ST10448420_TechMove_GLMS.Patterns.Builder
{
    public interface IServiceRequestBuilder
    {
        //we basically defining the process of step needed to build a service request, and the  builder will implement these steps
        void setContract(Contract _contract);
        void setContractDescription(string _contractDesc);
        void ApplyCurrencyConversion(decimal usdAmount);
        ServiceRequest Build();
    }
}
