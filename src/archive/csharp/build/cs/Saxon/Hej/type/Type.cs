using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.type {
    public abstract class Type {
        public const short ELEMENT = 1;
        public const short ATTRIBUTE = 2;
        public const short TEXT = 3;
        public const short WHITESPACE_TEXT = 4;
        public const short PROCESSING_INSTRUCTION = 7;
        public const short COMMENT = 8;
        public const short DOCUMENT = 9;
        public const short NAMESPACE = 13;
        public const short STOPPER = 11;
        public const short PARENT_POINTER = 12;
        public const short TEXTUAL_ELEMENT = 17;
        public const short NODE = 0;
        public static readonly Saxon.Hej.type.ItemType NODE_TYPE = Saxon.Hej.pattern.AnyNodeTest.getInstance();
        public const short ITEM = 88;
        public static readonly Saxon.Hej.type.ItemType ITEM_TYPE = Saxon.Hej.type.AnyItemType.getInstance();
        public const short FUNCTION = 99;
        private Type()  {
        }
        public static bool isNodeType(Saxon.Hej.type.ItemType type) {
            return (type is Saxon.Hej.pattern.NodeTest);
        }
        public static Saxon.Hej.type.ItemType getItemType(Saxon.Hej.om.Item item, Saxon.Hej.type.TypeHierarchy th) {
            if ((item == null)) {
                return Saxon.Hej.type.AnyItemType.getInstance();
            } else {
                if ((item is Saxon.Hej.value.AtomicValue)) {
                    return ((Saxon.Hej.value.AtomicValue)(item)).getItemType();
                } else {
                    if ((item is Saxon.Hej.om.NodeInfo)) {
                        Saxon.Hej.om.NodeInfo node = (Saxon.Hej.om.NodeInfo)(item);
                        if ((th == null)) {
                            th = node.getConfiguration().getTypeHierarchy();
                        }
                        switch (node.getNodeKind()) {
                          case Saxon.Hej.type.Type.DOCUMENT:
                            Saxon.Hej.type.ItemType elementType = null;
                            foreach (Saxon.Hej.om.NodeInfo n in node.children()) {
                                int kind = n.getNodeKind();
                                if ((kind == Saxon.Hej.type.Type.TEXT)) {
                                    elementType = null;
                                    break ;
                                } else {
                                    if ((kind == Saxon.Hej.type.Type.ELEMENT)) {
                                        if ((elementType != null)) {
                                            elementType = null;
                                            break ;
                                        }
                                        elementType = Saxon.Hej.type.Type.getItemType(n, th);
                                    }
                                }
                            }
                            if ((elementType == null)) {
                                return Saxon.Hej.pattern.NodeKindTest.DOCUMENT;
                            } else {
                                return new Saxon.Hej.pattern.DocumentNodeTest((Saxon.Hej.pattern.NodeTest)(elementType));
                            }
                          case Saxon.Hej.type.Type.ELEMENT:
                            Saxon.Hej.type.SchemaType eltype = node.getSchemaType();
                            if ((eltype.Equals(Saxon.Hej.type.Untyped.getInstance()) || eltype.Equals(Saxon.Hej.type.AnyType.getInstance()))) {
                                return new Saxon.Hej.pattern.SameNameTest(node);
                            } else {
                                return new Saxon.Hej.pattern.CombinedNodeTest(new Saxon.Hej.pattern.SameNameTest(node), Saxon.Hej.expr.parser.Token.INTERSECT, new Saxon.Hej.pattern.ContentTypeTest(Saxon.Hej.type.Type.ELEMENT, eltype, node.getConfiguration(), false));
                            }
                          case Saxon.Hej.type.Type.ATTRIBUTE:
                            Saxon.Hej.type.SchemaType attype = node.getSchemaType();
                            if (attype.Equals(Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC)) {
                                return new Saxon.Hej.pattern.SameNameTest(node);
                            } else {
                                return new Saxon.Hej.pattern.CombinedNodeTest(new Saxon.Hej.pattern.SameNameTest(node), Saxon.Hej.expr.parser.Token.INTERSECT, new Saxon.Hej.pattern.ContentTypeTest(Saxon.Hej.type.Type.ATTRIBUTE, attype, node.getConfiguration(), false));
                            }
                          case Saxon.Hej.type.Type.TEXT:
                            return Saxon.Hej.pattern.NodeKindTest.TEXT;
                          case Saxon.Hej.type.Type.COMMENT:
                            return Saxon.Hej.pattern.NodeKindTest.COMMENT;
                          case Saxon.Hej.type.Type.PROCESSING_INSTRUCTION:
                            return Saxon.Hej.pattern.NodeKindTest.PROCESSING_INSTRUCTION;
                          case Saxon.Hej.type.Type.NAMESPACE:
                            return Saxon.Hej.pattern.NodeKindTest.NAMESPACE;
                          default:
                            throw new Saxon.Ejava.lang.IllegalArgumentException(("Unknown node kind "+node.getNodeKind()));
                        }
                    } else {
                        if ((item.getGenre() == Saxon.Hej.om.Genre.EXTERNAL)) {
                            if ((th == null)) {
                                throw new Saxon.Ejava.lang.IllegalArgumentException("typeHierarchy is required for an external object");
                            }
                            return ((Saxon.Hej.value.AnyExternalObject)(item)).getItemType(th);
                        } else {
                            if ((item is Saxon.Hej.ma.map.MapItem)) {
                                return ((th == null) ? Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE : ((Saxon.Hej.ma.map.MapItem)(item)).getItemType(th));
                            } else {
                                if ((item is Saxon.Hej.ma.arrays.ArrayItem)) {
                                    return ((th == null) ? Saxon.Hej.ma.arrays.ArrayItemType.ANY_ARRAY_TYPE : new Saxon.Hej.ma.arrays.ArrayItemType(((Saxon.Hej.ma.arrays.ArrayItem)(item)).getMemberType(th)));
                                } else {
                                    return ((Saxon.Hej.om.Function)(item)).getFunctionItemType();
                                }
                            }
                        }
                    }
                }
            }
        }
        public static string displayTypeName(Saxon.Hej.om.Item item) {
            if ((item is Saxon.Hej.om.NodeInfo)) {
                Saxon.Hej.om.NodeInfo node = (Saxon.Hej.om.NodeInfo)(item);
                switch (node.getNodeKind()) {
                  case DOCUMENT:
                    return "document-node()";
                  case ELEMENT:
                    Saxon.Hej.type.SchemaType annotation = node.getSchemaType();
                    return (((("element("+((Saxon.Hej.om.NodeInfo)(item)).getDisplayName())+", ")+annotation.getDisplayName())+')');
                  case ATTRIBUTE:
                    Saxon.Hej.type.SchemaType annotation2 = node.getSchemaType();
                    return (((("attribute("+((Saxon.Hej.om.NodeInfo)(item)).getDisplayName())+", ")+annotation2.getDisplayName())+')');
                  case TEXT:
                    return "text()";
                  case COMMENT:
                    return "comment()";
                  case PROCESSING_INSTRUCTION:
                    return "processing-instruction()";
                  case NAMESPACE:
                    return "namespace()";
                  default:
                    return "";
                }
            } else {
                if ((item is Saxon.Hej.value.AtomicValue)) {
                    return ((Saxon.Hej.value.AtomicValue)(item)).getItemType().ToString();
                } else {
                    if ((item is Saxon.Hej.om.Function)) {
                        return "function(*)";
                    } else {
                        return item.GetType().ToString();
                    }
                }
            }
        }
        public static Saxon.Hej.type.ItemType getBuiltInItemType(string @namespace, string localName) {
            Saxon.Hej.type.SchemaType t = Saxon.Hej.type.BuiltInType.getSchemaType(Saxon.Hej.om.StandardNames.getFingerprint(@namespace, localName));
            if ((t is Saxon.Hej.type.ItemType)) {
                return (Saxon.Hej.type.ItemType)(t);
            } else {
                return null;
            }
        }
        public static Saxon.Hej.type.SimpleType getBuiltInSimpleType(string @namespace, string localName) {
            Saxon.Hej.type.SchemaType t = Saxon.Hej.type.BuiltInType.getSchemaType(Saxon.Hej.om.StandardNames.getFingerprint(@namespace, localName));
            if (((t is Saxon.Hej.type.SimpleType) && ((Saxon.Hej.type.SimpleType)(t)).isBuiltInType())) {
                return (Saxon.Hej.type.SimpleType)(t);
            } else {
                return null;
            }
        }
        public static bool isSubType(Saxon.Hej.type.AtomicType one, Saxon.Hej.type.AtomicType two) {
            while (true) {
                if ((one.getFingerprint() == two.getFingerprint())) {
                    return true;
                }
                Saxon.Hej.type.SchemaType s = one.getBaseType();
                if ((s is Saxon.Hej.type.AtomicType)) {
                    one = (Saxon.Hej.type.AtomicType)(s);
                } else {
                    return false;
                }
            }
        }
        public static Saxon.Hej.type.ItemType getCommonSuperType(Saxon.Hej.type.ItemType t1, Saxon.Hej.type.ItemType t2, Saxon.Hej.type.TypeHierarchy th) {
            if ((t1 == t2)) {
                return t1;
            }
            if ((t1 is Saxon.Hej.type.ErrorType)) {
                return t2;
            }
            if ((t2 is Saxon.Hej.type.ErrorType)) {
                return t1;
            }
            if (((t1 is Saxon.Hej.ma.map.MapType) && (t2 is Saxon.Hej.ma.map.MapType))) {
                if ((t1 == Saxon.Hej.ma.map.MapType.EMPTY_MAP_TYPE)) {
                    return t2;
                }
                if ((t2 == Saxon.Hej.ma.map.MapType.EMPTY_MAP_TYPE)) {
                    return t1;
                }
                Saxon.Hej.type.ItemType keyType = getCommonSuperType(((Saxon.Hej.ma.map.MapType)(t1)).getKeyType(), ((Saxon.Hej.ma.map.MapType)(t2)).getKeyType());
                Saxon.Hej.type.AtomicType k;
                if ((keyType is Saxon.Hej.type.AtomicType)) {
                    k = (Saxon.Hej.type.AtomicType)(keyType);
                } else {
                    k = ((/*1430*/Saxon.Hej.type.AtomicType)keyType.getAtomizedItemType().getPrimitiveItemType());
                }
                Saxon.Hej.value.SequenceType v = Saxon.Hej.value.SequenceType.makeSequenceType(getCommonSuperType(((Saxon.Hej.ma.map.MapType)(t1)).getValueType().getPrimaryType(), ((Saxon.Hej.ma.map.MapType)(t2)).getValueType().getPrimaryType()), Saxon.Hej.value.Cardinality.union(((Saxon.Hej.ma.map.MapType)(t1)).getValueType().getCardinality(), ((Saxon.Hej.ma.map.MapType)(t2)).getValueType().getCardinality()));
                return new Saxon.Hej.ma.map.MapType(k, v);
            }
            Saxon.Hej.type.Affinity r = th.relationship(t1, t2);
            if ((r == Saxon.Hej.type.Affinity.SAME_TYPE)) {
                return t1;
            } else {
                if ((r == Saxon.Hej.type.Affinity.SUBSUMED_BY)) {
                    return t2;
                } else {
                    if ((r == Saxon.Hej.type.Affinity.SUBSUMES)) {
                        return t1;
                    } else {
                        return t1.getUType().union(t2.getUType()).toItemType();
                    }
                }
            }
        }
        public static Saxon.Hej.type.ItemType getCommonSuperType(Saxon.Hej.type.ItemType t1, Saxon.Hej.type.ItemType t2) {
            if ((t1 == t2)) {
                return t1;
            }
            if ((t1 is Saxon.Hej.type.ErrorType)) {
                return t2;
            }
            if ((t2 is Saxon.Hej.type.ErrorType)) {
                return t1;
            }
            if (((t1 == Saxon.Hej.type.AnyItemType.getInstance()) || (t2 == Saxon.Hej.type.AnyItemType.getInstance()))) {
                return Saxon.Hej.type.AnyItemType.getInstance();
            }
            Saxon.Hej.type.ItemType p1 = t1.getPrimitiveItemType();
            Saxon.Hej.type.ItemType p2 = t2.getPrimitiveItemType();
            if ((p1 == p2)) {
                return p1;
            }
            if (((((p1 == Saxon.Hej.type.BuiltInAtomicType.DECIMAL) && (p2 == Saxon.Hej.type.BuiltInAtomicType.INTEGER))) || (((p2 == Saxon.Hej.type.BuiltInAtomicType.DECIMAL) && (p1 == Saxon.Hej.type.BuiltInAtomicType.INTEGER))))) {
                return Saxon.Hej.type.BuiltInAtomicType.DECIMAL;
            }
            if (((((p1 is Saxon.Hej.type.BuiltInAtomicType) && ((Saxon.Hej.type.BuiltInAtomicType)(p1)).isNumericType()) && (p2 is Saxon.Hej.type.BuiltInAtomicType)) && ((Saxon.Hej.type.BuiltInAtomicType)(p2)).isNumericType())) {
                return Saxon.Hej.type.NumericType.getInstance();
            }
            if ((t1.isAtomicType() && t2.isAtomicType())) {
                return Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC;
            }
            if (((t1 is Saxon.Hej.pattern.NodeTest) && (t2 is Saxon.Hej.pattern.NodeTest))) {
                return Saxon.Hej.pattern.AnyNodeTest.getInstance();
            }
            return Saxon.Hej.type.AnyItemType.getInstance();
        }
        public static Saxon.Hej.value.SequenceType getCommonSuperType(Saxon.Hej.value.SequenceType t1, Saxon.Hej.value.SequenceType t2) {
            if (t1.Equals(t2)) {
                return t1;
            } else {
                return Saxon.Hej.value.SequenceType.makeSequenceType(getCommonSuperType(t1.getPrimaryType(), t2.getPrimaryType()), Saxon.Hej.value.Cardinality.union(t1.getCardinality(), t2.getCardinality()));
            }
        }
        public static bool isPrimitiveAtomicType(int fingerprint) {
            return ((fingerprint>=0) && ((((((((fingerprint<=Saxon.Hej.om.StandardNames.XS_INTEGER) || (fingerprint == Saxon.Hej.om.StandardNames.XS_NUMERIC)) || (fingerprint == Saxon.Hej.om.StandardNames.XS_UNTYPED_ATOMIC)) || (fingerprint == Saxon.Hej.om.StandardNames.XS_ANY_ATOMIC_TYPE)) || (fingerprint == Saxon.Hej.om.StandardNames.XS_DAY_TIME_DURATION)) || (fingerprint == Saxon.Hej.om.StandardNames.XS_YEAR_MONTH_DURATION)) || (fingerprint == Saxon.Hej.om.StandardNames.XS_ANY_SIMPLE_TYPE))));
        }
        public static bool isPrimitiveAtomicUType(int fingerprint) {
            return ((fingerprint>=0) && (fingerprint<=Saxon.Hej.om.StandardNames.XS_INTEGER));
        }
        public static bool isGuaranteedComparable(Saxon.Hej.type.BuiltInAtomicType t1, Saxon.Hej.type.BuiltInAtomicType t2, bool ordered) {
            if ((t1 == t2)) {
                return true;
            }
            if (t1.isPrimitiveNumeric()) {
                return t2.isPrimitiveNumeric();
            }
            if ((t1.Equals(Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC) || t1.Equals(Saxon.Hej.type.BuiltInAtomicType.ANY_URI))) {
                t1 = Saxon.Hej.type.BuiltInAtomicType.STRING;
            }
            if ((t2.Equals(Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC) || t2.Equals(Saxon.Hej.type.BuiltInAtomicType.ANY_URI))) {
                t2 = Saxon.Hej.type.BuiltInAtomicType.STRING;
            }
            if (!(ordered)) {
                if (t1.Equals(Saxon.Hej.type.BuiltInAtomicType.DAY_TIME_DURATION)) {
                    t1 = Saxon.Hej.type.BuiltInAtomicType.DURATION;
                }
                if (t2.Equals(Saxon.Hej.type.BuiltInAtomicType.DAY_TIME_DURATION)) {
                    t2 = Saxon.Hej.type.BuiltInAtomicType.DURATION;
                }
                if (t1.Equals(Saxon.Hej.type.BuiltInAtomicType.YEAR_MONTH_DURATION)) {
                    t1 = Saxon.Hej.type.BuiltInAtomicType.DURATION;
                }
                if (t2.Equals(Saxon.Hej.type.BuiltInAtomicType.YEAR_MONTH_DURATION)) {
                    t2 = Saxon.Hej.type.BuiltInAtomicType.DURATION;
                }
            }
            return (t1 == t2);
        }
        public static bool isPossiblyComparable(Saxon.Hej.type.BuiltInAtomicType t1, Saxon.Hej.type.BuiltInAtomicType t2, bool ordered) {
            if ((t1 == t2)) {
                return true;
            }
            if ((t1.Equals(Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC) || t2.Equals(Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC))) {
                return true;
            }
            if (t1.isPrimitiveNumeric()) {
                return t2.isPrimitiveNumeric();
            }
            if ((t1.Equals(Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC) || t1.Equals(Saxon.Hej.type.BuiltInAtomicType.ANY_URI))) {
                t1 = Saxon.Hej.type.BuiltInAtomicType.STRING;
            }
            if ((t2.Equals(Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC) || t2.Equals(Saxon.Hej.type.BuiltInAtomicType.ANY_URI))) {
                t2 = Saxon.Hej.type.BuiltInAtomicType.STRING;
            }
            if (t1.Equals(Saxon.Hej.type.BuiltInAtomicType.DAY_TIME_DURATION)) {
                t1 = Saxon.Hej.type.BuiltInAtomicType.DURATION;
            }
            if (t2.Equals(Saxon.Hej.type.BuiltInAtomicType.DAY_TIME_DURATION)) {
                t2 = Saxon.Hej.type.BuiltInAtomicType.DURATION;
            }
            if (t1.Equals(Saxon.Hej.type.BuiltInAtomicType.YEAR_MONTH_DURATION)) {
                t1 = Saxon.Hej.type.BuiltInAtomicType.DURATION;
            }
            if (t2.Equals(Saxon.Hej.type.BuiltInAtomicType.YEAR_MONTH_DURATION)) {
                t2 = Saxon.Hej.type.BuiltInAtomicType.DURATION;
            }
            return (t1 == t2);
        }
        public static bool isGenerallyComparable(Saxon.Hej.type.BuiltInAtomicType t1, Saxon.Hej.type.BuiltInAtomicType t2, bool ordered) {
            return ((((t1.Equals(Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC) || t2.Equals(Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC)) || t1.Equals(Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC)) || t2.Equals(Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC)) || isGuaranteedComparable(t1, t2, ordered));
        }
        public static bool isGuaranteedGenerallyComparable(Saxon.Hej.type.BuiltInAtomicType t1, Saxon.Hej.type.BuiltInAtomicType t2, bool ordered) {
            return (!(((t1.Equals(Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC) || t2.Equals(Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC)))) && isGenerallyComparable(t1, t2, ordered));
        }
    
    }
}