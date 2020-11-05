using System;

namespace Skclusive.Mobx.StateTree
{
    public interface IVolatileProperty : IEquatable<IVolatileProperty>
    {
        string Name { get; }

        Type Kind { get; }

        object Default { get; }
    }
}
