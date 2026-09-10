package com.saxonica.serialize;

import net.sf.saxon.event.ProxyReceiver;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.expr.parser.Location;
import net.sf.saxon.expr.sort.GenericSorter;
import net.sf.saxon.expr.sort.Sortable;
import net.sf.saxon.om.NamespaceBinding;
import net.sf.saxon.om.NamespaceBindingSet;
import net.sf.saxon.om.NodeName;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.SchemaType;

import java.util.ArrayList;
import java.util.List;

/**
 * NamespaceSorter: This filter sorts namespace declarations into alphabetical order, required
 * for XML Canonicalization
 *
 * @author Michael Kay
 */


public class NamespaceSorter extends ProxyReceiver implements Sortable {

    private List<NamespaceBinding> namespaces = new ArrayList<>();

    /**
     * Create a namespace sorter
     *
     * @param next the next receiver in the pipeline
     */

    public NamespaceSorter(Receiver next) {
        super(next);
    }

    /**
     * Notify the start of an element
     * @param elemName   integer code identifying the name of the element within the name pool.
     * @param typeCode   integer code identifying the element's type within the name pool.
     * @param location   the element's line &amp; column number
     * @param properties properties of the element node
     */
    @Override
    public void startElement(NodeName elemName, SchemaType typeCode, Location location, int properties) throws XPathException {
        namespaces.clear();
        super.startElement(elemName, typeCode, location, properties);
    }

    @Override
    public void namespace(NamespaceBindingSet namespaceBindings, int properties) throws XPathException {
        for (NamespaceBinding binding : namespaceBindings) {
            namespaces.add(binding);
        }
    }

    /**
     * Notify the start of the content, that is, the completion of all attributes and namespaces.
     * Note that the initial receiver of output from XSLT instructions will not receive this event,
     * it has to detect it itself. Note that this event is reported for every element even if it has
     * no attributes, no namespaces, and no content.
     */
    @Override
    public void startContent() throws XPathException {
        if (namespaces.size() > 1) {
            GenericSorter.quickSort(0, namespaces.size(), this);
        }
        for (NamespaceBinding binding : namespaces) {
            nextReceiver.namespace(binding, 0);
        }
        super.startContent();
    }

    /**
     * Compare two objects within this Sortable, identified by their position.
     *
     * @return &lt;0 if obj[a]&lt;obj[b], 0 if obj[a]=obj[b], &gt;0 if obj[a]&gt;obj[b]
     */
    public int compare(int a, int b) {
        String p1 = namespaces.get(a).getPrefix();
        String p2 = namespaces.get(b).getPrefix();
        return p1.compareTo(p2);
    }

    /**
     * Swap two objects within this Sortable, identified by their position.
     */
    public void swap(int a, int b) {
        NamespaceBinding temp = namespaces.get(a);
        namespaces.set(a, namespaces.get(b));
        namespaces.set(b, temp);
    }
}

// Copyright (c) 2018 Saxonica Limited

