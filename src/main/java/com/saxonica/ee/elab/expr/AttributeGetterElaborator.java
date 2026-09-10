package com.saxonica.ee.elab;

import com.saxonica.ee.elab.eval.StringEvaluator;
import com.saxonica.ee.elab.gen.StringElaborator;
import net.sf.saxon.expr.AttributeGetter;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.str.StringView;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.tree.tiny.TinyElementImpl;
import net.sf.saxon.type.Type;

public class AttributeGetterElaborator extends StringElaborator {

    public StringEvaluator getStringEvaluator(UnicodeString resultIfEmpty) {
        final AttributeGetter exp = (AttributeGetter)getExpression();
        final int fingerprint = exp.getAttributeName().getFingerprint();
        final String uri = exp.getAttributeName().getURI();
        final String local = exp.getAttributeName().getLocalPart();
        return context -> {
            Item item = context.getContextItem();
            if (item instanceof TinyElementImpl) {
                // fast path
                String val = ((TinyElementImpl) item).getAttributeValue(fingerprint);
                return val == null ? resultIfEmpty : StringView.tidy(val);
            }
            assert item instanceof NodeInfo;
            NodeInfo node = (NodeInfo) item;
            if (node.getNodeKind() == Type.ELEMENT) {
                String val = node.getAttributeValue(uri, local);
                return val == null ? resultIfEmpty : StringView.tidy(val);
            } else {
                return resultIfEmpty;
            }
        };
    }

}

