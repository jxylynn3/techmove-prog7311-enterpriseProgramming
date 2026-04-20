using ST10448420_TechMove_GLMS.Models;

namespace ST10448420_TechMove_GLMS.Patterns.Builder
{
    public class ServiceRequestDirector
    {
        // We update the parameters to include the calculated ZAR and the saved PDF path
        public ServiceRequest Construct(
            ServiceRequestBuilder builder,
            int contractId,
            string description,
            decimal usdAmount,
            decimal zarAmount,
            string filePath)
        {
            // 1. The Director manages the sequence of building
            // Using the Fluent (Chained) approach makes this very readable
            return builder
                .SetContract(contractId)
                .SetDescription(description)
                .SetCostUSD(usdAmount)
                .SetCostZAR(zarAmount)
                .SetFilePath(filePath)
                .Build();
        }

        // Keep this version if you still want the State Check inside the Director
        public ServiceRequest ConstructWithStateCheck(
            ServiceRequestBuilder builder,
            Contract contract,
            string description,
            decimal usdAmount,
            decimal zarAmount,
            string filePath)
        {
            // Internal Pattern Rule: Validate state before allowing construction
            if (!contract.CurrentState.contractCanRaiseServiceRequest())
            {
                throw new InvalidOperationException("Cannot raise service request for a contract that is not active. Sworry!");
            }

            return builder
                .SetContract(contract.ContractID)
                .SetDescription(description)
                .SetCostUSD(usdAmount)
                .SetCostZAR(zarAmount)
                .SetFilePath(filePath)
                .Build();
        }
    }
}
