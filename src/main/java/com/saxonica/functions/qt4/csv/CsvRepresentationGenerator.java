package com.saxonica.functions.qt4.csv;

import net.sf.saxon.expr.ItemMapper;
import net.sf.saxon.ma.arrays.ArrayItem;
import net.sf.saxon.ma.arrays.SimpleArrayItem;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.*;

import java.util.ArrayList;
import java.util.List;
import java.util.Map;

public abstract class CsvRepresentationGenerator {
    protected final Map<String, GroundedValue> options;
    protected boolean extractHeaderRow = false;
    protected final boolean trimRows;

    protected GroundedValue explicitColumnNames = null;

    protected ItemMapper.Lambda rowAdjuster;

    public CsvRepresentationGenerator(Map<String, GroundedValue> options) throws XPathException {
        this.options = options;
        GroundedValue selectColumns = options.get("select-columns");
        if (selectColumns != null && selectColumns.head() != null) {
            rowAdjuster = makeRowAdjuster(selectColumns);
        }
        GroundedValue headerOption = options.get("header");
        if (headerOption != null) {
            if (headerOption instanceof BooleanValue) {
                extractHeaderRow = ((BooleanValue)headerOption).getBooleanValue();
            } else {
                explicitColumnNames = headerOption;
                extractHeaderRow = false;
            }
        }
        trimRows = getTrimRowsOption();
    }

    /**
     * Make a mapping function that rearranges the fields of a row as required
     * by the {@code select-columns} option
     * @param selectColumns the value of the select-columns option: a sequence of integer
     *                      values. The Nth integer indicates the column number (1-based)
     *                      that maps to the Nth column (also 1-based) in the result
     * @return a mapping function that rearranges the fields in a row
     * @throws XPathException if any integer in the select-columns value is out of range
     */

    private ItemMapper.Lambda makeRowAdjuster(GroundedValue selectColumns)
            throws XPathException {
        if (selectColumns instanceof IntegerRange && ((IntegerRange)selectColumns).getStart() == 1) {
            // Optimization for a case like select-columns: 1 to 5
            int width = (int)((IntegerRange)selectColumns).getEnd();
            return makeRowTrimmer(width);
        } else {
            int[] selection = new int[selectColumns.getLength()];
            for (int i=0; i<selection.length; i++) {
                selection[i] = (int)((IntegerValue)selectColumns.itemAt(i)).longValue() - 1;
            }
            return item -> {
                ArrayItem row = (ArrayItem)item;
                List<GroundedValue> resultList = new ArrayList<>(selection.length);
                for (int i : selection) {
                    resultList.add(i < row.arrayLength()
                                           ? row.get(i)
                                           : StringValue.EMPTY_STRING);
                }
                return new SimpleArrayItem(resultList);
            };
        }
    }

    /**
     * Make a mapping function that adjusts an array item representing
     * one CSV row by truncating excess fields or adding empty fields
     * as required
     * @param width the required number of fields in each row
     * @return a mapping function that can be used to adjust the rows as required
     */
    protected ItemMapper.Lambda makeRowTrimmer(int width) {
        return item -> {
            ArrayItem row = (ArrayItem)item;
            if (width == row.arrayLength()) {
                return row;
            } else if (width > row.arrayLength()) {
                while (width > row.arrayLength()) {
                    row = row.append(StringValue.EMPTY_STRING);
                }
                return row;
            } else {
                return row.subArray(0, width);
            }
        };
    }

    private boolean getTrimRowsOption() {
        BooleanValue value = (BooleanValue)options.getOrDefault("trim-rows", BooleanValue.FALSE).head();
        return value.getBooleanValue();
    }

    /**
     * Generate the item to be delivered as the function result, given the sequence
     * of rows returned by the CSV parser
     * @param rows the rows returned by the CSV parser, as a sequence of XDM arrays of XDM strings
     * @return the data as appropriate to each subclass
     * @throws XPathException if anything goes wrong
     */
    public abstract Item fromRowSequence(SequenceIterator rows) throws XPathException;

    /**
     * Extract the list of column names from an array item holding the contents of the
     * first row, selecting and reordering the names if required
     *
     * @param firstRow the first row of the data, containing column names
     * @return a list of column names in the order that the columns appear in the output
     */
    protected List<String> generateColumnNames(ArrayItem firstRow) {
        List<String> result = new ArrayList<>(firstRow.arrayLength());
        for (GroundedValue value : firstRow.members()) {
            result.add(((StringValue) value).getStringValue());
        }
        return result;
    }


}
