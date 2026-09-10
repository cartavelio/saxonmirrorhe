using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema.sdoc {
    public class XSDSimpleTypeRestriction : Saxon.Eej.ee.schema.sdoc.SchemaElement {
        private Saxon.Eej.ee.schema.sdoc.SimpleTypeDefinition typeDefinition = null;
        private bool foundBase = false;
        public XSDSimpleTypeRestriction()  {
        }
        protected internal override void prepareAttributes() {
            typeDefinition = getContainingSimpleType();
            Saxon.Hej.om.AttributeMap atts = attributes();
            string[] allowed = {"base", "id"};
            allowAttributes(atts, allowed);
            processId();
            string @base = atts.getValue("", "base");
            foundBase = ((@base != null));
            if (foundBase) {
                int baseFingerprint;
                try {
                    baseFingerprint = getFingerprint(@base, USE_DEFAULT_NS);
                } catch (Saxon.Hej.type.SchemaException err) {
                    error(err.Message);
                    return ;
                }
                checkPermittedType(baseFingerprint, @base, "a base type");
                Saxon.Eej.ee.schema.TypeReference @ref = new Saxon.Eej.ee.schema.TypeReference(baseFingerprint, typeDefinition.getConfiguration(), this);
                typeDefinition.setBaseTypeReference(@ref);
            }
        }
        public override void validate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            checkContentModel(stateTerms, stateTargets, false);
            bool foundSimpleType = false;
            foreach (Saxon.Hej.om.NodeInfo child in children()) {
                int fp = child.getFingerprint();
                if ((fp == Saxon.Hej.om.StandardNames.XS_SIMPLE_TYPE)) {
                    foundSimpleType = true;
                    if (foundBase) {
                        mutuallyExclusiveElementAndAttribute("simpleType", "base");
                        return ;
                    }
                }
            }
            if ((!(foundBase) && !(foundSimpleType))) {
                error("A restriction element must either have a 'base' attribute or contain a 'simpleType' definition");
            }
        }
        private readonly static int[] state0terms = {Saxon.Hej.om.StandardNames.XS_MIN_LENGTH, Saxon.Hej.om.StandardNames.XS_FRACTION_DIGITS, Saxon.Hej.om.StandardNames.XS_MAX_LENGTH, Saxon.Hej.om.StandardNames.XS_SIMPLE_TYPE, Saxon.Hej.om.StandardNames.XS_ANNOTATION, Saxon.Hej.om.StandardNames.XS_PATTERN, Saxon.Hej.om.StandardNames.XS_ENUMERATION, Saxon.Hej.om.StandardNames.XS_MAX_INCLUSIVE, Saxon.Hej.om.StandardNames.XS_MIN_INCLUSIVE, Saxon.Hej.om.StandardNames.XS_LENGTH, Saxon.Hej.om.StandardNames.XS_WHITE_SPACE, Saxon.Hej.om.StandardNames.XS_MAX_EXCLUSIVE, Saxon.Hej.om.StandardNames.XS_MIN_EXCLUSIVE, Saxon.Hej.om.StandardNames.XS_EXPLICIT_TIMEZONE, Saxon.Hej.om.StandardNames.XS_TOTAL_DIGITS, Saxon.Hej.om.StandardNames.XS_ASSERTION, Saxon.Hej.om.StandardNames.SAXON_PREPROCESS, Saxon.Hej.om.StandardNames.SAXON_DISTINCT, Saxon.Hej.om.StandardNames.SAXON_ORDER, Saxon.Hej.om.StandardNames.XS_MIN_SCALE, Saxon.Hej.om.StandardNames.XS_MAX_SCALE};
        private readonly static int[] state0targets = {1, 1, 1, 2, 3, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1};
        private readonly static int[] state1terms = {Saxon.Hej.om.StandardNames.XS_MIN_LENGTH, Saxon.Hej.om.StandardNames.XS_FRACTION_DIGITS, Saxon.Hej.om.StandardNames.XS_MAX_LENGTH, Saxon.Hej.om.StandardNames.XS_PATTERN, Saxon.Hej.om.StandardNames.XS_ENUMERATION, Saxon.Hej.om.StandardNames.XS_MAX_INCLUSIVE, Saxon.Hej.om.StandardNames.XS_MIN_INCLUSIVE, Saxon.Hej.om.StandardNames.XS_LENGTH, Saxon.Hej.om.StandardNames.XS_WHITE_SPACE, Saxon.Hej.om.StandardNames.XS_MAX_EXCLUSIVE, Saxon.Hej.om.StandardNames.XS_MIN_EXCLUSIVE, Saxon.Hej.om.StandardNames.XS_EXPLICIT_TIMEZONE, Saxon.Hej.om.StandardNames.XS_TOTAL_DIGITS, Saxon.Hej.om.StandardNames.XS_ASSERTION, Saxon.Hej.om.StandardNames.XS_MIN_SCALE, Saxon.Hej.om.StandardNames.XS_MAX_SCALE, Saxon.Hej.om.StandardNames.SAXON_PREPROCESS, Saxon.Hej.om.StandardNames.SAXON_DISTINCT, Saxon.Hej.om.StandardNames.SAXON_ORDER};
        private readonly static int[] state1targets = {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1};
        private readonly static int[] state2terms = {Saxon.Hej.om.StandardNames.XS_MIN_LENGTH, Saxon.Hej.om.StandardNames.XS_FRACTION_DIGITS, Saxon.Hej.om.StandardNames.XS_MAX_LENGTH, Saxon.Hej.om.StandardNames.XS_PATTERN, Saxon.Hej.om.StandardNames.XS_ENUMERATION, Saxon.Hej.om.StandardNames.XS_MAX_INCLUSIVE, Saxon.Hej.om.StandardNames.XS_MIN_INCLUSIVE, Saxon.Hej.om.StandardNames.XS_LENGTH, Saxon.Hej.om.StandardNames.XS_WHITE_SPACE, Saxon.Hej.om.StandardNames.XS_MAX_EXCLUSIVE, Saxon.Hej.om.StandardNames.XS_MIN_EXCLUSIVE, Saxon.Hej.om.StandardNames.XS_EXPLICIT_TIMEZONE, Saxon.Hej.om.StandardNames.XS_TOTAL_DIGITS, Saxon.Hej.om.StandardNames.XS_ASSERTION, Saxon.Hej.om.StandardNames.XS_MIN_SCALE, Saxon.Hej.om.StandardNames.XS_MAX_SCALE, Saxon.Hej.om.StandardNames.SAXON_PREPROCESS, Saxon.Hej.om.StandardNames.SAXON_DISTINCT, Saxon.Hej.om.StandardNames.SAXON_ORDER};
        private readonly static int[] state2targets = {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1};
        private readonly static int[] state3terms = {Saxon.Hej.om.StandardNames.XS_MIN_LENGTH, Saxon.Hej.om.StandardNames.XS_FRACTION_DIGITS, Saxon.Hej.om.StandardNames.XS_MAX_LENGTH, Saxon.Hej.om.StandardNames.XS_SIMPLE_TYPE, Saxon.Hej.om.StandardNames.XS_PATTERN, Saxon.Hej.om.StandardNames.XS_ENUMERATION, Saxon.Hej.om.StandardNames.XS_MAX_INCLUSIVE, Saxon.Hej.om.StandardNames.XS_MIN_INCLUSIVE, Saxon.Hej.om.StandardNames.XS_LENGTH, Saxon.Hej.om.StandardNames.XS_WHITE_SPACE, Saxon.Hej.om.StandardNames.XS_MAX_EXCLUSIVE, Saxon.Hej.om.StandardNames.XS_MIN_EXCLUSIVE, Saxon.Hej.om.StandardNames.XS_EXPLICIT_TIMEZONE, Saxon.Hej.om.StandardNames.XS_TOTAL_DIGITS, Saxon.Hej.om.StandardNames.XS_ASSERTION, Saxon.Hej.om.StandardNames.XS_MIN_SCALE, Saxon.Hej.om.StandardNames.XS_MAX_SCALE, Saxon.Hej.om.StandardNames.SAXON_PREPROCESS, Saxon.Hej.om.StandardNames.SAXON_DISTINCT, Saxon.Hej.om.StandardNames.SAXON_ORDER};
        private readonly static int[] state3targets = {1, 1, 1, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1};
        private readonly static int[][] stateTerms = {state0terms, state1terms, state2terms, state3terms};
        private readonly static int[][] stateTargets = {state0targets, state1targets, state2targets, state3targets};
        protected internal override bool isFinalState(int state) {
            return ((((state == 0) || (state == 1)) || (state == 2)) || (state == 3));
        }
        public override void postValidate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            Saxon.Hej.z.IntSet facetsSeen = new Saxon.Hej.z.IntHashSet();
            foreach (Saxon.Hej.om.NodeInfo child in children()) {
                if ((typeDefinition != null)) {
                    int fp = child.getFingerprint();
                    switch (fp) {
                      case Saxon.Hej.om.StandardNames.XS_ANNOTATION:
                        break ;
                      case Saxon.Hej.om.StandardNames.XS_SIMPLE_TYPE:
                        Saxon.Eej.ee.schema.sdoc.SimpleTypeDefinition type = ((Saxon.Eej.ee.schema.sdoc.XSDSimpleType)(child)).getSimpleTypeDefinition();
                        Saxon.Eej.ee.schema.TypeReference @ref = new Saxon.Eej.ee.schema.TypeReference(type.getFingerprint(), compiler.getConfiguration(), this);
                        typeDefinition.setBaseTypeReference(@ref);
                        break ;
                      case Saxon.Hej.om.StandardNames.XS_ATTRIBUTE:
                      case Saxon.Hej.om.StandardNames.XS_ANY_ATTRIBUTE:
                      case Saxon.Hej.om.StandardNames.XS_ATTRIBUTE_GROUP:
                        break ;
                      case Saxon.Hej.om.StandardNames.XS_ASSERTION:
                        if ((compiler.getLanguageVersion() == Saxon.Hej.Configuration.XSD10)) {
                            error("To use xs:assertion, you must set xsdversion=1.1");
                        }
                        Saxon.Eej.ee.schema.Assertion assertion = ((Saxon.Eej.ee.schema.sdoc.XSDAssert)(child)).getAssertion();
                        Saxon.Eej.ee.schema.Facet af = new Saxon.Eej.ee.schema.AssertionFacet(assertion);
                        af.setMessage(assertion.getMessage());
                        typeDefinition.addFacet(af);
                        break ;
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
                      case Saxon.Hej.om.StandardNames.SAXON_DISTINCT:
                      case Saxon.Hej.om.StandardNames.SAXON_ORDER:
                        if (facetsSeen.contains(fp)) {
                            error((("The "+child.getDisplayName())+" facet must not appear more than once"));
                        }
                        facetsSeen.add(fp);
                        goto case Saxon.Hej.om.StandardNames.XS_PATTERN;
                      case Saxon.Hej.om.StandardNames.XS_PATTERN:
                      case Saxon.Hej.om.StandardNames.XS_ENUMERATION:
                        Saxon.Eej.ee.schema.Facet facet = ((Saxon.Eej.ee.schema.sdoc.XSDFacet)(child)).getFacet();
                        if ((facet != null)) {
                            typeDefinition.addFacet(facet);
                        }
                        break ;
                      case Saxon.Hej.om.StandardNames.SAXON_PREPROCESS:
                        if ((compiler.getLanguageVersion() == Saxon.Hej.Configuration.XSD10)) {
                            error("To use saxon:preprocess, you must set xsdversion=1.1");
                        }
                        Saxon.Eej.ee.schema.Assertion action = ((Saxon.Eej.ee.schema.sdoc.SaxonPreprocess)(child)).getAction();
                        Saxon.Eej.ee.schema.Assertion reverseAction = ((Saxon.Eej.ee.schema.sdoc.SaxonPreprocess)(child)).getReverseAction();
                        Saxon.Eej.ee.schema.Facet af2 = new Saxon.Eej.ee.schema.PreprocessFacet(action, reverseAction);
                        typeDefinition.addFacet(af2);
                        break ;
                      default:
                        break;
                    }
                }
            }
        }
    
    }
}