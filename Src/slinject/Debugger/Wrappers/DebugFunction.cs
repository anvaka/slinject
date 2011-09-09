using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using slinject.Native.Debug.Interfaces;
using slinject.Debugger.Metadata;

namespace slinject.Debugger.Wrappers
{
    public class DebugFunction
    {
        private ICorDebugFunction _function;

        public DebugFunction(ICorDebugFunction function)
        {
            _function = function;
        }

        public DebugFunctionBreakpoint SetBreakpoint()
        {
            ICorDebugFunctionBreakpoint breakPoint;
            
            _function.CreateBreakpoint(out breakPoint);

            return new DebugFunctionBreakpoint(breakPoint);
        }

        public MetadataMethodInfo MetaData { get; set; }

        public ICorDebugFunction Interface { get { return _function; } }
    }
}
