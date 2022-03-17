using System;
using System.Text;

namespace CoreDll.Extensions.SefazExtensions
{
    public static class SefazExtensions
    {
        public static string ToSefazDateTimeUtc(this DateTime? value)
        {
            if (!value.HasValue)
                return null;

            return Formata_Data_NFe_Utc(value);
        }

        public static string ToSefazDateTimeUtc(this DateTime value)
        {
            return Formata_Data_NFe_Utc(value);
        }

        [Obsolete]
        private static string Formata_Data_NFe_Utc(DateTime? value)
        {
            if (!value.HasValue)
            {
                return string.Empty;
            }

            TimeSpan span = DateTime.UtcNow.Subtract(DateTime.Now);  //<-- identificando o fuso horário (inclusive o horário de verão) através do horário da máquina local
            return value.Value.ToString("yyyy-MM-ddTHH:mm:ss-" + ("00" + span.Hours).TakeRight(2) + ":00");

            //Dim span As TimeSpan = DateTime.UtcNow.Subtract(DateTime.Now)   '<-- identificando o fuso horário (inclusive o horário de verão) através do horário da máquina local
            //Formata_Data_NFe_Utc = Format(value, "yyyy-MM-ddTHH:mm:ss-" & Right("00" & span.Hours, 2) & ":00")
        }


        public static string ToSefazDateTime(this DateTime? value)
        {
            if (!value.HasValue)
                return null;

            return Formata_DataHora_NFe(value);
        }

        public static string ToSefazDateTime(this DateTime value)
        {
            //return ToSefazDateTime(value);
            return Formata_DataHora_NFe(value);
        }

        [Obsolete]
        private static string Formata_DataHora_NFe(DateTime? value)
        {

            if (!value.HasValue)
            {
                return string.Empty;
            }

            return value.Value.ToString("yyyy-MM-ddTHH:mm:ss");
            //Formata_DataHora_NFe = Format(value, "yyyy-MM-ddTHH:mm:ss")
        }


        public static string ToSefazDate(this DateTime? value)
        {
            return Formata_Data_NFe(value);
        }

        public static string ToSefazDate(this DateTime value)
        {
            return Formata_Data_NFe(value);
        }

        [Obsolete]
        private static string Formata_Data_NFe(object ObjData)
        {
            if (ObjData.IsNull())
            {
                return string.Empty;
            }

            return DateTime.Parse(ObjData.ToString()).ToString("yyyy-MM-dd");
            //Formata_Data_NFe = Format(CDate(ObjData), "yyyy-MM-dd")
        }

        public static string ToSefazXmlFormat(this object value)
        {
            return Formata_String_NFe(value);
        }

