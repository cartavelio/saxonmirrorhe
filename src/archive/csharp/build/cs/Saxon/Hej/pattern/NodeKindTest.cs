using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.pattern {
    public class NodeKindTest : Saxon.Hej.pattern.NodeTest {
        public static readonly Saxon.Hej.pattern.NodeKindTest DOCUMENT = new Saxon.Hej.pattern.NodeKindTest(Saxon.Hej.type.Type.DOCUMENT);
        public static readonly Saxon.Hej.pattern.NodeKindTest ELEMENT = new Saxon.Hej.pattern.NodeKindTest(Saxon.Hej.type.Type.ELEMENT);
        public static readonly Saxon.Hej.pattern.NodeKindTest ATTRIBUTE = new Saxon.Hej.pattern.NodeKindTest(Saxon.Hej.type.Type.ATTRIBUTE);
        public static readonly Saxon.Hej.pattern.NodeKindTest TEXT = new Saxon.Hej.pattern.NodeKindTest(Saxon.Hej.type.Type.TEXT);
        public static readonly Saxon.Hej.pattern.NodeKindTest COMMENT = new Saxon.Hej.pattern.NodeKindTest(Saxon.Hej.type.Type.COMMENT);
        public static readonly Saxon.Hej.pattern.NodeKindTest PROCESSING_INSTRUCTION = new Saxon.Hej.pattern.NodeKindTest(Saxon.Hej.type.Type.PROCESSING_INSTRUCTION);
        public static readonly Saxon.Hej.pattern.NodeKindTest NAMESPACE = new Saxon.Hej.pattern.NodeKindTest(Saxon.Hej.type.Type.NAMESPACE);
        private readonly int kind;
        private readonly Saxon.Hej.type.UType uType;
        private NodeKindTest(int nodeKind)  {
            kind = nodeKind;
            uType = Saxon.Hej.type.UType.fromTypeCode(nodeKind);
        }
        public int getNodeKind() {
            return kind;
        }
        public override Saxon.Hej.type.UType getUType() {
            return uType;
        }
        public static Saxon.Hej.pattern.NodeTest makeNodeKindTest(int kind) {
            switch (kind) {
              case Saxon.Hej.type.Type.DOCUMENT:
                return DOCUMENT;
              case Saxon.Hej.type.Type.ELEMENT:
                return ELEMENT;
              case Saxon.Hej.type.Type.ATTRIBUTE:
                return ATTRIBUTE;
              case Saxon.Hej.type.Type.COMMENT:
                return COMMENT;
              case Saxon.Hej.type.Type.TEXT:
                return TEXT;
              case Saxon.Hej.type.Type.PROCESSING_INSTRUCTION:
                return PROCESSING_INSTRUCTION;
              case Saxon.Hej.type.Type.NAMESPACE:
                return NAMESPACE;
              case Saxon.Hej.type.Type.NODE:
                return Saxon.Hej.pattern.AnyNodeTest.getInstance();
              default:
                throw new Saxon.Ejava.lang.IllegalArgumentException((("Unknown node kind "+kind)+" in NodeKindTest"));
            }
        }
        public override bool matches(Saxon.Hej.om.Item item, Saxon.Hej.type.TypeHierarchy th) {
            return ((item is Saxon.Hej.om.NodeInfo) && (kind == ((Saxon.Hej.om.NodeInfo)(item)).getNodeKind()));
        }
        public override bool matches(int nodeKind, Saxon.Hej.om.NodeName name, Saxon.Hej.type.SchemaType annotation) {
            return (kind == nodeKind);
        }
        public override Saxon.Hej.z.IntPredicateProxy getMatcher(Saxon.Hej.tree.tiny.NodeVectorTree tree) {
            byte[] nodeKindArray = tree.getNodeKindArray();
            if ((kind == Saxon.Hej.type.Type.TEXT)) {
                return Saxon.Hej.z.IntPredicateLambda.of(nodeNr => {
                    int k = nodeKindArray[nodeNr];
                    return ((k == Saxon.Hej.type.Type.TEXT) || (k == Saxon.Hej.type.Type.WHITESPACE_TEXT));
                } );
            } else {
                return Saxon.Hej.z.IntPredicateLambda.of(nodeNr => (((nodeKindArray[nodeNr]&0x0f)) == kind));
            }
        }
        public override bool test(Saxon.Hej.om.NodeInfo node) {
            return (node.getNodeKind() == kind);
        }
        public override double getDefaultPriority() {
            return -(0.5);
        }
        public override int getPrimitiveType() {
            return kind;
        }
        public override Saxon.Hej.type.SchemaType getContentType() {
            switch (kind) {
              case Saxon.Hej.type.Type.DOCUMENT:
                return Saxon.Hej.type.AnyType.getInstance();
              case Saxon.Hej.type.Type.ELEMENT:
                return Saxon.Hej.type.AnyType.getInstance();
              case Saxon.Hej.type.Type.ATTRIBUTE:
                return Saxon.Hej.type.AnySimpleType.getInstance();
              case Saxon.Hej.type.Type.COMMENT:
                return Saxon.Hej.type.BuiltInAtomicType.STRING;
              case Saxon.Hej.type.Type.TEXT:
                return Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC;
              case Saxon.Hej.type.Type.PROCESSING_INSTRUCTION:
                return Saxon.Hej.type.BuiltInAtomicType.STRING;
              case Saxon.Hej.type.Type.NAMESPACE:
                return Saxon.Hej.type.BuiltInAtomicType.STRING;
              default:
                throw new Saxon.Ejava.lang.AssertionError("Unknown node kind");
            }
        }
        public override Saxon.Hej.type.AtomicType getAtomizedItemType() {
            switch (kind) {
              case Saxon.Hej.type.Type.DOCUMENT:
                return Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC;
              case Saxon.Hej.type.Type.ELEMENT:
                return Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC;
              case Saxon.Hej.type.Type.ATTRIBUTE:
                return Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC;
              case Saxon.Hej.type.Type.COMMENT:
                return Saxon.Hej.type.BuiltInAtomicType.STRING;
              case Saxon.Hej.type.Type.TEXT:
                return Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC;
              case Saxon.Hej.type.Type.PROCESSING_INSTRUCTION:
                return Saxon.Hej.type.BuiltInAtomicType.STRING;
              case Saxon.Hej.type.Type.NAMESPACE:
                return Saxon.Hej.type.BuiltInAtomicType.STRING;
              default:
                throw new Saxon.Ejava.lang.AssertionError("Unknown node kind");
            }
        }
        public override string ToString() {
            return describe(kind);
        }
        public static string describe(int kind) {
            switch (kind) {
              case Saxon.Hej.type.Type.DOCUMENT:
                return "document-node()";
              case Saxon.Hej.type.Type.ELEMENT:
                return "element()";
              case Saxon.Hej.type.Type.ATTRIBUTE:
                return "attribute()";
              case Saxon.Hej.type.Type.COMMENT:
                return "comment()";
              case Saxon.Hej.type.Type.TEXT:
                return "text()";
              case Saxon.Hej.type.Type.PROCESSING_INSTRUCTION:
                return "processing-instruction()";
              case Saxon.Hej.type.Type.NAMESPACE:
                return "namespace-node()";
              default:
                return "** error **";
            }
        }
        public static string nodeKindName(int kind) {
            switch (kind) {
              case Saxon.Hej.type.Type.DOCUMENT:
                return "document";
              case Saxon.Hej.type.Type.ELEMENT:
                return "element";
              case Saxon.Hej.type.Type.ATTRIBUTE:
                return "attribute";
              case Saxon.Hej.type.Type.COMMENT:
                return "comment";
              case Saxon.Hej.type.Type.TEXT:
                return "text";
              case Saxon.Hej.type.Type.PROCESSING_INSTRUCTION:
                return "processing-instruction";
              case Saxon.Hej.type.Type.NAMESPACE:
                return "namespace";
              default:
                return "** error **";
            }
        }
        public override int GetHashCode() {
            return kind;
        }
        public override bool Equals(System.Object other) {
            return ((other is Saxon.Hej.pattern.NodeKindTest) && (((Saxon.Hej.pattern.NodeKindTest)(other)).kind == kind));
        }
        public override Saxon.Ejava.util.Optional<string> explainMismatch(Saxon.Hej.om.Item item, Saxon.Hej.type.TypeHierarchy th) {
            Saxon.Ejava.util.Optional<string> explanation = base.explainMismatch(item, th);
            if (explanation.isPresent()) {
                return explanation;
            }
            if ((item is Saxon.Hej.om.NodeInfo)) {
                Saxon.Hej.type.UType actualKind = Saxon.Hej.type.UType.getUType(item);
                if (!(getUType().overlaps(actualKind))) {
                    return Saxon.Ejava.util.Optional<string>.of(("The supplied value is "+actualKind.toStringWithIndefiniteArticle()));
                } else {
                    return Saxon.Ejava.util.Optional<string>.empty();
                }
            } else {
                return Saxon.Ejava.util.Optional<string>.of(("The supplied value is "+Saxon.Hej.trans.Err.describeGenre(item.getGenre())));
            }
        }
        public override string toShortString() {
            switch (getNodeKind()) {
              case Saxon.Hej.type.Type.ELEMENT:
                return "*";
              case Saxon.Hej.type.Type.ATTRIBUTE:
                return "@*";
              case Saxon.Hej.type.Type.DOCUMENT:
                return "/";
              default:
                return ToString();
            }
        }
    
    }
}