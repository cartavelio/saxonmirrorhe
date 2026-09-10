package com.saxonica.expr;

import net.sf.saxon.expr.instruct.GlobalParam;
import net.sf.saxon.expr.instruct.GlobalVariable;
import net.sf.saxon.query.*;
import net.sf.saxon.trans.XPathException;

/**
 * Concrete implementation of a separately-compiled XQuery library
 */
public class QueryLibraryImpl extends QueryLibrary {

    public QueryLibraryImpl(StaticQueryContext sqc) throws XPathException {
        super(sqc);
    }

    /**
     * Link this library module to a module that imports it
     *
     * @param importer the importing module (a user of the library)
     */

    @Override
    public void link(QueryModule importer) throws XPathException {
        // Import the global functions and variables from this library to the importing module
        XQueryFunctionLibrary lib = getGlobalFunctionLibrary();
        for (XQueryFunction fn : lib.getFunctionDefinitions()) {
            importer.getTopLevelModule().getGlobalFunctionLibrary().declareFunction(fn);
        }
        for (GlobalVariable libvar : getImportedGlobalVariables()) {
            // Need to create a copy of the variable, because its slot number will be different for each query
            // that the variable is used in
            GlobalVariable newvar = libvar instanceof GlobalParam ? new GlobalParam() : new GlobalVariable();
            newvar.setPackageData(importer.getPackageData());
            newvar.setVariableQName(libvar.getVariableQName());
            newvar.setRequiredType(libvar.getRequiredType());
            newvar.setBody(libvar.getBody());
            newvar.setLineNumber(libvar.getLineNumber());
            newvar.setColumnNumber(libvar.getColumnNumber());
            newvar.setSystemId(libvar.getSystemId());
            newvar.setOriginalVariable(libvar);
            importer.declareVariable(newvar);
        }
    }

}

// Copyright (c) 2009-2023 Saxonica Limited


