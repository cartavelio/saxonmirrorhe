using System;
using System.IO;
using java.lang;
using net.sf.saxon.lib;

namespace SaxonCS3.Saxon.Impl.Helpers {
    public class DynamicLoader : IDynamicLoader{
        public void setClassLoader(ClassLoader classLoader) {
            // No action
        }

        public Type getClass(string className, Logger traceOut, ClassLoader classLoader) {
            if (className == "net.sf.saxon.serialize.MessageEmitter") {
                return typeof(net.sf.saxon.serialize.MessageEmitter);
            }
            throw new NotImplementedException();
        }

        public object getInstance(string className, ClassLoader classLoader) {
            Type type = getClass(className, null, classLoader);
            return System.Activator.CreateInstance(type);
        }

        public object getInstance(string className, Logger traceOut, ClassLoader classLoader) {
            return getInstance(className, classLoader);
        }

        public Stream getResourceAsStream(string name) {
            throw new NotImplementedException();
        }
    }
}