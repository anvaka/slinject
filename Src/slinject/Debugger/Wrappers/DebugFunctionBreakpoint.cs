using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using slinject.Native.Debug.Interfaces;

namespace slinject.Debugger.Wrappers
{
    public class DebugFunctionBreakpoint
    {
        private ICorDebugFunctionBreakpoint _bp;

        public DebugFunctionBreakpoint(ICorDebugFunctionBreakpoint bp)
        {
            _bp = bp;
        }

        public ICorDebugFunctionBreakpoint Interface { get { return _bp; } }

        public bool IsActive
        {
            get
            {
                int pbActive;
                _bp.IsActive(out pbActive);
                return pbActive == 1;
            }
        }
    }
}
