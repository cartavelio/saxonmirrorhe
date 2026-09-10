package com.saxonica.ee.schema.sdoc;

import com.saxonica.ee.schema.Notation;
import net.sf.saxon.om.AttributeMap;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.om.StandardNames;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.type.SchemaException;

/**
 * An element in an XML Schema document representing a notation declaration
 */
public class XSDNotation extends AnnotationParent {

    private Notation notation;

    /**
     * Creates a new XSDNotation
     */
    public XSDNotation() {
    }

    /**
     * Validate and process the attributes on this element in the schema document
     *
     * @throws net.sf.saxon.type.SchemaException
     *          if any errors are found in the attributes
     */

    @Override
    protected void prepareAttributes() throws SchemaException {

        notation = new Notation(getSchema().getConfiguration(), this);

        AttributeMap atts = attributes();
        //-- handle attributes

        String[] allowed = {"id", "name", "public", "system"};
        allowAttributes(atts, allowed);

        //-- @id
        processId();

        //-- @name
        String name = atts.getValue(NamespaceUri.NULL, "name");
        if (name == null) {
            missingAttribute("name");
            name = "unnamed-notation-" + hashCode();
        }

        StructuredQName nc;
        try {
            nc = getComponentName(name, REQUIRE_TARGET_NS);
        } catch (SchemaException err) {
            error(err.getMessage());
            nc = StandardNames.SQ_XS_INVALID_NAME;
        }
        notation.setNotationName(nc);

        //-- @public
        String publicAtt = atts.getValue(NamespaceUri.NULL, "public");
        if (publicAtt != null) {
            notation.setNotationPublicId(publicAtt);
        }

        //-- @system
        String systemAtt = atts.getValue(NamespaceUri.NULL, "system");
        if (systemAtt != null) {
            notation.setNotationSystemId(systemAtt);
        }

        if (publicAtt == null && systemAtt == null) {
            error("A notation must have either a public identifier or a system identifier");
        }

//        if (publicAtt != null && systemAtt != null) {
//            warning("For compatibility, a notation should not have both a public identifier and a system identifier");
//        }
    }

    public Notation getNotation() {
        return notation;
    }


}

// Copyright (c) 2004-2022 Saxonica Limited
