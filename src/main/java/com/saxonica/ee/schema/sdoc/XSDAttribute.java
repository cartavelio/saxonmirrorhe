package com.saxonica.ee.schema.sdoc;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.ee.schema.*;
import net.sf.saxon.Configuration;
import net.sf.saxon.om.*;
import net.sf.saxon.str.StringView;
import net.sf.saxon.tree.iter.AxisIterator;
import net.sf.saxon.type.*;

/**
 * An xs:attribute element in a schema document
 */

public class XSDAttribute extends SchemaElement {

    /**
     * The Attribute declaration for the Attribute we are constructing. Null if this is an attribute reference.
     */
    private AttributeDecl attributeDecl = null;

    /**
     * The attribute reference, if this is a reference
     */
    private AttributeUse attributeUse = null;

    private boolean foundAnnotation = false;
    private boolean foundSimpleType = false;
    private boolean typeSpecified = false;
    private boolean isReference = false;

    /**
     * Create an XSDAttribute, representing an xs:attribute on the schema document tree
     */

    public XSDAttribute() {
    }

    /**
     * Validate and process the attributes on this element in the schema document
     *
     * @throws SchemaException if any errors are found in the attributes
     */

    @Override
    protected void prepareAttributes() throws SchemaException {

        AttributeMap atts = attributes();
        if (isTopLevel()) {
            String[] allowed = {"default", "fixed", "id", "inheritable", "name", "type"};
            allowAttributes(atts, allowed);
            requireAttribute(atts, "name");
        } else {
            String[] allowed = {"default", "fixed", "form", "id", "inheritable", "name",
                    "ref", "targetNamespace", "type", "use"};
            allowAttributes(atts, allowed);
        }

        isReference = atts.getValue(NamespaceUri.NULL, "ref") != null;

        //-- @form
        String form = atts.getValue(NamespaceUri.NULL, "form");
        if (form != null) {
            if (isReference) {
                mutuallyExclusiveAttributes("form", "ref");
            }
            if (form.equals("qualified") || form.equals("unqualified")) {
                //attributeDecl.setForm(form);
            } else {
                invalidAttributeValue("form", form,
                        "Must be 'qualified' or 'unqualified'");
                // error recovery
                form = "qualified";
                //attributeDecl.setForm("qualified");
            }
        } else if (isTopLevel()) {
            form = "qualified";
            //attributeDecl.setForm("qualified");
        } else {
            form = getXSDSchema().getAttributeFormDefault();
            if (form != null) {
                //attributeDecl.setForm(form);
            } else {
                form = "unqualified";
                //attributeDecl.setForm("unqualified");
            }
        }

        // @targetNamespace
        String localTargetNamespace = atts.getValue(NamespaceUri.NULL, "targetNamespace");
        if (localTargetNamespace != null) {
            if (getXSDSchema().getSchemaCompiler().getLanguageVersion() == Configuration.XSD10) {
                error("The targetNamespace cannot be defined locally unless XSD 1.1 is enabled");
            }
            if (atts.getValue(NamespaceUri.NULL, "name") == null) {
                error("If there is no @name then there must be no @targetNamespace");
            }
            checkMutuallyExclusiveAttributes("targetNamespace", "form");
        }

        // @name
        String attValue = atts.getValue(NamespaceUri.NULL, "name");
        StructuredQName nc = null;
        if (attValue != null) {

            attributeDecl = new AttributeDecl((EnterpriseConfiguration) getConfiguration(), isTopLevel());
            attributeDecl.setLocator(this);

            if (attValue.equals("xmlns")) {
                error("The attribute name 'xmlns' is reserved");
            }
            if (atts.getValue(NamespaceUri.NULL, "ref") != null) {
                mutuallyExclusiveAttributes("name", "ref");
                nc = StandardNames.SQ_XS_INVALID_NAME; // error recovery
            } else {
                try {
                    if (localTargetNamespace != null || "qualified".equals(form)) {
                        if (getXSDSchema().getTargetNamespace().equals(NamespaceUri.SCHEMA_INSTANCE)) {
                            error("Attributes cannot be declared in the 'xsi:' namespace");
                        }
                        nc = getComponentName(attValue, REQUIRE_TARGET_NS);
                    } else {
                        nc = getComponentName(attValue, REQUIRE_NULL_NS);
                    }
                    attributeDecl.setAttributeName(nc);
                } catch (SchemaException err) {
                    error(err.getMessage());
                    nc = StandardNames.SQ_XS_INVALID_NAME;
                }
            }
            attributeDecl.setAttributeName(nc);

            // If this is a local attribute declaration we create two components, an attribute declaration
            // and an attribute use.

            if (!isTopLevel()) {
                attributeUse = new AttributeUse((EnterpriseConfiguration) getConfiguration());
                attributeUse.setTargetComponentName(attributeDecl.getComponentName());
                attributeUse.setTarget(attributeDecl);
                attributeUse.setLocator(this);
            }
        }

        // @ref
        attValue = atts.getValue(NamespaceUri.NULL, "ref");
        if (attValue != null) {
            attributeUse = new AttributeUse((EnterpriseConfiguration) getConfiguration());
            attributeUse.setLocator(this);
            try {
                nc = getComponentName(attValue, USE_DEFAULT_NS);
                if (nc.hasURI(NamespaceUri.SCHEMA_INSTANCE)) {
                    getConfiguration().addSchemaForBuiltInNamespace(NamespaceUri.SCHEMA_INSTANCE);
                }
            } catch (SchemaException err) {
                nc = StandardNames.SQ_XS_INVALID_NAME;
                error(err.getMessage());
            }
            attributeUse.setTargetComponentName(nc);
        }

        if (nc == null) {
            nc = StandardNames.SQ_XS_INVALID_NAME; // error already reported
        }

        // @default
        attValue = atts.getValue(NamespaceUri.NULL, "default");
        if (attValue != null && !nc.hasURI(NamespaceUri.SCHEMA_INSTANCE)) {
            ValueConstraint vc = new ValueConstraint(ValueConstraint.Variety.DEFAULT, StringView.tidy(attValue));
            if (attributeUse != null) {
                attributeUse.setValueConstraint(vc);
            } else if (attributeDecl != null) {
                // only set the default on the attribute declaration if there is no attribute use
                attributeDecl.setValueConstraint(vc);
            }
        }

        //-- @id
        processId();

        //-- @fixed
        attValue = atts.getValue(NamespaceUri.NULL, "fixed");
        if (attValue != null) {
            if (atts.getValue(NamespaceUri.NULL, "default") != null && !nc.hasURI(NamespaceUri.SCHEMA_INSTANCE)) {
                mutuallyExclusiveAttributes("fixed", "default");
            }
            ValueConstraint vc = new ValueConstraint(ValueConstraint.Variety.FIXED, StringView.tidy(attValue));
            if (attributeUse != null) {
                attributeUse.setValueConstraint(vc);
            } else if (attributeDecl != null) {
                // only set the default on the attribute declaration if there is no attribute use
                attributeDecl.setValueConstraint(vc);
            }
        }

        //-- @type
        attValue = atts.getValue(NamespaceUri.NULL, "type");
        int typeFingerprint = -1;
        if (attValue == null) {
            if (attributeDecl != null) {
                attributeDecl.setSimpleType(AnySimpleType.getInstance());
            }
        } else {
            typeSpecified = true;
            if (isReference) {
                mutuallyExclusiveAttributes("type", "ref");
                return;
            }
            try {
                typeFingerprint = getFingerprint(attValue, USE_DEFAULT_NS);
            } catch (SchemaException err) {
                error(err.getMessage());
                return;
            }
            checkPermittedType(typeFingerprint, attValue, "an attribute type");
            if (attributeDecl != null) {
                attributeDecl.setTypeReference(
                        new TypeReference(typeFingerprint, getSchema().getConfiguration(), this));
            }
        }

        //-- @use
        attValue = atts.getValue(NamespaceUri.NULL, "use");
        if (attValue != null) {
            boolean hasDefault = atts.getValue(NamespaceUri.NULL, "default") != null;
            if (hasDefault && !attValue.equals("optional")) {
                error("When @default is present, the @use attribute must have the value \"optional\"");
                attValue = "optional";  // error recovery
            }
            boolean hasFixed = atts.getValue(NamespaceUri.NULL, "fixed") != null;
            if (hasFixed && attValue.equals("prohibited") &&
                    getXSDSchema().getSchemaCompiler().getLanguageVersion() != Configuration.XSD10) {
                // see test attP029 and test bug 14245
                error("When @fixed is present, the @use attribute must not have the value \"prohibited\"");
                attValue = "optional";  // error recovery
            }
            Optionality useFlag;
            switch (attValue) {
                case "required":
                    useFlag = Optionality.REQUIRED;
                    break;
                case "optional":
                    useFlag = Optionality.OPTIONAL;
                    break;
                case "prohibited":
                    useFlag = Optionality.PROHIBITED;
                    break;
                default:
                    invalidAttributeValue("use", attValue, "Must be required|optional|prohibited");
                    useFlag = Optionality.OPTIONAL; // error recovery

                    break;
            }
            if (attributeUse != null) {
                attributeUse.setUse(useFlag);
            }
        }

        //-- @inheritable
        String inheritAtt = atts.getValue(NamespaceUri.NULL, "inheritable");
        if (inheritAtt != null) {
            if (getXSDSchema().getSchemaCompiler().getLanguageVersion() == Configuration.XSD10) {
                error("The inheritable attribute cannot be used unless XSD 1.1 is enabled");
            }
            boolean inherit = parseBooleanAttribute("inheritable", inheritAtt);
            if (attributeUse != null) {
                attributeUse.setInheritable(inherit ? 1 : 0);
            }
            if (attributeDecl != null) {
                attributeDecl.setInheritable(inherit);
            }
        }


        // The attributeUse and/or attributeDecl might need a NamespaceResolver to resolve QName-valued
        // fixed or default values. We'll give them one if (a) there's a fixed or default value, and (b)
        // the type isn't known to be non-namespace-sensitive

        if (atts.getValue(NamespaceUri.NULL, "fixed") != null || atts.getValue(NamespaceUri.NULL, "default") != null) {
            boolean needResolver = true;
            if (typeSpecified) {
                if (typeFingerprint < 1024) {
                    SchemaType type = BuiltInType.getSchemaType(typeFingerprint);
                    if (type instanceof SimpleType && !((SimpleType) type).isNamespaceSensitive()) {
                        needResolver = false;
                    }
                }
            }
            if (needResolver) {
                NamespaceResolver resolver = makeNamespaceContext();
                if (attributeDecl != null) {
                    attributeDecl.setNamespaceResolver(resolver);
                }
                if (attributeUse != null) {
                    attributeUse.setNamespaceResolver(resolver);
                }
            }
        }

    }

