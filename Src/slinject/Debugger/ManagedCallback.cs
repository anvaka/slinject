using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using slinject.Native.Debug.Interfaces;
using System.Runtime.InteropServices.ComTypes;
using slinject.Debugger.Wrappers;
using System.Runtime.InteropServices;

namespace slinject.Debugger
{
    // Helper class to convert from COM-classic callback interface into managed args.
    // Derived classes can overide the HandleEvent method to define the handling.
    public class ManagedCallback : ICorDebugManagedCallback, ICorDebugManagedCallback2
    {
        public event EventHandler<DebuggerBreakpointEventArgs> OnBreakpoint;
        public event EventHandler<DebuggerEvalEventArgs> OnEvalCompleted;
        public event EventHandler<DebuggerEvalEventArgs> OnEvalFailed;
        public event EventHandler<DebuggerEventArgs> OnException;
        public event EventHandler<DebuggerModuleLoadedEventArgs> OnModuleLoaded;
        public event EventHandler<DebuggerDomainExitedEventArgs> OnDomainExited;

        public void Breakpoint(ICorDebugAppDomain pAppDomain, ICorDebugThread pThread, ICorDebugBreakpoint pBreakpoint)
        {
            var handler = OnBreakpoint;
            if (handler != null)
            {
                handler(this, new DebuggerBreakpointEventArgs(pThread, pBreakpoint));
            }

            pAppDomain.Continue(0);
        }

        public void StepComplete(ICorDebugAppDomain pAppDomain, ICorDebugThread pThread, ICorDebugStepper pStepper, CorDebugStepReason reason)
        {
            pAppDomain.Continue(0);
        }

        public void Break(ICorDebugAppDomain pAppDomain, ICorDebugThread thread)
        {
            pAppDomain.Continue(0);
        }

        public void Exception(ICorDebugAppDomain pAppDomain, ICorDebugThread pThread, int unhandled)
        {
            var handler = OnException;
            if (handler != null) handler(this, new DebuggerEventArgs(pThread));

            pAppDomain.Continue(0);
        }

        public void EvalComplete(ICorDebugAppDomain pAppDomain, ICorDebugThread pThread, ICorDebugEval pEval)
        {
            var handler = OnEvalCompleted;
            if (handler != null) handler(this, new DebuggerEvalEventArgs(pThread, pEval));

            pAppDomain.Continue(0);
        }

        public void EvalException(ICorDebugAppDomain pAppDomain, ICorDebugThread pThread, ICorDebugEval pEval)
        {
            var handler = OnEvalFailed;
            if (handler != null) handler(this, new DebuggerEvalEventArgs(pThread, pEval));

            pAppDomain.Continue(0);
        }

        public void CreateProcess(ICorDebugProcess pProcess)
        {
            pProcess.Continue(0);
        }

        public void ExitProcess(ICorDebugProcess pProcess)
        {
            pProcess.Continue(0);
        }

        public void CreateThread(ICorDebugAppDomain pAppDomain, ICorDebugThread thread)
        {
            pAppDomain.Continue(0);
        }

        public void ExitThread(ICorDebugAppDomain pAppDomain, ICorDebugThread thread)
        {
            pAppDomain.Continue(0);
        }

        public void LoadModule(ICorDebugAppDomain pAppDomain, ICorDebugModule pModule)
        {
            Logger.WriteLine("Module {0} loaded", new DebugModule(pModule).Name);

            var handler = OnModuleLoaded;
            if (handler != null) handler(this, new DebuggerModuleLoadedEventArgs(pModule));
            
            pAppDomain.Continue(0);
        }

        public void UnloadModule(ICorDebugAppDomain pAppDomain, ICorDebugModule pModule)
        {
            try
            {
                Logger.WriteLine("Module {0} unloaded", new DebugModule(pModule).Name);
            }
            catch (Exception)
            {
                Logger.WriteLine("Could not resolve unloaded module name");
            }
            pAppDomain.Continue(0);
        }

        public void LoadClass(ICorDebugAppDomain pAppDomain, ICorDebugClass c)
        {
            pAppDomain.Continue(0);
        }

        public void UnloadClass(ICorDebugAppDomain pAppDomain, ICorDebugClass c)
        {
            pAppDomain.Continue(0);
        }

        public void DebuggerError(ICorDebugProcess pProcess, int errorHR, uint errorCode)
        {
            Logger.WriteLine("Debugger Error. errorHR: {0}; erroCode: ", errorHR, errorCode);
            pProcess.Continue(0);
        }

