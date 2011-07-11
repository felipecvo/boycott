using Boycott.Helpers;
namespace Boycott.SqlTranslator {
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using Boycott.Attributes;

    public class QueryTranslator {
        private SqlQuery Query { get; set; }

        private QueryTranslator(SqlQuery query) {
            Query = query;
        }

        static internal SqlOperator GetOperator(ExpressionType expressionType) {
            switch (expressionType) {
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                    return SqlOperator.Add;
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    return SqlOperator.Subtract;
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                    return SqlOperator.Multiply;
                case ExpressionType.Divide:
                    return SqlOperator.Divide;
                case ExpressionType.Modulo:
                    return SqlOperator.Modulo;
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    return SqlOperator.And;
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return SqlOperator.Or;
                case ExpressionType.LessThan:
                    return SqlOperator.LessThan;
                case ExpressionType.LessThanOrEqual:
                    return SqlOperator.LessThanOrEqual;
                case ExpressionType.GreaterThan:
                    return SqlOperator.GreaterThan;
                case ExpressionType.GreaterThanOrEqual:
                    return SqlOperator.GreaterThanOrEqual;
                case ExpressionType.Equal:
                    return SqlOperator.Equal;
                case ExpressionType.NotEqual:
                    return SqlOperator.NotEqual;
                case ExpressionType.ExclusiveOr:
                    return SqlOperator.ExclusiveOr;
                case ExpressionType.Coalesce:
                case ExpressionType.ArrayIndex:
                case ExpressionType.RightShift:
                case ExpressionType.LeftShift:
                default:
                    throw new Exception(string.Format("Invalid expression type: '{0}'", expressionType));
            }
        }

        static internal string GetColumnName(MemberInfo memberInfo) {
            var attributes = memberInfo.GetCustomAttributes(typeof(ColumnAttribute), false);
            if (attributes != null && attributes.Length > 0) {
                var mapper = attributes[0] as ColumnAttribute;
                if (!string.IsNullOrEmpty(mapper.Name)) {
                    return mapper.Name;
                }
            }
            return Configuration.NamingProvider.GetColumnName(memberInfo.Name);
        }

        public static SqlQuery Translate(Expression expression) {
            var query = new SqlQuery();
            
            var translator = new QueryTranslator(query);
            
            translator.Visit(expression);
            
            return query;
        }

        public Expression Visit(Expression exp) {
            if (exp == null)
                return exp;
            switch (exp.NodeType) {
                case ExpressionType.Call:
                    return VisitMethodCall((MethodCallExpression)exp);
                case ExpressionType.Constant:
                    return VisitConstant((ConstantExpression)exp);
                case ExpressionType.Quote:
                    return VisitUnary((UnaryExpression)exp);
                case ExpressionType.TypeIs:
                    throw new InvalidOperationException("TypeIs is not supported.");
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.Not:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.ArrayLength:
                case ExpressionType.TypeAs:
                //return this.VisitUnary((UnaryExpression)exp);
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
                //return this.VisitBinary((BinaryExpression)exp);
                case ExpressionType.Conditional:
                //return this.VisitConditional((ConditionalExpression)exp);
                case ExpressionType.Parameter:
                //return this.VisitParameter((ParameterExpression)exp);
                case ExpressionType.MemberAccess:
                //return this.VisitMemberAccess((MemberExpression)exp);
                case ExpressionType.Lambda:
                //return this.VisitLambda((LambdaExpression)exp);
                case ExpressionType.New:
                //return this.VisitNew((NewExpression)exp);
                case ExpressionType.NewArrayInit:
                case ExpressionType.NewArrayBounds:
                //return this.VisitNewArray((NewArrayExpression)exp);
                case ExpressionType.Invoke:
                //return this.VisitInvocation((InvocationExpression)exp);
                case ExpressionType.MemberInit:
                //return this.VisitMemberInit((MemberInitExpression)exp);
                case ExpressionType.ListInit:
                default:
                    //return this.VisitListInit((ListInitExpression)exp);
                    throw new Exception(string.Format("Unhandled expression type: '{0}'", exp.NodeType));
            }
        }

        private Expression VisitUnary(UnaryExpression expression) {
            var operand = Visit(expression.Operand);
            if (operand != expression.Operand) {
                return Expression.MakeUnary(expression.NodeType, operand, expression.Type, expression.Method);
            }
            return expression;
        }

        private Expression VisitConstant(ConstantExpression constantExpression) {
            return constantExpression;
        }

