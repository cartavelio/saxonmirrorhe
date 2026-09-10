package com.saxonica.ee.schema;

import net.sf.saxon.om.AtomicSequence;
import net.sf.saxon.om.NamespaceResolver;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.value.EmptySequence;

/**
 * Interface for Element Declaration, Attribute Declaration and Attribute Use, created because they
 * share common functionality for handling fixed and default values
 */

public interface ComponentWithValueConstraint {

    ValueConstraint getValueConstraint();

    void setValueConstraint(ValueConstraint vc);

    default ValueConstraint getFixedValueConstraint() {
        ValueConstraint vc = getValueConstraint();
        if (vc != null && vc.getVariety() == ValueConstraint.Variety.FIXED) {
            return vc;
        } else {
            return null;
        }
    }

    default ValueConstraint getDefaultValueConstraint() {
        ValueConstraint vc = getValueConstraint();
        if (vc != null && vc.getVariety() == ValueConstraint.Variety.DEFAULT) {
            return vc;
        } else {
            return null;
        }
    }

    /**
     * For use with the saxon:schema extension function, return the value constraint in the form
     * of a function
     * @param att the component having the value constraint
     * @return a function whose result gives details of the value constraint
     */

    default Sequence getValueConstraintAsFunction(ComponentWithValueConstraint att) {
        if (getValueConstraint() != null) {
            return getValueConstraint().asFunction();
        } else {
            return EmptySequence.getInstance();
        }
    }

    /**
     * Returns the lexical form of the fixed value of this attribute use.
     *
     * @return the fixed value defined by this attribute use,
     * or null if no fixed value was specified. The value that is returned is the value as written
     * in the schema document with whitespace normalization applied as defined by the whiteSpace facet.
     */

    default UnicodeString getFixedValueLexicalForm() {
        ValueConstraint vc = getFixedValueConstraint();
        return vc == null ? null : vc.getLexicalForm();
    }

    /**
     * Returns the fixed value of this component. This corresponds
     * to one aspect of the {valueConstraint} property defined in the schema
     * component model.
     *
     * @return the fixed value defined by this attribute declaration, element declaration, or attribute use,
     * or null if no fixed value was specified. Once the schema has
     * been compiled this will be the correctly-typed value; until then
     * it will be the string value as specified in the source schema document.
     */

    /*@Nullable*/
    default AtomicSequence getFixedValue() {
        ValueConstraint vc = getFixedValueConstraint();
        return vc == null ? null : vc.getValue();
    }

    /**
     * Returns the lexical form of the default value of this component
     *
     * @return the default value defined by this component, or null if no default was specified.
     * The value returned is the value after whitespace normalization is applied to the value as
     * written in the source schema.
     */

    default UnicodeString getDefaultValueLexicalForm() {
        ValueConstraint vc = getDefaultValueConstraint();
        return vc == null ? null : vc.getLexicalForm();
    }

    /**
     * Gets the default value, as a typed value.
     *
     * @return the default value. This will initially be saved as an UntypedAtomicValue; later, when the type
     *              is known, it will be converted to the correct type
     */

    default AtomicSequence getDefaultValue() {
        ValueConstraint vc = getDefaultValueConstraint();
        return vc == null ? null : vc.getValue();
    }

    /**
     * Get the namespace resolver used when the fixed or value constraints include lexical QNames
     * @return the namespace resolver for lexical QNames
     */

    NamespaceResolver getNamespaceResolver();
}

// Copyright (c) 2018-2022 Saxonica Limited