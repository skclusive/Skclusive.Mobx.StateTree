using System;
using System.Collections.Generic;
using Skclusive.Mobx.Observable;

namespace Skclusive.Mobx.StateTree
{
    public class ObjectTypeConfig<S, T> : IObjectTypeConfig<S, T>
    {
        public string Name { set; get; }

        public Func<object, object> PreProcessor { set; get; }

        public Func<IObservableObject<T, INode>, T> Proxify { set; get; }

        public Func<S> Snapshoty { set;  get; }

        public IReadOnlyDictionary<Hook, List<Action<object[]>>> Hooks { set; get; } = new Dictionary<Hook, List<Action<object[]>>>();

        public IReadOnlyCollection<Func<object, object>> Initializers { set; get; } = new List<Func<object, object>>();

        public IReadOnlyCollection<IMutableProperty> Mutables { set; get; } = new List<IMutableProperty>();

        public IReadOnlyCollection<IVolatileProperty> Volatiles { set; get; } = new List<IVolatileProperty>();

        public IReadOnlyCollection<IViewProperty> Views { set; get; } = new List<IViewProperty>();

        public IReadOnlyCollection<IActionProperty> Actions { set; get; } = new List<IActionProperty>();
    }
}
