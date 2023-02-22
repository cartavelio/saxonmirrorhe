using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.extfn.js {
    public class IXSLSetProperty : Saxon.Hej.style.StyleElement {
        private Saxon.Hej.expr.Expression name;
        private Saxon.Hej.expr.Expression select;
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
                  case "select":
                    string selectAtt = value;
                    select = makeExpression(selectAtt, att);
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
            if ((select == null)) {
                reportAbsence("select");
            }
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            name = typeCheck("name", name);
            select = typeCheck("select", select);
            @object = typeCheck("object", @object);
        }
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            Saxon.Hej.expr.Expression e = /*134*/Saxon.Eej.ee.extfn.js.IXSLFunctionSet.getInstance().makeFunction("set-property", 3).makeFunctionCall(name, select, @object);
            e.setRetainedStaticContext(makeRetainedStaticContext());
            return e;
        }
    
    }
}