////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2026 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.pattern;

import net.sf.saxon.expr.*;
import net.sf.saxon.expr.instruct.Choose;
import net.sf.saxon.expr.parser.*;
import net.sf.saxon.functions.*;
import net.sf.saxon.functions.hof.FunctionLiteral;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.om.AxisInfo;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.om.QNameParser;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.Affinity;
import net.sf.saxon.type.ItemType;
import net.sf.saxon.type.NumericType;
import net.sf.saxon.type.TypeHierarchy;
import net.sf.saxon.type.gnode.AnyGNodeType;
import net.sf.saxon.type.gnode.AnyXNodeType;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.BooleanValue;
import net.sf.saxon.value.Int64Value;
import net.sf.saxon.value.SequenceType;

import java.util.ArrayList;
import java.util.List;

/**
 * Parser for XSLT patterns. This is created by overriding selected parts of the standard ExpressionParser.
 */

public class PatternParser extends XPathParser {

    private int inPredicate = 0;
    private int inXNodePattern = 0;

    public PatternParser(StaticContext env) {
        super(env);
    }

    /**
     * Parse a string representing an XSLT pattern
     *
     * @param pattern the pattern expressed as a String
     * @param env     the static context for the pattern
     * @return a Pattern object representing the result of parsing
     * @throws net.sf.saxon.trans.XPathException
     *          if the pattern contains a syntax error
     */

    /*@NotNull*/
    public Pattern parsePattern(String pattern, StaticContext env) throws XPathException {
        this.env = env;
        charChecker = env.getConfiguration().getValidCharacterChecker();
        setLanguage(ParsedLanguage.XSLT_PATTERN, env.getXPathVersion());

        if (qNameParser == null) {
            qNameParser = new QNameParser(env.getNamespaceResolver());
            if (languageVersion >= 30) {
                qNameParser = qNameParser.withAcceptEQName(true, languageVersion);
            }
        }
        int version = env.getXPathVersion();
        t = new Tokenizer();
        t.languageLevel = version;
        t.allowSaxonExtensions =
                env.getConfiguration().getBooleanProperty(Feature.ALLOW_SYNTAX_EXTENSIONS) || version >= 40;
        try {
            t.tokenize(pattern, 0, -1);
        } catch (XPathException err) {
            grumble(err.getMessage());
        }

        Pattern result = parsePattern();
        expect(Token.EOF);
        result.setRetainedStaticContextThoroughly(env.makeRetainedStaticContext());
        return result;
    }

    /**
     * Parse an instance of the Pattern production, either a top-level pattern
     * or a nested pattern
     * @return the parsed Pattern
     * @throws XPathException in the event of a syntax error
     */

    private Pattern parsePattern() throws XPathException {
        Pattern result = null;
        int version = env.getXPathVersion();
        if (version >= 40) {
            if (t.currentToken == Token.DOT) {
                result = parsePredicatePattern();
            } else if (t.currentToken == Token.TILDE) {
                result = parseTypePattern();
            } else if (t.currentToken == Token.LCURLY) {
                result = parseMapPattern();
            } else if (t.peekAhead() == Token.LCURLY && isKeyword("map")) {
                nextToken();
                result = parseMapPattern();
            } else if (t.peekAhead() == Token.LPAREN && isKeyword("array")) {
                result = parseArrayPattern();
            } else if (t.peekAhead() == Token.LPAREN && isKeyword("jnode")) {
                result = parseJNodePattern();
            } else {
                result = parsePathExprP();
            }
        } else {
            if (t.currentToken == Token.DOT) {
                result = parsePredicatePattern();
            } else {
                result = parsePathExprP();
            }
        }
        return result;
    }



