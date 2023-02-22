using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema.sdoc {
    public class XSDCompositor : Saxon.Eej.ee.schema.sdoc.SchemaElement {
        private Saxon.Eej.ee.schema.ModelGroup modelGroup = null;
        private Saxon.Eej.ee.schema.ModelGroupParticle particle = null;
        public XSDCompositor()  {
        }
        protected internal override void prepareAttributes() {
            Saxon.Hej.om.AttributeMap atts = attributes();
            string[] allowed = {"id", "maxOccurs", "minOccurs"};
            allowAttributes(atts, allowed);
            string attValue;
            int fp = getFingerprint();
            switch (fp) {
              case Saxon.Hej.om.StandardNames.XS_SEQUENCE:
                modelGroup = new Saxon.Eej.ee.schema.SequenceModelGroup();
                break ;
              case Saxon.Hej.om.StandardNames.XS_CHOICE:
                modelGroup = new Saxon.Eej.ee.schema.ChoiceModelGroup();
                break ;
              case Saxon.Hej.om.StandardNames.XS_ALL:
                modelGroup = new Saxon.Eej.ee.schema.AllModelGroup();
                break ;
              default:
                error((((("Invalid compositor element name: "+Saxon.Hej.trans.Err.wrap(getLocalPart(), Saxon.Hej.trans.Err.ELEMENT))+"(fp = ")+fp)+")"));
                return ;
            }
            modelGroup.setLocator(this);
            modelGroup.setConfiguration(getSchema().getConfiguration());
            particle = new Saxon.Eej.ee.schema.ModelGroupParticle(getSchema().getConfiguration(), this);
            particle.setTarget(modelGroup);
            Saxon.Hej.tree.linked.NodeImpl parent = ((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent());
            attValue = atts.getValue(Saxon.Hej.om.NamespaceUri.NULL, "maxOccurs");
            if ((attValue != null)) {
                if ((parent.getFingerprint() == Saxon.Hej.om.StandardNames.XS_GROUP)) {
                    error((("Cannot specify 'maxOccurs' on a top-level '"+getLocalPart())+"' compositor within a named group"));
                }
                int max = processMaxOccurs(attValue);
                particle.setMaxOccurs(max);
            }
            attValue = atts.getValue(Saxon.Hej.om.NamespaceUri.NULL, "minOccurs");
            if ((attValue != null)) {
                if ((parent.getFingerprint() == Saxon.Hej.om.StandardNames.XS_GROUP)) {
                    error((("Cannot specify 'minOccurs' on a top-level '"+getLocalPart())+"' compositor within a named group"));
                }
                int minOccurs = processMinOccurs(attValue);
                particle.setMinOccurs(minOccurs);
            }
            int maxOccurs = particle.getMaxOccurs();
            if (((maxOccurs != -1) && (particle.getMinOccurs()>maxOccurs))) {
                error("maxOccurs must not be less than minOccurs");
            }
            if ((modelGroup is Saxon.Eej.ee.schema.AllModelGroup)) {
                int max = particle.getMaxOccurs();
                if ((getXSDSchema().getSchemaCompiler().getLanguageVersion() == Saxon.Hej.Configuration.XSD10)) {
                    if ((max != 1)) {
                        invalidAttributeValue("maxOccurs", (((max == -1) ? "unbounded" : (max+""))), "Must be \"1\"");
                        return ;
                    }
                } else {
                    if (((max<0) || (max>1))) {
                        invalidAttributeValue("maxOccurs", (((max == -1) ? "unbounded" : (max+""))), "Must be \"0\" or \"1\"");
                        return ;
                    }
                }
                if ((particle.getMinOccurs()>1)) {
                    invalidAttributeValue("minOccurs", (particle.getMinOccurs()+""), "Must be \"0\" or \"1\"");
                    return ;
                }
            }
            processId();
        }
        public Saxon.Eej.ee.schema.ModelGroup getModelGroup() {
            return modelGroup;
        }
        public Saxon.Eej.ee.schema.ModelGroupParticle getModelGroupParticle() {
            return particle;
        }
        public override void validate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            bool foundElement = false;
            bool foundCompositor = false;
            bool foundGroup = false;
            bool foundAnnotation = false;
            foreach (Saxon.Hej.om.NodeInfo child in children()) {
                switch (child.getFingerprint()) {
                  case Saxon.Hej.om.StandardNames.XS_ANNOTATION:
                    if (((foundElement || foundCompositor) || foundGroup)) {
                        mustBeFirstElement("annotation");
                    }
                    if (foundAnnotation) {
                        duplicateElement("annotation");
                    }
                    foundAnnotation = true;
                    break ;
                  case Saxon.Hej.om.StandardNames.XS_ELEMENT:
                    foundElement = true;
                    break ;
                  case Saxon.Hej.om.StandardNames.XS_GROUP:
                    if (((modelGroup is Saxon.Eej.ee.schema.AllModelGroup) && (compiler.getLanguageVersion() == Saxon.Hej.Configuration.XSD10))) {
                        illegalElement(child);
                    }
                    foundGroup = true;
                    break ;
                  case Saxon.Hej.om.StandardNames.XS_SEQUENCE:
                  case Saxon.Hej.om.StandardNames.XS_CHOICE:
                    if ((modelGroup is Saxon.Eej.ee.schema.AllModelGroup)) {
                        illegalElement(child);
                    }
                    foundCompositor = true;
                    break ;
                  case Saxon.Hej.om.StandardNames.XS_ANY:
                    if (((modelGroup is Saxon.Eej.ee.schema.AllModelGroup) && (compiler.getLanguageVersion() == Saxon.Hej.Configuration.XSD10))) {
                        illegalElement(child);
                    }
                    break ;
                  default:
                    illegalElement(child);
                    break ;
                }
            }
        }
        public override void postValidate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            foreach (Saxon.Hej.om.NodeInfo child in children()) {
                switch (child.getFingerprint()) {
                  case Saxon.Hej.om.StandardNames.XS_ANY:
                    Saxon.Eej.ee.schema.ElementWildcard wildcard = ((Saxon.Eej.ee.schema.sdoc.XSDAny)(child)).getElementWildcard();
                    modelGroup.addParticle(wildcard);
                    break ;
                  case Saxon.Hej.om.StandardNames.XS_ANNOTATION:
                    break ;
                  case Saxon.Hej.om.StandardNames.XS_ELEMENT:
                    Saxon.Eej.ee.schema.ElementParticle element = ((Saxon.Eej.ee.schema.sdoc.XSDElement)(child)).getElementParticle();
                    if (((modelGroup is Saxon.Eej.ee.schema.AllModelGroup) && (compiler.getLanguageVersion() == Saxon.Hej.Configuration.XSD10))) {
                        if ((element.getMinOccurs()>1)) {
                            error("Within <xs:all>, an <xs:element> must have @minOccurs equal to 0 or 1");
                        }
                        if (((element.getMaxOccurs()>1) || (element.getMaxOccurs() == -1))) {
                            error("Within <xs:all>, an <xs:element> must have @maxOccurs equal to 0 or 1");
                        }
                    }
                    modelGroup.addParticle(element);
                    break ;
                  case Saxon.Hej.om.StandardNames.XS_GROUP:
                    Saxon.Eej.ee.schema.ModelGroupParticle modelGroupParticle = ((Saxon.Eej.ee.schema.sdoc.XSDGroup)(child)).getGroupReference();
                    this.modelGroup.addParticle(modelGroupParticle);
                    break ;
                  case Saxon.Hej.om.StandardNames.XS_SEQUENCE:
                  case Saxon.Hej.om.StandardNames.XS_CHOICE:
                  case Saxon.Hej.om.StandardNames.XS_ALL:
                    Saxon.Eej.ee.schema.ModelGroupParticle p = ((Saxon.Eej.ee.schema.sdoc.XSDCompositor)(child)).getModelGroupParticle();
                    this.modelGroup.addParticle(p);
                    break ;
                }
            }
        }
    
    }
}