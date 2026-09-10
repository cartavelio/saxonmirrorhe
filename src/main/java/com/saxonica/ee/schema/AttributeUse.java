package com.saxonica.ee.schema;

import com.saxonica.config.EnterpriseConfiguration;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.StaticProperty;
import net.sf.saxon.expr.sort.SimpleTypeComparison;
import net.sf.saxon.om.AtomicSequence;
import net.sf.saxon.om.FunctionItem;
import net.sf.saxon.om.NamespaceResolver;
import net.sf.saxon.om.StandardNames;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.Err;
import net.sf.saxon.trans.SaxonErrorCode;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.MissingComponentException;
import net.sf.saxon.type.SchemaException;
import net.sf.saxon.type.SimpleType;
import net.sf.saxon.value.BooleanValue;
import net.sf.saxon.value.EmptySequence;
import net.sf.saxon.value.ObjectValue;
import net.sf.saxon.value.StringValue;

import static net.sf.saxon.type.SchemaValidationStatus.VALIDATED;

/**
 * An XML Schema Attribute Use: that is, a reference to an attribute declaration
 * from within a complex type or attribute group.
 * <p>This corresponds to an xs:attribute ref="" element in the XML representation
 * of the schema.</p>
 * <p>Technically, a local attribute declaration (a non global xs:attribute name="")
 * should be represented by two schema components, an attribute use and an attribute
 * declaration. We don't currently do this, instead the containing attribute group
 * refers to the attribute declaration directly.</p>
 */

