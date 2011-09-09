using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace slinject
{
    public class BreakpointMethod
    {
        public BreakpointMethod(string typeName, string methodName)
        {
            ClassName  = typeName;
            MethodName = methodName;
        }

        public string ClassName { get; private set; }
        public string MethodName { get; private set; }

        public Native.Debug.Interfaces.ICorDebugFunctionBreakpoint Breakpoint { get; set; }
    }
}
