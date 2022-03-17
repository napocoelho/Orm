using System;
using System.Linq;
using CoreDll.Extensions;



namespace CoreDll.Orm
{
    public static class TypeHelper
    {
        private static System.Globalization.CultureInfo DefaultCulture { get; set; } = new System.Globalization.CultureInfo("en-US", false);
        //private static NumberFormatInfo NumberFormat { get; set; } = new NumberFormatInfo();

        #region Todas as conversões possíveis de tipos para sql (conversões com nulos deverão ser definidas por fora)
        private static Func<object, string> QuotedValueToSql = (value) => "'" + value.ToString() + "'";
        private static Func<object, string> BooleanValueToSql = (value) => ((bool)value) ? "1" : "0";
        private static Func<object, string> EnumValueToSql = (value) => value.GetHashCode().ToString();
        private static Func<object, string> NumericValueToSql = (value) => value.ToString().Replace(",", ".");

        private static Func<object, string> NullableQuotedValueToSql = (value) => value.IsNull() ? "null" : QuotedValueToSql(value);
        private static Func<object, string> NullableBooleanValueToSql = (value) => value.IsNull() ? "null" : BooleanValueToSql(value);
        private static Func<object, string> NullableEnumValueToSql = (value) => value.IsNull() ? "null" : EnumValueToSql(value);
        private static Func<object, string> NullableNumericValueToSql = (value) => value.IsNull() ? "null" : NumericValueToSql(value);
        #endregion

        /*
        #region Todas as conversões possíveis de tipos para value (conversões com nulos deverão ser definidas por fora)
        private static Func<Type, string, object> EnumSqlToValue = (type, value) =>
                        {
                            System.ComponentModel.TypeConverter convert = System.ComponentModel.TypeDescriptor.GetConverter(type);
                            return convert.ConvertFrom(value.ToString());
                        };
        private static Func<Type, string, object> NonEnumSqlToValue = (type, value) =>
        {
            System.ComponentModel.TypeConverter convert = System.ComponentModel.TypeDescriptor.GetConverter(type);
            return convert.ConvertFrom(value.ToString());
        };
        #endregion
        */

        private static Func<object, string>[] toSqlConversions = {
                                 QuotedValueToSql, QuotedValueToSql,  QuotedValueToSql,
                                 BooleanValueToSql,
                                 NumericValueToSql, NumericValueToSql,
                                 NumericValueToSql, NumericValueToSql, NumericValueToSql, NumericValueToSql,
                                 NumericValueToSql, NumericValueToSql, NumericValueToSql, EnumValueToSql
                             };

        private static Func<object, string>[] toNullableSqlConversions = {
                                 NullableQuotedValueToSql, NullableQuotedValueToSql,  NullableQuotedValueToSql,
                                 NullableBooleanValueToSql,
                                 NullableNumericValueToSql, NullableNumericValueToSql,
                                 NullableNumericValueToSql, NullableNumericValueToSql, NullableNumericValueToSql, NullableNumericValueToSql,
                                 NullableNumericValueToSql, NullableNumericValueToSql, NullableNumericValueToSql, NullableEnumValueToSql
                             };

        private static Type[] csTypes = {
                                 typeof(Char), typeof(DateTime),  typeof(String),
                                 typeof(Boolean),
                                 typeof(Decimal), typeof(Double),
                                 typeof(Int16), typeof(Int32), typeof(Int64), typeof(Single),
                                 typeof(UInt16), typeof(UInt32), typeof(UInt64), typeof(Enum)
                             };

        private static TypeCode[] typeCodes = {
                                       TypeCode.Char, TypeCode.DateTime, TypeCode.String,
                                       TypeCode.Boolean,
                                       TypeCode.Decimal, TypeCode.Double,
                                       TypeCode.Int16, TypeCode.Int32, TypeCode.Int64, TypeCode.Single ,
                                       TypeCode.UInt16, TypeCode.UInt32, TypeCode.UInt64, TypeCode.Int32
                                   };

        private static bool[] sqlQuotedType = {
                                true, true, true,
                                false,
                                false, false,
                                false, false, false, false,
                                false, false, false, false
                            };

        private enum TypeCodeIndexEnum
        {
            Char = 0, DateTime = 1, String = 2, Bool = 3, Decimal = 4, Double = 5, Int16 = 6, Int32 = 7, Int64 = 8, Single = 9, UInt16 = 10, UInt32 = 11, UInt64 = 12, Enum = 13
        }




        public static Func<object, string> ConvertionToSqlFunc(PrimaryType type, bool databaseColumnIsNullable = true)
        {


            if (type.IsEnum)
            {
                if (databaseColumnIsNullable)
                {
                    //Func<object, string> nullableFunc = (value) => value.IsNull() ? "null" : EnumValueToSql.ToString();
                    return toNullableSqlConversions[TypeCodeIndexEnum.Enum.GetHashCode()];
                }
                else
                {
                    return EnumValueToSql;
                }
            }
            else
            {
                for (int idx = 0; idx < csTypes.Count(); idx++)
                {
                    if (type.Type == csTypes[idx])
                    {
                        if (databaseColumnIsNullable)
                        {
                            //Func<object, string> nullableFunc = (value) => value.IsNull() ? "null" : toSqlConversions[idx].ToString();
                            return toNullableSqlConversions[idx];
                        }
                        else
                        {
                            return toSqlConversions[idx];
                        }
                    }
                }
            }

            throw new NotImplementedException($"The type '{type.Type.Name}' is a not implemented type!");
        }

        /*
        public static Func<Type, string, object> ConvertionFromSqlFunc(PrimaryType type, bool databaseColumnIsNullable = true)
        {
            
        }
        */

        public static bool IsPermitedType(PrimaryType type)
        {
            for (int idx = 0; idx < csTypes.Count(); idx++)
            {
                if (type.Type == csTypes[idx] || type.IsEnum)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsSqlQuoted(PrimaryType type)
        {
            if (type.IsEnum)
            {
                return sqlQuotedType[TypeCodeIndexEnum.Enum.GetHashCode()];
            }
            else
            {
                for (int idx = 0; idx < csTypes.Count(); idx++)
                {
                    if (type.Type == csTypes[idx])
                    {
                        return sqlQuotedType[idx];
                    }
                }
            }


            throw new NotImplementedException();
        }

        public static TypeCode GetTypeCodeOf(PrimaryType type)
        {
            PrimaryType xType = type.IsEnum ? new PrimaryType(typeof(int)) : type;

            for (int idx = 0; idx < csTypes.Count(); idx++)
            {
                if (xType.Type == csTypes[idx])
                {
                    return typeCodes[idx];
                }
            }

            throw new NotImplementedException();
        }
    }
}