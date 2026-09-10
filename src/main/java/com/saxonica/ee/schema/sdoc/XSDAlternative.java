package com.saxonica.ee.schema.sdoc;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.ee.schema.SchemaCompiler;
import com.saxonica.ee.schema.TypeAlternative;
import com.saxonica.ee.schema.TypeReference;
import com.saxonica.ee.schema.UserDefinedType;
import net.sf.saxon.expr.Literal;
import net.sf.saxon.expr.instruct.GlobalParam;
import net.sf.saxon.om.*;
import net.sf.saxon.sxpath.XPathEvaluator;
import net.sf.saxon.sxpath.XPathExpression;
import net.sf.saxon.sxpath.XPathVariable;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.SchemaException;

import java.util.HashMap;
import java.util.Map;

/**
 * An xs:alternative element in a schema (see draft XML Schema 1.1 Part 1)
 */
public class XSDAlternative extends AnnotationParent {

    /**
     * The TypeAlternative we are constructing
     */
    private TypeAlternative alternative = null;

    /**
     * Create a new TypeAlternative element
     */
    public XSDAlternative() {
    }

    /**
     * Validate and process the attributes on this element in the schema document
     *
     * @throws net.sf.saxon.type.SchemaException
     *          if any errors are found in the attributes
     */

    @Override
    protected void prepareAttributes() throws SchemaException {
        AttributeMap atts = attributes();
        String[] allowed = {"id", "test", "type", "xpathDefaultNamespace"};
        allowAttributes(atts, allowed);

        // @xpathDefaultNamespace

        String defaultNamespace = atts.getValue(NamespaceUri.NULL, "xpathDefaultNamespace");
        if (defaultNamespace == null) {
            defaultNamespace = getXSDSchemaElement().getXPathDefaultNamespace();
        }
        NamespaceUri defaultNamespaceUri;
        if (defaultNamespace.equals("##defaultNamespace")) {
            defaultNamespaceUri = getURIForPrefix("", true);
        } else if (defaultNamespace.equals("##targetNamespace")) {
            defaultNamespaceUri = getXSDSchemaElement().getTargetNamespace();
        } else if (defaultNamespace.equals("##local")) {
            defaultNamespaceUri = NamespaceUri.NULL;
        } else {
            defaultNamespaceUri = NamespaceUri.of(defaultNamespace);
        }

        // @test

        String xpath = atts.getValue(NamespaceUri.NULL, "test");
        // Note, the attribute can be omitted on the last element

        XPathExpression condition = null;
        Map<StructuredQName, XPathVariable> parameters = new HashMap<StructuredQName, XPathVariable>();
        if (xpath != null) {
            SchemaElement.XPathStaticContext env = new SchemaElement.XPathStaticContext(this);
            env.setDefaultElementNamespace(defaultNamespaceUri);
            env.setBaseURI(getBaseURI());
            env.setXPathLanguageLevel(getXSDSchemaElement().getAllowedExtensions().contains("any-xpath-version") ? 31 : 20);

            // Declare any variables defined in saxon:param elements
            for (GlobalParam param : getXSDSchemaElement().getGlobalParams()) {
                XPathVariable var = env.declareVariable(param.getVariableQName());
                GroundedValue defaultVal = ((Literal) param.getBody()).getGroundedValue();
                var.setDefaultValue(defaultVal);
                parameters.put(param.getVariableQName(), var);
            }

            XPathEvaluator xpe = new XPathEvaluator(getSchema().getConfiguration());
            try {
                xpe.setStaticContext(env);
                condition = xpe.createExpression(xpath);

            } catch (XPathException err) {
                error(err.getMessage());
                try {
                    condition = xpe.createExpression("false()");
                } catch (XPathException e) {
                    // can't happen
                }
            }
        }

        EnterpriseConfiguration config = getSchema().getConfiguration();
        alternative = new TypeAlternative(config, condition, xpath);
        alternative.setLocator(this);
        alternative.setNamespaceContext(makeNamespaceContext());
        alternative.setDefaultElementNamespace(defaultNamespaceUri);
        alternative.setBaseURI(getBaseURI());
        alternative.setParameters(parameters);

        // @type
        String attValue = atts.getValue(NamespaceUri.NULL, "type");
        if (attValue != null) {
            int fp;
            try {
                fp = getFingerprint(attValue, USE_DEFAULT_NS);
                if (fp == StandardNames.XS_ANY_ATOMIC_TYPE ||
                        fp == StandardNames.XS_UNTYPED) {
                    error("Type " + attValue + " cannot be used as an element type");
                }
                alternative.setTypeReference(new TypeReference(fp, config, this));
            } catch (SchemaException err) {
                error(err.getMessage());
            }
        }

        // @id
        processId();
    }


