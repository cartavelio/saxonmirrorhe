package com.saxonica.ee.validate;

import com.saxonica.ee.schema.ElementDecl;
import com.saxonica.ee.schema.PreparedSchema;
import net.sf.saxon.om.*;
import net.sf.saxon.pattern.NodeTest;
import net.sf.saxon.pattern.SchemaNodeTest;
import net.sf.saxon.tree.tiny.NodeVectorTree;
import net.sf.saxon.tree.tiny.TinyTree;
import net.sf.saxon.type.*;
import net.sf.saxon.z.*;

import java.util.Collection;
import java.util.HashMap;
import java.util.Optional;


/**
 * A NodeTest of the form schema-element(element-name)
 *
 * @since 9.3
 */

public class SchemaElementTest extends NodeTest implements SchemaNodeTest {

    // Prior to 9.3 a schema-element() test was represented by a CombinedNodeTest combining a NameTest and
    // a ContentTypeTest. This was changed as a consequence of the change to the specification introduced
    // by the resolution of specification bug 10065

    private final PreparedSchema schema;
    private final ElementDecl head;
    private final IntHashMap<ElementDecl> groupByFingerprint;
    private final HashMap<NodeName, ElementDecl> groupByName;
    private final SchemaType requiredType;

    /**
     * Constructor
     *
     * @param element The element declaration of the named element
     */

    public SchemaElementTest(PreparedSchema schema, ElementDecl element) throws MissingComponentException {
        head = element;
        this.schema = schema;
        Collection<ElementDecl> subsGroup = schema.getSubstitutionGroupMembers(element);
        groupByFingerprint = new IntHashMap<>(subsGroup.size());
        groupByName = new HashMap<>(subsGroup.size());
        for (ElementDecl decl : subsGroup) {
            if (!decl.isAbstract()) {
                groupByFingerprint.put(decl.getFingerprint(), decl);
                groupByName.put(new FingerprintedQName("", decl.getTargetNamespace(), decl.getName()), decl);
            }
        }
        requiredType = element.getType();
    }

    /**
     * Get the element declaration
     *
     * @return the element declaration that this schema-element relates to (the head of the substitution group)
     */

    public ElementDecl getElementDeclaration() {
        return head;
    }

    /**
     * Get the schema from the static context of this type designation
     * @return the in-scope schema
     */

    public Schema getSchema() {
        return schema;
    }

    @Override
    public StructuredQName getNodeName() {
        return head.getComponentName();
    }

    /**
     * Get the corresponding {@link net.sf.saxon.type.UType}. A UType is a union of primitive item
     * types.
     *
     * @return the smallest UType that subsumes this item type
     */
    @Override
    public UType getUType() {
        return UType.ELEMENT;
    }

    /**
     * Test whether this node test is satisfied by a given node. This method is only
     * fully supported for a subset of NodeTests, because it doesn't provide all the information
     * needed to evaluate all node tests. In particular (a) it can't be used to evaluate a node
     * test of the form element(N,T) or schema-element(E) where it is necessary to know whether the
     * node is nilled, and (b) it can't be used to evaluate a node test of the form
     * document-node(element(X)). This in practice means that it is used (a) to evaluate the
     * simple node tests found in the XPath 1.0 subset used in XML Schema, and (b) to evaluate
     * node tests where the node kind is known to be an attribute.
     *
     * @param nodeKind   The kind of node to be matched
     * @param name       identifies the expanded name of the node to be matched.
     *                   The value should be null for a node with no name.
     * @param annotation The actual content type of the node
     */
    @Override
    public boolean matches(int nodeKind, NodeName name, SchemaType annotation) {
        if (nodeKind != Type.ELEMENT) {
            return false;
        }
        try {
            TypeHierarchy th = head.getConfiguration().getTypeHierarchy();
            if (name.hasFingerprint()) {
                ElementDecl decl = groupByFingerprint.get(name.getFingerprint() & NamePool.FP_MASK);
                return decl != null && matchesAnnotation(decl.getType(), annotation, th);
            } else {
                ElementDecl decl = groupByName.get(name);
                return decl != null && matchesAnnotation(decl.getType(), annotation, th);
            }
        } catch (MissingComponentException e) {
            return false;
        }

    }

    @Override
    public IntPredicateProxy getMatcher(final NodeVectorTree tree) {
        if (!tree.isTyped()) {
            return IntSetPredicate.ALWAYS_FALSE;
        }
        final byte[] nodeKindArray = tree.getNodeKindArray();
        final int[] nameCodeArray = tree.getNameCodeArray();
        return IntPredicateLambda.of(nodeNr -> {
            if ((nodeKindArray[nodeNr] & 0x0f)  != Type.ELEMENT) {
                return false;
            }
            ElementDecl decl = groupByFingerprint.get(nameCodeArray[nodeNr] & 0xfffff);
            try {
                boolean ok = decl != null && !(((TinyTree) tree).isNilled(nodeNr) && !decl.isNillable()) /*&& !decl.isAbstract()*/ &&
                        matchesAnnotation(decl.getType(), ((TinyTree) tree).getSchemaType(nodeNr), decl.getConfiguration().getTypeHierarchy());
                System.err.println(((TinyTree)tree).getConfiguration().getNamePool().getDisplayName(nameCodeArray[nodeNr] & 0xfffff) + " = " + ok);
                return ok;
            } catch (MissingComponentException e) {
                return false;
            }
        });
    }

