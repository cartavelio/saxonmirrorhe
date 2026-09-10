using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema.sdoc {
    public class XSDFieldOrSelector : Saxon.Eej.ee.schema.sdoc.AnnotationParent {
        private Saxon.Eej.ee.schema.FieldOrSelector fieldOrSelector = null;
        internal XSDFieldOrSelector()  {
        }
        protected internal override void prepareAttributes() {
            bool isField = (getLocalPart().Equals("field"));
            Saxon.Hej.om.AttributeMap atts = attributes();
            string[] allowed = {"id", "xpath", "xpathDefaultNamespace"};
            allowAttributes(atts, allowed);
            string defaultNS = atts.getValue("", "xpathDefaultNamespace");
            if ((defaultNS == null)) {
                defaultNS = getXSDSchema().getXPathDefaultNamespace();
            }
            if ((defaultNS != null)) {
                switch (defaultNS) {
                  case "##defaultNamespace":
                    defaultNS = getURIForPrefix("", true);
                    break ;
                  case "##targetNamespace":
                    defaultNS = getXSDSchema().getTargetNamespace();
                    break ;
                  case "##local":
                    defaultNS = "";
                    break ;
                }
            } else {
                defaultNS = "";
            }
            string xpath = atts.getValue("", "xpath");
            if ((xpath == null)) {
                missingAttribute("xpath");
                xpath = ".";
            }
            Saxon.Eej.ee.schema.sdoc.SchemaElement.XPathStaticContext env = new Saxon.Eej.ee.schema.sdoc.SchemaElement.XPathStaticContext(this);
            env.setDefaultElementNamespace(defaultNS);
            Saxon.Hej.pattern.Pattern selector;
            Saxon.Hej.expr.Expression exp;
            try {
                exp = new Saxon.Hej.expr.parser.XPathParser().parse(xpath, 0, Saxon.Hej.expr.parser.Token.EOF, env);
                bool allowExtensions = getXSDSchema().getAllowedExtensions().Contains("id-xpath-syntax");
                if (allowExtensions) {
                    System.Collections.Generic.IList<string> reasonsForFailure = new System.Collections.Generic.List<string>();
                    selector = /*134*/Saxon.Eej.ee.stream.StreamingPatternMaker.makeStreamingPattern(exp, getConfiguration(), reasonsForFailure);
                    if (!(reasonsForFailure.Count==0)) {
                        throw new Saxon.Hej.trans.XPathException(reasonsForFailure[0]);
                    }
                } else {
                    Saxon.Eej.ee.schema.sdoc.SelectionParser selectionParser = new Saxon.Eej.ee.schema.sdoc.SelectionParser();
                    selector = selectionParser.parseSelector(xpath, env, isField);
                    exp = selectionParser.getOptimizedExpression();
                }
                selector.setLocation(env.getContainingLocation());
                selector.setRetainedStaticContext(env.makeRetainedStaticContext());
            } catch (Saxon.Hej.trans.XPathException err) {
                error(err.Message);
                selector = new Saxon.Hej.pattern.NodeTestPattern(Saxon.Hej.type.ErrorType.getInstance());
                exp = Saxon.Hej.expr.Literal.makeEmptySequence();
            }
            processId();
            if (isField) {
                string order = Saxon.Hej.value.Whitespace.trim(getAttributeValue(Saxon.Hej.lib.NamespaceConstant.SAXON, "order"));
                bool ascending = true;
                if ((order != null)) {
                    switch (order) {
                      case "ascending":
                        break ;
                      case "descending":
                        ascending = false;
                        break ;
                      default:
                        error("xs:field/@saxon:order must be 'ascending' or 'descending'");
                        break ;
                    }
                }
                fieldOrSelector = new Saxon.Eej.ee.schema.IdentityField(xpath);
                fieldOrSelector.setLocator(this);
                fieldOrSelector.setSelection(selector);
                fieldOrSelector.setSelectExpression(exp);
                fieldOrSelector.setNamespaceContext(makeNamespaceContext());
                fieldOrSelector.setXPathDefaultNamespace(defaultNS);
                ((Saxon.Eej.ee.schema.IdentityField)(fieldOrSelector)).setAscending(ascending);
            } else {
                fieldOrSelector = new Saxon.Eej.ee.schema.IdentitySelector(xpath);
                fieldOrSelector.setLocator(this);
                fieldOrSelector.setSelection(selector);
                fieldOrSelector.setSelectExpression(exp);
                fieldOrSelector.setNamespaceContext(makeNamespaceContext());
                fieldOrSelector.setXPathDefaultNamespace(defaultNS);
            }
        }
        internal Saxon.Eej.ee.schema.IdentityField getIdentityField() {
            if ((fieldOrSelector is Saxon.Eej.ee.schema.IdentityField)) {
                return (Saxon.Eej.ee.schema.IdentityField)(fieldOrSelector);
            } else {
                return null;
            }
        }
        internal Saxon.Eej.ee.schema.IdentitySelector getIdentitySelector() {
            if ((fieldOrSelector is Saxon.Eej.ee.schema.IdentitySelector)) {
                return (Saxon.Eej.ee.schema.IdentitySelector)(fieldOrSelector);
            } else {
                return null;
            }
        }
    
    }
}