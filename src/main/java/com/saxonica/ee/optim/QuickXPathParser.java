package com.saxonica.ee.optim;

import net.sf.saxon.expr.*;
import net.sf.saxon.expr.parser.OperatorSymbol;
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
import net.sf.saxon.value.Int64Value;
import net.sf.saxon.value.NumericValue;
import net.sf.saxon.value.StringValue;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.function.Predicate;

/**
 * This class performs a quick parse of commonly encountered XPath expressions that conform
 * to common syntactic patterns. Instead of using the full recursive-descent parser, the
 * expression is tokenized, and the sequence of tokens is tested against a table of known
 * common sequences.
 * <p>
 * If the XPath expression supplied cannot be parsed (either because it is too complicated
 * or because it is invalid), the quick parser simply returns null and leaves the work to
 * the regular parser.
 */

public class QuickXPathParser implements XPathParser.Accelerator {

    @FunctionalInterface
    private interface ExpressionMaker {
        Expression make(List<String> tokens, StaticContext env) throws XPathException;
    }

    public static final Token NOMINAL_NAME = new Token.NameToken("N");

    public static final Token NOMINAL_NUMBER = new Token.NumericLiteral("0", Int64Value.ZERO);

    public static final Token NOMINAL_STRING = new Token.StringLiteral(("S"));


    private static final Map<String, ExpressionMaker> patterns = new HashMap<>();

    public QuickXPathParser() {
    }

    /**
     * Attempt to parse an expression, provided it is very simple.
     *
     * @param t          the tokenizer
     * @param expression the string containing expression to be parsed
     * @param start      start position within the input string
     * @param terminator either EOF or RCURLY, indicating how parsing should end
     * @return either the parsed expression, or null if it is erroneous or too
     * complex to parse.
     */

    @Override
    public Expression parse(Tokenizer t, StaticContext env, String expression, int start, Predicate<Token> terminator) {
        if (expression.length() - start > 30 && !terminator.test(Token.RCURLY)) {
            return null;
        }
        if (env.getXPathVersion() < 31) {
            return null;
        }
        try {
            t.tokenize(expression, start, -1);

            List<Token> tokenTypes = new ArrayList<>();
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

            String keyValue = key(tokenTypes);
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
     * Get a key that uniquely identifies a sequence of  tokens.
     *
     * @param tokens the list of token numbers. Up to 8 integers in the range 0.255.
     * @return a key identifying the sequence of tokens.
     */
    private static String key(List<Token> tokens) {
        StringBuilder sb = new StringBuilder(16);
        for (Token token : tokens) {
            if (token instanceof Token.NameToken) {
                sb.append("N");
            } else if (token instanceof Token.NumericLiteral) {
                sb.append("0");
            } else if (token instanceof Token.StringLiteral) {
                sb.append("S");
            } else {
                sb.append(token.toString());
            }
            sb.append("§");
        }
        return sb.toString();
    }

    /**
     * Get a key that uniquely identifies a sequence of  tokens.
     *
     * @param tokens the array of tokens.
     * @return a key identifying the sequence of tokens.
     */
    private static String key(Token... tokens) {
        StringBuilder sb = new StringBuilder(16);
        for (Token token : tokens) {
            if (token instanceof Token.NameToken) {
                sb.append("N");
            } else if (token instanceof Token.NumericLiteral) {
                sb.append("0");
            } else if (token instanceof Token.StringLiteral) {
                sb.append("S");
            } else {
                sb.append(token.toString());
            }
            sb.append("§");
        }
        return sb.toString();
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
                key(Token.LPAREN, Token.RPAREN),
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
                key(Token.DOT_DOT),
                (values, env) -> new AxisExpression(AxisInfo.PARENT, AnyNodeTest.getInstance()));

        // * - all element children
        patterns.put(
                key(Token.STAR),
                (values, env) -> new AxisExpression(AxisInfo.CHILD, NodeKindTest.ELEMENT));

        // string literal
        patterns.put(
                key(NOMINAL_STRING),
                (values, env) -> StringLiteral.makeLiteral(StringValue.makeStringValue(values.get(0))));

        // numeric literal
        patterns.put(
                key(NOMINAL_NUMBER),
                (values, env) -> toNumericLiteral(values.get(0)));

        // "/" - select root of document
        patterns.put(
                key(Token.SLASH),
                (values, env) -> new RootExpression());

        // nodekind() - select children with given node kind.
        patterns.put(
                key(NOMINAL_NAME, Token.LPAREN, Token.RPAREN),
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
                            StructuredQName name = verifyFunctionQName(values.get(0), env);
                            List<String> reasons = new ArrayList<>();
                            return env.getFunctionLibrary().bind(
                                    new SymbolicName.F(name, 0), new Expression[]{}, null, env, reasons);
                    }
                    return new AxisExpression(AxisInfo.CHILD, test);
                });

