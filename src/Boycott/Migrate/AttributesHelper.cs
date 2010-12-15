namespace Boycott.Migrate {
    using System.Collections.Generic;
    using System.ComponentModel;

    static internal class AttributesHelper {
        static internal AttributesList Parse(object attributes) {
            var props = TypeDescriptor.GetProperties(attributes);
            var attrs = new AttributesList();
            
            foreach (PropertyDescriptor item in props) {
                attrs.Add(item.Name, item.GetValue(attributes));
            }
            
            return attrs;
        }
    }

    internal class AttributesList : Dictionary<string, object> {
        public T GetValueOrDefault<T>(string key, T _default) {
            try {
                if (ContainsKey(key)) {
                    return (T)this[key];
                }
            } catch {
            }
            
            return _default;
        }
    }
}
