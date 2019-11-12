using System;

namespace Skclusive.Mobx.StateTree
{
    public class Middleware : IMiddleware
    {
        public Action<IMiddlewareEvent, Action<IMiddlewareEvent, Func<object, object>>, Action<object>> Handler { set; get; }

        public bool IncludeHooks { set; get; }
    }
}
