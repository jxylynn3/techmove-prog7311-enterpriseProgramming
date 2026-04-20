namespace ST10448420_TechMove_GLMS.Patterns.State
{
    public class Contract_ExpiredState: IContractState
    {
        // In the expired state, the client is not allowed to create service requests.
        public bool contractCanRaiseServiceRequest() => false;
    }
}
