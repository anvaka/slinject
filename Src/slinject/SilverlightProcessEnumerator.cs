using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using slinject.Native.Windows;
using System.Diagnostics;

namespace slinject
{
    /// <summary>
    /// Enumerates all running Silverlight processes in the system.
    /// </summary>
    public class SilverlightProcessEnumerator : IEnumerable<Process>
    {
        /// <summary>
        /// Returns list of runing Silverlight processes.
        /// </summary>
        public IEnumerator<Process> GetEnumerator()
        {
            int[] procIds = new Int32[1024];
            int returned;
            NativeMethods.EnumProcesses(procIds, 1024, out returned);

            for (int i = 0; i < returned; i++)
            {
                var process = Process.GetProcessById(procIds[i]);
                foreach (var module in GetModules(process.Id))
                {
                    if (IsSilverlightModule(module.szModule))
                    {
                        yield return process;
                        break;
                    }
                }
            }
        }

        private IEnumerable<NativeMethods.MODULEENTRY32> GetModules(int processId)
        {
            var me32 = new NativeMethods.MODULEENTRY32();
            var hModuleSnap = NativeMethods.CreateToolhelp32Snapshot(NativeMethods.SnapshotFlags.Module | NativeMethods.SnapshotFlags.Module32, processId);
            if (!hModuleSnap.IsInvalid)
            {
                using (hModuleSnap)
                {
                    me32.dwSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(me32);
                    if (NativeMethods.Module32First(hModuleSnap, ref me32))
                    {
                        do
                        {
                            yield return me32;
                        } while (NativeMethods.Module32Next(hModuleSnap, ref me32));
                    }
                }
            }
        }

        private bool IsSilverlightModule(string moduleName)
        {
            return moduleName.Contains("coreclr");
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
