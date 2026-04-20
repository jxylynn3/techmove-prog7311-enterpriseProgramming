using ST10448420_TechMove_GLMS.Models;

namespace ST10448420_TechMove_GLMS.Patterns.Builder
{
    public class ServiceRequestDirector
    {
        public ServiceRequest Construct(IServiceRequestBuilder builder, Contract _contract, string _contractDesc, decimal usdAmount)
        {
        if(!_contract.CurrentState.contractCanRaiseServiceRequest())
        throw new InvalidOperationException("Cannot raise service request for a contract that is not active.Sworry");
            builder.setContract(_contract);
            builder.setContractDescription(_contractDesc);
            builder.ApplyCurrencyConversion(usdAmount);

            return builder.Build();
        }
    }
}
