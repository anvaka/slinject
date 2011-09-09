using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace slinject
{
    public static class AssemblyReader
    {
        public static byte[] ReadBytes(string fileName)
        {
            FileStream fs = File.OpenRead(fileName);
            try
            {
                byte[] bytes = new byte[fs.Length];
                fs.Read(bytes, 0, Convert.ToInt32(fs.Length));
                fs.Close();
                return bytes;
            }
            finally
            {
                fs.Close();
            }
        }
    }
}
