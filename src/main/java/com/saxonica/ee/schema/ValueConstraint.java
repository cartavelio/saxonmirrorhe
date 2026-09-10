package com.saxonica.ee.schema;

import net.sf.saxon.Configuration;
import net.sf.saxon.expr.sort.SimpleTypeComparison;
import net.sf.saxon.lib.ConversionRules;
import net.sf.saxon.om.AtomicSequence;
import net.sf.saxon.om.NamespaceResolver;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.Err;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.*;
import net.sf.saxon.value.EmptySequence;
import net.sf.saxon.value.StringValue;
import net.sf.saxon.value.Whitespace;

import static com.saxonica.ee.schema.ValueConstraint.Variety.FIXED;

/**
 * A value constraint represents the default or fixed value of an element declaration, attribute
 * declaration, or attribute use
 */

public class ValueConstraint {

    public enum Variety {DEFAULT, FIXED}

    /**
     * The variety of this constraint: default or fixed
     */

    private final Variety variety;

    /**
     * The lexical form of the constraint (the default or fixed value as it appears in the source
     * schema document, after whitespace normalization if required)
     */
    private UnicodeString lexicalForm;

    /**
     * The typed value of this constraint
     */
    private AtomicSequence typedValue;

    /**
     * A value that can be used in comparisons to test whether this value is equal to another
     */
    private int comparisonHashCode;

    /**
     * Constructor; supplies the lexical form of the value as a string. The typed value will initially
     * be set to the lexical form as an instance of UntypedAtomic. The lexical form may be null, in which
     * case {@link #setValue} must be called to supply the typed value
     */

    public ValueConstraint(Variety variety, UnicodeString lexicalForm) {
        this.variety = variety;
        this.lexicalForm = lexicalForm;
        if (lexicalForm != null) {
            this.typedValue = new StringValue(lexicalForm);
        }
    }

    /**
     * Get the variety of the constraint (fixed or default)
     * @return the variety
     */

    public Variety getVariety() {
        return variety;
    }

    /**
     * Returns the lexical form of the value
     *
     * @return the fixed or default value defined by this component.
     * The value returned is the value after whitespace normalization is applied to the value as
     * written in the source schema.
     */

    public UnicodeString getLexicalForm() {
        return lexicalForm;
    }

    /**
     * Sets the lexical form of the value. For internal use only.
     *
     * @param value the lexical form of the value.
     */

    void setLexicalForm(UnicodeString value) {
        lexicalForm = value;
    }

    /**
     * Gets the value of the constraint as a typed value.
     *
     * @return the value. This will initially be saved as an UntypedAtomicValue; later, when the type
     * is known, it will be converted to the correct type
     */

    public AtomicSequence getValue() {
        return typedValue;
    }

    /**
     * Sets the value, as a typed value. For internal use only.
     *
     * @param value the value. This will initially be saved as an UntypedAtomicValue; later, when the type
     *              is known, it will be converted to the correct type
     */
    void setValue(AtomicSequence value) {
        this.typedValue = value;
        this.comparisonHashCode = SimpleTypeComparison.getInstance().hash(value);
    }

    /**
     * Test whether a suplied value matches the constraint
     */
    public boolean matches(AtomicSequence supplied) {
        SimpleTypeComparison comp = SimpleTypeComparison.getInstance();
        return comp.hash(supplied) == this.comparisonHashCode && comp.equal(this.typedValue, supplied);
    }
    /**
     * For use with the saxon:schema extension function, return the value constraint in the form
     * of a function
     *
     * @return a function whose result gives details of the value constraint
     */

    public Sequence asFunction() {
        return SchemaStructure.accessorFunction(arg -> {
            String key = arg.head().getStringValue();
            switch (key) {
                case "class":
                    return StringValue.bmp("Value Constraint");
                case "variety":
                    return StringValue.bmp(variety == FIXED ? "fixed" : "default");
                case "value":
                    return getValue() == null ?
                            new StringValue(getLexicalForm()) : getValue();
                case "lexical form":
                    return new StringValue(getLexicalForm());
                default:
                    return EmptySequence.getInstance();
            }
        });

    }

    /**
     * Convert the fixed value to a typed value using the appropriate type for the element or attribute.
     * This method must only be called if the element or attribute has a fixed value (and a simple type).
     *
     * @param compiler the schema compiler
     * @param stype    the declared type
     * @param resolver used to resolve any namespace prefixes if the type is namespace-sensitive
     * @param loc location to be used in any error diagnostics
     * @return true if the conversion succeeds
     * @throws SchemaException if a fatal error occurs
     */

    boolean makeTypedValue(SchemaCompiler compiler, SimpleType stype, NamespaceResolver resolver, Location loc) throws SchemaException {
        lexicalForm = Whitespace.applyWhitespaceNormalization(
                stype.getWhitespaceAction(), lexicalForm);
        ConversionRules rules = compiler.getConfiguration().getConversionRules();
        ValidationFailure err = stype.validateContent(lexicalForm, resolver, rules);
        if (err != null) {
            String val = Err.wrap(lexicalForm, Err.VALUE);
            compiler.error((variety == FIXED ? "Fixed" : "Default") + " value " + Err.wrap(val) +
                                   " does not not conform to the declared type: " + err.getMessage(), loc);
            return false;
        }

        try {
            if (stype.isNamespaceSensitive()) {
                setValue(stype.getTypedValue(lexicalForm, resolver, rules));
            } else if (stype.isAtomicType()) {
                setValue(Converter.convert(new StringValue(lexicalForm), (AtomicType) stype, rules));
            } else if (stype instanceof AnySimpleType) {
                setValue(new StringValue(lexicalForm));
            } else if (getValue().getLength() == 1) { // MHK 2018-11-14 - I don't understand this
                setValue(stype.getTypedValue(lexicalForm, null, rules));
            }
            return true;
        } catch (XPathException e) {
            compiler.error(e.getMessage(), loc);
            return false;
        }

    }

    /**
     * Test that the actual value of an element or attribute conforms to the required fixed value
     * @param config the Saxon configuration
     * @param value the actual value
     * @param type the required type
     * @param resolver a namespace resolver, in case QNames are involved
     * @return true if the value is OK, otherwise false
     * @throws ValidationException if the value is not valid as an instance of the type
     */

    public boolean testFixedValue(Configuration config, UnicodeString value, SimpleType type, NamespaceResolver resolver)
            throws ValidationException {
        if (variety != FIXED) {
            return true;
        }
        AtomicSequence actual = type.getTypedValue(value, resolver, config.getConversionRules());
        SimpleTypeComparison comp = SimpleTypeComparison.getInstance();
        return comp.hash(actual) == comparisonHashCode && comp.equal(actual, getValue());

    }

    /**
     * Serialize the value constraint
     */

    public void serialize(SchemaModelSerializer serializer) throws XPathException {
        serializer.startElement(variety == FIXED ? "fixed" : "default");
        serializer.emitAttribute("lexicalForm", getLexicalForm().toString());
        serializer.emitTypedValue(getValue());
        serializer.endElement();
    }


}

// Copyright (c) 2018-2023 Saxonica Limited