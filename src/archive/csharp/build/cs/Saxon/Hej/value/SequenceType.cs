using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.value {
    public sealed class SequenceType {
        private readonly Saxon.Hej.type.ItemType primaryType;
        private readonly int cardinality;
        public static readonly Saxon.Hej.value.SequenceType ANY_SEQUENCE = Saxon.Hej.type.AnyItemType.getInstance().zeroOrMore();
        public static readonly Saxon.Hej.value.SequenceType SINGLE_ITEM = Saxon.Hej.type.AnyItemType.getInstance().one();
        public static readonly Saxon.Hej.value.SequenceType OPTIONAL_ITEM = Saxon.Hej.type.AnyItemType.getInstance().zeroOrOne();
        public static readonly Saxon.Hej.value.SequenceType SINGLE_ATOMIC = Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC.one();
        public static readonly Saxon.Hej.value.SequenceType OPTIONAL_ATOMIC = Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC.zeroOrOne();
        public static readonly Saxon.Hej.value.SequenceType ATOMIC_SEQUENCE = Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC.zeroOrMore();
        public static readonly Saxon.Hej.value.SequenceType SINGLE_STRING = Saxon.Hej.type.BuiltInAtomicType.STRING.one();
        public static readonly Saxon.Hej.value.SequenceType SINGLE_UNTYPED_ATOMIC = Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC.one();
        public static readonly Saxon.Hej.value.SequenceType OPTIONAL_STRING = Saxon.Hej.type.BuiltInAtomicType.STRING.zeroOrOne();
        public static readonly Saxon.Hej.value.SequenceType SINGLE_BOOLEAN = Saxon.Hej.type.BuiltInAtomicType.BOOLEAN.one();
        public static readonly Saxon.Hej.value.SequenceType OPTIONAL_BOOLEAN = Saxon.Hej.type.BuiltInAtomicType.BOOLEAN.zeroOrOne();
        public static readonly Saxon.Hej.value.SequenceType SINGLE_INTEGER = Saxon.Hej.type.BuiltInAtomicType.INTEGER.one();
        public static readonly Saxon.Hej.value.SequenceType SINGLE_DECIMAL = Saxon.Hej.type.BuiltInAtomicType.DECIMAL.one();
        public static readonly Saxon.Hej.value.SequenceType OPTIONAL_INTEGER = Saxon.Hej.type.BuiltInAtomicType.INTEGER.zeroOrOne();
        public static readonly Saxon.Hej.value.SequenceType SINGLE_SHORT = Saxon.Hej.type.BuiltInAtomicType.SHORT.one();
        public static readonly Saxon.Hej.value.SequenceType OPTIONAL_SHORT = Saxon.Hej.type.BuiltInAtomicType.SHORT.zeroOrOne();
        public static readonly Saxon.Hej.value.SequenceType SINGLE_BYTE = Saxon.Hej.type.BuiltInAtomicType.BYTE.one();
        public static readonly Saxon.Hej.value.SequenceType OPTIONAL_BYTE = Saxon.Hej.type.BuiltInAtomicType.BYTE.zeroOrOne();
        public static readonly Saxon.Hej.value.SequenceType SINGLE_DOUBLE = Saxon.Hej.type.BuiltInAtomicType.DOUBLE.one();
        public static readonly Saxon.Hej.value.SequenceType OPTIONAL_DOUBLE = Saxon.Hej.type.BuiltInAtomicType.DOUBLE.zeroOrOne();
        public static readonly Saxon.Hej.value.SequenceType SINGLE_FLOAT = Saxon.Hej.type.BuiltInAtomicType.FLOAT.one();
        public static readonly Saxon.Hej.value.SequenceType OPTIONAL_FLOAT = Saxon.Hej.type.BuiltInAtomicType.FLOAT.zeroOrOne();
        public static readonly Saxon.Hej.value.SequenceType OPTIONAL_DECIMAL = Saxon.Hej.type.BuiltInAtomicType.DECIMAL.zeroOrOne();
        public static readonly Saxon.Hej.value.SequenceType OPTIONAL_ANY_URI = Saxon.Hej.type.BuiltInAtomicType.ANY_URI.zeroOrOne();
        public static readonly Saxon.Hej.value.SequenceType OPTIONAL_DATE = Saxon.Hej.type.BuiltInAtomicType.DATE.zeroOrOne();
        public static readonly Saxon.Hej.value.SequenceType OPTIONAL_TIME = Saxon.Hej.type.BuiltInAtomicType.TIME.zeroOrOne();
        public static readonly Saxon.Hej.value.SequenceType OPTIONAL_G_YEAR = Saxon.Hej.type.BuiltInAtomicType.G_YEAR.zeroOrOne();
        public static readonly Saxon.Hej.value.SequenceType OPTIONAL_G_YEAR_MONTH = Saxon.Hej.type.BuiltInAtomicType.G_YEAR_MONTH.zeroOrOne();
        public static readonly Saxon.Hej.value.SequenceType OPTIONAL_G_MONTH = Saxon.Hej.type.BuiltInAtomicType.G_MONTH.zeroOrOne();
        public static readonly Saxon.Hej.value.SequenceType OPTIONAL_G_MONTH_DAY = Saxon.Hej.type.BuiltInAtomicType.G_MONTH_DAY.zeroOrOne();
        public static readonly Saxon.Hej.value.SequenceType OPTIONAL_G_DAY = Saxon.Hej.type.BuiltInAtomicType.G_DAY.zeroOrOne();
        public static readonly Saxon.Hej.value.SequenceType OPTIONAL_DATE_TIME = Saxon.Hej.type.BuiltInAtomicType.DATE_TIME.zeroOrOne();
        public static readonly Saxon.Hej.value.SequenceType OPTIONAL_DURATION = Saxon.Hej.type.BuiltInAtomicType.DURATION.zeroOrOne();
        public static readonly Saxon.Hej.value.SequenceType OPTIONAL_YEAR_MONTH_DURATION = Saxon.Hej.type.BuiltInAtomicType.YEAR_MONTH_DURATION.zeroOrOne();
        public static readonly Saxon.Hej.value.SequenceType OPTIONAL_DAY_TIME_DURATION = Saxon.Hej.type.BuiltInAtomicType.DAY_TIME_DURATION.zeroOrOne();
        public static readonly Saxon.Hej.value.SequenceType SINGLE_QNAME = Saxon.Hej.type.BuiltInAtomicType.QNAME.one();
        public static readonly Saxon.Hej.value.SequenceType OPTIONAL_QNAME = Saxon.Hej.type.BuiltInAtomicType.QNAME.zeroOrOne();
        public static readonly Saxon.Hej.value.SequenceType OPTIONAL_NOTATION = Saxon.Hej.type.BuiltInAtomicType.NOTATION.zeroOrOne();
        public static readonly Saxon.Hej.value.SequenceType OPTIONAL_BASE64_BINARY = Saxon.Hej.type.BuiltInAtomicType.BASE64_BINARY.zeroOrOne();
        public static readonly Saxon.Hej.value.SequenceType OPTIONAL_HEX_BINARY = Saxon.Hej.type.BuiltInAtomicType.HEX_BINARY.zeroOrOne();
        public static readonly Saxon.Hej.value.SequenceType OPTIONAL_NUMERIC = makeSequenceType(Saxon.Hej.type.NumericType.getInstance(), Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_ONE);
        public static readonly Saxon.Hej.value.SequenceType SINGLE_NUMERIC = makeSequenceType(Saxon.Hej.type.NumericType.getInstance(), Saxon.Hej.expr.StaticProperty.EXACTLY_ONE);
        public static readonly Saxon.Hej.value.SequenceType OPTIONAL_NODE = Saxon.Hej.pattern.AnyNodeTest.getInstance().zeroOrOne();
        public static readonly Saxon.Hej.value.SequenceType SINGLE_NODE = Saxon.Hej.pattern.AnyNodeTest.getInstance().one();
        public static readonly Saxon.Hej.value.SequenceType OPTIONAL_DOCUMENT_NODE = Saxon.Hej.pattern.NodeKindTest.DOCUMENT.zeroOrOne();
        public static readonly Saxon.Hej.value.SequenceType NODE_SEQUENCE = Saxon.Hej.pattern.AnyNodeTest.getInstance().zeroOrMore();
        public static readonly Saxon.Hej.value.SequenceType STRING_SEQUENCE = Saxon.Hej.type.BuiltInAtomicType.STRING.zeroOrMore();
        public static readonly Saxon.Hej.value.SequenceType SINGLE_FUNCTION = makeSequenceType(Saxon.Hej.type.AnyFunctionType.ANY_FUNCTION, Saxon.Hej.expr.StaticProperty.EXACTLY_ONE);
        public static readonly Saxon.Hej.value.SequenceType OPTIONAL_FUNCTION_ITEM = makeSequenceType(Saxon.Hej.type.AnyFunctionType.getInstance(), Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_ONE);
        public static readonly Saxon.Hej.value.SequenceType FUNCTION_ITEM_SEQUENCE = makeSequenceType(Saxon.Hej.type.AnyFunctionType.getInstance(), Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE);
        public static readonly Saxon.Hej.value.SequenceType EMPTY_SEQUENCE = new Saxon.Hej.value.SequenceType(Saxon.Hej.type.ErrorType.getInstance(), Saxon.Hej.expr.StaticProperty.EMPTY);
        public static readonly Saxon.Hej.value.SequenceType NON_EMPTY_SEQUENCE = makeSequenceType(Saxon.Hej.type.AnyItemType.getInstance(), Saxon.Hej.expr.StaticProperty.ALLOWS_ONE_OR_MORE);
        public static readonly Saxon.Hej.value.SequenceType VOID = makeSequenceType(Saxon.Hej.type.ErrorType.getInstance(), Saxon.Hej.expr.StaticProperty.ALLOWS_MANY);
        public SequenceType(Saxon.Hej.type.ItemType primaryType, int cardinality)  {
            this.primaryType = primaryType;
            if (((primaryType is Saxon.Hej.type.ErrorType) && Saxon.Hej.value.Cardinality.allowsZero(cardinality))) {
                this.cardinality = Saxon.Hej.expr.StaticProperty.EMPTY;
            } else {
                this.cardinality = cardinality;
            }
        }
        public static Saxon.Hej.value.SequenceType makeSequenceType(Saxon.Hej.type.ItemType primaryType, int cardinality) {
            if ((primaryType is Saxon.Hej.type.ItemTypeWithSequenceTypeCache)) {
                Saxon.Hej.type.ItemTypeWithSequenceTypeCache bat = (Saxon.Hej.type.ItemTypeWithSequenceTypeCache)(primaryType);
                switch (cardinality) {
                  case Saxon.Hej.expr.StaticProperty.EXACTLY_ONE:
                    return bat.one();
                  case Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_ONE:
                    return bat.zeroOrOne();
                  case Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE:
                    return bat.zeroOrMore();
                  case Saxon.Hej.expr.StaticProperty.ALLOWS_ONE_OR_MORE:
                    return bat.oneOrMore();
                  default:
                    break ;
                }
            }
            if ((cardinality == Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO)) {
                return Saxon.Hej.value.SequenceType.EMPTY_SEQUENCE;
            }
            return new Saxon.Hej.value.SequenceType(primaryType, cardinality);
        }
        public Saxon.Hej.type.ItemType getPrimaryType() {
            return primaryType;
        }
        public int getCardinality() {
            return cardinality;
        }
        public bool matches(Saxon.Hej.om.GroundedValue value, Saxon.Hej.type.TypeHierarchy th) {
            int count = 0;
            Saxon.Hej.om.SequenceIterator iter = value.iterate();
            Saxon.Hej.om.Item item;
            while (((item = iter.next()) != null)) {
                count++;
                if (!(primaryType.matches(item, th))) {
                    return false;
                }
            }
            return !(((((count == 0) && !(Saxon.Hej.value.Cardinality.allowsZero(cardinality))) || ((count>1) && !(Saxon.Hej.value.Cardinality.allowsMany(cardinality))))));
        }
        public Saxon.Ejava.util.Optional<string> explainMismatch(Saxon.Hej.om.GroundedValue value, Saxon.Hej.type.TypeHierarchy th) {
            int count = 0;
            Saxon.Hej.om.SequenceIterator iter = value.iterate();
            Saxon.Hej.om.Item item;
            while (((item = iter.next()) != null)) {
                count++;
                if (!(primaryType.matches(item, th))) {
                    string s = (("The "+Saxon.Hej.expr.parser.RoleDiagnostic.ordinal(count))+" item is not an instance of the required type");
                    Saxon.Ejava.util.Optional<string> more = primaryType.explainMismatch(item, th);
                    if (more.isPresent()) {
                        s = ((count == 1) ? more.get() : ((s+". ")+more.get()));
                    } else {
                        if ((count == 1)) {
                            return Saxon.Ejava.util.Optional<string>.empty();
                        }
                    }
                    return Saxon.Ejava.util.Optional<string>.of(s);
                }
            }
            if (((count == 0) && !(Saxon.Hej.value.Cardinality.allowsZero(cardinality)))) {
                return Saxon.Ejava.util.Optional<string>.of("The type does not allow an empty sequence");
            } else {
                if (((count>1) && !(Saxon.Hej.value.Cardinality.allowsMany(cardinality)))) {
                    return Saxon.Ejava.util.Optional<string>.of("The type does not allow a sequence of more than one item");
                }
            }
            return Saxon.Ejava.util.Optional<string>.empty();
        }
        public override string ToString() {
            if ((cardinality == Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO)) {
                return "empty-sequence()";
            } else {
                return (primaryType+Saxon.Hej.value.Cardinality.getOccurrenceIndicator(cardinality));
            }
        }
        public string toExportString() {
            if ((cardinality == Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO)) {
                return "empty-sequence()";
            } else {
                return (primaryType.toExportString()+Saxon.Hej.value.Cardinality.getOccurrenceIndicator(cardinality));
            }
        }
        public string toAlphaCode() {
            return Saxon.Hej.type.AlphaCode.fromSequenceType(this);
        }
        public override int GetHashCode() {
            return (primaryType.GetHashCode()^cardinality);
        }
        public override bool Equals(System.Object obj) {
            return (((obj is Saxon.Hej.value.SequenceType) && this.primaryType.Equals(((Saxon.Hej.value.SequenceType)(obj)).primaryType)) && (this.cardinality == ((Saxon.Hej.value.SequenceType)(obj)).cardinality));
        }
        public bool isSameType(Saxon.Hej.value.SequenceType other, Saxon.Hej.type.TypeHierarchy th) {
            return ((cardinality == other.cardinality) && (th.relationship(primaryType, other.primaryType) == Saxon.Hej.type.Affinity.SAME_TYPE));
        }
    
    }
}