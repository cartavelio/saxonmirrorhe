using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema.sdoc {
    public class XSDGroup : Saxon.Eej.ee.schema.sdoc.SchemaElement {
        private Saxon.Eej.ee.schema.ModelGroupDefinition groupDeclaration = null;
        private Saxon.Eej.ee.schema.ModelGroupParticle modelGroupParticle = null;
        private bool foundMinOccurs = false;
        private bool foundMaxOccurs = false;
        private bool isRedefinition;
        public XSDGroup()  {
        }
        protected internal override void prepareAttributes() {
            Saxon.Eej.ee.schema.PreparedSchema schema = getSchema();
            isRedefinition = ((((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()) is Saxon.Eej.ee.schema.sdoc.XSDRedefine));
            Saxon.Hej.om.AttributeMap atts = attributes();
            if (isTopLevel()) {
                groupDeclaration = new Saxon.Eej.ee.schema.ModelGroupDefinition((Saxon.Eej.config.EnterpriseConfiguration)(getConfiguration()), this);
                groupDeclaration.setSchemaDocumentURI(((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()).getSystemId());
                string[] allowed = {"id", "name"};
                allowAttributes(atts, allowed);
                requireAttribute(atts, "name");
            } else {
                modelGroupParticle = new Saxon.Eej.ee.schema.ModelGroupParticle((Saxon.Eej.config.EnterpriseConfiguration)(getConfiguration()), this);
                schema.addComponent(modelGroupParticle);
                string[] allowed = {"id", "maxOccurs", "minOccurs", "ref"};
                allowAttributes(atts, allowed);
                requireAttribute(atts, "ref");
            }
            string attValue;
            string name = atts.getValue("", "name");
            if ((groupDeclaration != null)) {
                if ((name != null)) {
                    Saxon.Hej.om.StructuredQName qName;
                    try {
                        qName = getComponentName(name, REQUIRE_TARGET_NS);
                    } catch (Saxon.Hej.type.SchemaException err) {
                        error(err.Message);
                        qName = Saxon.Hej.om.StandardNames.SQ_XS_INVALID_NAME;
                    }
                    groupDeclaration.setModelGroupName(qName);
                } else {
                    if ((name == null)) {
                        groupDeclaration.setModelGroupName(Saxon.Hej.om.StandardNames.SQ_XS_INVALID_NAME);
                    }
                }
            }
            attValue = atts.getValue("", "maxOccurs");
            if (((attValue != null) && (modelGroupParticle != null))) {
                foundMaxOccurs = true;
                if ((groupDeclaration != null)) {
                    mutuallyExclusiveAttributes("name", "maxOccurs");
                } else {
                    int max = processMaxOccurs(attValue);
                    modelGroupParticle.setMaxOccurs(max);
                }
            }
            attValue = atts.getValue("", "minOccurs");
            if (((attValue != null) && (modelGroupParticle != null))) {
                foundMinOccurs = true;
                if ((groupDeclaration != null)) {
                    mutuallyExclusiveAttributes("name", "minOccurs");
                } else {
                    int minOccurs = processMinOccurs(attValue);
                    modelGroupParticle.setMinOccurs(minOccurs);
                }
            }
            if ((modelGroupParticle != null)) {
                int maxOccurs = modelGroupParticle.getMaxOccurs();
                if (((maxOccurs != -1) && (modelGroupParticle.getMinOccurs()>maxOccurs))) {
                    error("maxOccurs must not be less than minOccurs");
                }
            }
            attValue = atts.getValue("", "ref");
            if (((attValue != null) && (modelGroupParticle != null))) {
                if ((groupDeclaration != null)) {
                    mutuallyExclusiveAttributes("name", "ref");
                } else {
                    Saxon.Hej.om.StructuredQName fp;
                    try {
                        fp = getComponentName(attValue, USE_DEFAULT_NS);
                    } catch (Saxon.Hej.type.SchemaException err) {
                        error(err.Message);
                        return ;
                    }
                    modelGroupParticle.setTargetComponentName(fp);
                }
            }
            processId();
        }
        public Saxon.Eej.ee.schema.ModelGroupDefinition getGroupDeclaration() {
            return groupDeclaration;
        }
        public Saxon.Eej.ee.schema.ModelGroupParticle getGroupReference() {
            return modelGroupParticle;
        }
        public override void validate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            bool foundAnnotation = false;
            Saxon.Hej.om.NodeInfo foundCompositor = null;
            foreach (Saxon.Hej.om.NodeInfo child in children()) {
                int fp = child.getFingerprint();
                switch (fp) {
                  case Saxon.Hej.om.StandardNames.XS_ANNOTATION:
                    if (foundAnnotation) {
                        duplicateElement("annotation");
                    }
                    if ((foundCompositor != null)) {
                        mustBeFirstElement("annotation");
                    }
                    foundAnnotation = true;
                    break ;
                  case Saxon.Hej.om.StandardNames.XS_ALL:
                    if (foundMinOccurs) {
                        mutuallyExclusiveElementAndAttribute("all", "minOccurs");
                    }
                    if (foundMaxOccurs) {
                        mutuallyExclusiveElementAndAttribute("all", "maxOccurs");
                    }
                    goto case Saxon.Hej.om.StandardNames.XS_SEQUENCE;
                  case Saxon.Hej.om.StandardNames.XS_SEQUENCE:
                  case Saxon.Hej.om.StandardNames.XS_CHOICE:
                    if ((foundCompositor != null)) {
                        mutuallyExclusive(child.getDisplayName(), foundCompositor.getDisplayName());
                    }
                    if ((modelGroupParticle != null)) {
                        mutuallyExclusiveElementAndAttribute(child.getDisplayName(), "ref");
                    }
                    foundCompositor = child;
                    break ;
                  default:
                    illegalElement(child);
                    break ;
                }
            }
        }
        public override void postValidate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            if ((groupDeclaration == null)) {
                return ;
            }
            foreach (Saxon.Hej.om.NodeInfo child in children()) {
                int fp = child.getFingerprint();
                switch (fp) {
                  case Saxon.Hej.om.StandardNames.XS_ALL:
                  case Saxon.Hej.om.StandardNames.XS_SEQUENCE:
                  case Saxon.Hej.om.StandardNames.XS_CHOICE:
                    Saxon.Eej.ee.schema.ModelGroupParticle compositor = ((Saxon.Eej.ee.schema.sdoc.XSDCompositor)(child)).getModelGroupParticle();
                    groupDeclaration.addParticle(compositor);
                    break ;
                }
            }
            if (isRedefinition) {
                Saxon.Eej.ee.schema.PreparedSchema externalSchema = ((Saxon.Eej.ee.schema.sdoc.XSDRedefine)(((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()))).getExternalSchema();
                Saxon.Eej.ee.schema.PreparedSchema redefinedSchema = ((Saxon.Eej.ee.schema.sdoc.XSDRedefine)(((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()))).getRedefinedSchema();
                if (((externalSchema == null) || (redefinedSchema == null))) {
                    return ;
                }
                Saxon.Eej.ee.schema.ModelGroupDefinition originalGroup = externalSchema.getGroup(groupDeclaration.getModelGroupName());
                if ((originalGroup == null)) {
                    error((("Group "+Saxon.Hej.trans.Err.wrap(groupDeclaration.getName()))+" is not defined in the schema being redefined"));
                    return ;
                }
                if (!(((Saxon.Eej.ee.schema.sdoc.XSDRedefine)(((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()))).getExternalSchemaDocumentURI().Equals(originalGroup.getSchemaDocumentURI()))) {
                    compiler.warning((("The redefined model group was found, but not in the schema document referenced by "+"the schemaLocation attribute of the containing <xs:redefine> element. This is not allowed by the XSD ")+"specification. However, Saxon does not currently enforce this rule."), this);
                }
                groupDeclaration.setRedefinitionLevel((originalGroup.getRedefinitionLevel()+1));
                bool found = patchUpSelfReferences(groupDeclaration.getParticles(), groupDeclaration.getModelGroupName(), originalGroup, false);
                if (!(found)) {
                    groupDeclaration.setRedefinedModelGroupDefinition(originalGroup);
                }
                redefinedSchema.addGroup(groupDeclaration);
            }
        }
        private bool patchUpSelfReferences(System.Collections.Generic.IList<Saxon.Eej.ee.schema.Particle> particles, Saxon.Hej.om.StructuredQName qName, Saxon.Eej.ee.schema.ModelGroupDefinition originalGroup, bool found) {
            Saxon.Eej.ee.schema.PreparedSchema redefinedSchema = ((Saxon.Eej.ee.schema.sdoc.XSDRedefine)(((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()))).getRedefinedSchema();
            bool f = found;
            foreach (Saxon.Eej.ee.schema.Particle p in particles) {
                if (((p is Saxon.Eej.ee.schema.ModelGroupParticle) && qName.Equals(p.getTargetComponentName()))) {
                    if (f) {
                        error((("Redefined group "+Saxon.Hej.trans.Err.wrap(groupDeclaration.getName()))+" contains more than one reference to itself"));
                    }
                    f = true;
                    Saxon.Eej.ee.schema.ModelGroupParticle gref = (Saxon.Eej.ee.schema.ModelGroupParticle)(p);
                    if ((gref.getMinOccurs() != 1)) {
                        error((("Redefined group "+Saxon.Hej.trans.Err.wrap(groupDeclaration.getName()))+" must have minOccurs=1"));
                    }
                    if ((gref.getMaxOccurs() != 1)) {
                        error((("Redefined group "+Saxon.Hej.trans.Err.wrap(groupDeclaration.getName()))+" must have maxOccurs=1"));
                    }
                    string uri = getXSDSchema().getTargetNamespace();
                    string localName = gref.getTargetComponentName().getLocalPart();
                    string newLocalName = ((localName+'_')+gref.GetHashCode());
                    Saxon.Hej.om.StructuredQName newNameCode = new Saxon.Hej.om.StructuredQName("", uri, newLocalName);
                    gref.setTargetComponentName(newNameCode);
                    originalGroup.setModelGroupName(newNameCode);
                    redefinedSchema.addGroup(originalGroup);
                } else {
                    if ((p is Saxon.Eej.ee.schema.ModelGroupParticle)) {
                        Saxon.Eej.ee.schema.ModelGroup group;
                        try {
                            group = ((Saxon.Eej.ee.schema.ModelGroupParticle)(p)).getGroup();
                        } catch (Saxon.Hej.type.MissingComponentException e) {
                            continue ;
                        }
                        f = patchUpSelfReferences(group.getParticles(), qName, originalGroup, f);
                    }
                }
            }
            return f;
        }
    
    }
}