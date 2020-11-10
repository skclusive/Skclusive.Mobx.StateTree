using System;
using System.Collections.Generic;
using Skclusive.Mobx.StateTree;
using Xunit;
using static Skclusive.Mobx.StateTree.Tests.TestTypes;

namespace Skclusive.Mobx.StateTree.Tests
{
    public class TestTodoStore
    {
        [Fact]
        public void TestStore()
        {
            var store = TodoStoreType.Create(new TodoStoreSnapshot
            {
                Filter = Filter.All,

                Todos = new ITodoSnapshot[]
                {
                    new TodoSnapshot { Title = "Get coffee" }
                }
            });

            Assert.NotNull(store);

            Assert.Equal(Filter.All, store.Filter);

            Assert.Equal("Get coffee", store.Todos[0].Title);

            store.Todos[0].Edit("Get Filter Coffee");

            Assert.Equal("Get Filter Coffee", store.Todos[0].Title);

            store.Todos[0].Toggle();

            Assert.True(store.Todos[0].Done);

            store.Todos[0].Remove();

            Assert.Empty(store.Todos);

            Assert.Equal(0, store.TotalCount);
        }

        [Fact]
        public void TestTodoCounts()
        {
            var store = TodoStoreType.Create(new TodoStoreSnapshot
            {
                Filter = Filter.All,

                Todos = new ITodoSnapshot[]
                {
                    new TodoSnapshot { Title = "Get coffee", Done = true },

                    new TodoSnapshot { Title = "Learn Blazor" }
                }
            });

            Assert.Equal(2, store.FilteredTodos.Count);

            Assert.Equal(2, store.TotalCount);

            Assert.Equal(1, store.ActiveCount);

            Assert.Equal(1, store.CompletedCount);

            store.Todos[1].Toggle();

            Assert.Equal(2, store.FilteredTodos.Count);

            store.SetFilter(Filter.Active);

            Assert.Equal(Filter.Active, store.Filter);

            Assert.Equal(0, store.FilteredTodos.Count);

            store.SetFilter(Filter.Completed);

            Assert.Equal(Filter.Completed, store.Filter);

            Assert.Equal(2, store.FilteredTodos.Count);

            Assert.Equal(0, store.ActiveCount);

            Assert.Equal(2, store.CompletedCount);
        }

        [Fact]
        public void TestTodoCompleteAll()
        {
            var store = TodoStoreType.Create(new TodoStoreSnapshot
            {
                Filter = Filter.All,

                Todos = new ITodoSnapshot[]
                {
                    new TodoSnapshot { Title = "Get coffee" },

                    new TodoSnapshot { Title = "Learn Blazor" }
                }
            });

            Assert.Equal(2, store.FilteredTodos.Count);

            Assert.Equal(2, store.ActiveCount);

            Assert.Equal(0, store.CompletedCount);

            store.CompleteAll();

            store.SetFilter(Filter.Completed);

            Assert.Equal(Filter.Completed, store.Filter);

            Assert.Equal(2, store.FilteredTodos.Count);

            Assert.Equal(0, store.ActiveCount);

            Assert.Equal(2, store.CompletedCount);
        }

        [Fact]
        public void TestTodoClearCompleted()
        {
            var store = TodoStoreType.Create(new TodoStoreSnapshot
            {
                Filter = Filter.All,

                Todos = new ITodoSnapshot[]
                {
                    new TodoSnapshot { Title = "Get coffee" },

                    new TodoSnapshot { Title = "Learn Blazor" }
                }
            });

            Assert.Equal(2, store.FilteredTodos.Count);

            Assert.Equal(2, store.ActiveCount);

            Assert.Equal(0, store.CompletedCount);

            store.Todos[0].Toggle();

            Assert.Equal(1, store.ActiveCount);

            Assert.Equal(1, store.CompletedCount);

            store.ClearCompleted();

            Assert.Equal(1, store.FilteredTodos.Count);

            Assert.Equal(1, store.ActiveCount);

            Assert.Equal(0, store.CompletedCount);

            Assert.Equal("Learn Blazor", store.Todos[0].Title);
        }

        [Fact]
        public void TestAddTodo()
        {
            var store = TodoStoreType.Create(new TodoStoreSnapshot
            {
                Filter = Filter.All,

                Todos = new ITodoSnapshot[]
                {
                    new TodoSnapshot { Title = "Get coffee" }
                }
            });

            Assert.Equal(1, store.TotalCount);

            store.AddTodo("Learn Blazor");

            Assert.Equal(2, store.TotalCount);

            Assert.Equal("Learn Blazor", store.Todos[0].Title);

            Assert.Equal("Get coffee", store.Todos[1].Title);
        }

