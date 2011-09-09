using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using slinject.Native.Debug.Interfaces;
using slinject.Native.Debug;
using slinject.Debugger.Wrappers;
using System.Threading;
using slinject.Debugger.Metadata;
using System.Runtime.InteropServices;

namespace slinject.Debugger
{
    /// <summary>
    /// This is the heart of all injection logic. I'm sorry about its complexity.
    /// TODO: refactor it.
    /// </summary>
    /// <remarks>
    /// The injection is an asynchronous two-step process. It may only occur in FuncEval-safe points.
    /// Exceptions are considered to be FE-safe. The class also does support breakpoint FuncEval probbing
    /// but if breakpoint is set on optimized method - it's ignored.
    /// 
    /// During first FuncEval call we initialize space in a target process to copy injection assembly code.
    /// Once space is initialized we copy code and invoke Assembly.Load() method.
    /// 
    /// After the call is made we look closely at Module Loaded event to 
    /// find confirmation that our assembly is loaded. When confirmation received we resolve address 
    /// of the injector method and wait for the second FuncEval-safe event to perform call to injector.
    /// </remarks>
    public class DebugEngine
    {
        private int _pid;
        private IntPtr[] _clrHandles;
        private string[] _clrNames;
        private ICorDebug _corDebugInterface;
        private DebugProcess _debugProcess;
        private byte[] _assemblyBytes;
        private BreakpointManager _breakpointManager;
        private DebugFunction _injectorFunction;
        private ICorDebugType _byteCorType;
        private DebugFunction _loadAssemblyMethod;
        private ICorDebugValue _assemblyCodeInTargetProcess;
        private bool _nextEvalShouldCloseApplication;

        public DebugEngine(int pid)
        {
            _pid = pid;
            _breakpointManager = new BreakpointManager();
        }

        public event EventHandler Finished;

        public ICorDebug Interface { get { return _corDebugInterface; } }

        public void Start()
        {
            Logger.Write("Attaching debugger...");
            InitializeDebugger();
            Logger.WriteLine("Done!");
        }

        public void Stop()
        {
            _debugProcess.Stop();
            _breakpointManager.DeactivateAll();
            _debugProcess.Continue();
            _debugProcess.Detach();
            Thread.Sleep(100);
            _corDebugInterface.Terminate();

            // TODO: Close CLR Handles.
        }

        public void SetAssemblyCode(byte[] assembly)
        {
            _assemblyBytes = assembly;
        }

        private void InitializeDebugger()
        {
            int foundClrs;
            DebugMethods.EnumerateCLRs(_pid, out _clrHandles, out _clrNames, out foundClrs);

            var version = new StringBuilder(256);
            int allocated;
            DebugMethods.CreateVersionStringFromModule(_pid, _clrNames[0], version, version.Capacity, out allocated);

            _corDebugInterface = DebugMethods.CreateDebuggingInterfaceFromVersion(version.ToString());
            _corDebugInterface.Initialize();

            var callback = CreateCallback();
            _corDebugInterface.SetManagedHandler(callback);

            _debugProcess = new DebugProcess(this, _pid);
            _debugProcess.Attach();
        }

        private ManagedCallback CreateCallback()
        {
            var callback = new ManagedCallback();
            callback.OnException += OnDebuggerException;
            callback.OnBreakpoint += OnDebuggerBreakpoint;
            callback.OnEvalCompleted += OnDebuggerEvalCompleted;
            callback.OnModuleLoaded += OnDebuggerModuleLoaded;
            callback.OnDomainExited += OnDebuggerDomainExited;
            callback.OnEvalFailed += OnDebuggerEvalFailed;
            return callback;
        }

        private void OnDebuggerDomainExited(object sender, DebuggerDomainExitedEventArgs e)
        {
            // TODO: Check only bp's in domain.
            //_breakpointManager.DeactivateAll();
        }

