using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLContextItem : Saxon.Hej.style.StyleElement {
        private Saxon.Hej.type.ItemType requiredType = Saxon.Hej.type.AnyItemType.getInstance();
        private bool mayBeOmitted = true;
        private bool absentFocus = false;
        protected internal override void prepareAttributes() {
            string asAtt = null;
            string useAtt = null;
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string f = attName.getDisplayName();
                string value = att.getValue();
                switch (f) {
                  case "as":
                    asAtt = Saxon.Hej.value.Whitespace.trim(value);
                    break ;
                  case "use":
                    useAtt = Saxon.Hej.value.Whitespace.trim(value);
                    break ;
                  default:
                    checkUnknownAttribute(attName);
                    break ;
                }
            }
            if ((asAtt != null)) {
                Saxon.Hej.value.SequenceType st;
                try {
                    st = makeSequenceType(asAtt);
                } catch (Saxon.Hej.trans.XPathException e) {
                    st = Saxon.Hej.value.SequenceType.SINGLE_ITEM;
                    compileErrorInAttribute(e.Message, e.getErrorCodeLocalPart(), "as");
                }
                if ((st.getCardinality() != Saxon.Hej.expr.StaticProperty.EXACTLY_ONE)) {
                    compileError("The xsl:context-item/@use attribute must be an item type (no occurrence indicator allowed)", "XTSE0020");
                    return ;
                }
                requiredType = st.getPrimaryType();
            }
            if ((useAtt != null)) {
                switch (useAtt) {
                  case "required":
                    mayBeOmitted = false;
                    break ;
                  case "optional":
                    break ;
                  case "absent":
                    absentFocus = true;
                    break ;
                  default:
                    invalidAttribute("use", "required|optional|absent");
                    break ;
                }
            }
            if (((asAtt != null) && absentFocus)) {
                compileError("The 'as' attribute must be omitted when use='absent' is specified", "XTSE3089");
            }
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            if (!(((((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()) is Saxon.Hej.style.XSLTemplate)))) {
                compileError("xsl:context-item can appear only as a child of xsl:template");
                return ;
            }
            if ((mayBeOmitted && (((Saxon.Hej.style.XSLTemplate)(((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()))).getTemplateName() == null))) {
                compileError("xsl:context-item appearing in an xsl:template declaration with no name attribute must specify use=required", "XTSE0020");
            }
            ((Saxon.Hej.style.XSLTemplate)(((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()))).setContextItemRequirements(requiredType, mayBeOmitted, absentFocus);
            Saxon.Hej.om.SequenceTool.supply(iterateAxis(Saxon.Hej.om.AxisInfo.PRECEDING_SIBLING), (Saxon.Hej.om.ItemConsumer<Saxon.Hej.om.Item>)((prec => {
                if (((((Saxon.Hej.om.NodeInfo)(prec)).getNodeKind() != Saxon.Hej.type.Type.TEXT) || !(Saxon.Hej.value.Whitespace.isAllWhite(prec.getUnicodeStringValue())))) {
                    compileError("xsl:context-item must be the first child of xsl:template");
                }
            } )));
        }
        public Saxon.Hej.type.ItemType getRequiredContextItemType() {
            return requiredType;
        }
        public bool isMayBeOmitted() {
            return mayBeOmitted;
        }
        public bool isAbsentFocus() {
            return absentFocus;
        }
    
    }
}