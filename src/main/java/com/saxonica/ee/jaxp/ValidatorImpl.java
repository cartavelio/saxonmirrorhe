package com.saxonica.ee.jaxp;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.ee.schema.PreparedSchema;
import net.sf.saxon.Configuration;
import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.event.Sender;
import net.sf.saxon.event.Sink;
import net.sf.saxon.lib.*;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.serialize.SerializationProperties;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.SchemaException;
import net.sf.saxon.type.ValidationException;
import net.sf.saxon.type.ValidationFailure;
import org.w3c.dom.ls.LSResourceResolver;
import org.xml.sax.*;

import javax.xml.XMLConstants;
import javax.xml.transform.*;
import javax.xml.validation.Validator;
import java.io.IOException;

/**
 * This class in the Saxon implementation of the JAXP 1.3 Validator "interface" (which is actually
 * defined as an abstract class). It is used to validate a source document against a schema.
 */

public class ValidatorImpl extends Validator {

    private final PipelineConfiguration pipe;
    private LSResourceResolver resourceResolver;

    /**
     * Create the Validator
     *
     * @param pipe   the pipeline configuration
     * @param schema the schema against which validation takes place
     */

    public ValidatorImpl(PipelineConfiguration pipe, PreparedSchema schema) {
        try {
            this.pipe = pipe;
            EnterpriseConfiguration config = (EnterpriseConfiguration) pipe.getConfiguration();
            config.addSchema(schema);
            setErrorHandler(null);
        } catch (SchemaException e) {
            // this only happens if using an incorrect license file
            throw new UnsupportedOperationException(e.getMessage());
        }
    }

    @Override
    public void reset() {
        //
    }

    @Override
    public void validate(Source source, Result result) throws SAXException, IOException {
        Receiver out;
        try {
            if (result == null) {
                out = new Sink(pipe);
            } else {
                SerializerFactory sf = pipe.getConfiguration().getSerializerFactory();
                out = sf.getReceiver(result, new SerializationProperties(), pipe);
            }
            ParseOptions options = pipe.getParseOptions()
                    .withSchemaValidationMode(Validation.STRICT)
                    .withContinueAfterValidationErrors(true);
            pipe.setErrorReporter(new SchemaFactoryImpl.ErrorReporterWrappingErrorHandler(getErrorHandler()));
            Sender.send(source, out, options);

        } catch (TransformerException e) {
            if (e.getException() instanceof SAXException) {
                throw (SAXException) e.getException();
            } else if (e.getException() instanceof IOException) {
                throw (IOException) e.getException();
            } else {
                throw new SAXException(e);
            }
        }
    }

    /**
     * Sets the {@link ErrorHandler} to receive errors encountered
     * during the <code>validate</code> method invocation.
     *
     * <p>
     * Error handler can be used to customize the error handling process
     * during a validation. When an {@link ErrorHandler} is set,
     * errors found during the validation will be first sent
     * to the {@link ErrorHandler}.
     *
     * <p>
     * The error handler can abort further validation immediately
     * by throwing {@link SAXException} from the handler. Or for example
     * it can print an error to the screen and try to continue the
     * validation by returning normally from the {@link ErrorHandler}
     *
     * <p>
     * If any {@link Throwable} is thrown from an {@link ErrorHandler},
     * the caller of the <code>validate</code> method will be thrown
     * the same {@link Throwable} object.
     *
     * <p>
     * {@link Validator} is not allowed to
     * throw {@link SAXException} without first reporting it to
     * {@link ErrorHandler}.
     *
     * <p>
     * When the {@link ErrorHandler} is null, the implementation will
     * behave as if the following {@link ErrorHandler} is set:
     * <pre>
     * class DraconianErrorHandler implements {@link ErrorHandler} {
     *     public void fatalError( {@link org.xml.sax.SAXParseException} e ) throws {@link SAXException} {
     *         throw e;
     *     }
     *     public void error( {@link org.xml.sax.SAXParseException} e ) throws {@link SAXException} {
     *         throw e;
     *     }
     *     public void warning( {@link org.xml.sax.SAXParseException} e ) throws {@link SAXException} {
     *         // noop
     *     }
     * }
     * </pre>
     *
     * <p>
     * When a new {@link Validator} object is created, initially
     * this field is set to null.
     *
     * @param   errorHandler
     *      A new error handler to be set. This parameter can be null.
     */

    @Override
    public void setErrorHandler(ErrorHandler errorHandler) {
        if (errorHandler == null) {
            errorHandler = SchemaFactoryImpl.DRACONIAN_ERROR_HANDLER;
        }
        InvalidityHandler handler = new InvalidityHandlerWrappingErrorHandler(errorHandler);
        pipe.setParseOptions(pipe.getParseOptions().withInvalidityHandler(handler));
    }




    /**
     * Interface for reporting validation errors found during validation of an instance document
     * against a schema.
     */
    public static class InvalidityHandlerWrappingErrorHandler implements InvalidityHandler {

        private final ErrorHandler errorHandler;

        public InvalidityHandlerWrappingErrorHandler(ErrorHandler handler) {
            this.errorHandler = handler;
        }

        /**
         * At the start of a validation episode, initialize the handler
         *
         * @param systemId optional; may be used to represent the destination of any report produced
         * @throws XPathException if initialization of the invalidity handler fails for any reason
         */
        @Override
        public void startReporting(String systemId) throws XPathException {
            // no action
        }

