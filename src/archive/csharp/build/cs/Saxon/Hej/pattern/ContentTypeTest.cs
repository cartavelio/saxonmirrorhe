using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.pattern {
    public class ContentTypeTest : Saxon.Hej.pattern.NodeTest {
        private readonly int kind;
        private readonly Saxon.Hej.type.SchemaType schemaType;
        private readonly Saxon.Hej.Configuration config;
        private bool nillable = false;
        public ContentTypeTest(int nodeKind, Saxon.Hej.type.SchemaType schemaType, Saxon.Hej.Configuration config, bool nillable)  {
            this.kind = nodeKind;
            this.schemaType = schemaType;
            this.config = config;
            this.nillable = nillable;
        }
        public override Saxon.Hej.type.UType getUType() {
            return ((kind == Saxon.Hej.type.Type.ELEMENT) ? Saxon.Hej.type.UType.ELEMENT : Saxon.Hej.type.UType.ATTRIBUTE);
        }
        public void setNillable(bool nillable) {
            this.nillable = nillable;
        }
        public override bool isNillable() {
            return nillable;
        }
        public Saxon.Hej.type.SchemaType getSchemaType() {
            return schemaType;
        }
        public int getNodeKind() {
            return kind;
        }
        public override bool matches(int nodeKind, Saxon.Hej.om.NodeName name, Saxon.Hej.type.SchemaType annotation) {
            return ((kind == nodeKind) && matchesAnnotation(annotation));
        }
        public override Saxon.Hej.z.IntPredicateProxy getMatcher(Saxon.Hej.tree.tiny.NodeVectorTree tree) {
            byte[] nodeKindArray = tree.getNodeKindArray();
            return Saxon.Hej.z.IntPredicateLambda.of(nodeNr => (((((nodeKindArray[nodeNr]&0x0f)) == kind) && matchesAnnotation(((Saxon.Hej.tree.tiny.TinyTree)(tree)).getSchemaType(nodeNr))) && ((nillable || !(((Saxon.Hej.tree.tiny.TinyTree)(tree)).isNilled(nodeNr))))));
        }
        public override bool test(Saxon.Hej.om.NodeInfo node) {
            return (((node.getNodeKind() == kind) && matchesAnnotation(node.getSchemaType())) && ((nillable || !(Saxon.Hej.functions.Nilled_1.isNilled(node)))));
        }
        private bool matchesAnnotation(Saxon.Hej.type.SchemaType annotation) {
            if ((annotation == null)) {
                return false;
            }
            if ((schemaType == Saxon.Hej.type.AnyType.getInstance())) {
                return true;
            }
            if (annotation.Equals(schemaType)) {
                return true;
            }
            Saxon.Hej.type.Affinity r = config.getTypeHierarchy().schemaTypeRelationship(annotation, schemaType);
            return ((r == Saxon.Hej.type.Affinity.SAME_TYPE) || (r == Saxon.Hej.type.Affinity.SUBSUMED_BY));
        }
        public override double getDefaultPriority() {
            return 0;
        }
        public override int getPrimitiveType() {
            return kind;
        }
        public override Saxon.Hej.type.SchemaType getContentType() {
            return schemaType;
        }
        public override Saxon.Hej.type.AtomicType getAtomizedItemType() {
            Saxon.Hej.type.SchemaType type = schemaType;
            try {
                if (type.isAtomicType()) {
                    return (Saxon.Hej.type.AtomicType)(type);
                } else {
                    if ((type is Saxon.Hej.type.ListType)) {
                        Saxon.Hej.type.SimpleType mem = ((Saxon.Hej.type.ListType)(type)).getItemType();
                        if (mem.isAtomicType()) {
                            return (Saxon.Hej.type.AtomicType)(mem);
                        }
                    } else {
                        if (((type is Saxon.Hej.type.ComplexType) && ((Saxon.Hej.type.ComplexType)(type)).isSimpleContent())) {
                            Saxon.Hej.type.SimpleType ctype = ((Saxon.Hej.type.ComplexType)(type)).getSimpleContentType();
                            System.Diagnostics.Debug.Assert((ctype != null));
                            if (ctype.isAtomicType()) {
                                return (Saxon.Hej.type.AtomicType)(ctype);
                            } else {
                                if ((ctype is Saxon.Hej.type.ListType)) {
                                    Saxon.Hej.type.SimpleType mem = ((Saxon.Hej.type.ListType)(ctype)).getItemType();
                                    if (mem.isAtomicType()) {
                                        return (Saxon.Hej.type.AtomicType)(mem);
                                    }
                                }
                            }
                        }
                    }
                }
            } catch (Saxon.Hej.type.MissingComponentException e) {
                return Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC;
            }
            return Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC;
        }
        public override bool isAtomizable(Saxon.Hej.type.TypeHierarchy th) {
            return !(((schemaType.isComplexType() && (((Saxon.Hej.type.ComplexType)(schemaType)).getVariety() == Saxon.Hej.type.ComplexVariety.ELEMENT_ONLY))));
        }
        public override string ToString() {
            return (((((kind == Saxon.Hej.type.Type.ELEMENT) ? "element(*, " : "attribute(*, "))+schemaType.getEQName())+')');
        }
        public override string toExportString() {
            return (((((kind == Saxon.Hej.type.Type.ELEMENT) ? "element(*, " : "attribute(*, "))+Saxon.Hej.type.TypeHierarchy.getNearestNamedType(schemaType).getEQName())+')');
        }
        public override int GetHashCode() {
            return ((kind<<20)^schemaType.GetHashCode());
        }
        public override bool Equals(System.Object other) {
            return ((((other is Saxon.Hej.pattern.ContentTypeTest) && (((Saxon.Hej.pattern.ContentTypeTest)(other)).kind == kind)) && (((Saxon.Hej.pattern.ContentTypeTest)(other)).schemaType == schemaType)) && (((Saxon.Hej.pattern.ContentTypeTest)(other)).nillable == nillable));
        }
        public override Saxon.Ejava.util.Optional<string> explainMismatch(Saxon.Hej.om.Item item, Saxon.Hej.type.TypeHierarchy th) {
            Saxon.Ejava.util.Optional<string> explanation = base.explainMismatch(item, th);
            if (explanation.isPresent()) {
                return explanation;
            }
            Saxon.Hej.om.NodeInfo node = (Saxon.Hej.om.NodeInfo)(item);
            if (!(matchesAnnotation(((Saxon.Hej.om.NodeInfo)(item)).getSchemaType()))) {
                if ((node.getSchemaType() == Saxon.Hej.type.Untyped.getInstance())) {
                    return Saxon.Ejava.util.Optional<string>.of("The supplied node has not been schema-validated");
                }
                if ((node.getSchemaType() == Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC)) {
                    return Saxon.Ejava.util.Optional<string>.of("The supplied node has not been schema-validated");
                }
                return Saxon.Ejava.util.Optional<string>.of((("The supplied node has the wrong type annotation ("+node.getSchemaType().getDescription())+")"));
            }
            if ((Saxon.Hej.functions.Nilled_1.isNilled(node) && !(nillable))) {
                return Saxon.Ejava.util.Optional<string>.of("The supplied node has xsi:nil='true', which the required type does not allow");
            }
            return Saxon.Ejava.util.Optional<string>.empty();
        }
    
    }
}