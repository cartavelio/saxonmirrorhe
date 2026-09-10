package com.saxonica.ee.schema;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.ee.schema.sdoc.SimpleTypeDefinition;
import com.saxonica.ee.validate.SchemaAttributeTest;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.CallableDelegate;
import net.sf.saxon.functions.CallableFunction;
import net.sf.saxon.om.*;
import net.sf.saxon.pattern.NodeTest;
import net.sf.saxon.trans.Err;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.*;
import net.sf.saxon.value.*;

import static net.sf.saxon.type.SchemaValidationStatus.INVALID;
import static net.sf.saxon.type.SchemaValidationStatus.UNVALIDATED;
import static net.sf.saxon.type.SchemaValidationStatus.VALIDATED;
import static net.sf.saxon.type.SchemaValidationStatus.VALIDATING;

/**
 * An XML Schema Attribute Declaration. This includes global attribute declarations,
 * local attribute declarations, and "attribute uses". It corresponds to either (a)
 * an attribute declaration schema component for a global attribute, or (b) an attribute
 * declaration schema component for a local attribute, plus an attribute use referencing
 * that attribute declaration, or (c) an attribute use referencing a global attribute
 * declaration.
 */

public final class AttributeDecl extends SchemaStructure
        implements ComponentWithValueConstraint, UserSchemaComponent, SerializableSchemaComponent, SchemaDeclaration {

    /**
     * In the case of a locally defined attribute declaration that is not part of a named attribute group,
     * the containing complex type
     */
    private UserComplexType containingComplexType = null;

    /**
     * In the case of a locally defined attribute declaration that is part of a named attribute group,
     * the containing attribute group
     */

    private AttributeGroupDecl containingAttributeGroup = null;

    /**
     * The value constraint (fixed or default value), if any
     */

    private ValueConstraint valueConstraint;

    /**
     * Flag to indicate whether the attribute declaration is global
     */
    private final boolean global;

    /**
     * The name of the attribute
     */
    private StructuredQName attributeName;

    /**
     * The fingerprint of the name of the attribute
     */
    private int fingerprint;

    /**
     * The simple type for this AttributeDecl.
     */
    private TypeReference simpleTypeRef = null;

    /**
     * Namespace resolver for resolving QName-valued fixed or default values
     */

    private NamespaceResolver namespaceResolver;


    /**
     * Flag to indicate this attribute is inheritable
     */

    private boolean inheritable;

    /**
     * Creates a new AttrDecl in the given schema. For system use only.
     *
     * @param config the configuration that contains the new attrDecl
     * @param global true if the attribute declaration is global
     */

    public AttributeDecl(EnterpriseConfiguration config, boolean global) {
        this.global = global;
        setConfiguration(config);
    }

    /**
     * Determine whether this attribute declaration is global. This represents
     * one aspect of the {scope} property defined in the schema data model.
     *
     * @return true if the attribute declaration is global, false if it is local
     */

    public boolean isGlobal() {
        return global;
    }

    /**
     * Sets the containing complex type definition.
     * For internal use only.
     *
     * @param type the containing complex type
     */

    public void setContainingComplexType(UserComplexType type) {
        containingComplexType = type;
    }

    /**
     * Get the containing complex type. This represents one aspect of the {scope} property
     * defined in the schema component model. If the attribute is a local attribute declaration
     * and is defined as part of a complex type (and not as part of a named attribute group),
     * returns the complex type definition. Otherwise, returns null.
     *
     * @return the containing complex type definition if there is one, otherwise null
     */

    public UserComplexType getContainingComplexType() {
        return containingComplexType;
    }

    /**
     * Sets the containing attribute group definition.
     * For internal use only.
     *
     * @param type the containing attribute group
     */

    public void setContainingAttributeGroup(AttributeGroupDecl type) {
        containingAttributeGroup = type;
    }

    /**
     * Get the containing attribute group. This represents one aspect of the {scope} property
     * defined in the schema component model. If the attribute is a local attribute declaration
     * and is defined as part of an attribute group (and not as part of a complex type),
     * returns the attribute group. Otherwise, returns null.
     *
     * @return the containing attribute group if there is one, otherwise null
     */

    public AttributeGroupDecl getContainingAttributeGroup() {
        return containingAttributeGroup;
    }


    /**
     * Get the local name of this Attribute Declaration. This corresponds to
     * the {name} property as defined in the schema component model.
     *
     * @return the local name of the attribute
     */

    public String getName() {
        return attributeName.getLocalPart();
    }

    /**
     * Get the namespace URI of this element declaration. This corresponds to the
     * {targetNamespace} property defined in the schema component model.
     *
     * @return the target namespace URI
     */

    public NamespaceUri getTargetNamespace() {
        return attributeName.getURI();
    }

    /**
     * Returns the name of the attribute defined by this AttributeDecl.
     * This is used only for diagnostic display purposes
     *
     * @return the name of the attribute as a lexical QName.
     */

    public String getDisplayName() {
        return attributeName.getDisplayName();
    }

    /**
     * Set the attribute name. For internal use only.
     *
     * @param attributeName the attribute name.
     * @param fingerprint the fingerprint of the attribute name
     */

    public void setAttributeName(StructuredQName attributeName, int fingerprint) {
        this.attributeName = attributeName;
        this.fingerprint = fingerprint;
    }

    /**
     * Set the attribute name. For internal use only.
     *
     * @param attributeName the attribute name.
     */

    public void setAttributeName(StructuredQName attributeName) {
        this.attributeName = attributeName;
        this.fingerprint = getConfiguration().getNamePool().allocateFingerprint(
            attributeName.getURI(), attributeName.getLocalPart());
    }

    /**
     * Get the attribute name
     * @return the name of the attribute being declared
     */

    @Override
    public StructuredQName getComponentName() {
        return attributeName;
    }

    @Override
    public NamespaceResolver getNamespaceResolver() {
        return namespaceResolver;
    }

    /**
     * Sets the schema type for this attribute declaration.
     * For internal use only.
     *
     * @param type the schema type for this element declaration.
     */

    public void setTypeReference(TypeReference type) {
        simpleTypeRef = type;
    }

    /**
     * Gets the fingerprint of this attribute name
     *
     * @return the fingerprint of the attribute name in the NamePool
     */

    @Override
    public int getFingerprint() {
        return fingerprint;
    }

    /**
     * Get the type associated with this Attribute Declaration.
     *
     * @return the type of this attribute.
     * @throws MissingComponentException
     *          if the reference from
     *          the attribute declaration to its type has not been resolved
     */

    public SimpleType getSimpleType() throws MissingComponentException {
        return (SimpleType) simpleTypeRef.getTarget();
    }

    /**
     * Get the type of this attribute declaration. This corresponds to the
     * {typeDefinition} property defined in the schema component model
     *
     * @return the type of this attribute. The returned SchemaType will
     *         always be an instanceof {@link SimpleType}
     */

    @Override
    public SimpleType getType() throws MissingComponentException {
        return getSimpleType();
    }

    /**
     * Get the value constraint (fixed or default value) that applies to this element declaration,
     * if any
     *
     * @return the applicable value constraint if there is one, or null otherwise
     */

    @Override
    public ValueConstraint getValueConstraint() {
        return valueConstraint;
    }


    /**
     * Sets the SimpleType for this attribute declaration. For internal use only.
     *
     * @param simpleType the SimpleType for this attribute
     *                   declaration
     */
    public void setSimpleType(SimpleType simpleType) {
        if (simpleTypeRef != null) {
            simpleTypeRef.setTarget(simpleType);
        } else {
            simpleTypeRef = new TypeReference(simpleType.getFingerprint(), getConfiguration(), null);
            simpleTypeRef.setTarget(simpleType);
        }
    }

    /**
     * Set the value constraint (fixed or default value) that applies to this element
     * declaration
     *
     * @param vc the value constraint
     */

    @Override
    public void setValueConstraint(ValueConstraint vc) {
        this.valueConstraint = vc;
    }

    /**
     * Set the namespace resolver for resolving QName-valued fixed and default values
     *
     * @param resolver the namespace resolver
     */

    public void setNamespaceResolver(NamespaceResolver resolver) {
        namespaceResolver = resolver;
    }

    /**
     * Create a NodeTest that implements the semantics of schema-element(name) or
     * schema-attribute(name) applied to this element or attribute declaration.
     * @throws MissingComponentException if the type of the declaration is not present in the schema.
     */

    @Override
    public NodeTest makeSchemaNodeTest() throws MissingComponentException {
        return new SchemaAttributeTest(this);
    }

    /**
     * Say whether this attribute is inheritable
     *
     * @param inherit true if it is inheritable
     */

    public void setInheritable(boolean inherit) {
        this.inheritable = inherit;
    }

    /**
     * Ask whether this attribute is inheritable
     *
     * @return true if it is inheritable
     */

    public boolean isInheritable() {
        return inheritable;
    }

    /**
     * Determine, in the case of an Element Declaration, whether there are type alternatives defined
     *
     * @return false for an attribute declaration
     */
    @Override
    public boolean hasTypeAlternatives() {
        return false;
    }

    /**
     * Check references from this component to other components
     *
     * @param compiler used for error reporting
     * @return true if all is well, false if errors found
     */

    @Override
    public boolean fixup(SchemaCompiler compiler) throws SchemaException {
        if (getFixupStatus() == UNVALIDATED) {
            setFixupStatus(VALIDATING);
            SchemaType decl = (SchemaType) PreparedSchema.validateReference(simpleTypeRef, compiler, false);
            if (decl instanceof UserSchemaComponent && decl.getValidationStatus() != INVALID) {
                boolean b = ((UserSchemaComponent) decl).fixup(compiler);
                if (!b) {
                    decl = null;
                }
            }
            if (decl == null || decl.getValidationStatus() == INVALID) {
                setFixupStatus(INVALID);
                return false;
            }
        }
        setFixupStatus(VALIDATED);
        return true;
    }


    /**
     * Checks the validity of this Attribute declaration
     *
     * @throws SchemaException if the error listener decides an error is fatal
     */
    @Override
    public boolean validate(SchemaCompiler compiler) throws SchemaException {

        if (getFixupStatus() == INVALID) {
            setValidationStatus(INVALID);
            return false;
        }

        boolean result = true;

        SchemaType type = (SchemaType) PreparedSchema.validateReference(simpleTypeRef, compiler, false);
        if (type == null) {
            return true;
        }

        if (type.isComplexType()) {
            String err = "The complex type " + type.getDescription() +
                    " cannot be used as the type of an attribute";
            compiler.error(err, this);
            return false;
        }
        SimpleType simpleType = (SimpleType) type;

        if (simpleType instanceof SimpleTypeDefinition) {
            simpleType = ((SimpleTypeDefinition) simpleType).getWorkingType();
            simpleTypeRef.setTarget(simpleType);
        }


        SimpleType stype = simpleType;
        if (stype == null) {
            String err = "The type of attribute " + Err.wrap(getDisplayName(), Err.ATTRIBUTE) + " has not been specified";
            compiler.error(err, this);
            return result = false;
        }
        if (stype instanceof UserSimpleType) {
            result = ((UserSimpleType) stype).validate(compiler);
        }

        if (getFingerprint() == StandardNames.XML_ID) {
            if (stype.getFingerprint() != StandardNames.XS_ID) {
                compiler.error("If the xml:id attribute is used, it must be of type xs:ID", this);
                return false;
            }
        }

        if (result && compiler.getLanguageVersion() == Configuration.XSD10 &&
                getValueConstraint() != null &&
                stype.getBuiltInBaseType().getFingerprint() == StandardNames.XS_ID) {
            compiler.error("An attribute of type xs:ID must have no fixed or default value", this);
            return false;
        }

        if (valueConstraint != null) {
            result = valueConstraint.makeTypedValue(compiler, stype, getNamespaceResolver(), this);
        }

        return result;

    }


    /**
     * Test whether this is the same declaration as another attribute declaration.
     * They are considered to be the same declaration if they came from the same xs:attribute
     * element in the same schema document
     *
     * @param other the other attribute declaration
     * @return true if the two attribute declarations are considered identical
     */

    public boolean isSameDeclaration(AttributeDecl other) {
        return this == other ||
                (getFingerprint() == other.getFingerprint() && hasSameLocation(other)) ||
                 // assume that declarations in the XML namespace are consistent
                getTargetNamespace().equals(NamespaceUri.XML);
    }

    /**
     * Determine, in the case of an Element Declaration, whether it is nillable. For an Attribute declaration,
     * always return false;
     */

    @Override
    public boolean isNillable() {
        return false;
    }

    /**
     * Determine, in the case of an Element Declaration, whether it is nillable. For an Attribute declaration,
     * always return false;
     */

    @Override
    public boolean isAbstract() {
        return false;
    }

    /**
     * Serialize the schema component
     */

    @Override
    public void serialize(SchemaModelSerializer serializer) throws XPathException {
        String id = serializer.getId(this, true);
        serializer.startElement("attribute");
        serializer.emitAttribute("id", id);
        if (getName() != null) {
            serializer.emitAttribute("name", getName());
            if (getTargetNamespace() != null && !getTargetNamespace().isEmpty()) {
                serializer.emitAttribute("targetNamespace", getTargetNamespace().toString());
            }
        }

        SimpleType type = getType();
        serializer.emitAttribute("type", serializer.getTypeLink(type));
        serializer.emitAttribute("global", isGlobal() ? "true" : "false");
        serializer.emitAttribute("inheritable", isInheritable() ? "true" : "false");

        if (!isGlobal() && getContainingComplexType() != null) {
            serializer.emitAttribute("containingComplexType", serializer.getId(getContainingComplexType(), false));
        }
        if (getDefaultValueLexicalForm() != null) { // for backwards compatibility
            serializer.emitAttribute("default", getDefaultValueLexicalForm().toString());
        }
        if (getValueConstraint() != null) {
            getValueConstraint().serialize(serializer);
        }
        serializer.endElement();
    }

    @Override
    public Function getComponentAsFunction() {
        AttributeDecl thisAttributeDecl = this;
        CallableDelegate callable = new CallableDelegate((context, arguments) -> {
            String key = arguments[0].head().getStringValue();
            switch (key) {
                case "class":
                    return StringValue.bmp("Attribute Declaration");
                case "implementation":
                    return new ObjectValue<>(thisAttributeDecl);
                case "name":
                    return new StringValue(getName(), BuiltInAtomicType.NCNAME);
                case "target namespace":
                    return new AnyURIValue((getTargetNamespace().toUnicodeString()));
                case "type definition":
                    return getType().getComponentAsFunction();
                case "scope":
                    CallableDelegate scopeFunction = new CallableDelegate((context1, arguments1) -> {
                        String key1 = arguments1[0].head().getStringValue();
                        switch (key1) {
                            case "class":
                                return StringValue.bmp("Scope");
                            case "variety":
                                return StringValue.bmp(isGlobal() ? "global" : "local");
                            case "parent":
                                if (isGlobal()) {
                                    return EmptySequence.getInstance();
                                } else if (getContainingComplexType() != null) {
                                    return getContainingComplexType().getComponentAsFunction();
                                } else if (getContainingAttributeGroup() != null) {
                                    return getContainingAttributeGroup().getComponentAsFunction();
                                } else {
                                    return EmptySequence.getInstance();
                                }
                            default:
                                return EmptySequence.getInstance();
                        }
                    });
                    return new CallableFunction(1, scopeFunction, SpecificFunctionType.COMPONENT_FUNCTION_TYPE);
                case "value constraint":
                    return getValueConstraintAsFunction(AttributeDecl.this);
                case "inheritable":
                    return BooleanValue.get(isInheritable());
                default:
                    return EmptySequence.getInstance();
            }
        });
        return new CallableFunction(1, callable, SpecificFunctionType.COMPONENT_FUNCTION_TYPE);
    }

}

// Copyright (c) 2004-2022 Saxonica Limited