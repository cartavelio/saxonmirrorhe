package com.saxonica.ee.schema;

import com.saxonica.config.EnterpriseConfiguration;
import net.sf.saxon.expr.CallableDelegate;
import net.sf.saxon.functions.CallableFunction;
import net.sf.saxon.om.Function;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.SpecificFunctionType;
import net.sf.saxon.value.AnyURIValue;
import net.sf.saxon.value.EmptySequence;
import net.sf.saxon.value.ObjectValue;
import net.sf.saxon.value.StringValue;

import javax.xml.transform.SourceLocator;

import static net.sf.saxon.type.SchemaValidationStatus.VALIDATED;

/**
 * A Notation Declaration Schema Component
 */

public class Notation extends SchemaStructure implements UserSchemaComponent, SerializableSchemaComponent {

    /**
     * The system identifier of the notation (possibly null)
     */
    private String notationSystemId;

    /**
     * The name code of the name of the notation
     */
    private StructuredQName notationName;

    /**
     * The public identifier of the notation (possibly null)
     */
    private String notationPublicId;

    /**
     * Creates a new Notation
     *
     * @param config  the Configuration to which this Notation belongs
     * @param locator the location of the declaration in the schema document
     */
    public Notation(EnterpriseConfiguration config, SourceLocator locator) {
        setConfiguration(config);
        setLocator(locator);
    }

    /**
     * Set the name code of the notation
     *
     * @param nameCode the NamePool code for the notation name
     */

    public void setNotationName(StructuredQName nameCode) {
        this.notationName = nameCode;
    }

    /**
     * Get the notation name
     *
     * @return the name of the notation as a NamePool fingerprint. This combines the values of the
     *         {name} and {target namespace} properties in the schema component model.
     */

    public StructuredQName getNotationName() {
        return notationName;
    }

    /**
     * Set the system identifier of the notation
     *
     * @param systemId the system identifier (possibly null)\
     */

    public void setNotationSystemId(String systemId) {
        this.notationSystemId = systemId;
    }

    /**
     * Set the public identifier of the notation
     *
     * @param publicId the public identifier (possibly null)\
     */

    public void setNotationPublicId(String publicId) {
        this.notationPublicId = publicId;
    }

    /**
     * Get the system identifier of the notation
     *
     * @return the value of the {system identifier} property (possibly null).
     */

    public String getNotationSystemId() {
        return notationSystemId;
    }

    /**
     * Get the public identifier of the notation
     *
     * @return the value of the {public identifier} property (possibly null)\
     */

    public String getNotationPublicId() {
        return notationPublicId;
    }

    /**
     * Return the local name of this Notation.
     *
     * @return the value of the {local name} property of the Notation Declaration
     */

    public String getName() {
        return notationName.getLocalPart();
    }

    /**
     * Return the target namespace of this Notation
     *
     * @return the value of the {target namespace} property of the Notation Declaration
     */

    public NamespaceUri getTargetNamespace() {
        return notationName.getNamespaceUri();
    }

    /**
     * Checks the validity of this Notation.
     * Always returns true for this implementation.
     */

    @Override
    public boolean validate(SchemaCompiler compiler) {
        return true;
    }

    /**
     * Check references from this component to other components
     *
     * @param compiler the schema compiler
     * @return true if all is well, false if errors found
     */

    @Override
    public boolean fixup(SchemaCompiler compiler) {
        setFixupStatus(VALIDATED);
        return true;
    }

    /**
     * Test whether this is the same notation as another. They are considered to be the same component
     * if they are derived from the same definition in the original XML representation (which
     * can happen when there are multiple includes of the same file)
     *
     * @param other the notation declaration that we are comparing with
     * @return true if the two notation declarations are identical
     */

    public boolean isSameDeclaration(Notation other) {
        return this == other ||
            (getNotationName().equals(other.getNotationName()) && hasSameLocation(other));

    }


    /**
     * Elaborate the schema component: after reloading a serialized schema component model,
     * this expands the component with derived information needed during validation episodes.
     * The model is assumed to be valid.
     *
     * @param compiler The schema compiler
     */

    @Override
    public void elaborate(SchemaCompiler compiler) {
        // no action
    }

    /**
     * Serialize the schema component
     */

    @Override
    public void serialize(SchemaModelSerializer serializer) throws XPathException {
        String id = serializer.getId(this, true);
        serializer.startElement("notation");
        serializer.emitAttribute("id", id);
        serializer.emitAttribute("name", getName());
        if (getTargetNamespace() != null && !getTargetNamespace().isEmpty()) {
            serializer.emitAttribute("targetNamespace", getTargetNamespace().toString());
        }
        if (notationPublicId != null) {
            serializer.emitAttribute("publicId", notationPublicId);
        }
        if (notationSystemId != null) {
            serializer.emitAttribute("systemId", notationSystemId);
        }
        serializer.endElement();

    }

    @Override
    public Function getComponentAsFunction() {
        Notation thisNotation = this;
        CallableDelegate callable = new CallableDelegate((context, arguments) -> {
                String key = arguments[0].head().getStringValue();
                switch (key) {
                    case "class":
                        return StringValue.bmp("Notation Declaration");
                    case "implementation":
                        return new ObjectValue<>(thisNotation);
                    case "name":
                        return new StringValue(getName(), BuiltInAtomicType.NCNAME);
                    case "target namespace":
                        return new AnyURIValue(getTargetNamespace().toUnicodeString());
                    case "system identifier":
                        if (getNotationSystemId() == null) {
                            return EmptySequence.getInstance();
                        } else {
                            return new AnyURIValue(getNotationSystemId());
                        }
                    case "public identifier":
                        if (getNotationPublicId() == null) {
                            return EmptySequence.getInstance();
                        } else {
                            return new AnyURIValue(getNotationSystemId());
                        }
                    default:
                        return EmptySequence.getInstance();
                }
        });
        return new CallableFunction(1, callable, SpecificFunctionType.COMPONENT_FUNCTION_TYPE);
    }


}

// Copyright (c) 2004-2022 Saxonica Limited