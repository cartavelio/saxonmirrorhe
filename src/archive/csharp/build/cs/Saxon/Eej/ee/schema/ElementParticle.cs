using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema {
    public class ElementParticle : Saxon.Eej.ee.schema.Particle {
        public ElementParticle(Saxon.Eej.config.EnterpriseConfiguration config, Saxon.Ejavax.xml.transform.SourceLocator locator)  {
            setMinOccurs(1);
            setMaxOccurs(1);
            setLocator(locator);
            setConfiguration(config);
            symbolSpace = Saxon.Eej.ee.schema.SymbolSpace.ELEMENT;
        }
        public Saxon.Eej.ee.schema.ElementParticle copy() {
            Saxon.Eej.ee.schema.ElementParticle p = new Saxon.Eej.ee.schema.ElementParticle(getConfiguration(), null);
            p.setTargetComponentName(getTargetComponentName());
            p.setReference((Saxon.Eej.ee.schema.ElementDecl)(target));
            p.setMinOccurs(getMinOccurs());
            p.setMaxOccurs(getMaxOccurs());
            return p;
        }
        public string getDisplayName() {
            return getTargetComponentName().getDisplayName();
        }
        public Saxon.Hej.om.NamespaceUri getNamespaceURI() {
            return getTargetComponentName().getNamespaceUri();
        }
        public string getName() {
            return getTargetComponentName().getLocalPart();
        }
        public Saxon.Eej.ee.schema.ElementDecl getDeclaration() {
            return (Saxon.Eej.ee.schema.ElementDecl)(getTarget());
        }
        public void setReference(Saxon.Eej.ee.schema.ElementDecl declaration) {
            setTargetComponentName(declaration.getComponentName());
            setTarget(declaration);
        }
        public override bool isEmptiable() {
            return (getMinOccurs() == 0);
        }
        public override bool isPointless(Saxon.Eej.ee.schema.ModelGroup container) {
            return false;
        }
        public override void lookForCycles(System.Collections.Generic.Stack<Saxon.Hej.type.SchemaComponent> references, Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            tryToResolve(compiler, false);
        }
        public override bool fixup(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            bool result = true;
            if ((getFixupStatus() == /*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.UNVALIDATED)) {
                setFixupStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.VALIDATING);
                Saxon.Eej.ee.schema.ElementDecl decl = (Saxon.Eej.ee.schema.ElementDecl)(/*134*/Saxon.Eej.ee.schema.PreparedSchema.validateReference(this, compiler, false));
                if ((decl == null)) {
                    setFixupStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                    return false;
                }
                result = decl.fixup(compiler);
            }
            setFixupStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.VALIDATED);
            return result;
        }
        public override bool validate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            switch (getValidationStatus()) {
              case Saxon.Hej.type.SchemaValidationStatus.VALIDATED:
                return true;
              case Saxon.Hej.type.SchemaValidationStatus.INVALID:
              case Saxon.Hej.type.SchemaValidationStatus.INCOMPLETE:
                return false;
              case Saxon.Hej.type.SchemaValidationStatus.VALIDATING:
                return true;
              default:
                break ;
            }
            if (!(isResolved())) {
                tryToResolve(compiler, false);
                if (!(isResolved())) {
                    setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INCOMPLETE);
                    return false;
                }
            }
            setValidationStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.VALIDATING);
            bool result = getDeclaration().validate(compiler);
            setValidationStatus((result ? /*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.VALIDATED : /*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID));
            return result;
        }
        public override void compile(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            if ((getValidationStatus() != /*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.VALIDATED)) {
                return ;
            }
            Saxon.Hej.type.SchemaType type = getDeclaration().getType();
            if ((type is Saxon.Eej.ee.schema.UserComplexType)) {
                Saxon.Eej.ee.schema.UserComplexType complexType = (Saxon.Eej.ee.schema.UserComplexType)(type);
                complexType.compile(compiler);
            }
        }
        public override Saxon.Eej.ee.schema.Term getTerm() {
            return getDeclaration();
        }
        public override Saxon.Hej.type.SchemaType getElementParticleType(int elementFingerprint) {
            try {
                Saxon.Eej.ee.schema.ElementDecl decl = (Saxon.Eej.ee.schema.ElementDecl)(getTarget());
                if ((elementFingerprint == getTargetFingerprint())) {
                    return decl.getType();
                } else {
                    System.Collections.Generic.ICollection<Saxon.Eej.ee.schema.ElementDecl> subsGroup = decl.getSubstitutionGroupMembers();
                    foreach (Saxon.Eej.ee.schema.ElementDecl mem in subsGroup) {
                        if ((mem.getFingerprint() == elementFingerprint)) {
                            return mem.getType();
                        }
                    }
                }
            } catch (Saxon.Hej.type.MissingComponentException e) {
                throw new Saxon.Ejava.lang.IllegalStateException(e.Message);
            }
            return null;
        }
        public override int getElementParticleCardinality(int elementName) {
            if ((elementName == getTargetFingerprint())) {
                return /*134*/Saxon.Eej.ee.schema.Particle.computeParticleCardinality(getMinOccurs(), getMaxOccurs());
            }
            Saxon.Eej.ee.schema.ElementDecl decl = (Saxon.Eej.ee.schema.ElementDecl)(getTarget());
            System.Collections.Generic.ICollection<Saxon.Eej.ee.schema.ElementDecl> subsGroup = decl.getSubstitutionGroupMembers();
            foreach (Saxon.Eej.ee.schema.ElementDecl mem in subsGroup) {
                if ((mem.getFingerprint() == elementName)) {
                    return /*134*/Saxon.Eej.ee.schema.Particle.computeParticleCardinality(getMinOccurs(), getMaxOccurs());
                }
            }
            return Saxon.Hej.expr.StaticProperty.EMPTY;
        }
        public override bool containsElementWildcard() {
            return false;
        }
        public override void gatherAllPermittedElements(Saxon.Hej.z.IntHashSet result, bool ignoreWildcards) {
            Saxon.Eej.ee.schema.ElementDecl decl = (Saxon.Eej.ee.schema.ElementDecl)(getTarget());
            result.add(decl.getFingerprint());
            System.Collections.Generic.ICollection<Saxon.Eej.ee.schema.ElementDecl> subsGroup = decl.getSubstitutionGroupMembers();
            foreach (Saxon.Eej.ee.schema.ElementDecl mem in subsGroup) {
                result.add(mem.getFingerprint());
            }
        }
        public override string ToString() {
            return ("ElementParticle "+getName());
        }
        public override void serializeParticle(Saxon.Eej.ee.schema.SchemaModelSerializer serializer) {
            serializer.startElement("elementParticle");
            serializer.emitAttribute("minOccurs", (""+getMinOccurs()));
            serializer.emitAttribute("maxOccurs", ((getMaxOccurs()<0) ? "unbounded" : (""+getMaxOccurs())));
            serializer.emitAttribute("ref", serializer.getId(getTarget(), false));
            serializer.endElement();
        }
    
    }
}