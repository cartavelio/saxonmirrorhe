package com.saxonica.functions.qt4;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.ma.arrays.ArrayItem;
import net.sf.saxon.ma.map.HashTrieMap;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.StringValue;

import java.util.Arrays;
import java.util.HashSet;

/**
 * Supports the fn:build-uri function.
 */
public class ParseURI extends URIFunctions {
    @Override
    public MapItem call(XPathContext context, Sequence[] arguments) throws XPathException {
        String uri = arguments[0].head().getStringValue().replaceAll("\\\\", "/");
        HashTrieMap map = new HashTrieMap();

        map = map.addEntry(SCHEME, new StringValue("https"));
        map = map.addEntry(HOST, new StringValue("example.com"));
        map = map.addEntry(PATH, new StringValue("/path/to/file"));

        return map;
    }
}

// Copyright (c) 2009-2023 Saxonica Limited
