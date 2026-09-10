package com.saxonica.ee.schema;

import net.sf.saxon.Configuration;
import net.sf.saxon.expr.Callable;
import net.sf.saxon.functions.CallableFunction;
import net.sf.saxon.om.Function;
import net.sf.saxon.regex.RegularExpression;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.SchemaComponent;
import net.sf.saxon.type.SchemaException;
import net.sf.saxon.type.SimpleType;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.EmptySequence;
import net.sf.saxon.value.ObjectValue;
import net.sf.saxon.value.StringValue;

import java.util.List;

/**
 * Represents a pattern facet on a schema-defined simple type
 */
public class PatternFacet extends Facet {

    Configuration config;
    StringValue requiredValue;
    RegularExpression regex;
    String flags = "";
    boolean noMerge;

    /**
     * Create a pattern facet
     *
     * @param config the Saxon configuration
     */

    public PatternFacet(Configuration config) {
        this.config = config;
    }

    public String getName() {
        return "pattern";
    }

    public String getValue() {
        return requiredValue.getStringValue();
    }

    public RegularExpression getRegularExpression() {
        return regex;
    }

    public void setValue(String value) {
        requiredValue = StringValue.makeStringValue(value);
    }

    /**
     * Saxon allows flags for the regular expression (e.g. "i" meaning case-independent) to be set
     * using the saxon:flags attribute
     *
     * @param flags the regular expression flags
     */

    public void setFlags(String flags) {
        this.flags = flags;
    }

    /**
     * Get the flags for the regular expression
     * @return the flags
     */

    public String getFlags() {
        return flags;
    }

    /**
     * Compile the regular expression
     *
     * @param warnings if non-null, receives any warnings that arise from compiling the regular expression
     * @throws SchemaException if the regular expression is invalid
     */

    public void compile(List<String> warnings) throws SchemaException {
        try {
            String lang = config.getXsdVersion() == Configuration.XSD11 ? "XSD11" : "XSD10";
            regex = config.compileRegularExpression(getValue(), flags, lang, warnings);
        } catch (XPathException e) {
            throw new SchemaException(e.getMessage());
        }
    }

    public void setDoNotMerge(boolean dont) {
        noMerge = dont;
    }

    public boolean isDoNotMerge() {
        return noMerge;
    }

    /**
     * Add a branch to a pattern: that is, combine this pattern with another,
     * so that the resulting pattern is the "or" of the two patterns
     *
     * @param pat the new branch, which must be a valid regular expression
     */

    public void addBranch(String pat) {
        try {
            setValue('(' + getValue() + ")|(" + pat + ')');
            compile(null);
        } catch (SchemaException e) {
            throw new AssertionError(e);
        }
    }

    /**
     * Check that this facet is legal when used on a type derived by restriction
     *
     * @param base     the type from which the restricted type is derived
     * @param compiler the schema compiler
     */

    public void checkFacetRestriction(UserSimpleType type, SimpleType base, SchemaCompiler compiler)  {
        // no restrictions
    }

    /**
     * Test whether an atomic value conforms to this facet
     *
     * @param value the value to be tested
     * @return true if the value conforms; false if it doesn't conform, or if the test
     *         fails
     */

    public boolean testAtomicValue(AtomicValue value) {
        return regex.matches(value.getCanonicalLexicalRepresentation());
    }

    @Override
    public Function getFacetAsFunctionItem() {
        // Contrary to the W3C SCM specification, multiple pattern facets are not merged into a single component
        Callable callable = (context, arguments) -> {
            String key = arguments[0].head().getStringValue();
            switch (key) {
                case "class":
                    return new StringValue(getName());
                case "implementation":
                    return new ObjectValue<>(PatternFacet.this);
                case "value":
                    return requiredValue;
                default:
                    return EmptySequence.getInstance();
            }
        };
        return new CallableFunction(1, callable, SchemaComponent.COMPONENT_FUNCTION_TYPE);
    }


}

// Copyright (c) 2004-2017 Saxonica Limited.