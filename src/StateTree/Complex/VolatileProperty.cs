using System;
using System.Collections.Generic;

namespace Skclusive.Mobx.StateTree
{
    public class VolatileProperty : IVolatileProperty
    {
        public string Name { set; get; }

        public Type Kind { set; get; }

        public object Default { set; get; }

        public bool Equals(IVolatileProperty other)
        {
            return EqualityComparer<string>.Default.Equals(Name, other?.Name);
        }

        public override bool Equals(object property)
        {
            return Equals(property as IVolatileProperty);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
