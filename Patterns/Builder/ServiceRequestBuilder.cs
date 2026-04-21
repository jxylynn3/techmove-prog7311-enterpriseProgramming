using ST10448420_TechMove_GLMS.Models;
using ST10448420_TechMove_GLMS.Patterns.Singleton;

namespace ST10448420_TechMove_GLMS.Patterns.Builder
{
    public class ServiceRequestBuilder : IServiceRequestBuilder
    {
        private ServiceRequest _sRequest = new ServiceRequest();

        public ServiceRequestBuilder SetContract(int contractId)
        {
            _sRequest.ContractID = contractId;
            return this;
        }

        public ServiceRequestBuilder SetDescription(string description)
        {
            _sRequest.Description = description;
            return this;
        }

        public ServiceRequestBuilder SetCostUSD(decimal cost)
        {
            _sRequest.CostUSD = cost;
            return this;
        }

        public ServiceRequestBuilder SetCostZAR(decimal cost)
        {
            _sRequest.CostZAR = cost;
            return this;
        }

        public ServiceRequestBuilder SetFilePath(string path)
        {
            _sRequest.DocumentPath = path;
            return this;
        }

        public ServiceRequest Build()
        {
            // ✅ FIX #7 — Default status is "Draft" per requirements (State pattern)
            _sRequest.Status = "Draft";
            // ✅ FIX — Set CreatedAt timestamp on build
            _sRequest.CreatedAt = DateTime.Now;
            return _sRequest;
        }

        public void ApplyCurrencyConversion(decimal usdAmount)
        {
            var rate = ExchangeRates.Instance.GetRate();
            _sRequest.CostUSD = usdAmount;
            _sRequest.CostZAR = usdAmount * rate;
        }
    }
}