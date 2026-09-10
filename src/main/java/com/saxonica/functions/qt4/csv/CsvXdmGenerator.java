package com.saxonica.functions.qt4.csv;

import net.sf.saxon.expr.Callable;
import net.sf.saxon.expr.ItemMappingIterator;
import net.sf.saxon.expr.StaticProperty;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.CallableFunction;
import net.sf.saxon.ma.arrays.ArrayItem;
import net.sf.saxon.ma.map.DictionaryMap;
import net.sf.saxon.ma.map.HashTrieMap;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.*;
import net.sf.saxon.trans.UncheckedXPathException;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.FunctionItemType;
import net.sf.saxon.type.LocalUnionType;
import net.sf.saxon.type.SpecificFunctionType;
import net.sf.saxon.value.*;

import java.math.BigInteger;
import java.util.*;
import java.util.concurrent.atomic.AtomicInteger;

/**
 * Handles generating a <code>parsed-csv-record-structure</code> from a sequence of array items
 * (<code>array(xs:string)*</code>).
 */
public class CsvXdmGenerator {
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
        item.initialPut("columns", EMPTY_COLUMNS_RECORD);
        item.initialPut("rows", EmptySequence.getInstance());
        EMPTY_CSV = HashTrieMap.copy(item);
    }

    private final Map<String, GroundedValue> options;
    private final boolean shouldExtractColumns;
    private final boolean shouldFilterColumns;
    private List<Integer> columnIndexes;
    private final NumberOfColumns numberOfColumnsMode;
    private int numberOfColumns = -1;

    /**
     * Represents how the number of columns will be calculated.
     */
    enum NumberOfColumns {
        /**
         * All columns should be included. (The number of columns may vary by row.)
         */
        ALL,
        /**
         * Number of columns is taken from the first row. (The number of columns will not vary)
         */
        FIRST_ROW,
        /**
         * Number of columns is explicitly specified by the caller. (The number of columns will not vary)
         */
        SPECIFIED
    }

    /**
     * Construct an instance using the passed in options Map.
     *
     * @param options options for the conversion as a map of String : Sequence pairs
     * @throws XPathException if there's a problem getting values from the options
     */
    public CsvXdmGenerator(Map<String, GroundedValue> options) throws XPathException {
        this.options = options;
        shouldExtractColumns = shouldExtractColumns();
        numberOfColumnsMode = numberOfColumnsMode();
        shouldFilterColumns = ((GroundedValue) options.getOrDefault("filter-columns", EmptySequence.getInstance())).getLength() > 0;
    }

    private Optional<Item> getNumberOfColumnsOpt() throws XPathException {
        return Optional.ofNullable(options.getOrDefault("number-of-columns", EmptySequence.getInstance()).head());
    }

    private int getNumberOfColumns() throws XPathException {
        AtomicValue number = getNumberOfColumnsOpt().get().atomize().head();
        return ((IntegerValue) number).asSubscript();
    }

    private int specifiedNumberOfColumns() throws XPathException {
        if (numberOfColumns == -1) {
            numberOfColumns = getNumberOfColumns();
        }
        return numberOfColumns;
    }

    private NumberOfColumns numberOfColumnsMode() throws XPathException {
        Optional<Item> opt = getNumberOfColumnsOpt();
        if (!opt.isPresent()) return NumberOfColumns.ALL;
        AtomicValue value = opt.get().atomize().head();
        if (value.getPrimitiveType() == BuiltInAtomicType.INTEGER) {
            return NumberOfColumns.SPECIFIED;
        } else {
            switch (value.getStringValue()) {
                case "first-row":
                    return NumberOfColumns.FIRST_ROW;
                default:
                    return NumberOfColumns.ALL;
            }
        }
    }

    private Item getColumnNamesOpt() throws XPathException {
        Optional<Item> opt = Optional.ofNullable(options.getOrDefault("column-names", EmptySequence.getInstance()).head());
        return opt.orElse(BooleanValue.FALSE);
    }

    private boolean shouldExtractColumns() throws XPathException {
        Item opt = getColumnNamesOpt();
        if (opt.getGenre() == Genre.MAP) return true;
        return opt.effectiveBooleanValue();
    }

    private List<Integer> getColumnFilter() {
        ArrayList<Integer> columnFilter = new ArrayList<>();
        SequenceTool.supply(options.getOrDefault("filter-columns", EmptySequence.getInstance()).iterate(), columnIndex -> {
            columnFilter.add(((IntegerValue) columnIndex).asSubscript());
        });
        return columnFilter;
    }

    private List<Integer> getColumnIndexes(ArrayItem rowArray) throws XPathException {
        if (columnIndexes != null) return columnIndexes;
        if (shouldFilterColumns) {
            columnIndexes = getColumnFilter();
        } else {
            int numberOfColumns;
            switch (numberOfColumnsMode) {
                case SPECIFIED:
                    numberOfColumns = specifiedNumberOfColumns();
                    break;
                case FIRST_ROW:
                    numberOfColumns = firstRowNumberOfColumns(rowArray);
                    break;
                default:
                    numberOfColumns = rowArray.arrayLength();
            }
            columnIndexes = new ArrayList<>();
            for (int i = 1; i <= numberOfColumns; i++) {
                columnIndexes.add(i);
            }
        }
        return columnIndexes;
    }

    /**
     * Generate a <code>parsed-csv-structure-record</code> from the provided sequence of rows
     * (<code>array(xs:string)*</code>.
     *
     * @param rows  the sequence of rows (ArrayItems)
     * @return the <code>parsed-csv-structure-record</code> MapItem
     * @throws XPathException
     */
    public MapItem fromRowSequence(Sequence rows) throws XPathException {
        if (shouldExtractColumns) {
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

    private GroundedValue rowAsSequence(Iterable<GroundedValue> row) {
        ArrayList<Item> members = new ArrayList<>();
        row.forEach(item -> members.add((Item) item));

        return SequenceExtent.makeSequenceExtent(members);
    }

    /**
     * Allows iteration via an Iterator over an underlying ArrayItem, but with the size of the resulting iterator fixed.
     * If the ArrayItem is bigger, the BoundedRowIterator will effectively truncate it, and if it's smaller, it will be
     * padded with empty string values up to the correct length.
     */
    static class BoundedRowIterator implements Iterator<GroundedValue> {
        private final Iterator<GroundedValue> rowIterator;
        private final int size;
        private int idx = 0;

        /**
         * Create an iterator of given size over the specified ArrayItem
         *
         * @param row  the ArrayItem to iterator over
         * @param size the size of the iterator
         */
        BoundedRowIterator(ArrayItem row, int size) {
            this.rowIterator = row.members().iterator();
            this.size = size;
        }

        @Override
        public boolean hasNext() {
            return idx < size;
        }

        @Override
        public GroundedValue next() {
            idx++;
            if (rowIterator.hasNext()) {
                return rowIterator.next();
            } else {
                return StringValue.EMPTY_STRING;
            }
        }
    }

    /**
     * Represents a single row in the CSV that has been explicitly bounded at a certain size (number of columns), that
     * can be iterated over as with any Iterable.
     */
    class BoundedRow implements Iterable<GroundedValue> {
        private final ArrayItem row;
        private final int size;

        /**
         * Create the row
         *
         * @param row  The underlying source ArrayItem
         * @param size the size (in columns) to constrain the row to
         */
        BoundedRow(ArrayItem row, int size) {
            this.row = row;
            this.size = size;
        }

        @Override
        public Iterator<GroundedValue> iterator() {
            return new BoundedRowIterator(row, size);
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
        Iterable<GroundedValue> filteredRow;
        if (shouldFilterColumns) {
            List<GroundedValue> filteredRowList = new ArrayList<>();
            IterationUtils.forEach(getColumnIndexes(row), oneBasedIndex -> {
                filteredRowList.add(row.get(oneBasedIndex - 1));
            });
            filteredRow = filteredRowList;
        } else {
            switch (numberOfColumnsMode) {
                case SPECIFIED:
                    filteredRow = new BoundedRow(row, specifiedNumberOfColumns());
                    break;
                case FIRST_ROW:
                    filteredRow = new BoundedRow(row, firstRowNumberOfColumns(row));
                    break;
                default:
                    filteredRow = row.members();
            }
        }

        rowMap.initialPut("fields", rowAsSequence(filteredRow));
        rowMap.initialPut("field", new CallableFunction(1, fieldGetter.bind(filteredRow), FIELD_FN_TYPE));

        return rowMap;
    }

    private int firstRowNumberOfColumns(ArrayItem row) {
        if (numberOfColumns == -1) {
            numberOfColumns = row.arrayLength();
        }
        return numberOfColumns;
    }

    private GroundedValue generateRows(SequenceIterator rows, MapItem columns) throws XPathException {
        UnboundFieldGetter fieldGetter = new UnboundFieldGetter(columns);
        try {
            return SequenceExtent.from(ItemMappingIterator.map(rows, row -> generateRowRecord((ArrayItem) row, fieldGetter)));
        } catch (UncheckedXPathException ex) {
            throw ex.getXPathException();
        }
    }

    private final static SequenceType FIELD_FN_ARG_TYPE = SequenceType.makeSequenceType(
            new LocalUnionType(BuiltInAtomicType.STRING, BuiltInAtomicType.INTEGER),
            StaticProperty.EXACTLY_ONE);
    private final static FunctionItemType FIELD_FN_TYPE = new SpecificFunctionType(
            new SequenceType[]{FIELD_FN_ARG_TYPE},
            SequenceType.OPTIONAL_STRING);

    /**
     * Generates the functions that fetch fields from a specific row. Wraps all the data common to all rows in the CSV
     * (the <code>csv-columns-record</code>, and provides a method to generate a function bound to a specific row.
     */
    private static class UnboundFieldGetter {
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

        /**
         * Generate a function that is bound to a specific row for inclusion in the <code>csv-row-record</code>
         *
         * @param fields the fields of the row
         * @return the new function instance
         */
        public FieldGetter bind(Iterable<GroundedValue> fields) {
            return new FieldGetter(this, fields);
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
    }

    /**
     * Binds an {@link UnboundFieldGetter} to a specific set of fields and acts as an XPath function so it can be called
     * by the user.
     */
    private static class FieldGetter implements Callable {
        private final UnboundFieldGetter fieldGetter;
        private final List<GroundedValue> fields;

        /**
         * Create a new instance by binding the UnboundFieldGetter to a row.
         *
         * @param fieldGetter the field getter for the CSV the row is part of.
         * @param iterable    the fields of the row.
         */
        public FieldGetter(UnboundFieldGetter fieldGetter, Iterable<GroundedValue> iterable) {
            this.fieldGetter = fieldGetter;
            fields = new ArrayList<>();
            iterable.forEach(fields::add);
        }

        /**
         * Call the Callable.
         *
         * @param context   the dynamic evaluation context
         * @param arguments the values of the arguments, supplied as Sequences.
         *                  <p>Generally it is advisable, if calling iterate() to process a supplied sequence, to
         *                  call it only once; if the value is required more than once, it should first be converted to
         *                  a {@link net.sf.saxon.om.GroundedValue} by calling the utility methd
         *                  SequenceTool.toGroundedValue().</p>
         *                  <p>If the expected value is a single item, the item should be obtained by calling
         *                  Sequence.head(): it cannot be assumed that the item will be passed as an instance of
         *                  {@link net.sf.saxon.om.Item} or {@link net.sf.saxon.value.AtomicValue}.</p>
         *                  <p>It is the caller's responsibility to perform any type conversions required
         *                  to convert arguments to the type expected by the callee. An exception is where this Callable
         *                  is explicitly an argument-converting wrapper around the original Callable.</p>
         * @return the result of the evaluation, in the form of a Sequence. It is the responsibility of the callee to
         *         ensure that the type of result conforms to the expected result type.
         * @throws net.sf.saxon.trans.XPathException if a dynamic error occurs during the evaluation of the expression
         */
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            AtomicValue key = arguments[0].head().atomize().head();
            if (key.getPrimitiveType() == BuiltInAtomicType.INTEGER) {
                return fieldGetter.get((Int64Value) key, fields);
            } else {
                return fieldGetter.get((StringValue) key, fields);
            }
        }

        /**
         * The value of toString() may be used to identify this function in diagnostics
         */
        public String toString() {
            return "csv-to-xdm.field";
        }
    }
}