    public Pattern parseUnionPattern(boolean topLevel, StaticContext env) throws XPathException {
        Pattern operand = parseIntersectExceptPattern(topLevel, env);
        while (t.currentToken == Token.VBAR || isKeyword("union")) {
            nextToken();
            Pattern next = parseIntersectExceptPattern(topLevel, env);
            operand = new UnionPattern(operand, next);
            // default in <= 3.0 is to take the priority from the component patterns.
            // In 4.0 it is the max of the component priorities
            if (env.getXPathVersion() >= 40) {
                UnionPattern up = (UnionPattern) operand;
                operand.setPriority(Math.max(up.p1.getDefaultPriority(), up.p2.getDefaultPriority()));
            } else {
                operand.setPriority(Double.NaN);
            }
        }
        return operand;
    }

    public Pattern parseIntersectExceptPattern(boolean topLevel, StaticContext env) throws XPathException {
        Pattern operand = parsePathExprP();
        while (isKeyword("intersect") || isKeyword("except")) {
            Pattern first = operand;
            String keyword = t.currentName();
            nextToken();
            Pattern next = parsePathExprP();
            if (keyword.equals("intersect")) {
                operand = new IntersectPattern(operand, next);
            } else {
                operand = new ExceptPattern(operand, next);
            }
            operand.setPriority(first.getDefaultPriority());
        }
        return operand;
    }

    protected void notAllowedInPattern() throws XPathException {
        if (inPredicate == 0) {
            grumble("Pattern uses syntax that is allowed in expressions, but not in patterns");
        }
    }


    public Pattern parsePredicatePattern() throws XPathException {
        readToken(Token.DOT);
        if (t.currentToken == Token.LSQB) {
            TypeHierarchy th = env.getConfiguration().getTypeHierarchy();
            Expression combinedCondition = null;
            while (t.currentToken == Token.LSQB) {
                nextToken();
                Expression predicate = parsePredicate();
                readToken(Token.RSQB);

                // Need to consider the possibility of a numeric predicate
                ItemType filterType = predicate.getItemType();
                Affinity rel = th.relationship(filterType, NumericType.getInstance());
                if (rel != Affinity.DISJOINT) {
                    // the predicate may be numeric
                    if (rel == Affinity.SAME_TYPE || rel == Affinity.SUBSUMED_BY) {
                        // the predicate IS numeric: rewrite as N eq 1, since other values don't match
                        predicate = new ValueComparison(predicate, OperatorSymbol.FEQ, Literal.makeLiteral(Int64Value.PLUS_ONE));
                    } else {
                        // the predicate MIGHT BE numeric: rewrite as
                        // let $P := predicate return if ($P instance of xs:numeric) then ($P eq 1) else $P
                        LetExpression let = new LetExpression();
                        StructuredQName varName =
                                new StructuredQName("vv", NamespaceUri.SAXON_GENERATED_VARIABLE, "v" + predicate.hashCode());
                        let.setVariableQName(varName);
                        InstanceOfExpression condition =
                                new InstanceOfExpression(new LocalVariableReference(let), SequenceType.SINGLE_NUMERIC);
                        LocalVariableReference ref = new LocalVariableReference(let);
                        ref.setStaticType(SequenceType.ANY_SEQUENCE, null, 0);
                        ValueComparison comparison =
                                new ValueComparison(ref, OperatorSymbol.FEQ, Literal.makeLiteral(Int64Value.PLUS_ONE));
                        Choose choice = new Choose(new Expression[]{condition, Literal.makeLiteral(BooleanValue.TRUE)},
                                                   new Expression[]{comparison, new LocalVariableReference(let)});
                        let.setSequence(predicate);
                        let.setAction(choice);
                        let.setRequiredType(SequenceType.ANY_SEQUENCE);
                        let.setRetainedStaticContext(env.makeRetainedStaticContext());
                        predicate = let;
                    }
                }
                if (combinedCondition == null) {
                    combinedCondition = predicate;
                } else {
                    combinedCondition = new AndExpression(combinedCondition, predicate);
                }
            }
            return new BooleanExpressionPattern(combinedCondition);
        } else {
            return new UniversalPattern();
        }
    }

    private Pattern parseTypePattern() throws XPathException {
        Pattern result;
        readToken(Token.TILDE);
        ItemType type = parseItemType();
        result = new ItemTypePattern(type);
        return addPredicates(result);
    }

