using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using slinject.Debugger;
using System.Threading;

namespace slinject
{
    public class Injector
    {
        private int _pid;
        private ManualResetEvent _finish = new ManualResetEvent(false);

        public Injector(int pid)
        {
            _pid = pid;
        }
        
        internal void Run(List<BreakpointMethod> breakpoints)
        {
            if (!File.Exists(InjectorSettings.DbgShimLibraryName))
            {
                WriteNoDebugShimMessage();
                return;
            }

            if (!File.Exists(InjectorSettings.AssemblyToInject))
            {
                WriteNoAssemblyToInject();
                return;
            }

            var debugEngine = new DebugEngine(_pid);

            var assembly = AssemblyReader.ReadBytes(InjectorSettings.AssemblyToInject);
            debugEngine.SetAssemblyCode(assembly);
            debugEngine.SetPotentialBreakpoints(breakpoints);

            debugEngine.Finished += (o, e) =>
                {
                    _finish.Set();
                };
                
            var userInputMonitor = new Thread(() => { Console.ReadLine(); _finish.Set(); }) { IsBackground = true };
            userInputMonitor.Start();
            
            debugEngine.Start();

            _finish.WaitOne();
            debugEngine.Stop();
        }

        private void WriteNoAssemblyToInject()
        {
            Logger.WriteLine("Cannot find assembly to inject: {0}", InjectorSettings.AssemblyToInject);
            Logger.WriteLine("Make sure this file exists in the current folder");
        }

        private void WriteNoDebugShimMessage()
        {
            Logger.WriteLine("No Silverlight debug shim library found.");
            Logger.WriteLine("Copy {0} file from Silverlight installation directory and retry attempt.", InjectorSettings.DbgShimLibraryName);
        }
    }
}
