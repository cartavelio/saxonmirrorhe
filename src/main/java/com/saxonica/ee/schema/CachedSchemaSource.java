package com.saxonica.ee.schema;

import javax.xml.transform.Source;

/**
 * A CachedSchemaSource encapsulates a {@link PreparedSchema}; it implements the {@link javax.xml.transform.Source}
 * interface purely so that it can be included in a list of sources used to assemble a schema in the
 * {@link SchemaCompiler#loadSources(Iterable)} method. In general, it is not accepted by other Saxon interfaces
 * that expect a Source object.
 */

public class CachedSchemaSource implements Source {

    private final PreparedSchema schema;
    public CachedSchemaSource(PreparedSchema schema) {
        this.schema = schema;
    }

    public PreparedSchema getSchema() {
        return schema;
    }



    /**
     * Set the system identifier for this Source.
     *
     * <p>The system identifier is optional if the source does not
     * get its data from a URL, but it may still be useful to provide one.
     * The application can use a system identifier, for example, to resolve
     * relative URIs and to include in error messages and warnings.</p>
     *
     * @param systemId The system identifier as a URL string.
     */
    @Override
    public void setSystemId(String systemId) {
        throw new UnsupportedOperationException();
    }

    /**
     * Get the system identifier that was set with setSystemId.
     *
     * @return The system identifier that was set with setSystemId, or null
     * if setSystemId was not called.
     */
    @Override
    public String getSystemId() {
        return null;
    }
}

