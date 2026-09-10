package com.saxonica.ee.schema;

import com.saxonica.config.EnterpriseConfiguration;
import net.sf.saxon.expr.MappingIterator;
import net.sf.saxon.lib.ConversionRules;
import net.sf.saxon.om.AtomicArray;
import net.sf.saxon.om.AtomicSequence;
import net.sf.saxon.om.EmptyAtomicSequence;
import net.sf.saxon.om.NamespaceResolver;
import net.sf.saxon.regex.RegularExpression;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.Err;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.transpile.CSharp;
import net.sf.saxon.tree.iter.AtomicIterator;
import net.sf.saxon.type.*;
import net.sf.saxon.value.StringValue;
import net.sf.saxon.value.Whitespace;

import java.util.ArrayList;
import java.util.List;
import java.util.function.Function;

/**
 * Represents a SimpleType that is a list of a given
 * SimpleType, known as its itemType. A ListType may be a restriction
 * of another ListType; the itemType must either be an AtomicType or
 * a UnionType whose members are not list types.
 * <p>This class is not used to implement the built-in
 * list types NMTOKENS, ENTITIES, IDREFS.</p>
 */

public class UserListType extends UserSimpleType implements ListType {

    /**
     * A reference to the SimpleType of the items in the list.
     */

    private TypeReference itemType = null;

    /**
     * Create a new user-defined ListType.
     *
     * @param config the Saxon configuration
     */

    public UserListType(EnterpriseConfiguration config) {
        setConfiguration(config);
    }

    /**
     * Test whether this Simple Type is an atomic type
     *
     * @return false, this is not an atomic type
     */

    @Override
    public boolean isAtomicType() {
        return false;
    }

    /**
     * Ask whether this type is an ID type. This is defined to be any simple type
     * who typed value may contain atomic values of type xs:ID: that is, it includes types derived
     * from ID by restriction, list, or union. Note that for a node to be treated
     * as an ID in XSD 1.0, its typed value must be a *single* atomic value of type ID; the type of the
     * node, however, can still allow a list. But in XSD 1.1, a list of IDs is permitted
     */

    @Override
    public boolean isIdType() throws MissingComponentException {
        return getItemType().isIdType();
    }

    /**
     * Ask whether this type is an IDREF or IDREFS type. This is defined to be any simple type
     * who typed value may contain atomic values of type xs:IDREF: that is, it includes types derived
     * from IDREF or IDREFS by restriction, list, or union
     */

    @Override
    public boolean isIdRefType() throws MissingComponentException {
        return getItemType().isIdRefType();
    }

    /**
     * Determine whether this is a built-in type or a user-defined type
     */

    @Override
    public boolean isBuiltInType() {
        return false;
    }


    /**
     * Returns true if this type is derived by list, or if it is derived by restriction
     * from a list type, or if it is a union that contains a list as one of its members
     */

    @Override
    public boolean isListType() {
        return true;
    }

    private Function<UnicodeString, AtomicIterator> tokenizerFactory =
            CSharp.constructorRef(Whitespace.Tokenizer::new, 1);

    /**
     * Get the extended facet list, creating it if necessary. The extended facet
     * list includes facets inherited from supertypes, unless they are redundant
     *
     * @return the extended facet list
     */

    @Override
    protected List<Facet> addInheritedFacets(List<Facet> localFacets) {
        localFacets = super.addInheritedFacets(localFacets);
        for (int i = 0; i < localFacets.size(); i++) {
            Facet f = localFacets.get(i);
            if (f instanceof LengthFacet || f instanceof MinLengthFacet) {
                return localFacets;
            }
        }

        SchemaType base = this;
        while (base != null) {
            base = base.getBaseType();
            if (base instanceof BuiltInListType) {
                // All the built-in list types have a minLength facet of 1
                List<Facet> extendedFacets = new ArrayList<>(localFacets);
                MinLengthFacet minLength = new MinLengthFacet();
                minLength.setNumericValue(1);
                extendedFacets.add(minLength);
                return extendedFacets;
            }
        }
        return localFacets;
    }

    @Override
    public void elaborate(SchemaCompiler compiler) throws SchemaException {
        // before elaborating a list type we must elaborate its item type
        SimpleType st = (SimpleType) itemType.getTarget();
        if (st instanceof UserSimpleType) {
            ((UserSimpleType) st).elaborate(compiler);
        }
        super.elaborate(compiler);
    }


    /**
     * Get the reference to the item type of the list
     *
     * @return the type reference
     */

    public TypeReference getItemTypeReference() {
        return itemType;
    }

    /**
     * Returns the simpleType of the items in this ListType. This method assumes that the
     * item type has been fully resolved
     *
     * @return the simpleType of the items in this ListType. This will either be an AtomicType
     *         or a UnionType
     * @throws MissingComponentException if the item type has not been fully resolved
     */

