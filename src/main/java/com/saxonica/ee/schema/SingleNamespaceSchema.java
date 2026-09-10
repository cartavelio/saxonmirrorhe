package com.saxonica.ee.schema;

import com.saxonica.config.EnterpriseConfiguration;
import net.sf.saxon.value.Whitespace;

/**
 * A schema (collection of schema components) that has an identifiable target namespace: typically
 * the set of components derived from a single import or include. It is not actually necessary that
 * all the components should belong to this target namespace. In the specification, this is "the schema
 * corresponding to some schema document".
 */
public class SingleNamespaceSchema extends PreparedSchema {

    /*@NotNull*/
    private String targetNamespace;

    public SingleNamespaceSchema(EnterpriseConfiguration config, /*@NotNull*/ String targetNamespace) {
        super(config);
        this.targetNamespace = targetNamespace;
    }

    /**
     * Sets the target namespace for this Schema
     *
     * @param targetNamespace the target namespace for this Schema
     * @see <B>&sect; 2.7 XML Schema Part 1: Structures</B>
     */

    public void setTargetNamespace(/*@NotNull*/ String targetNamespace) {
        //System.err.println("Schema " + this + " has namespace " + targetNamespace);
        this.targetNamespace = Whitespace.trim(targetNamespace);
    }

    /**
     * Returns the target namespace for this Schema, or "" if the schema is a
     * no-namespace schema.
     *
     * @return the target namespace for this Schema, or "" if the schema is a
     *         no-namespace schema.
     */

    /*@NotNull*/
    public String getTargetNamespace() {
        return targetNamespace;
    }
}

// Copyright (c) 2009-2022 Saxonica Limited



