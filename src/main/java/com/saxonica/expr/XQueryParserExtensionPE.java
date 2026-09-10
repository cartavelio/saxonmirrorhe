package com.saxonica.expr;

import net.sf.saxon.expr.parser.Token;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.query.XQueryParser;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.ItemType;

/**
 * Parser extension for syntax in XQuery Update and/or XQuery 3.0 that is supported
 * only in Saxon-PE or -EE. This includes all syntax needed for the HOF feature. Other XQuery 3.0
 * extensions, such as the switch expression and FLWOR extensions, are moved into the
 * (HE) QueryParser from Saxon 9.6.
 */
public class XQueryParserExtensionPE extends HofParserExtension {


    public XQueryParserExtensionPE() {
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
//        return allowed;
//    }

    protected boolean isReservedInQuery(String uri) {
        return NamespaceConstant.isReservedInQuery31(uri);
    }

    /**
     * Parse a type alias declaration. Allowed only in Saxon-PE and higher,
     * and only when Saxon syntax extensions are enabled
     * Syntax: declare type QName = ItemType;
     *
     * @throws XPathException if parsing fails
     */

    public void parseTypeAliasDeclaration(XQueryParser p) throws XPathException {
        p.nextToken();
        p.expect(Token.NAME);
        StructuredQName typeName = p.makeStructuredQName(p.getTokenizer().currentTokenValue, "");
        p.nextToken();
        p.expect(Token.EQUALS);
        p.nextToken();
        ItemType typeDef = p.parseItemType();
        p.getStaticContext().getPackageData().obtainTypeAliasManager().registerTypeAlias(typeName, typeDef);
    }



}

// Copyright (c) 2008-2014 Saxonica Limited.


