namespace Boycott.SqlTranslator {
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq.Expressions;

    static internal class SqlWhereVisitor {
        public static SqlExpression Visit(Expression expression) {
            expression = StripQuotes(expression);
            
            switch (expression.NodeType) {
                case ExpressionType.Lambda:
                    return VisitLambda((LambdaExpression)expression);
                case ExpressionType.Constant:
                    return VisitConstant((ConstantExpression)expression);
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.Coalesce:
                case ExpressionType.ArrayIndex:
                case ExpressionType.RightShift:
                case ExpressionType.LeftShift:
                case ExpressionType.ExclusiveOr:
                    return VisitBinary((BinaryExpression)expression);
                case ExpressionType.MemberAccess:
                    return VisitMemberAccess((MemberExpression)expression);
                case ExpressionType.Call:
                    return VisitMethodCall((MethodCallExpression)expression);
                case ExpressionType.Convert:
                    return VisitConvert((UnaryExpression)expression);
                default:
                    throw new Exception(expression.NodeType.ToString());
            }
        }

        private static SqlExpression VisitConvert(UnaryExpression expression) {
            if (expression.Operand.NodeType == ExpressionType.MemberAccess) {
                return VisitMemberAccess((MemberExpression)expression.Operand);
            }
            throw new NotImplementedException();
        }

        private static SqlExpression VisitMethodCall(MethodCallExpression methodCallExpression) {
            if (methodCallExpression.Method.DeclaringType.Name == "String") {
                return new SqlWhereClause { Left = Visit(methodCallExpression.Object), Operator = SqlOperator.Like, Right = VisitLike(Visit(methodCallExpression.Arguments[0]), methodCallExpression) };
            }
            
            if (methodCallExpression.Method.Name == "Contains") {
                var arg = ((SqlWhereConstant)Visit(methodCallExpression.Arguments[0])).Value;
                var column = (MemberExpression)methodCallExpression.Arguments[1];
                var left = new SqlWhereColumn { Prefix = ((ParameterExpression)column.Expression).Name, Name = QueryTranslator.GetColumnName(column.Member) };
                var right = new SqlWhereConstant { Value = arg };
                var opera = SqlOperator.In;
                
                return new SqlWhereClause { Left = left, Operator = opera, Right = right };
            }
            
            if (Type.GetTypeCode(methodCallExpression.Method.ReturnType) != TypeCode.Object) {
                var objExpression = Visit(methodCallExpression.Object);
                if (objExpression is SqlWhereConstant) {
                    var args = GetArgumentsValue(methodCallExpression.Arguments);
                    return new SqlWhereConstant { Value = methodCallExpression.Method.Invoke(((SqlWhereConstant)objExpression).Value, args) };
                }
            }
            
            return null;
        }

        private static object[] GetArgumentsValue(ReadOnlyCollection<Expression> arguments) {
            var list = new List<object>();
            foreach (var arg in arguments) {
                var exp = Visit(arg);
                if (exp is SqlWhereConstant) {
                    list.Add(((SqlWhereConstant)exp).Value);
                }
            }
            return list.ToArray();
        }

        private static SqlExpression VisitLike(SqlExpression sqlExpression, MethodCallExpression methodCallExpression) {
            SqlLikeType type;
            if (methodCallExpression.Method.Name == "Contains") {
                type = SqlLikeType.Contains;
            } else if (methodCallExpression.Method.Name == "StartsWith") {
                type = SqlLikeType.StartsWith;
            } else if (methodCallExpression.Method.Name == "EndsWith") {
                type = SqlLikeType.EndsWith;
            } else {
                throw new InvalidOperationException("LIKE");
            }
            
            if (sqlExpression is SqlWhereColumn) {
                return new SqlWhereLikeColumn { Column = (SqlWhereColumn)sqlExpression, Type = type };
            } else if (sqlExpression is SqlWhereConstant) {
                return new SqlWhereLikeConstant { Value = ((SqlWhereConstant)sqlExpression).Value.ToString(), Type = type };
            } else {
                throw new InvalidOperationException("LIKE");
            }
        }

        private static SqlExpression VisitMemberAccess(MemberExpression memberExpression) {
            if (memberExpression.Expression != null && memberExpression.Expression.NodeType == ExpressionType.Parameter) {
                var column = new SqlWhereColumn { Prefix = ((ParameterExpression)memberExpression.Expression).Name, Name = QueryTranslator.GetColumnName(memberExpression.Member) };
                return column;
            } else if (memberExpression.Expression != null && memberExpression.Expression is MemberExpression && ((MemberExpression)memberExpression.Expression).Expression.NodeType == ExpressionType.Parameter) {
                var sql = new SqlWhereColumn { Prefix = ((MemberExpression)memberExpression.Expression).Member.Name, Name = QueryTranslator.GetColumnName(memberExpression.Member) };
                return sql;
            } else if (memberExpression.Expression != null && memberExpression.Expression is MemberExpression && ((MemberExpression)memberExpression.Expression).Expression.NodeType == ExpressionType.Constant) {
                var lambda = Expression.Lambda(memberExpression);
                var fn = lambda.Compile();
                
                return Visit(Expression.Constant(fn.DynamicInvoke(null), memberExpression.Type));
            } else if (memberExpression.Expression != null && memberExpression.Expression is MemberExpression && !(((MemberExpression)memberExpression.Expression).Expression.NodeType == ExpressionType.MemberAccess)) {
                var sql = new SqlWhereColumn { Prefix = ((MemberExpression)memberExpression.Expression).Member.Name, Name = QueryTranslator.GetColumnName(memberExpression.Member) };
                return sql;
            } else {
                var lambda = Expression.Lambda(memberExpression);
                var fn = lambda.Compile();
                
                return Visit(Expression.Constant(fn.DynamicInvoke(null), memberExpression.Type));
            }
        }

        private static SqlExpression VisitConstant(ConstantExpression constantExpression) {
            return new SqlWhereConstant { Value = constantExpression.Value };
        }

        private static SqlExpression VisitBinary(BinaryExpression binaryExpression) {
            return new SqlWhereClause { Left = Visit(binaryExpression.Left), Right = Visit(binaryExpression.Right), Operator = QueryTranslator.GetOperator(binaryExpression.NodeType) };
        }

        private static SqlExpression VisitLambda(LambdaExpression expression) {
            return Visit(expression.Body);
        }

        private static Expression StripQuotes(Expression e) {
            while (e.NodeType == ExpressionType.Quote) {
                e = ((UnaryExpression)e).Operand;
            }
            return e;
        }
    }

    #if DEBUG
    public static class TestSqlWhereVisitor {
        public static SqlExpression Visit(Expression expression) {
            return SqlWhereVisitor.Visit(expression);
        }
    }
    #endif
}
