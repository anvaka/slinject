using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using slinject.Native.Debug.Interfaces;

namespace slinject.Debugger.Wrappers
{
    public class DebugProcess
    {
        private DebugEngine _debugEngine;
        private uint _pid;
        private ICorDebugProcess _debugProcess;
        private IList<DebugDomain> _domains;

        public DebugProcess(DebugEngine debugEngine, int pid)
        {
            // TODO: Complete member initialization
            _debugEngine = debugEngine;
            _pid = (uint)pid;
        }

        internal void Attach()
        {
            _debugEngine.Interface.DebugActiveProcess(_pid, 0, out _debugProcess);

            //_debugProcess.Stop(1000);
            //_debugProcess.Continue(0);

            _debugProcess.Stop(1000);
            _debugProcess.Continue(0);
        }

        public IList<DebugDomain> Domains
        {
            get
            {
                if (_domains == null)
                {
                    _domains = LoadDomains();
                }

                return _domains;
            }
        }

        private IList<DebugDomain> LoadDomains()
        {
            ICorDebugAppDomainEnum domainsEnum;
            _debugProcess.EnumerateAppDomains(out domainsEnum);

            uint domainsCount;
            domainsEnum.GetCount(out domainsCount);

            ICorDebugAppDomain[] domains = new ICorDebugAppDomain[domainsCount];
            uint domainsFetched;
            domainsEnum.Next(domainsCount, domains, out domainsFetched);

            var list = new List<DebugDomain>((int)domainsFetched);

            for (int i = 0; i < domainsFetched; i++)
            {
                list.Add(new DebugDomain(_debugEngine, domains[i]));
            }

            return list;
        }

        internal void Detach()
        {
            _debugProcess.Stop(100);
            _debugProcess.Detach();
        }

        internal void Continue()
        {
            _debugProcess.Continue(0);
        }

        internal void Stop()
        {
            _debugProcess.Stop(1000);
        }
    }
}
