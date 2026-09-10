package com.saxonica.ee.jaxp;

import com.saxonica.config.EnterpriseConfiguration;
import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.event.ReceivingContentHandler;
import net.sf.saxon.lib.ErrorReporter;
import org.w3c.dom.ls.LSResourceResolver;
import org.xml.sax.*;
import org.xml.sax.ext.LexicalHandler;

import javax.xml.validation.TypeInfoProvider;
import javax.xml.validation.ValidatorHandler;

/**
 * The is the Saxon implementation of the JAXP 1.3 ValidatorHandler interface. It is
 * a SAX ContentHandler that accepts events from an XML parser (or elsewhere), validates
 * them, reports any errors, and passes the events on to another ContentHandler. The events
 * that are passed on will be augmented with extra elements and attributes that expand
 * default values defined in the schema. In addition, the receiving ContentHandler has
 * access to a TypeInfoProvider that gives information about the type of the current
 * element or attribute event.
 * <p>
 * A ValidatorHandler is the front end to a rather complex pipeline. The ValidatorHandler
 * is the first stage in the pipeline: it receives events from a SAX parser or other source
 * of SAX events. The next stage is a ReceivingContentHandler, which translates SAX events
 * into Receiver events. This is followed by a number of Receivers to do the validation
 * (actually a stack of Receivers, since a new ComplexContentValidator is added each time
 * a new element starts). The last Receiver is the finalProxy: this translates Receiver
 * events back into SAX2 events, for delivery to the user's ContentHandler.
 * </p><p>
 * The user's ContentHandler has access to a TypeInfoProvider that gives information about
 * the types of elements and attributes. This information is passed down the Receiver pipeline
 * in the form of typeCodes associated with element and attribute events. The TypeInfoProvider
 * is part of the finalProxy, which saves the type information locally and returns it to the
 * user's ContentHandler on request. The TypeInfo objects supplied by the TypeInfoProvider are
 * in fact the schema components representing the type, as a SchemaType object.
 * </p>
 */

