using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.registry {
    public class XQUpdate10FunctionSet : Saxon.Hej.functions.registry.BuiltInFunctionSet {
        public static Saxon.Eej.functions.registry.XQUpdate10FunctionSet THE_INSTANCE = new Saxon.Eej.functions.registry.XQUpdate10FunctionSet();
        public static Saxon.Eej.functions.registry.XQUpdate10FunctionSet getInstance() {
            return THE_INSTANCE;
        }
        public XQUpdate10FunctionSet()  {
            init();
        }
        private void init() {
            importFunctionSet(Saxon.Hej.functions.registry.XPath31FunctionSet.getInstance());
            register("put", 2, typeof(Saxon.Hej.functions.Put), Saxon.Hej.type.AnyItemType.getInstance(), OPT, LATE).arg(0, Saxon.Hej.type.Type.NODE_TYPE, ONE, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null);
        }
    
    }
}