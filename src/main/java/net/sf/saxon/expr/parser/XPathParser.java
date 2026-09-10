////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.expr.parser;


import net.sf.saxon.Configuration;
import net.sf.saxon.Version;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.flwor.Clause;
import net.sf.saxon.expr.flwor.LocalVariableBinding;
import net.sf.saxon.expr.instruct.*;
import net.sf.saxon.functions.*;
import net.sf.saxon.functions.hof.FunctionLiteral;
import net.sf.saxon.functions.hof.PartialApply;
import net.sf.saxon.functions.hof.UnresolvedXQueryFunctionItem;
import net.sf.saxon.functions.hof.UserFunctionReference;
import net.sf.saxon.functions.registry.BuiltInFunctionSet;
import net.sf.saxon.functions.registry.VendorFunctionSetHE;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.ma.arrays.ArrayFunctionSet;
import net.sf.saxon.ma.arrays.ArrayItemType;
import net.sf.saxon.ma.arrays.SimpleArrayItem;
import net.sf.saxon.ma.arrays.SquareArrayConstructor;
import net.sf.saxon.ma.map.*;
import net.sf.saxon.om.*;
import net.sf.saxon.pattern.*;
import net.sf.saxon.query.AnnotationList;
import net.sf.saxon.query.XQueryFunction;
import net.sf.saxon.query.XQueryParser;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.s9api.UnprefixedElementMatchingPolicy;
import net.sf.saxon.str.StringTool;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.style.ExpressionContext;
import net.sf.saxon.style.SourceBinding;
import net.sf.saxon.style.StylesheetPackage;
import net.sf.saxon.sxpath.IndependentContext;
import net.sf.saxon.sxpath.XPathVariable;
import net.sf.saxon.trans.*;
import net.sf.saxon.transpile.CSharp;
import net.sf.saxon.transpile.CSharpReplaceBody;
import net.sf.saxon.transpile.CSharpSimpleEnum;
import net.sf.saxon.tree.util.IndexedStack;
import net.sf.saxon.type.*;
import net.sf.saxon.value.*;
import net.sf.saxon.z.IntPredicateProxy;

import java.util.*;
import java.util.function.Predicate;


/**
 * Parser for XPath expressions and XSLT patterns.
 * <p>This code was originally inspired by James Clark's xt but has been totally rewritten (several times)</p>
 * <p>The base class handles parsing of XPath 2.0, XPath 3.0 and XPath 3.1 syntax (switched by a languageVersion variable).
 * Subclasses refine this to handle XQuery syntax (1.0, 3.0 and 3.1) and XQuery Update syntax.</p>
 *
 */


public class XPathParser {

    protected Tokenizer t;
    protected StaticContext env;
    protected IndexedStack<LocalBinding> rangeVariables = new IndexedStack<>();
    // The stack holds a list of range variables that are in scope.
    // Each entry on the stack is a Binding object containing details
    // of the variable.

    public IndexedStack<InlineFunctionDetails> inlineFunctionStack = new IndexedStack<>();
    protected QNameParser qNameParser;
    protected ParserExtension parserExtension = new ParserExtension();

    protected IntPredicateProxy charChecker;

    protected boolean allowXPath30Syntax = false;
    protected boolean allowXPath30XSLTExtensions = false;
    protected boolean allowXPath31Syntax = false;
    protected boolean allowXPath40Syntax = false;
    protected boolean allowSaxonExtensions = false;

    protected boolean scanOnly = false;
    // scanOnly is set to true while attributes in direct element constructors
    // are being processed. We need to parse enclosed expressions in the attribute
    // in order to find the end of the attribute value, but we don't yet know the
    // full namespace context at this stage.

    private boolean allowAbsentExpression = false;
    // allowAbsentExpression is a flag that indicates that it is acceptable
    // for the expression to be empty (that is, to consist solely of whitespace and
    // comments). The result of parsing such an expression is equivalent to the
    // result of parsing an empty sequence literal, "()"

    /*@Nullable*/
    protected CodeInjector codeInjector = null;
    private Accelerator accelerator = null;


    @CSharpSimpleEnum
    public enum ParsedLanguage {XPATH, XSLT_PATTERN, SEQUENCE_TYPE, XQUERY, EXTENDED_ITEM_TYPE}

    protected ParsedLanguage language = ParsedLanguage.XPATH; // know which language we are parsing, for diagnostics

    protected int languageVersion = 20;
    protected int catchDepth = 0;



    public static class InlineFunctionDetails {
        public IndexedStack<LocalBinding> outerVariables;    // Local variables defined in the immediate outer scope (the father scope)
        public List<LocalBinding> outerVariablesUsed; // Local variables from the outer scope that are actually used
        public List<UserFunctionParameter> implicitParams; // Parameters corresponding (1:1) with the above
    }

    public interface Accelerator {

        /**
         * Attempt fast parsing of an expression, provided it is sufficiently simple.
         *
         * @param t          the tokenizer
         * @param env        the static context
         * @param expression the string containing expression to be parsed
         * @param start      start position within the input string
         * @param finished   either EOF or RCURLY, indicating how parsing should end
         * @return either the parsed expression, or null if it is erroneous or too
         * complex to parse.
         */

        Expression parse(Tokenizer t, StaticContext env, String expression, int start, Predicate<Tokenizer> finished);
    }

    /**
     * Create an expression parser
     */

    public XPathParser(StaticContext env) {
        this.env = env;
    }



    public boolean isAllowXPath40Syntax() {
        return allowXPath40Syntax;
    }

    /**
     * Set a CodeInjector which can be used to modify or wrap expressions on the tree
     * as the expression is parsed and the tree is constructed. This is typically used
     * to add tracing code.
     *
     * @param injector the code injector to be used
     */

    public void setCodeInjector(/*@Nullable*/ CodeInjector injector) {
        this.codeInjector = injector;
    }

    /**
     * Set a CodeInjector which can be used to modify or wrap expressions on the tree
     * as the expression is parsed and the tree is constructed. This is typically used
     * to add tracing code.
     *
     * @return the code injector in use, if any; or null otherwise
     */

    /*@Nullable*/
    public CodeInjector getCodeInjector() {
        return codeInjector;
    }

    /**
     * Set an accelerator which can be used for fast parsing of special cases
     * @param accelerator a parsing accelerator
     */

    public void setAccelerator(Accelerator accelerator) {
        this.accelerator = accelerator;
    }

    /**
     * Get the tokenizer (the lexical analyzer)
     *
     * @return the tokenizer (the lexical analyzer)
     */

    public Tokenizer getTokenizer() {
        return t;
    }

    /**
     * Get the static context used by this expression parser
     *
     * @return the static context
     */

    public StaticContext getStaticContext() {
        return env;
    }

//    /**
//     * Set the default container for newly constructed expressions
//     *
//     * @param container the default container
//     */
//
//    public void setDefaultContainer(Container container) {
//        this.defaultContainer = container;
//    }
//
//    /**
//     * Get the default container for newly constructed expressions
//     *
//     * @return the default container
//     */
//
//    public Container getDefaultContainer() {
//        return defaultContainer;
//    }

    /**
     * Set a parser extension which can handle extensions to the XPath syntax, e.g. for
     * XQuery update extensions
     * @param extension a parser extension
     */

    public void setParserExtension(ParserExtension extension) {
        this.parserExtension = extension;
    }

    /**
     * Set the depth of nesting within try/catch
     *
     * @param depth the depth of nesting
     */

    public void setCatchDepth(int depth) {
        catchDepth = depth;
    }

    /**
     * Read the next token, catching any exception thrown by the tokenizer
     *
     * @throws XPathException if an invalid token is found
     */

    public void nextToken() throws XPathException {
        try {
            previousToken = t.currentToken;
            t.next();
        } catch (XPathException e) {
            grumble(e.getMessage());
        }
    }

    Token previousToken = Token.UNKNOWN;

    /**
     * Expect a given token; fail if the current token is different. Note that this method
     * does not read any tokens.
     *
     * @param token the expected token
     * @throws XPathException if the current token is not the expected
     *                        token
     */

    public void expect(Token token) throws XPathException {
        if (t.currentToken != token) {
            grumble((previousToken == Token.UNKNOWN ? "" : ("After " + previousToken + " ")) +
                            "expected " + token.toString() +
                            ", found " + t.currentToken.toString());
        }
    }

    /**
     * Skip past the current token, checking that it matches. Equivalent
     * to expect(token) followed by nextToken().
     * @param token the expected token
     * @throws XPathException if the current token is not the one expected
     */
    public void readToken(Token token) throws XPathException {
        expect(token);
        nextToken();
    }

    public String expectStringLiteral() throws XPathException {
        if (t.currentToken instanceof Token.StringLiteral) {
            return ((Token.StringLiteral) t.currentToken).getValue();
        } else {
            grumble((previousToken == Token.UNKNOWN ? "" : ("After " + previousToken + ", ")) +
                            "expected string literal, found " + t.currentToken.toString());
            return null;
        }
    }

    public String expectName() throws XPathException {
        if (!(t.currentToken instanceof Token.NameToken)) {
            grumble((previousToken == Token.UNKNOWN ? "" : ("After " + previousToken + ", ")) +
                            "expected a name, found " + t.currentToken.toString());
        }
        return t.currentName();
    }

    /**
     * Expect the current token to be a name (perhaps a keyword). Return that name, after moving on
     * to the next token.
     * @return the name at the (old) current position
     * @throws XPathException if the current token is not a name
     */
    public String readName() throws XPathException {
        String name = expectName();
        nextToken();
        return name;
    }

    /**
     * Indicate that the current token is expected to be a specific name token,
     * triggering an error if it is not.
     * @param keyword the expected name
     * @throws XPathException if the current token differs from the expected name
     */
    public void expectKeyword(String keyword) throws XPathException {
        if (!isKeyword(keyword)) {
            grumble("expected '" + keyword + "', found " + t.currentToken.toString());
        }
    }

    /**
     * Skip past an expected keyword, failing if the current token is not that keyword
     * @param keyword the expected keyword
     * @throws XPathException if the current token is not the expected keyword
     */
    public void readKeyword(String keyword) throws XPathException {
        expectKeyword(keyword);
        nextToken();
    }

    /**
     * Report a syntax error (a static error with error code XPST0003)
     *
     * @param message the error message
     * @throws XPathException always thrown: an exception containing the
     *                        supplied message
     */

    public void grumble(String message) throws XPathException {
        grumble(message, language == ParsedLanguage.XSLT_PATTERN ? "XTSE0340" : "XPST0003");
    }

    /**
     * Report a static error
     *
     * @param message   the error message
     * @param errorCode the error code
     * @throws XPathException always thrown: an exception containing the
     *                        supplied message
     */

    public void grumble(String message, String errorCode) throws XPathException {
        grumble(message, new StructuredQName("", NamespaceUri.ERR, errorCode), -1);
    }

    /**
     * Report a static error, with location information
     *
     * @param message   the error message
     * @param errorCode the error code
     * @param offset    the coded location of the error, or -1 if the location of the current token should be used
     * @throws XPathException always thrown: an exception containing the
     *                        supplied message
     */

    public void grumble(String message, String errorCode, int offset) throws XPathException {
        grumble(message, new StructuredQName("", NamespaceUri.ERR, errorCode), offset);
    }

    /**
     * Report a static error
     *
     * @param message   the error message
     * @param errorCode the error code
     * @param offset    the coded location of the error, or -1 if the location of the current token should be used
     * @throws XPathException always thrown: an exception containing the
     *                        supplied message
     */

    protected void grumble(String message, StructuredQName errorCode, int offset) throws XPathException {
        if (errorCode == null) {
            errorCode = new StructuredQName("err", NamespaceUri.ERR, "XPST0003");
        }
        String nearbyText = null;
        int line = -1;
        int column = -1;
        if (t != null) {
            nearbyText = t.recentText(-1);
            if (offset == -1) {
                line = t.getLineNumber();
                column = t.getColumnNumber();
            } else {
                line = t.getLineNumber(offset);
                column = t.getColumnNumber(offset);
            }
        }

        Location loc = makeNestedLocation(env.getContainingLocation(), line, column, nearbyText);

        XPathException err = new XPathException(message)
                .withLocation(loc)
                .asStaticError()
                .withErrorCode(errorCode);
        err.setIsSyntaxError("XPST0003".equals(errorCode.getLocalPart()));
        err.setHostLanguage(getLanguage());
        throw err;
    }

    protected void grumble(String message, StructuredQName errorCode) throws XPathException {
        grumble(message, errorCode, -1);
    }


    /**
     * Output a warning message
     *
     * @param message the text of the message
     * @param errorCode the error code associated with the warning
     */

    protected void warning(String message, String errorCode)  {
        if (!env.getConfiguration().getBooleanProperty(Feature.SUPPRESS_XPATH_WARNINGS)) {
            String s = t.recentText(-1);
            String prefix =
                    (message.startsWith("...") ? "near" : "in") +
                            ' ' + Err.wrap(s) + ":\n    ";
            env.issueWarning(prefix + message, errorCode, makeLocation());
        }
    }

    /**
     * Set the current language (XPath or XQuery, XSLT Pattern, or SequenceType)
     *
     * @param language one of the constants {@link ParsedLanguage#XPATH}, {@link ParsedLanguage#XQUERY},
     * {@link ParsedLanguage#XSLT_PATTERN}, {@link ParsedLanguage#SEQUENCE_TYPE} etc
     * @param version  The XPath or XQuery language version. For XQuery the value must be
     *                 10 (for "1.0"), 30 (for "3.0") or 31 (for "3.1"); for XPath it must be 20 (="2.0"),
     *                 30 (="3.0") or 31 (="3.1"). The value 305 is also recognized to mean XPath 3.0 plus
     *                 the extensions defined in XSLT 3.0
     */

    protected void setLanguage(ParsedLanguage language, int version) {
        if (version == 0) {
            version = 30; // default
        }
        if (version == 305) {
            version = 30;
            allowXPath30XSLTExtensions = true;
        }
        if (version == 40) {
            getStaticContext().getConfiguration().checkLicensedFeature(
                    Configuration.LicenseFeature.PROFESSIONAL_EDITION, "XPath 4.0 Syntax", -1);
        }
        switch (language) {
            case XPATH:
                if (!(version == 20 || version == 30 || version == 31 || version == 40)) {
                    throw new IllegalArgumentException("Unsupported language version " + version);
                }
                break;
            case XSLT_PATTERN:
            case SEQUENCE_TYPE:
            case EXTENDED_ITEM_TYPE:
                if (!(version == 20 || version == 30 || version == 31 || version == 40)) {
                    throw new IllegalArgumentException("Unsupported language version " + version);
                }
                break;
            case XQUERY:
                if (!(version == 10 || version == 30 || version == 31 || version == 40)) {
                    throw new IllegalArgumentException("Unsupported language version " + version);
                }
                break;
            default:
                throw new IllegalArgumentException("Unknown language " + language);
        }
        this.language = language;
        this.languageVersion = version;
        this.allowXPath30Syntax = languageVersion >= 30;
        this.allowXPath31Syntax = languageVersion >= 31;
        this.allowXPath40Syntax = languageVersion >= 40;

    }

    /**
     * Get the current language (XPath or XQuery)
     *
     * @return a string representation of the language being parsed, for use in error messages
     */

    protected String getLanguage() {
        switch (language) {
            case XPATH:
                return "XPath";
            case XSLT_PATTERN:
                return "XSLT Pattern";
            case SEQUENCE_TYPE:
                return "SequenceType";
            case XQUERY:
                return "XQuery";
            case EXTENDED_ITEM_TYPE:
                return "Extended ItemType";
            default:
                return "XPath";
        }
    }

    /**
     * Ask if XPath 3.1 is in use
     *
     * @return true if XPath 3.1 syntax (and therefore XQuery 3.1 syntax) is permitted
     */

    public boolean isAllowXPath31Syntax() {
        return allowXPath31Syntax;
    }

    /**
     * Set the QNameParser to be used while parsing
     *
     * @param qp the QNameParser
     */

    public void setQNameParser(QNameParser qp) {
        this.qNameParser = qp;
    }

    /**
     * Get the QNameParser to be used while parsing
     *
     * @return the QNameParser
     */

    public QNameParser getQNameParser() {
        return qNameParser;
    }

    /**
     * Display the current token in an error message
     *
     * @return the display representation of the token
     */
    /*@NotNull*/
    protected String currentTokenDisplay() {
        return t.currentToken.toString();
    }

    /**
     * Parse a string representing an expression. This will accept an XPath expression if called on an
     * ExpressionParser, or an XQuery expression if called on a QueryParser.
     *
     * @param expression the expression expressed as a String
     * @param start      offset within the string where parsing is to start
     * @param finished predicate that enables the tokenizer to decide whether it has reached
     *                 the end of the input. When this condition is satisfied the tokenizer
     *                 will return {@code Token#EOF} in response to a next() request.
     * @param env        the static context for the expression
     * @return an Expression object representing the result of parsing
     * @throws XPathException if the expression contains a syntax error
     */

    /*@NotNull*/
    public Expression parse(String expression, int start, Predicate<Tokenizer> finished, StaticContext env)
            throws XPathException {
        this.env = env;
        int languageVersion = env.getXPathVersion();
        if (languageVersion == 20 && language == ParsedLanguage.XQUERY) {
            languageVersion = 10;
        }
        setLanguage(language, languageVersion);

        Expression exp = null;
        int offset;
        if (accelerator != null &&
                env.getUnprefixedElementMatchingPolicy() == UnprefixedElementMatchingPolicy.DEFAULT_NAMESPACE &&
                (expression.length() - start < 30 || finished == Tokenizer.CLOSING_CURLY)) {
            // We need the tokenizer to be visible so that the caller can ask
            // about where the expression ended within the input string
            t = new Tokenizer();
            t.languageLevel = env.getXPathVersion();
            exp = accelerator.parse(t, env, expression, start, finished);
        }

        if (exp == null) {

            qNameParser = new QNameParser(env.getNamespaceResolver())
                .withAcceptEQName(allowXPath30Syntax)
                .withErrorOnBadSyntax(language == ParsedLanguage.XSLT_PATTERN ? "XTSE0340" : "XPST0003")
                .withErrorOnUnresolvedPrefix("XPST0081");

            charChecker = env.getConfiguration().getValidCharacterChecker();
            t = new Tokenizer();
            t.languageLevel = env.getXPathVersion();
            t.setFinishCondition(finished);
            allowXPath40Syntax =
                    t.allowSaxonExtensions =
                            env.getConfiguration().getBooleanProperty(Feature.ALLOW_SYNTAX_EXTENSIONS) || t.languageLevel == 40;
            offset = t.currentTokenStartOffset;
            customizeTokenizer(t);
            try {
                t.tokenize(expression, start, -1);
            } catch (XPathException err) {
                grumble(err.getMessage());
            }
            if (t.currentToken == Token.EOF) {
                if (allowAbsentExpression) {
                    Expression result = Literal.makeEmptySequence();
                    result.setRetainedStaticContext(env.makeRetainedStaticContext());
                    setLocation(result);
                    return result;
                } else {
                    grumble("The expression is empty");
                }
            }
            exp = parseExpression();
            if (!finished.test(t)) {
                if (t.currentToken == Token.EOF && finished.test(t)) {
                    grumble("Missing curly brace after expression in value template", "XTSE0350");
                } else {
                    grumble("Unexpected token " + currentTokenDisplay() + " beyond end of expression");
                }
            }
            setLocation(exp, offset);
        }
        exp.setRetainedStaticContextThoroughly(env.makeRetainedStaticContext());
        //exp.verifyParentPointers();
        return exp;
    }

    /**
     * Callback to tailor the tokenizer
     *
     * @param t the Tokenizer to be customized
     */

    protected void customizeTokenizer(Tokenizer t) {
        // do nothing
    }


    /**
     * Parse a string representing a sequence type
     *
     * @param input the string, which should conform to the XPath SequenceType
     *              production
     * @param env   the static context
     * @return a SequenceType object representing the type
     * @throws XPathException if any error is encountered
     */

    public SequenceType parseSequenceType(String input, /*@NotNull*/ StaticContext env) throws XPathException {
        this.env = env;
        setLanguage(ParsedLanguage.SEQUENCE_TYPE, env.getXPathVersion());
        if (qNameParser == null) {
            qNameParser = new QNameParser(env.getNamespaceResolver());
            if (languageVersion >= 30) {
                qNameParser = qNameParser.withAcceptEQName(true);
            }
        }
        language = ParsedLanguage.SEQUENCE_TYPE;
        t = new Tokenizer();
        t.languageLevel = languageVersion;
        allowXPath40Syntax =
                t.allowSaxonExtensions =
                        env.getConfiguration().getBooleanProperty(Feature.ALLOW_SYNTAX_EXTENSIONS) || t.languageLevel == 40;
        try {
            t.tokenize(input, 0, -1);
        } catch (XPathException err) {
            grumble(err.getMessage());
        }
        SequenceType req = parseSequenceType();
        if (t.currentToken != Token.EOF) {
            grumble("Unexpected token " + currentTokenDisplay() + " beyond end of SequenceType");
        }
        return req;
    }

    /**
     * Parse a string representing an extended item type: specifically, the content of itemType
     * or nodeTest attributes in an exported package. As well as regular itemType syntax, these
     * allow combined node tests separated with "|", "except", or "intersect" operators. Expressions
     * using these operators will always be parenthesized.
     *
     * @param input the string, which should conform to the XPath SequenceType
     *              production
     * @param env   the static context
     * @return a SequenceType object representing the type
     * @throws XPathException if any error is encountered
     */

    public ItemType parseExtendedItemType(String input, StaticContext env) throws XPathException {
        this.env = env;
        setLanguage(ParsedLanguage.EXTENDED_ITEM_TYPE, env.getXPathVersion());
        t = new Tokenizer();
        t.languageLevel = env.getXPathVersion();
        allowSaxonExtensions = t.allowSaxonExtensions = true;
        try {
            t.tokenize(input, 0, -1);
        } catch (XPathException err) {
            grumble(err.getMessage());
        }
        ItemType req = parseItemType();
        if (t.currentToken != Token.EOF) {
            grumble("Unexpected token " + currentTokenDisplay() + " beyond end of ItemType");
        }
        return req;
    }

    /**
     * Parse a string representing a sequence type with syntax extensions used in exported stylesheets.
     * Also allows the extensions permitted in saxon:as, e.g. tuple types, type aliases
     *
     * @param input the string, which should conform to the XPath SequenceType
     *              production
     * @param env   the static context
     * @return a SequenceType object representing the type
     * @throws XPathException if any error is encountered
     */

    public SequenceType parseExtendedSequenceType(String input, /*@NotNull*/ StaticContext env) throws XPathException {
        this.env = env;
        language = ParsedLanguage.EXTENDED_ITEM_TYPE;
        t = new Tokenizer();
        t.languageLevel = languageVersion = 40;
        allowSaxonExtensions = t.allowSaxonExtensions = true;
        allowXPath30Syntax = allowXPath31Syntax = allowXPath40Syntax = true;
        try {
            t.tokenize(input, 0, -1);
        } catch (XPathException err) {
            grumble(err.getMessage());
        }
        SequenceType req = parseSequenceType();
        if (t.currentToken != Token.EOF) {
            grumble("Unexpected token " + currentTokenDisplay() + " beyond end of SequenceType");
        }
        return req;
    }


    //////////////////////////////////////////////////////////////////////////////////
    //                     EXPRESSIONS                                              //
    //////////////////////////////////////////////////////////////////////////////////

    /**
     * Parse a top-level Expression:
     * ExprSingle ( ',' ExprSingle )*
     *
     * @return the Expression object that results from parsing
     * @throws XPathException if the expression contains a syntax error
     */

    /*@NotNull*/
    public Expression parseExpression() throws XPathException {
        int offset = t.currentTokenStartOffset;
        Expression exp = parseExprSingle();
        ArrayList<Expression> list = null;
        while (t.currentToken == Token.COMMA) {
            // An expression containing a comma often contains many, so we accumulate all the
            // subexpressions into a list before creating the Block expression which reduces it to an array
            if (list == null) {
                list = new ArrayList<>(10);
                list.add(exp);
            }
            nextToken();
            Expression next = parseExprSingle();
            setLocation(next);
            list.add(next);
        }
        if (list != null) {
            exp = Block.makeBlock(list);
            setLocation(exp, offset);
        }
        return exp;
    }

    /**
     * Parse an ExprSingle
     *
     * @return the resulting subexpression
     * @throws XPathException if any error is encountered
     */

