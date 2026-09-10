package com.saxonica.functions.qt4.csv;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.parser.ExpressionTool;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.BooleanValue;
import net.sf.saxon.value.StringValue;

import java.util.HashMap;
import java.util.Map;

public class CsvOptions {
    private static final String[] booleanOptions = new String[]{
            "trim-whitespace"};
    private static final String[] stringOptions = new String[]{
            "field-delimiter", "row-delimiter", "quote-character"};

    public static final CsvOptions DEFAULT = new CsvOptions();
    public static final CsvOptions TAB_DELIMITED;
    static {
        CsvOptions csvOpts;
        try {
            Map<String, GroundedValue> opts = new HashMap<>();
            opts.put("field-delimiter", new StringValue("\t"));
            csvOpts = new CsvOptions(opts);
        } catch (XPathException ex) {
            csvOpts = null;
        }
        TAB_DELIMITED = csvOpts;
    }

    private String fieldDelimiter = ",";
    private String rowDelimiter = "\r\n";
    private String[] rowDelimiters = new String[] { "\r\n", "\n", "\r" };
    private boolean trimWhitespace = false;
    private String quoteCharacter = "\"";

    public static CsvOptions build(MapItem map, XPathContext context) throws XPathException {
        Map<String, GroundedValue> values = ParseCsv.OPTION_DETAILS.processSuppliedOptions(map, context);
        return new CsvOptions (values);
    }

    public CsvOptions() {
    }

    public CsvOptions(Map<String, GroundedValue> values) throws XPathException {
        for (String option : booleanOptions) {
            setBooleanOption(values, option);
        }
        for (String option : stringOptions) {
            setStringOption(values, option);
        }
    }

    public String getFieldDelimiter() {
        return fieldDelimiter;
    }

    public String getRowDelimiter() {
        return rowDelimiter;
    }

    public String[] getRowDelimiters() {
        return rowDelimiters;
    }

    public String getQuoteCharacter() {
        return quoteCharacter;
    }

    public boolean getTrimWhitespace() {
        return trimWhitespace;
    }

    private void setStringOption(Map<String, GroundedValue> map, String optionName) {
        Sequence value = map.get(optionName);
        if (value != null) {
            switch (optionName) {
                case "field-delimiter":
                    fieldDelimiter = value.toString();
                    return;
                case "row-delimiter":
                    rowDelimiters = new String[] { value.toString() };
                    return;
                case "quote-character":
                    quoteCharacter = value.toString();
                    return;
                default:
                    throw new IllegalArgumentException();
            }
        }
    }

    private void setBooleanOption(Map<String, GroundedValue> map, String optionName) throws XPathException {
        Sequence value = map.get(optionName);
        if (value != null) {
            boolean booleanValue = ExpressionTool.effectiveBooleanValue(value.iterate());
            switch (optionName) {
                case "trim-whitespace":
                    trimWhitespace = booleanValue;
                    return;
                default:
                    throw new IllegalArgumentException();
            }
        }
    }
}
