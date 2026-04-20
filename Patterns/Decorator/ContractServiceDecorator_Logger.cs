using ST10448420_TechMove_GLMS.Models;

namespace ST10448420_TechMove_GLMS.Patterns.Decorator
{
    public class ContractServiceDecorator_Logger: IContractService
    {
        private readonly IContractService _service;
        public ContractServiceDecorator_Logger(IContractService service)
        {
            _service = service;
        }

        public void updateContractStatus(Contract _contract)
        {
            Console.WriteLine($"Logging: Updating contract {_contract.ContractID} status to {_contract.Status}");
            _service.updateContractStatus(_contract);
        }
    }
}
