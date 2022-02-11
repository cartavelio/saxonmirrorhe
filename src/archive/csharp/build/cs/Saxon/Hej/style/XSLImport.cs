using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLImport : Saxon.Hej.style.XSLGeneralIncorporate {
        public override bool isImport() {
            return true;
        }
    
    }
}