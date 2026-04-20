using ST10448420_TechMove_GLMS.Models;

namespace ST10448420_TechMove_GLMS.Patterns.Observer
{
    public class ContractAudit_logger : IContractObserver
    {
        public void Update(Contract contract)
        {
            Console.WriteLine($"We Logging a change in a Contract \nContract {contract.ContractID} changed to {contract.Status}");
        }
    }
}