    private Pattern addPredicates(Pattern result) throws XPathException {
        while (t.currentToken == Token.LSQB) {
            nextToken();
            Expression predicate = parsePredicate();
            readToken(Token.RSQB);
            result = new BasePatternWithPredicate(result, predicate);
        }
        return result;
    }

    private Pattern parseArrayPattern() throws XPathException {
        nextToken();
        readToken(Token.LPAREN);
        SequenceMatcher sequenceMatcher = parseSequenceMatcher();
        readToken(Token.RPAREN);
        Pattern result = new ArrayPattern(sequenceMatcher);
        return addPredicates(result);
    }

    private Pattern parseMapPattern() throws XPathException {
        readToken(Token.LCURLY);
        boolean extensible = false;
        List<MapPattern.MapPatternEntry> entries = new ArrayList<MapPattern.MapPatternEntry>();
        while (t.currentToken != Token.RCURLY) {
            if (t.currentToken == Token.STAR) {
                extensible = true;
                nextToken();
                break;
            } else {
                AtomicValue key = parseConstant();
                if (t.currentToken == Token.COLON) {
                    nextToken();
                    SequenceMatcher sequenceMatcher = parseSequenceMatcher();
                    entries.add(new MapPattern.MapPatternEntry(key, sequenceMatcher));
                } else {
                    entries.add(new MapPattern.MapPatternEntry(key, SequenceMatcher.UNIVERSAL_MATCHER));
                }
                if (t.currentToken == Token.COMMA) {
                    nextToken();
                }
            }
        }
        readToken(Token.RCURLY);
        Pattern result = new MapPattern(entries, extensible);
        return addPredicates(result);
    }


    private Pattern parseJNodePattern() throws XPathException {
        nextToken();
        readToken(Token.LPAREN);
        AtomicValue key = null;
        if (t.currentToken == Token.STAR) {
            nextToken();
        } else {
            key = parseConstant();
        }
        readToken(Token.COMMA);
        SequenceMatcher sequenceMatcher = parseSequenceMatcher();
        readToken(Token.RPAREN);
        Pattern result = new JNodePattern(key, sequenceMatcher);
        return addPredicates(result);
    }

    private SequenceMatcher parseSequenceMatcher() throws XPathException {
        Pattern content = parsePattern();
        int cardinality = StaticProperty.EXACTLY_ONE;
        if (t.currentToken == Token.QMARK) {
            cardinality = StaticProperty.ALLOWS_ZERO_OR_ONE;
            nextToken();
        } else if (t.currentToken == Token.PLUS) {
            cardinality = StaticProperty.ALLOWS_ONE_OR_MORE;
            nextToken();
        } else if (t.currentToken == Token.STAR) {
            cardinality = StaticProperty.ALLOWS_ZERO_OR_MORE;
            nextToken();
        }
        return new SequenceMatcher(content, cardinality);
    }

    private Pattern parsePathExprP() throws XPathException {

        // The grammar for an parsePathExprP is a subset of XPath expression syntax. We therefore parse the
        // construct as an expression, and then convert the resulting expression to a pattern. Where there
        // are differences between expression syntax and pattern syntax, we either override the relevant
        // parsing logic, or in some cases there is conditional code in the XPath parser that is sensitive
        // to the language being parsed.

        inXNodePattern++;
        Expression exp = parseExpression();
        ExpressionTool.setDeepRetainedStaticContext(exp, env.makeRetainedStaticContext());

        // If we have a union pattern, check that neither operand is a PredicatePattern
        if (exp instanceof VennExpression) {
            checkNoPredicatePattern(((VennExpression) exp).getLhsExpression());
            checkNoPredicatePattern(((VennExpression) exp).getRhsExpression());
        }

        ExpressionVisitor visitor = ExpressionVisitor.make(env);
        visitor.setOptimizeForPatternMatching(true);
        ItemType contextItemType = isAllowXPath40Syntax() ? AnyGNodeType.getInstance() : AnyXNodeType.getInstance();
        ContextItemStaticInfo cit = visitor.getConfiguration().makeContextItemStaticInfo(
                contextItemType, Optionality.OPTIONAL);
        Pattern pat;
        try {
            pat = PatternMaker.fromExpression(exp.simplify().typeCheck(visitor, cit), env.getConfiguration(), true);
        } catch (XPathException e) {
            pat = PatternMaker.fromExpression(exp.simplify(), env.getConfiguration(), true);
        }

        if (exp instanceof FilterExpression) {
            // the pattern has been simplified but needs to retain a default priority based on its syntactic form (test match-058)
            pat.setPriority(0.5);
        }
        inXNodePattern--;
        return pat;
    }

