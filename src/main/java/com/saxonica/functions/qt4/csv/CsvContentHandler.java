package com.saxonica.functions.qt4.csv;

import net.sf.saxon.ma.arrays.ArrayItem;
import net.sf.saxon.ma.arrays.StringArrayItem;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.tree.iter.ListIterator;
import net.sf.saxon.value.BooleanValue;
import net.sf.saxon.value.Whitespace;

import java.util.ArrayList;
import java.util.List;
import java.util.Map;

/**
 * A CsvContentHandler's methods are called to create the output sequences from parsing a CSV,
 * much like a SAX ContentHandler
 */
public class CsvContentHandler {
    private List<UnicodeString> currentRow;
    private final List<ArrayItem> rows = new ArrayList<>();
    private final boolean trimWhitespace;

    public CsvContentHandler(Map<String, GroundedValue> options) {
        trimWhitespace = ((BooleanValue) options.get("trim-whitespace")).getBooleanValue();
    }
    /**
     * Appends a field value to the current Record
     */
    public void appendField(UnicodeString value) {
        if (currentRow == null) {
            currentRow = new ArrayList<>();
        }
        currentRow.add(trimWhitespace ? Whitespace.trim(value) : value.economize());
    }

    /**
     * Mark the current row as complete
     * @param delimiter the delimiter ending the row: either a row-delimiter, or -1 indicating
     *                  the end of the input string.
     */
    public void finishRow(int delimiter) {
        // A row with a single blank field is treated as a blank row, i.e. a row with zero fields
        if (currentRow.size() == 1 && currentRow.get(0).isEmpty()) {
            if (delimiter != -1) {
                blankRow();
            }
        } else {
            rows.add(new StringArrayItem(currentRow));
            currentRow = null;
        }
    }

    /**
     * Generate a blank row (that is, a row with zero fields)
     */
    public void blankRow() {
        rows.add(StringArrayItem.EMPTY_ARRAY);
        currentRow = null;
    }

    /**
     * Generates a sequence containing the rows (XDM Arrays) in the CSV
     * @return a sequence of rows, represented as XDM arrays of xs:string values
     */
    public SequenceIterator getSequenceOfRows() {
        return new ListIterator.Of<>(rows);
    }

    /**
     * Get the number of fields present in the current row
     * @return the number of fields in the row
     */
    public int getCurrentRowSize() {
        return currentRow == null ? 0 : currentRow.size();
    }
}
