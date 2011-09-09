using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using slinject.Utils;
using System.Diagnostics;

namespace slinject
{
    public class InjectorInstaller
    {
        private const string TargetAssembly = "System.Windows.dll";
        private const string TargetNativeImage = "System.Windows.ni.dll";
        private const string TargetIniFile = "System.Windows.ini";
        private const string UnoptimizedINIFile =
@"[.NET Framework Debugging Control]
GenerateTrackingInfo=1
AllowOptimize=0";

        public void Uninstall()
        {
            if (!Remove(TargetNativeImage, Messages.UnauthorizedNativeImageRemove)) return;
            if (!Remove(TargetIniFile, Messages.UnauthorizedINIRemove)) return;
            if (!ExecuteCoregen(TargetAssembly, Messages.FailedToCoregen)) return;

            Console.WriteLine(Messages.UninstallSuccess);
        }

        public void Install()
        {
            if (!Remove(TargetNativeImage, Messages.UnauthorizedNativeImageRemove)) return;
            if (!WriteIniFile(TargetIniFile, Messages.FailedToWriteIniFile)) return;
            if (!ExecuteCoregen(TargetAssembly, Messages.FailedToCoregen)) return;

            Console.WriteLine(Messages.InstallSuccess);
        }

        private bool ExecuteCoregen(string fileName, string errorMessage)
        {
            var startInfo = new ProcessStartInfo("coregen.exe")
                {
                    Arguments = fileName,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                };

            Console.WriteLine("Starting coregen...");

            try
            {
                using (var coreGenProc = Process.Start(startInfo))
                {
                    using (StreamReader reader = coreGenProc.StandardOutput)
                    {
                        string result = reader.ReadToEnd();
                        Console.Write(result);
                    }

                    return coreGenProc.ExitCode == 0;
                }
            }
            catch (FileNotFoundException)
            {
                Logger.Error(errorMessage);
                return false;
            }
        }

        private bool WriteIniFile(string fileName, string errorMessage)
        {
            try
            {
                using (var file = File.OpenWrite(fileName))
                {
                    // Ini file should be in little endian. Don't ask me why...
                    var content = Encoding.Unicode.GetBytes(UnoptimizedINIFile);
                    file.Write(content, 0, content.Length);
                    return true;
                }
            }
            catch (Exception)
            {
                Logger.Error(errorMessage);
                return false;             
            }
        }

        private bool Remove(string file, string unauthorizedMessage)
        {
            try
            {
                File.Delete(file);
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                Logger.Error(unauthorizedMessage);
                return false;
            }
        }

    }
}