        [Fact]
        public void TestEditTodo()
        {
            var store = TodoStoreType.Create(new TodoStoreSnapshot
            {
                Filter = Filter.All,

                Todos = new ITodoSnapshot[]
                {
                    new TodoSnapshot { Title = "Get coffee" }
                }
            });

            Assert.Equal("Get coffee", store.Todos[0].Title);

            store.Todos[0].Edit("Learn Blazor");

            Assert.Equal(1, store.TotalCount);

            Assert.Equal("Learn Blazor", store.Todos[0].Title);

            store.Todos[0].Edit("Learn Blazor");

            Assert.Equal("Learn Blazor", store.Todos[0].Title);
        }

        [Fact]
        public void TestNoEditTodo()
        {
            var store = TodoStoreType.Create(new TodoStoreSnapshot
            {
                Filter = Filter.All,

                Todos = new ITodoSnapshot[]
                {
                    new TodoSnapshot { Title = "Get coffee" }
                }
            });

            Assert.Equal("Get coffee", store.Todos[0].Title);

            store.Todos[0].Edit("Learn Blazor");

            store.Todos[0].Edit("Learn Blazor");

            Assert.Equal("Learn Blazor", store.Todos[0].Title);
        }

        [Fact]
        public void TestOnAction()
        {
            var store = TodoType.Create(new TodoSnapshot { Title = "Get coffee" });

            var list = new List<string>();

            store.OnAction((ISerializedActionCall call) =>
            {
                var snapshot = store.GetSnapshot<TodoSnapshot>();

                list.Add(snapshot.Title);
            });

            store.Edit("Learn Blazor");

            Assert.Single(list);

            Assert.Equal("Learn Blazor", list[0]);
        }

        [Fact]
        public void TestOnAction2()
        {
            var store = TodoStoreType.Create(new TodoStoreSnapshot
            {
                Filter = Filter.All,

                Todos = new ITodoSnapshot[]
                {
                   new TodoSnapshot { Title = "Get coffee" }
                }
            });

            var list = new List<int>();

            store.OnAction((ISerializedActionCall call) =>
            {
                var snapshot = store.GetSnapshot<TodoStoreSnapshot>();

                list.Add(snapshot.Todos.Length);
            });

            store.AddTodo("Learn Blazor");

            Assert.Single(list);

            Assert.Equal(2, list[0]);
        }

        [Fact]
        public void TestOnAction3()
        {
            var store = TodoStoreType.Create(new TodoStoreSnapshot
            {
                Filter = Filter.All,

                Todos = new ITodoSnapshot[]
                {
                   new TodoSnapshot { Title = "Get coffee" }
                }
            });

            var list = new List<(int, string)>();

            store.OnAction((ISerializedActionCall call) =>
            {
                var snapshot = store.GetSnapshot<TodoStoreSnapshot>();

                list.Add((snapshot.Todos.Length, snapshot.Todos[0].Title));
            });

            store.Todos[0].Edit("Learn Blazor");

            Assert.Single(list);

            Assert.Equal(1, list[0].Item1);
            Assert.Equal("Learn Blazor", list[0].Item2);
        }

        [Fact]
        public void TestOnAction4()
        {
            var list = TodoListType.Create(new ITodoSnapshot[]
            {
                new TodoSnapshot { Title = "Get coffee" }
            });

            list.Unprotected();

            list.Insert(0, TodoType.Create(new TodoSnapshot { Title = "Learn Blazor" }));

            var snapshots = list.GetSnapshot<ITodoSnapshot[]>();

            Assert.Equal(2, snapshots.Length);
        }

        [Fact]
        public void TestDeadObjectAccessError()
        {
            var store = TodoStoreType.Create(new TodoStoreSnapshot { Todo = new TodoSnapshot { Title = "sk" }, Todos = new ITodoSnapshot[] { new TodoSnapshot { Title = "Naguvan" } } });

            Assert.NotNull(store);

            store.Unprotected();

            var todo = store.Todo;

            todo.MakeUpper();

            Assert.Equal("SK", todo.Title);

            var snapshots = new List<ITodoStoreSnapshot>();

            store.OnSnapshot<ITodoStoreSnapshot>(snapshot => snapshots.Add(snapshot));

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
    }
}
