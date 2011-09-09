using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using slinject.Native.Debug.Interfaces;

namespace slinject.Native.Debug
{
    public static class DebugMethods
    {
        /// <summary>
        /// HRESULT EnumerateCLRs (
        ///   [in]  DWORD      debuggeePID,
        ///   [out] HANDLE**   ppHandleArrayOut,
        ///   [out] LPWSTR**   ppStringArrayOut,
        ///   [out] DWORD*     pdwArrayLengthOut
        /// );
        /// </summary>
        [DllImport(InjectorSettings.DbgShimLibraryName, CharSet = CharSet.Auto, PreserveSig = false)]
        public static extern void EnumerateCLRs(
            Int32 pid,
            out IntPtr[] ppHandleArrayOut,
            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr)] out string[] clrNames,
            out Int32 pdwArrayLengthOut);


        /// <summary>
        /// HRESULT CreateVersionStringFromModule (
        ///   [in]  DWORD      pidDebuggee,
        ///   [in]  LPCWSTR    szModuleName,
        ///   [out, size_is(cchBuffer),
        ///   length_is(*pdwLength)] LPWSTR Buffer,
        ///   [in]  DWORD      cchBuffer,
        ///   [out] DWORD*     pdwLength
        /// );
        /// </summary>
        [DllImport(InjectorSettings.DbgShimLibraryName, CharSet = CharSet.Auto, PreserveSig = false)]
        public static extern void CreateVersionStringFromModule(Int32 pidDebuggee, string szModuleName, StringBuilder buffer, Int32 bufferSize, out Int32 allocatedInBuffer);

        /// <summary>
        /// HRESULT CreateDebuggingInterfaceFromVersion (
        /// [in]  LPCWSTR      szDebuggeeVersion,
        /// [out] IUnknown**   ppCordb,
        /// );
        /// </summary>
        [DllImport(InjectorSettings.DbgShimLibraryName, CharSet = CharSet.Auto, PreserveSig = false)]
        public static extern ICorDebug CreateDebuggingInterfaceFromVersion(string debugeeVersion);

        /// <summary>
        /// TODO: This function does not work.
        /// HRESULT CloseCLREnumeration (
        ///   [in]  DWORD      pHandleArray,
        ///   [in]  LPWSTR**   pStringArray,
        ///   [in]  DWORD*     dwArrayLength
        /// );
        /// </summary>
        [DllImport(InjectorSettings.DbgShimLibraryName, CharSet = CharSet.Auto, PreserveSig = false)]
        public static extern void CloseCLREnumeration(
            ref IntPtr[] pHandleArray,
            ref string[] pStringArray,
            ref Int32 dwArrayLength);

    }
}
