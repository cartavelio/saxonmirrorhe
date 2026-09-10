package com.saxonica.ee.stream.feed;

import com.saxonica.ee.stream.watch.Terminator;
import com.saxonica.ee.stream.watch.WatchManager;
import net.sf.saxon.event.Outputter;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.GeneralComparison;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.sort.AtomicMatchKey;
import net.sf.saxon.lib.ConversionRules;
import net.sf.saxon.lib.StringCollator;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.om.StandardNames;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.*;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.BooleanValue;
import net.sf.saxon.value.NumericValue;
import net.sf.saxon.value.StringValue;
import net.sf.saxon.z.IntHashMap;
import net.sf.saxon.z.IntIterator;

import java.util.ArrayList;
import java.util.HashSet;
import java.util.List;
import java.util.Set;

/**
 * This class is used when a streaming template requests a general comparison with one operand
 * being pushed from the streamed input document, and the other being evaluated in pull mode.
 */


public class GeneralComparisonEqualityFeed extends AtomicItemFeed {

    private WatchManager watchManager;
    private final StringCollator collator;
    // A set of lookup tables, one for each primitive atomic type that has been encountered
    private IntHashMap<Set<AtomicMatchKey>> lookupTable;
    // All untyped atomic values that have been encountered
    private List<StringValue> untypedValues;
    private boolean done = false;
    private final Expression pullOperand;
    private final int implicitTimezone;
    private boolean closed;

    public GeneralComparisonEqualityFeed(WatchManager watchManager, Expression expr, int pushedArg, ItemFeed result, XPathContext context) {
        super(expr, result, context);
        this.watchManager = watchManager;
        final GeneralComparison gc = (GeneralComparison) getExpression();
        collator = gc.getStringCollator();
        pullOperand = pushedArg==0 ? gc.getRhsExpression() : gc.getLhsExpression();
        implicitTimezone = context.getImplicitTimezone();
    }

    @Override
    public void open(Terminator terminator) throws XPathException {
        super.open(terminator);
        done = false;
        lookupTable = null;
    }

    /**
     * Supply an item to the feed
     *
     * @param item the item being fed (in this case, an item in the "pushed" operand
     *             sequence that is being compared
     * @throws net.sf.saxon.trans.XPathException
     *          to report any failure
     */

