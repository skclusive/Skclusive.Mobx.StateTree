using Xunit;

namespace ClientSide.Models
{
    public class TestTodoStore
    {
        [Fact]
        public void TestStore()
        {
            var store = ModelTypes.StoreType.Create(new TodoStoreSnapshot
            {
                Filter = "ShowAll",

                Todos = new ITodoSnapshot[]
                {
                    new TodoSnapshot { Title = "Get coffee" }
                }
            });

            Assert.NotNull(store);

            Assert.Equal("ShowAll", store.Filter);

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
            var store = ModelTypes.StoreType.Create(new TodoStoreSnapshot
            {
                Filter = "ShowAll",

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

            store.SetFilter("ShowActive");

            Assert.Equal("ShowActive", store.Filter);

            Assert.Equal(0, store.FilteredTodos.Count);

            store.SetFilter("ShowCompleted");

            Assert.Equal("ShowCompleted", store.Filter);

            Assert.Equal(2, store.FilteredTodos.Count);

            Assert.Equal(0, store.ActiveCount);

            Assert.Equal(2, store.CompletedCount);
        }

        [Fact]
        public void TestTodoCompleteAll()
        {
            var store = ModelTypes.StoreType.Create(new TodoStoreSnapshot
            {
                Filter = "ShowAll",

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

            store.SetFilter("ShowCompleted");

            Assert.Equal("ShowCompleted", store.Filter);

            Assert.Equal(2, store.FilteredTodos.Count);

            Assert.Equal(0, store.ActiveCount);

            Assert.Equal(2, store.CompletedCount);
        }

        [Fact]
        public void TestTodoClearCompleted()
        {
            var store = ModelTypes.StoreType.Create(new TodoStoreSnapshot
            {
                Filter = "ShowAll",

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
    }
}
