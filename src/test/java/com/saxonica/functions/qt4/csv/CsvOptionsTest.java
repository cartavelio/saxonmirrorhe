package com.saxonica.functions.qt4.csv;

import net.sf.saxon.Configuration;
import net.sf.saxon.expr.EarlyEvaluationContext;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.ma.map.DictionaryMap;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.BooleanValue;
import net.sf.saxon.value.StringValue;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;

import java.util.Map;
import java.util.stream.Collectors;
import java.util.stream.Stream;

import static org.junit.jupiter.api.Assertions.*;

class CsvOptionsTest {
    CsvOptions subject;
    @Test
    @DisplayName("default options have the correct value")
    void hasCorrectDefaultOptions() {
        subject = new CsvOptions();

        assertAll("has expected default values",
            () -> { assertEquals("\"", subject.getQuoteCharacter(), "double-quote quote char"); },
            () -> { assertEquals(",", subject.getFieldDelimiter(), "comma-delimited"); },
            () -> { assertArrayEquals(new String[] { "\r\n", "\n", "\r" }, subject.getRowDelimiters(), "CRLF/CR/LF row delimited"); },
            () -> { assertFalse(subject.getTrimWhitespace(), "Don't trim whitespace"); }
        );
    }

    @Test
    @DisplayName("options can be set with a Map<String, Sequence>")
    void creatingFromAMap() throws XPathException {
        Map<String, GroundedValue> values = buildMap(Stream.of(new Object[][] {
            { "field-delimiter", new StringValue(";") },
            { "row-delimiter", new StringValue("\n") },
            { "quote-character", new StringValue("|") },
            { "trim-whitespace", BooleanValue.TRUE },
        }));
        subject = new CsvOptions(values);

        assertAll("has expected values",
                () -> { assertEquals(";", subject.getFieldDelimiter(), "semi-colon delimited"); },
                () -> { assertArrayEquals(new String[] { "\n" }, subject.getRowDelimiters(), "LF-only row delimited"); },
                () -> { assertEquals("|", subject.getQuoteCharacter(), "Pipe quote character"); },
                () -> { assertTrue(subject.getTrimWhitespace(), "Trim whitespace"); }
        );
    }

    @Test
    @DisplayName("options can be set from an XDM MapItem")
    void creatingFromXDM() throws XPathException {
        MapItem values = buildXdmMap(Stream.of(new Object[][] {
            { "field-delimiter", new StringValue(";") },
            { "row-delimiter", new StringValue("\n") },
            { "quote-character", new StringValue("|") },
            { "trim-whitespace", BooleanValue.TRUE },
        }));
        XPathContext ctxt = new EarlyEvaluationContext(new Configuration());
        subject = CsvOptions.build(values, ctxt);

        assertAll("has expected default values",
                () -> { assertEquals(";", subject.getFieldDelimiter(), "semi-colon delimited"); },
                () -> { assertArrayEquals(new String[] { "\n" }, subject.getRowDelimiters(), "LF-only row delimited"); },
                () -> { assertEquals("|", subject.getQuoteCharacter(), "Pipe quote character"); },
                () -> { assertTrue(subject.getTrimWhitespace(), "Trim whitespace"); }
        );
    }

    private Map<String, GroundedValue> buildMap(Stream<Object[]> entries) {
        return entries.collect(Collectors.toMap(kv -> (String) kv[0], kv -> (GroundedValue) kv[1]));
    }

    private MapItem buildXdmMap(Stream<Object[]> entries) {
        Map<String, GroundedValue> map = buildMap(entries);
        DictionaryMap resultMap = new DictionaryMap();
        map.forEach((k, v) -> {
            resultMap.initialPut(k, v);
        });
        return resultMap;
    }
}