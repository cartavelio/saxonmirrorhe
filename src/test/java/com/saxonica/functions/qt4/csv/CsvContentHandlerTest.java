package com.saxonica.functions.qt4.csv;

import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.BooleanValue;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;

import java.util.HashMap;
import java.util.Map;

import static com.saxonica.functions.qt4.csv.TestHelpers.makeRow;
import static com.saxonica.functions.qt4.csv.TestHelpers.makeRows;
import static net.sf.saxon.test.Assertions.*;
import static net.sf.saxon.test.OptionsParameterHelper.defaultOptionParameters;
import static net.sf.saxon.test.OptionsParameterHelper.parsedOptionParameters;
import static org.junit.jupiter.api.Assertions.assertAll;

class CsvContentHandlerTest {
    CsvContentHandler subject;
    Map<String, GroundedValue> options = defaultOptionParameters(CsvToArrays.OPTION_DETAILS);
    Sequence result;

    CsvContentHandlerTest() throws XPathException {
    }

    @Test
    @DisplayName("returns an empty ParsedCsv by default")
    void defaultReturn() {
        subject = new CsvContentHandler(options);

        result = subject.getSequenceOfRows();

        assertEmptySequence(result, "an empty ParsedCSV");
    }

    @Test
    @DisplayName("returns a ParsedCSV with a 1 row and 1 field if appendField called once")
    void oneField() {
        subject = new CsvContentHandler(options);

        subject.appendField("abc");

        result = subject.getSequenceOfRows();

        assertAll(
                () -> { assertSequenceLength(1, result, "expected one row"); },
                () -> { assertXPathDeepEqual(makeRow("abc"), result, "field 1 value is abc"); }
        );
    }

    @Test
    @DisplayName("returns a ParsedCSV with a two rows if startNewRow also called")
    void twoRecords() {
        subject = new CsvContentHandler(options);

        subject.appendField("abc");
        subject.startNewRow();
        subject.appendField("123");

        result = subject.getSequenceOfRows();

        assertAll(
                () -> { assertSequenceLength(2, result, "expected two rows"); },
                () -> { assertXPathDeepEqual(makeRows(rows -> rows.row("abc").row("123")), result); }
        );
    }

    @Test
    @DisplayName("Trimming whitespace happens if the option is set")
    void trimmingWhitespace() throws XPathException {
        Map<String, GroundedValue> opts = new HashMap<>();
        opts.put("trim-whitespace", BooleanValue.TRUE);
        options = parsedOptionParameters(CsvToArrays.OPTION_DETAILS, opts);
        subject = new CsvContentHandler(options);

        subject.appendField("   abc");
        subject.appendField("def   ");
        subject.appendField("   ghi   ");
        subject.appendField("\tjkl");
        subject.appendField("mno\t");
        subject.appendField("\tpqr\t");
        subject.appendField("\t  123");
        subject.appendField("456\t  ");
        subject.appendField("  \t789   \t");

        result = subject.getSequenceOfRows();

        assertAll(
                () -> { assertXPathDeepEqual(makeRow("abc", "def", "ghi", "jkl", "mno", "pqr", "123", "456", "789"), result); }
        );

    }
}