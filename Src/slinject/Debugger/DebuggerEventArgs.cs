using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using slinject.Native.Debug.Interfaces;

namespace slinject.Debugger
{
    public class DebuggerEventArgs : EventArgs
    {
        private ICorDebugThread _thread;

        public DebuggerEventArgs(ICorDebugThread thread)
        {
            _thread = thread;
        }

        public ICorDebugThread Thread { get { return _thread; } }
    }

    public class DebuggerBreakpointEventArgs : DebuggerEventArgs
    {
        private ICorDebugBreakpoint _breakpoint;
        
        public DebuggerBreakpointEventArgs(ICorDebugThread thread, ICorDebugBreakpoint breakpoint) : base(thread)
        {
            _breakpoint = breakpoint;
        }

        public ICorDebugBreakpoint Breakpoint { get { return _breakpoint; } }
    }

    public class DebuggerEvalEventArgs : DebuggerEventArgs
    {
        private ICorDebugEval _eval;
        
        public DebuggerEvalEventArgs(ICorDebugThread thread, ICorDebugEval eval) : base(thread)
        {
            _eval = eval;
        }

        public ICorDebugEval Eval { get { return _eval; } }
    }

    public class DebuggerModuleLoadedEventArgs : EventArgs
    {
        private ICorDebugModule _module;

        public DebuggerModuleLoadedEventArgs(ICorDebugModule module)
        {
            _module = module;
        }

        public ICorDebugModule Module { get { return _module; } }
    }

    public class DebuggerDomainExitedEventArgs : EventArgs
    {
        public DebuggerDomainExitedEventArgs(ICorDebugAppDomain domain)
        {
            Domain = domain;
        }

        public ICorDebugAppDomain Domain { get; private set; }
    }
}