    @Override
    public Expression parseParenthesizedExpression() throws XPathException {
        Expression exp = super.parseParenthesizedExpression();
        if (inPredicate > 0) {
            return exp;
        }
        // Unless within a predicate or within an XNodePattern, allow a parenthesized pattern here
        checkNoPredicatePattern(exp);
        return exp;
    }

 

    private void checkNoPredicatePattern(Expression exp) throws XPathException {
        if (exp instanceof ContextItemExpression) {
            grumble("A PredicatePattern can appear only at the outermost level (parentheses and union operator are not allowed)");
        }
        if (exp instanceof FilterExpression) {
            checkNoPredicatePattern(((FilterExpression) exp).getBase());
        }
        if (exp instanceof VennExpression) {
            checkNoPredicatePattern(((VennExpression) exp).getLhsExpression());
            checkNoPredicatePattern(((VennExpression) exp).getRhsExpression());
        }
    }

    /**
     * Callback to tailor the tokenizer
     */

    @Override
    protected void customizeTokenizer(Tokenizer t) {
        // no action
    }

    /**
     * Override the parsing of top-level expressions
     *
     * @return the parsed expression
     * @throws net.sf.saxon.trans.XPathException if the pattern is invalid
     *
     */

    /*@NotNull*/
    @Override
    public Expression parseExpression() throws XPathException {
        if (inPredicate > 0) {
            return super.parseExpression();
        } else {
            return parseBinaryExpression(parsePathExpression(), 10);
        }
    }

    /**
     * Parse a basic step expression (without the predicates)
     *
     * @param firstInPattern true only if we are parsing the first step in a
     *                       RelativePathPattern in the XSLT Pattern syntax
     * @return the resulting subexpression
     * @throws net.sf.saxon.trans.XPathException
     *          if any error is encountered
     */

    /*@NotNull*/
    @Override
    protected Expression parseBasicStep(boolean firstInPattern) throws XPathException {
        if (inPredicate > 0) {
            return super.parseBasicStep(firstInPattern);
        } else {
            Token tok = t.currentToken;
            if (tok == Token.DOLLAR) {
                if (!firstInPattern) {
                    grumble("In an XSLT pattern, a variable reference is allowed only as the first step in a path");
                    return null;
                } else {
                    return super.parseBasicStep(firstInPattern);
                }
            }
            if (tok instanceof Token.NameToken) {
                if (t.peekAhead() == Token.LCURLY || t.peekAhead() == Token.HASH) {
                    grumble("Token " + t.peekAhead() + " not allowed here in an XSLT pattern");
                    return null;
                } else {
                    return super.parseBasicStep(firstInPattern);
                }
            } else if (t.currentToken instanceof Token.StringLiteral
                    || t.currentToken instanceof Token.NumericLiteral
                    || t.currentToken instanceof Token.StringTemplate
                    || t.currentToken instanceof Token.StringConstructor) {
                grumble("Token " + currentTokenDisplay() + " not allowed here in an XSLT pattern");
                return null;
            } else {
                return super.parseBasicStep(firstInPattern);
            }

        }
    }

    @Override
    protected boolean isAllowArgumentKeywords() {
        return super.isAllowArgumentKeywords() && inPredicate > 0;
    }

