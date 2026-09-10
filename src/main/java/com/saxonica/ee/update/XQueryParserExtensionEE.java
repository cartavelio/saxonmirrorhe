package com.saxonica.ee.update;


import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.ee.schema.CachedSchemaSource;
import com.saxonica.ee.schema.PreparedSchema;
import com.saxonica.ee.schema.SchemaCompiler;
import com.saxonica.ee.schema.sdoc.SchemaReader;
import com.saxonica.expr.XQueryParserExtensionPE;

import com.saxonica.expr.*;
import com.saxonica.expr.DeepLookupExpression;

import net.sf.saxon.Configuration;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.LookupExpression;
import net.sf.saxon.expr.StringLiteral;
import net.sf.saxon.expr.parser.Tokenizer;
import net.sf.saxon.expr.parser.XPathParser;
import net.sf.saxon.lib.AugmentedSource;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.lib.Validation;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.query.Annotation;
import net.sf.saxon.query.AnnotationList;
import net.sf.saxon.query.QueryModule;
import net.sf.saxon.query.XQueryParser;
import net.sf.saxon.trans.LicenseException;
import net.sf.saxon.trans.XPathException;

import net.sf.saxon.type.Schema;
import net.sf.saxon.type.SchemaException;
import net.sf.saxon.value.Whitespace;

import javax.xml.transform.Source;
import java.util.ArrayList;
import java.util.List;

import net.sf.saxon.value.SequenceType;


/**
 * Parser extension for syntax in XQuery Update and/or XQuery 3.0 that is supported
 * only in -EE. This includes all syntax needed for XQuery Update.
 */
public class XQueryParserExtensionEE extends XQueryParserExtensionPE {

    private boolean acceptUpdateSyntax = false;
    private boolean foundRevalidationDeclaration = false;

    private SchemaCompiler schemaCompiler;
    private XQueryParser parser;


    public XQueryParserExtensionEE(XQueryParser parser, boolean updating, com.saxonica.ee.schema.SchemaCompiler schemaCompiler) {
        acceptUpdateSyntax = updating;
        this.parser = parser;
        this.schemaCompiler = schemaCompiler;

    }


    /**

     * Supporting code for lookup expressions (A?B) where B is the wildcard "*"
     *
     * @param lhs     the LHS operand of the lookup expression
     * @param allow40
     * @return the result of parsing the expression
     */
    @Override
    public Expression lookupStar(Expression lhs,
                                 LookupExpression.Modifier modifier,
                                 SequenceType type,
                                 boolean deep, boolean allow40) {
        if (deep) {
            return new DeepLookupAllExpression(lhs, modifier, type);
        } else {
            return new LookupAllExpression40(lhs, modifier, type);
        }
    }

    /**
     * Supporting code for lookup expressions (A?B) where B is an NCName
     *
     * @param config
     * @param lhs    the LHS operand of the lookup expression
     * @param rhs    the RHS operand of the lookup expression
     * @param deep   true if the operator is "??" (in 4.0)
     * @return the result of parsing the expression
     */

    public Expression lookupName(Configuration config, Expression lhs, LookupExpression.Modifier modifier, String rhs, boolean deep, boolean allow40) {
        if (deep) {
            return new DeepLookupExpression(lhs, modifier, new StringLiteral(rhs));
        } else {
            return new LookupExpression(lhs, modifier, new StringLiteral(rhs));
        }
    }


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
            } else if (p.isKeywordPair("insert", "nodes")) {
                return UpdateParser.parseInsertExpression(p);
            } else if (p.isKeywordPair("delete", "node")) {
                return UpdateParser.parseDeleteExpression(p);
            } else if (p.isKeywordPair("delete", "nodes")) {
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



    public void processSchemaImports(QueryModule module, List<XQueryParser.Import> schemaImports) throws XPathException {
        Configuration config = module.getConfiguration();
        try {
            config.checkLicensedFeature(Configuration.LicenseFeature.ENTERPRISE_XQUERY,
                                        "import schema",
                                        module.getPackageData().getLocalLicenseId());
        } catch (LicenseException err) {
            throw new XPathException(err).withErrorCode("XTSE1650");   //TODO error code
        }
        if (schemaCompiler == null) {
            schemaCompiler = new SchemaCompiler((EnterpriseConfiguration)module.getConfiguration());
        }
        List<Source> sources = new ArrayList<>();
        for (XQueryParser.Import schemaImport : schemaImports) {
            try {
                gatherSources(module, schemaImport, sources);
            } catch (XPathException e) {
                throw e.maybeWithLocation(parser.makeLocation(schemaImport.offset));
            }
        }
        PreparedSchema schema = schemaCompiler.loadSources(sources);
        module.getPackageData().setImportedSchema(schema);
    }


    private void gatherSources(QueryModule env, XQueryParser.Import schemaImport, List<Source> sources) throws XPathException {
        try {
            Configuration config = env.getConfiguration();

            String namespace = Whitespace.trim(schemaImport.namespaceURI.toString());
            if (namespace == null) {
                namespace = "";
            } else {
                namespace = namespace.trim();
            }
            NamespaceUri targetNamespace = NamespaceUri.of(namespace);
            Schema preLoaded = env.getUserQueryContext().getPreLoadedSchema();
            if (preLoaded != null) {
                if (preLoaded.includesTargetNamespace(targetNamespace)) {
                    sources.add(new CachedSchemaSource(preLoaded));
                    return;
                }
            }
            if (namespace.equals(NamespaceConstant.XML) ||
                    namespace.equals(NamespaceConstant.FN) ||
                    namespace.equals(NamespaceConstant.SCHEMA_INSTANCE)) {
                targetNamespace = NamespaceUri.of(namespace);
                Schema xmlSchema = config.getSchemaForBuiltInNamespace(targetNamespace);
                sources.add(new CachedSchemaSource(xmlSchema));
                return;
            }

            if (schemaImport.locationURIs.isEmpty()) {
                throw new XPathException("Unable to locate any schema for target namespace "
                                                 + schemaImport.namespaceURI, "XQST0059");
            }

            for (String hint : schemaImport.locationURIs) {
                if (targetNamespace.equals(NamespaceUri.XML) ||
                        targetNamespace.equals(NamespaceUri.FN) ||
                        targetNamespace.equals(NamespaceUri.SCHEMA_INSTANCE)) {
                    Schema xmlSchema = config.getSchemaForBuiltInNamespace(targetNamespace);
                    sources.add(new CachedSchemaSource(xmlSchema));
                    continue;
                }

                Source source = SchemaReader.getSource(env.getStaticBaseURI(), hint, schemaCompiler, targetNamespace);
                if (source != null) {

                    AugmentedSource as = AugmentedSource.makeAugmentedSource(source);
                    as.setRequiredTargetNamespace(targetNamespace);
                    sources.add(as);
                }
            }
        } catch (SchemaException err) {
            throw XPathException.makeXPathException(err);
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


        UpdateTarget target;
        parser.nextToken();
        if (parser.isKeyword("map")) {
            target = UpdateTarget.MAP;
        } else if (parser.isKeyword("array")) {
            target = UpdateTarget.ARRAY;
        } else {
            parser.grumble("Expected 'map' or 'array' after 'update'");
            return null;
        }

        parser.nextToken();
        Expression rootExpr = parser.parseExprSingle();
        parser.readKeyword("replace");
        Expression selectExpr = parser.parseExprSingle();
        parser.readKeyword("with");
        Expression actionExpr = parser.parseExprSingle();
        return new DeepUpdate2(target, rootExpr, selectExpr, actionExpr);
    }


}

// Copyright (c) 2008-2023 Saxonica Limited


