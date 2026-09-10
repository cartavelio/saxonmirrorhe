package com.saxonica.ee.schema;

import com.saxonica.config.EnterpriseConfiguration;
import net.sf.saxon.expr.CallableDelegate;
import net.sf.saxon.functions.CallableFunction;
import net.sf.saxon.om.Function;
import net.sf.saxon.om.StandardNames;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.trans.Err;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.*;
import net.sf.saxon.value.AnyURIValue;
import net.sf.saxon.value.EmptySequence;
import net.sf.saxon.value.ObjectValue;
import net.sf.saxon.value.StringValue;

import javax.xml.transform.SourceLocator;
import java.util.ArrayList;
import java.util.Stack;

import static net.sf.saxon.type.SchemaValidationStatus.INVALID;
import static net.sf.saxon.type.SchemaValidationStatus.VALIDATED;
import static net.sf.saxon.type.SchemaValidationStatus.VALIDATING;

/**
 * A Model Group Definition: a schema component corresponding to a top-level (named) xs:group
 * element in a schema document.
 */

public class ModelGroupDefinition extends ModelGroup implements SerializableSchemaComponent {

    /**
     * The name of the model group
     */
    private StructuredQName modelGroupName;

    /**
     * The model group definition that this one redefines. Only set if this is a redefining model group
     * that does not contain a self-reference; in this case validate() must check that it is a valid
     * refinement
     */

    private ModelGroupDefinition redefinedModelGroupDefinition;

    /**
     * Creates a new Model Group definition
     *
     * @param config  the Configuration to which this Model Group definition belongs
     * @param locator the source location of the model group definition
     */
    public ModelGroupDefinition(EnterpriseConfiguration config, SourceLocator locator) {
        setLocator(locator);
        setConfiguration(config);
    }

    /**
     * Set the model group definition that this one redefines. Only set if this is a redefining model group
     * that does not contain a self-reference; in this case validate() must check that it is a valid
     * refinement
     *
     * @param other the model group that this one redefines
     */
    public void setRedefinedModelGroupDefinition(ModelGroupDefinition other) {
        redefinedModelGroupDefinition = other;
    }

    public void setModelGroupName(StructuredQName nameCode) {
        this.modelGroupName = nameCode;
    }

    public StructuredQName getModelGroupName() {
        return modelGroupName;
    }

    /**
     * Returns the local part of the name of this named Group.
     *
     * @return the local name of this named Group.
     */
    public String getName() {
        return modelGroupName.getLocalPart();
    }

    /**
     * Returns the namespace URI of the name of this named Group.
     *
     * @return the namespace URI of this named Group: null if the group is in no namespace
     */
    public String getTargetNamespace() {
        return modelGroupName.getURI();
    }

    /**
     * Determine if empty content is allowed
     */

    @Override
    public boolean isEmptiable() throws MissingComponentException {
        for (Particle p : getParticles()) {
            if (!p.isEmptiable()) {
                return false;
            }
        }
        return true;
    }

    /**
     * Test whether the compositor is pointless, as described in XML Schema Part 1
     * Schema Component Constraint: Particle Valid (Restriction)
     *
     * @param container the particle that contains this model group definition
     * @return true if this compositor is pointless
     */

    @Override
    public boolean isPointless(ModelGroup container) {
        return contentModel.isEmpty() || contentModel.size() == 1;
    }

    /**
     * This method is called to look for cycles. The object implementing this
     * method is required (a) to raise an exception if the object itself appears
     * in the list of references, (b) to add itself to the list of references, and
     * (c) to call the lookForCycles method on all the objects that it references.
     *
     * @param references A list of objects that contain direct or indirect references
     *                   to this object, and that must therefore not be referred to from this object.
     * @param compiler   used for error reporting
     * @throws SchemaException if cycles are found
     */

    @Override
    public void lookForCycles(Stack<SchemaComponent> references, SchemaCompiler compiler)
            throws SchemaException {
        if (references.contains(this)) {
            String err = "The definition of group " +
                    Err.wrap(getModelGroupName().getDisplayName()) +
                    " is circular";
            // Empty the content model to prevent any further errors
            contentModel = new ArrayList<>(20);
            throw new SchemaException(err, this);
        }
        super.lookForCycles(references, compiler);
    }


