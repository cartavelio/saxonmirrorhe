using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema.fsa {
    public class Edge : System.Collections.Generic.IEnumerable<Saxon.Eej.ee.schema.fsa.Edge> {
        private Saxon.Eej.ee.schema.Term term;
        private Saxon.Eej.ee.schema.Particle particle;
        private Saxon.Eej.ee.schema.fsa.AutomatonState targetState;
        public void setTargetState(Saxon.Eej.ee.schema.fsa.AutomatonState targetState) {
            this.targetState = targetState;
        }
        public Saxon.Eej.ee.schema.fsa.AutomatonState getTargetState() {
            return targetState;
        }
        public Saxon.Eej.ee.schema.fsa.State makeTransition(Saxon.Eej.ee.schema.fsa.State previousState) {
            if ((previousState is Saxon.Eej.ee.schema.fsa.CountingState)) {
                if ((((Saxon.Eej.ee.schema.fsa.CountingState)(previousState)).getUnderlyingState() == targetState)) {
                    return ((Saxon.Eej.ee.schema.fsa.CountingState)(previousState)).incrementCounter();
                } else {
                    ((Saxon.Eej.ee.schema.fsa.CountingState)(previousState)).testMinimumReached();
                }
            }
            if (targetState.requiresCounter()) {
                return new Saxon.Eej.ee.schema.fsa.CountingState(targetState, 1);
            } else {
                return targetState;
            }
        }
        public void setTerm(Saxon.Eej.ee.schema.Term term) {
            this.term = term;
        }
        public Saxon.Eej.ee.schema.Term getTerm() {
            return term;
        }
        public void setParticle(Saxon.Eej.ee.schema.Particle particle) {
            this.particle = particle;
        }
        public Saxon.Eej.ee.schema.Particle getParticle() {
            return particle;
        }
        public string matches(Saxon.Eej.ee.schema.fsa.Edge de, Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            if ((term == de.term)) {
                return null;
            }
            if (((((term is Saxon.Eej.ee.schema.ElementDecl)) && ((de.term is Saxon.Eej.ee.schema.ElementDecl))) && (((Saxon.Eej.ee.schema.ElementDecl)(term)).getFingerprint() == ((Saxon.Eej.ee.schema.ElementDecl)(de.term)).getFingerprint()))) {
                Saxon.Eej.ee.schema.ElementDecl @base = (Saxon.Eej.ee.schema.ElementDecl)(term);
                Saxon.Eej.ee.schema.ElementDecl sub = (Saxon.Eej.ee.schema.ElementDecl)(de.term);
                Saxon.Hej.type.SchemaType baseType = @base.getType();
                Saxon.Hej.type.SchemaType subType = sub.getType();
                if ((sub.isNillable() && !(@base.isNillable()))) {
                    return "The element in the derived type is nillable but the corresponding element in the base type is not";
                }
                if ((@base.getFixedValue() != null)) {
                    if ((sub.getFixedValue() == null)) {
                        return "The element in the base type has a fixed value, but the corresponding element in derived type does not";
                    }
                    if (!(Saxon.Hej.expr.sort.SimpleTypeComparison.getInstance().equal(sub.getFixedValue(), @base.getFixedValue()))) {
                        return "The element in the derived type has a different fixed value from the corresponding element in the base type";
                    }
                }
                bool elementTypeOK = true;
                string reason = null;
                if (subType.isSimpleType()) {
                    reason = /*134*/Saxon.Eej.ee.schema.UserSimpleType.isTypeDerivationOK(((Saxon.Hej.type.SimpleType)(subType)), baseType, 0);
                    elementTypeOK = ((reason == null));
                } else {
                    while (!(baseType.isSameType(subType))) {
                        if ((subType.getDerivationMethod() != Saxon.Hej.type.Derivation.DERIVATION_RESTRICTION)) {
                            elementTypeOK = false;
                            break ;
                        }
                        subType = subType.getBaseType();
                        if ((subType == null)) {
                            elementTypeOK = false;
                            break ;
                        }
                    }
                }
                if (!(elementTypeOK)) {
                    return (("The type of the element in the restricted content model is not validly derived from the "+"type of the corresponding element in the base content model")+(((reason == null) ? "" : (". "+reason))));
                }
                if (((@base.getFixedValue() != null) && !(Saxon.Hej.expr.sort.SimpleTypeComparison.getInstance().equal(@base.getFixedValue(), sub.getFixedValue())))) {
                    return ("The fixed value of the element in the base content model is not reflected in the corresponding "+"element of the restricted content model");
                }
                int baseUniqueCount = 0;
                int baseKeyCount = 0;
                int baseKeyRefCount = 0;
                int derivedUniqueCount = 0;
                int derivedKeyCount = 0;
                int derivedKeyRefCount = 0;
                foreach (Saxon.Eej.ee.schema.IdentityConstraintReference icr in @base.getIdentityConstraints()) {
                    Saxon.Eej.ee.schema.IdentityConstraint ic = ((/*1430*/Saxon.Eej.ee.schema.IdentityConstraint)icr.getTarget());
                    if ((ic is Saxon.Eej.ee.schema.Unique)) {
                        baseUniqueCount++;
                    } else {
                        if ((ic is Saxon.Eej.ee.schema.Key)) {
                            baseKeyCount++;
                        } else {
                            if ((ic is Saxon.Eej.ee.schema.KeyRef)) {
                                baseKeyRefCount++;
                            }
                        }
                    }
                }
                foreach (Saxon.Eej.ee.schema.IdentityConstraintReference icr in sub.getIdentityConstraints()) {
                    Saxon.Eej.ee.schema.IdentityConstraint ic = ((/*1430*/Saxon.Eej.ee.schema.IdentityConstraint)icr.getTarget());
                    if ((ic is Saxon.Eej.ee.schema.Unique)) {
                        derivedUniqueCount++;
                    } else {
                        if ((ic is Saxon.Eej.ee.schema.Key)) {
                            derivedKeyCount++;
                        } else {
                            if ((ic is Saxon.Eej.ee.schema.KeyRef)) {
                                derivedKeyRefCount++;
                            }
                        }
                    }
                }
                if ((baseUniqueCount>derivedUniqueCount)) {
                    return ("Element in base content model has a uniqueness constraint that is not present"+" in the corresponding element of the restricted content model");
                }
                if ((baseKeyCount>derivedKeyCount)) {
                    return ("Element in base content model has a key constraint that is not present"+" in the corresponding element of the restricted content model");
                }
                if ((baseKeyRefCount>derivedKeyRefCount)) {
                    return ("Element in base content model has a keyref constraint that is not present"+" in the corresponding element of the restricted content model");
                }
                if ((((baseUniqueCount>0) || (baseKeyCount>0)) || (baseKeyRefCount>0))) {
                    compiler.warning(((("Saxon is not able to verify that the identity constraints defined on "+"element ")+@base.getDisplayName())+" are compatible between the base type and the derived type"), Saxon.Hej.trans.SaxonErrorCode.SXSD1009, sub);
                }
                if ((de.getTargetState().isFinalState() && !(((getTargetState().isFinalState() || getTargetState().isConditionallyFinalState()))))) {
                    return "Not final";
                }
                if ((((@base.getBlock()&~(sub.getBlock()))) != 0)) {
                    return ("The disallowed substitutions of the element in the restricted content model (@block) must be "+"a superset of those in the base content model");
                }
                if (!(@base.hasSameTypeTable(sub))) {
                    return ("The type table of the element in the restricted content model must be the same as "+"the type table of the element in the base content model");
                }
                return null;
            }
            if (((((term is Saxon.Eej.ee.schema.Wildcard)) && ((de.term is Saxon.Eej.ee.schema.ElementDecl))) && ((Saxon.Eej.ee.schema.Wildcard)(term)).matches(((Saxon.Eej.ee.schema.ElementDecl)(de.term)).getTargetNamespace()))) {
                string mode = ((Saxon.Eej.ee.schema.Wildcard)(term)).getProcessContents();
                Saxon.Eej.ee.schema.PreparedSchema schema = compiler.getPreparedSchema();
                if (mode.Equals("skip")) {
                    return null;
                } else {
                    if (mode.Equals("strict")) {
                        Saxon.Eej.ee.schema.ElementDecl een = schema.getElementDecl(((Saxon.Eej.ee.schema.ElementDecl)(de.term)).getComponentName());
                        if ((een == null)) {
                            compiler.warning((("There is no global element declaration named "+Saxon.Hej.trans.Err.wrap(((Saxon.Eej.ee.schema.ElementDecl)(de.term)).getDisplayName()))+", so the strict wildcard in the base type can never be satisfied"), Saxon.Hej.trans.SaxonErrorCode.SXSD1003, (Saxon.Eej.ee.schema.ElementDecl)(de.term));
                            return null;
                        }
                        Saxon.Hej.type.SchemaType eent = een.getType();
                        Saxon.Hej.type.SchemaType et = ((Saxon.Eej.ee.schema.ElementDecl)(de.term)).getType();
                        while ((et != null)) {
                            if ((et == eent)) {
                                return null;
                            }
                            if ((et.getDerivationMethod() != Saxon.Hej.type.Derivation.DERIVATION_RESTRICTION)) {
                                break ;
                            }
                            et = et.getBaseType();
                        }
                        compiler.warning(((("The type of the local element declaration named "+Saxon.Hej.trans.Err.wrap(((Saxon.Eej.ee.schema.ElementDecl)(de.term)).getDisplayName()))+" is not derived by restriction from that of the global element")+" with the same name, so the strict wildcard in the base type can never be satisfied"), Saxon.Hej.trans.SaxonErrorCode.SXSD1011, (Saxon.Eej.ee.schema.ElementDecl)(de.term));
                        return null;
                    } else {
                        if (mode.Equals("lax")) {
                            Saxon.Eej.ee.schema.ElementDecl een = schema.getElementDecl(((Saxon.Eej.ee.schema.ElementDecl)(de.term)).getComponentName());
                            if ((een == null)) {
                                return null;
                            }
                            Saxon.Hej.type.SchemaType eent = een.getType();
                            Saxon.Hej.type.SchemaType et = ((Saxon.Eej.ee.schema.ElementDecl)(de.term)).getType();
                            while ((et != null)) {
                                if ((et == eent)) {
                                    return null;
                                }
                                if ((et.getDerivationMethod() != Saxon.Hej.type.Derivation.DERIVATION_RESTRICTION)) {
                                    break ;
                                }
                                et = et.getBaseType();
                            }
                            compiler.warning(((("The type of the local element declaration named "+Saxon.Hej.trans.Err.wrap(((Saxon.Eej.ee.schema.ElementDecl)(de.term)).getDisplayName()))+" is not derived by restriction from that of the global element")+" with the same name, so the lax wildcard in the base type can never be satisfied"), Saxon.Hej.trans.SaxonErrorCode.SXSD1011, (Saxon.Eej.ee.schema.ElementDecl)(de.term));
                            return null;
                        }
                    }
                }
                return null;
            }
            if ((((term is Saxon.Eej.ee.schema.Wildcard)) && ((de.term is Saxon.Eej.ee.schema.Wildcard)))) {
                if (!(((Saxon.Eej.ee.schema.Wildcard)(de.term)).isSubset((Saxon.Eej.ee.schema.Wildcard)(term), compiler.getNamePool()))) {
                    return ("The xs:any wildcard in the restricted type allows namespaces that are "+"not permitted by the wildcard in the base type");
                }
                if ((((Saxon.Eej.ee.schema.Wildcard)(term)).compareStrength((Saxon.Eej.ee.schema.Wildcard)(de.term))>0)) {
                    return ("The xs:any wildcard in the restricted type has a processContents that is less "+"strict than the processContents in the base type");
                }
                return null;
            }
            return "No match";
        }
        public System.Collections.Generic.IEnumerator<Saxon.Eej.ee.schema.fsa.Edge> GetEnumerator() {
            return new Saxon.Hej.tree.jiter.MonoIterator<Saxon.Eej.ee.schema.fsa.Edge>(this);
        }
        // Extra method to make the class IEnumerable as well as IEnumerable<T>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
        public void serialize(Saxon.Eej.ee.schema.SchemaModelSerializer serializer) {
            serializer.startElement("edge");
            serializer.emitAttribute("term", serializer.getId((Saxon.Hej.type.SchemaComponent)(term), false));
            serializer.emitAttribute("to", (targetState.getStateNumber()+""));
            serializer.endElement();
        }
        public class MaxOccursTerm : Saxon.Eej.ee.schema.Term {
            private MaxOccursTerm()  {
            }
            private static readonly Saxon.Eej.ee.schema.fsa.Edge.MaxOccursTerm THE_INSTANCE = new Saxon.Eej.ee.schema.fsa.Edge.MaxOccursTerm();
            public static Saxon.Eej.ee.schema.fsa.Edge.MaxOccursTerm getInstance() {
                return THE_INSTANCE;
            }
        
        }
        public class MaxOccursParticle : Saxon.Eej.ee.schema.ModelGroupParticle {
            public MaxOccursParticle() : base (null, null) {
            }
            private static readonly Saxon.Eej.ee.schema.fsa.Edge.MaxOccursParticle THE_INSTANCE = new Saxon.Eej.ee.schema.fsa.Edge.MaxOccursParticle();
            public static Saxon.Eej.ee.schema.fsa.Edge.MaxOccursParticle getInstance() {
                return THE_INSTANCE;
            }
            public override Saxon.Eej.ee.schema.Term getTerm() {
                return /*134*/Saxon.Eej.ee.schema.fsa.Edge.MaxOccursTerm.getInstance();
            }
        
        }
        public static readonly Saxon.Eej.ee.schema.fsa.Edge.MaxOccursParticle MAX_OCCURS_PARTICLE = new Saxon.Eej.ee.schema.fsa.Edge.MaxOccursParticle();
    
    }
}