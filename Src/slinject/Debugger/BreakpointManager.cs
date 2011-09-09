using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using slinject.Native.Debug.Interfaces;
using slinject.Debugger.Wrappers;

namespace slinject.Debugger
{
    public class BreakpointManager
    {
        Dictionary<uint, BreakpointMethod> _breakpointsMap = new Dictionary<uint, BreakpointMethod>();

        private List<BreakpointMethod> _unresolved = new List<BreakpointMethod>();

        public string GetMethodName(ICorDebugBreakpoint bp)
        {
            uint funcToken = GetFunctionToken(bp);
            
            BreakpointMethod method;
            if (!_breakpointsMap.TryGetValue(funcToken, out method)) return string.Empty;
            
            return string.Format("{0}.{1}", method.ClassName, method.MethodName);
        }

        public void Deactivate(ICorDebugBreakpoint bp)
        {
            _breakpointsMap.Remove(GetFunctionToken(bp));
            bp.Activate(0);
        }

        public void DeactivateAll()
        {
            foreach (var bm in _breakpointsMap.Values)
            {
                bm.Breakpoint.Activate(0);
            }

            _breakpointsMap.Clear();
        }

        internal void AddRange(List<BreakpointMethod> breakpoints)
        {
            _unresolved.AddRange(breakpoints);
        }

        internal void UpdateBreakpointsInModule(DebugModule module)
        {
            BreakpointMethod[] copy = new BreakpointMethod[_unresolved.Count];
            _unresolved.CopyTo(copy);

            foreach (var bp in copy)
            {
                var methods = module.ResolveAllFunctionName(bp.ClassName, bp.MethodName);
                if (methods == null) continue; // Sorry about this null. TODO: Refactor.

                foreach (var method in methods)
                {
                    Logger.WriteLine("Setting breakpoint at {0}.{1}", bp.ClassName, bp.MethodName);
                    var token = (uint)method.MetaData.MetadataToken;
                    if (_breakpointsMap.ContainsKey(token)) continue;
                    var bpWrapper = method.SetBreakpoint();
                    bp.Breakpoint = bpWrapper.Interface;
                    _breakpointsMap[token] = bp;
                }
            }
        }

        private static uint GetFunctionToken(ICorDebugBreakpoint bp)
        {
            var funcBreakpoint = bp as ICorDebugFunctionBreakpoint;
            if (funcBreakpoint == null) return uint.MaxValue;

            ICorDebugFunction function;
            funcBreakpoint.GetFunction(out function);
            uint funcToken;
            function.GetToken(out funcToken);
            return funcToken;
        }
    }
}
