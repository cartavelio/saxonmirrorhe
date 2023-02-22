using System;
using System.Collections.Generic;
using java.lang;

/**
 * A C# emulation of java.util.Properties, incomplete but sufficient for use in Saxon
 */
 
namespace java.util {
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
            string value;
            if (dict.TryGetValue(key, out value)) {
                return value;
            } else if (defaults != null) {
                return defaults.getProperty(key);
            } else {
                return null;
            }
        }
        public void setProperty(string key, string value) {
            dict.Add(key, value);
        }

        public Enumeration<string> propertyNames() {
            throw new UnsupportedOperationException("not yet implemented");
        }
    }

}