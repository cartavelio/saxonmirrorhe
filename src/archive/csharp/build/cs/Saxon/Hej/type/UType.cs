using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.type {
    public class UType {
        public static readonly Saxon.Hej.type.UType VOID = new Saxon.Hej.type.UType(0);
        public static readonly Saxon.Hej.type.UType DOCUMENT = Saxon.Hej.type.PrimitiveUType.DOCUMENT.toUType();
        public static readonly Saxon.Hej.type.UType ELEMENT = Saxon.Hej.type.PrimitiveUType.ELEMENT.toUType();
        public static readonly Saxon.Hej.type.UType ATTRIBUTE = Saxon.Hej.type.PrimitiveUType.ATTRIBUTE.toUType();
        public static readonly Saxon.Hej.type.UType TEXT = Saxon.Hej.type.PrimitiveUType.TEXT.toUType();
        public static readonly Saxon.Hej.type.UType COMMENT = Saxon.Hej.type.PrimitiveUType.COMMENT.toUType();
        public static readonly Saxon.Hej.type.UType PI = Saxon.Hej.type.PrimitiveUType.PI.toUType();
        public static readonly Saxon.Hej.type.UType NAMESPACE = Saxon.Hej.type.PrimitiveUType.NAMESPACE.toUType();
        public static readonly Saxon.Hej.type.UType FUNCTION = Saxon.Hej.type.PrimitiveUType.FUNCTION.toUType();
        public static readonly Saxon.Hej.type.UType STRING = Saxon.Hej.type.PrimitiveUType.STRING.toUType();
        public static readonly Saxon.Hej.type.UType BOOLEAN = Saxon.Hej.type.PrimitiveUType.BOOLEAN.toUType();
        public static readonly Saxon.Hej.type.UType DECIMAL = Saxon.Hej.type.PrimitiveUType.DECIMAL.toUType();
        public static readonly Saxon.Hej.type.UType FLOAT = Saxon.Hej.type.PrimitiveUType.FLOAT.toUType();
        public static readonly Saxon.Hej.type.UType DOUBLE = Saxon.Hej.type.PrimitiveUType.DOUBLE.toUType();
        public static readonly Saxon.Hej.type.UType DURATION = Saxon.Hej.type.PrimitiveUType.DURATION.toUType();
        public static readonly Saxon.Hej.type.UType DATE_TIME = Saxon.Hej.type.PrimitiveUType.DATE_TIME.toUType();
        public static readonly Saxon.Hej.type.UType TIME = Saxon.Hej.type.PrimitiveUType.TIME.toUType();
        public static readonly Saxon.Hej.type.UType DATE = Saxon.Hej.type.PrimitiveUType.DATE.toUType();
        public static readonly Saxon.Hej.type.UType G_YEAR_MONTH = Saxon.Hej.type.PrimitiveUType.G_YEAR_MONTH.toUType();
        public static readonly Saxon.Hej.type.UType G_YEAR = Saxon.Hej.type.PrimitiveUType.G_YEAR.toUType();
        public static readonly Saxon.Hej.type.UType G_MONTH_DAY = Saxon.Hej.type.PrimitiveUType.G_MONTH_DAY.toUType();
        public static readonly Saxon.Hej.type.UType G_DAY = Saxon.Hej.type.PrimitiveUType.G_DAY.toUType();
        public static readonly Saxon.Hej.type.UType G_MONTH = Saxon.Hej.type.PrimitiveUType.G_MONTH.toUType();
        public static readonly Saxon.Hej.type.UType HEX_BINARY = Saxon.Hej.type.PrimitiveUType.HEX_BINARY.toUType();
        public static readonly Saxon.Hej.type.UType BASE64_BINARY = Saxon.Hej.type.PrimitiveUType.BASE64_BINARY.toUType();
        public static readonly Saxon.Hej.type.UType ANY_URI = Saxon.Hej.type.PrimitiveUType.ANY_URI.toUType();
        public static readonly Saxon.Hej.type.UType QNAME = Saxon.Hej.type.PrimitiveUType.QNAME.toUType();
        public static readonly Saxon.Hej.type.UType NOTATION = Saxon.Hej.type.PrimitiveUType.NOTATION.toUType();
        public static readonly Saxon.Hej.type.UType UNTYPED_ATOMIC = Saxon.Hej.type.PrimitiveUType.UNTYPED_ATOMIC.toUType();
        public static readonly Saxon.Hej.type.UType EXTENSION = Saxon.Hej.type.PrimitiveUType.EXTENSION.toUType();
        public static readonly Saxon.Hej.type.UType NUMERIC = DOUBLE.union(FLOAT).union(DECIMAL);
        public static readonly Saxon.Hej.type.UType STRING_LIKE = STRING.union(ANY_URI).union(UNTYPED_ATOMIC);
        public static readonly Saxon.Hej.type.UType CHILD_NODE_KINDS = ELEMENT.union(TEXT).union(COMMENT).union(PI);
        public static readonly Saxon.Hej.type.UType PARENT_NODE_KINDS = DOCUMENT.union(ELEMENT);
        public static readonly Saxon.Hej.type.UType ELEMENT_OR_ATTRIBUTE = ELEMENT.union(ATTRIBUTE);
        public static readonly Saxon.Hej.type.UType ANY_NODE = CHILD_NODE_KINDS.union(DOCUMENT).union(ATTRIBUTE).union(NAMESPACE);
        public static readonly Saxon.Hej.type.UType ANY_ATOMIC = new Saxon.Hej.type.UType(0x0FFFFF00);
        public static readonly Saxon.Hej.type.UType ANY = ANY_NODE.union(ANY_ATOMIC).union(FUNCTION).union(EXTENSION);
        private readonly int bits;
        public UType(int bits)  {
            this.bits = bits;
        }
        public override int GetHashCode() {
            return bits;
        }
        public override bool Equals(System.Object obj) {
            return ((obj is Saxon.Hej.type.UType) && (bits == ((Saxon.Hej.type.UType)(obj)).bits));
        }
        public Saxon.Hej.type.UType union(Saxon.Hej.type.UType other) {
            if ((other == null)) {
                System.Console.Error.WriteLine(new Saxon.Ejava.lang.NullPointerException());
            }
            return new Saxon.Hej.type.UType((bits|other.bits));
        }
        public Saxon.Hej.type.UType intersection(Saxon.Hej.type.UType other) {
            return new Saxon.Hej.type.UType((bits&other.bits));
        }
        public Saxon.Hej.type.UType except(Saxon.Hej.type.UType other) {
            return new Saxon.Hej.type.UType((bits&~(other.bits)));
        }
        public static Saxon.Hej.type.UType fromTypeCode(int code) {
            switch (code) {
              case Saxon.Hej.type.Type.NODE:
                return ANY_NODE;
              case Saxon.Hej.type.Type.ELEMENT:
                return ELEMENT;
              case Saxon.Hej.type.Type.ATTRIBUTE:
                return ATTRIBUTE;
              case Saxon.Hej.type.Type.TEXT:
              case Saxon.Hej.type.Type.WHITESPACE_TEXT:
                return TEXT;
              case Saxon.Hej.type.Type.DOCUMENT:
                return DOCUMENT;
              case Saxon.Hej.type.Type.COMMENT:
                return COMMENT;
              case Saxon.Hej.type.Type.PROCESSING_INSTRUCTION:
                return PI;
              case Saxon.Hej.type.Type.NAMESPACE:
                return NAMESPACE;
              case Saxon.Hej.type.Type.FUNCTION:
                return FUNCTION;
              case Saxon.Hej.type.Type.ITEM:
                return ANY;
              case Saxon.Hej.om.StandardNames.XS_ANY_ATOMIC_TYPE:
                return ANY_ATOMIC;
              case Saxon.Hej.om.StandardNames.XS_NUMERIC:
                return NUMERIC;
              case Saxon.Hej.om.StandardNames.XS_STRING:
                return STRING;
              case Saxon.Hej.om.StandardNames.XS_BOOLEAN:
                return BOOLEAN;
              case Saxon.Hej.om.StandardNames.XS_DURATION:
                return DURATION;
              case Saxon.Hej.om.StandardNames.XS_DATE_TIME:
                return DATE_TIME;
              case Saxon.Hej.om.StandardNames.XS_DATE:
                return DATE;
              case Saxon.Hej.om.StandardNames.XS_TIME:
                return TIME;
              case Saxon.Hej.om.StandardNames.XS_G_YEAR_MONTH:
                return G_YEAR_MONTH;
              case Saxon.Hej.om.StandardNames.XS_G_MONTH:
                return G_MONTH;
              case Saxon.Hej.om.StandardNames.XS_G_MONTH_DAY:
                return G_MONTH_DAY;
              case Saxon.Hej.om.StandardNames.XS_G_YEAR:
                return G_YEAR;
              case Saxon.Hej.om.StandardNames.XS_G_DAY:
                return G_DAY;
              case Saxon.Hej.om.StandardNames.XS_HEX_BINARY:
                return HEX_BINARY;
              case Saxon.Hej.om.StandardNames.XS_BASE64_BINARY:
                return BASE64_BINARY;
              case Saxon.Hej.om.StandardNames.XS_ANY_URI:
                return ANY_URI;
              case Saxon.Hej.om.StandardNames.XS_QNAME:
                return QNAME;
              case Saxon.Hej.om.StandardNames.XS_NOTATION:
                return NOTATION;
              case Saxon.Hej.om.StandardNames.XS_UNTYPED_ATOMIC:
                return UNTYPED_ATOMIC;
              case Saxon.Hej.om.StandardNames.XS_DECIMAL:
                return DECIMAL;
              case Saxon.Hej.om.StandardNames.XS_FLOAT:
                return FLOAT;
              case Saxon.Hej.om.StandardNames.XS_DOUBLE:
                return DOUBLE;
              case Saxon.Hej.om.StandardNames.XS_INTEGER:
                return DECIMAL;
              case Saxon.Hej.om.StandardNames.XS_NON_POSITIVE_INTEGER:
              case Saxon.Hej.om.StandardNames.XS_NEGATIVE_INTEGER:
              case Saxon.Hej.om.StandardNames.XS_LONG:
              case Saxon.Hej.om.StandardNames.XS_INT:
              case Saxon.Hej.om.StandardNames.XS_SHORT:
              case Saxon.Hej.om.StandardNames.XS_BYTE:
              case Saxon.Hej.om.StandardNames.XS_NON_NEGATIVE_INTEGER:
              case Saxon.Hej.om.StandardNames.XS_POSITIVE_INTEGER:
              case Saxon.Hej.om.StandardNames.XS_UNSIGNED_LONG:
              case Saxon.Hej.om.StandardNames.XS_UNSIGNED_INT:
              case Saxon.Hej.om.StandardNames.XS_UNSIGNED_SHORT:
              case Saxon.Hej.om.StandardNames.XS_UNSIGNED_BYTE:
                return DECIMAL;
              case Saxon.Hej.om.StandardNames.XS_YEAR_MONTH_DURATION:
              case Saxon.Hej.om.StandardNames.XS_DAY_TIME_DURATION:
                return DURATION;
              case Saxon.Hej.om.StandardNames.XS_DATE_TIME_STAMP:
                return DATE_TIME;
              case Saxon.Hej.om.StandardNames.XS_NORMALIZED_STRING:
              case Saxon.Hej.om.StandardNames.XS_TOKEN:
              case Saxon.Hej.om.StandardNames.XS_LANGUAGE:
              case Saxon.Hej.om.StandardNames.XS_NAME:
              case Saxon.Hej.om.StandardNames.XS_NMTOKEN:
              case Saxon.Hej.om.StandardNames.XS_NCNAME:
              case Saxon.Hej.om.StandardNames.XS_ID:
              case Saxon.Hej.om.StandardNames.XS_IDREF:
              case Saxon.Hej.om.StandardNames.XS_ENTITY:
                return STRING;
              default:
                throw new Saxon.Ejava.lang.IllegalArgumentException((""+code));
            }
        }
        public System.Collections.Generic.ISet<Saxon.Hej.type.PrimitiveUType> decompose() {
            System.Collections.Generic.ISet<Saxon.Hej.type.PrimitiveUType> result = new System.Collections.Generic.HashSet<Saxon.Hej.type.PrimitiveUType>();
            foreach (Saxon.Hej.type.PrimitiveUType p in Saxon.Hej.type.PrimitiveUType.values()) {
                if ((((bits&((1<<p.getBit())))) != 0)) {
                    result.Add(p);
                }
            }
            return result;
        }
        public override string ToString() {
            System.Collections.Generic.ISet<Saxon.Hej.type.PrimitiveUType> components = decompose();
            if (components.Count==0) {
                return "U{}";
            }
            Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder(256);
            System.Collections.Generic.IEnumerator<Saxon.Hej.type.PrimitiveUType> iter = components.GetEnumerator();
            bool started = false;
            while (iter.MoveNext() /*UNSAFE CONVERSION*/) {
                if (started) {
                    sb.append("|");
                }
                started = true;
                sb.append(iter.Current.ToString());
            }
            return sb.ToString();
        }
        public string toStringWithIndefiniteArticle() {
            string s = ToString();
            if (("aeiouxy".IndexOf(s[0])>=0)) {
                return (("an "+s)+" node");
            } else {
                return (("a "+s)+" node");
            }
        }
        public bool overlaps(Saxon.Hej.type.UType other) {
            return (((bits&other.bits)) != 0);
        }
        public bool subsumes(Saxon.Hej.type.UType other) {
            return (((bits&other.bits)) == other.bits);
        }
        public Saxon.Hej.type.ItemType toItemType() {
            System.Collections.Generic.ISet<Saxon.Hej.type.PrimitiveUType> p = decompose();
            if (p.Count==0) {
                return Saxon.Hej.type.ErrorType.getInstance();
            } else {
                if ((p.Count == 1)) {
                    System.Collections.Generic.IEnumerator<Saxon.Hej.type.PrimitiveUType> iter = p.GetEnumerator();
                    return (iter.MoveNext() /*UNSAFE CONVERSION*/ ? iter.Current.toItemType() : null);
                } else {
                    if (ANY_NODE.subsumes(this)) {
                        return Saxon.Hej.pattern.AnyNodeTest.getInstance();
                    } else {
                        if (Equals(NUMERIC)) {
                            return Saxon.Hej.type.NumericType.getInstance();
                        } else {
                            if (ANY_ATOMIC.subsumes(this)) {
                                return Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC;
                            } else {
                                return Saxon.Hej.type.AnyItemType.getInstance();
                            }
                        }
                    }
                }
            }
        }
        public bool matches(Saxon.Hej.om.Item item) {
            return subsumes(getUType(item));
        }
        public static Saxon.Hej.type.UType getUType(Saxon.Hej.om.Item item) {
            if ((item is Saxon.Hej.om.NodeInfo)) {
                return fromTypeCode(((Saxon.Hej.om.NodeInfo)(item)).getNodeKind());
            } else {
                if ((item is Saxon.Hej.value.AtomicValue)) {
                    return ((Saxon.Hej.value.AtomicValue)(item)).getUType();
                } else {
                    if ((item is Saxon.Hej.om.Function)) {
                        return Saxon.Hej.type.UType.FUNCTION;
                    } else {
                        if ((item.getGenre() == Saxon.Hej.om.Genre.EXTERNAL)) {
                            return Saxon.Hej.type.UType.EXTENSION;
                        } else {
                            return Saxon.Hej.type.UType.VOID;
                        }
                    }
                }
            }
        }
        public static Saxon.Hej.type.UType getUType(Saxon.Hej.om.GroundedValue sequence) {
            Saxon.Hej.om.SequenceIterator iter = sequence.iterate();
            Saxon.Hej.om.Item item;
            Saxon.Hej.type.UType u = Saxon.Hej.type.UType.VOID;
            while (((item = iter.next()) != null)) {
                u = u.union(getUType(item));
            }
            return u;
        }
        public static bool isPossiblyComparable(Saxon.Hej.type.UType t1, Saxon.Hej.type.UType t2, bool ordered) {
            if ((t1 == t2)) {
                return true;
            }
            if (((t1 == Saxon.Hej.type.UType.ANY_ATOMIC) || (t2 == Saxon.Hej.type.UType.ANY_ATOMIC))) {
                return true;
            }
            if (((t1 == Saxon.Hej.type.UType.UNTYPED_ATOMIC) || (t1 == Saxon.Hej.type.UType.ANY_URI))) {
                t1 = Saxon.Hej.type.UType.STRING;
            }
            if (((t2 == Saxon.Hej.type.UType.UNTYPED_ATOMIC) || (t2 == Saxon.Hej.type.UType.ANY_URI))) {
                t2 = Saxon.Hej.type.UType.STRING;
            }
            if (NUMERIC.subsumes(t1)) {
                t1 = NUMERIC;
            }
            if (NUMERIC.subsumes(t2)) {
                t2 = NUMERIC;
            }
            return (t1 == t2);
        }
        public static bool isGuaranteedComparable(Saxon.Hej.type.UType t1, Saxon.Hej.type.UType t2) {
            if ((t1 == t2)) {
                return true;
            }
            if (((t1 == Saxon.Hej.type.UType.UNTYPED_ATOMIC) || (t1 == Saxon.Hej.type.UType.ANY_URI))) {
                t1 = Saxon.Hej.type.UType.STRING;
            }
            if (((t2 == Saxon.Hej.type.UType.UNTYPED_ATOMIC) || (t2 == Saxon.Hej.type.UType.ANY_URI))) {
                t2 = Saxon.Hej.type.UType.STRING;
            }
            if (NUMERIC.subsumes(t1)) {
                t1 = NUMERIC;
            }
            if (NUMERIC.subsumes(t2)) {
                t2 = NUMERIC;
            }
            return t1.Equals(t2);
        }
        public static bool isGenerallyComparable(Saxon.Hej.type.UType t1, Saxon.Hej.type.UType t2) {
            return (((t1 == Saxon.Hej.type.UType.UNTYPED_ATOMIC) || (t2 == Saxon.Hej.type.UType.UNTYPED_ATOMIC)) || isGuaranteedComparable(t1, t2));
        }
    
    }
}