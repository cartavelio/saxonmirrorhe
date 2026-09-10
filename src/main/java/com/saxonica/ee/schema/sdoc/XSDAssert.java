package com.saxonica.ee.schema.sdoc;

import com.saxonica.ee.schema.Assertion;
import com.saxonica.ee.stream.Posture;
import com.saxonica.ee.stream.PostureAndSweep;
import com.saxonica.ee.stream.Streamability;
import com.saxonica.ee.stream.Sweep;
import com.saxonica.ee.trans.ContextItemStaticInfoEE;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.instruct.Choose;
import net.sf.saxon.expr.instruct.GlobalParam;
import net.sf.saxon.expr.parser.ContextItemStaticInfo;
import net.sf.saxon.expr.parser.ExpressionTool;
import net.sf.saxon.expr.parser.ExpressionVisitor;
import net.sf.saxon.expr.parser.Token;
import net.sf.saxon.functions.Empty;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.om.AttributeMap;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.pattern.NodeKindTest;
import net.sf.saxon.pattern.NodeTest;
import net.sf.saxon.sxpath.XPathEvaluator;
import net.sf.saxon.sxpath.XPathExpression;
import net.sf.saxon.sxpath.XPathVariable;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.AttributeLocation;
import net.sf.saxon.type.SchemaException;
import net.sf.saxon.value.BooleanValue;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

/**
 * An xs:assert or xs:assertion element in a schema (see draft XML Schema 1.1 Part 1)
 */
public class XSDAssert extends AnnotationParent {

    /**
     * The Assertion we are constructing
     */
    private Assertion assertion = null;

    /**
     * Creates a new Assertion element
     */
    public XSDAssert() {
    }

    /**
     * Validate and process the attributes on this element in the schema document
     *
     * @throws net.sf.saxon.type.SchemaException
     *          if any errors are found in the attributes
     */

    @Override
    protected void prepareAttributes() throws SchemaException {
        if (getXSDSchema().getSchemaCompiler().getLanguageVersion() == Configuration.XSD10) {
            error("The xs:assert element requires XSD 1.1 to be enabled");
        }
        AttributeMap atts = attributes();
        String[] allowed = {"id", "test", "xpathDefaultNamespace"};
        allowAttributes(atts, allowed);

        // @xpathDefaultNamespace

        String defaultNamespace = atts.getValue(NamespaceUri.NULL, "xpathDefaultNamespace");
        if (defaultNamespace == null) {
            defaultNamespace = getXSDSchema().getXPathDefaultNamespace();
        }
        NamespaceUri defaultNamespaceUri;
        switch (defaultNamespace) {
            case "##defaultNamespace":
                defaultNamespaceUri = getURIForPrefix("", true);
                break;
            case "##targetNamespace":
                defaultNamespaceUri = getXSDSchema().getTargetNamespace();
                break;
            case "##local":
                defaultNamespaceUri = NamespaceUri.NULL;
                break;
            default:
                defaultNamespaceUri = NamespaceUri.of(defaultNamespace);
                break;
        }

        // Saxon extension: @saxon:streamable attribute
        boolean streamable = false;
        String streamableAtt = atts.getValue(NamespaceUri.SAXON, "streamable");
        if (streamableAtt != null) {
            streamable = parseBooleanAttribute("streamable", streamableAtt);
        }

        // @test

        String xpath = atts.getValue(NamespaceUri.NULL, "test");
        if (xpath == null) {
            missingAttribute("test");
            xpath = ".";  // for error recovery
        }

        Configuration config = getConfiguration();


        XPathStaticContext env = new XPathStaticContext(this);
        env.setDefaultElementNamespace(defaultNamespaceUri);
        env.setNamespaceResolver(makeNamespaceContext());
        env.setBaseURI(getBaseURI());
        env.setRequiredContextItemType(NodeKindTest.ELEMENT);
        env.setContextItemParentless(true);
        env.setContainingLocation(new AttributeLocation(this, new StructuredQName("", NamespaceUri.NULL, "test")));
        int xpathVersion = config.getConfigurationProperty(Feature.XPATH_VERSION_FOR_XSD);
        if (getXSDSchema().getAllowedExtensions().contains("any-xpath-version")) {
            xpathVersion = 31;
        }
        env.setXPathLanguageLevel(xpathVersion);

        // If in a simpleType or simpleContent, declare the variable $value
        XPathVariable valueVariable = env.declareVariable(NamespaceUri.NULL, "value");

        // Declare any variables defined in saxon:param elements
        Map<StructuredQName, XPathVariable> parameters = new HashMap<>();
        for (GlobalParam param : getXSDSchema().getGlobalParams()) {
            XPathVariable var = env.declareVariable(param.getVariableQName());
            GroundedValue defaultVal = ((Literal) param.getBody()).getGroundedValue();
            var.setDefaultValue(defaultVal);
            parameters.put(param.getVariableQName(), var);
        }

        boolean isComplex = getLocalPart().equals("assert");
        XPathExpression condition;
        boolean returnFailingNodes = isComplex && !streamable;
        XPathEvaluator xpe = returnFailingNodes ?
                new CustomizedXPath(getConfiguration()) :
                new XPathEvaluator(getConfiguration());
        try {
            xpe.setStaticContext(env);
            condition = xpe.createExpression(xpath);

        } catch (XPathException err) {
            error(err.getMessage());
            // Error recovery path
            try {
                condition = xpe.createExpression("false()");
            } catch (XPathException e2) {
                throw new AssertionError(e2.getMessage());
            }
        }

        assertion = new Assertion(getSchema().getConfiguration(), condition, valueVariable, parameters, xpath);
        assertion.setReturnsFailingNodes(returnFailingNodes);
        assertion.setStaticContext(env);

        // Saxon extension: @saxon:message attribute
        String message = atts.getValue(NamespaceUri.SAXON, "message");
        assertion.setMessage(message);

        // Saxon extension: @saxon:streamable attribute
        if (streamable) {
            if (xpath.contains("$value")) {
                warning("Assertion is not streamable because it refers to $value");
            } else {
                ContextItemStaticInfoEE contextInfo = new ContextItemStaticInfoEE(NodeKindTest.ELEMENT, false, Posture.STRIDING);
                List<String> reasons = new ArrayList<>();
                PostureAndSweep ps = Streamability.getStreamability(condition.getInternalExpression(), contextInfo, reasons);
                if (ps.getSweep() == Sweep.FREE_RANGING) {
                    StringBuilder sb = new StringBuilder("Assertion {").append(xpath).append("} is not streamable. ");
                    for (String r : reasons) {
                        sb.append(r).append(". ");
                    }
                    warning(sb.toString());
                } else {
                    assertion.setSweep(ps.getSweep());
                }
            }
        }

        //-- @id
        processId();
    }

