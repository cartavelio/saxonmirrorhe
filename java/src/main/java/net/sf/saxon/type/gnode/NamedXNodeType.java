////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.type.gnode;

import net.sf.saxon.Configuration;
import net.sf.saxon.functions.Nilled_1;
import net.sf.saxon.om.*;
import net.sf.saxon.pattern.UnionQNameTest;
import net.sf.saxon.pattern.nodetest.NamedXNodePredicate;
import net.sf.saxon.pattern.nodetest.NodeTest;
import net.sf.saxon.pattern.qname.*;
import net.sf.saxon.type.*;

import java.util.Optional;

/**
 * {@code NamedXNodeType} is a node type corresponding to the syntax
 * {@code element(N, T)} or {@code attribute(N, T)} or {@code namespace(N)}
 * or {@code processing-instruction(N)}.
 * <p>It tests for a node of a particular kind, whose name matches a
 * QNameTest (which embraces wildcards and choices) and whose type
 * annotation matches a given schema type; it is also conditional on whether
 * the node is nilled.</p>
 */

public class NamedXNodeType extends XNodeType implements NamedXNodePredicate {

    private final int kind;
    private final QNameTest nameTest;
    private final SchemaType schemaType;
    private final boolean nillable;
    private final Configuration config;

    /**
     * Create an NamedNodeKindType
     *
     * @param nodeKind   the kind of nodes to be matched: always elements or attributes
     * @param nameTest   constrains the name of the node
     * @param schemaType the required type annotation, as a simple or complex schema type
     * @param nillable   indicates whether an element with xsi:nil=true satisifies the test
     * @param config     the Configuration, supplied because this KindTest needs access to schema information
     */

    public NamedXNodeType(int nodeKind, QNameTest nameTest, SchemaType schemaType, boolean nillable, Configuration config) {
        this.kind = nodeKind;
        this.nameTest = nameTest;
        this.schemaType = schemaType;
        this.config = config;
        this.nillable = nillable;
    }

    /**
     * Simplified convenience constructor
     * @param nodeKind   the kind of nodes to be matched: always elements or attributes
     * @param nameTest   constrains the name of the node
     * @param config     the Configuration, supplied because this KindTest needs access to schema information
     */
    public NamedXNodeType(int nodeKind, QNameTest nameTest, Configuration config) {
        this.kind = nodeKind;
        this.nameTest = nameTest;
        this.schemaType = AnyType.getInstance();
        this.config = config;
        this.nillable = true;
    }

    /**
     * Simplified convenience constructor
     *
     * @param nodeKind the kind of nodes to be matched: always elements or attributes
     * @param nameTest constrains the name of the node
     * @param config   the Configuration, supplied because this KindTest needs access to schema information
     */
    public NamedXNodeType(int nodeKind, StructuredQName nameTest, Configuration config) {
        this.kind = nodeKind;
        this.nameTest = new SpecificQNameTest(nameTest, config.getNamePool());
        this.schemaType = AnyType.getInstance();
        this.config = config;
        this.nillable = true;
    }

    public static NodeTest make(int nodeKind, NamespaceUri uri, String localName, Configuration config) {
        return new NamedXNodeType(nodeKind, new StructuredQName("", uri, localName), config);
    }

    public static NodeTest make(int nodeKind, int fingerprint, Configuration config) {
        return new NamedXNodeType(nodeKind, config.getNamePool().getStructuredQName(fingerprint), config);
    }


    /**
     * Get the corresponding {@link net.sf.saxon.type.UType}. A UType is a union of primitive item
     * types.
     *
     * @return the smallest UType that subsumes this item type
     */
    @Override
    public UType getUType() {
        switch (kind) {
            case Type.ELEMENT:
                return UType.ELEMENT;
            case Type.ATTRIBUTE:
                return UType.ATTRIBUTE;
            case Type.PROCESSING_INSTRUCTION:
                return UType.PI;
            case Type.NAMESPACE:
                return UType.NAMESPACE;
            default:
                return UType.ANY;
        }
    }

    /**
     * Get the set of allowed node names that this type if capable
     * of matching
     *
     * @return the allowed node names
     */
    @Override
    public QNameTest getAllowedNodeNames() {
        return nameTest;
    }

    /**
     * For an {@code XNodeType} that can only match one kind of node
     * and one node name, return that node name, as an integer
     * fingerprint. In other cases, return -1.
     *
     * @return an integer fingerprint in the case of a node type that
     * only matches one node kind and one qualified name. In other
     * cases return -1.
     */
    @Override
    public int getRequiredFingerprint() {
        if (nameTest instanceof SpecificQNameTest) {
            return ((SpecificQNameTest)nameTest).getFingerprint();
        }
        return -1;
    }

