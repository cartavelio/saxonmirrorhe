using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.lib {
    public class EmptySource : Saxon.Ejavax.xml.transform.Source {
        private readonly static Saxon.Hej.lib.EmptySource THE_INSTANCE = new Saxon.Hej.lib.EmptySource();
        private EmptySource()  {
        }
        public static Saxon.Hej.lib.EmptySource getInstance() {
            return THE_INSTANCE;
        }
        public void setSystemId(string systemId) {
        }
        public string getSystemId() {
            return null;
        }
    
    }
}