package com.saxonica.ee.update;

import com.saxonica.expr.DeepUpdate2;
import com.saxonica.expr.XQueryParserExtensionPE;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.parser.Tokenizer;
import net.sf.saxon.expr.parser.XPathParser;
import net.sf.saxon.lib.Validation;
import net.sf.saxon.query.Annotation;
import net.sf.saxon.query.AnnotationList;
import net.sf.saxon.query.QueryModule;
import net.sf.saxon.query.XQueryParser;
import net.sf.saxon.trans.XPathException;

/**
 * Parser extension for syntax in XQuery Update and/or XQuery 3.0 that is supported
 * only in -EE. This includes all syntax needed for XQuery Update.
 */
public class XQueryParserExtensionEE extends XQueryParserExtensionPE {

    private boolean acceptUpdateSyntax = false;
    private boolean foundRevalidationDeclaration = false;


    public XQueryParserExtensionEE(boolean updating) {
        acceptUpdateSyntax = updating;
    }


    /**
     * Get the permitted set of standard functions in this environment
     */

//    public int getPermittedFunctions() {
//        int allowed = BuiltInFunctionSet.CORE;
//        if (allowXPath30Syntax) {
//            allowed |= BuiltInFunctionSet.XPATH30;
//        }
//        if (allowXPath31Syntax) {
//            allowed |= BuiltInFunctionSet.XPATH31;
//        }
//        if (acceptUpdateSyntax) {
//            allowed |= BuiltInFunctionSet.XQUPDATE;
//        }
//        return allowed;
//    }

    /**
     * Parse the "declare revalidation" declaration (XQuery Update).
     * Syntax: &lt;"declare" "revalidation"&gt; ("strict" | "lax" | "skip")
     *
     * @throws XPathException if the declaration is invalid or duplicated
     *
     */
    @Override
    public void parseRevalidationDeclaration(XQueryParser p) throws XPathException {
        checkXQueryUpdate(p);
        if (foundRevalidationDeclaration) {
            p.grumble("declare revalidation appears more than once", "XUST0003");
        }
        foundRevalidationDeclaration = true;
        p.nextToken();
        String mode = p.readName();
        int val;
        if ("strict".equals(mode)) {
            val = Validation.STRICT;
        } else if ("lax".equals(mode)) {
            val = Validation.LAX;
        } else if ("skip".equals(mode)) {
            val = Validation.SKIP;
        } else {
            p.grumble("revalidation mode must be 'strict', 'lax', or 'skip'");
            val = Validation.SKIP;
        }
        ((QueryModule) p.getStaticContext()).setRevalidationMode(val);
    }

    /**
     * Parse an updating function declaration (allowed in XQuery Update only)
     */

    @Override
    public void parseUpdatingFunctionDeclaration(XQueryParser p) throws XPathException {
        p.parseFunctionDeclaration(AnnotationList.singleton(new Annotation(Annotation.UPDATING)));
    }

    /**
     * Parse an ExprSingle
     *
     * @return the resulting subexpression
     * @throws XPathException
     *          if any error is encountered
     */

    /*@NotNull*/
    @Override
    protected Expression parseExtendedExprSingle(XPathParser p) throws XPathException {
        if (acceptUpdateSyntax) {
            if (p.isKeyword("copy")) {
                return UpdateParser.parseTransformExpression(p);
            } else if (p.isKeywordPair("insert", "node")) {
                return UpdateParser.parseInsertExpression(p);
            } else if (p.isKeywordPair("delete", "node")) {
                return UpdateParser.parseDeleteExpression(p);
            } else if (p.isKeywordPair("replace", "node")) {
                return UpdateParser.parseReplaceNodeExpression(p);
            } else if (p.isKeywordPair("replace", "value")) {
                return UpdateParser.parseReplaceValueExpression(p);
            } else if (p.isKeywordPair("rename", "node")) {
                return UpdateParser.parseRenameExpression(p);
            }
        }
        return super.parseExtendedExprSingle(p);
    }

    private void checkXQueryUpdate(XPathParser p) throws XPathException {
        if (!acceptUpdateSyntax) {
            p.grumble("XQuery update syntax encountered in a non-updating query");
        }
    }

    /**
     * Parse an update map|array Expression.
     *
     * @return the parsed expression; except that this version of the method always
     * throws an exception
     * @throws XPathException if a static error is found
     */

    /*@NotNull*/
    @Override
    public Expression parseDeepUpdateExpression(XQueryParser parser) throws XPathException {
        Tokenizer t = parser.getTokenizer();
        if (!parser.isAllowXPath40Syntax()) {
            parser.grumble("update map/array syntax requires 4.0 to be enabled");
        }

        parser.nextToken();
        Expression rootExpr = parser.parseExprSingle();
        if (!parser.isKeyword("replace")) {
            parser.grumble("expected keyword 'replace'");
        }
        parser.nextToken();
        Expression selectExpr = parser.parseExprSingle();
        if (!parser.isKeyword("with")) {
            parser.grumble("expected keyword 'with");
        }
        parser.nextToken();
        Expression actionExpr = parser.parseExprSingle();

        return new DeepUpdate2(-12345678, rootExpr, selectExpr, actionExpr);   // TODO: fixme
    }


}

// Copyright (c) 2008-2023 Saxonica Limited


