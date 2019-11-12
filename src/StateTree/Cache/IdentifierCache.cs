using Skclusive.Mobx.Observable;
using System;
using System.Linq;

namespace Skclusive.Mobx.StateTree
{
    public class IdentifierCache
    {
        private IObservableMap<string, IObservableList<ObjectNode>> Cache = ObservableMap<string, IObservableList<ObjectNode>>.From();

        public IdentifierCache AddNodeToCache(ObjectNode node)
        {
            if (!string.IsNullOrWhiteSpace(node.IdentifierAttribute))
            {
                var identifier = node.Identifier;
                if (!Cache.ContainsKey(identifier))
                {
                    Cache[identifier] = ObservableList<ObjectNode>.From();
                }
                var set = Cache[identifier];
                if (set.Contains(node))
                {
                    throw new Exception("Already registered");
                }
                set.Add(node);
            }
            return this;
        }

        public IdentifierCache MergeCache(ObjectNode node)
        {
            var values = node.IdentifierCache.Cache.Values;
            foreach (var nodes in values)
            {
                foreach (var cnode in nodes)
                {
                    AddNodeToCache(cnode);
                }
            }
            return this;
        }

        public IdentifierCache NotifyDisposed(ObjectNode node)
        {
            if (!string.IsNullOrWhiteSpace(node.IdentifierAttribute))
            {
                var set = Cache[node.Identifier];
                set.Remove(node);
            }
            return this;
        }

        public IdentifierCache SplitCache(ObjectNode node)
        {
            var result = new IdentifierCache();
            var path = node.Path;
            var values = node.IdentifierCache.Cache.Values;
            foreach (var nodes in values)
            {
                for (int i = nodes.Count - 1; i >= 0; i--)
                {
                    if (nodes[i].Path.IndexOf(path) == 0)
                    {
                        result.AddNodeToCache(nodes[i]);
                        nodes.RemoveAt(i);
                    }
                }
            }
            return this;
        }

        public ObjectNode Resolve(IType type, string identifier)
        {
            if (!Cache.ContainsKey(identifier))
            {
                return null;
            }

            var set = Cache[identifier];

            var matches = set.Where(node => type.IsAssignableFrom(node.Type)).ToArray();

            switch (matches.Length)
            {
                case 0:
                    return null;
                case 1:
                    return matches[0];
                default:
                    throw new Exception($"Cannot resolve a reference to type '${type.Name}' with id: '{identifier}' unambigously, there are multiple candidates: {matches.Select(m => m.Path).Aggregate((ac, path) => $"{ac},{path}")}");
            }
        }
    }
}
