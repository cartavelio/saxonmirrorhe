package com.saxonica.ee.optim;

import net.sf.saxon.expr.*;
import net.sf.saxon.expr.parser.Token;
import net.sf.saxon.expr.parser.Tokenizer;
import net.sf.saxon.expr.parser.XPathParser;
import net.sf.saxon.ma.arrays.SimpleArrayItem;
import net.sf.saxon.ma.map.EmptyMap;
import net.sf.saxon.om.*;
import net.sf.saxon.pattern.AnyNodeTest;
import net.sf.saxon.pattern.NameTest;
import net.sf.saxon.pattern.NodeKindTest;
import net.sf.saxon.pattern.NodeTest;
import net.sf.saxon.s9api.UnprefixedElementMatchingPolicy;
import net.sf.saxon.trans.SymbolicName;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.Type;
import net.sf.saxon.value.EmptySequence;
import net.sf.saxon.value.NumericValue;
import net.sf.saxon.value.StringValue;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

/**
 * This class performs a quick parse of commonly encountered XPath expressions that conform
 * to common syntactic patterns. Instead of using the full recursive-descent parser, the
 * expression is tokenized, and the sequence of tokens is tested against a table of known
 * common sequences.
 *
 * If the XPath expression supplied cannot be parsed (either because it is too complicated
 * or because it is invalid), the quick parser simply returns null and leaves the work to
 * the regular parser.
 */

public class QuickXPathParser implements XPathParser.Accelerator {

    private static final Map<Long, ExpressionMaker> patterns = new HashMap<>();

    public QuickXPathParser() {
    }

    /**
     * Attempt to parse an expression, provided it is very simple.
     * @param t the tokenizer
     * @param expression the string containing expression to be parsed
     * @param start start position within the input string
     * @param terminator either EOF or RCURLY, indicating how parsing should end
     * @return either the parsed expression, or null if it is erroneous or too
     * complex to parse.
     */

    @Override
    public Expression parse(Tokenizer t, StaticContext env, String expression, int start, int terminator) {
        if (expression.length() - start > 30 && terminator != Token.RCURLY) {
            return null;
        }
        if (env.getXPathVersion() < 31) {
            return null;
        }
        try {
            t.tokenize(expression, start, -1);

            List<Integer> tokenTypes = new ArrayList<>();
            List<String> tokenValues = new ArrayList<>();

            while (t.currentToken != Token.EOF && t.currentToken != terminator) {
                if (tokenTypes.size() > 6) {
                    return null;
                }
                tokenTypes.add(t.currentToken);
                tokenValues.add(t.currentTokenValue);
                t.next();
                if (t.currentToken == Token.RCURLY) {
                    t.lookAhead();
                }
            }

            if (t.currentToken != terminator) {
                return null; // no closing brace after embedded expression in AVT
            }

            long keyValue = key(tokenTypes);
            ExpressionMaker maker = patterns.get(keyValue);
            if (maker == null) {
                return null;
            }
            Expression result = maker.make(tokenValues, env);
            if (result != null) {
                result.setLocation(env.getContainingLocation());
            }
            return result;

        } catch (XPathException e) {
            return null;
        }
    }

    /**
     * Get a {@code long} number that uniquely identifies a sequence of up to 8 tokens.
     * Note: tokens are identified by numbers in the range 0..255.
     * @param tokens the list of token numbers. Up to 8 integers in the range 0.255.
     * @return a key identifying the sequence of tokens.
     */
    private static long key(List<Integer> tokens) {
        long key = 0L;
        for (int token : tokens) {
            key = (key << 8) + token;
        }
        return key;
    }

    /**
     * Get a {@code long} number that uniquely identifies a sequence of up to 8 tokens.
     * Note: tokens are identified by numbers in the range 0..255.
     *
     * @param tokens the array of token numbers. Up to 8 integers in the range 0.255.
     * @return a key identifying the sequence of tokens.
     */
    private static long key(Integer... tokens) {
        long key = 0L;
        for (int token : tokens) {
            key = (key << 8) + token;
        }
        return key;
    }

    @FunctionalInterface
    private interface ExpressionMaker {
        Expression make(List<String> tokens, StaticContext env) throws XPathException;
    }

    static StructuredQName verifyQName(String name, StaticContext env) throws XPathException {
        return new QNameParser(env.getNamespaceResolver()).parse(name, NamespaceUri.NULL);
    }

    static StructuredQName verifyElementQName(String name, StaticContext env) throws XPathException {
        QNameParser qNameParser = new QNameParser(env.getNamespaceResolver());
        return qNameParser.parse(name, env.getDefaultElementNamespace());
    }

