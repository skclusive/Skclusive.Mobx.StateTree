Skclusive.Mobx.StateTree
=============================

Port of [MobX-State-Tree](https://github.com/mobxjs/mobx-state-tree) for the C# language.

> Supercharge the state-management in your Blazor apps with Transparent Functional Reactive Programming (TFRP)

# Philosophy & Overview

`mobx-state-tree` is a state container that combines the _simplicity and ease of mutable data_ with the _traceability of immutable data_ and the _reactiveness and performance of observable data_.

Simply put, mobx-state-tree tries to combine the best features of both immutability (transactionality, traceability and composition) and mutability (discoverability, co-location and encapsulation) based approaches to state management; everything to provide the best developer experience possible.
Unlike MobX itself, mobx-state-tree is very opinionated about how data should be structured and updated.
This makes it possible to solve many common problems out of the box.

Central in MST (mobx-state-tree) is the concept of a _living tree_. The tree consists of mutable, but strictly protected objects enriched with _runtime type information_. In other words, each tree has a _shape_ (type information) and _state_ (data).
From this living tree, immutable, structurally shared, snapshots are automatically generated.

```C#
public interface ITodoProps
{
    string Title { set; get; }
    bool Done { set; get; }
}

public interface ITodoActions
{
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
}

public interface IStoreProps
{
    ITodoProps[] Todos { set; get; }
}

public interface IStoreActions
{
}

public interface IStore : IStoreActions
{
    IList<ITodo> Todos { set; get; }
}

internal class StoreSnapshot : IStoreProps
{
    public ITodoProps[] Todos { set; get; }
}

internal class StoreProxy : ObservableProxy<IStore, INode>, IStore
{
    public override IStore Proxy => this;

    public StoreProxy(IObservableObject<IStore, INode> target) : base(target)
    {
    }

    public IList<ITodo> Todos
    {
        get => Read<IList<ITodo>>(nameof(Todos));
        set => Write(nameof(Todos), value);
    }
}

IObjectType<ITodoProps, ITodo> TodoType = Types.
                        Object<ITodoProps, ITodo>("Todo")
                       .Proxy(x => new TodoProxy(x))
                       .Snapshot(() => new TodoSnapshot())
                       .Mutable(o => o.Title, Types.String)
                       .Mutable(o => o.Done, Types.Boolean)
                       .Action(o => o.Toggle(), (o) => o.Done = !o.Done);

IObjectType<IStoreProps, IStore> StoreType = Types.
                        Object<IStoreProps, IStore>("Store")
                       .Proxy(x => new StoreProxy(x))
                       .Snapshot(() => new StoreSnapshot())
                       .Mutable(o => o.Todos, Types.List(TodoType));

// create an instance from a snapshot
var store = StoreType.Create(Todos = new ITodoProps[]
{
    new TodoSnapshot { Title = "Naguvan" }
});

// listen to new snapshots
store.OnSnapshot<IStoreProps>(snapshot => System.Console.WriteLine(snapshot));

// invoke action that modifies the tree
store.Todos[0].Toggle();
// prints: `{ Todos: [{ Title: "Get coffee", Done: true }]}`
```

# Samples Projects

Sample projects using Skclusive.Mobx.StateTree are availabe in [Skclusive.Blazor.Samples](https://github.com/skclusive/Skclusive.Blazor.Samples) repository.

## TodoApp Sample

The Blazor TodoApp sample project has been hosted [here](https://skclusive.github.io/Skclusive.Blazor.Samples/TodoApp/), which integrates with redux devtool.

![Blazor TodoApp](https://github.com/skclusive/Skclusive.Blazor.Samples/raw/master/images/todo-app.gif)

## FlightFinder Sample

The Blazor FlightFinder sample project has been hosted [here](https://skclusive.github.io/Skclusive.Blazor.Samples/FlightFinder/), which integrates with redux devtool.

![Blazor FlightFinder](https://github.com/skclusive/Skclusive.Blazor.Samples/raw/master/images/flight-finder.gif)

### Installation

Add a reference to the library from [![NuGet](https://img.shields.io/nuget/v/Skclusive.Mobx.StateTree.svg)](https://www.nuget.org/packages/Skclusive.Mobx.StateTree/)

## Credits

This is an attempt to port [mobx-state-tree](https://github.com/mobxjs/mobx-state-tree) to dotnet-standard C# libaray.

## License

Skclusive.Mobx.StateTree is licensed under [MIT license](http://www.opensource.org/licenses/mit-license.php)
