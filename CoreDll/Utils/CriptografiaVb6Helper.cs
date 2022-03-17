
using System;
using System.Text;
using System.Collections.Generic;
using CoreDll.Extensions;

namespace CoreDll.Utils
{
    public static class CriptografiaVb6Helper
    {
        /// <summary>
        /// Criptografa um texto (criptografia do projeto Opera em VB6)
        /// </summary>
        /// <param name="texto">Texto a ser criptografado</param>
        /// <param name="parametro">Código de embaralhamento (como se fosse uma senha)</param>
        /// <returns></returns>
        public static string Criptografar(string texto, int parametro)
        {
            return Cryptografia(texto.Trim(), true, parametro);
        }

        /// <summary>
        /// Descriptografa um texto (criptografia do projeto Opera em VB6)
        /// </summary>
        /// <param name="texto">Texto a ser descriptografado</param>
        /// <param name="parametro">Código de embaralhamento (como se fosse uma senha)</param>
        /// <returns></returns>
        public static string Descriptografar(string texto, int parametro)
        {
            return Cryptografia(texto, false, parametro).Trim();
        }

        public static string Cryptografia(String StrTexto, Boolean BooCrip = true, int IntParametro = 30)
        {
            if (StrTexto is null)
                return null;

            int IntTamanhoString;
            string StrB = "", StrC = "";
            int IntSinal, IntNx, /*IntCalculo, */ IntFaixaIni, IntFaixaFim, IntTamFaixa;

            IntFaixaIni = 125;
            IntFaixaFim = 255;
            IntTamFaixa = IntFaixaFim - IntFaixaIni + 1;
            IntTamanhoString = StrTexto.Length;

            IntSinal = 1;

            List<char> chrCodigos = new List<char>() { '}', '~', '', '€', '', '‚', 'ƒ', '„', '…', '†', '‡', 'ˆ', '‰', 'Š', '‹', 'Œ', '', 'Ž', '', '', '‘', '’', '“', '”', '•', '–', '—', '˜', '™', 'š', '›', 'œ', '', 'ž', 'Ÿ', ' ', '¡', '¢', '£', '¤', '¥', '¦', '§', '¨', '©', 'ª', '«', '¬', '­', '®', '¯', '°', '±', '²', '³', '´', 'µ', '¶', '·', '¸', '¹', 'º', '»', '¼', '½', '¾', '¿', 'À', 'Á', 'Â', 'Ã', 'Ä', 'Å', 'Æ', 'Ç', 'È', 'É', 'Ê', 'Ë', 'Ì', 'Í', 'Î', 'Ï', 'Ð', 'Ñ', 'Ò', 'Ó', 'Ô', 'Õ', 'Ö', '×', 'Ø', 'Ù', 'Ú', 'Û', 'Ü', 'Ý', 'Þ', 'ß', 'à', 'á', 'â', 'ã', 'ä', 'å', 'æ', 'ç', 'è', 'é', 'ê', 'ë', 'ì', 'í', 'î', 'ï', 'ð', 'ñ', 'ò', 'ó', 'ô', 'õ', 'ö', '÷', 'ø', 'ù', 'ú', 'û', 'ü', 'ý', 'þ', 'ÿ' };
            List<int> intCodigos = new List<int>() { 125, 126, 127, 128, 129, 130, 131, 132, 133, 134, 135, 136, 137, 138, 139, 140, 141, 142, 143, 144, 145, 146, 147, 148, 149, 150, 151, 152, 153, 154, 155, 156, 157, 158, 159, 160, 161, 162, 163, 164, 165, 166, 167, 168, 169, 170, 171, 172, 173, 174, 175, 176, 177, 178, 179, 180, 181, 182, 183, 184, 185, 186, 187, 188, 189, 190, 191, 192, 193, 194, 195, 196, 197, 198, 199, 200, 201, 202, 203, 204, 205, 206, 207, 208, 209, 210, 211, 212, 213, 214, 215, 216, 217, 218, 219, 220, 221, 222, 223, 224, 225, 226, 227, 228, 229, 230, 231, 232, 233, 234, 235, 236, 237, 238, 239, 240, 241, 242, 243, 244, 245, 246, 247, 248, 249, 250, 251, 252, 253, 254, 255 };


            if (BooCrip)    //'Criptografando
            {
                throw new NotImplementedException();

                StrB = StrTexto.ReverseIt();
                IntSinal = -1;
                IntNx = 1;

                foreach (char chr in StrB)
                {
                    //IntCalculo = ((byte)chr) + IntParametro * IntSinal + IntNx++;

                    //-----------------                    
                    char[] charArr = { chr };
                    byte byteAscii = Encoding.Unicode.GetBytes(charArr)[0];

                    // ISSUE: Potential Substring problem; VB6 Original: Mid(StrB, IntNx, 1)
                    //IntCalculo = (short)(StrB.Substring(IntNx - 1, 1)[0]) + IntParametro * IntSinal + IntNx;
                    int IntCalculo = byteAscii + (IntParametro * IntSinal) + IntNx;
                    //-----------------


                    while (IntCalculo > IntTamFaixa)
                    {
                        IntCalculo = IntCalculo - IntTamFaixa;
                    }

                    while (IntCalculo < 1)
                    {
                        IntCalculo = IntCalculo + IntTamFaixa;
                    }

                    IntCalculo = IntCalculo + IntFaixaIni - 1;

                    //StrC = StrC + ((char)IntCalculo);
                    StrC = StrC + chrCodigos[intCodigos.FindIndex(x => x == IntCalculo)];
                    IntSinal = IntSinal * -1;
                }
            }
            else    //'DesCriptografando
            {
                IntSinal = -1;
                IntNx = 1;

                foreach (char chr in StrTexto)
                {
                    //IntCalculo = ((byte)chr);
                    int IntCalculo = intCodigos[chrCodigos.FindIndex(x => x == chr)];

                    IntCalculo = IntCalculo - IntFaixaIni + 1;
                    IntCalculo = IntCalculo - IntParametro * IntSinal - IntNx++;

                    while (IntCalculo > IntTamFaixa)
                    {
                        IntCalculo = IntCalculo - IntTamFaixa;
                    }

                    while (IntCalculo < 1)
                    {
                        IntCalculo = IntCalculo + IntTamFaixa;
                    }

                    StrB = StrB + ((char)IntCalculo);
                    IntSinal = IntSinal * -1;
                }

                StrC = StrB.ReverseIt();
            }

            return StrC;
        }
    }
}