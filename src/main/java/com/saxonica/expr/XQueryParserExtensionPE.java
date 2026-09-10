package com.saxonica.expr;

import net.sf.saxon.expr.parser.Token;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.query.XQueryParser;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.ItemType;

/**
 * Parser extension for syntax in XQuery Update and/or XQuery 3.0 that is supported
 * only in Saxon-PE or -EE.
 */
public class XQueryParserExtensionPE extends SaxonSyntaxExtension {


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

    @Override
    public void parseTypeAliasDeclaration(XQueryParser p) throws XPathException {
        p.nextToken();
        p.expect(Token.NAME);
        StructuredQName typeName = p.makeStructuredQName(p.getTokenizer().currentTokenValue, NamespaceUri.NULL);
        p.nextToken();
        p.expect(Token.EQUALS);
        p.nextToken();
        ItemType typeDef = p.parseItemType();
        p.getStaticContext().getPackageData().obtainTypeAliasManager().registerTypeAlias(typeName, typeDef);
    }



}

// Copyright (c) 2020-2022 Saxonica Limited


