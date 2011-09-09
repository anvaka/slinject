//---------------------------------------------------------------------
//  This file is based on the CLR Managed Debugger (mdbg) Sample.
// 
//  Which is copyrighted by (C) Microsoft Corporation.  All rights reserved.
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using slinject.Native.Debug.Interfaces;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.Collections;
using System.Globalization;
using System.Diagnostics;

namespace slinject.Debugger.Metadata
{
    public sealed class MetadataParameterInfo : ParameterInfo
    {
        internal MetadataParameterInfo(IMetadataImport importer, int paramToken,
                                       MemberInfo memberImpl, Type typeImpl)
        {
            int parentToken;
            uint pulSequence, pdwAttr, pdwCPlusTypeFlag, pcchValue, size;

            IntPtr ppValue;
            importer.GetParamProps(paramToken,
                                   out parentToken,
                                   out pulSequence,
                                   null,
                                   0,
                                   out size,
                                   out pdwAttr,
                                   out pdwCPlusTypeFlag,
                                   out ppValue,
                                   out pcchValue
                                   );
            StringBuilder szName = new StringBuilder((int)size);
            importer.GetParamProps(paramToken,
                                   out parentToken,
                                   out pulSequence,
                                   szName,
                                   (uint)szName.Capacity,
                                   out size,
                                   out pdwAttr,
                                   out pdwCPlusTypeFlag,
                                   out ppValue,
                                   out pcchValue
                                   );
            NameImpl = szName.ToString();
            ClassImpl = typeImpl;
            PositionImpl = (int)pulSequence;
            AttrsImpl = (ParameterAttributes)pdwAttr;

            MemberImpl = memberImpl;
        }

        private MetadataParameterInfo(SerializationInfo info, StreamingContext context)
        {

        }

        public override String Name
        {
            get
            {
                return NameImpl;
            }
        }

        public override int Position
        {
            get
            {
                return PositionImpl;
            }
        }
    }

    public sealed class MetadataFieldInfo : FieldInfo
    {
        internal MetadataFieldInfo(IMetadataImport importer, int fieldToken, MetadataType declaringType)
        {
            m_importer = importer;
            m_fieldToken = fieldToken;
            m_declaringType = declaringType;

            // Initialize
            int mdTypeDef;
            int pchField, pcbSigBlob, pdwCPlusTypeFlab, pcchValue, pdwAttr;
            IntPtr ppvSigBlob;
            IntPtr ppvRawValue;
            m_importer.GetFieldProps(m_fieldToken,
                                     out mdTypeDef,
                                     null,
                                     0,
                                     out pchField,
                                     out pdwAttr,
                                     out ppvSigBlob,
                                     out pcbSigBlob,
                                     out pdwCPlusTypeFlab,
                                     out ppvRawValue,
                                     out pcchValue
                                     );

            StringBuilder szField = new StringBuilder(pchField);
            m_importer.GetFieldProps(m_fieldToken,
                                     out mdTypeDef,
                                     szField,
                                     szField.Capacity,
                                     out pchField,
                                     out pdwAttr,
                                     out ppvSigBlob,
                                     out pcbSigBlob,
                                     out pdwCPlusTypeFlab,
                                     out ppvRawValue,
                                     out pcchValue
                                     );
            m_fieldAttributes = (FieldAttributes)pdwAttr;
            m_name = szField.ToString();

            // Get the values for static literal fields with primitive types
            FieldAttributes staticLiteralField = FieldAttributes.Static | FieldAttributes.HasDefault | FieldAttributes.Literal;
            if ((m_fieldAttributes & staticLiteralField) == staticLiteralField)
            {
                m_value = ParseDefaultValue(declaringType, ppvSigBlob, ppvRawValue);
            }
        }

        private static object ParseDefaultValue(MetadataType declaringType, IntPtr ppvSigBlob, IntPtr ppvRawValue)
        {
            IntPtr ppvSigTemp = ppvSigBlob;
            CorCallingConvention callingConv = MetadataHelperFunctions.CorSigUncompressCallingConv(ref ppvSigTemp);
            Debug.Assert(callingConv == CorCallingConvention.Field);

