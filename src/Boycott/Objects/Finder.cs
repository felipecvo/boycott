namespace Boycott.Objects {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    public class Finder {
        private static readonly MethodInfo whereMethod;
        private static readonly MethodInfo singleMethod;
        private static readonly MethodInfo selectMethod;

        static Finder() {
            var methods = typeof(Queryable).GetMethods();
            whereMethod = methods.Where(x => x.Name == "Where" && x.GetParameters()[1].ParameterType.GetGenericArguments()[0].Name == "Func`2").Single();
            singleMethod = methods.Where(x => x.Name == "Single" && x.GetParameters().Length == 1).Single();
            selectMethod = methods.Where(x => x.Name == "Select" && x.GetParameters()[1].ParameterType.GetGenericArguments()[0].Name == "Func`2").Single();
        }

        public static T Find<T>(int id, PropertyInfo propertyInfo) {
            var genericType = new Type[] { typeof(T) };
            var type = Expression.Constant(Activator.CreateInstance<T>());
            var parameter = (ParameterExpression)ParameterExpression.Parameter(type.Type, "a");
            
            var property = Expression.Property(parameter, propertyInfo);
            var binary = BinaryExpression.Equal(property, Expression.Constant(id));
            var unary = UnaryExpression.Lambda(binary, new ParameterExpression[] { parameter });
            var callWhere = Expression.Call(null, whereMethod.MakeGenericMethod(genericType), new Expression[] { type, unary });
            var call = Expression.Call(null, singleMethod.MakeGenericMethod(genericType), new Expression[] { callWhere });
            
            return Configuration.DatabaseProvider.Execute<T>(call);
        }

        public static List<T> FindBy<T>(object value, PropertyInfo propertyInfo)
        {
            var genericType = new Type[] { typeof(T) };
            var type = Expression.Constant(Activator.CreateInstance<T>());
            var parameter = (ParameterExpression)ParameterExpression.Parameter(type.Type, "a");

            var property = Expression.Property(parameter, propertyInfo);
            var binary = BinaryExpression.Equal(property, Expression.Constant(value));
            var unary = UnaryExpression.Lambda(binary, new ParameterExpression[] { parameter });
            var callWhere = Expression.Call(null, whereMethod.MakeGenericMethod(genericType), new Expression[] { type, unary });
            var call = Expression.Call(null, singleMethod.MakeGenericMethod(genericType), new Expression[] { callWhere });

            return Configuration.DatabaseProvider.Execute<IEnumerable<T>>(call) as List<T>;
        }

        public static List<T> All<T>() {
            var genericType = new Type[] { typeof(T), typeof(T) };
            var type = Expression.Constant(Activator.CreateInstance<T>());
            var parameter = (ParameterExpression)ParameterExpression.Parameter(type.Type, "a");
            
            var unary = UnaryExpression.Lambda(parameter, new ParameterExpression[] { parameter });
            var call = Expression.Call(null, selectMethod.MakeGenericMethod(genericType), new Expression[] { type, unary });
            
            return Configuration.DatabaseProvider.Execute<IEnumerable<T>>(call) as List<T>;
        }

        public static List<T> FindAll<T>(object parameters) {
            var genericType = new Type[] { typeof(T) };
            var type = Expression.Constant(Activator.CreateInstance<T>());
            var parameter = (ParameterExpression)ParameterExpression.Parameter(type.Type, "a");
            
            var props = parameters.GetType().GetProperties();
            var conditions = new Stack<Expression>();
            foreach (var item in props) {
                var property = Expression.Property(parameter, type.Type.GetProperty(item.Name));
                var binary = BinaryExpression.Equal(property, Expression.Constant(item.GetValue(parameters, null)));
                conditions.Push(binary);
            }
            
            Expression exp;
            while ((exp = conditions.Pop()) != null) {
                if (conditions.Count > 0) {
                    var right = conditions.Pop();
                    conditions.Push(BinaryExpression.AndAlso(exp, right));
                } else {
                    conditions.Push(exp);
                    break;
                }
            }
            
            var unary = UnaryExpression.Lambda(conditions.Pop(), new ParameterExpression[] { parameter });
            var call = Expression.Call(null, whereMethod.MakeGenericMethod(genericType), new Expression[] { type, unary });
            return Configuration.DatabaseProvider.Execute<IEnumerable<T>>(call) as List<T>;
        }
    }
}
