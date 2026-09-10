package com.saxonica.ee.schema;

import net.sf.saxon.Configuration;
import net.sf.saxon.expr.CallableDelegate;
import net.sf.saxon.functions.CallableFunction;
import net.sf.saxon.om.Function;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.sxpath.XPathDynamicContext;
import net.sf.saxon.sxpath.XPathExpression;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.SchemaException;
import net.sf.saxon.type.SimpleType;
import net.sf.saxon.type.SpecificFunctionType;
import net.sf.saxon.type.ValidationException;
import net.sf.saxon.value.*;

/**
 * Represents the preprocess facet on a schema-defined simple type. This is a pre-lexical facet
 * defined as a Saxon extension to XML Schema 1.1: it allows an XPath expression to be used to convert
 * the value as written (after whitespace normalization) to a different form, which is then validated
 * using the constraining facets. For example the preprocessing action translate($value, ',', '.') allows
 * the input value 3,14159 to be converted to 3.14159 which then validates as a double.
 */
public class PreprocessFacet extends Facet {

    Configuration config;
    Assertion action;
    Assertion reverseAction;

    public PreprocessFacet() {
    }

    public PreprocessFacet(Assertion action, Assertion reverseAction) {
        this.action = action;
        this.reverseAction = reverseAction;
    }

    /**
     * Ask whether this is a constraining facet (as distinct from a pre-lexical facet)
     *
     * @return true if this is a constraining facet
     */

    @Override
    public boolean isConstraining() {
        return false;
    }

    @Override
    public String getName() {
        return "preprocess";
    }

    /**
     * Returns the lexical representation of this facet
     *
     * @return the value of this facet
     */

    /*@Nullable*/
    @Override
    public UnicodeString getValue() {
        return null;
    }

    /**
     * Get the XPath expression representing the action performed by this facet, represented as
     * an Assertion object
     */

    public Assertion getAction() {
        return action;
    }

    /**
     * Get the XPath expression representing the reverse action performed by this facet, represented as
     * an Assertion object
     */

    public Assertion getReverseAction() {
        return reverseAction;
    }

    /**
     * Set the XPath expression representing the action performed by this facet, represented as
     * an Assertion object
     */

    public void setAction(Assertion action) {
        this.action = action;
    }

    /**
     * Set the XPath expression representing the reverse action performed by this facet, represented as
     * an Assertion object
     */

    public void setReverseAction(Assertion action) {
        this.reverseAction = action;
    }

    /**
     * Check that this facet is legal when used on a type derived by restriction
     *
     * @param base     the type from which the restricted type is derived
     * @param compiler
     * @throws net.sf.saxon.type.SchemaException
     *          if the facet is not legal
     */

    @Override
    public void checkFacetRestriction(UserSimpleType type, SimpleType base, SchemaCompiler compiler) throws SchemaException, ValidationException {
        // TODO: do we need any rules here? What happens if there is a preprocess facet on the base type?
    }

    /**
     * Apply the preprocessing defined by this facet
     *
     * @param input the lexical value, after whitespace normalization, to be converted
     * @return the value after preprocessing using the XPath expression defined in this facet
     */

    public UnicodeString preprocess(UnicodeString input) throws ValidationException {
        XPathExpression exp = action.getCondition();
        try {
            XPathDynamicContext context = exp.createDynamicContext();
            if (action.getValueVariable() != null) {
                context.setVariable(action.getValueVariable(), new StringValue(input));
            }
            return ((GroundedValue) exp.evaluateSingle(context)).getUnicodeStringValue();
        } catch (XPathException err) {
            throw new ValidationException(err);
        }
    }

    /**
     * Apply the postprocessing defined by this facet
     *
     * @param input the lexical value to be converted
     * @return the value after postprocessing using the reverse XPath expression defined in this facet
     */

    public UnicodeString postprocess(UnicodeString input) throws ValidationException {
        if (reverseAction == null) {
            return input;
        }
        XPathExpression exp = reverseAction.getCondition();
        try {
            XPathDynamicContext context = exp.createDynamicContext();
            if (action.getValueVariable() != null) {
                context.setVariable(reverseAction.getValueVariable(), new StringValue(input));
            }
            return ((GroundedValue) exp.evaluateSingle(context)).getUnicodeStringValue();
        } catch (XPathException err) {
            throw new ValidationException(err);
        }
    }


    /**
     * Test whether an atomic value conforms to this facet
     *
     * @param value the value to be tested
     * @return true if the value conforms; false if it doesn't conform, or if the test
     *         fails
     */

    @Override
    public boolean testAtomicValue(AtomicValue value) {
        return true;
    }

    @Override
    public void serializeFacet(SchemaModelSerializer serializer) throws XPathException {
        serializer.startElement(getName());
        if (isFixed()) {
            serializer.emitAttribute("fixed", "true");
        }
        action.serialize(serializer);
        if (reverseAction != null) {
            reverseAction.serialize(serializer);
        }
        serializer.endElement();
    }

    @Override
    public Function getFacetAsFunctionItem() {
        PreprocessFacet thisFacet = this;
        CallableDelegate callable = new CallableDelegate((context, arguments) -> {
            String key = arguments[0].head().getStringValue();
            switch (key) {
                case "class":
                    return new StringValue(getName());
                case "implementation":
                    return new ObjectValue<>(thisFacet);
                case "action":
                    return action.getComponentAsFunction();
                case "reverse action":
                    return reverseAction.getComponentAsFunction();
                case "fixed":
                    return BooleanValue.get(isFixed());
                default:
                    return EmptySequence.getInstance();
            }
        });
        return new CallableFunction(1, callable, SpecificFunctionType.COMPONENT_FUNCTION_TYPE);
    }
}

// Copyright (c) 2008-2022 Saxonica Limited