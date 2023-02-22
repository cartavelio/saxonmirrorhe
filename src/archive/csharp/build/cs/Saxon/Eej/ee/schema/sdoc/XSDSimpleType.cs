using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema.sdoc {
    public class XSDSimpleType : Saxon.Eej.ee.schema.sdoc.SchemaElement {
        private bool isRedefinition;
        private Saxon.Eej.ee.schema.sdoc.SimpleTypeDefinition simpleTypeDef = null;
        internal bool foundRestriction = false;
        public XSDSimpleType()  {
        }
        protected internal override void prepareAttributes() {
            Saxon.Eej.ee.schema.PreparedSchema schema = getSchema();
            isRedefinition = (((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()) is Saxon.Eej.ee.schema.sdoc.XSDRedefine);
            Saxon.Hej.om.AttributeMap atts = attributes();
            if (isTopLevel()) {
                string[] allowed = {"final", "id", "name"};
                allowAttributes(atts, allowed);
                requireAttribute(atts, "name");
            } else {
                string[] allowed = {"id"};
                allowAttributes(atts, allowed);
            }
            string name = atts.getValue(Saxon.Hej.om.NamespaceUri.NULL, "name");
            simpleTypeDef = new Saxon.Eej.ee.schema.sdoc.SimpleTypeDefinition((Saxon.Eej.config.EnterpriseConfiguration)(getConfiguration()));
            simpleTypeDef.setLocator(this);
            simpleTypeDef.setSchemaDocumentURI(((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()).getSystemId());
            processId();
            if ((name != null)) {
                Saxon.Hej.om.StructuredQName typeName = null;
                try {
                    typeName = getComponentName(name, REQUIRE_TARGET_NS);
                } catch (Saxon.Hej.type.SchemaException err) {
                    error(err.Message);
                    typeName = Saxon.Hej.om.StandardNames.SQ_XS_INVALID_NAME;
                }
                int fp = getNamePool().allocateFingerprint(typeName.getNamespaceUri(), typeName.getLocalPart());
                simpleTypeDef.setTypeName(typeName, fp);
                if (!(isRedefinition)) {
                    if (((schema.getSchemaType(simpleTypeDef.getTypeName()) != null) && (fp>1023))) {
                        duplicate("type", name);
                    }
                }
            } else {
                Saxon.Hej.om.NodeName nn = allocateAnonymousTypeCode();
                simpleTypeDef.setTypeName(nn.getStructuredQName(), nn.getFingerprint());
                schema.addType(simpleTypeDef);
            }
            string finalVal = atts.getValue(Saxon.Hej.om.NamespaceUri.NULL, "final");
            if ((finalVal == null)) {
                simpleTypeDef.setFinalProhibitions(getXSDSchema().getFinalDefault());
            } else {
                int f = parseFinalOrBlock("final", finalVal, (((Saxon.Hej.type.Derivation.DERIVATION_LIST|Saxon.Hej.type.Derivation.DERIVATION_UNION)|Saxon.Hej.type.Derivation.DERIVATION_RESTRICTION)|Saxon.Hej.type.Derivation.DERIVATION_EXTENSION));
                simpleTypeDef.setFinalProhibitions(f);
            }
        }
        public Saxon.Eej.ee.schema.sdoc.SimpleTypeDefinition getSimpleTypeDefinition() {
            return simpleTypeDef;
        }
        public override void validate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            checkContentModel(stateTerms, stateTargets, false);
            foreach (Saxon.Hej.om.NodeInfo child in children()) {
                switch (child.getFingerprint()) {
                  case Saxon.Hej.om.StandardNames.XS_RESTRICTION:
                    foundRestriction = true;
                    simpleTypeDef.setDerivationMethod(Saxon.Hej.type.Derivation.DERIVATION_RESTRICTION);
                    break ;
                  case Saxon.Hej.om.StandardNames.XS_LIST:
                    simpleTypeDef.setDerivationMethod(Saxon.Hej.type.Derivation.DERIVATION_LIST);
                    break ;
                  case Saxon.Hej.om.StandardNames.XS_UNION:
                    simpleTypeDef.setDerivationMethod(Saxon.Hej.type.Derivation.DERIVATION_UNION);
                    break ;
                }
            }
        }
        private readonly static int[] state0terms = {Saxon.Hej.om.StandardNames.XS_LIST, Saxon.Hej.om.StandardNames.XS_UNION, Saxon.Hej.om.StandardNames.XS_RESTRICTION, Saxon.Hej.om.StandardNames.XS_ANNOTATION};
        private readonly static int[] state0targets = {1, 1, 1, 2};
        private readonly static int[] state1terms = {};
        private readonly static int[] state1targets = {};
        private readonly static int[] state2terms = {Saxon.Hej.om.StandardNames.XS_LIST, Saxon.Hej.om.StandardNames.XS_UNION, Saxon.Hej.om.StandardNames.XS_RESTRICTION};
        private readonly static int[] state2targets = {1, 1, 1};
        private readonly static int[][] stateTerms = {state0terms, state1terms, state2terms};
        private readonly static int[][] stateTargets = {state0targets, state1targets, state2targets};
        protected internal override bool isFinalState(int state) {
            return (state == 1);
        }
        public override void postValidate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            if (isRedefinition) {
                Saxon.Eej.ee.schema.PreparedSchema externalSchema = ((Saxon.Eej.ee.schema.sdoc.XSDRedefine)(((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()))).getExternalSchema();
                Saxon.Eej.ee.schema.PreparedSchema redefinedSchema = ((Saxon.Eej.ee.schema.sdoc.XSDRedefine)(((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()))).getRedefinedSchema();
                if (((externalSchema == null) || (redefinedSchema == null))) {
                    return ;
                }
                if (!(foundRestriction)) {
                    error("A redefined simple type must be a restriction");
                    return ;
                }
                if ((simpleTypeDef.getBaseTypeFingerprint() != simpleTypeDef.getFingerprint())) {
                    error("A redefined simple type must be based on the type with the same name");
                    return ;
                }
                Saxon.Hej.type.SimpleType oldSType = externalSchema.getSimpleType(simpleTypeDef.getTypeName());
                if ((oldSType == null)) {
                    error((("Cannot redefine the simple type "+simpleTypeDef.getDescription())+" because it is not present in the redefined schema"));
                    return ;
                }
                if (!(((oldSType is Saxon.Eej.ee.schema.UserSimpleType)))) {
                    error((("Cannot redefine a built-in simple type ("+simpleTypeDef.getDescription())+')'));
                    return ;
                }
                if (!(((Saxon.Eej.ee.schema.sdoc.XSDRedefine)(((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()))).getExternalSchemaDocumentURI().Equals(((Saxon.Eej.ee.schema.UserSimpleType)(oldSType)).getSchemaDocumentURI()))) {
                    compiler.warning((("The redefined simple type was found, but not in the schema document referenced by "+"the schemaLocation attribute of the containing <xs:redefine> element. This is not allowed by the XSD ")+"specification. However, Saxon does not currently enforce this rule."), Saxon.Hej.trans.SaxonErrorCode.SXSD1016, this);
                }
                simpleTypeDef.setRedefinitionLevel((oldSType.getRedefinitionLevel()+1));
                Saxon.Eej.ee.schema.UserSimpleType oldType = (Saxon.Eej.ee.schema.UserSimpleType)(oldSType);
                Saxon.Hej.om.NamespaceUri uri = getXSDSchema().getTargetNamespace();
                Saxon.Hej.om.NamePool pool = getNamePool();
                string localName = simpleTypeDef.getName();
                string prefix = simpleTypeDef.getTypeName().getPrefix();
                string newLocalName = ((localName+'_')+simpleTypeDef.GetHashCode());
                int newFingerprint = pool.allocateFingerprint(uri, newLocalName);
                oldType.setTypeName(new Saxon.Hej.om.StructuredQName(prefix, uri, newLocalName), newFingerprint);
                Saxon.Eej.ee.schema.TypeReference @ref = new Saxon.Eej.ee.schema.TypeReference(newFingerprint, simpleTypeDef.getConfiguration(), this);
                simpleTypeDef.setBaseTypeReference(@ref);
                redefinedSchema.addType(oldType);
                redefinedSchema.addType(simpleTypeDef);
            } else {
                int fp = simpleTypeDef.getFingerprint();
                if (((fp != -1) && (fp == simpleTypeDef.getBaseTypeFingerprint()))) {
                    error("A simple type must not be based on itself");
                }
                if (((((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()) is Saxon.Eej.ee.schema.sdoc.XSDList) || (((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()) is Saxon.Eej.ee.schema.sdoc.XSDUnion))) {
                    simpleTypeDef.setContextComponent(((Saxon.Eej.ee.schema.sdoc.XSDSimpleType)(((/*1430*/Saxon.Hej.tree.linked.NodeImpl)((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()).getParent()))).getSimpleTypeDefinition());
                }
            }
        }
    
    }
}