    /**
     * Get the corresponding StructuredQName: the name that all nodes must have
     * if they are to satisfy the predicate
     *
     * @return if all nodes selected by the node test are of the required nodeKind, and
     * have the same name, then return that name; otherwise return null.
     */
    @Override
    public StructuredQName getMatchingNodeName() {
        if (nameTest instanceof SpecificQNameTest) {
            return ((SpecificQNameTest) nameTest).getStructuredQName();
        }
        return null;
    }

    /**
     * Ask whether having the required fingerprint and node kind is a sufficient
     * condition for a node to satisfy the predicate, or whether other conditions
     * (such as type annotation or nillability) must also be satisfied.
     *
     * @return true if matching the fingerprint is a sufficient condition.
     */
    @Override
    public boolean isFingerprintSufficient() {
        if (kind == Type.ELEMENT) {
            return (nillable && schemaType.equals(AnyType.getInstance()));
        }
        if (kind == Type.ATTRIBUTE) {
            return schemaType.equals(BuiltInAtomicType.ANY_ATOMIC);
        }
        return true;
    }

    /**
     * Get the Saxon Configuration to which this type belongs
     * @return the Saxon configuration
     */

    public Configuration getConfiguration() {
        return config;
    }


    /**
     * The test is nillable if a question mark was specified as the occurrence indicator
     *
     * @return true if the test is nillable
     */

    public boolean isNillable() {
        return nillable;
    }

    public SchemaType getContentType() {
        return schemaType;
    }

    public int getNodeKind() {
        return kind;
    }

    /**
     * Test whether a given item conforms to this type
     *
     * @param item The item to be tested
     * @return true if the item is an instance of this type; false otherwise
     */
    @Override
    public boolean matches(Item item) {
        if (item instanceof NodeInfo) {
            NodeInfo n = (NodeInfo) item;
            if (n.getNodeKind() != kind) {
                return false;
            }
            if (n.hasFingerprint() && nameTest instanceof SpecificQNameTest) {
                return ((SpecificQNameTest) nameTest).getFingerprint() == n.getFingerprint() &&
                        matchesAnnotation(n.getSchemaType())
                        && (nillable || !Nilled_1.isNilled(n));
            } else {
                return nameTest.matches(n.getQName()) &&
                        matchesAnnotation(n.getSchemaType())
                        && (nillable || !Nilled_1.isNilled(n));
            }
        } else {
            return false;
        }
    }



    private boolean matchesAnnotation(SchemaType annotation) {
        if (annotation == null) {
            return true;
        }
        if (schemaType == AnyType.getInstance()) {
            return true;
        }

        if (annotation.equals(schemaType)) {
            return true;
        }

        // see if the type annotation is a subtype of the required type
        Affinity r = config.getTypeHierarchy().schemaTypeRelationship(annotation, schemaType);
        return r == Affinity.SAME_TYPE || r == Affinity.SUBSUMED_BY;
    }

    /**
     * Determine the default priority of this node test when used on its own as a Pattern
     */

    @Override
    public final double getDefaultPriority() {
        if (schemaType == null || schemaType == AnyType.getInstance()) {
            if (nameTest instanceof AnyQNameTest) {
                return -0.5;
            } else if (nameTest instanceof SpecificQNameTest) {
                return 0;
            } else if (nameTest instanceof UnionQNameTest) {
                return -0.25;
            } else if (nameTest instanceof LocalQNameTest || nameTest instanceof NamespaceQNameTest) {
                return -0.25;
            } else {
                // Not sure what this covers...
                return 0;
            }
        } else {
            if (nameTest instanceof AnyQNameTest) {
                return 0;
            } else if (nameTest instanceof SpecificQNameTest) {
                return 0.25;
            } else if (nameTest instanceof UnionQNameTest) {
                return 0.125;
            } else if (nameTest instanceof LocalQNameTest || nameTest instanceof NamespaceQNameTest) {
                return 0.125;
            } else {
                // Not sure what this covers...
                return 0;
            }
        }
    }

    /**
     * Determine the types of nodes to which this pattern applies. Used for optimisation.
     *
     * @return the type of node matched by this pattern. e.g. Type.ELEMENT or Type.TEXT
     */

    @Override
    public int getPrimitiveType() {
        return kind;
    }

    /**
     * Get the item type of the atomic values that will be produced when an item
     * of this type is atomized (assuming that atomization succeeds)
     */