    /*@NotNull*/
    public Expression parseExprSingle() throws XPathException {
        Expression e = parserExtension.parseExtendedExprSingle(this);
        if (e != null) {
            return e;
        }
        // Fast path for a single-token expression (often found in function arguments)
        Token peek = t.peekAhead();
        if (peek == Token.EOF || peek == Token.COMMA || peek == Token.RPAREN || peek == Token.RSQB) {
            Token tok = t.currentToken;
            if (tok instanceof Token.StringLiteral) {
                return parseStringLiteral(true);
            } else if (tok instanceof Token.NumericLiteral) {
                return parseNumericLiteral(true);
            } else if (tok instanceof Token.NameToken) {
                return parseBasicStep(true);
            } else if (tok == Token.DOT) {
                nextToken();
                Expression cie = new ContextItemExpression();
                setLocation(cie);
                return cie;
            } else if (tok == Token.DOT_DOT) {
                nextToken();
                Expression pne = new AxisExpression(AxisInfo.PARENT, null);
                setLocation(pne);
                return pne;
            }
        }

        if (t.currentToken instanceof Token.NameToken) {
            Token afterName = t.peekAhead();
            if (afterName == Token.DOLLAR) {
                 switch (t.currentName()) {
                     case "for":
                     case "let":
                         return parseFLWORExpression();
                     case "some":
                     case "every":
                         return parseQuantifiedExpression();
                 }
            } else if (afterName instanceof Token.NameToken) {
                if (isKeyword("for")) {
                    return parseFLWORExpression();
                } else if (isKeyword("switch") && isKeyword(afterName, "case")) {
                    return parseSwitchExpression();
                } 
            } else if (afterName == Token.LCURLY) {
                if (isKeyword("switch")) {
                    return parseSwitchExpression();
                } else if (isKeyword("try")) {
                    return parseTryCatchExpression();
                }
            } else if (afterName == Token.LPAREN) {
                if (isKeyword("if")) {
                    return parseIfExpression();
                } else if (isKeyword("switch")) {
                    return parseSwitchExpression();
                } else if (isKeyword("typeswitch")) {
                    return parseTypeswitchExpression();
                }
            }
        }
        return parseBinaryExpression(parseUnaryExpression(), 4);
    }

    

    /**
     * Parse a binary expression, using operator precedence parsing. This is used
     * to parse the part of the grammar consisting largely of binary operators
     * distinguished by precedence: from "or expressions" down to "unary expressions".
     * Algorithm for the mainstream binary operators is from Wikipedia article
     * on precedence parsing;  operator precedences are from the XQuery specification
     * appendix B.
     *
     * @param lhs           Left-hand side "basic expression"
     * @param minPrecedence the minimum precedence of an operator that is to be treated as not terminating the
     *                      current expression
     * @return the parsed expression
     * @throws XPathException if a static error is found
     */

    /*@NotNull*/
    public Expression parseBinaryExpression(Expression lhs, int minPrecedence) throws XPathException {
        while (true) {
            OperatorSymbol operator = getCurrentOperatorSymbol();
            if (operator == OperatorSymbol.NOT_AN_OPERATOR) {
                break;
            }
            int offset = t.currentTokenStartOffset;
            int prec = OperatorInfo.operatorPrecedence(operator);
            if (prec < minPrecedence) {
                break;
            }
            switch (operator) {
                case INSTANCE_OF:
                case TREAT_AS:
                    nextToken();
                    nextToken();
                    SequenceType seq = parseSequenceType();
                    lhs = makeSequenceTypeExpression(lhs, operator, seq);
                    setLocation(lhs, offset);
//                    if (operatorPrecedence(currentTokenAsOperator()) >= prec) {
//                        grumble("Left operand of '" + Token.tokens[t.currentToken] + "' needs parentheses");
//                    }
                    break;
                case CAST_AS:
                case CASTABLE_AS:
                    nextToken();
                    nextToken();
                    boolean leftParenFollows = t.peekAhead() == Token.LPAREN;
                    CastingTarget at;
                    if (allowXPath40Syntax &&
                            (t.currentToken == Token.LPAREN || (leftParenFollows && t.currentToken instanceof Token.NameToken))) {
                        ItemType type = parseItemType();
                        if (type instanceof CastingTarget) {
                            at = (CastingTarget)type;
                        } else {
                            grumble("Item type " + type + " cannot be used as the target of a cast/castable expression", "XPST0080");
                            return null;
                        }
                    } else {
                        String name = expectName();
                        if (scanOnly) {
                            at = BuiltInAtomicType.STRING;
                        } else {
                            StructuredQName sq = null;
                            try {
                                sq = qNameParser.parse(name, env.getDefaultElementNamespace());
                            } catch (XPathException e) {
                                grumble(e.getMessage(), e.getErrorCodeQName());
                                assert false;
                            }
                            ItemType alias = env.resolveTypeAlias(sq);
                            if (alias != null) {
                                if (alias instanceof CastingTarget) {
                                    at = (CastingTarget) alias;
                                } else {
                                    grumble("The type " + alias + " cannot be used as the target of a cast", "XPST0080");
                                    at = null;
                                }
                            } else {
                                at = getSimpleType(name);
                            }
                        }
                        nextToken();
                    }
                    if (at == BuiltInAtomicType.ANY_ATOMIC) {
                        grumble("No value is castable to xs:anyAtomicType", "XPST0080");
                    }
                    if (at == BuiltInAtomicType.NOTATION) {
                        grumble("No value is castable to xs:NOTATION", "XPST0080");
                    }

                    boolean allowEmpty = t.currentToken == Token.QMARK;
                    if (allowEmpty) {
                        nextToken();
                    }
                    lhs = makeSingleTypeExpression(lhs, operator, at, allowEmpty);
                    setLocation(lhs, offset);
                    if (getCurrentOperatorPrecedence() >= prec) {
                        grumble("Left operand of '" + t.currentToken + "' needs parentheses");
                    }
                    break;
                case FAT_ARROW:
                    lhs = parseArrowPostfix(lhs);
                    break;
                case MAPPING_ARROW:
                    checkLanguageVersion40();
                    lhs = parseMappingArrowPostfix(lhs);
                    break;
                default:
                    nextToken();
                    Expression rhs = parseUnaryExpression();
                    while (true) {
                        OperatorSymbol op2 = getCurrentOperatorSymbol();
                        if (op2 == OperatorSymbol.NOT_AN_OPERATOR) {
                            break;
                        }
                        int prec2 = OperatorInfo.operatorPrecedence(op2);
                        if (prec2 <= prec) {
                            break;
                        }
                        rhs = parseBinaryExpression(rhs, prec);
                    }
//                    if (getCurrentOperatorPrecedence() == prec && !allowMultipleOperators(operator)) {
//                        String tok = t.currentToken.toString();
//                        String message = "Left operand of '" + tok + "' needs parentheses";
//                        if (tok.equals("<") || tok.equals(">")) {
//                            // Example input: return <a>3</a><b>4</b> - bug 2659
//                            message += ". Or perhaps an XQuery element constructor appears where it is not allowed";
//                        }
//                        grumble(message);
//                    }
                    lhs = makeBinaryExpression(lhs, operator, rhs);
                    setLocation(lhs, offset);
                    break;
            }
        }
        return lhs;
    }

    private int getCurrentOperatorPrecedence() {
        return OperatorInfo.operatorPrecedence(getCurrentOperatorSymbol());
    }

    private OperatorSymbol getCurrentOperatorSymbol() {
        OperatorSymbol op = t.currentToken.getOperatorSymbol();
        if (op != OperatorSymbol.NOT_AN_OPERATOR) {
            return op;
        }
        if (isKeywordPair("instance", "of")) {
            return OperatorSymbol.INSTANCE_OF;
        } else if (isKeywordPair("cast", "as")) {
            return OperatorSymbol.CAST_AS;
        } else if (isKeywordPair("castable", "as")) {
            return OperatorSymbol.CASTABLE_AS;
        } else if (isKeywordPair("treat", "as")) {
            return OperatorSymbol.TREAT_AS;
        }
        return OperatorSymbol.NOT_AN_OPERATOR;
    }

    private OperatorSymbol readOperator() throws XPathException {
        OperatorSymbol op = currentOperator();
        if (op != OperatorSymbol.NOT_AN_OPERATOR) {
            nextToken();
        }
        return op;
    }

    private OperatorSymbol currentOperator() throws XPathException {
        OperatorSymbol op = t.currentToken.getOperatorSymbol();
        if (op == OperatorSymbol.NOT_AN_OPERATOR) {
            if (isKeywordPair("cast", "as")) {
                nextToken();
                return OperatorSymbol.CAST_AS;
            } else if (isKeywordPair("castable", "as")) {
                nextToken();
                return OperatorSymbol.CASTABLE_AS;
            } else if (isKeywordPair("treat", "as")) {
                nextToken();
                return OperatorSymbol.TREAT_AS;
            } else if (isKeywordPair("instance", "of")) {
                nextToken();
                return OperatorSymbol.INSTANCE_OF;
            }
        }
        return op;
    }


    private boolean allowMultipleOperators(OperatorSymbol op) {
        switch (op) {
            case FEQ:
            case FNE:
            case FLE:
            case FLT:
            case FGE:
            case FGT:
            case EQUALS:
            case NE:
            case LE:
            case LT:
            case GE:
            case GT:
            case IS:
            case PRECEDES:
            case FOLLOWS:
            case TO:
                return false;
            default:
                return true;
        }
    }


    /*@NotNull*/
    private Expression makeBinaryExpression(Expression lhs, OperatorSymbol operator, Expression rhs) throws XPathException {
        switch (operator) {
            case OR:
                return new OrExpression(lhs, rhs);
            case AND:
                return new AndExpression(lhs, rhs);
            case FEQ:
            case FNE:
            case FLE:
            case FLT:
            case FGE:
            case FGT:
                return new ValueComparison(lhs, operator, rhs);
            case EQUALS:
            case NE:
            case LE:
            case LT:
            case GE:
            case GT:
                return env.getConfiguration().getTypeChecker(env.isInBackwardsCompatibleMode()).makeGeneralComparison(lhs, operator, rhs);
            case IS:
            case PRECEDES:
            case FOLLOWS:
                return new IdentityComparison(lhs, operator, rhs);
            case TO:
                return new RangeExpression(lhs, rhs);
            case CONCAT: {
                if (!allowXPath30Syntax) {
                    grumble("Concatenation operator ('||') requires XPath 3.0 to be enabled");
                }
                RetainedStaticContext rsc = new RetainedStaticContext(env);
                Configuration config = env.getConfiguration();
                BuiltInFunctionSet lib= config.getXPathFunctionSet(env.getXPathVersion());

                if (lhs.isCallOn(Concat.class)) {
                    Expression[] args = ((SystemFunctionCall) lhs).getArguments();
                    Expression[] newArgs = new Expression[args.length + 1];
                    System.arraycopy(args, 0, newArgs, 0, args.length);
                    newArgs[args.length] = rhs;
                    SystemFunction concat = lib.makeFunction("concat", newArgs.length);
                    concat.setRetainedStaticContext(rsc);
                    return concat.makeFunctionCall(newArgs);
                } else {
                    SystemFunction concat = lib.makeFunction("concat", 2);
                    concat.setRetainedStaticContext(rsc);
                    Expression[] args = new Expression[]{lhs, rhs};
                    return concat.makeFunctionCall(args);
                }
            }
            case PLUS:
            case MINUS:
            case TIMES:
            case DIV:
            case IDIV:
            case MOD:
                return env.getConfiguration().getTypeChecker(env.isInBackwardsCompatibleMode()).makeArithmeticExpression(lhs, operator, rhs);
//            case TIMES:
//                return env.getConfiguration().getTypeChecker(env.isInBackwardsCompatibleMode()).makeArithmeticExpression(lhs, OperatorSymbol.TIMES, rhs);
//            case DIV:
//                return env.getConfiguration().getTypeChecker(env.isInBackwardsCompatibleMode()).makeArithmeticExpression(lhs, OperatorSymbol.DIV, rhs);
            case OTHERWISE:
                return makeOtherwiseExpression(lhs, rhs);
            case UNION:
            case INTERSECT:
            case EXCEPT:
                return new VennExpression(lhs, operator, rhs);
//            case OR_ELSE: {
//                // Compile ($x orElse $y) as (if ($x) then true() else boolean($y))
//                RetainedStaticContext rsc = new RetainedStaticContext(env);
//                rhs = SystemFunction.makeCall("boolean", rsc, rhs);
//                return Choose.makeConditional(lhs, Literal.makeLiteral(BooleanValue.TRUE), rhs);
//            }
//            case AND_ALSO: {
//                // Compile ($x andAlso $y) as (if ($x) then boolean($y) else false())
//                RetainedStaticContext rsc = new RetainedStaticContext(env);
//                rhs = SystemFunction.makeCall("boolean", rsc, rhs);
//                return Choose.makeConditional(lhs, rhs, Literal.makeLiteral(BooleanValue.FALSE));
//            }
            default:
                throw new IllegalArgumentException(operator.toString());
        }
    }

    /**
     * XPath 4.0 extension: A otherwise B, returns if (exists(A)) then A else B
     * @param lhs the A expression
     * @param rhs the B expression
     * @return a conditional expression with the correct semantics
     */
    private Expression makeOtherwiseExpression (Expression lhs, Expression rhs) throws XPathException {
        checkLanguageVersion40();
        LetExpression let = new LetExpression();
        let.setVariableQName(new StructuredQName("vv", NamespaceUri.ANONYMOUS, "n" + lhs.hashCode()));
        let.setSequence(lhs);
        let.setRequiredType(SequenceType.ANY_SEQUENCE);
        LocalVariableReference v1 = new LocalVariableReference(let.getVariableQName());
        v1.setBinding(let);
        let.addReference(v1, false);
        LocalVariableReference v2 = new LocalVariableReference(let.getVariableQName());
        v2.setBinding(let);
        let.addReference(v2, false);
        RetainedStaticContext rsc = new RetainedStaticContext(env);
        Expression[] conditions = new Expression[]{SystemFunction.makeCall(
                "exists", rsc, v1), Literal.makeLiteral(BooleanValue.TRUE, lhs)};
        Expression[] actions = new Expression[]{v2, rhs};
        let.setAction(new Choose(conditions, actions));
        return let;
    }

    private Expression makeSequenceTypeExpression(Expression lhs, OperatorSymbol operator, /*@NotNull*/ SequenceType type) {
        switch (operator) {
            case INSTANCE_OF:
                return new InstanceOfExpression(lhs, type);
            case TREAT_AS:
                return TreatExpression.make(lhs, type);
            default:
                throw new IllegalArgumentException();
        }

    }

    private Expression makeSingleTypeExpression(Expression lhs, OperatorSymbol operator, CastingTarget type, boolean allowEmpty)
            throws XPathException {
        if (type instanceof AtomicType && !(type == ErrorType.getInstance())) {
            switch (operator) {
                case CASTABLE_AS:
                    CastableExpression castable = new CastableExpression(lhs, (AtomicType) type, allowEmpty);
                    if (lhs instanceof StringLiteral) {
                        castable.setOperandIsStringLiteral(true);
                    }
                    return castable;

                case CAST_AS:
                    CastExpression cast = new CastExpression(lhs, (AtomicType) type, allowEmpty);
                    if (lhs instanceof StringLiteral) {
                        cast.setOperandIsStringLiteral(true);
                    }
                    return cast;

                default:
                    throw new IllegalArgumentException();
            }
        } else if (allowXPath30Syntax) {
            switch (operator) {
                case CASTABLE_AS:
                    if (type instanceof UnionType) {
                        NamespaceResolver resolver = env.getNamespaceResolver();
                        UnionCastableFunction ucf = new UnionCastableFunction((UnionType) type, resolver, allowEmpty);
                        return new StaticFunctionCall(ucf, new Expression[]{lhs});
                    } else if (type instanceof ListType) {
                        NamespaceResolver resolver = env.getNamespaceResolver();
                        ListCastableFunction lcf = new ListCastableFunction((ListType) type, resolver, allowEmpty);
                        return new StaticFunctionCall(lcf, new Expression[]{lhs});
                    }
                    break;
                case CAST_AS:
                    if (type instanceof UnionType) {
                        NamespaceResolver resolver = env.getNamespaceResolver();
                        UnionConstructorFunction ucf = new UnionConstructorFunction((UnionType) type, resolver, allowEmpty);
                        return new StaticFunctionCall(ucf, new Expression[]{lhs});
                    } else if (type instanceof ListType) {
                        NamespaceResolver resolver = env.getNamespaceResolver();
                        ListConstructorFunction lcf = new ListConstructorFunction((ListType) type, resolver, allowEmpty);
                        return new StaticFunctionCall(lcf, new Expression[]{lhs});
                    }
                    break;
                default:
                    throw new IllegalArgumentException();
            }
//            if (type == AnySimpleType.getInstance()) {
//                throw new XPathException("Cannot cast to xs:anySimpleType", "XPST0080");
//            } else {
                throw new XPathException("Cannot cast to " + type.getClass(), "XPST0051");
//            }
        } else {
            throw new XPathException("Casting to list or union types requires XPath 3.0 to be enabled", "XPST0051");
        }

    }

    /**
     * Parse a Typeswitch Expression.
     * This construct is XQuery-only, so the XPath version of this
     * method throws an error unconditionally
     *
     * @return the expression that results from the parsing
     * @throws XPathException if a static error is found
     */

    /*@NotNull*/
    protected Expression parseTypeswitchExpression() throws XPathException {
        grumble("typeswitch is not allowed in XPath");
        return new ErrorExpression();
    }


    /**
     * Parse a Switch Expression.
     * This construct is XQuery-only.
     * SwitchExpr ::= "switch" "(" Expr ")" SwitchCaseClause+ "default" "return" ExprSingle
     * SwitchCaseClause ::= ("case" ExprSingle)+ "return" ExprSingle
     *
     * @return the parsed expression
     * @throws XPathException in the event of a syntax error
     */

    /*@NotNull*/
    protected Expression parseSwitchExpression() throws XPathException {
        grumble("switch is not allowed in XPath");
        return new ErrorExpression();
    }

    /**
     * Parse a Validate Expression.
     * This construct is XQuery-only, so the XPath version of this
     * method throws an error unconditionally
     *
     * @return the parsed expression; except that this version of the method always
     * throws an exception
     * @throws XPathException if a static error is found
     */

    /*@NotNull*/
    protected Expression parseValidateExpression() throws XPathException {
        grumble("validate{} expressions are not allowed in XPath");
        return new ErrorExpression();
    }

    /**
     * Parse an update map|array Expression.
     * This construct is XQuery-only, so the XPath version of this
     * method throws an error unconditionally
     *
     * @return the parsed expression; except that this version of the method always
     * throws an exception
     * @throws XPathException if a static error is found
     */

    /*@NotNull*/
    protected Expression parseDeepUpdateExpression() throws XPathException {
        grumble("update map/array expressions are not allowed in XPath");
        return new ErrorExpression();
    }

    /**
     * Parse an Extension Expression
     * This construct is XQuery-only, so the XPath version of this
     * method throws an error unconditionally
     *
     * @return the parsed expression; except that this version of the method
     * always throws an exception
     * @throws XPathException if a static error is found
     */

    /*@NotNull*/
    protected Expression parseExtensionExpression() throws XPathException {
        grumble("extension expressions (#...#) are not allowed in XPath");
        return new ErrorExpression();
    }


    /**
     * Parse a try/catch Expression
     * This construct is XQuery-3.0 only, so the XPath version of this
     * method throws an error unconditionally
     *
     * @return the parsed expression; except that this version of the method
     * always throws an exception
     * @throws XPathException if a static error is found
     */

    /*@NotNull*/
    protected Expression parseTryCatchExpression() throws XPathException {
        grumble("try/catch expressions are not allowed in XPath");
        return new ErrorExpression();
    }

    /**
     * Parse a FOR or LET expression:
     * for 'member'? $x in expr (',' 'member'? $y in expr)* 'return' expr
     * let $x := expr (', $y := expr)* 'return' expr
     * This version of the method handles the subset of the FLWOR syntax allowed in XPath
     *
     * @return the resulting subexpression
     * @throws XPathException if any error is encountered
     */

    protected Expression parseFLWORExpression() throws XPathException {
        if (isKeyword("let") && !allowXPath30Syntax) {
            grumble("'let' is not permitted in XPath 2.0");
        }
        if (isKeyword("let")) {
            return parseLetExpression();
        } else {
            return parseForExpression(false);
        }
    }

    private Expression parseForExpression(boolean continuation) throws XPathException {
        ForQualifier iterand = ForQualifier.FOR_ITEM;
        Token second = t.peekAhead();
        if (isKeyword(second, "sliding") || isKeyword(second, "tumbling")) {
            grumble("sliding/tumbling windows can only be used in XQuery");
        }
        if (isKeyword(second, "member")) {
            iterand = ForQualifier.FOR_MEMBER;
        } else if (isKeyword(second, "key")) {
            iterand = ForQualifier.FOR_KEY;
        } else if (isKeyword(second, "value")) {
            iterand = ForQualifier.FOR_VALUE;
        }
        if (iterand != ForQualifier.FOR_ITEM && !allowXPath40Syntax) {
            grumble("'for member/key/value' requires XPath 4.0 to be enabled");
        }

        ForExpression result = new ForExpression();
        Assignation target = result;
        int offset = t.currentTokenStartOffset;
        setLocation(result, offset);
        int numberOfVariables = 0;
        if (continuation) {
            // we are currently positioned on a "," within a multi-variable for expression
            nextToken();
            if (allowXPath40Syntax) {
                if (tryKeyword("member")) {
                    iterand = ForQualifier.FOR_MEMBER;
                } else if (tryKeyword("key")) {
                    iterand = ForQualifier.FOR_KEY;
                } else if (tryKeyword("value")) {
                    iterand = ForQualifier.FOR_VALUE;
                } else {
                    iterand = ForQualifier.FOR_ITEM;
                }
            }
        } else {
            // we are currently positioned on the "for" or "for member/key/value" token
            nextToken();
            if (iterand != ForQualifier.FOR_ITEM) {
                nextToken();
            }
        }
        SequenceType requiredType = SequenceType.SINGLE_ITEM;
        if (iterand == ForQualifier.FOR_MEMBER || iterand == ForQualifier.FOR_VALUE) {
            requiredType = SequenceType.ANY_SEQUENCE;
        } else if (iterand == ForQualifier.FOR_KEY) {
            requiredType = SequenceType.SINGLE_ATOMIC;
        }

        final StructuredQName varQName = readVariableName();
        result.setVariableQName(varQName);
        String positionVar = null;

        if (allowXPath40Syntax) {
            requiredType = readOptionalAsClause(requiredType);
        }

        StructuredQName varQName2 = null;
        SequenceType requiredType2 = null;
        if (iterand == ForQualifier.FOR_KEY && isKeyword("value")) {
            nextToken();
            varQName2 = readVariableName();
            if (varQName2.equals(varQName)) {
                grumble("Range variables in 'for key/value' expression must have distinct names", "XQST0089");
            }
            requiredType2 = readOptionalAsClause(null);
        }

        LocalVariableBinding positionBinding = null;
        if (allowXPath40Syntax && isKeyword("at")) {
            nextToken();
            StructuredQName positionVarQName = readVariableName();
            if (positionVarQName.equals(varQName) || positionVarQName.equals(varQName2)) {
                grumble("Position variable in 'for' expression has the same name as the range variable", "XQST0089");
            }
            positionBinding = new LocalVariableBinding(positionVarQName, BuiltInAtomicType.INTEGER.one());
            result.setPositionVariable(positionBinding);
        }

        // process the "in" clause
        readKeyword("in");
        Expression collection = parseExprSingle();

        // "for member $m in $array" compiles to
        // "for $temp in array:members($array) let $m := $temp?value"

        // "for key $k value $v in $map" compiles to
        // "for $temp in map:pairs($map) let $k := $temp?key, $v := $temp?value"

        if (iterand != ForQualifier.FOR_ITEM) {
            if (iterand == ForQualifier.FOR_MEMBER) {
                collection = ArrayFunctionSet.getInstance(40).makeFunction("members", 1).makeFunctionCall(collection);
            } else {
                collection = MapFunctionSet.getInstance(40).makeFunction("pairs", 1).makeFunctionCall(collection);
            }
            result.setRequiredType(SequenceType.SINGLE_ITEM);
            result.setVariableQName(
                    new StructuredQName("vv", NamespaceUri.SAXON_GENERATED_VARIABLE, "fm" + result.hashCode()));
            result.setSequence(collection);
            declareRangeVariable(result);
            numberOfVariables++;
            if (positionVar != null) {
                declareRangeVariable(positionBinding);
                numberOfVariables++;
            }
            if (iterand == ForQualifier.FOR_MEMBER || iterand == ForQualifier.FOR_VALUE) {
                LetExpression temp = new LetExpression();
                setLocation(temp, offset);
                temp.setRequiredType(requiredType);
                temp.setVariableQName(varQName);
                LocalVariableReference tempRef = new LocalVariableReference(result);
                LookupExpression lookup = new LookupExpression(tempRef,
                                                               LookupExpression.Modifier.CONTENT,
                                                               new StringLiteral("value"));
                temp.setSequence(lookup);
                declareRangeVariable(temp);
                numberOfVariables++;
                result.setAction(temp);
                target = temp;
            } else { //if (iterand == ForExpression.Qualifier.FOR_KEY) {
                LetExpression temp = new LetExpression();
                setLocation(temp, offset);
                temp.setRequiredType(requiredType);
                temp.setVariableQName(varQName);
                LocalVariableReference tempRef = new LocalVariableReference(result);
                LookupExpression lookup = new LookupExpression(tempRef,
                                                               LookupExpression.Modifier.CONTENT,
                                                               new StringLiteral("key"));
                temp.setSequence(lookup);
                declareRangeVariable(temp);
                numberOfVariables++;
                result.setAction(temp);
                target = temp;
                if (varQName2 != null) {
                    LetExpression temp2 = new LetExpression();
                    setLocation(temp2, offset);
                    temp2.setRequiredType(requiredType2 == null ? SequenceType.ANY_SEQUENCE : requiredType2);
                    temp2.setVariableQName(varQName2);
                    LocalVariableReference tempRef2 = new LocalVariableReference(result);
                    LookupExpression lookup2 = new LookupExpression(tempRef2,
                                                                   LookupExpression.Modifier.CONTENT,
                                                                   new StringLiteral("value"));
                    temp2.setSequence(lookup2);
                    declareRangeVariable(temp2);
                    numberOfVariables++;
                    target.setAction(temp2);
                    target = temp2;
                }
            }
        } else {
            result.setRequiredType(requiredType);
            result.setSequence(collection);
            declareRangeVariable(result);
            numberOfVariables++;
            if (positionVar != null) {
                declareRangeVariable(positionBinding);
                numberOfVariables++;
            }
        }

        Expression action;
        if (tryKeyword("return")) {
            action = parseExprSingle();
        } else if (t.currentToken == Token.COMMA) {
            action = parseForExpression(true);
        } else if (allowXPath40Syntax && isKeyword("for")) {
            action = parseForExpression(false);
        } else if (allowXPath40Syntax && isKeyword("let")) {
            action = parseLetExpression();
        } else {
            grumble("In 'for' expression, expected ',', 'for', 'let', or 'return', but found " + t.currentToken);
            return null;
        }

        target.setAction(action);

        // undeclare all the range variables

        for (int i=0; i<numberOfVariables; i++) {
            undeclareRangeVariable();
        }

        return makeTracer(result, result.getVariableQName());
    }

