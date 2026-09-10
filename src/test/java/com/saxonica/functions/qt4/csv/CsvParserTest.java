package com.saxonica.functions.qt4.csv;

import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.test.xdmfixtures.FixtureBuilder;
import net.sf.saxon.trans.XPathException;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Nested;
import org.junit.jupiter.api.Test;

import java.util.HashMap;
import java.util.Map;

import static com.saxonica.functions.qt4.csv.TestHelpers.*;
import static net.sf.saxon.test.Assertions.*;
import static net.sf.saxon.test.OptionsParameterHelper.defaultOptionParameters;
import static net.sf.saxon.test.OptionsParameterHelper.parsedOptionParameters;
import static org.junit.jupiter.api.Assertions.assertAll;

public class CsvParserTest {
    CsvParser subject;
    Map<String,GroundedValue> options;
    CsvContentHandler handler;
    Sequence result;

    @BeforeEach
    void defaultOptions() throws XPathException {
        options = defaultOptionParameters(ParseCsv.OPTION_DETAILS);
        handler = new CsvContentHandler(options);
    }

    @Test
    void emptyInput() throws XPathException{
        subject = new CsvParser(stringValue(""), options, handler);

        result = subject.parseInput();

        assertEmptySequence(result, "expected an empty CSV");
    }

    @Nested
    class ParserMethods {
        @Test
        @DisplayName("row fields are passed correctly to the CsvContentHandler")
        void fieldsArePassed() throws XPathException {
            subject = new CsvParser(stringValue("abc,def,ghi"), options, handler);

            result = subject.parseInput();

            GroundedValue first_row = result.head();

            assertAll(
                    () -> { assertSequenceLength(1, result, "expected 1 Record"); },
                    () -> { assertXPathDeepEqual(makeRow("abc", "def", "ghi"), first_row, "expected (abc,def,ghi)"); }
            );
        }
        @Test
        @DisplayName("rows are passed correctly to the CsvContentHandler")
        void rowsArePassed() throws XPathException {
            subject = new CsvParser(stringValue("abc,def,ghi\r\n123,456,789"), options, handler);

            result = subject.parseInput();

            Sequence expectedRows = makeRows(rows -> {
                rows.row("abc", "def", "ghi").row("123", "456", "789");
            });

            assertXPathDeepEqual(expectedRows, result, "expected ([abc,def,ghi], [123,456,789])");
        }

        @Test
        @DisplayName("rows and fields are passed correctly to the CsvContentHandler when there's only one column")
        void singleColumnCSV() throws XPathException {
            subject = new CsvParser(stringValue("abc\r\n123"), options, handler);

            result = subject.parseInput();

            Sequence expectedRows = makeRows(rows -> rows.row("abc").row("123"));

            assertXPathDeepEqual(expectedRows, result, "expected ([abc], [123])");
        }

        @Test
        @DisplayName("CSV strings with trailing record separator are processed correctly")
        void trailingRowSeparator() throws XPathException {
            subject = new CsvParser(stringValue("abc\r\n123\r\n"), options, handler);

            result = subject.parseInput();
            Sequence expectedRows = makeRows(rows -> rows.row("abc").row("123"));

            assertXPathDeepEqual(expectedRows, result, "expected ([abc], [123])");
        }

        @Test
        @DisplayName("CSV string using CR to delimit rows")
        void defaultRowDelimiterCR() throws XPathException {
            subject = new CsvParser(stringValue("abc\r123\r"), options, handler);

            result = subject.parseInput();

            Sequence expectedRows = makeRows(rows -> rows.row("abc").row("123"));

            assertXPathDeepEqual(expectedRows, result, "expected ([abc], [123])");
        }
    }

    @Nested
    class QuoteHandling {
        @Test
        @DisplayName("simple quoted fields are handled correctly")
        void quotedFieldsHandled() throws XPathException {
            subject = new CsvParser(stringValue("\"abc\",\"def\",\"ghi\"\r\n\"123\",\"456\",\"789\"\r\n"), options, handler);

            result = subject.parseInput();

            Sequence expectedRows = makeRows(rows -> {
                rows.row("abc", "def", "ghi").row("123", "456", "789");
            });

            assertXPathDeepEqual(expectedRows, result, "expected ([abc, def, ghi], [123, 456, 789])");
        }

