package net.sf.saxon.test.matchers;

import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.s9api.XdmItem;
import net.sf.saxon.s9api.XdmNode;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.Type;
import org.hamcrest.Description;
import org.hamcrest.StringDescription;

public class SaxonStringDescription extends StringDescription {
    public Description appendValue(Object value) {
        if (value instanceof Sequence) {
            try {
                return super.appendValue(describeXdmObject((Sequence) value));
            } catch (XPathException e) {
                throw new RuntimeException(e);
            }
        }
        return super.appendValue(value);
    }

    private String describeXdmObject(Sequence value) throws XPathException {
        GroundedValue reduced = value.materialize().reduce();
        if (reduced instanceof NodeInfo) {
            return describeNode((NodeInfo) reduced);
        }
        return reduced.toShortString();
    }

    private String describeNode(NodeInfo node) {
        switch (node.getNodeKind()) {
            case Type.DOCUMENT:
            case Type.ELEMENT:
                return new XdmNode(node).toString();
            default:
                return node.toShortString();
        }
    }
}
