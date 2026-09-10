using System;
using System.IO;
using Saxon.Ejava.lang;
using Saxon.Hej.lib;

namespace Saxon.Impl.Helpers {
    public class DynamicLoader : IDynamicLoader{
        public void setClassLoader(ClassLoader classLoader) {
            // No action
        }

        public Type getClass(string className, Logger traceOut, ClassLoader classLoader) {
            if (className == "Saxon.Hej.serialize.MessageEmitter") {
                return typeof(Saxon.Hej.serialize.MessageEmitter);
            }
            throw new NotImplementedException("Cannot load class " + className);
        }

        public object getInstance(string className, ClassLoader classLoader) {
            Type type = getClass(className, null, classLoader);
            return System.Activator.CreateInstance(type);
        }

        public object getInstance(string className, Logger traceOut, ClassLoader classLoader) {
            return getInstance(className, classLoader);
        }

        public Stream getResourceAsStream(string name) {
            throw new NotImplementedException("cannot load resource " + name);
        }
    }
}

// Copyright (c) 2021 Saxonica Limited