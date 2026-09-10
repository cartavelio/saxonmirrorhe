package com.saxonica.functions.qt4;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.ma.map.HashTrieMap;
import net.sf.saxon.om.NamespaceBinding;
import net.sf.saxon.om.NamespaceMap;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.value.EmptySequence;
import net.sf.saxon.value.StringValue;

/**
 * Implementation of the saxon:in-scope-namespaces extension function. Introduced in 10.0.
 * Also available as experimental XPath 4.0 function fn:in-scope-namespaces().
 * Returns a map of prefixes to URIs representing the in-scope namespaces of a supplied
 * element node. <code>saxon:in-scope-namespaces($E)</code> is equivalent to
 * <code>map:merge(in-scope-prefixes($E) ! map{., namespace-uri-for-prefix(., $E)})</code>,
 * but more efficient.
 */

public class InScopeNamespacesFn extends SystemFunction {

    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        NodeInfo element = (NodeInfo)arguments[0].head();
        NamespaceMap nsMap = element.getAllNamespaces();
        if (nsMap == null) {
            return EmptySequence.getInstance();
        }
        // Can't use a dictionary map because the prefixes are of type NCName, not string
        HashTrieMap dict = new HashTrieMap();
        dict.initialPut(new StringValue("xml", BuiltInAtomicType.NCNAME), new StringValue(NamespaceConstant.XML));
        for (NamespaceBinding binding : nsMap) {
            StringValue prefix = new StringValue(binding.getPrefix(),
                                                 binding.getPrefix().isEmpty() ? BuiltInAtomicType.STRING : BuiltInAtomicType.NCNAME);
            dict.initialPut(prefix, new StringValue(binding.getNamespaceUri().toUnicodeString()));
        }
        return dict;
    }

}

// Copyright (c) 2018-2022 Saxonica Limited