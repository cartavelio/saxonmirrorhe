package net.sf.saxon.ma.jnode;

import net.sf.saxon.ma.arrays.ArrayItem;
import net.sf.saxon.ma.map.KeyValuePair;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.IteratorWrapper;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.tree.iter.ListIterator;
import net.sf.saxon.tree.jiter.MappingJavaIterator;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.Int64Value;

import java.util.ArrayList;
import java.util.Iterator;
import java.util.List;

public class ChildJNode extends JNode {
    private JNode parent;
    private int position;
    private AtomicValue selector;
    private GroundedValue value;

    public ChildJNode(JNode parent, int position, AtomicValue selector, GroundedValue value) {
        this.parent = parent;
        this.position = position;
        this.selector = selector;
        this.value = value;
    }

    @Override
    public GroundedValue getValue() {
        return value;
    }


    @Override
    public int getPosition() {
        return position;
    }

    @Override
    public AtomicValue getSelector() {
        return selector;
    }

    @Override
    public JNode getParent() {
        return parent;
    }


}

