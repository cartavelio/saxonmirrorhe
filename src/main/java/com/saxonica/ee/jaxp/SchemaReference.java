package com.saxonica.ee.jaxp;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.ee.schema.PreparedSchema;
import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.event.ReceivingContentHandler;
import net.sf.saxon.s9api.HostLanguage;
import net.sf.saxon.lib.ParseOptions;
import net.sf.saxon.lib.Validation;
import net.sf.saxon.om.NoElementsSpaceStrippingRule;
import org.xml.sax.helpers.XMLFilterImpl;

import javax.xml.validation.Schema;
import javax.xml.validation.Validator;
import javax.xml.validation.ValidatorHandler;

/**
 * A reference to a Saxon prepared schema. This is a wrapper around the Saxon PreparedSchema class; the wrapper
 * implements the JAXP Schema interface and thus allows a Saxon schema to be used via the JAXP validation API.
 */
public class SchemaReference extends Schema {

    private final PreparedSchema schema;

    /**
     * Create a SchemaReference to wrap a supplied PreparedSchema object
     *
     * @param schema the PreparedSchema to be wrapped
     */

    public SchemaReference(PreparedSchema schema) {
        this.schema = schema;
    }

    /**
     * Get the PreparedSchema object wrapped by this SchemaReference
     *
     * @return the PreparedSchema object, which gives access to the schema components
     */

    public PreparedSchema getPreparedSchema() {
        return schema;
    }

    /**
     * Create a new {@link javax.xml.validation.Validator} for this {@link javax.xml.validation.Schema}.
     * <p>A validator enforces/checks the set of constraints this object
     * represents.</p>
     *
     * @return Always return a non-null valid object.
     */

    @Override
    public Validator newValidator() {
        PipelineConfiguration pipe = schema.getConfiguration().makePipelineConfiguration();
        pipe.setHostLanguage(HostLanguage.XML_SCHEMA);
        return new ValidatorImpl(pipe, schema);
    }

    /**
     * Creates a new {@link javax.xml.validation.ValidatorHandler} for this {@link javax.xml.validation.Schema}.
     *
     * @return Always return a non-null valid object.
     */

    @Override
    public ValidatorHandler newValidatorHandler() {
        EnterpriseConfiguration config = schema.getConfiguration();
        PipelineConfiguration pipe = config.makePipelineConfiguration();
        pipe.setHostLanguage(HostLanguage.XML_SCHEMA);

        TypedContentHandler proxy = new TypedContentHandler();
        proxy.setPipelineConfiguration(pipe);
        // create a sink destination until such time as the user supplies his own
        proxy.setUnderlyingContentHandler(new XMLFilterImpl());

        ParseOptions options = new ParseOptions()
                .withSchemaValidationMode(Validation.STRICT)
                .withSpaceStrippingRule(NoElementsSpaceStrippingRule.getInstance())
                .withCheckEntityReferences(true);
        pipe.setParseOptions(options);

        Receiver realValidator = config.getDocumentValidator(proxy, proxy.getSystemId(), options, null);

        ReceivingContentHandler rch = new ReceivingContentHandler();
        rch.setReceiver(realValidator);
        rch.setPipelineConfiguration(pipe);

        ValidatorHandlerImpl val = new ValidatorHandlerImpl();
        val.setPipelineConfiguration(pipe);
        val.setReceivingContentHandler(rch);
        val.setFinalProxy(proxy);
        return val;
    }
}

// Copyright (c) 2004-2022 Saxonica Limited