    /*@NotNull*/
    @Override
    public AtomicType getAtomizedItemType() {
        SchemaType type = schemaType;
        try {
            if (type.isAtomicType()) {
                return (AtomicType) type;
            } else if (type instanceof ListType) {
                SimpleType mem = ((ListType) type).getItemType();
                if (mem.isAtomicType()) {
                    return (AtomicType) mem;
                }
            } else if (type instanceof ComplexType && ((ComplexType) type).isSimpleContent()) {
                SimpleType ctype = ((ComplexType) type).getSimpleContentType();
                assert ctype != null;
                if (ctype.isAtomicType()) {
                    return (AtomicType) ctype;
                } else if (ctype instanceof ListType) {
                    SimpleType mem = ((ListType) ctype).getItemType();
                    if (mem.isAtomicType()) {
                        return (AtomicType) mem;
                    }
                }
            }
        } catch (MissingComponentException e) {
            return BuiltInAtomicType.ANY_ATOMIC;
        }
        return BuiltInAtomicType.ANY_ATOMIC;
    }

    /**
     * Ask whether values of this type are atomizable
     *
     * @return true unless it is known that these items will be elements with element-only
     *         content, in which case return false
     * @param th The type hierarchy cache
     */

    @Override
    public boolean isAtomizable(TypeHierarchy th) {
        return !(schemaType.isComplexType() &&
                ((ComplexType) schemaType).getVariety() == ComplexVariety.ELEMENT_ONLY);
    }

    public String toString() {
        switch (kind) {
            case Type.ELEMENT:
                return "element(" + nameTest.toString() + ", " + schemaType.getEQName() + ')';
            case Type.ATTRIBUTE:
                return "attribute(" + nameTest.toString() + ", " + schemaType.getEQName() + ')';
            case Type.PROCESSING_INSTRUCTION:
                return "processing-instruction(" + nameTest.toString() + ")";
            case Type.NAMESPACE:
                return "namespace(" + nameTest.toString() + ")";
            default:
                return "???";
        }
    }

    /**
     * Return a string representation of this ItemType suitable for use in stylesheet
     * export files. This differs from the result of toString() in that it will not contain
     * any references to anonymous types. Note that it may also use the Saxon extended syntax
     * for union types and tuple types. The default implementation returns the result of
     * calling {@code toString()}.
     *
     * @return the string representation as an instance of the XPath SequenceType construct
     */
    @Override
    public String toExportString() {
        return (kind == Type.ELEMENT ? "element(" : "attribute(") +
                nameTest.toString() + ',' +
                TypeHierarchy.getNearestNamedType(schemaType).getEQName() + ')';
    }

    /**
     * Returns a hash code value for the object.
     */

    public int hashCode() {
        return kind << 20 ^ schemaType.hashCode();
    }

    /**
     * Indicates whether some other object is "equal to" this one.
     */
    public boolean equals(Object other) {
        return other instanceof NamedXNodeType &&
                ((NamedXNodeType) other).kind == kind &&
                ((NamedXNodeType) other).nameTest.equals(nameTest) &&
                ((NamedXNodeType) other).schemaType == schemaType &&
                ((NamedXNodeType) other).nillable == nillable;
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
        if (!nameTest.matches(NameOfNode.makeName(node).getStructuredQName())) {
            return Optional.of("The node name does not match");
        }
        if (!matchesAnnotation(((NodeInfo)item).getSchemaType())) {
            if (node.getSchemaType() == Untyped.getInstance()) {
                return Optional.of("The supplied node has not been schema-validated");
            }
            if (node.getSchemaType() == BuiltInAtomicType.UNTYPED_ATOMIC) {
                return Optional.of("The supplied node has not been schema-validated");
            }
            return Optional.of("The supplied node has the wrong type annotation (" + node.getSchemaType().getDescription() + ")");
        }
        if (Nilled_1.isNilled(node) && !nillable) {
            return Optional.of("The supplied node has xsi:nil='true', which the required type does not allow");
        }
        return Optional.empty();
    }

    /**
     * Get an alphabetic code representing the type, or at any rate, the nearest built-in type
     * from which this type is derived. The codes are designed so that for any two built-in types
     * A and B, alphaCode(A) is a prefix of alphaCode(B) if and only if A is a supertype of B.
     *
     * @return the alphacode for the nearest containing built-in type. For example: for xs:string
     * return "AS", for xs:boolean "AB", for node() "N", for element() "NE", for map(*) "FM", for
     * array(*) "FA".
     */
    @Override
    public String getBasicAlphaCode() {
        return "????";
    }


}

