package com.saxonica.ee.schema.sdoc;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.ee.schema.SchemaCompiler;
import com.saxonica.ee.schema.TypeReference;
import net.sf.saxon.om.AttributeCollection;
import net.sf.saxon.om.AxisInfo;
import net.sf.saxon.om.StandardNames;
import net.sf.saxon.tree.iter.AxisIterator;
import net.sf.saxon.tree.linked.NodeImpl;
import net.sf.saxon.type.SchemaException;


/**
 * This class represents an xs:list element in a schema document.
 */

public class XSDList extends SchemaElement {

    /**
     * The simpleType under construction
     */
    private SimpleTypeDefinition typeDefinition;
    private boolean foundItemType = false;

    /**
     * Creates a new List element
     */

    public XSDList() {
    }

    /**
     * Validate and process the attributes on this element in the schema document
     *
     * @throws SchemaException if any errors are found in the attributes
     */

    public void prepareAttributes() throws SchemaException {

        typeDefinition = getContainingSimpleType();
        typeDefinition.setDerivationMethodName("list");

        AttributeCollection atts = getAttributeList();
        String[] allowed = {"id", "itemType"};
        allowAttributes(atts, allowed);

        //-- @id
        processId();

        //-- @itemType
        String itemTypeName = atts.getValue("", "itemType");
        if (itemTypeName != null) {
            foundItemType = true;
            int itemTypeFingerprint;
            try {
                itemTypeFingerprint = getFingerprint(itemTypeName, USE_DEFAULT_NS);
            } catch (SchemaException err) {
                error(err.getMessage());
                return;
            }
            if (itemTypeFingerprint == StandardNames.XS_DAY_TIME_DURATION ||
                    itemTypeFingerprint == StandardNames.XS_YEAR_MONTH_DURATION) {
                warning("Type " + itemTypeName + " is recognized by Saxon but is not interoperable");
            } else if (/*itemTypeFingerprint == StandardNames.XS_ANY_ATOMIC_TYPE || */
                    itemTypeFingerprint == StandardNames.XS_ANY_SIMPLE_TYPE ||
                            itemTypeFingerprint == StandardNames.XS_UNTYPED_ATOMIC ||
                            itemTypeFingerprint == StandardNames.XS_UNTYPED) {
                error("Type " + itemTypeName + " cannot be used as an item type");
            }
            TypeReference typeRef = new TypeReference(itemTypeFingerprint, (EnterpriseConfiguration) getConfiguration(), this);
            typeDefinition.setItemTypeReference(typeRef);
        }
    }

    /**
     * Validate this schema element, checking all document-level constraints
     *
     * @param compiler the schema compiler
     * @throws SchemaException if any XML representation constraints are violated
     */

    public void validate(SchemaCompiler compiler) throws SchemaException {
        boolean foundAnnotation = false;
        boolean foundSimpleType = false;
        AxisIterator iter = iterateAxis(AxisInfo.CHILD);
        NodeImpl child;
        while ((child = (NodeImpl)iter.next()) != null) {
            int fp = child.getFingerprint();
            switch (fp) {
                case StandardNames.XS_ANNOTATION: {
                    if (foundSimpleType) {
                        mustBeFirstElement("annotation");
                    }
                    if (foundAnnotation) {
                        duplicateElement("annotation");
                    }
                    foundAnnotation = true;
                    break;
                }
                case StandardNames.XS_SIMPLE_TYPE: {
                    if (foundItemType) {
                        mutuallyExclusiveElementAndAttribute("simpleType", "itemType");
                    }
                    if (foundSimpleType) {
                        duplicateElement("simpleType");
                    }
                    foundSimpleType = true;
                    break;
                }
                default: {
                    illegalElement(child);
                }
            }
        }
        if (!foundItemType && !foundSimpleType) {
            String err = "A <list> element must either have an @itemType attribute " +
                    "or a <simpleType> child";
            error(err);
        }
    }

    /**
     * Process the child elements after validation
     *
     * @param compiler the schema compiler
     */
    public void postValidate(SchemaCompiler compiler) throws SchemaException {
        AxisIterator iter = iterateAxis(AxisInfo.CHILD);
        NodeImpl child;
        while ((child = (NodeImpl)iter.next()) != null) {
            if (child.getFingerprint() == StandardNames.XS_SIMPLE_TYPE) {
                SimpleTypeDefinition itemType = ((XSDSimpleType) child).getSimpleTypeDefinition();
                TypeReference typeRef = new TypeReference(itemType.getFingerprint(),
                        (EnterpriseConfiguration) getConfiguration(),
                        (XSDSimpleType) child);
                typeRef.setTarget(itemType);
                typeDefinition.setItemTypeReference(typeRef);
            }
        }
    }
}

// Copyright (c) 2004 - 2007 Saxonica Limited
