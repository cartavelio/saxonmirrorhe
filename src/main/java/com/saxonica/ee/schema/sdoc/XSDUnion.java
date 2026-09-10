package com.saxonica.ee.schema.sdoc;

import com.saxonica.ee.schema.SchemaCompiler;
import com.saxonica.ee.schema.TypeReference;
import net.sf.saxon.Configuration;
import net.sf.saxon.om.AttributeMap;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.StandardNames;
import net.sf.saxon.type.SchemaException;

import java.util.StringTokenizer;

/**
 * An xs:union element in an XML Schema document
 */

public class XSDUnion extends SchemaElement {

    /**
     * The simple type definition for this union
     */

    private SimpleTypeDefinition typeDefinition;

    /**
     * Count of member types
     */
    private int numberOfMemberTypes = 0;

    /**
     * Create a new XSDUnion element
     */

    public XSDUnion() {
    }

    /**
     * Validate and process the attributes on this element in the schema document
     *
     * @throws SchemaException if any errors are found in the attributes
     */

    @Override
    protected void prepareAttributes() throws SchemaException {

        typeDefinition = getContainingSimpleType();

        AttributeMap atts = attributes();
        String[] allowed = {"id", "memberTypes"};
        allowAttributes(atts, allowed);

        //-- @id
        processId();

        typeDefinition.setDerivationMethodName("union");

        String memberTypes = atts.getValue(NamespaceUri.NULL, "memberTypes");
        processMemberTypes(memberTypes);

    }

    /**
     * Validate this schema element, checking all document-level constraints
     *
     * @param compiler the schema compiler
     * @throws SchemaException if any XML representation constraints are violated
     */

    @Override
    public void validate(SchemaCompiler compiler) throws SchemaException {
        boolean foundAnnotation = false;
        boolean foundSimpleType = false;
        for (NodeInfo child : children()) {
            int fp = child.getFingerprint();

            if (fp == StandardNames.XS_ANNOTATION) {
                if (foundAnnotation) {
                    duplicateElement("annotation");
                }
                if (foundSimpleType) {
                    mustBeFirstElement("annotation");
                }
                foundAnnotation = true;
            } else if (fp == StandardNames.XS_SIMPLE_TYPE) {
                foundSimpleType = true;
                numberOfMemberTypes++;
            } else {
                illegalElement(child);
            }
        }
        if (numberOfMemberTypes == 0) {
            String err = "A union type must have at least one member type";
            error(err);
        }
    }

    /**
     * Process child elements after validation
     *
     * @param compiler the schema compiler
     */

    @Override
    public void postValidate(SchemaCompiler compiler) throws SchemaException {
        for (NodeInfo child : children()) {
            if (child.getFingerprint() == StandardNames.XS_SIMPLE_TYPE) {
                SimpleTypeDefinition memberType = ((XSDSimpleType) child).getSimpleTypeDefinition();
                int fp = ((XSDSimpleType) child).getSimpleTypeDefinition().getFingerprint();
                TypeReference ref = new TypeReference(fp, compiler.getConfiguration(), child);
                ref.setTarget(memberType);
                typeDefinition.addMemberType(ref);
            }
        }

    }

    /**
     * Process the given string into the referenced simpleTypes
     * for the Union being constructed.
     *
     * @param memberTypes the memberTypes list.
     */
    private void processMemberTypes(/*@Nullable*/ String memberTypes) throws SchemaException {
        if ((memberTypes == null) || (memberTypes.isEmpty())) {
            return;
        }
        StringTokenizer st = new StringTokenizer(memberTypes, " \t\n\r", false);
        while (st.hasMoreTokens()) {
            String typeName = st.nextToken();
            int fp;
            try {
                fp = getFingerprint(typeName, USE_DEFAULT_NS);
            } catch (SchemaException err) {
                error(err.getMessage());
                return;
            }
            if (fp == StandardNames.XS_DAY_TIME_DURATION ||
                    fp == StandardNames.XS_YEAR_MONTH_DURATION) {
                if (getConfiguration().getXsdVersion() == Configuration.XSD10) {
                    warning("Type " + typeName + " is recognized by Saxon but is not interoperable");
                }
            } else if (fp == StandardNames.XS_ANY_SIMPLE_TYPE ||
                    fp == StandardNames.XS_ANY_ATOMIC_TYPE ||
                    fp == StandardNames.XS_UNTYPED_ATOMIC ||
                    fp == StandardNames.XS_UNTYPED) {
                if (getConfiguration().getXsdVersion() == Configuration.XSD10) {
                    warning("In XSD 1.1, type " + typeName + " cannot be used as a member type of a union");
                } else {
                    error("In XSD 1.1, type " + typeName + " cannot be used as a member type of a union");
                }
            }
            TypeReference ref = new TypeReference(fp, typeDefinition.getConfiguration(), this);
            typeDefinition.addMemberType(ref);
            numberOfMemberTypes++;
        }
    }
}

// Copyright (c) 2004-2022 Saxonica Limited