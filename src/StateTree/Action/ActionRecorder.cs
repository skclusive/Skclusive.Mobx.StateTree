using System;
using System.Collections.Generic;
using System.Linq;

namespace Skclusive.Mobx.StateTree
{
    public class ActionRecorder<T> : IActionRecorder<T> where T : class
    {
        public ActionRecorder(T target, bool eager = true)
        {
            _Target = target;

            if (eager)
            {
                Start();
            }
        }

        private T _Target { set; get; }

        private IDisposable _Disposable;

        private IList<ISerializedActionCall> _Actions = new List<ISerializedActionCall>();

        public IEnumerable<ISerializedActionCall> Actions => _Actions;

        public IActionRecorder<T> Start()
        {
            if (_Disposable != null)
            {
                return this;
            }

            _Disposable = _Target.OnAction(action => _Actions.Add(action));

            return this;
        }

        public IActionRecorder<T> Stop(bool reset = false)
        {
            _Dispose(reset);

            return this;
        }

        public IActionRecorder<T> Replay(T target = null)
        {
            var retarget = target ?? _Target;

            retarget.ApplyAction(Actions.ToArray());

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
                _Actions.Clear();
            }
        }

        public void Dispose()
        {
            _Dispose();
        }
    }
}
