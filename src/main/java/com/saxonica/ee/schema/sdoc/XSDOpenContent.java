package com.saxonica.ee.schema.sdoc;

import com.saxonica.ee.schema.ElementWildcard;
import com.saxonica.ee.schema.SchemaCompiler;
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
public class XSDOpenContent extends AnnotationParent {

    private String mode = "interleave";
    private ElementWildcard wildcard;

    /**
     * Creates a new XSDDefaultOpenContent element
     */
    public XSDOpenContent() {
    }

    /**
     * Validate and process the attributes on this element in the schema document
     *
     * @throws net.sf.saxon.type.SchemaException
     *          if any errors are found in the attributes
     */

    public void prepareAttributes() throws SchemaException {

        AttributeCollection atts = getAttributeList();
        //-- handle attributes

        String[] allowed = {"id", "mode"};
        allowAttributes(atts, allowed);

        //-- @id
        processId();

        //-- @mode
        String attValue = Whitespace.trim(atts.getValue("", "mode"));
        if (attValue != null) {
            if (attValue.equals("interleave") || attValue.equals("suffix") || attValue.equals("none")) {
                mode = attValue;
            } else {
                invalidAttributeValue("mode", attValue, "Must be 'interleave' or 'suffix' or 'none");
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
                    if ("none".equals(mode)) {
                        error("When mode='none', the <xs:openContent> element must not have an <xs:any> child element");
                        // Became an error as a consequence of bug 7069
                    } else {
                        wildcard = ((XSDAny) child).getElementWildcard();
                    }
                    break;
                }
            }
        }
        if (wildcard == null && !"none".equals(mode)) {
            error("Except when mode='none', the <xs:openContent> element must have an <xs:any> child element");
            mode = "none";
        }
    }

    private final static int[] state0terms = {StandardNames.XS_ANNOTATION, StandardNames.XS_ANY};
    private final static int[] state0targets = {1, 2};

    private final static int[] state1terms = {StandardNames.XS_ANY};
    private final static int[] state1targets = {2};

    private final static int[] state2terms = {};
    private final static int[] state2targets = {};

    private final static int[][] stateTerms = {
            state0terms, state1terms, state2terms};
    private final static int[][] stateTargets = {
            state0targets, state1targets, state2targets};

    protected boolean isFinalState(int state) {
        return state == 0 || state == 1 || state == 2;
    }

    public String getMode() {
        return mode;
    }

    /*@Nullable*/
    public ElementWildcard getElementWildcard() {
        return wildcard;
    }


}

// Copyright (c) 2008 Saxonica Limited