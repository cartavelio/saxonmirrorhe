using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema {
    public class ElementWildcard : Saxon.Eej.ee.schema.Particle {
        private Saxon.Eej.ee.schema.Wildcard wildcard;
        public ElementWildcard(Saxon.Eej.config.EnterpriseConfiguration config)  {
            wildcard = new Saxon.Eej.ee.schema.Wildcard();
            wildcard.setConfiguration(config);
            setConfiguration(config);
        }
        public Saxon.Eej.ee.schema.ElementWildcard copy() {
            Saxon.Eej.ee.schema.ElementWildcard e = new Saxon.Eej.ee.schema.ElementWildcard(getConfiguration());
            e.wildcard = wildcard;
            e.setMinOccurs(getMinOccurs());
            e.setMaxOccurs(getMaxOccurs());
            return e;
        }
        public ElementWildcard(Saxon.Eej.ee.schema.Wildcard wc)  {
            wildcard = wc;
        }
        public void setWildcard(Saxon.Eej.ee.schema.Wildcard wildcard) {
            this.wildcard = wildcard;
        }
        public Saxon.Eej.ee.schema.Wildcard getWildcard() {
            return wildcard;
        }
        public override bool isEmptiable() {
            return ((getMinOccurs() == 0));
        }
        public override bool isPointless(Saxon.Eej.ee.schema.ModelGroup container) {
            return false;
        }
        public override Saxon.Eej.ee.schema.Term getTerm() {
            return wildcard;
        }
        public override bool fixup(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            setFixupStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.VALIDATED);
            return true;
        }
        public override void compile(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
        }
        public override Saxon.Hej.type.SchemaType getElementParticleType(int elementName) {
            Saxon.Eej.config.EnterpriseConfiguration config = getConfiguration();
            if (wildcard.matches(elementName, true, config, null)) {
                if (wildcard.getProcessContents().Equals("skip")) {
                    return Saxon.Hej.type.AnyType.getInstance();
                } else {
                    if (wildcard.getProcessContents().Equals("strict")) {
                        Saxon.Eej.ee.schema.ElementDecl decl = getConfiguration().getSuperSchema().getElementDecl(elementName);
                        if ((decl == null)) {
                            return null;
                        } else {
                            try {
                                return decl.getType();
                            } catch (Saxon.Hej.type.MissingComponentException e) {
                                throw new Saxon.Ejava.lang.IllegalStateException(e.Message);
                            }
                        }
                    } else {
                        Saxon.Eej.ee.schema.ElementDecl decl = getConfiguration().getSuperSchema().getElementDecl(elementName);
                        if ((decl == null)) {
                            return Saxon.Hej.type.AnyType.getInstance();
                        } else {
                            try {
                                return decl.getType();
                            } catch (Saxon.Hej.type.MissingComponentException e) {
                                throw new Saxon.Ejava.lang.IllegalStateException(e.Message);
                            }
                        }
                    }
                }
            }
            return null;
        }
        public override int getElementParticleCardinality(int elementName) {
            Saxon.Eej.config.EnterpriseConfiguration config = getConfiguration();
            if (wildcard.matches(elementName, true, config, null)) {
                return /*134*/Saxon.Eej.ee.schema.Particle.computeParticleCardinality(getMinOccurs(), getMaxOccurs());
            }
            return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO;
        }
        public override bool containsElementWildcard() {
            return true;
        }
        public override void gatherAllPermittedElements(Saxon.Hej.z.IntHashSet result, bool ignoreWildcards) {
            if (!(ignoreWildcards)) {
                result.add(-1);
            }
        }
        public override bool validate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            return wildcard.validate(compiler);
        }
        public override void serializeParticle(Saxon.Eej.ee.schema.SchemaModelSerializer serializer) {
            string id = serializer.getId(wildcard, false);
            serializer.startElement("elementWildcard");
            serializer.emitAttribute("minOccurs", (""+getMinOccurs()));
            serializer.emitAttribute("maxOccurs", ((getMaxOccurs()<0) ? "unbounded" : (""+getMaxOccurs())));
            serializer.emitAttribute("ref", id);
            serializer.endElement();
        }
        public Saxon.Hej.om.FunctionItem getTermComponentAsFunction() {
            return wildcard.getComponentAsFunction();
        }
    
    }
}