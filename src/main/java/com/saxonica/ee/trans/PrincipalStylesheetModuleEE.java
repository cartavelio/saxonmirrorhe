
package com.saxonica.ee.trans;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.ee.schema.CachedSchemaSource;
import com.saxonica.ee.schema.PreparedSchema;
import com.saxonica.ee.schema.SchemaCompiler;
import com.saxonica.ee.schema.XSLImportSchemaEE;
import com.saxonica.ee.stream.Posture;
import com.saxonica.ee.stream.PostureAndSweep;
import com.saxonica.ee.stream.Streamability;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.StaticProperty;
import net.sf.saxon.expr.instruct.AttributeSet;
import net.sf.saxon.expr.instruct.GlobalParam;
import net.sf.saxon.expr.instruct.GlobalVariable;
import net.sf.saxon.style.*;
import net.sf.saxon.trans.CompilerInfo;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.AnyItemType;

import javax.xml.transform.Source;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

/**
 * This version of the StylesheetPackage class represents a "real" package, that is one with
 * an xsl:package element, which potentially uses other packages and overrides selected
 * components in those packages.
 */

public class PrincipalStylesheetModuleEE extends PrincipalStylesheetModule {

    private final Map<Expression, GlobalVariable> indexOfVariablesByInitializer = new HashMap<>();

    public PrincipalStylesheetModuleEE(XSLPackage node) throws XPathException {
        super(node);
    }

    /**
     * Process import-schema declarations
     *
     * @throws net.sf.saxon.trans.XPathException
     *          if errors are detected
     */

    @Override
    protected void importSchemata() throws XPathException {

        CompilerInfo compilerInfo = getRootElement().getCompilation().getCompilerInfo();
        PreparedSchema preloadedSchema = (PreparedSchema) compilerInfo.getPreLoadedSchema();
//        if (preloadedSchema != null) {
//            getStylesheetPackage().setImportedSchema(preloadedSchema);
//        }

        boolean importsSchemas = false;
        for (ComponentDeclaration decl : topLevel) {
            if (decl.getSourceElement() instanceof XSLImportSchema) {
                importsSchemas = true;
                break;
            }
        }
        if (importsSchemas || preloadedSchema != null) {
            EnterpriseConfiguration config = (EnterpriseConfiguration) getConfiguration();
            List<Source> sources = new ArrayList<>();
            if (preloadedSchema != null) {
                sources.add(new CachedSchemaSource(preloadedSchema));
            }
            SchemaCompiler schemaCompiler = new SchemaCompiler(config);

            // Scan the declarations in reverse order, that is, highest precedence first
            if (importsSchemas) {
                for (int i = topLevel.size() - 1; i >= 0; i--) {
                    ComponentDeclaration decl = topLevel.get(i);
                    if (decl.getSourceElement() instanceof XSLImportSchemaEE) {
                        XSLImportSchemaEE is = (XSLImportSchemaEE) decl.getSourceElement();
                        Source source = is.asSource(schemaCompiler);
                        if (source != null) {
                            sources.add(source);
                        }
                    }
                    // TODO: this doesn't belong here
                    if (decl.getSourceElement() instanceof XSLItemType) {
                        ((XSLItemType) decl.getSourceElement()).indexTypeAlias(decl, this);
                    }
                }
            }
            if (!sources.isEmpty()) {
                PreparedSchema assembledSchema = schemaCompiler.loadSources(sources);
                getStylesheetPackage().setImportedSchema(assembledSchema);
            }
            // Now seal all the schemas that have been imported to guarantee consistency with instance documents
//            for (NamespaceUri ns : getStylesheetPackage().getSchemaNamespaces()) {
//                config.sealNamespace(ns);
//            }

        }
    }

    /**
     * Check the streamability of an attribute set declared within this stylesheet module.
     * (Null implementation for Saxon-HE).
     *
     * @param aSet       the attribute set to be checked (the method is called only
     *                   if the attribute set is declared streamable)
     * @throws XPathException if the streamability rules are not satisifed
     */

    @Override
    protected void checkStreamability(AttributeSet aSet) throws XPathException {
        List<String> reasons = new ArrayList<>();
        Expression body = aSet.getBody();
        if (body != null) {
            ContextItemStaticInfoEE info =
                    (ContextItemStaticInfoEE)getConfiguration().makeContextItemStaticInfo(AnyItemType.getInstance(), true);
            info.setContainer(aSet);
            PostureAndSweep s1 = Streamability.getStreamability(body, info, reasons);
            if (s1.getPosture() != Posture.GROUNDED) {
                String message = "Attribute set " + aSet.getObjectName().getDisplayName() +
                    " is declared streamable but is not actually streamable. ";
                for (String s : reasons) {
                    message += "\n    " + s;
                }
                XPathException err = new XPathException(message, "XTSE3430");
                err.setLocator(aSet);
                compileError(err);
            }
        }
    }
    
    private static boolean isVariableReusable(GlobalVariable variable) {
        Expression select = variable.getBody();
        return select != null && !variable.isAssignable() && !(variable instanceof GlobalParam) &&
                ((select.getDependencies() & StaticProperty.DEPENDS_ON_STATIC_CONTEXT) == 0);
    }

    @Override
    public void addGlobalVariable(GlobalVariable variable) {
        super.addGlobalVariable(variable);
        if (isVariableReusable(variable)) {
            // Note: we're avoiding commoning-up expressions that have dependencies on the static context,
            // e.g. static base URI or default collation. It would probably be better to ensure that Expression.equals()
            // never treats two expressions as equal if the static context is significantly different.

            // TODO: before calling addGlobalVariable(), the GlobalExtractor calls getEquivalentVariable(), so this
            // looks redundant.
            indexOfVariablesByInitializer.put(variable.getBody(), variable);
        }
    }

    @Override
    public GlobalVariable getEquivalentVariable(Expression select) {
        return indexOfVariablesByInitializer.get(select);
    }
}

// Copyright (c) 2018-2023 Saxonica Limited