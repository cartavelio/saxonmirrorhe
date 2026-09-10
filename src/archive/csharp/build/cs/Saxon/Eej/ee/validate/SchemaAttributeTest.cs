using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.validate {
    public class SchemaAttributeTest : Saxon.Hej.pattern.NodeTest, Saxon.Hej.pattern.SchemaNodeTest {
        private readonly Saxon.Hej.Configuration config;
        private readonly Saxon.Eej.ee.schema.AttributeDecl declaration;
        private readonly Saxon.Hej.type.SimpleType requiredType;
        public SchemaAttributeTest(Saxon.Eej.ee.schema.AttributeDecl attributeDecl)  {
            config = attributeDecl.getConfiguration();
            declaration = attributeDecl;
            requiredType = ((/*1430*/Saxon.Hej.type.SimpleType)attributeDecl.getType());
        }
        public Saxon.Hej.om.StructuredQName getNodeName() {
            return declaration.getComponentName();
        }
        public override Saxon.Hej.type.UType getUType() {
            return Saxon.Hej.type.UType.ATTRIBUTE;
        }
        public override bool matches(int nodeKind, Saxon.Hej.om.NodeName name, Saxon.Hej.type.SchemaType annotation) {
            if ((nodeKind != Saxon.Hej.type.Type.ATTRIBUTE)) {
                return false;
            }
            if (name.hasFingerprint()) {
                if ((name.getFingerprint() != declaration.getFingerprint())) {
                    return false;
                }
            } else {
                if (!(((name.hasURI(declaration.getTargetNamespace()) && name.getLocalPart().Equals(declaration.getName()))))) {
                    return false;
                }
            }
            return /*134*/Saxon.Eej.ee.validate.SchemaElementTest.matchesAnnotation(requiredType, annotation, config.getTypeHierarchy());
        }
        public override Saxon.Hej.z.IntPredicateProxy getMatcher(Saxon.Hej.tree.tiny.NodeVectorTree tree) {
            return Saxon.Hej.z.IntSetPredicate.ALWAYS_FALSE;
        }
        public override bool test(Saxon.Hej.om.NodeInfo node) {
            if (node.hasFingerprint()) {
                return (((node.getNodeKind() == Saxon.Hej.type.Type.ATTRIBUTE) && (node.getFingerprint() == declaration.getFingerprint())) && /*134*/Saxon.Eej.ee.validate.SchemaElementTest.matchesAnnotation(requiredType, node.getSchemaType(), config.getTypeHierarchy()));
            } else {
                return ((((node.getNodeKind() == Saxon.Hej.type.Type.ATTRIBUTE) && node.getNamespaceUri().Equals(declaration.getTargetNamespace())) && node.getLocalPart().Equals(declaration.getName())) && /*134*/Saxon.Eej.ee.validate.SchemaElementTest.matchesAnnotation(requiredType, node.getSchemaType(), config.getTypeHierarchy()));
            }
        }
        public override double getDefaultPriority() {
            return 0.0;
        }
        public override int getPrimitiveType() {
            return Saxon.Hej.type.Type.ATTRIBUTE;
        }
        public override Saxon.Ejava.util.Optional<Saxon.Hej.z.IntSet> getRequiredNodeNames() {
            return Saxon.Ejava.util.Optional<Saxon.Hej.z.IntSet>.of(new Saxon.Hej.z.IntSingletonSet(declaration.getFingerprint()));
        }
        public override Saxon.Hej.type.SchemaType getContentType() {
            return requiredType;
        }
        public override string ToString() {
            return (("schema-attribute("+declaration.getComponentName().getEQName())+')');
        }
        public override int GetHashCode() {
            return declaration.getFingerprint();
        }
        public override bool Equals(System.Object other) {
            return ((other is Saxon.Eej.ee.validate.SchemaAttributeTest) && (((Saxon.Eej.ee.validate.SchemaAttributeTest)(other)).declaration == declaration));
        }
        public override string getBasicAlphaCode() {
            return "NAS";
        }
        public override Saxon.Ejava.util.Optional<string> explainMismatch(Saxon.Hej.om.Item item, Saxon.Hej.type.TypeHierarchy th) {
            Saxon.Ejava.util.Optional<string> explanation = base.explainMismatch(item, th);
            if (explanation.isPresent()) {
                return explanation;
            }
            Saxon.Hej.om.NodeInfo node = (Saxon.Hej.om.NodeInfo)(item);
            Saxon.Hej.om.StructuredQName name = new Saxon.Hej.om.StructuredQName("", node.getNamespaceUri(), node.getLocalPart());
            if (!(declaration.getComponentName().Equals(name))) {
                return Saxon.Ejava.util.Optional<string>.of("The supplied attribute has the wrong name");
            }
            if (!(/*134*/Saxon.Eej.ee.validate.SchemaElementTest.matchesAnnotation(requiredType, node.getSchemaType(), config.getTypeHierarchy()))) {
                if ((node.getSchemaType() == Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC)) {
                    return Saxon.Ejava.util.Optional<string>.of("The supplied attribute has not been schema-validated");
                } else {
                    return Saxon.Ejava.util.Optional<string>.of((("The supplied attribute has a non-matching type annotation ("+node.getSchemaType().getDisplayName())+")"));
                }
            }
            return Saxon.Ejava.util.Optional<string>.empty();
        }
    
    }
}