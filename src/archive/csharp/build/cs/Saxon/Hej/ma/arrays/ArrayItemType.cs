using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.ma.arrays {
    public class ArrayItemType : Saxon.Hej.type.AnyFunctionType {
        public readonly static Saxon.Hej.ma.arrays.ArrayItemType ANY_ARRAY_TYPE = new Saxon.Hej.ma.arrays.ArrayItemType(Saxon.Hej.value.SequenceType.ANY_SEQUENCE);
        public readonly static Saxon.Hej.value.SequenceType SINGLE_ARRAY = Saxon.Hej.value.SequenceType.makeSequenceType(Saxon.Hej.ma.arrays.ArrayItemType.ANY_ARRAY_TYPE, Saxon.Hej.expr.StaticProperty.EXACTLY_ONE);
        private readonly Saxon.Hej.value.SequenceType memberType;
        public ArrayItemType(Saxon.Hej.value.SequenceType memberType)  {
            this.memberType = memberType;
        }
        public override  /* REPLACING public */ Saxon.Hej.om.Genre getGenre() {
            return Saxon.Hej.om.Genre.ARRAY;
        }
        public Saxon.Hej.value.SequenceType getMemberType() {
            return memberType;
        }
        public override bool isMapType() {
            return false;
        }
        public override bool isArrayType() {
            return true;
        }
        public override string getBasicAlphaCode() {
            return "FA";
        }
        public override bool isAtomizable(Saxon.Hej.type.TypeHierarchy th) {
            return true;
        }
        public override Saxon.Hej.type.PlainType getAtomizedItemType() {
            return memberType.getPrimaryType().getAtomizedItemType();
        }
        public int getArity() {
            return 1;
        }
        public override Saxon.Hej.value.SequenceType[] getArgumentTypes() {
            return new Saxon.Hej.value.SequenceType[]{Saxon.Hej.type.BuiltInAtomicType.INTEGER.one()};
        }
        public override double getDefaultPriority() {
            return memberType.getPrimaryType().getNormalizedDefaultPriority();
        }
        public override bool matches(Saxon.Hej.om.Item item, Saxon.Hej.type.TypeHierarchy th) {
            if (!(((item is Saxon.Hej.ma.arrays.ArrayItem)))) {
                return false;
            }
            if ((this == ANY_ARRAY_TYPE)) {
                return true;
            } else {
                foreach (Saxon.Hej.om.GroundedValue s in ((Saxon.Hej.ma.arrays.ArrayItem)(item)).members()) {
                    if (!(memberType.matches(s, th))) {
                        return false;
                    }
                }
                return true;
            }
        }
        public override Saxon.Hej.value.SequenceType getResultType() {
            return memberType;
        }
        public override string ToString() {
            return makeString(st => st.ToString());
        }
        private string makeString(System.Func<Saxon.Hej.value.SequenceType,string> show) {
            if (this.Equals(ANY_ARRAY_TYPE)) {
                return "array(*)";
            } else {
                Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder(100);
                sb.append("array(");
                sb.append(show(memberType));
                sb.append(")");
                return sb.ToString();
            }
        }
        public override  /* REPLACING public */ string toExportString() {
            return makeString(st => st.toExportString());
        }
        public override bool Equals(System.Object other) {
            if ((this == other)) {
                return true;
            }
            if ((other is Saxon.Hej.ma.arrays.ArrayItemType)) {
                Saxon.Hej.ma.arrays.ArrayItemType f2 = (Saxon.Hej.ma.arrays.ArrayItemType)(other);
                return memberType.Equals(f2.memberType);
            }
            return false;
        }
        public override int GetHashCode() {
            return memberType.GetHashCode();
        }
        public override Saxon.Hej.type.Affinity relationship(Saxon.Hej.type.FunctionItemType other, Saxon.Hej.type.TypeHierarchy th) {
            if ((other == Saxon.Hej.type.AnyFunctionType.getInstance())) {
                return Saxon.Hej.type.Affinity.SUBSUMED_BY;
            } else {
                if (Equals(other)) {
                    return Saxon.Hej.type.Affinity.SAME_TYPE;
                } else {
                    if ((other == Saxon.Hej.ma.arrays.ArrayItemType.ANY_ARRAY_TYPE)) {
                        return Saxon.Hej.type.Affinity.SUBSUMED_BY;
                    } else {
                        if (other.isMapType()) {
                            return Saxon.Hej.type.Affinity.DISJOINT;
                        } else {
                            if ((other is Saxon.Hej.ma.arrays.ArrayItemType)) {
                                Saxon.Hej.ma.arrays.ArrayItemType f2 = (Saxon.Hej.ma.arrays.ArrayItemType)(other);
                                Saxon.Hej.type.Affinity rel = th.sequenceTypeRelationship(memberType, f2.memberType);
                                return ((rel == Saxon.Hej.type.Affinity.DISJOINT) ? Saxon.Hej.type.Affinity.OVERLAPS : rel);
                            } else {
                                Saxon.Hej.type.Affinity rel = new Saxon.Hej.type.SpecificFunctionType(getArgumentTypes(), getResultType()).relationship(other, th);
                                if (((rel == Saxon.Hej.type.Affinity.SUBSUMES) || (rel == Saxon.Hej.type.Affinity.SAME_TYPE))) {
                                    rel = Saxon.Hej.type.Affinity.OVERLAPS;
                                }
                                return rel;
                            }
                        }
                    }
                }
            }
        }
        public override Saxon.Hej.expr.Expression makeFunctionSequenceCoercer(Saxon.Hej.expr.Expression exp, Saxon.Hej.expr.parser.RoleDiagnostic role) {
            return new Saxon.Hej.type.SpecificFunctionType(getArgumentTypes(), getResultType()).makeFunctionSequenceCoercer(exp, role);
        }
        public override  /* REPLACING public */ Saxon.Ejava.util.Optional<string> explainMismatch(Saxon.Hej.om.Item item, Saxon.Hej.type.TypeHierarchy th) {
            if ((item is Saxon.Hej.ma.arrays.ArrayItem)) {
                for (int i = 0; (i<((Saxon.Hej.ma.arrays.ArrayItem)(item)).arrayLength()); i++) {
                    Saxon.Hej.om.GroundedValue member = ((Saxon.Hej.ma.arrays.ArrayItem)(item)).get(i);
                    if (!(memberType.matches(member, th))) {
                        string s = ((((("The "+Saxon.Hej.expr.parser.RoleDiagnostic.ordinal((i+1)))+" member of the supplied array {")+Saxon.Hej.trans.Err.depictSequence(member))+"} does not match the required member type ")+memberType);
                        Saxon.Ejava.util.Optional<string> more = memberType.explainMismatch(member, th);
                        if (more.isPresent()) {
                            s = ((s+". ")+more.get());
                        }
                        return Saxon.Ejava.util.Optional<string>.of(s);
                    }
                }
            }
            return Saxon.Ejava.util.Optional<string>.empty();
        }
    
    }
}