using System;
using System.Xml;
using System.Xml.Linq;



namespace CoreDll.Extensions.Conversions
{
    public static class Conversions
    {
        ///// <summary>
        ///// Mescla o valor das propriedades de dois objetos quaisquer, ignorando suas respectivas classes.
        ///// As propriedades devem possuir mesmo nome (Case Sensitive) e mesmo tipo.
        ///// </summary>
        //public static void MergeTo<TSource, TTarget>(this TSource source, TTarget target)
        //{
        //    if (source is null || target is null)
        //        return;

        //    foreach (PropertyInfo sourceProperty in typeof(TSource).GetProperties())
        //    {
        //        foreach (PropertyInfo targetProperty in typeof(TTarget).GetProperties())
        //        {
        //            if (sourceProperty.Name.Equals(targetProperty.Name, StringComparison.Ordinal)
        //                && sourceProperty.PropertyType.Equals(targetProperty.PropertyType))
        //            {
        //                object value = sourceProperty.GetValue(source);
        //                targetProperty.SetValue(target, value);
        //            }
        //        }
        //    }
        //}

        public static T ToEnum<T>(this string enumString) where T : struct
        {
            return (T)Enum.Parse(typeof(T), enumString);
        }

        public static T ToEnum<T>(this object enumObject)
        {
            Type type = typeof(T);

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                if (enumObject.IsNull())
                {
                    return default(T);
                }
                else
                {
                    int intValue;
                    T value = default(T);

                    if (int.TryParse(enumObject.ToString(), out intValue))
                    {
                        //int intValue = int.Parse(enumObject.ToString());
                        System.ComponentModel.TypeConverter convert = System.ComponentModel.TypeDescriptor.GetConverter(type);
                        value = (T)convert.ConvertFrom(intValue.ToString());
                    }

                    return value;
                }
            }
            else
            {
                long valueObj;

                if (!(enumObject as Enum).IsNull())
                {
                    valueObj = enumObject.GetHashCode();
                }
                else
                {
                    valueObj = long.Parse(enumObject.ToString());
                }

                T value = (T)Enum.ToObject(typeof(T), valueObj);
                return value;
            }
        }

        public static T? ToEnumN<T>(this string enumString) where T : struct
        {
            if (string.IsNullOrEmpty(enumString))
            {
                return null;
            }
            else
            {
                return (T)Enum.Parse(typeof(T), enumString);
            }
        }