    @Override
    public void append(Item item) throws XPathException {
        if (!done) {
            // If we haven't already reported a result of true
            XPathContext context = getContext();
            ConversionRules rules = context.getConfiguration().getConversionRules();
            AtomicValue next = (AtomicValue) item;
            AtomicMatchKey k1 = next.getXPathMatchKey(collator, implicitTimezone);
            BuiltInAtomicType matchType;
            if (next instanceof NumericValue) {
                matchType = BuiltInAtomicType.DOUBLE;
            } else {
                matchType = ((AtomicValue) item).getPrimitiveType();
            }
            if (lookupTable == null) {
                lookupTable = new IntHashMap<>(4);
                untypedValues = new ArrayList<>(4);
                Set<AtomicMatchKey> strings = new HashSet<>(10);
                lookupTable.put(StandardNames.XS_UNTYPED_ATOMIC, strings);
                Set<AtomicMatchKey> typed = null;
                if (!(next.isUntypedAtomic())) {
                    typed = new HashSet<>(10);
                    lookupTable.put(matchType.getFingerprint(), typed);
                }
                SequenceIterator comparand = pullOperand.iterate(context);
                AtomicValue val;
                while ((val = (AtomicValue) comparand.next()) != null) {
                    if (val.isUntypedAtomic()) {
                        untypedValues.add((StringValue)val);
                        strings.add(((StringValue)val).getXPathMatchKey(collator, implicitTimezone));
                        if (typed != null) {
                            AtomicValue cval = Converter.convert(val, matchType, rules);
                            typed.add(cval.getXPathMatchKey(collator, implicitTimezone));
                            if (matchType != BuiltInAtomicType.UNTYPED_ATOMIC) {
                                strings.add(val.getXPathMatchKey(collator, implicitTimezone));
                            }
                        }
                    } else {
                        BuiltInAtomicType prim;
                        if (val instanceof NumericValue) {
                            prim = BuiltInAtomicType.DOUBLE;
                        } else {
                            prim = val.getPrimitiveType();
                        }
                        Set<AtomicMatchKey> keys = lookupTable.get(prim.getFingerprint());
                        if (keys == null) {
                            keys = new HashSet<>(10);
                            lookupTable.put(prim.getFingerprint(), keys);
                        }
                        keys.add(val.getXPathMatchKey(collator, implicitTimezone));
                    }
                }
                for (StringValue untypedVal : untypedValues) {
                    IntIterator types = lookupTable.keyIterator();
                    while (types.hasNext()) {
                        int type = types.next();
                        Set<AtomicMatchKey> keys = lookupTable.get(type);
                        BuiltInAtomicType atomicType = (BuiltInAtomicType)BuiltInType.getSchemaType(type);
                        keys.add(Converter.convert(untypedVal, atomicType, rules)
                                .getXPathMatchKey(collator, implicitTimezone));
                    }
                }
                if (lookupTable.size() == 0 && untypedValues.isEmpty()) {
                    Outputter out = getNextOutputter();
                    done = true;
                    out.append(BooleanValue.FALSE);
                    lookupTable = null;
                    if (watchManager.allowsEarlyExit()) {
                        closed = true;
                        out.close();
                        getTerminator().terminate();
                    }
                }
            } else {
                Set<AtomicMatchKey> typed = lookupTable.get(matchType.getFingerprint());
                if (typed == null) {
                    // This is the first value of this type found in the stream; convert all the untyped
                    // values from the other operand
                    typed = new HashSet<>(10);
                    lookupTable.put(matchType.getFingerprint(), typed);
                    for (StringValue val : untypedValues) {
                        AtomicValue cval = Converter.convert(val, matchType, rules);
                        typed.add(cval.getXPathMatchKey(collator, implicitTimezone));
                    }
                }
            }
            Set<AtomicMatchKey> lookup = lookupTable.get(matchType.getFingerprint());
            if (next.isUntypedAtomic()) {
                IntIterator types = lookupTable.keyIterator();
                while (types.hasNext() && !done) {
                    int t = types.next();
                    AtomicValue converted = Converter.convert(next, (AtomicType)BuiltInType.getSchemaType(t), rules);
                    lookup = lookupTable.get(t);
                    if (lookup != null) {
                        testValue(lookup, converted.getXPathMatchKey(collator, implicitTimezone));
                    }
                }
            } else {
                IntIterator types = lookupTable.keyIterator();
                while (types.hasNext() && !done) {
                    int t = types.next();
                    if (t != StandardNames.XS_UNTYPED_ATOMIC) {
                        BuiltInAtomicType u = (BuiltInAtomicType)BuiltInType.getSchemaType(t);
                        if (matchType != u && !Type.isGuaranteedComparable(matchType, u, false)) {
                            throw new XPathException("Comparison between " + matchType.getDisplayName() +
                                " and " + u.getDisplayName() + " is not allowed", "XPTY0004", getExpression().getLocation());
                        }
                    }
                }
                testValue(lookup, k1);
            }
        }
    }

    private void testValue(Set<AtomicMatchKey> lookup, AtomicMatchKey k) throws XPathException {
        Outputter out = getNextOutputter();
        if (lookup.contains(k)) {
            //System.err.println("Matched on " + k);
            done = true;
            out.append(BooleanValue.TRUE);
            lookupTable = null;
            if (watchManager.allowsEarlyExit()) {
                closed = true;
                out.close();
                getTerminator().terminate();
            }
        }
    }

    @Override
    public void close() throws XPathException {
        if (!done) {
            // If we've come to the end without finding an item that meets the conditions, report the result false
            getNextOutputter().append(BooleanValue.FALSE);
        }
        if (!closed) {
            super.close();
            lookupTable = null;
        }

    }
}

// Copyright (c) 2010-2023 Saxonica Limited