        private void OnDebuggerBreakpoint(object sender, DebuggerBreakpointEventArgs e)
        {
            var bpName = _breakpointManager.GetMethodName(e.Breakpoint);

            Logger.WriteLine("Breakpoint {0} hit. Checking whether function evaluation is possible", bpName);

            if (e.Thread.IsAtSafePoint())
            {
                Logger.WriteLine("Thread is at safe point. Performing FuncEval...");
                PerformFuncEval(e.Thread);
            }
            else
            {
                // This is probably mean we are in the optimized code.
                // More info on Mike Stall's blog: http://blogs.msdn.com/b/jmstall/archive/2005/11/15/funceval-rules.aspx
                Logger.WriteLine("FuncEval is not available at this breakpoint. Deactivating the breakpoint");
                Logger.WriteLine("Run 'slinject --install' with administrator privileges to enabel assembly injection");
                _breakpointManager.Deactivate(e.Breakpoint);
            }
        }

        private void OnDebuggerException(object sender, DebuggerEventArgs e)
        {
            Logger.WriteLine("Encountered exception. Performing FuncEval...");
            PerformFuncEval(e.Thread);
        }

        private void OnDebuggerEvalFailed(object sender, DebuggerEvalEventArgs e)
        {
            Logger.WriteLine("Function evaluation failed :(");
        }

        private void OnDebuggerModuleLoaded(object sender, DebuggerModuleLoadedEventArgs e)
        {
            var module = new DebugModule(e.Module);
            
            if (module.Is(InjectorSettings.ModuleToInject))
            {
                UpdateInjectorFunction(module);
            }
            else if (module.Is(@"mscorlib\.dll") && !IsModuleInDomain(module, "DefaultDomain"))
            {
                ResetOldMscrolibMethods();
                ResolveNewMscrolibMethods(module);
            }            

            _breakpointManager.UpdateBreakpointsInModule(module);
        }

        private bool IsModuleInDomain(DebugModule module, string domainName)
        {
            return module.Assembly.Domain.Name == domainName;
        }

        private void ResolveNewMscrolibMethods(DebugModule mscorlib)
        {
            var assemblyLoadMethods = mscorlib.ResolveAllFunctionName("System.Reflection.Assembly", "Load");
            if (assemblyLoadMethods != null)
            {
                foreach (var method in assemblyLoadMethods)
                {
                    var methodParams = method.MetaData.GetParameters();
                    if (methodParams.Length == 1 && methodParams[0].Name == "rawAssembly")
                    {
                        _loadAssemblyMethod = method;
                        
                        _byteCorType = mscorlib.FindType("System.Byte").GetDebugType();

                        Logger.WriteLine("Assembly.Load(byte[] rawAssembly) - method address resolved.");
                    }
                }
            }
            else
            {
                Logger.WriteLine("Could not find Assembly.Load() method in mscorlib. Something is wrong.");
            }
        }

        private void ResetOldMscrolibMethods()
        {
            _assemblyCodeInTargetProcess = null; // TODO: we should delete old code?
            _injectorFunction = null;
            _loadAssemblyMethod = null;
            _byteCorType = null;
        }

        private void UpdateInjectorFunction(DebugModule module)
        {
            var function = module.ResolveFunctionName(InjectorSettings.InjectClassName, InjectorSettings.InjectMethodName);
            if (function != null)
            {
                _injectorFunction = function;
                Logger.WriteLine("Injector function resolved. Waiting for a chance to perform a call...");
            }
            else
            {
                Logger.WriteLine("The injecting module '{0}' was loaded, but method {1}.{2}() was not found in it",
                    InjectorSettings.ModuleToInject, InjectorSettings.InjectClassName, InjectorSettings.InjectMethodName);
                Logger.WriteLine("Did you forget to update injector's configuration?");
            }
        }

        private void PerformFuncEval(ICorDebugThread thread)
        {
            try
            {
                var shouldCopyAssemblyCode = _assemblyCodeInTargetProcess == null && _injectorFunction == null;
                if (shouldCopyAssemblyCode)
                {
                    PrepareToCopyInjectorAssembly(thread);
                    return;
                }

                var injectorMethodAvailable = _injectorFunction != null;
                if (injectorMethodAvailable)
                {
                    CallInjector(thread);
                    Logger.WriteLine("Injector called.");
                }
            }
            catch (Exception e)
            {
                Logger.WriteLine("Failed to evaluate function: {0}", e);
            }
        }

