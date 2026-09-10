package com.saxonica.functions.qt4.csv;

import net.sf.saxon.ma.arrays.SimpleArrayItem;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.value.BooleanValue;
import net.sf.saxon.value.SequenceExtent;
import net.sf.saxon.value.StringValue;

import java.util.ArrayList;
import java.util.List;
import java.util.Map;
import java.util.stream.Collectors;

/**
 * A CsvContentHandler's methods are called to create the output sequences from parsing a CSV, much like a SAX ContentHandler
 */
public class CsvContentHandler {
    private List<GroundedValue> currentRow;
    private final List<List<GroundedValue>> rows = new ArrayList<>();
    private boolean trimWhitespace;

    public CsvContentHandler(Map<String, GroundedValue> options) {
        currentRow = new ArrayList<>();

        trimWhitespace = ((BooleanValue) options.get("trim-whitespace")).getBooleanValue();
    }
    /**
     * Appends a field value to the current Record
     */
    void appendField(String value) {
        if (rows.isEmpty()) {
            rows.add(currentRow);
        }
        currentRow.add(new StringValue(trimWhitespace ? value.trim() : value));
    }

    /**
     * Signal that a new Row should be begun
     */
    public void startNewRow() {
        currentRow = new ArrayList<>();
        rows.add(currentRow);
    }

    /**
     * Generates a sequence containing the rows (XDM Arrays) in the CSV
     * @return a sequence of rows
     */
    public Sequence getSequence() {
        return SequenceExtent.makeSequenceExtent(rows.stream().map(SimpleArrayItem::new).collect(Collectors.toList()));
    }
}
