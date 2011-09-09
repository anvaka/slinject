using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using slinject.Native.Debug.Interfaces;

namespace slinject.Debugger.Wrappers
{
    public class DebugAssembly
    {
        private ICorDebugAssembly _assembly;
        private string _name;
        private IList<DebugModule> _modules;

        public DebugAssembly(ICorDebugAssembly assembly)
        {
            _assembly = assembly;
        }

        public string Name
        {
            get
            {
                return _name ?? (_name = LoadName());
            }
        }

        public DebugDomain Domain
        {
            get
            {
                ICorDebugAppDomain ppAppDomain;
                
                _assembly.GetAppDomain(out ppAppDomain);
                
                return new DebugDomain(null, ppAppDomain);
            }
        }


        private string LoadName()
        {
            var name = new char[256];
            uint count;
            _assembly.GetName(256, out count, name);

            return new string(name, 0, (int)count - 1);
        }

        public IList<DebugModule> Modules
        {
            get
            {
                return _modules ?? (_modules = LoadModules());
            }
        }

        private IList<DebugModule> LoadModules()
        {
            ICorDebugModuleEnum modulesEnum;
            _assembly.EnumerateModules(out modulesEnum);
            uint modCount;
            modulesEnum.GetCount(out modCount);
            ICorDebugModule[] modules = new ICorDebugModule[modCount];
            uint modulesFetched;
            modulesEnum.Next(modCount, modules, out modulesFetched);

            var result = new List<DebugModule>((int)modulesFetched);
            for (int i = 0; i < modulesFetched; i++)
            {
                result.Add(new DebugModule(modules[i]));
            }

            return result;
        }
    }
}
