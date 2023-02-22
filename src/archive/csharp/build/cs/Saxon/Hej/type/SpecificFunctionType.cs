using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.type {
    public class SpecificFunctionType : Saxon.Hej.type.AnyFunctionType {
        private readonly Saxon.Hej.value.SequenceType[] argTypes;
        private readonly Saxon.Hej.value.SequenceType resultType;
        private readonly Saxon.Hej.query.AnnotationList annotations;
        public readonly static Saxon.Hej.type.FunctionItemType COMPONENT_FUNCTION_TYPE = new Saxon.Hej.type.SpecificFunctionType(new Saxon.Hej.value.SequenceType[]{Saxon.Hej.value.SequenceType.SINGLE_STRING}, Saxon.Hej.value.SequenceType.ANY_SEQUENCE);
        public SpecificFunctionType(Saxon.Hej.value.SequenceType[] argTypes, Saxon.Hej.value.SequenceType resultType)  {
            this.argTypes = Saxon.Ejava.util.Objects.requireNonNull(argTypes);
            this.resultType = Saxon.Ejava.util.Objects.requireNonNull(resultType);
            this.annotations = Saxon.Hej.query.AnnotationList.EMPTY;
        }
        public SpecificFunctionType(Saxon.Hej.value.SequenceType[] argTypes, Saxon.Hej.value.SequenceType resultType, Saxon.Hej.query.AnnotationList annotations)  {
            this.argTypes = Saxon.Ejava.util.Objects.requireNonNull(argTypes);
            this.resultType = Saxon.Ejava.util.Objects.requireNonNull(resultType);
            this.annotations = Saxon.Ejava.util.Objects.requireNonNull(annotations);
        }
        public int getArity() {
            return argTypes.Length;
        }
        public override Saxon.Hej.value.SequenceType[] getArgumentTypes() {
            return argTypes;
        }
        public override Saxon.Hej.value.SequenceType getResultType() {
            return resultType;
        }
        public override Saxon.Hej.query.AnnotationList getAnnotationAssertions() {
            return annotations;
        }
        public override bool isAtomizable(Saxon.Hej.type.TypeHierarchy th) {
            if ((getArity() != 1)) {
                return false;
            }
            Saxon.Hej.type.ItemType argType = getArgumentTypes()[0].getPrimaryType();
            return th.isSubType(Saxon.Hej.type.BuiltInAtomicType.INTEGER, argType);
        }
        public override string ToString() {
            Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder(100);
            sb.append("(function(");
            for (int i = 0; (i<argTypes.Length); i++) {
                sb.append(argTypes[i].ToString());
                if ((i<(argTypes.Length-1))) {
                    sb.append(", ");
                }
            }
            sb.append(") as ");
            sb.append(resultType.ToString());
            sb.append(')');
            return sb.ToString();
        }
        public override  /* REPLACING public */ string toExportString() {
            Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder(100);
            sb.append("(function(");
            for (int i = 0; (i<argTypes.Length); i++) {
                sb.append(argTypes[i].toExportString());
                if ((i<(argTypes.Length-1))) {
                    sb.append(", ");
                }
            }
            sb.append(") as ");
            sb.append(resultType.toExportString());
            sb.append(')');
            return sb.ToString();
        }
        public override bool Equals(System.Object other) {
            if ((other is Saxon.Hej.type.SpecificFunctionType)) {
                Saxon.Hej.type.SpecificFunctionType f2 = (Saxon.Hej.type.SpecificFunctionType)(other);
                if (!(resultType.Equals(f2.resultType))) {
                    return false;
                }
                if ((argTypes.Length != f2.argTypes.Length)) {
                    return false;
                }
                for (int i = 0; (i<argTypes.Length); i++) {
                    if (!(argTypes[i].Equals(f2.argTypes[i]))) {
                        return false;
                    }
                }
                if (!(getAnnotationAssertions().Equals(f2.getAnnotationAssertions()))) {
                    return false;
                }
                return true;
            }
            return false;
        }
        public override int GetHashCode() {
            int h = (resultType.GetHashCode()^argTypes.Length);
            foreach (Saxon.Hej.value.SequenceType argType in argTypes) {
                h ^= argType.GetHashCode();
            }
            return h;
        }
        public override Saxon.Hej.type.Affinity relationship(Saxon.Hej.type.FunctionItemType other, Saxon.Hej.type.TypeHierarchy th) {
            if (((other == Saxon.Hej.type.AnyFunctionType.getInstance()) || (other is Saxon.Hej.type.AnyFunctionTypeWithAssertions))) {
                return Saxon.Hej.type.Affinity.SUBSUMED_BY;
            } else {
                if (Equals(other)) {
                    return Saxon.Hej.type.Affinity.SAME_TYPE;
                } else {
                    if (((other is Saxon.Hej.ma.arrays.ArrayItemType) || (other is Saxon.Hej.ma.map.MapType))) {
                        Saxon.Hej.type.Affinity rrel = other.relationship(this, th);
                        switch (rrel) {
                          case Saxon.Hej.type.Affinity.SUBSUMES:
                            return Saxon.Hej.type.Affinity.SUBSUMED_BY;
                          case Saxon.Hej.type.Affinity.SUBSUMED_BY:
                            return Saxon.Hej.type.Affinity.SUBSUMES;
                          default:
                            return rrel;
                        }
                    } else {
                        if ((argTypes.Length != other.getArgumentTypes().Length)) {
                            return Saxon.Hej.type.Affinity.DISJOINT;
                        }
                        bool wider = false;
                        bool narrower = false;
                        for (int i = 0; (i<argTypes.Length); i++) {
                            Saxon.Hej.type.Affinity argRel = th.sequenceTypeRelationship(argTypes[i], other.getArgumentTypes()[i]);
                            switch (argRel) {
                              case Saxon.Hej.type.Affinity.DISJOINT:
                                return Saxon.Hej.type.Affinity.DISJOINT;
                              case Saxon.Hej.type.Affinity.SUBSUMES:
                                narrower = true;
                                break ;
                              case Saxon.Hej.type.Affinity.SUBSUMED_BY:
                                wider = true;
                                break ;
                              case Saxon.Hej.type.Affinity.OVERLAPS:
                                wider = true;
                                narrower = true;
                                break ;
                              case Saxon.Hej.type.Affinity.SAME_TYPE:
                              default:
                                break ;
                            }
                        }
                        Saxon.Hej.type.Affinity resRel = th.sequenceTypeRelationship(resultType, other.getResultType());
                        switch (resRel) {
                          case Saxon.Hej.type.Affinity.DISJOINT:
                            return Saxon.Hej.type.Affinity.DISJOINT;
                          case Saxon.Hej.type.Affinity.SUBSUMES:
                            wider = true;
                            break ;
                          case Saxon.Hej.type.Affinity.SUBSUMED_BY:
                            narrower = true;
                            break ;
                          case Saxon.Hej.type.Affinity.OVERLAPS:
                            wider = true;
                            narrower = true;
                            break ;
                          case Saxon.Hej.type.Affinity.SAME_TYPE:
                          default:
                            break ;
                        }
                        if (wider) {
                            if (narrower) {
                                return Saxon.Hej.type.Affinity.OVERLAPS;
                            } else {
                                return Saxon.Hej.type.Affinity.SUBSUMES;
                            }
                        } else {
                            if (narrower) {
                                return Saxon.Hej.type.Affinity.SUBSUMED_BY;
                            } else {
                                return Saxon.Hej.type.Affinity.SAME_TYPE;
                            }
                        }
                    }
                }
            }
        }
        public override double getDefaultPriority() {
            double prio = 1;
            foreach (Saxon.Hej.value.SequenceType st in getArgumentTypes()) {
                prio *= st.getPrimaryType().getNormalizedDefaultPriority();
            }
            return prio;
        }
        public override bool matches(Saxon.Hej.om.Item item, Saxon.Hej.type.TypeHierarchy th) {
            if (!(((item is Saxon.Hej.om.FunctionItem)))) {
                return false;
            }
            if ((item is Saxon.Hej.ma.map.MapItem)) {
                if (((((getArity() == 1) && (argTypes[0].getCardinality() == Saxon.Hej.expr.StaticProperty.EXACTLY_ONE)) && argTypes[0].getPrimaryType().isPlainType()) && Saxon.Hej.value.Cardinality.allowsZero(resultType.getCardinality()))) {
                    foreach (Saxon.Hej.ma.map.KeyValuePair pair in ((Saxon.Hej.ma.map.MapItem)(item)).keyValuePairs()) {
                        if (!(resultType.matches(pair.value, th))) {
                            return false;
                        }
                    }
                    return true;
                } else {
                    return false;
                }
            }
            if ((item is Saxon.Hej.ma.arrays.ArrayItem)) {
                if ((((getArity() == 1) && (argTypes[0].getCardinality() == Saxon.Hej.expr.StaticProperty.EXACTLY_ONE)) && argTypes[0].getPrimaryType().isPlainType())) {
                    Saxon.Hej.type.Affinity rel = th.relationship(argTypes[0].getPrimaryType(), Saxon.Hej.type.BuiltInAtomicType.INTEGER);
                    if (!((((rel == Saxon.Hej.type.Affinity.SAME_TYPE) || (rel == Saxon.Hej.type.Affinity.SUBSUMED_BY))))) {
                        return false;
                    }
                    foreach (Saxon.Hej.om.GroundedValue member in ((Saxon.Hej.ma.arrays.ArrayItem)(item)).members()) {
                        if (!(resultType.matches(member, th))) {
                            return false;
                        }
                    }
                    return true;
                } else {
                    return false;
                }
            }
            Saxon.Hej.type.Affinity affinity = th.relationship(((Saxon.Hej.om.FunctionItem)(item)).getFunctionItemType(), this);
            return ((affinity == Saxon.Hej.type.Affinity.SAME_TYPE) || (affinity == Saxon.Hej.type.Affinity.SUBSUMED_BY));
        }
        public override  /* REPLACING public */ Saxon.Ejava.util.Optional<string> explainMismatch(Saxon.Hej.om.Item item, Saxon.Hej.type.TypeHierarchy th) {
            if (!(((item is Saxon.Hej.om.FunctionItem)))) {
                return Saxon.Ejava.util.Optional<string>.empty();
            }
            if ((item is Saxon.Hej.ma.map.MapItem)) {
                if ((getArity() == 1)) {
                    if (((argTypes[0].getCardinality() == Saxon.Hej.expr.StaticProperty.EXACTLY_ONE) && argTypes[0].getPrimaryType().isPlainType())) {
                        foreach (Saxon.Hej.ma.map.KeyValuePair pair in ((Saxon.Hej.ma.map.MapItem)(item)).keyValuePairs()) {
                            if (!(resultType.matches(pair.value, th))) {
                                string s = (((((("The supplied map contains an entry with key ("+pair.key)+") whose corresponding value (")+Saxon.Hej.trans.Err.depictSequence(pair.value))+") is not an instance of the return type in the function signature (")+resultType)+")");
                                Saxon.Ejava.util.Optional<string> more = resultType.explainMismatch(pair.value, th);
                                if (more.isPresent()) {
                                    s = ((s+". ")+more.get());
                                }
                                return Saxon.Ejava.util.Optional<string>.of(s);
                            }
                        }
                    } else {
                        string s = (("The function argument is of type "+argTypes[0])+"; a map can only be supplied for a function type whose argument type is atomic");
                        return Saxon.Ejava.util.Optional<string>.of(s);
                    }
                } else {
                    string s = (("The function arity is "+getArity())+"; a map can only be supplied for a function type with arity 1");
                    return Saxon.Ejava.util.Optional<string>.of(s);
                }
            }
            if ((item is Saxon.Hej.ma.arrays.ArrayItem)) {
                if ((getArity() == 1)) {
                    if (((argTypes[0].getCardinality() == Saxon.Hej.expr.StaticProperty.EXACTLY_ONE) && argTypes[0].getPrimaryType().isPlainType())) {
                        Saxon.Hej.type.Affinity rel = th.relationship(argTypes[0].getPrimaryType(), Saxon.Hej.type.BuiltInAtomicType.INTEGER);
                        if (!((((rel == Saxon.Hej.type.Affinity.SAME_TYPE) || (rel == Saxon.Hej.type.Affinity.SUBSUMED_BY))))) {
                            string s = (("The function expects an argument of type "+argTypes[0])+"; an array can only be supplied for a function that expects an integer");
                            return Saxon.Ejava.util.Optional<string>.of(s);
                        } else {
                            foreach (Saxon.Hej.om.GroundedValue member in ((Saxon.Hej.ma.arrays.ArrayItem)(item)).members()) {
                                if (!(resultType.matches(member, th))) {
                                    string s = (((("The supplied array contains an entry ("+Saxon.Hej.trans.Err.depictSequence(member))+") is not an instance of the return type in the function signature (")+resultType)+")");
                                    Saxon.Ejava.util.Optional<string> more = resultType.explainMismatch(member, th);
                                    if (more.isPresent()) {
                                        s = ((s+". ")+more.get());
                                    }
                                    return Saxon.Ejava.util.Optional<string>.of(s);
                                }
                            }
                        }
                    } else {
                        string s = (("The function argument is of type "+argTypes[0])+"; an array can only be supplied for a function type whose argument type is xs:integer");
                        return Saxon.Ejava.util.Optional<string>.of(s);
                    }
                } else {
                    string s = (("The function arity is "+getArity())+"; an array can only be supplied for a function type with arity 1");
                    return Saxon.Ejava.util.Optional<string>.of(s);
                }
            }
            Saxon.Hej.type.FunctionItemType other = ((Saxon.Hej.om.FunctionItem)(item)).getFunctionItemType();
            if ((getArity() != ((Saxon.Hej.om.FunctionItem)(item)).getArity())) {
                string s = ((("The required function arity is "+getArity())+"; the supplied function has arity ")+((Saxon.Hej.om.FunctionItem)(item)).getArity());
                return Saxon.Ejava.util.Optional<string>.of(s);
            }
            Saxon.Hej.type.Affinity affinity = th.sequenceTypeRelationship(resultType, other.getResultType());
            if (((affinity != Saxon.Hej.type.Affinity.SAME_TYPE) && (affinity != Saxon.Hej.type.Affinity.SUBSUMES))) {
                string s = (((("The return type of the required function is "+resultType)+" but the return")+"type of the supplied function is ")+other.getResultType());
                return Saxon.Ejava.util.Optional<string>.of(s);
            }
            for (int j = 0; (j<getArity()); j++) {
                affinity = th.sequenceTypeRelationship(argTypes[j], other.getArgumentTypes()[j]);
                if (((affinity != Saxon.Hej.type.Affinity.SAME_TYPE) && (affinity != Saxon.Hej.type.Affinity.SUBSUMED_BY))) {
                    string s = (((((("The type of the "+Saxon.Hej.expr.parser.RoleDiagnostic.ordinal((j+1)))+" argument of the required function is ")+argTypes[j])+" but the declared")+"type of the corresponding argument of the supplied function is ")+other.getArgumentTypes()[j]);
                    return Saxon.Ejava.util.Optional<string>.of(s);
                }
            }
            return Saxon.Ejava.util.Optional<string>.empty();
        }
        public override Saxon.Hej.expr.Expression makeFunctionSequenceCoercer(Saxon.Hej.expr.Expression exp, Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> role) {
            return new Saxon.Hej.functions.hof.FunctionSequenceCoercer(exp, this, role);
        }
    
    }
}