        // $name - variable reference
        patterns.put(
                key(Token.DOLLAR, NOMINAL_NAME),
                (values, env) -> {
                    StructuredQName name = verifyQName(values.get(1), env);
                    return env.bindVariable(name);
                });

        // @name - select named attribute
        patterns.put(
                key(Token.AT, NOMINAL_NAME),
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
                key(NOMINAL_NAME),
                (values, env) -> {
                    if (env.getUnprefixedElementMatchingPolicy() != UnprefixedElementMatchingPolicy.DEFAULT_NAMESPACE) {
                        return null;
                    }
                    StructuredQName name = verifyElementQName(values.get(0), env);
                    return new AxisExpression(AxisInfo.CHILD,
                                              new NameTest(Type.ELEMENT, name.getNamespaceUri(), name.getLocalPart(), env.getConfiguration().getNamePool()));
                });

        // keyword { } - for example, empty map or array constructor
        patterns.put(
                key(NOMINAL_NAME, Token.LCURLY, Token.RCURLY),
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

        // { } - empty map constructor
        patterns.put(
                key(Token.LCURLY, Token.RCURLY),
                (values, env) -> Literal.makeLiteral(EmptyMap.getInstance()));

        // [ ] - empty map constructor
        patterns.put(
                key(Token.LSQB, Token.RSQB),
                (values, env) -> Literal.makeLiteral(SimpleArrayItem.EMPTY_ARRAY));

        // fn(.) - function with "." as single argument
        patterns.put(
                key(NOMINAL_NAME, Token.LPAREN, Token.DOT, Token.RPAREN),
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
                key(NOMINAL_NAME, Token.LPAREN, NOMINAL_NAME, Token.RPAREN),
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
                key(NOMINAL_NAME, Token.LPAREN, Token.DOLLAR, NOMINAL_NAME, Token.RPAREN),
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
                key(NOMINAL_NAME, Token.LPAREN, Token.AT, NOMINAL_NAME, Token.RPAREN),
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
                key(NOMINAL_NAME, Token.LPAREN, NOMINAL_STRING, Token.RPAREN),
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
                key(NOMINAL_NAME, Token.SLASH, NOMINAL_NAME),
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
                key(NOMINAL_NAME, Token.VBAR, NOMINAL_NAME),
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
                    return new VennExpression(one, OperatorSymbol.UNION, two);
                });

        // name/@name - simple path expression selecting attributes of children
        patterns.put(
                key(NOMINAL_NAME, Token.SLASH, Token.AT, NOMINAL_NAME),
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
                key(Token.DOLLAR, NOMINAL_NAME, Token.SLASH, Token.AT, NOMINAL_NAME),
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
                key(Token.DOLLAR, NOMINAL_NAME, Token.LSQB, NOMINAL_NUMBER, Token.RSQB),
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
                key(Token.DOLLAR, NOMINAL_NAME, Token.QMARK, NOMINAL_NAME),
                (values, env) -> {
                    StructuredQName first = verifyQName(values.get(1), env);
                    Expression varRef = env.bindVariable(first);
                    varRef.setLocation(env.getContainingLocation());
                    if (!NameChecker.isValidNCName(values.get(3))) {
                        return null;
                    }
                    return new LookupExpression(varRef, LookupExpression.Modifier.CONTENT, new StringLiteral(values.get(3)));
                });

