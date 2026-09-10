using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema.sdoc {
    public class XSDAlternative : Saxon.Eej.ee.schema.sdoc.AnnotationParent {
        private Saxon.Eej.ee.schema.TypeAlternative alternative = null;
        public XSDAlternative()  {
        }
        protected internal override void prepareAttributes() {
            Saxon.Hej.om.AttributeMap atts = attributes();
            string[] allowed = {"id", "test", "type", "xpathDefaultNamespace"};
            allowAttributes(atts, allowed);
            string defaultNamespace = atts.getValue("", "xpathDefaultNamespace");
            if ((defaultNamespace == null)) {
                defaultNamespace = getXSDSchema().getXPathDefaultNamespace();
            }
            if (defaultNamespace.Equals("##defaultNamespace")) {
                defaultNamespace = getURIForPrefix("", true);
            } else {
                if (defaultNamespace.Equals("##targetNamespace")) {
                    defaultNamespace = getXSDSchema().getTargetNamespace();
                } else {
                    if (defaultNamespace.Equals("##local")) {
                        defaultNamespace = "";
                    }
                }
            }
            string xpath = atts.getValue("", "test");
            Saxon.Hej.sxpath.XPathExpression condition = null;
            System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.sxpath.XPathVariable> parameters = new System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.sxpath.XPathVariable>();
            if ((xpath != null)) {
                Saxon.Eej.ee.schema.sdoc.SchemaElement.XPathStaticContext env = new Saxon.Eej.ee.schema.sdoc.SchemaElement.XPathStaticContext(this);
                env.setDefaultElementNamespace(defaultNamespace);
                env.setBaseURI(getBaseURI());
                env.setXPathLanguageLevel((getXSDSchema().getAllowedExtensions().Contains("any-xpath-version") ? 31 : 20));
                foreach (Saxon.Hej.expr.instruct.GlobalParam param in getXSDSchema().getGlobalParams()) {
                    Saxon.Hej.sxpath.XPathVariable var = env.declareVariable(param.getVariableQName());
                    Saxon.Hej.om.GroundedValue defaultVal = ((Saxon.Hej.expr.Literal)(param.getBody())).getGroundedValue();
                    var.setDefaultValue(defaultVal);
                    Saxon.Impl.Helpers.MapUtils.Put(parameters, param.getVariableQName(), var);
                }
                Saxon.Hej.sxpath.XPathEvaluator xpe = new Saxon.Hej.sxpath.XPathEvaluator(getSchema().getConfiguration());
                try {
                    xpe.setStaticContext(env);
                    condition = xpe.createExpression(xpath);
                } catch (Saxon.Hej.trans.XPathException err) {
                    error(err.Message);
                    try {
                        condition = xpe.createExpression("false()");
                    } catch (Saxon.Hej.trans.XPathException e) {
                    }
                }
            }
            Saxon.Eej.config.EnterpriseConfiguration config = getSchema().getConfiguration();
            alternative = new Saxon.Eej.ee.schema.TypeAlternative(config, condition, xpath);
            alternative.setLocator(this);
            alternative.setNamespaceContext(makeNamespaceContext());
            alternative.setDefaultElementNamespace(defaultNamespace);
            alternative.setBaseURI(getBaseURI());
            alternative.setParameters(parameters);
            string attValue = atts.getValue("", "type");
            if ((attValue != null)) {
                int fp;
                try {
                    fp = getFingerprint(attValue, USE_DEFAULT_NS);
                    if (((fp == Saxon.Hej.om.StandardNames.XS_ANY_ATOMIC_TYPE) || (fp == Saxon.Hej.om.StandardNames.XS_UNTYPED))) {
                        error((("Type "+attValue)+" cannot be used as an element type"));
                    }
                    alternative.setTypeReference(new Saxon.Eej.ee.schema.TypeReference(fp, config, this));
                } catch (Saxon.Hej.type.SchemaException err) {
                    error(err.Message);
                }
            }
            processId();
        }
        public override void validate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            checkContentModel(stateTerms, stateTargets, false);
            foreach (Saxon.Hej.om.NodeInfo child in children()) {
                int childfp = child.getFingerprint();
                switch (childfp) {
                  case Saxon.Hej.om.StandardNames.XS_ANNOTATION:
                    {
                        break ;
                    }
                  case Saxon.Hej.om.StandardNames.XS_COMPLEX_TYPE:
                    {
                        if ((alternative.getTypeReference() != null)) {
                            mutuallyExclusiveElementAndAttribute("complexType", "type");
                        }
                        break ;
                    }
                  case Saxon.Hej.om.StandardNames.XS_SIMPLE_TYPE:
                    {
                        if ((alternative.getTypeReference() != null)) {
                            mutuallyExclusiveElementAndAttribute("simpleType", "type");
                        }
                        break ;
                    }
                }
            }
        }
        private readonly static int[] state0terms = {Saxon.Hej.om.StandardNames.XS_ANNOTATION, Saxon.Hej.om.StandardNames.XS_SIMPLE_TYPE, Saxon.Hej.om.StandardNames.XS_COMPLEX_TYPE};
        private readonly static int[] state0targets = {1, 2, 2};
        private readonly static int[] state1terms = {Saxon.Hej.om.StandardNames.XS_SIMPLE_TYPE, Saxon.Hej.om.StandardNames.XS_COMPLEX_TYPE};
        private readonly static int[] state1targets = {2, 2};
        private readonly static int[] state2terms = {};
        private readonly static int[] state2targets = {};
        private readonly static int[][] stateTerms = {state0terms, state1terms, state2terms};
        private readonly static int[][] stateTargets = {state0targets, state1targets, state2targets};
        protected internal override bool isFinalState(int state) {
            return (((state == 0) || (state == 1)) || (state == 2));
        }
        public override void postValidate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            Saxon.Eej.config.EnterpriseConfiguration config = (Saxon.Eej.config.EnterpriseConfiguration)(getConfiguration());
            foreach (Saxon.Hej.om.NodeInfo child in children()) {
                int childfp = child.getFingerprint();
                switch (childfp) {
                  case Saxon.Hej.om.StandardNames.XS_COMPLEX_TYPE:
                    {
                        Saxon.Eej.ee.schema.UserDefinedType schemaType = ((Saxon.Eej.ee.schema.sdoc.XSDComplexType)(child)).getComplexType();
                        Saxon.Eej.ee.schema.TypeReference @ref = new Saxon.Eej.ee.schema.TypeReference(schemaType.getFingerprint(), config, child);
                        @ref.setTarget(schemaType);
                        alternative.setTypeReference(@ref);
                        schemaType.setContainingDeclaration(((Saxon.Eej.ee.schema.sdoc.XSDElement)(((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()))).getElementDeclaration().getComponentName(), true);
                        break ;
                    }
                  case Saxon.Hej.om.StandardNames.XS_SIMPLE_TYPE:
                    {
                        Saxon.Eej.ee.schema.UserDefinedType schemaType = ((Saxon.Eej.ee.schema.sdoc.XSDSimpleType)(child)).getSimpleTypeDefinition();
                        Saxon.Eej.ee.schema.TypeReference @ref = new Saxon.Eej.ee.schema.TypeReference(schemaType.getFingerprint(), config, child);
                        @ref.setTarget(schemaType);
                        alternative.setTypeReference(@ref);
                        schemaType.setContainingDeclaration(((Saxon.Eej.ee.schema.sdoc.XSDElement)(((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()))).getElementDeclaration().getComponentName(), true);
                        break ;
                    }
                }
            }
        }
        public Saxon.Eej.ee.schema.TypeAlternative getTypeAlternative() {
            return alternative;
        }
    
    }
}