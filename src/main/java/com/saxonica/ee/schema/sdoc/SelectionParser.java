package com.saxonica.ee.schema.sdoc;

import com.saxonica.ee.stream.StreamingPatternMaker;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.parser.*;
import net.sf.saxon.om.AxisInfo;
import net.sf.saxon.om.QNameParser;
import net.sf.saxon.pattern.AnyNodeTest;
import net.sf.saxon.pattern.NodeKindTest;
import net.sf.saxon.pattern.NodeTest;
import net.sf.saxon.pattern.Pattern;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.Type;

import java.util.ArrayList;
import java.util.List;

/**
 * This class parses the two XPath subsets defined in XML Schema for handling
 * the "selector" and "field" attributes of the &lt;unique&gt;, &lt;key&gt;, and &lt;keyref&gt;
 * elements.
 */
public class SelectionParser extends XPathParser {

    boolean isFieldSubset;
    Expression optimizedExpression;

    public SelectionParser(StaticContext env) {
        super(env);
    }

    /**
     * Parse a string representing the restricted XPath expression allowed
     * in an XML Schema selector or field attribute
     *
     * @param expression the expression expressed as a String
     * @param env        the static context for the expression
     * @param isField    true if the attribute is a "field" attribute, false if
     *                   it is a "selector" attribute
     * @return a Pattern object representing the result of parsing. The expression
     * equivalent to this pattern may be obtained by calling {@link #getOptimizedExpression()}.
     * @throws net.sf.saxon.trans.XPathException
     *          if the expression contains a syntax error
     */

    public Pattern parseSelector(String expression,
                                 StaticContext env,
                                 boolean isField) throws XPathException {
        //System.err.println("Parse expression: " + expression);

        this.env = env;
        this.charChecker = env.getConfiguration().getValidCharacterChecker();
        this.isFieldSubset = isField;
        t = new Tokenizer();
        t.languageLevel = 20;
        QNameParser qp = new QNameParser(env.getNamespaceResolver());
        setQNameParser(qp);
        try {
            t.tokenize(expression, 0, -1);
        } catch (XPathException err) {
            grumble(err.getMessage());
        }
        Expression exp = parseUnionExpression();
        if (t.currentToken != Token.EOF) {
            grumble("Unexpected token " + currentTokenDisplay() + " beyond end of expression");
        }
        exp.setLocation(makeLocation());
        exp.setRetainedStaticContext(env.makeRetainedStaticContext());
        ExpressionVisitor visitor = ExpressionVisitor.make(env);
        exp = exp.simplify().typeCheck(visitor, env.getConfiguration().makeContextItemStaticInfo(AnyNodeTest.getInstance(), true));
        optimizedExpression = exp;
        List<String> reasonsForFailure = new ArrayList<String>();
        Pattern pattern = StreamingPatternMaker.makeStreamingPattern(exp, env.getConfiguration(), reasonsForFailure);
        if (!reasonsForFailure.isEmpty()) {
            throw new XPathException(reasonsForFailure.get(0));
        }
        return pattern;
    }

    /*@Nullable*/
    protected Expression parseUnionExpression() throws XPathException {
        Expression exp = parseRelativePath();
        while (t.currentToken == Token.VBAR) {
            nextToken();
            exp = new VennExpression(exp, OperatorSymbol.UNION, parseRelativePath());
            setLocation(exp);
        }
        return exp;
    }

    /*@NotNull*/
    @Override
    protected Expression parseRelativePath() throws XPathException {
        boolean first = true;
        Expression exp = parseStepExpression();
        while (t.currentToken == Token.SLASH ||
                t.currentToken == Token.SLASH_SLASH) {
            if (t.currentToken == Token.SLASH_SLASH) {
                if (!(first && exp instanceof ContextItemExpression)) {
                    grumble("The // operator can be used only at the start of the path, preceded by '.'");
                }
            }
            first = false;
            Token op = t.currentToken;
            nextToken();
            Expression next = parseStepExpression();
            if (op == Token.SLASH) {
                exp = new SlashExpression(exp, next);
            } else {
                // add implicit descendant-or-self::node() step
                AxisExpression ae = new AxisExpression(AxisInfo.DESCENDANT_OR_SELF, null);
                setLocation(ae);
                SlashExpression se = new SlashExpression(ae, next);
                setLocation(se);
                if (exp instanceof ContextItemExpression) {
                    exp = se;
                } else {
                    exp = new SlashExpression(exp, se);
                }
            }
            setLocation(exp);
        }
        return exp;
    }


    /**
     * Override the general XPath parsing of a StepExpression to support the simpler
     * kind of Step used in XML Schema selectors
     *
     * @return an Expression, which will always be either an AxisExpression or a
     *         ContextItemExpression
     * @throws XPathException if parsing fails
     */

    protected Expression parseStepExpression() throws XPathException {
        if (t.currentToken == Token.DOT) {
            nextToken();
            Expression cie = new ContextItemExpression();
            setLocation(cie);
            return cie;
        } else if (t.currentToken instanceof Token.NameToken || t.currentToken instanceof Token.Wildcard) {
            Expression ae = new AxisExpression(AxisInfo.CHILD, parseNodeTest(Type.ELEMENT));
            setLocation(ae);
            return ae;
        } else if (t.currentToken == Token.STAR) {
            nextToken();
            Expression ae2 = new AxisExpression(AxisInfo.CHILD, NodeKindTest.ELEMENT);
            setLocation(ae2);
            return ae2;
        } else if (t.currentToken == Token.AT) {
            if (!isFieldSubset) {
                grumble("The attribute axis cannot be used in a selector XPath");
            }
            nextToken();
            if (t.currentToken == Token.STAR) {
                nextToken();
                Expression ae2 = new AxisExpression(AxisInfo.ATTRIBUTE, AnyNodeTest.getInstance());
                setLocation(ae2);
                return ae2;
            } else {
                NodeTest test = parseNodeTest(Type.ATTRIBUTE);
                Expression ae2 = new AxisExpression(AxisInfo.ATTRIBUTE, test);
                setLocation(ae2);
                return ae2;
            }
        } else if (t.peekAhead() == Token.COLON_COLON) {
            int axis = AxisInfo.getAxisNumber(t.currentName());
            if (axis != AxisInfo.CHILD && axis != AxisInfo.ATTRIBUTE) {
                grumble("Only the child and attribute axes are permitted");
            }
            if (axis == AxisInfo.ATTRIBUTE && !isFieldSubset) {
                grumble("The attribute axis cannot be used in a selector XPath");
            }
            short principalNodeType = AxisInfo.principalNodeType[axis];
            nextToken();
            if (t.currentToken == Token.STAR) {
                nextToken();
                Expression ae2 = new AxisExpression(axis, AnyNodeTest.getInstance());
                setLocation(ae2);
                return ae2;
            } else {
                NodeTest test = parseNodeTest(principalNodeType);
                Expression ae2 = new AxisExpression(axis, test);
                setLocation(ae2);
                return ae2;
            }
        }
        return null;
    }

    /**
     * After parsing a selector, return the optimized version of the XPath expression that was parsed
     * @return the optimized XPath expression equivalent to the pattern that was created by parsing
     * the selector
     */

    public Expression getOptimizedExpression() {
        return optimizedExpression;
    }

}

// Copyright (c) 2004-2023 Saxonica Limited
