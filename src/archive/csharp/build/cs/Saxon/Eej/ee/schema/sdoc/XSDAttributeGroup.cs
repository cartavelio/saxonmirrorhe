using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema.sdoc {
    public class XSDAttributeGroup : Saxon.Eej.ee.schema.sdoc.SchemaElement {
        private bool isRedefinition;
        private Saxon.Eej.ee.schema.AttributeGroupDecl attributeGroupDecl = null;
        private Saxon.Eej.ee.schema.AttributeGroupReference attributeGroupReference = null;
        private bool isRef = false;
        public XSDAttributeGroup()  {
        }
        protected internal override void prepareAttributes() {
            isRedefinition = ((((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()) is Saxon.Eej.ee.schema.sdoc.XSDRedefine));
            Saxon.Hej.om.AttributeMap atts = attributes();
            if (isTopLevel()) {
                string[] allowed = {"id", "name"};
                allowAttributes(atts, allowed);
                requireAttribute(atts, "name");
                string qname = atts.getValue("", "name");
                if ((qname == null)) {
                    return ;
                }
                Saxon.Hej.om.StructuredQName nc;
                try {
                    nc = getComponentName(qname, REQUIRE_TARGET_NS);
                } catch (Saxon.Hej.type.SchemaException err) {
                    error(err.Message);
                    return ;
                }
                attributeGroupDecl = new Saxon.Eej.ee.schema.AttributeGroupDecl((Saxon.Eej.config.EnterpriseConfiguration)(getConfiguration()));
                attributeGroupDecl.setLocator(this);
                attributeGroupDecl.setAttributeGroupName(nc);
                attributeGroupDecl.setSchemaDocumentURI(((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()).getSystemId());
            } else {
                string[] allowed = {"id", "ref"};
                allowAttributes(atts, allowed);
                requireAttribute(atts, "ref");
                string @ref = atts.getValue("", "ref");
                if ((@ref == null)) {
                    return ;
                }
                Saxon.Hej.om.StructuredQName fp;
                try {
                    fp = getComponentName(@ref, USE_DEFAULT_NS);
                } catch (Saxon.Hej.type.SchemaException err) {
                    error(err.Message);
                    return ;
                }
                isRef = true;
                attributeGroupReference = new Saxon.Eej.ee.schema.AttributeGroupReference((Saxon.Eej.config.EnterpriseConfiguration)(getConfiguration()), fp);
                attributeGroupReference.setLocator(this);
            }
            processId();
        }
        public bool isReference() {
            return isRef;
        }
        public Saxon.Eej.ee.schema.AttributeGroupDecl getAttributeGroupDecl() {
            return attributeGroupDecl;
        }
        public Saxon.Eej.ee.schema.AttributeGroupReference getAttributeGroupReference() {
            return attributeGroupReference;
        }
        public override void validate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            if (isRef) {
                checkOnlyChildIsAnnotation();
            } else {
                checkContentModel(stateTerms, stateTargets, false);
            }
        }
        private readonly static int[] state0terms = {Saxon.Hej.om.StandardNames.XS_ANY_ATTRIBUTE, Saxon.Hej.om.StandardNames.XS_ATTRIBUTE, Saxon.Hej.om.StandardNames.XS_ATTRIBUTE_GROUP, Saxon.Hej.om.StandardNames.XS_ANNOTATION};
        private readonly static int[] state0targets = {1, 2, 2, 3};
        private readonly static int[] state1terms = {};
        private readonly static int[] state1targets = {};
        private readonly static int[] state2terms = {Saxon.Hej.om.StandardNames.XS_ANY_ATTRIBUTE, Saxon.Hej.om.StandardNames.XS_ATTRIBUTE, Saxon.Hej.om.StandardNames.XS_ATTRIBUTE_GROUP};
        private readonly static int[] state2targets = {1, 2, 2};
        private readonly static int[] state3terms = {Saxon.Hej.om.StandardNames.XS_ANY_ATTRIBUTE, Saxon.Hej.om.StandardNames.XS_ATTRIBUTE, Saxon.Hej.om.StandardNames.XS_ATTRIBUTE_GROUP};
        private readonly static int[] state3targets = {1, 2, 2};
        private readonly static int[][] stateTerms = {state0terms, state1terms, state2terms, state3terms};
        private readonly static int[][] stateTargets = {state0targets, state1targets, state2targets, state3targets};
        protected internal override bool isFinalState(int state) {
            return true;
        }
        public override void postValidate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            foreach (Saxon.Hej.om.NodeInfo child in children()) {
                string name = child.getLocalPart();
                if ("anyAttribute".Equals(name)) {
                    Saxon.Eej.ee.schema.AttributeWildcard wildcard = ((Saxon.Eej.ee.schema.sdoc.XSDAnyAttribute)(child)).getWildcard();
                    if ((attributeGroupDecl != null)) {
                        attributeGroupDecl.setAnyAttribute(wildcard);
                    }
                } else {
                    if ("attribute".Equals(name)) {
                        Saxon.Eej.ee.schema.AttributeUse attributeUse = ((Saxon.Eej.ee.schema.sdoc.XSDAttribute)(child)).getAttributeUse();
                        if (((attributeGroupDecl != null) && (attributeUse != null))) {
                            attributeUse.setContainingAttributeGroupDecl(attributeGroupDecl);
                            if (attributeUse.isProhibited()) {
                                compiler.warning("Specifying use='prohibited' within an attribute group has no effect", this);
                            } else {
                                attributeGroupDecl.addAttributeUse(attributeUse);
                            }
                        }
                    } else {
                        if ("attributeGroup".Equals(name)) {
                            Saxon.Eej.ee.schema.AttributeGroupReference obj = ((Saxon.Eej.ee.schema.sdoc.XSDAttributeGroup)(child)).getAttributeGroupReference();
                            if ((attributeGroupDecl != null)) {
                                attributeGroupDecl.addAttributeGroupReference(obj);
                            }
                        } else {
                            if ("annotation".Equals(name)) {
                            }
                        }
                    }
                }
            }
            if (isRedefinition) {
                Saxon.Eej.ee.schema.SingleNamespaceSchema externalSchema = ((Saxon.Eej.ee.schema.sdoc.XSDRedefine)(((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()))).getExternalSchema();
                Saxon.Eej.ee.schema.SingleNamespaceSchema redefinedSchema = ((Saxon.Eej.ee.schema.sdoc.XSDRedefine)(((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()))).getRedefinedSchema();
                if (((externalSchema == null) || (redefinedSchema == null))) {
                    return ;
                }
                Saxon.Eej.config.EnterpriseConfiguration config = getSchema().getConfiguration();
                if (config.isSealedNamespace(redefinedSchema.getTargetNamespace())) {
                    error(((("It is not possible to redefine schema components in namespace "+redefinedSchema.getTargetNamespace())+" because components in this namespace have already been used ")+" for validating instance documents, or for compiling queries or stylesheets"));
                    return ;
                }
                if (isRef) {
                    error("An attribute group redefinition must not be a reference");
                }
                Saxon.Eej.ee.schema.AttributeGroupDecl found = patchUpSelfReferences(attributeGroupDecl.getLocalAttributeGroupReferences(), attributeGroupDecl.getAttributeGroupName(), null, compiler);
                if ((found != null)) {
                    attributeGroupDecl.setRedefinitionLevel((found.getRedefinitionLevel()+1));
                    checkImmediateRedefinition(found, compiler);
                } else {
                    Saxon.Eej.ee.schema.AttributeGroupDecl d = externalSchema.getAttributeGroup(attributeGroupDecl.getAttributeGroupName());
                    if ((d == null)) {
                        error((("Attribute group "+Saxon.Hej.trans.Err.wrap(attributeGroupDecl.getDisplayName()))+" is not defined in the schema being redefined"));
                        return ;
                    }
                    attributeGroupDecl.setRedefinedAttributeGroupDecl(d);
                    attributeGroupDecl.setRedefinitionLevel((d.getRedefinitionLevel()+1));
                }
                redefinedSchema.addAttributeGroup(attributeGroupDecl);
            }
        }
        private Saxon.Eej.ee.schema.AttributeGroupDecl patchUpSelfReferences(System.Collections.Generic.IList<Saxon.Eej.ee.schema.AttributeGroupReference> references, Saxon.Hej.om.StructuredQName name, Saxon.Eej.ee.schema.AttributeGroupDecl found, Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            Saxon.Eej.ee.schema.PreparedSchema containingSchema = getSchema();
            Saxon.Eej.ee.schema.PreparedSchema redefinedSchema = ((Saxon.Eej.ee.schema.sdoc.XSDRedefine)(((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()))).getRedefinedSchema();
            Saxon.Eej.ee.schema.PreparedSchema externalSchema = ((Saxon.Eej.ee.schema.sdoc.XSDRedefine)(((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()))).getExternalSchema();
            Saxon.Eej.ee.schema.AttributeGroupDecl f = found;
            foreach (Saxon.Eej.ee.schema.AttributeGroupReference @ref in references) {
                if (@ref.getTargetComponentName().Equals(name)) {
                    if ((f != null)) {
                        error((("Redefined attribute group "+Saxon.Hej.trans.Err.wrap(name.getDisplayName()))+" contains more than one reference to itself"));
                    }
                    string uri = getXSDSchema().getTargetNamespace();
                    string localName = @ref.getTargetComponentName().getLocalPart();
                    string newLocalName = ((localName+'_')+@ref.GetHashCode());
                    Saxon.Hej.om.StructuredQName newNameCode = new Saxon.Hej.om.StructuredQName("", uri, newLocalName);
                    f = externalSchema.getAttributeGroup(name);
                    if ((f == null)) {
                        error((("Attribute group "+Saxon.Hej.trans.Err.wrap(name.getDisplayName()))+" is not defined in the schema being redefined"));
                        return null;
                    }
                    checkImmediateRedefinition(f, compiler);
                    @ref.setTargetComponentName(newNameCode);
                    f.setAttributeGroupName(newNameCode);
                    redefinedSchema.addAttributeGroup(f);
                    f = patchUpSelfReferences(f.getLocalAttributeGroupReferences(), name, f, compiler);
                }
            }
            return f;
        }
        private void checkImmediateRedefinition(Saxon.Eej.ee.schema.AttributeGroupDecl f, Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            if (!(((Saxon.Eej.ee.schema.sdoc.XSDRedefine)(((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()))).getExternalSchemaDocumentURI().Equals(f.getSchemaDocumentURI()))) {
                compiler.warning((("The redefined attribute group was found, but not in the schema document referenced by "+"the schemaLocation attribute of the containing <xs:redefine> element. This is not allowed by the XSD ")+"specification. However, Saxon does not currently enforce this rule."), this);
            }
        }
    
    }
}