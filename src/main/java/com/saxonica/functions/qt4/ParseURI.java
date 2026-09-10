package com.saxonica.functions.qt4;

import net.sf.saxon.expr.DefaultedArgumentExpression;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.Literal;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.ma.arrays.SimpleArrayItem;
import net.sf.saxon.ma.map.DictionaryMap;
import net.sf.saxon.ma.map.HashTrieMap;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.s9api.XdmAtomicValue;
import net.sf.saxon.s9api.XdmMap;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.BooleanValue;
import net.sf.saxon.value.EmptySequence;
import net.sf.saxon.value.SequenceExtent;
import net.sf.saxon.value.StringValue;

import java.util.ArrayList;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

/**
 * Supports the fn:build-uri function.
 */
public class ParseURI extends URIFunctions {
    @Override
    public MapItem call(XPathContext context, Sequence[] arguments) throws XPathException {
        String uristr = arguments[0].head().getStringValue();
        MapItem map = new HashTrieMap();

        map = map.addEntry(URI, new StringValue(uristr));
        uristr = uristr.replaceAll("\\\\", "/");

        checkOptions(arguments);

        int pos = uristr.indexOf("#");
        if (pos >= 0) {
            map = map.addEntry(FRAGMENT, new StringValue(uristr.substring(pos + 1)));
            uristr = uristr.substring(0, pos);
        }

        final String query;
        pos = uristr.indexOf("?");
        if (pos >= 0) {
            query = uristr.substring(pos + 1);
            map = map.addEntry(QUERY, new StringValue(query));
            uristr = uristr.substring(0, pos);
        } else {
            query = null;
        }

        String filepath = null;
        final String scheme;
        if (uristr.matches("^[A-Za-z][:|].*$")) {
            scheme = "file";
            // Force the second character to be a colon
            uristr = uristr.charAt(0) + ":" + uristr.substring(2);
            map = map.addEntry(SCHEME, new StringValue(scheme));
            filepath = uristr;
            uristr = "/" + uristr;
        } else {
            if (uristr.matches("^[A-Za-z]+:.*$")) {
                pos = uristr.indexOf(":");
                scheme = uristr.substring(0, pos);
                map = map.addEntry(SCHEME, new StringValue(scheme));
                uristr = uristr.substring(pos + 1);
            } else {
                if (uncPath && uristr.matches("^//[^/].*$")) {
                    scheme = "file";
                    map = map.addEntry(SCHEME, new StringValue(scheme));
                    filepath = uristr;
                } else {
                    scheme = null;
                }
            }
        }

        if (scheme != null && hierarchicalSchemes.contains(scheme)) {
            hierarchical = true;
            map = map.addEntry(HIERARCHICAL, BooleanValue.get(hierarchical));
        } else if (scheme != null && nonHierarchicalSchemes.contains(scheme)) {
            hierarchical = false;
            map = map.addEntry(HIERARCHICAL, BooleanValue.get(hierarchical));
        } else {
            hierarchical = uristr.startsWith("/");
            if (!uristr.isEmpty()) {
                map = map.addEntry(HIERARCHICAL, BooleanValue.get(hierarchical));
            }
        }

        final String authority;
        if ((scheme == null || "file".equals(scheme)) && uristr.matches("^//*[a-zA-Z][:|].*$")) {
            authority = null;
            uristr = uristr.replaceAll("^/+([a-zA-Z]).", "/$1:");
            filepath = uristr.substring(1);
        } else {
            Pattern pattern = Pattern.compile("^(///*)([^/]+)(/.*)?$");
            Matcher matcher = pattern.matcher(uristr);
            if (matcher.find()) {
                if (matcher.group(3) == null || matcher.group(3).isEmpty()) {  // second condition is for transpiled code
                    if (matcher.group(1).length() == 2) {
                        authority = matcher.group(2);
                        map = map.addEntry(AUTHORITY, new StringValue(authority));
                        uristr = "";
                    } else {
                        authority = null;
                        uristr = "/" + matcher.group(2);
                    }
                } else {
                    authority = matcher.group(2);
                    map = map.addEntry(AUTHORITY, new StringValue(authority));
                    uristr = matcher.group(3);
                }
            } else {
                authority = null;
            }
        }

        String port = null;
        if (authority != null) {
            final String userinfo;

            pos = authority.indexOf("@");
            if (pos >= 0 && (allowDeprecatedFeatures || !authority.substring(0, pos).contains(":"))) {
                userinfo = authority.substring(0, pos);
            } else {
                userinfo = null;
            }

            if (userinfo != null) {
                map = map.addEntry(USERINFO, new StringValue(userinfo));
            }

            Pattern pattern = Pattern.compile("^([^@]*@)?(\\[[^]]*])(:([^:]*))?$");
            Matcher matcher = pattern.matcher(authority);
            if (matcher.find()) {
                map = map.addEntry(HOST, new StringValue(matcher.group(2)));
                port = "".equals(matcher.group(4)) ? null : matcher.group(4);
            } else {
                pattern = Pattern.compile("^([^@]*@)?\\[.*$");
                matcher = pattern.matcher(authority);
                if (matcher.find()) {
                    throw new RuntimeException("Unparsable authority component");
                } else {
                    pattern = Pattern.compile("^([^@]*@)?([^:]+)(:([^:]*))?$");
                    matcher = pattern.matcher(authority);
                    if (matcher.find()) {
                        map = map.addEntry(HOST, new StringValue(matcher.group(2)));
                        port = "".equals(matcher.group(4)) ? null : matcher.group(4);
                    }
                }
            }
        }

        if (port != null && omitDefaultPorts && port.equals(defaultPorts.get(scheme))) {
            port = null;
        }

        if (port != null) {
            map = map.addEntry(PORT, new StringValue(port));
        }

        if (!"".equals(uristr)) {
            if (filepath == null && (scheme == null || "file".equals(scheme))) {
                filepath = uristr;
            }
            map = map.addEntry(PATH, new StringValue(uristr));
            GroundedValue pathSegments = EmptySequence.getInstance();
            // Can't use string split here because it collapses leading, trailing, and repeated delimiters
            pos = uristr.indexOf(pathSeparator);
            while (pos >= 0) {
                String seg = DecodeFromURI.decode(uristr.substring(0,pos));
                if (pathSegments.getLength() == 0 && "tel".equals(scheme) && seg.startsWith(" ")) {
                    seg = "+" + seg.substring(1);
                }
                pathSegments = pathSegments.concatenate(new StringValue(seg));
                uristr = uristr.substring(pos+1); // pathseparator is a single character
                pos = uristr.indexOf(pathSeparator);
            }
            pathSegments = pathSegments.concatenate(new StringValue(DecodeFromURI.decode(uristr)));
            map = map.addEntry(PATH_SEGMENTS, pathSegments);
        }

        if (filepath != null) {
             filepath = DecodeFromURI.decode(filepath);
            map = map.addEntry(FILEPATH, new StringValue(filepath));
        }

        if (query != null) {
            HashTrieMap querySegments = new HashTrieMap();
            for (String str : query.split(""+querySeparator)) {
                pos = str.indexOf("=");
                final StringValue key;
                final StringValue value;
                if (pos >= 0) {
                    key = new StringValue(DecodeFromURI.decode(str.substring(0, pos)));
                    value = new StringValue(DecodeFromURI.decode(str.substring(pos+1)));
                } else {
                    key = new StringValue("");
                    value = new StringValue(DecodeFromURI.decode(str));
                }

                GroundedValue currentValue = querySegments.get(key);
                if (currentValue == null) {
                    querySegments = querySegments.addEntry(key, value);
                } else {
                    querySegments = querySegments.addEntry(key, currentValue.concatenate(value));
                }
            }
            map = map.addEntry(QUERY_PARAMETERS, querySegments);
        }

        return map;
    }
}

// Copyright (c) 2009-2023 Saxonica Limited
