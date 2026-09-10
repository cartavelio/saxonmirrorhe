////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2020 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.type;

import net.sf.saxon.om.Genre;
import net.sf.saxon.om.Item;
import net.sf.saxon.type.coercion.ChoiceCoercionPlan;
import net.sf.saxon.type.coercion.CoercionPlan;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;
import java.util.function.Predicate;

/**
 * A class that represents a union type declared locally, for example using
 * the (Saxon-extension) item-type syntax union(a,b,c), or internally in Java code.
 */

public class ChoiceItemType<T extends ItemType> implements ItemType, ChoiceType {

    public static <T extends ItemType> ItemType makeChoiceItemType(List<T> memberTypes) {
        for (ItemType member : memberTypes) {
            if (member.getGenre() != Genre.ATOMIC) {
                return new ChoiceItemType<T>(memberTypes);
            }
        }
        List<AtomicType> atomicMemberTypes = new ArrayList<>();
        for (ItemType member : memberTypes) {
            if (member instanceof AtomicType) {
                atomicMemberTypes.add((AtomicType) member);
            } else if (member instanceof PlainType) {
                for (PlainType at : ((PlainType)member).getPlainMemberTypes()) {
                    if (at instanceof AtomicType) {
                        atomicMemberTypes.add((AtomicType) at);
                    } else {
                        return new ChoiceItemType<>(memberTypes);
                    }
                }
            }
        }
        return new LocalUnionType<>(atomicMemberTypes);
    }

    public static ChoiceItemType<ItemType> of(ItemType... memberTypes) {
        List<ItemType> members = Arrays.asList(memberTypes);
        return new ChoiceItemType<>(members);
    }

    protected final List<T> memberTypes;


    /**
     * Creates a new ChoiceItemType.
     *
     * @param memberTypes the atomic member types of the union
     */

    public ChoiceItemType(List<T> memberTypes) {
        this.memberTypes = memberTypes;
    }

    public List<T> getMemberTypes() {
        return memberTypes;
    }

    public Iterable<? extends ItemType> getAlternatives() {
        return getMemberTypes();
    }

    /**
     * Get the genre of this item
     *
     * @return the genre
     */
    @Override
    public Genre getGenre() {
        return Genre.ANY;
    }

    public String getDescription() {
        return toString();
    }


    /**
     * Ask whether this Simple Type is an atomic type
     *
     * @return false, this is not an atomic type
     */

    @Override
    public boolean isAtomicType() {
        return false;
    }

    /**
     * Ask whether this Union type is a "plain type", defined as a union
     * type whose member types are all atomic types or plain unions. That is,
     * it disallows unions that are derived by restriction from another union.
     * The significance of this is that an atomic value will never match
     * a non-plain union type
     */

    @Override
    public boolean isPlainType() {
        return true;
    }


    protected boolean someMemberTypeSatisfies(Predicate<? super ItemType> condition) {
        for (ItemType member : memberTypes) {
            if (condition.test(member)) {
                return true;
            }
        }
        return false;
    }

    /**
     * Determine whether this is a built-in type or a user-defined type
     */

    public boolean isBuiltInType() {
        return false;
    }

    /**
     * Determine whether this is a list type
     */

    public boolean isListType() {
        return false;
    }

    /**
     * Return true if this type is a union type (that is, if its variety is union)
     *
     * @return true for a union type
     */

    public boolean isUnionType() {
        return true;
    }

    /**
     * Get the corresponding {@link UType}. A UType is a union of primitive item
     * types.
     *
     * @return the smallest UType that subsumes this item type
     */
    @Override
    public UType getUType() {
        UType u = UType.VOID;
        for (ItemType at : memberTypes) {
            u = u.union(at.getUType());
        }
        return u;
    }

    /**
     * Get an alphabetic code representing the type, or at any rate, the nearest built-in type
     * from which this type is derived. The codes are designed so that for any two built-in types
     * A and B, alphaCode(A) is a prefix of alphaCode(B) if and only if A is a supertype of B.
     *
     * @return the alphacode for the nearest containing built-in type
     */
    @Override
    public String getBasicAlphaCode() {
        return "U";
    }

    /**
     * Test whether a given item conforms to this type
     * @param item    The item to be tested
     * @param th   The type hierarchy
     * @return true if the item is an instance of this type; false otherwise
     */
    @Override
    public boolean matches(Item item, TypeHierarchy th) {
        return someMemberTypeSatisfies(at -> at.matches(item, th));
    }

    /**
     * Method defined in ItemType: get a primitive supertype in the ItemType type hierarchy
     *
     * @return BuiltInAtomicType.ANY_ATOMIC
     */

    /*@NotNull*/
    @Override
    public ItemType getPrimitiveItemType() {
        return AnyItemType.getInstance();
    }

    /**
     * Method defined in ItemType: get a primitive supertype in the ItemType type hierarchy
     *
     * @return StandardNames.XS_ANY_ATOMIC_TYPE
     */

    @Override
    public int getPrimitiveType() {
        return Type.ITEM;
    }

    /*@NotNull*/
    @Override
    public PlainType getAtomizedItemType() {
        return BuiltInAtomicType.ANY_ATOMIC;
    }

    @Override
    public boolean isAtomizable(TypeHierarchy th) {
        return someMemberTypeSatisfies(t -> isAtomizable(th));
    }

    /**
     * Get the default priority when this ItemType is used as an XSLT pattern
     *
     * @return the default priority. For a union type this is defined as the product
     * of the default priorities of the member types. Because the priorities of member
     * types are in the range 0 to 1, their product is also in the range 0 to 1.
     */
    @Override
    public double getDefaultPriority() {
        double result = 1;
        for (ItemType t : memberTypes) {
             result *= t.getDefaultPriority();
        }
        return result;

    }

    /**
     * Produce a string representation of the type name. If the type is anonymous, an internally-allocated
     * type name will be returned.
     *
     * @return the name of the atomic type in the form Q{uri}local
     */

    public String toString() {
        StringBuilder builder = new StringBuilder("(");
        for (ItemType at : memberTypes) {
            builder.append(at.toString());
            builder.append("|");
        }
        builder.setLength(builder.length() - 1);
        builder.append(")");
        return builder.toString();
    }

    @Override
    public String toExportString() {
        return toString();
    }

    /**
     * Get the coercion plan for use when this type is the required type for (say) coercion
     * of arguments in a function call
     *
     * @param version the XPath language version (40 or 31)
     */
    @Override
    public CoercionPlan getCoercionPlan(int version) {
        return ChoiceCoercionPlan.INSTANCE40;
    }

}

// Copyright (c) 2004-2023 Saxonica Limited