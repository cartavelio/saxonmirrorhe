package com.saxonica.functions.qt4;


import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.ma.arrays.ArrayItem;
import net.sf.saxon.ma.arrays.ArrayItemType;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.ma.map.RecordTest;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.SequenceType;
import net.sf.saxon.value.StringValue;

/**
 * Supports the fn:build-uri function.
 */
public class BuildURI extends URIFunctions {

    private static RecordTest.Field f(String name, SequenceType type, boolean optional) {
        return new RecordTest.Field(name, type, optional);
    }

    private static RecordTest keyValueRecord = RecordTest.extensible(
            f("key", SequenceType.SINGLE_STRING, true),
            f("value", SequenceType.SINGLE_STRING, true)
    );

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
            f("path-segments", SequenceType.one(new ArrayItemType(SequenceType.SINGLE_STRING)), true),
            f("query-parameters", SequenceType.one(new ArrayItemType(SequenceType.one(keyValueRecord))), true));


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
            ArrayItem value = (ArrayItem) parts.get(PATH_SEGMENTS);
            uri.append(hierarchical ? encodePathSegment(value.get(0)) : value.get(0).getStringValue());
            for (int pos = 1; pos < value.arrayLength(); pos++) {
                uri.append(pathSeparator);
                uri.append(encodePathSegment(value.get(pos)));
            }
        } else if (parts.get(PATH) != null) {
            uri.append(parts.get(PATH).getStringValue());
        }

        if (parts.get(QUERY_PARAMETERS) != null) {
            String sep = "?";
            ArrayItem value = (ArrayItem) parts.get(QUERY_PARAMETERS);
            for (int pos = 0; pos < value.arrayLength(); pos++) {
                MapItem param = (MapItem) value.get(pos);
                if (param.get(KEY) != null || param.get(VALUE) != null) {
                    uri.append(sep);
                    sep = querySeparator;
                    if (param.get(KEY) != null) {
                        if (param.get(VALUE) != null) {
                            uri.append(encodeOtherSegment(param.get(KEY)));
                            uri.append("=");
                            uri.append(encodeOtherSegment(param.get(VALUE)));
                        } else {
                            uri.append(encodeOtherSegment(param.get(KEY)));
                        }
                    } else {
                        uri.append(encodeOtherSegment(param.get(VALUE)));
                    }
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
