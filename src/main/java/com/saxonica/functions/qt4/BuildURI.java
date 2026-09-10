package com.saxonica.functions.qt4;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.ma.arrays.ArrayItem;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.StringValue;

import java.util.Arrays;
import java.util.HashMap;
import java.util.HashSet;

/**
 * Supports the fn:build-uri function.
 */
public class BuildURI extends URIFunctions {
    private static final HashSet<Character> reservedChars
            = new HashSet<>(Arrays.asList(':', '/', '?', '#', '[', ']', '@', // gen-delims
            '!', '$', '&', '\'', '(', ')', '*', '+', ',', ';', '='));    // sub-delmis

    @Override
    public StringValue call(XPathContext context, Sequence[] arguments) throws XPathException {
        MapItem parts = (MapItem) arguments[0].head();

        final String pathSeparator;
        final String querySeparator;
        final boolean allowDeprecatedFeatures;
        final boolean omitDefaultPorts;
        final MapItem options;
        if (getArity() == 2) {
            options = (MapItem) arguments[1].head();
            Object x = options.get(PATH_SEPARATOR);
            pathSeparator = options.get(PATH_SEPARATOR) == null ? "/" : options.get(PATH_SEPARATOR).getStringValue();
            querySeparator = options.get(QUERY_SEPARATOR) == null ? "&" : options.get(QUERY_SEPARATOR).getStringValue();
            allowDeprecatedFeatures = options.get(ALLOW_DEPRECATED_FEATURES) != null && options.get(ALLOW_DEPRECATED_FEATURES).effectiveBooleanValue();
            omitDefaultPorts = options.get(OMIT_DEFAULT_PORTS) != null && options.get(OMIT_DEFAULT_PORTS).effectiveBooleanValue();
        } else {
            options = null;
            pathSeparator = "/";
            querySeparator = "&";
            allowDeprecatedFeatures = false;
            omitDefaultPorts = false;
        }

        StringBuilder uri = new StringBuilder();
        String scheme = "";
        if (parts.get(SCHEME) != null) {
            scheme = parts.get(SCHEME).getStringValue();
            uri.append(scheme);
            if (nonHierarchicalSchemes.contains(parts.get(SCHEME).getStringValue())) {
                uri.append(":");
            } else {
                uri.append("://");
            }
        } else {
            uri.append("//");
        }

        if (parts.get(USERINFO) != null || parts.get(HOST) != null || parts.get(PORT) != null) {
            String userinfo = parts.get(USERINFO) == null ? "" : parts.get(USERINFO).getStringValue();
            String host = parts.get(HOST) == null ? "" : parts.get(HOST).getStringValue();
            String port = parts.get(PORT) == null ? "" : parts.get(PORT).getStringValue();

            if (!allowDeprecatedFeatures) {
                int pos = userinfo.indexOf(":");
                if (pos >= 0 && pos != userinfo.length()-1) {
                    userinfo = "";
                }
            }

            if (!"".equals(userinfo)) {
                uri.append(userinfo).append("@");
            }

            uri.append(host);

            if (!"".equals(port)) {
                if (!omitDefaultPorts || !defaultPorts.containsKey(scheme) || !port.equals(defaultPorts.get(scheme))) {
                    uri.append(":").append(port);
                }
            }
        } else {
            uri.append(parts.get(AUTHORITY) == null ? "" : parts.get(AUTHORITY).getStringValue());
        }

        if (parts.get(PATH_SEGMENTS) != null) {
            ArrayItem value = (ArrayItem) parts.get(PATH_SEGMENTS);
            for (int pos = 0; pos < value.arrayLength(); pos++) {
                uri.append(pathSeparator);
                uri.append(encodeString(value.get(pos)));
            }
        } else if (parts.get(PATH) != null) {
            uri.append(parts.get(PATH).getStringValue());
        }

        if (parts.get(QUERY_SEGMENTS) != null) {
            String sep = "?";
            ArrayItem value = (ArrayItem) parts.get(QUERY_SEGMENTS);
            for (int pos = 0; pos < value.arrayLength(); pos++) {
                MapItem param = (MapItem) value.get(pos);
                if (param.get(KEY) != null || param.get(VALUE) != null) {
                    uri.append(sep);
                    sep = querySeparator;
                    if (param.get(KEY) != null) {
                        if (param.get(VALUE) != null) {
                            uri.append(encodeString(param.get(KEY)));
                            uri.append("=");
                            uri.append(encodeString(param.get(VALUE)));
                        } else {
                            uri.append(encodeString(param.get(KEY)));
                        }
                    } else {
                        uri.append(encodeString(param.get(VALUE)));
                    }
                }
            }
        } else if (parts.get(QUERY) != null) {
            uri.append("?").append(parts.get(QUERY).getStringValue());
        }

        if (parts.get(FRAGMENT) != null) {
            uri.append("#").append(encodeString(parts.get(FRAGMENT)));
        }

        System.err.println(uri);
        return new StringValue(uri.toString());
    }

    private String encodeString(GroundedValue value) throws XPathException {
        String input = value.getStringValue();
        StringBuilder output = new StringBuilder();
        for (int pos = 0; pos < input.length(); pos++) {
            char ch = input.charAt(pos);
            if (ch == ' ') {
                output.append("+");
            } else if (reservedChars.contains(ch)) {
                output.append(String.format("%%%02X", (int) ch));
            } else {
                output.append(ch);
            }
        }
        return output.toString();
    }
}

// Copyright (c) 2009-2023 Saxonica Limited