public final class AttributeUse extends SimpleComponentReference
        implements ComponentWithValueConstraint, UserSchemaComponent {

    /**
     * The current value of the 'use' property. The value
     * is OPTIONAL by default.
     */
    private Optionality useFlag = Optionality.OPTIONAL;

    /**
     * The value constraint (fixed or default value), if any
     */

    private ValueConstraint valueConstraint;

    /**
     * Namespace resolver for resolving QName-valued fixed or default values
     */

    private NamespaceResolver namespaceResolver;

    /**
     * If the attribute use appears within an attribute group, the containing attribute declaration
     */

    private AttributeGroupDecl containingAttributeGroupDecl;

    /**
     * Flag to indicate this attribute is inheritable: values are 0 (false), 1 (true), -1 (unspecified, use value
     * from attribute declaration)
     */

    private int inheritable = -1;

    /**
     * Creates a new Attribute Use in the given schema. For system use only.
     *
     * @param config the Configuration that contains the new attributeUse
     */

    public AttributeUse(EnterpriseConfiguration config) {
        setConfiguration(config);
        symbolSpace = SymbolSpace.ATTRIBUTE;
    }

    @Override
    public void tryToResolve(SchemaCompiler compiler, boolean fatal) throws SchemaException {
        super.tryToResolve(compiler, fatal);
        if (isResolved()) {
            AttributeDecl decl = (AttributeDecl)target;
            if (!decl.isGlobal()) {
                decl.setContainingAttributeGroup(getContainingAttributeGroupDecl());
            }
        } else {
            String name = getTargetComponentName().getEQName();
            if (compiler.isAllowDanglingReferences() && !fatal) {
                compiler.warning("Unknown attribute declaration " + name + " (substituting dummy declaration)", SaxonErrorCode.SXSD1001, this);
                AttributeDecl decl = new AttributeDecl(compiler.getConfiguration(), false);
                decl.setAttributeName(getTargetComponentName());
                decl.setSimpleType(BuiltInAtomicType.STRING);
                target = decl;
            } else {
                throw new SchemaException("Unknown attribute declaration " + name, this);
            }
        }
    }

    /**
     * Get the corresponding attribute declaration
     *
     * @return the attribute declaration referenced by this attribute use
     * @throws MissingComponentException
     *          if the reference from the AttributeUse
     *          to the AttributeDecl has not been resolved
     */

    public AttributeDecl getAttributeDeclaration() throws MissingComponentException {
        return (AttributeDecl) getTarget();
    }

    /**
     * Get the attribute group declaration containing this attribute use, if any
     *
     * @return the containing attribute declaration, or null
     */

    public AttributeGroupDecl getContainingAttributeGroupDecl() {
        return containingAttributeGroupDecl;
    }

    /**
     * Set the attribute group declaration containing this attribute use, if any
     *
     * @param containingAttributeGroupDecl the containing attribute declaration, or null
     */

    public void setContainingAttributeGroupDecl(AttributeGroupDecl containingAttributeGroupDecl) {
        this.containingAttributeGroupDecl = containingAttributeGroupDecl;
    }


    /**
     * Returns the name of the attribute defined by this AttributeDecl.
     * This is used only for diagnostic display purposes
     *
     * @return the name of the attribute as a lexical QName.
     */

    public String getDisplayName() {
        return getTargetComponentName().getDisplayName();
    }

    @Override
    public NamespaceResolver getNamespaceResolver() {
        return namespaceResolver;
    }

    /**
     * Get the value of the 'use' attribute for this attribute
     * declaration or attribute reference. If this is a reference
     * the value of the use attribute will *not* be obtained
     * from the referenced attribute declaration as top-level
     * attributes do not take into account the use attribute.
     *
     * @return the value of the 'use' attribute for this attribute
     *         declaration, as a string: "prohibited", "required", or "optional".
     */
    public String getUse() {
        switch (useFlag) {
            case PROHIBITED:
                return "prohibited";
            case REQUIRED:
                return "required";
            default:
                return "optional";
        }
    }

    /**
     * Get the value of the "use" attribute, expressed as a Cardinality value
     * of {@link StaticProperty#EXACTLY_ONE}, {@link StaticProperty#EMPTY}, or
     * {@link StaticProperty#ALLOWS_ZERO_OR_ONE}
     *
     * @return the cardinality implied by the "use" attribute.
     * If the attribute is optional but either the attribute use or the corresponding attribute
     * declaration defines a default or fixed value, then it is treated as optional (zero-or-one),
     * because we don't know whether fixed/default attribute values are being reported to the
     * application or not: see bug 3072.
     */

    public int getCardinality() {
        switch (useFlag) {
            case PROHIBITED:
                return StaticProperty.EMPTY;
            case REQUIRED:
                return StaticProperty.EXACTLY_ONE;
            default:
                return StaticProperty.ALLOWS_ZERO_OR_ONE;
        }
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
     * Get the default value of this attribute use if there is one; if there is none, get
     * the default value of the corresponding attribute declaration; if the declaration
     * defines no default value, return null
     *
     * @return the default attribute value if there is one, otherwise null
     */

    public UnicodeString getUnderlyingDefaultValue() throws MissingComponentException {
        if (getDefaultValueLexicalForm() == null) {
            AttributeDecl declaration = (AttributeDecl) getTarget();
            if (declaration != null) {
                return declaration.getDefaultValueLexicalForm();
            } else {
                return null;
            }
        } else {
            return getDefaultValueLexicalForm();
        }
    }

    /**
     * Returns the fixed value of this attribute use, or of the corresponding attribute
     * declaration if the attribute use has no fixed value.
     *
     * @return the fixed value defined by this attribute use, if one was specified;
     *         failing that, the fixed value defined for the corresponding attribute
     *         declaration; failing that, null. Once the schema has
     *         been compiled this will be the correctly-typed value; until then
     *         it will be the string value as specified in the source schema.
     */

    /*@Nullable*/
    public AtomicSequence getUnderlyingFixedValue() throws MissingComponentException {
        if (getFixedValue() == null) {
            AttributeDecl declaration = (AttributeDecl) getTarget();
            if (declaration != null) {
                return declaration.getFixedValue();
            } else {
                return null;
            }
        } else {
            return getFixedValue();
        }
    }

    /**
     * Set the namespace resolver for resolving QName-valued fixed and default values
     *
     * @param resolver the namespace resolver to be used
     */

    public void setNamespaceResolver(NamespaceResolver resolver) {
        namespaceResolver = resolver;
    }

    /**
     * Returns true if the use attribute is equal to "optional".
     *
     * @return true if the use attribute is equal to "optional".
     */
    public boolean isOptional() {
        return useFlag == Optionality.OPTIONAL;
    }

    /**
     * Returns true if the use attribute is equal to "prohibited". Note that in the W3C schema component model,
     * no attribute use ever has the value "prohibited": this should be treated as if the attribute use did
     * not exist.
     *
     * @return true if the use attribute is equal to "prohibited".
     */

    public boolean isProhibited() {
        return useFlag == Optionality.PROHIBITED;
    }

    /**
     * Returns true if the 'use' attribute is equal to REQUIRED and
     * there is no specified value. If a value is specified and the
     * 'use' attribute is  "required" then required is will return
     * false, because the attribute value automatically becomes
     * fixed.
     *
     * @return true if the use attribute is equal to "required" and
     *         no default value has been specified, otherwise false
     */

    public boolean isRequired() {
        return useFlag == Optionality.REQUIRED;
    }

    /**
     * Sets the 'use' attribute of this attribute declaration. For internal use only.
     *
     * @param value one of the following:
     *              ("prohibited" | "optional" | "required")
     */

    public void setUse(Optionality value) {
        useFlag = value;
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
     * Say whether this attribute is inheritable
     *
     * @param inherit values are 0 (false), 1 (true), -1 (unspecified, use value
     *                from attribute declaration)
     */

    public void setInheritable(int inherit) {
        this.inheritable = inherit;
    }

    /**
     * Ask whether this attribute is inheritable
     *
     * @return true if it is inheritable
     */

    public boolean isInheritable() throws MissingComponentException {
        switch (inheritable) {
            case 0:
                return false;
            case 1:
                return true;
            case -1:
            default:
                return ((AttributeDecl) getTarget()).isInheritable();
        }
    }


    /**
     * Fix up references to other elements in the schema.
     *
     * @param compiler the schema compiler, used for error reporting
     * @return true if successful.
     */

    @Override
    public boolean fixup(SchemaCompiler compiler) throws SchemaException {
        PreparedSchema.validateReference(this, compiler, false);
        setValidationStatus(VALIDATED);
        return true;
    }

    /**
     * Checks the validity of this Attribute Use
     *
     * @throws net.sf.saxon.type.SchemaException
     *          if the error listener decides an error is fatal
     */
    @Override
    public boolean validate(SchemaCompiler compiler) throws SchemaException {
        boolean result;

        tryToResolve(compiler, false);
        if (!isResolved()) {
            return false;
        }
        AttributeDecl declaration = (AttributeDecl) getTarget();

        result = declaration.validate(compiler);
        if (!result) {
            return false;
        }

        SimpleType stype = declaration.getType();
        EnterpriseConfiguration config = compiler.getConfiguration();

        if (compiler.getLanguageVersion() == Configuration.XSD10 &&
                (getFixedValue() != null || getDefaultValueLexicalForm() != null) &&
                stype.getBuiltInBaseType().getFingerprint() == StandardNames.XS_ID) {
            compiler.error("An attribute of type xs:ID must have no fixed or default value", this);
            return false;
        }

        if (valueConstraint != null) {
            boolean ok = valueConstraint.makeTypedValue(compiler, stype, namespaceResolver, this);
            if (!ok) {
                return false;
            }
        }

        AtomicSequence fixedVal = declaration.getFixedValue();
        if (fixedVal != null) {
            if (getDefaultValueLexicalForm() != null) {
                String err = "An attribute reference " + Err.wrap(getDisplayName(), Err.ATTRIBUTE) +
                        " cannot have a default value if the attribute declaration has a fixed value";
                compiler.error(err, this);
                return false;
            }
            AtomicSequence thisFixedValue = getFixedValue();
            if (thisFixedValue != null &&
                    !SimpleTypeComparison.getInstance().equal(thisFixedValue, fixedVal)) {
                String err = "The attribute reference " + Err.wrap(getDisplayName(), Err.ATTRIBUTE) +
                        " and the corresponding declaration have different fixed values";
                compiler.error(err, this);
                return false;
            }
        }
        return true;
    }

    /**
     * Serialize the schema component
     *
     * @param serializer the schema serializer
     * @throws XPathException if a failure occurs
     */

    public void serialize(SchemaModelSerializer serializer) throws XPathException {
        if (!isProhibited()) {
            serializer.startElement("attributeUse");
            serializer.emitAttribute("required", isRequired() ? "true" : "false");
            serializer.emitAttribute("inheritable", isInheritable() ? "true" : "false");
            serializer.emitAttribute("ref", serializer.getId(getAttributeDeclaration(), false));
            if (getDefaultValueLexicalForm() != null) { // for backwards compatibility
                serializer.emitAttribute("default", getDefaultValueLexicalForm().toString());
            }
            if (getValueConstraint() != null) {
                getValueConstraint().serialize(serializer);
            }
            serializer.endElement();
        }
    }

    @Override
    public FunctionItem getComponentAsFunction() {
        AttributeUse thisAttributeUse = this;
        return accessorFunction(arg -> {
            String key = arg.head().getStringValue();
            switch (key) {
                case "class":
                    return StringValue.bmp("Attribute Use");
                case "implementation":
                    return new ObjectValue<>(thisAttributeUse);
                case "required":
                    return BooleanValue.get(isRequired());
                case "attribute declaration":
                    return getAttributeDeclaration().getComponentAsFunction();
                case "value constraint":
                    return getValueConstraintAsFunction(AttributeUse.this);
                case "inheritable":
                    return BooleanValue.get(isInheritable());
                default:
                    return EmptySequence.getInstance();
            }
        });
    }

}

// Copyright (c) 2004-2023 Saxonica Limited