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

class CsvXdmGenerator {
    public static MapItem EMPTY_COLUMNS_RECORD;
    static {
        DictionaryMap item = new DictionaryMap();
        item.initialPut("names", new HashTrieMap());
        item.initialPut("fields", EmptySequence.getInstance());
        EMPTY_COLUMNS_RECORD = HashTrieMap.copy(item);
    }

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

    enum NumberOfColumns {
        ALL,
        FIRST_ROW,
        SPECIFIED
    }

    public CsvXdmGenerator(Map<String, GroundedValue> options) throws XPathException {
        this.options = options;
        shouldExtractColumns = shouldExtractColumns();
        numberOfColumnsMode = numberOfColumnsMode();
        shouldFilterColumns = ((GroundedValue) options.getOrDefault("filter-columns", EmptySequence.getInstance())).getLength() > 0;
    }

    private void verifyPositiveInteger(AtomicValue maybeNumber) throws XPathException {
        if (maybeNumber == null) return;
        if (maybeNumber.getPrimitiveType() != BuiltInAtomicType.INTEGER) throw new XPathException("Cannot supply non-integer value here", "FOCV0005");
        if (maybeNumber instanceof IntegerValue && ((IntegerValue) maybeNumber).asSubscript() == -1) throw new XPathException("Cannot supply zero or non-positive integer value here", "FOCV0005");
    }

    private void verifyPositiveInteger(int number) throws XPathException {
        if (number < 1) throw new XPathException("Cannot supply zero or non-positive integer value here", "FOCV0005");
    }

    private Long getNumberOfColumns() throws XPathException {
        Item numberOfColumns = options.getOrDefault("number-of-columns", EmptySequence.getInstance()).head();
        AtomicValue number = numberOfColumns.atomize().head();
        verifyPositiveInteger(number);
        return ((IntegerValue) number).longValue();
    }

    private Optional<Item> getNumberOfColumnsOpt() throws XPathException {
        return Optional.ofNullable(options.getOrDefault("number-of-columns", EmptySequence.getInstance()).head());
    }

    private int specifiedNumberOfColumns() throws XPathException {
        if (numberOfColumns == -1) {
            numberOfColumns = getNumberOfColumns().intValue();
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

        for (int i = 1; i <= fieldsLength; i ++) {
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

    static class BoundedRowIterator implements Iterator<GroundedValue> {
        private final Iterator<GroundedValue> rowIterator;
        private final int size;
        private int idx = 0;

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

    class BoundedRow implements Iterable<GroundedValue> {
        private final ArrayItem row;
        private final int size;

        BoundedRow(ArrayItem row, int size) {
            this.row = row;
            this.size = size;
        }

        @Override
        public Iterator<GroundedValue> iterator() {
            return new BoundedRowIterator(row, size);
        }
    }

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

    private static class UnboundFieldGetter {
        private final MapItem names;

        public UnboundFieldGetter(MapItem columns) {
            MapItem maybeNames = null;
            Item namesItem = columns.get(new StringValue("names")).head();
            if (namesItem.getGenre() == Genre.MAP) maybeNames = (MapItem) namesItem;
            this.names = maybeNames;
        }

        public FieldGetter bind(Iterable<GroundedValue> fields) {
            return new FieldGetter(this, fields);
        }

        public Sequence get(StringValue key, List<GroundedValue> fields) throws XPathException {
            Int64Value idx = (Int64Value) names.get(key);
            if (idx == null) throw new XPathException("Named column does not exist", "FOCV0004");
            return get(idx, fields);
        }

        public Sequence get(Int64Value idx, List<GroundedValue> fields) {
            // keys are 1-based indexes, while the array item here is 0-based
            // TODO: deal with asSubscript == -1, idx OOB
            int zeroBasedIndex = idx.asSubscript() - 1;
            if (zeroBasedIndex >= fields.size()) return StringValue.EMPTY_STRING;
            return fields.get(zeroBasedIndex);
        }
    }

    private static class FieldGetter implements Callable {
        private final UnboundFieldGetter fieldGetter;
        private final List<GroundedValue> fields;
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
         *                  call it only once; if the value is required more than once, it should first be converted
         *                  to a {@link net.sf.saxon.om.GroundedValue} by calling the utility methd
         *                  SequenceTool.toGroundedValue().</p>
         *                  <p>If the expected value is a single item, the item should be obtained by calling
         *                  Sequence.head(): it cannot be assumed that the item will be passed as an instance of
         *                  {@link net.sf.saxon.om.Item} or {@link net.sf.saxon.value.AtomicValue}.</p>
         *                  <p>It is the caller's responsibility to perform any type conversions required
         *                  to convert arguments to the type expected by the callee. An exception is where
         *                  this Callable is explicitly an argument-converting wrapper around the original
         *                  Callable.</p>
         * @return the result of the evaluation, in the form of a Sequence. It is the responsibility
         * of the callee to ensure that the type of result conforms to the expected result type.
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