    private Expression parseLetExpression() throws XPathException {
        int clauses = 0;
        int offset;
        Assignation first = null;
        Assignation previous = null;
        do {
            offset = t.currentTokenStartOffset;
            nextToken();
            StructuredQName var = readVariableName();
            
            SequenceType requiredType = SequenceType.ANY_SEQUENCE;
            if (allowXPath40Syntax) {
                requiredType = readOptionalAsClause(SequenceType.ANY_SEQUENCE);
            }

            // declare the range variable
            Assignation firstClause;
            Assignation lastClause;
            firstClause = lastClause = new LetExpression();
            firstClause.setRequiredType(requiredType);

            clauses++;
            setLocation(firstClause, offset);
            setLocation(lastClause, offset);
            lastClause.setVariableQName(var);


            // process the  ":=" clause
            readToken(Token.COLON_EQUALS);
            Expression collection = parseExprSingle();
            firstClause.setSequence(collection);
            declareRangeVariable(lastClause);
            if (previous == null) {
                first = firstClause;
            } else {
                previous.setAction(firstClause);
            }
            previous = lastClause;
        } while (t.currentToken == Token.COMMA || (allowXPath40Syntax && isKeyword("let")));

        // process the "return" expression (called the "action") - or in 4.0 a FOR expression
        if (!(allowXPath40Syntax && isKeyword("for"))) {
            readKeyword("return");
        }
        previous.setAction(parseExprSingle());

        // undeclare all the range variables

        for (int i = 0; i < clauses; i++) {
            undeclareRangeVariable();
        }
        return makeTracer(first, first.getVariableQName());
    }


    /**
     * Parse a quantified expression:
     * (some|every) $x in expr (',' $y in expr)* 'satisfies' expr
     *
     * @return the resulting subexpression
     * @throws XPathException if any error is encountered
     */

    private Expression parseQuantifiedExpression() throws XPathException {
        int clauses = 0;
        boolean isSome = isKeyword("some");
        QuantifiedExpression first = null;
        QuantifiedExpression previous = null;
        do {
            int offset = t.currentTokenStartOffset;
            nextToken();
            StructuredQName var = readVariableName();
            clauses++;

            // declare the range variable
            QuantifiedExpression v = new QuantifiedExpression();
            v.setVariableQName(var);
            v.setRequiredType(SequenceType.SINGLE_ITEM);
            v.setOperator(isSome ? QuantifiedExpression.Qualifier.SOME : QuantifiedExpression.Qualifier.EVERY);
            setLocation(v, offset);


            if (isKeyword("as") && language == ParsedLanguage.XQUERY) {
                // We use this path for quantified expressions in XQuery, which permit an "as" clause
                nextToken();
                SequenceType type = parseSequenceType();
                if (type.getCardinality() != StaticProperty.EXACTLY_ONE) {
                    warning("Occurrence indicator on singleton range variable has no effect", SaxonErrorCode.SXWN9039);
                    type = SequenceType.makeSequenceType(type.getPrimaryType(), StaticProperty.EXACTLY_ONE);
                }
                v.setRequiredType(type);
            }

            // process the "in" clause
            readKeyword("in");
            v.setSequence(parseExprSingle());
            declareRangeVariable(v);
            if (previous != null) {
                previous.setAction(v);
            } else {
                first = v;
            }
            previous = v;

        } while (t.currentToken == Token.COMMA);

        // process the "return/satisfies" expression (called the "action")
        readKeyword("satisfies");
        previous.setAction(parseExprSingle());


        // undeclare all the range variables

        for (int i = 0; i < clauses; i++) {
            undeclareRangeVariable();
        }
        return makeTracer(first, first.getVariableQName());

    }

    /**
     * Parse an IF expression:
     * if '(' expr ')' 'then' expr 'else' expr
     *
     * @return the resulting subexpression
     * @throws XPathException if any error is encountered
     */

    private Expression parseIfExpression() throws XPathException {
        int ifoffset = t.currentTokenStartOffset;
        nextToken();
        readToken(Token.LPAREN);
        Expression condition = parseExpression();
        readToken(Token.RPAREN);
        int thenoffset = t.currentTokenStartOffset;
        if (t.currentToken == Token.LCURLY) {
            checkLanguageVersion40();
            return parseBracedActions(condition);
        }
        readKeyword("then");
        Expression thenExp = makeTracer(parseExprSingle(), null);
        setLocation(thenExp, thenoffset);
        int elseoffset = t.currentTokenStartOffset;
        readKeyword("else");
        Expression elseExp = makeTracer(parseExprSingle(), null);
        setLocation(elseExp, elseoffset);
        Expression ifExp = Choose.makeConditional(condition, thenExp, elseExp);
        setLocation(ifExp, ifoffset);
        return makeTracer(ifExp, null);
    }

    private Expression parseBracedActions(Expression condition) throws XPathException {
        List<Expression> conditions = new ArrayList<>();
        List<Expression> actions = new ArrayList<>();
        conditions.add(condition);
        nextToken();
        Expression thenExp;
        if (t.currentToken == Token.RCURLY) {
            thenExp = Literal.makeEmptySequence();
        } else {
            thenExp = parseExpression();
            expect(Token.RCURLY);
        }
        actions.add(thenExp);
        nextToken();
        while (isKeyword("else")) {
            nextToken();
            if (tryKeyword("if")) {
                expect(Token.LPAREN);
                condition = parseExpression();
                nextToken();
                readToken(Token.RPAREN);
                readToken(Token.LCURLY);
                thenExp = parseExpression();
                readToken(Token.RCURLY);
                conditions.add(condition);
                actions.add(thenExp);
            } else {
                readToken(Token.LCURLY);
                Expression elseExp;
                if (t.currentToken == Token.RCURLY) {
                    elseExp = Literal.makeEmptySequence();
                } else {
                    elseExp = parseExpression();
                    expect(Token.RCURLY);
                }
                nextToken();
                conditions.add(Literal.makeLiteral(BooleanValue.TRUE));
                actions.add(elseExp);
                break;
            }
        }
        Choose result = new Choose(conditions.toArray(new Expression[]{}), actions.toArray(new Expression[]{}));
        setLocation(result);
        return result;
    }

    /**
     * Analyze a token whose expected value is the name of an atomic type,
     * or in XPath 3.0 a "plain" union type and return the object representing the atomic or union type.
     *
     * @param qname The lexical QName of the atomic type; alternatively, a Clark name
     * @return The atomic type
     * @throws XPathException if the QName is invalid or if no atomic type of that
     *                        name exists as a built-in type or a type in an imported schema
     */
    /*@NotNull*/
    private ItemType getPlainType(String qname) throws XPathException {
        if (scanOnly) {
            return BuiltInAtomicType.STRING;
        }
        StructuredQName sq;
        try {
            sq = qNameParser.parse(qname, env.getDefaultElementNamespace());
        } catch (XPathException e) {
            grumble(e.getMessage(), e.getErrorCodeQName());
            return null;
        }
        return getPlainType(sq);
    }

    public ItemType getPlainType(StructuredQName sq) throws XPathException {
        Configuration config = env.getConfiguration();
        NamespaceUri uri = sq.getNamespaceUri();
        if (uri.isEmpty()) {
            uri = env.getDefaultElementNamespace();
        }
        String local = sq.getLocalPart();
        String qname = sq.getDisplayName();

        boolean builtInNamespace = uri.equals(NamespaceUri.SCHEMA);

        if (builtInNamespace) {
            ItemType t = Type.getBuiltInItemType(uri, local);
            if (t == null) {
                grumble("Unknown atomic type " + qname, "XPST0051");
                assert false;
            }
            if (t instanceof BuiltInAtomicType) {
                checkAllowedType(env, (BuiltInAtomicType) t);
                return t;
            } else if (t.isPlainType()) {
                return t;
            } else {
                grumble("The type " + qname + " is not atomic", "XPST0051");
                assert false;
            }
//#if CSHARP==false
        } else if (uri.equals(NamespaceUri.JAVA_TYPE)) {
            Class<?> theClass;
            try {
                String className = JavaExternalObjectType.localNameToClassName(local);
                theClass = config.getClass(className, false);
            } catch (XPathException err) {
                grumble("Unknown Java class " + local, "XPST0051");
                return AnyItemType.getInstance();
            }
            //noinspection SynchronizationOnLocalVariableOrMethodParameter
            synchronized(config) {
                return JavaExternalObjectType.of(theClass);
            }
//#endif
        } else if (uri.equals(NamespaceUri.DOT_NET_TYPE)) {
            return Version.platform.getExternalObjectType(config, uri, local);
        } else {
            if (allowXPath40Syntax) {
                ItemType it = env.resolveTypeAlias(sq);
                if (it != null) {
                    return it;
                }
            }
            SchemaType st = config.getSchemaType(sq);
            if (st == null) {
                grumble("Unknown simple type " + qname, "XPST0051");
            } else if (st.isAtomicType()) {
                if (!env.isImportedSchema(uri)) {
                    grumble("Atomic type " + qname + " exists, but its schema definition has not been imported", "XPST0051");
                }
                return (AtomicType) st;
            } else if (st instanceof ItemType && ((ItemType) st).isPlainType() && allowXPath30Syntax) {
                if (!env.isImportedSchema(uri)) {
                    grumble("Type " + qname + " exists, but its schema definition has not been imported", "XPST0051");
                }
                return (ItemType) st;
            } else if (st.isComplexType()) {
                grumble("Type (" + qname + ") is a complex type", "XPST0051");
                return BuiltInAtomicType.ANY_ATOMIC;
            } else if (((SimpleType) st).isListType()) {
                grumble("Type (" + qname + ") is a list type", "XPST0051");
                return BuiltInAtomicType.ANY_ATOMIC;
            } else if (allowXPath30Syntax) {
                grumble("Type (" + qname + ") is a union type that cannot be used as an item type", "XPST0051");
                return BuiltInAtomicType.ANY_ATOMIC;
            } else {
                grumble("The union type (" + qname + ") cannot be used as an item type unless XPath 3.0 is enabled", "XPST0051");
                return BuiltInAtomicType.ANY_ATOMIC;
            }
        }
        grumble("Unknown atomic type " + qname, "XPST0051");
        return BuiltInAtomicType.ANY_ATOMIC;
    }

    private void checkAllowedType(StaticContext env, BuiltInAtomicType type) throws XPathException {
        String s = whyDisallowedType(env.getPackageData(), type);
        if (s != null) {
            grumble(s, "XPST0080");
        }
    }

    /**
     * Determine whether a given built-in type is disallowed in a given environment, and in the
     * case where it is disallowed, return a string explaining why
     *
     * @param pack the containing package
     * @param type the built-in type to be tested
     * @return null if the type is OK to be used; or a string explaining why not.
     */

    public static String whyDisallowedType(PackageData pack, BuiltInAtomicType type) {
        if (!type.isAllowedInXSD10() && pack.getConfiguration().getXsdVersion() == Configuration.XSD10) {
            return "The built-in atomic type " + type.getDisplayName() + " is not recognized unless XSD 1.1 is enabled";
        }
        return null;
    }


    /**
     * Analyze a token whose expected value is the name of a simple type: any type name
     * allowed as the operand of "cast" or "castable".
     *
     * @param qname The lexical QName of the atomic type; alternatively, a Clark name
     * @return The atomic type
     * @throws XPathException if the QName is invalid or if no atomic type of that
     *                        name exists as a built-in type or a type in an imported schema
     */
    /*@NotNull*/
    private CastingTarget getSimpleType(/*@NotNull*/ String qname) throws XPathException {
        if (scanOnly) {
            return BuiltInAtomicType.STRING;
        }
        StructuredQName sq = null;
        try {
            sq = qNameParser.parse(qname, env.getDefaultElementNamespace());
        } catch (XPathException e) {
            grumble(e.getMessage(), e.getErrorCodeQName());
            assert false;
        }
        NamespaceUri uri = sq.getNamespaceUri();
        String local = sq.getLocalPart();

        boolean builtInNamespace = uri.equals(NamespaceUri.SCHEMA);
        if (builtInNamespace) {
            SimpleType target = Type.getBuiltInSimpleType(uri, local);
            if (target == null) {
                grumble("Unknown simple type " + qname, allowXPath30Syntax ? "XQST0052" : "XPST0051");
            } else if (!(target instanceof CastingTarget)) {
                grumble("Unsuitable type for cast: " + target.getDescription(), "XPST0080");
            }
            assert target instanceof CastingTarget;
            CastingTarget t = (CastingTarget) target;
            if (t instanceof BuiltInAtomicType) {
                checkAllowedType(env, (BuiltInAtomicType) t);
            }
            return t;
        } else if (uri.equals(NamespaceUri.DOT_NET_TYPE)) {
            return (AtomicType) Version.platform.getExternalObjectType(env.getConfiguration(), uri, local);

        } else {

            SchemaType st = env.getConfiguration().getSchemaType(new StructuredQName("", uri, local));
            if (st == null) {
                if (allowXPath30Syntax) {
                    grumble("Unknown simple type " + qname, "XQST0052");
                } else {
                    grumble("Unknown simple type " + qname, "XPST0051");
                }
                return BuiltInAtomicType.ANY_ATOMIC;
            }
            if (allowXPath30Syntax) {
                // XPath 3.0
                if (!env.isImportedSchema(uri)) {
                    grumble("Simple type " + qname + " exists, but its target namespace has not been imported in the static context");
                }
                return (CastingTarget) st;

            } else {
                // XPath 2.0
                if (st.isAtomicType()) {
                    if (!env.isImportedSchema(uri)) {
                        grumble("Atomic type " + qname + " exists, but its target namespace has not been imported in the static context");
                    }
                    return (AtomicType) st;
                } else if (st.isComplexType()) {
                    grumble("Cannot cast to a complex type (" + qname + ")", "XPST0051");
                    return BuiltInAtomicType.ANY_ATOMIC;
                } else if (((SimpleType) st).isListType()) {
                    grumble("Casting to a list type (" + qname + ") requires XPath 3.0", "XPST0051");
                    return BuiltInAtomicType.ANY_ATOMIC;
                } else {
                    grumble("casting to a union type (" + qname + ") requires XPath 3.0", "XPST0051");
                    return BuiltInAtomicType.ANY_ATOMIC;
                }
            }
        }
    }

    /**
     * Parse the sequence type production.
     * The QName must be the name of a built-in schema-defined data type.
     *
     * @return the resulting subexpression
     * @throws XPathException if any error is encountered
     */

    public SequenceType parseSequenceType() throws XPathException {
        boolean disallowIndicator = isKeyword("empty-sequence");
        ItemType primaryType = parseItemType();
        if (disallowIndicator) {
            // No occurrence indicator allowed
            return SequenceType.makeSequenceType(primaryType, StaticProperty.EMPTY);
        }
        int occurrenceFlag = parseOccurrenceIndicator();
        return SequenceType.makeSequenceType(primaryType, occurrenceFlag);
    }

    public int parseOccurrenceIndicator() throws XPathException {
        int occurrenceFlag;
        Token tok = t.currentToken;
        if (tok == Token.STAR) {
            occurrenceFlag = StaticProperty.ALLOWS_ZERO_OR_MORE;
            nextToken();
        } else if (tok == Token.PLUS) {
            occurrenceFlag = StaticProperty.ALLOWS_ONE_OR_MORE;
            nextToken();
        } else if (tok == Token.QMARK) {
            occurrenceFlag = StaticProperty.ALLOWS_ZERO_OR_ONE;
            nextToken();
        } else {
            occurrenceFlag = StaticProperty.EXACTLY_ONE;
        }
        return occurrenceFlag;
    }

    /**
     * Parse an ItemType within a SequenceType
     *
     * @return the ItemType after parsing
     * @throws XPathException if a static error is found
     */

    /*@NotNull*/
    public ItemType parseItemType() throws XPathException {
        ItemType extended = parserExtension.parseExtendedItemType(this);
        return extended == null ? parseSimpleItemType() : extended;
    }

    private ItemType parseSimpleItemType() throws XPathException {
        ItemType primaryType;
        if (t.currentToken == Token.LPAREN) {
            primaryType = parseParenthesizedItemType();
            //nextToken();
        } else if (t.currentToken instanceof Token.NameToken) {
            String typeName = t.currentName();
            if (t.peekAhead() == Token.LPAREN) {
                return parseKeywordItemType();
            } else {
                primaryType = getPlainType(typeName);
                nextToken();
            }
        } else if (t.currentToken == Token.PERCENT) {
            AnnotationList annotations = parseAnnotationsList();
            if ((isKeyword("function") || isKeyword("fn")) && t.peekAhead() == Token.LPAREN) {
                primaryType = parseFunctionItemType(annotations);
            } else {
                grumble("Expected 'function(...)' to follow annotation assertions, found " + t.currentToken);
                return null;
            }
//        } else if (language == ParsedLanguage.EXTENDED_ITEM_TYPE && t.currentToken == Token.PREFIX) {
//            String tokv = t.currentTokenValue;
//            nextToken();
//            return makeNamespaceTest(Type.ELEMENT, tokv);
//        } else if (language == ParsedLanguage.EXTENDED_ITEM_TYPE && t.currentToken == Token.SUFFIX) {
//            nextToken();
//            expectName();
//            String tokv = t.currentTokenValue;
//            nextToken();
//            return makeLocalNameTest(Type.ELEMENT, tokv);
//        } else if (language == ParsedLanguage.EXTENDED_ITEM_TYPE && t.currentToken == Token.AT) {
//            nextToken();
//            if (t.currentToken == Token.PREFIX) {
//                String tokv = t.currentTokenValue;
//                nextToken();
//                return makeNamespaceTest(Type.ATTRIBUTE, tokv);
//            } else if (t.currentToken == Token.SUFFIX) {
//                nextToken();
//                expectName();
//                String tokv = t.currentTokenValue;
//                nextToken();
//                return makeLocalNameTest(Type.ATTRIBUTE, tokv);
//            } else {
//                grumble("Expected NodeTest after '@'");
//                return BuiltInAtomicType.ANY_ATOMIC;
//            }
        } else {
            grumble("Expected type name in SequenceType, found " + t.currentToken);
            return BuiltInAtomicType.ANY_ATOMIC;
        }
        return primaryType;
    }

    /**
     * Parse an item type designated in the form NAME(ARGS) where name is the
     * current token value.
     * @return the item type
     * @throws XPathException if invalid
     */
    private ItemType parseKeywordItemType() throws XPathException {
        String kindName = expectName();
        switch (kindName) {
            case "item":
                nextToken();
                readToken(Token.LPAREN);
                readToken(Token.RPAREN);
                return AnyItemType.getInstance();
            case "node":
            case "element":
            case "attribute":
            case "text":
            case "comment":
            case "processing-instruction":
            case "document-node":
            case "namespace-node":
            case "schema-element":
            case "schema-attribute":
                return parseKindTest();
            case "map":
                return parseMapItemType();
            case "array":
                return parseArrayItemType();
            case "function":
            case "fn":
                checkLanguageVersion30();
                AnnotationList annotations = AnnotationList.EMPTY;
                return parseFunctionItemType(annotations);
            case "record":
                return parseRecordTest(this);
            case "enum":
                return parseEnumType();
            case "empty-sequence":
                nextToken();
                readToken(Token.LPAREN);
                readToken(Token.RPAREN);
                return ErrorType.getInstance();
            case "type":
                checkLanguageVersion40();
                if (language != ParsedLanguage.XSLT_PATTERN) {
                    grumble("The syntax type(N) is allowed only in XSLT patterns, not in XPath expressions");
                }
                return parseTypePattern();
            default:
                grumble("Unknown item kind " + kindName);
                return null;

        }
    }

    /**
     * Parse a record type (Saxon 9.8 / XPath 4.0 extension).
     * Syntax: "record" "(" (name (":" sequenceType)?) ("," (name (":" sequenceType)?))* ")"
     */

    private ItemType parseRecordTest(XPathParser p) throws XPathException {
        // The initial "record" has been read
        checkLanguageVersion40();
        Tokenizer t = p.getTokenizer();
        nextToken();
        p.readToken(Token.LPAREN);
        List<String> fieldNames = new ArrayList<>(6);
        List<String> optionalFieldNames = new ArrayList<>(6);
        List<SequenceType> fieldTypes = new ArrayList<>(6);
        boolean extensible = false;
        RecordTest recordTest = new RecordTest();
        while (true) {
            String name;
            if (t.currentToken == Token.STAR) {
                extensible = true;
                p.nextToken();
                p.expect(Token.RPAREN);
                break;
            }
            if (t.currentToken instanceof Token.NameToken) {
                name = t.currentName();
                if (!NameChecker.isValidNCName(name)) {
                    p.grumble(Err.wrap(name) + " is not a valid NCName");
                }
            } else if (t.currentToken instanceof Token.StringLiteral) {
                name = expectStringLiteral();
            } else {
                p.grumble("Name of field in tuple must be either an NCName or a quoted string literal");
                name = "dummy";
            }
            if (fieldNames.contains(name)) {
                p.grumble("Duplicate field name (" + name + ")");
                name = "dummy";
            }
            fieldNames.add(name);
            p.nextToken();
            if (t.currentToken == Token.QMARK) {
                optionalFieldNames.add(name);
                p.nextToken();
            }
            SequenceType arg = SequenceType.ANY_SEQUENCE;
            if (tryKeyword("as")) {
                if (t.currentToken == Token.DOT_DOT) {
                    // self-reference
                    p.nextToken();
                    int occ = parseOccurrenceIndicator();
                    arg = SequenceType.makeSequenceType(new SelfReferenceRecordTest(recordTest), occ);
                    if (!Cardinality.allowsZero(occ) && !optionalFieldNames.contains(name)) {
                        throw new XPathException("A self-referencing field in a record type must be emptiable or optional", "XPST0140");
                    }
                } else {
                    arg = p.parseSequenceType();
                }
            }
            fieldTypes.add(arg);
            if (t.currentToken == Token.RPAREN) {
                break;
            } else if (t.currentToken == Token.COMMA) {
                p.nextToken();
            } else {
                p.grumble("Expected ',' or ')' after field in RecordTest, found '" +
                                  t.currentToken + '\'');
            }
        }
        p.nextToken();
        recordTest.setDetails(fieldNames, fieldTypes, optionalFieldNames, extensible);
        return recordTest;
    }

//    /**
//     * Parse a union type (Saxon 9.8 extension, no longer in XPath 4.0).
//     * Syntax: "union" "(" qname ("," qname)* ")"
//     * @return the item type
//     * @throws XPathException if a syntax error is found
//     */
//
//    public ItemType parseUnionType() throws XPathException {
//        // The initial "union(" has been read
//        checkLanguageVersion40();
//        nextToken();
//        List<ItemType> memberTypes = new ArrayList<>(6);
//
//        while (true) {
//            if (t.currentToken == Token.KEYWORD_LBRA && t.currentTokenValue.equals("enum")) {
//                EnumerationUnionType type = parseEnumType();
//                for (ItemType it : type.getAlternatives()) {
//                    memberTypes.add((SingletonEnumType) it);
//                }
//            } else {
//                expectName();
//                if (scanOnly) {
//                    memberTypes.add(BuiltInAtomicType.STRING);
//                } else {
//                    StructuredQName member = getQNameParser().parse(
//                            t.currentTokenValue, getStaticContext().getDefaultElementNamespace());
//                    ItemType type = getPlainType(member);
//                    if (type instanceof AtomicType) {
//                        memberTypes.add(type);
//                    } else if (type instanceof PlainType) {
//                        for (PlainType pt : ((UnionType) type).getPlainMemberTypes()) {
//                            if (pt instanceof AtomicType) {
//                                memberTypes.add(pt);
//                            } else {
//                                grumble("Union type " + type + " has a non-atomic member type " + pt);
//                            }
//                        }
//                    } else {
//                        grumble("Type " + t.currentTokenValue + " exists, but is not atomic");
//                    }
//                }
//                nextToken();
//            }
//            if (t.currentToken == Token.RPAREN) {
//                break;
//            } else if (t.currentToken == Token.COMMA) {
//                nextToken();
//            } else {
//                grumble("Expected ',' or ')' after member name in union type, found '" +
//                                  Token.tokens[t.currentToken] + '\'');
//            }
//        }
//        nextToken();
//        return new LocalUnionType(memberTypes);
//
//    }

