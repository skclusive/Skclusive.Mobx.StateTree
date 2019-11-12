using System;
using System.Collections.Generic;

namespace Skclusive.Mobx.StateTree
{
    public interface IPatchRecorder<T> : IDisposable where T : class
    {
        IEnumerable<IJsonPatch> Patches { get; }

        IEnumerable<IJsonPatch> InversePatches { get; }

        IPatchRecorder<T> Start();

        IPatchRecorder<T> Stop(bool reset = false);

        IPatchRecorder<T> Replay(T target = null);

        IPatchRecorder<T> Undo(T target = null);
    }
}
