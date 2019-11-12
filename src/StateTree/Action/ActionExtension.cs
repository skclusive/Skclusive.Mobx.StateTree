using Skclusive.Mobx.Observable;
using System;
using System.Linq;

namespace Skclusive.Mobx.StateTree
{
    public static class ActionExtension
    {
        public static IDisposable OnAction(this object target, Action<ISerializedActionCall> listener, bool attachAfter = true)
        {
            if (!target.IsStateTreeNode())
            {
                throw new InvalidOperationException("Can not listen for action on Non State Tree Node");
            }

            var node = target.GetStateTreeNode();

            void FireListener(IMiddlewareEvent call)
            {
                if (call.Type == MiddlewareEventType.Action && call.Id == call.RootId)
                {
                    var source = call.Context.GetStateTreeNode();

                    var data = new SerializedActionCall
                    {
                        Name = call.Name,

                        Path = StateTreeUtils.GetRelativePathBetweenNodes(node, source),

                        //TODO: serialize arguments
                        Arguments = call.Arguments.ToArray()
                    };

                    listener(data);
                }
            }

            Action<IMiddlewareEvent, Action<IMiddlewareEvent, Func<object, object>>, Action<object>> onAfterAction = (IMiddlewareEvent call, Action<IMiddlewareEvent, Func<object, object>> next, Action<object> action) =>
            {
                next(call, null);

                FireListener(call);
            };

            Action<IMiddlewareEvent, Action<IMiddlewareEvent, Func<object, object>>, Action<object>> onBeforeAction = (IMiddlewareEvent call, Action<IMiddlewareEvent, Func<object, object>> next, Action<object> action) =>
            {
                FireListener(call);

                next(call, null);
            };

            return StateTreeAction.AddMiddleware(target.GetStateTree(), attachAfter ? onAfterAction : onBeforeAction);
        }

        public static object[] ApplyAction(this object target, params ISerializedActionCall[] calls)
        {
            return calls.Select(call => target.ApplyAction(call)).ToArray();
        }

        public static object ApplyAction(this object target, ISerializedActionCall call)
        {
            if (!target.IsStateTreeNode())
            {
                throw new InvalidOperationException("Can not listen for action on Non State Tree Node");
            }

            var resolved = target.TryResolve(call.Path ?? "");

            if (resolved == null)
            {
                throw new InvalidOperationException($"Invalid action path: {call.Path}");
            }

            if (call.Name == "@APPLY_PATCHES")
            {
                resolved.ApplyPatch((IJsonPatch[])call.Arguments[0]);
            }

            if (call.Name == "@APPLY_SNAPSHOT")
            {
                resolved.ApplySnapshot(call.Arguments[0]);
            }

            var node = resolved.GetStateTreeNode();

            if (resolved is IObservableObject observable)
            {
                var arguments = call.Arguments.ToArray();
                if (observable.TryInvokeAction(call.Name, arguments, out object result))
                {
                    return result;
                }
            }

            throw new InvalidOperationException($"Not able to apply the action: {call.Name} on target: {target} on path: {call.Path}");
        }
    }
}
