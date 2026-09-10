package com.saxonica.ee.schema.sdoc;

//-- imported classes and packages

import com.saxonica.ee.schema.SchemaCompiler;
import net.sf.saxon.om.AttributeMap;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.StandardNames;
import net.sf.saxon.type.SchemaException;

/**
 * An xs:annotation element in an XML Schema document
 */
public class XSDAnnotation extends SchemaElement {

    /**
     * Creates a new Annotation element
     */
    public XSDAnnotation() {
    }

    /**
     * Validate and process the attributes on this element in the schema document
     *
     * @throws SchemaException if any errors are found in the attributes
     */

    @Override
    protected void prepareAttributes() throws SchemaException {

        AttributeMap atts = attributes();
        String[] allowed = {"id"};
        allowAttributes(atts, allowed);

        processId();

    }

    /**
     * Validate this schema element, checking all document-level constraints
     *
     * @param compiler the schema compiler
     * @throws SchemaException if any XML representation constraints are violated
     */

    @Override
    public void validate(SchemaCompiler compiler) throws SchemaException {

        for (NodeInfo child : children()) {
            int fp = child.getFingerprint();
            if (fp == StandardNames.XS_APPINFO || fp == StandardNames.XS_DOCUMENTATION) {
                // ignore it
            } else {
                illegalElement(child);
            }
        }

    }

}

// Copyright (c) 2004-2022 Saxonica Limited
