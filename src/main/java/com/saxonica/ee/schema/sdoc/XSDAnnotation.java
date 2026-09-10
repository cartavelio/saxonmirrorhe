package com.saxonica.ee.schema.sdoc;

//-- imported classes and packages

import com.saxonica.ee.schema.SchemaCompiler;
import net.sf.saxon.om.AttributeCollection;
import net.sf.saxon.om.AxisInfo;
import net.sf.saxon.om.StandardNames;
import net.sf.saxon.tree.iter.AxisIterator;
import net.sf.saxon.tree.linked.NodeImpl;
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

    public void prepareAttributes() throws SchemaException {

        AttributeCollection atts = getAttributeList();
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

    public void validate(SchemaCompiler compiler) throws SchemaException {

        AxisIterator iter = iterateAxis(AxisInfo.CHILD);
        NodeImpl child;
        while ((child = (NodeImpl)iter.next()) != null) {
            int fp = child.getFingerprint();
            if (fp == StandardNames.XS_APPINFO || fp == StandardNames.XS_DOCUMENTATION) {
                // ignore it
            } else {
                illegalElement(child);
            }
        }

    }

}

// Copyright (c) 2004 - 2007 Saxonica Limited
