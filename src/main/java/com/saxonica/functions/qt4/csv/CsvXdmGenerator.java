package com.saxonica.functions.qt4.csv;

import net.sf.saxon.expr.Callable;
import net.sf.saxon.expr.StaticProperty;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.CallableFunction;
import net.sf.saxon.ma.arrays.ArrayItem;
import net.sf.saxon.ma.arrays.SimpleArrayItem;
import net.sf.saxon.ma.map.DictionaryMap;
import net.sf.saxon.ma.map.HashTrieMap;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.*;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.jiter.WrappingJavaIterator;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.FunctionItemType;
import net.sf.saxon.type.LocalUnionType;
import net.sf.saxon.type.SpecificFunctionType;
import net.sf.saxon.value.*;

import java.math.BigInteger;
import java.util.*;
import java.util.concurrent.atomic.AtomicInteger;
import java.util.stream.Collectors;
import java.util.stream.Stream;
import java.util.stream.StreamSupport;

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

    public CsvXdmGenerator(Map<String, GroundedValue> options) throws XPathException {
        this.options = options;
        shouldExtractColumns = shouldExtractColumns();
        shouldFilterColumns = ((GroundedValue) options.getOrDefault("filter-columns", EmptySequence.getInstance())).getLength() > 0;
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

    private List<Integer> getColumnFilter() throws XPathException {
        ArrayList<Integer> columnFilter = new ArrayList<>();
        for (GroundedValue columnIndex : options.getOrDefault("filter-columns", EmptySequence.getInstance()).materialize().asIterable()) {
            columnFilter.add(((IntegerValue) columnIndex).asSubscript());
        }
        return columnFilter;
    }

    public MapItem fromRowSequence(Sequence rows) throws XPathException {
        if (shouldExtractColumns) {
            SequenceIterator iter = rows.iterate();
            Optional<MapItem> columnsRecord = generateColumnsRecord(iter);
            if (!columnsRecord.isPresent()) return EMPTY_CSV;
            return generateCsvRecord(columnsRecord.get(), iter);
        } else {
            return generateCsvRecord(EMPTY_COLUMNS_RECORD, rows.iterate());
        }
    }

    private MapItem generateCsvRecord(MapItem columns, SequenceIterator iter) {
        DictionaryMap csvRecord = new DictionaryMap();
        csvRecord.initialPut("columns", columns);
        csvRecord.initialPut("rows", generateRows(iter, columns));
        return csvRecord;
    }

    private List<Integer> getColumnIndexes(ArrayItem rowArray) throws XPathException {
        if (columnIndexes != null) return columnIndexes;
        if (shouldFilterColumns) {
            columnIndexes = getColumnFilter();
        } else {
            columnIndexes = Stream.iterate(1, n -> n + 1)
                    .limit(rowArray.arrayLength())
                    .collect(Collectors.toList());
        }
        return columnIndexes;
    }

    private Optional<MapItem> generateColumnsRecord(SequenceIterator rowsIter) throws XPathException {
        Item columnNamesOpt = getColumnNamesOpt();
        if (columnNamesOpt.getGenre() == Genre.MAP) {
            return generateColumnsRecord((MapItem) columnNamesOpt);
        }
        ArrayItem headerRow = (ArrayItem) rowsIter.next();
        return generateColumnsRecord(headerRow);
    }

    private Optional<MapItem> generateColumnsRecord(MapItem columnNamesMap) {
        List<GroundedValue> fields = new ArrayList<>();
        Map<Integer, GroundedValue> fieldsMap = new HashMap<>();
        columnNamesMap.keyValuePairs().forEach(kvPair -> {
            try {
                fieldsMap.put((int) ((IntegerValue) kvPair.value.head()).longValue(), kvPair.key);
            } catch (XPathException ex) {
                throw new RuntimeException(ex);
            }
        });

        int fieldsLength = Collections.max(fieldsMap.entrySet(), Comparator.comparingInt(Map.Entry::getKey)).getKey();

        for (int i = 0; i < fieldsLength; i ++) {
            fields.add(fieldsMap.getOrDefault(i + 1, new StringValue("")));
        }

        DictionaryMap columns = new DictionaryMap();
        columns.initialPut("names", HashTrieMap.copy(columnNamesMap));
        columns.initialPut("fields", rowAsSequence(fields));
        return Optional.of(HashTrieMap.copy(columns));
    }

    private Optional<MapItem> generateColumnsRecord(ArrayItem rowArray) throws XPathException {
        if (rowArray == null) return Optional.empty();

        DictionaryMap columns = new DictionaryMap();
        DictionaryMap names = new DictionaryMap();
        ArrayList<GroundedValue> wantedFields = new ArrayList<>();
        getColumnIndexes(rowArray).forEach(oneBasedIndex -> wantedFields.add(rowArray.get(oneBasedIndex - 1)));
        AtomicInteger currentIndex = new AtomicInteger(1);
        wantedFields.forEach(field -> {
            try {
                names.initialPut(field.getStringValue(), IntegerValue.makeIntegerValue(BigInteger.valueOf(currentIndex.get())));
            } catch (XPathException e) {
                throw new RuntimeException(e);
            }
            currentIndex.incrementAndGet();
        });
        columns.initialPut("names", HashTrieMap.copy(names));
        columns.initialPut("fields", rowAsSequence(wantedFields));
        return Optional.of(HashTrieMap.copy(columns));
    }

    private GroundedValue rowAsSequence(Iterable<GroundedValue> row) {
        ArrayList<Item> members = new ArrayList<>();
        row.forEach(item -> members.add((Item) item));

        return SequenceExtent.makeSequenceExtent(members);
    }

    private MapItem generateRowRecord(ArrayItem row, UnboundFieldGetter fieldGetter) {
        DictionaryMap rowMap = new DictionaryMap();
        Iterable<GroundedValue> filteredRow;
        if (shouldFilterColumns) {
            try {
                List<GroundedValue> filteredRowList = new ArrayList<>();
                getColumnIndexes(row).forEach(oneBasedIndex -> filteredRowList.add(row.get(oneBasedIndex - 1)));
                filteredRow = filteredRowList;
            } catch (XPathException ex) {
                throw new RuntimeException(ex);
            }
        } else {
            filteredRow = row.members();
        }
        rowMap.initialPut("fields", rowAsSequence(filteredRow));
        rowMap.initialPut("field", new CallableFunction(1, fieldGetter.bind(filteredRow), FIELD_FN_TYPE));

        return rowMap;
    }

    private GroundedValue generateRows(SequenceIterator rows, MapItem columns) {
        UnboundFieldGetter fieldGetter = new UnboundFieldGetter(columns);
        Spliterator<ArrayItem> rowsSpliterator = Spliterators.spliteratorUnknownSize(new WrappingJavaIterator<>(rows), Spliterator.ORDERED);
        return SequenceExtent.from(new IteratorWrapper(StreamSupport.stream(rowsSpliterator, false).map(row -> generateRowRecord(row, fieldGetter)).iterator()));
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
            if (zeroBasedIndex >= fields.size()) return new StringValue("");
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