    /**
     * Parse a type pattern (XSLT 4.0 extension).
     * Syntax: "type" "(" itemType ("|" itemType)* ")"
     *
     * @return the item type
     * @throws XPathException if a syntax error is found
     */

    public ItemType parseTypePattern() throws XPathException {
        // The initial "type(" has been read
        checkLanguageVersion40();
        nextToken();
        List<ItemType> memberTypes = new ArrayList<>(6);
        while (true) {
            memberTypes.add(parseItemType());
            if (t.currentToken == Token.VBAR) {
                nextToken();
            } else {
                break;
            }
        }
        readToken(Token.RPAREN);
        return ChoiceItemType.makeChoiceItemType(memberTypes);
    }

    /**
     * Parse an enum type (XPath 4.0 proposal).
     * Syntax: "enum" "(" StringLiteral ("," StringLiteral)* ")"
     * @return the item type
     * @throws XPathException if a syntax error is found
     */

    public EnumerationUnionType parseEnumType() throws XPathException {
        // The initial "enum" has been read
        checkLanguageVersion40();
        nextToken();
        readToken(Token.LPAREN);
        List<SingletonEnumType> singletonEnums = new ArrayList<>();

        while (true) {
            String s = expectStringLiteral();
            UnicodeString val = StringTool.fromCharSequence(s);
            singletonEnums.add(new SingletonEnumType(val));
            nextToken();
            if (t.currentToken == Token.RPAREN) {
                break;
            } else if (t.currentToken == Token.COMMA) {
                nextToken();
            } else {
                grumble("Expected ',' or ')' after string literal in enum type, found '" +
                                  t.currentToken + '\'');
            }
        }
        nextToken();

        return new EnumerationUnionType(singletonEnums);

    }

    /**
     * Parse the item type used for function items (for higher order functions)
     * Syntax (changed by WG decision on 2009-09-22):
     * function '(' '*' ') |
     * function '(' (SeqType (',' SeqType)*)? ')' 'as' SeqType
     * The "function(" has already been read
     *
     * @param annotations the list of annotation assertions for this function item type
     * @return the ItemType after parsing
     * @throws XPathException if a static error is found
     */

    /*@NotNull*/
    protected ItemType parseFunctionItemType(AnnotationList annotations) throws XPathException {
        nextToken();
        readToken(Token.LPAREN);
        List<SequenceType> argTypes = new ArrayList<>(3);
        SequenceType resultType;

        if (t.currentToken == Token.STAR) {
            // Allow both to be safe
            nextToken();
            readToken(Token.RPAREN);
            if (annotations.isEmpty()) {
                return AnyFunctionType.getInstance();
            } else {
                return new AnyFunctionTypeWithAssertions(annotations, getStaticContext().getConfiguration());
            }
        } else {
            while (t.currentToken != Token.RPAREN) {
                SequenceType arg = parseSequenceType();
                argTypes.add(arg);
                if (t.currentToken == Token.RPAREN) {
                    break;
                } else if (t.currentToken == Token.COMMA) {
                    nextToken();
                } else {
                    grumble("Expected ',' or ')' after function argument type, found '" +
                                      t.currentToken + '\'');
                }
            }
            nextToken();
            if (tryKeyword("as")) {
                resultType = parseSequenceType();
                SequenceType[] argArray = new SequenceType[argTypes.size()];
                argArray = argTypes.toArray(argArray);
                return new SpecificFunctionType(argArray, resultType, annotations);
            } else if (!argTypes.isEmpty()) {
                grumble("Result type must be given if an argument type is given: expected 'as (type)'");
                return null;
            } else {
                grumble("function() is no longer allowed for a general function type: must be function(*)");
                return null;
                // in the new syntax adopted on 2009-09-22, this case is an error
                // return AnyFunctionType.getInstance();
            }
        }
    }


    /**
     * Parse the item type used for maps (XSLT extension to XPath 3.0)
     * Syntax:
     * map '(' '*' ') |
     * map '(' ItemType ',' SeqType ')' 'as' SeqType
     * The "map(" has already been read
     * @return the item type of the map
     * @throws XPathException if a parsing error occurs or if the map syntax
     *                        is not available
     */

    /*@NotNull*/
    protected ItemType parseMapItemType() throws XPathException {
        checkMapExtensions();
        Tokenizer t = getTokenizer();
        nextToken();
        readToken(Token.LPAREN);
        if (t.currentToken == Token.STAR) {
            // Allow both to be safe
            nextToken();
            readToken(Token.RPAREN);
            return MapType.ANY_MAP_TYPE;
        } else {
            ItemType keyType = parseItemType();
            readToken(Token.COMMA);
            SequenceType valueType = parseSequenceType();
            readToken(Token.RPAREN);
            if (!(keyType instanceof PlainType)) {
                grumble("Key type of a map must be an atomic or pure union type: found " + keyType);
                return null;
            }
            return new MapType((PlainType) keyType, valueType);
        }
    }

    /**
     * Get the item type used for array items (XPath 3.1)
     * Syntax:
     *    array '(' '*' ') |
     *    array '(' SeqType ')'
     * The "array(" has already been read
     * @return the item type of the array
     * @throws XPathException if a parsing error occurs or if the array syntax
     *                        is not available
     */

    /*@NotNull*/
    protected ItemType parseArrayItemType() throws XPathException {
        checkLanguageVersion31();
        Tokenizer t = getTokenizer();
        nextToken();
        readToken(Token.LPAREN);
        if (t.currentToken == Token.STAR) {
            // Allow both to be safe
            nextToken();
            readToken(Token.RPAREN);
            return ArrayItemType.ANY_ARRAY_TYPE;
        } else {
            SequenceType memberType = parseSequenceType();
            readToken(Token.RPAREN);
            return new ArrayItemType(memberType);
        }
    }

    /**
     * Parse a parenthesized item type (allowed in XQuery 3.0 and XPath 3.0 only)
     *
     * @return the item type
     * @throws XPathException in the event of a syntax error (or if 3.0 is not enabled)
     */

    /*@NotNull*/
    private ItemType parseParenthesizedItemType() throws XPathException {
        if (!allowXPath30Syntax) {
            grumble("Parenthesized item types require 3.0 to be enabled");
        }
        nextToken();
        List<ItemType> alternatives = new ArrayList<>();
        alternatives.add(parseItemType());
        while (allowXPath40Syntax && t.currentToken == Token.VBAR) {
            nextToken();
            alternatives.add(parseItemType());
        }
        readToken(Token.RPAREN);
        if (alternatives.size() == 1) {
            return alternatives.get(0);
        } else {
            return ChoiceItemType.makeChoiceItemType(alternatives);
        }
    }


    /**
     * Parse a UnaryExpr:<br>
     * ('+'|'-')* ValueExpr
     * parsed as ('+'|'-')? UnaryExpr
     *
     * @return the resulting subexpression. On return, the current token must be the
     * token that follows the expression.
     * @throws XPathException if any error is encountered
     */

    /*@NotNull*/
    private Expression parseUnaryExpression() throws XPathException {
        Expression exp = null;
        if (t.currentToken == Token.MINUS) {
            nextToken();
            Expression operand = parseUnaryExpression();
            exp = makeUnaryExpression(OperatorSymbol.NEGATE, operand);
        } else if (t.currentToken == Token.PLUS) {
            nextToken();
            // Unary plus: can't ignore it completely, it might be a type error, or it might
            // force conversion to a number which would affect operations such as "=".
            Expression operand = parseUnaryExpression();
            exp = makeUnaryExpression(OperatorSymbol.PLUS, operand);
        } else if (t.currentToken instanceof Token.Pragma) {
            exp = parseExtensionExpression();
        } else if (isKeyword("validate")) {
            Token second = t.peekAhead();
            if (second == Token.LCURLY
                    || isKeyword(second, "strict")
                    || isKeyword(second, "lax")
                    || isKeyword(second, "type")) {
                exp = parseValidateExpression();
            }
        }
        if (exp == null) {
            exp = parseSimpleMappingExpression();
        }
        setLocation(exp);
        return exp;
    }

    private Expression makeUnaryExpression(OperatorSymbol operator, Expression operand) {
        if (Literal.isAtomic(operand)) {
            // very early evaluation of expressions like "-1", so they are treated as numeric literals
            AtomicValue val = (AtomicValue) ((Literal) operand).getGroundedValue();
            if (val instanceof NumericValue) {
                if (env.isInBackwardsCompatibleMode()) {
                    val = new DoubleValue(((NumericValue) val).getDoubleValue());
                }
                AtomicValue value = operator == OperatorSymbol.NEGATE ? ((NumericValue) val).negate() : (NumericValue) val;
                return Literal.makeLiteral(value);
            }
        }
        return env.getConfiguration().getTypeChecker(env.isInBackwardsCompatibleMode()).makeArithmeticExpression(
                Literal.makeLiteral(Int64Value.ZERO), operator, operand);
    }

    /**
     * Test whether the current token is one that can start a RelativePathExpression,
     * satisfying the special rule for disambiguation after a lone leading slash.
     *
     * @return true if the current token is to be interpreted as the start of a relative
     * path expression.
     */

    protected boolean atStartOfRelativePath() {
        Token tok = t.currentToken;
        return tok instanceof Token.NameToken
                || tok == Token.AT
                || tok == Token.STAR
                || tok == Token.DOT
                || tok == Token.DOT_DOT
                || tok == Token.LPAREN
                || tok == Token.DOLLAR
                || tok == Token.QMARK
                || tok == Token.QMARK_QMARK
                || tok == Token.LSQB
                || tok == Token.LCURLY
                || tok instanceof Token.Pragma
                || tok instanceof Token.StringLiteral
                || tok instanceof Token.NumericLiteral
                || tok instanceof Token.Wildcard
                || tok instanceof Token.StringTemplate
                ;
    }
    
    /**
     * Parse a PathExpresssion. This includes "true" path expressions such as A/B/C, and also
     * constructs that may start a path expression such as a variable reference $name or a
     * parenthesed expression (A|B). Numeric and string literals also come under this heading.
     *
     * @return the resulting subexpression
     * @throws XPathException if any error is encountered
     */

    /*@NotNull*/
    protected Expression parsePathExpression() throws XPathException {
        int offset = t.currentTokenStartOffset;
        if (t.currentToken == Token.SLASH) {
            nextToken();
            final RootExpression start = new RootExpression();
            setLocation(start);
            if (atStartOfRelativePath()) {
                final Expression path = parseRemainingPath(start);
                setLocation(path, offset);
                return path;
            } else {
                return start;
            }
        } else if (t.currentToken == Token.SLASH_SLASH) {
            nextToken();
            final RootExpression start2 = new RootExpression();
            setLocation(start2, offset);
            final AxisExpression axisExp = new AxisExpression(AxisInfo.DESCENDANT_OR_SELF, null);
            setLocation(axisExp, offset);
            final Expression slashExp = ExpressionTool.makePathExpression(start2, axisExp);
            setLocation(slashExp, offset);
            final Expression exp = parseRemainingPath(slashExp);
            setLocation(exp, offset);
            return exp;
        } else if (t.currentToken instanceof Token.NameToken && t.peekAhead() != Token.LPAREN) {
            String name = t.currentName();
            if (name.equals("true") || name.equals("false")) {
                warning("The expression is looking for a child element named '" + name +
                                "' - perhaps " + name + "() was intended? To avoid this warning, use child::" +
                                name + " or ./" + name + ".", SaxonErrorCode.SXWN9040);
            } else if (t.currentToken.getOperatorSymbol() != OperatorSymbol.NOT_AN_OPERATOR
                    && language != ParsedLanguage.XSLT_PATTERN
                    && (offset > 0 || t.peekAhead() != Token.EOF)) {
                warning("The keyword '" + name + "' in this context means 'child::" + name +
                                "'. If this was intended, use 'child::" + name + "' or './" + name + "' to avoid this warning.", SaxonErrorCode.SXWN9040);
            }
        }
        return parseRelativePath();

    }

    /**
     * Parse an XPath 3.0 simple mapping expression ("!" operator)
     *
     * @return the parsed expression
     * @throws XPathException in the event of a syntax error
     */

    protected Expression parseSimpleMappingExpression() throws XPathException {
        int offset = t.currentTokenStartOffset;
        Expression exp = parsePathExpression();
        while (t.currentToken == Token.BANG) {
            if (!allowXPath30Syntax) {
                grumble("XPath '!' operator requires XPath 3.0 to be enabled");
            }
            nextToken();
            Expression next = parsePathExpression();
            exp = new ForEach(exp, next);
            setLocation(exp, offset);
        }
        return exp;
    }

    /**
     * Parse a relative path (a sequence of steps). Called when the current token immediately
     * follows a separator (/ or //), or an implicit separator (XYZ is equivalent to ./XYZ)
     *
     * @return the resulting subexpression
     * @throws XPathException if any error is encountered
     */

    /*@NotNull*/
    protected Expression parseRelativePath() throws XPathException {

        int offset = t.currentTokenStartOffset;
        Expression exp = parseStepExpression(language == ParsedLanguage.XSLT_PATTERN);
        while (t.currentToken == Token.SLASH ||
                t.currentToken == Token.SLASH_SLASH) {
            Token op = t.currentToken;
            nextToken();
            Expression next = parseStepExpression(false);
            if (op == Token.SLASH) {

                //return new RawSlashExpression(start, step);
                exp = new HomogeneityChecker(new SlashExpression(exp, next));
            } else /* (op == Token.SLASH_SLASH)*/ {
                // add implicit descendant-or-self::node() step
                AxisExpression ae = new AxisExpression(AxisInfo.DESCENDANT_OR_SELF, null);
                setLocation(ae, offset);
                Expression one = ExpressionTool.makePathExpression(exp, ae);
                setLocation(one, offset);
                exp = ExpressionTool.makePathExpression(one, next);
                exp = new HomogeneityChecker(exp);
            }
            setLocation(exp, offset);
        }
        return exp;
    }

    /**
     * Parse the remaining steps of an absolute path expression (one starting in "/" or "//"). Note that the
     * token immediately after the "/" or "//" has already been read, and in the case of "/", it has been confirmed
     * that we have a path expression starting with "/" rather than a standalone "/" expression.
     *
     * @param start the initial implicit expression: root() in the case of "/", root()/descendant-or-self::node in
     *              the case of "//"
     * @return the completed path expression
     * @throws XPathException if a static error is found
     */
    /*@NotNull*/
    protected Expression parseRemainingPath(Expression start) throws XPathException {
        int offset = t.currentTokenStartOffset;
        Expression exp = start;
        Token op = Token.SLASH;
        while (true) {
            Expression next = parseStepExpression(false);
            if (op == Token.SLASH) {

                //return new RawSlashExpression(start, step);
                exp = new HomogeneityChecker(new SlashExpression(exp, next));
            } else if (op == Token.SLASH_SLASH) {
                // add implicit descendant-or-self::node() step
                AxisExpression descOrSelf = new AxisExpression(AxisInfo.DESCENDANT_OR_SELF, null);
                setLocation(descOrSelf);
                Expression step = ExpressionTool.makePathExpression(descOrSelf, next);
                setLocation(step);
                exp = ExpressionTool.makePathExpression(exp, step);
                exp = new HomogeneityChecker(exp);
            } else /*if (op == Token.BANG)*/ {
                if (!allowXPath30Syntax) {
                    grumble("XPath '!' operator requires XPath 3.0 to be enabled");
                }
                exp = new ForEach(exp, next);
            }
            setLocation(exp, offset);
            op = t.currentToken;
            if (op != Token.SLASH && op != Token.SLASH_SLASH && op != Token.BANG) {
                break;
            }
            nextToken();
        }
        return exp;
    }


    /**
     * Parse a step (including an optional sequence of predicates)
     *
     * @param firstInPattern true only if we are parsing the first step in a
     *                       RelativePathPattern in the XSLT Pattern syntax
     * @return the resulting subexpression
     * @throws XPathException if any error is encountered
     */

    /*@NotNull*/
    protected Expression parseStepExpression(boolean firstInPattern) throws XPathException {
        Expression step = parseBasicStep(firstInPattern);

        // When the filter is applied to an Axis step, the nodes are considered in
        // axis order. In all other cases they are considered in document order
        boolean reverse = (step instanceof AxisExpression) &&
                !AxisInfo.isForwards[((AxisExpression) step).getAxis()];

        while (true) {
            if (t.currentToken == Token.LSQB) {
                step = parsePredicate(step);
            } else if (t.currentToken == Token.LPAREN) {
                // dynamic function call (XQuery 3.0/XPath 3.0 syntax)
                step = parseDynamicFunctionCall(step, null);
                setLocation(step);
            } else if (t.currentToken == Token.QMARK || t.currentToken == Token.QMARK_QMARK) {
                step = parseLookup(step);
                setLocation(step);
            } else if (t.currentToken == Token.QMARK_LSQB) {
                step = parseFilterExprAM(step);
                setLocation(step);
            } else {
                break;
            }
        }
        if (reverse) {
            // An AxisExpression such as preceding-sibling::x delivers nodes in axis
            // order, so that positional predicate like preceding-sibling::x[1] work
            // correctly. To satisfy the XPath semantics we turn preceding-sibling::x
            // into reverse(preceding-sibling::x), and preceding-sibling::x[3] into
            // reverse(preceding-sibling::x[3]). The call on reverse() will be eliminated
            // later in the case where the predicate selects a singleton.
            RetainedStaticContext rsc = env.makeRetainedStaticContext();
            step = SystemFunction.makeCall("reverse", rsc, step);
        }
        return step;
    }

    protected Expression parsePredicate(Expression step) throws XPathException {
        nextToken();
        Expression predicate = parsePredicate();
        if (Literal.isConstantZero(predicate)) {
            warning("Positions are numbered from one; the predicate [0] selects nothing", SaxonErrorCode.SXWN9046);
        }
        readToken(Token.RSQB);
        step = new FilterExpression(step, predicate);
        setLocation(step);
        return step;
    }


    protected Expression parseFilterExprAM(Expression step) throws XPathException {
        checkLanguageVersion40();
        nextToken();
        Expression predicate = parsePredicate();
        readToken(Token.RSQB);
        step = new FilterExpressionAM(step, predicate);
        setLocation(step);
        return step;
    }

    /**
     * Parse an XPath 3.1 arrow operator ("=&gt;")
     * @param lhs the expression on the left of the arrow operator
     * @return the expression that results from the parsing
     * @throws XPathException if the syntax is wrong
     */

    /*@NotNull*/
    protected Expression parseArrowPostfix(Expression lhs) throws XPathException {
        checkLanguageVersion31();
        readToken(Token.FAT_ARROW);
        Token token = t.currentToken;
        if (token instanceof Token.NameToken) {
            if (t.peekAhead() == Token.LCURLY) {
                if (tryKeyword("function") || tryKeyword("fn")) {
                    Expression fn = parseFocusFunction(AnnotationList.EMPTY);
                    expect(Token.LPAREN);
                    return parseDynamicFunctionCall(fn, lhs);
                } else {
                    grumble("Unexpected " + token + " after '=>'");
                    return null;
                }
            }
            return parseFunctionCall(t.currentName(), lhs);
        } else if (token == Token.DOLLAR) {
            int offset = t.currentTokenStartOffset;
            StructuredQName varName = parseVariableName();
            Expression var = resolveVariableReference(offset, varName);
            expect(Token.LPAREN);
            return parseDynamicFunctionCall(var, lhs);
        } else if (token == Token.LPAREN) {
            Expression var = parseParenthesizedExpression();
            expect(Token.LPAREN);
            return parseDynamicFunctionCall(var, lhs);
//        } else if (token == Token.KEYWORD_LBRA && t.currentTokenValue.equals("function")) {
//            Expression fn = parseInlineFunction(AnnotationList.EMPTY);
//            expect(Token.LPAREN);
//            return parseDynamicFunctionCall(fn, lhs);
//        } else if (token == Token.KEYWORD_CURLY && (t.currentTokenValue.equals("function") || t.currentTokenValue.equals("fn"))) {
//            Expression fn = parseFocusFunction(AnnotationList.EMPTY);
//            expect(Token.LPAREN);
//            return parseDynamicFunctionCall(fn, lhs);
        } else {
            grumble("Unexpected " + token + " after '=>'");
            return null;
        }
    }

    /**
     * Parse an XPath 4.0 mapping arrow operator ("=!&gt;")
     *
     * @param lhs the expression on the left of the arrow operator
     * @return the expression that results from the parsing
     * @throws XPathException if the syntax is wrong
     */

    /*@NotNull*/
    protected Expression parseMappingArrowPostfix(Expression lhs) throws XPathException {
        checkLanguageVersion40();
        nextToken();

        ForExpression forExpr = new ForExpression();
        forExpr.setSequence(lhs);
        StructuredQName varName = new StructuredQName("vv", NamespaceUri.SAXON_GENERATED_VARIABLE, ""+lhs.hashCode());
        forExpr.setVariableQName(varName);
        VariableReference varRef = new LocalVariableReference(forExpr);

        Token token = t.currentToken;
        Expression rhs;
        if (token instanceof Token.NameToken) {
            if (t.peekAhead() == Token.LCURLY) {
                if (tryKeyword("function") || tryKeyword("fn")) {
                    Expression fn = parseFocusFunction(AnnotationList.EMPTY);
                    expect(Token.LPAREN);
                    rhs = parseDynamicFunctionCall(fn, varRef);
                } else {
                    grumble("Unexpected " + token + " after '=!>'");
                    return null;
                }
            } else {
                String fname = t.currentName();
                if (isKeyword("function") || isKeyword("fn")) {
                    Expression fn;
                    if (t.peekAhead() == Token.LPAREN) {
                        fn = parseInlineFunction(AnnotationList.EMPTY);
                    } else {
                        grumble("Unexpected " + t.peekAhead() + " after 'function'");
                        return null;
                    }
                    rhs = parseDynamicFunctionCall(fn, varRef);
                } else {
                    rhs = parseFunctionCall(fname, varRef);
                }
            }
        } else if (token == Token.DOLLAR) {
            int offset = t.currentTokenStartOffset;
            StructuredQName variableName = parseVariableName();
            Expression var = resolveVariableReference(offset, variableName);
            expect(Token.LPAREN);
            rhs = parseDynamicFunctionCall(var, varRef);
        } else if (token == Token.LPAREN) {
            Expression var = parseParenthesizedExpression();
            expect(Token.LPAREN);
            rhs = parseDynamicFunctionCall(var, varRef);
//        } else if (token == Token.KEYWORD_CURLY && (t.currentTokenValue.equals("function") || t.currentTokenValue.equals("fn"))) {
//            Expression fn = parseFocusFunction(AnnotationList.EMPTY);
//            expect(Token.LPAREN);
//            rhs = parseDynamicFunctionCall(fn, varRef);
        } else {
            grumble("Unexpected " + token + " after '=!>'");
            return null;
        }
        forExpr.setAction(rhs);
        return forExpr;
    }


    /**
     * Parse the expression within a predicate. A separate method so it can be overridden
     *
     * @return the expression within the predicate
     * @throws XPathException if a static error is found
     */

    /*@NotNull*/
    protected Expression parsePredicate() throws XPathException {
        return parseExpression();
    }

    protected boolean isReservedInQuery(NamespaceUri uri) {
        return NamespaceUri.isReservedInQuery31(uri);
    }

    protected boolean isReservedUnprefixedFunctionName(String fName) {
        // TODO: this is the 4.0 list. Make it version-dependent.
        switch (fName) {
            case "attribute":
            case "comment":
            case "document-node":
            case "element":
            case "namespace-node":
            case "node":
            case "schema-attribute":
            case "schema-element":
            case "processing-instruction":
            case "text":
            case "fn":
            case "function":
            case "if":
            case "switch":
            case "typeswitch":
                return true;
            default:
                return false;
        }
    }

    /**
     * Parse a basic step expression (without the predicates).
     * Specifically, we are looking for either a PrimaryExpr, that is:
     * Literal
     * | VarRef
     * | ParenthesizedExpr
     * | ContextValueRef
     * | FunctionCall
     * | OrderedExpr
     * | UnorderedExpr
     * | NodeConstructor
     * | FunctionItemExpr
     * | MapConstructor
     * | ArrayConstructor
     * | StringTemplate
     * | StringConstructor
     * | UnaryLookup
     * or a full axis step (axis::nodetest) or an abbreviated axis step (.., @nodetest, or
     * SimpleNodeTest, which is a KindTest or NameTest)
     *
     * @param firstInPattern true only if we are parsing the first step in a
     *                       RelativePathPattern in the XSLT Pattern syntax
     * @return the resulting subexpression
     * @throws XPathException if any error is encountered
     */

