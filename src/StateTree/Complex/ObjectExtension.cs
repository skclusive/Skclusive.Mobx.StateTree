//using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Skclusive.Mobx.StateTree
{
    public static class ObjectExtension
    {
        public static bool IsStateTreeNode(this object value)
        {
            return value != null && (value is IStateTreeNode || NodeCache.Contains(value));
        }

        //public static IType<S, T> GetNodeType<S, T>(this IStateTreeNode node)
        //{
        //    return (IType<S, T>)node.GetStateTreeNode().Type;
        //}

        //public static IType<S, T> GetNodeType<S, T>(this object node)
        //{
        //    return (IType<S, T>)node.GetStateTreeNode().Type;
        //}

        public static IType GetNodeType(this IStateTreeNode node)
        {
            return node.GetStateTreeNode().Type;
        }

        public static IType GetNodeType(this object node)
        {
            return node.GetStateTreeNode().Type;
        }

        public static IType<S, T> GetChildType<S, T>(this IStateTreeNode node, string child)
        {
            return node.GetStateTreeNode().GetChildType<S, T>(child);
        }

        public static IType<S, T> GetChildType<S, T>(this object node, string child)
        {
            return node.GetStateTreeNode().GetChildType<S, T>(child);
        }

        public static IStateTreeNode GetRoot(this IStateTreeNode target)
        {
            // check all arguments
            if (!target.IsStateTreeNode())
            {
                throw new Exception($"expected first argument to be a mobx-state-tree node, got {target} instead");
            }
            return target.GetStateTreeNode().Root.StoredValue.GetStateTree();
        }

        public static ObjectNode GetStateTreeNode(this IStateTreeNode node, bool throwing = true)
        {
            return node.TreeNode as ObjectNode;
        }

        public static IStateTreeNode GetStateTree(this object node, bool throwing = true)
        {
            if (node.IsStateTreeNode())
            {
                if (node is IStateTreeNode snode)
                {
                    return snode;
                }

                if (NodeCache.TryGetValue(node, out IStateTreeNode tnode))
                {
                    return tnode;
                }
            }

            return null;
        }

        public static ObjectNode GetStateTreeNode(this object node, bool throwing = true)
        {
            if (node.IsStateTreeNode())
            {
                if (node is IStateTreeNode snode)
                {
                    return snode.TreeNode as ObjectNode;
                }

                if (NodeCache.TryGetValue(node, out IStateTreeNode tnode))
                {
                    return tnode.TreeNode as ObjectNode;
                }
            }

            if (throwing)
            {
                throw new Exception($"Value {node} is no MST Node");
            }

            return null;
        }

        public static object ToSnapshot(this object target)
        {
            return target.GetStateTreeNode().Snapshot;
        }

        public static string ToJson(this object target)
        {
            // return JsonConvert.SerializeObject(target.ToSnapshot());
            return $"{target.ToSnapshot()}";
        }

        public static void Protected(this object target)
        {
            target.GetStateTreeNode().IsProtectionEnabled = true;
        }

        public static void Unprotected(this object target)
        {
            target.GetStateTreeNode().IsProtectionEnabled = false;
        }

        public static bool IsProtected(this object target)
        {
            return target.GetStateTreeNode().IsProtected;
        }

        public static IDisposable OnPatch(this object target, Action<IJsonPatch, IJsonPatch> onPatch)
        {
            return target.GetStateTreeNode().OnPatch(onPatch);
        }

        public static IDisposable OnSnapshot(this object target, Action<object> onSnapshot)
        {
            return target.GetStateTreeNode().OnSnapshot(onSnapshot);
        }

        public static IDisposable OnSnapshot<S>(this object target, Action<S> onSnapshot)
        {
            return target.OnSnapshot((object snapshot) => onSnapshot((S)snapshot));
        }

        public static object GetSnapshot(this object target, bool applyPostProcess = true)
        {
            var node = target.GetStateTreeNode();

            var snapshot = applyPostProcess ? node.Snapshot : node.Type.GetSnapshot(node, applyPostProcess);

            return snapshot;
        }

        public static S GetSnapshot<S>(this object target, bool applyPostProcess = true)
        {
            return (S)target.GetSnapshot();
        }

        public static bool HasParent(this object target, int depth = 1)
        {
            if (depth < 0)
            {
                throw new InvalidOperationException($"Invalid depth: ${depth}, should be >= 1");
            }

            var parent = target.GetStateTreeNode().Parent;
            while (parent != null)
            {
                if (--depth == 0)
                {
                    return true;
                }
                parent = parent.Parent;
            }
            return false;
        }

        public static T GetParent<T>(this object target, int depth = 1)
        {
            if (depth < 0)
            {
                throw new InvalidOperationException($"Invalid depth: ${depth}, should be >= 1");
            }
            var depthx = depth;
            var node = target.GetStateTreeNode();
            var parent = node.Parent;
            while (parent != null)
            {
                if (--depth == 0)
                {
                    return (T)parent.StoredValue;
                }
                parent = parent.Parent;
            }

            throw new InvalidOperationException($"Failed to find the parent of {node} at depth {depthx}");
        }

        public static T GetRoot<T>(this object target)
        {
            return (T)target.GetStateTreeNode().Root.StoredValue;
        }

        public static string GetPath(this object target)
        {
            return target.GetStateTreeNode().Path;
        }

        public static string[] GetPathParts(this object target)
        {
            return target.GetPath().SplitJsonPath();
        }

        public static bool IsRoot(this object target)
        {
            return target.GetStateTreeNode().IsRoot;
        }

        public static object ResolvePath(this object target, string path)
        {
            return target.ResolvePath<object>(path);
        }

        public static T ResolvePath<T>(this object target, string path)
        {
            var node = target.GetStateTreeNode().ResolveNodeByPath(path);

            return (T)node?.Value;
        }

        public static object ResolveIdentifier(this object target, IType type, string identifier)
        {
            return target.ResolveIdentifier<object>(type, identifier);
        }

        public static T ResolveIdentifier<T>(this object target, IType type, string identifier)
        {
            var node = target.GetStateTreeNode().Root.IdentifierCache?.Resolve(type, identifier);
            return (T)node?.Value;
        }

        public static string GetIdentifier(this object target)
        {
            return target.GetStateTreeNode().Identifier;
        }

        public static object TryResolve(this object target, string path)
        {
            var node = target.GetStateTreeNode().ResolveNodeByPath(path, false);
            try
            {
                return node?.Value;
            }
            catch
            {
                // For what ever reason not resolvable (e.g. totally not existing path, or value that cannot be fetched)
                // see test / issue: 'try resolve doesn't work #686'
                return null;
            }
        }

        public static string GetRelativePath(this object target, object relative)
        {
            return StateTreeUtils.GetRelativePathBetweenNodes(target.GetStateTreeNode(), relative.GetStateTreeNode());
        }

        public static void ApplySnapshot<S>(this object target, S snapshot)
        {
            GetStateTreeNode(target).ApplySnapshot(snapshot);
        }

        public static void ApplySnapshot(this object target, IDictionary<string, object> snapshot)
        {
            GetStateTreeNode(target).ApplySnapshot(snapshot);
        }

        public static void ApplyPatch(this object target, params IJsonPatch[] patches)
        {
            GetStateTreeNode(target).ApplyPatches(patches);
        }
    }
}
