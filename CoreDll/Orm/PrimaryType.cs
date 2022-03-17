using System;

namespace CoreDll.Orm
{
    /// <summary>
    /// Esta classe simplifica ao expor o tipo elementar de Type como sua base, pois Type se tornou confuso 
    /// ao lidar com tipos mais recentes do framework, como Nullable<> e Generics.
    /// Exempo do problema: para Type, um simples "int?" não é um inteiro, mas um Nullable<int>, assim 
    /// como um TipoExemploEnum? não é um enum, mas um Nullable<TipoExemploEnum>. Type trata
    /// esses novos tipos de uma forma indireta, tornando a manipulação dos novos tipos completamente diferente da
    /// forma que lidamos com os tipos primitivos e enums "normais" (int, bool, etc...) e aumentando a complexidade
    /// do tratamento.
    /// </summary>
    public class PrimaryType
    {
        /// <summary>
        /// The type as its original type.
        /// </summary>
        public Type OriginalType { get; private set; }

        /// <summary>
        /// Primary or essential type.
        /// </summary>
        public Type Type { get; private set; }


        /// <summary>
        /// If the instance type accepts null value
        /// </summary>
        public bool IsNullable { get; private set; }
        public bool IsGeneric { get { return Type.IsGenericType; } }

        public bool IsValueType { get { return Type.IsValueType; } }
        public bool IsReferenceType { get { return !IsValueType; } }


        public bool IsPrimitiveType { get { return Type.IsPrimitive; } }
        public bool IsEnum { get { return Type.IsEnum; } }


        public PrimaryType(Type originalType)
        {
            OriginalType = originalType;
            Type = Nullable.GetUnderlyingType(originalType) == null ? originalType : Nullable.GetUnderlyingType(originalType);
            IsNullable = Nullable.GetUnderlyingType(originalType) != null || IsReferenceType;
        }

        public static PrimaryType GetPrimaryType(Type type)
        {
            return new PrimaryType(type);
        }
    }
}