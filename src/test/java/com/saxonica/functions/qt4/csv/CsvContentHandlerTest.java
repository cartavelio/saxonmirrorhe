package com.saxonica.functions.qt4.csv;

import net.sf.saxon.functions.Count;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.BooleanValue;
import net.sf.saxon.value.StringValue;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;

import java.util.HashMap;
import java.util.Map;

import static org.junit.jupiter.api.Assertions.*;
import static net.sf.saxon.test.Assertions.*;
import static com.saxonica.functions.qt4.csv.TestHelpers.*;

class CsvContentHandlerTest {
    CsvContentHandler subject;

    @Test
    @DisplayName("returns an empty ParsedCsv by default")
    void defaultReturn() {
        subject = new CsvContentHandler(CsvOptions.DEFAULT);

        ParsedCSV result = subject.parsedCsv();

        assertEquals(ParsedCSV.EMPTY_CSV, result, "an empty ParsedCSV");
    }

    @Test
    @DisplayName("returns a ParsedCSV with a 1 row and 1 field if appendField called once")
    void oneField() {
        subject = new CsvContentHandler(CsvOptions.DEFAULT);

        subject.appendField("abc");

        ParsedCSV result = subject.parsedCsv();

        assertAll(
                () -> { assertEquals(1, Count.count(result.getSequence().iterate()), "expected one row"); },
                () -> { assertXPathDeepEqual(makeRow(b -> b.row("abc")), result.getSequence(), "field 1 value is abc"); }
        );
    }

    @Test
    @DisplayName("returns a ParsedCSV with a two rows if startNewRow also called")
    void twoRecords() {
        subject = new CsvContentHandler(CsvOptions.DEFAULT);

        subject.appendField("abc");
        subject.startNewRow();
        subject.appendField("123");

        ParsedCSV result = subject.parsedCsv();

        assertAll(
                () -> { assertSequenceLength(2, result.getSequence(), "expected two rows"); },
                () -> { assertXPathDeepEqual(makeRows(b -> b.row("abc"), b -> b.row("123")), result.getSequence()); }
        );
    }

    @Test
    @DisplayName("Trimming whitespace happens if the option is set")
    void trimmingWhitespace() throws XPathException {
        Map<String, GroundedValue> opts = new HashMap<>();
        opts.put("trim-whitespace", BooleanValue.TRUE);
        subject = new CsvContentHandler(new CsvOptions(opts));

        subject.appendField("   abc");
        subject.appendField("def   ");
        subject.appendField("   ghi   ");
        subject.appendField("\tjkl");
        subject.appendField("mno\t");
        subject.appendField("\tpqr\t");
        subject.appendField("\t  123");
        subject.appendField("456\t  ");
        subject.appendField("  \t789   \t");

        ParsedCSV result = subject.parsedCsv();

        assertAll(
                () -> { assertXPathDeepEqual(makeRow(b -> b.row("abc", "def", "ghi", "jkl", "mno", "pqr", "123", "456", "789")), result.getSequence()); }
        );

    }
}