package com.saxonica.functions.qt4.csv;

import net.sf.saxon.expr.ItemMappingIterator;
import net.sf.saxon.ma.arrays.ArrayItem;
import net.sf.saxon.ma.map.*;
import net.sf.saxon.om.*;
import net.sf.saxon.str.Twine8;
import net.sf.saxon.trans.UncheckedXPathException;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.*;

import java.util.*;

/**
 * Handles generating a <code>parsed-csv-record-structure</code> from a sequence of array items
 * (<code>array(xs:string)*</code>).
 */
public class CsvXdmGenerator extends CsvRepresentationGenerator {

    private static final Shape COLUMNS_RECORD_SHAPE = new Shape(new Twine8("names"), new Twine8("fields"));
    private static final Shape ROW_RECORD_SHAPE = new Shape(new Twine8("fields"), new Twine8("field"));
    private static final Shape RESULT_RECORD_SHAPE = new Shape(new Twine8("columns"), new Twine8("rows"));
    /**
     * Represents an empty <code>csv-columns-record</code>, used when a CSV has no column information.
     */
    public static MapItem EMPTY_COLUMNS_RECORD =
            new ShapedMap(COLUMNS_RECORD_SHAPE, new HashTrieMap(), EmptySequence.getInstance());


    /**
     * Represents an entirely empty CSV - no column information, and no rows of data.
     */
    public static MapItem EMPTY_CSV =
            new ShapedMap(RESULT_RECORD_SHAPE, EMPTY_COLUMNS_RECORD, EmptySequence.getInstance());

    /**
     * Construct an instance using the passed in options Map.
     *
     * @param options options for the conversion as a map of String : Sequence pairs
     * @throws XPathException if there's a problem getting values from the options
     */

    public CsvXdmGenerator(Map<String, GroundedValue> options) throws XPathException {
        super(options);
    }

    /**
     * Generate a <code>parsed-csv-structure-record</code> from the provided sequence of rows
     * (<code>array(xs:string)*</code>.
     *
     * @param rows  the sequence of rows (ArrayItems)
     * @return the <code>parsed-csv-structure-record</code> MapItem
     * @throws XPathException  if it fails
     */
    @Override
    public MapItem fromRowSequence(Sequence rows) throws XPathException {
        if (_shouldExtractColumns) {
            SequenceIterator iter = rows.iterate();
            MapItem columnsRecord = generateColumnsRecord(iter);
            return generateCsvRecord(columnsRecord, iter);
        } else {
            return generateCsvRecord(EMPTY_COLUMNS_RECORD, rows.iterate());
        }
    }

    private MapItem generateCsvRecord(MapItem columns, SequenceIterator iter) throws XPathException {
        return new ShapedMap(RESULT_RECORD_SHAPE, columns, generateRows(iter, columns));
    }

    private MapItem generateColumnsRecord(SequenceIterator rowsIter) throws XPathException {
        Item columnNamesOpt = getColumnNamesOpt();
        if (columnNamesOpt.getGenre() == Genre.MAP) {
            return generateColumnsRecord((MapItem) columnNamesOpt);
        }
        ArrayItem headerRow = (ArrayItem) rowsIter.next();
        return generateColumnsRecord(headerRow);
    }

    private MapItem generateColumnsRecord(MapItem columnNamesMap) throws XPathException {
        List<GroundedValue> fields = new ArrayList<>();
        Map<Integer, GroundedValue> fieldsMap = new HashMap<>();
        int fieldsLength = 0;
        for (KeyValuePair kvPair : columnNamesMap.keyValuePairs()) {
            int idx = (int)((IntegerValue) kvPair.value.head()).longValue();
            if (idx > fieldsLength) {
                fieldsLength = idx;
            }
            fieldsMap.put(idx, kvPair.key);
        }

        for (int i = 1; i <= fieldsLength; i++) {
            fields.add(fieldsMap.getOrDefault(i, StringValue.EMPTY_STRING));
        }
        if (numberOfColumnsMode() == NumberOfColumns.FIRST_ROW) {
            numberOfColumns = fieldsLength;
        }
        return new ShapedMap(COLUMNS_RECORD_SHAPE, columnNamesMap, rowAsSequence(fields));
    }

    private MapItem generateColumnsRecord(ArrayItem rowArray) throws XPathException {
        if (rowArray == null) {
            return EMPTY_COLUMNS_RECORD;
        }
        DictionaryMap names = new DictionaryMap();
        ArrayList<GroundedValue> wantedFields = new ArrayList<>();
        for (int index : getColumnIndexes(rowArray)) {
            wantedFields.add(rowArray.get(index - 1));
        }
        int currentIndex = 1;
        for (GroundedValue field : wantedFields) {
            names.initialPut(field.getStringValue(), Int64Value.makeIntegerValue(currentIndex++));
        }
        return new ShapedMap(COLUMNS_RECORD_SHAPE, names, rowAsSequence(wantedFields));
    }

    private GroundedValue generateRows(SequenceIterator rows, MapItem columns) throws XPathException {
        UnboundFieldGetter fieldGetter = new UnboundFieldGetter(columns);
        try {
            return SequenceExtent.from(
                    ItemMappingIterator.map(rows, row -> generateRowRecord((ArrayItem) row, fieldGetter)));
        } catch (UncheckedXPathException ex) {
            throw ex.getXPathException();
        }
    }

    /**
     * Generates the <code>csv-row-record</code> XDM structure for a single row.
     *
     * @param row         The source row.
     * @param fieldGetter The unbound field getter for the CSV this row is part of.
     * @return The resulting <code>csv-row-record</code>
     * @throws XPathException if it fails
     */
    private MapItem generateRowRecord(ArrayItem row, UnboundFieldGetter fieldGetter) throws XPathException {
        Iterable<GroundedValue> theFilteredRow = filteredRow(row);
        return new ShapedMap(ROW_RECORD_SHAPE,
                             rowAsSequence(theFilteredRow),
                             fieldGetter.boundGetterFunction(theFilteredRow));
    }


}
