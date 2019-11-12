using Skclusive.Mobx.Observable;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Skclusive.Mobx.StateTree
{
    public interface IObjectType<S, T, I> : IComplexType<S, T>
    {
        IReadOnlyCollection<Func<object, object>> Initializers { get; }

        IReadOnlyDictionary<string, IType> Properties { get; }

        IReadOnlyCollection<IMutableProperty> Mutables { get; }

        IReadOnlyCollection<IViewProperty> Views { get; }

        IReadOnlyCollection<IActionProperty> Actions { get; }

        I Include<Sx, Tx>(IObjectType<Sx, Tx> type);

        I Named(string name);

        I Proxy(Func<IObservableObject<T, INode>, T> proxify);

        I Snapshot(Func<S> snpashoty);

        I PreProcessSnapshot(Func<object, S> fn);

        I Mutable<P>(Expression<Func<T, P>> expression, IType type, P defaultValue = default(P));

        I View<P>(Expression<Func<T, P>> expression, IType type, Func<T, P> view);

        I Action<R>(Expression<Action<T>> expression, Func<T, R> func);

        I Action<A1, R>(Expression<Action<T>> expression, Func<T, A1, R> func);

        I Action<A1, A2, R>(Expression<Action<T>> expression, Func<T, A1, A2, R> func);

        I Action<A1, A2, A3, R>(Expression<Action<T>> expression, Func<T, A1, A2, A3, R> func);

        I Action<A1, A2, A3, A4, R>(Expression<Action<T>> expression, Func<T, A1, A2, A3, A4, R> func);

        I Action(Expression<Action<T>> expression, Action<T> func);

        I Action<A1>(Expression<Action<T>> expression, Action<T, A1> func);

        I Action<A1, A2>(Expression<Action<T>> expression, Action<T, A1, A2> func);

        I Action<A1, A2, A3>(Expression<Action<T>> expression, Action<T, A1, A2, A3> func);

        I Action<A1, A2, A3, A4>(Expression<Action<T>> expression, Action<T, A1, A2, A3, A4> func);

        //IModelType<S, T> Volatile<TP>(Func<T, TP> fn); // where TP : IStateTreeNode, ISnapshottable<S>;

        //IModelType<S, T> Views<V>(Func<T, V> fn); // where V : IStateTreeNode, ISnapshottable<S>;

        //IModelType<S, T> Actions<A>(Func<T, A> fn);// where A : IStateTreeNode, ISnapshottable<S>, IReadOnlyDictionary<string, Action>;

        //IModelType<S, T> Extend<A, V, VS>(Func<T, IModelExtend<A, V, VS>> fn) where A : IReadOnlyDictionary<string, Action>;

        //IModelType<S, T> Props(IDictionary<string, IType> props);
    }

    public interface IObjectType<S, T> : IObjectType<S, T, IObjectType<S, T>>
    {
    }

    public interface IObjectType<T> : IObjectType<IDictionary<string, object>, T, IObjectType<T>>
    {
    }
}
