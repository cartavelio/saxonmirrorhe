package com.saxonica.functions.extfn;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.ma.map.DictionaryMap;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.*;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.StringValue;

/**
 * Implementation of the saxon:in-scope-namespaces extension function. Introduced in 10.0.
 * Returns a map of prefixes to URIs representing the in-scope namespaces of a supplied
 * element node. <code>saxon:in-scope-namespaces($E)</code> is equivalent to
 * <code>map:merge(in-scope-prefixes($E) ! map{., namespace-uri-for-prefix(., $E)})</code>,
 * but more efficient.
 */

public class InScopeNamespacesFn extends SystemFunction {

    @Override
    public ZeroOrOne<MapItem> call(XPathContext context, Sequence[] arguments) throws XPathException {
        NodeInfo element = (NodeInfo)arguments[0].head();
        NamespaceMap nsMap = element.getAllNamespaces();
        if (nsMap == null) {
            return ZeroOrOne.empty();
        }
        DictionaryMap dict = new DictionaryMap();
        for (NamespaceBinding binding : nsMap) {
            dict.initialPut(binding.getPrefix(),new StringValue(binding.getURI()));
        }
        return new ZeroOrOne<>(dict);
    }

}

// Copyright (c) 2018-2020 Saxonica Limited