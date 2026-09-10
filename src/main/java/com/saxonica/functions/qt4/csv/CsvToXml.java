package com.saxonica.functions.qt4.csv;

import net.sf.saxon.event.Builder;
import net.sf.saxon.event.ComplexContentOutputter;
import net.sf.saxon.event.Outputter;
import net.sf.saxon.event.ReceiverOption;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.parser.Loc;
import net.sf.saxon.functions.OptionsParameter;
import net.sf.saxon.ma.map.DictionaryMap;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.*;
import net.sf.saxon.str.StringView;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.Untyped;
import net.sf.saxon.value.StringValue;

import java.util.HashMap;
import java.util.Map;

public class CsvToXml extends CsvFunction {
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

        CsvXmlGenerator generator = new CsvXmlGenerator(options, context, getStaticBaseUriString());
        Sequence result = generator.fromRowSequence(CsvParser.parse(input, options));
        return result;
    }

}
