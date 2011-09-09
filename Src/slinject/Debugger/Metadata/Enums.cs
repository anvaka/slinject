//---------------------------------------------------------------------
//  This file is based on the CLR Managed Debugger (mdbg) Sample.
// 
//  Which is copyrighted by (C) Microsoft Corporation.  All rights reserved.
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace slinject.Debugger.Metadata
{
    public enum MetadataTokenType
    {
        Module = 0x00000000,
        TypeRef = 0x01000000,
        TypeDef = 0x02000000,
        FieldDef = 0x04000000,
        MethodDef = 0x06000000,
        ParamDef = 0x08000000,
        InterfaceImpl = 0x09000000,
        MemberRef = 0x0a000000,
        CustomAttribute = 0x0c000000,
        Permission = 0x0e000000,
        Signature = 0x11000000,
        Event = 0x14000000,
        Property = 0x17000000,
        ModuleRef = 0x1a000000,
        TypeSpec = 0x1b000000,
        Assembly = 0x20000000,
        AssemblyRef = 0x23000000,
        File = 0x26000000,
        ExportedType = 0x27000000,
        ManifestResource = 0x28000000,
        GenericPar = 0x2a000000,
        MethodSpec = 0x2b000000,
        String = 0x70000000,
        Name = 0x71000000,
        BaseType = 0x72000000,
        Invalid = 0x7FFFFFFF,
    }

    public enum CorCallingConvention
    {
        Default = 0x0,

        VarArg = 0x5,
        Field = 0x6,
        LocalSig = 0x7,
        Property = 0x8,
        Unmanaged = 0x9,
        GenericInst = 0xa,  // generic method instantiation
        NativeVarArg = 0xb,  // used ONLY for 64bit vararg PInvoke calls

        // The high bits of the calling convention convey additional info
        Mask = 0x0f,  // Calling convention is bottom 4 bits
        HasThis = 0x20,  // Top bit indicates a 'this' parameter
        ExplicitThis = 0x40,  // This parameter is explicitly in the signature
        Generic = 0x10,  // Generic method sig with explicit number of type arguments (precedes ordinary parameter count)
    };

    // keep in sync with CorHdr.h
    public enum CorTokenType
    {
        mdtModule = 0x00000000,       //          
        mdtTypeRef = 0x01000000,       //          
        mdtTypeDef = 0x02000000,       //          
        mdtFieldDef = 0x04000000,       //           
        mdtMethodDef = 0x06000000,       //       
        mdtParamDef = 0x08000000,       //           
        mdtInterfaceImpl = 0x09000000,       //  
        mdtMemberRef = 0x0a000000,       //       
        mdtCustomAttribute = 0x0c000000,       //      
        mdtPermission = 0x0e000000,       //       
        mdtSignature = 0x11000000,       //       
        mdtEvent = 0x14000000,       //           
        mdtProperty = 0x17000000,       //           
        mdtModuleRef = 0x1a000000,       //       
        mdtTypeSpec = 0x1b000000,       //           
        mdtAssembly = 0x20000000,       //
        mdtAssemblyRef = 0x23000000,       //
        mdtFile = 0x26000000,       //
        mdtExportedType = 0x27000000,       //
        mdtManifestResource = 0x28000000,       //
        mdtGenericParam = 0x2a000000,       //
        mdtMethodSpec = 0x2b000000,       //
        mdtGenericParamConstraint = 0x2c000000,

        mdtString = 0x70000000,       //          
        mdtName = 0x71000000,       //
        mdtBaseType = 0x72000000,       // Leave this on the high end value. This does not correspond to metadata table
    }
}
