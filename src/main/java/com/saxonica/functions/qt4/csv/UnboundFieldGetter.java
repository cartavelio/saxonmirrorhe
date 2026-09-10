package com.saxonica.functions.qt4.csv;

import net.sf.saxon.functions.CallableFunction;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.Genre;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.Int64Value;
import net.sf.saxon.value.StringValue;

import java.util.List;

/**
 * Generates the functions that fetch fields from a specific row. Wraps all the data common to all rows in the CSV (the
 * <code>csv-columns-record</code>, and provides a method to generate a function bound to a specific row.
 */
class UnboundFieldGetter {
    private final MapItem names;

    /**
     * @param columns The <code>csv-columns-record</code> containing column names
     */
    public UnboundFieldGetter(MapItem columns) {
        MapItem maybeNames = null;
        Item namesItem = columns.get(new StringValue("names")).head();
        if (namesItem.getGenre() == Genre.MAP) maybeNames = (MapItem) namesItem;
        this.names = maybeNames;
    }

    public FieldGetter bind(Iterable<GroundedValue> fields) {
        return new FieldGetter(this, fields);
    }

    /**
     * Generate a function that is bound to a specific row for inclusion in the <code>csv-row-record</code>
     *
     * @param fields the fields of the row
     * @return the new function instance
     */
    public CallableFunction boundGetterFunction(Iterable<GroundedValue> fields) {
        return FieldGetter.createBoundFunction(this, fields);
    }

    /**
     * Fetch a field value using name lookup
     *
     * @param key    the column name
     * @param fields the fields to lookup within
     * @return the field value
     * @throws XPathException if the named column does not exist
     */
    public Sequence get(StringValue key, List<GroundedValue> fields) throws XPathException {
        Int64Value idx = (Int64Value) names.get(key);
        if (idx == null) throw new XPathException("Named column does not exist", "FOCV0004");
        return get(idx, fields);
    }

    /**
     * Fetch a field value using name lookup
     *
     * @param idx    the index of the column to fetch (zero-based)
     * @param fields the fields to lookup within
     * @return the field value
     * @throws XPathException if the named column does not exist
     */
    public Sequence get(Int64Value idx, List<GroundedValue> fields) {
        // keys are 1-based indexes, while the array item here is 0-based
        // TODO: deal with asSubscript == -1, idx OOB
        int zeroBasedIndex = idx.asSubscript() - 1;
        if (zeroBasedIndex >= fields.size()) return StringValue.EMPTY_STRING;
        return fields.get(zeroBasedIndex);
    }

    public Sequence get(AtomicValue key, List<GroundedValue> fields) throws XPathException {
        if (key.getItemType() == BuiltInAtomicType.INTEGER) {
            return get((Int64Value) key, fields);
        }
        return get((StringValue) key, fields);
    }
}
