package com.saxonica.xsltextn.pedigree;

import net.sf.saxon.ma.arrays.ArrayItem;
import net.sf.saxon.ma.map.KeyValuePair;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.value.AtomicValue;

import java.util.ArrayList;
import java.util.List;

/**
 * A <code>PedigreeMapItem</code> represents an XDM map together with a {@link Pedigree}:
 * that is, retained information about how the map was selected within a JSON-like tree.
 */

public class PedigreeMapItem extends DelegatingMapItem implements PedigreeValue {

    private final Pedigree pedigree;

    /**
     * Create a PedigreeMapItem with an empty pedigree
     * @param base the underlying map item
     */

    public PedigreeMapItem(MapItem base) {
        this(base, Pedigree.PEDIGREE_ROOT);
    }

    /**
     * Create a PedigreeMapItem with a known pedigree
     * @param base the underlying array item
     */

    public PedigreeMapItem(MapItem base, Pedigree pedigree) {
        super(base);
        this.pedigree = pedigree;
    }

    /**
     * Get the value of the entry in the map with a specified key, retaining information
     * about how that value was selected
     *
     * @param key the key of the required entry
     * @return the corresponding value, which will itself have a pedigree if it is
     * a singleton map or array; or null if there is no entry with this key
     */

    @Override
    public GroundedValue get(AtomicValue key) {
        GroundedValue value = super.get(key);
        if (value instanceof ArrayItem) {
            Pedigree ped = new Pedigree(this, key);
            return new PedigreeArrayItem((ArrayItem) value, ped);
        } else if (value instanceof MapItem) {
            Pedigree ped = new Pedigree(this, key);
            return new PedigreeMapItem((MapItem) value, ped);
        } else {
            return value;
        }
    }

    /**
     * Get an iterable collection of key-value pairs representing the entries
     * in this map; any entry whose value is a singleton array or map will
     * itself have a pedigree indicating how it was selected
     * @return the entries in this map as an iterable collection
     */

    @Override
    public Iterable<KeyValuePair> keyValuePairs() {
        List<KeyValuePair> pairs = new ArrayList<>();
        for (KeyValuePair val : super.keyValuePairs()) {
            Pedigree ped = new Pedigree(PedigreeMapItem.this, val.key);
            GroundedValue newValue = ped.makePedigreeValue(val.value);
            pairs.add(new KeyValuePair(val.key, newValue));
        }
        return pairs;
    }

    /**
     * Get the pedigree of this map item, providing information as to how it was selected.
     * @return the pedigree of this map item
     */

    @Override
    public Pedigree getPedigree() {
        return pedigree;
    }

}

// Copyright (c) 2018-2023 Saxonica Limited

