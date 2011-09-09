using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using Mono.Options;
using slinject.Utils;

namespace slinject
{
    public class InjectorSettings
    {
        public const string DbgShimLibraryName = "dbgshim.dll";
        
        public static readonly string ModuleToInject = "AssemblyToInject";
        public static readonly string InjectClassName = "AssemblyToInject.GoBabyGo";
        public static readonly string InjectMethodName = "Inject";
        public static readonly string AssemblyToInject;

        private ParsedOptions _options;
        private OptionSet _optionsSet;

        static InjectorSettings()
        {
            var mainExecutableFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            AssemblyToInject = Path.Combine(mainExecutableFolder, ModuleToInject + ".dll");
        }
        
        public bool Install { get { return _options.Install.GetValueOrDefault(); } }

        public bool Uninstall { get { return _options.Uninstall.GetValueOrDefault(); } }

        public bool IsValid { get; private set; }

        public List<BreakpointMethod> Breakpoints { get; private set; }

        public InjectorSettings(string[] args)
        {
            Breakpoints = new List<BreakpointMethod>()
            {
                new BreakpointMethod("MS.Internal.JoltHelper", "FireEvent"),
            };

            _options = ParseOptions(args);

            IsValid = (_options.Mode == PidSelectionMode.Args && _options.Pid != null) ||
                (_options.Mode != PidSelectionMode.Args && _options.Pid == null) ||
                (_options.Install != null && args.Length == 1) ||
                (_options.Uninstall != null && args.Length == 1);
        }

        private ParsedOptions ParseOptions(string[] args)
        {
            var options = new ParsedOptions();

            _optionsSet = new OptionSet()
            {
                {"m|mode=", Messages.ModeHelp, (PidSelectionMode mode) => options.Mode = mode},
                {"p|pid=", Messages.PidHelp, (int pid) => options.Pid = pid},                
                {"i|install", Messages.InstallHelp, v => options.Install = true},
                {"u|uninstall", Messages.UninstallHelp, v => options.Uninstall = true},                
            };

            try
            {
                _optionsSet.Parse(args);
            }
            catch (OptionException)
            {
                return new ParsedOptions(); // Should be invalid.
            }
            
            return options;
        }

        private void WriteUsage(OptionSet options)
        {
            Console.WriteLine(Messages.GreetingMessage);
            Console.WriteLine("Options:");
            options.WriteOptionDescriptions(Console.Out);
            Console.WriteLine(Messages.InstallNode);
        }

        public void WriteUsage()
        {
            WriteUsage(_optionsSet);
        }

        public bool EnableEvalInExceptions { get { return _options.EvalInException.GetValueOrDefault(false); } }

        public int Pid { get { return _options.Pid ?? -1; } }

        public PidSelectionMode PidSource { get { return _options.Mode; } }

        private sealed class ParsedOptions
        {
            public bool? Install = null;

            public bool? Uninstall = null;

            public int? Pid = null;

            public PidSelectionMode Mode = PidSelectionMode.Args;

            public bool? EvalInException = null;
        }

        public enum PidSelectionMode
        {
            Args,
            Enum,
            Auto
        }
    }
}
