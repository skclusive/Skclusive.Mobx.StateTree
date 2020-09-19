using Skclusive.Mobx.Observable;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Skclusive.Mobx.StateTree
{
    public class StateTreeUtils
    {
        public static string SafeString(object value)
        {
            return value?.ToString();
        }

        public static void Typecheck(IType type, object value)
        {
            TypecheckPublic(type, value);
        }

        public static void TypecheckPublic(IType type, object value)
        {
            var errors = type.Validate(value, new IContextEntry[] { new ContextEntry { Path = "", Type = type as IType } });

            if (errors.Length > 0)
            {
                throw new Exception(errors.Select(error => error.Message).Aggregate((ac, str) => $"{ac}-{str}"));
            }
        }

        public static void Walk(IStateTreeNode target, Action<IStateTreeNode> processor)
        {
            var node = target.GetStateTreeNode();

            foreach (var child in node.GetChildren())
            {
                if (child.StoredValue.IsStateTreeNode())
                {
                    Walk(child.StoredValue.GetStateTree(), processor);
                }
            }

            processor(node.StoredValue.GetStateTree());
        }


        public static string GetRelativePathBetweenNodes(ObjectNode from, ObjectNode to)
        {
            // TODO: investigate below commented code on node removal
            //pre condition is to is child of from
            //if (from.Root != to.Root)
            //{
            //    throw new InvalidOperationException($"Cannot calculate relative path: objects '{from}' and '{to}' are not part of the same object tree");
            //}

            var fromPaths = from.Path.SplitJsonPath();
            var toPaths = to.Path.SplitJsonPath();

            int common = 0;
            for (; common < fromPaths.Length; common++)
            {
                if (fromPaths[common] != toPaths[common])
                {
                    break;
                }
            }

            // TODO: assert that no targetParts paths are "..", "." or ""!
            return $"{fromPaths.Skip(common).Select(p => "..").Join("/")}{toPaths.Skip(common).JoinJsonPath()}";
        }

        public static bool IsNode(object node)
        {
            return node is ScalarNode || node is ObjectNode;
        }

        public static bool IsMutatble(object value)
        {
            return value != null;
        }

        public static bool IsPlainObject(object value)
        {
            return value is IDictionary;
        }

        public static bool AreSame(INode oldNode, object newValue)
        {
            // the new value has the same node
            if (newValue.IsStateTreeNode())
            {
                return newValue.GetStateTreeNode() == oldNode;
            }

            if (IsMutatble(newValue) && oldNode.Snapshot == newValue)
            {
                return true;
            }

            if (oldNode is ObjectNode oNode && newValue is IDictionary dictionary)
            {
                if (!string.IsNullOrWhiteSpace(oNode.Identifier) && !string.IsNullOrWhiteSpace(oNode.IdentifierAttribute))
                {
                    return EqualityComparer<object>.Default.Equals(dictionary[oNode.IdentifierAttribute], oNode.Identifier);
                }
            }

            return false;
        }

        public static INode ValueAsNode<S, T>(IType<S, T> type, ObjectNode parent, string subpath, object value, bool typeCheck)
        {
            return ValueAsNode<S, T>(type, parent, subpath, value, null, typeCheck);
        }

        public static INode ValueAsNode<S, T>(IType<S, T> type, ObjectNode parent, string subpath, object value, INode oldNode, bool typeCheck)
        {
            if (typeCheck)
            {
                Typecheck(type, value);
            }

            // the new value has a MST node
            if (value.IsStateTreeNode())
            {
                var node = value.GetStateTreeNode();

                node.AssertAlive();

                // the node lives here
                if (node.Parent != null && node.Parent == parent)
                {
                    node.SetParent(parent, subpath);

                    if (oldNode != null && oldNode != node)
                    {
                        oldNode.Dispose();
                    }
                    return node;
                }
            }

            // there is old node and new one is a value/snapshot
            if (oldNode != null)
            {
                var node = type.Reconcile(oldNode, value);
                node.SetParent(parent, subpath);
                return node;
            }

            // TODO: revisit custom impl
            if (value is INodeHolder nodeHolder)
            {
                var node = nodeHolder.Node;
                node.SetParent(parent, subpath);
                return node;
            }

            // nothing to do, create from scratch
            return type.Instantiate(parent, subpath, parent.Environment, value);
        }

        public static List<INode> ReconcileListItems<S, T>(IType<S, T> type, ObjectNode parent, List<INode> oldNodes, object[] newValues, string[] newPaths, bool typeCheck = true)
        {
            INode oldNode, oldMatch;

            object newValue;

            bool hasNewNode = false;

            for (int i = 0; ; i++)
            {
                hasNewNode = i <= newValues.Length - 1;
                oldNode = i < oldNodes.Count ? oldNodes[i] : null;
                newValue = hasNewNode ? newValues[i] : null;

                // for some reason, instead of newValue we got a node, fallback to the storedValue
                // TODO: https://github.com/mobxjs/mobx-state-tree/issues/340#issuecomment-325581681
                if (IsNode(newValue) || newValue is TempNode)
                {
                    newValue = (newValue as INode).StoredValue;
                }

                // both are empty, end
                if (oldNode == null && !hasNewNode)
                {
                    break;
                    // new one does not exists, old one dies
                }
                else if (!hasNewNode)
                {
                    oldNode.Dispose();
                    oldNodes.Splice(i, 1);
                    i--;
                    // there is no old node, create it
                }
                else if (oldNode == null)
                {
                    // check if already belongs to the same parent. if so, avoid pushing item in. only swapping can occur.
                    if (newValue.IsStateTreeNode() && newValue.GetStateTreeNode().Parent == parent)
                    {
                        // this node is owned by this parent, but not in the reconcilable set, so it must be double
                        throw new Exception($"Cannot add an object to a state tree if it is already part of the same or another state tree.Tried to assign an object to '{parent.Path}/{newPaths[i]}', but it lives already at '{newValue.GetStateTreeNode().Path}'");
                    }
                    oldNodes.Splice(i, 0, ValueAsNode(type, parent, newPaths[i], newValue, typeCheck));

                }
                else if (AreSame(oldNode, newValue)) // both are the same, reconcile
                {
                    oldNodes[i] = ValueAsNode(type, parent, newPaths[i], newValue, oldNode, typeCheck);
                    // nothing to do, try to reorder
                }
                else
                {
                    oldMatch = null;

                    // find a possible candidate to reuse
                    for (int j = i; j < oldNodes.Count; j++)
                    {
                        if (AreSame(oldNodes[j], newValue))
                        {
                            oldMatch = oldNodes.Splice(j, 1)[0];
                            break;
                        }
                    }

                    oldNodes.Splice(i, 0, ValueAsNode(type, parent, newPaths[i], newValue, oldMatch, typeCheck));
                }
            }

            return oldNodes;
        }

        public static IContextEntry[] GetContextForPath(IContextEntry[] context, string path, IType type)
        {
            return context.Concat(new IContextEntry[] { new ContextEntry { Path = path, Type = type } }).ToArray();
        }
    }
}
