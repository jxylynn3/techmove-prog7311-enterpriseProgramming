namespace ST10448420_TechMove_GLMS.Patterns.State
{
    public class Contract_ActiveState : IContractState
    {
        // In the active state, the client is allowed to create service requests.
        public bool contractCanRaiseServiceRequest() => true;
    }
}