    /**
     * Checks the validity of this Model Group Definition.
     *
     * @throws SchemaException if the declaration is invalid
     */
    @Override
    public boolean validate(SchemaCompiler compiler) throws SchemaException {
        boolean result = true;
        // check for previous validation
        if (!isValidationNeeded()) {
            return true;
        }
        setValidationStatus(VALIDATING);
        if (contentModel == null || contentModel.isEmpty()) {
            String err = "The group should contain 'all' or 'sequence' or 'choice'";
            compiler.error(err, this);
            setValidationStatus(INVALID);
            return false;
        }

        // We need to check that the new group definition is a valid refinement
        // of the old group definition. See Schema Part 1 section 4.2.2,
        // "Schema Representation Constraint: Redefinition Constraints and Semantics",
        // rule 6.2.2

        if (redefinedModelGroupDefinition != null) {
            TypeReference ref = new TypeReference(StandardNames.XS_ANY_TYPE, getConfiguration(), this);
            ref.setTarget(AnyType.getInstance());
            UserComplexType r = new UserComplexType(compiler.getConfiguration(), this);
            ModelGroupParticle rp = new ModelGroupParticle(compiler.getConfiguration(), null);
            rp.setTarget(this);
            r.setParticle(rp);
            r.setVariety(ComplexVariety.ELEMENT_ONLY);
            r.setBaseTypeReference(ref);
            r.validate(compiler);
            UserComplexType b = new UserComplexType(compiler.getConfiguration(), redefinedModelGroupDefinition);
            ModelGroupParticle bp = new ModelGroupParticle(compiler.getConfiguration(), null);
            bp.setTarget(redefinedModelGroupDefinition);
            b.setParticle(bp);
            b.setVariety(ComplexVariety.ELEMENT_ONLY);
            b.setBaseTypeReference(ref);
            b.validate(compiler);
            String problem = r.isSubsumedBy(b, compiler);
            if (problem != null) {
                compiler.error("Group " + getName() +
                        " is not a valid restriction of the group that it redefines. " + problem, this);
                result = false;
            }
        }
        result &= super.validate(compiler);
        setValidationStatus(result ? VALIDATED : INVALID);
        return result;
    }

    /**
     * Test whether this is the same group as another group. They are considered to be the same type
     * if they are derived from the same type definition in the original XML representation (which
     * can happen when there are multiple includes of the same file)
     *
     * @param other the other group
     * @return true if the groups are the same
     */

    public boolean isSameGroup(ModelGroupDefinition other) {
        return this == other ||
                (getModelGroupName().equals(other.getModelGroupName()) && hasSameLocation(other));

    }


    /**
     * Serialize the schema component
     */

    @Override
    public void serialize(SchemaModelSerializer serializer) throws XPathException {
        String id = serializer.getId(this, true);
        serializer.startElement("modelGroupDefinition");
        serializer.emitAttribute("id", id);
        serializer.emitAttribute("name", getName());
        if (getTargetNamespace() != null && !getTargetNamespace().isEmpty()) {
            serializer.emitAttribute("targetNamespace", getTargetNamespace());
        }
        for (Particle p : getParticles()) {
            p.serializeParticle(serializer);
        }
        serializer.endElement();
    }

    @Override
    public String getCompositorName() {
        return "modelGroupDefinition";
    }

    @Override
    public Function getComponentAsFunction() {
        ModelGroupDefinition thisDefinition = this;
        Function modelGroupFunction = super.getComponentAsFunction();
        CallableDelegate callable = new CallableDelegate((context, arguments) -> {
            String key = arguments[0].head().getStringValue();
            switch (key) {
                case "class":
                    return StringValue.bmp("Model Group Definition");
                case "implementation":
                    return new ObjectValue<>(thisDefinition);
                case "name":
                    return new StringValue(getName(), BuiltInAtomicType.NCNAME);
                case "target namespace":
                    return new AnyURIValue(getTargetNamespace());
                case "model group":
                    return modelGroupFunction;
                default:
                    return EmptySequence.getInstance();
            }

        });
        return new CallableFunction(1, callable, SpecificFunctionType.COMPONENT_FUNCTION_TYPE);
    }

}

// Copyright (c) 2004-2022 Saxonica Limited
