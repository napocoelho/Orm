using CoreDll.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreDll.Utils
{
    public static class Xql
    {
        public static XqlCommands Select(string expression)
        {
            return new XqlCommands().Select(expression);
        }

        public static XqlCommands Top(int value)
        {
            return new XqlCommands().Top(value);
        }

        public static XqlCommands From(string expression)
        {
            return new XqlCommands().From(expression);
        }

        public static XqlCommands Where(string expression, params object[] args)
        {
            return new XqlCommands().Where(expression, args);
        }

        public static XqlCommands GroupBy(string expression)
        {
            return new XqlCommands().GroupBy(expression);
        }

        public static XqlCommands OrderBy(string expression)
        {
            return new XqlCommands().OrderBy(expression);
        }
    }


    public class XqlCommands
    {
        public XqlExpression Info { get; private set; }


        public XqlCommands()
        {
            Info = new XqlExpression();
        }

        public XqlCommands(XqlExpression info)
        {
            Info = info;
        }

        public XqlCommands Select(string expression)
        {
            Info.Select = expression;
            return this;
        }

        public XqlCommands From(string expression)
        {
            Info.From = expression;
            return this;
        }

        public XqlCommands Top(int value)
        {
            Info.Top = value;
            return this;
        }

        public XqlCommands Where(string expression, params object[] args)
        {
            for (int idx = 0; idx < args.Length; idx++)
            {
                Enum value = args[idx] as Enum;

                if (value != null)
                {
                    args[idx] = value.GetHashCode();
                }
            }

            Info.Where = expression.FormatText(args);
            return this;
        }

        public XqlCommands Where(string expression)
        {
            Info.Where = expression;
            return this;
        }

        public XqlCommands GroupBy(string expression)
        {
            Info.GroupBy = expression;
            return this;
        }

        public XqlCommands OrderBy(string expression)
        {
            Info.OrderBy = expression;
            return this;
        }

        public string Query()
        {
            return Info.ToString();
        }

        public XqlUnionCommands Union(XqlExpression info)
        {
            return new XqlUnionCommands(Info, info);
        }
    }


    [Serializable]
    public class XqlExpression
    {
        public string Select { get; set; }
        public int Top { get; set; }
        public string From { get; set; }
        public string Where { get; set; }
        public string GroupBy { get; set; }
        public string OrderBy { get; set; }


        public XqlExpression()
        {
            Select = string.Empty;
            Top = 0;
            From = string.Empty;
            Where = string.Empty;
            GroupBy = string.Empty;
            OrderBy = string.Empty;
        }


        public XqlExpression AddWhere(string expression, params string[] args)
        {
            if (expression.Trim() != string.Empty)
            {
                if (Where.Trim() != string.Empty)
                {
                    Where = Where + " AND " + expression.FormatText(args);
                }
                else
                {
                    Where = expression;
                }
            }

            return this;
        }

        public XqlExpression AddWhere(string expression)
        {
            if (expression.Trim() != string.Empty)
            {
                if (Where.Trim() != string.Empty)
                {
                    Where = Where + " AND " + expression;
                }
                else
                {
                    Where = expression;
                }
            }

            return this;
        }

        public XqlExpression AddGroupBy(string expression)
        {
            if (expression.Trim() != string.Empty)
            {
                if (GroupBy.Trim() != string.Empty)
                {
                    GroupBy = GroupBy + ", " + expression;
                }
                else
                {
                    GroupBy = expression;
                }
            }

            return this;
        }

        public XqlExpression AddOrderBy(string expression)
        {
            if (expression.Trim() != string.Empty)
            {
                if (OrderBy.Trim() != string.Empty)
                {
                    OrderBy = OrderBy + ", " + expression;
                }
                else
                {
                    OrderBy = expression;
                }
            }

            return this;
        }

        public XqlCommands ToCommand()
        {
            return new XqlCommands(this);
        }

        public XqlExpression MakeClone()
        {
            return ObjectCopier.Clone<XqlExpression>(this);
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder("");

            builder.Append("SELECT ");

            if (Top > 0)
            {
                builder.Append(" TOP " + Top + " ");
            }

            if (Select.Trim() != string.Empty)
            {
                builder.Append(Select);
            }
            else
            {
                builder.Append("*");
            }

            if (From.Trim() != string.Empty)
            {
                builder.Append(" FROM ");
                builder.Append(From);
            }

            if (Where.Trim() != string.Empty)
            {
                builder.Append(" WHERE ");
                builder.Append(Where);
            }

            if (GroupBy.Trim() != string.Empty)
            {
                builder.Append(" GROUP BY ");
                builder.Append(GroupBy);
            }

            if (OrderBy.Trim() != string.Empty)
            {
                builder.Append(" ORDER BY ");
                builder.Append(OrderBy);
            }

            return builder.ToString();
        }
    }




    public class XqlUnionCommands
    {
        public XqlUnionExpression Info { get; private set; }

        public XqlUnionCommands()
        {
            Info = new XqlUnionExpression();
        }

        public XqlUnionCommands(params XqlExpression[] infoArgs)
            : this()
        {
            Info.Unions.AddRange(infoArgs);
        }

        public XqlUnionCommands Union(params XqlExpression[] infoArgs)
        {
            Info.Unions.AddRange(infoArgs);
            return this;
        }

        public string Query()
        {
            return Info.ToString();
        }
    }


    [Serializable]
    public class XqlUnionExpression
    {
        public string OrderBy { get; set; }
        public List<XqlExpression> Unions { get; private set; }



        public XqlUnionExpression()
        {
            Unions = new List<XqlExpression>();
            OrderBy = string.Empty;
        }

        public XqlUnionExpression(XqlExpression exp1, XqlExpression exp2)
            : this()
        {
            Unions.Add(exp1);
            Unions.Add(exp2);
        }

        public XqlUnionExpression MakeClone()
        {
            return ObjectCopier.Clone<XqlUnionExpression>(this);
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder("");

            for (int idx = 0; idx < Unions.Count(); idx++)
            {
                if (idx > 0)
                {
                    builder.Append(" UNION ");
                }

                string orderBySave = Unions[idx].OrderBy;
                Unions[idx].OrderBy = string.Empty;

                builder.Append(Unions[idx].ToString());

                Unions[idx].OrderBy = orderBySave;
            }

            if (Unions.Count() > 1 && OrderBy.Trim() != string.Empty)
            {
                builder.Append(" ORDER BY ");
                builder.Append(OrderBy);
            }

            return builder.ToString();
        }
    }
}