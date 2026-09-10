using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema.sdoc {
    public class SaxonPreprocess : Saxon.Eej.ee.schema.sdoc.AnnotationParent {
        private Saxon.Eej.ee.schema.Assertion action = null;
        private Saxon.Eej.ee.schema.Assertion reverseAction = null;
        public SaxonPreprocess()  {
        }
        protected internal override void prepareAttributes() {
            Saxon.Hej.om.AttributeMap atts = attributes();
            string[] allowed = {"action", "id", "reverse", "xpathDefaultNamespace"};
            allowAttributes(atts, allowed);
            string defaultNamespace = atts.getValue(Saxon.Hej.om.NamespaceUri.NULL, "xpathDefaultNamespace");
            if ((defaultNamespace == null)) {
                defaultNamespace = getXSDSchema().getXPathDefaultNamespace();
            }
            Saxon.Hej.om.NamespaceUri defaultNamespaceUri;
            if (defaultNamespace.Equals("##defaultNamespace")) {
                defaultNamespaceUri = getURIForPrefix("", true);
            } else {
                if (defaultNamespace.Equals("##targetNamespace")) {
                    defaultNamespaceUri = getXSDSchema().getTargetNamespace();
                } else {
                    if (defaultNamespace.Equals("##local")) {
                        defaultNamespaceUri = Saxon.Hej.om.NamespaceUri.NULL;
                    } else {
                        defaultNamespaceUri = Saxon.Hej.om.NamespaceUri.of(defaultNamespace);
                    }
                }
            }
            string xpath = atts.getValue(Saxon.Hej.om.NamespaceUri.NULL, "action");
            if ((xpath == null)) {
                missingAttribute("action");
                xpath = ".";
            }
            Saxon.Eej.ee.schema.sdoc.SchemaElement.XPathStaticContext env = new Saxon.Eej.ee.schema.sdoc.SchemaElement.XPathStaticContext(this);
            env.setDefaultElementNamespace(defaultNamespaceUri);
            env.setNamespaceResolver(makeNamespaceContext());
            env.setBaseURI(getBaseURI());
            Saxon.Hej.sxpath.XPathVariable valueVariable = env.declareVariable(Saxon.Hej.om.NamespaceUri.NULL, "value");
            Saxon.Hej.sxpath.XPathExpression expression;
            Saxon.Hej.sxpath.XPathEvaluator xpe = new Saxon.Hej.sxpath.XPathEvaluator(getSchema().getConfiguration());
            try {
                xpe.setStaticContext(env);
                expression = xpe.createExpression(xpath);
            } catch (Saxon.Hej.trans.XPathException err) {
                error(err.Message);
                try {
                    expression = xpe.createExpression("'*error*'");
                } catch (Saxon.Hej.trans.XPathException e2) {
                    throw new Saxon.Ejava.lang.AssertionError(e2.Message);
                }
            }
            action = new Saxon.Eej.ee.schema.Assertion(getSchema().getConfiguration(), expression, valueVariable, null, xpath);
            action.setStaticContext(env);
            xpath = atts.getValue(Saxon.Hej.om.NamespaceUri.NULL, "reverse");
            if ((xpath != null)) {
                try {
                    expression = xpe.createExpression(xpath);
                } catch (Saxon.Hej.trans.XPathException err) {
                    error(err.Message);
                    try {
                        expression = xpe.createExpression("'*error*'");
                    } catch (Saxon.Hej.trans.XPathException e2) {
                        throw new Saxon.Ejava.lang.AssertionError(e2.Message);
                    }
                }
                reverseAction = new Saxon.Eej.ee.schema.Assertion(getSchema().getConfiguration(), expression, valueVariable, null, xpath);
                reverseAction.setStaticContext(env);
            }
            processId();
        }
        public Saxon.Eej.ee.schema.Assertion getAction() {
            return action;
        }
        public Saxon.Eej.ee.schema.Assertion getReverseAction() {
            return reverseAction;
        }
    
    }
}