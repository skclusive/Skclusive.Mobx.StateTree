namespace Skclusive.Mobx.StateTree
{
    public enum NodeLifeCycle
    {
        INITIALIZING, // setting up
        CREATED, // afterCreate has run
        FINALIZED, // afterAttach has run
        DETACHING, // being detached from the tree
        DEAD // no coming back from this one
    }
}
