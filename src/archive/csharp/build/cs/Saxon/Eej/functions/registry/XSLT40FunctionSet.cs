using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.registry {
    public class XSLT40FunctionSet : Saxon.Hej.functions.registry.XSLT30FunctionSet {
        private static readonly Saxon.Eej.functions.registry.XSLT40FunctionSet THE_INSTANCE = new Saxon.Eej.functions.registry.XSLT40FunctionSet();
        public static new Saxon.Eej.functions.registry.XSLT40FunctionSet getInstance() {
            return THE_INSTANCE;
        }
        protected internal override Saxon.Hej.functions.registry.BuiltInFunctionSet correspondingXPathFunctionSet() {
            return /*134*/Saxon.Eej.functions.registry.XPath40FunctionSet.getInstance();
        }
        private XSLT40FunctionSet()  {
        }
    
    }
}