//using GData.Utils.Extensions;

using CoreDll.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace CoreDll.Utils
{
    public class SqlHelper
    {

        private string StrTabela;
        private Dictionary<string, string> DicCampos = new Dictionary<string, string>();
        private List<string> LstCondicoes = new List<string>();
        //private string IdentityFieldName { get; set; }

        public SqlHelper(string StrNomeTabela)
        {

            StrTabela = StrNomeTabela.Trim();
        }

        public static List<string> GetTableFieldsName(string StrTableName, string StrDataBaseName = "")
        {

            string StrSql = "";
            List<string> fields = new List<string>();

            //
            StrDataBaseName = System.Convert.ToString(!StrDataBaseName.IsEmpty() ? StrDataBaseName.Trim() + "." : "");


            StrSql = " SELECT   COLUNAS.name " +
                " FROM     {1}dbo.sysobjects AS TABELAS " +
                "          INNER JOIN dbo.syscolumns AS COLUNAS " +
                "          ON COLUNAS.id = TABELAS.id " +
                " WHERE    TABELAS.name = {0}";

            StrSql = System.Convert.ToString(StrSql.FormatText(StrTableName.Aspa(), StrDataBaseName));


            DataTable table = CoreDll.Utils.ConexaoHelper.Connection.ExecuteDataTable(StrSql);

            foreach (DataRow row in table.Rows)
            {
                fields.Add(row["name"].ToString().ToUpper());
            }

            //CoreDll.Utils.ConexaoHelper.Connection.CloseDataReader();

            return fields;
        }


        /*
        public SqlHelper SetIdentityField(string fieldName)
        {
            this.IdentityFieldName = fieldName;
            return this;
        }
        */

        public SqlHelper SetParamInstance(object instance, params string[] fields)
        {

            Type theType = instance.GetType();
            PropertyInfo[] properties = theType.GetProperties();


            foreach (string field in fields)
            {
                string StrCampoClasse = "";
                string StrCampoTabela = "";
                object fieldValue = null;
                //Dim TipoConversao As Type


                StrCampoClasse = field.SplitText(" AS ").First().Trim();
                StrCampoTabela = field.SplitText(" AS ").Last().Trim();



                fieldValue = theType.GetProperty(StrCampoClasse).GetValue(instance, null);
                //TipoConversao = theType.GetProperty(StrCampoClasse).GetType


                if (fieldValue == null || fieldValue == DBNull.Value)
                {
                    SetParam(StrCampoTabela, null);
                }
                else if (fieldValue is DateTime || fieldValue is DateTime? || fieldValue is string || fieldValue is char)
                {
                    SetParam(StrCampoTabela, fieldValue.ToString().Replace("'", "''"), true, TiposDeConversaoEnum.EmptyToNull);
                }
                else if (fieldValue is bool?)
                {
                    SetParam(StrCampoTabela, fieldValue.ToString(), false, TiposDeConversaoEnum.BooToBit);
                }
                else if (fieldValue is bool)
                {
                    SetParam(StrCampoTabela, fieldValue.ToString(), false, TiposDeConversaoEnum.BooToBit);
                }
                else if (fieldValue is short? || fieldValue is UInt16? || fieldValue is int? || fieldValue is UInt32? || fieldValue is long? || fieldValue is UInt64? || fieldValue is long? || fieldValue is short? || fieldValue is SByte? || fieldValue is byte?)
                {
                    SetParam(StrCampoTabela, fieldValue.ToString(), false, TiposDeConversaoEnum.EmptyToNull);
                }
                else if (fieldValue is short || fieldValue is UInt16 || fieldValue is int || fieldValue is UInt32 || fieldValue is long || fieldValue is UInt64 || fieldValue is long || fieldValue is short || fieldValue is SByte || fieldValue is byte)
                {
                    SetParam(StrCampoTabela, fieldValue.ToString(), false, TiposDeConversaoEnum.NullToZero);
                }
                else if (fieldValue is decimal? || fieldValue is Single? || fieldValue is double?)
                {
                    SetParam(StrCampoTabela, fieldValue.ToString(), false, TiposDeConversaoEnum.EmptyToNull);
                }
                else if (fieldValue is decimal || fieldValue is Single || fieldValue is double)
                {
                    SetParam(StrCampoTabela, fieldValue.ToString(), false, TiposDeConversaoEnum.NullToZero);
                }

            }

            return this;
        }

        public SqlHelper SetParam(string nomeCampo, object valor, bool adicionarAspas = false, TiposDeConversaoEnum tipoConversao = TiposDeConversaoEnum.SemForcarConversao)
        {

            string StrValorFinal = "";

            if (Convert.IsDBNull(valor) || valor.IsNull())
            {

                if (tipoConversao == TiposDeConversaoEnum.NullToEmpty)
                {

                    StrValorFinal = "''";
                }
                else if (tipoConversao == TiposDeConversaoEnum.NullToZero)
                {

                    StrValorFinal = System.Convert.ToString(adicionarAspas ? ("'0'") : "0");
                }
                else
                {
                    StrValorFinal = "Null";
                }
            }
            else
            {

                if (valor.IsEnum())
                {
                    valor = ((Enum)valor).GetHashCode();
                }

                if (valor.GetType() == typeof(bool))
                {
                    valor = ((bool)valor) ? 1 : 0;
                }


                if (tipoConversao == TiposDeConversaoEnum.BooToBit)
                {

                    StrValorFinal = System.Convert.ToString((System.Convert.ToBoolean(valor)) ? 1 : 0);

                }
                else if (tipoConversao == TiposDeConversaoEnum.EmptyToNull)
                {

                    StrValorFinal = System.Convert.ToString(valor.ToString().ReturnsDefaultIfEmpty("Null")); // IfIsEmpty(valor.ToString, "Null")

                }
                else if (tipoConversao == TiposDeConversaoEnum.ZeroToNull)
                {

                    StrValorFinal = (System.Convert.ToInt32(valor) == 0) ? "Null" : valor.ToString();

                }
                else if (tipoConversao == TiposDeConversaoEnum.SemForcarConversao)
                {

                    StrValorFinal = valor.ToString();

                }
                else
                {

                    if (!adicionarAspas && valor.IsNumeric())
                    {

                        StrValorFinal = valor.ToString().Replace(",", ".");
                    }
                    else
                    {

                        StrValorFinal = valor.ToString();
                    }
                }

                StrValorFinal = System.Convert.ToString(StrValorFinal != "Null" && adicionarAspas ? (StrValorFinal.Aspa()) : StrValorFinal);

            }

            DicCampos[nomeCampo.Trim()] = StrValorFinal;
            //DicCampos.Add(nomeCampo.Trim(), StrValorFinal);

            return this;
        }

        public SqlHelper Where(string condicao, params object[] values)
        {
            int index = 0;

            foreach (object value in values)
            {
                condicao = condicao.Replace("{" + index.ToString() + "}", value.ToString());
                index++;
            }

            LstCondicoes.Add(condicao.Trim());

            return this;
        }

        public SqlHelper Where(string whereExpression)
        {
            LstCondicoes.Add(whereExpression);
            return this;
        }


        public string From
        {
            get
            {
                return StrTabela;
            }
        }


        public string GetSqlInsert()
        {
            string StrInsertInto = "";
            string StrFields = "";
            string StrValues = "";
            List<string> ListaSets = new List<string>();
            List<string> ListaFields = new List<string>();
            KeyValuePair<string, string> ItemPair = default(KeyValuePair<string, string>);

            foreach (KeyValuePair<string, string> tempLoopVar_ItemPair in DicCampos)
            {
                ItemPair = tempLoopVar_ItemPair;

                ListaFields.Add(ItemPair.Key);
                ListaSets.Add(string.Format("{0} AS [{1}]", ItemPair.Value, ItemPair.Key));
            }

            //StrFields = String.Join(", ", ListaFields.ToArray())
            //StrSets = String.Join(", ", ListaSets.ToArray())
            //StrFrom = StrTabela


            StrInsertInto = "insert into [{0}] ".FormatText(StrTabela);
            StrFields = " ({0}) ".FormatText(string.Join(", ", ListaFields.ToArray()));
            StrValues = " select " + string.Join(", ", ListaSets.ToArray());

            return (StrInsertInto + StrFields + StrValues).Trim();


            //Return String.Format("INSERT INTO [{0}] ( {1} ) SELECT {2}", StrFrom, StrFields, StrSets)
        }

        public string GetSqlUpdate()
        {

            string StrUpdate = "";
            string StrSet = "";
            string StrWhere = "";
            List<string> ListaSets = new List<string>();
            KeyValuePair<string, string> ItemPair = default(KeyValuePair<string, string>);



            foreach (KeyValuePair<string, string> tempLoopVar_ItemPair in DicCampos)
            {
                ItemPair = tempLoopVar_ItemPair;

                ListaSets.Add(string.Format("[{0}] = {1}", ItemPair.Key, ItemPair.Value));
            }


            StrUpdate = "update " + StrTabela;
            StrSet = " set " + string.Join(", ", ListaSets.ToArray());
            StrWhere = string.Join(" and ", LstCondicoes.ToArray());

            StrWhere = System.Convert.ToString(StrWhere.IsEmpty() ? "" : " where " + StrWhere);

            return (StrUpdate + StrSet + StrWhere).Trim();
        }

        public string GetSqlSelect()
        {

            string StrSelect = "";
            string StrWhere = "";
            string StrFrom = "";
            KeyValuePair<string, string> ItemPair = default(KeyValuePair<string, string>);
            List<string> ListaFields = new List<string>();

            foreach (KeyValuePair<string, string> tempLoopVar_ItemPair in DicCampos)
            {
                ItemPair = tempLoopVar_ItemPair;
                ListaFields.Add("[" + ItemPair.Key + "]");
            }

            StrSelect = "select " + string.Join(", ", ListaFields.ToArray());
            StrFrom = " from " + StrTabela.Trim();
            StrWhere = string.Join(" AND ", LstCondicoes.ToArray());

            StrWhere = System.Convert.ToString(StrWhere.IsEmpty() ? "" : " where " + StrWhere);

            return (StrSelect + StrFrom + StrWhere).Trim();
        }
    }
}