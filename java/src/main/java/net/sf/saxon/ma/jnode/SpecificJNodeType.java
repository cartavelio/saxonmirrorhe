package net.sf.saxon.ma.jnode;

import net.sf.saxon.om.Item;
import net.sf.saxon.type.PlainType;
import net.sf.saxon.type.TypeHierarchy;
import net.sf.saxon.value.SequenceType;

import java.util.Optional;

/**
 * Represents the type jnode(T) where T is a sequence type giving the type
 * of the JNode content.
 */

public class SpecificJNodeType extends JNodeType {

    private final SequenceType valueType;
    private final TypeHierarchy typeHierarchy;


    public SpecificJNodeType(SequenceType valueType, TypeHierarchy th) {
        this.valueType = valueType;
        this.typeHierarchy = th;
    }

    public SequenceType getValueType() {
        return valueType;
    }


    /**
     * Test whether a given item conforms to this type
     *
     * @param item The item to be tested
     * @return true if the item is an instance of this type; false otherwise
     */
    @Override
    public boolean matches(Item item) {
        return item instanceof JNode && valueType.matches(((JNode)item).getContent(), typeHierarchy);
    }

    /**
     * Get the item type of the atomic values that will be produced when an item
     * of this type is atomized
     *
     * @return the best available item type of the atomic values that will be produced when an item
     * of this type is atomized, or null if it is known that atomization will throw an error.
     */
    @Override
    public PlainType getAtomizedItemType() {
        return getValueType().getPrimaryType().getAtomizedItemType();
    }

    /**
     * Ask whether values of this type are atomizable
     *
     * @param th the type hierarchy cache
     * @return true if some or all instances of this type can be successfully atomized; false
     * if no instances of this type can be atomized
     */
    @Override
    public boolean isAtomizable(TypeHierarchy th) {
        return getValueType().getPrimaryType().isAtomizable(th);
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

    /**
     * Return a string representation of this ItemType.
     *
     * @return the string representation
     */
    @Override
    public String toString() {
        return "jnode(" + valueType.toString() + ")";
    }
}

