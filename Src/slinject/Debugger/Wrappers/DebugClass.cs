using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using slinject.Native.Debug.Interfaces;

namespace slinject.Debugger.Wrappers
{
    public class DebugClass
    {
        private ICorDebugClass _debugClass;

        public DebugClass(ICorDebugClass debugClass)
        {
            _debugClass = debugClass;
        }

        internal ICorDebugType GetDebugType()
        {
            var class2 = (ICorDebugClass2)_debugClass;
            ICorDebugType ppType;
            ICorDebugType[] typeArgs = new ICorDebugType[0];
            class2.GetParameterizedType(CorElementType.ELEMENT_TYPE_CLASS, 0, typeArgs, out ppType);
            return ppType;
        }
    }
}
