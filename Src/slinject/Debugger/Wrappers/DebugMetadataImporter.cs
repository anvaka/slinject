using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using slinject.Native.Debug.Interfaces;
using System.Runtime.InteropServices;
using slinject.Debugger.Metadata;

namespace slinject.Debugger.Wrappers
{
    public class DebugMetadataImporter
    {
        private ICorDebugModule _module;
        private IMetadataImport _metadatImport;
        private static int CLDB_E_RECORD_NOTFOUND = unchecked((int)0x80131130);

        public const int TokenNotFound = -1;
        public const int TokenGlobalNamespace = 0;

        public IMetadataImport Importer { get { return _metadatImport; } }

        public DebugMetadataImporter(ICorDebugModule module)
        {
            _module = module;

            Guid interfaceGuid = typeof(IMetadataImport).GUID;
            _module.GetMetaDataInterface(ref interfaceGuid, out _metadatImport);
        }

        internal int GetTypeTokenFromName(string name)
        {
            int token = DebugMetadataImporter.TokenNotFound;
            if (name.Length == 0)
            {
                token = DebugMetadataImporter.TokenGlobalNamespace;
            }
            else
            {
                try
                {
                    _metadatImport.FindTypeDefByName(name, 0, out token);
                }
                catch (COMException e)
                {
                    token = DebugMetadataImporter.TokenNotFound;
                    if (e.ErrorCode == DebugMetadataImporter.CLDB_E_RECORD_NOTFOUND)
                    {
                        int i = name.LastIndexOf('.');
                        if (i > 0)
                        {
                            int parentToken = GetTypeTokenFromName(name.Substring(0, i));
                            if (parentToken != DebugMetadataImporter.TokenNotFound)
                            {
                                try
                                {
                                    _metadatImport.FindTypeDefByName(name.Substring(i + 1), parentToken, out token);
                                }
                                catch (COMException e2)
                                {
                                    token = DebugMetadataImporter.TokenNotFound;
                                    if (e2.ErrorCode != DebugMetadataImporter.CLDB_E_RECORD_NOTFOUND)
                                        throw;
                                }
                            }
                        }
                    }
                    else
                        throw;
                }
            }

            return token;
        }

        public Type GetType(int typeToken)
        {
            return new MetadataType(_metadatImport, typeToken);
        }
    }
}
