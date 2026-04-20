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
            return this; // Allows chaining
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
            // Assuming your ServiceRequest model has a DocumentPath or FilePath property
            _sRequest.DocumentPath = path;
            return this;
        }

        public ServiceRequest Build()
        {
            _sRequest.Status = "Requested";
            // Ensure we return a fresh instance if needed, 
            // but for this flow, returning the built object is standard.
            return _sRequest;
        }

        // Keep your old method for backward compatibility if Director still uses it
        public void ApplyCurrencyConversion(decimal usdAmount)
        {
            var rate = ExchangeRates.Instance.GetRate();
            _sRequest.CostUSD = usdAmount;
            _sRequest.CostZAR = usdAmount * (decimal)rate;
        }
    }
}