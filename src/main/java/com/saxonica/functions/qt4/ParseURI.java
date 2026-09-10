package com.saxonica.functions.qt4;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.ma.map.HashTrieMap;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.BooleanValue;
import net.sf.saxon.value.EmptySequence;
import net.sf.saxon.value.StringValue;

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

        try {
            checkOptions(arguments);
        } catch (IllegalArgumentException e) {
            throw new XPathException(e);
        }

        int pos = uristr.indexOf("#");
        if (pos >= 0) {
            map = map.addEntry(FRAGMENT, new StringValue(DecodeFromURI.decode(uristr.substring(pos + 1))));
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

        String scheme = null;

        Pattern pattern = Pattern.compile("^([A-Za-z][-+.A-Za-z0-9]+):(.*)$");
        Matcher matcher = pattern.matcher(uristr);
        if (matcher.matches()) {
            scheme = matcher.group(1).toLowerCase();
            uristr = matcher.group(2);
        }

        if (scheme == null || "file".equals(scheme)) {
            pattern = Pattern.compile("^/*([A-Za-z])[:|](.*)$");
            matcher = pattern.matcher(uristr);
            if (matcher.find()) {
                scheme = "file";
                uristr = "/" + matcher.group(1) + ":" + matcher.group(2);
            } else {
                if (uncPath) {
                    scheme = "file";
                }
            }
        }

        if (scheme != null) {
            map = map.addEntry(SCHEME, new StringValue(scheme));
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

        String filepath = null;
        final String authority;
        if ("file".equals(scheme)) {
            authority = null;
            pattern = Pattern.compile("^/*(//[^/].*)$");
            matcher = pattern.matcher(uristr);
            if (uncPath && matcher.find()) {
                uristr = matcher.group(1);
                filepath = uristr;
            } else {
                uristr = uristr.replaceAll("^/+", "/");
                if (uristr.matches("^/[A-Za-z]:.*$")) {
                    filepath = uristr.substring(1);
                } else {
                    filepath = uristr;
                }
            }
        } else if (hierarchical) {
            pattern = Pattern.compile("^//([^/]+)$");
            matcher = pattern.matcher(uristr);
            if (matcher.find()) {
                authority = matcher.group(1);
                uristr = "";
            } else {
                pattern = Pattern.compile("^//([^/]*)(/.*)$");
                matcher = pattern.matcher(uristr);
                if (matcher.find()) {
                    if (matcher.group(1) != null && !matcher.group(1).isEmpty()) {
                        authority = matcher.group(1);
                    } else {
                        authority = null;
                    }
                    uristr = matcher.group(2);
                } else {
                    authority = null;
                }
            }
        } else {
            authority = null;
        }

        if (authority != null) {
            map = map.addEntry(AUTHORITY, new StringValue(authority));
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

            pattern = Pattern.compile("^([^@]*@)?(\\[[^]]*])(:([^:]*))?$");
            matcher = pattern.matcher(authority);
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
             String decoded = DecodeFromURI.decode(filepath);
            map = map.addEntry(FILEPATH, new StringValue(decoded));
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
                    // Cast needed for transpiled C#
                    querySegments = (HashTrieMap) querySegments.addEntry(key, value);
                } else {
                    querySegments = (HashTrieMap) querySegments.addEntry(key, currentValue.concatenate(value));
                }
            }
            map = map.addEntry(QUERY_PARAMETERS, querySegments);
        }

        return map;
    }
}

// Copyright (c) 2023-2024 Saxonica Limited
