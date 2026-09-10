package com.saxonica.ee.validate;


import net.sf.saxon.event.ProxyReceiver;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.lib.ErrorReporter;
import net.sf.saxon.om.*;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.XmlProcessingException;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.SchemaType;

/**
 * This class catches all errors thrown by a Receiver pipeline, and reports them to a specified
 * ErrorReporter; after an error has occurred, all further events are ignored. The class is added
 * to a validation pipeline when validation of a streamed input document is requested, to allow
 * a try/catch expression to catch validation errors.
 */

public class CatchingReceiver extends ProxyReceiver {

    private final ErrorReporter listener;
    private boolean failed = false;

    public CatchingReceiver(Receiver next, ErrorReporter listener) {
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
                listener.report(new XmlProcessingException(err));
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
                listener.report(new XmlProcessingException(err));
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
                listener.report(new XmlProcessingException(err));
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
                listener.report(new XmlProcessingException(err));
            }
        }
    }

    @Override
    public void startElement(NodeName elemName, SchemaType type,
                             AttributeMap attributes, NamespaceMap namespaces,
                             Location location, int properties) {
        if (!failed) {
            try {
                super.startElement(elemName, type, attributes, namespaces, location, properties);
            } catch (XPathException err) {
                failed = true;
                listener.report(new XmlProcessingException(err));
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
                listener.report(new XmlProcessingException(err));
            }
        }
    }

    @Override
    public void characters(UnicodeString chars, Location locationId, int properties) {
        if (!failed) {
            try {
                super.characters(chars, locationId, properties);
            } catch (XPathException err) {
                failed = true;
                listener.report(new XmlProcessingException(err));
            }
        }
    }

    @Override
    public void processingInstruction(String target, UnicodeString data, Location locationId, int properties) {
        if (!failed) {
            try {
                super.processingInstruction(target, data, locationId, properties);
            } catch (XPathException err) {
                failed = true;
                listener.report(new XmlProcessingException(err));
            }
        }
    }

    @Override
    public void comment(UnicodeString chars, Location locationId, int properties) {
        if (!failed) {
            try {
                super.comment(chars, locationId, properties);
            } catch (XPathException err) {
                failed = true;
                listener.report(new XmlProcessingException(err));
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
                listener.report(new XmlProcessingException(err));
            }
        }
    }
}

// Copyright (c) 2018-2023 Saxonica Limited
