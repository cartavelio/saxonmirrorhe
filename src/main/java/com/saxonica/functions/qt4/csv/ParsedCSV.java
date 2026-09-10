package com.saxonica.functions.qt4.csv;

import net.sf.saxon.ma.arrays.ArrayItem;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.value.SequenceExtent;

import java.util.ArrayList;
import java.util.List;

public class ParsedCSV {
    public static ParsedCSV EMPTY_CSV = new ParsedCSV(new ArrayList<>());
    public final List<ArrayItem> rows;

    public ParsedCSV(List<ArrayItem> rows) {
        this.rows = rows;
    }

    public Sequence getSequence() {
        return SequenceExtent.makeSequenceExtent(rows);
    }
}