    /*@NotNull*/
    @Override
    public SimpleType getItemType() throws MissingComponentException {
        return (SimpleType) itemType.getTarget();
    }

    /**
     * Sets the itemType for this ListType (the type of
     * item that instances of this list type contain).
     * For internal use only.
     *
     * @param type the SimpleType for this ListType.
     */

    public void setItemTypeReference(TypeReference type) {
        itemType = type;
    }

    /**
     * Set the regular expression defined in saxon:separator for tokenizing the supplied value
     * @param regex the regular expression
     */

    public void setTokenSeparator(RegularExpression regex) {
        if (regex != null) {
            tokenizerFactory = value -> regex.tokenize(value);
        }
    }

    public void inheritSeparator(UserListType base) {
        tokenizerFactory = base.tokenizerFactory;
    }

    /**
     * Check whether a given input string is valid according to this SimpleType
     *
     * @param value      the input string to be checked
     * @param nsResolver a namespace resolver used to resolve namespace prefixes if the type
     *                   is namespace sensitive. The value supplied may be null; in this case any namespace-sensitive
     *                   content will throw an UnsupportedOperationException.
     * @param rules      the conversion rules used in this configuration
     * @return null if validation succeeds; return a ValidationException describing the validation failure
     *         if validation fails, unless throwException is true, in which case the exception is thrown rather than
     *         being returned.
     * @throws UnsupportedOperationException if the type is namespace-sensitive and no namespace
     *                                       resolver is supplied
     */

    /*@Nullable*/
    @Override
    public ValidationFailure validateContent(/*@NotNull*/ UnicodeString value, NamespaceResolver nsResolver, /*@NotNull*/ ConversionRules rules) {
        if (value.isEmpty()) {
            return checkAgainstListFacets(value, nsResolver, rules, 0);
        } else {
            SimpleType itemType = null;
            try {
                itemType = getItemType();
            } catch (MissingComponentException e) {
                return new ValidationFailure(e.getMessage());
            }
            AtomicIterator iter = tokenizerFactory.apply(value);
            int count = 0;
            StringValue val;
            //try {
                while ((val = (StringValue)iter.next()) != null) {
                    ValidationFailure err = itemType.validateContent(val.getUnicodeStringValue(), nsResolver, rules);
                    if (err != null) {
                        return err;
                    }
                    count++;
                }
//            } catch (XPathException e) {
//                return new ValidationFailure(e.getMessage());
//            }
            return checkAgainstListFacets(value, nsResolver, rules, count);
        }
    }

    /**
     * Check whether a supplied value is valid against the facets defined on the list type. This method does NOT check
     * that the items in the list are valid against the item type of the list
     *
     * @param value      the supplied value
     * @param nsResolver the namespace context
     * @param rules      the conversion rules
     * @param count      the number of items in the list
     * @return either null if the value is valid, or a ValidationFailure giving diagnostics
     */

