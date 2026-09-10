using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    #pragma warning disable 659
    public class ContextItemExpression : Saxon.Hej.expr.Expression {
        private Saxon.Hej.expr.parser.ContextItemStaticInfo staticInfo = Saxon.Hej.expr.parser.ContextItemStaticInfo.DEFAULT;
        private string errorCodeForAbsentContext = "XPDY0002";
        private bool absentContextIsTypeError = false;
        public ContextItemExpression()  {
        }
        public override string getExpressionName() {
            return "dot";
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            Saxon.Hej.expr.ContextItemExpression cie2 = new Saxon.Hej.expr.ContextItemExpression();
            cie2.staticInfo = staticInfo;
            cie2.setErrorCodeForUndefinedContext(errorCodeForAbsentContext, false);
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, cie2);
            return cie2;
        }
        public void setErrorCodeForUndefinedContext(string errorCode, bool isTypeError) {
            errorCodeForAbsentContext = errorCode;
            absentContextIsTypeError = isTypeError;
        }
        public string getErrorCodeForUndefinedContext() {
            return errorCodeForAbsentContext;
        }
        public void setStaticInfo(Saxon.Hej.expr.parser.ContextItemStaticInfo info) {
            staticInfo = info;
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            if ((contextInfo.getItemType() == Saxon.Hej.type.ErrorType.getInstance())) {
                visitor.issueWarning("Evaluation will always fail: there is no context item", Saxon.Hej.trans.SaxonErrorCode.SXWN9027, getLocation());
                Saxon.Hej.expr.ErrorExpression ee = new Saxon.Hej.expr.ErrorExpression("There is no context item", getErrorCodeForUndefinedContext(), absentContextIsTypeError);
                ee.setOriginalExpression(this);
                Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, ee);
                return ee;
            } else {
                staticInfo = contextInfo;
            }
            return this;
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            if ((contextItemType == null)) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("The context item is undefined at this point");
                err.setErrorCode(getErrorCodeForUndefinedContext());
                err.setIsTypeError(absentContextIsTypeError);
                err.setLocation(getLocation());
                throw err;
            }
            return this;
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return staticInfo.getItemType();
        }
        public override Saxon.Hej.type.UType getStaticUType(Saxon.Hej.type.UType contextItemType) {
            return contextItemType;
        }
        public bool isContextPossiblyUndefined() {
            return staticInfo.isPossiblyAbsent();
        }
        protected internal override int computeCardinality() {
            return Saxon.Hej.expr.StaticProperty.EXACTLY_ONE;
        }
        protected internal override int computeSpecialProperties() {
            int p = base.computeSpecialProperties();
            return ((p|Saxon.Hej.expr.StaticProperty.NO_NODES_NEWLY_CREATED)|Saxon.Hej.expr.StaticProperty.CONTEXT_DOCUMENT_NODESET);
        }
        public override int getImplementationMethod() {
            return EVALUATE_METHOD;
        }
        public override bool Equals(System.Object other) {
            return (other is Saxon.Hej.expr.ContextItemExpression);
        }
        protected internal override int computeHashCode() {
            return "ContextItemExpression".GetHashCode();
        }
        public override int getIntrinsicDependencies() {
            return Saxon.Hej.expr.StaticProperty.DEPENDS_ON_CONTEXT_ITEM;
        }
        public override Saxon.Hej.expr.parser.PathMap.PathMapNodeSet addToPathMap(Saxon.Hej.expr.parser.PathMap pathMap, Saxon.Hej.expr.parser.PathMap.PathMapNodeSet pathMapNodeSet) {
            if ((pathMapNodeSet == null)) {
                pathMapNodeSet = new Saxon.Hej.expr.parser.PathMap.PathMapNodeSet(pathMap.makeNewRoot(this));
            }
            return pathMapNodeSet;
        }
        public override bool isSubtreeExpression() {
            return true;
        }
        public override int getNetCost() {
            return 0;
        }
        public override Saxon.Hej.pattern.Pattern toPattern(Saxon.Hej.Configuration config) {
            return Saxon.Hej.pattern.AnchorPattern.getInstance();
        }
        public override string getStreamerName() {
            return "ContextItemExpr";
        }
        public override Saxon.Hej.om.SequenceIterator iterate(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.om.Item item = context.getContextItem();
            if ((item == null)) {
                reportAbsentContext(context);
            }
            return Saxon.Hej.tree.iter.SingletonIterator.makeIterator(item);
        }
        public override Saxon.Hej.om.Item evaluateItem(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.om.Item item = context.getContextItem();
            if ((item == null)) {
                reportAbsentContext(context);
            }
            return item;
        }
        public void reportAbsentContext(Saxon.Hej.expr.XPathContext context) {
            if (absentContextIsTypeError) {
                typeError("The context item is absent", getErrorCodeForUndefinedContext(), context);
            } else {
                dynamicError("The context item is absent", getErrorCodeForUndefinedContext(), context);
            }
        }
        public override string ToString() {
            return ".";
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter destination) {
            destination.startElement("dot", this);
            Saxon.Hej.type.ItemType type = getItemType();
            if (!(((type == Saxon.Hej.type.AnyItemType.getInstance())))) {
                Saxon.Hej.value.SequenceType st = Saxon.Hej.value.SequenceType.makeSequenceType(type, Saxon.Hej.expr.StaticProperty.EXACTLY_ONE);
                destination.emitAttribute("type", st.toAlphaCode());
            }
            if (staticInfo.isPossiblyAbsent()) {
                destination.emitAttribute("flags", "a");
            }
            destination.endElement();
        }
        public override string toShortString() {
            return ".";
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.expr.ContextItemExpression.ContextItemElaborator();
        }
        public class ContextItemElaborator : Saxon.Hej.expr.elab.ItemElaborator {
            public override Saxon.Hej.expr.elab.ItemEvaluator elaborateForItem() {
                Saxon.Hej.expr.ContextItemExpression cie = (Saxon.Hej.expr.ContextItemExpression)(getExpression());
                if (cie.isContextPossiblyUndefined()) {
                    return context => {
                        Saxon.Hej.om.Item current = context.getContextItem();
                        if ((current == null)) {
                            cie.reportAbsentContext(context);
                        }
                        return current;
                    } ;
                } else {
                    return x => x.getContextItem() /* method reference */;
                }
            }
            public override Saxon.Hej.expr.elab.PullEvaluator elaborateForPull() {
                Saxon.Hej.expr.ContextItemExpression cie = (Saxon.Hej.expr.ContextItemExpression)(getExpression());
                if (cie.isContextPossiblyUndefined()) {
                    return context => {
                        Saxon.Hej.om.Item current = context.getContextItem();
                        if ((current == null)) {
                            cie.reportAbsentContext(context);
                        }
                        return Saxon.Hej.tree.iter.SingletonIterator.makeIterator(current);
                    } ;
                } else {
                    return context => Saxon.Hej.tree.iter.SingletonIterator.makeIterator(context.getContextItem());
                }
            }
        
        }
    
    }
}