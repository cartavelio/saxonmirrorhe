package com.saxonica.functions.qt4.csv;


import net.sf.saxon.expr.StaticProperty;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.OptionsParameter;
import net.sf.saxon.ma.map.DictionaryMap;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.*;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.*;
import net.sf.saxon.value.*;

import java.util.HashSet;
import java.util.Map;
import java.util.Set;

/**
 * This class implements the function <code>fn:csv-to-xdm()</code>, which is a proposed function for XPath 4.0.
 *
 * <p>Calling this function with an input of CSV-formatted data, and an optional options hash, returns an XDM record
 * type representing the CSV data, that is, a sequence containing a single {@link MapItem}.
 * </p>
 */
public class CsvToXdm extends CsvFunction {
    /**
     * Sets OptionsParameter with all the needed options. {@see #extendedOptions}
     */
    public static OptionsParameter OPTION_DETAILS = extendedOptions();

    private Map<String, GroundedValue> options;

    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        Item arg0 = arguments[0].head();
        if (arg0 == null) {
            return CsvXdmGenerator.EMPTY_CSV;
        }
        StringValue input = (StringValue) arg0;
        MapItem unparsedOptions = new DictionaryMap();
        if (arguments.length >= 2) {
            unparsedOptions = (MapItem) arguments[1].head();
        }

        options = getDetails().optionDetails.processSuppliedOptions(unparsedOptions, context);
        CsvOptionsParameterValidator.validate(options);

        CsvXdmGenerator generator = new CsvXdmGenerator(options);
        Sequence result = generator.fromRowSequence(CsvParser.parse(input, options));
        return result;
    }
}