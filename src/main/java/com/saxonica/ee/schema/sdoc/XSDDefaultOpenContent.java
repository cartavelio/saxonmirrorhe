package com.saxonica.ee.schema.sdoc;

import com.saxonica.ee.schema.SchemaCompiler;
import com.saxonica.ee.schema.Wildcard;
import net.sf.saxon.Configuration;
import net.sf.saxon.om.AttributeCollection;
import net.sf.saxon.om.AxisInfo;
import net.sf.saxon.om.StandardNames;
import net.sf.saxon.tree.iter.AxisIterator;
import net.sf.saxon.tree.linked.NodeImpl;
import net.sf.saxon.type.SchemaException;
import net.sf.saxon.value.Whitespace;

/**
 * An element in an XML Schema document defining the default open content
 */
public class XSDDefaultOpenContent extends AnnotationParent {

    private boolean appliesToEmpty = false;
    private String mode = "interleave";

    /**
     * Creates a new XSDDefaultOpenContent element
     */
    public XSDDefaultOpenContent() {
    }

    /**
     * Validate and process the attributes on this element in the schema document
     *
     * @throws net.sf.saxon.type.SchemaException
     *          if any errors are found in the attributes
     */

    public void prepareAttributes() throws SchemaException {

        if (getXSDSchema().getSchemaCompiler().getLanguageVersion() == Configuration.XSD10) {
            error("The xs:defaultOpenContent element requires XSD 1.1 to be enabled");
        }

        AttributeCollection atts = getAttributeList();
        //-- handle attributes

        String[] allowed = {"appliesToEmpty", "id", "mode"};
        allowAttributes(atts, allowed);

        //-- @id
        processId();

        //-- @appliesToEmpty
        String attValue = atts.getValue("", "appliesToEmpty");
        if (attValue != null) {
            appliesToEmpty = parseBooleanAttribute("appliesToEmpty", attValue);
        }

        //-- @mode
        attValue = Whitespace.trim(atts.getValue("", "mode"));
        if (attValue != null) {
            if (attValue.equals("interleave") || attValue.equals("suffix")) {
                mode = attValue;
            } else {
                invalidAttributeValue("mode", attValue, "Must be 'interleave' or 'suffix'");
            }
        }
    }

    /**
     * Validate this schema element, checking all document-level constraints
     *
     * @param compiler the schema compiler
     * @throws net.sf.saxon.type.SchemaException
     *          if any XML representation constraints are violated
     */

    public void validate(SchemaCompiler compiler) throws SchemaException {
        checkContentModel(stateTerms, stateTargets, false);
        AxisIterator iter = iterateAxis(AxisInfo.CHILD);
        NodeImpl child;
        while ((child = (NodeImpl)iter.next()) != null) {
            int fp = child.getFingerprint();
            switch (fp) {
                case StandardNames.XS_ANY: {
                    Wildcard wildcard = ((XSDAny) child).getElementWildcard().getWildcard();
                    getXSDSchema().setDefaultOpenContent(wildcard, mode, appliesToEmpty);
                    break;
                }
            }
        }
    }

    private final static int[] state0terms = {StandardNames.XS_ANY, StandardNames.XS_ANNOTATION};
    private final static int[] state0targets = {1, 2};

    private final static int[] state1terms = {};
    private final static int[] state1targets = {};

    private final static int[] state2terms = {StandardNames.XS_ANY};
    private final static int[] state2targets = {1};

    private final static int[][] stateTerms = {
            state0terms, state1terms, state2terms};
    private final static int[][] stateTargets = {
            state0targets, state1targets, state2targets};

    protected boolean isFinalState(int state) {
        return state == 1;
    }


}

// Copyright (c) 2008 Saxonica Limited