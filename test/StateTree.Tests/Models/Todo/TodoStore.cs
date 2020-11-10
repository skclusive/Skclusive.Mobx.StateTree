using Skclusive.Mobx.Observable;
using Skclusive.Mobx.StateTree;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Skclusive.Mobx.StateTree.Tests
{
    #region ITodoStore

    public interface ITodoStoreSnapshot
    {
        ITodoSnapshot Todo { set; get; }

        Filter Filter { set; get; }

        ITodoSnapshot[] Todos { set; get; }
    }

    public interface ITodoStoreActions
    {
        void AddTodo(string title);

        void SetFilter(Filter filter);

        void Remove(ITodo todo);

        void CompleteAll();

        void ClearCompleted();
    }

    public interface ITodoStore : ITodoStoreActions
    {
        ITodo Todo { set; get; }

        IList<ITodo> Todos { set; get; }

        IList<ITodo> FilteredTodos { get; }

        int TotalCount { get; }

        int ActiveCount { get; }

        int CompletedCount { get; }

        Filter Filter { set; get; }
    }

    internal class TodoStoreSnapshot : ITodoStoreSnapshot
    {
        public ITodoSnapshot Todo { set; get; }

        public Filter Filter { set; get; }

        public ITodoSnapshot[] Todos { set; get; }
    }

    internal class TodoStoreProxy : ObservableProxy<ITodoStore, INode>, ITodoStore
    {
        public override ITodoStore Proxy => this;

        public TodoStoreProxy(IObservableObject<ITodoStore, INode> target) : base(target)
        {
        }

        public Filter Filter
        {
            get => Read<Filter>(nameof(Filter));
            set => Write(nameof(Filter), value);
        }

        public ITodo Todo
        {
            get => Read<ITodo>(nameof(Todo));
            set => Write(nameof(Todo), value);
        }

        public IList<ITodo> Todos
        {
            get => Read<IList<ITodo>>(nameof(Todos));
            set => Write(nameof(Todos), value);
        }

        public IList<ITodo> FilteredTodos => Read<IList<ITodo>>(nameof(FilteredTodos));

        public int TotalCount => Read<int>(nameof(TotalCount));

        public int ActiveCount => Read<int>(nameof(ActiveCount));

        public int CompletedCount => Read<int>(nameof(CompletedCount));

        public void AddTodo(string title)
        {
            (Target as dynamic).AddTodo(title);
        }

        public void Remove(ITodo todo)
        {
            (Target as dynamic).Remove(todo);
        }

        public void SetFilter(Filter filter)
        {
            (Target as dynamic).SetFilter(filter);
        }

        public void CompleteAll()
        {
            (Target as dynamic).CompleteAll();
        }

        public void ClearCompleted()
        {
            (Target as dynamic).ClearCompleted();
        }
    }

    #endregion

    public partial class TestTypes
    {
        public readonly static IObjectType<ITodoSnapshot, ITodo> TodoType = Types.
                        Object<ITodoSnapshot, ITodo>("Todo")
                       .Proxy(x => new TodoProxy(x))
                       .Snapshot(() => new TodoSnapshot())
                       .Mutable(o => o.Title, Types.String)
                       .Mutable(o => o.Done, Types.Boolean)
                       .Action(o => o.Toggle(), (o) => o.Done = !o.Done)
                       .Action(o => o.MakeUpper(), (o) => o.Title = o.Title?.ToUpper())
                       .Action<string>(o => o.Edit(null), (o, title) => o.Title = title)
                       .Action(o => o.Remove(), (o) => o.GetRoot<ITodoStore>().Remove(o));

        private readonly static IDictionary<Filter, Func<ITodo, bool>> FilterMapping = new Dictionary<Filter, Func<ITodo, bool>>
        {
            { Filter.All, (_) => true },
            { Filter.Active, (todo) => !todo.Done },
            { Filter.Completed, (todo) => todo.Done }
        };

        public readonly static IType<ITodoSnapshot[], IObservableList<INode, ITodo>> TodoListType = Types.List(TodoType);

        public readonly static IObjectType<ITodoStoreSnapshot, ITodoStore> TodoStoreType = Types.
                        Object<ITodoStoreSnapshot, ITodoStore>("Store")
                       .Proxy(x => new TodoStoreProxy(x))
                       .Snapshot(() => new TodoStoreSnapshot())
                       .Mutable(o => o.Todo, Types.Maybe(TodoType))
                       .Mutable(o => o.Todos, Types.List(TodoType))
                       .Mutable(o => o.Filter, FilterType)
                       .View(o => o.TotalCount, Types.Int, (o) => o.Todos.Count())
                       .View(o => o.CompletedCount, Types.Int, (o) => o.Todos.Where(t => t.Done).Count())
                       .View(o => o.FilteredTodos, Types.List(TodoType), (o) => o.Todos.Where(FilterMapping[o.Filter]).ToList())
                       .View(o => o.ActiveCount, Types.Int, (o) => o.TotalCount - o.CompletedCount)
                       .Action((o) => o.CompleteAll(), (o) => o.Todos.Select(todo => todo.Done = true).ToList())
                       .Action((o) => o.ClearCompleted(), (o) =>
                       {
                           foreach (var completed in o.Todos.Where(todo => todo.Done).ToArray())
                               o.Todos.Remove(completed);
                       })
                       .Action<Filter>((o) => o.SetFilter(Filter.None), (o, filter) => o.Filter = filter)
                       .Action<string>((o) => o.AddTodo(null), (o, title) => o.Todos.Insert(0, TodoType.Create(new TodoSnapshot { Title = title })))
                       .Action<ITodo>((o) => o.Remove(null), (o, x) => o.Todos.Remove(x));
    }
}