public class ValidatorHandlerImpl extends ValidatorHandler
        implements ContentHandler, LexicalHandler, DTDHandler {
    private ReceivingContentHandler next;
    private TypedContentHandler finalProxy;
    private PipelineConfiguration pipe;
    private LSResourceResolver resourceResolver;

    /**
     * Set the pipeline configuration used by this validator handler
     *
     * @param pipe the pipeline configuration
     */

    public void setPipelineConfiguration(PipelineConfiguration pipe) {
        this.pipe = pipe;
    }

    /**
     * Get the ReceivingContentHandler that receives the SAX events at the start of this pipeline
     *
     * @param rch the ReceivingContentHandler
     */

    public void setReceivingContentHandler(ReceivingContentHandler rch) {
        next = rch;
    }

    /**
     * Set the TypedContentHandler that acts as the final stage in this pipeline, receiving
     * typed events from the validator stack
     *
     * @param proxy the TypedContentHandler at the end of the pipeline
     */

    public void setFinalProxy(TypedContentHandler proxy) {
        finalProxy = proxy;
    }

    /**
     * Sets the {@link org.xml.sax.ContentHandler} which receives
     * the augmented validation result.
     * <p>A {@link javax.xml.validation.ValidatorHandler} may buffer events to certain
     * extent, but to allow {@link javax.xml.validation.ValidatorHandler} to be used
     * by a parser, the following requirement has to be met.</p>
     * <ol>
     * <li>When
     * {@link org.xml.sax.ContentHandler#startElement(String, String, String, Attributes)},
     * {@link org.xml.sax.ContentHandler#endElement(String, String, String)},
     * {@link org.xml.sax.ContentHandler#startDocument()}, or
     * {@link org.xml.sax.ContentHandler#endDocument()}
     * are invoked on a {@link javax.xml.validation.ValidatorHandler},
     * the same method on the user-specified {@link org.xml.sax.ContentHandler}
     * must be invoked for the same event before the callback
     * returns.</li></ol>
     * <p><b>The Saxon implementation does not meet this requirement. StartElement events are
     * buffered until the first thing following the attributes is encountered.</b></p>
     *
     * @param receiver A {@link org.xml.sax.ContentHandler} or a null value.
     */
    @Override
    public void setContentHandler(ContentHandler receiver) {
        finalProxy.setUnderlyingContentHandler(receiver);
    }

    /**
     * Gets the {@link org.xml.sax.ContentHandler} which receives the
     * augmented validation result.
     *
     * @return This method returns the object that was last set through
     *         the {@link #setContentHandler(org.xml.sax.ContentHandler)} method, or null
     *         if that method has never been called since this {@link javax.xml.validation.ValidatorHandler}
     *         has created.
     * @see #setContentHandler(org.xml.sax.ContentHandler)
     */
    @Override
    public ContentHandler getContentHandler() {
        return finalProxy.getUnderlyingContentHandler();
    }

    /**
     * Sets the {@link org.xml.sax.ErrorHandler} to receive errors encountered
     * during the validation.
     *
     * @param errorHandler A new error handler to be set. This parameter can be null.
     */

    @Override
    public void setErrorHandler(ErrorHandler errorHandler) {
        if (errorHandler == null) {
            errorHandler = SchemaFactoryImpl.DRACONIAN_ERROR_HANDLER;
        }
        ErrorReporter el = new SchemaFactoryImpl.ErrorReporterWrappingErrorHandler(errorHandler);
        pipe.setErrorReporter(el);
    }

    /**
     * Gets the current {@link org.xml.sax.ErrorHandler}
     *
     * @return This method returns the object that was last set through
     *         the {@link #setErrorHandler(org.xml.sax.ErrorHandler)} method, or null
     *         if that method has never been called
     * @see #setErrorHandler(org.xml.sax.ErrorHandler)
     */
    @Override
    public ErrorHandler getErrorHandler() {
        ErrorReporter el = pipe.getErrorReporter();
        if (el instanceof SchemaFactoryImpl.ErrorReporterWrappingErrorHandler) {
            ErrorHandler eh = ((SchemaFactoryImpl.ErrorReporterWrappingErrorHandler) el).getErrorHandler();
            return eh == SchemaFactoryImpl.DRACONIAN_ERROR_HANDLER ? null : eh;
        } else {
            return null;
        }
    }

    /**
     * Sets the {@link org.w3c.dom.ls.LSResourceResolver} to customize
     * resource resolution while in a validation episode.
     *
     * @param resourceResolver A new resource resolver to be set. This parameter can be null.
     */
    @Override
    public void setResourceResolver(LSResourceResolver resourceResolver) {
        this.resourceResolver = resourceResolver;
    }

    /**
     * Gets the current {@link org.w3c.dom.ls.LSResourceResolver}
     *
     * @return This method returns the object that was last set through
     *         the {@link #setResourceResolver(org.w3c.dom.ls.LSResourceResolver)} method, or null
     *         if that method has never been called
     */
    /*@Nullable*/
    @Override
    public LSResourceResolver getResourceResolver() {
        return resourceResolver;
    }

    /**
     * Obtains the {@link javax.xml.validation.TypeInfoProvider} implementation of this
     * {@link javax.xml.validation.ValidatorHandler}.
     * The obtained {@link javax.xml.validation.TypeInfoProvider} can be queried during a parse
     * to access the type information determined by the validator.
     *
     * @return A non-null valid {@link javax.xml.validation.TypeInfoProvider}.
     */
    @Override
    public TypeInfoProvider getTypeInfoProvider() {
        return finalProxy.getTypeInfoProvider();
    }

    /**
     * Receive an object for locating the origin of SAX document events.
     *
     * @param locator an object that can return the location of
     *                any SAX document event
     * @see org.xml.sax.Locator
     */
    @Override
    public void setDocumentLocator(Locator locator) {
        next.setDocumentLocator(locator);
    }

    /**
     * Look up the value of a property.
     * <p>The property name is any fully-qualified URI.  It is
     * possible for a {@link javax.xml.validation.ValidatorHandler} to recognize a property name but
     * temporarily be unable to return its value.
     * Some property values may be available only in specific
     * contexts, such as before, during, or after a validation.</p>
     * <p>{@link javax.xml.validation.Validator}s are not required to recognize any specific
     * property names.</p>
     * <p>In the Saxon implementation, the property names that are recognized are the same as the
     * attribute names recognised by a {@link javax.xml.transform.TransformerFactory}.
     * These are listed as constant fields
     * in the class {@link net.sf.saxon.lib.FeatureKeys}</p>
     *
     * @param name The property name, which is a non-null fully-qualified URI.
     * @return The current value of the property.
     * @throws org.xml.sax.SAXNotRecognizedException
     *                              If the property
     *                              value can't be assigned or retrieved.
     * @throws org.xml.sax.SAXNotSupportedException
     *                              When the
     *                              XMLReader recognizes the property name but
     *                              cannot determine its value at this time.
     * @throws NullPointerException When the name parameter is null.
     * @see #setProperty(String, Object)
     */

    @Override
    public Object getProperty(String name) throws SAXNotRecognizedException, SAXNotSupportedException {
        try {
            return pipe.getConfiguration().getConfigurationProperty(name);
        } catch (IllegalArgumentException err) {
            throw new SAXNotSupportedException(err.getMessage());
        }
    }

    /**
     * Set the value of a property.
     * <p>The property name is any fully-qualified URI.  It is
     * possible for a {@link javax.xml.validation.ValidatorHandler} to recognize a property name but
     * to be unable to change the current value.
     * Some property values may be immutable or mutable only
     * in specific contexts, such as before, during, or after
     * a validation.</p>
     * <p>In the Saxon implementation, the property names that are recognized are the same as the
     * attribute names recognised by a {@link javax.xml.transform.TransformerFactory}.
     * These are listed as constant fields
     * in the class {@link net.sf.saxon.lib.FeatureKeys}</p>
     *
     * @param name   The property name, which is a non-null fully-qualified URI.
     * @param object The requested value for the property.
     * @throws org.xml.sax.SAXNotRecognizedException
     *                              If the property
     *                              value can't be assigned or retrieved.
     * @throws org.xml.sax.SAXNotSupportedException
     *                              When the
     *                              {@link javax.xml.validation.Validator} recognizes the property name but
     *                              cannot set the requested value.
     * @throws NullPointerException When the name parameter is null.
     */
    @Override
    public void setProperty(String name, Object object) throws SAXNotRecognizedException, SAXNotSupportedException {
        try {
            pipe.getConfiguration().setConfigurationProperty(name, object);
        } catch (IllegalArgumentException err) {
            throw new SAXNotSupportedException(err.getMessage());
        }
    }

    /**
     * Get the Configuration object
     *
     * @return the Configuration, which will always be schema-aware
     */

    public EnterpriseConfiguration getConfiguration() {
        return (EnterpriseConfiguration) pipe.getConfiguration();
    }

    /**
     * Receive notification of the beginning of a document.
     * <p>The SAX parser will invoke this method only once, before any
     * other event callbacks (except for {@link #setDocumentLocator
     * setDocumentLocator}).</p>
     *
     * @throws org.xml.sax.SAXException any SAX exception, possibly
     *                                  wrapping another exception
     * @see #endDocument
     */
    @Override
    public void startDocument() throws SAXException {
        next.startDocument();
    }

    /**
     * Receive notification of the end of a document.
     *
     * @throws org.xml.sax.SAXException any SAX exception, possibly
     *                                  wrapping another exception
     */
    @Override
    public void endDocument() throws SAXException {
        next.endDocument();
    }

    /**
     * Begin the scope of a prefix-URI Namespace mapping.
     * <p>There should never be start/endPrefixMapping events for the
     * "xml" prefix, since it is predeclared and immutable.</p>
     *
     * @param prefix the Namespace prefix being declared.
     *               An empty string is used for the default element namespace,
     *               which has no prefix.
     * @param uri    the Namespace URI the prefix is mapped to
     * @throws org.xml.sax.SAXException the client may throw
     *                                  an exception during processing
     * @see #endPrefixMapping
     * @see #startElement
     */
    @Override
    public void startPrefixMapping(String prefix, String uri) throws SAXException {
        next.startPrefixMapping(prefix, uri);
    }

    /**
     * End the scope of a prefix-URI mapping.
     *
     * @param prefix the prefix that was being mapped.
     *               This is the empty string when a default mapping scope ends.
     * @throws org.xml.sax.SAXException the client may throw
     *                                  an exception during processing
     */
    @Override
    public void endPrefixMapping(String prefix) throws SAXException {
        next.endPrefixMapping(prefix);
    }

    /**
     * Receive notification of the beginning of an element.
     *
     * @param uri       the Namespace URI, or the empty string if the
     *                  element has no Namespace URI or if Namespace
     *                  processing is not being performed
     * @param localName the local name (without prefix), or the
     *                  empty string if Namespace processing is not being
     *                  performed
     * @param qName     the qualified name (with prefix), or the
     *                  empty string if qualified names are not available
     * @param atts      the attributes attached to the element.  If
     *                  there are no attributes, it shall be an empty
     *                  Attributes object.  The value of this object after
     *                  startElement returns is undefined
     * @throws org.xml.sax.SAXException any SAX exception, possibly
     *                                  wrapping another exception
     */
    @Override
    public void startElement(String uri, String localName, String qName, Attributes atts) throws SAXException {
        next.startElement(uri, localName, qName, atts);
    }

    /**
     * Receive notification of the end of an element.
     */
    @Override
    public void endElement(String uri, String localName, String qName) throws SAXException {
        next.endElement(uri, localName, qName);
    }

    /**
     * Receive notification of character data.
     */

    @Override
    public void characters(char[] ch, int start, int length) throws SAXException {
        next.characters(ch, start, length);
    }

    /**
     * Receive notification of ignorable whitespace in element content.
     */
    @Override
    public void ignorableWhitespace(char[] ch, int start, int length) throws SAXException {
        next.ignorableWhitespace(ch, start, length);
    }

    /**
     * Receive notification of a processing instruction.
     */
    @Override
    public void processingInstruction(String target, String data) throws SAXException {
        next.processingInstruction(target, data);
    }

    /**
     * Receive notification of a skipped entity.
     */
    @Override
    public void skippedEntity(String name) throws SAXException {
        next.skippedEntity(name);
    }

    /**
     * Report the start of DTD declarations, if any.
     */
    @Override
    public void startDTD(String name, String publicId, String systemId) throws SAXException {
        next.startDTD(name, publicId, systemId);
    }

    /**
     * Report the end of DTD declarations.
     */
    @Override
    public void endDTD() throws SAXException {
        next.endDTD();
    }

    /**
     * Report the beginning of some internal and external XML entities.
     */
    @Override
    public void startEntity(String name) throws SAXException {
        next.startEntity(name);
    }

    /**
     * Report the end of an entity.
     */
    @Override
    public void endEntity(String name) throws SAXException {
        next.endEntity(name);
    }

    /**
     * Report the start of a CDATA section.
     */
    @Override
    public void startCDATA() throws SAXException {
        next.startCDATA();
    }

    /**
     * Report the end of a CDATA section.
     */
    @Override
    public void endCDATA() throws SAXException {
        next.endCDATA();
    }

    /**
     * Report an XML comment anywhere in the document.
     */
    @Override
    public void comment(char[] ch, int start, int length) throws SAXException {
        next.comment(ch, start, length);
    }

    /**
     * Receive notification of a notation declaration event.
     */
    @Override
    public void notationDecl(String name, String publicId, String systemId) throws SAXException {
        next.notationDecl(name, publicId, systemId);
    }

    /**
     * Receive notification of an unparsed entity declaration event.
     */
    @Override
    public void unparsedEntityDecl(String name, String publicId, String systemId, String notationName) throws SAXException {
        next.unparsedEntityDecl(name, publicId, systemId, notationName);
    }
}

// Copyright (c) 2004-2022 Saxonica Limited