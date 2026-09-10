package com.saxonica.ee.schema;

import net.sf.saxon.expr.Expression;
import net.sf.saxon.om.NamespaceResolver;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.pattern.Pattern;

/**
 * The selector of an IdentityConstraint
 */

public abstract class FieldOrSelector extends SchemaStructure {

    /**
     * Source XPath expression
     */
    private String xpath;

    /**
     * Compiled XPath expression
     */

    protected Expression selectExpression;

    /**
     * Compiled XPath expression as a match pattern
     */

    private Pattern selection;

    /**
     * Saved XPath context: needed when serializing the schema component model
     */

    private NamespaceResolver namespaceContext;

    /**
     * Saved XPath default namespace: needed when serializing
     */

    private NamespaceUri xpathDefaultNamespace;

    /**
     * Return the source XPath expression of this IdentitySelector.
     *
     * @return the XPath expression defined in this IdentitySelector.
     */
    public String getXPath() {
        return xpath;
    }

    /**
     * Return the compiled XPath expression as a match pattern
     *
     * @return the expression as a Pattern object
     */

    public Pattern getSelection() {
        return selection;
    }

    /**
     * Get the compiled XPath expression
     * @return the compiled XPath expression
     */

    public Expression getSelectExpression() {
        return selectExpression;
    }

    /**
     * Sets the XPath expression for this Selector.
     *
     * @param xpath the XPath expression for this IdentitySelector. Must not
     *              be null.
     */
    public void setXPath(String xpath)  {
        this.xpath = xpath;
    }

    /**
     * Set the compiled XPath expression as a match pattern
     *
     * @param selection the XPath expression compiled into a Pattern object
     */

    public void setSelection(Pattern selection) {
        this.selection = selection;
    }

    /**
     * Set the compiled XPath expression
     *
     * @param selectExpression the XPath expression
     */


    public void setSelectExpression(Expression selectExpression) {
        this.selectExpression = selectExpression;
    }

    /**
     * Set the namespace context. This is needed to allow the assertion to be serialized, so that the
     * namespace context can be replicated when it is reloaded.
     *
     * @param resolver the resolver used to resolve prefixes in the XPath expression to URIs
     */

    public void setNamespaceContext(NamespaceResolver resolver) {
        this.namespaceContext = resolver;
    }

    /**
     * Get the namespace context for evaluating the XPath expression
     *
     * @return the namespace resolver used to resolve namespace prefixes in the XPath expression
     */

    public NamespaceResolver getNamespaceContext() {
        return namespaceContext;
    }

    /**
     * Set the XPath default namespace
     *
     * @param uri the XPath default namespace
     */

    public void setXPathDefaultNamespace(NamespaceUri uri) {
        xpathDefaultNamespace = uri;
    }

    /**
     * Get the XPath default namespace
     *
     * @return the XPath default namespace
     */

    public NamespaceUri getXPathDefaultNamespace() {
        return xpathDefaultNamespace;
    }


}

// Copyright (c) 2004-2022 Saxonica Limited