        private Expression VisitMethodCall(MethodCallExpression methodCallExpression) {
            switch (methodCallExpression.Method.Name) {
                case "Where":
                    VisitWhere(methodCallExpression);
                    break;
                case "Select":
                    VisitSelect(methodCallExpression);
                    break;
                case "SelectMany":
                    VisitSelectMany(methodCallExpression);
                    break;
                case "Take":
                    VisitTake(methodCallExpression);
                    break;
                case "Skip":
                    VisitSkip(methodCallExpression);
                    break;
                case "ThenBy":
                case "OrderBy":
                    VisitOrderBy(methodCallExpression, SqlOrderBy.Asc);
                    break;
                case "ThenByDescending":
                case "OrderByDescending":
                    VisitOrderBy(methodCallExpression, SqlOrderBy.Desc);
                    break;
                case "First":
                case "Single":
                    VisitFirst(methodCallExpression);
                    break;
                case "Join":
                    VisitJoin(methodCallExpression);
                    break;
                case "Count":
                    VisitCount(methodCallExpression);
                    break;
                case "Max":
                    VisitAverage(methodCallExpression, "MAX");
                    break;
                case "Min":
                    VisitAverage(methodCallExpression, "MIN");
                    break;
                case "Average":
                    VisitAverage(methodCallExpression, "AVG");
                    break;
                case "ToString":
                default:
                    //ProcessToString(methodCallExpression);
                    //break;
                    throw new InvalidOperationException("Invalid method name: " + methodCallExpression.Method.Name);
            }
            return methodCallExpression;
        }

        private void VisitAverage(MethodCallExpression methodCallExpression, string averageFunction) {
            var alias = ((LambdaExpression)StripQuotes(((MethodCallExpression)methodCallExpression.Arguments[0]).Arguments[1])).Parameters[0].Name;
            var column = GetColumnName(((MemberExpression)((LambdaExpression)StripQuotes(methodCallExpression.Arguments[1])).Body).Member);
            Query.AddColumnOutput(string.Format("{0}({1}.{2})", averageFunction, alias, column));
            Query.LockColumnOutput = true;
            Visit(methodCallExpression.Arguments[0]);
        }

        private void VisitCount(MethodCallExpression methodCallExpression) {
            Query.AddColumnOutput("COUNT(*)");
            Query.LockColumnOutput = true;
            if (methodCallExpression.Arguments[0] is ConstantExpression) {
                var table = new SqlTable { Alias = "c", Name = GetTableName(methodCallExpression.Arguments[0]) };
                Query.Tables.Add(table);
            } else {
                Visit(methodCallExpression.Arguments[0]);
            }
        }

        private void VisitJoin(MethodCallExpression expression) {
            if (expression.Arguments[0] is ConstantExpression) {
                var tableName = GetTableName(expression.Arguments[0]);
                var alias = ((LambdaExpression)StripQuotes(expression.Arguments[2])).Parameters[0].Name;
                var table = new SqlTable { Alias = alias, Name = tableName };
                if (!Query.Tables.Contains(table))
                    Query.Tables.Add(table);
                Query.AddColumnOutput(alias + ".*");
            } else {
                Visit(expression.Arguments[0]);
            }
            
            var x1 = (LambdaExpression)StripQuotes(expression.Arguments[2]);
            var x2 = (LambdaExpression)StripQuotes(expression.Arguments[3]);
            var m1 = (MemberExpression)x1.Body;
            var m2 = (MemberExpression)x2.Body;
            
            SqlJoin @join = new SqlJoin { JoinAlias = ((LambdaExpression)StripQuotes(expression.Arguments[3])).Parameters[0].Name, JoinTable = GetTableName(expression.Arguments[1].Type), Left = new SqlWhereColumn { Prefix = GetParameterExpression(m1.Expression), Name = GetColumnName(m1.Member) }, Right = new SqlWhereColumn { Prefix = GetParameterExpression(m2.Expression), Name = GetColumnName(m2.Member) } };
            
            Query.Joins.Add(@join);
        }

        private string GetParameterExpression(Expression expression) {
            if (expression is MemberExpression) {
                return ((MemberExpression)expression).Member.Name;
            } else if (expression is ParameterExpression) {
                return ((ParameterExpression)expression).Name;
            } else {
                return null;
            }
        }

        private void VisitFirst(MethodCallExpression expression) {
            Query.Take = 1;
            if (expression.Arguments[0] is ConstantExpression && ((ConstantExpression)expression.Arguments[0]).Value.GetType().IsSubclassOf(typeof(Base))) {
                var table = new SqlTable { Name = GetTableName(expression.Arguments[0]), Alias = "t" };
                if (!Query.Tables.Contains(table))
                    Query.Tables.Add(table);
            } else {
                Visit(expression.Arguments[0]);
            }
        }

