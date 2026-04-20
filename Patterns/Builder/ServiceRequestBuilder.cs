using ST10448420_TechMove_GLMS.Models;
using ST10448420_TechMove_GLMS.Patterns.Singleton;

namespace ST10448420_TechMove_GLMS.Patterns.Builder
{
    public class ServiceRequestBuilder: IServiceRequestBuilder
    {
        private ServiceRequest _sRequest = new ServiceRequest();

        public void setContract(Contract _contract)
        {
            _sRequest.ContractID = _contract.ContractID;
        }

        public void setContractDescription(string _contractDesc)
        {
            _sRequest.Description = _contractDesc;
        }

        public void ApplyCurrencyConversion(decimal usdAmount)
        {
            var rate = ExchangeRates.Instance.GetRate();
            _sRequest.CostUSD = usdAmount;
            _sRequest.CostZAR = usdAmount * rate;
        }

        public ServiceRequest Build()
        {
            _sRequest.Status = "Requested";
            return _sRequest;
        }
    }
}