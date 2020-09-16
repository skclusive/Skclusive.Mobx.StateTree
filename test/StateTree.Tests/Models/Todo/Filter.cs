using Skclusive.Mobx.Observable;
using Skclusive.Mobx.StateTree;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Skclusive.Mobx.StateTree.Tests
{
    public enum Filter : int
    {
        None = 0,

        Active = 1,

        Completed = 2,

        All = 3
    }

    public partial class TestTypes
    {
        public readonly static IType<Filter, Filter> FilterType = Types.Enumeration("Filter", Filter.None, Filter.Active, Filter.Completed, Filter.All);
    }
}
