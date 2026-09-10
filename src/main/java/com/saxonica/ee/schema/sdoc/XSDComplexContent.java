package com.saxonica.ee.schema.sdoc;

//-- imported classes and packages

import com.saxonica.ee.schema.SchemaCompiler;
import net.sf.saxon.om.AttributeCollection;
import net.sf.saxon.om.StandardNames;
import net.sf.saxon.type.SchemaException;

/**
 * An xs:complexContent element in an XML Schema Document
 */


public class XSDComplexContent extends SchemaElement {

    /**
     * The value of the mixed attribute: true, false, or null (unspecified)
     */

    private Boolean mixed = null;

    /**
     * Creates a new ComplexContent element
     */
    public XSDComplexContent() {
    }

    /**
     * Validate and process the attributes on this element in the schema document
     *
     * @throws SchemaException if any errors are found in the attributes
     */

    public void prepareAttributes() throws SchemaException {

        AttributeCollection atts = getAttributeList();
        String[] allowed = {"id", "mixed"};
        allowAttributes(atts, allowed);

        //-- @id
        processId();

        //-- @mixed
        String mixedAtt = atts.getValue("", "mixed");
        if (mixedAtt != null) {
            // It's not an error in XSD 1.0 if the value conflicts with an explicit value
            // on the complexType element - this value wins. But in XSD 1.1, the values must be consistent
            mixed = parseBooleanAttribute("mixed", mixedAtt);
        }

    }


    /**
     * Validate this schema element, checking all document-level constraints
     *
     * @param compiler the schema compiler
     * @throws SchemaException if any XML representation constraints are violated
     */

    public void validate(SchemaCompiler compiler) throws SchemaException {
        checkContentModel(stateTerms, stateTargets, false);
    }

    // Following FSM generated using:
    // Transform s4s11.scm generate-s4s-code.xsl type=C277
    // where C277 is the complexType of element name="complexContent"

    private final static int[] state0terms =
            {StandardNames.XS_RESTRICTION, StandardNames.XS_EXTENSION, StandardNames.XS_ANNOTATION};
    private final static int[] state0targets = {1, 1, 2};

    private final static int[] state1terms = {};
    private final static int[] state1targets = {};

    private final static int[] state2terms =
            {StandardNames.XS_RESTRICTION, StandardNames.XS_EXTENSION};
    private final static int[] state2targets = {1, 1};

    private final static int[][] stateTerms = {
            state0terms, state1terms, state2terms};
    private final static int[][] stateTargets = {
            state0targets, state1targets, state2targets};

    protected boolean isFinalState(int state) {
        return state == 1;
    }

    /*@Nullable*/
    public Boolean getMixedAttribute() {
        return mixed;
    }
}

// Copyright (C) 2004-2007 Saxonica Limited
