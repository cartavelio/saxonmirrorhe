package com.saxonica.functions.qt4.csv;

import net.sf.saxon.expr.ItemMappingIterator;
import net.sf.saxon.functions.SimpleBinaryFunction;
import net.sf.saxon.ma.arrays.ArrayItem;
import net.sf.saxon.ma.map.*;
import net.sf.saxon.om.FunctionItem;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.str.Twine8;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.UncheckedXPathException;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.PrependSequenceIterator;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.LocalUnionType;
import net.sf.saxon.value.*;

import java.util.HashMap;
import java.util.List;
import java.util.Map;

/**
 * Handles generating a <code>parsed-csv-record-structure</code> from a sequence of array items
 * (<code>array(xs:string)*</code>).
 */
public class CsvRecordGenerator extends CsvRepresentationGenerator {

    private static final Shape RESULT_RECORD_SHAPE = new Shape(
            new Twine8("columns"),
            new Twine8("column-index"),
            new Twine8("rows"),
            new Twine8("get"));

    /**
     * Represents an entirely empty CSV - no column information, and no rows of data.
     */
    public static MapItem EMPTY_CSV =
            new ShapedMap(RESULT_RECORD_SHAPE,
                    EmptySequence.getInstance(),
                    EmptyMap.getInstance(),
                    EmptySequence.getInstance(),
                    makeGetterFunction(EmptySequence.getInstance(), null)); // "get" function

    /**
     * Construct an instance using the passed in options Map.
     *
     * @param options options for the conversion as a map of String : Sequence pairs
     * @throws XPathException if there's a problem getting values from the options
     */

    public CsvRecordGenerator(Map<String, GroundedValue> options) throws XPathException {
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
    public MapItem fromRowSequence(SequenceIterator rows) throws XPathException {

        ArrayItem firstRow = (ArrayItem)rows.next();
        if (firstRow == null) {
            if (explicitColumnNames != null) {
                new ShapedMap(RESULT_RECORD_SHAPE,
                              explicitColumnNames,
                              EmptyMap.getInstance(),
                              EmptySequence.getInstance(),
                              makeGetterFunction(EmptySequence.getInstance(), null));
            }
            return EMPTY_CSV;
        }
        if (rowAdjuster == null) {
            if (trimRows) {
                rowAdjuster = makeRowTrimmer(firstRow.arrayLength());
            } else {
                rowAdjuster = row -> row;
            }
        }
        if (extractHeaderRow) {
            GroundedValue returnedRows = generateRows(rows);
            firstRow = (ArrayItem)rowAdjuster.map(firstRow);
            List<StringValue> columnNames = generateColumnNames(firstRow);
            MapItem columnIndex = makeColumnIndex(columnNames);

            return new ShapedMap(RESULT_RECORD_SHAPE,
                                 SequenceExtent.makeSequenceExtent(columnNames),
                                 columnIndex,
                                 returnedRows,
                                 makeGetterFunction(returnedRows, columnIndex));
        } else {
            GroundedValue returnedRows = generateRows(new PrependSequenceIterator(firstRow, rows));
            GroundedValue columnNames = explicitColumnNames;

            if (explicitColumnNames == null) {
//                for (int i=0; i<explicitColumnNames.getLength(); i++) {
//                    columnIndex.initialPut(explicitColumnNames.itemAt(i).getStringValue(),
//                                                       Int64Value.makeIntegerValue(i+1));
//                }
//            } else {
                columnNames = EmptySequence.getInstance();
            }
            MapItem columnIndex = makeColumnIndex(columnNames);
            return new ShapedMap(RESULT_RECORD_SHAPE,
                                 columnNames,
                                 columnIndex,
                                 returnedRows,
                                 makeGetterFunction(returnedRows, columnIndex));
        }
    }

    /**
     * Make a column index (map from strings to integers) from a list
     * of column names supplied as a Java List
     * @param names the list of column names
     * @return the corresponding column index
     */

    private MapItem makeColumnIndex(List<StringValue> names) {
        Map<UnicodeString, GroundedValue> index = new HashMap<>(names.size());
        int pos = 1;
        for (StringValue name : names) {
            UnicodeString s = name.getUnicodeStringValue();
            if (!s.isEmpty() && index.get(s) == null) {
                index.put(s, Int64Value.makeIntegerValue(pos));
            }
            pos++;
        }
        return new DictionaryMap(index);
    }

    /**
     * Make a column index (map from strings to integers) from a list
     * of column names supplied as an XDM sequence of xs:string values
     *
     * @param names the list of column names
     * @return the corresponding column index
     */

    private MapItem makeColumnIndex(GroundedValue names) {
        Map<UnicodeString, GroundedValue> index = new HashMap<>(names.getLength());
        int pos = 1;
        for (Item item : names.asIterable()) {
            UnicodeString name = item.getUnicodeStringValue();
            if (!name.isEmpty() && index.get(name) == null) {
                index.put(name, Int64Value.makeIntegerValue(pos));
            }
            pos++;
        }
        return new DictionaryMap(index);
    }

    /**
     * Convert the sequence of rows delivered by the CSV parser to the sequence of
     * rows to be returned in the result; this typically involves adjusting each row
     * to change the order of fields, or to truncate or pad to a fixed size.
     * @param rows the rows as delivered by the CSV parser
     * @return a sequence of rows, suitably adjusted
     * @throws XPathException if things go wrong
     */
    private GroundedValue generateRows(SequenceIterator rows) throws XPathException {
        try {
            return SequenceExtent.from(ItemMappingIterator.map(rows, rowAdjuster));
        } catch (UncheckedXPathException ex) {
            throw ex.getXPathException();
        }
    }


//    private final static FunctionItemType GETTER_TYPE =
//            new SpecificFunctionType(
//                    SequenceType.one(BuiltInAtomicType.POSITIVE_INTEGER),
//                    SequenceType.one(LocalUnionType.of(BuiltInAtomicType.POSITIVE_INTEGER,
//                                                       BuiltInAtomicType.STRING)),
//                    SequenceType.SINGLE_STRING);

    /**
     * Make the "get" function item to be delivered in the result
     * @param rows the rows of data, after conversion
     * @param columnIndex the column index (map from strings to integer positions), as it
     *                    will be delivered
     * @return the function item returned in the "get" field
     */
    private static FunctionItem makeGetterFunction(GroundedValue rows, MapItem columnIndex) {
        return new SimpleBinaryFunction(
                (a0, a1) -> {
                    int rowNum = ((IntegerValue) a0.head()).asSubscript();
                    ArrayItem row = (ArrayItem) rows.itemAt(rowNum - 1);
                    if (row == null) {
                        return StringValue.EMPTY_STRING;
                    }
                    AtomicValue col = (AtomicValue) a1.head();
                    int colNum;
                    if (col instanceof StringValue) {
                        IntegerValue colValue = ((IntegerValue) columnIndex.get(col));
                        if (colValue == null) {
                            throw new XPathException("Column '" + col.getStringValue() +
                                                             "' is not present in CSV", "FOCV0004");
                        }
                        colNum = colValue.asSubscript();
                    } else {
                        colNum = ((IntegerValue) col).asSubscript();
                    }
                    return row.get(colNum - 1);
                },
                SequenceType.one(BuiltInAtomicType.POSITIVE_INTEGER),
                SequenceType.one(LocalUnionType.of(BuiltInAtomicType.POSITIVE_INTEGER,
                                                   BuiltInAtomicType.STRING)),
                SequenceType.SINGLE_STRING)
                .withDescription("fn:parse-csv()?get");
    }
}
