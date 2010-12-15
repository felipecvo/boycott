namespace Boycott.Extensions {
    using System;
    using System.Reflection;

    public static class PropertyInfoExtension {
        public static T GetAttribute<T>(this PropertyInfo prop) where T : Attribute {
            var attrs = prop.GetCustomAttributes(typeof(T), false);
            if (attrs.Length == 1)
                return (T)attrs[0];
            
            return default(T);
        }

        public static T GetAttribute<T>(this PropertyInfo prop, Type type, Func<Type, T> method) where T : Attribute {
            var attrs = prop.GetCustomAttributes(type, false);
            if (attrs.Length == 1)
                return (T)attrs[0];
            
            if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition().Equals(typeof(Nullable<>))) {
                return method(prop.PropertyType.GetGenericArguments()[0]);
            } else {
                return method(prop.PropertyType);
            }
        }

        public static bool HasAttribute(this PropertyInfo prop, Type type) {
            var attrs = prop.GetCustomAttributes(type, false);
            
            return attrs.Length == 1;
        }
    }
}