            CorElementType elementType = MetadataHelperFunctions.CorSigUncompressElementType(ref ppvSigTemp);
            if (elementType == CorElementType.ELEMENT_TYPE_VALUETYPE)
            {
                uint token = MetadataHelperFunctions.CorSigUncompressToken(ref ppvSigTemp);

                if (token == declaringType.MetadataToken)
                {
                    // Static literal field of the same type as the enclosing type
                    // may be one of the value fields of an enum
                    if (declaringType.ReallyIsEnum)
                    {
                        // If so, the value will be of the enum's underlying type,
                        // so we change it from VALUETYPE to be that type so that
                        // the following code will get the value
                        elementType = declaringType.EnumUnderlyingType;
                    }
                }
            }

            switch (elementType)
            {
                case CorElementType.ELEMENT_TYPE_CHAR:
                    return (char)Marshal.ReadByte(ppvRawValue);
                case CorElementType.ELEMENT_TYPE_I1:
                    return (sbyte)Marshal.ReadByte(ppvRawValue);
                case CorElementType.ELEMENT_TYPE_U1:
                    return Marshal.ReadByte(ppvRawValue);
                case CorElementType.ELEMENT_TYPE_I2:
                    return Marshal.ReadInt16(ppvRawValue);
                case CorElementType.ELEMENT_TYPE_U2:
                    return (ushort)Marshal.ReadInt16(ppvRawValue);
                case CorElementType.ELEMENT_TYPE_I4:
                    return Marshal.ReadInt32(ppvRawValue);
                case CorElementType.ELEMENT_TYPE_U4:
                    return (uint)Marshal.ReadInt32(ppvRawValue);
                case CorElementType.ELEMENT_TYPE_I8:
                    return Marshal.ReadInt64(ppvRawValue);
                case CorElementType.ELEMENT_TYPE_U8:
                    return (ulong)Marshal.ReadInt64(ppvRawValue);
                case CorElementType.ELEMENT_TYPE_I:
                    return Marshal.ReadIntPtr(ppvRawValue);
                case CorElementType.ELEMENT_TYPE_U:
                case CorElementType.ELEMENT_TYPE_R4:
                case CorElementType.ELEMENT_TYPE_R8:
                // Technically U and the floating-point ones are options in the CLI, but not in the CLS or C#, so these are NYI
                default:
                    return null;
            }
        }

        public override Object GetValue(Object obj)
        {
            FieldAttributes staticLiteralField = FieldAttributes.Static | FieldAttributes.HasDefault | FieldAttributes.Literal;
            if ((m_fieldAttributes & staticLiteralField) != staticLiteralField)
            {
                throw new InvalidOperationException("Field is not a static literal field.");
            }
            if (m_value == null)
            {
                throw new NotImplementedException("GetValue not implemented for the given field type.");
            }
            else
            {
                return m_value;
            }
        }

        public override void SetValue(Object obj, Object value, BindingFlags invokeAttr, Binder binder, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override Object[] GetCustomAttributes(bool inherit)
        {
            throw new NotImplementedException();
        }

        public override Object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            throw new NotImplementedException();
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            throw new NotImplementedException();
        }


        public override Type FieldType
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override RuntimeFieldHandle FieldHandle
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override FieldAttributes Attributes
        {
            get
            {
                return m_fieldAttributes;
            }
        }

        public override MemberTypes MemberType
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override String Name
        {
            get
            {
                return m_name;
            }
        }

        public override Type DeclaringType
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override Type ReflectedType
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override int MetadataToken
        {
            get
            {
                return m_fieldToken;
            }
        }

        private IMetadataImport m_importer;
        private int m_fieldToken;
        private MetadataType m_declaringType;

        private string m_name;
        private FieldAttributes m_fieldAttributes;
        private Object m_value;
    }

