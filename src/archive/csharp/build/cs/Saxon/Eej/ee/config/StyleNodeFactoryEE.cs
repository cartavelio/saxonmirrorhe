using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.config {
    public class StyleNodeFactoryEE : Saxon.Eej.config.pe.StyleNodeFactoryPE {
        public StyleNodeFactoryEE(Saxon.Hej.Configuration config, Saxon.Hej.style.Compilation compilation) : base (config, compilation) {
        }
        public override Saxon.Eej.ee.stream.AccumulatorRegistryEE makeAccumulatorManager() {
            return new Saxon.Eej.ee.stream.AccumulatorRegistryEE();
        }
        public override Saxon.Hej.style.PrincipalStylesheetModule newPrincipalModule(Saxon.Hej.style.XSLPackage node) {
            return new Saxon.Eej.ee.trans.PrincipalStylesheetModuleEE(node);
        }
    
    }
}