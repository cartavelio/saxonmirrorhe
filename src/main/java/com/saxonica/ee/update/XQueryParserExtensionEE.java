package com.saxonica.ee.update;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.ee.schema.CachedSchemaSource;
import com.saxonica.ee.schema.PreparedSchema;
import com.saxonica.ee.schema.SchemaCompiler;
import com.saxonica.ee.schema.sdoc.SchemaReader;
import com.saxonica.expr.XQueryParserExtensionPE;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.parser.Token;
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

            for (String hint : schemaImport.locationURIs) {
                if (targetNamespace.equals(NamespaceUri.XML) ||
                        targetNamespace.equals(NamespaceUri.FN) ||
                        targetNamespace.equals(NamespaceUri.SCHEMA_INSTANCE)) {
                    Schema xmlSchema = config.getSchemaForBuiltInNamespace(targetNamespace);
                    sources.add(new CachedSchemaSource((PreparedSchema) xmlSchema));
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
}

// Copyright (c) 2008-2023 Saxonica Limited


