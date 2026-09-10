package com.saxonica.expr;

import com.saxonica.config.ProfessionalConfiguration;
import net.sf.saxon.Configuration;
import net.sf.saxon.functions.FunctionLibrary;
import net.sf.saxon.query.StaticQueryContext;

/**
 * A version of StaticQueryContext for Saxon-PE
 */
public class StaticQueryContextPE extends StaticQueryContext {

    /*@Nullable*/ private FunctionLibrary extensionFunctionLibrary;

    /**
     * Create a StaticQueryContext using a given Configuration. This creates a StaticQueryContext for a main module
     * (that is, a module that is not a library module). The static context is initialized from defaults held
     * in the Configuration.
     *
     * @param config the Saxon Configuration
     * @since 9.2
     */

    public StaticQueryContextPE(Configuration config) {
        super(config);
    }

    public StaticQueryContextPE(ProfessionalConfiguration config, boolean initialize) {
        super(config, initialize);
    }

    /**
     * Set a library of extension functions. The functions in this library will be available
     * in all modules of the query. The function library will be searched after language-defined
     * libraries (such as built-in functions, user-defined XQuery functions, and constructor
     * functions) but before extension functions defined at the Configuration level.
     *
     * @param library the function library to be added (replacing any that has previously been set).
     *                May be null to clear a previously-set library
     * @since 9.4
     */

    public void setExtensionFunctionLibrary(/*@Nullable*/ FunctionLibrary library) {
        this.extensionFunctionLibrary = library;
    }

    /**
     * Get any function library that was previously set using {@link #setExtensionFunctionLibrary(net.sf.saxon.functions.FunctionLibrary)}.
     *
     * @return the extension function library, or null if none has been set.
     * @since 9.4
     */

    /*@Nullable*/
    @Override
    public FunctionLibrary getExtensionFunctionLibrary() {
        return extensionFunctionLibrary;
    }


}

// Copyright (c) 2009-2022 Saxonica Limited