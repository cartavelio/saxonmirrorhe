using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema {
    public class ModelGroupParticle : Saxon.Eej.ee.schema.Particle {
        public ModelGroupParticle(Saxon.Eej.config.EnterpriseConfiguration config, Saxon.Ejavax.xml.transform.SourceLocator locator)  {
            setLocator(locator);
            setConfiguration(config);
        }
        public override bool isResolved() {
            return ((target != null));
        }
        public Saxon.Eej.ee.schema.ModelGroup getGroup() {
            return (Saxon.Eej.ee.schema.ModelGroup)(getTarget());
        }
        public override int getSymbolSpace() {
            return Saxon.Eej.ee.schema.SymbolSpace.GROUP;
        }
        public override bool isEmptiable() {
            return ((getMinOccurs() == 0) || getGroup().isEmptiable());
        }
        public override bool fixup(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            Saxon.Hej.type.SchemaComponent decl = /*134*/Saxon.Eej.ee.schema.PreparedSchema.validateReference(this, compiler, false);
            if ((decl == null)) {
                setFixupStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                return false;
            }
            lookForCycles(new System.Collections.Generic.Stack<Saxon.Hej.type.SchemaComponent>(), compiler);
            setFixupStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.VALIDATED);
            return true;
        }
        public override void lookForCycles(System.Collections.Generic.Stack<Saxon.Hej.type.SchemaComponent> references, Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            if (references.Contains(this)) {
                string err = (("The definition of group "+Saxon.Hej.trans.Err.wrap(getTargetComponentName().getDisplayName()))+" is circular");
                throw new Saxon.Hej.type.SchemaException(err);
            }
            tryToResolve(compiler, false);
            if (isResolved()) {
                getGroup().lookForCycles(references, compiler);
            }
        }
        public override Saxon.Hej.type.SchemaComponent getTarget() {
            if ((target == null)) {
                throw new Saxon.Eej.ee.schema.MissingComponentExceptionImpl(this);
            }
            return target;
        }
        public override void tryToResolve(Saxon.Eej.ee.schema.SchemaCompiler compiler, bool fatal) {
            if ((target == null)) {
                target = compiler.getPreparedSchema().getGroup(getTargetComponentName());
            }
            if ((target == null)) {
                target = compiler.getConfiguration().getSuperSchema().getGroup(getTargetComponentName());
            }
            if ((target == null)) {
                string name = getTargetComponentName().getEQName();
                if ((compiler.isAllowDanglingReferences() && !(fatal))) {
                    compiler.warning((("Unknown model group "+name)+" (substituting empty group)"), this);
                    Saxon.Eej.ee.schema.ModelGroupDefinition decl = new Saxon.Eej.ee.schema.ModelGroupDefinition(getConfiguration(), this);
                    decl.setModelGroupName(getTargetComponentName());
                    target = decl;
                } else {
                    throw new Saxon.Hej.type.SchemaException(("Unknown attribute declaration "+name), this);
                }
            }
        }
        public override bool isPointless(Saxon.Eej.ee.schema.ModelGroup container) {
            return (((getMinOccurs() == 1) && (getMaxOccurs() == 1)) && ((Saxon.Eej.ee.schema.ModelGroup)(getTarget())).isPointless(container));
        }
        public override Saxon.Hej.type.SchemaType getElementParticleType(int elementName) {
            return getGroup().getElementParticleType(elementName);
        }
        public override int getElementParticleCardinality(int elementName) {
            int card = getGroup().getElementParticleCardinality(elementName);
            return Saxon.Hej.value.Cardinality.multiply(card, computeParticleCardinality(getMinOccurs(), getMaxOccurs()));
        }
        public override bool containsElementWildcard() {
            return getGroup().containsElementWildcard();
        }
        public override void gatherAllPermittedElements(Saxon.Hej.z.IntHashSet result, bool ignoreWildcards) {
            getGroup().gatherAllPermittedElements(result, ignoreWildcards);
        }
        public override void markVulnerableSubParticles() {
            getGroup().markVulnerableSubParticles(((getMaxOccurs()>1) || (getMaxOccurs() == -1)));
        }
        public override bool validate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            switch (getValidationStatus()) {
              case Saxon.Hej.type.SchemaValidationStatus.VALIDATED:
                return true;
              case Saxon.Hej.type.SchemaValidationStatus.INVALID:
                return false;
              case Saxon.Hej.type.SchemaValidationStatus.VALIDATING:
                return false;
              case Saxon.Hej.type.SchemaValidationStatus.UNVALIDATED:
                break ;
            }
            Saxon.Eej.ee.schema.ModelGroup decl = (Saxon.Eej.ee.schema.ModelGroup)(/*134*/Saxon.Eej.ee.schema.PreparedSchema.validateReference(this, compiler, false));
            if (isResolved()) {
                bool result = decl.validate(compiler);
                if (result) {
                    System.Collections.Generic.IList<Saxon.Eej.ee.schema.Particle> particles;
                    Saxon.Eej.ee.schema.Particle first;
                    if (((((((getMaxOccurs() == -1) || (getMaxOccurs()>1))) && !((particles = decl.getParticles()).Count==0)) && ((first = particles[0]) is Saxon.Eej.ee.schema.ModelGroupParticle)) && (first.getTerm() is Saxon.Eej.ee.schema.AllModelGroup))) {
                        compiler.error("A group defined using <xs:all> cannot occur more than once", this);
                        result = false;
                    }
                }
                if (((getMaxOccurs()>1) || (getMaxOccurs() == -1))) {
                    decl.markVulnerableSubParticles(true);
                }
                return result;
            } else {
                setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                return false;
            }
        }
        public override void compile(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            ((Saxon.Eej.ee.schema.ModelGroup)(getTarget())).compile(compiler);
        }
        public override Saxon.Eej.ee.schema.Term getTerm() {
            return getGroup();
        }
        public override void elaborate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            getGroup().elaborate(compiler);
        }
        public override void serializeParticle(Saxon.Eej.ee.schema.SchemaModelSerializer serializer) {
            serializer.startElement("modelGroupParticle");
            serializer.emitAttribute("minOccurs", (""+getMinOccurs()));
            serializer.emitAttribute("maxOccurs", ((getMaxOccurs()<0) ? "unbounded" : (""+getMaxOccurs())));
            if ((getGroup() is Saxon.Eej.ee.schema.ModelGroupDefinition)) {
                serializer.emitAttribute("ref", serializer.getId(getGroup(), false));
            } else {
                getGroup().serialize(serializer);
            }
            serializer.endElement();
        }
    
    }
}