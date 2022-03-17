using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Threading.Tasks;
using System.Data;

namespace CoreDll.Extensions
{
    public static class Extensions
    {
        private static Regex IgnoreNonNumericPattern = new Regex(@"[^0-9]+");

        private static string isDecimalPattern = @"^(\d+(\,\d*)|\d+|\d*(\,\d+))$";
        private static Regex IsFullStopDecimalPattern = new Regex(isDecimalPattern.Replace(",", "."));
        private static Regex IsCommaDecimalPattern = new Regex(isDecimalPattern);


        public enum SeparatorSymbolEnum
        {
            FullStop, Comma, Both
        }


        //private static int[] powof10 = new int[10]
        //{
        //    1,
        //    10,
        //    100,
        //    1000,
        //    10000,
        //    100000,
        //    1000000,
        //    10000000,
        //    100000000,
        //    1000000000
        //};

        //public static decimal ParseDecimal(string input)
        //{
        //    int len = input.Length;
        //    if (len != 0)
        //    {
        //        bool negative = false;
        //        long n = 0;
        //        int start = 0;
        //        if (input[0] == '-')
        //        {
        //            negative = true;
        //            start = 1;
        //        }
        //        if (len <= 19)
        //        {
        //            int decpos = len;
        //            for (int k = start; k < len; k++)
        //            {
        //                char c = input[k];
        //                if (c == '.')
        //                {
        //                    decpos = k + 1;
        //                }
        //                else
        //                {
        //                    n = (n * 10) + (int)(c - '0');
        //                }
        //            }
        //            return new decimal((int)n, (int)(n >> 32), 0, negative, (byte)(len - decpos));
        //        }
        //        else
        //        {
        //            if (len > 28)
        //            {
        //                len = 28;
        //            }
        //            int decpos = len;
        //            for (int k = start; k < 19; k++)
        //            {
        //                char c = input[k];
        //                if (c == '.')
        //                {
        //                    decpos = k + 1;
        //                }
        //                else
        //                {
        //                    n = (n * 10) + (int)(c - '0');
        //                }
        //            }
        //            int n2 = 0;
        //            bool secondhalfdec = false;
        //            for (int k = 19; k < len; k++)
        //            {
        //                char c = input[k];
        //                if (c == '.')
        //                {
        //                    decpos = k + 1;
        //                    secondhalfdec = true;
        //                }
        //                else
        //                {
        //                    n2 = (n2 * 10) + (int)(c - '0');
        //                }
        //            }
        //            byte decimalPosition = (byte)(len - decpos);
        //            return new decimal((int)n, (int)(n >> 32), 0, negative, decimalPosition) * powof10[len - (!secondhalfdec ? 19 : 20)] + new decimal(n2, 0, 0, negative, decimalPosition);
        //        }
        //    }
        //    return 0;
        //}



        public static string Dump(this object obj, DumpStyle style = DumpStyle.CSharp)
        {

            return ObjectDumper.Dump(obj, style);
        }

        public static List<T> Replace<T>(this List<T> collection, T oldItem, T newItem)
        {
            for (int idx = 0; idx < collection.Count; idx++)
            {
                T item = collection[idx];

                if (object.ReferenceEquals(item, oldItem))
                {
                    collection[idx] = newItem;
                }
            }

            return collection;
        }

        public static IList<T> Replace<T>(this IList<T> collection, T oldItem, T newItem)
        {
            for (int idx = 0; idx < collection.Count; idx++)
            {
                T item = collection[idx];

                if (object.ReferenceEquals(item, oldItem))
                {
                    collection[idx] = newItem;
                }
            }

            return collection;
        }

        public static void AddRange<T>(this ICollection<T> collection, ICollection<T> args)
        {
            if (args.NotIsNull() && collection.NotIsNull())
            {
                args.ForEach(email => collection.Add(email));
            }
        }

        //public static void AddRange(this System.Net.Mail.MailAddressCollection mailAddressCollection, ICollection<string> emailAddressCollection)
        //{
        //    if (emailAddressCollection.IsNotNull() && mailAddressCollection.IsNotNull())
        //    {
        //        emailAddressCollection.ForEach(email => mailAddressCollection.Add(email));
        //    }
        //}

        public static IEnumerable<T> DequeueRange<T>(this Queue<T> queue, int count)
        {
            if (queue is null)
                yield break;

            for (int idx = 0; idx < count && queue.Any(); idx++)
            {
                yield return queue.Dequeue();
            }
        }