    public ValidationFailure checkAgainstListFacets(UnicodeString value, NamespaceResolver nsResolver, ConversionRules rules, int count) {
        AtomicSequence typedValue = null;
        for (Facet f : getExtendedFacetList()) {
            if (f instanceof PatternFacet) {
                boolean match = f.testAtomicValue(
                        new StringValue(Whitespace.collapseWhitespace(value)));
                if (!match) {
                    ValidationFailure vf = new ValidationFailure(
                            "List " + Err.wrap(value) + " contravenes the pattern facet " +
                                    Err.wrap(f.getValue()));
                    vf.setConstraintReference(2, "cvc-pattern-valid", "1");
                    return vf;
                }
            } else if (f instanceof EnumerationFacetSet) {
                try {
                    if (typedValue == null) {
                        typedValue = ((SimpleType) getBaseType()).getTypedValue(value, nsResolver, rules);
                    }
                    boolean match = f.testListValue(typedValue);
                    if (!match) {
                        String message = "List " + Err.wrap(value) +
                                " does not match any of the values in the enumeration facet " + Err.wrap(f.getValue());
                        if (f.getMessage() != null) {
                            message += ". " + f.getMessage();
                        }
                        ValidationFailure vf = new ValidationFailure(message);
                        vf.setConstraintReference(2, "cvc-enumeration-valid", "1");
                        return vf;
                    }
                } catch (XPathException e) {
                    ValidationFailure vf = new ValidationFailure(
                            "List " + Err.wrap(value) + " cannot be compared against the enumeration facet " +
                                    Err.wrap(f.getValue()) + ": " + e.getMessage());
                    vf.setConstraintReference(2, "cvc-enumeration-valid", "1");
                    return vf;
                }
            } else if (f instanceof DistinctFacet || f instanceof OrderFacet) {
                try {
                    if (typedValue == null) {
                        typedValue = ((SimpleType) getBaseType()).getTypedValue(value, nsResolver, rules);
                    }
                } catch (ValidationException e) {
                    ValidationFailure vf = new ValidationFailure(
                            "List " + Err.wrap(value) + " cannot be compared against the " + f.getName() + " facet " +
                                    Err.wrap(f.getValue()) + ": " + e.getMessage());
                    vf.setConstraintReference(2, "cvc-facet-valid", "1");
                    return vf;
                }
                boolean match = f.testListValue(typedValue);
                if (!match) {
                    ValidationFailure vf = new ValidationFailure(
                            "List " + Err.wrap(value) + " does not satisfy the saxon:" + f.getName() + " facet");
                    vf.setConstraintReference(2, "cvc-facet-valid", "1");
                    return vf;
                }
            } else if (f instanceof AssertionFacet) {
                try {
                    if (typedValue == null) {
                        typedValue = ((SimpleType) getBaseType()).getTypedValue(value, nsResolver, rules);
                    }
                    boolean match = ((AssertionFacet) f).testListValue(typedValue, rules);
                    if (!match) {
                        String message = f.getMessage();
                        if (message == null) {
                            message = "List " + Err.wrap(value) + " does not satisfy the assertion " + Err.wrap(f.getValue());
                        }
                        ValidationFailure vf = new ValidationFailure(message);
                        vf.setConstraintReference(2, "cvc-assertions-valid", "1");
                        return vf;
                    }
                } catch (XPathException e) {
                    ValidationFailure vf = new ValidationFailure(
                            "List " + Err.wrap(value) + " cannot be compared against the assertion facet " +
                                    Err.wrap(f.getValue()) + ": " + e.getMessage());
                    vf.setConstraintReference(2, "cvc-assertions-valid", "1");
                    return vf;
                }
            }
            if (!f.testLength(count)) {
                ValidationFailure vf = new ValidationFailure(
                        "Length of list " + Err.wrap(count + "") + " contravenes the " +
                                f.getName() + " facet " + Err.wrap(f.getValue()));
                vf.setConstraintReference(2, f.getConstraintName(), "1");
                return vf;
            }
        }
        return null;
    }

    /**
     * Get the typed value of a given input string. This method assumes that the input value
     * is valid according to this SimpleType
     *
     * @param value    the string whose typed value is required
     * @param resolver namespace resolver
     * @param rules    the conversion rules used in this configuration
     * @return the typed value as an iterator
     * @throws ValidationException if the input is invalid against this type
     */

    @Override
    public AtomicSequence getTypedValue(UnicodeString value, NamespaceResolver resolver, ConversionRules rules)
            throws ValidationException {
        if (value.isEmpty()) {
            return EmptyAtomicSequence.getInstance();
        }
        AtomicIterator iter = tokenizerFactory.apply(value);
        SimpleType theItemType = getItemType();
        try {
            return new AtomicArray(MappingIterator.map(iter,
                item -> theItemType.getTypedValue(item.getUnicodeStringValue(), resolver, rules).iterate()));
        } catch (XPathException err) {
            // should not occur
            throw new ValidationException(err);
        }
    }

    /**
     * Test whether this simple type is namespace-sensitive, that is, whether
     * the item types is derived from xs:QName or xs:NOTATION
     *
     * @return true if the item type is derived from xs:QName or xs:NOTATION; or if the item type
     * cannot be determined (because it is missing from the schema)
     */

    @Override
    public boolean isNamespaceSensitive() {
        try {
            return getItemType().isNamespaceSensitive();
        } catch (MissingComponentException e) {
            return true;
        }
    }

    /**
     * Ask whether this type only allows zero-length content
     *
     * @return true if this is a list type with a length or maxLength of zero, or an atomic type
     * that restricts the content to be empty
     */
    @Override
    public boolean isZeroLength() {
        Facet lengthFacet = getFacet(LengthFacet.class);
        if (lengthFacet != null && lengthFacet.toLong() == 0) {
            return true;
        }
        Facet maxLengthFacet = getFacet(MaxLengthFacet.class);
        return maxLengthFacet != null && maxLengthFacet.toLong() == 0;
    }

    /**
     * Serialize the schema component
     */

    @Override
    protected void serializeVariety(SchemaModelSerializer serializer) throws XPathException {
        serializer.emitAttribute("variety", "list");
        serializer.emitAttribute("itemType", serializer.getTypeLink(getItemType()));
    }


    /**
     * Produce a string representation of the type name. If the type is anonymous, an internally-allocated
     * type name will be returned.
     *
     * @return the name of the atomic type in the form Q{uri}local
     */

    @Override
    public String toString() {
        return getTypeName().getEQName();
    }

}

// Copyright (c) 2004-2023 Saxonica Limited