    /*@NotNull*/
    protected Expression parseBasicStep(boolean firstInPattern) throws XPathException {
        Token token = t.currentToken;
        if (token instanceof Token.StringLiteral) {
            return parseStringLiteral(true);
        } else if (token instanceof Token.NumericLiteral) {
            return parseNumericLiteral(true);
        } else if (token instanceof Token.NameToken) {
            if (t.peekAhead() == Token.LPAREN) {
                String name = t.currentName();
                switch (name) {
                    case "namespace-node":
                        if (!allowXPath30Syntax) {
                            return parseFunctionCall(name, null);
                        };
                        CSharp.emitCode("goto case \"node\";");
                    case "node":
                    case "schema-element":
                    case "processing-instruction":
                    case "document-node":
                    case "comment":
                    case "element":
                    case "text":
                        NodeTest nodeTest = parseNodeTest(Type.ELEMENT);
                        AxisExpression ae = new AxisExpression(AxisInfo.CHILD, nodeTest);
                        setLocation(ae);
                        return ae;
                    case "attribute":
                    case "schema-attribute":
                        NodeTest attNodeTest = parseNodeTest(Type.ATTRIBUTE);
                        AxisExpression ae2 = new AxisExpression(AxisInfo.ATTRIBUTE, attNodeTest);
                        setLocation(ae2);
                        return ae2;
                    case "function":
                    case "fn":
                        return parseInlineFunction(AnnotationList.EMPTY);
                    default:
                        return parseFunctionCall(name, null);

                }
            } else if (t.peekAhead() == Token.LCURLY) {
                String name = t.currentName();
                switch (name) {
                    case "map":
                        nextToken();
                        expect(Token.LCURLY);
                        return parseMapExpression();
                    case "array":
                        nextToken();
                        expect(Token.LCURLY);
                        return parseArrayCurlyConstructor();
                    case "function":
                    case "fn":
                        nextToken();
                        return parseFocusFunction(null);
                    case "ordered":
                    case "unordered":
                        // ignore these wrappers
                        nextToken();
                        readToken(Token.LCURLY);
                        Expression wrapped = parseExpression();
                        readToken(Token.RCURLY);
                        return wrapped;
                    case "document":
                    case "element":
                    case "attribute":
                    case "namespace":
                    case "text":
                    case "comment":
                    case "processing-instruction":
                        return parseComputedNodeConstructor();
                    default:
                        grumble("Unrecognized construct " + name + "{...}");
                        return null;
                        
                }

            } else if (t.peekAhead() == Token.HASH) {
                return parseNamedFunctionReference();
            } else if (t.peekAhead() == Token.COLON_COLON) {
                int axis;
                try {
                    axis = AxisInfo.getAxisNumber(t.currentName());
                } catch (XPathException err) {
                    grumble(err.getMessage());
                    axis = AxisInfo.CHILD; // error recovery
                }
                testPermittedAxis(axis, "XPST0003");
                short principalNodeType = AxisInfo.principalNodeType[axis];
                nextToken();
                readToken(Token.COLON_COLON);
                NodeTest nodeTest = parseNodeTest(principalNodeType);
                AxisExpression ae = new AxisExpression(axis, nodeTest);
                setLocation(ae);
                return ae;

            } else if (t.peekAhead() instanceof Token.NameToken) {
                String name = t.currentName();
                String next = ((Token.NameToken)t.peekAhead()).getValue();
                switch (name) {
                    case "element":
                    case "attribute":
                    case "processing-instruction":
                    case "namespace":
                        return parseComputedNodeConstructor();
                    case "update":
                        if (next.equals("map") || next.equals("array")) {
                            return parseDeepUpdateExpression();
                        }
                        break;
                }
            }
            AxisExpression resultExpr = new AxisExpression(AxisInfo.CHILD, parseNodeTest(Type.ELEMENT));
            setLocation(resultExpr);
            return resultExpr;

        } else if (token instanceof Token.Wildcard) {
            NodeTest test = getNodeTestForWildcard((Token.Wildcard) token, Type.ELEMENT);
            AxisExpression ae = new AxisExpression(AxisInfo.CHILD, test);
            setLocation(ae);
            nextToken();
            return ae;

        } else if (token == Token.DOT) {
            nextToken();
            Expression cie = new ContextItemExpression();
            setLocation(cie);
            return cie;

        } else if (token == Token.DOLLAR) {
            int offset = t.currentTokenStartOffset;
            StructuredQName variableName = parseVariableName();
            return resolveVariableReference(offset, variableName);

        } else if (token == Token.DOT_DOT) {
            nextToken();
            Expression pne = new AxisExpression(AxisInfo.PARENT, null);
            setLocation(pne);
            return pne;

        } else if (token == Token.STAR) {
            nextToken();
            AxisExpression ae = new AxisExpression(AxisInfo.CHILD, NodeKindTest.ELEMENT);
            setLocation(ae);
            return ae;

        } else if (token == Token.LPAREN) {
            return parseParenthesizedExpression();

        } else if (token == Token.LSQB) {
            return parseArraySquareConstructor();

        } else if (token == Token.QMARK || token == Token.QMARK_QMARK) {
            return parseLookup(new ContextItemExpression());

        } else if (token == Token.AT) {
            nextToken();
            NodeTest nt2 = parseNodeTest(Type.ATTRIBUTE);
            AxisExpression ae2 = new AxisExpression(AxisInfo.ATTRIBUTE, nt2);
            setLocation(ae2);
            return ae2;

        } else if (token == Token.LCURLY) {
            checkLanguageVersion40();
            try {
                return parseMapExpression();
            } catch (XPathException e) {
                if (e.getMessage().startsWith("Expression starting with '{'")) {
                    throw e;
                } else{
                    grumble("Expression starting with '{' is not a valid map constructor: " + e.getMessage());
                    return null;
                }
            }

        } else if (token == Token.PERCENT) {
            AnnotationList annotations = parseAnnotationsList();
            if (!(isKeyword("function") || isKeyword("fn"))) {
                grumble("Expected 'function' to follow the annotation assertion");
            }
            annotations.check(env.getConfiguration(), "IF");
            if (t.peekAhead() == Token.LCURLY) {
                nextToken();
                return parseFocusFunction(annotations);
            } else {
                return parseInlineFunction(annotations);
            }

        } else if (token instanceof Token.StringTemplate) {
            checkLanguageVersion40();
            return parseStringTemplate();

        } else if (token instanceof Token.StringConstructor) {
            return parseStringConstructor();

        } else if (token instanceof Token.DirectCommentConstructor) {
            requireXQuery("A comment constructor");
            Comment instr = new Comment();
            instr.setSelect(new StringLiteral(((Token.DirectCommentConstructor)token).getContent()));
            setLocation(instr);
            nextToken();
            return makeTracer(instr, null);

        } else if (token instanceof Token.DirectProcessingInstructionConstructor) {
            requireXQuery("A processing instruction constructor");
            return parseDirectPIConstructor();

        } else if (token instanceof Token.DirectElementConstructor) {
            requireXQuery("An element constructor");
            return parseDirectElementConstructor(false);

        } else {
            grumble("Unexpected token " + currentTokenDisplay() + " at start of sub-expression");
            return null;
        }








//            case Token.AXIS:
//                int axis;
//                try {
//                    axis = AxisInfo.getAxisNumber(t.currentTokenValue);
//                } catch (XPathException err) {
//                    grumble(err.getMessage());
//                    axis = AxisInfo.CHILD; // error recovery
//                }
//                testPermittedAxis(axis, "XPST0003");
//                short principalNodeType = AxisInfo.principalNodeType[axis];
//                nextToken();
//                Token tok3 = t.currentToken;
//                switch (tok3) {
//
//                    case Token.NAME:
//                    case Token.PREFIX:
//                    case Token.SUFFIX:
//                    case Token.STAR:
//                    case Token.LPAREN:
//                        Expression ax = new AxisExpression(axis, parseNodeTest(principalNodeType));
//                        setLocation(ax);
//                        return ax;
//
//                    default:
//                        grumble("Unexpected token " + currentTokenDisplay() + " after axis name");
//                        break;
//                }
//                break;



//            case Token.START_TAG:
//                Expression constructor = getPendingDirectElementConstructor();
//                Objects.requireNonNull(constructor);
//                nextToken();
//                return constructor;

//            default:
//                grumble("Unexpected token " + currentTokenDisplay() + " at start of expression");
//                break;
//        }
//        return new ErrorExpression();
    }

    /**
     * Construct a NodeTest corresponding to a Wildcard token (prefix:*, *:local, or Q{uri}*)
     * @param token the wildcard token
     * @param kind the node kind if known
     * @return the corresponding NodeTest
     * @throws XPathException for example if the local name is invalid
     */
    private NodeTest getNodeTestForWildcard(Token.Wildcard token, short kind) throws XPathException {
        String prefix = token.getPrefix();
        String suffix = token.getSuffix();
        return prefix.equals("*")
                ? makeLocalNameTest(kind, suffix)
                : makeNamespaceTest(kind, prefix);
    }

    protected Expression getPendingDirectElementConstructor() {
        throw new UnsupportedOperationException();
    }



//    public Expression parseNodeTestExpression(int token, boolean firstInPattern) throws XPathException{
//        NodeTest test = parseKindTest();
//        assert test != null;
//        int kind = test.getPrimitiveType();
//        byte defaultAxis = AxisInfo.CHILD;
//        if (kind == Type.ATTRIBUTE) {
//            defaultAxis = AxisInfo.ATTRIBUTE;
//        } else if (kind == Type.NAMESPACE) {
//            defaultAxis = AxisInfo.NAMESPACE;
//        }
//        if (test instanceof AnyNodeTest) {
//            // handles patterns of the form match="node()"
//            if (defaultAxis == AxisInfo.CHILD) {
//                test = MultipleNodeKindTest.CHILD_NODE;
//            } else {
//                test = NodeKindTest.ATTRIBUTE;
//            }
//        }
//        AxisExpression ae = new AxisExpression(defaultAxis, test);
//        setLocation(ae);
//        return ae;
//    }

    public Expression parseParenthesizedExpression() throws XPathException {
        nextToken();
        if (t.currentToken == Token.RPAREN) {
            nextToken();
            return makeTracer(Literal.makeEmptySequence(), null);
        }
        Expression seq = parseExpression();
        readToken(Token.RPAREN);
        return seq;
    }

    protected void testPermittedAxis(int axis, String errorCode) throws XPathException {
        if (axis == AxisInfo.PRECEDING_OR_ANCESTOR) {
            grumble("The preceding-or-ancestor axis is for internal use only", errorCode);
        }
    }


    public Expression parseNumericLiteral(boolean traceable) throws XPathException {
        int offset = t.currentTokenStartOffset;
        NumericValue number = ((Token.NumericLiteral)t.currentToken).getValue();
//        if (number.isNaN()) {
//            grumble("Invalid numeric literal " + Err.wrap(t.currentTokenValue, Err.VALUE));
//        }
        nextToken();
        Literal lit = Literal.makeLiteral(number);
        setLocation(lit, offset);
        //lit.setRetainedStaticContext(env.makeRetainedStaticContext());
        return traceable ? makeTracer(lit, null) : lit;
    }

    protected Expression parseStringLiteral(boolean traceable) throws XPathException {
        Literal literal = makeStringLiteral(expectStringLiteral(), true);
        nextToken();
        return traceable ? makeTracer(literal, null) : literal;
    }

    protected Expression parseStringConstructor() throws XPathException {
        grumble("String constructor expressions are allowed only in XQuery");
        return null;
    }

    protected void requireXQuery(String construct) throws XPathException {
        grumble(construct + " is allowed only in XQuery, not in XPath");
    }

    public Expression parseStringTemplate() throws XPathException {
        int offset = t.currentTokenStartOffset;
        Token.StringTemplate template = (Token.StringTemplate)t.currentToken;
        List<Expression> components = new ArrayList<>();
        StringBuilder currentPart = new StringBuilder();
        boolean finished = false;
        do {
            char c = t.nextChar();
            switch (c) {
                case (char) 0:
                    grumble("Unclosed string template");
                    return null;
                case '`':
                    if (t.peekChar() == '`') {
                        currentPart.append('`');
                        t.nextChar();
                    } else {
                        if (currentPart.length() > 0) {
                            components.add(new StringLiteral(currentPart.toString()));
                            currentPart.setLength(0);
                        }
                        template.close();
                        finished = true;
                    }
                    break;
                case '{':
                    if (t.peekChar() == '{') {
                        currentPart.append('{');
                        t.nextChar();
                    } else {
                        if (currentPart.length() > 0) {
                            components.add(new StringLiteral(currentPart.toString()));
                            currentPart.setLength(0);
                        }
                        t.startEmbeddedExpression();
                        if (t.currentToken != Token.EOF) {
                            Expression exp = parseExpression();
                            RetainedStaticContext rscSJ = new RetainedStaticContext(env);
                            Expression fnSJ = SystemFunction.makeCall("string-join", rscSJ, exp, new StringLiteral(StringValue.SINGLE_SPACE));
                            ExpressionTool.copyLocationInfo(exp, fnSJ);
                            components.add(fnSJ);
                            expect(Token.EOF);
                        }
                        t.endEmbeddedExpression();
                    }
                    break;
                case '}':
                    if (t.peekChar() == '}') {
                        currentPart.append('}');
                        t.nextChar();
                    } else {
                        grumble("Closing brace ('}') in string template must be doubled");
                    }
                    break;
                default:
                    currentPart.append(c);
                    break;
            }
        } while (!finished);

        Expression[] args = components.toArray(new Expression[0]);
        Expression result = SystemFunction.makeCall("concat", env.makeRetainedStaticContext(), args);
        setLocation(result, offset);
        return result;
    }

    public StructuredQName parseVariableName() throws XPathException {
        nextToken();
        String var = readName();
        if (scanOnly) {
            return new StructuredQName("", NamespaceUri.SAXON_GENERATED_VARIABLE, "dummy");
        }
        StructuredQName vtest = makeStructuredQName(var, NamespaceUri.NULL);
        assert vtest != null;
        return vtest;
    }

    /*@NotNull*/
    public Expression resolveVariableReference(int offset, StructuredQName vtest) throws XPathException {
        // See if it's a range variable or a variable in the context
        if (scanOnly) {
            return Literal.makeEmptySequence();
        }
        LocalBinding b = findRangeVariable(vtest);
        Expression ref;
        if (b != null) {
            ref = new LocalVariableReference(b);
        } else {
            if (catchDepth > 0) {
                for (StructuredQName errorVariable : StandardNames.errorVariables) {
                    if (errorVariable.getLocalPart().equals(vtest.getLocalPart())) {
                        StructuredQName functionName =
                                new StructuredQName("saxon", NamespaceUri.SAXON, "dynamic-error-info");
                        SymbolicName.F sn = new SymbolicName.F(functionName, 1);
                        Expression[] args = new Expression[]{new StringLiteral(vtest.getLocalPart())};
                        return VendorFunctionSetHE.getInstance().bind(sn, args, null, env, new ArrayList<>());
                    }
                }
            }
            try {
                ref = env.bindVariable(vtest);
            } catch (XPathException err) {
                throw err.maybeWithLocation(makeLocation());
            }
        }
        setLocation(ref, offset);
        return ref;
    }

    /**
     * Method to make a string literal from a token identified as a string
     * literal. This is trivial in XPath, but in XQuery the method is overridden
     * to identify pseudo-XML character and entity references. Note that the job of handling
     * doubled string delimiters is done by the tokenizer.
     *
     * @param currentTokenValue the token as read (excluding quotation marks)
     * @param unescape true if (in XQuery only) entity and character references are to be unescaped
     * @return The string value of the string literal
     * @throws net.sf.saxon.trans.XPathException if a static error is found
     */

    /*@NotNull*/
    protected Literal makeStringLiteral(String currentTokenValue, boolean unescape) throws XPathException {
        StringLiteral literal = new StringLiteral(currentTokenValue);
        setLocation(literal);
        return literal;
    }

    /**
     * Unescape character references and built-in entity references in a string. Does nothing
     * in XPath, because XPath does not recognize entity references in string literals
     *
     * @param token the input string, which may include XML-style character references or built-in
     *              entity references
     * @return the string with character references and built-in entity references replaced by their expansion
     * @throws XPathException if a malformed character or entity reference is found
     */

    /*@NotNull*/
    protected String unescape(String token) throws XPathException {
        return token;
    }


    /**
     * Parse a computed node constructor. This is allowed only in XQuery, so the method throws
     * an error for XPath.
     *
     * @return the expression that results from the parsing
     * @throws net.sf.saxon.trans.XPathException if a static error occurs
     */

    /*@NotNull*/
    protected Expression parseComputedNodeConstructor() throws XPathException {
        grumble("Node constructor expressions are allowed only in XQuery, not in XPath");
        return new ErrorExpression();
    }

    protected Expression parseDirectPIConstructor() throws XPathException {
        grumble("Node constructor expressions are allowed only in XQuery, not in XPath");
        return new ErrorExpression();
    }

    protected Expression parseDirectCommentConstructor() throws XPathException {
        grumble("Node constructor expressions are allowed only in XQuery, not in XPath");
        return new ErrorExpression();
    }

    /**
     * Parse a direct element constructor
     *
     * @param isNested true if this constructor is nested directly as part of the content of another
     *                 element constructor. This has the effect that the child element is not copied, which means
     *                 that namespace inheritance (which only happens during copying) has no effect
     * @return the expression representing the constructor
     * @throws XPathException if a syntax error is found
     */

    protected Expression parseDirectElementConstructor(boolean isNested) throws XPathException {
        grumble("Node constructor expressions are allowed only in XQuery, not in XPath");
        return new ErrorExpression();
    }

    /**
     * Parse a dynamic function call
     *
     * @param functionItem the expression that determines the function to be called
     * @param prefixArgument the LHS of an arrow operator, or null if this is not part of an arrow expression
     * @return the expression that results from the parsing
     * @throws net.sf.saxon.trans.XPathException if a static error is found
     */

    /*@NotNull*/
    public Expression parseDynamicFunctionCall(Expression functionItem, Expression prefixArgument) throws XPathException {
        checkLanguageVersion30();

        ArrayList<Expression> args = new ArrayList<>(10);
        if (prefixArgument != null) {
            args.add(prefixArgument);
        }
        List<Object> placeMarkers = null;

        // the "(" has already been read by the Tokenizer: now parse the arguments
        nextToken();
        if (t.currentToken != Token.RPAREN) {
            while (true) {
                Expression arg;
                Token peek = t.peekAhead();
                if (t.currentToken == Token.QMARK && (peek == Token.COMMA || peek == Token.RPAREN)) {
                    nextToken();
                    // this is a "?" placemarker
                    if (placeMarkers == null) {
                        placeMarkers = new ArrayList<>();
                    }
                    placeMarkers.add(args.size());
                    arg = Literal.makeEmptySequence(); // a convenient fiction
                } else {
                    arg = parseFunctionArgument();
                }
                args.add(arg);
                if (t.currentToken == Token.COMMA) {
                    nextToken();
                } else if (t.currentToken == Token.COLON) {
                    grumble("Keyword arguments are not allowed in a dynamic function call");
                } else {
                    break;
                }
            }
            expect(Token.RPAREN);
        }
        nextToken();

        if (placeMarkers == null) {
            return generateApplyCall(functionItem, args);
        } else {
            return createDynamicCurriedFunction(this, functionItem, args, placeMarkers);
        }
    }

    protected Expression generateApplyCall(Expression functionItem, ArrayList<Expression> args) {
        DynamicFunctionCall call = new DynamicFunctionCall(functionItem, args);
        setLocation(call, t.currentTokenStartOffset);
        return call;
    }

    /**
     * Parse a lookup operator ("?")
     *
     * @param lhs the expression that the function to be called
     * @return the expression that results from the parsing
     * @throws net.sf.saxon.trans.XPathException if a static error is found
     */

    /*@NotNull*/
    protected Expression parseLookup(Expression lhs) throws XPathException {
        checkLanguageVersion31();
        boolean deep = t.currentToken == Token.QMARK_QMARK;
        if (deep) {
            checkLanguageVersion40();
        }
        int offset = t.currentTokenStartOffset;
        nextToken();
        Token token = t.currentToken;
        
        Expression result;
        LookupExpression.Modifier modifier = LookupExpression.Modifier.CONTENT;
        if (token instanceof Token.NameToken && t.peekAhead() == Token.COLON_COLON) {
            checkLanguageVersion40();
            switch (t.currentName()) {
                case "content":
                    //modifier = LookupExpression.LookupModifier.CONTENT;
                    break;
                case "key":
                    modifier = LookupExpression.Modifier.KEY;
                    break;
                case "value":
                    modifier = LookupExpression.Modifier.VALUE;
                    break;
                case "entry":
                    modifier = LookupExpression.Modifier.ENTRY;
                    break;
                default:
                    grumble("Unknown modifier after '?' operator: " + t.currentName());
                    break;
            }
            nextToken();
            readToken(Token.COLON_COLON);
            token = t.currentToken;
        }
        if (token instanceof Token.NameToken) {
            String name = t.currentName();
            if (!NameChecker.isValidNCName(StringTool.codePoints(name))) {
                grumble("The name following '" + (deep ? "??" : "?") + "' must be a valid NCName");
            }
            nextToken();
            if (name.equals("type") && t.currentToken == Token.LPAREN) {
                nextToken();
                SequenceType requiredType = parseSequenceType();
                expect(Token.RPAREN);
                nextToken();
                result = parserExtension.lookupStar(lhs, modifier, requiredType, deep, allowXPath40Syntax);
            } else {
                result = parserExtension.lookupName(lhs, modifier, name, deep, allowXPath40Syntax);
            }
        } else if (token instanceof Token.NumericLiteral) {
            NumericValue number = ((Token.NumericLiteral)token).getValue();
            if (!(number instanceof IntegerValue)) {
                grumble("Number following '" + (deep ? "??" : "?") + "' must be an integer");
            }
            if (((Token.NumericLiteral) token).getRadix() != 10) {
                grumble("Number following '" + (deep ? "??" : "?") + "' must be a decimal integer");
            }
            nextToken();
            result = makeLookupExpression(lhs, modifier, Literal.makeLiteral(number), deep);
        } else if (token == Token.STAR) {
            nextToken();
            result = parserExtension.lookupStar(lhs, modifier, SequenceType.ANY_SEQUENCE, deep, allowXPath40Syntax);
        } else if (token == Token.LPAREN) {
            result = makeLookupExpression(lhs, modifier, parseParenthesizedExpression(), deep);
        } else if (token instanceof Token.StringLiteral) {
            checkLanguageVersion40();
            result = parserExtension.lookupName(lhs, modifier, ((Token.StringLiteral)token).getValue(), deep, allowXPath40Syntax);
            nextToken();
        } else if (token == Token.DOLLAR) {
            checkLanguageVersion40();
            offset = t.currentTokenStartOffset;
            StructuredQName varName = parseVariableName();
            result = makeLookupExpression(lhs, modifier, resolveVariableReference(offset, varName), deep);
        } else {
            grumble("Unexpected " + token + " after '?'");
            return null;
        }
        setLocation(result, offset);
        return result;
    }

    protected Expression makeLookupExpression(Expression lhs, LookupExpression.Modifier modifier, Expression rhs, boolean deep) {
        return parserExtension.makeLookupExpression(lhs, modifier, rhs, deep, allowXPath40Syntax);
    }


    /**
     * Parse a NodeTest.
     * One of QName, prefix:*, *:suffix, *, text(), node(), comment(), or
     * processing-instruction(literal?), or element(~,~), attribute(~,~), etc.
     *
     * @param nodeType the node type being sought if one is specified
     * @return the resulting NodeTest object
     * @throws XPathException if any error is encountered
     */

    /*@NotNull*/
    protected NodeTest parseNodeTest(short nodeType) throws XPathException {
        Token tok = t.currentToken;
        if (tok instanceof Token.NameToken) {
            if (t.peekAhead() == Token.LPAREN) {
                return parseKindTest();
            }
            NodeTest test = makeNameTest(nodeType, ((Token.NameToken)tok).getValue(), nodeType == Type.ELEMENT);
            nextToken();
            return test;
        } else if (tok == Token.LPAREN) {
            checkLanguageVersion40();
            return parseUnionNodeTest(nodeType);
        } else if (tok == Token.STAR) {
            nextToken();
            return NodeKindTest.makeNodeKindTest(nodeType);
        } else if (tok instanceof Token.Wildcard) {
            nextToken();
            return getNodeTestForWildcard(((Token.Wildcard) tok), nodeType);
        } else {
            grumble("Unrecognized node test");
            return null;
        }
    }

    protected NodeTest parseUnionNodeTest(short nodeType) throws XPathException {
        nextToken();
        NodeTest test = parseNodeTest(nodeType);
        while (t.currentToken == Token.VBAR) {
            nextToken();
            test = new CombinedNodeTest(test, OperatorSymbol.UNION, parseNodeTest(nodeType));
        }
        readToken(Token.RPAREN);
        return test;
    }

