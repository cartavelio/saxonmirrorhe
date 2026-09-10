using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema {
    public abstract class ModelGroup : Saxon.Eej.ee.schema.SchemaStructure, Saxon.Eej.ee.schema.UserSchemaComponent, Saxon.Eej.ee.schema.Term {
        protected internal System.Collections.Generic.IList<Saxon.Eej.ee.schema.Particle> contentModel;
        protected internal System.Collections.Generic.IList<Saxon.Eej.ee.schema.Particle> simplifiedContentModel;
        public ModelGroup()  {
            contentModel = new System.Collections.Generic.List<Saxon.Eej.ee.schema.Particle>(20);
        }
        public abstract string getCompositorName();
        public virtual void addParticle(Saxon.Eej.ee.schema.Particle particle) {
            if (((particle != null) && (((particle.getMinOccurs() != 0) || (particle.getMaxOccurs() != 0))))) {
                contentModel.Add(particle);
            }
        }
        public void setContentModel(System.Collections.Generic.IList<Saxon.Eej.ee.schema.Particle> particles) {
            contentModel = particles;
        }
        public abstract bool isEmptiable();
        public virtual bool isPointless(Saxon.Eej.ee.schema.ModelGroup container) {
            return getSimplifiedContentModel().Count==0;
        }
        public System.Collections.Generic.IList<Saxon.Eej.ee.schema.Particle> getParticles() {
            return contentModel;
        }
        public int getContentModelSize() {
            return contentModel.Count;
        }
        public bool isEmpty() {
            return getParticles().Count==0;
        }
        public virtual bool containsAll(bool simplified) {
            System.Collections.Generic.IList<Saxon.Eej.ee.schema.Particle> particles = (simplified ? getSimplifiedContentModel() : contentModel);
            foreach (Saxon.Eej.ee.schema.Particle p in particles) {
                if (((p is Saxon.Eej.ee.schema.ModelGroupParticle) && ((Saxon.Eej.ee.schema.ModelGroupParticle)(p)).getGroup().containsAll(simplified))) {
                    return true;
                }
            }
            return false;
        }
        public virtual System.Collections.Generic.IList<Saxon.Eej.ee.schema.Particle> getSimplifiedContentModel() {
            if ((simplifiedContentModel == null)) {
                simplifiedContentModel = new System.Collections.Generic.List<Saxon.Eej.ee.schema.Particle>(contentModel.Count);
                foreach (Saxon.Eej.ee.schema.Particle p in contentModel) {
                    if ((p is Saxon.Eej.ee.schema.ModelGroupParticle)) {
                        Saxon.Eej.ee.schema.ModelGroup mgd = ((Saxon.Eej.ee.schema.ModelGroupParticle)(p)).getGroup();
                        if (p.isPointless(this)) {
                            Saxon.Impl.Helpers.ListUtils.AddAll(simplifiedContentModel, mgd.getSimplifiedContentModel());
                        } else {
                            if (((this is Saxon.Eej.ee.schema.AllModelGroup) && (mgd is Saxon.Eej.ee.schema.AllModelGroup))) {
                                Saxon.Impl.Helpers.ListUtils.AddAll(simplifiedContentModel, mgd.getSimplifiedContentModel());
                            } else {
                                simplifiedContentModel.Add(p);
                            }
                        }
                    } else {
                        simplifiedContentModel.Add(p);
                    }
                }
            }
            return simplifiedContentModel;
        }
        public virtual void markVulnerableSubParticles(bool isRepeatable) {
            if (isRepeatable) {
                int numberOfOptionalSubParticles = 0;
                foreach (Saxon.Eej.ee.schema.Particle p in contentModel) {
                    if (((p.getMinOccurs() == 0) || p.isEmptiable())) {
                        numberOfOptionalSubParticles++;
                    }
                }
                if ((numberOfOptionalSubParticles == contentModel.Count)) {
                    foreach (Saxon.Eej.ee.schema.Particle p in contentModel) {
                        p.setVulnerable(true);
                    }
                } else {
                    if ((numberOfOptionalSubParticles == (contentModel.Count-1))) {
                        foreach (Saxon.Eej.ee.schema.Particle p in contentModel) {
                            if (!((((p.getMinOccurs() == 0) || p.isEmptiable())))) {
                                p.setVulnerable(true);
                                break ;
                            }
                        }
                    }
                }
            }
            foreach (Saxon.Eej.ee.schema.Particle p in contentModel) {
                if ((p is Saxon.Eej.ee.schema.ModelGroupParticle)) {
                    p.markVulnerableSubParticles();
                }
            }
        }
        public override void lookForCycles(System.Collections.Generic.Stack<Saxon.Hej.type.SchemaComponent> references, Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            references.Push(this);
            foreach (Saxon.Eej.ee.schema.Particle p in getParticles()) {
                p.lookForCycles(references, compiler);
            }
            references.Pop();
        }
        public override bool validate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            bool result = true;
            foreach (Saxon.Eej.ee.schema.Particle p in getParticles()) {
                if ((p is Saxon.Eej.ee.schema.ModelGroupParticle)) {
                    Saxon.Eej.ee.schema.ModelGroupParticle @ref = (Saxon.Eej.ee.schema.ModelGroupParticle)(p);
                    @ref.tryToResolve(compiler, false);
                    if (@ref.isResolved()) {
                        result &= @ref.getGroup().validate(compiler);
                    } else {
                        string err = (("Group "+@ref.getTargetComponentName().getDisplayName())+" is referenced, but has not been declared");
                        compiler.error(err, this);
                        setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INCOMPLETE);
                        return false;
                    }
                } else {
                    result &= p.validate(compiler);
                }
            }
            if (!(result)) {
                setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                return false;
            }
            Saxon.Hej.z.IntHashMap<Saxon.Eej.ee.schema.ElementDecl> elements = new Saxon.Hej.z.IntHashMap<Saxon.Eej.ee.schema.ElementDecl>(20);
            result &= checkElements(elements, compiler);
            if (!(result)) {
                setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                return false;
            }
            return true;
        }
        protected internal virtual bool checkElements(Saxon.Hej.z.IntHashMap<Saxon.Eej.ee.schema.ElementDecl> map, Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            System.Collections.Generic.ISet<Saxon.Eej.ee.schema.ElementWildcard> wildcards = new System.Collections.Generic.HashSet<Saxon.Eej.ee.schema.ElementWildcard>();
            foreach (Saxon.Eej.ee.schema.Particle p in getParticles()) {
                if ((p is Saxon.Eej.ee.schema.ModelGroupParticle)) {
                    bool b = ((Saxon.Eej.ee.schema.ModelGroupParticle)(p)).getGroup().checkElements(map, compiler);
                    if (!(b)) {
                        return false;
                    }
                } else {
                    if ((p is Saxon.Eej.ee.schema.ElementWildcard)) {
                        wildcards.Add((Saxon.Eej.ee.schema.ElementWildcard)(p));
                    } else {
                        if ((p is Saxon.Eej.ee.schema.ElementParticle)) {
                            if (!(p.isResolved())) {
                                string err = ("Unresolved reference to element "+Saxon.Hej.trans.Err.wrap(p.getTargetComponentName().getDisplayName(), Saxon.Hej.trans.Err.ELEMENT));
                                compiler.error(err, this);
                                return false;
                            }
                            Saxon.Eej.ee.schema.ElementDecl e = ((Saxon.Eej.ee.schema.ElementParticle)(p)).getDeclaration();
                            int key = e.getFingerprint();
                            Saxon.Eej.ee.schema.ElementDecl prev = map.get(key);
                            if ((prev == null)) {
                                map.put(key, e);
                            } else {
                                if (!(e.getType().Equals(prev.getType()))) {
                                    string err = ((("Two elements in the content model have the same name "+Saxon.Hej.trans.Err.wrap(e.getDisplayName(), Saxon.Hej.trans.Err.ELEMENT))+" but different types. ")+"This violates the constraint 'Element Declarations Consistent'.");
                                    compiler.error(err, this);
                                    return false;
                                }
                                if (!(e.hasSameTypeTable(prev))) {
                                    string err = ((("Two elements in the content model have the same name "+Saxon.Hej.trans.Err.wrap(e.getDisplayName(), Saxon.Hej.trans.Err.ELEMENT))+" but different type alternatives. ")+"This violates the constraint 'Element Declarations Consistent'.");
                                    compiler.error(err, this);
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
            System.Collections.Generic.IEnumerator<Saxon.Eej.ee.schema.ElementDecl> iterM = map.valueIterator();
            while (iterM.MoveNext() /*UNSAFE CONVERSION*/) {
                Saxon.Eej.ee.schema.ElementDecl e = iterM.Current;
                if (e.isGlobal()) {
                    foreach (Saxon.Eej.ee.schema.ElementDecl member in e.getSubstitutionGroupMembers()) {
                        Saxon.Eej.ee.schema.ElementDecl local = map.get(member.getFingerprint());
                        if (((local != null) && (local != member))) {
                            if (!(e.getType().isSameType(local.getType()))) {
                                string err = (((((("An element "+Saxon.Hej.trans.Err.wrap(member.getDisplayName(), Saxon.Hej.trans.Err.ELEMENT))+" appears in the content model both as a local element and also as ")+"a member of the substitution group of element ")+Saxon.Hej.trans.Err.wrap(e.getDisplayName(), Saxon.Hej.trans.Err.ELEMENT))+", and the two element declarations have different types. ")+"This violates the constraint 'Element Declarations Consistent'.");
                                compiler.error(err, this);
                                setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                                return false;
                            }
                            if (!(e.hasSameTypeTable(local))) {
                                string err = (((((("An element "+Saxon.Hej.trans.Err.wrap(member.getDisplayName(), Saxon.Hej.trans.Err.ELEMENT))+" appears in the content model both as a local element and also as ")+"a member of the substitution group of element ")+Saxon.Hej.trans.Err.wrap(e.getDisplayName(), Saxon.Hej.trans.Err.ELEMENT))+", and the two element declarations have different type alternatives. ")+"This violates the constraint 'Element Declarations Consistent'.");
                                compiler.error(err, this);
                                return false;
                            }
                        }
                    }
                }
            }
            if ((wildcards.Count==0 || (compiler.getConfiguration().getXsdVersion() == Saxon.Hej.Configuration.XSD10))) {
                return true;
            }
            iterM = map.valueIterator();
            while (iterM.MoveNext() /*UNSAFE CONVERSION*/) {
                Saxon.Eej.ee.schema.ElementDecl local = iterM.Current;
                if (!(local.isGlobal())) {
                    Saxon.Eej.ee.schema.ElementDecl global = compiler.getPreparedSchema().getElementDecl(local.getComponentName());
                    if ((global != null)) {
                        foreach (Saxon.Eej.ee.schema.ElementWildcard wild in wildcards) {
                            Saxon.Eej.ee.schema.Wildcard card = wild.getWildcard();
                            if ((!(card.getProcessContents().Equals("skip")) && card.matches(global.getComponentName(), true, getConfiguration(), null))) {
                                if (!(local.hasSameTypeTable(global))) {
                                    string err = (((((("A local element "+Saxon.Hej.trans.Err.wrap(local.getDisplayName(), Saxon.Hej.trans.Err.ELEMENT))+" appears in the content model that has the same name as a global element ")+"declaration that could match a strict or lax wildcard in the content model. ")+"The table of type alternatives differs between the local element declaration ")+"and the global element declaration, which violates the Element Declarations ")+"Consistent constraint.");
                                    compiler.error(err, this);
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }
        public Saxon.Hej.type.SchemaType getElementParticleType(int elementName) {
            foreach (Saxon.Eej.ee.schema.Particle p in getParticles()) {
                Saxon.Hej.type.SchemaType t = p.getElementParticleType(elementName);
                if ((t != null)) {
                    return t;
                }
            }
            return null;
        }
        public int getElementParticleCardinality(int elementName) {
            int card = Saxon.Hej.expr.StaticProperty.EMPTY;
            foreach (Saxon.Eej.ee.schema.Particle p in getParticles()) {
                int t = p.getElementParticleCardinality(elementName);
                if ((t != Saxon.Hej.expr.StaticProperty.EMPTY)) {
                    if ((this is Saxon.Eej.ee.schema.SequenceModelGroup)) {
                        card = Saxon.Hej.value.Cardinality.sum(card, t);
                    } else {
                        card = Saxon.Hej.value.Cardinality.union(card, t);
                    }
                }
            }
            return card;
        }
        public bool containsElementWildcard() {
            foreach (Saxon.Eej.ee.schema.Particle p in getParticles()) {
                if (p.containsElementWildcard()) {
                    return true;
                }
            }
            return false;
        }
        public void gatherAllPermittedElements(Saxon.Hej.z.IntHashSet result, bool ignoreWildcards) {
            foreach (Saxon.Eej.ee.schema.Particle p in getParticles()) {
                p.gatherAllPermittedElements(result, ignoreWildcards);
            }
        }
        public void compile(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            foreach (Saxon.Eej.ee.schema.Particle p in getParticles()) {
                p.compile(compiler);
            }
        }
        public override bool fixup(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            if ((getFixupStatus() == /*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.UNVALIDATED)) {
                setFixupStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.VALIDATING);
                foreach (Saxon.Eej.ee.schema.Particle p in contentModel) {
                    p.fixup(compiler);
                }
                lookForCycles(new System.Collections.Generic.Stack<Saxon.Hej.type.SchemaComponent>(), compiler);
            }
            setFixupStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.VALIDATED);
            return true;
        }
        public Saxon.Eej.ee.schema.Term getTerm() {
            return this;
        }
        public virtual void serialize(Saxon.Eej.ee.schema.SchemaModelSerializer serializer) {
            string compositor = getCompositorName();
            serializer.startElement(compositor);
            foreach (Saxon.Eej.ee.schema.Particle p in getParticles()) {
                p.serializeParticle(serializer);
            }
            serializer.endElement();
        }
        public override Saxon.Hej.om.FunctionItem getComponentAsFunction() {
            Saxon.Eej.ee.schema.ModelGroup thisModelGroup = this;
            Saxon.Hej.expr.CallableDelegate callable = new Saxon.Hej.expr.CallableDelegate((context, arguments) => {
                string key = arguments[0].head().getStringValue();
                switch (key) {
                  case "class":
                    return Saxon.Hej.value.StringValue.bmp("Model Group");
                  case "implementation":
                    return new Saxon.Hej.value.ObjectValue<Saxon.Eej.ee.schema.ModelGroup>(thisModelGroup);
                  case "compositor":
                    return new Saxon.Hej.value.StringValue(getCompositorName());
                  case "particles":
                    System.Collections.Generic.IList<Saxon.Hej.om.Item> particles = new System.Collections.Generic.List<Saxon.Hej.om.Item>();
                    foreach (Saxon.Eej.ee.schema.Particle p in getParticles()) {
                        particles.Add(p.getComponentAsFunction());
                    }
                    return Saxon.Hej.value.SequenceExtent.makeSequenceExtent(particles);
                  default:
                    return Saxon.Hej.value.EmptySequence.getInstance();
                }
            } );
            return new Saxon.Hej.functions.CallableFunction(1, callable, Saxon.Hej.type.SpecificFunctionType.COMPONENT_FUNCTION_TYPE);
        }
    
    }
}