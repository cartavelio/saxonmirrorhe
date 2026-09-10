package net.sf.saxon.s9api;

public abstract class XsdSchema {


    /**
     * Create a SchemaValidator which can be used to validate instance documents against this schema
     * @return a new SchemaValidator
     * @since 13.0
     */

    public abstract SchemaValidator newSchemaValidator();

    /**
     * Create an {@link ItemTypeFactory} which can be used to create {@link ItemType}
     * objects based on the definitions in this schema.
     * @return an {@link ItemTypeFactory} linked to this schema.
     */

    public abstract ItemTypeFactory newItemTypeFactory();

    /**
     * Combine this schema with another, to form a new combined schema.
     * <p>Note: in relatively unusual circumstances, a document that is valid against one of the two input schemas
     * might not be valid against the combined schema. For example, if the first schema has a wildcard with
     * <code>processContents="lax"</code> then an element might be laxly validated when assessed using that schema,
     * but strictly validated when assessed against the combined schema; and strict validation may fail while
     * lax validation succeeded.</p>
     * @param other the other schema to combine with this one.
     * @return the combined schema, assuming the two schemas are compatible
     * @throws SaxonApiException if the two schemas cannot be combined, for example because they
     * contain components with duplicated names
     * @since 13.0
     */

    public abstract XsdSchema combine(XsdSchema other) throws SaxonApiException;
}

