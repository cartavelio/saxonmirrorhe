using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions.registry {
    public class UseWhen30FunctionSet : Saxon.Hej.functions.registry.BuiltInFunctionSet {
        private static readonly Saxon.Hej.functions.registry.UseWhen30FunctionSet THE_INSTANCE = new Saxon.Hej.functions.registry.UseWhen30FunctionSet();
        public static Saxon.Hej.functions.registry.UseWhen30FunctionSet getInstance() {
            return THE_INSTANCE;
        }
        protected internal UseWhen30FunctionSet()  {
            init();
        }
        protected internal void init() {
            addXPathFunctions();
            register("available-system-properties", 0, e => e.populate(() => new Saxon.Hej.functions.AvailableSystemProperties(), Saxon.Hej.type.BuiltInAtomicType.QNAME, STAR, LATE));
            register("element-available", 1, e => e.populate(() => new Saxon.Hej.functions.ElementAvailable(), Saxon.Hej.type.BuiltInAtomicType.BOOLEAN, ONE, NS).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null));
            register("function-available", 1, e => e.populate(() => new Saxon.Hej.functions.FunctionAvailable(), Saxon.Hej.type.BuiltInAtomicType.BOOLEAN, ONE, (NS|LATE)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null));
            register("function-available", 2, e => e.populate(() => new Saxon.Hej.functions.FunctionAvailable(), Saxon.Hej.type.BuiltInAtomicType.BOOLEAN, ONE, (NS|LATE)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null).arg(1, Saxon.Hej.type.BuiltInAtomicType.INTEGER, ONE, null));
            register("system-property", 1, e => e.populate(() => new Saxon.Hej.functions.SystemProperty(), Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, (NS|LATE)).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null));
            register("type-available", 1, e => e.populate(() => new Saxon.Hej.functions.TypeAvailable(), Saxon.Hej.type.BuiltInAtomicType.BOOLEAN, ONE, NS).arg(0, Saxon.Hej.type.BuiltInAtomicType.STRING, ONE, null));
        }
        protected internal void addXPathFunctions() {
            importFunctionSet(Saxon.Hej.functions.registry.XPath31FunctionSet.getInstance());
        }
    
    }
}