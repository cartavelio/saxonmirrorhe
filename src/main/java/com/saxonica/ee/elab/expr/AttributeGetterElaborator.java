package com.saxonica.ee.elab.expr;

import com.saxonica.ee.elab.eval.StringEvaluator;
import com.saxonica.ee.elab.eval.UnicodeStringEvaluator;
import com.saxonica.ee.elab.gen.StringElaborator;
import net.sf.saxon.expr.AttributeGetter;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.str.StringView;
import net.sf.saxon.tree.tiny.TinyElementImpl;
import net.sf.saxon.type.Type;

/**
 * Elaborator for an AttributeGetter expression (which gets a named attribute of the context item
 * and returns its value as an untyped atomic value)
 */

public class AttributeGetterElaborator extends StringElaborator {

    public UnicodeStringEvaluator elaborateForUnicodeString(boolean zeroLengthWhenAbsent) {
        final AttributeGetter exp = (AttributeGetter)getExpression();
        final int fingerprint = exp.getAttributeName().getFingerprint();
        final String uri = exp.getAttributeName().getURI();
        final String local = exp.getAttributeName().getLocalPart();
        return context -> {
            Item item = context.getContextItem();
            if (item instanceof TinyElementImpl) {
                // fast path
                String val = ((TinyElementImpl) item).getAttributeValue(fingerprint);
                return val == null ? handleNullUnicodeString(zeroLengthWhenAbsent) : StringView.tidy(val);
            }
            assert item instanceof NodeInfo;
            NodeInfo node = (NodeInfo) item;
            if (node.getNodeKind() == Type.ELEMENT) {
                String val = node.getAttributeValue(uri, local);
                if (val != null) {
                    return StringView.tidy(val);
                }
            }
            return handleNullUnicodeString(zeroLengthWhenAbsent);
        };
    }

    public StringEvaluator elaborateForString(boolean zeroLengthWhenAbsent) {
        final AttributeGetter exp = (AttributeGetter) getExpression();
        final int fingerprint = exp.getAttributeName().getFingerprint();
        final String uri = exp.getAttributeName().getURI();
        final String local = exp.getAttributeName().getLocalPart();
        return context -> {
            Item item = context.getContextItem();
            if (item instanceof TinyElementImpl) {
                // fast path
                String val = ((TinyElementImpl) item).getAttributeValue(fingerprint);
                return val == null ? handleNullString(zeroLengthWhenAbsent) : val;
            }
            assert item instanceof NodeInfo;
            NodeInfo node = (NodeInfo) item;
            if (node.getNodeKind() == Type.ELEMENT) {
                String val = node.getAttributeValue(uri, local);
                if (val != null) {
                    return val;
                }
            }
            return handleNullString(zeroLengthWhenAbsent);
        };
    }

}

