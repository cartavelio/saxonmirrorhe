package com.saxonica.ee.schema.sdoc;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.ee.schema.SchemaCompiler;
import com.saxonica.ee.schema.TypeReference;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.om.*;
import net.sf.saxon.regex.RegularExpression;
import net.sf.saxon.str.EmptyUnicodeString;
import net.sf.saxon.str.StringView;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.SchemaException;

import java.util.ArrayList;
import java.util.List;


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

    @Override
    protected void prepareAttributes() throws SchemaException {

        typeDefinition = getContainingSimpleType();
        typeDefinition.setDerivationMethodName("list");

        AttributeMap atts = attributes();
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

        //-- @saxon:separator
        String separator = atts.getValue(NamespaceConstant.SAXON, "separator");
        if (separator != null) {
            List<String> warnings = new ArrayList<>();
            try {
                RegularExpression regex = getConfiguration().compileRegularExpression(
                        StringView.tidy(separator), "", "XSD11", warnings);
                if (regex.matches(EmptyUnicodeString.getInstance())) {
                    error("The regular expression in saxon:separator must not be one that matches a zero-length string");
                }
                for (String message : warnings) {
                    warning(message);
                }
                typeDefinition.setTokenSeparator(regex);
            } catch (XPathException e) {
                error("Error in saxon:separator regular expression: " + e.getMessage());
            }
        }
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
                    break;
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
    @Override
    public void postValidate(SchemaCompiler compiler) throws SchemaException {
        for (NodeInfo child : children()) {
            if (child.getFingerprint() == StandardNames.XS_SIMPLE_TYPE) {
                SimpleTypeDefinition itemType = ((XSDSimpleType) child).getSimpleTypeDefinition();
                TypeReference typeRef = new TypeReference(itemType.getFingerprint(),
                        (EnterpriseConfiguration) getConfiguration(),
                                                          child);
                typeRef.setTarget(itemType);
                typeDefinition.setItemTypeReference(typeRef);
            }
        }
    }
}

// Copyright (c) 2004-2022 Saxonica Limited