    public sealed class MetadataMethodInfo : MethodInfo
    {
        internal MetadataMethodInfo(IMetadataImport importer, int methodToken)
        {
            if (!importer.IsValidToken((uint)methodToken))
                throw new ArgumentException();

            m_importer = importer;
            m_methodToken = methodToken;

            int size;
            uint pdwAttr;
            IntPtr ppvSigBlob;
            uint pulCodeRVA, pdwImplFlags;
            uint pcbSigBlob;

            m_importer.GetMethodProps((uint)methodToken,
                                      out m_classToken,
                                      null,
                                      0,
                                      out size,
                                      out pdwAttr,
                                      out ppvSigBlob,
                                      out pcbSigBlob,
                                      out pulCodeRVA,
                                      out pdwImplFlags);

            StringBuilder szMethodName = new StringBuilder(size);
            m_importer.GetMethodProps((uint)methodToken,
                                    out m_classToken,
                                    szMethodName,
                                    szMethodName.Capacity,
                                    out size,
                                    out pdwAttr,
                                    out ppvSigBlob,
                                    out pcbSigBlob,
                                    out pulCodeRVA,
                                    out pdwImplFlags);

            m_name = szMethodName.ToString();
            m_methodAttributes = (MethodAttributes)pdwAttr;
        }

        public override Type ReturnType
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override ICustomAttributeProvider ReturnTypeCustomAttributes
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override Type ReflectedType
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override Type DeclaringType
        {
            get
            {
                if (TokenUtils.IsNullToken(m_classToken))
                    return null;                            // this is method outside of class

                return new MetadataType(m_importer, m_classToken);
            }
        }

        public override string Name
        {
            get
            {
                return m_name;
            }
        }

        public override MethodAttributes Attributes
        {
            get
            {
                return m_methodAttributes;
            }
        }

        public override RuntimeMethodHandle MethodHandle
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override MethodInfo GetBaseDefinition()
        {
            throw new NotImplementedException();
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            throw new NotImplementedException();
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            throw new NotImplementedException();
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            throw new NotImplementedException();
        }

        public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override System.Reflection.MethodImplAttributes GetMethodImplementationFlags()
        {
            throw new NotImplementedException();
        }

        public override System.Reflection.ParameterInfo[] GetParameters()
        {
            ArrayList al = new ArrayList();
            IntPtr hEnum = new IntPtr();
            try
            {
                while (true)
                {
                    uint count;
                    int paramToken;
                    m_importer.EnumParams(ref hEnum,
                                          m_methodToken, out paramToken, 1, out count);
                    if (count != 1)
                        break;
                    al.Add(new MetadataParameterInfo(m_importer, paramToken,
                                                     this, DeclaringType));
                }
            }
            finally
            {
                m_importer.CloseEnum(hEnum);
            }
            return (ParameterInfo[])al.ToArray(typeof(ParameterInfo));
        }

        public override int MetadataToken
        {
            get
            {
                return m_methodToken;
            }
        }

        public string[] GetGenericArgumentNames()
        {
            return MetadataHelperFunctions.GetGenericArgumentNames(m_importer, m_methodToken);
        }

        private IMetadataImport m_importer;
        private string m_name;
        private int m_classToken;
        private int m_methodToken;
        private MethodAttributes m_methodAttributes;
    }

    // Struct isn't complete yet; just here for the IsTokenOfType method
    public struct MetadataToken
    {
        public MetadataToken(int value)
        {
            this.value = value;
        }

        public int Value
        {
            get
            {
                return value;
            }
        }

        public MetadataTokenType Type
        {
            get
            {
                return (MetadataTokenType)(value & 0xFF000000);
            }
        }

        public int Index
        {
            get
            {
                return value & 0x00FFFFFF;
            }
        }

        public static implicit operator int(MetadataToken token) { return token.value; }
        public static bool operator ==(MetadataToken v1, MetadataToken v2) { return (v1.value == v2.value); }
        public static bool operator !=(MetadataToken v1, MetadataToken v2) { return !(v1 == v2); }

        public static bool IsTokenOfType(int token, params MetadataTokenType[] types)
        {
            for (int i = 0; i < types.Length; i++)
            {
                if ((int)(token & 0xFF000000) == (int)types[i])
                    return true;
            }

            return false;
        }

        public bool IsOfType(params MetadataTokenType[] types) { return IsTokenOfType(Value, types); }

        public override bool Equals(object other)
        {
            if (other is MetadataToken)
            {
                MetadataToken oToken = (MetadataToken)other;
                return (value == oToken.value);
            }
            return false;
        }

        public override int GetHashCode() { return value.GetHashCode(); }

        private int value;
    }

}
