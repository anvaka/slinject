//---------------------------------------------------------------------
//  This file is based on the CLR Managed Debugger (mdbg) Sample.
// 
//  Which is copyrighted by (C) Microsoft Corporation.  All rights reserved.
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using slinject.Native.Debug.Interfaces;
using System.Diagnostics;

namespace slinject.Debugger.Metadata
{
    static class MetadataHelperFunctions
    {
        private static uint TokenFromRid(uint rid, uint tktype) { return (rid) | (tktype); }

        // The below have been translated manually from the inline C++ helpers in cor.h

        internal static uint CorSigUncompressBigData(
            ref IntPtr pData)             // [IN,OUT] compressed data 
        {
            unsafe
            {
                byte* pBytes = (byte*)pData;
                uint res;

                // 1 byte data is handled in CorSigUncompressData   
                //  Debug.Assert(*pBytes & 0x80);    

                // Medium.  
                if ((*pBytes & 0xC0) == 0x80)  // 10?? ????  
                {
                    res = (uint)((*pBytes++ & 0x3f) << 8);
                    res |= *pBytes++;
                }
                else // 110? ???? 
                {
                    res = (uint)(*pBytes++ & 0x1f) << 24;
                    res |= (uint)(*pBytes++) << 16;
                    res |= (uint)(*pBytes++) << 8;
                    res |= (uint)(*pBytes++);
                }

                pData = (IntPtr)pBytes;
                return res;
            }
        }

        internal static uint CorSigUncompressData(
            ref IntPtr pData)             // [IN,OUT] compressed data 
        {
            unsafe
            {
                byte* pBytes = (byte*)pData;

                // Handle smallest data inline. 
                if ((*pBytes & 0x80) == 0x00)        // 0??? ????    
                {
                    uint retval = (uint)(*pBytes++);
                    pData = (IntPtr)pBytes;
                    return retval;
                }
                return CorSigUncompressBigData(ref pData);
            }
        }

        // Function translated directly from cor.h but never tested; included here in case someone wants to use it in future
        /*        internal static uint CorSigUncompressData(      // return number of bytes of that compressed data occupied in pData 
                    IntPtr pData,              // [IN] compressed data 
                    out uint pDataOut)              // [OUT] the expanded *pData    
                {   
                    unsafe
                    {
                        uint       cb = 0xffffffff;    
                        byte *pBytes = (byte*)(pData); 
                        pDataOut = 0;

                        // Smallest.    
                        if ((*pBytes & 0x80) == 0x00)       // 0??? ????    
                        {   
                            pDataOut = *pBytes;    
                            cb = 1; 
                        }   
                        // Medium.  
                        else if ((*pBytes & 0xC0) == 0x80)  // 10?? ????    
                        {   
                            pDataOut = (uint)(((*pBytes & 0x3f) << 8 | *(pBytes+1)));  
                            cb = 2; 
                        }   
                        else if ((*pBytes & 0xE0) == 0xC0)      // 110? ????    
                        {   
                            pDataOut = (uint)(((*pBytes & 0x1f) << 24 | *(pBytes+1) << 16 | *(pBytes+2) << 8 | *(pBytes+3)));  
                            cb = 4; 
                        }   
                        return cb;  
                    }
                }*/

        static uint[] g_tkCorEncodeToken = { (uint)MetadataTokenType.TypeDef, (uint)MetadataTokenType.TypeRef, (uint)MetadataTokenType.TypeSpec, (uint)MetadataTokenType.BaseType };

        // uncompress a token
        internal static uint CorSigUncompressToken(   // return the token.    
            ref IntPtr pData)             // [IN,OUT] compressed data 
        {
            uint tk;
            uint tkType;

            tk = CorSigUncompressData(ref pData);
            tkType = g_tkCorEncodeToken[tk & 0x3];
            tk = TokenFromRid(tk >> 2, tkType);
            return tk;
        }


        // Function translated directly from cor.h but never tested; included here in case someone wants to use it in future
        /*        internal static uint CorSigUncompressToken(     // return number of bytes of that compressed data occupied in pData 
                    IntPtr pData,              // [IN] compressed data 
                    out uint     pToken)                // [OUT] the expanded *pData    
                {
                    uint       cb; 
                    uint     tk; 
                    uint     tkType; 

                    cb = CorSigUncompressData(pData, out tk); 
                    tkType = g_tkCorEncodeToken[tk & 0x3];  
                    tk = TokenFromRid(tk >> 2, tkType); 
                    pToken = tk;   
                    return cb;  
                }*/

        internal static CorCallingConvention CorSigUncompressCallingConv(
            ref IntPtr pData)             // [IN,OUT] compressed data 
        {
            unsafe
            {
                byte* pBytes = (byte*)pData;
                CorCallingConvention retval = (CorCallingConvention)(*pBytes++);
                pData = (IntPtr)pBytes;
                return retval;
            }
        }

