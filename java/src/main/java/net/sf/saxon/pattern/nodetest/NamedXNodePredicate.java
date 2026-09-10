package net.sf.saxon.pattern.nodetest;

import net.sf.saxon.om.StructuredQName;

/**
 * Interface implemented by a node test that selects nodes of a specific node
 * kind with a specific node name with a known
 * fingerprint. Having the right name and node kind is a necessary
 * condition for satisfying the predicate, and if {@link #isFingerprintSufficient()} returns
 * true, it is also a sufficient condition.
 */

public interface NamedXNodePredicate extends NodePredicate {

    /**
     * Get the kind of nodes that this node predicate matches
     * @return the kind of nodes, for example {@link net.sf.saxon.type.Type#ELEMENT} or {@code net.sf.saxon.type.Type#ATTRIBUTE}
     */

    int getNodeKind();

    /**
     * Get a fingerprint that all nodes (of the specified node kind) must have
     * if they are to satisfy the predicate.
     *
     * @return the fingerprint of the name that a node must have if it is to satisfy the predicate
     */

    int getRequiredFingerprint();

    /**
     * Get the corresponding StructuredQName: the name that all nodes must have
     * if they are to satisfy the predicate
     *
     * @return the name that a node must have if it is to satisfy the predicate
     */

    StructuredQName getMatchingNodeName();

    /**
     * Ask whether having the required fingerprint and node kind is a sufficient
     * condition for a node to satisfy the predicate, or whether other conditions
     * (such as type annotation or nillability) must also be satisfied.
     * @return true if matching the fingerprint is a sufficient condition.
     */

    boolean isFingerprintSufficient();

}

