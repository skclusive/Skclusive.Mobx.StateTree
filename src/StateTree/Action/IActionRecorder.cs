using System;
using System.Collections.Generic;

namespace Skclusive.Mobx.StateTree
{
    public interface IActionRecorder<T> : IDisposable
    {
        IEnumerable<ISerializedActionCall> Actions { get; }

        IActionRecorder<T> Start();

        IActionRecorder<T> Stop(bool reset = false);

        IActionRecorder<T> Replay(T target);
    }
}
