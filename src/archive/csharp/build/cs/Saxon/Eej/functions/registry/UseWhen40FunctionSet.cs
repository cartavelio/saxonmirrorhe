using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.registry {
    public class UseWhen40FunctionSet : Saxon.Hej.functions.registry.UseWhen30FunctionSet {
        private static readonly Saxon.Eej.functions.registry.UseWhen40FunctionSet THE_INSTANCE_31 = new Saxon.Eej.functions.registry.UseWhen40FunctionSet(31);
        private static readonly Saxon.Eej.functions.registry.UseWhen40FunctionSet THE_INSTANCE_40 = new Saxon.Eej.functions.registry.UseWhen40FunctionSet(40);
        public static new Saxon.Hej.functions.registry.UseWhen30FunctionSet getInstance(int version) {
            return ((version == 40) ? THE_INSTANCE_40 : THE_INSTANCE_31);
        }
        protected internal UseWhen40FunctionSet(int version) : base (version) {
        }
        protected internal override void addXPathFunctions(int version) {
            importFunctionSet(((version == 40) ? /*134*/Saxon.Eej.functions.registry.XPath40FunctionSet.getInstance() : Saxon.Hej.functions.registry.XPath31FunctionSet.getInstance()));
        }
    
    }
}