package net.sf.saxon.jaxp;

import org.xml.sax.*;

import java.io.IOException;
import java.util.Stack;

/**
 * An implementation of {@link XMLReader} that delegates parsing
 * to an underlying XMLReader; it has the ability to reset the state
 * of the supplied XMLReader on completion, which it does by monitoring
 * all state changes and undoing these at the end.
 */

public class ReusableXMLReader implements XMLReader {

    @FunctionalInterface
    private interface Action {
        void run() throws SAXNotRecognizedException, SAXNotSupportedException;
    }

    private final XMLReader xmlReader;
    private final Stack<Action> actionStack = new Stack<>();

    public ReusableXMLReader(XMLReader xmlReader) {
        this.xmlReader = xmlReader;
    }

    @Override
    public boolean getFeature(String name) throws SAXNotRecognizedException, SAXNotSupportedException {
        return xmlReader.getFeature(name);
    }

    @Override
    public void setFeature(String name, boolean value) throws SAXNotRecognizedException, SAXNotSupportedException {
        boolean oldValue = xmlReader.getFeature(name);
        if (value != oldValue) {
            xmlReader.setFeature(name, value);
            actionStack.push(() -> xmlReader.setFeature(name, oldValue));
        }
    }

    @Override
    public Object getProperty(String name) throws SAXNotRecognizedException, SAXNotSupportedException {
        return xmlReader.getProperty(name);
    }

    @Override
    public void setProperty(String name, Object value) throws SAXNotRecognizedException, SAXNotSupportedException {
        Object oldValue = xmlReader.getProperty(name);
        if (value != oldValue) {
            xmlReader.setProperty(name, value);
            actionStack.push(() -> xmlReader.setProperty(name, oldValue));
        }
    }

    @Override
    public void setEntityResolver(EntityResolver resolver) {
        EntityResolver oldValue = xmlReader.getEntityResolver();
        if (oldValue != resolver) {
            xmlReader.setEntityResolver(resolver);
            actionStack.push(() -> xmlReader.setEntityResolver(oldValue));
        }
    }

    @Override
    public EntityResolver getEntityResolver() {
        return xmlReader.getEntityResolver();
    }

    @Override
    public void setDTDHandler(DTDHandler handler) {
        xmlReader.setDTDHandler(handler);
    }

    @Override
    public DTDHandler getDTDHandler() {
        return xmlReader.getDTDHandler();
    }

    @Override
    public void setContentHandler(ContentHandler handler) {
        xmlReader.setContentHandler(handler);
    }

    @Override
    public ContentHandler getContentHandler() {
        return xmlReader.getContentHandler();
    }

    @Override
    public void setErrorHandler(ErrorHandler handler) {
        ErrorHandler oldValue = xmlReader.getErrorHandler();
        if (oldValue != handler) {
            xmlReader.setErrorHandler(handler);
            actionStack.push(() -> xmlReader.setErrorHandler(oldValue));
        }
    }

    @Override
    public ErrorHandler getErrorHandler() {
        return xmlReader.getErrorHandler();
    }

    @Override
    public void parse(InputSource input) throws IOException, SAXException {
        xmlReader.parse(input);
    }

    @Override
    public void parse(String systemId) throws IOException, SAXException {
        xmlReader.parse(systemId);
    }

    public XMLReader resetOriginal() throws SAXNotRecognizedException, SAXNotSupportedException {
        while (!actionStack.isEmpty()) {
            actionStack.pop().run();
        }
        return xmlReader;
    }
}