        private void OnDebuggerEvalCompleted(object sender, DebuggerEvalEventArgs e)
        {
            if (_assemblyCodeInTargetProcess == null && _injectorFunction == null)
            {
                _assemblyCodeInTargetProcess = CopyInjectorAssembly(e);
                InvokeAssemblyLoad((ICorDebugEval2)e.Eval, _assemblyCodeInTargetProcess);
            }

            if (_nextEvalShouldCloseApplication)
            {
                OnFinished();
            }
        }

        private void OnFinished()
        {
            var handler = Finished;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private void InvokeAssemblyLoad(ICorDebugEval2 eval, ICorDebugValue assemblyCodeInTargetProcess)
        {
            Logger.WriteLine("Calling Assembly.Load()");

            ICorDebugType[] types = new ICorDebugType[0];
            ICorDebugValue[] values = new ICorDebugValue[1] { assemblyCodeInTargetProcess };

            eval.CallParameterizedFunction(_loadAssemblyMethod.Interface, 0, types, (uint)values.Length, values);
            Logger.WriteLine("Assembly.Load() call made. Waiting for results.");
        }

        private ICorDebugValue CopyInjectorAssembly(DebuggerEvalEventArgs e)
        {
            Logger.WriteLine("Copying assembly code to target process...");

            ICorDebugValue assemblyCode;
            e.Eval.GetResult(out assemblyCode);

            var arrayReference = assemblyCode as ICorDebugReferenceValue;

            ICorDebugValue arrayDereferenced;
            arrayReference.Dereference(out arrayDereferenced);

            var array = arrayDereferenced as ICorDebugArrayValue;

            ICorDebugValue startElement;
            array.GetElementAtPosition((uint)0, out startElement);
            ICorDebugGenericValue startElementValue = startElement as ICorDebugGenericValue;
            ulong startAddress;
            startElementValue.GetAddress(out startAddress);

            ICorDebugProcess ppProcess;
            e.Thread.GetProcess(out ppProcess);
            IntPtr written;
            var assemblyLength = (uint)_assemblyBytes.Length;
            ppProcess.WriteMemory(startAddress, assemblyLength, _assemblyBytes, out written);

            Logger.WriteLine("{0} out {1} bytes of assembly code is written to the target process.", written.ToInt32(), assemblyLength);
            return assemblyCode;
        }

        private void PrepareToCopyInjectorAssembly(ICorDebugThread thread)
        {
            // Here we create space for new bytes array in the target process.
            // When next OnDebuggerEvalCompleted occur - we'll fill this array
            // with bytes of injecting assembly.
            if (_byteCorType == null)
            {
                Logger.WriteLine("Cannot copy assembly because mscrolib module was not resolved");
                return;
            }

            if (_assemblyCodeInTargetProcess == null)
            {
                ICorDebugEval ppEval;
                thread.CreateEval(out ppEval);
                var eval2 = (ICorDebugEval2)ppEval;
                            
                uint lowBounds = 0;
                var dims = (uint)_assemblyBytes.Length;
                eval2.NewParameterizedArray(_byteCorType, 1, ref dims, ref lowBounds);
            }
        }

        private void CallInjector(ICorDebugThread thread)
        {
            ICorDebugEval ppEval;
            thread.CreateEval(out ppEval);
            var eval2 = (ICorDebugEval2)ppEval;
            ICorDebugType[] types = new ICorDebugType[0];
            ICorDebugValue[] values = new ICorDebugValue[0];
            _nextEvalShouldCloseApplication = true;
            eval2.CallParameterizedFunction(_injectorFunction.Interface, 0, types, (uint)values.Length, values);
        }

        internal void SetPotentialBreakpoints(List<BreakpointMethod> breakpoints)
        {
            _breakpointManager.AddRange(breakpoints);
        }
    }
}
