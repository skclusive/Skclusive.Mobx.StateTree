using System;

namespace Skclusive.Mobx.StateTree
{
    public interface IMutableProperty : IEquatable<IMutableProperty>
    {
        string Name { get; }

        Type Kind { get; }

        IType Type { get; }

        object Default { get; }
    }
}