    /**
     * Test whether this node test is satisfied by a given node. This alternative
     * method is used in the case of nodes where calculating the fingerprint is expensive,
     * for example DOM or JDOM nodes.
     *
     * @param node the node to be matched
     */

    @Override
    public boolean test(/*@NotNull*/ NodeInfo node) {
        if (node.getNodeKind() != Type.ELEMENT) {
            return false;
        }
        ElementDecl decl = groupByName.get(NameOfNode.makeName(node));
        try {
            return decl != null &&
                    !(node.isNilled() && !decl.isNillable()) /*&& !decl.isAbstract()*/ &&
                    matchesAnnotation(decl.getType(), node.getSchemaType(), decl.getConfiguration().getTypeHierarchy());
        } catch (MissingComponentException e) {
            return false;
        }
    }

    /**
     * Determine whether the type annotation of an element or attribute instances matches the required
     * type of the element or attribute declaration named in a schema-element() or schema-attribute() test
     *
     * @param required the required type of the declaration named in the schema-element() or schema-attribute test
     * @param actual   the actual type annotation of the instance
     * @param th       the type hierarchy cache
     * @return true if there is a match, false otherwise
     */

    public static boolean matchesAnnotation(SchemaType required, SchemaType actual, TypeHierarchy th) {
        int requiredType = required.getFingerprint();
        if (requiredType == StandardNames.XS_ANY_TYPE) {
            return true;
        }

        if (AnyType.getInstance().equals(required)) {
            return true;
        }

        Affinity rel = th.schemaTypeRelationship(required, actual);
        return rel == Affinity.SAME_TYPE || rel == Affinity.SUBSUMES;
    }

    /**
     * Determine the default priority of this node test when used on its own as a Pattern
     */

    @Override
    public final double getDefaultPriority() {
        return 0.0;
    }

    /**
     * Determine the types of nodes to which this pattern applies. Used for optimisation.
     * For patterns that match nodes of several types, return Type.NODE
     *
     * @return the type of node matched by this pattern. e.g. Type.ELEMENT or Type.TEXT
     */

    @Override
    public int getPrimitiveType() {
        return Type.ELEMENT;
    }

    /**
     * Get the set of node names allowed by this NodeTest. This is returned as a set of Integer fingerprints.
     * A null value indicates that all names are permitted (i.e. that there are no constraints on the node name.
     * The default implementation returns null.
     */

    /*@NotNull*/
    @Override
    public Optional<IntSet> getRequiredNodeNames() {
        return Optional.of(groupByFingerprint.keySet());
    }

    /**
     * Get the content type allowed by this NodeTest (that is, the type annotation of the matched nodes).
     * Return AnyType if there are no restrictions. The default implementation returns AnyType.
     */

    @Override
    public SchemaType getContentType() {
        return requiredType;
    }

    /**
     * Determine whether nilled nodes can match this node test
     *
     * @return true if nilled nodes can match this node test
     */
    @Override
    public boolean isNillable() {
        return head.isNillable();
    }

    /**
     * Determine if nilled nodes named N can match this node test
     *
     * @param fingerprint the element name
     * @return true if nilled nodes with the given name can match the node test
     */

    public boolean isNillable(int fingerprint) {
        ElementDecl decl = groupByFingerprint.get(fingerprint);
        return decl == null ? isNillable() : decl.isNillable();
    }

    /**
     * Get the fingerprint of the head of the substitution group
     *
     * @return the fingerprint of the head of the substitution group
     */

    public int getHeadFingerprint() {
        return head.getFingerprint();
    }

    public String toString() {
        return "schema-element(" + head.getComponentName().getEQName() + ')';
    }

    /**
     * Returns a hash code value for the object.
     */

    public int hashCode() {
        return head.getFingerprint();
    }

    public boolean equals(Object other) {
        return other instanceof SchemaElementTest &&
                ((SchemaElementTest) other).head == head;
    }

    @Override
    public String getBasicAlphaCode() {
        return "NES";
    }

    /**
     * Get extra diagnostic information about why a supplied item does not conform to this
     * item type, if available. If extra information is returned, it should be in the form of a complete
     * sentence, minus the closing full stop. No information should be returned for obvious cases.
     *
     * @param item the item that doesn't match this type
     * @param th   the type hierarchy cache
     * @return optionally, a message explaining why the item does not match the type
     */
    @Override
    public Optional<String> explainMismatch(Item item, TypeHierarchy th) {
        Optional<String> explanation = super.explainMismatch(item, th);
        if (explanation.isPresent()) {
            return explanation;
        }
        NodeInfo node = (NodeInfo) item;
        NodeName name = NameOfNode.makeName(node);

        try {
            ElementDecl decl = groupByName.get(name);
            if (decl == null) {
                return Optional.of("The supplied element is not in the substitution group of element " + head.getDisplayName());
            }
            if (!matchesAnnotation(decl.getType(), node.getSchemaType(), th)) {
                if (node.getSchemaType() == Untyped.getInstance()) {
                    return Optional.of("The supplied element has not been schema-validated");
                } else {
                    return Optional.of("The type annotation of the supplied element (" +
                                               decl.getType().getDescription() +
                                               ") does not match the type for the global element declaration in the schema");
                }
            }
        } catch (MissingComponentException e) {
            return Optional.empty();
        }
        return Optional.empty();
    }

}

// Copyright (c) 2005-2023 Saxonica Limited