        private void VisitSelectMany(MethodCallExpression expression) {
            ProcessTable(expression);
            
            var table = new SqlTable { Alias = ((LambdaExpression)StripQuotes(expression.Arguments[2])).Parameters[1].Name, Name = GetTableName(((LambdaExpression)StripQuotes(expression.Arguments[1])).Body.Type) };
            if (!Query.Tables.Contains(table))
                Query.Tables.Add(table);
        }

        private void VisitOrderBy(MethodCallExpression expression, SqlOrderBy orderBy) {
            ProcessTable(expression);
            
            var lambda = (LambdaExpression)((UnaryExpression)expression.Arguments[1]).Operand;
            var member = (MemberExpression)lambda.Body;
            string column = GetColumnName(member.Member);
            var tableAlias = lambda.Parameters[0].Name;
            if (member.Expression is MemberExpression) {
                tableAlias = ((MemberExpression)member.Expression).Member.Name;
            }
            var order = new SqlOrderByColumn { Prefix = tableAlias, Name = column, Direction = orderBy };
            Query.OrderBy.Add(order);
        }

        private void VisitSkip(MethodCallExpression expression) {
            Query.Skip = (int)((ConstantExpression)expression.Arguments[1]).Value;
            Visit(expression.Arguments[0]);
        }

        private void VisitTake(MethodCallExpression expression) {
            Query.Take = (int)((ConstantExpression)expression.Arguments[1]).Value;
            Visit(expression.Arguments[0]);
        }

        private void VisitSelect(MethodCallExpression expression) {
            if (expression.Arguments[0].Type.IsSubclassOf(typeof(Base))) {
                var tableName = GetTableName(((ConstantExpression)expression.Arguments[0]).Value);
                var alias = ((LambdaExpression)StripQuotes(expression.Arguments[1])).Parameters[0].Name;
                var table = new SqlTable { Alias = alias, Name = tableName };
                if (!Query.Tables.Contains(table))
                    Query.Tables.Add(table);
            } else {
                Visit(expression.Arguments[0]);
            }
            
            var output = ((LambdaExpression)StripQuotes(expression.Arguments[1])).Body;
            
            if (output.NodeType == ExpressionType.MemberAccess) {
                if (!output.Type.IsSubclassOf(typeof(Base))) {
                    AddColumnOutput((MemberExpression)output);
                } else {
                    Query.AddColumnOutput(string.Format("{0}.*", ((MemberExpression)output).Member.Name));
                }
            } else if (output.NodeType == ExpressionType.New) {
                var ne = (NewExpression)output;
                foreach (var arg in ne.Arguments) {
                    if (arg is MemberExpression) {
                        if (!arg.Type.IsSubclassOf(typeof(Base))) {
                            AddColumnOutput((MemberExpression)arg);
                        } else {
                            Query.AddColumnOutput(((MemberExpression)arg).Member.Name + ".*");
                        }
                    } else {
                        Query.AddColumnOutput(((ParameterExpression)arg).Name + ".*");
                    }
                }
            }
        }

        private void AddColumnOutput(MemberExpression ma) {
            var name = GetColumnName(ma.Member);
            var alias = ((ParameterExpression)ma.Expression).Name;
            Query.AddColumnOutput(string.Format("{0}.{1}", alias, name));
        }

        private void VisitWhere(MethodCallExpression expression) {
            ProcessTable(expression);
            
            if (Query.WhereClause == null) {
                Query.WhereClause = SqlWhereVisitor.Visit(expression.Arguments[1]);
            } else {
                Query.WhereClause = new SqlWhereClause { Left = Query.WhereClause, Operator = SqlOperator.And, Right = SqlWhereVisitor.Visit(expression.Arguments[1]) };
            }
        }

        private void ProcessTable(MethodCallExpression expression) {
            if (expression.Arguments[0].Type.IsSubclassOf(typeof(Base))) {
                var tableName = GetTableName(expression.Arguments[0]);
                var alias = ((LambdaExpression)StripQuotes(expression.Arguments[1])).Parameters[0].Name;
                var table = new SqlTable { Alias = alias, Name = tableName };
                if (!Query.Tables.Contains(table))
                    Query.Tables.Add(table);
            } else {
                this.Visit(expression.Arguments[0]);
            }
        }

        private static Expression StripQuotes(Expression e) {
            while (e.NodeType == ExpressionType.Quote) {
                e = ((UnaryExpression)e).Operand;
            }
            return e;
        }

        private string GetTableName(object type) {
            if (type is ConstantExpression)
                type = ((ConstantExpression)type).Value;
            if (type is Type) {
                if (((Type)type).Name == "IEnumerable`1") {
                    type = TypeSystem.GetElementType((Type)type);
                }
                type = Activator.CreateInstance((Type)type);
            }
            return ((Base)type).Mapper.Name;
        }
    }
}
