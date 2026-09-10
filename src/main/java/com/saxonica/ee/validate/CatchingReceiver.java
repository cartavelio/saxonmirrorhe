package com.saxonica.ee.validate;


import net.sf.saxon.event.ProxyReceiver;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.expr.parser.Location;
import net.sf.saxon.lib.UnfailingErrorListener;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.NamespaceBindingSet;
import net.sf.saxon.om.NodeName;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.SchemaType;
import net.sf.saxon.type.SimpleType;

/**
 * This class catches all errors thrown by a Receiver pipeline, and reports them to a specified
 * ErrorListener; after an error has occurred, all further events are ignored. The class is added
 * to a validation pipeline when validation of a streamed input document is requested, to allow
 * a try/catch expression to catch validation errors.
 */

public class CatchingReceiver extends ProxyReceiver {

    private UnfailingErrorListener listener;
    private boolean failed = false;

    public CatchingReceiver(Receiver next, UnfailingErrorListener listener) {
        super(next);
        this.listener = listener;
    }

    @Override
    public void open() {
        if (!failed) {
            try {
                super.open();
            } catch (XPathException err) {
                failed = true;
                listener.error(err);
            }
        }
    }

    @Override
    public void close() {
        if (!failed) {
            try {
                super.close();
            } catch (XPathException err) {
                failed = true;
                listener.error(err);
            }
        }
    }

    @Override
    public void startDocument(int properties) {
        if (!failed) {
            try {
                super.startDocument(properties);
            } catch (XPathException err) {
                failed = true;
                listener.error(err);
            }
        }
    }

    @Override
    public void endDocument() {
        if (!failed) {
            try {
                super.endDocument();
            } catch (XPathException err) {
                failed = true;
                listener.error(err);
            }
        }
    }

    @Override
    public void startElement(NodeName elemName, SchemaType typeCode, Location location, int properties)  {
        if (!failed) {
            try {
                super.startElement(elemName, typeCode, location, properties);
            } catch (XPathException err) {
                failed = true;
                listener.error(err);
            }
        }
    }

    @Override
    public void namespace(NamespaceBindingSet namespaceBindings, int properties) {
         if (!failed) {
            try {
                super.namespace(namespaceBindings, properties);
            } catch (XPathException err) {
                failed = true;
                listener.error(err);
            }
        }
    }

    @Override
    public void attribute(NodeName nameCode, SimpleType typeCode, CharSequence value, Location locationId, int properties) {
        if (!failed) {
            try {
                super.attribute(nameCode, typeCode, value, locationId, properties);
            } catch (XPathException err) {
                failed = true;
                listener.error(err);
            }
        }
    }

    @Override
    public void startContent()  {
        if (!failed) {
            try {
                super.startContent();
            } catch (XPathException err) {
                failed = true;
                listener.error(err);
            }
        }
    }

    @Override
    public void endElement() {
        if (!failed) {
            try {
                super.endElement();
            } catch (XPathException err) {
                failed = true;
                listener.error(err);
            }
        }
    }

    @Override
    public void characters(CharSequence chars, Location locationId, int properties) {
        if (!failed) {
            try {
                super.characters(chars, locationId, properties);
            } catch (XPathException err) {
                failed = true;
                listener.error(err);
            }
        }
    }

    @Override
    public void processingInstruction(String target, CharSequence data, Location locationId, int properties) {
        if (!failed) {
            try {
                super.processingInstruction(target, data, locationId, properties);
            } catch (XPathException err) {
                failed = true;
                listener.error(err);
            }
        }
    }

    @Override
    public void comment(CharSequence chars, Location locationId, int properties) {
        if (!failed) {
            try {
                super.comment(chars, locationId, properties);
            } catch (XPathException err) {
                failed = true;
                listener.error(err);
            }
        }
    }

    @Override
    public void append(Item item, Location locationId, int copyNamespaces) {
        if (!failed) {
            try {
                super.append(item, locationId, copyNamespaces);
            } catch (XPathException err) {
                failed = true;
                listener.error(err);
            }
        }
    }
}

// Copyright (c) 2018 Saxonica Limited
