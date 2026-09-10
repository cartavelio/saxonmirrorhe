using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.extfn.js {
    public class IXSLRemoveAttribute : Saxon.Hej.style.StyleElement {
        private Saxon.Hej.expr.Expression name;
        private Saxon.Hej.expr.Expression @object;
        public override bool isInstruction() {
            return true;
        }
        protected internal override bool mayContainSequenceConstructor() {
            return false;
        }
        protected internal override void prepareAttributes() {
            Saxon.Hej.om.AttributeMap atts = attributes();
            foreach (Saxon.Hej.om.AttributeInfo att in atts) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string f = attName.getDisplayName();
                if (f.Equals("name")) {
                    string nameAtt = Saxon.Hej.value.Whitespace.trim(att.getValue());
                    name = makeAttributeValueTemplate(nameAtt, att);
                } else {
                    if (f.Equals("object")) {
                        string objectAtt = att.getValue();
                        @object = makeExpression(objectAtt, att);
                    } else {
                        checkUnknownAttribute(attName);
                    }
                }
            }
            if ((@object == null)) {
                @object = makeExpression(".", null);
            }
            if ((name == null)) {
                reportAbsence("name");
            }
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            name = typeCheck("name", name);
            @object = typeCheck("object", @object);
        }
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            Saxon.Hej.expr.Expression e = /*134*/Saxon.Eej.ee.extfn.js.IXSLFunctionSet.getInstance().makeFunction("remove-attribute", 2).makeFunctionCall(name, @object);
            e.setRetainedStaticContext(makeRetainedStaticContext());
            return e;
        }
    
    }
}