package com.saxonica.functions.qt4.csv;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.OptionsParameter;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.ma.arrays.SimpleArrayItem;
import net.sf.saxon.ma.map.DictionaryMap;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.*;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.*;

/**
 * Implements XPath 4 fn:parse-csv() to parse CSV data from an xs:string into an array of sequences, with header row information optionally returned
 */
public class ParseCsv extends SystemFunction {
    public static OptionsParameter OPTION_DETAILS;
    private static Sequence EMPTY_CSV = EmptyAtomicSequence.getInstance();


    static {
        OptionsParameter o = new OptionsParameter();
        o.addAllowedOption("trim-whitespace", SequenceType.SINGLE_BOOLEAN, BooleanValue.FALSE);
        o.addAllowedOption("quote-character", SequenceType.SINGLE_STRING);
        o.addAllowedOption("row-delimiter", SequenceType.SINGLE_STRING);
        o.addAllowedOption("field-delimiter", SequenceType.SINGLE_STRING);
        OPTION_DETAILS = o;

    }

    /**
     * Execute a dynamic call to the function
     *
     * @param context   the dynamic evaluation context
     * @param arguments the values of the arguments, supplied as Sequences.
     * @return the result of the evaluation, as a Sequence. It is the responsibility
     * of the callee to ensure that the type of result conforms to the expected result type.
     * @throws XPathException (should not happen)
     */
    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        Item arg0 = arguments[0].head();
        if (arg0 == null) {
            return EMPTY_CSV;
        }
        StringValue input = (StringValue) arg0;
        MapItem options  = new DictionaryMap();
        if (arguments.length >= 2) {
            options = (MapItem) arguments[1].head();
        }
        CsvOptions csvOptions = CsvOptions.build(options, context);

        ParsedCSV parsedCsv = CsvParser.parse(input, csvOptions);
        return parsedCsv.getSequence();
    }

}
