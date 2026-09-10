package com.saxonica.functions.qt4.csv;

import net.sf.saxon.expr.ItemMappingIterator;
import net.sf.saxon.ma.arrays.ArrayItem;
import net.sf.saxon.ma.map.DictionaryMap;
import net.sf.saxon.ma.map.HashTrieMap;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.*;
import net.sf.saxon.trans.UncheckedXPathException;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.value.*;

import java.util.*;

public abstract class CsvRepresentationGenerator {
    protected final Map<String, GroundedValue> options;
    protected final boolean shouldExtractColumns;
    protected final boolean shouldFilterColumns;
    protected final NumberOfColumns numberOfColumnsMode;
    protected List<Integer> columnIndexes;
    protected int numberOfColumns = -1;

    public CsvRepresentationGenerator(Map<String, GroundedValue> options) throws XPathException {
        this.options = options;
        shouldFilterColumns = (options.getOrDefault("filter-columns", EmptySequence.getInstance())).getLength() > 0;
        numberOfColumnsMode = numberOfColumnsMode();
        shouldExtractColumns = shouldExtractColumns();
    }

    private boolean shouldExtractColumns() throws XPathException {
        Item opt = getColumnNamesOpt();
        if (opt.getGenre() == Genre.MAP) return true;
        return opt.effectiveBooleanValue();
    }

    private Optional<Item> getNumberOfColumnsOpt() throws XPathException {
        return Optional.ofNullable(options.getOrDefault("number-of-columns", EmptySequence.getInstance()).head());
    }

    private int getNumberOfColumns() throws XPathException {
        AtomicValue number = getNumberOfColumnsOpt().get().atomize().head();
        return ((IntegerValue) number).asSubscript();
    }

    protected int specifiedNumberOfColumns() throws XPathException {
        if (numberOfColumns == -1) {
            numberOfColumns = getNumberOfColumns();
        }
        return numberOfColumns;
    }

    protected NumberOfColumns numberOfColumnsMode() throws XPathException {
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

    protected Item getColumnNamesOpt() throws XPathException {
        Optional<Item> opt = Optional.ofNullable(options.getOrDefault("column-names", EmptySequence.getInstance()).head());
        return opt.orElse(BooleanValue.FALSE);
    }

    private List<Integer> getColumnFilter() {
        ArrayList<Integer> columnFilter = new ArrayList<>();
        SequenceTool.supply(options.getOrDefault("filter-columns", EmptySequence.getInstance()).iterate(), columnIndex -> {
            columnFilter.add(((IntegerValue) columnIndex).asSubscript());
        });
        return columnFilter;
    }

    protected List<Integer> getColumnIndexes(ArrayItem rowArray) throws XPathException {
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

    protected int firstRowNumberOfColumns(ArrayItem row) {
        if (numberOfColumns == -1) {
            numberOfColumns = row.arrayLength();
        }
        return numberOfColumns;
    }

    protected Iterable<GroundedValue> filteredRow(ArrayItem row) throws XPathException {
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
        return filteredRow;
    }

    public abstract Item fromRowSequence(Sequence rows) throws XPathException;

    protected GroundedValue rowAsSequence(Iterable<GroundedValue> row) {
        ArrayList<Item> members = new ArrayList<>();
        row.forEach(item -> members.add((Item) item));

        return SequenceExtent.makeSequenceExtent(members);
    }

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
}
