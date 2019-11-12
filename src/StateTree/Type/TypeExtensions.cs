using System;
using System.Linq;

namespace Skclusive.Mobx.StateTree
{
    public static class TypeExtensions
    {
        public static bool IsSimpleType(
            this object target)
        {
            return target == null || target.GetType().IsSimpleType();
        }

        /// <summary>
        /// Determine whether a type is simple (String, Decimal, DateTime, etc)
        /// or complex (i.e. custom class with public properties and methods).
        /// </summary>
        /// <see cref="http://stackoverflow.com/questions/2442534/how-to-test-if-type-is-primitive"/>
        public static bool IsSimpleType(
        this Type type)
        {
            return
                type.IsValueType ||
                type.IsPrimitive ||
                new Type[]
                {
                    typeof(String),
                    typeof(Decimal),
                    typeof(DateTime),
                    typeof(DateTimeOffset),
                    typeof(TimeSpan),
                    typeof(Guid)
                }.Contains(type) ||
                Convert.GetTypeCode(type) != TypeCode.Object;
        }

        public static INode CreateNode<S, T>(this IType<S, T> type, ObjectNode parent, string subpath,
          IEnvironment environment, object initialValue)
        {
            return (type as IType).CreateNode<S, T>(parent, subpath, environment, initialValue);
        }

        public static INode CreateNode<S, T>(this IType type, ObjectNode parent, string subpath,
            IEnvironment environment, object initialValue)
        {
            return type.CreateNode<S, T>(parent, subpath, environment, initialValue, (_) => (T)_);
        }

        public static INode CreateNode<S, T>(this IType<S, T> type, ObjectNode parent, string subpath,
        IEnvironment environment, object initialValue, Func<object, T> create, Action<INode, object> finalize = null)
        {
            return (type as IType).CreateNode<S, T>(parent, subpath, environment, initialValue, create, finalize);
        }

        public static INode CreateNode<S, T>(this IType type, ObjectNode parent, string subpath, IEnvironment environment,
            object initialValue, Func<object, T> create, Action<INode, object> finalize = null)
        {
            if (initialValue.IsStateTreeNode())
            {
                var targetNode = initialValue.GetStateTreeNode();
                if (!targetNode.IsRoot)
                {
                    throw new Exception($"Cannot add an object to a state tree if it is already part of the same or another state tree. Tried to assign an object to '{parent?.Path ?? ""}/{subpath}', but it lives already at '{targetNode.Path}'");
                }
                targetNode.SetParent(parent, subpath);
                return targetNode;
            }

            var storedValue = create(initialValue);

            if (type.ShouldAttachNode)
            {
                var node = new ObjectNode(
                    type,
                    parent,
                    subpath,
                    environment,
                    initialValue,
                    storedValue,
                    type.ShouldAttachNode,
                    finalize
                );
                node.FinalizeCreation();
                return node;
            }

            return new ScalarNode(
                type,
                parent,
                subpath,
                environment,
                initialValue,
                storedValue,
                type.ShouldAttachNode,
                finalize);
        }
    }
}
