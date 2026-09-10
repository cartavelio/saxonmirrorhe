package com.saxonica.xsltextn.pedigree;

import net.sf.saxon.ma.map.KeyValuePair;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.ma.map.MapType;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.tree.iter.AtomicIterator;
import net.sf.saxon.type.PlainType;
import net.sf.saxon.type.TypeHierarchy;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.SequenceType;

/**
 * A <code>DelegatingMapItem</code> is an abstract class representing a map that
 * is implemented by delegating all map-related functionality to another map.
 * Unless any methods are overridden, the functionality is identical to that
 * of the wrapped map item.
 * <p>Note that methods such as {@link #put} that create a new map will
 * (unless overridden) simply delegate to the base map, and the new map will
 * therefore not be a delegating map.</p>
 */

public abstract class DelegatingMapItem extends MapItem {

    private final MapItem base;

    /**
     * Create a {@code DelegatingMapItem} that delegates all map-related access
     * to a specified base map.
     * @param base the map item to which this implementation delegates
     */

    public DelegatingMapItem(MapItem base) {
        this.base = base;
    }

    @Override
    public GroundedValue get(AtomicValue key) {
        return base.get(key);
    }

    @Override
    public int size() {
        return base.size();
    }

    @Override
    public boolean isEmpty() {
        return base.isEmpty();
    }

    @Override
    public AtomicIterator keys() {
        return base.keys();
    }

    @Override
    public Iterable<KeyValuePair> keyValuePairs() {
        return base.keyValuePairs();
    }

    @Override
    public MapItem put(AtomicValue key, GroundedValue value) {
        return base.put(key, value);
    }

    @Override
    public MapItem remove(AtomicValue key) {
        return base.remove(key);
    }

    @Override
    public boolean conforms(PlainType keyType, SequenceType valueType, TypeHierarchy th) {
        return base.conforms(keyType, valueType, th);
    }

    @Override
    public MapType getItemType(TypeHierarchy th) {
        return (MapType)base.getItemType(th);
    }

    public MapItem getBaseItem() {
        return base;
    }

    public String toString() {
        return base.toString();
    }
}

// Copyright (c) 2018-2023 Saxonica Limited

