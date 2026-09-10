using System.Collections.Generic;
using System.Linq;

/*
 * A C# emulation of java.util.Properties, incomplete but sufficient for use in Saxon
 */
 
namespace Saxon.Ejava.util {
    public sealed class Properties {
        private readonly Dictionary<string, string> dict;
        private readonly Properties defaults;
        public Properties() {
            dict = new Dictionary<string, string>();
        }
        public Properties(Properties defaults) {
            dict = new Dictionary<string, string>();
            this.defaults = defaults;
        }
        public string getProperty(string key) {
            if (dict.TryGetValue(key, out var value)) {
                return value;
            } else {
                return defaults?.getProperty(key);
            }
        }

        public string getProperty(string key, string defaultValue) {
            return getProperty(key) ?? defaultValue;
        }
        public void setProperty(string key, string value) {
            dict[key] = value;
        }

        public void remove(string key) {
            dict.Remove(key);
        }

        public void clear() {
            dict.Clear();
        }

        public void putAll(Properties existing) {
            foreach (var kvp in existing.dict) {
                dict.Add(kvp.Key, kvp.Value);
            }
        }
        
        public IEnumerable<string> stringPropertyNames() {
            if (defaults == null) {
                return dict.Keys;
            }
            System.Collections.Generic.HashSet<string> result = new(defaults.stringPropertyNames());
            foreach (string k in dict.Keys)
            {
                result.Add(k);
            }
            return result;
        }

        public bool isEmpty() {
            // TODO: check the semantics - what about defaults?
            return dict.Count == 0;
        }
        
        
    }

}