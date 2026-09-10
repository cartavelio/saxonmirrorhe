package net.sf.saxon.ma;

import net.sf.saxon.ma.jnode.RootJNode;
import net.sf.saxon.om.FunctionItem;

public abstract class MapOrArray implements FunctionItem {

    private RootJNode rootJNode;

    public synchronized RootJNode obtainRootJNode() {
        if (rootJNode == null) {
            return rootJNode = new RootJNode(this);
        } else {
            return rootJNode;
        }
    }
}

