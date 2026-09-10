package com.saxonica.functions.extfn;

import net.sf.saxon.Configuration;
import net.sf.saxon.PreparedStylesheet;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.style.Compilation;
import net.sf.saxon.trans.CompilerInfo;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.ObjectValue;


/**
 * This class implements the saxon:compile-stylesheet() extension function
 */

public class CompileStylesheet extends SystemFunction {

    /**
     * Evaluate this function call at run-time
     *
     * @param context   The XPath dynamic evaluation context
     * @param arguments The values of the arguments to the function call. Each argument value (which is in general
     *                  a sequence) is supplied in the form of a sequence.
     * @return the results of the function.
     * @throws net.sf.saxon.trans.XPathException if a dynamic error occurs during evaluation of the function. The Saxon run-time
     *                                           code will add information about the error location.
     */

    @Override
    public ObjectValue<PreparedStylesheet> call(XPathContext context, Sequence[] arguments) throws XPathException {
        NodeInfo doc = (NodeInfo) arguments[0].head();
        Configuration config = context.getConfiguration();
        CompilerInfo compilerInfo = new CompilerInfo(config.getDefaultXsltCompilerInfo());
        compilerInfo.setErrorReporter(context.getErrorReporter());
        compilerInfo.setResourceResolver(context.getResourceResolver());
        PreparedStylesheet pss = Compilation.compileSingletonPackage(config, compilerInfo, doc.asActiveSource());
        return new ObjectValue<>(pss);
    }


}

// Copyright (c) 2009-2020 Saxonica Limited
