using ST10448420_TechMove_GLMS.Models;

namespace ST10448420_TechMove_GLMS.Patterns.Observer
{
    public class ContractManager : IContractSubject
    {
        private List<IContractObserver> _observers = new List<IContractObserver>();
        private Contract _contract;

        public ContractManager(Contract contract)
        {
            _contract = contract;
        }

        public void Attach(IContractObserver observer)
        {
            _observers.Add(observer);
        }

        public void Detach(IContractObserver observer)
        {
            _observers.Remove(observer);
        }

        public void Notify()
        {
            foreach (var obs in _observers)
            {
                obs.Update(_contract);
            }
        }
    }
}
