package com.saxonica.functions.extfn;

import net.sf.saxon.Configuration;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.sort.GenericAtomicComparer;
import net.sf.saxon.functions.DeepEqual;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.lib.Logger;
import net.sf.saxon.lib.StringCollator;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.query.QueryResult;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.BooleanValue;

import javax.xml.transform.OutputKeys;
import java.util.Properties;

/**
 * Implements the saxon:deep-equal() function, a variant of fn:deep-equal that provides
 * additional control over how the comparison is performed.
 * <p>The specification is as follows:</p>
 * <p>arg1      The first sequence to be compared</p>
 * <p>arg2      The second sequence to be compared</p>
 * <p>collation The collation to be used (null if the default collation is to be used)</p>
 * <p>flags     A string whose characters select options that cause the comparison to vary from the
 * standard fn:deep-equals() function. The flags are:</p>
 * <ul>
 * <li>N - take namespace nodes into account</li>
 * <li>J - join adjacent text nodes (e.g, nodes either side of a comment)
 * <li>A - compare type annotations</li>
 * <li>C - take comments into account</li>
 * <li>F - take namespace prefixes into account</li>
 * <li>I - take the is-id() and is-idref() properties into account</li>
 * <li>P - take processing instructions into account</li>
 * <li>S - compare string values, not typed values</li>
 * <li>w - don't take whitespace-only text nodes into account</li>
 * </ul>
 * <p>returns true if the sequences are deep equal, otherwise false</p>
 */

public class SaxonDeepEqual extends SystemFunction {


    /**
     * Evaluate this function call at run-time
     *
     * @param context   The XPath dynamic evaluation context
     * @param arguments The values of the arguments to the function call. Each argument value (which is in general
     *                  a sequence) is supplied in the form of a sequence.
     * @return the results of the function.
     * @throws net.sf.saxon.trans.XPathException if a dynamic error occurs during evaluation of the function. The Saxon run-time
     *                                           code will add information about the error location.
     */

    @Override
    public BooleanValue call(XPathContext context, Sequence[] arguments) throws XPathException {
        String flags = arguments[3].head().getStringValue();
        if (flags.indexOf('!') >= 0) {
            // undocumented diagnostic option
            Logger err = context.getConfiguration().getLogger();
            Properties indent = new Properties();
            indent.setProperty(OutputKeys.METHOD, "xml");
            indent.setProperty(OutputKeys.INDENT, "yes");
            err.info("DeepEqual: first argument:");
            QueryResult.serialize(QueryResult.wrap(arguments[0].iterate(), context.getConfiguration()),
                                  err.asStreamResult(), indent);
            err.info("DeepEqual: second argument:");
            QueryResult.serialize(QueryResult.wrap(arguments[1].iterate(), context.getConfiguration()),
                                  err.asStreamResult(), indent);
        }
        Item collationValue = arguments[2].head();
        Configuration config = context.getConfiguration();
        String collation = collationValue == null ? config.getDefaultCollationName() : collationValue.getStringValue();
        StringCollator collator = config.getCollation(collation);
        if (collator == null) {
            XPathException e = new XPathException("Unknown collation " + collation);
            e.setErrorCode("FOCH0002");
            throw e;
        }
        GenericAtomicComparer comparer = new GenericAtomicComparer(collator, context);
        int flag = 0;
        if (flags.contains("N")) {
            flag |= DeepEqual.INCLUDE_NAMESPACES;
        }
        if (flags.contains("J")) {
            flag |= DeepEqual.JOIN_ADJACENT_TEXT_NODES;
        }
        if (flags.contains("C")) {
            flag |= DeepEqual.INCLUDE_COMMENTS;
        }
        if (flags.contains("P")) {
            flag |= DeepEqual.INCLUDE_PROCESSING_INSTRUCTIONS;
        }
        if (flags.contains("F")) {
            flag |= DeepEqual.INCLUDE_PREFIXES;
        }
        if (flags.contains("S")) {
            flag |= DeepEqual.COMPARE_STRING_VALUES;
        }
        if (flags.contains("A")) {
            flag |= DeepEqual.COMPARE_ANNOTATIONS;
        }
        if (flags.contains("I")) {
            flag |= DeepEqual.COMPARE_ID_FLAGS;
        }
        if (flags.contains("v")) {
            flag |= DeepEqual.EXCLUDE_VARIETY;
        }
        if (flags.contains("w")) {
            flag |= DeepEqual.EXCLUDE_WHITESPACE_TEXT_NODES;
        }
        if (flags.contains("?")) {
            flag |= DeepEqual.WARNING_IF_FALSE;
        }
        boolean result = DeepEqual.deepEqual(
                arguments[0].iterate(), arguments[1].iterate(), comparer, context, flag);
        return BooleanValue.get(result);

    }


}

// Copyright (c) 2009-2022 Saxonica Limited

