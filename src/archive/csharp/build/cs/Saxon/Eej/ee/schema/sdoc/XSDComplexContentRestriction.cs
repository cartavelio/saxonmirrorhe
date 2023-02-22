using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema.sdoc {
    public class XSDComplexContentRestriction : Saxon.Eej.ee.schema.sdoc.SchemaElement {
        private Saxon.Eej.ee.schema.UserComplexType complexType = null;
        public XSDComplexContentRestriction()  {
        }
        protected internal override void prepareAttributes() {
            try {
                complexType = getContainingComplexType();
            } catch (Saxon.Hej.type.SchemaException e) {
                e.setLocator(this);
                error(e);
                return ;
            }
            complexType.setDerivationMethodName("restriction");
            complexType.setRestriction(true);
            Saxon.Hej.om.AttributeMap atts = attributes();
            string[] allowed = {"base", "id"};
            allowAttributes(atts, allowed);
            processId();
            string @base = atts.getValue(Saxon.Hej.om.NamespaceUri.NULL, "base");
            if ((@base != null)) {
                int baseFingerprint;
                try {
                    baseFingerprint = getFingerprint(@base, USE_DEFAULT_NS);
                } catch (Saxon.Hej.type.SchemaException err) {
                    error(err.Message);
                    return ;
                }
                if (((baseFingerprint == Saxon.Hej.om.StandardNames.XS_DAY_TIME_DURATION) || (baseFingerprint == Saxon.Hej.om.StandardNames.XS_YEAR_MONTH_DURATION))) {
                    warning((("Type "+@base)+" is recognized by Saxon but is not interoperable"));
                } else {
                    if (((baseFingerprint == Saxon.Hej.om.StandardNames.XS_ANY_ATOMIC_TYPE) || (baseFingerprint == Saxon.Hej.om.StandardNames.XS_UNTYPED))) {
                        error((("Type "+@base)+" cannot be used as a base type"));
                    }
                }
                Saxon.Eej.ee.schema.TypeReference baseType = new Saxon.Eej.ee.schema.TypeReference(baseFingerprint, (Saxon.Eej.config.EnterpriseConfiguration)(getConfiguration()), this);
                complexType.setBaseTypeReference(baseType);
            }
        }
        public override void validate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            checkContentModel(stateTerms, stateTargets, false);
        }
        private readonly static int[] state0terms = {Saxon.Hej.om.StandardNames.XS_CHOICE, Saxon.Hej.om.StandardNames.XS_SEQUENCE, Saxon.Hej.om.StandardNames.XS_ASSERT, Saxon.Hej.om.StandardNames.XS_OPEN_CONTENT, Saxon.Hej.om.StandardNames.XS_ANNOTATION, Saxon.Hej.om.StandardNames.XS_ATTRIBUTE_GROUP, Saxon.Hej.om.StandardNames.XS_ANY_ATTRIBUTE, Saxon.Hej.om.StandardNames.XS_ALL, Saxon.Hej.om.StandardNames.XS_ATTRIBUTE, Saxon.Hej.om.StandardNames.XS_GROUP};
        private readonly static int[] state0targets = {2, 2, 6, 4, 5, 1, 3, 2, 1, 2};
        private readonly static int[] state1terms = {Saxon.Hej.om.StandardNames.XS_ATTRIBUTE, Saxon.Hej.om.StandardNames.XS_ATTRIBUTE_GROUP, Saxon.Hej.om.StandardNames.XS_ANY_ATTRIBUTE, Saxon.Hej.om.StandardNames.XS_ASSERT};
        private readonly static int[] state1targets = {1, 1, 3, 6};
        private readonly static int[] state2terms = {Saxon.Hej.om.StandardNames.XS_ATTRIBUTE, Saxon.Hej.om.StandardNames.XS_ATTRIBUTE_GROUP, Saxon.Hej.om.StandardNames.XS_ANY_ATTRIBUTE, Saxon.Hej.om.StandardNames.XS_ASSERT};
        private readonly static int[] state2targets = {1, 1, 3, 6};
        private readonly static int[] state3terms = {Saxon.Hej.om.StandardNames.XS_ASSERT};
        private readonly static int[] state3targets = {6};
        private readonly static int[] state4terms = {Saxon.Hej.om.StandardNames.XS_GROUP, Saxon.Hej.om.StandardNames.XS_CHOICE, Saxon.Hej.om.StandardNames.XS_ALL, Saxon.Hej.om.StandardNames.XS_SEQUENCE};
        private readonly static int[] state4targets = {2, 2, 2, 2};
        private readonly static int[] state5terms = {Saxon.Hej.om.StandardNames.XS_CHOICE, Saxon.Hej.om.StandardNames.XS_SEQUENCE, Saxon.Hej.om.StandardNames.XS_ASSERT, Saxon.Hej.om.StandardNames.XS_OPEN_CONTENT, Saxon.Hej.om.StandardNames.XS_ATTRIBUTE_GROUP, Saxon.Hej.om.StandardNames.XS_ANY_ATTRIBUTE, Saxon.Hej.om.StandardNames.XS_ALL, Saxon.Hej.om.StandardNames.XS_ATTRIBUTE, Saxon.Hej.om.StandardNames.XS_GROUP};
        private readonly static int[] state5targets = {2, 2, 6, 4, 1, 3, 2, 1, 2};
        private readonly static int[] state6terms = {Saxon.Hej.om.StandardNames.XS_ASSERT};
        private readonly static int[] state6targets = {6};
        private readonly static int[][] stateTerms = {state0terms, state1terms, state2terms, state3terms, state4terms, state5terms, state6terms};
        private readonly static int[][] stateTargets = {state0targets, state1targets, state2targets, state3targets, state4targets, state5targets, state6targets};
        protected internal override bool isFinalState(int state) {
            return true;
        }
        public Saxon.Hej.type.ComplexType getComplexType() {
            return complexType;
        }
        public override void postValidate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            if ((complexType == null)) {
                return ;
            }
            foreach (Saxon.Hej.om.NodeInfo child in children()) {
                switch (child.getFingerprint()) {
                  case Saxon.Hej.om.StandardNames.XS_ANY_ATTRIBUTE:
                    {
                        Saxon.Eej.ee.schema.AttributeWildcard wildcard = ((Saxon.Eej.ee.schema.sdoc.XSDAnyAttribute)(child)).getWildcard();
                        complexType.setAnyAttribute(wildcard);
                        break ;
                    }
                  case Saxon.Hej.om.StandardNames.XS_ATTRIBUTE:
                    {
                        Saxon.Eej.ee.schema.AttributeUse attrDecl = ((Saxon.Eej.ee.schema.sdoc.XSDAttribute)(child)).getAttributeUse();
                        complexType.addAttributeUse(attrDecl);
                        break ;
                    }
                  case Saxon.Hej.om.StandardNames.XS_ATTRIBUTE_GROUP:
                    {
                        Saxon.Eej.ee.schema.AttributeGroupReference attrGroupRef = ((Saxon.Eej.ee.schema.sdoc.XSDAttributeGroup)(child)).getAttributeGroupReference();
                        complexType.addAttributeGroupReference(attrGroupRef);
                        break ;
                    }
                  case Saxon.Hej.om.StandardNames.XS_SEQUENCE:
                  case Saxon.Hej.om.StandardNames.XS_CHOICE:
                  case Saxon.Hej.om.StandardNames.XS_ALL:
                    {
                        complexType.setParticle(((Saxon.Eej.ee.schema.sdoc.XSDCompositor)(child)).getModelGroupParticle());
                        break ;
                    }
                  case Saxon.Hej.om.StandardNames.XS_GROUP:
                    {
                        Saxon.Eej.ee.schema.ModelGroupParticle modelGroup = ((Saxon.Eej.ee.schema.sdoc.XSDGroup)(child)).getGroupReference();
                        complexType.setParticle(modelGroup);
                        break ;
                    }
                  case Saxon.Hej.om.StandardNames.XS_ASSERT:
                    {
                        Saxon.Eej.ee.schema.Assertion assertion = ((Saxon.Eej.ee.schema.sdoc.XSDAssert)(child)).getAssertion();
                        complexType.addAssertion(assertion);
                        break ;
                    }
                  case Saxon.Hej.om.StandardNames.XS_OPEN_CONTENT:
                    {
                        Saxon.Eej.ee.schema.ElementWildcard ew = ((Saxon.Eej.ee.schema.sdoc.XSDOpenContent)(child)).getElementWildcard();
                        Saxon.Eej.ee.schema.Wildcard openContentWildcard = (((ew == null) ? null : ew.getWildcard()));
                        string openContentMode = ((Saxon.Eej.ee.schema.sdoc.XSDOpenContent)(child)).getMode();
                        complexType.setOpenContentWildcard(openContentWildcard, openContentMode, true);
                        break ;
                    }
                }
            }
        }
    
    }
}