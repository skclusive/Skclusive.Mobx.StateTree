using System.Runtime.CompilerServices;

namespace Skclusive.Mobx.StateTree
{
    public class NodeCache
    {
        private static ConditionalWeakTable<object, IStateTreeNode> cache = new ConditionalWeakTable<object, IStateTreeNode>();

        public static void Add(object target, IStateTreeNode node)
        {
            if (target != null)
            {
                cache.Add(target, node);
            }
        }

        public static bool TryGetValue(object target, out IStateTreeNode node)
        {
            return cache.TryGetValue(target, out node);
        }

        public static bool Remove(object target)
        {
            return cache.Remove(target);
        }

        public static bool Contains(object target)
        {
            return cache.TryGetValue(target, out IStateTreeNode node);
        }
    }
}