    /**
     * Parse a KindTest
     *
     * @return the KindTest, expressed as a NodeTest object
     * @throws net.sf.saxon.trans.XPathException if a static error is found
     */

    /*@NotNull*/
    private NodeTest parseKindTest() throws XPathException {
        NamePool pool = env.getConfiguration().getNamePool();
        boolean empty = false;
        String kind = expectName();
        nextToken();
        readToken(Token.LPAREN);
        if (t.currentToken == Token.RPAREN) {
            empty = true;
            nextToken();
        }
        switch (kind) {
            case "node":
                if (empty) {
                    return AnyNodeTest.getInstance();
                } else {
                    grumble("Expected ')': no arguments are allowed in node()");
                    return null;
                }
            case "text":
                if (empty) {
                    return NodeKindTest.TEXT;
                } else {
                    grumble("Expected ')': no arguments are allowed in text()");
                    return null;
                }
            case "comment":
                if (empty) {
                    return NodeKindTest.COMMENT;
                } else {
                    grumble("Expected ')': no arguments are allowed in comment()");
                    return null;
                }
            case "namespace-node":
                if (empty) {
                    if (!isNamespaceTestAllowed()) {
                        grumble("namespace-node() test is not allowed in XPath 2.0/XQuery 1.0");
                    }
                    return NodeKindTest.NAMESPACE;
                } else {
                    if (language == ParsedLanguage.EXTENDED_ITEM_TYPE && t.currentToken instanceof Token.NameToken) {
                        String nsName = t.currentName();
                        nextToken();
                        readToken(Token.RPAREN);
                        return new NameTest(Type.NAMESPACE, NamespaceUri.NULL, nsName, pool);
                    } else {
                        grumble("No arguments are allowed in namespace-node()");
                        return null;
                    }
                }
            case "document-node":
                if (empty) {
                    return NodeKindTest.DOCUMENT;
                } else {
                    if (!(isKeyword("element") || isKeyword("schema-element"))) {
                        grumble("Argument to document-node() must be element(...) or schema-element(...)");
                    }
                    NodeTest inner = parseKindTest();
                    readToken(Token.RPAREN);
                    return new DocumentNodeTest(inner);
                }
            case "processing-instruction":
                int fp = -1;
                if (empty) {
                    return NodeKindTest.PROCESSING_INSTRUCTION;
                } else if (t.currentToken instanceof Token.StringLiteral) {
                    String piName = Whitespace.trim(unescape(expectStringLiteral()));
                    if (!NameChecker.isValidNCName(StringTool.codePoints(piName))) {
                        // Became an error as a result of XPath erratum XP.E7
                        grumble("Processing instruction name must be a valid NCName", "XPTY0004");
                    } else {
                        fp = pool.allocateFingerprint(NamespaceUri.NULL, piName);
                    }
                } else if (t.currentToken instanceof Token.NameToken) {
                    try {
                        String[] parts = NameChecker.getQNameParts(t.currentName());
                        if (parts[0].isEmpty()) {
                            fp = pool.allocateFingerprint(NamespaceUri.NULL, parts[1]);
                        } else {
                            grumble("Processing instruction name must not contain a colon");
                        }
                    } catch (QNameException e) {
                        grumble("Invalid processing instruction name. " + e.getMessage());
                    }
                } else {
                    grumble("Processing instruction name must be a QName or a string literal");
                }
                nextToken();
                readToken(Token.RPAREN);
                return new NameTest(Type.PROCESSING_INSTRUCTION, fp, pool);

            case "schema-attribute":
                if (empty) {
                    grumble("schema-attribute() requires a name to be supplied");
                    return null;
                } else {
                    String name = readName();
                    fp = makeFingerprint(name, false);
                    readToken(Token.RPAREN);
                    if (!env.isImportedSchema(pool.getURI(fp))) {
                        grumble("No schema has been imported for namespace '" + pool.getURI(fp) + '\'', "XPST0008");
                    }
                    SchemaDeclaration decl = env.getConfiguration().getAttributeDeclaration(fp);
                    if (decl == null) {
                        grumble("There is no declaration for attribute @" + name + " in an imported schema", "XPST0008");
                        return null;
                    } else {
                        return decl.makeSchemaNodeTest();
                    }
                }
            case "schema-element":
                if (empty) {
                    grumble("schema-element() requires a name to be supplied");
                    return null;
                } else {
                    String name = readName();
                    fp = makeFingerprint(name, true);
                    readToken(Token.RPAREN);
                    if (!env.isImportedSchema(pool.getURI(fp))) {
                        grumble("No schema has been imported for namespace '" + pool.getURI(fp) + '\'', "XPST0008");
                    }
                    SchemaDeclaration decl = env.getConfiguration().getElementDeclaration(fp);
                    if (decl == null) {
                        grumble("There is no declaration for element " + name + " in an imported schema", "XPST0008");
                        return null;
                    } else {
                        return decl.makeSchemaNodeTest();
                    }
                }

            case "attribute":
            case "element":
                boolean isElementTest = kind.equals("element");
                short nodeKind = isElementTest ? Type.ELEMENT : Type.ATTRIBUTE;
                NodeTest nodeTest;
                if (empty) {
                    return isElementTest ? NodeKindTest.ELEMENT : NodeKindTest.ATTRIBUTE;
                }
                List<NodeTest> tests = parseNameTestUnion(nodeKind);
                if (tests.size() == 1) {
                    nodeTest = tests.get(0);
                } else {
                    if (!allowXPath40Syntax) {
                        grumble("NameTestUnion syntax requires 4.0 to be enabled");
                    }
                    nodeTest = NameTestUnion.withTests(tests);
                }
                if (t.currentToken == Token.RPAREN) {
                    nextToken();
                    return nodeTest;
                } else if (t.currentToken == Token.COMMA) {
                    nextToken();
                    NodeTest result;
                    if (t.currentToken instanceof Token.NameToken) {
                        StructuredQName contentType = makeStructuredQName(t.currentName(), env.getDefaultElementNamespace());
                        NamespaceUri uri = contentType.getNamespaceUri();

                        if (!(uri.equals(NamespaceUri.SCHEMA) || env.isImportedSchema(uri))) {
                            grumble("No schema has been imported for namespace '" + uri + '\'', "XPST0008");
                        }
                        SchemaType schemaType = env.getConfiguration().getSchemaType(contentType);
                        if (schemaType == null) {
                            grumble("Unknown type name " + contentType.getEQName(), "XPST0008");
                            return null;
                        }
                        if (nodeKind == Type.ATTRIBUTE && schemaType.isComplexType()) {
                            warning("An attribute cannot have a complex type", SaxonErrorCode.SXWN9041);
                        }
                        nextToken();
                        boolean nillable = false;
                        if (t.currentToken == Token.QMARK) {
                            nillable = true;
                            if (nodeKind == Type.ATTRIBUTE) {
                                grumble("attribute() tests must not be nillable");
                            }
                            nextToken();
                        }
                        if ((schemaType == AnyType.getInstance() && nillable) ||
                                (nodeKind == Type.ATTRIBUTE && schemaType == AnySimpleType.getInstance())) {
                            result = nodeTest;
                        } else {
                            NodeTest typeTest = new ContentTypeTest(nodeKind, schemaType, env.getConfiguration(), nillable);
                            if (nodeTest instanceof NodeKindTest) {
                                // this represents element(*,T) or attribute(*,T)
                                result = typeTest;
                            } else {
                                result = new CombinedNodeTest(nodeTest, OperatorSymbol.INTERSECT, typeTest);
                            }
                        }
                    } else {
                        grumble("Unexpected " + t.currentToken + " after ',' in SequenceType");
                        return null;
                    }
                    readToken(Token.RPAREN);
                    return result;
                } else {
                    grumble("Expected ')' or ',' in SequenceType");
                }
                return null;
            default:
                // can't happen!
                grumble("Unknown node kind " + kind);
                return null;
        }
    }


    public List<NodeTest> parseNameTestUnion(short nodeKind) throws XPathException {
        List<NodeTest> tests = new ArrayList<>();
        boolean matchesAll = false;
        while (true) {
            Token tok = t.currentToken;
            if (tok instanceof Token.NameToken) {
                tests.add(makeNameTest(nodeKind, t.currentName(), true));
                nextToken();
            } else if (tok instanceof Token.Wildcard) {
                nextToken();
                NodeTest test = getNodeTestForWildcard((Token.Wildcard) tok, nodeKind);
                tests.add(test);
            } else if (tok == Token.STAR) {
                nextToken();
                matchesAll = true;
            } else {
                grumble("Unrecognized name test at " + t.currentToken);
                return null;
            }
            if (t.currentToken == Token.VBAR) {
                nextToken();
            } else {
                break;
            }
        }
        if (matchesAll) {
            // If there's a "*" in the list, then anything else gets swallowed
            tests.clear();
            tests.add(NodeKindTest.makeNodeKindTest(nodeKind));
        }
        return tests;
    }

    /**
     * Ask whether the syntax namespace-node() is allowed in a node kind test.
     *
     * @return true unless XPath 2.0 / XQuery 1.0 syntax is required
     */

    protected boolean isNamespaceTestAllowed() {
        return allowXPath30Syntax;
    }

    /**
     * Check that XPath 3.0 is in use
     *
     * @throws net.sf.saxon.trans.XPathException if XPath 3.0 support was not requested
     */

    protected void checkLanguageVersion30() throws XPathException {
        if (!allowXPath30Syntax) {
            grumble("To use XPath 3.0 syntax, you must configure the XPath parser to handle it");
        }
    }

    /**
     * Check that XPath 3.1 is in use
     *
     * @throws net.sf.saxon.trans.XPathException if XPath 3.1 support was not requested
     */

    protected void checkLanguageVersion31() throws XPathException {
        if (!allowXPath31Syntax) {
            grumble("The XPath parser is not configured to allow use of XPath 3.1 syntax");
        }
    }

    /**
     * Check that XPath/XQuery 4.0 is in use
     *
     * @throws net.sf.saxon.trans.XPathException if XPath 3.1 support was not requested
     */

    protected void checkLanguageVersion40() throws XPathException {
        String lang = getLanguage();
        if (!allowXPath40Syntax) {
            grumble("The parser is not configured to allow use of " + lang + " 4.0 syntax");
        }
    }

    /**
     * Check that the map syntax is enabled: this covers the extensions to XPath 3.0
     * syntax defined in XSLT 3.0 (and also, of course, in XPath 3.1)
     *
     * @throws net.sf.saxon.trans.XPathException if XPath 3.1 support was not requested
     */

    protected void checkMapExtensions() throws XPathException {
        if (!(allowXPath31Syntax || allowXPath30XSLTExtensions)) {
            grumble("The XPath parser is not configured to allow use of the map syntax from XSLT 3.0 or XPath 3.1");
        }
    }

    /**
     * Check that Saxon syntax extensions are permitted
     * @param construct name of the construct, for use in error messages
     * @throws XPathException if Saxon syntax extensions have not been enabled
     */

    public void checkSyntaxExtensions(String construct) throws XPathException {
        if (!allowXPath40Syntax) {
            grumble("Saxon XPath syntax extensions have not been enabled: " + construct + " is not allowed");
        }
    }

    /**
     * Parse a map expression. Syntax
     * 3.0/3.1:  map { expr : expr (, expr : expr )*} }
     * 4.0:      { expr : expr (, expr : expr )*} }
     *
     * @return the map expression
     * @throws XPathException if parsing fails
     */

    /*@NotNull*/
    protected Expression parseMapExpression() throws XPathException {
        checkMapExtensions();

        // have read the "map {" or bare "{"
        int offset = t.currentTokenStartOffset;
        List<Expression> entries = new ArrayList<>();
        nextToken();
        if (t.currentToken != Token.RCURLY) {
            while (true) {
                Expression key = parseExprSingle();
                if (t.currentToken == Token.COLON_EQUALS) {
                    grumble("The ':=' notation is no longer accepted in map expressions: use ':' instead");
                }
                readToken(Token.COLON);
                Expression value = parseExprSingle();
                Expression entry;
                if (key instanceof Literal && ((Literal)key).getGroundedValue() instanceof AtomicValue
                        && value instanceof Literal) {
                    entry = Literal.makeLiteral(
                            new SingleEntryMap((AtomicValue) ((Literal)key).getGroundedValue(),
                                             ((Literal)value).getGroundedValue()));
                } else {
                    entry = MapFunctionSet.getInstance(31).makeFunction("entry", 2).makeFunctionCall(key, value);
                }
                entries.add(entry);
                if (t.currentToken == Token.RCURLY) {
                    break;
                } else {
                    readToken(Token.COMMA);
                }
            }
        }
        nextToken();

        Expression result;
        switch (entries.size()) {
            case 0:
                result = Literal.makeLiteral(EmptyMap.getInstance());
                break;
            case 1:
                result = entries.get(0);
                break;
            default:
                Expression[] entriesArray = new Expression[entries.size()];
                Block block = new Block(entries.toArray(entriesArray));
                HashTrieMap options = new HashTrieMap();
                options.initialPut(new StringValue("duplicates"), new StringValue("reject"));
                options.initialPut(new QNameValue("", NamespaceUri.SAXON, "duplicates-error-code"), new StringValue("XQDY0137"));
                result = MapFunctionSet.getInstance(31).makeFunction("merge", 2).makeFunctionCall(block, Literal.makeLiteral(options));
                break;
        }
        setLocation(result, offset);
        return result;

    }

    /**
     * Parse a "square" array constructor
     * "[" (exprSingle ("," exprSingle)* )? "]"
     * Applies to XPath/XQuery 3.1 only
     * @return the parsed expression
     * @throws XPathException if the syntax is wrong
     */
    protected Expression parseArraySquareConstructor() throws XPathException {
        checkLanguageVersion31();
        Tokenizer t = getTokenizer();
        int offset = t.currentTokenStartOffset;
        List<Expression> members = new ArrayList<>();
        nextToken();
        if (t.currentToken == Token.RSQB) {
            nextToken();
            SquareArrayConstructor arrayBlock = new SquareArrayConstructor(members);
            setLocation(arrayBlock, offset);
            return arrayBlock;
        }
        while (true) {
            Expression member = parseExprSingle();
            members.add(member);
            if (t.currentToken == Token.COMMA) {
                nextToken();
                continue;
            } else if (t.currentToken == Token.RSQB) {
                nextToken();
                break;
            }
            grumble("Expected ',' or ']', " + "found " + t.currentToken);
            return new ErrorExpression();
        }
        SquareArrayConstructor block = new SquareArrayConstructor(members);
        setLocation(block, offset);
        return block;
    }

    /**
     * Parse a "curly" array constructor
     * array "{" expr "}"
     * Applies to XPath/XQuery 3.1 only
     *
     * @return the parsed expression
     * @throws XPathException if the syntax is invalid or the construct is not permitted
     */

    protected Expression parseArrayCurlyConstructor() throws XPathException {
        checkLanguageVersion31();
        Tokenizer t = getTokenizer();
        int offset = t.currentTokenStartOffset;
        nextToken();
        if (t.currentToken == Token.RCURLY) {
            nextToken();
            return Literal.makeLiteral(SimpleArrayItem.EMPTY_ARRAY);
        }
        Expression body = parseExpression();
        readToken(Token.RCURLY);

        SystemFunction sf = ArrayFunctionSet.getInstance(40).makeFunction("_from-sequence", 1);
        Expression result = sf.makeFunctionCall(body);
        setLocation(result, offset);
        return result;
    }

    /**
     * Parse a function call.
     * function-name '(' ( Expression (',' Expression )* )? ')'
     *
     * @param prefixArgument left hand operand of arrow operator,
     *                       or null in the case of a conventional function call
     * @return the resulting subexpression
     * @throws XPathException if any error is encountered
     */

    /*@NotNull*/
    public Expression parseFunctionCall(String fname, Expression prefixArgument) throws XPathException {

        int offset = t.currentTokenStartOffset;
        nextToken();
        expect(Token.LPAREN);
        ArrayList<Expression> args = new ArrayList<>(10);
        int position = 0;
        if (prefixArgument != null) {
            args.add(prefixArgument);
            position++;
        }

        StructuredQName functionName = resolveFunctionName(fname);

        // placeMarkerArgs holds details of the arguments that were set to "?", indicating
        // a partially applied function. The list is in the order that the placeMarkers appear,
        // and each Object in the list is either the 0-based position of the placeMarker if given
        // positionally, or the StructuredQName representing the associated keyword if given by keyword.
        List<Object> placeMarkerArgs = null;

        Map<StructuredQName, Integer> keywordArgs = null;
        nextToken();
        if (t.currentToken != Token.RPAREN) {
            while (true) {
                Token peek = t.peekAhead();
                Expression arg;
                if (t.currentToken instanceof Token.NameToken && peek == Token.COLON_EQUALS && allowXPath40Syntax) {
                    // keyword argument
                    StructuredQName paramName = qNameParser.parse(t.currentName(), NamespaceUri.NULL);
                    nextToken(); // read the operator
                    nextToken(); // position on the expression giving the value
                    peek = t.peekAhead();
                    if (t.currentToken == Token.QMARK && (peek == Token.COMMA || peek == Token.RPAREN)) {
                        // keyword := ?
                        nextToken();
                        if (placeMarkerArgs == null) {
                            placeMarkerArgs = new ArrayList<>();
                        }
                        placeMarkerArgs.add(paramName);
                        arg = Literal.makeEmptySequence(); // a convenient fiction
                    } else {
                        arg = parseExprSingle();
                    }
                    if (keywordArgs == null) {
                        keywordArgs = new HashMap<>();
                    } else if (keywordArgs.containsKey(paramName)) {
                        grumble("Duplicate keyword '" + paramName + "'in function arguments");
                    }
                    keywordArgs.put(paramName, args.size());
                    args.add(arg);
                } else {
                    if (keywordArgs != null) {
                        grumble("Keyword arguments must not be followed by positional arguments in a function call");
                    }
                    if (t.currentToken == Token.QMARK && (peek == Token.COMMA || peek == Token.RPAREN)) {
                        nextToken();
                        // this is a "?" placemarker
                        if (placeMarkerArgs == null) {
                            placeMarkerArgs = new ArrayList<>();
                        }
                        placeMarkerArgs.add(position);
                        arg = Literal.makeEmptySequence(); // a convenient fiction
                    } else {
                        arg = parseExprSingle();
                    }
                    args.add(arg);
                }
                if (t.currentToken == Token.COMMA) {
                    nextToken();
                } else {
                    break;
                }
                position++;
            }
            expect(Token.RPAREN);
        }
        nextToken();

        if (scanOnly) {
            return new StringLiteral(StringValue.EMPTY_STRING);
        }

        Expression[] arguments = new Expression[args.size()];
        arguments = args.toArray(arguments);

        if (placeMarkerArgs != null) {
            return makeCurriedFunction(this, offset, functionName, arguments, placeMarkerArgs);
        }

        Expression fcall;
        SymbolicName.F sn = new SymbolicName.F(functionName, args.size());
        List<String> reasons = new ArrayList<>();
        try {
            fcall = env.getFunctionLibrary().bind(sn, arguments, keywordArgs, env, reasons);
        } catch (UncheckedXPathException e) {
            fcall = null;
            reasons.add(e.getMessage());
        }
        if (fcall == null) {
            return reportMissingFunction(offset, functionName, arguments, reasons);
        }
        //  A QName or NOTATION constructor function must be given the namespace context now
//        if (fcall instanceof CastExpression &&
//                ((AtomicType) fcall.getItemType()).isNamespaceSensitive()) {
//            ((CastExpression) fcall).bindStaticContext(env);
//        }
        // There are special rules for certain functions appearing in a pattern
        if (language == ParsedLanguage.XSLT_PATTERN) {
            if (fcall.isCallOn(RegexGroup.class)) {
                return Literal.makeEmptySequence();
            } else if (fcall instanceof CurrentGroupCall) {
                grumble("The current-group() function cannot be used in a pattern",
                        "XTSE1060", offset);
                return new ErrorExpression();
            } else if (fcall instanceof CurrentGroupingKeyCall) {
                grumble("The current-grouping-key() function cannot be used in a pattern",
                        "XTSE1070", offset);
                return new ErrorExpression();
            } else if (fcall.isCallOn(CurrentMergeGroup.class)) {
                grumble("The current-merge-group() function cannot be used in a pattern",
                        "XTSE3470", offset);
                return new ErrorExpression();
            } else if (fcall.isCallOn(CurrentMergeKey.class)) {
                grumble("The current-merge-key() function cannot be used in a pattern",
                        "XTSE3500", offset);
                return new ErrorExpression();
            }
        }
        setLocation(fcall, offset);
        for (Expression argument : arguments) {
            if (fcall != argument && argument.getParentExpression() == null
                    && !functionName.hasURI(NamespaceUri.GLOBAL_JS)) {
                // avoid doing this when the function has already been optimized away, e.g. unordered()
                // Also avoid doing this when a js: function is parsed into an ixsl:call()
                // TODO move the adoptChildExpression into individual function libraries
                fcall.adoptChildExpression(argument);
            }
        }

        return makeTracer(fcall, functionName);

    }

    /**
     * Process a function call in which one or more of the argument positions are
     * represented as "?" placemarkers (indicating partial application or currying)
     *
     * @param parser           the XPath parser
     * @param offset           offset in the query source of the start of the expression
     * @param name             the function call (as if there were no currying)
     * @param args             the arguments (with EmptySequence in the placemarker positions)
     * @param placeMarkerArgs  the positions or keywords of the placemarkers, in order
     * @return the curried function
     * @throws XPathException if a dynamic error occurs
     */

    public Expression makeCurriedFunction(
            XPathParser parser, int offset,
            StructuredQName name, Expression[] args,
            List<Object> placeMarkerArgs) throws XPathException {
        StaticContext env = parser.getStaticContext();
        FunctionLibrary lib = env.getFunctionLibrary();
        SymbolicName.F sn = new SymbolicName.F(name, args.length);
        FunctionItem target = lib.getFunctionItem(sn, env);
        if (target == null) {
            // This will not happen in XQuery; instead, a dummy function will be created in the
            // UnboundFunctionLibrary in case it's a forward reference to a function not yet compiled
            List<String> reasons = new ArrayList<>();
            return parser.reportMissingFunction(offset, name, args, reasons);
        }
        Expression targetExp = makeNamedFunctionReference(name, target);
        parser.setLocation(targetExp, offset);
        return curryFunction(targetExp, args, placeMarkerArgs);
    }

    /**
     * Process a function expression in which one or more of the argument positions are
     * represented as "?" placemarkers (indicating partial application or currying)
     *
     * @param functionExp     an expression that returns the function to be curried
     * @param args            the arguments (with EmptySequence in the placemarker positions)
     * @param placeMarkerArgs the positions or keywords of the placemarkers, in order
     * @return the curried function
     */

    public static Expression curryFunction(
            Expression functionExp,
            Expression[] args,
            List<Object> placeMarkerArgs) throws XPathException {

        boolean hasKeywordPlaceMarkers = false;
        for (Object o : placeMarkerArgs) {
            if (o instanceof Integer) {
                args[(Integer)o] = new PlaceHolder((Integer) o);
            } else {
                hasKeywordPlaceMarkers = true;
                break;
            }
        }

        if (hasKeywordPlaceMarkers) {
            // Map the declared parameter names to their 0-based numeric positions
            // It must be a function item that knows the original parameter names
            if (functionExp instanceof IFunctionWithRetainedParameterNames) {
                StructuredQName[] paramNames = ((IFunctionWithRetainedParameterNames)functionExp).getParameterNames();
                Map<StructuredQName, Integer> paramNameMap = new HashMap<>();
                for (int i = 0; i< paramNames.length; i++) {
                    paramNameMap.put(paramNames[i], i);
                }
                for (int pm = 0; pm < placeMarkerArgs.size(); pm++) {
                    Object pmId = placeMarkerArgs.get(pm);
                    if (pmId instanceof StructuredQName) {
                        boolean present = paramNameMap.containsKey((StructuredQName) pmId);
                        if (!present) {
                            throw new XPathException("Keyword " + ((StructuredQName) pmId).getEQName()
                                                             + " does not identify any parameter in the function being partially applied",
                                                     "XPST9999");
                        }
                        Integer pos = paramNameMap.get((StructuredQName) pmId);
                        args[pos] = new PlaceHolder(pm);
                    }
                }
            } else {
                throw new XPathException("Keyword placemarkers not supported on " + functionExp.toShortString());
            }
        }
        return new PartialApply(functionExp, args);
    }


    public Expression createDynamicCurriedFunction(
            XPathParser p, Expression functionItem, ArrayList<Expression> args, List<Object> placeMarkers) throws XPathException {
        Expression[] arguments = new Expression[args.size()];
        arguments = args.toArray(arguments);
        Expression result = curryFunction(functionItem, arguments, placeMarkers);
        p.setLocation(result, p.getTokenizer().currentTokenStartOffset);
        return result;
    }

    public void handleExternalFunctionDeclaration(XQueryParser p, XQueryFunction func) throws XPathException {
        parserExtension.needExtension(p, "External function declarations");
    }

