package com.saxonica.functions.registry;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.CurrentMergeGroup;
import net.sf.saxon.functions.DocumentFn;
import net.sf.saxon.functions.FunctionAvailable;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.functions.registry.BuiltInFunctionSet;
import net.sf.saxon.functions.registry.XSLT30FunctionSet;
import net.sf.saxon.ma.arrays.ArrayItemType;
import net.sf.saxon.ma.map.MapType;
import net.sf.saxon.ma.map.StringMapBuilder;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.serialize.CharacterMap;
import net.sf.saxon.str.UnicodeChar;
import net.sf.saxon.style.StylesheetPackage;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.AnyItemType;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.Type;
import net.sf.saxon.value.EmptySequence;
import net.sf.saxon.value.QNameValue;
import net.sf.saxon.value.SequenceType;
import net.sf.saxon.value.StringValue;
import net.sf.saxon.z.IntHashMap;
import net.sf.saxon.z.IntIterator;

/**
 * Function signatures (and pointers to implementations) of the functions defined in XSLT 4.0.
 * This includes the functions defined in XPath 4.0 by reference.
 */

public class XSLT40FunctionSet extends XSLT30FunctionSet {

    private static final XSLT40FunctionSet THE_INSTANCE = new XSLT40FunctionSet();

    public static XSLT40FunctionSet getInstance() {
        return THE_INSTANCE;
    }

    @Override
    protected BuiltInFunctionSet correspondingXPathFunctionSet() {
        return XPath40FunctionSet.getInstance();
    }

    private XSLT40FunctionSet() {

        importFunctionSet(correspondingXPathFunctionSet());

        register("character-map", 1, e -> e.populate(CharacterMapFn::new, new MapType(BuiltInAtomicType.STRING, SequenceType.SINGLE_STRING),
                                                     OPT, LATE)
                .arg(0, BuiltInAtomicType.QNAME, ONE, null));

        register("current-merge-group", 1, e -> e.populate(CurrentMergeGroup::new, AnyItemType.getInstance(),
                                                           STAR, LATE)
                .arg(0, BuiltInAtomicType.STRING, OPT, null));

        register("current-merge-key-array", 0, e -> e.populate(net.sf.saxon.functions.CurrentMergeKeyArray::new,
                                                               new ArrayItemType(SequenceType.ATOMIC_SEQUENCE),
                                                               STAR, LATE));

        register("document", 2, e -> e.populate(DocumentFn::new, Type.NODE_TYPE, STAR, BASE | LATE | UO)
                .arg(0, Type.ITEM_TYPE, STAR, null)
                .arg(1, Type.NODE_TYPE, OPT, null));

        register("function-available", 2, e -> e.populate(FunctionAvailable::new, BuiltInAtomicType.BOOLEAN, ONE, NS | LATE)
                .arg(0, BuiltInAtomicType.STRING, ONE, null)
                .arg(1, BuiltInAtomicType.INTEGER, OPT, null));

    }

    public static class CharacterMapFn extends SystemFunction {
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            QNameValue name = (QNameValue)arguments[0].head();
            StylesheetPackage pack = (StylesheetPackage)getRetainedStaticContext().getPackageData();
            CharacterMap charMap = pack.getCharacterMapIndex().getCharacterMap(name.getStructuredQName());
            if (charMap == null) {
                return EmptySequence.getInstance();
            }
            IntHashMap<String> intMap = charMap.getMap();
            StringMapBuilder stringMap = new StringMapBuilder(intMap.size()).unordered();
            IntIterator iter = intMap.keyIterator();
            while (iter.hasNext()) {
                int cp = iter.next();
                stringMap.put(new UnicodeChar(cp), new StringValue(intMap.get(cp)));
            }
            return stringMap.getCompletedMapConfidently();
        }
    }




}

// Copyright (c) 2020-2023 Saxonica Limited.

