package com.saxonica.functions.qt4;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.ma.map.StringMapBuilder;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.str.EmptyUnicodeString;
import net.sf.saxon.str.StringView;
import net.sf.saxon.str.UnicodeString;
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
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        Item arg0 = arguments[0].head();
        if (arg0 == null) {
            return EmptySequence.getInstance();
        }
        String uristr = arg0.getStringValue();
        StringMapBuilder builder = new StringMapBuilder().unordered();

        builder.put(URI.getUnicodeStringValue(), new StringValue(uristr));
        uristr = uristr.replaceAll("\\\\", "/");

        try {
            checkOptions(arguments);
        } catch (IllegalArgumentException e) {
            throw new XPathException(e);
        }

        final String fragid;
        int pos = uristr.indexOf("#");
        if (pos >= 0) {
            fragid = DecodeFromURI.decode(uristr.substring(pos + 1));
            if (!fragid.trim().isEmpty()) {
                builder.put(FRAGMENT.getUnicodeStringValue(), new StringValue(fragid));
            }
            uristr = uristr.substring(0, pos);
        } else {
            fragid = null;
        }

        final String query;
        pos = uristr.indexOf("?");
        if (pos >= 0) {
            String querystr = uristr.substring(pos + 1).trim();
            if (querystr.isEmpty()) {
                query = null;
            } else {
                query = querystr;
                builder.put(QUERY.getUnicodeStringValue(), new StringValue(query));
            }
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

        boolean absolute = scheme != null && (fragid == null || fragid.trim().isEmpty());

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
            builder.put(SCHEME.getUnicodeStringValue(), new StringValue(scheme));
        }

        if (scheme != null && hierarchicalSchemes.contains(scheme)) {
            hierarchical = true;
            builder.put(HIERARCHICAL.getUnicodeStringValue(), BooleanValue.get(hierarchical));
        } else if (scheme != null && nonHierarchicalSchemes.contains(scheme)) {
            hierarchical = false;
            builder.put(HIERARCHICAL.getUnicodeStringValue(), BooleanValue.get(hierarchical));
        } else {
            hierarchical = uristr.startsWith("/");
            if (!uristr.isEmpty()) {
                builder.put(HIERARCHICAL.getUnicodeStringValue(), BooleanValue.get(hierarchical));
            }
        }

        if (!hierarchical) {
            absolute = false;
        }

        if (absolute) {
            builder.put(ABSOLUTE.getUnicodeStringValue(), BooleanValue.get(absolute));
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
            builder.put(AUTHORITY.getUnicodeStringValue(), new StringValue(authority));
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
                builder.put(USERINFO.getUnicodeStringValue(), new StringValue(userinfo));
            }

            pattern = Pattern.compile("^([^@]*@)?(\\[[^]]*])(:([^:]*))?$");
            matcher = pattern.matcher(authority);
            if (matcher.find()) {
                builder.put(HOST.getUnicodeStringValue(), new StringValue(matcher.group(2)));
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
                        builder.put(HOST.getUnicodeStringValue(), new StringValue(matcher.group(2)));
                        port = "".equals(matcher.group(4)) ? null : matcher.group(4);
                    }
                }
            }
        }

        if (port != null && omitDefaultPorts && port.equals(defaultPorts.get(scheme))) {
            port = null;
        }

        if (port != null) {
            builder.put(PORT.getUnicodeStringValue(), new StringValue(port));
        }

        if (!"".equals(uristr)) {
            if (filepath == null && (scheme == null || "file".equals(scheme))) {
                filepath = uristr;
            }
            builder.put(PATH.getUnicodeStringValue(), new StringValue(uristr));
            GroundedValue pathSegments = EmptySequence.getInstance();
            // Can't use string split here because it collapses leading, trailing, and repeated delimiters
            pos = uristr.indexOf("/");
            while (pos >= 0) {
                String seg = DecodeFromURI.decode(uristr.substring(0,pos));
                if (pathSegments.getLength() == 0 && "tel".equals(scheme) && seg.startsWith(" ")) {
                    seg = "+" + seg.substring(1);
                }
                pathSegments = pathSegments.concatenate(new StringValue(seg));
                uristr = uristr.substring(pos+1); // pathseparator is a single character
                pos = uristr.indexOf("/");
            }
            pathSegments = pathSegments.concatenate(new StringValue(DecodeFromURI.decode(uristr)));
            builder.put(PATH_SEGMENTS.getUnicodeStringValue(), pathSegments);
        }

        if (filepath != null) {
            String decoded = DecodeFromURI.decode(filepath);
            builder.put(FILEPATH.getUnicodeStringValue(), new StringValue(decoded));
        }

        if (query != null) {
            StringMapBuilder querySegments = new StringMapBuilder();
            querySegments.setCombiner(GroundedValue::concatenate);
            for (String str : query.split("&")) {
                pos = str.indexOf("=");
                final UnicodeString key;
                final StringValue value;
                if (pos >= 0) {
                    key = StringView.of(DecodeFromURI.decode(str.substring(0, pos)));
                    value = new StringValue(DecodeFromURI.decode(str.substring(pos+1)));
                } else {
                    key = EmptyUnicodeString.getInstance();
                    value = new StringValue(DecodeFromURI.decode(str));
                }
                querySegments.put(key, value);
            }
            builder.put(QUERY_PARAMETERS.getUnicodeStringValue(), querySegments.getCompletedMap());
        }

        return builder.getCompletedMapConfidently();
    }
}

// Copyright (c) 2023-2024 Saxonica Limited