    static StructuredQName verifyFunctionQName(String name, StaticContext env) throws XPathException {
        QNameParser qNameParser = new QNameParser(env.getNamespaceResolver());
        return qNameParser.parse(name, env.getDefaultFunctionNamespace());
    }

    static {

        // The expressions handled here are those that occur most frequently in either
        // (a) the XSLT-in-XSLT compiler, or (b) the docbook stylesheets, with the exception
        // of some cases that are considered either too complex to handle here, or atypical.
        // There's no great harm in adding further patterns, but there are diminishing returns.
        // For the cases studied, we catch about 75% of short (< 30 character) XPath expressions.

        // () - empty sequence
        patterns.put(
                key(Token.LPAR, Token.RPAR),
                (values, env) -> Literal.makeLiteral(EmptySequence.getInstance()));

        // [] - empty array
        patterns.put(
                key(Token.LSQB, Token.RSQB),
                (values, env) -> Literal.makeLiteral(SimpleArrayItem.EMPTY_ARRAY));

        // . - context item
        patterns.put(
                key(Token.DOT),
                (values, env) -> new ContextItemExpression());

        // .. - select parent
        patterns.put(
                key(Token.DOTDOT),
                (values, env) -> new AxisExpression(AxisInfo.PARENT, AnyNodeTest.getInstance()));

        // * - all element children
        patterns.put(
                key(Token.STAR),
                (values, env) -> new AxisExpression(AxisInfo.CHILD, NodeKindTest.ELEMENT));

        // string literal
        patterns.put(
                key(Token.STRING_LITERAL),
                (values, env) -> StringLiteral.makeLiteral(StringValue.makeStringValue(values.get(0))));

        // numeric literal
        patterns.put(
                key(Token.NUMBER),
                (values, env) -> toNumericLiteral(values.get(0)));

        // "/" - select root of document
        patterns.put(
                key(Token.SLASH),
                (values, env) -> new RootExpression());

        // nodekind() - select children with given node kind.
        patterns.put(
                key(Token.KEYWORD_LBRA, Token.RPAR),
                (values, env) -> {
                    String kind = values.get(0);
                    NodeTest test = null;
                    switch (kind) {
                        case "node":
                            test = AnyNodeTest.getInstance();
                            break;
                        case "element":
                            test = NodeKindTest.ELEMENT;
                            break;
                        case "text":
                            test = NodeKindTest.TEXT;
                            break;
                        case "comment":
                            test = NodeKindTest.COMMENT;
                            break;
                        case "processing-instruction":
                            test = NodeKindTest.PROCESSING_INSTRUCTION;
                            break;
                        default:
                            return null;
                    }
                    return new AxisExpression(AxisInfo.CHILD, test);
                });

        // $name - variable reference
        patterns.put(
                key(Token.DOLLAR, Token.NAME),
                (values, env) -> {
                    StructuredQName name = verifyQName(values.get(1), env);
                    return env.bindVariable(name);
                });

        // @name - select named attribute
        patterns.put(
                key(Token.AT, Token.NAME),
                (values, env) -> {
                    StructuredQName name = verifyQName(values.get(1), env);
                    return new AxisExpression(AxisInfo.ATTRIBUTE,
                                              new NameTest(Type.ATTRIBUTE, name.getNamespaceUri(), name.getLocalPart(), env.getConfiguration().getNamePool()));
                });

        // @* - select all attributes
        patterns.put(
                key(Token.AT, Token.STAR),
                (values, env) -> new AxisExpression(AxisInfo.ATTRIBUTE, NodeKindTest.ATTRIBUTE));

        // name - select named child elements
        patterns.put(
                key(Token.NAME),
                (values, env) -> {
                    if (env.getUnprefixedElementMatchingPolicy() != UnprefixedElementMatchingPolicy.DEFAULT_NAMESPACE) {
                        return null;
                    }
                    StructuredQName name = verifyElementQName(values.get(0), env);
                    return new AxisExpression(AxisInfo.CHILD,
                                              new NameTest(Type.ELEMENT, name.getNamespaceUri(), name.getLocalPart(), env.getConfiguration().getNamePool()));
                });

        // keyword { } - for example, empty map constructor
        patterns.put(
                key(Token.KEYWORD_CURLY, Token.RCURLY),
                (values, env) -> {
                    String name = values.get(0);
                    if (name.equals("map")) {
                        return Literal.makeLiteral(EmptyMap.getInstance());
                    } else if (name.equals("array")) {
                        return Literal.makeLiteral(SimpleArrayItem.EMPTY_ARRAY);
                    } else {
                        return null;
                    }
                });
        
        // fn() - function with no arguments
        patterns.put(
                key(Token.FUNCTION, Token.RPAR),
                (values, env) -> {
                    StructuredQName name = verifyFunctionQName(values.get(0), env);
                    List<String> reasons = new ArrayList<>();
                    return env.getFunctionLibrary().bind(
                            new SymbolicName.F(name, 0), new Expression[]{}, null, env, reasons);
                });

        // fn(.) - function with "." as single argument
        patterns.put(
                key(Token.FUNCTION, Token.DOT, Token.RPAR),
                (values, env) -> {
                    StructuredQName name = verifyFunctionQName(values.get(0), env);
                    List<String> reasons = new ArrayList<>();
                    Expression cie = new ContextItemExpression();
                    cie.setLocation(env.getContainingLocation());
                    return env.getFunctionLibrary().bind(
                            new SymbolicName.F(name, 1), new Expression[]{cie}, null, env, reasons);
                });

        // fn(name) - function with child element reference as single argument
        patterns.put(
                key(Token.FUNCTION, Token.NAME, Token.RPAR),
                (values, env) -> {
                    StructuredQName fnName = verifyFunctionQName(values.get(0), env);
                    StructuredQName elName = verifyElementQName(values.get(1), env);
                    List<String> reasons = new ArrayList<>();
                    Expression child = new AxisExpression(
                            AxisInfo.CHILD,
                            new NameTest(Type.ELEMENT, elName.getNamespaceUri(), elName.getLocalPart(), env.getConfiguration().getNamePool()));
                    child.setLocation(env.getContainingLocation());
                    return env.getFunctionLibrary().bind(
                            new SymbolicName.F(fnName, 1), new Expression[]{child}, null, env, reasons);
                });

        // fn($var) - function with variable reference as single argument
        patterns.put(
                key(Token.FUNCTION, Token.DOLLAR, Token.NAME, Token.RPAR),
                (values, env) -> {
                    StructuredQName varName = verifyQName(values.get(2), env);
                    Expression varRef = env.bindVariable(varName);
                    varRef.setLocation(env.getContainingLocation());
                    StructuredQName name = verifyFunctionQName(values.get(0), env);
                    List<String> reasons = new ArrayList<>();
                    return env.getFunctionLibrary().bind(
                            new SymbolicName.F(name, 1), new Expression[]{varRef}, null, env, reasons);
                });

        // fn(@att) - function with attribute reference as single argument
        patterns.put(
                key(Token.FUNCTION, Token.AT, Token.NAME, Token.RPAR),
                (values, env) -> {
                    StructuredQName attName = verifyQName(values.get(2), env);
                    AxisExpression arg = new AxisExpression(
                            AxisInfo.ATTRIBUTE,
                            new NameTest(Type.ATTRIBUTE, attName.getNamespaceUri(), attName.getLocalPart(), env.getConfiguration().getNamePool()));
                    arg.setLocation(env.getContainingLocation());
                    StructuredQName name = verifyFunctionQName(values.get(0), env);
                    List<String> reasons = new ArrayList<>();
                    return env.getFunctionLibrary().bind(
                            new SymbolicName.F(name, 1), new Expression[]{arg}, null, env, reasons);
                });

        // fn('string') - function with string literal as single argument
        patterns.put(
                key(Token.FUNCTION, Token.STRING_LITERAL, Token.RPAR),
                (values, env) -> {
                    Expression arg = StringLiteral.makeLiteral(StringValue.makeStringValue(values.get(1)));
                    arg.setLocation(env.getContainingLocation());
                    StructuredQName name = verifyFunctionQName(values.get(0), env);
                    List<String> reasons = new ArrayList<>();
                    return env.getFunctionLibrary().bind(
                            new SymbolicName.F(name, 1), new Expression[]{arg}, null, env, reasons);
                });

        // name/name - simple path expression selecting grandchildren
        patterns.put(
                key(Token.NAME, Token.SLASH, Token.NAME),
                (values, env) -> {
                    if (env.getUnprefixedElementMatchingPolicy() != UnprefixedElementMatchingPolicy.DEFAULT_NAMESPACE) {
                        return null;
                    }
                    StructuredQName first = verifyElementQName(values.get(0), env);
                    AxisExpression start =
                            new AxisExpression(
                                    AxisInfo.CHILD,
                                    new NameTest(Type.ELEMENT, first.getNamespaceUri(), first.getLocalPart(), env.getConfiguration().getNamePool()));
                    start.setLocation(env.getContainingLocation());
                    StructuredQName second = verifyElementQName(values.get(2), env);
                    AxisExpression step =
                            new AxisExpression(
                                    AxisInfo.CHILD,
                                    new NameTest(Type.ELEMENT, second.getNamespaceUri(), second.getLocalPart(), env.getConfiguration().getNamePool()));
                    step.setLocation(env.getContainingLocation());
                    return new SlashExpression(start, step);
                });

        // name | name - simple union expression selecting children
        patterns.put(
                key(Token.NAME, Token.UNION, Token.NAME),
                (values, env) -> {
                    if (env.getUnprefixedElementMatchingPolicy() != UnprefixedElementMatchingPolicy.DEFAULT_NAMESPACE) {
                        return null;
                    }
                    StructuredQName first = verifyElementQName(values.get(0), env);
                    AxisExpression one =
                            new AxisExpression(
                                    AxisInfo.CHILD,
                                    new NameTest(Type.ELEMENT, first.getNamespaceUri(), first.getLocalPart(), env.getConfiguration().getNamePool()));
                    one.setLocation(env.getContainingLocation());
                    StructuredQName second = verifyElementQName(values.get(2), env);
                    AxisExpression two =
                            new AxisExpression(
                                    AxisInfo.CHILD,
                                    new NameTest(Type.ELEMENT, second.getNamespaceUri(), second.getLocalPart(), env.getConfiguration().getNamePool()));
                    two.setLocation(env.getContainingLocation());
                    return new VennExpression(one, Token.UNION, two);
                });

        // name/@name - simple path expression selecting attributes of children
        patterns.put(
                key(Token.NAME, Token.SLASH, Token.AT, Token.NAME),
                (values, env) -> {
                    if (env.getUnprefixedElementMatchingPolicy() != UnprefixedElementMatchingPolicy.DEFAULT_NAMESPACE) {
                        return null;
                    }
                    StructuredQName first = verifyElementQName(values.get(0), env);
                    AxisExpression start =
                            new AxisExpression(
                                    AxisInfo.CHILD,
                                    new NameTest(Type.ELEMENT, first.getNamespaceUri(), first.getLocalPart(), env.getConfiguration().getNamePool()));
                    start.setLocation(env.getContainingLocation());
                    StructuredQName second = verifyQName(values.get(3), env);
                    AxisExpression step =
                            new AxisExpression(
                                    AxisInfo.ATTRIBUTE,
                                    new NameTest(Type.ATTRIBUTE, second.getNamespaceUri(), second.getLocalPart(), env.getConfiguration().getNamePool()));
                    step.setLocation(env.getContainingLocation());
                    return new SlashExpression(start, step);
                });

        // $var/@name - simple path expression selecting attributes of element in variable
        patterns.put(
                key(Token.DOLLAR, Token.NAME, Token.SLASH, Token.AT, Token.NAME),
                (values, env) -> {
                    StructuredQName first = verifyQName(values.get(1), env);
                    Expression varRef = env.bindVariable(first);
                    varRef.setLocation(env.getContainingLocation());
                    StructuredQName second = verifyQName(values.get(4), env);
                    AxisExpression step =
                            new AxisExpression(
                                    AxisInfo.ATTRIBUTE,
                                    new NameTest(Type.ATTRIBUTE, second.getNamespaceUri(), second.getLocalPart(), env.getConfiguration().getNamePool()));
                    step.setLocation(env.getContainingLocation());
                    return new SlashExpression(varRef, step);
                });

        // $var[number] - simple numeric filter
        patterns.put(
                key(Token.DOLLAR, Token.NAME, Token.LSQB, Token.NUMBER, Token.RSQB),
                (values, env) -> {
                    StructuredQName first = verifyQName(values.get(1), env);
                    Expression varRef = env.bindVariable(first);
                    varRef.setLocation(env.getContainingLocation());
                    if (values.get(3).equals("1")) {
                        return FirstItemExpression.makeFirstItemExpression(varRef);
                    } else {
                        Expression subscript = toNumericLiteral(values.get(3));
                        subscript.setLocation(env.getContainingLocation());
                        return new FilterExpression(varRef, subscript);
                    }
                });

        // $var?name - lookup expression in a map
        patterns.put(
                key(Token.DOLLAR, Token.NAME, Token.QMARK, Token.NAME),
                (values, env) -> {
                    StructuredQName first = verifyQName(values.get(1), env);
                    Expression varRef = env.bindVariable(first);
                    varRef.setLocation(env.getContainingLocation());
                    if (!NameChecker.isValidNCName(values.get(3))) {
                        return null;
                    }
                    return new LookupExpression(varRef, new StringLiteral(values.get(3)));
                });

        // $name = string-literal - variable reference = string literal
        patterns.put(
                key(Token.DOLLAR, Token.NAME, Token.EQUALS, Token.STRING_LITERAL),
                (values, env) -> {
                    if (env.isInBackwardsCompatibleMode()) {
                        return null;
                    }
                    StructuredQName name = verifyQName(values.get(1), env);
                    Expression varRef = env.bindVariable(name);
                    varRef.setLocation(env.getContainingLocation());
                    Expression comparand = new StringLiteral(values.get(3));
                    comparand.setLocation(env.getContainingLocation());
                    return new GeneralComparison20(varRef, Token.EQUALS, comparand);
                });

        // $name != string-literal - variable reference != string literal
        patterns.put(
                key(Token.DOLLAR, Token.NAME, Token.NE, Token.STRING_LITERAL),
                (values, env) -> {
                    if (env.isInBackwardsCompatibleMode()) {
                        return null;
                    }
                    StructuredQName name = verifyQName(values.get(1), env);
                    Expression varRef = env.bindVariable(name);
                    varRef.setLocation(env.getContainingLocation());
                    Expression comparand = new StringLiteral(values.get(3));
                    comparand.setLocation(env.getContainingLocation());
                    return new GeneralComparison20(varRef, Token.NE, comparand);
                });

        // $name = numeric-literal - variable reference = numeric literal
        patterns.put(
                key(Token.DOLLAR, Token.NAME, Token.EQUALS, Token.NUMBER),
                (values, env) -> {
                    if (env.isInBackwardsCompatibleMode()) {
                        return null;
                    }
                    StructuredQName name = verifyQName(values.get(1), env);
                    Expression varRef = env.bindVariable(name);
                    varRef.setLocation(env.getContainingLocation());
                    Expression comparand = toNumericLiteral(values.get(3));
                    comparand.setLocation(env.getContainingLocation());
                    return new GeneralComparison20(varRef, Token.EQUALS, comparand);
                });

        // $name != numeric-literal - variable reference != numeric literal
        patterns.put(
                key(Token.DOLLAR, Token.NAME, Token.NE, Token.NUMBER),
                (values, env) -> {
                    if (env.isInBackwardsCompatibleMode()) {
                        return null;
                    }
                    StructuredQName name = verifyQName(values.get(1), env);
                    Expression varRef = env.bindVariable(name);
                    varRef.setLocation(env.getContainingLocation());
                    Expression comparand = toNumericLiteral(values.get(3));
                    comparand.setLocation(env.getContainingLocation());
                    return new GeneralComparison20(varRef, Token.NE, comparand);
                });

        // $name + number - variable reference + numeric literal
        patterns.put(
                key(Token.DOLLAR, Token.NAME, Token.PLUS, Token.NUMBER),
                (values, env) -> {
                    if (env.isInBackwardsCompatibleMode()) {
                        return null;
                    }
                    StructuredQName name = verifyQName(values.get(1), env);
                    Expression varRef = env.bindVariable(name);
                    varRef.setLocation(env.getContainingLocation());
                    Expression addend = toNumericLiteral(values.get(3));
                    addend.setLocation(env.getContainingLocation());
                    return new ArithmeticExpression(varRef, Token.PLUS, addend);
                });

        // <axis> name - select elements on explicit axis
        patterns.put(
                key(Token.AXIS, Token.NAME),
                (values, env) -> {
                    int axis = AxisInfo.getAxisNumber(values.get(0));
                    boolean reverse = !AxisInfo.isForwards[axis];
                    if (reverse || axis == AxisInfo.ATTRIBUTE || axis == AxisInfo.NAMESPACE) {
                        return null;
                    }
                    StructuredQName name = verifyElementQName(values.get(1), env);
                    return new AxisExpression(
                            axis,
                            new NameTest(Type.ELEMENT, name.getNamespaceUri(), name.getLocalPart(), env.getConfiguration().getNamePool()));
                });

    }

    private static Expression toNumericLiteral(String value) {
        NumericValue number = NumericValue.parseNumber(value);
        if (number.isNaN()) {
            return null;
        }
        return Literal.makeLiteral(number);
    }


}

// Copyright (c) 2020-2023 Saxonica Limited

