package com.saxonica.ee.schema.sdoc;

import com.saxonica.ee.schema.ExplicitTimezoneFacet;
import com.saxonica.ee.schema.Facet;
import com.saxonica.ee.schema.PatternFacet;
import com.saxonica.ee.schema.SchemaCompiler;
import net.sf.saxon.Configuration;
import net.sf.saxon.om.AttributeMap;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.type.SchemaException;

import java.util.ArrayList;
import java.util.List;

/**
 * An element in an XML Schema document representing a facet, for example minLength or pattern or enumeration:
 * any facet except assertion, which is represented by the {@link XSDAssert} class
 */
public class XSDFacet extends AnnotationParent {

    /**
     * The Facet we are constructing
     */
    private Facet facet = null;

    /**
     * Creates a new Facet element
     */
    public XSDFacet() {
    }

    /**
     * Validate and process the attributes on this element in the schema document
     *
     * @throws SchemaException if any errors are found in the attributes
     */

    @Override
    protected void prepareAttributes() throws SchemaException {

        AttributeMap atts = attributes();
        //-- handle attributes

        if (getLocalPart().equals("enumeration") || getLocalPart().equals("pattern")) {
            String[] allowed = {"id", "value"};
            allowAttributes(atts, allowed);
        } else {
            String[] allowed = {"fixed", "id", "value"};
            allowAttributes(atts, allowed);
        }

        String value = atts.getValue(NamespaceUri.NULL, "value");
        if (value == null) {
            if (getLocalPart().equals("distinct")) {
                value = "true";
            } else {
                missingAttribute("value");
                value = "";
            }
        }

        try {
            facet = Facet.makeFacet(getLocalPart(), value,
                    getSchema().getConfiguration(),
                    makeNamespaceContext()
            );
        } catch (SchemaException err) {
            error(err.getMessage());
            return;
        }

        // Saxon extension: @saxon:message="error message"

        String message = atts.getValue(NamespaceUri.SAXON, "message");
        if (message != null) {
            facet.setMessage(message);
        }

        // Saxon extension for pattern facet: @saxon:flags="flags"

        if (facet instanceof PatternFacet) {
            String flags = atts.getValue(NamespaceUri.SAXON, "flags");
            if (flags != null) {
                ((PatternFacet) facet).setFlags(flags);
            }
            List<String> warnings = new ArrayList<>();
            try {
                ((PatternFacet) facet).compile(warnings);
            } catch (SchemaException e) {
                error(e.getMessage());
            }
            for (String s : warnings) {
                warning(s);
            }
        }

        //-- @fixed = true/false

        String fixedAtt = atts.getValue(NamespaceUri.NULL, "fixed");
        boolean fixed = false;
        if (fixedAtt != null) {
            fixed = parseBooleanAttribute("fixed", fixedAtt);
        }

        facet.setFixed(fixed);

        //-- @id
        processId();
    }

    /**
     * Validate this schema element, checking all document-level constraints
     *
     * @param compiler the schema compiler
     * @throws net.sf.saxon.type.SchemaException
     *          if any XML representation constraints are violated
     */

    @Override
    public void validate(SchemaCompiler compiler) throws SchemaException {
        super.validate(compiler);
        if (compiler.getLanguageVersion() == Configuration.XSD10) {
            if (facet instanceof ExplicitTimezoneFacet) {
                error("Timezone facet requires XSD 1.1 to be enabled");
            }
        }
    }

    /**
     * Get the Facet "component" created from this facet definition
     *
     * @return the Facet component (not strictly a schema component, but effectively part of one)
     */

    /*@Nullable*/
    public Facet getFacet() {
        return facet;
    }


}

// Copyright (c) 2004-2022 Saxonica Limited
