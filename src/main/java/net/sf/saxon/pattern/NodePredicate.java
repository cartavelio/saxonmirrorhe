package net.sf.saxon.pattern;

import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.transpile.CSharpDelegate;

@FunctionalInterface
@CSharpDelegate(false)
public interface NodePredicate {
    boolean test(NodeInfo node);
}

