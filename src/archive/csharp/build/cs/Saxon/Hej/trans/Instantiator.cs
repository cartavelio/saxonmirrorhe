using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.trans {
    public class Instantiator<T>  : Saxon.Hej.trans.Maker<T> {
        private readonly string className;
        private readonly Saxon.Hej.Configuration config;
        public Instantiator(string className, Saxon.Hej.Configuration config)  {
            this.className = className;
            this.config = config;
        }
        public T make() {
            System.Object o = config.getInstance(className);
            try {
                return (T)(o);
            } catch (System.InvalidCastException e) {
                throw new Saxon.Hej.trans.XPathException(((("Instantiating "+className)+" produced an instance of incompatible class ")+o.GetType()));
            }
        }
    
    }
}