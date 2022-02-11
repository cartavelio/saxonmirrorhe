using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.pattern {
    public abstract class NodeTest : Saxon.Hej.pattern.NodePredicate, Saxon.Hej.type.ItemTypeWithSequenceTypeCache {
        private Saxon.Hej.value.SequenceType _one;
        private Saxon.Hej.value.SequenceType _oneOrMore;
        private Saxon.Hej.value.SequenceType _zeroOrOne;
        private Saxon.Hej.value.SequenceType _zeroOrMore;
        public virtual Saxon.Hej.om.Genre getGenre() {
            return Saxon.Hej.om.Genre.NODE;
        }
        public abstract double getDefaultPriority();
        public virtual bool matches(Saxon.Hej.om.Item item, Saxon.Hej.type.TypeHierarchy th) {
            return ((item is Saxon.Hej.om.NodeInfo) && test((Saxon.Hej.om.NodeInfo)(item)));
        }
        public virtual Saxon.Hej.type.ItemType getPrimitiveItemType() {
            int p = getPrimitiveType();
            if ((p == Saxon.Hej.type.Type.NODE)) {
                return Saxon.Hej.pattern.AnyNodeTest.getInstance();
            } else {
                return Saxon.Hej.pattern.NodeKindTest.makeNodeKindTest(p);
            }
        }
        public virtual int getPrimitiveType() {
            return Saxon.Hej.type.Type.NODE;
        }
        public virtual int getFingerprint() {
            return -1;
        }
        public virtual Saxon.Hej.om.StructuredQName getMatchingNodeName() {
            return null;
        }
        public virtual string getBasicAlphaCode() {
            switch (getPrimitiveType()) {
              case Saxon.Hej.type.Type.NODE:
                return "N";
              case Saxon.Hej.type.Type.ELEMENT:
                return "NE";
              case Saxon.Hej.type.Type.ATTRIBUTE:
                return "NA";
              case Saxon.Hej.type.Type.TEXT:
                return "NT";
              case Saxon.Hej.type.Type.COMMENT:
                return "NC";
              case Saxon.Hej.type.Type.PROCESSING_INSTRUCTION:
                return "NP";
              case Saxon.Hej.type.Type.DOCUMENT:
                return "ND";
              case Saxon.Hej.type.Type.NAMESPACE:
                return "NN";
              default:
                return "*";
            }
        }
        public virtual bool isAtomicType() {
            return false;
        }
        public virtual bool isPlainType() {
            return false;
        }
        public virtual Saxon.Hej.type.PlainType getAtomizedItemType() {
            return Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC;
        }
        public virtual bool isAtomizable(Saxon.Hej.type.TypeHierarchy th) {
            return true;
        }
        public virtual Saxon.Hej.z.IntPredicateProxy getMatcher(Saxon.Hej.tree.tiny.NodeVectorTree tree) {
            return Saxon.Hej.z.IntPredicateLambda.of(nodeNr => ((tree.getNodeKind(nodeNr) != Saxon.Hej.type.Type.PARENT_POINTER) && test(tree.getNode(nodeNr))));
        }
        public abstract bool matches(int nodeKind, Saxon.Hej.om.NodeName name, Saxon.Hej.type.SchemaType annotation);
        public virtual bool test(Saxon.Hej.om.NodeInfo node) {
            return matches(node.getNodeKind(), Saxon.Hej.om.NameOfNode.makeName(node), node.getSchemaType());
        }
        public virtual Saxon.Hej.type.SchemaType getContentType() {
            System.Collections.Generic.ISet<Saxon.Hej.type.PrimitiveUType> m = getUType().decompose();
            System.Collections.Generic.IEnumerator<Saxon.Hej.type.PrimitiveUType> it = m.GetEnumerator();
            if (((m.Count == 1) && it.MoveNext())) {
                Saxon.Hej.type.PrimitiveUType p = it.Current;
                switch (p.innerEnumValue) {
                  case Saxon.Hej.type.PrimitiveUType.InnerEnum.DOCUMENT:
                    return Saxon.Hej.type.AnyType.getInstance();
                  case Saxon.Hej.type.PrimitiveUType.InnerEnum.ELEMENT:
                    return Saxon.Hej.type.AnyType.getInstance();
                  case Saxon.Hej.type.PrimitiveUType.InnerEnum.ATTRIBUTE:
                    return Saxon.Hej.type.AnySimpleType.getInstance();
                  case Saxon.Hej.type.PrimitiveUType.InnerEnum.COMMENT:
                    return Saxon.Hej.type.BuiltInAtomicType.STRING;
                  case Saxon.Hej.type.PrimitiveUType.InnerEnum.TEXT:
                    return Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC;
                  case Saxon.Hej.type.PrimitiveUType.InnerEnum.PI:
                    return Saxon.Hej.type.BuiltInAtomicType.STRING;
                  case Saxon.Hej.type.PrimitiveUType.InnerEnum.NAMESPACE:
                    return Saxon.Hej.type.BuiltInAtomicType.STRING;
                }
            }
            return Saxon.Hej.type.AnyType.getInstance();
        }
        public virtual Saxon.Ejava.util.Optional<Saxon.Hej.z.IntSet> getRequiredNodeNames() {
            return Saxon.Ejava.util.Optional<Saxon.Hej.z.IntSet>.of(Saxon.Hej.z.IntUniversalSet.getInstance());
        }
        public virtual bool isNillable() {
            return true;
        }
        public Saxon.Hej.pattern.NodeTest copy() {
            return this;
        }
        public Saxon.Hej.value.SequenceType one() {
            if ((_one == null)) {
                _one = new Saxon.Hej.value.SequenceType(this, Saxon.Hej.expr.StaticProperty.EXACTLY_ONE);
            }
            return _one;
        }
        public Saxon.Hej.value.SequenceType zeroOrOne() {
            if ((_zeroOrOne == null)) {
                _zeroOrOne = new Saxon.Hej.value.SequenceType(this, Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_ONE);
            }
            return _zeroOrOne;
        }
        public Saxon.Hej.value.SequenceType oneOrMore() {
            if ((_oneOrMore == null)) {
                _oneOrMore = new Saxon.Hej.value.SequenceType(this, Saxon.Hej.expr.StaticProperty.ALLOWS_ONE_OR_MORE);
            }
            return _oneOrMore;
        }
        public Saxon.Hej.value.SequenceType zeroOrMore() {
            if ((_zeroOrMore == null)) {
                _zeroOrMore = new Saxon.Hej.value.SequenceType(this, Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE);
            }
            return _zeroOrMore;
        }
        public virtual Saxon.Ejava.util.Optional<string> explainMismatch(Saxon.Hej.om.Item item, Saxon.Hej.type.TypeHierarchy th) {
            if ((item is Saxon.Hej.om.NodeInfo)) {
                Saxon.Hej.type.UType actualKind = Saxon.Hej.type.UType.getUType(item);
                if (!(getUType().overlaps(actualKind))) {
                    return Saxon.Ejava.util.Optional<string>.of(("The supplied value is "+actualKind.toStringWithIndefiniteArticle()));
                }
                return Saxon.Ejava.util.Optional<string>.empty();
            } else {
                return Saxon.Ejava.util.Optional<string>.of(("The supplied value is "+Saxon.Hej.trans.Err.describeGenre(item.getGenre())));
            }
        }
        public virtual string toShortString() {
            return ToString();
        }
    
        /* Expanding default method getNormalizedDefaultPriority from interface ItemType */
        public virtual double getNormalizedDefaultPriority() {
            return (((getDefaultPriority()+1))/2);
        }

        /* Expanding default method getFullAlphaCode from interface ItemType */
        public virtual string getFullAlphaCode() {
            return getBasicAlphaCode();
        }

        /* Expanding default method toExportString from interface ItemType */
        public virtual string toExportString() {
            return ToString();
        }

        /* Creating stub method getUType to satisfy interface ItemType */
        public virtual Saxon.Hej.type.UType getUType() {
            throw new System.NotImplementedException();
        }

    }
}