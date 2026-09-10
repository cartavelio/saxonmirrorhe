package com.saxonica.serialize;

import net.sf.saxon.expr.parser.Location;
import net.sf.saxon.ma.map.KeyValuePair;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.om.FingerprintedQName;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.query.SequenceWrapper;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.Untyped;

/**
 * This class can be used in a push pipeline: it accepts any sequence as input, and generates
 * a document in which the items of the sequence are wrapped by elements containing information about
 * the types of the items in the input sequence.
 * <p>This class adds to the superclass the ability to serialize maps.</p>
 */

public class SequenceWrapperPE extends SequenceWrapper {

    private FingerprintedQName resultMap;
    private FingerprintedQName resultMapEntry;
    private FingerprintedQName resultMapKey;
    private FingerprintedQName resultMapValue;

    /**
     * Create a sequence wrapper. This creates an XML representation of the items sent to destination
     * in which the types of all items are made explicit
     *
     * @param destination the sequence being wrapped
     */

    public SequenceWrapperPE(Receiver destination) {
        super(destination);
    }

    public void open() throws XPathException {
        super.open();
        resultMap = new FingerprintedQName("result", RESULT_NS, "map");
        resultMapEntry = new FingerprintedQName("result", RESULT_NS, "map-entry");
        resultMapKey = new FingerprintedQName("result", RESULT_NS, "map-key");
        resultMapValue = new FingerprintedQName("result", RESULT_NS, "map-value");
    }


    /**
     * Output an item (atomic value or node) to the sequence
     */

    public void append(/*@NotNull*/ Item item, Location locationId, int copyNamespaces) throws XPathException {
        if (item instanceof MapItem) {
            Receiver out = getDestination();
            out.startElement(resultMap, Untyped.getInstance(), locationId, 0);
            MapItem map = (MapItem) item;
            for (KeyValuePair pair : map.keyValuePairs()) {
                out.startElement(resultMapEntry, Untyped.getInstance(), locationId, 0);
                out.startElement(resultMapKey, Untyped.getInstance(), locationId, 0);
                append(pair.key);
                out.endElement();
                out.startElement(resultMapValue, Untyped.getInstance(), locationId, 0);
                SequenceIterator value = pair.value.iterate();
                Item valItem;
                while ((valItem = value.next()) != null) {
                    append(valItem);
                }
                out.endElement();
                out.endElement();
            }
            out.endElement();
        } else {
            super.append(item, locationId, copyNamespaces);
        }
    }

}

// Copyright (c) 2018 Saxonica Limited.

