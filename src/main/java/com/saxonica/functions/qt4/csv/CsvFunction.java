package com.saxonica.functions.qt4.csv;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.OptionsParameter;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.om.EmptyAtomicSequence;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.value.BooleanValue;
import net.sf.saxon.value.SequenceType;
import net.sf.saxon.value.StringValue;

/**
 * This abstract class provides shared option-handling support used by the functions <code>fn:parse-csv()</code>,
 * <code>fn:csv-to-xdm()</code>, and <code>fn:csv-to-xml()</code>.
 */
public abstract class CsvFunction extends SystemFunction {
    /**
     * Provides an OptionsParameter with all the options used by every csv-* function.
     *
     * @return an OptionsParameter instance with the core options already added.
     */
    public static OptionsParameter coreOptions() {
        OptionsParameter opts = new OptionsParameter(40);
        opts.addAllowedOption("trim-whitespace", SequenceType.SINGLE_BOOLEAN, BooleanValue.FALSE);
        opts.addAllowedOption("quote-character", SequenceType.SINGLE_STRING, new StringValue("\""));
        opts.addAllowedOption("row-delimiter", SequenceType.SINGLE_STRING, new StringValue("\n"));
        opts.addAllowedOption("field-delimiter", SequenceType.SINGLE_STRING, new StringValue(","));
        return opts;
    }

//    static SequenceType NUMBER_OF_COLUMNS_TYPE;
//
//    static {
//        Set<String> values = new HashSet<>(2);
//        values.add("first-row");
//        values.add("all");
//        ItemType numberOfColumnsItemType = new LocalUnionType(new EnumerationType(values), BuiltInAtomicType.INTEGER);
//        NUMBER_OF_COLUMNS_TYPE = SequenceType.makeSequenceType(numberOfColumnsItemType, StaticProperty.EXACTLY_ONE);
//    }

    public static OptionsParameter extendedOptions() {
        OptionsParameter opts = coreOptions();
        opts.addAllowedOption("header", SequenceType.ATOMIC_SEQUENCE, BooleanValue.FALSE);
        opts.addAllowedOption("select-columns", BuiltInAtomicType.POSITIVE_INTEGER.oneOrMore(), EmptyAtomicSequence.getInstance());
        opts.addAllowedOption("trim-rows", SequenceType.SINGLE_BOOLEAN, BooleanValue.FALSE);
        return opts;
    }

    @Override
    abstract public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException;
}
