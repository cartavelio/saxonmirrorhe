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

    public RecordItem(MapItem map, RecordType recordType) {
        this.theMap = map;
        this.theRecordType = recordType;
    }

    public MapItem getMap() {
        return theMap;
    }
    public RecordType getRecordType() {
        return theRecordType;
    }

    @Override
    public GroundedValue get(AtomicValue key) {
        return theMap.get(key);
    }

    public GroundedValue getU(UnicodeString key) {
        return theMap.getU(key);
    }

    public GroundedValue safeGet(String key) throws XPathException {
        if (theRecordType.getFieldType(key) == null) {
            throw new XPathException("Key '" + key + "' is not a valid field name for this record type", "XPTY0004")
                    .asTypeError();
        }
        return theMap.get(new StringValue(key));
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

