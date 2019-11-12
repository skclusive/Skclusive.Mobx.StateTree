using System;
using System.Collections.Generic;

namespace Skclusive.Mobx.StateTree
{
    public class ActionProperty : IActionProperty
    {
        public string Name { set; get; }

        public Type Kind { set; get; }

        public IType Type { set; get; }

        public Func<object[], object> Action { set; get; }

        public bool Equals(IActionProperty other)
        {
            return EqualityComparer<string>.Default.Equals(Name, other?.Name);
        }

        public override bool Equals(object property)
        {
            return Equals(property as IActionProperty);
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
