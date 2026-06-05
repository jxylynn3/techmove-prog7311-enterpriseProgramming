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
// what is the purpose of the Decorator pattern in this context?
// its a structural design pattern that allows behavior to be added to individual objects, either statically or dynamically, without affecting the behavior of other objects from the same class.
// In this context, it allows us to add additional functionality (like logging) to the contract service without modifying the original ContractService class