using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.lib {
    public interface IDynamicLoader {
        void setClassLoader(Saxon.Ejava.lang.ClassLoader classLoader);
        System.Type getClass(string className, Saxon.Hej.lib.Logger traceOut, Saxon.Ejava.lang.ClassLoader classLoader);
        System.Object getInstance(string className, Saxon.Ejava.lang.ClassLoader classLoader);
        System.Object getInstance(string className, Saxon.Hej.lib.Logger traceOut, Saxon.Ejava.lang.ClassLoader classLoader);
        System.IO.Stream getResourceAsStream(string name);
    
    }
}