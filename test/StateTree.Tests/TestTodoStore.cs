using Skclusive.Mobx.Observable;
using System;
using System.Collections.Generic;
using Xunit;

namespace Skclusive.Mobx.StateTree.Tests
{
    #region ITodo

    public interface ITodoProps
    {
        string Title { set; get; }

        bool Done { set; get; }
    }

    public interface ITodoActions
    {
        void MakeUpper();

        void Toggle();
    }

    public interface ITodo : ITodoProps, ITodoActions
    {
    }

    internal class TodoSnapshot : ITodoProps
    {
        public string Title { set; get; }

        public bool Done { set; get; }
    }

    internal class TodoProxy : ObservableProxy<ITodo, INode>, ITodo
    {
        public override ITodo Proxy => this;

        public TodoProxy(IObservableObject<ITodo, INode> target) : base(target)
        {
        }

        public string Title
        {
            get => Read<string>(nameof(Title));
            set => Write(nameof(Title), value);
        }

        public bool Done
        {
            get => Read<bool>(nameof(Done));
            set => Write(nameof(Done), value);
        }

        public void Toggle()
        {
            (Target as dynamic).Toggle();
        }

        public void MakeUpper()
        {
            (Target as dynamic).MakeUpper();
        }
    }

    #endregion

    #region IStore

    public interface IStoreProps
    {
        ITodoProps Todo { set; get; }

        ITodoProps[] Todos { set; get; }
    }

    public interface IStoreActions
    {
    }

    public interface IStore : IStoreActions
    {
        ITodo Todo { set; get; }

        IList<ITodo> Todos { set; get; }
    }

    internal class StoreSnapshot : IStoreProps
    {
        public ITodoProps Todo { set; get; }

        public ITodoProps[] Todos { set; get; }
    }

    internal class StoreProxy : ObservableProxy<IStore, INode>, IStore
    {
        public override IStore Proxy => this;

        public StoreProxy(IObservableObject<IStore, INode> target) : base(target)
        {
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
    }

    #endregion

    public class TestTodoStore
    {
        private static IObjectType<ITodoProps, ITodo> TodoType = Types.
                        Object<ITodoProps, ITodo>("Todo")
                       .Proxy(x => new TodoProxy(x))
                       .Snapshot(() => new TodoSnapshot())
                       .Mutable(o => o.Title, Types.String)
                       .Mutable(o => o.Done, Types.Boolean)
                       .Action(o => o.MakeUpper(), (o) => o.Title = o.Title?.ToUpper())
                       .Action(o => o.Toggle(), (o) => o.Done = !o.Done);

        private static IObjectType<IStoreProps, IStore> StoreType = Types.
                        Object<IStoreProps, IStore>("Store")
                       .Proxy(x => new StoreProxy(x))
                       .Snapshot(() => new StoreSnapshot())
                       .Mutable(o => o.Todo, Types.Maybe(TodoType))
                       .Mutable(o => o.Todos, Types.List(TodoType));

        #region Computed Tests

        [Fact]
        public void TestDeadObjectAccessError()
        {
            var store = StoreType.Create(new StoreSnapshot { Todo = new TodoSnapshot { Title = "sk" }, Todos = new ITodoProps[] { new TodoSnapshot { Title = "Naguvan" } } });

            Assert.NotNull(store);

            store.Unprotected();

            var todo = store.Todo;

            todo.MakeUpper();

            Assert.Equal("SK", todo.Title);

            var snapshots = new List<IStoreProps>();

            store.OnSnapshot<IStoreProps>(snapshot => snapshots.Add(snapshot));

            store.Todos[0].Toggle();

            Assert.Equal("Naguvan", store.Todos[0].Title);

            Assert.True(store.Todos[0].Done);

            Assert.Single(snapshots);

            Assert.Equal("Naguvan", snapshots[0].Todos[0].Title);

            Assert.True(snapshots[0].Todos[0].Done);

            store.Todo = TodoType.Create(new TodoSnapshot { Title = "naguvan" });

            Assert.Equal("naguvan", store.Todo.Title);

            var ex1 = Assert.Throws<Exception>(() => todo.Title);

            Assert.Equal("You are trying to read or write to an object that is no longer part of a state tree. (Object type was 'Todo').", ex1.Message);

            var ex2 = Assert.Throws<Exception>(() => todo.MakeUpper());

            Assert.Equal("You are trying to read or write to an object that is no longer part of a state tree. (Object type was 'Todo').", ex2.Message);
        }

        #endregion
    }
}
