using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLPerformSort : Saxon.Hej.style.StyleElement {
        internal Saxon.Hej.expr.Expression select = null;
        public override bool isInstruction() {
            return true;
        }
        protected internal override bool mayContainSequenceConstructor() {
            return true;
        }
        protected internal override bool isPermittedChild(Saxon.Hej.style.StyleElement child) {
            return ((child is Saxon.Hej.style.XSLSort));
        }
        protected internal override void prepareAttributes() {
            string selectAtt = null;
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string value = att.getValue();
                string f = attName.getDisplayName();
                if (f.Equals("select")) {
                    selectAtt = value;
                    select = makeExpression(selectAtt, att);
                } else {
                    checkUnknownAttribute(attName);
                }
            }
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            checkSortComesFirst(true);
            if ((select != null)) {
                foreach (Saxon.Hej.om.NodeInfo child in children()) {
                    if (((child is Saxon.Hej.style.XSLSort) || (child is Saxon.Hej.style.XSLFallback))) {
                    } else {
                        if (((child.getNodeKind() == Saxon.Hej.type.Type.TEXT) && !(Saxon.Hej.value.Whitespace.isAllWhite(child.getUnicodeStringValue())))) {
                            compileError("Within xsl:perform-sort, significant text must not appear if there is a select attribute", "XTSE1040");
                        } else {
                            ((Saxon.Hej.style.StyleElement)(child)).compileError("Within xsl:perform-sort, child instructions are not allowed if there is a select attribute", "XTSE1040");
                        }
                    }
                }
            }
            select = typeCheck("select", select);
        }
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation compilation, Saxon.Hej.style.ComponentDeclaration decl) {
            Saxon.Hej.expr.sort.SortKeyDefinitionList sortKeys = makeSortKeys(compilation, decl);
            if ((select != null)) {
                return new Saxon.Hej.expr.sort.SortExpression(select, sortKeys);
            } else {
                Saxon.Hej.expr.Expression body = compileSequenceConstructor(compilation, decl, true);
                if ((body == null)) {
                    body = Saxon.Hej.expr.Literal.makeEmptySequence();
                }
                try {
                    return new Saxon.Hej.expr.sort.SortExpression(body.simplify(), sortKeys);
                } catch (Saxon.Hej.trans.XPathException e) {
                    compileError(e);
                    return null;
                }
            }
        }
    
    }
}