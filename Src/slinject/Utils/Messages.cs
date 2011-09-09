using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace slinject.Utils
{
    public static class Messages
    {
        public static string ErrorNoSilverlight = "Silverlight installation path not found. Try one of the following:\r\n" +
            "1. Make sure Silverlight is installed on this machine;\r\n" +
            "2. Run this application as administrator;\r\n" +
            "3. Copy this application to Silverlight installation folder";

        public const string GreetingMessage =
@"slinject 1.0 alpha
Allows to inject arbitrary Silverlight assembly to running Silverlight process
Copyright (C) 2011 Andrei Kashcha (anvaka@gmail.com)

Usage: slinject [--mode [Args | Auto | Enum]] [-p Pid]";

        public const string ModeHelp =
@"Injection mode:
Enum - find all running Silverlight apps and suggest one to choose;
Args - process Id should be specified by -p parameter;
Auto - inject to the first found Silverlight application;";

        public const string InstallNode = 
@"Note: If you are running slinject for the first time make sure to install it 
by calling 'slinject --install'";
        public const string PidHelp = "Target Siliverlight process identifier";

        public const string UninstallHelp = "Revert changes made by --install operation. Doing this disables further assembly injection attempts.";

        public const string InstallHelp = "Installs injector. Should be called only once and requires elevated trust. Can't be called with other options.";

        public const string UnauthorizedNativeImageRemove = "Cannot remove precompiled file. Make sure no Silverlight application is runnig and you have administrator priviledges";
        
        public const string UnauthorizedINIRemove = "Cannot remove INI file. Make sure no Silverlight application is runnig and you have administrator priviledges";
        
        public const string FailedToWriteIniFile = "Cannot write ini file for System.Windows.dll. Make sure you have administrator rights and the file is not being edited by other program";

        public const string FailedToCoregen = "Cannot execute CoreGen on System.Windows.dll. Make sure you have administrator rights and Silverlight installation contains coregen.exe file";

        public const string InstallSuccess = @"
Success. Assembly injection is now available. You can always revert changes with 
-u command line switch.";

        public const string UninstallSuccess = @"
Success. Assembly injection is now disabled. Use -i command line switch make 
it available again.";
        
    }
}
