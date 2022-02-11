using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema.sdoc {
    public class XSDIdentityConstraint : Saxon.Eej.ee.schema.sdoc.SchemaElement {
        private bool isRef = false;
        private Saxon.Hej.om.StructuredQName constraintName;
        private Saxon.Eej.ee.schema.IdentityConstraint identityConstraint = null;
        public XSDIdentityConstraint()  {
        }
        protected internal override void prepareAttributes() {
            Saxon.Hej.om.AttributeMap atts = attributes();
            if ((getFingerprint() == Saxon.Hej.om.StandardNames.XS_KEYREF)) {
                string[] allowed = {"id", "name", "ref", "refer"};
                allowAttributes(atts, allowed);
            } else {
                string[] allowed = {"id", "name", "ref"};
                allowAttributes(atts, allowed);
            }
            processId();
            checkMutuallyExclusiveAttributes("ref", "name");
            checkMutuallyExclusiveAttributes("ref", "refer");
            string refAtt = atts.getValue("", "ref");
            Saxon.Hej.om.StructuredQName nameCode;
            if ((refAtt != null)) {
                if ((getXSDSchema().getSchemaCompiler().getLanguageVersion() == Saxon.Hej.Configuration.XSD10)) {
                    error("The @ref attribute cannot be used on an integrity constraint unless XSD 1.1 is enabled");
                }
                nameCode = getComponentName(refAtt, USE_DEFAULT_NS);
                isRef = true;
            } else {
                string name = atts.getValue("", "name");
                if ((name == null)) {
                    missingAttribute("name");
                    name = (("saxon-error-"+getLocalPart())+"-name");
                }
                nameCode = getComponentName(name, REQUIRE_TARGET_NS);
                if ((getFingerprint() == Saxon.Hej.om.StandardNames.XS_KEYREF)) {
                    string referAtt = atts.getValue("", "refer");
                    if ((referAtt == null)) {
                        missingAttribute("refer");
                        return ;
                    }
                    Saxon.Hej.om.StructuredQName referFingerprint;
                    try {
                        referFingerprint = getComponentName(referAtt, USE_DEFAULT_NS);
                    } catch (Saxon.Hej.type.SchemaException err) {
                        referFingerprint = Saxon.Hej.om.StandardNames.SQ_XS_INVALID_NAME;
                        error(err.Message);
                    }
                    identityConstraint = new Saxon.Eej.ee.schema.KeyRef((Saxon.Eej.config.EnterpriseConfiguration)(getConfiguration()), referFingerprint);
                } else {
                    if ((getFingerprint() == Saxon.Hej.om.StandardNames.XS_UNIQUE)) {
                        identityConstraint = new Saxon.Eej.ee.schema.Unique();
                    } else {
                        identityConstraint = new Saxon.Eej.ee.schema.Key();
                    }
                }
                Saxon.Eej.ee.schema.PreparedSchema schema = getSchema();
                identityConstraint.setConfiguration((Saxon.Eej.config.EnterpriseConfiguration)(getConfiguration()));
                identityConstraint.setLocator(this);
                identityConstraint.setConstraintName(nameCode);
                if ((schema.getIdentityConstraint(nameCode) != null)) {
                    duplicate("identity constraint", name);
                }
                schema.addIdentityConstraint(identityConstraint);
            }
            constraintName = nameCode;
        }
        public bool isReference() {
            return isRef;
        }
        public Saxon.Eej.ee.schema.IdentityConstraintReference getIdentityConstraint() {
            if ((constraintName == null)) {
                return null;
            }
            Saxon.Eej.ee.schema.IdentityConstraintReference @ref = new Saxon.Eej.ee.schema.IdentityConstraintReference(constraintName, getFingerprint(), (Saxon.Eej.config.EnterpriseConfiguration)(getConfiguration()), this);
            if (isRef) {
                return @ref;
            } else {
                if ((identityConstraint == null)) {
                    return null;
                }
                @ref.setTarget(identityConstraint);
            }
            return @ref;
        }
        public override void validate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            bool foundAnnotation = false;
            bool foundSelector = false;
            bool foundField = false;
            foreach (Saxon.Hej.om.NodeInfo child in children()) {
                switch (child.getFingerprint()) {
                  case Saxon.Hej.om.StandardNames.XS_ANNOTATION:
                    {
                        if (foundAnnotation) {
                            duplicateElement("annotation");
                        }
                        if ((foundSelector || foundField)) {
                            mustBeFirstElement("annotation");
                        }
                        foundAnnotation = true;
                        break ;
                    }
                  case Saxon.Hej.om.StandardNames.XS_SELECTOR:
                    {
                        if (foundField) {
                            mustPrecede("selector", "field");
                        }
                        if (foundSelector) {
                            duplicateElement("selector");
                        }
                        foundSelector = true;
                        break ;
                    }
                  case Saxon.Hej.om.StandardNames.XS_FIELD:
                    {
                        foundField = true;
                        break ;
                    }
                  default:
                    {
                        illegalElement(child);
                        break ;
                    }
                }
            }
            if (isRef) {
                if (foundSelector) {
                    mutuallyExclusiveElementAndAttribute("selector", "ref");
                }
                if (foundField) {
                    mutuallyExclusiveElementAndAttribute("field", "ref");
                }
            } else {
                if (!(foundSelector)) {
                    missingChildElement("selector");
                }
                if (!(foundField)) {
                    missingChildElement("field");
                }
            }
        }
        public override void postValidate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            if ((identityConstraint == null)) {
                return ;
            }
            foreach (Saxon.Hej.om.NodeInfo child in children()) {
                switch (child.getFingerprint()) {
                  case Saxon.Hej.om.StandardNames.XS_ANNOTATION:
                    {
                        break ;
                    }
                  case Saxon.Hej.om.StandardNames.XS_SELECTOR:
                    {
                        Saxon.Eej.ee.schema.IdentitySelector selector = ((Saxon.Eej.ee.schema.sdoc.XSDFieldOrSelector)(child)).getIdentitySelector();
                        identityConstraint.setSelector(selector);
                        break ;
                    }
                  case Saxon.Hej.om.StandardNames.XS_FIELD:
                    {
                        Saxon.Eej.ee.schema.IdentityField field = ((Saxon.Eej.ee.schema.sdoc.XSDFieldOrSelector)(child)).getIdentityField();
                        identityConstraint.addField(field);
                        if ((child.getAttributeValue(Saxon.Hej.lib.NamespaceConstant.SAXON, "order") != null)) {
                            if ((getFingerprint() != Saxon.Hej.om.StandardNames.XS_UNIQUE)) {
                                error("saxon:order is allowed only on a field of xs:unique");
                            }
                            identityConstraint.setOrdered(true);
                        }
                        break ;
                    }
                }
            }
        }
    
    }
}