        public static IEnumerable<IEnumerable<T>> Distribute<T>(this IEnumerable<T> enumerable, int groups)
        {
            if (enumerable is null)
                yield break;

            bool temResto = (enumerable.Count() % groups) > 0;
            int maxItemsInAGroup = ((int)(enumerable.Count() / groups)) + (temResto ? 1 : 0);

            Queue<T> fila = new Queue<T>(enumerable);

            for (int idx = 0; idx < groups; idx++)
            {
                yield return fila.DequeueRange(maxItemsInAGroup);
            }
        }

        public static async ValueTask ToAsync(this Action runnable)
        {
            await Task.Run(() => runnable()).ConfigureAwait(true);
        }

        public static async ValueTask<T> ToAsync<T>(this Func<T> runnable)
        {
            return await Task<T>.Run(() => runnable()).ConfigureAwait(true);
        }

        //public static List<T> ToList<T>(this Caliburn.Micro.BindableCollection<T> collection)
        //{
        //    List<T> list = new List<T>();

        //    if (collection != null)
        //    {
        //        list.AddRange(collection);
        //    }

        //    return list;
        //}

        public static void ChangeItem<T>(this IList<T> list, T element, T newElement)
        {
            if (list is null)
                return;

            int indexOf = list.IndexOf(element);

            if (indexOf >= 0)
            {
                list.RemoveAt(indexOf);
                list.Insert(indexOf, newElement);
            }
        }

        /// <summary>
        /// Produz a diferença de conjunto de duas sequências.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <param name="comparer">Um Func<T, T, bool> para comparar valores.</param>
        /// <returns></returns>
        public static IEnumerable<T> Except<T>(this IEnumerable<T> first, IEnumerable<T> second, Func<T, T, bool> comparer)
        {
            if (first.NotIsNull() && second.NotIsNull() && comparer.NotIsNull())
            {
                foreach (T firstItem in first)
                {
                    if (!second.Any(secondItem => comparer(firstItem, secondItem)))
                    {
                        yield return firstItem;
                    }
                }

                foreach (T secondItem in second)
                {
                    if (!first.Any(firstItem => comparer(secondItem, firstItem)))
                    {
                        yield return secondItem;
                    }
                }
            }
        }


        public static void Insert<T>(this IList<T> list, int index, IEnumerable<T> items)
        {
            if (list.NotIsNull() && items.NotIsNull())
            {
                foreach (T item in items)
                {
                    list.Insert(index, item);
                }
            }
        }

        

        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            if (collection != null && action != null)
            {
                foreach (T item in collection)
                {
                    action(item);
                }
            }

