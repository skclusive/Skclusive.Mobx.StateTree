using Skclusive.Mobx.Observable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Skclusive.Mobx.StateTree
{
    public static class StateTreeAction
    {
        #region Core Actions

        private static readonly ThreadLocal<IMiddlewareEvent> CurrentActionContext = new ThreadLocal<IMiddlewareEvent>();

        public static int NextActionId = 1;

        public static int GetNextActionId()
        {
            return NextActionId++;
        }

        public static IMiddlewareEvent GetCurrentActionContext()
        {
            if (!CurrentActionContext.IsValueCreated)
            {
                throw new Exception("Not running an action!");
            }

            return CurrentActionContext.Value;
        }

        public static IDisposable AddMiddleware(IStateTreeNode target, Action<IMiddlewareEvent, Action<IMiddlewareEvent, Func<object, object>>, Action<object>> handler, bool includeHooks = true)
        {
            var node = target.GetStateTreeNode();

            if (!node.IsProtectionEnabled)
            {
                Console.WriteLine("It is recommended to protect the state tree before attaching action middleware, as otherwise it cannot be guaranteed that all changes are passed through middleware. See `protect`");
            }

            return node.AddMiddleware(handler, includeHooks);
        }

        public static Func<object[], object> CreateActionInvoker(object target, string name, Func<object[], object> action)
        {
            return (object[] arguments) =>
            {
                if (NodeCache.TryGetValue(target, out IStateTreeNode node))
                {
                    var id = GetNextActionId();

                    var currentActionContext = CurrentActionContext.IsValueCreated ? CurrentActionContext.Value : null;

                    var context = new MiddlewareEvent
                    {
                        Type = MiddlewareEventType.Action,

                        Name = name,

                        Id = id,

                        Arguments = arguments,

                        Context = node,

                        Target = target,

                        Tree = node.GetRoot(),

                        RootId = currentActionContext?.RootId ?? id,

                        ParentId = currentActionContext?.Id ?? 0
                    };

                    return RunWithActionContext(context, action);
                }

                throw new Exception($"Target does have associated node {target}");
            };
        }

        public static object RunWithActionContext(IMiddlewareEvent context, Func<object[], object> action)
        {
            var node = context.Context.GetStateTreeNode();

            var baseIsRunningAction = node._IsRunningAction;

            var prevContext = CurrentActionContext.Value;

            if (context.Type == MiddlewareEventType.Action)
            {
                node.AssertAlive();
            }

            node._IsRunningAction = true;
            CurrentActionContext.Value = context;

            try
            {
                return RunMiddlewares(node, context, action);
            }
            finally
            {
                CurrentActionContext.Value = prevContext;
                node._IsRunningAction = baseIsRunningAction;
            }
        }

        public static IEnumerable<IMiddleware> CollectMiddlewares(ObjectNode node)
        {
            while (node != null)
            {
                foreach (var middleware in node.Middlewares)
                {
                    yield return middleware;
                }

                node = node.Parent;
            }
        }

        public static object RunMiddlewares(ObjectNode node, IMiddlewareEvent baseCall, Func<object[], object> action)
        {
            IEnumerator<IMiddleware> middlewares = CollectMiddlewares(node).GetEnumerator();

            object result = null;

            bool nextInvoked = false;

            bool abortInvoked = false;

            return RunMiddlewares(baseCall);

            object RunMiddlewares(IMiddlewareEvent call)
            {
                IMiddleware middleware = middlewares.MoveNext() ? middlewares.Current : null;

                void Next(IMiddlewareEvent ncall, Func<object, object> callback)
                {
                    nextInvoked = true;
                    // the result can contain
                    // - the non manipulated return value from an action
                    // - the non manipulated abort value
                    // - one of the above but manipulated through the callback function
                    if (callback != null)
                    {
                        result = callback(RunMiddlewares(ncall) ?? result);
                    }
                    else
                    {
                        result = RunMiddlewares(ncall);
                    }
                }

                void Abort(object value)
                {
                    abortInvoked = true;
                    // overwrite the result
                    // can be manipulated through middlewares earlier in the queue using the callback fn
                    result = value;
                }

                object InvokeHandler()
                {
                    middleware.Handler(call, Next, Abort);
                    var xnode = call.Tree.GetStateTreeNode();
                    if (!nextInvoked && !abortInvoked)
                    {
                        Console.WriteLine($"Neither the next() nor the abort() callback within the middleware {middleware.Handler.ToString()} for the action: '{call.Name}' on the node: {xnode.Type.Name} was invoked.");
                    }
                    if (nextInvoked && abortInvoked)
                    {
                        Console.WriteLine($"The next() and abort() callback within the middleware {middleware.Handler.ToString()} for the action: '{call.Name}' on the node: ${node.Type.Name} were invoked.");
                    }
                    return result;
                }

                if (middleware?.Handler != null)
                {
                    if (middleware.IncludeHooks)
                    {
                        return InvokeHandler();
                    }
                    else
                    {
                        if (Enum.TryParse(call.Name, out Hooks hook))
                        {
                            return RunMiddlewares(call);
                        }
                        else
                        {
                            return InvokeHandler();
                        }
                    }
                } else
                {
                    return Actions.RunInAction(call.Name, action, new object[] { call.Target }.Concat(call.Arguments).ToArray());
                }
            }
        }

        #endregion

        #region Func WrapActionInvokers

        public static Func<object[], object> SkipTargetActionInvoker(object target, string name, Func<object[], object> action)
        {
            return CreateActionInvoker(target, name, (arguments) =>
            {
                return action(arguments.Skip(1).ToArray());
            });
        }

        public static Func<object[], object> WrapActionInvoker<R>(object target, string name, Func<R> action)
        {
            return SkipTargetActionInvoker(target, name, action.Pack());
        }

        public static Func<object[], object> WrapActionInvoker<A1, R>(object target, string name, Func<A1, R> action)
        {
            return SkipTargetActionInvoker(target, name, action.Pack());
        }

        public static Func<object[], object> WrapActionInvoker<A1, A2, R>(object target, string name, Func<A1, A2, R> action)
        {
            return SkipTargetActionInvoker(target, name, action.Pack());
        }

        public static Func<object[], object> WrapActionInvoker<A1, A2, A3, R>(object target, string name, Func<A1, A2, A3, R> action)
        {
            return SkipTargetActionInvoker(target, name, action.Pack());
        }

        public static Func<object[], object> WrapActionInvoker<A1, A2, A3, A4, R>(object target, string name, Func<A1, A2, A3, A4, R> action)
        {
            return SkipTargetActionInvoker(target, name, action.Pack());
        }

        public static Func<object[], object> WrapActionInvoker<A1, A2, A3, A4, A5, R>(object target, string name, Func<A1, A2, A3, A4, A5, R> action)
        {
            return SkipTargetActionInvoker(target, name, action.Pack());
        }

        #endregion

        #region Func CreateActionInvoker

        public static Func<R> CreateActionInvoker<R>(object target, string name, Func<R> action)
        {
            return WrapActionInvoker(target, name, action).Unpack<R>();
        }

        public static Func<A1, R> CreateActionInvoker<A1, R>(object target, string name, Func<A1, R> action)
        {
            return WrapActionInvoker(target, name, action).Unpack<A1, R>();
        }

        public static Func<A1, A2, R> CreateActionInvoker<A1, A2, R>(object target, string name, Func<A1, A2, R> action)
        {
            return WrapActionInvoker(target, name, action).Unpack<A1, A2, R>();
        }

        public static Func<A1, A2, A3, R> CreateActionInvoker<A1, A2, A3, R>(object target, string name, Func<A1, A2, A3, R> action)
        {
            return WrapActionInvoker(target, name, action).Unpack<A1, A2, A3, R>();
        }

        public static Func<A1, A2, A3, A4, R> CreateActionInvoker<A1, A2, A3, A4, R>(object target, string name, Func<A1, A2, A3, A4, R> action)
        {
            return WrapActionInvoker(target, name, action).Unpack<A1, A2, A3, A4, R>();
        }

        public static Func<A1, A2, A3, A4, A5, R> CreateActionInvoker<A1, A2, A3, A4, A5, R>(object target, string name, Func<A1, A2, A3, A4, A5, R> action)
        {
            return WrapActionInvoker(target, name, action).Unpack<A1, A2, A3, A4, A5, R>();
        }

        #endregion

        #region Action WrapActionInvokers

        public static Action<object[]> SkipTargetActionInvoker(object target, string name, Action<object[]> action)
        {
            return CreateActionInvoker(target, name, (arguments) =>
            {
                action(arguments.Skip(1).ToArray());
            });
        }

        public static Action<object[]> WrapActionInvoker(object target, string name, Action action)
        {
            return SkipTargetActionInvoker(target, name, action.Pack());
        }

        public static Action<object[]> WrapActionInvoker<A1>(object target, string name, Action<A1> action)
        {
            return SkipTargetActionInvoker(target, name, action.Pack());
        }

        public static Action<object[]> WrapActionInvoker<A1, A2>(object target, string name, Action<A1, A2> action)
        {
            return SkipTargetActionInvoker(target, name, action.Pack());
        }

        public static Action<object[]> WrapActionInvoker<A1, A2, A3, A4>(object target, string name, Action<A1, A2, A3, A4> action)
        {
            return SkipTargetActionInvoker(target, name, action.Pack());
        }

        public static Action<object[]> WrapActionInvoker<A1, A2, A3>(object target, string name, Action<A1, A2, A3> action)
        {
            return SkipTargetActionInvoker(target, name, action.Pack());
        }

        public static Action<object[]> WrapActionInvoker<A1, A2, A3, A4, A5>(object target, string name, Action<A1, A2, A3, A4, A5> action)
        {
            return SkipTargetActionInvoker(target, name, action.Pack());
        }

        #endregion

        #region Action CreateActionInvoker

        public static Action<object[]> CreateActionInvoker(object target, string name, Action<object[]> action)
        {
            var execute = CreateActionInvoker(target, name, (a) =>
            {
                action(a);
                return null;
            });

            return (object[] arguments) =>
            {
                execute(arguments);
            };
        }

        public static Action CreateActionInvoker(object target, string name, Action action)
        {
            return WrapActionInvoker(target, name, action).Unpack();
        }

        public static Action<A1> CreateActionInvoker<A1>(object target, string name, Action<A1> action)
        {
            return WrapActionInvoker(target, name, action).Unpack<A1>();
        }

        public static Action<A1, A2> CreateActionInvoker<A1, A2>(object target, string name, Action<A1, A2> action)
        {
            return WrapActionInvoker(target, name, action).Unpack<A1, A2>();
        }

        public static Action<A1, A2, A3> CreateActionInvoker<A1, A2, A3>(object target, string name, Action<A1, A2, A3> action)
        {
            return WrapActionInvoker(target, name, action).Unpack<A1, A2, A3>();
        }

        public static Action<A1, A2, A3, A4> CreateActionInvoker<A1, A2, A3, A4>(object target, string name, Action<A1, A2, A3, A4> action)
        {
            return WrapActionInvoker(target, name, action).Unpack<A1, A2, A3, A4>();
        }

        public static Action<A1, A2, A3, A4, A5> CreateActionInvoker<A1, A2, A3, A4, A5>(object target, string name, Action<A1, A2, A3, A4, A5> action)
        {
            return WrapActionInvoker(target, name, action).Unpack<A1, A2, A3, A4, A5>();
        }

        #endregion
    }
}
