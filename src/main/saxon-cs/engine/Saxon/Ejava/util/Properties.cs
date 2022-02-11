using System.Collections.Generic;

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

        // public Enumeration<string> propertyNames() {
        //     throw new UnsupportedOperationException("not yet implemented");
        // }
        
        public IEnumerable<string> stringPropertyNames() {
            return dict.Keys;
        }

        public bool isEmpty() {
            // TODO: check the semantics - what about defaults?
            return dict.Count == 0;
        }
        
        
    }

}