            return collection;
        }

        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> collection, Action<T, int> action)
        {
            int idx = 0;

            if (collection != null && action != null)
            {
                foreach (T item in collection)
                {
                    action(item, idx++);
                }
            }

            return collection;
        }

        public static void EnqueueRange<T>(this Queue<T> queue, IEnumerable<T> collection)
        {
            if (queue is null || collection is null)
                return;

            foreach (T item in collection)
            {
                queue.Enqueue(item);
            }
        }

        public static void EnqueueRange<T>(this Queue<T> queue, IList<T> collection)
        {
            if (queue is null || collection is null)
                return;

            foreach (T item in collection)
            {
                queue.Enqueue(item);
            }
        }

        //public static void AddRange<T>(this List<T> list, IEnumerable<T> rangeCollection)
        //{
        //    if (list != null && rangeCollection != null)
        //    {
        //        foreach (T item in rangeCollection)
        //        {
        //            list.Add(item);
        //        }
        //    }

        //    //return list;
        //}

        public static string Replace(this string value, char oldChar, string newString)
        {
            return value.Replace(oldChar.ToString(), newString);
        }

        public static string Replace(this string value, string oldString, char newChar)
        {
            return value.Replace(oldString, newChar.ToString());
        }


        /// <summary>
        /// Quando a referência não é null. 
        /// Expande a funcionalidade de Nullables<>.HasValue() para outras variáveis não anuláveis.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool HasValue(this string value)
        {
            return (value.NotIsNull());
        }

        public static string[] SplitText(this string expression, string separator, StringSplitOptions option = StringSplitOptions.None)
        {
            if (expression == null)
                return null;

            return expression.Split(new string[] { separator }, option);
        }

        public static string[] SplitText(this string expression, char separator, StringSplitOptions option = StringSplitOptions.None)
        {
            if (expression == null)
                return null;

            return expression.Split(new string[] { separator.ToString() }, option);
        }



        public static DataRow FirstOrDefault(this DataRowCollection collection)
        {
            if (collection is null)
                return null;

            if (collection.Count > 0)
                return collection[0];

            return null;
        }

        public static bool Any(this DataRowCollection collection)
        {
            if (collection is null)
                return false;

            if (collection.Count > 0)
                return true;

            return false;
        }

        public static DataRow LastOrDefault(this DataRowCollection collection)
        {
            if (collection is null)
                return null;

            if (collection.Count > 0)
                return collection[collection.Count - 1];

            return null;
        }

        public static Dictionary<string, string> SplitText(this string expression, string paramSeparator, string keyValueSeparator, StringSplitOptions option = StringSplitOptions.None)
        {
            string[] parameters = expression.Split(new string[] { paramSeparator }, option);
            Dictionary<string, string> dictionary = new Dictionary<string, string>();

            foreach (string pair in parameters)
            {
                string[] keyValue = pair.SplitText(keyValueSeparator);
                dictionary.Add(keyValue.First(), keyValue.Last());
            }

            return dictionary;
        }

        /// <summary>
        /// Duplicará um caracter ou texto a quantidade de vezes determinada pelo parâmetro [count].
        /// </summary>        
        public static string DuplicateText(this char expression, int count)
        {
            return expression.ToString().DuplicateText(count);
        }

        /// <summary>
        /// Duplicará um caracter ou texto a quantidade de vezes determinada pelo parâmetro [count].
        /// </summary>
        public static string DuplicateText(this string expression, int count)
        {
            StringBuilder builder = new StringBuilder("");

            for (int idx = 0; idx < count; idx++)
            {
                builder.Append(expression);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Preenche o texto com zeros à esquerda.
        /// </summary>
        public static string ZeroLeftText(this string expression, int length)
        {
            return ("0".DuplicateText(length) + expression.Trim()).TakeRight(length);
        }

        /// <summary>
        /// Trunca valores decimais à direita.
        /// </summary>
        public static decimal Truncate(this decimal value, int decimals)
        {
            int fator = (int)Math.Pow(10, Math.Abs(decimals));
            return Math.Truncate(value * fator) / fator;
        }

        /// <summary>
        /// Distribui um valor considerando o menor valor decimal divisível possível em um determinado sistema monetário.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="divisor"></param>
        /// <param name="cuture"></param>
        /// <returns></returns>
        public static decimal[] DivideMoney(this decimal value, int divisor, System.Globalization.CultureInfo cuture = null)
        {
            cuture = cuture == null ? System.Globalization.CultureInfo.CurrentCulture : cuture;

            bool isNegative = value < 0m;

            divisor = Math.Abs(divisor);
            decimal uValue = Math.Abs(value); // unsigned value

            int fator = (int)Math.Pow(10, Math.Abs(cuture.NumberFormat.CurrencyDecimalDigits));
            int leftValue = (int)Math.Truncate(uValue * fator);
            List<decimal> result = new List<decimal>();
            List<int> values = new List<int>();

            // Preenche com valor padrão da divisão:
            for (int idx = 0; idx < divisor; idx++)
            {
                values.Add(0);
            }

            // Tratando cada caso
            if (uValue == 0)
            {
                //values.Add(0);
                // ;;;não vai fazer nada mesmo, pois já está tudo preenchido com ZEROs;;;
            }
            else if (divisor == 1)
            {
                values[0] = leftValue;
            }
            else if (divisor == 0)
            {
                throw new Exception("Divisão por zero não é permitida!");
                //values.Add(0);
                // ;;;não vai fazer nada mesmo, pois já está tudo preenchido com ZEROs;;;
            }
            else if (divisor > leftValue)
            {
                while (leftValue > 0)
                {
                    for (int idx = 0; idx < divisor && leftValue > 0; idx++)
                    {
                        values[idx] = values[idx] + 1;
                        leftValue = leftValue - 1;
                    }
                }
            }
            else
            {
                int divisionValue = (int)Math.Truncate(leftValue / (decimal)divisor);

                for (int idx = 0; idx < divisor && leftValue >= divisionValue; idx++)
                {
                    values[idx] = values[idx] + divisionValue;
                    leftValue = leftValue - divisionValue;
                }

                while (leftValue > 0)
                {
                    for (int idx = 0; idx < divisor && leftValue > 0; idx++)
                    {
                        values[idx] = values[idx] + 1;
                        leftValue = leftValue - 1;
                    }
                }
            }

            for (int idx = 0; idx < divisor; idx++)
            {
                if (isNegative && values[idx] != 0)
                {
                    values[idx] = values[idx] * -1;
                }

                decimal resultValue = values[idx] / (decimal)fator;

                result.Add(resultValue);
            }

            if (result.Sum() != value)
            {
                throw new Exception("O valor total da divisão não confere com a soma dos valores divididos!");
            }

            return result.ToArray();
        }



        /// <summary>
        /// Verifica se valor é nulo ou vazio.
        /// </summary>
        /// <returns>Retorna verdadeiro se valor for Null (Nothing) ou Empty.</returns>
        public static bool IsEmpty(this string SelfObj)
        {
            return string.IsNullOrEmpty(SelfObj);
        }

        /// <summary>
        /// Verifica se valor não é nulo ou vazio.
        /// </summary>
        /// <returns>Retorna verdadeiro se valor for Null (Nothing) ou Empty.</returns>
        public static bool IsNotEmpty(this string SelfObj)
        {
            return !string.IsNullOrEmpty(SelfObj);
        }

        /// <summary>
        /// Retorna uma string reversa (de trás para frente).
        /// </summary>
        public static string ReverseIt(this string expression)
        {
            StringBuilder builder = new StringBuilder("");

            foreach (char c in (expression ?? string.Empty))
            {
                builder.Insert(0, c);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Retorna um valor DEFAULT caso o valor da string seja EMPTY.
        /// </summary>
        /// <param name="SelfObj"></param>
        /// <param name="DefaultValue">Valor padrão a ser retornado</param>
        /// <returns>Retorna uma string</returns>
        /// <remarks></remarks>
        public static string DefaultIfEmpty(this string text, string defaultValue)
        {
            return ((string.IsNullOrEmpty(text)) ? defaultValue : text).ToString();
        }

        /// <summary>
        /// Retorna um valor DEFAULT caso o valor da string seja EMPTY.
        /// </summary>
        /// <param name="SelfObj"></param>
        /// <param name="DefaultValue">Valor padrão a ser retornado</param>
        /// <returns>Retorna uma string</returns>
        /// <remarks></remarks>
        public static string ReturnsDefaultIfEmpty(this string text, string defaultValue)
        {
            return ((string.IsNullOrEmpty(text)) ? defaultValue : text).ToString();
        }

        /// <summary>
        /// Retorna um valor DEFAULT caso o valor da string seja NULL.
        /// </summary>
        /// <param name="SelfObj"></param>
        /// <param name="DefaultValue">Valor padrão a ser retornado</param>
        /// <returns>Retorna uma string</returns>
        /// <remarks></remarks>
        public static string ReturnsDefaultIfNull(this string text, string defaultValue)
        {
            return ((text.IsNull()) ? defaultValue : text).ToString();
        }

        /// <summary>
        /// Obtém uma quantidade de caracteres à esquerda.
        /// </summary>
        /// <param name="IntCorte">Quantidade de caracteres.</param>
        /// <returns>Retorna sequência de caracteres indicada.</returns>
        public static string TakeLeft(this string valor, int corte)
        {
            int idx = 0;
            StringBuilder leftPart = new StringBuilder("");

            for (; idx < valor.Count() && idx < corte; idx++)
            {
                leftPart.Append(valor[idx]);
            }

            return leftPart.ToString(); //
        }

        /// <summary>
        /// Obtém uma quantidade de caracteres à direita.
        /// </summary>
        /// <param name="IntCorte">Quantidade de caracteres.</param>
        /// <returns>Retorna sequência de caracteres indicada.</returns>
        public static string TakeRight(this string valor, int corte)
        {
            int idx = valor.Count() - corte;
            idx = (idx < 0 ? 0 : idx);

            StringBuilder rightPart = new StringBuilder("");

            for (; idx < valor.Count(); idx++)
            {
                rightPart.Append(valor[idx]);
            }

            return rightPart.ToString();
        }



        public static string TakeBefore(this string text, string seekValue, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            if (seekValue.IsNull())
                return string.Empty;

            int index = text.IndexOf(seekValue, comparison);

            if (index == -1)
            {
                return string.Empty;
            }

            return text.Substring(0, index);
        }

        public static string TakeAfter(this string text, string seekValue, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            if (seekValue.IsNull())
                return string.Empty;

            int index = text.IndexOf(seekValue, comparison);

            if (index == -1)
            {
                return string.Empty;
            }

            return text.Substring(index + seekValue.Length);
        }

        public static string TakeBetween(this string text, string seekValue, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            return text.TakeBetween(seekValue, seekValue, comparison);
        }

        public static string TakeBetween(this string text, string leftSeekValue, string rightSeekValue, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            string result = text.TakeAfter(leftSeekValue, comparison);

            if (result != string.Empty)
            {
                result = result.TakeBefore(rightSeekValue, comparison);
            }

            return result;
        }

        /// <summary>
        /// Ignora os caracteres à esquerda (antes)
        /// </summary>
        /// <param name="text"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static string SkipLeft(this string text, int count)
        {
            return text.Skip(count).JoinText();
        }

        /// <summary>
        /// Ignora os caracteres à direita (depois)
        /// </summary>
        /// <param name="text"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static string SkipRight(this string text, int count)
        {
            return text.ReverseIt().Skip(count).JoinText().ReverseIt();
        }

        /// <summary>
        /// Ignora o que estiver à esquerda (antes) do texto encontrado
        /// </summary>
        /// <param name="text"></param>
        /// <param name="seekValue"></param>
        /// <param name="comparison"></param>
        /// <param name="seekValueInclusive"></param>
        /// <returns></returns>
        public static string SkipBefore(this string text, string seekValue, StringComparison comparison = StringComparison.OrdinalIgnoreCase, bool seekValueInclusive = true)
        {
            if (seekValue.IsNull())
                return text;

            int index = text.IndexOf(seekValue, comparison);

            if (index == -1)
            {
                return text;
            }

            return seekValueInclusive ? text.Substring(index) : text.Substring(index + seekValue.Length);
        }

        /// <summary>
        /// Ignora tudo o que estiver à direita (depois) do texto encontrado
        /// </summary>
        /// <param name="text"></param>
        /// <param name="seekValue"></param>
        /// <param name="comparison"></param>
        /// <param name="seekValueInclusive"></param>
        /// <returns></returns>
        public static string SkipAfter(this string text, string seekValue, StringComparison comparison = StringComparison.OrdinalIgnoreCase, bool seekValueInclusive = true)
        {
            if (seekValue.IsNull())
                return text;

            int index = text.IndexOf(seekValue, comparison);

            if (index == -1)
            {
                return text;
            }

            return seekValueInclusive ? text.Substring(0, index + seekValue.Length) : text.Substring(0, index);
        }

        public static string FormatText(this string text, params object[] args)
        {
            if (args == null)
                return text;

            for (int idx = 0; idx < args.Count(); idx++)
            {
                Enum value = args[idx] as Enum;

                if (value != null)
                {
                    args[idx] = value.GetHashCode();
                }
            }

            return string.Format(text, args);
        }

        public static string FormatText(this string text, System.IFormatProvider provider, params object[] args)
        {
            if (args == null)
                return text;



            for (int idx = 0; idx < args.Count(); idx++)
            {
                Enum value = args[idx] as Enum;

                if (value != null)
                {
                    args[idx] = value.GetHashCode();
                }
            }

            return string.Format(provider, text, args);
        }

        /// <summary>
        /// Adiciona aspas simples à string.
        /// </summary>
        /// <param name="SelfObj"></param>
        /// <returns>Retorna a string com aspas simples</returns>
        /// <remarks></remarks>
        public static string Aspa(this string text)
        {
            return "'" + text + "'";
        }

        /// <summary>
        /// Adiciona
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string Aspas(this string text)
        {
            return '\u0022' + text + "\u0022";
        }

        /// <summary>
        /// Verifica se a string equivale a null ou string.Empty.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }

        public static bool IsNotNullOrEmpty(this string value)
        {
            return !Extensions.IsNullOrEmpty(value);
        }

        /// <summary>
        /// Transforma uma lista de inteiros em uma sequência string.
        /// </summary>
        /// <param name="separator">Valor inserido entre os itens da lista</param>
        /// <returns>Retorna uma string contendo o conteúdo de cada elemento da collection especificada, separados pelo argumento "separator".</returns>
        public static string JoinText<T>(this ICollection<T> collection, string separator = null)
        {
            List<string> lista = new List<string>();

            foreach (T item in collection)
            {
                if(item is null)
                {
                    string parou = "parou";
                }

                lista.Add(item?.ToString() ?? string.Empty);
            }

            separator = separator.IsNull() ? string.Empty : separator;
            return string.Join(separator, lista.ToArray());
        }

        /// <summary>
        /// Transforma uma lista de inteiros em uma sequência string.
        /// </summary>
        /// <param name="separator">Valor inserido entre os itens da lista</param>
        /// <returns>Retorna uma string contendo o conteúdo de cada elemento da collection especificada, separados pelo argumento "separator".</returns>
        public static string JoinText<T>(this IEnumerable<T> collection, string separator = null)
        {
            List<string> lista = new List<string>();

            foreach (T item in collection)
            {
                lista.Add(item.ToString());
            }

            separator = separator.IsNull() ? string.Empty : separator;
            return string.Join(separator, lista.ToArray());
        }



        public static IEnumerable<T> Except<T>(this IEnumerable<T> enumerable, params T[] secondArgs)
        {
            List<T> exceptList = new List<T>();

            foreach (T item in secondArgs)
            {
                exceptList.Add(item);
            }

            return enumerable.Except(exceptList);
        }

        public static bool Contains<T>(this IEnumerable<T> enumerable, T value, Func<T, T, bool> comparer)
        {
            if (enumerable is null || value is null || comparer is null)
                return false;

            foreach (T item in enumerable)
            {
                if (comparer(value, item))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Retorna a parte chave (nome) de um "Enum" instanciado.
        /// </summary>
        /// <param name="SelfObj"></param>
        /// <returns>Retorna nome/chave da instância de Enum</returns>
        public static string GetNameFrom(this System.Enum enumInstance)
        {
            return Enum.GetName(enumInstance.GetType(), enumInstance);
        }

        /// <summary>
        /// Obtém os pares com cada nome e valor de um Enum.
        /// </summary>
        /// <param name="enumInstance"></param>
        /// <returns></returns>
        public static List<KeyValuePair<string, int>> GetPairs(this System.Enum enumInstance)
        {
            List<KeyValuePair<string, int>> content = new List<KeyValuePair<string, int>>();
            string[] names = Enum.GetNames(enumInstance.GetType());
            Array values = Enum.GetValues(enumInstance.GetType());

            for (int i = 0; i < names.Length && i < values.Length; i++)
            {
                KeyValuePair<string, int> value = new KeyValuePair<string, int>(names[i], (int)values.GetValue(i));
                content.Add(value);
            }

            return content;
        }


        public static bool IsNull(this object value)
        {
            return value is null || value == DBNull.Value;
        }

        public static bool NotIsNull(this object value)
        {
            return !IsNull(value);
        }

        /*
        public static object IfIsNull<T>(this T value, object ifNullValue = default(object))
        {
            if (value.IsNotNull())
            {
                return value;
            }
            else
            {
                return ifNullValue;
            }
        }

        public static decimal IfIsNull(this decimal? value, decimal ifNullValue)
        {
            if (value.IsNotNull())
            {
                return value.Value;
            }
            else
            {
                return ifNullValue;
            }
        }

        public static int IfIsNull(this int? value, int ifNullValue)
        {
            if (value.IsNotNull())
            {
                return value.Value;
            }
            else
            {
                return ifNullValue;
            }
        }

        public static object IfIsNull<T>(this T value, Func<T, Object> ifNotNullExpression, object ifNullValue = default(object))
        {
            if (ifNotNullExpression != null && value.IsNotNull())
            {
                return ifNotNullExpression(value);
            }
            else
            {
                return ifNullValue;
            }
        }
        */

        public static TReturn IfIsNull<T, TReturn>(this T value, Func<T, TReturn> ifNotNullExpression, TReturn ifNullValue = default(TReturn))
        {
            if (ifNotNullExpression != null && value.NotIsNull())
            {
                return ifNotNullExpression(value);
            }
            else
            {
                return ifNullValue;
            }
        }


        /// <summary>
        /// Verifica se existe a string dentro de Collection.
        /// </summary>
        public static bool ExistsIn(this string text, string[] collection)
        {
            //IEnumerable
            foreach (string item in collection)
            {
                if (item == text)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Verifica se existe a string dentro de Collection.
        /// </summary>
        public static bool ExistsIn(this string text, IEnumerable collection)
        {
            //IEnumerable
            foreach (string item in collection)
            {
                if (item == text)
                {
                    return true;
                }
            }

            return false;
        }

        public static System.Boolean IsNumeric(this System.Object expression)
        {
            if (expression.IsNull() || expression is DateTime)
                return false;

            if (expression is Int16 || expression is Int32 || expression is Int64 || expression is Decimal
                || expression is Single || expression is Double || expression is Boolean)
                return true;

            Double isNumeric;
            return Double.TryParse(expression.ToString(), out isNumeric);
        }

        /// <summary>
        /// Ignora todos os caracteres não numéricos de uma sequência de caracteres.
        /// </summary>
        /// <param name="input"></param>        
        public static System.String IgnoreNonNumeric(this string input)
        {
            string output = IgnoreNonNumericPattern.Replace(input, "");
            return output;
        }

        /// <summary>
        /// Verifica se a string [input] representa um número decimal
        /// </summary>
        /// <param name="input"></param>        
        public static System.Boolean IsDecimal(this string input, SeparatorSymbolEnum symbol = SeparatorSymbolEnum.Both)
        {
            try
            {
                if (symbol == SeparatorSymbolEnum.Both)
                {
                    return IsCommaDecimalPattern.IsMatch(input)
                            || IsFullStopDecimalPattern.IsMatch(input);
                }
                else if (symbol == SeparatorSymbolEnum.Comma)
                {
                    return IsCommaDecimalPattern.IsMatch(input);
                }
                else
                {
                    return IsFullStopDecimalPattern.IsMatch(input);
                }
            }
            catch
            {
                return false;
            }
        }





        public static System.String IgnoreChars(this string input, string pattern)
        {
            string output = input.Replace(pattern, "");
            //string output = Regex.Replace(input, pattern, "");
            return output;
        }

        public static System.String IgnoreChars(this string input, Regex pattern)
        {
            if (pattern is null)
                throw new ArgumentNullException(nameof(pattern));

            string output = pattern.Replace(input, "");
            return output;
        }

        /// <summary>
        /// Verifica se é um Enum.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Boolean IsEnum(this Object value)
        {
            return (value as Enum).NotIsNull();
        }

        /// <summary>
        /// Remove qualquer caracter fora das tags (<...>).
        /// </summary>
        /// <param name="xmlDoc"></param>
        public static void RemoveSpacesOuterTags(this XmlDocument xmlDoc)
        {
            StringBuilder builder = new StringBuilder("");
            bool isTagOpen = false;
            string xml = xmlDoc.OuterXml.Trim();

            List<string> blocks = new List<string>();

            foreach (char x in xml)
            {
                if (char.IsControl(x))
                {
                    continue;
                }

                if (!isTagOpen && x == '<')
                {
                    if (builder.Length > 0)
                    {
                        blocks.Add(builder.ToString());
                        builder.Clear();
                    }

                    isTagOpen = true;
                    builder.Append(x);
                    continue;
                }

                if (isTagOpen && x == '>')
                {
                    isTagOpen = false;
                    builder.Append(x);

                    if (builder.Length > 0)
                    {
                        blocks.Add(builder.ToString());
                        builder.Clear();
                    }

                    continue;
                }

                builder.Append(x);
            }

            if (builder.Length > 0)
            {
                blocks.Add(builder.ToString());
            }

            builder.Clear();

            foreach (string block in blocks)
            {
                builder.Append(block.Trim());
            }

            xmlDoc.LoadXml(builder.ToString());
        }

        /// <summary>
        /// Obtém primeiro nodo de um XmlNodeList.
        /// </summary>
        public static XmlNode FirstOrDefault(this XmlNodeList nodeList)
        {
            if (nodeList.NotIsNull() && nodeList.Count > 0)
            {
                return nodeList[0];
            }

            return null;
        }

        /// <summary>
        /// Obtém último nodo de um XmlNodeList.
        /// </summary>
        public static XmlNode LastOrDefault(this XmlNodeList nodeList)
        {
            if (nodeList.NotIsNull() && nodeList.Count > 0)
            {
                return nodeList[nodeList.Count - 1];
            }

            return null;
        }

        /// <summary>
        /// Verifica se contém a tag especificada.
        /// </summary>
        public static bool ContainsTag(this XmlElement element, string tagName)
        {
            return (element.GetElementsByTagName(tagName).Count > 0);
        }

        /// <summary>
        /// Verifica se contém a tag especificada.
        /// </summary>
        public static bool ContainsTag(this XmlNode node, string tagName)
        {
            return (((XmlElement)node).GetElementsByTagName(tagName).Count > 0);
        }

        /// <summary>
        /// Verifica se contém a tag especificada.
        /// </summary>
        public static bool ContainsTag(this XmlDocument doc, string tagName)
        {
            return (doc.GetElementsByTagName(tagName).Count > 0);
        }

        /// <summary>
        /// Verifica se contém o atributo especificado.
        /// </summary>
        public static bool ContainsAttribute(this XmlElement element, string attName)
        {
            foreach (XmlAttribute item in element.Attributes)
            {
                if (item.Name == attName)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Verifica se contém o atributo especificado.
        /// </summary>
        public static bool ContainsAttribute(this XmlNode node, string attName)
        {
            foreach (XmlAttribute item in node.Attributes)
            {
                if (item.Name == attName)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Verifica se contém o atributo especificado.
        /// </summary>
        public static bool ContainsAttribute(this XmlDocument doc, string attName)
        {
            foreach (XmlAttribute item in doc.Attributes)
            {
                if (item.Name == attName)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Verifica se contém o nome especificado.
        /// </summary>
        public static XmlNode GetTag(this XmlNode node, string name)
        {
            foreach (XmlNode item in node.ChildNodes)
            {
                if (item.Name == name)
                {
                    return item;
                }
            }

            return null;
        }

        public static List<XmlNode> GetTags(this XmlNode node, string name)
        {
            List<XmlNode> nodes = new List<XmlNode>();

            foreach (XmlNode item in node.ChildNodes)
            {
                if (item.Name == name)
                {
                    nodes.Add(item);
                }
            }

            return nodes;
        }

        /// <summary>
        /// Verifica se o nodo equivale ao predicado especificado.
        /// </summary>
        public static XmlNode GetTag(this XmlNode node, Func<XmlNode, bool> predicate)
        {
            foreach (XmlNode item in node.ChildNodes)
            {
                if (predicate(item))
                {
                    return item;
                }
            }

            return null;
        }

        /// <summary>
        /// Verifica se o nodo equivale ao predicado especificado.
        /// </summary>
        public static List<XmlNode> GetTags(this XmlNode node, Func<XmlNode, bool> predicate)
        {
            List<XmlNode> nodes = new List<XmlNode>();

            foreach (XmlNode item in node.ChildNodes)
            {
                if (predicate(item))
                {
                    nodes.Add(item);
                }
            }

            return nodes;
        }




        ///// <summary>
        ///// Adiciona uma coleção de itens em BindingList.
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="list"></param>
        ///// <param name="items"></param>
        //public static void AddRange<T>(this System.ComponentModel.BindingList<T> list, IEnumerable<T> items)
        //{
        //    if (items.IsNotNull())
        //    {
        //        foreach (T item in items)
        //        {
        //            list.Add(item);
        //        }
        //    }
        //}

        ///// <summary>
        ///// Adiciona uma coleção de itens em BindingList.
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="list"></param>
        ///// <param name="items"></param>
        //public static void AddRange<T>(this System.ComponentModel.BindingList<T> list, params T[] args)
        //{
        //    if (args.IsNotNull())
        //    {
        //        foreach (T item in args)
        //        {
        //            list.Add(item);
        //        }
        //    }
        //}


        public static void AddRange<T>(this ICollection<T> collection, params T[] args)
        {
            if (args.NotIsNull() && collection.NotIsNull())
            {
                foreach (T item in args)
                {
                    collection.Add(item);
                }
            }
        }

        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> enumerable)
        {
            if (enumerable.NotIsNull() && collection.NotIsNull())
            {
                foreach (T item in enumerable)
                {
                    collection.Add(item);
                }
            }
        }


        ///// <summary>
        ///// Obtém um bloco de valores da Queue.
        ///// </summary>
        //public static List<T> DequeueRange<T>(this System.Collections.Generic.Queue<T> queue, int count = 1)
        //{
        //    List<T> dequeueList = new List<T>();

        //    for (int idx = 0; idx < count; idx++)
        //    {
        //        if (queue.Count > 0)
        //        {
        //            dequeueList.Add(queue.Dequeue());
        //        }
        //        else
        //        {
        //            break;
        //        }
        //    }

        //    return dequeueList;
        //}

        /// <summary>
        /// Ignora todos os caracteres especificados
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static System.String Ignore(this string input, params char[] charArgs)
        {
            if (charArgs.IsNull() || !charArgs.Any())
                return input;

            string ignoreList = charArgs.JoinText();
            string output = Regex.Replace(input, $"[{ignoreList}]+", "");
            return output;
        }



        /*
        /// <summary>
        /// Remove qualquer caracter fora das tags (<...>).
        /// </summary>
        /// <param name="xmlDoc"></param>
        public static void RemoveControlAndWhiteSpaceOuterTags(this XmlDocument xmlDoc)
        {
            StringBuilder builder = new StringBuilder("");
        

            string xml = xmlDoc.OuterXml.Replace(Environment.NewLine, "")
                                        .Replace("\t", "");

            xmlDoc.LoadXml(xml);

            RemoveControlAndWhiteSpaceOuterTags(xmlDoc.DocumentElement);
        }
        */

        /*
        /// <summary>
        /// Remove qualquer caracter fora das tags (<...>).
        /// </summary>
        /// <param name="xmlDoc"></param>
        public static void RemoveControlAndWhiteSpaceOuterTags(this XmlElement xmlElement)
        {
            xmlElement.InnerText = xmlElement.InnerText.Trim();
            xmlElement.InnerXml = xmlElement.InnerXml.Trim();

            foreach (XmlNode childNode in xmlElement.ChildNodes)
            {
                XmlElement childElement = childNode as XmlElement;

                if (childElement .IsNotNull())
                {
                    RemoveControlAndWhiteSpaceOuterTags(childElement);
                }
                else
                {
                    RemoveControlAndWhiteSpaceOuterTags(childNode);
                }
            }
        }

        /// <summary>
        /// Remove qualquer caracter fora das tags (<...>).
        /// </summary>
        /// <param name="xmlDoc"></param>
        public static void RemoveControlAndWhiteSpaceOuterTags(this XmlNode xmlNode)
        {
            xmlNode.InnerText = xmlNode.InnerText.Trim();
            //xmlNode.InnerXml = xmlNode.InnerXml.Trim();

            foreach (XmlNode childNode in xmlNode.ChildNodes)
            {
                XmlElement childElement = childNode as XmlElement;

                if (childElement .IsNotNull())
                {
                    RemoveControlAndWhiteSpaceOuterTags(childElement);
                }
                else
                {
                    RemoveControlAndWhiteSpaceOuterTags(childNode);
                }
            }
        }
        */

        //************************* EXTENSION METHODS *******************************
    }
}