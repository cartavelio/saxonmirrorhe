using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.pattern {
    public class CombinedNodeTest : Saxon.Hej.pattern.NodeTest {
        private readonly Saxon.Hej.pattern.NodeTest nodetest1;
        private readonly Saxon.Hej.pattern.NodeTest nodetest2;
        private readonly int @operator;
        public CombinedNodeTest(Saxon.Hej.pattern.NodeTest nt1, int @operator, Saxon.Hej.pattern.NodeTest nt2)  {
            nodetest1 = nt1;
            this.@operator = @operator;
            nodetest2 = nt2;
        }
        public override Saxon.Hej.type.UType getUType() {
            Saxon.Hej.type.UType u1 = nodetest1.getUType();
            Saxon.Hej.type.UType u2 = nodetest2.getUType();
            switch (@operator) {
              case Saxon.Hej.expr.parser.Token.UNION:
                return u1.union(u2);
              case Saxon.Hej.expr.parser.Token.INTERSECT:
                return u1.intersection(u2);
              case Saxon.Hej.expr.parser.Token.EXCEPT:
                return u1;
              default:
                throw new Saxon.Ejava.lang.IllegalArgumentException("Unknown operator in Combined Node Test");
            }
        }
        public override bool matches(int nodeKind, Saxon.Hej.om.NodeName name, Saxon.Hej.type.SchemaType annotation) {
            switch (@operator) {
              case Saxon.Hej.expr.parser.Token.UNION:
                return ((((nodetest1 == null) || (nodetest2 == null)) || nodetest1.matches(nodeKind, name, annotation)) || nodetest2.matches(nodeKind, name, annotation));
              case Saxon.Hej.expr.parser.Token.INTERSECT:
                return ((((nodetest1 == null) || nodetest1.matches(nodeKind, name, annotation))) && (((nodetest2 == null) || nodetest2.matches(nodeKind, name, annotation))));
              case Saxon.Hej.expr.parser.Token.EXCEPT:
                return ((((nodetest1 == null) || nodetest1.matches(nodeKind, name, annotation))) && !((((nodetest2 == null) || nodetest2.matches(nodeKind, name, annotation)))));
              default:
                throw new Saxon.Ejava.lang.IllegalArgumentException("Unknown operator in Combined Node Test");
            }
        }
        public override Saxon.Hej.z.IntPredicateProxy getMatcher(Saxon.Hej.tree.tiny.NodeVectorTree tree) {
            switch (@operator) {
              case Saxon.Hej.expr.parser.Token.UNION:
                return Saxon.Hej.z.IntUnionPredicate.makeUnion(nodetest1.getMatcher(tree), (nodetest2.getMatcher(tree)));
              case Saxon.Hej.expr.parser.Token.INTERSECT:
                return Saxon.Hej.z.IntIntersectionPredicate.makeIntersection(nodetest1.getMatcher(tree), (nodetest2.getMatcher(tree)));
              case Saxon.Hej.expr.parser.Token.EXCEPT:
                return Saxon.Hej.z.IntExceptPredicate.makeDifference(nodetest1.getMatcher(tree), nodetest2.getMatcher(tree));
              default:
                throw new Saxon.Ejava.lang.IllegalArgumentException("Unknown operator in Combined Node Test");
            }
        }
        public override bool test(Saxon.Hej.om.NodeInfo node) {
            switch (@operator) {
              case Saxon.Hej.expr.parser.Token.UNION:
                return ((((nodetest1 == null) || (nodetest2 == null)) || nodetest1.test(node)) || nodetest2.test(node));
              case Saxon.Hej.expr.parser.Token.INTERSECT:
                return ((((nodetest1 == null) || nodetest1.test(node))) && (((nodetest2 == null) || nodetest2.test(node))));
              case Saxon.Hej.expr.parser.Token.EXCEPT:
                return ((((nodetest1 == null) || nodetest1.test(node))) && !((((nodetest2 == null) || nodetest2.test(node)))));
              default:
                throw new Saxon.Ejava.lang.IllegalArgumentException("Unknown operator in Combined Node Test");
            }
        }
        public override string ToString() {
            return makeString(false);
        }
        private string makeString(bool forExport) {
            if (((nodetest1 is Saxon.Hej.pattern.NameTest) && (@operator == Saxon.Hej.expr.parser.Token.INTERSECT))) {
                int kind = nodetest1.getPrimitiveType();
                string skind = ((kind == Saxon.Hej.type.Type.ELEMENT) ? "element(" : "attribute(");
                string content = "";
                if ((nodetest2 is Saxon.Hej.pattern.ContentTypeTest)) {
                    Saxon.Hej.type.SchemaType schemaType = ((Saxon.Hej.pattern.ContentTypeTest)(nodetest2)).getSchemaType();
                    if (forExport) {
                        schemaType = Saxon.Hej.type.TypeHierarchy.getNearestNamedType(schemaType);
                    }
                    content = (", "+schemaType.getEQName());
                    if (nodetest2.isNillable()) {
                        content += "?";
                    }
                }
                string name = nodetest1.getMatchingNodeName().getEQName();
                return (((skind+name)+content)+')');
            } else {
                string nt1 = ((nodetest1 == null) ? "item()" : nodetest1.ToString());
                string nt2 = ((nodetest2 == null) ? "item()" : nodetest2.ToString());
                return (((((('('+nt1)+' ')+Saxon.Hej.expr.parser.Token.tokens[@operator])+' ')+nt2)+')');
            }
        }
        public override string toExportString() {
            return makeString(true);
        }
        public string getContentTypeForAlphaCode() {
            if ((((nodetest1 is Saxon.Hej.pattern.NameTest) && (@operator == Saxon.Hej.expr.parser.Token.INTERSECT)) && (nodetest2 is Saxon.Hej.pattern.ContentTypeTest))) {
                return getContentTypeForAlphaCode((Saxon.Hej.pattern.NameTest)(nodetest1), (Saxon.Hej.pattern.ContentTypeTest)(nodetest2));
            } else {
                if ((((nodetest2 is Saxon.Hej.pattern.NameTest) && (@operator == Saxon.Hej.expr.parser.Token.INTERSECT)) && (nodetest1 is Saxon.Hej.pattern.ContentTypeTest))) {
                    return getContentTypeForAlphaCode((Saxon.Hej.pattern.NameTest)(nodetest2), (Saxon.Hej.pattern.ContentTypeTest)(nodetest1));
                } else {
                    return null;
                }
            }
        }
        private static string getContentTypeForAlphaCode(Saxon.Hej.pattern.NameTest nodetest1, Saxon.Hej.pattern.ContentTypeTest nodetest2) {
            if ((nodetest1.getNodeKind() == Saxon.Hej.type.Type.ELEMENT)) {
                if (((nodetest2.getContentType() == Saxon.Hej.type.Untyped.getInstance()) && nodetest2.isNillable())) {
                    return null;
                } else {
                    Saxon.Hej.type.SchemaType contentType = nodetest2.getContentType();
                    return contentType.getEQName();
                }
            } else {
                if ((nodetest1.getNodeKind() == Saxon.Hej.type.Type.ATTRIBUTE)) {
                    if ((nodetest2.getContentType() == Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC)) {
                        return null;
                    } else {
                        Saxon.Hej.type.SchemaType contentType = nodetest2.getContentType();
                        return contentType.getEQName();
                    }
                } else {
                    throw new Saxon.Ejava.lang.IllegalStateException();
                }
            }
        }
        public void addTypeDetails(Saxon.Hej.ma.map.DictionaryMap map) {
            if (((nodetest1 is Saxon.Hej.pattern.NameTest) && (@operator == Saxon.Hej.expr.parser.Token.INTERSECT))) {
                map.initialPut("n", new Saxon.Hej.value.StringValue(nodetest1.getMatchingNodeName().getEQName()));
                if ((nodetest2 is Saxon.Hej.pattern.ContentTypeTest)) {
                    Saxon.Hej.type.SchemaType schemaType = ((Saxon.Hej.pattern.ContentTypeTest)(nodetest2)).getSchemaType();
                    if (((schemaType != Saxon.Hej.type.Untyped.getInstance()) && (schemaType != Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC))) {
                        map.initialPut("c", new Saxon.Hej.value.StringValue((schemaType.getEQName()+((nodetest2.isNillable() ? "?" : "")))));
                    }
                }
            }
        }
        public override int getPrimitiveType() {
            Saxon.Hej.type.UType mask = getUType();
            if (mask.Equals(Saxon.Hej.type.UType.ELEMENT)) {
                return Saxon.Hej.type.Type.ELEMENT;
            }
            if (mask.Equals(Saxon.Hej.type.UType.ATTRIBUTE)) {
                return Saxon.Hej.type.Type.ATTRIBUTE;
            }
            if (mask.Equals(Saxon.Hej.type.UType.DOCUMENT)) {
                return Saxon.Hej.type.Type.DOCUMENT;
            }
            return Saxon.Hej.type.Type.NODE;
        }
        public override Saxon.Ejava.util.Optional<Saxon.Hej.z.IntSet> getRequiredNodeNames() {
            Saxon.Ejava.util.Optional<Saxon.Hej.z.IntSet> os1 = nodetest1.getRequiredNodeNames();
            Saxon.Ejava.util.Optional<Saxon.Hej.z.IntSet> os2 = nodetest2.getRequiredNodeNames();
            if ((os1.isPresent() && os2.isPresent())) {
                Saxon.Hej.z.IntSet s1 = os1.get();
                Saxon.Hej.z.IntSet s2 = os2.get();
                switch (@operator) {
                  case Saxon.Hej.expr.parser.Token.UNION:
                    {
                        return Saxon.Ejava.util.Optional<Saxon.Hej.z.IntSet>.of(s1.union(s2));
                    }
                  case Saxon.Hej.expr.parser.Token.INTERSECT:
                    {
                        return Saxon.Ejava.util.Optional<Saxon.Hej.z.IntSet>.of(s1.intersect(s2));
                    }
                  case Saxon.Hej.expr.parser.Token.EXCEPT:
                    {
                        return Saxon.Ejava.util.Optional<Saxon.Hej.z.IntSet>.of(s1.except(s2));
                    }
                  default:
                    throw new Saxon.Ejava.lang.IllegalStateException();
                }
            } else {
                return Saxon.Ejava.util.Optional<Saxon.Hej.z.IntSet>.empty();
            }
        }
        public override Saxon.Hej.type.SchemaType getContentType() {
            Saxon.Hej.type.SchemaType type1 = nodetest1.getContentType();
            Saxon.Hej.type.SchemaType type2 = nodetest2.getContentType();
            if (type1.isSameType(type2)) {
                return type1;
            }
            if ((@operator == Saxon.Hej.expr.parser.Token.INTERSECT)) {
                if (((type2 is Saxon.Hej.type.AnyType) || (((type2 is Saxon.Hej.type.AnySimpleType) && type1.isSimpleType())))) {
                    return type1;
                }
                if (((type1 is Saxon.Hej.type.AnyType) || (((type1 is Saxon.Hej.type.AnySimpleType) && type2.isSimpleType())))) {
                    return type2;
                }
            }
            return Saxon.Hej.type.AnyType.getInstance();
        }
        public override Saxon.Hej.type.AtomicType getAtomizedItemType() {
            Saxon.Hej.type.AtomicType type1 = ((/*1430*/Saxon.Hej.type.AtomicType)nodetest1.getAtomizedItemType());
            Saxon.Hej.type.AtomicType type2 = ((/*1430*/Saxon.Hej.type.AtomicType)nodetest2.getAtomizedItemType());
            if (type1.isSameType(type2)) {
                return type1;
            }
            if ((@operator == Saxon.Hej.expr.parser.Token.INTERSECT)) {
                if (type2.Equals(Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC)) {
                    return type1;
                }
                if (type1.Equals(Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC)) {
                    return type2;
                }
            }
            return Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC;
        }
        public override bool isAtomizable(Saxon.Hej.type.TypeHierarchy th) {
            switch (@operator) {
              case Saxon.Hej.expr.parser.Token.UNION:
                return (nodetest1.isAtomizable(th) || nodetest2.isAtomizable(th));
              case Saxon.Hej.expr.parser.Token.INTERSECT:
                return (nodetest1.isAtomizable(th) && nodetest2.isAtomizable(th));
              case Saxon.Hej.expr.parser.Token.EXCEPT:
                return nodetest1.isAtomizable(th);
              default:
                return true;
            }
        }
        public override int getFingerprint() {
            int fp1 = nodetest1.getFingerprint();
            int fp2 = nodetest2.getFingerprint();
            if ((fp1 == fp2)) {
                return fp1;
            }
            if (((fp2 == -1) && (@operator == Saxon.Hej.expr.parser.Token.INTERSECT))) {
                return fp1;
            }
            if (((fp1 == -1) && (@operator == Saxon.Hej.expr.parser.Token.INTERSECT))) {
                return fp2;
            }
            return -1;
        }
        public override Saxon.Hej.om.StructuredQName getMatchingNodeName() {
            Saxon.Hej.om.StructuredQName n1 = nodetest1.getMatchingNodeName();
            Saxon.Hej.om.StructuredQName n2 = nodetest2.getMatchingNodeName();
            if (((n1 != null) && n1.Equals(n2))) {
                return n1;
            }
            if (((n1 == null) && (@operator == Saxon.Hej.expr.parser.Token.INTERSECT))) {
                return n2;
            }
            if (((n2 == null) && (@operator == Saxon.Hej.expr.parser.Token.INTERSECT))) {
                return n1;
            }
            return null;
        }
        public override bool isNillable() {
            return (nodetest1.isNillable() && nodetest2.isNillable());
        }
        public override int GetHashCode() {
            return (nodetest1.GetHashCode()^nodetest2.GetHashCode());
        }
        public override bool Equals(System.Object other) {
            return ((((other is Saxon.Hej.pattern.CombinedNodeTest) && ((Saxon.Hej.pattern.CombinedNodeTest)(other)).nodetest1.Equals(nodetest1)) && ((Saxon.Hej.pattern.CombinedNodeTest)(other)).nodetest2.Equals(nodetest2)) && (((Saxon.Hej.pattern.CombinedNodeTest)(other)).@operator == @operator));
        }
        public override double getDefaultPriority() {
            if ((@operator == Saxon.Hej.expr.parser.Token.UNION)) {
                return nodetest1.getDefaultPriority();
            } else {
                return ((nodetest1 is Saxon.Hej.pattern.NameTest) ? 0.25 : 0.125);
            }
        }
        public Saxon.Hej.pattern.NodeTest[] getComponentNodeTests() {
            return new Saxon.Hej.pattern.NodeTest[]{nodetest1, nodetest2};
        }
        public int getOperator() {
            return @operator;
        }
        public Saxon.Hej.pattern.NodeTest getOperand(int which) {
            return ((which == 0) ? nodetest1 : nodetest2);
        }
        public override Saxon.Ejava.util.Optional<string> explainMismatch(Saxon.Hej.om.Item item, Saxon.Hej.type.TypeHierarchy th) {
            Saxon.Ejava.util.Optional<string> explanation = base.explainMismatch(item, th);
            if (explanation.isPresent()) {
                return explanation;
            }
            if ((@operator == Saxon.Hej.expr.parser.Token.INTERSECT)) {
                if (!(nodetest1.test((Saxon.Hej.om.NodeInfo)(item)))) {
                    return nodetest1.explainMismatch(item, th);
                } else {
                    if (!(nodetest2.test((Saxon.Hej.om.NodeInfo)(item)))) {
                        return nodetest2.explainMismatch(item, th);
                    }
                }
            }
            return Saxon.Ejava.util.Optional<string>.empty();
        }
    
    }
}