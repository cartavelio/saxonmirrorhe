package com.saxonica.functions.qt4;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.ma.map.KeyValuePair;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.ma.map.MapType;
import net.sf.saxon.ma.map.RecordTest;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.value.SequenceType;
import net.sf.saxon.value.StringValue;

/**
 * Supports the fn:build-uri function.
 */
public class BuildURI extends URIFunctions {

    private static RecordTest.Field f(String name, SequenceType type, boolean optional) {
        return new RecordTest.Field(name, type, optional);
    }

    private static final MapType STRING_MAP_TYPE = new MapType(BuiltInAtomicType.STRING, SequenceType.STRING_SEQUENCE);

    public static RecordTest uriStructureRecord = RecordTest.extensible(
            f("uri", SequenceType.SINGLE_STRING, true),
            f("scheme", SequenceType.SINGLE_STRING, true),
            f("authority", SequenceType.SINGLE_STRING, true),
            f("userinfo", SequenceType.SINGLE_STRING, true),
            f("host", SequenceType.SINGLE_STRING, true),
            f("port", SequenceType.SINGLE_STRING, true),
            f("path", SequenceType.SINGLE_STRING, true),
            f("query", SequenceType.SINGLE_STRING, true),
            f("fragment", SequenceType.SINGLE_STRING, true),
            f("path-segments", SequenceType.STRING_SEQUENCE, true),
            f("query-parameters", SequenceType.one(STRING_MAP_TYPE), true));


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
            uri.append(hierarchical ? encodePathSegment(value.itemAt(0)) : value.itemAt(0).getStringValue());
            for (int pos = 1; pos < value.getLength(); pos++) {
                uri.append(pathSeparator);
                uri.append(encodePathSegment(value.itemAt(pos)));
            }
        } else if (parts.get(PATH) != null) {
            uri.append(parts.get(PATH).getStringValue());
        }

        if (parts.get(QUERY_PARAMETERS) != null) {
            char sep = '?';
            for (KeyValuePair pair : ((MapItem) parts.get(QUERY_PARAMETERS)).keyValuePairs()) {
                if (pair.key != null || pair.value != null) {
                    uri.append(sep);
                    sep = querySeparator;
                    if (pair.key != null && !pair.key.getStringValue().isEmpty()) {
                        if (pair.value != null && pair.value.getLength() > 0) {
                            for (int idx = 0; idx < pair.value.getLength(); idx++) {
                                if (idx > 0) {
                                    uri.append(sep);
                                }
                                uri.append(encodeQueryPart(pair.key));
                                uri.append("=");
                                uri.append(encodeQueryPart(pair.value.itemAt(idx)));
                            }
                        } else {
                            uri.append(encodeQueryPart(pair.key));
                        }
                    } else {
                        uri.append(encodeQueryPart(pair.value));
                    }
                }
            }
        } else if (parts.get(QUERY) != null) {
            uri.append("?").append(parts.get(QUERY).getStringValue());
        }

        if (parts.get(FRAGMENT) != null) {
            uri.append("#").append(encodeFragmentIdentifier(parts.get(FRAGMENT)));
        }

        return new StringValue(uri.toString());
    }
}

// Copyright (c) 2009-2023 Saxonica Limited
