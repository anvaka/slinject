using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using slinject.Utils;

namespace slinject
{
    class Program
    {
        // TODO: Add verbose log settings that will print all log strings to console
        static void Main(string[] args)
        {
            var silverlightInfo = new SilverlightInstallationInfo();
            silverlightInfo.TrySetWorkingFolder();

            var appSettings = new InjectorSettings(args);
            if (!appSettings.IsValid)
            {
                appSettings.WriteUsage();
                Environment.Exit(-1);
                return;
            }

            if (appSettings.Install || appSettings.Uninstall)
            {
                var injectorInstaller = new InjectorInstaller();
                if (appSettings.Install) injectorInstaller.Install();
                else if (appSettings.Uninstall) injectorInstaller.Uninstall();

                Environment.Exit(0);
            }

            var pid = CalculatePid(appSettings);

            if (pid == -1)
            {
                Environment.Exit(-2);
                return;
            }

            var injectorEngine = new Injector(pid);
            injectorEngine.Run(appSettings.Breakpoints);
        }

        private static int CalculatePid(InjectorSettings appSettings)
        {
            if (appSettings.PidSource == InjectorSettings.PidSelectionMode.Args) return appSettings.Pid;

            var slProcessEnumerator = new SilverlightProcessEnumerator();
            if (appSettings.PidSource == InjectorSettings.PidSelectionMode.Auto)
            {
                var process = slProcessEnumerator.FirstOrDefault();
                if (process == null) return -1;

                Logger.WriteLine("First process {0}. Id: {1}", process.ProcessName, process.Id);
                
                return process.Id;
            }

            return GetPidFromUser(slProcessEnumerator);
        }

        private static int GetPidFromUser(SilverlightProcessEnumerator slProcessEnumerator)
        {
            Console.WriteLine("Enumerating Silverlight processes...");
            
            HashSet<int> found = new HashSet<int>();

            foreach (var process in slProcessEnumerator)
            {
                if (found.Contains(process.Id)) continue;

                Console.WriteLine("Pid: {0}; Name: {1}", process.Id, process.ProcessName);
                found.Add(process.Id);
            }

            if (found.Count == 0)
            {
                Console.WriteLine("No running Silverlight processes found");
                return -1;
            }

            Console.Write("Enter process id to start (blank line to quit): ");
            var selectedPidRaw = Console.ReadLine();
            int selectedPid = 0;
            if (!Int32.TryParse(selectedPidRaw, out selectedPid))
            {
                return -1;
            }

            return selectedPid;
        }
    }
}
