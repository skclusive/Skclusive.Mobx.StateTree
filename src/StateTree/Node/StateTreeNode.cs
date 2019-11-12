namespace Skclusive.Mobx.StateTree
{
    public class StateTreeNode : IStateTreeNode
    {
        public StateTreeNode(object treenode)
        {
            TreeNode = treenode;
        }

        public object TreeNode { private set; get; }
    }
}
