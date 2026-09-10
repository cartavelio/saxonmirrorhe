using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.ma.map {
    public class RecordTest : Saxon.Hej.type.AnyFunctionType, Saxon.Hej.ma.map.TupleType {
        private readonly System.Collections.Generic.IDictionary<string,Saxon.Hej.value.SequenceType> fieldTypes = new System.Collections.Generic.Dictionary<string, Saxon.Hej.value.SequenceType>();
        private readonly System.Collections.Generic.ISet<string> optionalFields = new System.Collections.Generic.HashSet<string>();
        private bool extensible;
        public RecordTest()  {
        }
        public RecordTest(System.Collections.Generic.IList<string> names, System.Collections.Generic.IList<Saxon.Hej.value.SequenceType> types, System.Collections.Generic.ICollection<string> optionalFieldNames, bool extensible)  {
            setDetails(names, types, optionalFieldNames, extensible);
        }
        public void setDetails(System.Collections.Generic.IList<string> names, System.Collections.Generic.IList<Saxon.Hej.value.SequenceType> types, System.Collections.Generic.ICollection<string> optionalFieldNames, bool extensible) {
            for (int i = 0; (i<names.Count); i++) {
                Saxon.Impl.Helpers.MapUtils.Put(fieldTypes, names[i], types[i]);
            }
            Saxon.Impl.Helpers.SetUtils.AddAll(optionalFields, optionalFieldNames);
            this.extensible = extensible;
        }
        public override  /* REPLACING public */ Saxon.Hej.om.Genre getGenre() {
            return Saxon.Hej.om.Genre.MAP;
        }
        public override bool isMapType() {
            return true;
        }
        public override bool isArrayType() {
            return false;
        }
        public System.Collections.Generic.IEnumerable<string> getFieldNames() {
            return Saxon.Impl.Helpers.MapUtils.KeySet(fieldTypes);
        }
        public Saxon.Hej.value.SequenceType getFieldType(string field) {
            return Saxon.Impl.Helpers.MapUtils.Get(fieldTypes, field);
        }
        public bool isOptionalField(string field) {
            return optionalFields.Contains(field);
        }
        public bool isExtensible() {
            return extensible;
        }
        public override bool matches(Saxon.Hej.om.Item item, Saxon.Hej.type.TypeHierarchy th) {
            if (!(((item is Saxon.Hej.ma.map.MapItem)))) {
                return false;
            }
            Saxon.Hej.ma.map.MapItem map = (Saxon.Hej.ma.map.MapItem)(item);
            foreach (System.Collections.Generic.KeyValuePair<string,Saxon.Hej.value.SequenceType> field in fieldTypes) {
                Saxon.Hej.om.GroundedValue val = map.get(new Saxon.Hej.value.StringValue(field.Key));
                if ((val == null)) {
                    if (!(isOptionalField(field.Key))) {
                        return false;
                    }
                } else {
                    if (!(field.Value.matches(val, th))) {
                        return false;
                    }
                }
            }
            if (!(extensible)) {
                Saxon.Hej.tree.iter.AtomicIterator keyIter = map.keys();
                Saxon.Hej.value.AtomicValue key;
                while (((key = ((/*1430*/Saxon.Hej.value.AtomicValue)keyIter.next())) != null)) {
                    if ((!(((key is Saxon.Hej.value.StringValue))) || !(fieldTypes.ContainsKey(key.getStringValue())))) {
                        return false;
                    }
                }
            }
            return true;
        }
        public int getArity() {
            return 1;
        }
        public override Saxon.Hej.value.SequenceType[] getArgumentTypes() {
            return new Saxon.Hej.value.SequenceType[]{Saxon.Hej.value.SequenceType.SINGLE_ATOMIC};
        }
        public override Saxon.Hej.value.SequenceType getResultType() {
            if (extensible) {
                return Saxon.Hej.value.SequenceType.ANY_SEQUENCE;
            } else {
                Saxon.Hej.type.ItemType resultType = null;
                bool allowsMany = false;
                foreach (System.Collections.Generic.KeyValuePair<string,Saxon.Hej.value.SequenceType> field in fieldTypes) {
                    if ((resultType == null)) {
                        resultType = field.Value.getPrimaryType();
                    } else {
                        resultType = Saxon.Hej.type.Type.getCommonSuperType(resultType, field.Value.getPrimaryType());
                    }
                    allowsMany = (allowsMany || Saxon.Hej.value.Cardinality.allowsMany(field.Value.getCardinality()));
                }
                return Saxon.Hej.value.SequenceType.makeSequenceType(resultType, (allowsMany ? Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE : Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_ONE));
            }
        }
        public override double getDefaultPriority() {
            double prio = 1;
            foreach (Saxon.Hej.value.SequenceType st in fieldTypes.Values) {
                prio *= st.getPrimaryType().getNormalizedDefaultPriority();
            }
            return (extensible ? (0.5+(prio/2)) : prio);
        }
        public override string ToString() {
            return makeString(x => x.ToString() /* method reference */);
        }
        public override  /* REPLACING public */ string toExportString() {
            return makeString(x => x.toExportString() /* method reference */);
        }
        public override string getBasicAlphaCode() {
            return "FM";
        }
        private string makeString(System.Func<Saxon.Hej.value.SequenceType,string> show) {
            Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder(100);
            sb.append("record(");
            bool first = true;
            foreach (System.Collections.Generic.KeyValuePair<string,Saxon.Hej.value.SequenceType> field in fieldTypes) {
                if (first) {
                    first = false;
                } else {
                    sb.append(", ");
                }
                if (Saxon.Hej.om.NameChecker.isValidNCName(field.Key)) {
                    sb.append(field.Key);
                } else {
                    sb.append('"').append(field.Key).append('"');
                }
                if (isOptionalField(field.Key)) {
                    sb.append('?');
                }
                sb.append(": ");
                if ((field.Value.getPrimaryType() == this)) {
                    sb.append("..").append(Saxon.Hej.value.Cardinality.getOccurrenceIndicator(field.Value.getCardinality()));
                } else {
                    sb.append(show(field.Value));
                }
            }
            if (isExtensible()) {
                sb.append(", *");
            }
            sb.append(")");
            return sb.ToString();
        }
        public override bool Equals(System.Object other) {
            return ((this == other) || ((((other is Saxon.Hej.ma.map.RecordTest) && (extensible == ((Saxon.Hej.ma.map.RecordTest)(other)).extensible)) && fieldTypes.Equals(((Saxon.Hej.ma.map.RecordTest)(other)).fieldTypes)) && optionalFields.Equals(((Saxon.Hej.ma.map.RecordTest)(other)).optionalFields)));
        }
        public override int GetHashCode() {
            int h = 0x27ca481f;
            foreach (System.Collections.Generic.KeyValuePair<string,Saxon.Hej.value.SequenceType> entry in fieldTypes) {
                h ^= entry.Key.GetHashCode();
                if ((entry.Value.getPrimaryType() == this)) {
                    h ^= 0x05050505;
                } else {
                    h ^= entry.Value.GetHashCode();
                }
            }
            return h;
        }
        public override Saxon.Hej.type.Affinity relationship(Saxon.Hej.type.FunctionItemType other, Saxon.Hej.type.TypeHierarchy th) {
            if ((other == Saxon.Hej.type.AnyFunctionType.getInstance())) {
                return Saxon.Hej.type.Affinity.SUBSUMED_BY;
            } else {
                if ((other is Saxon.Hej.ma.map.RecordTest)) {
                    return recordTypeRelationship((Saxon.Hej.ma.map.RecordTest)(other), th);
                } else {
                    if ((other == Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE)) {
                        return Saxon.Hej.type.Affinity.SUBSUMED_BY;
                    } else {
                        if (other.isArrayType()) {
                            return Saxon.Hej.type.Affinity.DISJOINT;
                        } else {
                            if ((other is Saxon.Hej.ma.map.MapType)) {
                                return recordToMapRelationship((Saxon.Hej.ma.map.MapType)(other), th);
                            } else {
                                Saxon.Hej.type.Affinity rel;
                                rel = new Saxon.Hej.type.SpecificFunctionType(getArgumentTypes(), getResultType()).relationship(other, th);
                                return rel;
                            }
                        }
                    }
                }
            }
        }
        private Saxon.Hej.type.Affinity recordToMapRelationship(Saxon.Hej.ma.map.MapType other, Saxon.Hej.type.TypeHierarchy th) {
            Saxon.Hej.type.AtomicType recordKeyType = (isExtensible() ? Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC : Saxon.Hej.type.BuiltInAtomicType.STRING);
            Saxon.Hej.type.Affinity keyRel = th.relationship(recordKeyType, other.getKeyType());
            if ((keyRel == Saxon.Hej.type.Affinity.DISJOINT)) {
                return Saxon.Hej.type.Affinity.DISJOINT;
            }
            if ((other.getValueType().getPrimaryType().Equals(Saxon.Hej.type.AnyItemType.getInstance()) && (other.getValueType().getCardinality() == Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE))) {
                if (((keyRel == Saxon.Hej.type.Affinity.SUBSUMED_BY) || (keyRel == Saxon.Hej.type.Affinity.SAME_TYPE))) {
                    return Saxon.Hej.type.Affinity.SUBSUMED_BY;
                } else {
                    return Saxon.Hej.type.Affinity.OVERLAPS;
                }
            } else {
                if (isExtensible()) {
                    return Saxon.Hej.type.Affinity.OVERLAPS;
                } else {
                    foreach (Saxon.Hej.value.SequenceType entry in fieldTypes.Values) {
                        Saxon.Hej.type.Affinity rel = th.sequenceTypeRelationship(entry, other.getValueType());
                        if (!((((rel == Saxon.Hej.type.Affinity.SUBSUMED_BY) || (rel == Saxon.Hej.type.Affinity.SAME_TYPE))))) {
                            return Saxon.Hej.type.Affinity.OVERLAPS;
                        }
                    }
                    return Saxon.Hej.type.Affinity.SUBSUMED_BY;
                }
            }
        }
        private Saxon.Hej.type.Affinity recordTypeRelationship(Saxon.Hej.ma.map.RecordTest other, Saxon.Hej.type.TypeHierarchy th) {
            System.Collections.Generic.ISet<string> keys = new System.Collections.Generic.HashSet<string>(Saxon.Impl.Helpers.MapUtils.KeySet(fieldTypes));
            Saxon.Impl.Helpers.SetUtils.AddAll(keys, Saxon.Impl.Helpers.MapUtils.KeySet(other.fieldTypes));
            bool foundSubsuming = false;
            bool foundSubsumed = false;
            bool foundOverlap = false;
            if (isExtensible()) {
                if (!(other.isExtensible())) {
                    foundSubsuming = true;
                }
            } else {
                if (other.isExtensible()) {
                    foundSubsumed = true;
                }
            }
            foreach (string key in keys) {
                Saxon.Hej.value.SequenceType t1 = Saxon.Impl.Helpers.MapUtils.Get(fieldTypes, key);
                Saxon.Hej.value.SequenceType t2 = Saxon.Impl.Helpers.MapUtils.Get(other.fieldTypes, key);
                if ((t1 == null)) {
                    if (isExtensible()) {
                        foundSubsuming = true;
                    } else {
                        if (Saxon.Hej.value.Cardinality.allowsZero(t2.getCardinality())) {
                            foundOverlap = true;
                        } else {
                            return Saxon.Hej.type.Affinity.DISJOINT;
                        }
                    }
                } else {
                    if ((t2 == null)) {
                        if (other.isExtensible()) {
                            foundSubsumed = true;
                        } else {
                            if (Saxon.Hej.value.Cardinality.allowsZero(t1.getCardinality())) {
                                foundOverlap = true;
                            } else {
                                return Saxon.Hej.type.Affinity.DISJOINT;
                            }
                        }
                    } else {
                        Saxon.Hej.type.Affinity a = th.sequenceTypeRelationship(t1, t2);
                        switch (a) {
                          case Saxon.Hej.type.Affinity.SAME_TYPE:
                            break ;
                          case Saxon.Hej.type.Affinity.SUBSUMED_BY:
                            foundSubsumed = true;
                            break ;
                          case Saxon.Hej.type.Affinity.SUBSUMES:
                            foundSubsuming = true;
                            break ;
                          case Saxon.Hej.type.Affinity.OVERLAPS:
                            foundOverlap = true;
                            break ;
                          case Saxon.Hej.type.Affinity.DISJOINT:
                            return Saxon.Hej.type.Affinity.DISJOINT;
                        }
                    }
                }
            }
            if ((foundOverlap || ((foundSubsumed && foundSubsuming)))) {
                return Saxon.Hej.type.Affinity.OVERLAPS;
            } else {
                if (foundSubsuming) {
                    return Saxon.Hej.type.Affinity.SUBSUMES;
                } else {
                    if (foundSubsumed) {
                        return Saxon.Hej.type.Affinity.SUBSUMED_BY;
                    } else {
                        return Saxon.Hej.type.Affinity.SAME_TYPE;
                    }
                }
            }
        }
        public override  /* REPLACING public */ Saxon.Ejava.util.Optional<string> explainMismatch(Saxon.Hej.om.Item item, Saxon.Hej.type.TypeHierarchy th) {
            if ((item is Saxon.Hej.ma.map.MapItem)) {
                foreach (System.Collections.Generic.KeyValuePair<string,Saxon.Hej.value.SequenceType> entry in fieldTypes) {
                    string key = entry.Key;
                    Saxon.Hej.value.SequenceType required = entry.Value;
                    Saxon.Hej.om.GroundedValue value = ((Saxon.Hej.ma.map.MapItem)(item)).get(new Saxon.Hej.value.StringValue(key));
                    if ((value == null)) {
                        if (!(Saxon.Hej.value.Cardinality.allowsZero(required.getCardinality()))) {
                            return Saxon.Ejava.util.Optional<string>.of((("Field "+key)+" is absent; it must have a value"));
                        }
                    } else {
                        if (!(required.matches(value, th))) {
                            string s = ((((("Field "+key)+" has value ")+Saxon.Hej.trans.Err.depictSequence(value))+" which does not match the required type ")+required.ToString());
                            Saxon.Ejava.util.Optional<string> more = required.explainMismatch(value, th);
                            if (more.isPresent()) {
                                s += (". "+more.get());
                            }
                            return Saxon.Ejava.util.Optional<string>.of(s);
                        }
                    }
                }
                if (!(extensible)) {
                    Saxon.Hej.tree.iter.AtomicIterator keyIter = ((Saxon.Hej.ma.map.MapItem)(item)).keys();
                    Saxon.Hej.value.AtomicValue key;
                    while (((key = ((/*1430*/Saxon.Hej.value.AtomicValue)keyIter.next())) != null)) {
                        if (!(((key is Saxon.Hej.value.StringValue)))) {
                            return Saxon.Ejava.util.Optional<string>.of((("Undeclared field "+key)+" is present, but it is not a string, and the record type is not extensible"));
                        } else {
                            if (!(fieldTypes.ContainsKey(key.getStringValue()))) {
                                return Saxon.Ejava.util.Optional<string>.of((("Undeclared field "+key)+" is present, but the record type is not extensible"));
                            }
                        }
                    }
                }
            }
            return Saxon.Ejava.util.Optional<string>.empty();
        }
        public override Saxon.Hej.expr.Expression makeFunctionSequenceCoercer(Saxon.Hej.expr.Expression exp, Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> role) {
            return new Saxon.Hej.type.SpecificFunctionType(getArgumentTypes(), getResultType()).makeFunctionSequenceCoercer(exp, role);
        }
    
    }
}