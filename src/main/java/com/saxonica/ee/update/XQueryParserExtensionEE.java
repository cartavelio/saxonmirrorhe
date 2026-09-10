package com.saxonica.ee.update;

import com.saxonica.expr.XQueryParserExtensionPE;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.parser.Token;
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
        p.expect(Token.NAME);
        int val;
        Tokenizer t = p.getTokenizer();
        if ("strict".equals(t.currentTokenValue)) {
            val = Validation.STRICT;
        } else if ("lax".equals(t.currentTokenValue)) {
            val = Validation.LAX;
        } else if ("skip".equals(t.currentTokenValue)) {
            val = Validation.SKIP;
        } else {
            p.grumble("revalidation mode must be 'strict', 'lax', or 'skip'");
            val = Validation.SKIP;
        }
        ((QueryModule) p.getStaticContext()).setRevalidationMode(val);
        p.nextToken();
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
            Tokenizer t = p.getTokenizer();
            switch (t.currentToken) {
                case Token.COPY:
                    return UpdateParser.parseTransformExpression(p);
                case Token.INSERT_NODE:
                    return UpdateParser.parseInsertExpression(p);
                case Token.DELETE_NODE:
                    return UpdateParser.parseDeleteExpression(p);
                case Token.REPLACE_NODE:
                    return UpdateParser.parseReplaceNodeExpression(p);
                case Token.REPLACE_VALUE:
                    return UpdateParser.parseReplaceValueExpression(p);
                case Token.RENAME_NODE:
                    return UpdateParser.parseRenameExpression(p);
                default:
                    break;
                    // fall through;
            }
        }
        return super.parseExtendedExprSingle(p);
    }

    private void checkXQueryUpdate(XPathParser p) throws XPathException {
        if (!acceptUpdateSyntax) {
            p.grumble("XQuery update syntax encountered in a non-updating query");
        }
    }
    
}

// Copyright (c) 2008-2022 Saxonica Limited