        [Obsolete]
        private static string Formata_String_NFe(object ObjString)
        {
            string StrString = String.Empty;
            StringBuilder StrBuilder = new StringBuilder("");

            if (ObjString.IsNull())
            {
                return string.Empty;
            }

            StrString = ObjString.ToString().ToUpper();

            /*
            'StrString = StrString.Replace("&", "&amp;")
            'StrString = StrString.Replace("<", "&lt;")
            'StrString = StrString.Replace(">", "&gt;")
            'StrString = StrString.Replace(Chr(34), "&quot;")
            'StrString = StrString.Replace(Chr(39), "&#39;")
            */

            StrString = StrString.Replace("ª", "a");
            StrString = StrString.Replace("º", "o");
            StrString = StrString.Replace("°", "o");

            StrString = StrString.Replace((char)10, "");
            StrString = StrString.Replace((char)11, "");
            StrString = StrString.Replace((char)12, "");
            StrString = StrString.Replace((char)13, " ");
            StrString = StrString.Replace((char)9, " ");

            StrString = StrString.Replace("Á", "A");
            StrString = StrString.Replace("É", "E");
            StrString = StrString.Replace("Í", "I");
            StrString = StrString.Replace("Ó", "O");
            StrString = StrString.Replace("Ú", "U");

            StrString = StrString.Replace("À", "A");
            StrString = StrString.Replace("È", "E");
            StrString = StrString.Replace("Ì", "I");
            StrString = StrString.Replace("Ò", "O");
            StrString = StrString.Replace("Ù", "U");

            StrString = StrString.Replace("Â", "A");
            StrString = StrString.Replace("Ê", "E");
            StrString = StrString.Replace("Î", "I");
            StrString = StrString.Replace("Ô", "O");
            StrString = StrString.Replace("Û", "U");

            StrString = StrString.Replace("Ä", "A");
            StrString = StrString.Replace("Ë", "E");
            StrString = StrString.Replace("Ï", "I");
            StrString = StrString.Replace("Ö", "O");
            StrString = StrString.Replace("Ü", "U");

            StrString = StrString.Replace("Ã", "A");
            StrString = StrString.Replace("Õ", "O");

            StrString = StrString.Replace("Ç", "C");

            //Evitando caracteres inválidos:
            foreach (char CharX in StrString)
            {
                if (Char.IsLetterOrDigit(CharX))
                    StrBuilder.Append(CharX);
                else
                    StrBuilder.Append(" ");
            }

            /*
            'StrString = StrString.Replace("\", " ")
            'StrString = StrString.Replace("/", " ")

            'StrString = StrString.Replace("*", " ")
            'StrString = StrString.Replace("@", " ")
            'StrString = StrString.Replace("%", " ")
            'StrString = StrString.Replace("$", " ")
            ''StrString = StrString.Replace("#", " ")

            'StrString = StrString.Replace(",", " ")
            'StrString = StrString.Replace("!", " ")
            'StrString = StrString.Replace("?", " ")
            'StrString = StrString.Replace(";", " ")
            'StrString = StrString.Replace(":", " ")
            'StrString = StrString.Replace(".", " ")

            'StrString = StrString.Replace("¨", " ")
            'StrString = StrString.Replace("(", " ")
            'StrString = StrString.Replace(")", " ")
            'StrString = StrString.Replace("_", " ")
            'StrString = StrString.Replace("-", " ")

            'StrString = StrString.Replace("^", " ")
            'StrString = StrString.Replace("~", " ")
            'StrString = StrString.Replace("'", " ")
            'StrString = StrString.Replace("~", " ")
            ''StrString = StrString.Replace("""", " ")
            'StrString = StrString.Replace("{", " ")
            'StrString = StrString.Replace("[", " ")
            'StrString = StrString.Replace("}", " ")
            'StrString = StrString.Replace("]", " ")

            'StrString = StrString.Replace("¹", " ")
            'StrString = StrString.Replace("²", " ")
            'StrString = StrString.Replace("³", " ")
            */

            StrString = StrBuilder.ToString();

            while (StrString.Contains("  "))
            {
                StrString = StrString.Replace("  ", " ");
            }

            StrString = StrString.Trim();

            return StrString;
        }

        public static string ToSefazXmlFormat(this string value)
        {
            return ToSefazXmlFormat(value);
        }

        public static string ToSefazDecimal(this object value, int decimalSpaces = 0)
        {
            return Formata_Valor_NFe(value, decimalSpaces);
        }

        [Obsolete]
        private static string Formata_Valor_NFe(object ObjValor, int CasasDecimais = 0)
        {

            decimal result;


            if (ObjValor.IsNull() || !decimal.TryParse(ObjValor.ToString(), out result))
            {
                ObjValor = 0;
            }
            else
            {
                ObjValor = result;
            }


            if (CasasDecimais > 0)
            {
                string casas = "0".DuplicateText(CasasDecimais);
                string formato = "{0:0." + casas + "}";

                ObjValor = string.Format(formato, ObjValor);
                return ObjValor.ToString();
            }
            else
            {
                ObjValor = ObjValor.ToString().Replace(".", "").Replace(",", ".");
                return ObjValor.ToString();
            }
        }

        public static string ToSefazDecimal(this decimal value, int decimalSpaces = 0)
        {
            return ToSefazDecimal(value.ToString(), decimalSpaces);
        }

        public static string ToSefazDecimal(this int value, int decimalSpaces = 0)
        {
            return ToSefazDecimal(value.ToString(), decimalSpaces);
        }
    }
}