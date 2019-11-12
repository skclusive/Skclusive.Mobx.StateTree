using System;
using System.Collections.Generic;
using System.Linq;

namespace Skclusive.Mobx.StateTree
{
    public class PatchRecorder<T> : IPatchRecorder<T> where T : class
    {
        public PatchRecorder(T target, bool eager = true)
        {
            _Target = target;

            if (eager)
            {
                Start();
            }
        }

        private T _Target { set; get; }

        private IDisposable _Disposable;

        private IList<IJsonPatch> _Patches = new List<IJsonPatch>();

        private IList<IJsonPatch> _InversePatches = new List<IJsonPatch>();

        public IEnumerable<IJsonPatch> Patches => _Patches;

        public IEnumerable<IJsonPatch> InversePatches => _InversePatches;

        public IPatchRecorder<T> Start()
        {
            if (_Disposable != null)
            {
                return this;
            }

            _Disposable = _Target.OnPatch((patch, inversePatch) =>
            {
                _Patches.Add(patch);

                _InversePatches.Add(inversePatch);
            });

            return this;
        }

        public IPatchRecorder<T> Stop(bool reset = false)
        {
            _Dispose(reset);

            return this;
        }

        public IPatchRecorder<T> Replay(T target = null)
        {
            var retarget = target ?? _Target;

            retarget.ApplyPatch(Patches.ToArray());

            return this;
        }

        public IPatchRecorder<T> Undo(T target = null)
        {
            var retarget = target ?? _Target;

            retarget.ApplyPatch(InversePatches.Reverse().ToArray());

            return this;
        }

        private void _Dispose(bool reset = true)
        {
            if (_Disposable != null)
            {
                _Disposable.Dispose();
                _Disposable = null;
            }

            if (reset)
            {
                _Patches.Clear();
                _InversePatches.Clear();
            }
        }

        public void Dispose()
        {
            _Dispose();
        }
    }
}
