package com.saxonica.ee.elab.expr;

import com.saxonica.ee.elab.eval.ItemEvaluator;
import com.saxonica.ee.elab.gen.ItemElaborator;
import net.sf.saxon.expr.AttributeGetter;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.tree.tiny.TinyElementImpl;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.Type;
import net.sf.saxon.value.StringValue;

/**
 * Elaborator for an AttributeGetter expression (which gets a named attribute of the context item
 * and returns its value as an untyped atomic value)
 */

public class AttributeGetterElaborator extends ItemElaborator {

    public ItemEvaluator elaborateForItem() {
        final AttributeGetter exp = (AttributeGetter)getExpression();
        final int fingerprint = exp.getAttributeName().getFingerprint();
        final NamespaceUri uri = exp.getAttributeName().getURI();
        final String local = exp.getAttributeName().getLocalPart();
        return context -> {
            Item item = context.getContextItem();
            if (item instanceof TinyElementImpl) {
                // fast path
                String val = ((TinyElementImpl) item).getAttributeValue(fingerprint);
                return val == null ? null : new StringValue(val, BuiltInAtomicType.UNTYPED_ATOMIC);
            }
            assert item instanceof NodeInfo;
            NodeInfo node = (NodeInfo) item;
            if (node.getNodeKind() == Type.ELEMENT) {
                String val = node.getAttributeValue(uri, local);
                if (val != null) {
                    return new StringValue(val, BuiltInAtomicType.UNTYPED_ATOMIC);
                }
            }
            return null;
        };
    }


}

