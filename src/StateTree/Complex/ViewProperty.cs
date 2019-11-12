using System;
using System.Collections.Generic;

namespace Skclusive.Mobx.StateTree
{
    public class ViewProperty : IViewProperty
    {
        public string Name { set; get; }

        public Type Kind { set; get; }

        public IType Type { set; get; }

        public Func<object, object> View { set; get; }

        public bool Equals(IViewProperty other)
        {
            return EqualityComparer<string>.Default.Equals(Name, other?.Name);
        }

        public override bool Equals(object property)
        {
            return Equals(property as IViewProperty);
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
