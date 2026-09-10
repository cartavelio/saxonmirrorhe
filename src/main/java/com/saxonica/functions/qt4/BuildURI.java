package com.saxonica.functions.qt4;

import com.ibm.icu.util.ValueIterator;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.ma.arrays.ArrayItem;
import net.sf.saxon.ma.map.KeyValuePair;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.StringValue;

import java.util.ArrayList;

/**
 * Supports the fn:build-uri function.
 */
public class BuildURI extends URIFunctions {
    @Override
    public StringValue call(XPathContext context, Sequence[] arguments) throws XPathException {
        MapItem parts = (MapItem) arguments[0].head();

        checkOptions(arguments);

        StringBuilder uri = new StringBuilder();
        String scheme = "";
        if (parts.get(SCHEME) != null) {
            scheme = parts.get(SCHEME).getStringValue();
            uri.append(scheme);
            if (nonHierarchicalSchemes.contains(parts.get(SCHEME).getStringValue())) {
                hierarchical = false;
            }
        }

        if (parts.get(HIERARCHICAL) != null) {
            hierarchical = parts.get(HIERARCHICAL).effectiveBooleanValue();
        }

        if (!"".equals(scheme)) {
            if (hierarchical) {
                uri.append("://");
            } else {
                uri.append(":");
            }
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
            GroundedValue value = parts.get(PATH_SEGMENTS);
            int pos = 0;
            try (SequenceIterator iter = value.iterate()) {
                GroundedValue svalue = iter.next();
                while (svalue != null) {
                    if (pos == 0) {
                        uri.append(hierarchical ? encodePathSegment(svalue) : svalue.getStringValue());
                    } else {
                        uri.append(pathSeparator);
                        uri.append(encodePathSegment(svalue));
                    }
                    pos = pos + 1;
                    svalue = iter.next();
                }
            }
       } else if (parts.get(PATH) != null) {
            uri.append(parts.get(PATH).getStringValue());
        }

        if (parts.get(QUERY_PARAMETERS) != null) {
            String sep = "?";
            MapItem qmap = (MapItem) parts.get(QUERY_PARAMETERS).head();
            try (SequenceIterator kiter = qmap.keys()) {
                GroundedValue kvalue = kiter.next();
                while (kvalue != null) {
                    String key = kvalue.getStringValue();
                    GroundedValue vvalue = qmap.get((AtomicValue) kvalue);
                    try (SequenceIterator viter = vvalue.iterate()) {
                        GroundedValue value = viter.next();
                        while (value != null) {
                            uri.append(sep);
                            sep = querySeparator;
                            if (key.isEmpty()) {
                                uri.append(encodeOtherSegment(value));
                            } else {
                                uri.append(encodeOtherSegment(kvalue));
                                uri.append("=");
                                uri.append(encodeOtherSegment(value));
                            }
                            value = viter.next();
                        }
                    }
                    kvalue = kiter.next();
                }
            }
        } else if (parts.get(QUERY) != null) {
            uri.append("?").append(parts.get(QUERY).getStringValue());
        }

        if (parts.get(FRAGMENT) != null) {
            uri.append("#").append(encodeOtherSegment(parts.get(FRAGMENT)));
        }

        return new StringValue(uri.toString());
    }
}

// Copyright (c) 2009-2023 Saxonica Limited
