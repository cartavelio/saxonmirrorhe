package com.saxonica.ee.schema;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.ee.stream.Sweep;
import com.saxonica.ee.validate.ValidationContext;
import net.sf.saxon.Configuration;
import net.sf.saxon.Controller;
import net.sf.saxon.expr.CallableDelegate;
import net.sf.saxon.expr.parser.ExpressionTool;
import net.sf.saxon.functions.CallableFunction;
import net.sf.saxon.functions.CollectionFn;
import net.sf.saxon.lib.ConversionRules;
import net.sf.saxon.lib.ResourceResolverDelegate;
import net.sf.saxon.om.*;
import net.sf.saxon.sxpath.XPathDynamicContext;
import net.sf.saxon.sxpath.XPathExpression;
import net.sf.saxon.sxpath.XPathStaticContext;
import net.sf.saxon.sxpath.XPathVariable;
import net.sf.saxon.trans.UncheckedXPathException;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.trans.XmlProcessingException;
import net.sf.saxon.type.SchemaComponent;
import net.sf.saxon.type.SpecificFunctionType;
import net.sf.saxon.type.ValidationParams;
import net.sf.saxon.value.EmptySequence;
import net.sf.saxon.value.ObjectValue;
import net.sf.saxon.value.StringValue;

import java.util.ArrayList;
import java.util.List;
import java.util.Map;

/**
 * An Assertion schema component (See XML Schema 1.1 Part 1)
 */
public class Assertion extends SchemaStructure implements SchemaComponent, SerializableSchemaComponent {

    private XPathExpression condition;
    private XPathVariable valueVariable;
    private Map<StructuredQName, XPathVariable> declaredParams;
    private String conditionText;
    private String message;
    private XPathStaticContext staticContext;
    private Sweep sweep = Sweep.FREE_RANGING;
    private boolean returnsFailingNodes;

    public Assertion() {}

    /**
     * Create an Assertion component
     *
     * @param config        the Saxon configuration
     * @param condition     the compiled XPath expression
     * @param valueVariable the variable to hold $value (may be null)
     * @param params        parameters declared using saxon:param and used in this expression
     * @param text          the original text of the XPath expression
     */

    public Assertion(EnterpriseConfiguration config, XPathExpression condition,
                     XPathVariable valueVariable, Map<StructuredQName, XPathVariable> params, String text) {
        setConfiguration(config);
        this.condition = condition;
        this.valueVariable = valueVariable;
        this.declaredParams = params;
        this.conditionText = text;
    }

    /**
     * Get the XPath expression representing the assertion. If xs:report is used, this will be the negation
     * of the condition in the source schema.
     *
     * @return the XPath expression
     */

    public XPathExpression getCondition() {
        return condition;
    }

    /**
     * Get the variable bound to the name $value
     *
     * @return the variable whose name is $value (may be null)
     */

    public XPathVariable getValueVariable() {
        return valueVariable;
    }

    /**
     * Get the original text of the condition.
     *
     * @return the original text of the XPath expression
     */

    public String getConditionText() {
        return conditionText;
    }

    /**
     * Set the user-defined error message associated with the assertion
     *
     * @param message the message defined using the saxon:message attribute, if present
     */

    public void setMessage(String message) {
        this.message = message;
    }

    /**
     * Get the user-defined error message associated with the assertion
     *
     * @return the user-defined error message, if any; otherwise null
     */

    public String getMessage() {
        return message;
    }

    /**
     * Set the sweep of the expression; if motionless or consuming, the validator
     * may attempt streamed evaluation.
     * @param sweep the sweep of the expression (indicating whether it is capable of streamed
     *              evaluation)
     */

    public void setSweep(Sweep sweep) {
        this.sweep = sweep;
    }

    /**
     * Get the sweep of the expression; if motionless or consuming, the validator
     * may attempt streamed evaluation.
     * @return the sweep of the expression (indicating whether it is capable of streamed
     *              evaluation)
     */

    public Sweep getSweep() {
        return sweep;
    }

