////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2020 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.functions.registry;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.ResolveURI;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.ma.map.MapType;
import net.sf.saxon.om.Function;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.pattern.NodeKindTest;
import net.sf.saxon.trans.SymbolicName;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.AnyItemType;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.SpecificFunctionType;
import net.sf.saxon.value.SequenceType;
import net.sf.saxon.value.StringItem;

import java.net.URISyntaxException;

/**
 * Function signatures (and pointers to implementations) of the functions defined in XPath 3.1 without the
 * Higher-Order-Functions feature
 */

public class XPath40FunctionSet extends BuiltInFunctionSet {

    private static final XPath40FunctionSet THE_INSTANCE = new XPath40FunctionSet();

    public static XPath40FunctionSet getInstance() {
        return THE_INSTANCE;
    }

    private XPath40FunctionSet() {
        init();
    }

    private void init() {

        importFunctionSet(XPath31FunctionSet.getInstance());

        SpecificFunctionType predicate = new SpecificFunctionType(
                new SequenceType[]{SequenceType.SINGLE_ITEM},
                SequenceType.SINGLE_BOOLEAN);

        register("all", 2, SefFunction.class, BuiltInAtomicType.BOOLEAN, ONE, AS_ARG0 | LATE)
                .arg(0, AnyItemType.getInstance(), STAR, EMPTY)
                .arg(1, predicate, ONE, null);

        register("highest", 1, Highest.class, AnyItemType.getInstance(), STAR, LATE)
                .arg(0, AnyItemType.getInstance(), STAR, null);

        register("highest", 2, Highest.class, AnyItemType.getInstance(), STAR, LATE)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, BuiltInAtomicType.STRING, OPT, null);

        SpecificFunctionType ft = new SpecificFunctionType(
                new SequenceType[]{SequenceType.SINGLE_ITEM},
                SequenceType.ATOMIC_SEQUENCE);

        register("highest", 3, Highest.class, AnyItemType.getInstance(), STAR, LATE)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, BuiltInAtomicType.STRING, OPT, null)
                .arg(2, ft, ONE, null);


        register("in-scope-namespaces", 1, SefFunction.class, MapType.ANY_MAP_TYPE, ONE, LATE)
                .arg(0, NodeKindTest.ELEMENT, STAR, null);

        register("is-NaN", 1, SefFunction.class, BuiltInAtomicType.BOOLEAN, ONE, LATE)
                .arg(0, BuiltInAtomicType.ANY_ATOMIC, ONE, null);

        register("items-before", 2, SefFunction.class, AnyItemType.getInstance(), STAR, LATE)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, predicate, ONE, null);

        register("items-until", 2, SefFunction.class, AnyItemType.getInstance(), STAR, LATE)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, predicate, ONE, null);

        register("items-from", 2, SefFunction.class, AnyItemType.getInstance(), STAR, LATE)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, predicate, ONE, null);

        register("items-after", 2, SefFunction.class, AnyItemType.getInstance(), STAR, LATE)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, predicate, ONE, null);

        register("lowest", 1, SefFunction.class, AnyItemType.getInstance(), STAR, LATE)
                .arg(0, AnyItemType.getInstance(), STAR, null);

        register("lowest", 2, SefFunction.class, AnyItemType.getInstance(), STAR, LATE)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, BuiltInAtomicType.STRING, OPT, null);
        
        register("lowest", 3, SefFunction.class, AnyItemType.getInstance(), STAR, LATE)
                .arg(0, AnyItemType.getInstance(), STAR, null)
                .arg(1, BuiltInAtomicType.STRING, OPT, null)
                .arg(2, ft, ONE, null);

        register("some", 2, SefFunction.class, BuiltInAtomicType.BOOLEAN, ONE, AS_ARG0 | LATE)
                .arg(0, AnyItemType.getInstance(), STAR, EMPTY)
                .arg(1, predicate, ONE, null);
    }

    public static class Highest extends SefFunction {

        public Highest(){
            super();
        };

        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            Sequence input = arguments[0];
            String collationUri = null;
            if (arguments.length >= 2) {
                StringItem val = (StringItem)arguments[1].head();
                if (val != null) {
                    try {
                        collationUri = ResolveURI.makeAbsolute(val.getStringValue(), getStaticBaseUriString()).toString();
                    } catch (URISyntaxException e) {
                        throw new XPathException(e);
                    }
                }
            }
            if (collationUri == null) {
                collationUri = getRetainedStaticContext().getDefaultCollationName();
            }
            Function key;
            if (arguments.length >= 3) {
                key = (Function) arguments[2].head();
            } else {
                key = SystemFunction.makeFunction("data", getRetainedStaticContext(), 1);
            }
            SymbolicName.F delegate =
                    new SymbolicName.F(new StructuredQName("", "http://ns.saxonica.com/xpath-functions", "mhk_highest"), 3);

            Sequence[] args = new Sequence[3];
            args[0] = input.materialize();
            args[1] = new StringItem(collationUri);
            args[2] = key;

            return callFunction(context, delegate, args);
        }
    }

}