        public void LogMessage(ICorDebugAppDomain pAppDomain, ICorDebugThread pThread, int lLevel, string pLogSwitchName, string pMessage)
        {
            Logger.WriteLine("Debug Output: {0}", pMessage);
            pAppDomain.Continue(0);
        }

        public void LogSwitch(ICorDebugAppDomain pAppDomain, ICorDebugThread pThread, int lLevel, uint ulReason, string pLogSwitchName, string pParentName)
        {
            pAppDomain.Continue(0);
        }

        public void CreateAppDomain(ICorDebugProcess pProcess, ICorDebugAppDomain pAppDomain)
        {
            var domain = new DebugDomain(null, pAppDomain);
            Logger.WriteLine("App domain {0} created", domain.Name);
            
            pAppDomain.Attach();

            pProcess.Continue(0);
        }

        public void ExitAppDomain(ICorDebugProcess pProcess, ICorDebugAppDomain pAppDomain)
        {
            var domain = new DebugDomain(null, pAppDomain);
            Logger.WriteLine("App domain {0} exited", domain.Name);
            var handler = OnDomainExited;
            if (handler != null) handler(this, new DebuggerDomainExitedEventArgs(pAppDomain));
            pProcess.Continue(0);
        }

        public void LoadAssembly(ICorDebugAppDomain pAppDomain, ICorDebugAssembly pAssembly)
        {
            var assembly = new DebugAssembly(pAssembly);
            Logger.WriteLine("Loaded {0} assembly", assembly.Name);
            pAppDomain.Continue(0);
        }

        public void UnloadAssembly(ICorDebugAppDomain pAppDomain, ICorDebugAssembly pAssembly)
        {
            var assembly = new DebugAssembly(pAssembly);
            Logger.WriteLine("Unloaded {0} assembly", assembly.Name);
            pAppDomain.Continue(0);
        }

        public void ControlCTrap(ICorDebugProcess pProcess)
        {
            pProcess.Continue(0);
        }

        public void NameChange(ICorDebugAppDomain pAppDomain, ICorDebugThread pThread)
        {
            pAppDomain.Continue(0);
        }

        public void UpdateModuleSymbols(ICorDebugAppDomain pAppDomain, ICorDebugModule pModule, IStream pSymbolStream)
        {
            pAppDomain.Continue(0);
        }

        public void EditAndContinueRemap(ICorDebugAppDomain pAppDomain, ICorDebugThread pThread, ICorDebugFunction pFunction, int fAccurate)
        {
            pAppDomain.Continue(0);
        }

        public void BreakpointSetError(ICorDebugAppDomain pAppDomain, ICorDebugThread pThread, ICorDebugBreakpoint pBreakpoint, uint dwError)
        {
            Logger.WriteLine("Failed to set breakpoing. dwError: {0}", dwError);
            pAppDomain.Continue(0);
        }

        public void FunctionRemapOpportunity(ICorDebugAppDomain pAppDomain, ICorDebugThread pThread, ICorDebugFunction pOldFunction, ICorDebugFunction pNewFunction, uint oldILOffset)
        {
            pAppDomain.Continue(0);
        }

        public void CreateConnection(ICorDebugProcess pProcess, uint dwConnectionId, ref ushort pConnName)
        {
            pProcess.Continue(0);
        }

        public void ChangeConnection(ICorDebugProcess pProcess, uint dwConnectionId)
        {
            pProcess.Continue(0);
        }

        public void DestroyConnection(ICorDebugProcess pProcess, uint dwConnectionId)
        {
            pProcess.Continue(0);
        }

        public void Exception(ICorDebugAppDomain pAppDomain, ICorDebugThread pThread, ICorDebugFrame pFrame, uint nOffset, CorDebugExceptionCallbackType dwEventType, uint dwFlags)
        {
            pAppDomain.Continue(0);
        }

        public void ExceptionUnwind(ICorDebugAppDomain pAppDomain, ICorDebugThread pThread, CorDebugExceptionUnwindCallbackType dwEventType, uint dwFlags)
        {
            pAppDomain.Continue(0);
        }

        public void FunctionRemapComplete(ICorDebugAppDomain pAppDomain, ICorDebugThread pThread, ICorDebugFunction pFunction)
        {
            pAppDomain.Continue(0);
        }

        public void MDANotification(ICorDebugController pController, ICorDebugThread pThread, ICorDebugMDA pMDA)
        {
            pController.Continue(0);
        }
    }
}
