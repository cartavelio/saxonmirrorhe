package net.sf.saxon.pattern;

import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.NodeName;
import net.sf.saxon.type.SchemaType;
import net.sf.saxon.type.UType;

import java.util.function.Predicate;

/**
 * A NodeTest that wraps a general Predicate applied to nodes.
 *
 * <p>This is needed for C#, where it is not possible for a method such as {@link NodeInfo#iterateAxis(int, NodePredicate)}
 * to accept either a NodeTest object or a Predicate supplied as a lambda expression. To supply a predicate, it is
 * therefore necessary to wrap it in a {@code NodePredicate}.</p>
 */

public class NodeSelector extends NodeTest {

    private final Predicate<? super NodeInfo> predicate;

    private NodeSelector(Predicate<? super NodeInfo> predicate) {
        this.predicate = predicate;
    }

    public static NodeSelector of(Predicate<? super NodeInfo> predicate) {
        return new NodeSelector(predicate);
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

