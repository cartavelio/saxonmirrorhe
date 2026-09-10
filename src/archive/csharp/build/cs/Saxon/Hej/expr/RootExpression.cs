using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    #pragma warning disable 659
    public class RootExpression : Saxon.Hej.expr.Expression {
        private bool contextMaybeUndefined = true;
        private bool doneWarnings = false;
        public RootExpression()  {
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            Saxon.Hej.type.TypeHierarchy th = visitor.getConfiguration().getTypeHierarchy();
            if ((((contextInfo == null) || (contextInfo.getItemType() == null)) || contextInfo.getItemType().Equals(Saxon.Hej.type.ErrorType.getInstance()))) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((noContextMessage()+": the context item is absent"));
                err.setErrorCode("XPDY0002");
                err.setIsTypeError(true);
                err.setLocation(getLocation());
                throw err;
            } else {
                if (((!(doneWarnings) && contextInfo.isParentless()) && (th.relationship(contextInfo.getItemType(), Saxon.Hej.pattern.NodeKindTest.DOCUMENT) == Saxon.Hej.type.Affinity.DISJOINT))) {
                    visitor.issueWarning((noContextMessage()+": the context item is parentless and is not a document node"), getLocation());
                    doneWarnings = true;
                }
            }
            contextMaybeUndefined = contextInfo.isPossiblyAbsent();
            if (th.isSubType(contextInfo.getItemType(), Saxon.Hej.pattern.NodeKindTest.DOCUMENT)) {
                Saxon.Hej.expr.ContextItemExpression cie = new Saxon.Hej.expr.ContextItemExpression();
                Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, cie);
                cie.setStaticInfo(contextInfo);
                return cie;
            }
            Saxon.Hej.type.Affinity relation = th.relationship(contextInfo.getItemType(), Saxon.Hej.pattern.AnyNodeTest.getInstance());
            if ((relation == Saxon.Hej.type.Affinity.DISJOINT)) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((noContextMessage()+": the context item is not a node"));
                err.setErrorCode("XPTY0020");
                err.setIsTypeError(true);
                err.setLocation(getLocation());
                throw err;
            }
            return this;
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            return typeCheck(visitor, contextItemType);
        }
        protected internal override int computeSpecialProperties() {
            return (((Saxon.Hej.expr.StaticProperty.ORDERED_NODESET|Saxon.Hej.expr.StaticProperty.CONTEXT_DOCUMENT_NODESET)|Saxon.Hej.expr.StaticProperty.SINGLE_DOCUMENT_NODESET)|Saxon.Hej.expr.StaticProperty.NO_NODES_NEWLY_CREATED);
        }
        public bool isContextPossiblyUndefined() {
            return contextMaybeUndefined;
        }
        protected internal string noContextMessage() {
            return "Leading '/' selects nothing";
        }
        public override bool Equals(System.Object other) {
            return (other is Saxon.Hej.expr.RootExpression);
        }
        protected internal override int computeCardinality() {
            return Saxon.Hej.expr.StaticProperty.EXACTLY_ONE;
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return Saxon.Hej.pattern.NodeKindTest.DOCUMENT;
        }
        public override Saxon.Hej.type.UType getStaticUType(Saxon.Hej.type.UType contextItemType) {
            return Saxon.Hej.type.UType.DOCUMENT;
        }
        public override int getImplementationMethod() {
            return EVALUATE_METHOD;
        }
        protected internal override int computeHashCode() {
            return "RootExpression".GetHashCode();
        }
        public Saxon.Hej.om.NodeInfo getNode(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.om.Item current = context.getContextItem();
            if ((current == null)) {
                dynamicError("Finding root of tree: the context item is absent", "XPDY0002", context);
            }
            if ((current is Saxon.Hej.om.NodeInfo)) {
                Saxon.Hej.om.NodeInfo doc = ((Saxon.Hej.om.NodeInfo)(current)).getRoot();
                if ((doc.getNodeKind() != Saxon.Hej.type.Type.DOCUMENT)) {
                    dynamicError("The root of the tree containing the context item is not a document node", "XPDY0050", context);
                }
                return doc;
            }
            typeError("Finding root of tree: the context item is not a node", "XPTY0020", context);
            return null;
        }
        public override int getIntrinsicDependencies() {
            return Saxon.Hej.expr.StaticProperty.DEPENDS_ON_CONTEXT_DOCUMENT;
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.RootExpression exp = new Saxon.Hej.expr.RootExpression();
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, exp);
            return exp;
        }
        public override Saxon.Hej.pattern.Pattern toPattern(Saxon.Hej.Configuration config) {
            return new Saxon.Hej.pattern.NodeTestPattern(Saxon.Hej.pattern.NodeKindTest.DOCUMENT);
        }
        public override Saxon.Hej.expr.parser.PathMap.PathMapNodeSet addToPathMap(Saxon.Hej.expr.parser.PathMap pathMap, Saxon.Hej.expr.parser.PathMap.PathMapNodeSet pathMapNodeSet) {
            if ((pathMapNodeSet == null)) {
                Saxon.Hej.expr.ContextItemExpression cie = new Saxon.Hej.expr.ContextItemExpression();
                Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, cie);
                pathMapNodeSet = new Saxon.Hej.expr.parser.PathMap.PathMapNodeSet(pathMap.makeNewRoot(cie));
            }
            return pathMapNodeSet.createArc(Saxon.Hej.om.AxisInfo.ANCESTOR_OR_SELF, Saxon.Hej.pattern.NodeKindTest.DOCUMENT);
        }
        public override string ToString() {
            return "(/)";
        }
        public override string getExpressionName() {
            return "root";
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter destination) {
            destination.startElement("root", this);
            destination.endElement();
        }
        public override Saxon.Hej.om.SequenceIterator iterate(Saxon.Hej.expr.XPathContext context) {
            return Saxon.Hej.tree.iter.SingletonIterator.makeIterator(getNode(context));
        }
        public override Saxon.Hej.om.NodeInfo evaluateItem(Saxon.Hej.expr.XPathContext context) {
            return getNode(context);
        }
        public override bool effectiveBooleanValue(Saxon.Hej.expr.XPathContext context) {
            return (getNode(context) != null);
        }
        public override string getStreamerName() {
            return "RootExpression";
        }
    
    }
}