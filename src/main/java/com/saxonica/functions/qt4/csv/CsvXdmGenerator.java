package com.saxonica.functions.qt4.csv;

import net.sf.saxon.expr.ItemMappingIterator;
import net.sf.saxon.ma.arrays.ArrayItem;
import net.sf.saxon.ma.map.DictionaryMap;
import net.sf.saxon.ma.map.HashTrieMap;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.*;
import net.sf.saxon.trans.UncheckedXPathException;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.*;

import java.math.BigInteger;
import java.util.*;
import java.util.concurrent.atomic.AtomicInteger;

/**
 * Handles generating a <code>parsed-csv-record-structure</code> from a sequence of array items
 * (<code>array(xs:string)*</code>).
 */
public class CsvXdmGenerator extends CsvRepresentationGenerator {
    /**
     * Represents an empty <code>csv-columns-record</code>, used when a CSV has no column information.
     */
    public static MapItem EMPTY_COLUMNS_RECORD;
    static {
        DictionaryMap item = new DictionaryMap();
        item.initialPut("names", new HashTrieMap());
        item.initialPut("fields", EmptySequence.getInstance());
        EMPTY_COLUMNS_RECORD = HashTrieMap.copy(item);
    }

    /**
     * Represents an entirely empty CSV - no column information, and no rows of data.
     */
    public static MapItem EMPTY_CSV;
    static {
        DictionaryMap item = new DictionaryMap();
        item.initialPut("columns", CsvXdmGenerator.EMPTY_COLUMNS_RECORD);
        item.initialPut("rows", EmptySequence.getInstance());
        EMPTY_CSV = HashTrieMap.copy(item);
    }

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
     * @throws XPathException
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
        DictionaryMap csvRecord = new DictionaryMap();
        csvRecord.initialPut("columns", columns);
        csvRecord.initialPut("rows", generateRows(iter, columns));
        return csvRecord;
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
        IterationUtils.forEach(columnNamesMap.keyValuePairs(), kvPair -> {
            IntegerValue idx = (IntegerValue) kvPair.value.head();
            fieldsMap.put((int) idx.longValue(), kvPair.key);
        });

        int fieldsLength = Collections.max(fieldsMap.entrySet(), Comparator.comparingInt(Map.Entry::getKey)).getKey();

        for (int i = 1; i <= fieldsLength; i++) {
            fields.add(fieldsMap.getOrDefault(i, StringValue.EMPTY_STRING));
        }
        if (numberOfColumnsMode() == NumberOfColumns.FIRST_ROW) numberOfColumns = fieldsLength;
        DictionaryMap columns = new DictionaryMap();
        columns.initialPut("names", HashTrieMap.copy(columnNamesMap));
        columns.initialPut("fields", rowAsSequence(fields));
        return HashTrieMap.copy(columns);
    }

    private MapItem generateColumnsRecord(ArrayItem rowArray) throws XPathException {
        if (rowArray == null) return EMPTY_COLUMNS_RECORD;

        DictionaryMap columns = new DictionaryMap();
        DictionaryMap names = new DictionaryMap();
        ArrayList<GroundedValue> wantedFields = new ArrayList<>();
        getColumnIndexes(rowArray).forEach(oneBasedIndex -> wantedFields.add(rowArray.get(oneBasedIndex - 1)));
        AtomicInteger currentIndex = new AtomicInteger(1);
        IterationUtils.forEach(wantedFields, field -> {
            names.initialPut(field.getStringValue(), IntegerValue.makeIntegerValue(BigInteger.valueOf(currentIndex.get())));
            currentIndex.incrementAndGet();
        });
        columns.initialPut("names", HashTrieMap.copy(names));
        columns.initialPut("fields", rowAsSequence(wantedFields));
        return HashTrieMap.copy(columns);
    }

    private GroundedValue generateRows(SequenceIterator rows, MapItem columns) throws XPathException {
        UnboundFieldGetter fieldGetter = new UnboundFieldGetter(columns);
        try {
            return SequenceExtent.from(ItemMappingIterator.map(rows, row -> generateRowRecord((ArrayItem) row, fieldGetter)));
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
     * @throws XPathException
     */
    private MapItem generateRowRecord(ArrayItem row, UnboundFieldGetter fieldGetter) throws XPathException {
        DictionaryMap rowMap = new DictionaryMap();
        Iterable<GroundedValue> filteredRow = filteredRow(row);

        rowMap.initialPut("fields", rowAsSequence(filteredRow));
        rowMap.initialPut("field", fieldGetter.boundGetterFunction(filteredRow));

        return rowMap;
    }
}
