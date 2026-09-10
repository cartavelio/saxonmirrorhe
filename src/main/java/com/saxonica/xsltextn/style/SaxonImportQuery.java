package com.saxonica.xsltextn.style;

import net.sf.saxon.Configuration;
import net.sf.saxon.expr.Component;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.instruct.GlobalVariable;
import net.sf.saxon.expr.instruct.UserFunction;
import net.sf.saxon.om.AttributeInfo;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.om.NodeName;
import net.sf.saxon.query.*;
import net.sf.saxon.style.*;
import net.sf.saxon.trans.CompilerInfo;
import net.sf.saxon.trans.SaxonErrorCode;
import net.sf.saxon.trans.Visibility;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.Whitespace;


/**
 * The class implements a saxon:import-query declaration in a stylesheet. This
 * declaration imports an XQuery library module and adds the functions defined
 * in that module to the static context, making them available for calling from
 * XPath expressions in the stylesheet.
 */

public class SaxonImportQuery extends StyleElement {

    private String href;
    private NamespaceUri moduleURI;

    /**
     * Ask whether the element is in the XSLT namespace
     *
     * @return true if the element is in the XSLT namespace
     */
    @Override
    public boolean isInXsltNamespace() {
        return false;
    }

    @Override
    protected void prepareAttributes() {

        // Avoid reporting errors twice
        if (href != null || moduleURI != null) {
            return;
        }

        for (AttributeInfo att : attributes()) {
            NodeName attName = att.getNodeName();
            String f = attName.getDisplayName();
            String value = att.getValue();
            if (f.equals("href")) {
                href = Whitespace.trim(value);
            } else if (f.equals("namespace")) {
                moduleURI = NamespaceUri.of(Whitespace.trim(value));
            } else {
                checkUnknownAttribute(attName);
                moduleURI = NamespaceUri.NULL;   // for error recovery path
            }
        }

        if (moduleURI == null) {
            reportAbsence("namespace");
        }

    }

    @Override
    public void index(ComponentDeclaration decl, PrincipalStylesheetModule top) throws XPathException {
        prepareAttributes();

        if (moduleURI == null) {
            // error already reported
            return;
        }

        try {
            PrincipalStylesheetModule top1 = getPrincipalStylesheetModule();
            top1.getStylesheetPackage().markNonExportable("Uses saxon:import-query", SaxonErrorCode.SXST0069);
            QueryModule importedModule = getQueryLibrary(moduleURI);
            if (importedModule == null) {
                if (href == null) {
                    compileError("Cannot locate query module for namespace " + moduleURI, "XQST0059");
                    return;
                }
                importedModule = loadModule();
            }

            // Do the importing
            // First import the global variables

            for (GlobalVariable queryVar : importedModule.getPackageData().getGlobalVariableList()) {
                if (!queryVar.isPrivate() && top1.getGlobalVariableBinding(queryVar.getVariableQName()) != null) {
                    throw new XPathException("Duplicate global variable declaration "
                                                     + queryVar.getVariableQName().getDisplayName(), "XQST0049");
                }
                top1.addGlobalVariable(queryVar, queryVar.isPrivate() ? Visibility.HIDDEN : Visibility.PRIVATE);
            }

            // Now import the functions

            for (XQueryFunction def : importedModule.getGlobalFunctionLibrary().getFunctionDefinitions()) {
                // don't import functions transitively
                if (def.getFunctionName().hasURI(moduleURI) && !def.isPrivate()) {
                    top1.declareXQueryFunction(def);
                    UserFunction fn = def.getUserFunction();
                    StylesheetPackage pack = top1.getStylesheetPackage();
                    Component component = fn.makeDeclaringComponent(Visibility.PRIVATE, pack);
                    if (fn.getPackageData() == null) {
                        fn.setPackageData(pack);
                    }
                    pack.getComponentIndex().put(def.getIdentificationKey(), component);
                }
            }
        } catch (XPathException err) {
            compileError(err);
        }
    }

    @Override
    public void validate(ComponentDeclaration decl) throws XPathException {
        checkEmpty();
        checkTopLevel("XTSE0010", false);
        getConfiguration().checkLicensedFeature(Configuration.LicenseFeature.PROFESSIONAL_EDITION,
                                                "saxon:import-query",
                                                getPackageData().getLocalLicenseId());
    }

    private QueryLibrary getQueryLibrary(NamespaceUri namespace) {
        CompilerInfo info = getCompilation().getCompilerInfo();
        Iterable<QueryLibrary> libs = info.getQueryLibraries();
        if (libs != null) {
            for (QueryLibrary lib : libs) {
                if (lib.getModuleNamespace().equals(namespace)) {
                    return lib;
                }
            }
        }
        return null;
    }

    /**
     * Load a query library module
     *
     * @return the QueryModule object representing the loaded library module
     * @throws XPathException if the query module contains syntax errors
     */

    private QueryModule loadModule() throws XPathException {
        // Create a dummy main query module and compile it
        StaticQueryContext sqc = getConfiguration().newStaticQueryContext();
        //sqc.setExecutable(getExecutable());
        sqc.setBaseURI(getBaseURI());
        sqc.setErrorReporter(getCompilation().getCompilerInfo().getErrorReporter());
        String mainModule = "import module namespace m = \"" + moduleURI + "\" at \"" + href + "\"; ()";
        XQueryExpression exp = sqc.compileQuery(mainModule);
        return exp.getMainModule();
    }

    /**
     * Compile this XSLT declaration. This always fails for this class.
     *
     * @param exec the Executable
     * @param decl the containing top-level declaration, for example xsl:function or xsl:template
     * @return a compiled expression or null.
     * @throws XPathException (always)
     */

    /*@Nullable*/
    @Override
    public Expression compile(Compilation exec, ComponentDeclaration decl) throws XPathException {
        return null;
    }
}

// Copyright (c) 2009-2022 Saxonica Limited