package net.sf.saxon.ma.jnode;

import net.sf.saxon.om.Genre;
import net.sf.saxon.om.Item;
import net.sf.saxon.type.ItemType;
import net.sf.saxon.type.Type;
import net.sf.saxon.type.TypeHierarchy;
import net.sf.saxon.type.UType;
import net.sf.saxon.type.coercion.CoercionPlan;
import net.sf.saxon.type.coercion.JNodeCoercionPlan;
import net.sf.saxon.type.gnode.GNodeType;
import net.sf.saxon.value.SequenceType;

import java.util.Optional;

public abstract class JNodeType extends GNodeType {

    public SequenceType getValueType() {
        return SequenceType.ANY_SEQUENCE;
    }

    /**
     * Determine the Genre (top-level classification) of this type
     *
     * @return the Genre to which this type belongs, for example node or atomic value
     */
    @Override
    public Genre getGenre() {
        return Genre.JNODE;
    }

//    /**
//     * Test whether a given item conforms to this type
//     *
//     * @param item The item to be tested
//     * @param th   The type hierarchy cache. Currently used only when matching function items.
//     * @return true if the item is an instance of this type; false otherwise
//     */
//    @Override
//    public boolean matches(Item item) {
//        return item instanceof JNode && valueType.matches(((JNode)item).getContent());
//    }

    /**
     * Get the primitive item type corresponding to this item type. For item(),
     * this is Type.ITEM. For node(), it is Type.NODE. For specific node kinds,
     * it is the value representing the node kind, for example Type.ELEMENT.
     * For anyAtomicValue and union types it is Type.ATOMIC_VALUE. For numeric it is Type.NUMBER.
     * For other atomic types it is the primitive type as defined in XML Schema,
     * except that integer, xs:dayTimeDuration, and xs:yearMonthDuration
     * are considered to be primitive types.
     *
     * @return the corresponding primitive type
     */
    @Override
    public ItemType getPrimitiveItemType() {
        return AnyJNodeType.getInstance();
    }

    /**
     * Get the primitive type corresponding to this item type. For item(),
     * this is Type.ITEM. For node(), it is Type.NODE. For specific node kinds,
     * it is the value representing the node kind, for example Type.ELEMENT.
     * For anyAtomicValue it is BuiltInAtomicType.ANY_ATOMIC. For numeric it is Type.NUMBER.
     * For other atomic types it is the primitive type as defined in XML Schema,
     * except that INTEGER is considered to be a primitive type.
     *
     * @return the integer fingerprint of the corresponding primitive type
     */
    @Override
    public int getPrimitiveType() {
        return Type.JNODE;
    }

    /**
     * Get the corresponding {@link UType}. A UType is a union of primitive item
     * types.
     *
     * @return the smallest UType that subsumes this item type
     */
    @Override
    public UType getUType() {
        return UType.JNODE;
    }


    /**
     * Get the default priority when this ItemType is used as an XSLT pattern
     *
     * @return the default priority
     */
    @Override
    public double getDefaultPriority() {
        return 0;
    }

//    /**
//     * Get the item type of the atomic values that will be produced when an item
//     * of this type is atomized
//     *
//     * @return the best available item type of the atomic values that will be produced when an item
//     * of this type is atomized, or null if it is known that atomization will throw an error.
//     */
//    @Override
//    public PlainType getAtomizedItemType() {
//        return getValueType().getPrimaryType().getAtomizedItemType();
//    }

//    /**
//     * Ask whether values of this type are atomizable
//     *
//     * @param th the type hierarchy cache
//     * @return true if some or all instances of this type can be successfully atomized; false
//     * if no instances of this type can be atomized
//     */
//    @Override
//    public boolean isAtomizable(TypeHierarchy th) {
//        return getValueType().getPrimaryType().isAtomizable(th);
//    }

    /**
     * Get the coercion plan for use when this type is the required type for (say) coercion
     * of arguments in a function call
     *
     * @param version the XPath language version (40 or 31)
     */
    @Override
    public CoercionPlan getCoercionPlan(int version) {
        return JNodeCoercionPlan.getInstance();
    }

    /**
     * Get an alphabetic code representing the type, or at any rate, the nearest built-in type
     * from which this type is derived. The codes are designed so that for any two built-in types
     * A and B, alphaCode(A) is a prefix of alphaCode(B) if and only if A is a supertype of B.
     *
     * @return the alphacode for the nearest containing built-in type. For example: for xs:string
     * return "AS", for xs:boolean "AB", for node() "N", for element() "NE", for map(*) "FM", for
     * array(*) "FA".
     */
    @Override
    public String getBasicAlphaCode() {
        return "J";
    }

    /**
     * Get extra diagnostic information about why a supplied item does not conform to this
     * item type, if available. If extra information is returned, it should be in the form of a complete
     * sentence, minus the closing full stop. No information should be returned for obvious cases.
     *
     * @param item the item that doesn't match this type
     * @param th   the type hierarchy cache
     * @return optionally, a message explaining why the item does not match the type
     */
    @Override
    public Optional<String> explainMismatch(Item item, TypeHierarchy th) {
        return Optional.empty();
    }

//    /**
//     * Return a string representation of this ItemType.
//     *
//     * @return the string representation
//     */
//    @Override
//    public String toString() {
//        if (this == ANY_JNODE) {
//            return "jnode()";
//        } else {
//            return "jnode(" + valueType.toString() + ")";
//        }
//    }
}

