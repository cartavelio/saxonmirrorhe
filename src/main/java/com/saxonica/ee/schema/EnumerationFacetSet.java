package com.saxonica.ee.schema;

import net.sf.saxon.expr.CallableDelegate;
import net.sf.saxon.expr.sort.CustomMap;
import net.sf.saxon.expr.sort.SimpleTypeIdentityComparison;
import net.sf.saxon.functions.CallableFunction;
import net.sf.saxon.ma.arrays.SimpleArrayItem;
import net.sf.saxon.om.*;
import net.sf.saxon.str.UnicodeBuilder;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.SimpleType;
import net.sf.saxon.type.SpecificFunctionType;
import net.sf.saxon.value.*;

import java.util.ArrayList;
import java.util.List;

/**
 * Represents the set of enumeration values defined on a single simple type, This is a "virtual" facet
 * that combines all the enumeration facets in one simple type definition.
 */
public class EnumerationFacetSet extends Facet {

    /**
     * The set of allowed values, held in a map where values are compared
     * using the XSD-defined "equal or identical" comparison semantics.
     */

    private final CustomMap<AtomicSequence, AtomicSequence> typedValues =
            new CustomMap<>(SimpleTypeIdentityComparison.getInstance());

    /**
     * The allowed values, as strings
     */

    private final List<String> stringValues = new ArrayList<>(20);

    /**
     * The name of this facet.
     *
     * @return "enumeration"
     */
    @Override
    public String getName() {
        return "enumeration";
    }

    /**
     * Get the enumeration values, as strings
     * @return the list of strings in the enumeration facets
     */

    public List<String> getStringValues() {
        return stringValues;
    }

    /**
     * Add a permitted value to the set of enumeration values.
     * For internal use only.
     *
     * @param val         the value to be added. This must be an atomic value or a sequence of atomic values.
     * @param stringValue the string value of the facet as written
     */

    public void addEnumerationValue(AtomicSequence val, String stringValue) {
        typedValues.put(val, val);
        stringValues.add(stringValue);

    }

    /**
     * Get a string representation of the value of the facet
     * @return the concatenation of the enumeration values, comma-separated
     */

    @Override
    public UnicodeString getValue() {
        UnicodeBuilder sb = new UnicodeBuilder();
        boolean first = true;
        for (String s : stringValues) {
            if (first) {
                first = false;
            } else {
                sb.appendLatin(", ");
            }
            sb.append(s);
        }
        return sb.toUnicodeString();
    }

    /**
     * Check that this facet is legal when used on a type derived by restriction
     *
     * @param base     the type from which the restricted type is derived
     * @param compiler the schema compiler
     */

    @Override
    public void checkFacetRestriction(UserSimpleType type, SimpleType base, SchemaCompiler compiler) {
        //
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
        return typedValues.containsKey(value);
    }

    /**
     * Test whether a list value conforms to this facet. This is used only for the enumeration
     * facet.
     *
     * @param value the value to be tested
     * @return true if the value conforms; false if it doesn't conform, or if the test fails
     */

    @Override
    public boolean testListValue(AtomicSequence value) {
        return typedValues.containsKey(value);
    }

    /**
     * Get an instance of the facet value that is stored in the schema rather than the instance
     *
     * @param value the value required
     * @return a string which is equal to the value required, but which if possible is the instance
     *         held in the compiled schema, reducing storage requirements in instances.
     */

    public String getSharedInstance(String value) {
        for (String s : stringValues) {
            if (s.equals(value)) {
                return s;
            }
        }
        return value;
    }

    /**
     * Get an instance of the facet value that is stored in the schema rather than the instance
     *
     * @param value the value required
     * @return a string which is equal to the value required, but which if possible is the instance
     *         held in the compiled schema, reducing storage requirements in instances.
     */

    public AtomicSequence getSharedInstance(AtomicSequence value) {
        return typedValues.get(value);
    }


    /**
     * Serialize this facet
     */

    @Override
    public void serializeFacet(SchemaModelSerializer serializer) throws XPathException {
        for (AtomicSequence v : typedValues.values()) {
            serializer.startElement("enumeration");
            boolean namespaceSensitive = false;
            for (AtomicValue av : v) {
                if (av instanceof QualifiedNameValue) {
                    String prefix = ((QualifiedNameValue) av).getPrefix();
                    NamespaceUri uri = ((QualifiedNameValue) av).getNamespaceURI();
                    serializer.declareNamespace(prefix, uri);
                    namespaceSensitive = true;
                }
            }
            serializer.emitAttribute("value", v.getUnicodeStringValue().toString());
            if (namespaceSensitive) {
                serializer.emitAttribute("namespaceSensitive", "true");
            }
            serializer.endElement();
        }
    }

    @Override
    public Function getFacetAsFunctionItem() {
        EnumerationFacetSet facet = this;
        CallableDelegate callable = new CallableDelegate((context, arguments) -> {
            String key = arguments[0].head().getStringValue();
            switch (key) {
                case "class":
                    return new StringValue(getName());
                case "implementation":
                    return new ObjectValue<>(facet);
                case "value":
                    // This is a set of typed values, where each typed value is potentially a list.
                    // We therefore return the string values (bug 4024)
                    List<AtomicValue> list = new ArrayList<>();
                    for (String s : stringValues) {
                        list.add(new StringValue(s));
                    }
                    return new AtomicArray(list);
                case "typed-value":
                    // Introduced by bug 4024
                    List<GroundedValue> members = new ArrayList<>(typedValues.values());
                    return new SimpleArrayItem(members);
                default:
                    return EmptySequence.getInstance();
            }
        });
        return new CallableFunction(1, callable, SpecificFunctionType.COMPONENT_FUNCTION_TYPE);
    }


}

// Copyright (c) 2004-2022 Saxonica Limited
