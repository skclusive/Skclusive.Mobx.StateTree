using System;

namespace Skclusive.Mobx.StateTree
{
    public interface IMiddleware
    {
        Action<IMiddlewareEvent, Action<IMiddlewareEvent, Func<object, object>>, Action<object>> Handler { set; get; }

        bool IncludeHooks { set; get; }
    }
}
