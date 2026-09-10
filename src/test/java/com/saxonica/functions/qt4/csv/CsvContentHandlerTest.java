package com.saxonica.functions.qt4.csv;

import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.str.BMPString;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.BooleanValue;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;

import java.util.HashMap;
import java.util.Map;

import static com.saxonica.functions.qt4.csv.TestHelpers.makeRow;
import static com.saxonica.functions.qt4.csv.TestHelpers.makeRows;
import static net.sf.saxon.test.OptionsParameterHelper.defaultOptionParameters;
import static net.sf.saxon.test.OptionsParameterHelper.parsedOptionParameters;
import static net.sf.saxon.test.assertions.SaxonMatcherAssert.assertThat;
import static net.sf.saxon.test.matchers.SequenceIteratorIsDeepEqualTo.deepEquals;
import static net.sf.saxon.test.matchers.SequenceIteratorIsEmptySequence.isEmptySequence;

class CsvContentHandlerTest {
    CsvContentHandler subject;
    Map<String, GroundedValue> options = defaultOptionParameters(CsvToArrays.OPTION_DETAILS);
    SequenceIterator result;

    CsvContentHandlerTest() throws XPathException {
    }

    static UnicodeString u(String str) {
        return BMPString.of(str);
    }

    @Test
    @DisplayName("returns an empty ParsedCsv by default")
    void defaultReturn() {
        subject = new CsvContentHandler(options);

        result = subject.getSequenceOfRows();

        assertThat(result, isEmptySequence());
    }

    @Test
    @DisplayName("returns a ParsedCSV with a 1 row and 1 field if appendField called once")
    void oneField() {
        subject = new CsvContentHandler(options);

        subject.appendField(u("abc"));
        subject.finishRow(0);

        result = subject.getSequenceOfRows();

        assertThat(result, deepEquals(makeRow("abc")));
    }

    @Test
    @DisplayName("returns a ParsedCSV with a two rows if startNewRow also called")
    void twoRecords() {
        subject = new CsvContentHandler(options);

        subject.appendField(u("abc"));
        subject.finishRow(0);
        subject.appendField(u("123"));
        subject.finishRow(0);

        result = subject.getSequenceOfRows();

        assertThat(result, deepEquals(makeRows(rows -> rows.row("abc").row("123"))));
    }

    @Test
    @DisplayName("Trimming whitespace happens if the option is set")
    void trimmingWhitespace() throws XPathException {
        Map<String, GroundedValue> opts = new HashMap<>();
        opts.put("trim-whitespace", BooleanValue.TRUE);
        options = parsedOptionParameters(CsvToArrays.OPTION_DETAILS, opts);
        subject = new CsvContentHandler(options);

        subject.appendField(u("   abc"));
        subject.appendField(u("def   "));
        subject.appendField(u("   ghi   "));
        subject.appendField(u("\tjkl"));
        subject.appendField(u("mno\t"));
        subject.appendField(u("\tpqr\t"));
        subject.appendField(u("\t  123"));
        subject.appendField(u("456\t  "));
        subject.appendField(u("  \t789   \t"));
        subject.finishRow(0);

        result = subject.getSequenceOfRows();

        assertThat(result, deepEquals(makeRow("abc", "def", "ghi", "jkl", "mno", "pqr", "123", "456", "789")));
    }
}