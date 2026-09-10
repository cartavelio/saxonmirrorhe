package com.saxonica.ee.schema;

import com.saxonica.config.EnterpriseConfiguration;
import net.sf.saxon.expr.CallableDelegate;
import net.sf.saxon.expr.parser.ContextItemStaticInfo;
import net.sf.saxon.expr.parser.ExpressionVisitor;
import net.sf.saxon.functions.CallableFunction;
import net.sf.saxon.om.*;
import net.sf.saxon.pattern.ContentTypeTest;
import net.sf.saxon.pattern.NodeTest;
import net.sf.saxon.str.StringTool;
import net.sf.saxon.sxpath.IndependentContext;
import net.sf.saxon.trans.SaxonErrorCode;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.*;
import net.sf.saxon.value.*;

import java.util.ArrayList;
import java.util.List;

import static net.sf.saxon.type.SchemaValidationStatus.VALIDATED;

/**
 * The base class for the XML Schema Identity Constraints
 * (key, keyref, unique).
 */

public abstract class IdentityConstraint extends SchemaStructure
        implements UserSchemaComponent, SerializableSchemaComponent {

    /**
     * Identity Constraint Selector
     */

    private IdentitySelector selector = null;

    /**
     * The fields of this Identity Constraint. A List whose members are of class IdentityField.
     */

    private List<IdentityField> fields = null;

    /**
     * The name of this identity constraint
     */

    private StructuredQName constraintName;

    /**
     * The type of constraint: one of "key", "unique", or "keyref"
     * @return one of "key", "unique", or "keyref" (note the spelling)
     */

    public abstract String getConstraintCategory();

    /**
     * Boolean set to true if this is an ordered constraint (currently true
     * only for xs:unique; indicated by saxon:ordered="true")
     */

    private boolean ordered;

    /**
     * Constructor used by sub-classes. Creates a new IdentityConstraint.
     */
    protected IdentityConstraint() {
        fields = new ArrayList<>(3);
    }

    /**
     * Adds the given IdentityField to this IdentityConstraint
     *
     * @param field the IdentityField to add.
     */

    public void addField(IdentityField field) {
        if (field != null) {
            fields.add(field);
        }
    }

    /**
     * Returns the List of Fields contained within this IdentityConstraint.
     *
     * @return the List of fields
     */

    public List<IdentityField> getFields() {
        return fields;
    }

    /**
     * Return the name of this IdentityConstraint. This value will never be null.
     *
     * @return the name of this IdentityConstraint
     */

    public String getName() {
        return constraintName.getLocalPart();
    }

    /**
     * Return the target namespace of this IdentityConstraint.
     *
     * @return the target namespace of this IdentityConstraint. Return null if the
     *         target namespace is "absent".
     */

    public NamespaceUri getTargetNamespace() {
        return constraintName.getNamespaceUri();
    }

    /**
     * Returns the selector of this IdentityConstraint.
     *
     * @return the IdentitySelector of this IdentityConstraint
     */
    /*@Nullable*/
    public IdentitySelector getSelector() {
        return selector;
    }

    /**
     * Sets the selector for this IdentityConstraint.
     *
     * @param selector the Selector for this IdentityConstraint. Must not be
     *                 null.
     */
    public void setSelector(IdentitySelector selector) {
        this.selector = selector;
    }

    /**
     * Set the integer cdde of the name of this identity constraint
     *
     * @param nc the namecode
     */

    public void setConstraintName(StructuredQName nc) {
        constraintName = nc;
    }

    /**
     * Get the name of this identity constraint
     *
     * @return the component's name
     */

    public StructuredQName getConstraintName() {
        return constraintName;
    }

    public void setOrdered(boolean ordered) {
        this.ordered = ordered;
    }

    public boolean isOrdered() {
        return ordered;
    }

    /**
     * Check references from this component to other components
     *
     * @param compiler the schema compiler
     * @return true if all is well, false if errors found
     */

    @Override
    public boolean fixup(SchemaCompiler compiler) throws SchemaException {
        setFixupStatus(VALIDATED);
        return true;
    }

    /**
     * Checks the validity of this Schema definition.
     *
     * @throws SchemaException if the definition is invalid
     */
    @Override
    public boolean validate(SchemaCompiler compiler) throws SchemaException {
        String err = null;

        //-- name must be a NCName
        if (!NameChecker.isValidNCName(StringTool.codePoints(getName()))) {
            err = "The name of an IdentityConstraint must be an NCName.";
        }
        //-- selector must exist
        else if (selector == null) {
            err = "Selector for IdentityConstraint cannot be null.";
        }
        //-- at least 1 (one) field must exist
        else if (fields.size() < 1) {
            err = "There must be at least one field in an identity constraint.";
        }

        if (err != null) {
            compiler.error(err, this);
            return false;
        }

        return true;
    }

    /**
     * Typecheck the selector and the fields. This reports warnings if the XPath expressions used in defining
     * the constraint cannot select anything, or under similar conditions
     *
     * @param compiler    the schema compiler
     * @param contextType the type of the element whose constraint is being defined
     * @throws net.sf.saxon.trans.XPathException
     *          if a type error is found
     */

    public void typeCheck(final SchemaCompiler compiler, SchemaType contextType) throws XPathException {
        final EnterpriseConfiguration config = compiler.getConfiguration();
        IndependentContext env = new IndependentContext(config);
        env.setWarningHandler((message, code, loc) -> compiler.warning(message, code, this));
        env.setSchemaAware(true);
        ExpressionVisitor visitor = ExpressionVisitor.make(env);
        ItemType contextItemType = new ContentTypeTest(Type.ELEMENT, contextType, config, false);
        ContextItemStaticInfo cit = config.makeContextItemStaticInfo(contextItemType, true);
        selector.setSelectExpression(selector.getSelectExpression().typeCheck(visitor, cit));
        ItemType selectorType = selector.getSelectExpression().getItemType();
        if (selectorType instanceof NodeTest && ((NodeTest) selectorType).getContentType() != AnyType.getInstance()) {
            ContextItemStaticInfo sit = config.makeContextItemStaticInfo(selectorType, false);
            for (IdentityField field : fields) {
                field.setSelectExpression(field.getSelectExpression().typeCheck(visitor, sit));
                int card = field.getCardinality(visitor, sit);
                if (Cardinality.allowsMany(card)) {
                    compiler.warning("The field expression (" + field.getXPath() + ") may select multiple nodes, which would make the instance document invalid", SaxonErrorCode.SXSD1004, field);
                } else if (this instanceof Key && Cardinality.allowsZero(card)) {
                    compiler.warning("The field expression (" + field.getXPath() + ") may select no nodes, which would make the instance document invalid", SaxonErrorCode.SXSD1004, field);
                }
                NodeTest fieldType = (NodeTest) field.getSelectExpression().getItemType();
                AtomicType atomizedType = fieldType.getAtomizedItemType();
                if (atomizedType == null) {
                    compiler.warning("The node selected by the field expression cannot be atomized", SaxonErrorCode.SXSD1005, field);
                }
                SchemaType contentType = fieldType.getContentType();
                if (!(contentType == AnyType.getInstance() || contentType.isSimpleType() || contentType.isComplexType() && ((ComplexType) contentType).isSimpleContent())) {
                    // we allow AnyType because although it has mixed content, it has subtypes that are simple types, which would be OK.
                    compiler.warning("The node selected by the field expression must have a simple type, or a complex type with simple content", SaxonErrorCode.SXSD1005, field);
                }
                @SuppressWarnings("ConstantConditions")
                boolean listValued = contentType.isSimpleType() && ((SimpleType)contentType).isListType();
                if (atomizedType != null
                        && ((ItemType)atomizedType).isAtomicType()
                        && contentType != BuiltInAtomicType.ANY_ATOMIC) {
                    field.setBuiltInBaseType((BuiltInAtomicType) atomizedType.getBuiltInBaseType(), listValued);
                }
            }
        }
    }

    /**
     * Test whether this is the same constraint as another. They are considered to be the same component
     * if they are derived from the same definition in the original XML representation (which
     * can happen when there are multiple includes of the same file)
     *
     * @param other the notation declaration that we are comparing with
     * @return true if the two notation declarations are identical
     */

    public boolean isSameDeclaration(IdentityConstraint other) {
        return this == other ||
            (getConstraintName().equals(other.getConstraintName()) && hasSameLocation(other));

    }

    /**
     * Elaborate the schema component: after reloading a serialized schema component model,
     * this expands the component with derived information needed during validation episodes.
     * The model is assumed to be valid.
     *
     * @param compiler the schema compiler
     */

    @Override
    public void elaborate(SchemaCompiler compiler) {
        // no action
    }

    /**
     * Serialize the identity constraint as part of the serialized Schema Component Model
     */

    @Override
    public void serialize(SchemaModelSerializer serializer) throws XPathException {
        serializer.setIsSerialized(this);
        String kind = getConstraintCategory();
        serializer.startElement(kind);
        serializer.emitAttribute("id", serializer.getId(this, false));
        serializer.emitAttribute("name", getName());
        if (getTargetNamespace() != null) {
            serializer.emitAttribute("targetNamespace", getTargetNamespace().toString());
        }
        if (this instanceof KeyRef) {
            IdentityConstraint target = (IdentityConstraint) ((KeyRef) this).getTarget();
            serializer.emitAttribute("key", serializer.getId(target, false));
        }
        serializer.startElement("selector");
        serializer.emitNamespaceContext(selector.getNamespaceContext());
        serializer.emitAttribute("xpath", selector.getXPath());
        serializer.emitAttribute("defaultNamespace", selector.getXPathDefaultNamespace().toString());
        serializer.endElement();
        for (IdentityField f : fields) {
            serializer.startElement("field");
            serializer.emitNamespaceContext(f.getNamespaceContext());
            serializer.emitAttribute("xpath", f.getXPath());
            serializer.emitAttribute("defaultNamespace", f.getXPathDefaultNamespace().toString());
            BuiltInAtomicType baseType = f.getBuiltInBaseType();
            if (baseType != null) {
                serializer.emitAttribute("type", "#" + baseType.getName() +
                        (f.isListValued() ? "*" : ""));
            }
            serializer.endElement();
        }
        serializer.endElement();
    }

    @Override
    public Function getComponentAsFunction() {
        IdentityConstraint thisConstraint = this;
        CallableDelegate callable = new CallableDelegate((context, arguments) -> {
            String key = arguments[0].head().getStringValue();
            switch (key) {
                case "class":
                    return StringValue.bmp("Identity-Constraint Definition");
                case "implementation":
                    return new ObjectValue<>(thisConstraint);
                case "name":
                    return new StringValue(getName(), BuiltInAtomicType.NCNAME);
                case "target namespace":
                    return new AnyURIValue((getTargetNamespace().toUnicodeString()));
                case "identity-constraint category":
                    String kind = IdentityConstraint.this.getConstraintCategory();
                    return new StringValue(kind);
                case "selector":
                    IdentitySelector selector = getSelector();
                    return SchemaStructure.makeXPathExpressionPropertyRecord(
                            selector.getXPath(), selector.getNamespaceContext(), selector.getSchemaDocumentURI());
                case "fields":
                    List<Item> list = new ArrayList<>();
                    for (IdentityField field : getFields()) {
                        list.add(SchemaStructure.makeXPathExpressionPropertyRecord(
                                field.getXPath(), field.getNamespaceContext(), field.getSchemaDocumentURI()));
                    }
                    return SequenceExtent.makeSequenceExtent(list);
                case "referenced key":
                    if (IdentityConstraint.this instanceof KeyRef) {
                        return ((KeyRef) IdentityConstraint.this).getTarget().getComponentAsFunction();
                    } else {
                        return EmptySequence.getInstance();
                    }
                default:
                    return EmptySequence.getInstance();
            }
        });
        return new CallableFunction(1, callable, SpecificFunctionType.COMPONENT_FUNCTION_TYPE);
    }
}

// Copyright (c) 2004-2022 Saxonica Limited