using ST10448420_TechMove_GLMS.Models;


namespace ST10448420_TechMove_GLMS.Patterns.Observer
{
    public class ServiceRequestGuard : IContractObserver
    {
        public void Update(Models.Contract _contract)
        {
            if (_contract.Status == "Expired")
            {
                Console.WriteLine("Contract you trying to use is expired. Block new requests.");
            }
            else if (_contract.Status == "On Hold")
            {
                Console.WriteLine("Contract is on hold. Block new requests and notify the client to contact support.");
            }
            else if (_contract.Status == "Active")
            {
                Console.WriteLine("Contract is active. Allow new requests to be created.");
            }
        }

        //add clauses the handle the different states of the contract, for example if the contract is on hold,
        //then block new requests and also notify the client that their contract is on hold.
        // and if active then allow new requests
        public bool Validate(Contract contract)
        {
            if (contract.Status == "Expired" || contract.Status == "On Hold")
            {
                return false;
            }

            return true;
        }
    }
}
