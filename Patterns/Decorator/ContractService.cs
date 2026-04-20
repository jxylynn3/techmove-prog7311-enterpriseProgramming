using ST10448420_TechMove_GLMS.Models;

namespace ST10448420_TechMove_GLMS.Patterns.Decorator
{
    public class ContractService : IContractService
    {
        public void updateContractStatus(Contract _contract)
        {
            // code to update the contract status in the database
            Console.WriteLine($"Contract {_contract.ContractID} status updated to: {_contract.Status}");
        }
    }
}