    /**
     * Get the Assertion schema component corresponding to this xsd:assert element
     *
     * @return the Assertion schema component
     */

    /*@Nullable*/
    public Assertion getAssertion() {
        return assertion;
    }

    /**
     * Define a customized XPath evaluator which injects code into the XPath code generation;
     * this has the effect for a complex-type XSDAssert of modifying the expression so that
     * instead of returning a boolean, it returns a list of nodes that contribute to the invalidity
     * of the assertion.
     */

    public static class CustomizedXPath extends XPathEvaluator {

         public CustomizedXPath(Configuration config) {
             super(config);
         }

        /**
         * Customize the expression post-optimization. This method does nothing,
         * but can be overridden in a subclass
         *
         * @param exp     the expression after optimization
         * @param visitor an expression visitor that includes static context information
         * @param cit     information about the context item type
         */

        @Override
        protected Expression postProcess(Expression exp, ExpressionVisitor visitor, ContextItemStaticInfo cit) throws XPathException {
            Expression core = exp;
            Operand lastOp = null;
            while (core instanceof LetExpression) {
                lastOp = ((LetExpression) core).getActionOp();
                core = lastOp.getChildExpression();
            }
            if (core instanceof QuantifiedExpression &&
                    ((QuantifiedExpression) core).getOperator() == Token.EVERY) {
                QuantifiedExpression every = (QuantifiedExpression) core;
                if (every.getSequence().getItemType() instanceof NodeTest) {
                    Operand sequenceOp = every.getSequenceOp();
                    Operand satisfiesOp = every.getActionOp();

                    Expression satisfies = satisfiesOp.getChildExpression();

                    // Rewrite "satisfies X" as "satisfies (if (X) then () else $node

                    Expression[] conditions = new Expression[]{satisfies, Literal.makeLiteral(BooleanValue.TRUE)};
                    Expression[] actions = new Expression[]{Literal.makeEmptySequence(), new LocalVariableReference(every)};
                    Choose choose = new Choose(conditions, actions);
                    ForExpression forEx = new ForExpression();
                    forEx.setSequence(sequenceOp.getChildExpression());
                    forEx.setVariableQName(every.getVariableQName());
                    forEx.setAction(choose);
                    ExpressionTool.rebindVariableReferences(choose, every, forEx);
                    if (lastOp != null) {
                        lastOp.setChildExpression(forEx);
                        return exp.optimize(visitor, cit);
                    } else {
                        return forEx.optimize(visitor, cit);
                    }
                }
            } else if (core.isCallOn(Empty.class)) {
                Operand op = ((SystemFunctionCall)core).getOperanda().getOperand(0);
                if (op.getChildExpression().getItemType() instanceof NodeTest) {
                    // Rewrite "empty(X)" as "X"
                    if (lastOp != null) {
                        lastOp.setChildExpression(op.getChildExpression());
                        return exp;
                    } else {
                        return op.getChildExpression();
                    }
                }
            }
            // replace the boolean assertion A by the filter expression .[not(A)] to return the
            // invalid node if the assertion is false
            Expression notExp = SystemFunction.makeCall("not", exp.getRetainedStaticContext(), exp);
            return new FilterExpression(new ContextItemExpression(), notExp);
        }
    }



}

// Copyright (c) 2007-2022, Saxonica Limited
