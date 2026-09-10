package com.saxonica.ee.schema;

import com.saxonica.config.EnterpriseConfiguration;
import net.sf.saxon.om.NamespaceUri;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

/**
 * A schema (collection of schema components) that has an identifiable target namespace: typically
 * the set of components derived from a single import or include. It is not actually necessary that
 * all the components should belong to this target namespace. In the specification, this is "the schema
 * corresponding to some schema document".
 */
public class SingleNamespaceSchema extends PreparedSchema {

    /*@NotNull*/
    private NamespaceUri targetNamespace;

    public SingleNamespaceSchema(EnterpriseConfiguration config, NamespaceUri targetNamespace) {
        super(config);
        this.targetNamespace = targetNamespace;
    }

    /**
     * Sets the target namespace for this Schema
     *
     * @param targetNamespace the target namespace for this Schema
     * @see "2.7 XML Schema Part 1: Structures"
     */

    public void setTargetNamespace(NamespaceUri targetNamespace) {
        //System.err.println("Schema " + this + " has namespace " + targetNamespace);
        this.targetNamespace = targetNamespace;
    }

    /**
     * Returns the target namespace for this Schema, or "" if the schema is a
     * no-namespace schema.
     *
     * @return the target namespace for this Schema, or "" if the schema is a
     *         no-namespace schema.
     */

    /*@NotNull*/
    public NamespaceUri getTargetNamespace() {
        return targetNamespace;
    }

    @Override
    public Map<NamespaceUri, List<String>> getTargetNamespaceMap() {
        HashMap<NamespaceUri, List<String>> map = new HashMap<>();
        map.put(getTargetNamespace(), new ArrayList<>()); // TODO - add the location
        return map;
    }
}

// Copyright (c) 2009-2023 Saxonica Limited



