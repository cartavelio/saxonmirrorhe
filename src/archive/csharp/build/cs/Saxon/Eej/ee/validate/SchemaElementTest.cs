using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.validate {
    public class SchemaElementTest : Saxon.Hej.pattern.NodeTest, Saxon.Hej.pattern.SchemaNodeTest {
        private readonly Saxon.Eej.ee.schema.ElementDecl head;
        private readonly Saxon.Hej.z.IntHashMap<Saxon.Eej.ee.schema.ElementDecl> groupByFingerprint;
        private readonly System.Collections.Generic.Dictionary<Saxon.Hej.om.NodeName,Saxon.Eej.ee.schema.ElementDecl> groupByName;
        private readonly Saxon.Hej.type.SchemaType requiredType;
        public SchemaElementTest(Saxon.Eej.ee.schema.ElementDecl element)  {
            head = element;
            System.Collections.Generic.ICollection<Saxon.Eej.ee.schema.ElementDecl> subsGroup = element.getSubstitutionGroupMembers();
            groupByFingerprint = new Saxon.Hej.z.IntHashMap<Saxon.Eej.ee.schema.ElementDecl>(subsGroup.Count);
            groupByName = new System.Collections.Generic.Dictionary<Saxon.Hej.om.NodeName, Saxon.Eej.ee.schema.ElementDecl>(subsGroup.Count);
            foreach (Saxon.Eej.ee.schema.ElementDecl decl in subsGroup) {
                if (!(decl.isAbstract())) {
                    groupByFingerprint.put(decl.getFingerprint(), decl);
                    Saxon.Impl.Helpers.MapUtils.Put(groupByName, new Saxon.Hej.om.FingerprintedQName("", decl.getTargetNamespace(), decl.getName()), decl);
                }
            }
            requiredType = element.getType();
        }
        public Saxon.Eej.ee.schema.ElementDecl getElementDeclaration() {
            return head;
        }
        public Saxon.Hej.om.StructuredQName getNodeName() {
            return head.getComponentName();
        }
        public override Saxon.Hej.type.UType getUType() {
            return Saxon.Hej.type.UType.ELEMENT;
        }
        public override bool matches(int nodeKind, Saxon.Hej.om.NodeName name, Saxon.Hej.type.SchemaType annotation) {
            if ((nodeKind != Saxon.Hej.type.Type.ELEMENT)) {
                return false;
            }
            try {
                Saxon.Hej.type.TypeHierarchy th = head.getConfiguration().getTypeHierarchy();
                if (name.hasFingerprint()) {
                    Saxon.Eej.ee.schema.ElementDecl decl = groupByFingerprint.get((name.getFingerprint()&Saxon.Hej.om.NamePool.FP_MASK));
                    return ((decl != null) && matchesAnnotation(decl.getType(), annotation, th));
                } else {
                    Saxon.Eej.ee.schema.ElementDecl decl = Saxon.Impl.Helpers.MapUtils.Get(groupByName, name);
                    return ((decl != null) && matchesAnnotation(decl.getType(), annotation, th));
                }
            } catch (Saxon.Hej.type.MissingComponentException e) {
                return false;
            }
        }
        public override Saxon.Hej.z.IntPredicateProxy getMatcher(Saxon.Hej.tree.tiny.NodeVectorTree tree) {
            if (!(tree.isTyped())) {
                return Saxon.Hej.z.IntSetPredicate.ALWAYS_FALSE;
            }
            byte[] nodeKindArray = tree.getNodeKindArray();
            int[] nameCodeArray = tree.getNameCodeArray();
            return Saxon.Hej.z.IntPredicateLambda.of(nodeNr => {
                if ((((nodeKindArray[nodeNr]&0x0f)) != Saxon.Hej.type.Type.ELEMENT)) {
                    return false;
                }
                Saxon.Eej.ee.schema.ElementDecl decl = groupByFingerprint.get((nameCodeArray[nodeNr]&unchecked((int)0xfffff)));
                try {
                    return (((decl != null) && !(((((Saxon.Hej.tree.tiny.TinyTree)(tree)).isNilled(nodeNr) && !(decl.isNillable()))))) && matchesAnnotation(decl.getType(), ((Saxon.Hej.tree.tiny.TinyTree)(tree)).getSchemaType(nodeNr), decl.getConfiguration().getTypeHierarchy()));
                } catch (Saxon.Hej.type.MissingComponentException e) {
                    return false;
                }
            } );
        }
        public override bool test(Saxon.Hej.om.NodeInfo node) {
            if ((node.getNodeKind() != Saxon.Hej.type.Type.ELEMENT)) {
                return false;
            }
            Saxon.Eej.ee.schema.ElementDecl decl = Saxon.Impl.Helpers.MapUtils.Get(groupByName, Saxon.Hej.om.NameOfNode.makeName(node));
            try {
                return (((decl != null) && !(((node.isNilled() && !(decl.isNillable()))))) && matchesAnnotation(decl.getType(), node.getSchemaType(), decl.getConfiguration().getTypeHierarchy()));
            } catch (Saxon.Hej.type.MissingComponentException e) {
                return false;
            }
        }
        public static bool matchesAnnotation(Saxon.Hej.type.SchemaType required, Saxon.Hej.type.SchemaType actual, Saxon.Hej.type.TypeHierarchy th) {
            int requiredType = required.getFingerprint();
            if ((requiredType == Saxon.Hej.om.StandardNames.XS_ANY_TYPE)) {
                return true;
            }
            if (Saxon.Hej.type.AnyType.getInstance().Equals(required)) {
                return true;
            }
            Saxon.Hej.type.Affinity rel = th.schemaTypeRelationship(required, actual);
            return ((rel == Saxon.Hej.type.Affinity.SAME_TYPE) || (rel == Saxon.Hej.type.Affinity.SUBSUMES));
        }
        public override double getDefaultPriority() {
            return 0.0;
        }
        public override int getPrimitiveType() {
            return Saxon.Hej.type.Type.ELEMENT;
        }
        public override Saxon.Ejava.util.Optional<Saxon.Hej.z.IntSet> getRequiredNodeNames() {
            return Saxon.Ejava.util.Optional<Saxon.Hej.z.IntSet>.of(groupByFingerprint.keySet());
        }
        public override Saxon.Hej.type.SchemaType getContentType() {
            return requiredType;
        }
        public override bool isNillable() {
            return head.isNillable();
        }
        public bool isNillable(int fingerprint) {
            Saxon.Eej.ee.schema.ElementDecl decl = groupByFingerprint.get(fingerprint);
            return ((decl == null) ? isNillable() : decl.isNillable());
        }
        public int getHeadFingerprint() {
            return head.getFingerprint();
        }
        public override string ToString() {
            return (("schema-element("+head.getComponentName().getEQName())+')');
        }
        public override int GetHashCode() {
            return head.getFingerprint();
        }
        public override bool Equals(System.Object other) {
            return ((other is Saxon.Eej.ee.validate.SchemaElementTest) && (((Saxon.Eej.ee.validate.SchemaElementTest)(other)).head == head));
        }
        public override string getBasicAlphaCode() {
            return "NES";
        }
        public override Saxon.Ejava.util.Optional<string> explainMismatch(Saxon.Hej.om.Item item, Saxon.Hej.type.TypeHierarchy th) {
            Saxon.Ejava.util.Optional<string> explanation = base.explainMismatch(item, th);
            if (explanation.isPresent()) {
                return explanation;
            }
            Saxon.Hej.om.NodeInfo node = (Saxon.Hej.om.NodeInfo)(item);
            Saxon.Hej.om.NodeName name = Saxon.Hej.om.NameOfNode.makeName(node);
            try {
                Saxon.Eej.ee.schema.ElementDecl decl = Saxon.Impl.Helpers.MapUtils.Get(groupByName, name);
                if ((decl == null)) {
                    return Saxon.Ejava.util.Optional<string>.of(("The supplied element is not in the substitution group of element "+head.getDisplayName()));
                }
                if (!(matchesAnnotation(decl.getType(), node.getSchemaType(), th))) {
                    if ((node.getSchemaType() == Saxon.Hej.type.Untyped.getInstance())) {
                        return Saxon.Ejava.util.Optional<string>.of("The supplied element has not been schema-validated");
                    } else {
                        return Saxon.Ejava.util.Optional<string>.of((("The type annotation of the supplied element ("+decl.getType().getDescription())+") does not match the type for the global element declaration in the schema"));
                    }
                }
            } catch (Saxon.Hej.type.MissingComponentException e) {
                return Saxon.Ejava.util.Optional<string>.empty();
            }
            return Saxon.Ejava.util.Optional<string>.empty();
        }
    
    }
}