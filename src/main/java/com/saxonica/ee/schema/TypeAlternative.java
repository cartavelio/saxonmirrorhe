package com.saxonica.ee.schema;

import com.saxonica.config.EnterpriseConfiguration;
import net.sf.saxon.expr.CallableDelegate;
import net.sf.saxon.functions.CallableFunction;
import net.sf.saxon.om.Function;
import net.sf.saxon.om.NamespaceResolver;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.sxpath.XPathExpression;
import net.sf.saxon.sxpath.XPathVariable;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.*;
import net.sf.saxon.value.EmptySequence;
import net.sf.saxon.value.ObjectValue;
import net.sf.saxon.value.StringValue;

import java.util.Map;

import static net.sf.saxon.type.SchemaValidationStatus.INVALID;

/**
 * A TypeAlternative schema component (See XML Schema 1.1 Part 1 Working Draft)
 */
public class TypeAlternative extends SchemaStructure implements SchemaComponent {

    private final XPathExpression condition;
    private final String conditionText;
    private NamespaceResolver namespaceContext;
    private String baseURI;
    private String defaultNamespace;
    private Map<StructuredQName, XPathVariable> declaredParams;
    private TypeReference typeReference;

    /**
     * Create an TypeAlternative component
     *
     * @param config    the Saxon configuration
     * @param condition the compiled XPath expression. May be null if no condition was specified (this
     *                  is allowed on the last alternative)
     * @param text      the original text of the XPath expression. May be null if no condition was specified (this
     *                  is allowed on the last alternative)
     */

    public TypeAlternative(EnterpriseConfiguration config, XPathExpression condition, String text) {
        setConfiguration(config);
        this.condition = condition;
        this.conditionText = text;
    }

    /**
     * Get the XPath expression used in the test attribute of the alternative. Null represents an absent condition,
     * equivalent to "true()".
     *
     * @return the XPath expression
     */

    public XPathExpression getCondition() {
        return condition;
    }

    /**
     * Get the original text of the condition.
     *
     * @return the original text of the XPath expression. May be null if no condition was specified
     */

    public String getConditionText() {
        return conditionText;
    }

    /**
     * Set the default namesapce for elements and types from the static context of the XPath expression
     *
     * @param namespace the default namespace for elements and types. The null namespace is represented as ""
     */

    public void setDefaultElementNamespace(String namespace) {
        defaultNamespace = namespace;
    }

    /**
     * Get the default namesapce for elements and types from the static context of the XPath expression
     *
     * @return the default namespace for elements and types. The null namespace is represented as ""
     */

    public String getDefaultElementNamespace() {
        return defaultNamespace;
    }


    /**
     * Set the namespace context. This is needed to allow the assertion to be serialized, so that the
     * namespace context can be replicated when it is reloaded.
     *
     * @param resolver defines the namespace context
     */

    public void setNamespaceContext(NamespaceResolver resolver) {
        this.namespaceContext = resolver;
    }

    /**
     * Get the namespace context for evaluating the XPath expression
     *
     * @return a NamespaceResolver representing the namespace context of the expression
     */

    public NamespaceResolver getNamespaceContext() {
        return namespaceContext;
    }

    /**
     * Set the base URI of the XPath expression. This is needed to allow the assertion
     * to be serialized.
     *
     * @param baseURI the static base URI of the expression
     */

    public void setBaseURI(String baseURI) {
        this.baseURI = baseURI;
    }

    /**
     * Get the base URI of the XPath expression
     *
     * @return the static base URI of the expression
     */

    public String getBaseURI() {
        return baseURI;
    }

    /**
     * Set the type reference for this alternative
     *
     * @param type a reference to the type selected by this alternative
     */

    public void setTypeReference(TypeReference type) {
        typeReference = type;
    }

    /**
     * Get the type reference for this alternative
     *
     * @return the type reference
     */

    public TypeReference getTypeReference() {
        return typeReference;
    }

    /**
     * Set the parameters declared for this schema document using the saxon:param extension element
     *
     * @param parameters the declared saxon:param parameters in scope for the XPath expression
     */
    public void setParameters(Map<StructuredQName, XPathVariable> parameters) {
        declaredParams = parameters;
    }

    /**
     * Get the parameters declared for this schema document using the saxon:param extension element
     *
     * @return the declared saxon:param parameters in scope for the XPath expression
     */
    public Map<StructuredQName, XPathVariable> getParameters() {
        return declaredParams;
    }

    /**
     * Get the fixed-up schema type for this alternative
     *
     * @return the schema type
     */

    public SchemaType getSchemaType() throws MissingComponentException {
        return (SchemaType) typeReference.getTarget();
    }

    /**
     * Fixup references from this schema component to others
     *
     * @param compiler the SchemaCompiler, used primarily for reporting errors
     * @return true if all references were fixed up successfully, false if dangling references were found
     */

    @Override
    public boolean fixup(SchemaCompiler compiler) throws SchemaException {
        SchemaType type = (SchemaType) PreparedSchema.validateReference(typeReference, compiler, false);
        if (type == null) {
            setFixupStatus(INVALID);
            return false;
        }
        return !(type instanceof UserSchemaComponent) ||
                ((UserSchemaComponent) type).fixup(compiler);
    }

    /**
     * Serialize the schema component
     *
     * @param serializer the object responsible for serialization
     * @throws net.sf.saxon.trans.XPathException
     *          if a serialization error occur
     */

    public void serialize(SchemaModelSerializer serializer) throws XPathException {
        serializer.startElement("alternativeType");
        serializer.emitNamespaceContext(getNamespaceContext());
        serializer.emitAttribute("test", conditionText == null ? "true()" : conditionText);
        serializer.emitAttribute("type", serializer.getTypeLink((SchemaType) typeReference.getTarget()));
        serializer.emitAttribute("defaultNamespace", getDefaultElementNamespace());
        serializer.emitAttribute("xml:base", getBaseURI());
        serializer.endElement();
    }

    @Override
    public Function getComponentAsFunction() {
        TypeAlternative thisTypeAlternative = this;
        CallableDelegate callable = new CallableDelegate((context, arguments) -> {
            String key = arguments[0].head().getStringValue();
            switch (key) {
                case "class":
                    return StringValue.bmp("Type Alternative");
                case "implementation":
                    return new ObjectValue<>(thisTypeAlternative);
                case "test":
                    return SchemaStructure.makeXPathExpressionPropertyRecord(conditionText, namespaceContext, baseURI);
                case "type definition":
                    return getTypeReference().getTarget().getComponentAsFunction();
                default:
                    return EmptySequence.getInstance();
            }
        });
        return new CallableFunction(1, callable, SpecificFunctionType.COMPONENT_FUNCTION_TYPE);
    }
}

// Copyright (c) 2007-2022 Saxonica Limited