    /*@NotNull*/
    public Expression reportMissingFunction(int offset, StructuredQName functionName, Expression[] arguments, List<String> reasons) throws XPathException {
        StringBuilder sb = new StringBuilder();
        sb.append("Cannot find a ").append(arguments.length).append(
                "-argument function named ").append(functionName.getEQName()).append("()");
        Configuration config = env.getConfiguration();
        for (String reason : reasons) {
            sb.append(". ").append(reason);
        }
        if (config.getBooleanProperty(Feature.ALLOW_EXTERNAL_FUNCTIONS)) {
            boolean existsWithDifferentArity = false;
            for (int i = 0; i < arguments.length + 5; i++) {
                if (i != arguments.length) {
                    SymbolicName.F sn = new SymbolicName.F(functionName, i);
                    if (env.getFunctionLibrary().isAvailable(sn, 31)) {
                        existsWithDifferentArity = true;
                        break;
                    }
                }
            }
            if (existsWithDifferentArity) {
                sb.append(". The namespace URI and local name are recognized, but the number of arguments is wrong");
            } else {
                String supplementary = getMissingFunctionExplanation(functionName, config);
                if (supplementary != null) {
                    sb.append(". ").append(supplementary);
                }
            }
        } else {
            sb.append(". External function calls have been disabled");
        }
        if (env.isInBackwardsCompatibleMode()) {
            // treat this as a dynamic error to be reported only if the function call is executed
            return new ErrorExpression(sb.toString(), "XTDE1425", false);
        } else {
            grumble(sb.toString(), "XPST0017", offset);
            return null;
        }
    }

    /**
     * Get a message containing suggestions as to why a requested function might not be available
     * @param functionName the name of the required function
     * @param config the Saxon configuration
     * @return a suggestion as to why the function was not found; or null if no suggestions can be offered.
     */

    public static String getMissingFunctionExplanation(StructuredQName functionName, Configuration config) {
        String actualURI = functionName.getNamespaceUri().toString();
        String similarNamespace = NamespaceConstant.findSimilarNamespace(actualURI);
        if (similarNamespace != null) {
            if (similarNamespace.equals(actualURI)) {
                switch (similarNamespace) {
                    case NamespaceConstant.FN:
                        return null;
                    case NamespaceConstant.SAXON:
                        if (config.getEditionCode().equals("HE")) {
                            return "Saxon extension functions are not available under Saxon-HE";
                        } else if (!config.isLicensedFeature(Configuration.LicenseFeature.PROFESSIONAL_EDITION)) {
                            return "Saxon extension functions require a Saxon-PE or Saxon-EE license";
                        }
                        break;
                    case NamespaceConstant.XSLT:
                        if (functionName.getLocalPart().equals("original")) {
                            return "Function name xsl:original is only available within an overriding function";
                        } else {
                            return "There are no functions defined in the XSLT namespace";
                        }
                }
            } else {
                return "Perhaps the intended namespace was '" + similarNamespace + "'";
            }
        } else if (actualURI.contains("java")) {
            return diagnoseCallToJavaMethod(config);
        } else if (actualURI.startsWith("clitype:")) {
            return diagnoseCallToCliMethod(config);
        }
        return null;
    }

    @CSharpReplaceBody(code="return \"Reflexive calls to external Java methods are not available under SaxonCS\";")
    private static String diagnoseCallToJavaMethod(Configuration config) {
        if (config.getEditionCode().equals("HE")) {
            return "Reflexive calls to Java methods are not available under Saxon-HE";
        } else if (!config.isLicensedFeature(Configuration.LicenseFeature.PROFESSIONAL_EDITION)) {
            return "Reflexive calls to Java methods require a Saxon-PE or Saxon-EE license, and none was found";
        } else {
            return "For diagnostics on calls to Java methods, use the -TJ command line option " +
                    "or set the Configuration property FeatureKeys.TRACE_EXTERNAL_FUNCTIONS";
        }
    }

    @CSharpReplaceBody(code = "return \"Reflexive calls to external .NET methods are not available under SaxonCS\";")
    private static String diagnoseCallToCliMethod(Configuration config) {
        if (config.getEditionCode().equals("HE")) {
            return "Reflexive calls to external .NET methods are not available under Saxon-HE";
        } else if (!config.isLicensedFeature(Configuration.LicenseFeature.PROFESSIONAL_EDITION)) {
            return "Reflexive calls to external .NET methods require a Saxon-PE or Saxon-EE license, and none was found";
        } else {
            return "For diagnostics on calls to .NET methods, use the -TJ command line option " +
                    "or call processor.SetProperty(\"http://saxon.sf.net/feature/trace-external-functions\", \"true\")";
        }
    }

    /**
     * Interpret a function name, returning it as a resolved QName
     *
     * @param fname the lexical QName used as the function name; or an EQName presented
     *              by the tokenizer as a name in Clark notation
     * @return the Structured QName obtained by resolving any prefix in the function name
     * @throws XPathException if the supplied name is not a valid QName or if its prefix
     *                        is not in scope
     */

    /*@NotNull*/
    protected StructuredQName resolveFunctionName(String fname) throws XPathException {
        if (scanOnly) {
            return NamespaceUri.SAXON.qName("dummy");
        }
        StructuredQName functionName = null;
        try {
            functionName = qNameParser.parse(fname, env.getDefaultFunctionNamespace());
        } catch (XPathException e) {
            grumble(e.getMessage(), e.getErrorCodeQName());
            assert false;
        }

        if (functionName.hasURI(NamespaceUri.SCHEMA)) {
            ItemType t = Type.getBuiltInItemType(functionName.getNamespaceUri(), functionName.getLocalPart());
            if (t instanceof BuiltInAtomicType) {
                checkAllowedType(env, (BuiltInAtomicType) t);
            }
        }
        return functionName;
    }

    /**
     * Parse an argument to a function call. Separate method so it can
     * be overridden. With higher-order-function syntax in XPath 3.0/XQuery 3.0,
     * this returns null if the pseudo-argument "?" is found.
     *
     * @return the Expression used as the argument, or null if the argument is the place-holder "?"
     * @throws XPathException if the argument expression does not parse correctly
     */

    /*@Nullable*/
    public Expression parseFunctionArgument() throws XPathException {
        return parseExprSingle();
    }

    /**
     * Parse a literal function item (introduced in XQuery 1.1)
     * Syntax: QName # integer
     * The QName and # have already been read
     *
     * @return an ExternalObject representing the function item
     * @throws net.sf.saxon.trans.XPathException if a static error is encountered
     */

    /*@NotNull*/
    protected Expression parseNamedFunctionReference() throws XPathException {

        String fname = expectName();
        int offset = t.currentTokenStartOffset;

        StaticContext env = getStaticContext();

        nextToken();
        readToken(Token.HASH);

        if (!(t.currentToken instanceof Token.NumericLiteral)) {
            grumble("Expected number following '#'");
        };
        NumericValue number = ((Token.NumericLiteral)t.currentToken).getValue();
        if (!(number instanceof IntegerValue)) {
            grumble("Number following '#' must be an integer");
        }
        if (((Token.NumericLiteral) t.currentToken).getRadix() != 10) {
            grumble("Number following '#' must be a decimal integer");
        }
        if (number.compareTo(0) < 0 || number.compareTo(Integer.MAX_VALUE) > 0) {
            grumble("Number following '#' is out of range", "FOAR0002");
        }
        int arity = (int) number.longValue();
        nextToken();
        StructuredQName functionName = null;

        try {
            functionName = getQNameParser().parse(fname, env.getDefaultFunctionNamespace());
            if (functionName.getPrefix().equals("")) {
                if (XPathParser.isReservedFunctionName(functionName.getLocalPart(), languageVersion)) {
                    grumble("The unprefixed function name '" + functionName.getLocalPart() + "' is reserved");
                }
            }
        } catch (XPathException e) {
            grumble(e.getMessage(), e.getErrorCodeQName());
            assert functionName != null;
        }

        try {
            FunctionLibrary lib = env.getFunctionLibrary();
            SymbolicName.F sn = new SymbolicName.F(functionName, arity);
            FunctionItem foundFunction = lib.getFunctionItem(sn, env);
            if (foundFunction == null) {
                grumble("Function " + functionName.getEQName() + "#" + arity + " not found", "XPST0017", offset);
            }

            if (foundFunction instanceof IContextAccessorFunction) {
                // This indicates that the function being referenced is dependent on the
                // dynamic context, and cannot be invoked directly until it is bound to
                // the dynamic context by calling the method {@link IContextAccessorFunction#bindContext}.
                // This cannot be done until run-time. In the meantime we return an expression whose
                // effect, when evaluated, is to make the call on {@code bindContext} and return
                // the bound function item.
                IContextAccessorFunction unboundFunction = (IContextAccessorFunction)foundFunction;
                CallableDelegate callable = new CallableDelegate(
                        (context, args) -> unboundFunction.bindContext(context));
                callable.setDescription(foundFunction.getDescription());
                CallableFunction binder = new CallableFunction(
                        0, callable, new SpecificFunctionType(new SequenceType[]{}, SequenceType.SINGLE_FUNCTION));
                DynamicFunctionCall dfc = new DynamicFunctionCall(
                        new FunctionLiteral(binder), Collections.emptyList());
                setLocation(dfc, offset);
                return dfc;
            }

            Expression ref = makeNamedFunctionReference(functionName, foundFunction);
            setLocation(ref, offset);
            return ref;
        } catch (XPathException e) {
            grumble(e.getMessage(), "XPST0017", offset);
            return null;
        }

    }

    private static Expression makeNamedFunctionReference(StructuredQName functionName, FunctionItem fcf) {
        if (fcf instanceof UserFunction && !functionName.hasURI(NamespaceUri.XSLT)) {
            // This case is treated specially because a UserFunctionReference in XSLT can be redirected
            // at link time to an overriding function. However, this doesn't apply to xsl:original
            return new UserFunctionReference((UserFunction) fcf);
        } else if (fcf instanceof UnresolvedXQueryFunctionItem) {
            return ((UnresolvedXQueryFunctionItem) fcf).getFunctionReference();
        } else {
            return new FunctionLiteral(fcf);
        }
    }


    /**
     * Parse the annotations that can appear in a variable or function declaration
     *
     * @return the annotations as a list
     * @throws XPathException in the event of a syntax error
     */

    protected AnnotationList parseAnnotationsList() throws XPathException {
        grumble("Function annotations are not allowed in XPath");
        return null;
    }

    protected Expression parseInlineFunction(AnnotationList annotations) throws XPathException {
        nextToken();
        readToken(Token.LPAREN);
        List<UserFunctionParameter> params = new ArrayList<>(8);

        int paramSlot = 0;
        while (t.currentToken != Token.RPAREN) {
            //     ParamList   ::=     Param ("," Param)*
            //     Param       ::=     "$" VarName  TypeDeclaration?

            StructuredQName argQName = readVariableName();
            SequenceType paramType = readOptionalAsClause(SequenceType.ANY_SEQUENCE);

            UserFunctionParameter arg = new UserFunctionParameter();
            arg.setRequiredType(paramType);
            arg.setVariableQName(argQName);
            arg.setSlotNumber(paramSlot++);
            params.add(arg);

            if (t.currentToken == Token.RPAREN) {
                break;
            } else if (t.currentToken == Token.COMMA) {
                nextToken();
            } else {
                grumble("Expected ',' or ')' after function argument, found '" +
                                t.currentToken + '\'');
            }
        }
        nextToken();
        SequenceType resultType = readOptionalAsClause(SequenceType.ANY_SEQUENCE);
        return parseInlineFunctionBody(annotations, params, resultType);

    }

    /**
     * Read a variable name ('$' QName). The current token should be the `$` sign; on return,
     * the input is positioned at the next token after the QName.
     * @return the name
     * @throws XPathException if no name was found
     */
    protected StructuredQName readVariableName() throws XPathException {
        readToken(Token.DOLLAR);
        String name = readName();
        return makeStructuredQName(name, NamespaceUri.NULL);
    }

    protected SequenceType readOptionalAsClause(SequenceType defaultType) throws XPathException {
        if (tryKeyword("as")) {
            return parseSequenceType();
        } else {
            return defaultType;
        }
    }

    protected Expression parseInlineFunctionBody(
            AnnotationList annotations,
            List<UserFunctionParameter> params,
            SequenceType resultType) throws XPathException {
        // the next token should be the "{" at the start of the function body

        int offset = t.currentTokenStartOffset;

        InlineFunctionDetails details = new InlineFunctionDetails();
        details.outerVariables = new IndexedStack<>();
        for (LocalBinding lb : getRangeVariables()) {
            details.outerVariables.push(lb);
        }
        details.outerVariablesUsed = new ArrayList<>(4);
        details.implicitParams = new ArrayList<>(4);
        inlineFunctionStack.push(details);
        //noinspection Convert2Diamond
        setRangeVariables(new IndexedStack<LocalBinding>());

        HashSet<StructuredQName> paramNameSet = new HashSet<>(8);
        for (UserFunctionParameter arg : params) {
            if (!scanOnly) {
                if (!paramNameSet.add(arg.getVariableQName())) {
                    grumble("Duplicate parameter name " + Err.wrap(arg.getVariableQName().getEQName(), Err.VARIABLE), "XQST0039");
                }
            }
            declareRangeVariable(arg);
        }

        readToken(Token.LCURLY);
        Expression body;
        if (t.currentToken == Token.RCURLY && isAllowXPath31Syntax()) {
            nextToken();
            body = Literal.makeEmptySequence();
        } else {
            body = parseExpression();
            readToken(Token.RCURLY);
        }
        ExpressionTool.setDeepRetainedStaticContext(body, getStaticContext().makeRetainedStaticContext());
        Expression result = makeInlineFunctionValue(this, annotations, details, params, resultType, body);

        setLocation(result, offset);

        for (UserFunctionParameter arg : params) {
            undeclareRangeVariable();
        }
        // restore the previous stack of range variables
        setRangeVariables(details.outerVariables);
        inlineFunctionStack.pop();
        return result;
    }

    public static Expression makeInlineFunctionValue(
            XPathParser p, AnnotationList annotations,
            InlineFunctionDetails details, List<UserFunctionParameter> params,
            SequenceType resultType, Expression body) {
        // Does this function access any outer variables?
        // If so, we create a UserFunction in which the outer variables are defined as extra parameters
        // in addition to the declared parameters, and then we return a call to partial-apply() that
        // sets these additional parameters to the values they have in the calling context.
        int arity = params.size();

        UserFunction uf = new UserFunction();
        uf.setFunctionName(new StructuredQName("anon", NamespaceUri.ANONYMOUS, "f_" + uf.hashCode()));
        uf.setPackageData(p.getStaticContext().getPackageData());
        uf.setBody(body);
        uf.setAnnotations(annotations);
        uf.setResultType(resultType);
        uf.incrementReferenceCount();

        if (uf.getPackageData() instanceof StylesheetPackage) {
            // Add the inline function as a private component to the package, so that it can have binding
            // slots allocated for any references to global variables or functions, and so that it will
            // be copied as a hidden component into any using packages
            StylesheetPackage pack = (StylesheetPackage) uf.getPackageData();
            Component comp = Component.makeComponent(uf, Visibility.PRIVATE, VisibilityProvenance.DEFAULTED, pack, pack);
            uf.setDeclaringComponent(comp);
        }

        Expression result;
        List<UserFunctionParameter> implicitParams = details.implicitParams;
        if (!implicitParams.isEmpty()) {
            int extraParams = implicitParams.size();
            int expandedArity = params.size() + extraParams;
            UserFunctionParameter[] paramArray = new UserFunctionParameter[expandedArity];
            for (int i = 0; i < params.size(); i++) {
                paramArray[i] = params.get(i);
            }
            int k = params.size();
            for (UserFunctionParameter implicitParam : implicitParams) {
                paramArray[k++] = implicitParam;
            }
            uf.setParameterDefinitions(paramArray);
            SlotManager stackFrame = p.getStaticContext().getConfiguration().makeSlotManager();
            for (int i = 0; i < expandedArity; i++) {
                int slot = stackFrame.allocateSlotNumber(paramArray[i].getVariableQName(), paramArray[i]);
                paramArray[i].setSlotNumber(slot);
            }

            ExpressionTool.allocateSlots(body, expandedArity, stackFrame);
            uf.setStackFrameMap(stackFrame);
            result = new UserFunctionReference(uf);

            Expression[] partialArgs = new Expression[expandedArity];
            for (int i = 0; i < arity; i++) {
                partialArgs[i] = new PlaceHolder(i);
            }
            for (int ip = 0; ip < implicitParams.size(); ip++) {
                UserFunctionParameter ufp = implicitParams.get(ip);
                LocalBinding binding = details.outerVariablesUsed.get(ip);
                VariableReference var;
                if (binding instanceof ParserExtension.TemporaryXSLTVariableBinding) {
                    var = new LocalVariableReference(binding);
                    ((ParserExtension.TemporaryXSLTVariableBinding) binding).declaration.registerReference(var);
                } else {
                    var = new LocalVariableReference(binding);
                }
                var.setStaticType(binding.getRequiredType(), null, 0);
                ufp.setRequiredType(binding.getRequiredType());
                partialArgs[ip + arity] = var;
            }
            result = new PartialApply(result, partialArgs);

        } else {

            // there are no implicit parameters
            UserFunctionParameter[] paramArray = params.toArray(new UserFunctionParameter[0]);
            uf.setParameterDefinitions(paramArray);

            SlotManager stackFrame = p.getStaticContext().getConfiguration().makeSlotManager();
            for (UserFunctionParameter param : paramArray) {
                stackFrame.allocateSlotNumber(param.getVariableQName(), param);
            }

            ExpressionTool.allocateSlots(body, params.size(), stackFrame);
            uf.setStackFrameMap(stackFrame);
            result = new UserFunctionReference(uf);
        }

        if (uf.getPackageData() instanceof StylesheetPackage) {
            // Note: inline functions in XSLT are registered as components; but not if they
            // are declared within a static expression, e.g. the initializer of a static
            // global variable
            ((StylesheetPackage) uf.getPackageData()).addComponent(uf.getDeclaringComponent());
        }
        return result;
    }


    /**
     * Locate a range variable with a given name. (By "range variable", we mean a
     * variable declared within the expression where it is used.)
     *
     * @param qName identifies the name of the range variable
     * @return null if not found (this means the variable is probably a
     * context variable); otherwise the relevant RangeVariable
     */

    /*@Nullable*/
    public LocalBinding findOuterRangeVariable(StructuredQName qName) {
        return findOuterRangeVariable(qName, inlineFunctionStack, getStaticContext());
    }


    /**
     * When a variable reference occurs within an inline function, it might be a reference to a variable declared
     * outside the inline function (which needs to become part of the closure). This code looks for such an outer
     * variable
     *
     * @param qName               the name of the variable
     * @param inlineFunctionStack the stack of inline functions that we are within
     * @param env                 the static context
     * @return a binding for the variable; this will typically be a binding to a newly added parameter
     * for the innermost function in which the variable reference appears. As a side effect, all the inline
     * functions between the declaration of the variable and its use will have this variable as an additional
     * parameter, each one bound to the corresponding parameter in the containing function.
     */

    public static LocalBinding findOuterRangeVariable(StructuredQName qName, IndexedStack<InlineFunctionDetails> inlineFunctionStack, StaticContext env) {
        // If we didn't find the variable, it might be defined in an outer scope.
        LocalBinding b2 = findOuterXPathRangeVariable(qName, inlineFunctionStack);
        if (b2 != null) {
            return b2;
        }
        // It's not an in-scope range variable. If this is a free-standing XPath expression, it might be
        // a parameter declared in the static context
        if (env instanceof IndependentContext && !inlineFunctionStack.isEmpty()) {
            b2 = findXPathParameter(qName, inlineFunctionStack, env);
        }
        // It's not an in-scope range variable. If we're in XSLT, it might be an XSLT-defined local variable
        if (env instanceof ExpressionContext && !inlineFunctionStack.isEmpty()) {
            b2 = findOuterXSLTVariable(qName, inlineFunctionStack, env);
        }
        return b2;  // if null, it's not an in-scope range variable
    }

    /**
     * Look for an XPath/XQuery declaration of a variable used inside an inline function, but declared outside
     *
     * @param qName               the name of the variable
     * @param inlineFunctionStack the stack of inline functions that we are within
     * @return a binding to the innermost declaration of the variable
     */
    private static LocalBinding findOuterXPathRangeVariable(StructuredQName qName, IndexedStack<InlineFunctionDetails> inlineFunctionStack) {
        for (int s = inlineFunctionStack.size() - 1; s >= 0; s--) {
            InlineFunctionDetails details = inlineFunctionStack.get(s);
            IndexedStack<LocalBinding> outerVariables = details.outerVariables;
            for (int v = outerVariables.size() - 1; v >= 0; v--) {
                LocalBinding b2 = outerVariables.get(v);
                if (b2.getVariableQName().equals(qName)) {
                    for (int bs = s; bs <= inlineFunctionStack.size() - 1; bs++) {
                        details = inlineFunctionStack.get(bs);
                        boolean found = false;
                        for (int p = 0; p < details.outerVariablesUsed.size() - 1; p++) {
                            if (details.outerVariablesUsed.get(p) == b2) {
                                // the inner function already uses the outer variable
                                b2 = details.implicitParams.get(p);
                                found = true;
                                break;
                            }
                        }
                        if (!found) {
                            // Need to add an implicit parameter to the inner function
                            details.outerVariablesUsed.add(b2);
                            UserFunctionParameter ufp = new UserFunctionParameter();
                            ufp.setVariableQName(qName);
                            ufp.setRequiredType(b2.getRequiredType());
                            details.implicitParams.add(ufp);
                            b2 = ufp;
                        }
                    }
                    return b2;
                }
            }
            LocalBinding b3 = bindParametersInNestedFunctions(qName, inlineFunctionStack, s);
            if (b3 != null) {
                return b3;
            }
        }
        return null;
    }

    /**
     * Look for a declaration of a variable used inside an inline function, but declared as part of
     * the static context of a free-standing XPath expression
     *
     * @param qName               the name of the variable
     * @param inlineFunctionStack the stack of inline functions that we are within
     * @return a binding to the innermost declaration of the variable
     */

    private static LocalBinding findXPathParameter(
            StructuredQName qName, IndexedStack<InlineFunctionDetails> inlineFunctionStack, StaticContext env) {
        if (env instanceof IndependentContext) {
            XPathVariable var = ((IndependentContext) env).getExternalVariable(qName);
            if (var != null) {
                InlineFunctionDetails details = inlineFunctionStack.get(0);
                LocalBinding innermostBinding;
                boolean found = false;
                for (int p = 0; p < details.outerVariablesUsed.size(); p++) {
                    if (details.outerVariablesUsed.get(p).getVariableQName().equals(qName)) {
                        // the inner function already uses the outer variable
                        found = true;
                        break;
                    }
                }
                if (!found) {
                    // Need to add an implicit parameter to the inner function
                    details.outerVariablesUsed.add(var);
                    UserFunctionParameter ufp = new UserFunctionParameter();
                    ufp.setVariableQName(qName);
                    ufp.setRequiredType(var.getRequiredType());
                    details.implicitParams.add(ufp);
                }
                // Now do the same for all inner inline functions, but this time binding to the
                // relevant parameter of the next containing function
                innermostBinding = bindParametersInNestedFunctions(qName, inlineFunctionStack, 0);
                return innermostBinding;
            }
        }
        return null;
    }


    /**
     * Look for an XSLT declaration of a variable used inside an inline function, but declared outside
     *
     * @param qName               the name of the variable
     * @param inlineFunctionStack the stack of inline functions that we are within
     * @return a binding to the innermost declaration of the variable
     */

    private static LocalBinding findOuterXSLTVariable(
            StructuredQName qName, IndexedStack<InlineFunctionDetails> inlineFunctionStack, StaticContext env) {
        StructuredQName attName = ((ExpressionContext) env).getAttributeName();
        SourceBinding decl = ((ExpressionContext) env).getStyleElement().bindLocalVariable(qName, attName);
        if (decl != null) {
            InlineFunctionDetails details = inlineFunctionStack.get(0);
            LocalBinding innermostBinding;
            boolean found = false;
            for (int p = 0; p < details.outerVariablesUsed.size(); p++) {
                if (details.outerVariablesUsed.get(p).getVariableQName().equals(qName)) {
                    // the inner function already uses the outer variable
                    found = true;
                    break;
                }
            }
            if (!found) {
                // Need to add an implicit parameter to the inner function
                details.outerVariablesUsed.add(new ParserExtension.TemporaryXSLTVariableBinding(decl));
                UserFunctionParameter ufp = new UserFunctionParameter();
                ufp.setVariableQName(qName);
                ufp.setRequiredType(decl.getInferredType(true));
                details.implicitParams.add(ufp);
            }
            // Now do the same for all inner inline functions, but this time binding to the
            // relevant parameter of the next containing function
            innermostBinding = bindParametersInNestedFunctions(qName, inlineFunctionStack, 0);
            return innermostBinding;
        }
        return null;
    }


    /**
     * Given that a variable is referenced within an inline function and is declared outside it,
     * add implicit parameters to all the functions that appear in the containment stack between
     * the declaration and the reference, in each case binding the value of the argument to the inner
     * function to the corresponding declared parameter in its containing function.
     *
     * @param qName               the name of the variable
     * @param inlineFunctionStack the stack of nested inline functions
     * @param start               the position in this stack of the function that contains the variable
     *                            declaration
     * @return a binding to the relevant (newly declared) parameter of the innermost function.
     */

