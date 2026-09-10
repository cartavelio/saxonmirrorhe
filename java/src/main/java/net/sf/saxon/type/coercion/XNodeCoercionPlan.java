package net.sf.saxon.type.coercion;

import net.sf.saxon.ma.Parcel;
import net.sf.saxon.ma.jnode.JNode;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.ItemType;
import net.sf.saxon.type.TypeHierarchy;
import net.sf.saxon.value.EmptySequence;
import net.sf.saxon.value.SequenceExtent;

import java.util.ArrayList;
import java.util.List;

/**
 * A coercion plan represents a process for implementing the coercion rules for converting
 * a supplied value to a given required type. This class handles the case where the required
 * type is xnode(). The only coercion available to XNode is from a JNode, in the case where
 * the content property of the XNode is a JNode
 */
public final class XNodeCoercionPlan extends CoercionPlan {

    public final static XNodeCoercionPlan INSTANCE = new XNodeCoercionPlan();

    public static XNodeCoercionPlan getInstance() {
        return INSTANCE;
    }

    /**
     * Apply the coercion rules (function conversion rules) to a value, given a required type.
     *
     * @param item the value to be converted
     * @param requiredType the required array type
     * @param request      the input to the coercion
     * @return the converted value (possibly as a lazily-evaluated sequence)
     */

    public GroundedValue coerceItem(Item item, ItemType requiredType, CoercionRequest request) throws XPathException {
        TypeHierarchy th = request.config.getTypeHierarchy();
        if (requiredType.matches(item, th)) {
            return item;
        }
        if (item instanceof JNode) {
            GroundedValue content = ((JNode)item).getContent();
            if (content.getLength() == 0) {
                return EmptySequence.getInstance();
            } else if (content.getLength() == 1) {
                if (content.itemAt(0) instanceof NodeInfo) {
                    return content;
                } else {
                    throw coercionError(item, requiredType, request);
                }
            } else {
                List<Item> result = new ArrayList<>();
                for (Item it : content.asIterable()) {
                    if (requiredType.matches(it, th)) {
                        return content;
                    } else {
                        throw coercionError(it, requiredType, request);
                    }
                }
                return SequenceExtent.makeSequenceExtent(result);
            }
        }
        if (item instanceof Parcel) {
            GroundedValue val = ((Parcel) item).getValue();
            for (Item it : val.asIterable()) {
                if (!requiredType.matches(it, th)) {
                    throw coercionError(item, requiredType, request);
                }
            }
            return val;
        }
        throw coercionError(item, requiredType, request);
    }


}