        // Function translated directly from cor.h but never tested; included here in case someone wants to use it in future
        /*        private enum SignMasks : uint {
                    ONEBYTE  = 0xffffffc0,        // Mask the same size as the missing bits.  
                    TWOBYTE  = 0xffffe000,        // Mask the same size as the missing bits.  
                    FOURBYTE = 0xf0000000,        // Mask the same size as the missing bits.  
                };

                // uncompress a signed integer
                internal static uint CorSigUncompressSignedInt( // return number of bytes of that compressed data occupied in pData
                    IntPtr pData,              // [IN] compressed data 
                    out int         pInt)                  // [OUT] the expanded *pInt 
                {
                    uint       cb; 
                    uint       ulSigned;   
                    uint       iData;  

                    cb = CorSigUncompressData(pData, out iData);
                    pInt = 0;
                    if (cb == 0xffffffff) return cb;
                    ulSigned = iData & 0x1; 
                    iData = iData >> 1; 
                    if (ulSigned != 0)   
                    {   
                        if (cb == 1)    
                        {   
                            iData |= (uint)SignMasks.ONEBYTE; 
                        }   
                        else if (cb == 2)   
                        {   
                            iData |= (uint)SignMasks.TWOBYTE; 
                        }   
                        else    
                        {   
                            iData |= (uint)SignMasks.FOURBYTE;    
                        }   
                    }   
                    pInt = (int)iData;  
                    return cb;  
                }*/


        // uncompress encoded element type
        internal static CorElementType CorSigUncompressElementType(//Element type
            ref IntPtr pData)             // [IN,OUT] compressed data 
        {
            unsafe
            {
                byte* pBytes = (byte*)pData;

                CorElementType retval = (CorElementType)(*pBytes++);
                pData = (IntPtr)pBytes;
                return retval;
            }
        }

        // Function translated directly from cor.h but never tested; included here in case someone wants to use it in future
        /*        internal static uint CorSigUncompressElementType(// return number of bytes of that compressed data occupied in pData
                    IntPtr pData,              // [IN] compressed data 
                    out CorElementType pElementType)       // [OUT] the expanded *pData    
                {  
                    unsafe
                    {
                        byte *pBytes = (byte*)pData;
                        pElementType = (CorElementType)(*pBytes & 0x7f);    
                        return 1;   
                    }
                }*/

        static internal string[] GetGenericArgumentNames(IMetadataImport importer,
                                                int typeOrMethodToken)
        {
            IMetadataImport2 importer2 = (importer as IMetadataImport2);
            if (importer2 == null)
                return new string[0]; // this means we're pre v2.0 debuggees.

            Debug.Assert(importer2 != null);

            string[] genargs = null;

            IntPtr hEnum = IntPtr.Zero;
            try
            {
                int i = 0;
                do
                {
                    uint nOut;
                    int genTypeToken;
                    importer2.EnumGenericParams(ref hEnum, typeOrMethodToken,
                                                out genTypeToken, 1, out nOut);
                    if (genargs == null)
                    {
                        int count;
                        importer.CountEnum(hEnum, out count);
                        genargs = new string[count];
                    }
                    if (nOut == 0)
                        break;

                    Debug.Assert(nOut == 1);
                    if (nOut == 1)
                    {
                        uint genIndex;
                        int genFlags, ptkOwner, ptkKind;
                        ulong genArgNameSize;

                        importer2.GetGenericParamProps(genTypeToken,
                                                       out genIndex,
                                                       out genFlags,
                                                       out ptkOwner,
                                                       out ptkKind,
                                                       null,
                                                       0,
                                                       out genArgNameSize);
                        StringBuilder genArgName = new StringBuilder((int)genArgNameSize);
                        importer2.GetGenericParamProps(genTypeToken,
                                                       out genIndex,
                                                       out genFlags,
                                                       out ptkOwner,
                                                       out ptkKind,
                                                       genArgName,
                                                       (ulong)genArgName.Capacity,
                                                       out genArgNameSize);

                        genargs[i] = genArgName.ToString();
                    }
                    ++i;
                } while (i < genargs.Length);
            }
            finally
            {
                if (hEnum != IntPtr.Zero)
                    importer2.CloseEnum(hEnum);
            }
            return genargs;
        }
    }

    public abstract class TokenUtils
    {
        public static CorTokenType TypeFromToken(int token)
        {
            return (CorTokenType)((UInt32)token & 0xff000000);
        }

        public static int RidFromToken(int token)
        {
            return (int)((UInt32)token & 0x00ffffff);
        }

        public static bool IsNullToken(int token)
        {
            return (RidFromToken(token) == 0);
        }
    }
}
