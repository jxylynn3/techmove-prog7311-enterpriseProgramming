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
//what does the builder do within this application?
// The Builder pattern in this application is used to construct complex ServiceRequest objects step by step.
// The ServiceRequestDirector class orchestrates the construction process, ensuring that all necessary properties of a ServiceRequest are set correctly. 
// allows for the service request to have a clear and consistent way of being created, especially when there are multiple properties that need to be set, such as contract ID, description, costs in both USD and ZAR, and the file path for the associated document.