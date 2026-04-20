namespace ST10448420_TechMove_GLMS.Patterns.State
{
    public class Contract_OnHoldState: IContractState
    {
        // In the on-hold state, the client is not allowed to create service requests.
        public bool contractCanRaiseServiceRequest() => false;
    }
}
