using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema {
    public class Assertion : Saxon.Eej.ee.schema.SchemaStructure, Saxon.Hej.type.SchemaComponent, Saxon.Eej.ee.schema.SerializableSchemaComponent {
        private Saxon.Hej.sxpath.XPathExpression condition;
        private Saxon.Hej.sxpath.XPathVariable valueVariable;
        private System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.sxpath.XPathVariable> declaredParams;
        private string conditionText;
        private string message;
        private Saxon.Hej.sxpath.XPathStaticContext staticContext;
        private Saxon.Eej.ee.stream.Sweep sweep = Saxon.Eej.ee.stream.Sweep.FREE_RANGING;
        private bool returnsFailingNodes;
        public Assertion()  {
        }
        public Assertion(Saxon.Eej.config.EnterpriseConfiguration config, Saxon.Hej.sxpath.XPathExpression condition, Saxon.Hej.sxpath.XPathVariable valueVariable, System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.sxpath.XPathVariable> @params, string text)  {
            setConfiguration(config);
            this.condition = condition;
            this.valueVariable = valueVariable;
            this.declaredParams = @params;
            this.conditionText = text;
        }
        public Saxon.Hej.sxpath.XPathExpression getCondition() {
            return condition;
        }
        public Saxon.Hej.sxpath.XPathVariable getValueVariable() {
            return valueVariable;
        }
        public string getConditionText() {
            return conditionText;
        }
        public void setMessage(string message) {
            this.message = message;
        }
        public string getMessage() {
            return message;
        }
        public void setSweep(Saxon.Eej.ee.stream.Sweep sweep) {
            this.sweep = sweep;
        }
        public Saxon.Eej.ee.stream.Sweep getSweep() {
            return sweep;
        }
        public void setReturnsFailingNodes(bool failingNodes) {
            this.returnsFailingNodes = failingNodes;
        }
        public void setStaticContext(Saxon.Hej.sxpath.XPathStaticContext staticContext) {
            this.staticContext = staticContext;
        }
        public Saxon.Hej.sxpath.XPathStaticContext getStaticContext() {
            return staticContext;
        }
        public bool testSimple(Saxon.Hej.om.AtomicSequence value, Saxon.Hej.lib.ConversionRules rules) {
            Saxon.Hej.Configuration config;
            Saxon.Hej.Controller controller;
            Saxon.Hej.type.ValidationParams actualParams;
            Saxon.Hej.sxpath.XPathDynamicContext context;
            if ((rules is Saxon.Eej.ee.validate.ValidationContext)) {
                try {
                    controller = ((Saxon.Eej.ee.validate.ValidationContext)(rules)).getController();
                    config = controller.getConfiguration();
                    actualParams = ((Saxon.Eej.ee.validate.ValidationContext)(rules)).getValidationParams();
                    context = condition.createDynamicContext(controller, null);
                    Saxon.Hej.type.ValidationParams.setValidationParams(declaredParams, actualParams, context);
                } catch (Saxon.Hej.trans.XPathException err) {
                    throw new Saxon.Ejava.lang.IllegalStateException(err.Message);
                }
            } else {
                context = condition.createDynamicContext();
                controller = context.getXPathContextObject().getController();
                config = staticContext.getConfiguration();
            }
            setDocumentsAndCollections(config, context);
            try {
                if ((getValueVariable() != null)) {
                    context.setVariable(getValueVariable(), ((value == null) ? Saxon.Hej.value.EmptySequence.getInstance() : value));
                }
                return Saxon.Hej.expr.parser.ExpressionTool.effectiveBooleanValue(condition.iterate(context));
            } catch (Saxon.Hej.trans.XPathException err) {
                context.getErrorReporter().report(new Saxon.Hej.trans.XmlProcessingException(err).asWarning());
                return false;
            }
        }
        public System.Collections.Generic.IList<Saxon.Hej.om.NodeInfo> testComplex(Saxon.Hej.om.NodeInfo contextItem, Saxon.Hej.om.AtomicSequence value, Saxon.Eej.ee.validate.ValidationContext rules) {
            Saxon.Hej.Configuration config;
            Saxon.Hej.Controller controller;
            Saxon.Hej.type.ValidationParams actualParams;
            Saxon.Hej.sxpath.XPathDynamicContext context;
            try {
                controller = rules.getController();
                config = controller.getConfiguration();
                actualParams = rules.getValidationParams();
                context = condition.createDynamicContext(controller, contextItem);
                Saxon.Hej.type.ValidationParams.setValidationParams(declaredParams, actualParams, context);
                if ((getValueVariable() != null)) {
                    context.setVariable(getValueVariable(), ((value == null) ? Saxon.Hej.value.EmptySequence.getInstance() : value));
                }
            } catch (Saxon.Hej.trans.XPathException err) {
                throw new Saxon.Ejava.lang.IllegalStateException(err.Message);
            }
            setDocumentsAndCollections(config, context);
            System.Collections.Generic.IList<Saxon.Hej.om.NodeInfo> result = new System.Collections.Generic.List<Saxon.Hej.om.NodeInfo>();
            try {
                if (returnsFailingNodes) {
                    Saxon.Hej.om.SequenceTool.supply(condition.iterate(context), (Saxon.Hej.om.ItemConsumer<Saxon.Hej.om.Item>)((item => result.Add((Saxon.Hej.om.NodeInfo)(item)))));
                    return result;
                } else {
                    bool ok = condition.effectiveBooleanValue(context);
                    if (!(ok)) {
                        result.Add(contextItem);
                    }
                }
            } catch (Saxon.Hej.trans.UncheckedXPathException err) {
                context.getErrorReporter().report(new Saxon.Hej.trans.XmlProcessingException(err.getXPathException()).asWarning());
                result.Add(contextItem);
            } catch (Saxon.Hej.trans.XPathException err) {
                context.getErrorReporter().report(new Saxon.Hej.trans.XmlProcessingException(err).asWarning());
                result.Add(contextItem);
            }
            return result;
        }
        public static void setDocumentsAndCollections(Saxon.Hej.Configuration config, Saxon.Hej.sxpath.XPathDynamicContext context) {
            context.setResourceResolver(new Saxon.Hej.lib.ResourceResolverDelegate(request => {
                throw new Saxon.Hej.trans.UncheckedXPathException(new Saxon.Hej.trans.XPathException("Use of doc() function within an assertion is not allowed", "FODC0002"));
            } ));
            context.getXPathContextObject().getController().setDefaultCollection(Saxon.Hej.functions.CollectionFn.EMPTY_COLLECTION_URI);
            context.setCollectionFinder((context1, collectionURI) => {
                if (collectionURI.Equals(Saxon.Hej.functions.CollectionFn.EMPTY_COLLECTION_URI)) {
                    return Saxon.Hej.functions.CollectionFn.EMPTY_COLLECTION;
                } else {
                    throw new Saxon.Hej.trans.XPathException("No collections are available when processing assertions");
                }
            } );
        }
        public override int GetHashCode() {
            return condition.getInternalExpression().GetHashCode();
        }
        public override bool Equals(System.Object other) {
            return ((other is Saxon.Eej.ee.schema.Assertion) && condition.getInternalExpression().isEqual(((Saxon.Eej.ee.schema.Assertion)(other)).condition.getInternalExpression()));
        }
        public void serialize(Saxon.Eej.ee.schema.SchemaModelSerializer serializer) {
            serializer.startElement("assertion");
            serializer.emitNamespaceContext(staticContext.getNamespaceResolver());
            serializer.emitAttribute("test", getConditionText());
            serializer.emitAttribute("defaultNamespace", staticContext.getDefaultElementNamespace().ToString());
            serializer.emitAttribute("xml:base", staticContext.getStaticBaseURI());
            if ((sweep != Saxon.Eej.ee.stream.Sweep.FREE_RANGING)) {
                serializer.emitAttribute("sweep", sweep.ToString());
            }
            serializer.endElement();
        }
        public override Saxon.Hej.om.FunctionItem getComponentAsFunction() {
            Saxon.Eej.ee.schema.Assertion thisAssertion = this;
            Saxon.Hej.expr.CallableDelegate callable = new Saxon.Hej.expr.CallableDelegate((context, arguments) => {
                string key = arguments[0].head().getStringValue();
                switch (key) {
                  case "class":
                    return Saxon.Hej.value.StringValue.bmp("Assertion");
                  case "implementation":
                    return new Saxon.Hej.value.ObjectValue<Saxon.Eej.ee.schema.Assertion>(thisAssertion);
                  case "test":
                    return /*134*/Saxon.Eej.ee.schema.SchemaStructure.makeXPathExpressionPropertyRecord(conditionText, staticContext.getNamespaceResolver(), staticContext.getStaticBaseURI());
                  default:
                    return Saxon.Hej.value.EmptySequence.getInstance();
                }
            } );
            return new Saxon.Hej.functions.CallableFunction(1, callable, Saxon.Hej.type.SpecificFunctionType.COMPONENT_FUNCTION_TYPE);
        }
    
    }
}