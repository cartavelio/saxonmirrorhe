////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2020 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.functions;

import net.sf.saxon.expr.Callable;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.ma.map.RecordType;
import net.sf.saxon.ma.map.Shape;
import net.sf.saxon.ma.map.ShapedMap;
import net.sf.saxon.om.*;
import net.sf.saxon.regex.ARegularExpression;
import net.sf.saxon.regex.RegularExpression;
import net.sf.saxon.str.Twine8;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.UncheckedXPathException;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.ChoiceItemType;
import net.sf.saxon.type.ItemType;
import net.sf.saxon.type.SpecificFunctionType;
import net.sf.saxon.value.BooleanValue;
import net.sf.saxon.value.SequenceExtent;
import net.sf.saxon.value.SequenceType;
import net.sf.saxon.value.StringValue;

import java.util.ArrayList;
import java.util.List;
import java.util.function.BiFunction;

import static net.sf.saxon.value.SequenceType.SINGLE_STRING;

/**
 * This class implements the function fn:regex(), which is a standard function in XPath 4.0
 */
public class RegexFn extends SystemFunction implements Callable {

    public static SpecificFunctionType MATCHES_TYPE =
            new SpecificFunctionType(SINGLE_STRING, SequenceType.SINGLE_BOOLEAN);

    public static SpecificFunctionType TOKENIZE_TYPE =
            new SpecificFunctionType(SINGLE_STRING, SequenceType.STRING_SEQUENCE);

    public static SpecificFunctionType REPLACE_TYPE =
            new SpecificFunctionType(SINGLE_STRING,
                                     SequenceType.optional(
                                             ChoiceItemType.of(BuiltInAtomicType.STRING,
                                                               new SpecificFunctionType(SequenceType.SINGLE_UNTYPED_ATOMIC,
                                                                                SequenceType.UNTYPED_ATOMIC_SEQUENCE,
                                                                                SequenceType.OPTIONAL_ITEM))),
                                     SINGLE_STRING);

    public static SpecificFunctionType ANALYZE_STRING_TYPE =
            new SpecificFunctionType(SINGLE_STRING, SINGLE_STRING);

    public static SpecificFunctionType MATCHING_SEGMENTS_TYPE =
            new SpecificFunctionType(SINGLE_STRING, SequenceType.SINGLE_BOOLEAN);

    public static ItemType RETURN_TYPE = RecordType.extensible(
            new RecordType.Field("pattern", SINGLE_STRING, false),
            new RecordType.Field("flags", SINGLE_STRING, false),
            new RecordType.Field("matches", SequenceType.one(MATCHES_TYPE), false),
            new RecordType.Field("tokenize", SequenceType.one(TOKENIZE_TYPE), false),
            new RecordType.Field("replace", SequenceType.one(REPLACE_TYPE), false),
            new RecordType.Field("analyze-string", SequenceType.one(ANALYZE_STRING_TYPE), false),
            new RecordType.Field("matching-segments", SequenceType.one(MATCHING_SEGMENTS_TYPE), false));



    private static final Shape SHAPE = new Shape(new Twine8("pattern"),
                                                 new Twine8("flags"),
                                                 new Twine8("matches"),
                                                 new Twine8("tokenize"),
                                                 new Twine8("replace"),
                                                 new Twine8("analyze-string"),
                                                 new Twine8("matching-segments"));


    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        UnicodeString pattern = arguments[0].head().getUnicodeStringValue();
        String flags = "";
        if (arguments.length > 1) {
            StringValue flagsVal = (StringValue) arguments[1].head();
            if (flagsVal != null) {
                flags = arguments[1].head().getStringValue();
            }
        }
        List<String> warnings = new ArrayList<>();
        RegularExpression rex = new ARegularExpression(pattern, flags, "XP40",warnings, context.getConfiguration());
        CallableFunction matches = new CallableFunction(
                (cxt, args) -> BooleanValue.get(rex.containsMatch(args[0].head().getUnicodeStringValue())),
                MATCHES_TYPE);
        CallableFunction tokenize = new CallableFunction(
                (cxt, args) -> SequenceExtent.from(rex.tokenize(args[0].head().getUnicodeStringValue())),
                TOKENIZE_TYPE);
        CallableFunction replace = new CallableFunction(
                (cxt, args) -> {
                    UnicodeString in = args[0].head().getUnicodeStringValue();
                    Item rep = args[1].head();
                    if (rep == null) {
                        rep = StringValue.EMPTY_STRING;
                    }
                    if (rep instanceof StringValue) {
                        return new StringValue(rex.replace(in, rep.getUnicodeStringValue()));
                    } else {
                        FunctionItem callback = ((FunctionItem) rep);
                        BiFunction<UnicodeString, UnicodeString[], UnicodeString> lambda = (m, groups) -> {
                            List<Item> list = new ArrayList<>();
                            for (UnicodeString group : groups) {
                                list.add(StringValue.makeUntypedAtomic(group));
                            }
                            GroundedValue groupValues = SequenceExtent.makeSequenceExtent(list);
                            try {
                                return ((StringValue)callback.call(cxt,
                                                                   new Sequence[]{
                                                                           StringValue.makeUntypedAtomic(m),
                                                                           groupValues})).getUnicodeStringValue();
                            } catch (XPathException e) {
                                throw new UncheckedXPathException(e);
                            }
                        };
                        UnicodeString out = rex.replaceWith(in, lambda);
                        return new StringValue(out);
                    }
                },
                REPLACE_TYPE);
        CallableFunction analyzeString = new CallableFunction(
                (cxt, args) -> {
                    AnalyzeStringFn asfn = new AnalyzeStringFn();
                    asfn.setRetainedStaticContext(getRetainedStaticContext());
                    return asfn.buildResult(cxt, rex.analyze(args[0].head().getUnicodeStringValue()));
                },
                ANALYZE_STRING_TYPE);
        CallableFunction matchingSegments = new CallableFunction(
                (cxt, args) -> BooleanValue.get(rex.matches(args[0].head().getUnicodeStringValue())),
                MATCHING_SEGMENTS_TYPE);
        return new ShapedMap(SHAPE,
                             new StringValue(pattern),
                             new StringValue(flags),
                             matches,
                             tokenize,
                             replace,
                             analyzeString,
                             matchingSegments);
                             
    }


}

// Copyright (c) 2018-2026 Saxonica Limited