    @Override
    public void validate(SchemaCompiler compiler) throws SchemaException {
        checkContentModel(stateTerms, stateTargets, false);
        for (NodeInfo child : children()) {
            int childfp = child.getFingerprint();
            switch (childfp) {
                case StandardNames.XS_ANNOTATION: {
                    break;
                }

                case StandardNames.XS_COMPLEX_TYPE: {
                    if (alternative.getTypeReference() != null) {
                        mutuallyExclusiveElementAndAttribute("complexType", "type");
                    }
                    break;
                }
                case StandardNames.XS_SIMPLE_TYPE: {
                    if (alternative.getTypeReference() != null) {
                        mutuallyExclusiveElementAndAttribute("simpleType", "type");
                    }
                    break;
                }
            }
        }
    }

    // This content model was generated from the schema component model using
    // Transform -s:s4s11b.scm generate-s4s-code.xsl type=altType

    private final static int[] state0terms = {StandardNames.XS_ANNOTATION, StandardNames.XS_SIMPLE_TYPE, StandardNames.XS_COMPLEX_TYPE};
    private final static int[] state0targets = {1, 2, 2};

    private final static int[] state1terms = {StandardNames.XS_SIMPLE_TYPE, StandardNames.XS_COMPLEX_TYPE};
    private final static int[] state1targets = {2, 2};

    private final static int[] state2terms = {};
    private final static int[] state2targets = {};

    private final static int[][] stateTerms = {
            state0terms, state1terms, state2terms};
    private final static int[][] stateTargets = {
            state0targets, state1targets, state2targets};

    @Override
    protected boolean isFinalState(int state) {
        return state == 0 || state == 1 || state == 2;
    }

    /**
     * Additional processing after all the children of this element have been validated.
     *
     * @param compiler used primarily for reporting errors
     */

    @Override
    public void postValidate(SchemaCompiler compiler) throws SchemaException {

        EnterpriseConfiguration config = (EnterpriseConfiguration) getConfiguration();
        for (NodeInfo child : children()) {
            int childfp = child.getFingerprint();
            switch (childfp) {
                case StandardNames.XS_COMPLEX_TYPE: {
                    UserDefinedType schemaType = ((XSDComplexType) child).getComplexType();
                    TypeReference ref = new TypeReference(schemaType.getFingerprint(), config, child);
                    ref.setTarget(schemaType);
                    alternative.setTypeReference(ref);
                    schemaType.setContainingDeclaration(((XSDElement) getParent()).getElementDeclaration().getComponentName(), true);
                    break;
                }
                case StandardNames.XS_SIMPLE_TYPE: {
                    UserDefinedType schemaType = ((XSDSimpleType) child).getSimpleTypeDefinition();
                    TypeReference ref = new TypeReference(schemaType.getFingerprint(), config, child);
                    ref.setTarget(schemaType);
                    alternative.setTypeReference(ref);
                    schemaType.setContainingDeclaration(((XSDElement) getParent()).getElementDeclaration().getComponentName(), true);
                    break;
                }
            }
        }

    }


    /**
     * Get the TypeAlternative created by this xs:alternative element
     *
     * @return the corresponding TypeAlternative
     */

    /*@Nullable*/
    public TypeAlternative getTypeAlternative() {
        return alternative;
    }

}

// Copyright (c) 2007-2023 Saxonica Limited