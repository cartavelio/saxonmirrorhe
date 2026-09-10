package net.sf.saxon.ma.map;

import net.sf.saxon.type.Affinity;
import net.sf.saxon.type.PlainType;
import net.sf.saxon.type.Subsumption;
import net.sf.saxon.value.SequenceType;

public abstract class MapWithTypeCache extends MapItem {


    protected PlainType knownKeyType;
    protected SequenceType knownValueType;


    protected void setKnownType(PlainType keyType, SequenceType valueType) {
        this.knownKeyType = keyType;
        this.knownValueType = valueType;
    }

    protected void notifyPut() {}

    protected void notifyRemove(){}

    /**
     * Ask whether the map conforms to a given map type. This implementation performs
     * no optimization, it simply scans the entries in the map.
     *
     * @param keyType   the required keyType
     * @param valueType the required valueType
     * @return true if the map conforms to the required type
     */
    public boolean conforms(PlainType keyType, SequenceType valueType) {

        if (knownKeyType != null) {
            Affinity affinityKeyRelationship = Subsumption.computeRelationship(keyType, knownKeyType);
            boolean ok = false;

            ok = affinityKeyRelationship == Affinity.SAME_TYPE || affinityKeyRelationship == Affinity.SUBSUMED_BY;

            if(ok) {
                Affinity valueTypeRelationship = Subsumption.sequenceTypeRelationship(valueType, knownValueType);
                ok = valueTypeRelationship == Affinity.SAME_TYPE || valueTypeRelationship == Affinity.SUBSUMED_BY;
            }

            if(ok) {
                return true;
            }

        }
        for (KeyValuePair pair : keyValuePairs()) {
            if (!keyType.matches(pair.key)) {
                return false;
            }
            if (!valueType.matches(pair.value)) {
                return false;
            }
        }
        knownKeyType = keyType;
        knownValueType = valueType;
        return true;
    }

}
