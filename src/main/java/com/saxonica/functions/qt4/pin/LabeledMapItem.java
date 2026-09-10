package com.saxonica.functions.qt4.pin;

import com.saxonica.xsltextn.pedigree.DelegatingMapItem;
import net.sf.saxon.expr.CallableDelegate;
import net.sf.saxon.functions.CallableFunction;
import net.sf.saxon.ma.map.KeyValuePair;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.ma.map.ShapedMap;
import net.sf.saxon.ma.map.StringMap;
import net.sf.saxon.om.FunctionItem;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Item;
import net.sf.saxon.type.SpecificFunctionType;
import net.sf.saxon.value.*;

import java.util.ArrayList;
import java.util.List;

/**                                                   n
 * A <code>LabeledMapItem</code> represents an XDM map together with a label, which is a map:
 * the map typically contains retained information about how the map was selected within a JSON-like tree.
 */

public class LabeledMapItem extends DelegatingMapItem implements LabeledItem {

    private final StringMap label;

    /**
     * Create a LabeledMapItem with a supplied label
     * @param base the subject map item
     * @param label the label
     */

    public LabeledMapItem(MapItem base, StringMap label) {
        super(base);
        this.label = label;
    }

    /**
     * Get the subject item without its label
     *
     * @return the subject item
     */
    @Override
    public Item getSubject() {
        return getBaseItem();
    }

    public static LabeledMapItem makePinnedMapItem(MapItem base) {
        return new LabeledMapItem(base, LabelTool.PIN_LABEL);
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
        if (value == null) {
            return null;
        }
        return adaptValue(this, key, value);
    }

    /**
     * Get an iterable collection of key-value pairs representing the entries
     * in this map; any entry whose value is a singleton array or map will
     * itself have a pedigree indicating how it was selected
     *
     * @return the entries in this map as an iterable collection
     */

    @Override
    public Iterable<KeyValuePair> keyValuePairs() {
        // TODO: lazy evaluation
        List<KeyValuePair> pairs = new ArrayList<>();
        for (KeyValuePair kvp : super.keyValuePairs()) {
            pairs.add(new KeyValuePair(kvp.key, adaptValue(this, kvp.key, kvp.value)));
        }
        return pairs;
    }

    @Override
    public MapItem remove(AtomicValue key) {
        // TODO: handle multiple calls on remove
        return super.remove(key);
    }

    public static GroundedValue adaptValue(LabeledItem parent, AtomicValue key, GroundedValue value) {
        List<Item> labeledItems = new ArrayList<>();
        int position = 0;
        for (Item item : value.asIterable()) {
            Item subject = item;
            if (item instanceof LabeledItem) {
                subject = ((LabeledItem) item).getSubject();
            }
            // Tricky because the new item and the label both reference each other
            ShapedMap label = new ShapedMap(LabelTool.NAVIGATION_LABEL_SHAPE,
                                            parent,
                                            key,
                                            Int64Value.makeIntegerValue(++position),
                                            EmptySequence.getInstance(),
                                            EmptySequence.getInstance());
            final Item labeledItem = LabelTool.addLabel(subject, label);
            labeledItems.add(labeledItem);
            label.initialPut(3, makeAncestorsFunction(labeledItem));
            label.initialPut(4, makePathFunction(labeledItem));
        }
        return SequenceExtent.makeSequenceExtent(labeledItems);
    }



    private static FunctionItem makeAncestorsFunction(Item start) {
        CallableDelegate logic = new CallableDelegate((context, arguments) -> {
            List<Item> ancestors = new ArrayList<>();
            Item parent = (Item)start.getLabel().get(new StringValue("parent"));
            while (parent != null && parent.getLabel() != null) {
                ancestors.add(parent);
                parent = (Item)parent.getLabel().get(new StringValue("parent"));
            }
            return SequenceExtent.makeSequenceExtent(ancestors);
        });
        logic.setDescription("fn:label()?ancestors");
        return new CallableFunction(0, logic,
                                    new SpecificFunctionType(SequenceType.ANY_SEQUENCE));
    }

    private static FunctionItem makePathFunction(Item start) {
        CallableDelegate logic = new CallableDelegate((context, arguments) -> {
            List<Item> path = new ArrayList<>();
            Item parent = start;
            do {
                MapItem label = parent.getLabel();
                AtomicValue key = (AtomicValue) label.get(new StringValue("key"));
                if (key == null) {
                    break;
                }
                path.add(0, key);
                parent = (Item) label.get(new StringValue("parent"));
            } while (parent != null && parent.getLabel() != null);
            return SequenceExtent.makeSequenceExtent(path);
        });
        logic.setDescription("fn:label()?key");
        return new CallableFunction(0, logic,
                                    new SpecificFunctionType(SequenceType.ANY_SEQUENCE));
    }

//    public static LabeledItem makeLabeledItem(Item subject, StringMap label) {
//        if (subject instanceof MapItem) {
//            return new LabeledMapItem((MapItem)subject, label);
//        } else if (subject instanceof ArrayItem) {
//            return new LabeledArrayItem((ArrayItem)subject, label);
//        } else {
//            return new AnyLabeledItem(subject, label);
//        }
//    }

    /**
     * Get the pedigree of this map item, providing information as to how it was selected.
     *
     * @return the pedigree of this map item
     */

    @Override
    public StringMap getLabel() {
        return label;
    }

    public String toString() {
        return "¶" + getBaseItem().toString();
    }


}

// Copyright (c) 2018-2023 Saxonica Limited

