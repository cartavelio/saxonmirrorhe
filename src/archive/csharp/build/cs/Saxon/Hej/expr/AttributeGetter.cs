using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr {
    #pragma warning disable 659
    public sealed class AttributeGetter : Saxon.Hej.expr.Expression {
        private readonly Saxon.Hej.om.FingerprintedQName attributeName;
        public AttributeGetter(Saxon.Hej.om.FingerprintedQName attributeName)  {
            this.attributeName = attributeName;
        }
        public Saxon.Hej.om.FingerprintedQName getAttributeName() {
            return attributeName;
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC;
        }
        protected internal override int computeCardinality() {
            return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_ONE;
        }
        protected internal override int computeSpecialProperties() {
            return Saxon.Hej.expr.StaticProperty.NO_NODES_NEWLY_CREATED;
        }
        public override int getIntrinsicDependencies() {
            return Saxon.Hej.expr.StaticProperty.DEPENDS_ON_CONTEXT_ITEM;
        }
        public override Saxon.Hej.expr.AttributeGetter copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            return new Saxon.Hej.expr.AttributeGetter(attributeName);
        }
        public override Saxon.Hej.expr.parser.PathMap.PathMapNodeSet addToPathMap(Saxon.Hej.expr.parser.PathMap pathMap, Saxon.Hej.expr.parser.PathMap.PathMapNodeSet pathMapNodeSet) {
            if ((pathMapNodeSet == null)) {
                Saxon.Hej.expr.ContextItemExpression cie = new Saxon.Hej.expr.ContextItemExpression();
                pathMapNodeSet = new Saxon.Hej.expr.parser.PathMap.PathMapNodeSet(pathMap.makeNewRoot(cie));
            }
            return pathMapNodeSet.createArc(Saxon.Hej.om.AxisInfo.ATTRIBUTE, new Saxon.Hej.pattern.NameTest(Saxon.Hej.type.Type.ATTRIBUTE, attributeName, getConfiguration().getNamePool()));
        }
        public override int getImplementationMethod() {
            return EVALUATE_METHOD;
        }
        public override Saxon.Hej.om.Item evaluateItem(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.om.Item item = context.getContextItem();
            if ((item is Saxon.Hej.tree.tiny.TinyElementImpl)) {
                string val = ((Saxon.Hej.tree.tiny.TinyElementImpl)(item)).getAttributeValue(attributeName.getFingerprint());
                return ((val == null) ? null : Saxon.Hej.value.StringValue.makeUntypedAtomic(Saxon.Hej.str.StringView.tidy(val)));
            }
            if ((item == null)) {
                dynamicError((("The context item for @"+attributeName.getDisplayName())+" is absent"), "XPDY0002", context);
            }
            if (!(((item is Saxon.Hej.om.NodeInfo)))) {
                typeError((("The context item for @"+attributeName.getDisplayName())+" is not a node"), "XPDY0002", context);
            }
            System.Diagnostics.Debug.Assert((item is Saxon.Hej.om.NodeInfo));
            Saxon.Hej.om.NodeInfo node = (Saxon.Hej.om.NodeInfo)(item);
            if ((node.getNodeKind() == Saxon.Hej.type.Type.ELEMENT)) {
                string val = node.getAttributeValue(attributeName.getNamespaceUri(), attributeName.getLocalPart());
                return ((val == null) ? null : Saxon.Hej.value.StringValue.makeUntypedAtomic(Saxon.Hej.str.StringView.tidy(val)));
            } else {
                return null;
            }
        }
        public override Saxon.Hej.str.UnicodeString evaluateAsString(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.om.Item item = context.getContextItem();
            if ((item is Saxon.Hej.tree.tiny.TinyElementImpl)) {
                string val = ((Saxon.Hej.tree.tiny.TinyElementImpl)(item)).getAttributeValue(attributeName.getFingerprint());
                return ((val == null) ? Saxon.Hej.str.EmptyUnicodeString.getInstance() : Saxon.Hej.str.StringView.tidy(val));
            }
            if ((item == null)) {
                dynamicError((("The context item for @"+attributeName.getDisplayName())+" is absent"), "XPDY0002", context);
            }
            if (!(((item is Saxon.Hej.om.NodeInfo)))) {
                typeError((("The context item for @"+attributeName.getDisplayName())+" is not a node"), "XPDY0002", context);
            }
            System.Diagnostics.Debug.Assert((item is Saxon.Hej.om.NodeInfo));
            Saxon.Hej.om.NodeInfo node = (Saxon.Hej.om.NodeInfo)(item);
            if ((node.getNodeKind() == Saxon.Hej.type.Type.ELEMENT)) {
                string val = node.getAttributeValue(attributeName.getNamespaceUri(), attributeName.getLocalPart());
                if ((val != null)) {
                    return Saxon.Hej.str.StringView.tidy(val);
                }
            }
            return Saxon.Hej.str.EmptyUnicodeString.getInstance();
        }
        public override string getExpressionName() {
            return "attGetter";
        }
        public override string toShortString() {
            return ("@"+attributeName.getDisplayName());
        }
        public override string ToString() {
            return (("data(@"+attributeName.getDisplayName())+")");
        }
        public override bool Equals(System.Object obj) {
            return ((obj is Saxon.Hej.expr.AttributeGetter) && ((Saxon.Hej.expr.AttributeGetter)(obj)).attributeName.Equals(attributeName));
        }
        protected internal override int computeHashCode() {
            return (83571^attributeName.GetHashCode());
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("attVal", this);
            @out.emitAttribute("name", attributeName.getStructuredQName());
            @out.endElement();
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.expr.AttributeGetter.AttributeGetterElaborator();
        }
        public class AttributeGetterElaborator : Saxon.Hej.expr.elab.ItemElaborator {
            public override Saxon.Hej.expr.elab.ItemEvaluator elaborateForItem() {
                Saxon.Hej.expr.AttributeGetter exp = (Saxon.Hej.expr.AttributeGetter)(getExpression());
                int fingerprint = exp.getAttributeName().getFingerprint();
                Saxon.Hej.om.NamespaceUri uri = exp.getAttributeName().getNamespaceUri();
                string local = exp.getAttributeName().getLocalPart();
                return context => {
                    Saxon.Hej.om.Item item = context.getContextItem();
                    if ((item is Saxon.Hej.tree.tiny.TinyElementImpl)) {
                        string val = ((Saxon.Hej.tree.tiny.TinyElementImpl)(item)).getAttributeValue(fingerprint);
                        return ((val == null) ? null : new Saxon.Hej.value.StringValue(val, Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC));
                    }
                    System.Diagnostics.Debug.Assert((item is Saxon.Hej.om.NodeInfo));
                    Saxon.Hej.om.NodeInfo node = (Saxon.Hej.om.NodeInfo)(item);
                    if ((node.getNodeKind() == Saxon.Hej.type.Type.ELEMENT)) {
                        string val = node.getAttributeValue(uri, local);
                        if ((val != null)) {
                            return new Saxon.Hej.value.StringValue(val, Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC);
                        }
                    }
                    return null;
                } ;
            }
            public override Saxon.Hej.expr.elab.StringEvaluator elaborateForString(bool zeroLengthWhenAbsent) {
                Saxon.Hej.expr.AttributeGetter expr = (Saxon.Hej.expr.AttributeGetter)(getExpression());
                int fingerprint = expr.getAttributeName().getFingerprint();
                Saxon.Hej.om.NamespaceUri uri = expr.getAttributeName().getNamespaceUri();
                string local = expr.getAttributeName().getLocalPart();
                return context => {
                    Saxon.Hej.om.Item item = context.getContextItem();
                    if ((item is Saxon.Hej.tree.tiny.TinyElementImpl)) {
                        string val = ((Saxon.Hej.tree.tiny.TinyElementImpl)(item)).getAttributeValue(fingerprint);
                        return handlePossiblyNullString(val, zeroLengthWhenAbsent);
                    }
                    if (!(((item is Saxon.Hej.om.NodeInfo)))) {
                        expr.typeError((("The context item for @"+expr.getAttributeName().getDisplayName())+" is not a node"), "XPDY0002", context);
                    }
                    System.Diagnostics.Debug.Assert((item is Saxon.Hej.om.NodeInfo));
                    Saxon.Hej.om.NodeInfo node = (Saxon.Hej.om.NodeInfo)(item);
                    if ((node.getNodeKind() == Saxon.Hej.type.Type.ELEMENT)) {
                        string val = node.getAttributeValue(uri, local);
                        return handlePossiblyNullString(val, zeroLengthWhenAbsent);
                    }
                    return (zeroLengthWhenAbsent ? "" : null);
                } ;
            }
        
        }
    
    }
}