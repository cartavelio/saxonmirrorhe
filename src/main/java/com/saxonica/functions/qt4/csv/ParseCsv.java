package com.saxonica.functions.qt4.csv;


import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.OptionsParameter;
import net.sf.saxon.ma.map.EmptyMap;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.StringValue;

import java.util.Map;

/**
 * This class implements the function <code>fn:parse-csv()</code>, which is a proposed function for XPath 4.0.
 *
 * <p>Calling this function with an input of CSV-formatted data, and an optional options hash, returns an XDM record
 * type representing the CSV data, that is, a sequence containing a single {@link MapItem}.
 * </p>
 */
public class ParseCsv extends CsvFunction {
    /**
     * Sets OptionsParameter with all the needed options. {@link #extendedOptions}
     */
    public static OptionsParameter OPTION_DETAILS = extendedOptions();

    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        Item arg0 = arguments[0].head();
        if (arg0 == null) {
            return CsvRecordGenerator.EMPTY_CSV;
        }
        StringValue input = (StringValue) arg0;
        MapItem unparsedOptions;
        if (arguments.length >= 2) {
            unparsedOptions = (MapItem) arguments[1].head();
        } else {
            unparsedOptions = EmptyMap.getInstance();
        }

        Map<String, GroundedValue> options =
                getDetails().optionDetails.processSuppliedOptions(unparsedOptions, context);
        CsvOptionsParameterValidator.validate(options);

        CsvRecordGenerator generator = new CsvRecordGenerator(options);
        return generator.fromRowSequence(CsvParser.parse(input, options));
    }
}