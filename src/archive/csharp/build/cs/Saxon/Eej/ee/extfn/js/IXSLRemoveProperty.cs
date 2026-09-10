using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.extfn.js {
    public class IXSLRemoveProperty : Saxon.Hej.style.StyleElement {
        private Saxon.Hej.expr.Expression name;
        private Saxon.Hej.expr.Expression @object;
        public override bool isInstruction() {
            return true;
        }
        protected internal override bool mayContainSequenceConstructor() {
            return false;
        }
        protected internal override void prepareAttributes() {
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string f = attName.getDisplayName();
                string value = att.getValue();
                switch (f) {
                  case "name":
                    string nameAtt = Saxon.Hej.value.Whitespace.trim(value);
                    name = makeAttributeValueTemplate(nameAtt, att);
                    break ;
                  case "object":
                    string objectAtt = value;
                    @object = makeExpression(objectAtt, att);
                    break ;
                  default:
                    checkUnknownAttribute(attName);
                    break ;
                }
            }
            if ((@object == null)) {
                try {
                    @object = /*134*/Saxon.Eej.ee.extfn.js.IXSLFunctionSet.getInstance().makeFunction("window", 0).makeFunctionCall();
                } catch (Saxon.Hej.trans.XPathException e) {
                    compileError(e);
                }
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
            Saxon.Hej.expr.Expression e = /*134*/Saxon.Eej.ee.extfn.js.IXSLFunctionSet.getInstance().makeFunction("remove-property", 2).makeFunctionCall(name, @object);
            e.setRetainedStaticContext(makeRetainedStaticContext());
            return e;
        }
    
    }
}