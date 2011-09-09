using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using slinject.Native.Debug.Interfaces;

namespace slinject.Debugger.Wrappers
{
    public class DebugDomain
    {
        private DebugEngine _debugEngine;
        private ICorDebugAppDomain _domain;
        private string _name;
        private IList<DebugAssembly> _assemblies;
        private IList<DebugThread> _threads;

        public DebugDomain(DebugEngine debugEngine, ICorDebugAppDomain iCorDebugAppDomain)
        {
            _debugEngine = debugEngine;
            this._domain = iCorDebugAppDomain;
        }

        public string Name
        {
            get
            {
                return _name ?? (_name = LoadName());
            }
        }

        private string LoadName()
        {
            var buffLen = 256;
            var sb = new StringBuilder(buffLen);
            uint actualLength;
            _domain.GetName((uint)buffLen, out actualLength, sb);

            return sb.ToString();
        }

        public bool IsAttached
        {
            get
            {
                int isAttached;
                _domain.IsAttached(out isAttached);
                return isAttached == 1;
            }
        }

        #region // Assemblies

        public IList<DebugAssembly> Assemblies
        {
            get
            {
                return _assemblies ?? (_assemblies = LoadAssemblies());
            }
        }

        private IList<DebugAssembly> LoadAssemblies()
        {
            ICorDebugAssemblyEnum assembliesEnum;
            _domain.EnumerateAssemblies(out assembliesEnum);
            uint assembliesCount;
            assembliesEnum.GetCount(out assembliesCount);

            ICorDebugAssembly[] assemblies = new ICorDebugAssembly[assembliesCount];
            uint actualAssembliesCount;
            assembliesEnum.Next(assembliesCount, assemblies, out actualAssembliesCount);

            var result = new List<DebugAssembly>((int)actualAssembliesCount);
            for (int i = 0; i < actualAssembliesCount; i++)
            {
                result.Add(new DebugAssembly(assemblies[i]));
            }

            return result;
        }

        #endregion // Assemblies

        #region // Threads

        public IList<DebugThread> Threads
        {
            get
            {
                return _threads ?? (_threads = LoadThreads());
            }
        }

        private IList<DebugThread> LoadThreads()
        {
            ICorDebugThreadEnum threadsEnum;
            _domain.EnumerateThreads(out threadsEnum);

            uint threadsCount;
            threadsEnum.GetCount(out threadsCount);

            ICorDebugThread[] threads = new ICorDebugThread[threadsCount];
            uint actualThreadsCount;
            threadsEnum.Next(threadsCount, threads, out actualThreadsCount);

            var resultThreads = new List<DebugThread>((int)actualThreadsCount);
            for (int i = 0; i < actualThreadsCount; i++)
            {
                resultThreads.Add(new DebugThread(threads[i]));
            }

            return resultThreads;
        }

        #endregion // Threads


    }
}
