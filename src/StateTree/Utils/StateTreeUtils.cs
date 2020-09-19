using Skclusive.Mobx.Observable;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

        public static IContextEntry[] GetContextForPath(IContextEntry[] context, string path, IType type)
        {
            return context.Concat(new IContextEntry[] { new ContextEntry { Path = path, Type = type } }).ToArray();
        }

        public static object GetPropertyValue(object snapshot, string property)
        {
            if (snapshot is IDictionary dictionary)
            {
                return dictionary[property];
            }

            return GetPropertyInfo(snapshot, property)?.GetValue(snapshot);
        }

        public static void SetPropertyValue(object snapshot, string property, object value)
        {
            if (snapshot is IDictionary dictionary)
            {
                dictionary[property] = value;
            }
            else
            {
                GetPropertyInfo(snapshot, property).SetValue(snapshot, value);
            }
        }

        public static PropertyInfo GetPropertyInfo(object snapshot, string property)
        {
            if (snapshot == null)
            {
                return null;
            }

            var propInfo = snapshot.GetType().GetProperty(property);

            if (propInfo == null)
            {
                throw new InvalidOperationException($"Property {property} is not available on Snapshot type {snapshot.GetType().FullName}");
            }

            return propInfo;
        }
    }
}
