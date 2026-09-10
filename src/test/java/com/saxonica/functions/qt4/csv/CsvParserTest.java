package com.saxonica.functions.qt4.csv;

import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Nested;
import org.junit.jupiter.api.Test;

import static com.saxonica.functions.qt4.csv.TestHelpers.*;
import static net.sf.saxon.test.Assertions.*;
import static org.junit.jupiter.api.Assertions.assertAll;

public class CsvParserTest {
    CsvParser subject;
    final CsvContentHandler handler = new CsvContentHandler(CsvOptions.DEFAULT);

    @Test
    void emptyInput() throws XPathException{
        subject = new CsvParser(stringValue(""), CsvOptions.DEFAULT, new CsvContentHandler(CsvOptions.DEFAULT));

        ParsedCSV result = subject.parseInput();

        assertEmptySequence(result.getSequence(), "expected an empty CSV");
    }

    @Nested
    class ParserMethods {
//        final CsvContentHandler handler = new CsvContentHandler();
        @Test
        @DisplayName("row fields are passed correctly to the CsvContentHandler")
        void fieldsArePassed() throws XPathException {
            subject = new CsvParser(stringValue("abc,def,ghi"), CsvOptions.DEFAULT, handler);

            ParsedCSV result = subject.parseInput();

            GroundedValue first_row = result.getSequence().head();

            assertAll(
                    () -> { assertSequenceLength(1, result.getSequence(), "expected 1 Record"); },
                    () -> { assertXPathDeepEqual(stringsArrayItem("abc", "def", "ghi"), first_row, "expected (abc,def,ghi)"); }
            );
        }
        @Test
        @DisplayName("rows are passed correctly to the CsvContentHandler")
        void rowsArePassed() throws XPathException {
            subject = new CsvParser(stringValue("abc,def,ghi\r\n123,456,789"), CsvOptions.DEFAULT, handler);

            ParsedCSV result = subject.parseInput();

            Sequence expectedRows = makeRows(b -> b.row("abc", "def", "ghi"), b -> b.row("123", "456", "789"));

            assertXPathDeepEqual(expectedRows, result.getSequence(), "expected ([abc,def,ghi], [123,456,789])");
        }

        @Test
        @DisplayName("rows and fields are passed correctly to the CsvContentHandler when there's only one column")
        void singleColumnCSV() throws XPathException {
            subject = new CsvParser(stringValue("abc\r\n123"), CsvOptions.DEFAULT, handler);

            ParsedCSV result = subject.parseInput();

            Sequence expectedRows = makeRows(b -> b.row("abc"), b -> b.row("123"));

            assertXPathDeepEqual(expectedRows, result.getSequence(), "expected ([abc], [123])");
        }

        @Test
        @DisplayName("CSV strings with trailing record separator are processed correctly")
        void trailingRowSeparator() throws XPathException {
            subject = new CsvParser(stringValue("abc\r\n123\r\n"), CsvOptions.DEFAULT, handler);

            ParsedCSV result = subject.parseInput();

            Sequence expectedRows = makeRows(b -> b.row("abc"), b -> b.row("123"));

            assertXPathDeepEqual(expectedRows, result.getSequence(), "expected ([abc], [123])");
        }

        @Test
        @DisplayName("CSV string using CR to delimit rows")
        void defaultRowDelimiterCR() throws XPathException {
            subject = new CsvParser(stringValue("abc\r123\r"), CsvOptions.DEFAULT, handler);

            ParsedCSV result = subject.parseInput();

            Sequence expectedRows = makeRows(b -> b.row("abc"), b -> b.row("123"));

            assertXPathDeepEqual(expectedRows, result.getSequence(), "expected ([abc], [123])");
        }
    }

    @Nested
    class QuoteHandling {
        @Test
        @DisplayName("simple quoted fields are handled correctly")
        void quotedFieldsHandled() throws XPathException {
            subject = new CsvParser(stringValue("\"abc\",\"def\",\"ghi\"\r\n\"123\",\"456\",\"789\"\r\n"), CsvOptions.DEFAULT, handler);

            ParsedCSV result = subject.parseInput();

            Sequence expectedRows = makeRows(b -> b.row("abc", "def", "ghi"), b -> b.row("123", "456", "789"));

            assertXPathDeepEqual(expectedRows, result.getSequence(), "expected ([abc, def, ghi], [123, 456, 789])");
        }

        @Test
        @DisplayName("escaped quotes within quoted fields are handled")
        void escapedQuotes() throws XPathException {
            subject = new CsvParser(stringValue("\"abc \"\"quote\"\" def\",\"ghi\"\r\n"), CsvOptions.DEFAULT, handler);

            ParsedCSV result = subject.parseInput();

            Sequence expectedRows = makeRow(b -> b.row("abc \"quote\" def", "ghi"));

            assertXPathDeepEqual(expectedRows, result.getSequence(), "expected ([abc 'quote' def, ghi])");

        }
    }

}
