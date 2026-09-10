using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.ma.map {
    public class MapType : Saxon.Hej.type.AnyFunctionType {
        public readonly static Saxon.Hej.ma.map.MapType ANY_MAP_TYPE = new Saxon.Hej.ma.map.MapType(Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC, Saxon.Hej.value.SequenceType.ANY_SEQUENCE);
        public readonly static Saxon.Hej.ma.map.MapType EMPTY_MAP_TYPE = new Saxon.Hej.ma.map.MapType(Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC, Saxon.Hej.value.SequenceType.ANY_SEQUENCE, true);
        public static readonly Saxon.Hej.value.SequenceType OPTIONAL_MAP_ITEM = Saxon.Hej.value.SequenceType.makeSequenceType(ANY_MAP_TYPE, Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_ONE);
        public static readonly Saxon.Hej.value.SequenceType SINGLE_MAP_ITEM = Saxon.Hej.value.SequenceType.makeSequenceType(ANY_MAP_TYPE, Saxon.Hej.expr.StaticProperty.ALLOWS_ONE);
        public static readonly Saxon.Hej.value.SequenceType SEQUENCE_OF_MAPS = Saxon.Hej.value.SequenceType.makeSequenceType(ANY_MAP_TYPE, Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE);
        private readonly Saxon.Hej.type.AtomicType keyType;
        private readonly Saxon.Hej.value.SequenceType valueType;
        private readonly bool mustBeEmpty;
        public MapType(Saxon.Hej.type.AtomicType keyType, Saxon.Hej.value.SequenceType valueType)  {
            this.keyType = keyType;
            this.valueType = valueType;
            this.mustBeEmpty = false;
        }
        public MapType(Saxon.Hej.type.AtomicType keyType, Saxon.Hej.value.SequenceType valueType, bool mustBeEmpty)  {
            this.keyType = keyType;
            this.valueType = valueType;
            this.mustBeEmpty = mustBeEmpty;
        }
        public override  /* REPLACING public */ Saxon.Hej.om.Genre getGenre() {
            return Saxon.Hej.om.Genre.MAP;
        }
        public Saxon.Hej.type.AtomicType getKeyType() {
            return keyType;
        }
        public Saxon.Hej.value.SequenceType getValueType() {
            return valueType;
        }
        public override bool isMapType() {
            return true;
        }
        public override bool isArrayType() {
            return false;
        }
        public override string getBasicAlphaCode() {
            return "FM";
        }
        public override bool isAtomizable(Saxon.Hej.type.TypeHierarchy th) {
            return false;
        }
        public override double getDefaultPriority() {
            return (keyType.getNormalizedDefaultPriority()*valueType.getPrimaryType().getNormalizedDefaultPriority());
        }
        public override bool matches(Saxon.Hej.om.Item item, Saxon.Hej.type.TypeHierarchy th) {
            if (!(((item is Saxon.Hej.ma.map.MapItem)))) {
                return false;
            }
            if (((Saxon.Hej.ma.map.MapItem)(item)).isEmpty()) {
                return true;
            } else {
                if (mustBeEmpty) {
                    return false;
                }
            }
            if ((this == ANY_MAP_TYPE)) {
                return true;
            } else {
                return ((Saxon.Hej.ma.map.MapItem)(item)).conforms(keyType, valueType, th);
            }
        }
        public int getArity() {
            return 1;
        }
        public override Saxon.Hej.value.SequenceType[] getArgumentTypes() {
            return new Saxon.Hej.value.SequenceType[]{Saxon.Hej.value.SequenceType.makeSequenceType(Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC, Saxon.Hej.expr.StaticProperty.EXACTLY_ONE)};
        }
        public override Saxon.Hej.value.SequenceType getResultType() {
            if (Saxon.Hej.value.Cardinality.allowsZero(valueType.getCardinality())) {
                return valueType;
            } else {
                return Saxon.Hej.value.SequenceType.makeSequenceType(valueType.getPrimaryType(), Saxon.Hej.value.Cardinality.union(valueType.getCardinality(), Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO));
            }
        }
        public override string ToString() {
            if ((this == ANY_MAP_TYPE)) {
                return "map(*)";
            } else {
                if ((this == EMPTY_MAP_TYPE)) {
                    return "map{}";
                } else {
                    Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder(100);
                    sb.append("map(");
                    sb.append(keyType.ToString());
                    sb.append(", ");
                    sb.append(valueType.ToString());
                    sb.append(")");
                    return sb.ToString();
                }
            }
        }
        public override  /* REPLACING public */ string toExportString() {
            if ((this == ANY_MAP_TYPE)) {
                return "map(*)";
            } else {
                if ((this == EMPTY_MAP_TYPE)) {
                    return "map{}";
                } else {
                    Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder(100);
                    sb.append("map(");
                    sb.append(keyType.toExportString());
                    sb.append(", ");
                    sb.append(valueType.toExportString());
                    sb.append(")");
                    return sb.ToString();
                }
            }
        }
        public override bool Equals(System.Object other) {
            if ((this == other)) {
                return true;
            }
            if ((other is Saxon.Hej.ma.map.MapType)) {
                Saxon.Hej.ma.map.MapType f2 = (Saxon.Hej.ma.map.MapType)(other);
                return ((keyType.Equals(f2.keyType) && valueType.Equals(f2.valueType)) && (mustBeEmpty == f2.mustBeEmpty));
            }
            return false;
        }
        public override int GetHashCode() {
            return (keyType.GetHashCode()^valueType.GetHashCode());
        }
        public override Saxon.Hej.type.Affinity relationship(Saxon.Hej.type.FunctionItemType other, Saxon.Hej.type.TypeHierarchy th) {
            if ((other == Saxon.Hej.type.AnyFunctionType.getInstance())) {
                return Saxon.Hej.type.Affinity.SUBSUMED_BY;
            } else {
                if (Equals(other)) {
                    return Saxon.Hej.type.Affinity.SAME_TYPE;
                } else {
                    if ((other == Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE)) {
                        return Saxon.Hej.type.Affinity.SUBSUMED_BY;
                    } else {
                        if (other.isArrayType()) {
                            return Saxon.Hej.type.Affinity.DISJOINT;
                        } else {
                            if ((other is Saxon.Hej.ma.map.RecordTest)) {
                                return Saxon.Hej.type.TypeHierarchy.inverseRelationship(other.relationship(this, th));
                            } else {
                                if ((other is Saxon.Hej.ma.map.MapType)) {
                                    Saxon.Hej.ma.map.MapType f2 = (Saxon.Hej.ma.map.MapType)(other);
                                    Saxon.Hej.type.Affinity keyRel = th.relationship(keyType, f2.keyType);
                                    if ((keyRel == Saxon.Hej.type.Affinity.DISJOINT)) {
                                        return Saxon.Hej.type.Affinity.OVERLAPS;
                                    }
                                    Saxon.Hej.type.Affinity valueRel = th.sequenceTypeRelationship(valueType, f2.valueType);
                                    if ((valueRel == Saxon.Hej.type.Affinity.DISJOINT)) {
                                        return Saxon.Hej.type.Affinity.OVERLAPS;
                                    }
                                    if ((keyRel == valueRel)) {
                                        return keyRel;
                                    }
                                    if (((((keyRel == Saxon.Hej.type.Affinity.SAME_TYPE) || (keyRel == Saxon.Hej.type.Affinity.SUBSUMES))) && (((valueRel == Saxon.Hej.type.Affinity.SAME_TYPE) || (valueRel == Saxon.Hej.type.Affinity.SUBSUMES))))) {
                                        return Saxon.Hej.type.Affinity.SUBSUMES;
                                    }
                                    if (((((keyRel == Saxon.Hej.type.Affinity.SAME_TYPE) || (keyRel == Saxon.Hej.type.Affinity.SUBSUMED_BY))) && (((valueRel == Saxon.Hej.type.Affinity.SAME_TYPE) || (valueRel == Saxon.Hej.type.Affinity.SUBSUMED_BY))))) {
                                        return Saxon.Hej.type.Affinity.SUBSUMED_BY;
                                    }
                                    return Saxon.Hej.type.Affinity.OVERLAPS;
                                } else {
                                    Saxon.Hej.value.SequenceType st = getResultType();
                                    if (!(Saxon.Hej.value.Cardinality.allowsZero(st.getCardinality()))) {
                                        st = Saxon.Hej.value.SequenceType.makeSequenceType(st.getPrimaryType(), Saxon.Hej.value.Cardinality.union(st.getCardinality(), Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO));
                                    }
                                    return new Saxon.Hej.type.SpecificFunctionType(new Saxon.Hej.value.SequenceType[]{Saxon.Hej.value.SequenceType.ATOMIC_SEQUENCE}, st).relationship(other, th);
                                }
                            }
                        }
                    }
                }
            }
        }
        public override  /* REPLACING public */ Saxon.Ejava.util.Optional<string> explainMismatch(Saxon.Hej.om.Item item, Saxon.Hej.type.TypeHierarchy th) {
            if ((item is Saxon.Hej.ma.map.MapItem)) {
                foreach (Saxon.Hej.ma.map.KeyValuePair kvp in ((Saxon.Hej.ma.map.MapItem)(item)).keyValuePairs()) {
                    if (!(keyType.matches(kvp.key, th))) {
                        string s = ((((("The map contains a key ("+kvp.key.show())+") of type ")+kvp.key.getItemType())+" that is not an instance of the required type ")+keyType);
                        return Saxon.Ejava.util.Optional<string>.of(s);
                    }
                    if (!(valueType.matches(kvp.value, th))) {
                        string s = ((((("The map contains an entry with key ("+kvp.key.show())+") whose corresponding value (")+Saxon.Hej.trans.Err.depictSequence(kvp.value))+") is not an instance of the required type ")+valueType);
                        Saxon.Ejava.util.Optional<string> more = valueType.explainMismatch(kvp.value, th);
                        if (more.isPresent()) {
                            s = ((s+". ")+more.get());
                        }
                        return Saxon.Ejava.util.Optional<string>.of(s);
                    }
                }
            }
            return Saxon.Ejava.util.Optional<string>.empty();
        }
        public override Saxon.Hej.expr.Expression makeFunctionSequenceCoercer(Saxon.Hej.expr.Expression exp, Saxon.Hej.expr.parser.RoleDiagnostic role) {
            return new Saxon.Hej.type.SpecificFunctionType(getArgumentTypes(), getResultType()).makeFunctionSequenceCoercer(exp, role);
        }
    
    }
}