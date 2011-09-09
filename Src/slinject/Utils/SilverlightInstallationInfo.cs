using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.IO;
using System.Diagnostics;

namespace slinject.Utils
{
    public class SilverlightInstallationInfo
    {
        public const string SilverlightControlKey = "CLSID\\{DFEAF541-F3E1-4c24-ACAC-99C30715084A}\\InprocServer32";
        private string _oldWorkingDirectory;

        public string InstallationFolder
        {
            get
            {
                try
                {
                    return GetFromRegistry();
                }
                catch (Exception e)
                {
                    Logger.Error(string.Format("Error while getting silverlight path {0}\r\n{1}", e, Messages.ErrorNoSilverlight));
                    return string.Empty;
                }
            }
        }

        private string GetFromRegistry()
        {
            using (var silverlightKey = Registry.ClassesRoot.OpenSubKey(SilverlightControlKey))
            {
                var npctrlPath = silverlightKey.GetValue(string.Empty, string.Empty) as string;
                if (string.IsNullOrEmpty(npctrlPath)) return string.Empty;

                return GetPathSafe(npctrlPath);
            }
        }

        private string GetPathSafe(string fullFilePath)
        {
            try
            {
                return Path.GetDirectoryName(fullFilePath);
            }
            catch (ArgumentException)
            {
                return string.Empty;
            }
            catch (PathTooLongException)
            {
                return string.Empty;
            }
        }

        internal void TrySetWorkingFolder()
        {
            var silverlightFolder = InstallationFolder;
            if (string.IsNullOrEmpty(silverlightFolder))
            {
                return;
            }
            
            _oldWorkingDirectory = Directory.GetCurrentDirectory();
            var currentProcess = Process.GetCurrentProcess();
            currentProcess.Exited += (o, e) => Directory.SetCurrentDirectory(_oldWorkingDirectory);

            Directory.SetCurrentDirectory(silverlightFolder);
        }
    }
}