        public static T? ToEnumN<T>(this object enumObject) where T : struct
        {
            Type type = typeof(T);

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                if (enumObject.IsNull())
                {
                    return default(T);
                }
                else
                {
                    int intValue;
                    T value = default(T);

                    if (int.TryParse(enumObject.ToString(), out intValue))
                    {
                        //int intValue = int.Parse(enumObject.ToString());
                        System.ComponentModel.TypeConverter convert = System.ComponentModel.TypeDescriptor.GetConverter(type);
                        value = (T)convert.ConvertFrom(intValue.ToString());
                    }

                    return value;
                }
            }
            else
            {
                long valueObj;

                if (enumObject.IsNull())
                {
                    return null;
                }
                else if (!(enumObject as Enum).IsNull())
                {
                    valueObj = enumObject.GetHashCode();
                }
                else
                {
                    valueObj = long.Parse(enumObject.ToString());
                }

                T value = (T)Enum.ToObject(typeof(T), valueObj);
                return value;
            }
        }

        public static int? ToIntN(this object expression, int? defaultValue = null)
        {
            try
            {
                return Convert.ToInt32(expression);
            }
            catch
            {
                return defaultValue;
            }
        }

        public static int? ToIntN(this string expression, int? defaultValue = null)
        {
            try
            {
                return Convert.ToInt32(expression);
            }
            catch
            {
                return defaultValue;
            }
        }

        public static decimal? ToDecimalN(this object expression, decimal? defaultValue = null)
        {
            try
            {
                return Convert.ToDecimal(expression);
            }
            catch
            {
                return defaultValue;
            }
        }

        public static decimal? ToDecimalN(this string expression, decimal? defaultValue = null)
        {
            try
            {
                return decimal.Parse(expression.Replace(",", "."));
            }
            catch
            {
                return defaultValue;
            }
        }

        public static bool? ToBoolN(this string expression, bool? defaultValue = null)
        {
            if (expression.IsNullOrEmpty())
                return defaultValue;

            try
            {
                return expression.ToBool();
            }
            catch
            {
                return defaultValue;
            }
        }

        public static bool? ToBoolN(this object expression, bool? defaultValue = null)
        {
            if (expression.IsNull())
                return defaultValue;

            try
            {
                return expression.ToBool();
            }
            catch
            {
                return defaultValue;
            }
        }


        public static string ToString(this object expression, string defaultIfNull = "")
        {
            try
            {
                return expression.ToString();
            }
            catch //(Exception ex)
            {
                return defaultIfNull;
            }
        }

        public static int ToInt(this object expression, int defaultValue = 0)
        {
            try
            {
                return Convert.ToInt32(expression);
            }
            catch
            {
                return defaultValue;
            }
        }

        public static int ToInt(this string expression, int defaultValue = 0)
        {
            if (expression.IsNullOrEmpty())
                return defaultValue;

            try
            {
                return Int32.Parse(expression);
            }
            catch
            {
                return defaultValue;
            }
        }

        public static bool ToBool(this string expression)
        {
            bool retorno;

            if (expression.IsNullOrEmpty())
                return false;

            if (!bool.TryParse(expression, out retorno))
            {
                retorno = BoolParse(expression);
            }

            return retorno;
        }

        public static int ToInt(this Enum value)
        {
            return value.GetHashCode();
        }

        public static int? ToIntN(this Enum? value)
        {
            return value?.GetHashCode();
        }

        private static string FalseRegexPattern = "^(0|false|f|n|no|nao|não){1}$";
        private static string TrueRegexPattern = "^(1|true|t|y|yes|s|sim){1}$";

        private static bool BoolParse(string expression)
        {
            try
            {
                if (expression.IsNullOrEmpty())
                    return false;
                else if (System.Text.RegularExpressions.Regex.IsMatch(expression, FalseRegexPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                    return false;
                else if (System.Text.RegularExpressions.Regex.IsMatch(expression, TrueRegexPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                    return true;
                else
                    return false;
            }
            catch
            {
                return false;
            }
        }

        public static bool ToBool(this object expression)
        {
            if (expression.IsNull())
                return false;

            try
            {
                return Convert.ToBoolean(expression);
            }
            catch
            {
                return ToBool(expression.ToString());
            }
        }

        public static decimal ToDecimal(this string expression, decimal defaultIfEmptyOrNullOrInvalid = 0)
        {
            try
            {
                if (System.Threading.Thread.CurrentThread.CurrentUICulture.NumberFormat.NumberDecimalSeparator == ",")
                {
                    return decimal.Parse(expression.Replace(".", ","));
                }
                else
                {
                    return decimal.Parse(expression.Replace(",", "."));
                }
            }
            catch
            {
                return defaultIfEmptyOrNullOrInvalid;
            }
        }

        public static decimal ToDecimal(this object expression, decimal defaultIfEmptyOrNullOrInvalid = 0)
        {
            try
            {
                return Convert.ToDecimal(expression);
            }
            catch
            {
                return defaultIfEmptyOrNullOrInvalid;
            }
        }

        public static decimal ToDouble(this string expression, decimal defaultValue = 0)
        {
            try
            {
                return decimal.Parse(expression.Replace(",", "."));
            }
            catch
            {
                return defaultValue;
            }
        }

        public static decimal ToDouble(this object expression, decimal defaultValue = 0)
        {
            try
            {
                return Convert.ToDecimal(expression);
            }
            catch
            {
                return defaultValue;
            }
        }

        public static DateTime ToDateTime(this string expression, DateTime defaultIfInvalid = default(DateTime))
        {
            DateTime retorno;

            if (DateTime.TryParse(expression, out retorno))
            {
                return retorno;
            }

            return defaultIfInvalid;
        }

        public static DateTime ToDateTime(this object expression)
        {
            return ToDateTime(expression.ToText());
        }

        public static DateTime? ToDateTimeN(this string expression)
        {
            DateTime retorno;

            if (DateTime.TryParse(expression, out retorno))
            {
                return retorno;
            }

            return null;
        }

        public static DateTime? ToDateTimeN(this object expression)
        {
            DateTime? retorno = null;
            DateTime tryValue;

            if (DateTime.TryParse(expression.ToText(), out tryValue))
            {
                retorno = tryValue;
            }

            return retorno;
        }

        public static string ToStringN(this object expression)
        {
            string retorno = null;

            try
            {
                retorno = expression.IsNull() ? null : expression.ToString();
            }
            catch
            { }

            return retorno;
        }

        /// <summary>
        /// Always returns a not null string. If it was null (or DB.Null), returns String.Empty.
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static string ToText(this object expression, string ifIsNull = "", string ifIsEmpty = "")
        {
            if (expression.IsNull())
                return ifIsNull ?? string.Empty;

            string @return = expression.ToString();

            if (@return.IsEmpty())
                return ifIsEmpty ?? string.Empty;

            return @return;
        }

        /*
        public static NfeCsDll.Models.Estado ToUF(this object expression)
        {
            NfeCsDll.Models.Estado uf = null;

            try
            {
                if (expression.IsNumeric())
                {
                    uf = NfeCsDll.Models.Estado.GetByCodigoIbge(expression.ToInt());
                }
                else
                {
                    uf = NfeCsDll.Models.Estado.GetByUF(expression.ToString());
                }
            }
            catch (Exception ex)
            { }

            return uf;
        }
        */

        /// <summary>
        /// Converte o XDocument atual em um XmlDocument.
        /// </summary>
        /// <returns>Retorna um objeto do tipo XmlDocument.</returns>
        public static XmlDocument ToXmlDocument(this XDocument SelfObj)
        {

            XmlDocument xmlDoc = new XmlDocument();

            xmlDoc.Load(SelfObj.CreateReader());

            return xmlDoc;
        }

        /// <summary>
        /// Transforma um XmlElement em um XmlDocument.
        /// A tag inicial do documento é sempre a [<?xml version="1.0" encoding="utf-8" ?>].
        /// </summary>
        /// <param name="element">Um elemento xml qualquer</param>
        /// <param name="xmlTextEditorAction">Permite a edição do texto do xml antes de criar o XmlDocument</param>
        /// <returns></returns>
        public static System.Xml.XmlDocument ToXmlDocument(this System.Xml.XmlElement element, Func<string, string> xmlTextEditorAction = null)
        {
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            string xmlText = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + element.OuterXml;

            if (xmlTextEditorAction.NotIsNull())
            {
                xmlText = xmlTextEditorAction(xmlText);
            }

            doc.LoadXml(xmlText);

            return doc;
        }

        /// <summary>
        /// Converte o XmlDocument atual em um XDocument.
        /// </summary>
        /// <returns>Retorna um objeto do tipo XDocument.</returns>
        public static XDocument ToXDocument(this XmlDocument SelfObj)
        {

            return XDocument.Parse(SelfObj.OuterXml, LoadOptions.PreserveWhitespace);
        }



        /// <summary>
        /// Converte o XElement atual em um XmlElement.
        /// </summary>
        /// <returns>Retorna um objeto do tipo XmlElement.</returns>
        public static XmlElement ToXmlElement(this XElement SelfObj)
        {

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.PreserveWhitespace = true;
            return (XmlElement)xmlDoc.ReadNode(SelfObj.CreateReader());
        }

        /// <summary>
        /// Converte o XmlElement atual em um XElement.
        /// </summary>
        /// <returns>Retorna um objeto do tipo XElement.</returns>
        public static XElement ToXElement(this XmlElement SelfObj)
        {

            return XElement.Parse(SelfObj.OuterXml, LoadOptions.PreserveWhitespace);
        }

        public static decimal Round(this decimal value, int decimals = 0)
        {
            return decimal.Round(value, decimals);
        }

        /// <summary>
        /// Converte separadores decimais com vírgula, para ponto e retorna um valor do tipo STRING.
        /// </summary>
        /// <param name="SelfObj"></param>
        /// <param name="RoundTo">Valor de arredondamento</param>
        /// <returns>Retorna um valor formatado do tipo STRING</returns>
        public static string ToSql(this decimal value, int roundTo = 0)
        {
            //decimal value = input.IsNull() ? 0 : input.Value;
            string output = null;


            if (roundTo <= 0)
            {
                output = value.ToString("0");
            }
            else if (roundTo == 1)
            {
                output = value.ToString("0.0");
            }
            else if (roundTo == 2)
            {
                output = value.ToString("0.00");
            }
            else if (roundTo == 3)
            {
                output = value.ToString("0.000");
            }
            else if (roundTo == 4)
            {
                output = value.ToString("0.0000");
            }
            else if (roundTo > 4)
            {
                output = value.ToString("0." + "0".DuplicateText(roundTo));
            }

            return output.Replace(".", "").Replace(",", ".");
        }

        public static string ToNfe(this decimal value, int roundTo = 2)
        {
            return Conversions.ToSql(value, roundTo);
        }

        /// <summary>
        /// Formata o valor dentro dos padrões exigidos pelo XML da NFe.
        /// Ex.: AAAA-MM-DD
        /// </summary>
        public static string ToNfe(this DateTime value)
        {
            DateTime? conv = value;
            return conv.ToNfe();
        }

        /// <summary>
        /// Formata o valor dentro dos padrões exigidos pelo XML da NFe.
        /// Ex.: AAAA-MM-DD
        /// </summary>
        public static string ToNfe(this DateTime? value)
        {
            string returnValue = "";

            returnValue = string.Empty;

            if (value.IsNull())
            {
                return returnValue;
            }

            returnValue = value.Value.ToString("yyyy-MM-dd");

            return returnValue;
        }

        /// <summary>
        /// Formata o valor dentro dos padrões exigidos pelo XML da NFe.
        /// Ex.: AAAA-MM-DDT24:59:59-03:00
        /// </summary>
        public static string ToNfeUtc(this DateTime value)
        {
            int diffTime = TimeSpan.FromTicks(DateTime.Now.Ticks).Subtract(TimeSpan.FromTicks(DateTime.UtcNow.Ticks)).Hours;
            string fusoHorario = ("00" + Math.Abs(diffTime)).TakeRight(2);
            char sinal = diffTime < 0 ? '-' : '+';
            string output = value.ToString("yyyy-MM-ddTHH:mm:ss{0}{1}:00".FormatText(sinal, fusoHorario));
            //-03:00

            return output;
        }

        /// <summary>
        /// Formata o valor dentro dos padrões exigidos pelo XML da NFe.
        /// Ex.: AAAA-MM-DDT24:59:59-03:00
        /// </summary>
        public static string ToNfeUtc(this DateTime? value)
        {
            string returnValue = string.Empty;

            if (!value.HasValue)
            {
                return string.Empty;
            }

            string output = value.Value.ToNfeUtc();
            return output;
        }

        /// <summary>
        /// Elimina caracteres especiais não aceitos pelo projeto da NFe.
        /// </summary>
        /// <returns>Retorna um valor formatado do tipo STRING</returns>
        public static string ToNfe(this string input)
        {
            if (input.IsNull())
            {
                return string.Empty;
            }

            string output = input.ToString().ToUpper();

            // Eliminando caracteres especiais que não são aceitos:
            output = System.Text.RegularExpressions.Regex.Replace(output, @"[\r\n\t\f\v'""<>&]+", " ");

            // Substituindo vogais e variações:
            output = System.Text.RegularExpressions.Regex.Replace(output, @"[ªÁÀÂÄÃ]+", "A");
            output = System.Text.RegularExpressions.Regex.Replace(output, @"[ÉÈÊË]+", "E");
            output = System.Text.RegularExpressions.Regex.Replace(output, @"[ÍÌÎÏ]+", "I");
            output = System.Text.RegularExpressions.Regex.Replace(output, @"[º°ÓÒÔÖÕ]+", "O");
            output = System.Text.RegularExpressions.Regex.Replace(output, @"[ÚÙÛÜ]+", "U");
            output = System.Text.RegularExpressions.Regex.Replace(output, @"[Ç]+", "C");

            // Eliminando espaços duplos:
            output = System.Text.RegularExpressions.Regex.Replace(output, "[ ]{2,}", " ").Trim();

            output = System.Web.HttpUtility.HtmlEncode(output);
            return output;
        }

        //----------------------------------------------




        /*
        public static T ToEnum<T>(this string enumString)
        {
            return (T)Enum.Parse(typeof(T), enumString);
        }

        public static T ToEnum<T>(this object enumObject)
        {
            return (T)Enum.Parse(typeof(T), enumObject.ToString());
        }
        

        public static int ToInt(this object expression, int defaultIfEmptyOrNullOrInvalid = 0)
        {
            int retorno = 0;

            try
            {
                retorno = int.Parse(expression.ToString());
            }
            catch (Exception ex)
            {
                retorno = defaultIfEmptyOrNullOrInvalid;
            }

            return retorno;
        }
        */








    }
}