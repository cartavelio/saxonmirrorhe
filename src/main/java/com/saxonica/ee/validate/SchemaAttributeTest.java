package com.saxonica.ee.validate;

import com.saxonica.ee.schema.AttributeDecl;
import com.saxonica.ee.schema.PreparedSchema;
import net.sf.saxon.Configuration;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.NodeName;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.pattern.NodeTest;
import net.sf.saxon.pattern.SchemaNodeTest;
import net.sf.saxon.pattern.qname.QNameTest;
import net.sf.saxon.pattern.qname.SpecificQNameTest;
import net.sf.saxon.tree.tiny.NodeVectorTree;
import net.sf.saxon.type.*;
import net.sf.saxon.z.IntPredicateProxy;
import net.sf.saxon.z.IntSet;
import net.sf.saxon.z.IntSetPredicate;
import net.sf.saxon.z.IntSingletonSet;

import java.util.Optional;

/**
 * A NodeTest of the form schema-attribute(attribute-name)
 *
 * @since 9.3
 */

public class SchemaAttributeTest extends NodeTest implements SchemaNodeTest {

    private final Configuration config;
    private final AttributeDecl declaration;
    private final SimpleType requiredType;

    /**
     * Constructor
     *
     * @param schema the relevant schema
     * @param attributeDecl The attribute declaration of the named attribute
     * @throws MissingComponentException if the type of the attribute is missing from the schema
     */

    public SchemaAttributeTest(PreparedSchema schema, AttributeDecl attributeDecl) throws MissingComponentException {
        config = attributeDecl.getConfiguration();
        declaration = attributeDecl;
        requiredType = attributeDecl.getType();
    }

    @Override
    public StructuredQName getNodeName() {
        return declaration.getComponentName();
    }

    @Override
    public QNameTest getQNameTest() {
        return new SpecificQNameTest(
                declaration.getComponentName(), config.getNamePool(), declaration.getFingerprint());
    }

    /**
     * Get the corresponding {@link net.sf.saxon.type.UType}. A UType is a union of primitive item
     * types.
     *
     * @return the smallest UType that subsumes this item type
     */
    @Override
    public UType getUType() {
        return UType.ATTRIBUTE;
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
        if (nodeKind != Type.ATTRIBUTE) {
            return false;
        }
        if (name.hasFingerprint()) {
            if (name.getFingerprint() != declaration.getFingerprint()) {
                return false;
            }
        } else {
            if (!(name.hasURI(declaration.getTargetNamespace()) && name.getLocalPart().equals(declaration.getName()))) {
                return false;
            }
        }
        return SchemaElementTest.matchesAnnotation(requiredType, annotation, config.getTypeHierarchy());

    }

    @Override
    public IntPredicateProxy getMatcher(NodeVectorTree tree) {
        return IntSetPredicate.ALWAYS_FALSE;
        // because this method is never used for attribute nodes
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
        if (node.hasFingerprint()) {
            return node.getNodeKind() == Type.ATTRIBUTE &&
                node.getFingerprint() == declaration.getFingerprint() &&
                SchemaElementTest.matchesAnnotation(requiredType, node.getSchemaType(), config.getTypeHierarchy());
        } else {
            return node.getNodeKind() == Type.ATTRIBUTE &&
                node.getNamespaceUri().equals(declaration.getTargetNamespace()) &&
                node.getLocalPart().equals(declaration.getName()) &&
                SchemaElementTest.matchesAnnotation(requiredType, node.getSchemaType(), config.getTypeHierarchy());
        }
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
        return Type.ATTRIBUTE;
    }

    /**
     * Get the set of node names allowed by this NodeTest. This is returned as a set of Integer fingerprints.
     * A null value indicates that all names are permitted (i.e. that there are no constraints on the node name.
     * The default implementation returns null.
     */

    /*@NotNull*/
    @Override
    public Optional<IntSet> getRequiredNodeNames() {
        return Optional.of(new IntSingletonSet(declaration.getFingerprint()));
    }

    /**
     * Get the content type allowed by this NodeTest (that is, the type annotation of the matched nodes).
     * Return AnyType if there are no restrictions. The default implementation returns AnyType.
     */

    @Override
    public SchemaType getContentType() {
        return requiredType;
    }

    public String toString() {
        return "schema-attribute(" + declaration.getComponentName().getEQName() + ')';
    }

    /**
     * Returns a hash code value for the object.
     */

    public int hashCode() {
        return declaration.getFingerprint();
    }

    public boolean equals(Object other) {
        return other instanceof SchemaAttributeTest &&
                ((SchemaAttributeTest) other).declaration == declaration;
    }

    @Override
    public String getBasicAlphaCode() {
        return "NAS";
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
        NodeInfo node = (NodeInfo)item;
        StructuredQName name = new StructuredQName("", node.getNamespaceUri(), node.getLocalPart());
        if (!declaration.getComponentName().equals(name)) {
            return Optional.of("The supplied attribute has the wrong name");
        }
        if (!SchemaElementTest.matchesAnnotation(requiredType, node.getSchemaType(), config.getTypeHierarchy())) {
            if (node.getSchemaType() == BuiltInAtomicType.UNTYPED_ATOMIC) {
                return Optional.of("The supplied attribute has not been schema-validated");
            } else {
                return Optional.of("The supplied attribute has a non-matching type annotation (" + node.getSchemaType().getDisplayName() + ")");
            }
        }
        return Optional.empty();
    }


}

// Copyright (c) 2005-2023 Saxonica Limited