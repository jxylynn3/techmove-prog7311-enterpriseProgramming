namespace ST10448420_TechMove_GLMS.Patterns.Observer
{
    public interface IContractSubject
    {
            void Attach(IContractObserver _observers);
            void Detach(IContractObserver _observers);
            void Notify();
    }
}
