using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace slinject
{
    public class Logger
    {
        public static void WriteLine(string format)
        {
            WriteLine(format, null);
        }

        public static void WriteLine(string format, params object[] arg)
        {
            Console.WriteLine(format, arg);
        }

        internal static void Write(string value)
        {
            Console.Write(value);
        }

        internal static void Error(string value)
        {
            Console.WriteLine(value);
        }
    }
}