    /**
     * Say that the expression should return the nodes that cause the expression to be
     * invalid, rather than returning a true/false result
     * @param failingNodes set to true if the result is a set of nodes that cause the assertion to fail
     */

    public void setReturnsFailingNodes(boolean failingNodes) {
        this.returnsFailingNodes = failingNodes;
    }

    /**
     * Set the static context for the expression.
     * Note: this is needed only to support exporting of the schema component model
     *
     * @param staticContext the static context
     */

    public void setStaticContext(XPathStaticContext staticContext) {
        this.staticContext = staticContext;
    }

    /**
     * Get the static context for the expression
     *
     * @return the static context
     */

    public XPathStaticContext getStaticContext() {
        return staticContext;
    }

    /**
     * Evaluate the assertion
     *
     * @param value       the simple value to be tested, if any. If present, and if the expression
     *                    uses $value, this value will be bound to the variable $value for evaluating the assertion.
     * @param rules       used to provide context information
     *                    including the current error listener, URIResolver, and document pool, current date/time, etc.
     *                    To take advantage of validation parameters (saxon:param) this must be a ValidationContext; but
     *                    on other paths such as casting it is not possible to supply parameters, and the defaults are used
     * @return null if the assertion succeeds; otherwise a list of offending items which will be reported
     *         in any error message. Returning an empty list indicates that the assertion has failed but there
     *         are no specific offending items to be reported.
     */

    /*@Nullable*/
    public boolean testSimple(AtomicSequence value, ConversionRules rules) {
        Configuration config;
        Controller controller;
        ValidationParams actualParams;
        XPathDynamicContext context;
        if (rules instanceof ValidationContext) {
            try {
                controller = ((ValidationContext) rules).getController();
                config = controller.getConfiguration();
                actualParams = ((ValidationContext) rules).getValidationParams();
                context = condition.createDynamicContext(controller, null);
                //setDocumentsAndCollections(config, context);

                // Set the values of any saxon:param validation parameters
                ValidationParams.setValidationParams(declaredParams, actualParams, context);

            } catch (XPathException err) {
                // Indicates a configuration problem
                throw new IllegalStateException(err.getMessage());
            }
        } else {
            context = condition.createDynamicContext();
            controller = context.getXPathContextObject().getController();
            config = staticContext.getConfiguration();
        }

        setDocumentsAndCollections(config, context);

        try {
            if (getValueVariable() != null) {
                context.setVariable(getValueVariable(), value == null ? EmptySequence.getInstance() : value);
            }

            return ExpressionTool.effectiveBooleanValue(condition.iterate(context));
        } catch (XPathException err) {
            // Treat an error in evaluating the assertion as false
            context.getErrorReporter().report(new XmlProcessingException(err).asWarning());
            return false;
        }
    }

    /**
     * Evaluate a complex type assertion
     *
     * @param contextItem the item to be used as the context item in evaluating the condition
     * @param value       for a complex type with simple content, the typed value of the simple content,
     *                    as a sequence of atomic values.
     * @param rules       used to provide context information
     *                    including the current error listener, URIResolver, and document pool, current date/time, etc.
     *                    To take advantage of validation parameters (saxon:param) this must be a ValidationContext; but
     *                    on other paths such as casting it is not possible to supply parameters, and the defaults are used
     * @return an empty sequence if the assertion succeeds; otherwise a list of offending nodes which will be reported
     * in any error message.
     */

