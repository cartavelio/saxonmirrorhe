package net.sf.saxon.pattern.nodetest;

import net.sf.saxon.om.StructuredQName;

/**
 * Interface implemented by a node test that selects nodes of a specific node
 * kind (element or attribute) with a specific node name with a known
 * fingerprint. Having the right fingerprint and node kind is a sufficient
 * condition for satisfying the predicate.
 */

public interface FingerprintedNameTest extends NodePredicate {

    /**
     * Get a fingerprint that all nodes (of the specified node kind) must have
     * if they are to satisfy the predicate.
     *
     * @param nodeKind the kind of nodes required.
     * @return if all nodes selected by the node test are of the required nodeKind, and
     * have the same fingerprint, then return that fingerprint; otherwise return -1.
     */

    int getRequiredFingerprint(int nodeKind);

    /**
     * Get the corresponding StructuredQName: the name that all nodes must have
     * if they are to satisfy the predicate
     * @param nodeKind the kind of nodes required.
     * @return if all nodes selected by the node test are of the required nodeKind, and
     * have the same name, then return that name; otherwise return null.
     */

    StructuredQName getMatchingNodeName(int nodeKind);

    /**
     * Ask whether having the required fingerprint and node kind is a sufficient
     * condition for a node to satisfy the predicate, or whether other conditions
     * (such as type annotation or nillability) must also be satisfied.
     * @return true if matching the fingerprint is a sufficient condition.
     */

    boolean isFingerprintSufficient();

}

