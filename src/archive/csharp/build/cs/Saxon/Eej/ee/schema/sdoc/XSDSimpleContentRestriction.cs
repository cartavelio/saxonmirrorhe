using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema.sdoc {
    public class XSDSimpleContentRestriction : Saxon.Eej.ee.schema.sdoc.SchemaElement {
        private Saxon.Eej.ee.schema.UserComplexType complexType = null;
        private Saxon.Eej.ee.schema.sdoc.SimpleTypeDefinition simpleTypeDef = null;
        public XSDSimpleContentRestriction()  {
        }
        protected internal override void prepareAttributes() {
            Saxon.Eej.ee.schema.PreparedSchema schema = getSchema();
            try {
                complexType = getContainingComplexType();
            } catch (Saxon.Hej.type.SchemaException e) {
                e.setLocator(this);
                error(e);
                return ;
            }
            complexType.setDerivationMethodName("restriction");
            complexType.setRestriction(true);
            simpleTypeDef = new Saxon.Eej.ee.schema.sdoc.SimpleTypeDefinition((Saxon.Eej.config.EnterpriseConfiguration)(getConfiguration()));
            simpleTypeDef.setLocator(this);
            simpleTypeDef.setIsSimpleContent(true);
            simpleTypeDef.setDerivationMethod(Saxon.Hej.type.Derivation.DERIVATION_RESTRICTION);
            simpleTypeDef.setContextComponent(complexType);
            Saxon.Hej.om.NodeName nn = allocateAnonymousTypeCode();
            simpleTypeDef.setTypeName(nn.getStructuredQName(), nn.getFingerprint());
            schema.addType(simpleTypeDef);
            Saxon.Eej.ee.schema.TypeReference @ref = new Saxon.Eej.ee.schema.TypeReference(simpleTypeDef.getFingerprint(), (Saxon.Eej.config.EnterpriseConfiguration)(getConfiguration()), this);
            @ref.setTarget(simpleTypeDef);
            complexType.setSimpleContentTypeReference(@ref);
            Saxon.Hej.om.AttributeMap atts = attributes();
            processId();
            string[] allowed = {"base", "id"};
            allowAttributes(atts, allowed);
            string @base = atts.getValue("", "base");
            if ((@base == null)) {
                missingAttribute("base");
            } else {
                int baseFingerprint;
                try {
                    baseFingerprint = getFingerprint(@base, USE_DEFAULT_NS);
                } catch (Saxon.Hej.type.SchemaException err) {
                    error(err.Message);
                    return ;
                }
                checkPermittedType(baseFingerprint, @base, "a base type");
                Saxon.Eej.ee.schema.TypeReference baseType = new Saxon.Eej.ee.schema.TypeReference(baseFingerprint, (Saxon.Eej.config.EnterpriseConfiguration)(getConfiguration()), this);
                complexType.setBaseTypeReference(baseType);
            }
        }
        public override void validate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            checkContentModel(stateTerms, stateTargets, true);
            bool foundSimpleType = false;
            foreach (Saxon.Hej.om.NodeInfo child in children()) {
                string name = child.getLocalPart();
                if ("simpleType".Equals(name)) {
                    foundSimpleType = true;
                }
            }
            if (!(foundSimpleType)) {
                Saxon.Eej.ee.schema.TypeReference @ref = new Saxon.Eej.ee.schema.TypeReference(complexType.getBaseTypeFingerprint(), (Saxon.Eej.config.EnterpriseConfiguration)(getConfiguration()), this);
                simpleTypeDef.setBaseTypeReference(@ref);
            }
        }
        private const int WILD = -1;
        private readonly static int[] state0terms = {Saxon.Hej.om.StandardNames.XS_MIN_INCLUSIVE, Saxon.Hej.om.StandardNames.XS_EXPLICIT_TIMEZONE, Saxon.Hej.om.StandardNames.XS_ANNOTATION, Saxon.Hej.om.StandardNames.XS_PATTERN, Saxon.Hej.om.StandardNames.XS_MAX_INCLUSIVE, Saxon.Hej.om.StandardNames.XS_ASSERTION, Saxon.Hej.om.StandardNames.XS_LENGTH, Saxon.Hej.om.StandardNames.XS_MAX_SCALE, Saxon.Hej.om.StandardNames.XS_ANY_ATTRIBUTE, Saxon.Hej.om.StandardNames.XS_MIN_EXCLUSIVE, Saxon.Hej.om.StandardNames.XS_ATTRIBUTE_GROUP, Saxon.Hej.om.StandardNames.XS_SIMPLE_TYPE, Saxon.Hej.om.StandardNames.XS_WHITE_SPACE, Saxon.Hej.om.StandardNames.XS_MAX_EXCLUSIVE, Saxon.Hej.om.StandardNames.XS_ASSERT, Saxon.Hej.om.StandardNames.XS_FRACTION_DIGITS, Saxon.Hej.om.StandardNames.XS_MIN_LENGTH, Saxon.Hej.om.StandardNames.XS_TOTAL_DIGITS, Saxon.Hej.om.StandardNames.XS_MIN_SCALE, Saxon.Hej.om.StandardNames.XS_ENUMERATION, Saxon.Hej.om.StandardNames.XS_MAX_LENGTH, Saxon.Hej.om.StandardNames.XS_ATTRIBUTE, WILD};
        private readonly static int[] state0targets = {1, 1, 4, 1, 1, 1, 1, 1, 2, 1, 5, 6, 1, 1, 3, 1, 1, 1, 1, 1, 1, 5, 1};
        private readonly static int[] state1terms = {Saxon.Hej.om.StandardNames.XS_MIN_INCLUSIVE, Saxon.Hej.om.StandardNames.XS_EXPLICIT_TIMEZONE, Saxon.Hej.om.StandardNames.XS_PATTERN, Saxon.Hej.om.StandardNames.XS_MAX_INCLUSIVE, Saxon.Hej.om.StandardNames.XS_ASSERTION, Saxon.Hej.om.StandardNames.XS_LENGTH, Saxon.Hej.om.StandardNames.XS_MAX_SCALE, Saxon.Hej.om.StandardNames.XS_ANY_ATTRIBUTE, Saxon.Hej.om.StandardNames.XS_MIN_EXCLUSIVE, Saxon.Hej.om.StandardNames.XS_ATTRIBUTE_GROUP, Saxon.Hej.om.StandardNames.XS_WHITE_SPACE, Saxon.Hej.om.StandardNames.XS_MAX_EXCLUSIVE, Saxon.Hej.om.StandardNames.XS_ASSERT, Saxon.Hej.om.StandardNames.XS_FRACTION_DIGITS, Saxon.Hej.om.StandardNames.XS_MIN_LENGTH, Saxon.Hej.om.StandardNames.XS_TOTAL_DIGITS, Saxon.Hej.om.StandardNames.XS_MIN_SCALE, Saxon.Hej.om.StandardNames.XS_ENUMERATION, Saxon.Hej.om.StandardNames.XS_MAX_LENGTH, Saxon.Hej.om.StandardNames.XS_ATTRIBUTE, WILD};
        private readonly static int[] state1targets = {1, 1, 1, 1, 1, 1, 1, 2, 1, 5, 1, 1, 3, 1, 1, 1, 1, 1, 1, 5, 1};
        private readonly static int[] state2terms = {Saxon.Hej.om.StandardNames.XS_ASSERT};
        private readonly static int[] state2targets = {3};
        private readonly static int[] state3terms = {Saxon.Hej.om.StandardNames.XS_ASSERT};
        private readonly static int[] state3targets = {3};
        private readonly static int[] state4terms = {Saxon.Hej.om.StandardNames.XS_MIN_INCLUSIVE, Saxon.Hej.om.StandardNames.XS_EXPLICIT_TIMEZONE, Saxon.Hej.om.StandardNames.XS_PATTERN, Saxon.Hej.om.StandardNames.XS_MAX_INCLUSIVE, Saxon.Hej.om.StandardNames.XS_ASSERTION, Saxon.Hej.om.StandardNames.XS_LENGTH, Saxon.Hej.om.StandardNames.XS_MAX_SCALE, Saxon.Hej.om.StandardNames.XS_ANY_ATTRIBUTE, Saxon.Hej.om.StandardNames.XS_MIN_EXCLUSIVE, Saxon.Hej.om.StandardNames.XS_ATTRIBUTE_GROUP, Saxon.Hej.om.StandardNames.XS_SIMPLE_TYPE, Saxon.Hej.om.StandardNames.XS_WHITE_SPACE, Saxon.Hej.om.StandardNames.XS_MAX_EXCLUSIVE, Saxon.Hej.om.StandardNames.XS_ASSERT, Saxon.Hej.om.StandardNames.XS_FRACTION_DIGITS, Saxon.Hej.om.StandardNames.XS_MIN_LENGTH, Saxon.Hej.om.StandardNames.XS_TOTAL_DIGITS, Saxon.Hej.om.StandardNames.XS_MIN_SCALE, Saxon.Hej.om.StandardNames.XS_ENUMERATION, Saxon.Hej.om.StandardNames.XS_MAX_LENGTH, Saxon.Hej.om.StandardNames.XS_ATTRIBUTE, WILD};
        private readonly static int[] state4targets = {1, 1, 1, 1, 1, 1, 1, 2, 1, 5, 6, 1, 1, 3, 1, 1, 1, 1, 1, 1, 5, 1};
        private readonly static int[] state5terms = {Saxon.Hej.om.StandardNames.XS_ANY_ATTRIBUTE, Saxon.Hej.om.StandardNames.XS_ASSERT, Saxon.Hej.om.StandardNames.XS_ATTRIBUTE, Saxon.Hej.om.StandardNames.XS_ATTRIBUTE_GROUP};
        private readonly static int[] state5targets = {2, 3, 5, 5};
        private readonly static int[] state6terms = {Saxon.Hej.om.StandardNames.XS_MIN_INCLUSIVE, Saxon.Hej.om.StandardNames.XS_EXPLICIT_TIMEZONE, Saxon.Hej.om.StandardNames.XS_PATTERN, Saxon.Hej.om.StandardNames.XS_MAX_INCLUSIVE, Saxon.Hej.om.StandardNames.XS_ASSERTION, Saxon.Hej.om.StandardNames.XS_LENGTH, Saxon.Hej.om.StandardNames.XS_MAX_SCALE, Saxon.Hej.om.StandardNames.XS_ANY_ATTRIBUTE, Saxon.Hej.om.StandardNames.XS_MIN_EXCLUSIVE, Saxon.Hej.om.StandardNames.XS_ATTRIBUTE_GROUP, Saxon.Hej.om.StandardNames.XS_WHITE_SPACE, Saxon.Hej.om.StandardNames.XS_MAX_EXCLUSIVE, Saxon.Hej.om.StandardNames.XS_ASSERT, Saxon.Hej.om.StandardNames.XS_FRACTION_DIGITS, Saxon.Hej.om.StandardNames.XS_MIN_LENGTH, Saxon.Hej.om.StandardNames.XS_TOTAL_DIGITS, Saxon.Hej.om.StandardNames.XS_MIN_SCALE, Saxon.Hej.om.StandardNames.XS_ENUMERATION, Saxon.Hej.om.StandardNames.XS_MAX_LENGTH, Saxon.Hej.om.StandardNames.XS_ATTRIBUTE, WILD};
        private readonly static int[] state6targets = {1, 1, 1, 1, 1, 1, 1, 2, 1, 5, 1, 1, 3, 1, 1, 1, 1, 1, 1, 5, 1};
        private readonly static int[][] stateTerms = {state0terms, state1terms, state2terms, state3terms, state4terms, state5terms, state6terms};
        private readonly static int[][] stateTargets = {state0targets, state1targets, state2targets, state3targets, state4targets, state5targets, state6targets};
        protected internal override bool isFinalState(int state) {
            return (((((((state == 0) || (state == 1)) || (state == 2)) || (state == 3)) || (state == 4)) || (state == 5)) || (state == 6));
        }
        public override void postValidate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            Saxon.Hej.z.IntSet facetsSeen = new Saxon.Hej.z.IntHashSet();
            foreach (Saxon.Hej.om.NodeInfo child in children()) {
                int fp = child.getFingerprint();
                switch (fp) {
                  case Saxon.Hej.om.StandardNames.XS_ANNOTATION:
                    break ;
                  case Saxon.Hej.om.StandardNames.XS_ANY_ATTRIBUTE:
                    Saxon.Eej.ee.schema.AttributeWildcard wildcard = ((Saxon.Eej.ee.schema.sdoc.XSDAnyAttribute)(child)).getWildcard();
                    complexType.setAnyAttribute(wildcard);
                    break ;
                  case Saxon.Hej.om.StandardNames.XS_ATTRIBUTE:
                    Saxon.Eej.ee.schema.AttributeUse attrDecl = ((Saxon.Eej.ee.schema.sdoc.XSDAttribute)(child)).getAttributeUse();
                    complexType.addAttributeUse(attrDecl);
                    break ;
                  case Saxon.Hej.om.StandardNames.XS_ATTRIBUTE_GROUP:
                    Saxon.Eej.ee.schema.AttributeGroupReference attrGroupRef = ((Saxon.Eej.ee.schema.sdoc.XSDAttributeGroup)(child)).getAttributeGroupReference();
                    complexType.addAttributeGroupReference(attrGroupRef);
                    break ;
                  case Saxon.Hej.om.StandardNames.XS_SIMPLE_TYPE:
                    Saxon.Eej.ee.schema.sdoc.SimpleTypeDefinition type = ((Saxon.Eej.ee.schema.sdoc.XSDSimpleType)(child)).getSimpleTypeDefinition();
                    Saxon.Eej.ee.schema.TypeReference @ref = new Saxon.Eej.ee.schema.TypeReference(type.getFingerprint(), (Saxon.Eej.config.EnterpriseConfiguration)(getConfiguration()), this);
                    simpleTypeDef.setBaseTypeReference(@ref);
                    break ;
                  case Saxon.Hej.om.StandardNames.XS_ASSERT:
                    {
                        Saxon.Eej.ee.schema.Assertion assertion = ((Saxon.Eej.ee.schema.sdoc.XSDAssert)(child)).getAssertion();
                        complexType.addAssertion(assertion);
                        break ;
                    }
                  case Saxon.Hej.om.StandardNames.XS_ASSERTION:
                    {
                        if ((compiler.getLanguageVersion() == Saxon.Hej.Configuration.XSD10)) {
                            error("To use xs:assertion, you must set xsdversion=1.1");
                        }
                        Saxon.Eej.ee.schema.Assertion assertion = ((Saxon.Eej.ee.schema.sdoc.XSDAssert)(child)).getAssertion();
                        Saxon.Eej.ee.schema.Facet af = new Saxon.Eej.ee.schema.AssertionFacet(assertion);
                        af.setMessage(assertion.getMessage());
                        if ((simpleTypeDef != null)) {
                            simpleTypeDef.addFacet(af);
                        }
                        break ;
                    }
                  case Saxon.Hej.om.StandardNames.XS_MIN_LENGTH:
                  case Saxon.Hej.om.StandardNames.XS_FRACTION_DIGITS:
                  case Saxon.Hej.om.StandardNames.XS_MAX_LENGTH:
                  case Saxon.Hej.om.StandardNames.XS_MAX_INCLUSIVE:
                  case Saxon.Hej.om.StandardNames.XS_MIN_INCLUSIVE:
                  case Saxon.Hej.om.StandardNames.XS_LENGTH:
                  case Saxon.Hej.om.StandardNames.XS_WHITE_SPACE:
                  case Saxon.Hej.om.StandardNames.XS_MAX_EXCLUSIVE:
                  case Saxon.Hej.om.StandardNames.XS_MIN_EXCLUSIVE:
                  case Saxon.Hej.om.StandardNames.XS_MAX_SCALE:
                  case Saxon.Hej.om.StandardNames.XS_MIN_SCALE:
                  case Saxon.Hej.om.StandardNames.XS_EXPLICIT_TIMEZONE:
                  case Saxon.Hej.om.StandardNames.XS_TOTAL_DIGITS:
                    if (facetsSeen.contains(fp)) {
                        error((("The "+child.getDisplayName())+" facet must not appear more than once"));
                    }
                    facetsSeen.add(fp);
                    goto case Saxon.Hej.om.StandardNames.XS_PATTERN;
                  case Saxon.Hej.om.StandardNames.XS_PATTERN:
                  case Saxon.Hej.om.StandardNames.XS_ENUMERATION:
                    Saxon.Eej.ee.schema.Facet facet = ((Saxon.Eej.ee.schema.sdoc.XSDFacet)(child)).getFacet();
                    if (((facet != null) && (simpleTypeDef != null))) {
                        simpleTypeDef.addFacet(facet);
                    }
                    break ;
                  default:
                    break;
                }
            }
        }
    
    }
}