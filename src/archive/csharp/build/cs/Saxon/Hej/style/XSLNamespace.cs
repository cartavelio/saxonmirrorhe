using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLNamespace : Saxon.Hej.style.XSLLeafNodeConstructor {
        internal Saxon.Hej.expr.Expression name;
        protected internal override void prepareAttributes() {
            name = prepareAttributesNameAndSelect();
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            name = typeCheck("name", name);
            select = typeCheck("select", select);
            int countChildren = 0;
            Saxon.Hej.om.NodeInfo firstChild = null;
            foreach (Saxon.Hej.om.NodeInfo child in children()) {
                if ((child is Saxon.Hej.style.XSLFallback)) {
                    continue ;
                }
                if ((select != null)) {
                    string errorCode = getErrorCodeForSelectPlusContent();
                    compileError((("An "+getDisplayName())+" element with a select attribute must be empty"), errorCode);
                }
                countChildren++;
                if ((firstChild == null)) {
                    firstChild = child;
                } else {
                    break ;
                }
            }
            if ((select == null)) {
                if ((countChildren == 0)) {
                    select = new Saxon.Hej.expr.StringLiteral(Saxon.Hej.value.StringValue.EMPTY_STRING);
                    select.setRetainedStaticContext(makeRetainedStaticContext());
                } else {
                    if ((countChildren == 1)) {
                        if ((firstChild.getNodeKind() == Saxon.Hej.type.Type.TEXT)) {
                            select = new Saxon.Hej.expr.StringLiteral(firstChild.getUnicodeStringValue());
                            select.setRetainedStaticContext(makeRetainedStaticContext());
                        }
                    }
                }
            }
        }
        protected internal override string getErrorCodeForSelectPlusContent() {
            return "XTSE0910";
        }
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            Saxon.Hej.expr.instruct.NamespaceConstructor inst = new Saxon.Hej.expr.instruct.NamespaceConstructor(name);
            compileContent(exec, decl, inst, new Saxon.Hej.expr.StringLiteral(Saxon.Hej.value.StringValue.SINGLE_SPACE));
            return inst;
        }
    
    }
}