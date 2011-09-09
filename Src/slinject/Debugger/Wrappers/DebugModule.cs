using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using slinject.Native.Debug.Interfaces;
using slinject.Debugger.Metadata;
using System.IO;

namespace slinject.Debugger.Wrappers
{
    public class DebugModule
    {
        private ICorDebugModule _module;
        private DebugMetadataImporter _debugImporter;

        public DebugModule(ICorDebugModule module)
        {
            _module = module;
        }

        public string Name
        {
            get
            {
                uint count;
                var name = new char[256];
                _module.GetName(256, out count, name);

                return new string(name, 0, (int)count - 1);
            }
        }

        public DebugAssembly Assembly
        {
            get
            {
                ICorDebugAssembly ppAssembly;
                _module.GetAssembly(out ppAssembly);
                return new DebugAssembly(ppAssembly);
            }
        }

        public ulong BaseAddress
        {
            get
            {
                ulong address;
                _module.GetBaseAddress(out address);
                return address;
            }
        }

        public int Size
        {
            get
            {
                uint size;
                _module.GetSize(out size);
                return (int)size;
            }
        }

        public bool IsDynamic
        {
            get
            {
                int pDynamic;
                _module.IsDynamic(out pDynamic);
                return pDynamic == 1;
            }
        }

        public bool IsInMemory
        {
            get
            {
                int pInMemory;
                _module.IsInMemory(out pInMemory);
                return pInMemory == 1;
            }
        }

        public DebugMetadataImporter Importer
        {
            get
            {
                return _debugImporter ?? (_debugImporter = new DebugMetadataImporter(_module));
            }
        }

        private DebugFunction GetMethodByToken(int methodDefToken)
        {
            ICorDebugFunction ppFunction;
            _module.GetFunctionFromToken((uint)methodDefToken, out ppFunction);

            return new DebugFunction(ppFunction);
        }

        internal DebugClass GetClassFromToken(int classToken)
        {
            ICorDebugClass debugClass;
            _module.GetClassFromToken((uint)classToken, out debugClass);

            return new DebugClass(debugClass);
        }

        internal DebugFunction GetMethodByMetadataInfo(MetadataMethodInfo metadataMethodInfo)
        {
            var function = GetMethodByToken(metadataMethodInfo.MetadataToken);
            if (function != null)
            {
                function.MetaData = metadataMethodInfo;
            }

            return function;
        }        
    }
}