    private static LocalBinding bindParametersInNestedFunctions(
            StructuredQName qName, IndexedStack<InlineFunctionDetails> inlineFunctionStack, int start) {
        InlineFunctionDetails details = inlineFunctionStack.get(start);
        List<UserFunctionParameter> params = details.implicitParams;
        for (UserFunctionParameter param : params) {
            if (param.getVariableQName().equals(qName)) {
                // The variable reference corresponds to a parameter of an outer inline function
                // We potentially need to add implicit parameters to any inner inline functions, and
                // bind the variable reference to the innermost of these implicit parameters
                LocalBinding b2 = param;
                for (int bs = start + 1; bs <= inlineFunctionStack.size() - 1; bs++) {
                    details = inlineFunctionStack.get(bs);
                    boolean found = false;
                    for (int p = 0; p < details.outerVariablesUsed.size() - 1; p++) {
                        if (details.outerVariablesUsed.get(p) == param) {
                            // the inner function already uses the outer variable
                            b2 = details.implicitParams.get(p);
                            found = true;
                            break;
                        }
                    }
                    if (!found) {
                        // Need to add an implicit parameter to the inner function
                        details.outerVariablesUsed.add(param);
                        UserFunctionParameter ufp = new UserFunctionParameter();
                        ufp.setVariableQName(qName);
                        ufp.setRequiredType(param.getRequiredType());
                        details.implicitParams.add(ufp);
                        b2 = ufp;
                    }
                }
                if (b2 != null) {
                    return b2;
                }
            }
        }
        return null;
    }


    public Expression parseFocusFunction(AnnotationList annotations) throws XPathException {
        checkLanguageVersion40();
        //Tokenizer t = getTokenizer();
        int offset = t.currentTokenStartOffset;

        InlineFunctionDetails details = new InlineFunctionDetails();
        details.outerVariables = new IndexedStack<>();
        for (LocalBinding lb : getRangeVariables()) {
            details.outerVariables.push(lb);
        }
        details.outerVariablesUsed = new ArrayList<>(4);
        details.implicitParams = new ArrayList<>(4);
        inlineFunctionStack.push(details);
        setRangeVariables(new IndexedStack<>());
        nextToken();
        List<UserFunctionParameter> params = new ArrayList<>(1);
        SequenceType resultType = SequenceType.ANY_SEQUENCE;

        StructuredQName argQName = new StructuredQName("saxon", NamespaceUri.SAXON, "dot");

        UserFunctionParameter arg = new UserFunctionParameter();
        arg.setRequiredType(SequenceType.ANY_SEQUENCE);
        arg.setVariableQName(argQName);
        arg.setSlotNumber(0);
        params.add(arg);

        Expression body;
        if (t.currentToken == Token.RCURLY) {
            nextToken();
            body = Literal.makeEmptySequence();
        } else {
            body = parseExpression();
            readToken(Token.RCURLY);
            body.setRetainedStaticContext(getStaticContext().makeRetainedStaticContext());

            LocalVariableReference ref = new LocalVariableReference(arg);
            body = new ContextValueSetter(ref, body);
        }
        Expression result = makeInlineFunctionValue(this, AnnotationList.EMPTY, details, params, resultType, body);

        setLocation(result, offset);
        // restore the previous stack of range variables
        setRangeVariables(details.outerVariables);
        inlineFunctionStack.pop();
        return result;
    }


    /* Must be in alphabetical order, since a binary search is used */

    private final static String[] reservedFunctionNames31 = new String[]{
            "array", "attribute", "comment", "document-node", "element", "empty-sequence", "function", "if", "item", "map",
            "namespace-node", "node", "processing-instruction", "schema-attribute", "schema-element", "switch", "text", "typeswitch"
    };

    private final static String[] reservedFunctionNames40 = new String[]{
            "attribute", "comment", "document-node", "element", "empty-sequence", "fn", "function", "if", "item",
            "namespace-node", "node", "processing-instruction", "schema-attribute", "schema-element", "switch", "text", "typeswitch"
    };

    /**
     * Check whether a function name is reserved in XPath 3.1 or 4.0 (when unprefixed)
     *
     * @param name the function name (the local-name as a string)
     * @param version set to 31 for XPath 3.1, 40 for XPath 4.0
     * @return true if the function name is reserved
     */

    public static boolean isReservedFunctionName(String name, int version) {
        int x = Arrays.binarySearch(version>=40 ? reservedFunctionNames40 : reservedFunctionNames31, name);
        return x >= 0;
    }


    //////////////////////////////////////////////////////////////////////////////////
    // Routines for handling range variables
    //////////////////////////////////////////////////////////////////////////////////

    /**
     * Get the stack of in-scope range variables
     *
     * @return the stack of variables
     */

    public IndexedStack<LocalBinding> getRangeVariables() {
        return rangeVariables;
    }

    /**
     * Set a new stack of in-scope range variables
     *
     * @param variables the stack of variables
     */

    public void setRangeVariables(IndexedStack<LocalBinding> variables) {
        this.rangeVariables = variables;
    }

    /**
     * Declare a range variable (record its existence within the parser).
     * A range variable is a variable declared within an expression, as distinct
     * from a variable declared in the context.
     *
     * @param declaration the variable declaration to be added to the stack
     */

    public void declareRangeVariable(LocalBinding declaration) {
        rangeVariables.push(declaration);
    }

    /**
     * Note when the most recently declared range variable has gone out of scope
     */

    public void undeclareRangeVariable() {
        rangeVariables.pop();
    }

    /**
     * Locate a range variable with a given name. (By "range variable", we mean a
     * variable declared within the expression where it is used.)
     *
     * @param qName identifies the name of the range variable
     * @return null if not found (this means the variable is probably a
     * context variable); otherwise the relevant RangeVariable
     */

    /*@Nullable*/
    protected LocalBinding findRangeVariable(StructuredQName qName) {
        for (int v = rangeVariables.size() - 1; v >= 0; v--) {
            LocalBinding b = rangeVariables.get(v);
            if (b.getVariableQName().equals(qName)) {
                return b;
            }
        }
        return findOuterRangeVariable(qName);
    }

    /**
     * Set the range variable stack. Used when parsing a nested subexpression
     * inside an attribute constructor.
     *
     * @param stack the stack to be used for local variables declared within the expression
     */

    public void setRangeVariableStack(IndexedStack<LocalBinding> stack) {
        rangeVariables = stack;
    }

    /**
     * Make a NameCode, using the static context for namespace resolution
     *
     * @param qname      The name as written, in the form "[prefix:]localname"; alternatively,
     *                   a QName in Clark notation ({uri}local)
     * @param useDefault Defines the action when there is no prefix. If
     *                   true, use the default namespace URI for element names. If false,
     *                   use no namespace URI (as for attribute names).
     * @return the fingerprint, which can be used to identify this name in the
     * name pool
     * @throws XPathException if the name is invalid, or the prefix
     *                        undeclared
     */

    public final int makeFingerprint(/*@NotNull*/ String qname, boolean useDefault) throws XPathException {
        if (scanOnly) {
            return StandardNames.XML_SPACE;
        }
        try {
            NamespaceUri defaultNS = useDefault ? env.getDefaultElementNamespace() : NamespaceUri.NULL;
            StructuredQName sq = qNameParser.parse(qname, defaultNS);
            return env.getConfiguration().getNamePool().allocateFingerprint(sq.getNamespaceUri(), sq.getLocalPart());
        } catch (XPathException e) {
            grumble(e.getMessage(), e.getErrorCodeQName());
            return -1;
        }
    }

    /**
     * Make a NameCode, using the static context for namespace resolution.
     * This variant of the method does not call "grumble" to report any errors
     * to the ErrorListener, it only reports errors by throwing exceptions. This
     * allows the caller to control the message output.
     *
     * @param qname      The name as written, in the form "[prefix:]localname"
     * @param defaultUri Defines the URI to be returned if there is no prefix.
     * @return the structured QName
     * @throws XPathException if the name is invalid, or the prefix
     *                        undeclared or if the name is not a lexically valid QName
     */

    public final StructuredQName makeStructuredQNameSilently(/*@NotNull*/ String qname, NamespaceUri defaultUri)
            throws XPathException {
        if (scanOnly) {
            return NamespaceUri.SAXON.qName("dummy");
        }
        return qNameParser.parse(qname, defaultUri);
    }

    /**
     * Make a Structured QName, using the static context for namespace resolution
     *
     * @param qname      The name as written, in the form "[prefix:]localname"; alternatively, a QName in
     *                   Clark format ({uri}local)
     * @param defaultUri The URI to be used if the name is written as a localname with no prefix
     * @return the QName as an instance of StructuredQName
     * @throws XPathException if the name is invalid, or the prefix
     *                        undeclared
     */

    /*@NotNull*/
    public final StructuredQName makeStructuredQName(/*@NotNull*/ String qname, NamespaceUri defaultUri) throws XPathException {
        try {
            return makeStructuredQNameSilently(qname, defaultUri);
        } catch (XPathException err) {
            grumble(err.getMessage(), err.getErrorCodeQName());
            return NamespaceUri.NULL.qName("error");  // Not executed; here to keep the compiler happy
        }
    }

    /**
     * Make a FingerprintedQName, using the static context for namespace resolution
     *
     * @param qname      The name as written, in the form "[prefix:]localname"; alternatively, a QName in
     *                   Clark format ({uri}local)
     * @param useDefault Defines the action when there is no prefix. If
     *                   true, use the default namespace URI for element names. If false,
     *                   use no namespace URI (as for attribute names).
     * @return the fingerprinted QName
     * @throws XPathException if the name is invalid, or the prefix
     *                        undeclared
     */

    /*@NotNull*/
    public final NodeName makeNodeName(String qname, boolean useDefault) throws XPathException {
        StructuredQName sq = makeStructuredQNameSilently(qname,
                                                         useDefault ? env.getDefaultElementNamespace() : NamespaceUri.NULL);
        String prefix = sq.getPrefix();
        NamespaceUri uri = sq.getNamespaceUri();
        String local = sq.getLocalPart();
        if (uri.isEmpty()) {
            int fp = env.getConfiguration().getNamePool().allocateFingerprint(NamespaceUri.NULL, local);
            return new NoNamespaceName(local, fp);
        } else {
            int fp = env.getConfiguration().getNamePool().allocateFingerprint(uri, local);
            return new FingerprintedQName(prefix, uri, local, fp);
        }
    }


    /**
     * Make a NameTest, using the static context for namespace resolution
     *
     * @param nodeKind   the type of node required (identified by a constant in
     *                   class Type)
     * @param qname      the lexical QName of the required node; alternatively,
     *                   a QName in Clark notation ({uri}local)
     * @param useDefault true if the default namespace should be used when
     *                   the QName is unprefixed
     * @return a NameTest, representing a pattern that tests for a node of a
     * given node kind and a given name
     * @throws XPathException if the QName is invalid
     */

    /*@NotNull*/
    public NodeTest makeNameTest(int nodeKind, /*@NotNull*/ String qname, boolean useDefault)
            throws XPathException {
        NamePool pool = env.getConfiguration().getNamePool();
        NamespaceUri defaultNS = NamespaceUri.NULL;
        if (useDefault && nodeKind == Type.ELEMENT && !qname.startsWith("Q{") && !qname.contains(":")) {
            UnprefixedElementMatchingPolicy policy = env.getUnprefixedElementMatchingPolicy();
            switch (policy) {
                case DEFAULT_NAMESPACE:
                    defaultNS = env.getDefaultElementNamespace();
                    break;
                case DEFAULT_NAMESPACE_OR_NONE:
                    defaultNS = env.getDefaultElementNamespace();
                    StructuredQName q = makeStructuredQName(qname, defaultNS);
                    int fp1 = pool.allocateFingerprint(q.getNamespaceUri(), q.getLocalPart());
                    NameTest test1 = new NameTest(nodeKind, fp1, pool);
                    int fp2 = pool.allocateFingerprint(NamespaceUri.NULL, q.getLocalPart());
                    NameTest test2 = new NameTest(nodeKind, fp2, pool);
                    return new CombinedNodeTest(test1, OperatorSymbol.UNION, test2);
                case ANY_NAMESPACE:
                    if (!NameChecker.isValidNCName(StringTool.codePoints(qname))) {
                        grumble("Invalid name '" + qname + "'");
                    }
                    return new LocalNameTest(pool, nodeKind, qname);
            }
        }
        StructuredQName qName = makeStructuredQName(qname, defaultNS);
        int fp = pool.allocateFingerprint(qName.getNamespaceUri(), qName.getLocalPart());
        return new NameTest(nodeKind, fp, pool);
    }

    public QNameTest makeQNameTest(int nodeKind, String qname)
            throws XPathException {
        NamePool pool = env.getConfiguration().getNamePool();
        StructuredQName q = makeStructuredQName(qname, NamespaceUri.NULL);
        assert q != null;
        int fp = pool.allocateFingerprint(q.getNamespaceUri(), q.getLocalPart());
        return new NameTest(nodeKind, fp, pool);
    }

    /**
     * Make a NamespaceTest (name:*)
     *
     * @param nodeKind integer code identifying the type of node required
     * @param prefix   the namespace prefix, or a string in the form Q{uri}
     * @return the NamespaceTest, a pattern that matches all nodes in this
     * namespace
     * @throws XPathException if the namespace prefix is not declared
     */

    /*@NotNull*/
    public NamespaceTest makeNamespaceTest(int nodeKind, String prefix)
            throws XPathException {
        NamePool pool = env.getConfiguration().getNamePool();
        if (scanOnly) {
            // return an arbitrary namespace if we're only doing a syntax check
            return new NamespaceTest(pool, nodeKind, NamespaceUri.SAXON);
        }
        if (prefix.startsWith("Q{")) {
            String uri = prefix.substring(2, prefix.length() - 1);
            return new NamespaceTest(pool, nodeKind, NamespaceUri.of(uri));
        }
        try {
            StructuredQName sq = qNameParser.parse(prefix + ":dummy", NamespaceUri.NULL);
            return new NamespaceTest(pool, nodeKind, sq.getNamespaceUri());
        } catch (XPathException err) {
            grumble(err.getMessage(), err.getErrorCodeQName());
            return null;
        }
    }

    /**
     * Make a LocalNameTest (*:name)
     *
     * @param nodeKind  the kind of node to be matched
     * @param localName the requred local name
     * @return a LocalNameTest, a pattern which matches all nodes of a given
     * local name, regardless of namespace
     * @throws XPathException if the local name is invalid
     */

    /*@NotNull*/
    public LocalNameTest makeLocalNameTest(int nodeKind, String localName)
            throws XPathException {
        if (!NameChecker.isValidNCName(StringTool.codePoints(localName))) {
            grumble("Local name [" + localName + "] contains invalid characters");
        }
        return new LocalNameTest(env.getConfiguration().getNamePool(), nodeKind, localName);
    }

    /**
     * Set location information on an expression. At present this consists of a simple
     * line number. Needed mainly for XQuery.
     *
     * @param exp the expression whose location information is to be set
     */

    protected void setLocation(/*@NotNull*/ Expression exp) {
        setLocation(exp, t.currentTokenStartOffset);
    }

    /**
     * Set location information on an expression. At present only the line number
     * is retained. Needed mainly for XQuery. This version of the method supplies an
     * explicit offset (character position within the expression or query), which the tokenizer
     * can convert to a line number and column number.
     *
     * @param exp    the expression whose location information is to be set
     * @param offset the character position within the expression (ignoring newlines)
     */

    public void setLocation(Expression exp, int offset) {
        if (exp != null) {
            if (exp.getLocation() == null || exp.getLocation() == Loc.NONE) {
                exp.setLocation(makeLocation(offset));
            }
        }
    }

    /**
     * Make a location object corresponding to a specified offset in the query
     * @param offset the offset (character position) in the query
     * @return an object containing location information
     */

    public Location makeLocation(int offset) {
        int line = t.getLineNumber(offset);
        int column = t.getColumnNumber(offset);
        return makeNestedLocation(env.getContainingLocation(), line, column, null);
    }

    /**
     * Set location information on a clause of a FLWOR expression. This version of the method supplies an
     * explicit offset (character position within the expression or query), which the tokenizer
     * can convert to a line number and column number.
     *
     * @param clause the clause whose location information is to be set
     * @param offset the character position within the expression (ignoring newlines)
     */

    public void setLocation(Clause clause, int offset) {
        int line = t.getLineNumber(offset);
        int column = t.getColumnNumber(offset);
        Location loc = makeNestedLocation(env.getContainingLocation(), line, column, null);
        clause.setLocation(loc);
        clause.setPackageData(env.getPackageData());
    }

    private Location mostRecentLocation = Loc.NONE;

    public Location makeLocation() {
        if (t.getLineNumber() == mostRecentLocation.getLineNumber() &&
                t.getColumnNumber() == mostRecentLocation.getColumnNumber() &&
                ((env.getSystemId() == null && mostRecentLocation.getSystemId() == null) ||
                        env.getSystemId().equals(mostRecentLocation.getSystemId()))) {
            return mostRecentLocation;
        } else {
            int line = t.getLineNumber();
            int column = t.getColumnNumber();
            mostRecentLocation = makeNestedLocation(env.getContainingLocation(), line, column, null);
            return mostRecentLocation;
        }
    }

    /**
     * Make a Location object relative to an existing location
     * @param containingLoc the containing location
     * @param line the line number relative to the containing location (zero-based)
     * @param column the column number relative to the containing location (zero-based)
     * @param nearbyText (maybe null) expression text around the point of the error
     * @return a suitable Location object
     */

    public Location makeNestedLocation(Location containingLoc, int line, int column, String nearbyText) {
        if (containingLoc instanceof Loc &&
                containingLoc.getLineNumber() <= 1 && containingLoc.getColumnNumber() == -1 &&
                nearbyText == null) {
            // No extra information available about the container
            return new Loc(env.getSystemId(), line + 1, column + 1);
        } else {
            return new NestedLocation(containingLoc, line, column, nearbyText);
        }
    }


    /**
     * If tracing, wrap an expression in a trace instruction
     *
     * <p>NB, this no longer happens. Instead of creating trace expressions in the course of parsing and buildint
     * the expression tree, trace code is now injected into the tree after the event, when parsing is complete.
     * See {@link ExpressionTool#injectCode(Expression, CodeInjector)}.</p>
     *
     * <p>However, the method has another effect, which is to set the retainedStaticContext in the node in the
     * expression tree.</p>
     *
     * @param exp         the expression to be wrapped
     * @param qName       the name of the construct (if applicable)
     * @return the expression that does the tracing
     */

    public Expression makeTracer(Expression exp,  /*@Nullable*/ StructuredQName qName) {
        exp.setRetainedStaticContextLocally(env.makeRetainedStaticContext());
        return exp;
//        if (codeInjector != null) {
//            return codeInjector.inject(exp);
//        } else {
//            return exp;
//        }
    }

    /**
     * Test whether the current token is a given keyword.
     *
     * @param s The string to be compared with the current token
     * @return true if they are the same
     */

    public boolean isKeyword(String s) {
        return isKeyword(t.currentToken, s);
    }

    public boolean tryKeyword(String s) throws XPathException {
        boolean result = isKeyword(t.currentToken, s);
        if (result) {
            nextToken();
        }
        return result;
    }

    public boolean isKeyword(Token tok, String s) {
        return tok instanceof Token.NameToken && ((Token.NameToken)tok).getValue().equals(s);
    }

    /**
     * Ask if the current token and next token constitute a given keyword pair
     */

    public boolean isKeywordPair(String s1, String s2) {
        return isKeyword(s1) && isKeyword(t.peekAhead(), s2);
    }

    public boolean tryKeywordPair(String s1, String s2) throws XPathException {
        boolean result = isKeyword(s1) && isKeyword(t.peekAhead(), s2);
        if (result) {
            nextToken();
            nextToken();
        }
        return result;
    }


    /**
     * Set that we are parsing in "scan only" mode
     *
     * @param scanOnly true if parsing is to proceed in scan-only mode. In this mode
     *                 namespace bindings are not yet known, so no attempt is made to look up namespace
     *                 prefixes.
     */

    public void setScanOnly(boolean scanOnly) {
        this.scanOnly = scanOnly;
    }


    /**
     * Say whether an absent expression is permitted
     *
     * @param allowEmpty true if it is permitted for the expression to consist
     *                   only of whitespace and comments, in which case the result
     *                   of parsing will be an EmptySequence literal
     */

    public void setAllowAbsentExpression(boolean allowEmpty) {
        this.allowAbsentExpression = allowEmpty;
    }



    /**
     * Ask whether an absent expression is permitted
     *
     * @return true if it is permitted for the expression to consist
     * only of whitespace and comments, in which case the result
     * of parsing will be an EmptySequence literal
     */

    public boolean isAllowAbsentExpression() {
        return this.allowAbsentExpression;
    }

    /**
     * A nested location: for use with XPath expressions and queries nested within some
     * larger document. The location information comes in two parts: the location of the query
     * or expression within the containing document, and the location of an error within the
     * query or XPath expression.
     */

    public static class NestedLocation implements Location {

        private final Location containingLocation;
        private final int localLineNumber;
        private final int localColumnNumber;
        private final String nearbyText;

        /**
         * Create a NestedLocation
         *
         * @param containingLocation the location of the containing construct, typically an attribute or
         *                           text node in an XML document
         * @param localLineNumber    the line number within the containing construct, starting at zero
         * @param localColumnNumber  the column number within the containing construct, starting at zero
         * @param nearbyText         text appearing in the vicinity of the error location
         */

        public NestedLocation(Location containingLocation, int localLineNumber, int localColumnNumber, String nearbyText) {
            this.containingLocation = containingLocation.saveLocation();
            this.localLineNumber = localLineNumber;
            this.localColumnNumber = localColumnNumber;
            this.nearbyText = nearbyText;
        }

        /**
         * Get the location of the container. This is normally used for expressions nested within
         * an XML document, where the container location gives the location of the attribute or text
         * node holding the XPath expression as a whole
         *
         * @return the location of the containing expression or query
         */

        public Location getContainingLocation() {
            return containingLocation;
        }

        /**
         * Get the column number of the error within the expression or query
         *
         * @return the column number. This is generally maintained only during parsing,
         * so it will be returned as -1 (meaning not available) in the case of dynamic
         * errors. Column numbers start at 0. For expressions held within XML attributes,
         * the position is within the attribute after XML attribute-value normalization,
         * which replaces newlines by spaces and expands entity references.
         */

        @Override
        public int getColumnNumber() {
            return localColumnNumber;
        }

        /**
         * Get the system identifier of the expression's container. This will normally
         * be the URI of the document (or external entity) in which the expression appears.
         *
         * @return the system identifier of the expression's container, or null if not known
         */

        @Override
        public String getSystemId() {
            return containingLocation.getSystemId();
        }

        /**
         * Get the public identifier. This will normally be null, but is provided for
         * compatibility with SAX and JAXP interfaces
         *
         * @return the public identifier - usually null
         */

        @Override
        public String getPublicId() {
            return containingLocation.getPublicId();
        }

        /**
         * Get the local line number, that is the line number relative to the start of the
         * expression or query. For expressions held within XML attributes,
         * the position is within the attribute after XML attribute-value normalization,
         * which replaces newlines by spaces and expands entity references; the value
         * will therefore in many cases not be usable. Local line numbers start at 0.
         *
         * @return the local line number within the expression or query. Set to -1
         * if not known.
         */

        public int getLocalLineNumber() {
            return localLineNumber;
        }

        /**
         * Get the line number within the containing entity. This is the sum of the containing
         * location's line number, plus the local line number. Returns -1 if unknown.
         *
         * @return the line number within the containing entity, or -1 if unknown.
         */

        @Override
        public int getLineNumber() {
            return containingLocation.getLineNumber() + localLineNumber;
        }

        /**
         * Get text appearing near to the error (typically a syntax error) within the source
         * text of the expression or query.
         *
         * @return nearby text to the error. May be null.
         */

        public String getNearbyText() {
            return nearbyText;
        }

        /**
         * Save an immutable copy of the location information. This implementation does
         * nothing, because the object is already immutable
         *
         * @return immutable location information.
         */

        @Override
        public Location saveLocation() {
            return this;
        }
    }

}

/*

The following copyright notice is copied from the licence for xt, from which the
original version of this module was derived:
--------------------------------------------------------------------------------
Copyright (c) 1998, 1999 James Clark

Permission is hereby granted, free of charge, to any person obtaining
a copy of this software and associated documentation files (the
"Software"), to deal in the Software without restriction, including
without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to
permit persons to whom the Software is furnished to do so, subject to
the following conditions:

The above copyright notice and this permission notice shall be included
in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED ``AS IS'', WITHOUT WARRANTY OF ANY KIND, EXPRESS
OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL JAMES CLARK BE LIABLE FOR ANY CLAIM, DAMAGES OR
OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.

Except as contained in this notice, the name of James Clark shall
not be used in advertising or otherwise to promote the sale, use or
other dealings in this Software without prior written authorization
from James Clark.
---------------------------------------------------------------------------
*/