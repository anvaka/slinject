using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using slinject.Native.Debug.Interfaces;

namespace slinject.Debugger.Wrappers
{
    public class DebugThread
    {
        private ICorDebugThread _thread;

        public ICorDebugThread Model { get { return _thread; } }

        public DebugThread(ICorDebugThread thread)
        {
            _thread = thread;
        }

        public int Id
        {
            get
            {
                uint pdwThreadId;
                _thread.GetID(out pdwThreadId);
                return (int)pdwThreadId;
            }
        }
    }
}
