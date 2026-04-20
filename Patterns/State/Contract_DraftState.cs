namespace ST10448420_TechMove_GLMS.Patterns.State
{
    public class Contract_DraftState: IContractState
    {
    // In the draft state, the client is not allowed to create service requests.
        public bool contractCanRaiseServiceRequest() => false;
    }
}
