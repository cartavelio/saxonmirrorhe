package com.saxonica.ee.s9api;

import com.saxonica.ee.schema.PreparedSchema;
import net.sf.saxon.Configuration;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.s9api.*;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.Schema;
import net.sf.saxon.type.SchemaException;

public class XsdSchemaImpl extends XsdSchema {

    private final Processor processor;
    private final PreparedSchema schema;

    XsdSchemaImpl(Processor processor, PreparedSchema schema) {
        this.processor = processor;
        this.schema = schema;
    }

    /**
     * Create a SchemaValidator which can be used to validate instance documents against this schema
     *
     * @return a new SchemaValidator
     * @since 13.0
     */
    @Override
    public SchemaValidator newValidator() {
        return new SchemaValidatorImpl(processor, this);
    }

    //#if CSHARP==false
    /**
     * Create an {@link ItemTypeFactory} which can be used to create {@link ItemType}
     * objects based on the definitions in this schema.
     *
     * @return an {@link ItemTypeFactory} linked to this schema.
     */
    @Override
    public ItemTypeFactory newItemTypeFactory() {
        return new ItemTypeFactory(processor, this);
    }
    //#endif

    /**
     * Combine this schema with another, to form a new combined schema.
     * <p>Note: in relatively unusual circumstances, a document that is valid against one of the two input schemas
     * might not be valid against the combined schema. For example, if the first schema has a wildcard with
     * <code>processContents="lax"</code> then an element might be laxly validated when assessed using that schema,
     * but strictly validated when assessed against the combined schema; and strict validation may fail while
     * lax validation succeeded.</p>
     *
     * @param other the other schema to combine with this one.
     * @return the combined schema, assuming the two schemas are compatible
     * @throws SaxonApiException if the two schemas cannot be combined, for example because they
     *                           contain components with duplicated names
     * @since 13.0
     */
    @Override
    public XsdSchema combine(XsdSchema other) throws SaxonApiException {
        try {
            return new XsdSchemaImpl(processor, PreparedSchema.merge(
                    schema, (PreparedSchema) other.getUnderlyingSchema()));
        } catch (SchemaException e) {
            throw new SaxonApiException(e);
        }
    }

    /**
     * Get the underlying implementation-level Schema
     */

    public Schema getUnderlyingSchema() {
        return schema;
    }

    /**
     * Export a precompiled Schema Component Model containing all the components (except built-in components)
     * that have been loaded into this Processor.
     *
     * @param destination the destination to recieve the precompiled Schema Component Model in the form of an
     *                    XML document
     * @throws SaxonApiException if a failure occurs writing the schema components to the supplied destination
     */
    @Override
    public void exportComponents(Destination destination) throws SaxonApiException {
        try {
            Configuration config = schema.getConfiguration();
            if (config.getConfigurationProperty(Feature.ALLOW_UNRESOLVED_SCHEMA_COMPONENTS)) {
                throw new SaxonApiException("Exporting to an SCM file is not possible when unresolved references to schema components have been permitted");
            }
            Receiver out = destination.getReceiver(config.makePipelineConfiguration(), config.obtainDefaultSerializationProperties());
            schema.exportComponents(out);
        } catch (XPathException e) {
            throw new SaxonApiException(e);
        }
    }
}


