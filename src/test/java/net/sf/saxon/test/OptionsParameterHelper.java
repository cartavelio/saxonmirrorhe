package net.sf.saxon.test;

import net.sf.saxon.Configuration;
import net.sf.saxon.expr.EarlyEvaluationContext;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.OptionsParameter;
import net.sf.saxon.ma.map.DictionaryMap;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;

import java.util.Map;
import java.util.stream.Stream;

public class OptionsParameterHelper {
    /**
     * Return the default options from the supplied OptionsParameter instance,
     * as if the function using it had been called without options.
     *
     * @param params the OptionsParameter instance
     * @return the default options for the provider OptionsParameter
     * @throws XPathException
     */
    public static Map<String, Sequence> defaultOptionParameters(OptionsParameter params) throws XPathException {
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
    public static Map<String, Sequence> parsedOptionParameters(OptionsParameter params, Map<String, Sequence> options) throws XPathException {
        XPathContext ctxt = new EarlyEvaluationContext(new Configuration());
        return params.processSuppliedOptions(buildXdmMap(options), ctxt);
    }

    /**
     *
     * @param params the OptionsParameters instance
     * @param options the options to process, as an XDM MapItem
     * @return the parsed options
     * @throws XPathException
     */
    public static Map<String, Sequence> parsedOptionParameters(OptionsParameter params, MapItem options) throws XPathException {
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
    public static Map<String, Sequence> parsedOptionParameters(OptionsParameter params, Map<String, Sequence> options, XPathContext ctxt) throws XPathException {
        return params.processSuppliedOptions(buildXdmMap(options), ctxt);
    }

    /**
     *
     * @param params the OptionsParameters instance
     * @param options the options to process, as a Map rather than an XDM MapItem
     * @param ctxt the XPathContext to evaluate the options using
     * @return the parsed options
     * @throws XPathException
     */
    public static Map<String, Sequence> parsedOptionParameters(OptionsParameter params, MapItem options, XPathContext ctxt) throws XPathException {
        return params.processSuppliedOptions(options, ctxt);
    }

    /**
     * Build a MapItem from a Map.
     * @param options the map to convert
     * @return
     */
    private static MapItem buildXdmMap(Map<String, Sequence> options) {
        DictionaryMap resultMap = new DictionaryMap();
        options.forEach((k, v) -> {
            resultMap.initialPut(k, (GroundedValue) v);
        });
        return resultMap;
    }

    /**
     * Build a MapItem from a stream of key value pairs
     * @param entries the stream to convert
     * @return
     */
    private static MapItem buildXdmMap(Stream<Object[]> entries) {
        DictionaryMap resultMap = new DictionaryMap();
        entries.forEach((pair) -> {
            resultMap.initialPut((String) pair[0], (GroundedValue) pair[1]);
        });
        return resultMap;
    }

}
