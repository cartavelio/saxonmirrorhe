package com.saxonica.functions.extfn;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.ma.map.DictionaryMap;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.BooleanValue;
import net.sf.saxon.value.EmptySequence;
import net.sf.saxon.value.Int64Value;
import net.sf.saxon.value.StringItem;

import java.net.URI;
import java.net.URISyntaxException;
import java.util.StringTokenizer;

/**
 * This class implements the saxon:analyze-uri() function which returns a map containing the components
 * of a URI
 */

public class AnalyzeUri extends SystemFunction {

    /**
     * Evaluate this function call at run-time
     *
     * @param context   The XPath dynamic evaluation context. Not used in this case
     * @param arguments The values of the arguments to the function call. In this case there are none.
     * @return the result of the function. This will have a timezone, specifically, the platform
     * default timezone. The returned value will be an instance of the built-in
     * subtype {@code xs:dateTimeStamp}
     */

    @Override
    public MapItem call(XPathContext context, Sequence[] arguments) throws XPathException {
        DictionaryMap dict = new DictionaryMap();
        String arg = arguments[0].head().getStringValue();
        try {
            URI uri = new URI(arg);
            dict.initialPut("isValid", BooleanValue.TRUE);
            boolean test = uri.isAbsolute();
            dict.initialPut("isAbsolute",BooleanValue.get(test));
            test = uri.isOpaque();
            dict.initialPut("isOpaque", BooleanValue.get(test));
            String val = uri.getAuthority();
            if (val != null) {
                dict.initialPut("authority", new StringItem(val));
            }
            val = uri.getRawAuthority();
            if (val != null) {
                dict.initialPut("rawAuthority", new StringItem(val));
            }
            val = uri.getUserInfo();
            if (val != null) {
                dict.initialPut("userInfo", new StringItem(val));
            }
            val = uri.getRawUserInfo();
            if (val != null) {
                dict.initialPut("rawUserInfo", new StringItem(val));
            }
            val = uri.getScheme();
            if (val != null) {
                dict.initialPut("scheme", new StringItem(val));
            }
            val = uri.getHost();
            if (val != null) {
                dict.initialPut("host", new StringItem(val));
            }
            val = uri.getSchemeSpecificPart();
            if (val != null) {
                dict.initialPut("schemeSpecificPart", new StringItem(val));
            }
            val = uri.getRawSchemeSpecificPart();
            if (val != null) {
                dict.initialPut("rawSchemeSpecificPart", new StringItem(val));
            }
            int port = uri.getPort();
            if (port >= 0) {
                dict.initialPut("port", new Int64Value(port));
            }
            val = uri.getPath();
            if (val != null) {
                dict.initialPut("path", new StringItem(val));
            }
            val = uri.getRawPath();
            if (val != null) {
                dict.initialPut("rawPath", new StringItem(val));
            }
            val = uri.getRawQuery();
            if (val != null) {
                dict.initialPut("rawQuery", new StringItem(val));
            }
            val = uri.getQuery();
            if (val != null) {
                dict.initialPut("query", new StringItem(val));
                DictionaryMap params = new DictionaryMap();
                StringTokenizer t = new StringTokenizer(val, ";&");
                while (t.hasMoreTokens()) {
                    String tok = t.nextToken();
                    int eq = tok.indexOf('=');
                    if (eq >= 0) {
                        String keyword = tok.substring(0, eq);
                        String value = tok.substring(eq + 1);
                        params.initialAppend(keyword, new StringItem(value));
                    } else {
                        params.initialAppend(tok, EmptySequence.getInstance());
                    }
                }
                dict.initialPut("queryParams", params);
            }
            val = uri.getRawFragment();
            if (val != null) {
                dict.initialPut("rawFragment", new StringItem(val));
            }
            val = uri.getFragment();
            if (val != null) {
                dict.initialPut("fragment", new StringItem(val));
            }
            val = uri.toASCIIString();
            if (val != null) {
                dict.initialPut("asciiString", new StringItem(val));
            }
            return dict;
        } catch (URISyntaxException e) {
            dict.initialPut("isValid", BooleanValue.FALSE);
            dict.initialPut("error", new StringItem(e.getMessage()));
            return dict;
        }
    }


}

// Copyright (c) 2020 Saxonica Limited