        // $name = string-literal - variable reference = string literal
        patterns.put(
                key(Token.DOLLAR, NOMINAL_NAME, Token.EQUALS, NOMINAL_STRING),
                (values, env) -> {
                    if (env.isInBackwardsCompatibleMode()) {
                        return null;
                    }
                    StructuredQName name = verifyQName(values.get(1), env);
                    Expression varRef = env.bindVariable(name);
                    varRef.setLocation(env.getContainingLocation());
                    Expression comparand = new StringLiteral(values.get(3));
                    comparand.setLocation(env.getContainingLocation());
                    return new GeneralComparison20(varRef, OperatorSymbol.EQUALS, comparand);
                });

        // $name != string-literal - variable reference != string literal
        patterns.put(
                key(Token.DOLLAR, NOMINAL_NAME, Token.NE, NOMINAL_STRING),
                (values, env) -> {
                    if (env.isInBackwardsCompatibleMode()) {
                        return null;
                    }
                    StructuredQName name = verifyQName(values.get(1), env);
                    Expression varRef = env.bindVariable(name);
                    varRef.setLocation(env.getContainingLocation());
                    Expression comparand = new StringLiteral(values.get(3));
                    comparand.setLocation(env.getContainingLocation());
                    return new GeneralComparison20(varRef, OperatorSymbol.NE, comparand);
                });

        // $name = numeric-literal - variable reference = numeric literal
        patterns.put(
                key(Token.DOLLAR, NOMINAL_NAME, Token.EQUALS, NOMINAL_NUMBER),
                (values, env) -> {
                    if (env.isInBackwardsCompatibleMode()) {
                        return null;
                    }
                    StructuredQName name = verifyQName(values.get(1), env);
                    Expression varRef = env.bindVariable(name);
                    varRef.setLocation(env.getContainingLocation());
                    Expression comparand = toNumericLiteral(values.get(3));
                    comparand.setLocation(env.getContainingLocation());
                    return new GeneralComparison20(varRef, OperatorSymbol.EQUALS, comparand);
                });

        // $name != numeric-literal - variable reference != numeric literal
        patterns.put(
                key(Token.DOLLAR, NOMINAL_NAME, Token.NE, NOMINAL_NUMBER),
                (values, env) -> {
                    if (env.isInBackwardsCompatibleMode()) {
                        return null;
                    }
                    StructuredQName name = verifyQName(values.get(1), env);
                    Expression varRef = env.bindVariable(name);
                    varRef.setLocation(env.getContainingLocation());
                    Expression comparand = toNumericLiteral(values.get(3));
                    comparand.setLocation(env.getContainingLocation());
                    return new GeneralComparison20(varRef, OperatorSymbol.NE, comparand);
                });

        // $name + number - variable reference + numeric literal
        patterns.put(
                key(Token.DOLLAR, NOMINAL_NAME, Token.PLUS, NOMINAL_NUMBER),
                (values, env) -> {
                    if (env.isInBackwardsCompatibleMode()) {
                        return null;
                    }
                    StructuredQName name = verifyQName(values.get(1), env);
                    Expression varRef = env.bindVariable(name);
                    varRef.setLocation(env.getContainingLocation());
                    Expression addend = toNumericLiteral(values.get(3));
                    addend.setLocation(env.getContainingLocation());
                    return new ArithmeticExpression(varRef, OperatorSymbol.PLUS, addend);
                });

        // <axis> name - select elements on explicit axis
        patterns.put(
                key(NOMINAL_NAME, Token.COLON_COLON, NOMINAL_NAME),
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

