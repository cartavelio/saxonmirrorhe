using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public abstract class ExtensionInstruction : Saxon.Hej.style.StyleElement {
        public override bool isInstruction() {
            return true;
        }
        protected internal override bool mayContainFallback() {
            return true;
        }
        public override bool isInXsltNamespace() {
            return false;
        }
    
    }
}