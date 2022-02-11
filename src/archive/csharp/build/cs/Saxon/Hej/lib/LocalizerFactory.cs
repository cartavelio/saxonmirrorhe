using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.lib {
    public abstract class LocalizerFactory {
        public void setLanguageProperties(string lang, Saxon.Ejava.util.Properties properties) {
        }
        public abstract Saxon.Hej.lib.Numberer getNumberer(string language, string country);
        public Saxon.Hej.lib.LocalizerFactory copy() {
            return this;
        }
    
    }
}