package com.saxonica.functions.qt4.csv;

import java.util.List;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.OptionsParameter;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.value.BooleanValue;
import net.sf.saxon.value.SequenceExtent;
import net.sf.saxon.value.SequenceType;
import net.sf.saxon.value.StringValue;

import java.util.stream.Collectors;
import java.util.stream.Stream;

public abstract class CsvFunction extends SystemFunction {
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

    @Override
    abstract public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException;
}
