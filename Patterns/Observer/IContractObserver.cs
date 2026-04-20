using ST10448420_TechMove_GLMS.Models;

namespace ST10448420_TechMove_GLMS.Patterns.Observer
{
    public interface IContractObserver
    {
        // calls the update method to notify the observer of changes in the contract (crud)
        void Update(Contract _contract);
    }
}
