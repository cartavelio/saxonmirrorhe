package com.saxonica.functions.extfn;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.sort.AtomicMatchKey;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.ma.map.RangeKey;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.*;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.ItemType;
import net.sf.saxon.type.TypeHierarchy;

import java.util.TreeMap;

/**
 * Implementation of the saxon:key-map() extension function. Introduced in 9.5. Views an index, as built
 * using xsl:key, as a map, allowing queries to get the values corresponding to a range of key values,
 * in sorted order.
 */

public class KeyMap extends SystemFunction {

    @Override
    public MapItem call(XPathContext context, Sequence[] arguments) throws XPathException {
        String keyName = arguments[0].head().getStringValue();
        NodeInfo doc = (NodeInfo) arguments[1].head();
        Item minItem = arguments[2].head();
        Item maxItem = arguments[3].head();
        UnicodeString min = minItem == null ? null : minItem.getUnicodeStringValue();
        UnicodeString max = maxItem == null ? null : maxItem.getUnicodeStringValue();
        TypeHierarchy th = context.getConfiguration().getTypeHierarchy();

        StructuredQName keyQName = StructuredQName.fromLexicalQName(
                keyName, false, true, getRetainedStaticContext()
        );
        KeyManager keyManager = getRetainedStaticContext().getPackageData().getKeyManager();
        KeyDefinitionSet kds = keyManager.getKeyDefinitionSet(keyQName);
        if (kds == null) {
            throw new XPathException("Key definition " + keyName + " not found");
        }
        checkKeyDefinition(th, kds, keyName);
        KeyIndex index = keyManager.obtainIndex(kds, doc.getTreeInfo(), context);
        return new RangeKey(
                min, max, (TreeMap<AtomicMatchKey, Object>) index.getUnderlyingMap());
    }


    private static void checkKeyDefinition(TypeHierarchy th, KeyDefinitionSet kds, String keyName) throws XPathException {
        if (kds.getCollationName() != null && !kds.getCollationName().equals(NamespaceConstant.CODEPOINT_COLLATION_URI)) {
            throw new XPathException("Key definition " + keyName + " does not use codepoint collation");
        }
        if (!kds.isRangeKey()) {
            throw new XPathException("Key definition " + keyName + " is not defined as a range key");
        }
        for (KeyDefinition def : kds.getKeyDefinitions()) {
            ItemType it = def.getUse().getItemType();
            if (it != BuiltInAtomicType.UNTYPED_ATOMIC && it != BuiltInAtomicType.STRING &&
                    !th.isSubType(it, BuiltInAtomicType.STRING)) {
                throw new XPathException("Key definition " + keyName + " has non-string key values");
            }
        }
    }
}

// Copyright (c) 2012-2023 Saxonica Limited