    protected void checkAllowedFunctionArgument(Expression arg) throws XPathException {
        if (inPredicate > 0) {
            return;
        }
        if (!(arg instanceof VariableReference || arg instanceof Literal)
                || arg instanceof FunctionLiteral
                || Literal.isEmptySequence(arg)) {
            grumble("Expression " + arg + " is not allowed as a function argument in an XSLT pattern");
        }
    }

    @Override
    protected void testPermittedAxis(int axis, String errorCode) throws XPathException {
        super.testPermittedAxis(axis, errorCode);
        if (inPredicate == 0) {
            if (!AxisInfo.isSubtreeAxis[axis]) {
                grumble("The " + AxisInfo.axisName[axis] + " axis is not allowed in a pattern");
            }
        }
    }

    /**
     * Parse an expression appearing within a predicate. This enables full XPath parsing, without
     * the normal rules that apply within an XSLT pattern
     *
     * @return the parsed expression that appears within the predicate
     * @throws net.sf.saxon.trans.XPathException if the predicate is invalid
     *
     */

    /*@NotNull*/
    @Override
    protected Expression parsePredicate() throws XPathException {
        ++inPredicate;
        Expression exp = parseExpression();
        --inPredicate;
        return exp;
    }

    /**
     * Parse a function call appearing within a pattern. Unless within a predicate, this
     * imposes the constraints on which function calls are allowed to appear in a pattern
     *
     * @return the expression that results from the parsing (usually a FunctionCall)
     * @throws net.sf.saxon.trans.XPathException if the function call is invalid
     *
     * @param prefixArgument left hand operand of arrow operator,
     *                    or null in the case of a conventional function call
     */

    /*@NotNull*/
    @Override
    public Expression parseFunctionCall(String fname, Expression prefixArgument) throws XPathException {
        // The XSLT rules allow root() but not root(.). Unfortunately the XPath parser expands root() to root(.)
        // So (in desperation) we count the arguments that are actually parsed
        argumentsParsed = 0;
        int offset = t.currentTokenStartOffset;
        Expression fn = super.parseFunctionCall(fname, prefixArgument);
        if (inPredicate <= 0 && !fn.isCallOn(SuperId.class) && !fn.isCallOn(KeyFn.class) &&
                !fn.isCallOn(Doc.class) && !(fn.isCallOn(Root_1.class) && argumentsParsed == 0)) {
            grumble("The " + fn + " function is not allowed in a pattern (unless in a predicate)", "XTSE0340", offset);
        }
        if (fn instanceof CurrentGroupCall) {
            grumble("The current-group() function cannot be used in a pattern",
                    "XTSE1060", offset);
            return new ErrorExpression();
        }
        if (fn instanceof CurrentGroupingKeyCall) {
            grumble("The current-grouping-key() function cannot be used in a pattern",
                    "XTSE1070", offset);
            return new ErrorExpression();
        }
        return fn;
    }

    private int argumentsParsed = 0;

    @Override
    public Expression parseFunctionArgument() throws XPathException {
        argumentsParsed++;
        if (inPredicate > 0) {
            return super.parseFunctionArgument();
        } else {
            Token tok = t.currentToken;
            if (tok == Token.DOLLAR) {
                int offset = t.currentTokenStartOffset;
                StructuredQName variableName = parseEQName();
                return resolveVariableReference(offset, variableName);
            } else if (tok instanceof Token.StringLiteral) {
                return parseStringLiteral(true);
            } else if (tok instanceof Token.NumericLiteral) {
                return parseNumericLiteral(true);
            } else if (tok == Token.MINUS || tok == Token.HASH || isKeyword("true") || isKeyword("false")) {
                if (languageVersion >= 40) {
                    AtomicValue val = parseConstant();
                    return Literal.makeLiteral(val);
                } else {
                    grumble("Prior to 4.0, a function argument in an XSLT pattern must be a variable reference or literal");
                    return null;
                }
            } else {
                grumble("A function argument in an XSLT pattern must be a variable reference or literal");
                return null;
            }
        }
    }

    @Override
    public Expression makeTracer(Expression exp, StructuredQName qName) {
        // Suppress tracing of pattern evaluation
        return exp;
    }
}
