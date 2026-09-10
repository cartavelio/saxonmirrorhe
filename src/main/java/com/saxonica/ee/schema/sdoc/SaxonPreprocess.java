package com.saxonica.ee.schema.sdoc;

import com.saxonica.ee.schema.Assertion;
import net.sf.saxon.om.AttributeMap;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.sxpath.XPathEvaluator;
import net.sf.saxon.sxpath.XPathExpression;
import net.sf.saxon.sxpath.XPathVariable;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.SchemaException;

/**
 * saxon:preprocess is an implementation-defined pre-lexical facet as permitted by XML Schema 1.1. It allows
 * the lexical value as written in the source document to be transformed prior to validation against the
 * constraining facets.
 * <p>The effect of saxon:preprocess is that the lexical space for a type derived by restriction is no longer
 * constrained to be a subset of the lexical space for the type that it restricts. For example, a type derived
 * from xs:decimal can use <code>&lt;saxon:preprocess action="translate($value, ',', '.')"&gt;</code> to convert the input
 * value "3,14159" to "3.14159", which then validates as an <code>xs:decimal</code>.</p>
 * <p>More specifically, the saxon:preprocess facet converts the value from the lexical space of a restricted
 * type to the lexical space of its base type. This means that preprocessing may be defined at multiple levels
 * of the type hierarchy; the preprocessing will be performed on the most specific type first.</p>
 * <p>Note that the pattern facet, if present, applies to the value output by the preprocess facet, not to its input.</p>
 */
public class SaxonPreprocess extends AnnotationParent {

    /**
     * The Preprocessing expression, represented by an Assertion object
     */
    private Assertion action = null;

    /**
     * The reverse (or postprocessing) expression
     */

    private Assertion reverseAction = null;

    /**
     * Creates a new SaxonPreprocess element
     */
    public SaxonPreprocess() {
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
        String[] allowed = {"action", "id", "reverse", "xpathDefaultNamespace"};
        allowAttributes(atts, allowed);

        // @xpathDefaultNamespace

        String defaultNamespace = atts.getValue(NamespaceUri.NULL, "xpathDefaultNamespace");
        if (defaultNamespace == null) {
            defaultNamespace = getXSDSchema().getXPathDefaultNamespace();
        }
        NamespaceUri defaultNamespaceUri;
        if (defaultNamespace.equals("##defaultNamespace")) {
            defaultNamespaceUri = getURIForPrefix("", true);
        } else if (defaultNamespace.equals("##targetNamespace")) {
            defaultNamespaceUri = getXSDSchema().getTargetNamespace();
        } else if (defaultNamespace.equals("##local")) {
            defaultNamespaceUri = NamespaceUri.NULL;
        } else {
            defaultNamespaceUri = NamespaceUri.of(defaultNamespace);
        }

        // @action

        String xpath = atts.getValue(NamespaceUri.NULL, "action");
        if (xpath == null) {
            missingAttribute("action");
            xpath = ".";  // for error recovery
        }

        XPathStaticContext env = new XPathStaticContext(this);
        env.setDefaultElementNamespace(defaultNamespaceUri);
        env.setNamespaceResolver(makeNamespaceContext());
        env.setBaseURI(getBaseURI());

        // Declare the variable $value
        XPathVariable valueVariable = env.declareVariable(NamespaceUri.NULL, "value");
        XPathExpression expression;
        XPathEvaluator xpe = new XPathEvaluator(getSchema().getConfiguration());
        try {
            xpe.setStaticContext(env);
            expression = xpe.createExpression(xpath);

        } catch (XPathException err) {
            error(err.getMessage());
            // Error recovery path
            try {
                expression = xpe.createExpression("'*error*'");
            } catch (XPathException e2) {
                throw new AssertionError(e2.getMessage());
            }
        }

        action = new Assertion(getSchema().getConfiguration(), expression, valueVariable, null, xpath);
        action.setStaticContext(env);

        // @reverse

        xpath = atts.getValue(NamespaceUri.NULL, "reverse");
        if (xpath != null) {

//            XPathStaticContext env = new XPathStaticContext(getConfiguration());
//            env.setDefaultElementNamespace(defaultNamespace);
//            env.setNamespaceResolver(makeNamespaceContext());
//            env.setBaseURI(getBaseURI());

            // Declare the variable $value
//            XPathVariable valueVariable = env.declareVariable("", "value");
//            XPathExpression expression;
//            XPathEvaluator xpe = new XPathEvaluator(getSchema().getConfiguration());
            try {
//                xpe.setStaticContext(env);
                expression = xpe.createExpression(xpath);

            } catch (XPathException err) {
                error(err.getMessage());
                // Error recovery path
                try {
                    expression = xpe.createExpression("'*error*'");
                } catch (XPathException e2) {
                    throw new AssertionError(e2.getMessage());
                }
            }

            reverseAction = new Assertion(getSchema().getConfiguration(), expression, valueVariable, null, xpath);
            reverseAction.setStaticContext(env);
        }


        //-- @id
        processId();
    }

    /**
     * Get the Assertion schema component corresponding to the action in this saxon:preprocess element
     *
     * @return the Assertion schema component
     */

    public Assertion getAction() {
        return action;
    }

    /**
     * Get the Assertion schema component corresponding to the reverse action in this saxon:preprocess element
     *
     * @return the Assertion schema component
     */

    /*@Nullable*/
    public Assertion getReverseAction() {
        return reverseAction;
    }

}

// Copyright (c) 2008-2023 Saxonica Limited
