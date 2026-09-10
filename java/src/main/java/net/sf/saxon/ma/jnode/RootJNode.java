package net.sf.saxon.ma.jnode;

import net.sf.saxon.ma.MapOrArray;
import net.sf.saxon.value.AtomicValue;

public class RootJNode extends JNode {

    private final MapOrArray value;

    public RootJNode(MapOrArray value) {
        this.value = value;
    }

    @Override
    public MapOrArray getValue() {
        return value;
    }

    @Override
    public int getPosition() {
        return -1;
    }

    @Override
    public AtomicValue getSelector() {
        return null;
    }

    @Override
    public JNode getParent() {
        return null;
    }
}

