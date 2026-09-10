package net.sf.saxon.ma.map;

import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.SequenceTool;
import net.sf.saxon.tree.iter.AtomicIterator;
import net.sf.saxon.type.*;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.Cardinality;
import net.sf.saxon.value.SequenceType;

public abstract class MapTypeMaintainer extends MapItem {



    // The following values are maintained incrementally when
    // entries are added to the map. They are not changed when entries are removed,
    // so the actual type may be narrower than these values suggest. The purpose of
    // keeping this information is to try and avoid dynamic checking of the map
    // contents against a required type wherever possible. We therefore have
    // a guarantee that all entries in the map conform to the types maintained here;
    // but there is no guarantee that they do not also conform to some more specific
    // type.

    // The set of UTypes of keys in the map
    private UType keyUType = UType.VOID;

    // The set of UTypes of items in the values in the map
    private UType valueUType = UType.VOID;

    // The atomic type of all keys, if they are homogeneous; otherwise null
    private AtomicType keyAtomicType = ErrorType.getInstance();

    // The item type of all items in values, if they are homogeneous; otherwise null
    private ItemType valueItemType = ErrorType.getInstance();

    // The "envelope" cardinality of values in the map (0:1, 0:0, 1:1, 0:many, 1:many)
    private int valueCardinality = 0;

    /**
     * After adding an entry to the map, update the cached type information
     *
     * @param key      the new key
     * @param val      the new associated value
     * @param wasEmpty true if the map was empty before adding these values
     */

    protected void updateTypeInformation(AtomicValue key, Sequence val, boolean wasEmpty) {
//        if (Instrumentation.ACTIVE) {
//            Instrumentation.count("updateTypeInformation");
//        }
        if (wasEmpty) {
            keyUType = key.getUType();
            valueUType = SequenceTool.getUType(val);
            keyAtomicType = key.getItemType();
            valueItemType = MapItem.getItemTypeOfSequence(val);
            valueCardinality = SequenceTool.getCardinality(val);
        } else {
            keyUType = keyUType.union(key.getUType());
            valueUType = valueUType.union(SequenceTool.getUType(val));
            valueCardinality = Cardinality.union(valueCardinality, SequenceTool.getCardinality(val));
            if (key.getItemType() != keyAtomicType) {
                keyAtomicType = null;
            }
            if (!MapItem.isKnownToConform(val, valueItemType)) {
                valueItemType = null;
            }
        }
    }

    /**
     * Ask whether the map conforms to a given map type
     *
     * @param requiredKeyType   the required keyType
     * @param requiredValueType the required valueType
     * @param th                the type hierarchy cache for the configuration
     * @return true if the map conforms to the required type
     */
    @Override
    public boolean conforms(PlainType requiredKeyType, SequenceType requiredValueType, TypeHierarchy th) {
        if (isEmpty()) {
            return true;
        }
        if (keyAtomicType == requiredKeyType && valueItemType == requiredValueType.getPrimaryType() &&
                Cardinality.subsumes(requiredValueType.getCardinality(), valueCardinality)) {
            return true;
        }
        boolean needFullCheck = false;
        if (requiredKeyType != BuiltInAtomicType.ANY_ATOMIC) {
            ItemType upperBoundKeyType = keyUType.toItemType();
            Affinity rel = th.relationship(requiredKeyType, upperBoundKeyType);
            if (rel == Affinity.SAME_TYPE || rel == Affinity.SUBSUMES) {
                // The key type is matched
            } else if (rel == Affinity.DISJOINT) {
                return false;
            } else {
                needFullCheck = true;
            }
        }
        ItemType requiredValueItemType = requiredValueType.getPrimaryType();
        if (requiredValueItemType != BuiltInAtomicType.ANY_ATOMIC) {
            ItemType upperBoundValueType = valueUType.toItemType();
            Affinity rel = th.relationship(requiredValueItemType, upperBoundValueType);
            if (rel == Affinity.SAME_TYPE || rel == Affinity.SUBSUMES) {
                // The value type is matched
            } else if (rel == Affinity.DISJOINT) {
                return false;
            } else {
                needFullCheck = true;
            }
        }
        int requiredValueCard = requiredValueType.getCardinality();
        if (!Cardinality.subsumes(requiredValueCard, valueCardinality)) {
            needFullCheck = true;
        }

        if (needFullCheck) {
            // we need to test the entries individually
            AtomicIterator keyIter = keys();
            AtomicValue key;
            while ((key = keyIter.next()) != null) {
                if (!requiredKeyType.matches(key, th)) {
                    return false;
                }
                if (!requiredValueType.matches(get(key), th)) {
                    return false;
                }
            }
        }

        return true;

    }

    /**
     * Get the type of the map. This method is used largely for diagnostics, to report
     * the type of a map when it differs from the required type. This method has the side-effect
     * of updating the internal type information held within the map.
     *
     * @param th The type hierarchy cache for the configuration
     * @return the type of this map
     */
    @Override
    public MapType getItemType(TypeHierarchy th) {
        UType keyType = UType.VOID;
        UType valueType = UType.VOID;
        int valueCard = 0;
        // we need to test the entries individually
        AtomicIterator keyIter = keys();
        AtomicValue key;
        while ((key = keyIter.next()) != null) {
            GroundedValue val = get(key);
            keyType = keyType.union(key.getUType());
            valueType = valueType.union(SequenceTool.getUType(val));
            valueCard = Cardinality.union(valueCard, SequenceTool.getCardinality(val));
        }
        ItemType keyItemType = keyUType.toItemType();
        ItemType valueItemType = valueUType.toItemType();
        if (keyType == null) {
            // implies the map is empty
            return MapType.ANY_MAP_TYPE;
        } else {
            this.keyUType = keyType;
            this.valueUType = valueType;
            this.valueCardinality = valueCard;
            return new MapType((AtomicType) keyItemType, SequenceType.makeSequenceType(valueItemType, valueCard));
        }
    }

    /**
     * Get the lowest common item type of the keys in the map
     *
     * @return the most specific type to which all the keys belong. If the map is
     * empty, return UType.VOID (the type with no instances)
     */
    @Override
    public UType getKeyUType() {
        return keyUType;
    }

    protected void copyTypeInfoFrom(MapTypeMaintainer that) {
        valueCardinality = that.valueCardinality;
        keyUType = that.keyUType;
        valueUType = that.valueUType;
        keyAtomicType = that.keyAtomicType;
        valueItemType = that.valueItemType;
    }


}

