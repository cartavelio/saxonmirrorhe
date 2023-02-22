using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLAccumulator : Saxon.Hej.style.StyleElement, Saxon.Hej.style.StylesheetComponent {
        private readonly Saxon.Hej.expr.accum.Accumulator accumulator = new Saxon.Hej.expr.accum.Accumulator();
        private Saxon.Hej.expr.instruct.SlotManager slotManager;
        public Saxon.Hej.expr.instruct.Actor getActor() {
            if ((accumulator.getDeclaringComponent() == null)) {
                accumulator.makeDeclaringComponent(Saxon.Hej.trans.Visibility.PRIVATE, getContainingPackage());
            }
            return accumulator;
        }
        public Saxon.Hej.trans.SymbolicName getSymbolicName() {
            Saxon.Hej.om.StructuredQName qname = accumulator.getAccumulatorName();
            return ((qname == null) ? null : new Saxon.Hej.trans.SymbolicName(Saxon.Hej.om.StandardNames.XSL_ACCUMULATOR, null));
        }
        public void checkCompatibility(Saxon.Hej.expr.Component component) {
        }
        public override bool isDeclaration() {
            return true;
        }
        private void prepareSimpleAttributes() {
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string value = att.getValue();
                string f = attName.getDisplayName();
                if (f.Equals("name")) {
                    string name = Saxon.Hej.value.Whitespace.trim(value);
                    accumulator.setAccumulatorName(makeQName(name, null, "name"));
                } else {
                    if (f.Equals("streamable")) {
                        accumulator.setDeclaredStreamable(false);
                        bool streamable = processStreamableAtt(value);
                        accumulator.setDeclaredStreamable(streamable);
                    } else {
                        if ((attName.hasURI(Saxon.Hej.om.NamespaceUri.SAXON) && attName.getLocalPart().Equals("trace"))) {
                            if (isExtensionAttributeAllowed(attName.getDisplayName())) {
                                accumulator.setTracing(processBooleanAttribute("saxon:trace", value));
                            }
                        } else {
                        }
                    }
                }
            }
            if ((accumulator.getAccumulatorName() == null)) {
                reportAbsence("name");
                accumulator.setAccumulatorName(new Saxon.Hej.om.StructuredQName("anon", Saxon.Hej.lib.NamespaceConstant.ANONYMOUS, ("acc"+GetHashCode())));
            }
        }
        protected internal override void prepareAttributes() {
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string f = attName.getDisplayName();
                string value = att.getValue();
                if (f.Equals("name")) {
                } else {
                    if (f.Equals("streamable")) {
                    } else {
                        if (f.Equals("initial-value")) {
                            accumulator.setInitialValueExpression(makeExpression(value, att));
                        } else {
                            if (f.Equals("as")) {
                                try {
                                    Saxon.Hej.value.SequenceType requiredType = makeSequenceType(value);
                                    accumulator.setType(requiredType);
                                } catch (Saxon.Hej.trans.XPathException e) {
                                    compileErrorInAttribute(e.Message, e.getErrorCodeLocalPart(), "as");
                                }
                            } else {
                                if ((attName.hasURI(Saxon.Hej.om.NamespaceUri.SAXON) && attName.getLocalPart().Equals("trace"))) {
                                    if (isExtensionAttributeAllowed(attName.getDisplayName())) {
                                        accumulator.setTracing(processBooleanAttribute("saxon:trace", value));
                                    }
                                } else {
                                    checkUnknownAttribute(attName);
                                }
                            }
                        }
                    }
                }
            }
            if ((accumulator.getType() == null)) {
                accumulator.setType(Saxon.Hej.value.SequenceType.ANY_SEQUENCE);
            }
            if ((accumulator.getInitialValueExpression() == null)) {
                reportAbsence("initial-value");
                Saxon.Hej.expr.StringLiteral zls = new Saxon.Hej.expr.StringLiteral(Saxon.Hej.value.StringValue.EMPTY_STRING);
                zls.setRetainedStaticContext(makeRetainedStaticContext());
                accumulator.setInitialValueExpression(zls);
            }
        }
        public override void compileDeclaration(Saxon.Hej.style.Compilation compilation, Saxon.Hej.style.ComponentDeclaration decl) {
            Saxon.Hej.Configuration config = compilation.getConfiguration();
            {
                accumulator.setPackageData(compilation.getPackageData());
                accumulator.obtainDeclaringComponent(decl.getSourceElement());
                Saxon.Hej.expr.Expression init = accumulator.getInitialValueExpression();
                Saxon.Hej.expr.parser.ExpressionVisitor visitor = Saxon.Hej.expr.parser.ExpressionVisitor.make(getStaticContext());
                init = init.typeCheck(visitor, config.getDefaultContextItemStaticInfo());
                Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> role = () => new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.INSTRUCTION, "xsl:accumulator-rule/select", 0);
                init = config.getTypeChecker(false).staticTypeCheck(init, accumulator.getType(), role, visitor);
                init = init.optimize(visitor, config.getDefaultContextItemStaticInfo());
                Saxon.Hej.expr.instruct.SlotManager stackFrameMap = config.makeSlotManager();
                Saxon.Hej.expr.parser.ExpressionTool.allocateSlots(init, 0, stackFrameMap);
                accumulator.setSlotManagerForInitialValueExpression(stackFrameMap);
                checkInitialStreamability(init);
                accumulator.setInitialValueExpression(init);
                accumulator.addChildExpression(init);
            }
            int position = 0;
            foreach (Saxon.Hej.om.NodeInfo curr in children(x => (x is Saxon.Hej.style.XSLAccumulatorRule))) {
                Saxon.Hej.style.XSLAccumulatorRule rule = (Saxon.Hej.style.XSLAccumulatorRule)(curr);
                Saxon.Hej.pattern.Pattern pattern = rule.getMatch();
                Saxon.Hej.expr.Expression newValueExp = rule.getNewValueExpression(compilation, decl);
                Saxon.Hej.expr.parser.ExpressionVisitor visitor = Saxon.Hej.expr.parser.ExpressionVisitor.make(getStaticContext());
                newValueExp = newValueExp.typeCheck(visitor, config.makeContextItemStaticInfo(pattern.getItemType(), false));
                Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> role = () => new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.INSTRUCTION, "xsl:accumulator-rule/select", 0);
                newValueExp = config.getTypeChecker(false).staticTypeCheck(newValueExp, accumulator.getType(), role, visitor);
                newValueExp = newValueExp.optimize(visitor, getConfiguration().makeContextItemStaticInfo(pattern.getItemType(), false));
                slotManager.allocateSlotNumber(new Saxon.Hej.om.StructuredQName("", Saxon.Hej.om.NamespaceUri.NULL, "value"));
                Saxon.Hej.expr.parser.ExpressionTool.allocateSlots(newValueExp, 1, slotManager);
                bool isPreDescent = !(rule.isPostDescent());
                Saxon.Hej.trans.SimpleMode mode = (isPreDescent ? accumulator.getPreDescentRules() : accumulator.getPostDescentRules());
                Saxon.Hej.expr.accum.AccumulatorRule action = new Saxon.Hej.expr.accum.AccumulatorRule(newValueExp, slotManager, rule.isPostDescent());
                mode.addRule(pattern, action, decl.getModule(), decl.getModule().getPrecedence(), 1, position++, 0);
                checkRuleStreamability(rule, pattern, newValueExp);
                if (((accumulator.isDeclaredStreamable() && rule.isPostDescent()) && rule.isCapture())) {
                    action.setCapturing(true);
                }
                Saxon.Hej.type.ItemType itemType = pattern.getItemType();
                if ((itemType is Saxon.Hej.pattern.NodeTest)) {
                    if (!(itemType.getUType().overlaps(Saxon.Hej.type.UType.DOCUMENT.union(Saxon.Hej.type.UType.CHILD_NODE_KINDS)))) {
                        rule.issueWarning("An accumulator rule that matches attribute or namespace nodes has no effect", "SXWN9999");
                    }
                } else {
                    if ((itemType is Saxon.Hej.type.AtomicType)) {
                        rule.issueWarning("An accumulator rule that matches atomic values has no effect", "SXWN9999");
                    }
                }
                accumulator.addChildExpression(newValueExp);
                accumulator.addChildExpression(pattern);
            }
            accumulator.getPreDescentRules().allocateAllPatternSlots();
            accumulator.getPostDescentRules().allocateAllPatternSlots();
        }
        public override Saxon.Hej.om.StructuredQName getObjectName() {
            Saxon.Hej.om.StructuredQName qn = base.getObjectName();
            if ((qn == null)) {
                string nameAtt = Saxon.Hej.value.Whitespace.trim(getAttributeValue(Saxon.Hej.om.NamespaceUri.NULL, "name"));
                if ((nameAtt == null)) {
                    return new Saxon.Hej.om.StructuredQName("saxon", Saxon.Hej.om.NamespaceUri.SAXON, ("badly-named-accumulator"+generateId()));
                }
                qn = makeQName(nameAtt, null, "name");
                setObjectName(qn);
            }
            return qn;
        }
        public override void index(Saxon.Hej.style.ComponentDeclaration decl, Saxon.Hej.style.PrincipalStylesheetModule top) {
            if ((accumulator.getAccumulatorName() == null)) {
                prepareSimpleAttributes();
            }
            accumulator.setImportPrecedence(decl.getPrecedence());
            if ((top.getAccumulatorManager() == null)) {
                Saxon.Hej.style.StyleNodeFactory styleNodeFactory = getCompilation().getStyleNodeFactory(true);
                Saxon.Hej.expr.accum.AccumulatorRegistry manager = styleNodeFactory.makeAccumulatorManager();
                top.setAccumulatorManager(manager);
                getCompilation().getPackageData().setAccumulatorRegistry(manager);
            }
            Saxon.Hej.expr.accum.AccumulatorRegistry mgr = top.getAccumulatorManager();
            Saxon.Hej.expr.accum.Accumulator existing = mgr.getAccumulator(accumulator.getAccumulatorName());
            if ((existing != null)) {
                int existingPrec = existing.getImportPrecedence();
                if ((existingPrec == decl.getPrecedence())) {
                    compileError((("There are two accumulators with the same name ("+accumulator.getAccumulatorName().getDisplayName())+") and the same import precedence"), "XTSE3350");
                }
                if ((existingPrec>decl.getPrecedence())) {
                    return ;
                }
            }
            mgr.addAccumulator(accumulator);
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            slotManager = getConfiguration().makeSlotManager();
            checkTopLevel("XTSE0010", true);
            bool foundRule = false;
            foreach (Saxon.Hej.om.NodeInfo curr in children()) {
                if ((curr is Saxon.Hej.style.XSLAccumulatorRule)) {
                    foundRule = true;
                } else {
                    compileError("Only xsl:accumulator-rule is allowed here", "XTSE0010");
                }
            }
            if (!(foundRule)) {
                compileError("xsl:accumulator must contain at least one xsl:accumulator-rule", "XTSE0010");
            }
        }
        public Saxon.Hej.expr.instruct.SlotManager getSlotManager() {
            return slotManager;
        }
        public void optimize(Saxon.Hej.style.ComponentDeclaration declaration) {
        }
        public Saxon.Hej.value.SequenceType getResultType() {
            return accumulator.getType();
        }
        private void checkInitialStreamability(Saxon.Hej.expr.Expression init) {
            if (accumulator.isDeclaredStreamable()) {
                Saxon.Eej.ee.stream.PostureAndSweep ps = /*134*/Saxon.Eej.ee.stream.Streamability.getStreamability(init, Saxon.Eej.ee.trans.ContextItemStaticInfoEE.DEFAULT, null);
                if ((ps.getSweep() != Saxon.Eej.ee.stream.Sweep.MOTIONLESS)) {
                    notStreamable(this, "The initial value expression for a streaming accumulator must be motionless");
                }
            }
        }
        private void checkRuleStreamability(Saxon.Hej.style.XSLAccumulatorRule rule, Saxon.Hej.pattern.Pattern pattern, Saxon.Hej.expr.Expression newValueExp) {
            if (accumulator.isDeclaredStreamable()) {
                if (!(pattern.isMotionless())) {
                    string message = "The patterns for the accumulator rules in a streaming accumulator must be motionless";
                    notStreamable(rule, message);
                }
                Saxon.Eej.ee.trans.ContextItemStaticInfoEE csi = (Saxon.Eej.ee.trans.ContextItemStaticInfoEE)(getConfiguration().makeContextItemStaticInfo(pattern.getItemType(), false));
                csi.setContextPostureStriding();
                csi.setAccumulatorPhase((rule.isPostDescent() ? Saxon.Hej.functions.AccumulatorFn.Phase.AFTER : Saxon.Hej.functions.AccumulatorFn.Phase.BEFORE));
                System.Collections.Generic.IList<string> reasons = new System.Collections.Generic.List<string>(4);
                Saxon.Eej.ee.stream.PostureAndSweep ps = /*134*/Saxon.Eej.ee.stream.Streamability.getStreamability(newValueExp, csi, reasons);
                if ((!(ps.Equals(Saxon.Eej.ee.stream.PostureAndSweep.GROUNDED_AND_MOTIONLESS)) && !(rule.isCapture()))) {
                    Saxon.Ejava.lang.StringBuilder message = new Saxon.Ejava.lang.StringBuilder(("The xsl:accumulator-rule/@select expression (or contained sequence constructor) "+"for a streaming accumulator must be grounded and motionless"));
                    foreach (string reason in reasons) {
                        message.append(". ").append(reason);
                    }
                    notStreamable(rule, message.ToString());
                }
            }
        }
        private void notStreamable(Saxon.Hej.style.StyleElement rule, string message) {
            bool fallback = getConfiguration().getBooleanProperty(Saxon.Hej.lib.Feature<bool>.STREAMING_FALLBACK);
            if (fallback) {
                message += ". Falling back to non-streaming implementation";
                rule.issueWarning(message, "XTSE3430");
                rule.getCompilation().setFallbackToNonStreaming(true);
            } else {
                rule.compileError(message, "XTSE3430");
            }
        }
    
    }
}