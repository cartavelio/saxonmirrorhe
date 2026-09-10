package com.saxonica.ee.schema;

import com.saxonica.ee.validate.CompactStringValue;
import net.sf.saxon.expr.Callable;
import net.sf.saxon.functions.CallableFunction;
import net.sf.saxon.ma.arrays.SimpleArrayItem;
import net.sf.saxon.om.AtomicSequence;
import net.sf.saxon.om.Function;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.util.FastStringBuffer;
import net.sf.saxon.type.SchemaComponent;
import net.sf.saxon.type.SimpleType;
import net.sf.saxon.value.*;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;

/**
 * Represents the set of enumeration values defined on a single simple type, This is a "virtual" facet
 * that combines all the enumeration facets in one simple type definition.
 */
public class EnumerationFacetSet extends Facet {

    /**
     * The set of allowed values, indexed by the result of calling getSchemaComparable()
     */

    private HashMap<Comparable, AtomicSequence> typedValues = new HashMap<>(20);

    /**
     * The allowed values, as strings
     */

    private List<String> stringValues = new ArrayList<>(20);

    /**
     * The name of this facet.
     *
     * @return "enumeration"
     */
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
        if (val instanceof StringValue) {
            val = new CompactStringValue(val.getStringValue());
        }
        typedValues.put(val.getSchemaComparable(), val);
        stringValues.add(stringValue);

    }

    /**
     * Get a string representation of the value of the facet
     */

    public String getValue() {
        FastStringBuffer sb = new FastStringBuffer(FastStringBuffer.C64);
        for (AtomicSequence v : typedValues.values()) {
            sb.append(v.getStringValue());
            sb.append(", ");
        }
        sb.setLength(sb.length() - 2);
        return sb.toString();
    }

    /**
     * Check that this facet is legal when used on a type derived by restriction
     *
     * @param base     the type from which the restricted type is derived
     * @param compiler the schema compiler
     */

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

    public boolean testAtomicValue(AtomicValue value) {
        return typedValues.containsKey(value.getSchemaComparable());
    }

    /**
     * Test whether a list value conforms to this facet. This is used only for the enumeration
     * facet.
     *
     * @param value the value to be tested
     * @return true if the value conforms; false if it doesn't conform, or if the test fails
     */

    public boolean testListValue(AtomicSequence value) {
        return typedValues.containsKey(value.getSchemaComparable());
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
        return typedValues.get(value.getSchemaComparable());
    }


    /**
     * Serialize this facet
     */

    public void serializeFacet(SchemaModelSerializer serializer) throws XPathException {
        for (AtomicSequence v : typedValues.values()) {
            serializer.startElement("enumeration");
            boolean namespaceSensitive = false;
            for (AtomicValue av : v) {
                if (av instanceof QualifiedNameValue) {
                    String prefix = ((QualifiedNameValue) av).getPrefix();
                    String uri = ((QualifiedNameValue) av).getNamespaceURI();
                    serializer.declareNamespace(prefix, uri);
                    namespaceSensitive = true;
                }
            }
            serializer.emitAttribute("value", v.getStringValue());
            if (namespaceSensitive) {
                serializer.emitAttribute("namespaceSensitive", "true");
            }
            serializer.endElement();
        }
    }

    @Override
    public Function getFacetAsFunctionItem() {
        Callable callable = (context, arguments) -> {
            String key = arguments[0].head().getStringValue();
            switch (key) {
                case "class":
                    return new StringValue(getName());
                case "implementation":
                    return new ObjectValue<>(EnumerationFacetSet.this);
                case "value":
                    // This is a set of typed values, where each typed value is potentially a list.
                    // We therefore return the string values (bug 4024)
                    List<StringValue> list = new ArrayList<>();
                    for (String s : stringValues) {
                        list.add(new StringValue(s));
                    }
                    return SequenceExtent.makeSequenceExtent(list);
                case "typed-value":
                    // Introduced by bug 4024
                    List<GroundedValue<?>> members = new ArrayList<>(typedValues.values());
                    return new SimpleArrayItem(members);
                default:
                    return EmptySequence.getInstance();
            }
        };
        return new CallableFunction(1, callable, SchemaComponent.COMPONENT_FUNCTION_TYPE);
    }


}

// Copyright (c) 2004 - 2017 Saxonica Limited.
