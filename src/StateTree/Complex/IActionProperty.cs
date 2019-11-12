using System;

namespace Skclusive.Mobx.StateTree
{
    public interface IActionProperty : IEquatable<IActionProperty>
    {
        string Name { get; }

        Type Kind { get; }

        IType Type { get; }

        Func<object[], object> Action { get; }
    }
}
