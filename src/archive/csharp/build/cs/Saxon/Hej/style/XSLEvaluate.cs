using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLEvaluate : Saxon.Hej.style.StyleElement {
        internal Saxon.Hej.expr.Expression xpath = null;
        internal Saxon.Hej.value.SequenceType requiredType = Saxon.Hej.value.SequenceType.ANY_SEQUENCE;
        internal Saxon.Hej.expr.Expression namespaceContext = null;
        internal Saxon.Hej.expr.Expression contextItem = null;
        internal Saxon.Hej.expr.Expression baseUri = null;
        internal Saxon.Hej.expr.Expression schemaAware = null;
        internal Saxon.Hej.expr.Expression withParams = null;
        internal Saxon.Hej.expr.Expression options = null;
        internal bool hasFallbackChildren;
        public override bool isInstruction() {
            return true;
        }
        protected internal override bool isPermittedChild(Saxon.Hej.style.StyleElement child) {
            return (child is Saxon.Hej.style.XSLLocalParam);
        }
        protected internal Saxon.Hej.type.ItemType getReturnedItemType() {
            return Saxon.Hej.type.AnyItemType.getInstance();
        }
        protected internal override bool mayContainSequenceConstructor() {
            return false;
        }
        protected internal override void prepareAttributes() {
            Saxon.Hej.om.AttributeMap atts = attributes();
            string xpathAtt = null;
            string asAtt = null;
            string contextItemAtt = null;
            string baseUriAtt = null;
            string namespaceContextAtt = null;
            string schemaAwareAtt = null;
            string withParamsAtt = null;
            foreach (Saxon.Hej.om.AttributeInfo att in atts) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string f = attName.getDisplayName();
                switch (f) {
                  case "xpath":
                    xpathAtt = att.getValue();
                    xpath = makeExpression(xpathAtt, att);
                    break ;
                  case "as":
                    asAtt = att.getValue();
                    break ;
                  case "context-item":
                    contextItemAtt = att.getValue();
                    contextItem = makeExpression(contextItemAtt, att);
                    break ;
                  case "base-uri":
                    baseUriAtt = att.getValue();
                    baseUri = makeAttributeValueTemplate(baseUriAtt, att);
                    break ;
                  case "namespace-context":
                    namespaceContextAtt = att.getValue();
                    namespaceContext = makeExpression(namespaceContextAtt, att);
                    break ;
                  case "schema-aware":
                    schemaAwareAtt = Saxon.Hej.value.Whitespace.trim(att.getValue());
                    schemaAware = makeAttributeValueTemplate(schemaAwareAtt, att);
                    break ;
                  case "with-params":
                    withParamsAtt = att.getValue();
                    withParams = makeExpression(withParamsAtt, att);
                    break ;
                  default:
                    if ((attName.getLocalPart().Equals("options") && attName.getNamespaceUri().Equals(Saxon.Hej.om.NamespaceUri.SAXON))) {
                        if (isExtensionAttributeAllowed(attName.getDisplayName())) {
                            options = makeExpression(att.getValue(), att);
                        }
                    } else {
                        checkUnknownAttribute(attName);
                    }
                    break ;
                }
            }
            if ((xpathAtt == null)) {
                reportAbsence("xpath");
            }
            if ((asAtt != null)) {
                try {
                    requiredType = makeSequenceType(asAtt);
                } catch (Saxon.Hej.trans.XPathException e) {
                    compileErrorInAttribute(e.Message, e.getErrorCodeLocalPart(), "as");
                }
            }
            if ((contextItemAtt == null)) {
                contextItem = Saxon.Hej.expr.Literal.makeEmptySequence();
            }
            if ((schemaAwareAtt == null)) {
                schemaAware = new Saxon.Hej.expr.StringLiteral("no");
            } else {
                if ((schemaAware is Saxon.Hej.expr.StringLiteral)) {
                    checkAttributeValue("schema-aware", schemaAwareAtt, true, Saxon.Hej.style.StyleElement.YES_NO);
                }
            }
            if ((withParamsAtt == null)) {
                withParamsAtt = "map{}";
                withParams = makeExpression(withParamsAtt, null);
            }
            if ((options == null)) {
                options = makeExpression("map{}", null);
            }
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            getContainingPackage().setRetainUnusedFunctions();
            if ((xpath == null)) {
                xpath = new Saxon.Hej.expr.StringLiteral("''");
            }
            xpath = typeCheck("xpath", xpath);
            baseUri = typeCheck("base-uri", baseUri);
            contextItem = typeCheck("context-item", contextItem);
            namespaceContext = typeCheck("namespace-context", namespaceContext);
            schemaAware = typeCheck("schema-aware", schemaAware);
            withParams = typeCheck("with-params", withParams);
            options = typeCheck("options", options);
            foreach (Saxon.Hej.om.NodeInfo child in children()) {
                if ((child is Saxon.Hej.style.XSLWithParam)) {
                } else {
                    if ((child is Saxon.Hej.style.XSLFallback)) {
                        hasFallbackChildren = true;
                    } else {
                        if ((child.getNodeKind() == Saxon.Hej.type.Type.TEXT)) {
                            if (!(Saxon.Hej.value.Whitespace.isAllWhite(child.getUnicodeStringValue()))) {
                                compileError("No character data is allowed within xsl:evaluate", "XTSE0010");
                            }
                        } else {
                            compileError((("Child element "+Saxon.Hej.trans.Err.wrap(child.getDisplayName(), Saxon.Hej.trans.Err.ELEMENT))+" is not allowed as a child of xsl:evaluate"), "XTSE0010");
                        }
                    }
                }
            }
            try {
                Saxon.Hej.expr.parser.ExpressionVisitor visitor = makeExpressionVisitor();
                Saxon.Hej.expr.parser.TypeChecker tc = getConfiguration().getTypeChecker(false);
                Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> role = () => new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.INSTRUCTION, "xsl:evaluate/xpath", 0);
                xpath = tc.staticTypeCheck(xpath, Saxon.Hej.value.SequenceType.SINGLE_STRING, role, visitor);
                role = () => new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.INSTRUCTION, "xsl:evaluate/context-item", 0, "XTTE3210");
                contextItem = tc.staticTypeCheck(contextItem, Saxon.Hej.value.SequenceType.OPTIONAL_ITEM, role, visitor);
                role = () => new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.INSTRUCTION, "xsl:evaluate/namespace-context", 0, "XTTE3170");
                if ((namespaceContext != null)) {
                    namespaceContext = tc.staticTypeCheck(namespaceContext, Saxon.Hej.value.SequenceType.SINGLE_NODE, role, visitor);
                }
                role = () => new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.INSTRUCTION, "xsl:evaluate/with-params", 0, "XTTE3170");
                withParams = tc.staticTypeCheck(withParams, Saxon.Hej.value.SequenceType.makeSequenceType(Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE, Saxon.Hej.expr.StaticProperty.EXACTLY_ONE), role, visitor);
                role = () => new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.INSTRUCTION, "xsl:evaluate/saxon:options", 0);
                options = tc.staticTypeCheck(options, Saxon.Hej.value.SequenceType.makeSequenceType(Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE, Saxon.Hej.expr.StaticProperty.EXACTLY_ONE), role, visitor);
            } catch (Saxon.Hej.trans.XPathException err) {
                compileError(err);
            }
        }
        public Saxon.Hej.expr.Expression getTargetExpression() {
            return xpath;
        }
        public Saxon.Hej.expr.Expression getContextItemExpression() {
            return contextItem;
        }
        public Saxon.Hej.expr.Expression getBaseUriExpression() {
            return baseUri;
        }
        public Saxon.Hej.expr.Expression getNamespaceContextExpression() {
            return namespaceContext;
        }
        public Saxon.Hej.expr.Expression getSchemaAwareExpression() {
            return schemaAware;
        }
        public Saxon.Hej.expr.Expression getWithParamsExpression() {
            return withParams;
        }
        public Saxon.Hej.expr.Expression getOptionsExpression() {
            return options;
        }
        public Saxon.Hej.value.SequenceType getRequiredType() {
            return requiredType;
        }
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            if (getConfiguration().getBooleanProperty(Saxon.Hej.lib.Feature<bool>.DISABLE_XSL_EVALUATE)) {
                validationError = new Saxon.Hej.trans.XmlProcessingIncident("xsl:evaluate is not available in this configuration", "XTDE3175");
                return fallbackProcessing(exec, decl, this);
            } else {
                Saxon.Hej.expr.Expression evaluateExpr = getConfiguration().makeEvaluateInstruction(this, decl);
                if ((evaluateExpr is Saxon.Hej.expr.ErrorExpression)) {
                    return evaluateExpr;
                }
                if (hasFallbackChildren) {
                    Saxon.Hej.expr.Expression[] conditions = new Saxon.Hej.expr.Expression[2];
                    Saxon.Hej.expr.Expression sysProp = Saxon.Hej.functions.SystemFunction.makeCall("system-property", makeRetainedStaticContext(), new Saxon.Hej.expr.StringLiteral((("Q{"+Saxon.Hej.lib.NamespaceConstant.XSLT)+"}supports-dynamic-evaluation")));
                    conditions[0] = new Saxon.Hej.expr.ValueComparison(sysProp, Saxon.Hej.expr.parser.Token.FEQ, new Saxon.Hej.expr.StringLiteral("no"));
                    conditions[1] = Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.BooleanValue.TRUE);
                    Saxon.Hej.expr.Expression[] actions = new Saxon.Hej.expr.Expression[2];
                    System.Collections.Generic.IList<Saxon.Hej.expr.Expression> fallbackExpressions = new System.Collections.Generic.List<Saxon.Hej.expr.Expression>();
                    foreach (Saxon.Hej.om.NodeInfo child in children(x => (x is Saxon.Hej.style.XSLFallback))) {
                        fallbackExpressions.Add(((Saxon.Hej.style.XSLFallback)(child)).compileSequenceConstructor(exec, decl, false));
                    }
                    actions[0] = new Saxon.Hej.expr.instruct.Block(fallbackExpressions.ToArray());
                    actions[1] = evaluateExpr;
                    return new Saxon.Hej.expr.instruct.Choose(conditions, actions);
                } else {
                    return evaluateExpr;
                }
            }
        }
    
    }
}