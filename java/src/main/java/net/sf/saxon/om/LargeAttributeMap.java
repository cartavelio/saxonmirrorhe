////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.om;


import net.sf.saxon.ma.map.Vavr;
import net.sf.saxon.transpile.CSharpReplaceBody;

import java.util.ArrayList;
import java.util.Iterator;
import java.util.List;

/**
 * An implementation of AttributeMap suitable for larger collections of attributes (say, more than five).
 * This provides direct access to an attribute by name, avoiding the cost of a sequential search. The
 * map preserves the order of insertion of attributes.
 */

public class LargeAttributeMap implements AttributeMap {


    private final io.vavr.collection.LinkedHashMap<NodeName, AttributeInfo> attributes;


    public LargeAttributeMap(List<AttributeInfo> atts) {
        io.vavr.collection.LinkedHashMap<NodeName, AttributeInfo> map = emptyMap();
        for (AttributeInfo att : atts) {
            if (map.containsKey(att.getNodeName())) {
                throw new IllegalArgumentException("Attribute map contains duplicates");
            }
            map = map.put(att.getNodeName(), att);
        }
        this.attributes = map;
    }

    @CSharpReplaceBody(code="return System.Collections.Immutable.ImmutableDictionary<Saxon.Hej.om.NodeName,Saxon.Hej.om.AttributeInfo>.Empty;")
    private static io.vavr.collection.LinkedHashMap<NodeName, AttributeInfo> emptyMap() {
        return io.vavr.collection.LinkedHashMap.empty();
    }


    private LargeAttributeMap(io.vavr.collection.LinkedHashMap<NodeName, AttributeInfo> attributes) {
        this.attributes = attributes;
    }

    /**
     * Return the number of attributes in the map.
     *
     * @return The number of attributes in the map.
     */

    @Override
    public int size() {
        return attributes.size();
    }

    @Override
    public AttributeInfo get(NodeName name) {
        return Vavr.get(attributes, name);
    }

    @Override
    public AttributeInfo get(NamespaceUri uri, String local) {
        NodeName name = new FingerprintedQName("", uri, local);
        return get(name);
    }

    @Override
    public AttributeInfo getByFingerprint(int fingerprint, NamePool namePool) {
        NodeName name = new FingerprintedQName(namePool.getStructuredQName(fingerprint), fingerprint);
        return get(name);
    }

    @Override
    public AttributeMap put(AttributeInfo att) {
        return new LargeAttributeMap(attributes.put(att.getNodeName(), att));
    }

    @Override
    public AttributeMap remove(NodeName name) {
        // Not actually used (or tested)
        if (attributes.containsKey(name)) {
            return new LargeAttributeMap(attributes.remove(name));
        } else {
            return this;
        }
    }

    @Override
    public Iterator<AttributeInfo> iterator() {
        return Vavr.valuesIterator(attributes);
    }

    @Override
    public synchronized ArrayList<AttributeInfo> asList() {
        ArrayList<AttributeInfo> result = new ArrayList<>(size());
        for (AttributeInfo att : this) {
            result.add(att);
        }
        return result;
    }

    @Override
    public String toString() {
        StringBuilder sb = new StringBuilder(256);
        for (AttributeInfo att : this) {
            sb.append(att.getNodeName().getDisplayName()).append("=\"").append(att.getValue()).append("\" ");
        }
        return sb.toString().trim();
    }

}

