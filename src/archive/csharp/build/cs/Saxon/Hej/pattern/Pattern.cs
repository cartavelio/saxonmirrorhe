using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.pattern {
    public abstract class Pattern : Saxon.Hej.expr.PseudoExpression {
        private double priority = 0.5;
        private bool recoverable = true;
        private string originalText;
        public static Saxon.Hej.pattern.Pattern make(string pattern, Saxon.Hej.expr.StaticContext env, Saxon.Hej.expr.PackageData packageData) {
            Saxon.Hej.pattern.PatternParser parser = (Saxon.Hej.pattern.PatternParser)(env.getConfiguration().newExpressionParser("PATTERN", false));
            Saxon.Hej.pattern.Pattern pat = parser.parsePattern(pattern, env);
            pat.setRetainedStaticContext(env.makeRetainedStaticContext());
            pat = pat.simplify();
            return pat;
        }
        protected internal static void replaceCurrent(Saxon.Hej.expr.Expression exp, Saxon.Hej.expr.LocalBinding binding) {
            foreach (Saxon.Hej.expr.Operand o in exp.operands()) {
                Saxon.Hej.expr.Expression child = o.getChildExpression();
                if (child.isCallOn(typeof(Saxon.Hej.functions.Current))) {
                    Saxon.Hej.expr.LocalVariableReference @ref = new Saxon.Hej.expr.LocalVariableReference(binding);
                    o.setChildExpression(@ref);
                } else {
                    replaceCurrent(child, binding);
                }
            }
        }
        public static bool patternContainsVariable(Saxon.Hej.pattern.Pattern pattern) {
            return ((pattern != null) && (((pattern.getDependencies()&Saxon.Hej.expr.StaticProperty.DEPENDS_ON_LOCAL_VARIABLES)) != 0));
        }
        public override bool isLiftable(bool forStreaming) {
            return false;
        }
        public virtual void bindCurrent(Saxon.Hej.expr.LocalBinding binding) {
        }
        public virtual bool matchesCurrentGroup() {
            return false;
        }
        public void setOriginalText(string text) {
            originalText = text;
        }
        public bool isRecoverable() {
            return recoverable;
        }
        public void setRecoverable(bool recoverable) {
            this.recoverable = recoverable;
        }
        protected internal void handleDynamicError(Saxon.Hej.trans.XPathException ex, Saxon.Hej.expr.XPathContext context) {
            if ("XTDE0640".Equals(ex.getErrorCodeLocalPart())) {
                throw ex;
            }
            if (!(isRecoverable())) {
                throw ex;
            }
            context.getController().warning(((("An error occurred matching pattern {"+this)+"}: ")+ex.Message), ex.getErrorCodeQName().getEQName(), getLocation());
        }
        public override Saxon.Hej.pattern.Pattern simplify() {
            return this;
        }
        public override Saxon.Hej.pattern.Pattern typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            typeCheckChildren(visitor, contextInfo);
            return this;
        }
        public override int getDependencies() {
            return 0;
        }
        public virtual int allocateSlots(Saxon.Hej.expr.instruct.SlotManager slotManager, int nextFree) {
            return nextFree;
        }
        public virtual bool isMotionless() {
            return true;
        }
        public override bool effectiveBooleanValue(Saxon.Hej.expr.XPathContext context) {
            return matches(context.getContextItem(), context);
        }
        public abstract bool matches(Saxon.Hej.om.Item item, Saxon.Hej.expr.XPathContext context);
        public virtual bool matchesBeneathAnchor(Saxon.Hej.om.NodeInfo node, Saxon.Hej.om.NodeInfo anchor, Saxon.Hej.expr.XPathContext context) {
            return matches(node, context);
        }
        public virtual Saxon.Hej.om.SequenceIterator selectNodes(Saxon.Hej.om.TreeInfo document, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.om.NodeInfo doc = document.getRootNode();
            Saxon.Hej.type.UType uType = getUType();
            if (Saxon.Hej.type.UType.DOCUMENT.subsumes(uType)) {
                if (matches(doc, context)) {
                    return Saxon.Hej.tree.iter.SingletonIterator.makeIterator(doc);
                } else {
                    return Saxon.Hej.tree.iter.EmptyIterator.ofNodes();
                }
            } else {
                if (Saxon.Hej.type.UType.ATTRIBUTE.subsumes(uType)) {
                    Saxon.Hej.tree.iter.AxisIterator allElements = doc.iterateAxis(Saxon.Hej.om.AxisInfo.DESCENDANT, Saxon.Hej.pattern.NodeKindTest.ELEMENT);
                    Saxon.Hej.om.SequenceIterator allAttributes = Saxon.Hej.expr.MappingIterator.map(allElements, item => ((Saxon.Hej.om.NodeInfo)(item)).iterateAxis(Saxon.Hej.om.AxisInfo.ATTRIBUTE));
                    return Saxon.Hej.expr.ItemMappingIterator.filter(allAttributes, item => matches(item, context));
                } else {
                    if (Saxon.Hej.type.UType.NAMESPACE.subsumes(uType)) {
                        Saxon.Hej.tree.iter.AxisIterator allElements = doc.iterateAxis(Saxon.Hej.om.AxisInfo.DESCENDANT, Saxon.Hej.pattern.NodeKindTest.ELEMENT);
                        Saxon.Hej.om.SequenceIterator allNamespaces = Saxon.Hej.expr.MappingIterator.map(allElements, item => ((Saxon.Hej.om.NodeInfo)(item)).iterateAxis(Saxon.Hej.om.AxisInfo.NAMESPACE));
                        return Saxon.Hej.expr.ItemMappingIterator.filter(allNamespaces, item => matches(item, context));
                    } else {
                        if (Saxon.Hej.type.UType.CHILD_NODE_KINDS.subsumes(uType)) {
                            Saxon.Hej.pattern.NodeTest nodeTest;
                            if (uType.Equals(Saxon.Hej.type.UType.ELEMENT)) {
                                nodeTest = Saxon.Hej.pattern.NodeKindTest.ELEMENT;
                            } else {
                                nodeTest = new Saxon.Hej.pattern.MultipleNodeKindTest(uType);
                            }
                            Saxon.Hej.tree.iter.AxisIterator allChildren = doc.iterateAxis(Saxon.Hej.om.AxisInfo.DESCENDANT, nodeTest);
                            return Saxon.Hej.expr.ItemMappingIterator.filter(allChildren, item => matches(item, context));
                        } else {
                            int axis = (uType.subsumes(Saxon.Hej.type.UType.DOCUMENT) ? Saxon.Hej.om.AxisInfo.DESCENDANT_OR_SELF : Saxon.Hej.om.AxisInfo.DESCENDANT);
                            Saxon.Hej.tree.iter.AxisIterator allChildren = doc.iterateAxis(axis);
                            Saxon.Hej.om.SequenceIterator attributesOrSelf = Saxon.Hej.expr.MappingIterator.map(allChildren, item => {
                                Saxon.Hej.tree.iter.AxisIterator mapper = Saxon.Hej.tree.iter.SingleNodeIterator.makeIterator((Saxon.Hej.om.NodeInfo)(item));
                                if (uType.subsumes(Saxon.Hej.type.UType.NAMESPACE)) {
                                    mapper = new Saxon.Hej.tree.iter.ConcatenatingAxisIterator(mapper, ((Saxon.Hej.om.NodeInfo)(item)).iterateAxis(Saxon.Hej.om.AxisInfo.NAMESPACE));
                                }
                                if (uType.subsumes(Saxon.Hej.type.UType.ATTRIBUTE)) {
                                    mapper = new Saxon.Hej.tree.iter.ConcatenatingAxisIterator(mapper, ((Saxon.Hej.om.NodeInfo)(item)).iterateAxis(Saxon.Hej.om.AxisInfo.ATTRIBUTE));
                                }
                                return mapper;
                            } );
                            return Saxon.Hej.expr.ItemMappingIterator.filter(attributesOrSelf, item => matches(item, context));
                        }
                    }
                }
            }
        }
        public abstract Saxon.Hej.type.UType getUType();
        public virtual int getFingerprint() {
            return -1;
        }
        public abstract override Saxon.Hej.type.ItemType getItemType();
        public void setPriority(double priority) {
            this.priority = priority;
        }
        public virtual double getDefaultPriority() {
            return priority;
        }
        public override string ToString() {
            if ((originalText != null)) {
                return originalText;
            } else {
                return reconstruct();
            }
        }
        public virtual string reconstruct() {
            return ("pattern matching "+getItemType());
        }
        public virtual Saxon.Api.HostLanguage getHostLanguage() {
            return Saxon.Api.HostLanguage.XSLT;
        }
        public virtual Saxon.Hej.pattern.Pattern convertToTypedPattern(string val) {
            return null;
        }
        public override Saxon.Hej.pattern.Pattern toPattern(Saxon.Hej.Configuration config) {
            return this;
        }
        public abstract override void export(Saxon.Hej.trace.ExpressionPresenter presenter);
        public abstract override Saxon.Hej.pattern.Pattern copy(Saxon.Hej.expr.parser.RebindingMap rebindings);
        public override Saxon.Hej.pattern.Pattern optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            return this;
        }
        public string getOriginalText() {
            return originalText;
        }
        public override string toShortString() {
            return ToString();
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.pattern.Pattern.PatternElaborator();
        }
        private class PatternElaborator : Saxon.Hej.expr.elab.BooleanElaborator {
            public override Saxon.Hej.expr.elab.BooleanEvaluator elaborateForBoolean() {
                Saxon.Hej.pattern.Pattern pat = (Saxon.Hej.pattern.Pattern)(getExpression());
                return context => pat.matches(context.getContextItem(), context);
            }
        
        }
    
    }
}