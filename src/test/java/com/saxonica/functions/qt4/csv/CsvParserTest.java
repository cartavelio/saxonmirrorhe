package com.saxonica.functions.qt4.csv;

import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.om.SequenceTool;
import net.sf.saxon.test.TestXdmHelpers;
import net.sf.saxon.trans.XPathException;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Nested;
import org.junit.jupiter.api.Test;

import java.util.HashMap;
import java.util.Map;

import static com.saxonica.functions.qt4.csv.TestHelpers.*;
import static net.sf.saxon.test.assertions.SaxonMatcherAssert.assertThat;
import static net.sf.saxon.test.Assertions.*;
import static net.sf.saxon.test.OptionsParameterHelper.defaultOptionParameters;
import static net.sf.saxon.test.OptionsParameterHelper.parsedOptionParameters;
import static net.sf.saxon.test.matchers.SequenceIteratorIsDeepEqualTo.deepEquals;
import static net.sf.saxon.test.matchers.SequenceIteratorIsEmptySequence.isEmptySequence;
import static org.junit.jupiter.api.Assertions.assertAll;

public class CsvParserTest {
    CsvParser subject;
    Map<String,GroundedValue> options;
    CsvContentHandler handler;
    SequenceIterator result;
    GroundedValue expectedRows;

    @BeforeEach
    void defaultOptions() throws XPathException {
        options = defaultOptionParameters(CsvToArrays.OPTION_DETAILS);
        handler = new CsvContentHandler(options);
    }

    @Test
    void emptyInput() throws XPathException{
        subject = new CsvParser(TestXdmHelpers.stringValue(""), options, handler);

        result = subject.parseInput();

        assertThat(result, isEmptySequence());
    }

    @Nested
    class ParserMethods {
        @Test
        @DisplayName("row fields are passed correctly to the CsvContentHandler")
        void fieldsArePassed() throws XPathException {
            subject = new CsvParser(TestXdmHelpers.stringValue("abc,def,ghi"), options, handler);

            result = subject.parseInput();

            assertThat(result, deepEquals(makeRow("abc", "def", "ghi")));
        }
        @Test
        @DisplayName("rows are passed correctly to the CsvContentHandler")
        void rowsArePassed() throws XPathException {
            subject = new CsvParser(TestXdmHelpers.stringValue("abc,def,ghi\n123,456,789"), options, handler);

            result = subject.parseInput();

            expectedRows = makeRows(rows -> {
                rows.row("abc", "def", "ghi").row("123", "456", "789");
            });

            assertThat(result, deepEquals(expectedRows));
        }

        @Test
        @DisplayName("rows and fields are passed correctly to the CsvContentHandler when there's only one column")
        void singleColumnCSV() throws XPathException {
            subject = new CsvParser(TestXdmHelpers.stringValue("abc\n123"), options, handler);

            SequenceIterator result = subject.parseInput();

            expectedRows = makeRows(rows -> rows.row("abc").row("123"));

            assertThat(result, deepEquals(expectedRows));
        }

        @Test
        @DisplayName("CSV strings with trailing record separator are processed correctly")
        void trailingRowSeparator() throws XPathException {
            subject = new CsvParser(TestXdmHelpers.stringValue("abc\n123\n"), options, handler);

            result = subject.parseInput();
            expectedRows = makeRows(rows -> rows.row("abc").row("123"));

            assertThat(result, deepEquals(expectedRows));
        }

        @Test
        @DisplayName("CSV string using CR to delimit rows")
        void defaultRowDelimiterCR() throws XPathException {
            subject = new CsvParser(TestXdmHelpers.stringValue("abc\r123\r"), options, handler);

            result = subject.parseInput();

            expectedRows = makeRows(rows -> rows.row("abc").row("123"));

            assertThat(result, deepEquals(expectedRows));
        }
    }

    @Nested
    class QuoteHandling {
        @Test
        @DisplayName("simple quoted fields are handled correctly")
        void quotedFieldsHandled() throws XPathException {
            subject = new CsvParser(TestXdmHelpers.stringValue("\"abc\",\"def\",\"ghi\"\n\"123\",\"456\",\"789\"\n"), options, handler);

            result = subject.parseInput();

            expectedRows = makeRows(rows -> {
                rows.row("abc", "def", "ghi").row("123", "456", "789");
            });

            assertThat(result, deepEquals(expectedRows));
        }

        @Test
        @DisplayName("escaped quotes within quoted fields are handled")
        void escapedQuotes() throws XPathException {
            subject = new CsvParser(TestXdmHelpers.stringValue("\"abc \"\"quote\"\" def\",\"ghi\"\n"), options, handler);

            result = subject.parseInput();

            expectedRows = makeRow("abc \"quote\" def", "ghi");

            assertThat(result, deepEquals(expectedRows));
        }

        @Test
        @DisplayName("non-default quote characters are handled")
        void nonDefaultQuoteChar() throws XPathException {
            options = parsedOptionParameters(CsvToArrays.OPTION_DETAILS, opts -> opts.put("quote-character", "|"));
            handler = new CsvContentHandler(options);
            subject = new CsvParser(TestXdmHelpers.stringValue("|abc ||quote|| def|,|ghi|\n"), options, handler);

            result = subject.parseInput();

            expectedRows = makeRow("abc |quote| def", "ghi");

            assertThat(result, deepEquals(expectedRows));
        }
    }

    @Nested
    class DelimiterHandling {
        @Test
        @DisplayName("Setting row delimiter to a single string")
        void rowDelimiterSingleString() throws XPathException {
            Map<String, GroundedValue> opts = new HashMap<>();
            opts.put("row-delimiter", TestXdmHelpers.stringValue("$"));
            options = parsedOptionParameters(CsvToArrays.OPTION_DETAILS, opts);

            subject = new CsvParser(TestXdmHelpers.stringValue("abc,def$123,456"), options, handler);

            result = subject.parseInput();

            expectedRows = makeRows(rows -> {
                rows.row("abc", "def").row("123", "456");
            });

            assertThat(result, deepEquals(expectedRows));
        }

        @Test
        @DisplayName("Setting field delimiter")
        void fieldDelimiter() throws XPathException {
            options = parsedOptionParameters(CsvToArrays.OPTION_DETAILS, opts -> opts.put("field-delimiter", ";"));

            subject = new CsvParser(TestXdmHelpers.stringValue("abc;def\n123;456\nghi;789"), options, handler);

            result = subject.parseInput();

            expectedRows = makeRows(rows -> {
                rows.row("abc", "def").row("123", "456").row("ghi", "789");
            });

            assertThat(result, deepEquals(expectedRows));
        }
    }
}