        @Override
        public void reportInvalidity(Invalidity failure) throws XPathException {
            try {
                ValidationException exception = ((ValidationFailure) failure).makeException();
                errorHandler.error(new SAXParseException(failure.getMessage(), exception.getLocator() , exception));
            }  catch (SAXException e) {
                throw XPathException.makeXPathException(e);
            }
        }

        public ErrorHandler getErrorHandler() {
            return errorHandler;
        }

        /**
         * Get the value to be associated with a validation exception. May return null.
         * In the case of the InvalidityReportGenerator, this returns the XML document
         * containing the validation report
         *
         * @return a value (or null). This will be the value returned as the value of
         * the variable $err:value during try/catch processing
         */
        @Override
        public Sequence endReporting() {
            return null;
        }
    }

    /**
     * Gets the current {@link ErrorHandler} set to this {@link Validator}.
     *
     * @return
     *      This method returns the object that was last set through
     *      the {@link #setErrorHandler(ErrorHandler)} method, or null
     *      if that method has never been called since this {@link Validator}
     *      has created.
     *
     * @see #setErrorHandler(ErrorHandler)
     */

    @Override
    public ErrorHandler getErrorHandler() {
        InvalidityHandler handler = pipe.getParseOptions().getInvalidityHandler();
        if (handler instanceof InvalidityHandlerWrappingErrorHandler) {
            return ((InvalidityHandlerWrappingErrorHandler)handler).getErrorHandler();
        }
        ErrorReporter listener = pipe.getErrorReporter();
        if (listener instanceof SchemaFactoryImpl.ErrorReporterWrappingErrorHandler) {
            ErrorHandler eh = ((SchemaFactoryImpl.ErrorReporterWrappingErrorHandler) listener).getErrorHandler();
            return eh == SchemaFactoryImpl.DRACONIAN_ERROR_HANDLER ? null : eh;
        }
        return null;
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
     * Get the underlying Saxon Configuration object
     *
     * @return the Saxon Configuration
     */

    public Configuration getConfiguration() {
        return pipe.getConfiguration();
    }

    /**
     * Look up the value of a property.
     * <p>The property name is any fully-qualified URI.  It is
     * possible for a {@link javax.xml.validation.Validator} to recognize a property name but
     * temporarily be unable to return its value.
     * Some property values may be available only in specific
     * contexts, such as before, during, or after a validation.</p>
     * <p>{@link javax.xml.validation.Validator}s are not required to recognize any specific
     * property names.</p>
     * <p>In the Saxon implementation, the property names that are recognized are the same as the
     * attribute names recognised by a {@link TransformerFactory}. These are listed as constant fields
     * in the class {@link net.sf.saxon.lib.FeatureKeys}. In addition, the following properties
     * are recognized:</p>
     * <ul>
     *     <li>{@link XMLConstants#FEATURE_SECURE_PROCESSING}</li>
     *     <li>{@link XMLConstants#ACCESS_EXTERNAL_DTD}</li>
     *     <li>{@link XMLConstants#ACCESS_EXTERNAL_SCHEMA}</li>
     * </ul>
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
        switch (name) {
            case XMLConstants.FEATURE_SECURE_PROCESSING:
            case XMLConstants.ACCESS_EXTERNAL_DTD:
            case XMLConstants.ACCESS_EXTERNAL_SCHEMA:
                return pipe.getParseOptions().getParserProperty(name);
            default:
                try {
                    return pipe.getConfiguration().getConfigurationProperty(name);
                } catch (IllegalArgumentException err) {
                    throw new SAXNotSupportedException(err.getMessage());
                }
        }
    }

    /**
     * Set the value of a property.
     * <p>The property name is any fully-qualified URI.  It is
     * possible for a {@link javax.xml.validation.Validator} to recognize a property name but
     * to be unable to change the current value.
     * Some property values may be immutable or mutable only
     * in specific contexts, such as before, during, or after
     * a validation.</p>
     * <p>In the Saxon implementation, the property names that are recognized are the same as the
     * attribute names recognised by a {@link TransformerFactory}. These are listed as constant fields
     * in the class {@link net.sf.saxon.lib.FeatureKeys}. In addition, the following properties
     * are recognized, and passed directly to the underlying XML parser:</p>
     * <ul>
     *      <li>{@link XMLConstants#FEATURE_SECURE_PROCESSING}</li>
     *      <li>{@link XMLConstants#ACCESS_EXTERNAL_DTD}</li>
     *      <li>{@link XMLConstants#ACCESS_EXTERNAL_SCHEMA}</li>
     * </ul>
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
        switch (name) {
            case XMLConstants.FEATURE_SECURE_PROCESSING:
            case XMLConstants.ACCESS_EXTERNAL_DTD:
                pipe.setParseOptions(pipe.getParseOptions().withAdditionalParserProperties(name, object));
                break;
            case XMLConstants.ACCESS_EXTERNAL_SCHEMA:
                throw new SAXNotSupportedException();
                // TODO: see bug 4234. We should support this option, though it's not clear how it interacts
                //  with setResourceResolver(). We should probably say that the property is ignored if a user-
                //  specified ResourceResolver is used. We could perhaps be smart and implement restrictions
                //  on protocols in some component shared by all the various resource resolvers.
            default:
                try {
                    pipe.getConfiguration().setConfigurationProperty(name, object);
                } catch (IllegalArgumentException err) {
                    throw new SAXNotSupportedException(err.getMessage());
                }
        }
    }


}

// Copyright (c) 2004-2022 Saxonica Limited