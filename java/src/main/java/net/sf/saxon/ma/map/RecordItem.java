package net.sf.saxon.ma.map;

import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.SequenceType;
import net.sf.saxon.value.StringValue;

/**
 * A Record represents the XDM 4.0 concept of a record: it is essentially a map with a type
 * annotation, which identifies its record type
 */
public class RecordItem extends MapItem {

    private final MapItem theMap;
    private final RecordType theRecordType;

    /**
     * Create a record based on an underlying map and a type annotation. The caller warrants that the
     * map conforms to the suppled record type.
     * @param map the underlying map
     * @param recordType the type annotation
     */

    public RecordItem(MapItem map, RecordType recordType) {
        this.theMap = map;
        this.theRecordType = recordType;
    }

    /**
     * Get the underlying map
     * @return  the underlying map
     */

    public MapItem getMap() {
        return theMap;
    }

    /**
     * Get the type annotation
     * @return the type annotation
     */

    public RecordType getRecordType() {
        return theRecordType;
    }

    /**
     * Get the value of the field with a given key, supplied as a general atomic value
     * @param key     the value of the key
     * @return the corresponding value, or null if the key is not defined for this record type
     */
    @Override
    public GroundedValue get(AtomicValue key) {
        return theMap.get(key);
    }

    /**
     * Get the value of the field with a given key, supplied as a Unicode string
     *
     * @param key the given key
     * @return the corresponding value, or null if the key is not defined for this record type
     */

    public GroundedValue getU(UnicodeString key) {
        return theMap.getU(key);
    }

    /**
     * Ask whether the field with a given key exists and is non-empty
     *
     * @param key the given key
     * @return the corresponding value
     */

    public boolean hasNonEmptyValue(UnicodeString key) {
        GroundedValue val = theMap.getU(key);
        return val != null && val.head() != null;
    }

    /**
     * Get the value of the field with a given key, supplied as a Unicode string,
     * throwing an exception if the key is not defined for this record type
     *
     * @param key the given key
     * @return the corresponding value
     * @throws XPathException if the key is not defined for this record type
     */

    public GroundedValue safeGet(UnicodeString key) throws XPathException {
        if (!theRecordType.isDefinedField(key)) {
            throw new XPathException("Key '" + key + "' is not a valid field name for this record type", "XPTY0004")
                    .asTypeError();
        }
        return theMap.getU(key);
    }

    @Override
    public int size() {
        return theMap.size();
    }

    @Override
    public Iterable<KeyValuePair> keyValuePairs() {
        return theMap.keyValuePairs();
    }

    @Override
    public MapItem put(AtomicValue key, GroundedValue value) {
        return theMap.put(key, value);
    }

    @Override
    public MapItem remove(AtomicValue key) {
        return theMap.remove(key);
    }

    /**
     * Create a new instance of the same record type in which a given field has a new value,
     * throwing an exception if the key is not a valid field for thie record type or if
     * the value does not conform to the required type of that field
     *
     * @param key the given field name
     * @param value the new value
     * @return a new record containing the new value
     * @throws XPathException if the key is not defined for this record type or the new value
     * is invalid for the specified field
     */

    public RecordItem safePut(String key, GroundedValue value) throws XPathException {
        SequenceType fieldType = theRecordType.getFieldType(key);
        if (fieldType == null) {
            throw new XPathException("Key " + key + " is not a valid field name for this record type");
        }
        if (!fieldType.matches(value)) {
            throw new XPathException("Value " + value + " is not a valid value for field " + key);
        }
        return new RecordItem(theMap.put(new StringValue(key), value), theRecordType);
    }
}

