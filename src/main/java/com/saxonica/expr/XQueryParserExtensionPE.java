package com.saxonica.expr;

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
     * Parse a type alias declaration. Allowed only in Saxon-PE and higher,
     * and only when Saxon syntax extensions are enabled
     * Syntax: declare type QName = ItemType;
     *
     * @throws XPathException if parsing fails
     */

    @Override
    public void parseItemTypeDeclaration(XQueryParser p) throws XPathException {
        String name = p.readName();
        StructuredQName typeName = p.makeStructuredQName(name, NamespaceUri.NULL);
        p.readKeyword("as");
        ItemType typeDef = p.parseItemType();
        p.getStaticContext().getPackageData().obtainTypeAliasManager().registerTypeAlias(typeName, typeDef);
    }



}

// Copyright (c) 2020-2023 Saxonica Limited


