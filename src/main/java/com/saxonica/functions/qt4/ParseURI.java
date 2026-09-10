package com.saxonica.functions.qt4;

import net.sf.saxon.expr.DefaultedArgumentExpression;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.Literal;
import net.sf.saxon.expr.StringLiteral;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.OptionsParameter;
import net.sf.saxon.ma.arrays.ArrayItem;
import net.sf.saxon.ma.arrays.SimpleArrayItem;
import net.sf.saxon.ma.map.DictionaryMap;
import net.sf.saxon.ma.map.HashTrieMap;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.AtomicIterator;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.BooleanValue;
import net.sf.saxon.value.SequenceType;
import net.sf.saxon.value.StringValue;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.HashSet;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

/**
 * Supports the fn:build-uri function.
 */
public class ParseURI extends URIFunctions {
    @Override
    public Expression makeFunctionCall(Expression... arguments) {
        Expression[] newArgs = new Expression[2];
        newArgs[0] = arguments[0];
        if (arguments.length < 2 || arguments[1] instanceof DefaultedArgumentExpression) {
            newArgs[1] = Literal.makeLiteral(new DictionaryMap());
        } else {
            newArgs[1] = arguments[1];
        }
        setArity(2);
        return super.makeFunctionCall(newArgs);
    }

    @Override
    public MapItem call(XPathContext context, Sequence[] arguments) throws XPathException {
        String uristr = arguments[0].head().getStringValue();
        HashTrieMap map = new HashTrieMap();

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

        final String scheme;
        if (uristr.matches("^[A-Za-z]:.*$")) {
            scheme = "file";
            map = map.addEntry(SCHEME, new StringValue(scheme));
            uristr = "/" + uristr;
        } else {
            if (uristr.matches("^[A-Za-z]+:.*$")) {
                pos = uristr.indexOf(":");
                scheme = uristr.substring(0, pos);
                map = map.addEntry(SCHEME, new StringValue(scheme));
                uristr = uristr.substring(pos + 1);
            } else {
                scheme = null;
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
            if (!"".equals(uristr)) {
                map = map.addEntry(HIERARCHICAL, BooleanValue.get(hierarchical));
            }
        }

        final String authority;
        if (uristr.matches("^//*[a-zA-Z]:.*$")) {
            authority = null;
            uristr = uristr.replaceAll("^/+", "/");
        } else {
            Pattern pattern = Pattern.compile("^(///*)([^/]+)(/.*)?$");
            Matcher matcher = pattern.matcher(uristr);
            if (matcher.find()) {
                if (matcher.group(3) == null) {
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
            map = map.addEntry(PATH, new StringValue(uristr));
            final ArrayList<GroundedValue> pathSegments = new ArrayList<>();
            // Can't use string split here because it collapses leading, trailing, and repeated delimiters
            pos = uristr.indexOf(pathSeparator);
            while (pos >= 0) {
                String seg = decodeString(uristr.substring(0,pos));
                if (pathSegments.size() == 0 && "tel".equals(scheme) && seg.startsWith(" ")) {
                    seg = "+" + seg.substring(1);
                }
                pathSegments.add(new StringValue(seg));
                uristr = uristr.substring(pos+pathSeparator.length());
                pos = uristr.indexOf(pathSeparator);
            }
            pathSegments.add(new StringValue(decodeString(uristr)));
            map = map.addEntry(PATH_SEGMENTS, new SimpleArrayItem(pathSegments));
        }

        if (query != null) {
            final ArrayList<GroundedValue> querySegments = new ArrayList<>();
            for (String str : query.split(querySeparator)) {
                HashTrieMap seg = new HashTrieMap();
                pos = str.indexOf("=");
                if (pos >= 0) {
                    String key = str.substring(0, pos);
                    String value = str.substring(pos+1);
                    seg = seg.addEntry(KEY, new StringValue(decodeString(key)));
                    seg = seg.addEntry(VALUE, new StringValue(decodeString(value)));
                } else {
                    seg = seg.addEntry(VALUE, new StringValue(decodeString(str)));
                }
                querySegments.add(seg);
            }
            map = map.addEntry(QUERY_SEGMENTS, new SimpleArrayItem(querySegments));
        }

        return map;
    }
}

// Copyright (c) 2009-2023 Saxonica Limited
