using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema {
    public class AllModelGroup : Saxon.Eej.ee.schema.ModelGroup {
        private int[] minima;
        private int[] maxima;
        private Saxon.Hej.om.StructuredQName[] elementNames;
        private int numberOfElementParticles = 0;
        private Saxon.Hej.z.IntToIntMap counterMap;
        private Saxon.Hej.z.IntHashMap<Saxon.Eej.ee.schema.ElementDecl> declarationMap;
        private System.Collections.Generic.IList<Saxon.Eej.ee.schema.ElementWildcard> wildcards;
        public AllModelGroup()  {
        }
        public override void addParticle(Saxon.Eej.ee.schema.Particle particle) {
            base.addParticle(particle);
            if ((particle is Saxon.Eej.ee.schema.ElementWildcard)) {
                if ((wildcards == null)) {
                    wildcards = new System.Collections.Generic.List<Saxon.Eej.ee.schema.ElementWildcard>(4);
                }
                wildcards.Add((Saxon.Eej.ee.schema.ElementWildcard)(particle));
            } else {
                numberOfElementParticles++;
            }
        }
        public int getNumberOfElementParticles() {
            return numberOfElementParticles;
        }
        public override bool containsAll(bool simplified) {
            return true;
        }
        public override bool isEmptiable() {
            foreach (Saxon.Eej.ee.schema.Particle aContentModel in contentModel) {
                if (!(aContentModel.isEmptiable())) {
                    return false;
                }
            }
            return true;
        }
        public override bool isPointless(Saxon.Eej.ee.schema.ModelGroup container) {
            return false;
        }
        protected internal override bool checkElements(Saxon.Hej.z.IntHashMap<Saxon.Eej.ee.schema.ElementDecl> map, Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            if ((map.size()>0)) {
                compiler.error("<xs:all> cannot appear within the content model of another compositor", this);
                return false;
            }
            foreach (Saxon.Eej.ee.schema.Particle p in getSimplifiedContentModel()) {
                if ((p is Saxon.Eej.ee.schema.ElementParticle)) {
                    Saxon.Eej.ee.schema.ElementParticle ep = (Saxon.Eej.ee.schema.ElementParticle)(p);
                    int key = ((Saxon.Eej.ee.schema.ElementDecl)(ep.getTarget())).getFingerprint();
                    Saxon.Eej.ee.schema.ElementDecl prev = map.get(key);
                    if ((prev == null)) {
                        map.put(key, ep.getDeclaration());
                    } else {
                        string err = (("Two elements with the same name "+Saxon.Hej.trans.Err.wrap(ep.getName(), Saxon.Hej.trans.Err.ELEMENT))+" appear in the content model of <xs:all>. This violates the Unique Particle Attribution constraint.");
                        compiler.error(err, this);
                        return false;
                    }
                }
            }
            Saxon.Hej.z.IntHashMap<Saxon.Eej.ee.schema.ElementDecl> implicitMembers = new Saxon.Hej.z.IntHashMap<Saxon.Eej.ee.schema.ElementDecl>(20);
            System.Collections.Generic.IEnumerator<Saxon.Eej.ee.schema.ElementDecl> iterM = map.valueIterator();
            while (iterM.MoveNext() /*UNSAFE CONVERSION*/) {
                Saxon.Eej.ee.schema.ElementDecl e = iterM.Current;
                if (e.isGlobal()) {
                    foreach (Saxon.Eej.ee.schema.ElementDecl member in e.getSubstitutionGroupMembers()) {
                        Saxon.Eej.ee.schema.ElementDecl existing = implicitMembers.put(member.getFingerprint(), e);
                        if ((existing != null)) {
                            string err = ((((((("An element "+Saxon.Hej.trans.Err.wrap(member.getDisplayName(), Saxon.Hej.trans.Err.ELEMENT))+" appears implicitly in the expansion of more than one particle of the xs:all group, ")+"because it is a member of the substitution groups of both element ")+Saxon.Hej.trans.Err.wrap(existing.getDisplayName(), Saxon.Hej.trans.Err.ELEMENT))+" and element ")+Saxon.Hej.trans.Err.wrap(e.getDisplayName(), Saxon.Hej.trans.Err.ELEMENT))+". This violates the Unique Particle Attribution constraint.");
                            compiler.error(err, this);
                            setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                            return false;
                        }
                    }
                }
            }
            if ((wildcards != null)) {
                for (int i = 0; (i<wildcards.Count); i++) {
                    for (int j = 0; (j<i); j++) {
                        if (wildcards[j].getWildcard().overlaps(wildcards[i].getWildcard())) {
                            string err = (("The <xs:all> content model contains two <xs:any> wildcards that overlap each other "+"(there are elements that could match either wildcard). ")+"This violates the Unique Particle Attribution Constraint.");
                            compiler.error(err, this);
                            setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                            return false;
                        }
                    }
                }
            }
            return true;
        }
        public override string getCompositorName() {
            return "all";
        }
        public override System.Collections.Generic.IList<Saxon.Eej.ee.schema.Particle> getSimplifiedContentModel() {
            if ((simplifiedContentModel == null)) {
                System.Collections.Generic.IList<Saxon.Eej.ee.schema.Particle> s1 = base.getSimplifiedContentModel();
                System.Collections.Generic.IList<Saxon.Eej.ee.schema.Particle> s2 = new System.Collections.Generic.List<Saxon.Eej.ee.schema.Particle>();
                foreach (Saxon.Eej.ee.schema.Particle p in s1) {
                    if ((p is Saxon.Eej.ee.schema.ModelGroupParticle)) {
                        Saxon.Eej.ee.schema.ModelGroup mg = (Saxon.Eej.ee.schema.ModelGroup)(p.getTarget());
                        foreach (Saxon.Eej.ee.schema.Particle q in mg.getSimplifiedContentModel()) {
                            s2.Add(q);
                        }
                    } else {
                        s2.Add(p);
                    }
                }
                simplifiedContentModel = s2;
            }
            return simplifiedContentModel;
        }
        public override bool validate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            if (!(base.validate(compiler))) {
                return false;
            }
            foreach (Saxon.Eej.ee.schema.Particle structure in getParticles()) {
                if ((structure is Saxon.Eej.ee.schema.ModelGroupParticle)) {
                    Saxon.Eej.ee.schema.ModelGroupParticle @ref = (Saxon.Eej.ee.schema.ModelGroupParticle)(structure);
                    if (@ref.isResolved()) {
                        bool ok = false;
                        if (((@ref.getMinOccurs() == 1) && (@ref.getMaxOccurs() == 1))) {
                            Saxon.Eej.ee.schema.ModelGroupDefinition def = (Saxon.Eej.ee.schema.ModelGroupDefinition)(@ref.getGroup());
                            System.Collections.Generic.IList<Saxon.Eej.ee.schema.Particle> groupContents = def.getParticles();
                            if (((groupContents.Count == 1) && (groupContents[0] is Saxon.Eej.ee.schema.ModelGroupParticle))) {
                                Saxon.Eej.ee.schema.ModelGroupParticle mgp = (Saxon.Eej.ee.schema.ModelGroupParticle)(groupContents[0]);
                                if ((((mgp.getMinOccurs() == 1) && (mgp.getMaxOccurs() == 1)) && (mgp.getTarget() is Saxon.Eej.ee.schema.AllModelGroup))) {
                                    ok = true;
                                }
                            }
                        }
                        if (!(ok)) {
                            string err = (("Group "+@ref.getTargetComponentName().getDisplayName())+" is referenced in an xs:all group, so it must itself contain an xs:all group with minOccurs = maxOccurs = 1");
                            compiler.error(err, @ref);
                            setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                            return false;
                        }
                    }
                } else {
                    structure.validate(compiler);
                }
            }
            elaborate(compiler);
            return true;
        }
        public override void elaborate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            System.Collections.Generic.IList<Saxon.Eej.ee.schema.Particle> particles = getSimplifiedContentModel();
            int len = particles.Count;
            minima = new int[len];
            maxima = new int[len];
            elementNames = new Saxon.Hej.om.StructuredQName[len];
            if ((particles.Count<8)) {
                counterMap = new Saxon.Hej.z.IntToIntArrayMap((particles.Count+1));
            } else {
                counterMap = new Saxon.Hej.z.IntToIntHashMap((particles.Count+1));
            }
            declarationMap = new Saxon.Hej.z.IntHashMap<Saxon.Eej.ee.schema.ElementDecl>((particles.Count+1));
            int wildcardIndex = 0;
            int elementIndex = ((wildcards == null) ? 0 : wildcards.Count);
            foreach (Saxon.Eej.ee.schema.Particle particle in particles) {
                if ((particle is Saxon.Eej.ee.schema.ElementParticle)) {
                    minima[elementIndex] = particle.getMinOccurs();
                    maxima[elementIndex] = particle.getMaxOccurs();
                    Saxon.Eej.ee.schema.ElementDecl decl = ((Saxon.Eej.ee.schema.ElementParticle)(particle)).getDeclaration();
                    int key = decl.getFingerprint();
                    elementNames[elementIndex] = particle.getTargetComponentName();
                    counterMap.put(key, elementIndex);
                    declarationMap.put(key, decl);
                    System.Collections.Generic.ICollection<Saxon.Eej.ee.schema.ElementDecl> group = decl.getSubstitutionGroupMembers();
                    foreach (Saxon.Eej.ee.schema.ElementDecl substituteDecl in group) {
                        int fp = substituteDecl.getFingerprint();
                        counterMap.put(fp, elementIndex);
                        declarationMap.put(fp, substituteDecl);
                    }
                    elementIndex++;
                } else {
                    minima[wildcardIndex] = particle.getMinOccurs();
                    maxima[wildcardIndex] = particle.getMaxOccurs();
                    wildcardIndex++;
                }
            }
        }
        public int[] getMaxima() {
            return maxima;
        }
        public int[] getMinima() {
            return minima;
        }
        public Saxon.Hej.om.StructuredQName[] getNameCodes() {
            return elementNames;
        }
        public Saxon.Hej.z.IntToIntMap getCounterMap() {
            return counterMap;
        }
        public Saxon.Hej.z.IntHashMap<Saxon.Eej.ee.schema.ElementDecl> getDeclarationMap() {
            return declarationMap;
        }
        public System.Collections.Generic.IList<Saxon.Eej.ee.schema.ElementWildcard> getWildcards() {
            if ((wildcards == null)) {
                return new System.Collections.Generic.List<Saxon.Eej.ee.schema.ElementWildcard>();
            } else {
                return wildcards;
            }
        }
        protected internal static string allSubsumes(Saxon.Eej.ee.schema.UserComplexType @base, Saxon.Hej.type.ComplexType sub, Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            Saxon.Eej.ee.schema.ModelGroupParticle all = @base.getAllCompositor();
            if ((sub is Saxon.Eej.ee.schema.UserComplexType)) {
                Saxon.Eej.ee.schema.Particle subp = ((Saxon.Eej.ee.schema.UserComplexType)(sub)).makeExtendedParticle(compiler);
                return subsumesParticle(all, subp, compiler);
            } else {
                return (("Type "+sub.getDescription())+" is not a valid restriction of a type defined using <xs:all>");
            }
        }
        private static string subsumesParticle(Saxon.Eej.ee.schema.ModelGroupParticle all, Saxon.Eej.ee.schema.Particle subp, Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            while ((((subp is Saxon.Eej.ee.schema.ModelGroupParticle) && (subp.getMinOccurs() == 1)) && (subp.getMaxOccurs() == 1))) {
                if ((((Saxon.Eej.ee.schema.ModelGroupParticle)(subp)).getGroup().getSimplifiedContentModel().Count == 1)) {
                    subp = ((Saxon.Eej.ee.schema.ModelGroupParticle)(subp)).getGroup().getSimplifiedContentModel()[0];
                } else {
                    break ;
                }
            }
            if (((subp is Saxon.Eej.ee.schema.ElementParticle) || (subp is Saxon.Eej.ee.schema.ElementWildcard))) {
                Saxon.Eej.ee.schema.SequenceModelGroup group = new Saxon.Eej.ee.schema.SequenceModelGroup();
                group.addParticle(subp);
                return ((Saxon.Eej.ee.schema.AllModelGroup)(all.getGroup())).subsumesAllOrSequence(group, compiler);
            } else {
                if ((subp is Saxon.Eej.ee.schema.ModelGroupParticle)) {
                    Saxon.Eej.ee.schema.ModelGroup subg = ((Saxon.Eej.ee.schema.ModelGroupParticle)(subp)).getGroup();
                    if ((all.getMinOccurs()>subp.getMinOccurs())) {
                        return (((("The derived type has a lower minOccurs ("+subp.getMinOccurs())+") than the base type (")+all.getMinOccurs())+")");
                    }
                    if (((all.getMaxOccurs()>0) && (all.getMaxOccurs()<subp.getMaxOccurs()))) {
                        return (((("The derived type has a greater maxOccurs ("+subp.getMaxOccurs())+") than the base type (")+all.getMaxOccurs())+")");
                    }
                    if ((((subg is Saxon.Eej.ee.schema.AllModelGroup) || (subg is Saxon.Eej.ee.schema.SequenceModelGroup)) || (subg is Saxon.Eej.ee.schema.ModelGroupDefinition))) {
                        return ((Saxon.Eej.ee.schema.AllModelGroup)(all.getGroup())).subsumesAllOrSequence(subg, compiler);
                    } else {
                        if ((subg is Saxon.Eej.ee.schema.ChoiceModelGroup)) {
                            Saxon.Eej.ee.schema.ChoiceModelGroup choice = (Saxon.Eej.ee.schema.ChoiceModelGroup)(subg);
                            int branchNr = 1;
                            foreach (Saxon.Eej.ee.schema.Particle branch in choice.getParticles()) {
                                string result = subsumesParticle(all, branch, compiler);
                                if ((result != null)) {
                                    return (((((branchNr == 1) ? "The first branch" : ("Branch "+branchNr)))+" of the choice is not a valid restriction of the content model of the base type. ")+result);
                                }
                                branchNr++;
                            }
                            return null;
                        } else {
                            throw new Saxon.Ejava.lang.AssertionError(("Unknown class of ModelGroup "+subg.GetType().Name));
                        }
                    }
                } else {
                    throw new Saxon.Ejava.lang.AssertionError(("Unknown class of particle "+subp.GetType().Name));
                }
            }
        }
        private string subsumesAllOrSequence(Saxon.Eej.ee.schema.ModelGroup subg, Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            System.Collections.Generic.IList<Saxon.Eej.ee.schema.Particle> baseParticles = getSimplifiedContentModel();
            Saxon.Hej.z.IntHashMap<Saxon.Eej.ee.schema.ElementParticle> baseElements = new Saxon.Hej.z.IntHashMap<Saxon.Eej.ee.schema.ElementParticle>(baseParticles.Count);
            int[] minCounters = new int[getMinima().Length];
            int[] maxCounters = new int[getMaxima().Length];
            Saxon.Eej.ee.schema.ElementParticle[] baseParticleArray = new Saxon.Eej.ee.schema.ElementParticle[getMinima().Length];
            foreach (Saxon.Eej.ee.schema.Particle p in baseParticles) {
                if ((p is Saxon.Eej.ee.schema.ElementParticle)) {
                    int fp = ((Saxon.Eej.ee.schema.ElementParticle)(p)).getDeclaration().getFingerprint();
                    baseElements.put(fp, (Saxon.Eej.ee.schema.ElementParticle)(p));
                    baseParticleArray[counterMap.get(fp)] = (Saxon.Eej.ee.schema.ElementParticle)(p);
                }
            }
            System.Collections.Generic.IList<Saxon.Eej.ee.schema.Particle> derivedParticles = subg.getSimplifiedContentModel();
            derivedParticles = flattenParticleList(derivedParticles, 1, 1);
            for (int i = 0; (i<derivedParticles.Count); i++) {
                if ((derivedParticles[i] is Saxon.Eej.ee.schema.ElementParticle)) {
                    Saxon.Eej.ee.schema.ElementParticle derivedElement = (Saxon.Eej.ee.schema.ElementParticle)(derivedParticles[i]);
                    int derivedfp = derivedElement.getDeclaration().getFingerprint();
                    int counterIndex = counterMap.get(derivedfp);
                    if ((counterIndex == counterMap.getDefaultValue())) {
                        string clarkName = derivedElement.getDisplayName();
                        return (("The derived type allows element "+Saxon.Hej.trans.Err.wrap(clarkName, Saxon.Hej.trans.Err.ELEMENT))+" but the base type does not");
                    }
                    minCounters[counterIndex] += derivedElement.getMinOccurs();
                    if ((derivedElement.getMaxOccurs() == -1)) {
                        if ((getMaxima()[counterIndex] != -1)) {
                            string clarkName = derivedElement.getDisplayName();
                            return (("The derived type allows unlimited occurrences of element "+Saxon.Hej.trans.Err.wrap(clarkName, Saxon.Hej.trans.Err.ELEMENT))+" but the base type does not");
                        }
                    } else {
                        maxCounters[counterIndex] += derivedElement.getMaxOccurs();
                        int baseMax = getMaxima()[counterIndex];
                        if (((baseMax != -1) && (maxCounters[counterIndex]>baseMax))) {
                            string clarkName = derivedElement.getDisplayName();
                            return (((((("The derived type allows more occurrences ("+maxCounters[counterIndex])+") of element ")+Saxon.Hej.trans.Err.wrap(clarkName, Saxon.Hej.trans.Err.ELEMENT))+" than the base type allows (")+baseMax)+")");
                        }
                    }
                    Saxon.Eej.ee.schema.ElementParticle baseParticle = baseParticleArray[counterIndex];
                    Saxon.Eej.ee.schema.ElementDecl baseElement = baseParticle.getDeclaration();
                    string err = /*134*/Saxon.Eej.ee.schema.UserComplexType.elementParticleMatches(baseElement, derivedElement.getDeclaration(), compiler);
                    if ((err != null)) {
                        return err;
                    }
                    try {
                        derivedElement.getDeclaration().getType().checkTypeDerivationIsOK(baseElement.getType(), baseElement.getType().getBlock());
                    } catch (Saxon.Hej.type.SchemaException err2) {
                        string clarkName = derivedElement.getDisplayName();
                        return (("The type of element "+Saxon.Hej.trans.Err.wrap(clarkName, Saxon.Hej.trans.Err.ELEMENT))+" in the derived type is not validly derived from the type of the corresponding element in the base type");
                    }
                } else {
                    if ((derivedParticles[i] is Saxon.Eej.ee.schema.ElementWildcard)) {
                        Saxon.Eej.ee.schema.ElementWildcard wp = (Saxon.Eej.ee.schema.ElementWildcard)(derivedParticles[i]);
                        Saxon.Eej.ee.schema.Wildcard w = wp.getWildcard();
                        int counterIndex = 0;
                        Saxon.Eej.ee.schema.Wildcard union = null;
                        int unionMax = 0;
                        foreach (Saxon.Eej.ee.schema.Particle baseParticle in baseParticles) {
                            if ((baseParticle is Saxon.Eej.ee.schema.ElementWildcard)) {
                                Saxon.Eej.ee.schema.ElementWildcard vp = (Saxon.Eej.ee.schema.ElementWildcard)(baseParticle);
                                Saxon.Eej.ee.schema.Wildcard v = vp.getWildcard();
                                if (v.overlaps(w)) {
                                    if ((union == null)) {
                                        union = v;
                                    } else {
                                        union = /*134*/Saxon.Eej.ee.schema.Wildcard.makeUnion(union, v, compiler.getNamePool());
                                    }
                                    if (union.isInexpressible()) {
                                        return "The union of the wildcards in the base type that match a wildcard in the derived type is inexpressible";
                                    }
                                    if (((vp.getMaxOccurs() == -1) || (unionMax == -1))) {
                                        unionMax = -1;
                                    } else {
                                        unionMax += vp.getMaxOccurs();
                                    }
                                    if ((wp.getMaxOccurs() != -1)) {
                                        maxCounters[counterIndex] += wp.getMaxOccurs();
                                        int baseMax = vp.getMaxOccurs();
                                        if (((baseMax != -1) && (maxCounters[counterIndex]>baseMax))) {
                                            return ((((("The derived type allows more occurrences ("+maxCounters[counterIndex])+") of a wildcard ")+"than the base type allows (")+baseMax)+")");
                                        }
                                    }
                                    if ((v.compareStrength(w)>0)) {
                                        return ("A wildcard in the derived type has a less strict processContents than the"+" corresponding wildcard in the base type");
                                    }
                                }
                                if (w.isSubset(v, getConfiguration().getNamePool())) {
                                    minCounters[counterIndex] += wp.getMinOccurs();
                                }
                                counterIndex++;
                            } else {
                                if ((baseParticle is Saxon.Eej.ee.schema.ElementParticle)) {
                                    Saxon.Eej.ee.schema.ElementParticle ep = (Saxon.Eej.ee.schema.ElementParticle)(baseParticle);
                                    if (wp.getWildcard().matches(ep.getTargetComponentName(), true, getConfiguration(), null)) {
                                        bool found = false;
                                        foreach (Saxon.Eej.ee.schema.Particle derivedParticle in derivedParticles) {
                                            if (((derivedParticle is Saxon.Eej.ee.schema.ElementParticle) && derivedParticle.getTargetComponentName().Equals(ep.getTargetComponentName()))) {
                                                found = true;
                                            }
                                        }
                                        if ((!(found) && (ep.getDeclaration().getType() != Saxon.Hej.type.AnyType.getInstance()))) {
                                            return (((("The derived type contains a wildcard which matches elements named "+Saxon.Hej.trans.Err.wrap(ep.getDisplayName(), Saxon.Hej.trans.Err.ELEMENT))+" but which allows element content that the element particle for ")+Saxon.Hej.trans.Err.wrap(ep.getDisplayName(), Saxon.Hej.trans.Err.ELEMENT))+" in the base type does not allow");
                                        }
                                    }
                                }
                            }
                        }
                        if ((unionMax != -1)) {
                            if ((unionMax == 0)) {
                                return "A wildcard in the derived type allows elements that are not permitted by the base type";
                            } else {
                                if ((wp.getMaxOccurs() == -1)) {
                                    return "A wildcard in the derived type allows unlimited occurrences, but the base type imposes a limit";
                                } else {
                                    if ((wp.getMaxOccurs()>unionMax)) {
                                        return (((("A wildcard in the derived type allows more occurrences ("+wp.getMaxOccurs())+") than the maximum allowed by the base type (")+unionMax)+")");
                                    }
                                }
                            }
                        }
                        if (!(w.isSubset(union, compiler.getNamePool()))) {
                            return "A wildcard in the derived type allows elements that do not match any wildcard in the base type";
                        }
                    } else {
                        return ("?A type derived by restriction from <xs:all> must only contain element particles and "+"wildcard particles in its content model");
                    }
                }
            }
            for (int i = 0; (i<baseParticles.Count); i++) {
                Saxon.Eej.ee.schema.Particle p = baseParticles[i];
                if ((p is Saxon.Eej.ee.schema.ElementParticle)) {
                    int counterIndex = counterMap.get(((Saxon.Eej.ee.schema.ElementParticle)(p)).getDeclaration().getFingerprint());
                    if ((minCounters[counterIndex]<p.getMinOccurs())) {
                        string clarkName = p.getTargetComponentName().getDisplayName();
                        if ((minCounters[counterIndex] == 0)) {
                            if ((maxCounters[counterIndex] == 0)) {
                                return (("Element "+Saxon.Hej.trans.Err.wrap(clarkName, Saxon.Hej.trans.Err.ELEMENT))+" is mandatory in the base type, but is not permitted in the derived type");
                            } else {
                                return (("Element "+Saxon.Hej.trans.Err.wrap(clarkName, Saxon.Hej.trans.Err.ELEMENT))+" is mandatory in the base type, but is not mandatory in the derived type");
                            }
                        } else {
                            return (((((("Element "+Saxon.Hej.trans.Err.wrap(clarkName, Saxon.Hej.trans.Err.ELEMENT))+" allows a smaller number of occurrences (")+minCounters[i])+") in the derived type than the minimum required by the base type (")+p.getMinOccurs())+")");
                        }
                    }
                }
            }
            if ((wildcards != null)) {
                for (int i = 0; (i<wildcards.Count); i++) {
                    if ((minCounters[i]<wildcards[i].getMinOccurs())) {
                        if ((minCounters[i] == 0)) {
                            return "A wildcard is mandatory in the base type, but is not permitted in the derived type";
                        } else {
                            return ((((("A wildcard in the base type allows a "+"smaller number of occurrences (")+minCounters[i])+") in the derived type than the minimum required by the base type (")+wildcards[i].getMinOccurs())+")");
                        }
                    }
                }
            }
            return null;
        }
        private System.Collections.Generic.IList<Saxon.Eej.ee.schema.Particle> flattenParticleList(System.Collections.Generic.IList<Saxon.Eej.ee.schema.Particle> particles, int min, int max) {
            if (((min == 1) && (max == 1))) {
                bool alreadyFlat = true;
                foreach (Saxon.Eej.ee.schema.Particle p in particles) {
                    if ((p is Saxon.Eej.ee.schema.ModelGroupParticle)) {
                        alreadyFlat = false;
                        break ;
                    }
                }
                if (alreadyFlat) {
                    return particles;
                }
            }
            System.Collections.Generic.IList<Saxon.Eej.ee.schema.Particle> p2 = new System.Collections.Generic.List<Saxon.Eej.ee.schema.Particle>();
            foreach (Saxon.Eej.ee.schema.Particle p in particles) {
                if ((p is Saxon.Eej.ee.schema.ModelGroupParticle)) {
                    Saxon.Eej.ee.schema.ModelGroup group = (Saxon.Eej.ee.schema.ModelGroup)(p.getTarget());
                    int newMin = (min*p.getMinOccurs());
                    if ((group is Saxon.Eej.ee.schema.ChoiceModelGroup)) {
                        newMin = 0;
                    }
                    int newMax;
                    if (((max == -1) || (p.getMaxOccurs() == -1))) {
                        newMax = -1;
                    } else {
                        newMax = (max*p.getMaxOccurs());
                    }
                    System.Collections.Generic.IList<Saxon.Eej.ee.schema.Particle> mp = flattenParticleList(group.getSimplifiedContentModel(), newMin, newMax);
                    Saxon.Impl.Helpers.ListUtils.AddAll(p2, mp);
                } else {
                    if (((min == 1) && (max == 1))) {
                        p2.Add(p);
                    } else {
                        if ((p is Saxon.Eej.ee.schema.ElementParticle)) {
                            Saxon.Eej.ee.schema.ElementParticle e2 = ((Saxon.Eej.ee.schema.ElementParticle)(p)).copy();
                            e2.setMinOccurs((p.getMinOccurs()*min));
                            e2.setMaxOccurs((((max == -1) || (p.getMaxOccurs() == -1)) ? -1 : (p.getMaxOccurs()*max)));
                            p2.Add(e2);
                        } else {
                            if ((p is Saxon.Eej.ee.schema.ElementWildcard)) {
                                Saxon.Eej.ee.schema.ElementWildcard e2 = ((Saxon.Eej.ee.schema.ElementWildcard)(p)).copy();
                                e2.setMinOccurs((p.getMinOccurs()*min));
                                e2.setMaxOccurs((((max == -1) || (p.getMaxOccurs() == -1)) ? -1 : (p.getMaxOccurs()*max)));
                                p2.Add(e2);
                            }
                        }
                    }
                }
            }
            return p2;
        }
    
    }
}