        @Test
        @DisplayName("escaped quotes within quoted fields are handled")
        void escapedQuotes() throws XPathException {
            subject = new CsvParser(stringValue("\"abc \"\"quote\"\" def\",\"ghi\"\r\n"), options, handler);

            result = subject.parseInput();

            Sequence expectedRows = makeRow("abc \"quote\" def", "ghi");

            assertXPathDeepEqual(expectedRows, result, "expected ([abc 'quote' def, ghi])");
        }

        @Test
        @DisplayName("non-default quote characters are handled")
        void nonDefaultQuoteChar() throws XPathException {
            options = parsedOptionParameters(ParseCsv.OPTION_DETAILS, opts -> opts.put("quote-character", "|"));
            handler = new CsvContentHandler(options);
            subject = new CsvParser(stringValue("|abc ||quote|| def|,|ghi|\r\n"), options, handler);

            result = subject.parseInput();

            Sequence expectedRows = makeRow("abc |quote| def", "ghi");

            assertXPathDeepEqual(expectedRows, result, "expected ([abc |quote| def, ghi])");
        }
    }

    @Nested
    class DelimiterHandling {
        @Test
        @DisplayName("Setting row delimiter to a single string")
        void rowDelimiterSingleString() throws XPathException {
            Map<String, GroundedValue> opts = new HashMap<>();
            opts.put("row-delimiter", stringValue("$"));
            options = parsedOptionParameters(ParseCsv.OPTION_DETAILS, opts);

            subject = new CsvParser(stringValue("abc,def$123,456"), options, handler);

            result = subject.parseInput();

            Sequence expectedRows = makeRows(rows -> {
                rows.row("abc", "def").row("123", "456");
            });

            assertXPathDeepEqual(expectedRows, result, "expected ([abc, def], [123, 456])");
        }

        @Test
        @DisplayName("Setting row delimiter to a sequence of strings (as with \\r\\n / \\n / \\r)")
        void rowDelimiterSequence() throws XPathException {
            Map<String, GroundedValue> opts = new HashMap<>();
            opts.put("row-delimiter", simpleValueSequence("$", "£"));
            options = parsedOptionParameters(ParseCsv.OPTION_DETAILS, opts);

            subject = new CsvParser(stringValue("abc,def$123,456£ghi,789"), options, handler);

            result = subject.parseInput();
            Sequence expectedRows = makeRows(rows -> {
                rows.row("abc", "def").row("123", "456").row("ghi", "789");
            });

            assertXPathDeepEqual(expectedRows, result, "expected ([abc, def], [123, 456], [ghi, 789])");
        }

        @Test
        @DisplayName("Setting row delimiter to a daft sequence of strings")
        void rowDelimiterDaftSequence() throws XPathException {
            Map<String, GroundedValue> opts = new HashMap<>();
            opts.put("row-delimiter", simpleValueSequence("$$", "$"));
            options = parsedOptionParameters(ParseCsv.OPTION_DETAILS, opts);

            subject = new CsvParser(stringValue("abc,def$123,456$$ghi,789"), options, handler);

            result = subject.parseInput();

            Sequence expectedRows = makeRows(rows -> {
                rows.row("abc", "def").row("123", "456").row("ghi", "789");
            });

            assertXPathDeepEqual(expectedRows, result, "expected ([abc, def], [123, 456], [ghi, 789])");
        }


        @Test
        @DisplayName("Setting field delimiter")
        void fieldDelimiter() throws XPathException {
            options = parsedOptionParameters(ParseCsv.OPTION_DETAILS, opts -> opts.put("field-delimiter", ";"));

            subject = new CsvParser(stringValue("abc;def\n123;456\nghi;789"), options, handler);

            result = subject.parseInput();

            Sequence expectedRows = makeRows(rows -> {
                rows.row("abc", "def").row("123", "456").row("ghi", "789");
            });

            assertXPathDeepEqual(expectedRows, result, "expected ([abc, def], [123, 456], [ghi, 789])");
        }
    }
}
