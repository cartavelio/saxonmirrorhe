using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class ComponentDeclaration {
        private readonly Saxon.Hej.style.StyleElement sourceElement;
        private readonly Saxon.Hej.style.StylesheetModule module;
        public ComponentDeclaration(Saxon.Hej.style.StylesheetModule module, Saxon.Hej.style.StyleElement source)  {
            this.module = module;
            this.sourceElement = source;
        }
        public Saxon.Hej.style.StylesheetModule getModule() {
            return module;
        }
        public Saxon.Hej.style.StyleElement getSourceElement() {
            return sourceElement;
        }
        public int getPrecedence() {
            return module.getPrecedence();
        }
    
    }
}