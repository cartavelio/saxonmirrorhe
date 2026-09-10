package com.saxonica.functions.qt4.csv;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.OptionsParameter;
import net.sf.saxon.ma.map.EmptyMap;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.EmptySequence;
import net.sf.saxon.value.StringValue;

import java.util.Map;

public class CsvToXml extends CsvFunction {
    /**
     * Sets OptionsParameter with all the needed options. {@link #extendedOptions}
     */
    public static OptionsParameter OPTION_DETAILS = extendedOptions();

    private Map<String, GroundedValue> options;

    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        Item arg0 = arguments[0].head();
        if (arg0 == null) {
            return EmptySequence.getInstance();
        }
        StringValue input = (StringValue) arg0;
        MapItem unparsedOptions;
        if (arguments.length >= 2) {
            unparsedOptions = (MapItem) arguments[1].head();
        } else {
            unparsedOptions = EmptyMap.getInstance();
        }

        options = getDetails().optionDetails.processSuppliedOptions(unparsedOptions, context);
        CsvOptionsParameterValidator.validate(options);

        CsvXmlGenerator generator = new CsvXmlGenerator(options, context, getStaticBaseUriString());
        return generator.fromRowSequence(CsvParser.parse(input, options));
    }

}
