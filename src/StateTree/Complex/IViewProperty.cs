using System;

namespace Skclusive.Mobx.StateTree
{
    public interface IViewProperty : IEquatable<IViewProperty>
    {
        string Name { get; }

        Type Kind { get; }

        IType Type { get; }

        Func<object, object> View { get; }
    }
}
