package com.saxonica.functions.qt4.csv;

import java.util.HashSet;
import java.util.List;

import net.sf.saxon.expr.StaticProperty;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.OptionsParameter;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.om.EmptyAtomicSequence;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.EnumerationType;
import net.sf.saxon.type.ItemType;
import net.sf.saxon.type.LocalUnionType;
import net.sf.saxon.value.BooleanValue;
import net.sf.saxon.value.SequenceExtent;
import net.sf.saxon.value.SequenceType;
import net.sf.saxon.value.StringValue;

import java.util.Set;
import java.util.stream.Collectors;
import java.util.stream.Stream;

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
    static OptionsParameter coreOptions() {
        OptionsParameter opts = new OptionsParameter();
        List<StringValue> defaultRowDelimiter = Stream.of("\r\n", "\n", "\r").map(StringValue::new).collect(Collectors.toList());
        opts.addAllowedOption("trim-whitespace", SequenceType.SINGLE_BOOLEAN, BooleanValue.FALSE);
        opts.addAllowedOption("quote-character", SequenceType.SINGLE_STRING, new StringValue("\""));
        opts.addAllowedOption("row-delimiter", BuiltInAtomicType.STRING.oneOrMore(),
                SequenceExtent.makeSequenceExtent(defaultRowDelimiter));
        opts.addAllowedOption("field-delimiter", SequenceType.SINGLE_STRING, new StringValue(","));
        return opts;
    }

    static SequenceType NUMBER_OF_COLUMNS_TYPE;

    static {
        Set<String> values = new HashSet<>(2);
        values.add("first-row");
        values.add("all");
        ItemType numberOfColumnsItemType = new LocalUnionType(new EnumerationType(values), BuiltInAtomicType.INTEGER);
        NUMBER_OF_COLUMNS_TYPE = SequenceType.makeSequenceType(numberOfColumnsItemType, StaticProperty.EXACTLY_ONE);
    }

    static OptionsParameter extendedOptions() {
        OptionsParameter opts = coreOptions();
        opts.addAllowedOption("column-names", SequenceType.SINGLE_ITEM, BooleanValue.FALSE);
        opts.addAllowedOption("filter-columns", SequenceType.INTEGER_SEQUENCE, EmptyAtomicSequence.getInstance());
        opts.addAllowedOption("number-of-columns", NUMBER_OF_COLUMNS_TYPE);
        return opts;
    }

    @Override
    abstract public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException;
}
