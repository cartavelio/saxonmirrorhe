using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema.sdoc {
    public class XSDAssert : Saxon.Eej.ee.schema.sdoc.AnnotationParent {
        private Saxon.Eej.ee.schema.Assertion assertion = null;
        public XSDAssert()  {
        }
        protected internal override void prepareAttributes() {
            if ((getXSDSchema().getSchemaCompiler().getLanguageVersion() == Saxon.Hej.Configuration.XSD10)) {
                error("The xs:assert element requires XSD 1.1 to be enabled");
            }
            Saxon.Hej.om.AttributeMap atts = attributes();
            string[] allowed = {"id", "test", "xpathDefaultNamespace"};
            allowAttributes(atts, allowed);
            string defaultNamespace = atts.getValue("", "xpathDefaultNamespace");
            if ((defaultNamespace == null)) {
                defaultNamespace = getXSDSchema().getXPathDefaultNamespace();
            }
            switch (defaultNamespace) {
              case "##defaultNamespace":
                defaultNamespace = getURIForPrefix("", true);
                break ;
              case "##targetNamespace":
                defaultNamespace = getXSDSchema().getTargetNamespace();
                break ;
              case "##local":
                defaultNamespace = "";
                break ;
            }
            bool streamable = false;
            string streamableAtt = atts.getValue(Saxon.Hej.lib.NamespaceConstant.SAXON, "streamable");
            if ((streamableAtt != null)) {
                streamable = parseBooleanAttribute("streamable", streamableAtt);
            }
            string xpath = atts.getValue("", "test");
            if ((xpath == null)) {
                missingAttribute("test");
                xpath = ".";
            }
            Saxon.Hej.Configuration config = getConfiguration();
            Saxon.Eej.ee.schema.sdoc.SchemaElement.XPathStaticContext env = new Saxon.Eej.ee.schema.sdoc.SchemaElement.XPathStaticContext(this);
            env.setDefaultElementNamespace(defaultNamespace);
            env.setNamespaceResolver(makeNamespaceContext());
            env.setBaseURI(getBaseURI());
            env.setRequiredContextItemType(Saxon.Hej.pattern.NodeKindTest.ELEMENT);
            env.setContextItemParentless(true);
            env.setContainingLocation(new Saxon.Hej.tree.AttributeLocation(this, new Saxon.Hej.om.StructuredQName("", "", "test")));
            int xpathVersion = config.getConfigurationProperty(Saxon.Hej.lib.Feature<int>.XPATH_VERSION_FOR_XSD);
            if (getXSDSchema().getAllowedExtensions().Contains("any-xpath-version")) {
                xpathVersion = 31;
            }
            env.setXPathLanguageLevel(xpathVersion);
            Saxon.Hej.sxpath.XPathVariable valueVariable = env.declareVariable("", "value");
            System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.sxpath.XPathVariable> parameters = new System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName, Saxon.Hej.sxpath.XPathVariable>();
            foreach (Saxon.Hej.expr.instruct.GlobalParam param in getXSDSchema().getGlobalParams()) {
                Saxon.Hej.sxpath.XPathVariable var = env.declareVariable(param.getVariableQName());
                Saxon.Hej.om.GroundedValue defaultVal = ((Saxon.Hej.expr.Literal)(param.getBody())).getGroundedValue();
                var.setDefaultValue(defaultVal);
                Saxon.Impl.Helpers.MapUtils.Put(parameters, param.getVariableQName(), var);
            }
            bool isComplex = getLocalPart().Equals("assert");
            Saxon.Hej.sxpath.XPathExpression condition;
            bool returnFailingNodes = (isComplex && !(streamable));
            Saxon.Hej.sxpath.XPathEvaluator xpe = (returnFailingNodes ? new Saxon.Eej.ee.schema.sdoc.XSDAssert.CustomizedXPath(getConfiguration()) : new Saxon.Hej.sxpath.XPathEvaluator(getConfiguration()));
            try {
                xpe.setStaticContext(env);
                condition = xpe.createExpression(xpath);
            } catch (Saxon.Hej.trans.XPathException err) {
                error(err.Message);
                try {
                    condition = xpe.createExpression("false()");
                } catch (Saxon.Hej.trans.XPathException e2) {
                    throw new Saxon.Ejava.lang.AssertionError(e2.Message);
                }
            }
            assertion = new Saxon.Eej.ee.schema.Assertion(getSchema().getConfiguration(), condition, valueVariable, parameters, xpath);
            assertion.setReturnsFailingNodes(returnFailingNodes);
            assertion.setStaticContext(env);
            string message = atts.getValue(Saxon.Hej.lib.NamespaceConstant.SAXON, "message");
            assertion.setMessage(message);
            if (streamable) {
                if (xpath.Contains("$value")) {
                    warning("Assertion is not streamable because it refers to $value");
                } else {
                    Saxon.Eej.ee.trans.ContextItemStaticInfoEE contextInfo = new Saxon.Eej.ee.trans.ContextItemStaticInfoEE(Saxon.Hej.pattern.NodeKindTest.ELEMENT, false, Saxon.Eej.ee.stream.Posture.STRIDING);
                    System.Collections.Generic.IList<string> reasons = new System.Collections.Generic.List<string>();
                    Saxon.Eej.ee.stream.PostureAndSweep ps = /*134*/Saxon.Eej.ee.stream.Streamability.getStreamability(condition.getInternalExpression(), contextInfo, reasons);
                    if ((ps.getSweep() == Saxon.Eej.ee.stream.Sweep.FREE_RANGING)) {
                        Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder("Assertion {").append(xpath).append("} is not streamable. ");
                        foreach (string r in reasons) {
                            sb.append(r).append(". ");
                        }
                        warning(sb.ToString());
                    } else {
                        assertion.setSweep(ps.getSweep());
                    }
                }
            }
            processId();
        }
        public Saxon.Eej.ee.schema.Assertion getAssertion() {
            return assertion;
        }
        public class CustomizedXPath : Saxon.Hej.sxpath.XPathEvaluator {
            public CustomizedXPath(Saxon.Hej.Configuration config) : base (config) {
            }
            protected internal override Saxon.Hej.expr.Expression postProcess(Saxon.Hej.expr.Expression exp, Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo cit) {
                Saxon.Hej.expr.Expression core = exp;
                Saxon.Hej.expr.Operand lastOp = null;
                while ((core is Saxon.Hej.expr.LetExpression)) {
                    lastOp = ((Saxon.Hej.expr.LetExpression)(core)).getActionOp();
                    core = lastOp.getChildExpression();
                }
                if (((core is Saxon.Hej.expr.QuantifiedExpression) && (((Saxon.Hej.expr.QuantifiedExpression)(core)).getOperator() == Saxon.Hej.expr.parser.Token.EVERY))) {
                    Saxon.Hej.expr.QuantifiedExpression every = (Saxon.Hej.expr.QuantifiedExpression)(core);
                    if ((every.getSequence().getItemType() is Saxon.Hej.pattern.NodeTest)) {
                        Saxon.Hej.expr.Operand sequenceOp = every.getSequenceOp();
                        Saxon.Hej.expr.Operand satisfiesOp = every.getActionOp();
                        Saxon.Hej.expr.Expression satisfies = satisfiesOp.getChildExpression();
                        Saxon.Hej.expr.Expression[] conditions = new Saxon.Hej.expr.Expression[]{satisfies, Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.BooleanValue.TRUE)};
                        Saxon.Hej.expr.Expression[] actions = new Saxon.Hej.expr.Expression[]{Saxon.Hej.expr.Literal.makeEmptySequence(), new Saxon.Hej.expr.LocalVariableReference(every)};
                        Saxon.Hej.expr.instruct.Choose choose = new Saxon.Hej.expr.instruct.Choose(conditions, actions);
                        Saxon.Hej.expr.ForExpression forEx = new Saxon.Hej.expr.ForExpression();
                        forEx.setSequence(sequenceOp.getChildExpression());
                        forEx.setVariableQName(every.getVariableQName());
                        forEx.setAction(choose);
                        Saxon.Hej.expr.parser.ExpressionTool.rebindVariableReferences(choose, every, forEx);
                        if ((lastOp != null)) {
                            lastOp.setChildExpression(forEx);
                            return exp.optimize(visitor, cit);
                        } else {
                            return forEx.optimize(visitor, cit);
                        }
                    }
                } else {
                    if (core.isCallOn(typeof(Saxon.Hej.functions.Empty))) {
                        Saxon.Hej.expr.Operand op = ((Saxon.Hej.expr.SystemFunctionCall)(core)).getOperanda().getOperand(0);
                        if ((op.getChildExpression().getItemType() is Saxon.Hej.pattern.NodeTest)) {
                            if ((lastOp != null)) {
                                lastOp.setChildExpression(op.getChildExpression());
                                return exp;
                            } else {
                                return op.getChildExpression();
                            }
                        }
                    }
                }
                Saxon.Hej.expr.Expression notExp = Saxon.Hej.functions.SystemFunction.makeCall("not", exp.getRetainedStaticContext(), exp);
                return new Saxon.Hej.expr.FilterExpression(new Saxon.Hej.expr.ContextItemExpression(), notExp);
            }
        
        }
    
    }
}