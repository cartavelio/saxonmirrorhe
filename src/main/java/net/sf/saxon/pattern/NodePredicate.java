package net.sf.saxon.pattern;

import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.NodeName;
import net.sf.saxon.type.SchemaType;
import net.sf.saxon.type.UType;

import java.util.function.Predicate;

/**
 * A NodeTest that wraps a general Predicate applied to nodes
 */

public class NodePredicate extends NodeTest {

    private final Predicate<? super NodeInfo> predicate;

    public NodePredicate(Predicate<? super NodeInfo> predicate) {
        this.predicate = predicate;
    }

    @Override
    public boolean test(NodeInfo node) {
        return predicate.test(node);
    }

    @Override
    public double getDefaultPriority() {
        return 0;
    }

    @Override
    public boolean matches(int nodeKind, NodeName name, SchemaType annotation) {
        throw new UnsupportedOperationException("NodePredicate doesn't support this method");
    }

    @Override
    public UType getUType() {
        return UType.ANY;
    }
}

