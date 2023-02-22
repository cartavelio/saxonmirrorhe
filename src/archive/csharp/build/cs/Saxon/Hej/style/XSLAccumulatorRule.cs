using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLAccumulatorRule : Saxon.Hej.style.StyleElement {
        private Saxon.Hej.pattern.Pattern match;
        private bool postDescent;
        private Saxon.Hej.expr.Expression select;
        private bool capture;
        protected internal override void prepareAttributes() {
            string matchAtt = null;
            string newValueAtt = null;
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string value = att.getValue();
                string f = attName.getDisplayName();
                if (attName.getNamespaceUri().isEmpty()) {
                    switch (f) {
                      case "match":
                        matchAtt = value;
                        break ;
                      case "select":
                        newValueAtt = value;
                        select = makeExpression(newValueAtt, att);
                        break ;
                      case "phase":
                        string phaseAtt = Saxon.Hej.value.Whitespace.trim(value);
                        if ("start".Equals(phaseAtt)) {
                            postDescent = false;
                        } else {
                            if ("end".Equals(phaseAtt)) {
                                postDescent = true;
                            } else {
                                postDescent = true;
                                compileError("phase must be 'start' or 'end'", "XTSE0020");
                            }
                        }
                        break ;
                      default:
                        checkUnknownAttribute(attName);
                        break ;
                    }
                } else {
                    if (attName.hasURI(Saxon.Hej.om.NamespaceUri.SAXON)) {
                        if (isExtensionAttributeAllowed(attName.getDisplayName())) {
                            if (attName.getLocalPart().Equals("capture")) {
                                capture = processBooleanAttribute("saxon:capture", value);
                            }
                        }
                    } else {
                        checkUnknownAttribute(attName);
                    }
                }
            }
            if ((matchAtt == null)) {
                reportAbsence("match");
                matchAtt = "non-existent-element";
            }
            match = makePattern(matchAtt, "match");
            if ((capture && !(postDescent))) {
                issueWarning("saxon:capture has no effect on a pre-descent accumulator rule", Saxon.Hej.trans.SaxonErrorCode.SXWN9044);
            }
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            select = typeCheck("select", select);
            match = typeCheck("match", match);
            if (((select != null) && hasChildNodes())) {
                compileError("If the xsl:accumulator-rule element has a select attribute then it must have no children");
            }
        }
        public Saxon.Hej.expr.Expression getNewValueExpression(Saxon.Hej.style.Compilation compilation, Saxon.Hej.style.ComponentDeclaration decl) {
            if ((select == null)) {
                select = compileSequenceConstructor(compilation, decl, true);
            }
            return select;
        }
        public Saxon.Hej.pattern.Pattern getMatch() {
            return match;
        }
        public void setMatch(Saxon.Hej.pattern.Pattern match) {
            this.match = match;
        }
        public bool isPostDescent() {
            return postDescent;
        }
        public void setPostDescent(bool postDescent) {
            this.postDescent = postDescent;
        }
        public bool isCapture() {
            return capture;
        }
        public Saxon.Hej.expr.Expression getSelect() {
            return select;
        }
        public void setSelect(Saxon.Hej.expr.Expression select) {
            this.select = select;
        }
        protected internal override Saxon.Hej.style.SourceBinding hasImplicitBinding(Saxon.Hej.om.StructuredQName name) {
            if ((name.getLocalPart().Equals("value") && name.hasURI(Saxon.Hej.om.NamespaceUri.NULL))) {
                Saxon.Hej.style.SourceBinding sb = new Saxon.Hej.style.SourceBinding(this);
                sb.setVariableQName(new Saxon.Hej.om.StructuredQName("", Saxon.Hej.om.NamespaceUri.NULL, "value"));
                System.Diagnostics.Debug.Assert((((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()) != null));
                sb.setDeclaredType(((Saxon.Hej.style.XSLAccumulator)(((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()))).getResultType());
                sb.setProperty(Saxon.Hej.style.SourceBinding.BindingProperty.IMPLICITLY_DECLARED, true);
                return sb;
            } else {
                return null;
            }
        }
    
    }
}