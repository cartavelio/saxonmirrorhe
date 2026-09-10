using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.trans {
    public class ModePE : Saxon.Hej.trans.SimpleMode {
        public ModePE(Saxon.Hej.om.StructuredQName modeName) : base (modeName) {
        }
        public override void prepareStreamability() {
            if (isDeclaredStreamable()) {
                computeStreamability();
                invertStreamableTemplates();
            }
        }
    
    }
}