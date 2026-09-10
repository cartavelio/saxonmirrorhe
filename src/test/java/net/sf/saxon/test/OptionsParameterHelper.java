package net.sf.saxon.test;

import net.sf.saxon.Configuration;
import net.sf.saxon.expr.EarlyEvaluationContext;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.OptionsParameter;
import net.sf.saxon.ma.map.DictionaryMap;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.test.xdmfixtures.DslLambda;
import net.sf.saxon.test.xdmfixtures.MapBuilder;
import net.sf.saxon.trans.XPathException;

import java.util.HashMap;
import java.util.Map;
import java.util.function.Consumer;

public class OptionsParameterHelper {
    /**
     * Return the default options from the supplied OptionsParameter instance,
     * as if the function using it had been called without options.
     *
     * @param params the OptionsParameter instance
     * @return the default options for the provider OptionsParameter
     * @throws XPathException
     */
    public static Map<String, GroundedValue> defaultOptionParameters(OptionsParameter params) throws XPathException {
        XPathContext ctxt = new EarlyEvaluationContext(new Configuration());
        return params.processSuppliedOptions(new DictionaryMap(), ctxt);
    }

    /**
     *
     * @param params the OptionsParameters instance
     * @param options the options to process, as a Map rather than an XDM MapItem
     * @return the parsed options
     * @throws XPathException
     */
    public static Map<String, GroundedValue> parsedOptionParameters(OptionsParameter params, Map<String, GroundedValue> options) throws XPathException {
        XPathContext ctxt = new EarlyEvaluationContext(new Configuration());
        return params.processSuppliedOptions(TestXdmHelpers.buildXdmMap(options), ctxt);
    }

    /**
     *
     * @param params the OptionsParameters instance
     * @param options the options to process, as an XDM MapItem
     * @return the parsed options
     * @throws XPathException
     */
    public static Map<String, GroundedValue> parsedOptionParameters(OptionsParameter params, MapItem options) throws XPathException {
        XPathContext ctxt = new EarlyEvaluationContext(new Configuration());
        return params.processSuppliedOptions(options, ctxt);
    }

    /**
     *
     * @param params the OptionsParameters instance
     * @param options the options to process, as an XDM MapItem
     * @param ctxt the XPathContext to evaluate the options using
     * @return the parsed options
     * @throws XPathException
     */
    public static Map<String, GroundedValue> parsedOptionParameters(OptionsParameter params, Map<String, GroundedValue> options, XPathContext ctxt) throws XPathException {
        return params.processSuppliedOptions(TestXdmHelpers.buildXdmMap(options), ctxt);
    }

    /**
     *
     * @param params the OptionsParameters instance
     * @param options the options to process, as a MapItem
     * @param ctxt the XPathContext to evaluate the options using
     * @return the parsed options
     * @throws XPathException
     */
    public static Map<String, GroundedValue> parsedOptionParameters(OptionsParameter params, MapItem options, XPathContext ctxt) throws XPathException {
        return params.processSuppliedOptions(options, ctxt);
    }

    public static Map<String, GroundedValue> parsedOptionParameters(OptionsParameter params, DslLambda<MapBuilder> definition) throws XPathException {
        MapBuilder builder = new MapBuilder();
        definition.buildWith(builder);
        return parsedOptionParameters(params, builder.getValue());
    }
}
