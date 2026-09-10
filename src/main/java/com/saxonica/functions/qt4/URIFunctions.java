package com.saxonica.functions.qt4;

import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.StringValue;

import java.util.Arrays;
import java.util.HashMap;
import java.util.HashSet;

/**
 * Common declarations for BuildURI and ParseURI.
 */

abstract public class URIFunctions extends SystemFunction {
    protected static HashSet<String> nonHierarchicalSchemes = new HashSet<>(Arrays.asList("urn", "mailto"));
    protected static HashMap<String,String> defaultPorts = new HashMap<>();
    static {
        defaultPorts.put("http", "80");
        defaultPorts.put("https", "443");
        defaultPorts.put("ssh", "22");
        defaultPorts.put("ftp", "21");
    }

    protected static final AtomicValue PATH_SEPARATOR = new StringValue("path-separator");
    protected static final AtomicValue QUERY_SEPARATOR = new StringValue("query-separator");
    protected static final AtomicValue ALLOW_DEPRECATED_FEATURES = new StringValue("allow-deprecated-features");
    protected static final AtomicValue OMIT_DEFAULT_PORTS = new StringValue("omit-default-ports");
    protected static final AtomicValue SCHEME = new StringValue("scheme");
    protected static final AtomicValue AUTHORITY = new StringValue("authority");
    protected static final AtomicValue USERINFO = new StringValue("userinfo");
    protected static final AtomicValue HOST = new StringValue("host");
    protected static final AtomicValue PORT = new StringValue("port");
    protected static final AtomicValue PATH = new StringValue("path");
    protected static final AtomicValue PATH_SEGMENTS = new StringValue("path-segments");
    protected static final AtomicValue QUERY_SEGMENTS = new StringValue("query-segments");
    protected static final AtomicValue QUERY = new StringValue("query");
    protected static final AtomicValue KEY = new StringValue("key");
    protected static final AtomicValue VALUE = new StringValue("value");
    protected static final AtomicValue FRAGMENT = new StringValue("fragment");
}
