using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema.sdoc {
    public class XSDElement : Saxon.Eej.ee.schema.sdoc.SchemaElement {
        private bool foundTypeReference = false;
        private bool isReference = false;
        private Saxon.Hej.om.StructuredQName elementName;
        private Saxon.Eej.ee.schema.ElementDecl element = null;
        private Saxon.Eej.ee.schema.ElementParticle elementParticle = null;
        public XSDElement()  {
        }
        protected internal override void prepareAttributes() {
            Saxon.Eej.ee.schema.PreparedSchema schema = getSchema();
            Saxon.Hej.om.AttributeMap atts = attributes();
            isReference = (atts.getValue(Saxon.Hej.om.NamespaceUri.NULL, "ref") != null);
            if (isTopLevel()) {
                string[] allowed = {"abstract", "block", "default", "final", "fixed", "id", "name", "nillable", "substitutionGroup", "type"};
                allowAttributes(atts, allowed);
                requireAttribute(atts, "name");
            } else {
                string[] allowed = {"block", "default", "fixed", "form", "id", "maxOccurs", "minOccurs", "name", "nillable", "ref", "targetNamespace", "type"};
                allowAttributes(atts, allowed);
            }
            string form = atts.getValue(Saxon.Hej.om.NamespaceUri.NULL, "form");
            if ((form != null)) {
                if ((form.Equals("qualified") || form.Equals("unqualified"))) {
                } else {
                    invalidAttributeValue("form", form, "Must be 'qualified' or 'unqualified'");
                }
            } else {
                if (isTopLevel()) {
                    form = "qualified";
                } else {
                    form = getXSDSchema().getElementFormDefault();
                    if ((form == null)) {
                        form = "unqualified";
                    }
                }
            }
            string localTargetNamespace = atts.getValue(Saxon.Hej.om.NamespaceUri.NULL, "targetNamespace");
            if ((localTargetNamespace != null)) {
                if ((getXSDSchema().getSchemaCompiler().getLanguageVersion() == Saxon.Hej.Configuration.XSD10)) {
                    error("The targetNamespace cannot be defined locally unless XSD 1.1 is enabled");
                }
                if ((atts.getValue(Saxon.Hej.om.NamespaceUri.NULL, "name") == null)) {
                    error("If there is no @name then there must be no @targetNamespace");
                }
                checkMutuallyExclusiveAttributes("targetNamespace", "form");
            }
            string attValue = atts.getValue(Saxon.Hej.om.NamespaceUri.NULL, "name");
            if ((attValue != null)) {
                element = new Saxon.Eej.ee.schema.ElementDecl((Saxon.Eej.config.EnterpriseConfiguration)(getConfiguration()), isTopLevel(), this);
                element.setGeneratedId(Saxon.Hej.tree.util.Navigator.getPath(this));
                element.setNamespaceResolver(makeNamespaceContext());
                Saxon.Hej.om.StructuredQName nc;
                try {
                    if (((localTargetNamespace != null) || "qualified".Equals(form))) {
                        nc = getComponentName(attValue, REQUIRE_TARGET_NS);
                    } else {
                        nc = getComponentName(attValue, REQUIRE_NULL_NS);
                    }
                } catch (Saxon.Hej.type.SchemaException err) {
                    nc = Saxon.Hej.om.StandardNames.SQ_XS_INVALID_NAME;
                    error(err.Message);
                }
                elementName = nc;
                element.setElementName(nc);
                if (!(isTopLevel())) {
                    elementParticle = new Saxon.Eej.ee.schema.ElementParticle((Saxon.Eej.config.EnterpriseConfiguration)(getConfiguration()), this);
                    elementParticle.setTargetComponentName(nc);
                    elementParticle.setReference(element);
                    schema.addComponent(elementParticle);
                    if ((((atts.getValue(Saxon.Hej.om.NamespaceUri.NULL, "type") == null) && !(hasChildNodes())) && !(isReference))) {
                        bool existsAsTopLevel = (schema.getElementDecl(nc) != null);
                        if (!(existsAsTopLevel)) {
                            string localName = nc.getLocalPart();
                            Saxon.Eej.ee.schema.sdoc.XSDSchema s = getXSDSchema();
                            foreach (Saxon.Hej.om.NodeInfo ignored in s.children(Saxon.Hej.pattern.NodeSelector.of(x => ((x is Saxon.Eej.ee.schema.sdoc.XSDElement) && localName.Equals(x.getAttributeValue(Saxon.Hej.om.NamespaceUri.NULL, "name")))))) {
                                existsAsTopLevel = true;
                                break ;
                            }
                        }
                        if (existsAsTopLevel) {
                            warning((((((("Local element declaration matches the name of a global element, and allows any content. "+"Did you mean to write ref='")+attValue)+"' instead of name='")+attValue)+"'? ")+"To suppress this warning, add type='xs:anyType'"));
                        }
                    }
                }
            } else {
                if (!(isReference)) {
                    error("Either the @name or the @ref attribute must be supplied");
                }
            }
            attValue = atts.getValue(Saxon.Hej.om.NamespaceUri.NULL, "ref");
            if ((attValue != null)) {
                Saxon.Hej.om.StructuredQName nc;
                try {
                    nc = getComponentName(attValue, USE_DEFAULT_NS);
                } catch (Saxon.Hej.type.SchemaException err) {
                    nc = Saxon.Hej.om.StandardNames.SQ_XS_INVALID_NAME;
                    error(err.Message);
                }
                elementName = nc;
                elementParticle = new Saxon.Eej.ee.schema.ElementParticle((Saxon.Eej.config.EnterpriseConfiguration)(getConfiguration()), this);
                elementParticle.setTargetComponentName(nc);
                schema.addComponent(elementParticle);
                checkMutuallyExclusiveAttributes("ref", "name");
                checkMutuallyExclusiveAttributes("ref", "block");
                checkMutuallyExclusiveAttributes("ref", "default");
                checkMutuallyExclusiveAttributes("ref", "fixed");
                checkMutuallyExclusiveAttributes("ref", "form");
                checkMutuallyExclusiveAttributes("ref", "nillable");
                checkMutuallyExclusiveAttributes("ref", "type");
            }
            if (((element == null) && isTopLevel())) {
                element = new Saxon.Eej.ee.schema.ElementDecl((Saxon.Eej.config.EnterpriseConfiguration)(getConfiguration()), isTopLevel(), this);
                Saxon.Ejava.lang.StringBuilder buff = new Saxon.Ejava.lang.StringBuilder(64);
                generateId(buff);
                element.setGeneratedId(buff.ToString());
                element.setNamespaceResolver(getAllNamespaces());
                element.setElementName(new Saxon.Hej.om.StructuredQName("", Saxon.Hej.om.NamespaceUri.NULL, ("unnamed-error-element-at-line-"+getLineNumber())));
            }
            if (((elementParticle == null) && !(isTopLevel()))) {
                element = new Saxon.Eej.ee.schema.ElementDecl((Saxon.Eej.config.EnterpriseConfiguration)(getConfiguration()), isTopLevel(), this);
                Saxon.Ejava.lang.StringBuilder buff = new Saxon.Ejava.lang.StringBuilder(64);
                generateId(buff);
                element.setGeneratedId(buff.ToString());
                element.setNamespaceResolver(getAllNamespaces());
                Saxon.Hej.om.StructuredQName nameCode = new Saxon.Hej.om.StructuredQName("", Saxon.Hej.om.NamespaceUri.NULL, ("unnamed-error-element-at-line-"+getLineNumber()));
                element.setElementName(nameCode);
                elementParticle = new Saxon.Eej.ee.schema.ElementParticle((Saxon.Eej.config.EnterpriseConfiguration)(getConfiguration()), this);
                elementParticle.setTargetComponentName(nameCode);
            }
            processId();
            if ((element != null)) {
                attValue = atts.getValue(Saxon.Hej.om.NamespaceUri.NULL, "abstract");
                if ((attValue != null)) {
                    bool isAbstract = parseBooleanAttribute("abstract", attValue);
                    element.setAbstract(isAbstract);
                }
                attValue = atts.getValue(Saxon.Hej.om.NamespaceUri.NULL, "block");
                if ((attValue == null)) {
                    if ((element != null)) {
                        element.setBlock(getXSDSchema().getBlockDefault());
                    }
                } else {
                    int f = parseFinalOrBlock("block", attValue, ((Saxon.Hej.type.Derivation.DERIVATION_EXTENSION|Saxon.Hej.type.Derivation.DERIVATION_RESTRICTION)|Saxon.Hej.type.Derivation.DERIVE_BY_SUBSTITUTION));
                    element.setBlock(f);
                }
                attValue = atts.getValue(Saxon.Hej.om.NamespaceUri.NULL, "default");
                if ((attValue != null)) {
                    element.setValueConstraint(new Saxon.Eej.ee.schema.ValueConstraint(Saxon.Eej.ee.schema.ValueConstraint.Variety.DEFAULT, Saxon.Hej.str.StringView.tidy(attValue)));
                }
                attValue = atts.getValue(Saxon.Hej.om.NamespaceUri.NULL, "final");
                if ((attValue == null)) {
                    if ((element != null)) {
                        element.setFinalProhibitions(getXSDSchema().getFinalDefault());
                    }
                } else {
                    int f = parseFinalOrBlock("final", attValue, (Saxon.Hej.type.Derivation.DERIVATION_EXTENSION|Saxon.Hej.type.Derivation.DERIVATION_RESTRICTION));
                    element.setFinalProhibitions(f);
                }
                attValue = atts.getValue(Saxon.Hej.om.NamespaceUri.NULL, "fixed");
                if ((attValue != null)) {
                    if ((atts.getValue(Saxon.Hej.om.NamespaceUri.NULL, "default") != null)) {
                        mutuallyExclusiveAttributes("default", "fixed");
                    } else {
                        element.setValueConstraint(new Saxon.Eej.ee.schema.ValueConstraint(Saxon.Eej.ee.schema.ValueConstraint.Variety.FIXED, Saxon.Hej.str.StringView.tidy(attValue)));
                    }
                }
                attValue = atts.getValue(Saxon.Hej.om.NamespaceUri.NULL, "substitutionGroup");
                if ((attValue != null)) {
                    Saxon.Ejava.util.StringTokenizer st = new Saxon.Ejava.util.StringTokenizer(attValue, " \t\r\n");
                    while (st.hasMoreTokens()) {
                        string head = st.nextToken();
                        try {
                            Saxon.Hej.om.StructuredQName qName = getComponentName(head, USE_DEFAULT_NS);
                            element.addSubstitutionGroupHead(qName);
                        } catch (Saxon.Hej.type.SchemaException err) {
                            error(err.Message);
                        }
                    }
                }
                attValue = atts.getValue(Saxon.Hej.om.NamespaceUri.NULL, "type");
                if ((attValue != null)) {
                    foundTypeReference = true;
                    int fp;
                    try {
                        fp = getFingerprint(attValue, USE_DEFAULT_NS);
                        checkPermittedType(fp, attValue, "an element type");
                        Saxon.Eej.ee.schema.TypeReference @ref = new Saxon.Eej.ee.schema.TypeReference(fp, element.getConfiguration(), this);
                        element.setTypeReference(@ref);
                    } catch (Saxon.Hej.type.SchemaException err) {
                        error(err.Message);
                    }
                }
                attValue = atts.getValue(Saxon.Hej.om.NamespaceUri.NULL, "nillable");
                if ((attValue != null)) {
                    bool isNillable = parseBooleanAttribute("nillable", attValue);
                    element.setNillable(isNillable);
                }
            }
            if ((elementParticle != null)) {
                attValue = atts.getValue(Saxon.Hej.om.NamespaceUri.NULL, "minOccurs");
                int minOccurs = 1;
                if ((attValue != null)) {
                    minOccurs = processMinOccurs(attValue);
                    elementParticle.setMinOccurs(minOccurs);
                }
                attValue = atts.getValue(Saxon.Hej.om.NamespaceUri.NULL, "maxOccurs");
                if ((attValue != null)) {
                    int maxOccurs = processMaxOccurs(attValue);
                    if (((maxOccurs>=0) && (maxOccurs<minOccurs))) {
                        error("maxOccurs is less than minOccurs");
                    }
                    elementParticle.setMaxOccurs(maxOccurs);
                } else {
                    if ((minOccurs>0)) {
                        elementParticle.setMaxOccurs(minOccurs);
                    } else {
                        elementParticle.setMaxOccurs(1);
                    }
                    int maxOccurs = elementParticle.getMaxOccurs();
                    if (((maxOccurs != -1) && (elementParticle.getMinOccurs()>maxOccurs))) {
                        error("maxOccurs must not be less than minOccurs");
                    }
                }
            }
        }
        public Saxon.Eej.ee.schema.ElementDecl getElementDeclaration() {
            return element;
        }
        public Saxon.Eej.ee.schema.ElementParticle getElementParticle() {
            return elementParticle;
        }
        public override void validate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            string localTargetNamespace = getAttributeValue(Saxon.Hej.om.NamespaceUri.NULL, "targetNamespace");
            if (((localTargetNamespace != null) && !(Saxon.Hej.om.NamespaceUri.of(localTargetNamespace).Equals(getXSDSchema().getTargetNamespace())))) {
                checkLocalTargetNamespace("element");
            }
            checkContentModel(stateTerms, stateTargets, false);
            if ((element != null)) {
                Saxon.Hej.tree.iter.AxisIterator ancIter = iterateAxis(Saxon.Hej.om.AxisInfo.ANCESTOR);
                Saxon.Hej.om.NodeInfo anc;
                while (((anc = ((/*1430*/Saxon.Hej.om.NodeInfo)ancIter.next())) != null)) {
                    if ((anc is Saxon.Eej.ee.schema.sdoc.XSDComplexType)) {
                        element.setContainingComplexType(((Saxon.Eej.ee.schema.sdoc.XSDComplexType)(anc)).getComplexType());
                        break ;
                    } else {
                        if ((anc is Saxon.Eej.ee.schema.sdoc.XSDGroup)) {
                            element.setContainingModelGroup(((Saxon.Eej.ee.schema.sdoc.XSDGroup)(anc)).getGroupDeclaration());
                            break ;
                        }
                    }
                }
            }
            Saxon.Eej.ee.schema.sdoc.XSDAlternative lastAlternative = null;
            foreach (Saxon.Hej.om.NodeInfo child in children()) {
                string childName = child.getLocalPart();
                int childfp = child.getFingerprint();
                switch (childfp) {
                  case Saxon.Hej.om.StandardNames.XS_ANNOTATION:
                    {
                        break ;
                    }
                  case Saxon.Hej.om.StandardNames.XS_COMPLEX_TYPE:
                    {
                        if (isReference) {
                            mutuallyExclusiveElementAndAttribute("complexType", "ref");
                        }
                        if (foundTypeReference) {
                            mutuallyExclusiveElementAndAttribute("complexType", "type");
                        }
                        break ;
                    }
                  case Saxon.Hej.om.StandardNames.XS_SIMPLE_TYPE:
                    {
                        if (isReference) {
                            mutuallyExclusiveElementAndAttribute("simpleType", "ref");
                        }
                        if (foundTypeReference) {
                            mutuallyExclusiveElementAndAttribute("simpleType", "type");
                        }
                        break ;
                    }
                  case Saxon.Hej.om.StandardNames.XS_ALTERNATIVE:
                    {
                        if (isReference) {
                            mutuallyExclusiveElementAndAttribute("alternative", "ref");
                        }
                        if ((compiler.getLanguageVersion() == Saxon.Hej.Configuration.XSD10)) {
                            error("Conditional Type Assignment requires XML Schema 1.1 to be enabled");
                            return ;
                        }
                        if ((lastAlternative != null)) {
                            if ((lastAlternative.getTypeAlternative().getCondition() == null)) {
                                lastAlternative.error("The test attribute is required on all alternatives except the last");
                            }
                        }
                        lastAlternative = (Saxon.Eej.ee.schema.sdoc.XSDAlternative)(child);
                        break ;
                    }
                  case Saxon.Hej.om.StandardNames.XS_KEY:
                  case Saxon.Hej.om.StandardNames.XS_KEYREF:
                  case Saxon.Hej.om.StandardNames.XS_UNIQUE:
                    {
                        if (isReference) {
                            mutuallyExclusiveElementAndAttribute(childName, "ref");
                        }
                        break ;
                    }
                  default:
                    {
                        illegalElement(child);
                        break ;
                    }
                }
            }
        }
        private readonly static int[] state0terms = {Saxon.Hej.om.StandardNames.XS_ANNOTATION, Saxon.Hej.om.StandardNames.XS_SIMPLE_TYPE, Saxon.Hej.om.StandardNames.XS_KEY, Saxon.Hej.om.StandardNames.XS_COMPLEX_TYPE, Saxon.Hej.om.StandardNames.XS_KEYREF, Saxon.Hej.om.StandardNames.XS_UNIQUE, Saxon.Hej.om.StandardNames.XS_ALTERNATIVE};
        private readonly static int[] state0targets = {1, 2, 3, 2, 3, 3, 4};
        private readonly static int[] state1terms = {Saxon.Hej.om.StandardNames.XS_SIMPLE_TYPE, Saxon.Hej.om.StandardNames.XS_KEY, Saxon.Hej.om.StandardNames.XS_COMPLEX_TYPE, Saxon.Hej.om.StandardNames.XS_KEYREF, Saxon.Hej.om.StandardNames.XS_UNIQUE, Saxon.Hej.om.StandardNames.XS_ALTERNATIVE};
        private readonly static int[] state1targets = {2, 3, 2, 3, 3, 4};
        private readonly static int[] state2terms = {Saxon.Hej.om.StandardNames.XS_KEY, Saxon.Hej.om.StandardNames.XS_KEYREF, Saxon.Hej.om.StandardNames.XS_UNIQUE, Saxon.Hej.om.StandardNames.XS_ALTERNATIVE};
        private readonly static int[] state2targets = {3, 3, 3, 4};
        private readonly static int[] state3terms = {Saxon.Hej.om.StandardNames.XS_KEY, Saxon.Hej.om.StandardNames.XS_KEYREF, Saxon.Hej.om.StandardNames.XS_UNIQUE};
        private readonly static int[] state3targets = {3, 3, 3};
        private readonly static int[] state4terms = {Saxon.Hej.om.StandardNames.XS_KEY, Saxon.Hej.om.StandardNames.XS_KEYREF, Saxon.Hej.om.StandardNames.XS_UNIQUE, Saxon.Hej.om.StandardNames.XS_ALTERNATIVE};
        private readonly static int[] state4targets = {3, 3, 3, 4};
        private readonly static int[][] stateTerms = {state0terms, state1terms, state2terms, state3terms, state4terms};
        private readonly static int[][] stateTargets = {state0targets, state1targets, state2targets, state3targets, state4targets};
        protected internal override bool isFinalState(int state) {
            return (((((state == 0) || (state == 1)) || (state == 2)) || (state == 3)) || (state == 4));
        }
        public override void postValidate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            if (isReference) {
                return ;
            }
            Saxon.Eej.ee.schema.TypeAlternative lastAlternative = null;
            foreach (Saxon.Hej.om.NodeInfo child in children()) {
                int childfp = child.getFingerprint();
                switch (childfp) {
                  case Saxon.Hej.om.StandardNames.XS_COMPLEX_TYPE:
                    {
                        Saxon.Eej.ee.schema.UserDefinedType schemaType = ((Saxon.Eej.ee.schema.sdoc.XSDComplexType)(child)).getComplexType();
                        Saxon.Eej.ee.schema.TypeReference @ref = new Saxon.Eej.ee.schema.TypeReference(schemaType.getFingerprint(), element.getConfiguration(), child);
                        @ref.setTarget(schemaType);
                        element.setTypeReference(@ref);
                        schemaType.setContainingDeclaration(elementName, true);
                        break ;
                    }
                  case Saxon.Hej.om.StandardNames.XS_SIMPLE_TYPE:
                    {
                        Saxon.Eej.ee.schema.UserDefinedType schemaType = ((Saxon.Eej.ee.schema.sdoc.XSDSimpleType)(child)).getSimpleTypeDefinition();
                        Saxon.Eej.ee.schema.TypeReference @ref = new Saxon.Eej.ee.schema.TypeReference(schemaType.getFingerprint(), element.getConfiguration(), child);
                        @ref.setTarget(schemaType);
                        element.setTypeReference(@ref);
                        schemaType.setContainingDeclaration(elementName, true);
                        ((Saxon.Eej.ee.schema.UserSimpleType)(schemaType)).setContextComponent(element);
                        break ;
                    }
                  case Saxon.Hej.om.StandardNames.XS_ALTERNATIVE:
                    lastAlternative = ((Saxon.Eej.ee.schema.sdoc.XSDAlternative)(child)).getTypeAlternative();
                    element.addTypeAlternative(lastAlternative);
                    break ;
                  case Saxon.Hej.om.StandardNames.XS_KEY:
                  case Saxon.Hej.om.StandardNames.XS_KEYREF:
                  case Saxon.Hej.om.StandardNames.XS_UNIQUE:
                    {
                        Saxon.Eej.ee.schema.IdentityConstraintReference constraint = ((Saxon.Eej.ee.schema.sdoc.XSDIdentityConstraint)(child)).getIdentityConstraint();
                        element.addIdentityConstraint(constraint);
                        break ;
                    }
                }
                if (((lastAlternative != null) && (lastAlternative.getCondition() != null))) {
                    Saxon.Eej.config.EnterpriseConfiguration config = (Saxon.Eej.config.EnterpriseConfiguration)(getConfiguration());
                    lastAlternative = new Saxon.Eej.ee.schema.TypeAlternative(config, null, null);
                    int typeRef = element.getTypeFingerprint();
                    if ((typeRef == -1)) {
                        typeRef = Saxon.Hej.om.StandardNames.XS_ANY_TYPE;
                    }
                    lastAlternative.setTypeReference(new Saxon.Eej.ee.schema.TypeReference(typeRef, config, element));
                    lastAlternative.setLocator(this);
                }
            }
        }
    
    }
}