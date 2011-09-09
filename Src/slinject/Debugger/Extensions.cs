using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using slinject.Native.Debug.Interfaces;
using slinject.Debugger.Wrappers;
using System.Reflection;
using slinject.Debugger.Metadata;
using System.Text.RegularExpressions;

namespace slinject.Debugger
{
    public static class Extensions
    {
        public static DebugModule FindModule(this DebugProcess process, string moduleNameRegexPattern)
        {
            Regex pattern = new Regex(moduleNameRegexPattern);

            foreach (var domain in process.Domains)
            {
                foreach (var assembly in domain.Assemblies)
                {
                    foreach (var module in assembly.Modules)
                    {
                        if (pattern.IsMatch(module.Name))
                        {
                            return module;
                        }
                    }
                }
            }

            return null;
        }

        public static bool Is(this DebugModule module, string pattern)
        {
            var regex = new Regex(pattern, RegexOptions.IgnoreCase);
            return regex.IsMatch(module.Name);
        }

        public static bool IsAtSafePoint(this ICorDebugThread thread)
        {
            CorDebugUserState threadState;
            thread.GetUserState(out threadState);
            return ((threadState & CorDebugUserState.USER_UNSAFE_POINT) != CorDebugUserState.USER_UNSAFE_POINT);
        }

        public static DebugFunction ResolveFunctionName(this DebugModule module, string className, string functionName)
        {
            int typeToken = module.Importer.GetTypeTokenFromName(className);
            if (typeToken == DebugMetadataImporter.TokenNotFound)
                return null;

            DebugFunction func = null;
            Type t = module.Importer.GetType(typeToken);
            foreach (MethodInfo mi in t.GetMethods())
            {
                if (mi.Name.Equals(functionName))
                {
                    func = module.GetMethodByMetadataInfo(mi as MetadataMethodInfo);
                    break;
                }
            }

            return func;
        }

        public static List<DebugFunction> FindAllFunction(this DebugProcess process, string className, string functionName)
        {
            foreach (var domain in process.Domains)
            {
                foreach (var assembly in domain.Assemblies)
                {
                    foreach (var module in assembly.Modules)
                    {
                        var functions = ResolveAllFunctionName(module, className, functionName);
                        if (functions != null)
                        {
                            return functions;
                        }
                    }
                }
            }

            return null;
        }

        public static List<DebugFunction> ResolveAllFunctionName(this DebugModule module, string className, string functionName)
        {
            int typeToken = module.Importer.GetTypeTokenFromName(className);
            if (typeToken == DebugMetadataImporter.TokenNotFound)
                return null;

            Type t = module.Importer.GetType(typeToken);
            List<DebugFunction> result = null;

            foreach (MethodInfo mi in t.GetMethods())
            {
                if (mi.Name.Equals(functionName))
                {
                    if (result == null)
                    {
                        result = new List<DebugFunction>();
                    }

                    result.Add(module.GetMethodByMetadataInfo(mi as MetadataMethodInfo));
                }
            }

            return result;
        }

        public static DebugClass FindType(this DebugProcess process, string typeName, out DebugModule mod)
        {
            mod = null;
            foreach (var domain in process.Domains)
            {
                foreach (var assembly in domain.Assemblies)
                {
                    foreach (var module in assembly.Modules)
                    {
                        var type = module.FindType(typeName);
                        if (type != null)
                        {
                            mod = module;
                            return type;
                        }
                    }
                }
            }

            return null;
        }

        public static DebugClass FindType(this DebugModule module, string typeName)
        {
            int classToken = module.Importer.GetTypeTokenFromName(typeName);
            if (classToken != DebugMetadataImporter.TokenNotFound)
            {
                return module.GetClassFromToken(classToken);
            }

            return null;
        }
    }
}