    /**
     * Get the attribute declaration. Returns null if this is simply a reference
     *
     * @return the attribute declaration, or null
     */
    public AttributeDecl getAttributeDeclaration() {
        return attributeDecl;
    }

    /**
     * Get the attribute use. Returns null if this is a global attribute declaration
     *
     * @return the attribute use, or null
     */
    /*@Nullable*/
    public AttributeUse getAttributeUse() {
        return attributeUse;
    }

    /**
     * Validate this schema element, checking all document-level constraints
     *
     * @param compiler used for reporting error messages
     * @throws SchemaException if any XML representation constraints are violated
     */

    @Override
    public void validate(SchemaCompiler compiler) throws SchemaException {

        String localTargetNamespace = getAttributeValue(NamespaceUri.NULL, "targetNamespace");
        if (localTargetNamespace != null &&
                !NamespaceUri.of(localTargetNamespace).equals(getXSDSchema().getTargetNamespace())) {
            checkLocalTargetNamespace("attribute");
        }

        // Set the containing complex type of this attribute declaration, if any

        if (attributeDecl != null) {
            AxisIterator ancIter = iterateAxis(AxisInfo.ANCESTOR);
            NodeInfo anc;
            while ((anc = ancIter.next()) != null) {
                if (anc instanceof XSDComplexType) {
                    attributeDecl.setContainingComplexType(((XSDComplexType) anc).getComplexType());
                    break;
                }
            }
        }

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
                if (typeSpecified) {
                    mutuallyExclusiveElementAndAttribute("simpleType", "type");
                }
                if (isReference) {
                    mutuallyExclusiveElementAndAttribute("simpleType", "ref");
                }
                if (foundSimpleType) {
                    duplicateElement("simpleType");
                }
                foundSimpleType = true;
            } else {
                illegalElement(child);
            }
        }

    }

    /**
     * Additional processing after all the children of this element have been validated.
     *
     * @param compiler user for reporting error messages
     */

    @Override
    public void postValidate(SchemaCompiler compiler) throws SchemaException {

        for (NodeInfo child : children()) {
            int fp = child.getFingerprint();

            if (fp == StandardNames.XS_SIMPLE_TYPE && attributeDecl != null) {
                SimpleTypeDefinition simpleTypeDef =
                        ((XSDSimpleType) child).getSimpleTypeDefinition();
                TypeReference ref = new TypeReference(
                        simpleTypeDef.getFingerprint(), getSchema().getConfiguration(), this);
                attributeDecl.setTypeReference(ref);
                simpleTypeDef.setContainingDeclaration(attributeDecl.getComponentName(), false);
                simpleTypeDef.setContextComponent(attributeDecl);
            }

        }

    }

}

// Copyright (c) 2011-2022 Saxonica Limited
