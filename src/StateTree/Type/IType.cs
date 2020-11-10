using Skclusive.Core.Collection;
using System;
using System.Collections.Generic;
using Skclusive.Mobx.Observable;

namespace Skclusive.Mobx.StateTree
{
    public interface IContextEntry
    {
        string Path { get; set; }

        IType Type { get; set; }
    }

    public interface IValidationError
    {
        IContextEntry[] Context { set; get; }

        object Value { set; get; }

        string Message { set; get; }
    }

    public interface IEnvironment
    {
    }

    public interface INode : IDisposable
    {
        IType Type { get; }

        object Value { get; }

        object Snapshot { get; }

        object StoredValue { get; }

        string Path { get; }

        bool IsRoot { get; }

        ObjectNode Parent { get; }

        ObjectNode Root { get; }

        IEnvironment Environment { get; }

        string Subpath { set; get; }

        bool IsAlive { get; }

        bool AutoUnbox { get; }

        void SetParent(ObjectNode newParent, string subpath);
    }

    public interface INode<S, T> : INode
    {
        new IType<S, T> Type { get; }

        new T Value { get; }

        new S Snapshot { get; }
    }

    public interface IStateTreeNode
    {
        object TreeNode { get; }
    }

    public enum TypeFlags
    {
        String = 1,
        Number = 2,
        Boolean = 4,
        Date = 8,
        Literal = 16,
        List = 32,
        Map = 64,
        Object = 128,
        Frozen = 256,
        Optional = 512,
        Reference = 1024,
        Identifier = 2048,
        Late = 4096,
        Refinement = 8192,
        Union = 16384,
        Null = 32768,
        Undefined = 65536
    }

    public interface ISnapshottable<S>
    {
    }

    public interface IType
    {
        string Name { set; get; }

        TypeFlags Flags { get; }

        bool IsType { get; }

        string Describe { get; }

        bool ShouldAttachNode { get; }

        object Type { get; }

        object SnapshotType { get; }

        bool Is(object thing);

        IValidationError[] Validate(object thing, IContextEntry[] context);

        // Internal api's
        INode Instantiate(INode parent, string subpath, IEnvironment environment, object initialValue);

        IMap<string, INode> InitializeChildNodes(INode node, object snapshot);

        INode Reconcile(INode current, object newValue);

        object Create(object snapshot, IEnvironment environment);

        object GetValue(INode node);

        object GetSnapshot(INode node, bool applyPostProcess);

        void ApplySnapshot(INode node, object snapshot);

        void ApplyPatchLocally(INode node, string subpath, IJsonPatch patch);

        IReadOnlyCollection<INode> GetChildren(INode node);

        INode GetChildNode(INode node, string key);

        IType GetChildType(string key);

        void RemoveChild(INode node, string subpath);

        bool IsAssignableFrom(IType type);
    }

    public interface IType<S, T> : IType
    {
        new T Type { get; }

        new S SnapshotType { get; }

        new T GetValue(INode node);

        new S GetSnapshot(INode node, bool applyPostProcess);

        T Create(S snapshot = default(S), IEnvironment environment = null);

        void ApplySnapshot(INode node, S snapshot);
    }

    public interface ISimpleType<T> : IType<T, T>
    {
    }

    public interface IComplexType<S, T> : IType<S, T> // where T : IStateTreeNode, ISnapshottable<S>
    {
    }

    public interface IIdentifierType<T> : IType<T, T>
    {
    }
}
