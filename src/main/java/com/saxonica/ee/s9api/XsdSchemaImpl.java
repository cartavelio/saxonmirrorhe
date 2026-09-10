package com.saxonica.ee.s9api;

import com.saxonica.ee.schema.PreparedSchema;
import net.sf.saxon.s9api.ItemTypeFactory;
import net.sf.saxon.s9api.SaxonApiException;
import net.sf.saxon.s9api.SchemaValidator;
import net.sf.saxon.s9api.XsdSchema;

public class XsdSchemaImpl extends XsdSchema {

    private PreparedSchema schema;

    XsdSchemaImpl(PreparedSchema schema) {
        this.schema = schema;
    }

    /**
     * Create a SchemaValidator which can be used to validate instance documents against this schema
     *
     * @return a new SchemaValidator
     * @since 13.0
     */
    @Override
    public SchemaValidator newSchemaValidator() {
        return null;
    }

    /**
     * Create an {@link ItemTypeFactory} which can be used to create {@link ItemType}
     * objects based on the definitions in this schema.
     *
     * @return an {@link ItemTypeFactory} linked to this schema.
     */
    @Override
    public ItemTypeFactory newItemTypeFactory() {
        return null;
    }

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
        return null;
    }
}


