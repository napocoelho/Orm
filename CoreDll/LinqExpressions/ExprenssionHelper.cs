using System;
using System.Linq.Expressions;

namespace CoreDll.LinqExpressions
{
    public static class ExprenssionHelper
    {
        /// <summary>
        /// Exemplos: CoreDll.LinqExpressions.ExprenssionHelper.MemberName(() => Controller.Filtro) ---> retornará ---> "Controller.Filtro"
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        private static string GetMemberNameHierarchy(Expression expression)
        {
            switch (expression?.NodeType)
            {
                case ExpressionType.MemberAccess:
                    var memberExpression = (MemberExpression)expression;
                    var supername = GetMemberNameHierarchy(memberExpression.Expression);

                    if (String.IsNullOrEmpty(supername))
                        return memberExpression.Member.Name;
                    else
                        return String.Concat(supername, '.', memberExpression.Member.Name);
                case ExpressionType.Call:
                    var callExpression = (MethodCallExpression)expression;
                    return callExpression.Method.Name;
                case ExpressionType.Convert:
                    var unaryExpression = (UnaryExpression)expression;
                    return GetMemberNameHierarchy(unaryExpression.Operand);
                case ExpressionType.Parameter:
                case ExpressionType.Constant: //Change
                    return String.Empty;
                default:
                    throw new ArgumentException("The expression is not a member access or method call expression");
            }
        }

        /// <summary>
        /// Exemplos: CoreDll.LinqExpressions.ExprenssionHelper.MemberName(() => Controller.Filtro) ---> retornará ---> "Filtro"
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        private static string GetMemberName(Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.MemberAccess:
                    var memberExpression = (MemberExpression)expression;
                    var supername = GetMemberName(memberExpression.Expression);

                    if (String.IsNullOrEmpty(supername))
                        return memberExpression.Member.Name;
                    else
                        return memberExpression.Member.Name;
                case ExpressionType.Call:
                    var callExpression = (MethodCallExpression)expression;
                    return callExpression.Method.Name;
                case ExpressionType.Convert:
                    var unaryExpression = (UnaryExpression)expression;
                    return GetMemberName(unaryExpression.Operand);
                case ExpressionType.Parameter:
                case ExpressionType.Constant: //Change
                    return String.Empty;
                default:
                    throw new ArgumentException("The expression is not a member access or method call expression");
            }
        }

        public static string MemberName<T, T2>(Expression<Func<T, T2>> expression)
        {
            return GetMemberName(expression.Body);
        }

        //NEW
        public static string MemberName<T>(Expression<Func<T>> expression)
        {
            return GetMemberName(expression.Body);
        }

        public static string FullMemberName<T, T2>(Expression<Func<T, T2>> expression)
        {
            return GetMemberNameHierarchy(expression.Body);
        }

        public static string FullMemberName<T>(Expression<Func<T>> expression)
        {
            return GetMemberNameHierarchy(expression.Body);
        }

        /*
        public static void Bind<TC, TD, TP>(this TC control, Expression<Func<TC, TP>> controlProperty, TD dataSource, Expression<Func<TD, TP>> dataMember) where TC : Control
        {
            control.DataBindings.Add(Name(controlProperty), dataSource, Name(dataMember));
        }

        public static void BindLabelText<T>(this Label control, T dataObject, Expression<Func<T, object>> dataMember)
        {
            // as this is way one any type of property is ok
            control.DataBindings.Add("Text", dataObject, Name(dataMember));
        }

        public static void BindEnabled<T>(this Control control, T dataObject, Expression<Func<T, bool>> dataMember)
        {
            control.Bind(c => c.Enabled, dataObject, dataMember);
        }
        */
    }
}