    /*@Nullable*/
    public List<NodeInfo> testComplex(NodeInfo contextItem, AtomicSequence value, ValidationContext rules) {
        Configuration config;
        Controller controller;
        ValidationParams actualParams;
        XPathDynamicContext context;
        try {
            controller = rules.getController();
            config = controller.getConfiguration();
            actualParams = rules.getValidationParams();
            context = condition.createDynamicContext(controller, contextItem);
            //setDocumentsAndCollections(config, context);

            // Set the values of any saxon:param validation parameters
            ValidationParams.setValidationParams(declaredParams, actualParams, context);

            if (getValueVariable() != null) {
                context.setVariable(getValueVariable(), value == null ? EmptySequence.getInstance() : value);
            }

        } catch (XPathException err) {
            // Indicates a configuration problem
            throw new IllegalStateException(err.getMessage());
        }

        setDocumentsAndCollections(config, context);

        List<NodeInfo> result = new ArrayList<>();
        try {
            if (returnsFailingNodes) {
                SequenceTool.supply(condition.iterate(context), (ItemConsumer<? super Item>) item -> result.add((NodeInfo) item));
                return result;
            } else {
                boolean ok = condition.effectiveBooleanValue(context);
                if (!ok) {
                    result.add(contextItem);
                }
            }
        } catch (UncheckedXPathException err) {
            // Treat an error in evaluating the assertion as false
            context.getErrorReporter().report(new XmlProcessingException(err.getXPathException()).asWarning());
            result.add(contextItem);
        } catch (XPathException err) {
            // Treat an error in evaluating the assertion as false
            context.getErrorReporter().report(new XmlProcessingException(err).asWarning());
            result.add(contextItem);
        }
        return result;
    }


    /**
     * Set the dynamic XPath context to respond appropriately to requests for documents and collections
     * @param config the Saxon configuration
     * @param context the dynamic context for evaluation of XPath expressions, which will be modified
     *                by this request.
     */

    public static void setDocumentsAndCollections(final Configuration config, XPathDynamicContext context) {
        context.setResourceResolver(new ResourceResolverDelegate(request -> {
                throw new UncheckedXPathException(
                        new XPathException("Use of doc() function within an assertion is not allowed", "FODC0002"));
        }));
        context.getXPathContextObject().getController().setDefaultCollection(CollectionFn.EMPTY_COLLECTION_URI);
        context.setCollectionFinder((context1, collectionURI) -> {
            if (collectionURI.equals(CollectionFn.EMPTY_COLLECTION_URI)) {
                return CollectionFn.EMPTY_COLLECTION;
            } else {
                throw new XPathException("No collections are available when processing assertions");
            }
        });
    }

    /**
     * Hashcode is used when deciding whether two assertions are equal, as part of the type
     * subsumption algorithm. Assertions are equal if their XPath expressions are equal.
     */

    public int hashCode() {
        return condition.getInternalExpression().hashCode();
    }

    /**
     * equals() is used when deciding whether two assertions are equal, as part of the type
     * subsumption algorithm. Assertions are equal if their XPath expressions are equal.
     */

    public boolean equals(Object other) {
        return other instanceof Assertion &&
                condition.getInternalExpression().isEqual(((Assertion) other).condition.getInternalExpression());
    }

    /**
     * Serialize the schema component
     */

    @Override
    public void serialize(SchemaModelSerializer serializer) throws XPathException {
        serializer.startElement("assertion");
        serializer.emitNamespaceContext(staticContext.getNamespaceResolver());
        serializer.emitAttribute("test", getConditionText());
        serializer.emitAttribute("defaultNamespace", staticContext.getDefaultElementNamespace().toString());
        serializer.emitAttribute("xml:base", staticContext.getStaticBaseURI());
        if (sweep != Sweep.FREE_RANGING) {
            serializer.emitAttribute("sweep", sweep.toString());
        }
        serializer.endElement();
    }

    @Override
    public Function getComponentAsFunction() {
        Assertion thisAssertion = this;
        CallableDelegate callable = new CallableDelegate((context, arguments) -> {
            String key = arguments[0].head().getStringValue();
            switch (key) {
                case "class":
                    return StringValue.bmp("Assertion");
                case "implementation":
                    return new ObjectValue<>(thisAssertion);
                case "test":
                    return SchemaStructure.makeXPathExpressionPropertyRecord(
                            conditionText, staticContext.getNamespaceResolver(), staticContext.getStaticBaseURI());
                default:
                    return EmptySequence.getInstance();
            }
        });
        return new CallableFunction(1, callable, SpecificFunctionType.COMPONENT_FUNCTION_TYPE);
    }
}

// Copyright (c) 2007-2022 Saxonica Limited