using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.expr {
    public class StaticQueryContextPE : Saxon.Hej.query.StaticQueryContext {
        private Saxon.Hej.functions.FunctionLibrary extensionFunctionLibrary;
        public StaticQueryContextPE(Saxon.Hej.Configuration config) : base (config) {
        }
        public StaticQueryContextPE(Saxon.Eej.config.ProfessionalConfiguration config, bool initialize) : base (config, initialize) {
        }
        public void setExtensionFunctionLibrary(Saxon.Hej.functions.FunctionLibrary library) {
            this.extensionFunctionLibrary = library;
        }
        public override Saxon.Hej.functions.FunctionLibrary getExtensionFunctionLibrary() {
            return extensionFunctionLibrary;
        }
    
    }
}