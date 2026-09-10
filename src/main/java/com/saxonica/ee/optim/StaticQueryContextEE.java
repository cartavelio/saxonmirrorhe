package com.saxonica.ee.optim;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.expr.QueryLibraryImpl;
import com.saxonica.expr.StaticQueryContextPE;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.instruct.Executable;
import net.sf.saxon.functions.ExecutableFunctionLibrary;
import net.sf.saxon.functions.FunctionLibraryList;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.pattern.NodeKindTest;
import net.sf.saxon.query.QueryLibrary;
import net.sf.saxon.query.QueryReader;
import net.sf.saxon.query.StaticQueryContext;
import net.sf.saxon.query.XQueryParser;
import net.sf.saxon.trans.UncheckedXPathException;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.transpile.CSharpReplaceBody;

import java.io.IOException;
import java.io.InputStream;
import java.io.Reader;
import java.util.Map;
import java.util.concurrent.ConcurrentHashMap;

/**
 * A version of StaticQueryContext for Saxon-EE, that extends the capability by allowing compilation of library
 * modules.
 */
public class StaticQueryContextEE extends StaticQueryContextPE {

    private final Map<NamespaceUri, QueryLibrary> compiledLibraries = new ConcurrentHashMap<>();
            //Collections.synchronizedMap(new HashMap<String, QueryLibrary>());
    private boolean schemaAware;
    private boolean streaming;

    /**
     * Create a StaticQueryContext using a given Configuration. This creates a StaticQueryContext for a main module
     * (that is, a module that is not a library module). The StaticQueryContext is initialized from defaults held
     * in the Configuration.
     *
     * @param config the Saxon Configuration
     * @since 9.2
     */

    public StaticQueryContextEE(Configuration config) {
        super(config);
    }

    public StaticQueryContextEE(EnterpriseConfiguration config, boolean initialize) {
        super(config, initialize);
    }


    @Override
    public void copyFrom(/*@NotNull*/ StaticQueryContext c) {
        super.copyFrom(c);
        if (c instanceof StaticQueryContextEE) {
            schemaAware = ((StaticQueryContextEE)c).schemaAware;
            streaming = ((StaticQueryContextEE)c).streaming;
        }
    }

    /**
     * Say whether this query is schema-aware
     *
     * @param aware true if this query is schema-aware
     * @since 9.2.1.2
     */

    @Override
    public void setSchemaAware(boolean aware) {
        if (aware) {
            getConfiguration().checkLicensedFeature(Configuration.LicenseFeature.ENTERPRISE_XQUERY, "schema-awareness", -1);
        }
        this.schemaAware = aware;
    }

    /**
     * Ask whether this query is schema-aware
     *
     * @return true if this query is schema-aware
     * @since 9.2.1.2
     */

    @Override
    public boolean isSchemaAware() {
        return schemaAware;
    }

    /**
     * Say whether the query should be compiled and evaluated to use streaming.
     * This affects subsequent calls on the compile() methods. This option requires
     * Saxon-EE.
     * @param option if true, the compiler will attempt to compile a query to be
     * capable of executing in streaming mode. If the query cannot be streamed,
     * a compile-time exception is reported. In streaming mode, the source
     * document is supplied as a stream, and no tree is built in memory. The default
     * is false. Setting the value to true has the side-effect of setting the required
     * context item type to "document node"; this is to ensure that expressions such as
     * //BOOK are streamable.
     * @since 9.6
     */

    @Override
    public void setStreaming(boolean option) {
        streaming = option;
        if (option) {
            getConfiguration().checkLicensedFeature(Configuration.LicenseFeature.ENTERPRISE_XQUERY, "streaming", -1);
            setRequiredContextItemType(NodeKindTest.DOCUMENT);
        }
    }

    /**
     * Ask whether the streaming option has been set, that is, whether
     * subsequent calls on compile() will compile queries to be capable
     * of executing in streaming mode.
     * @return true if the streaming option has been set.
     * @since 9.6
     */

    @Override
    public boolean isStreaming() {
        return streaming;
    }



    /**
     * Compile an XQuery library module for subsequent evaluation. The source text of the query
     * is supplied as a String. The base URI of the query is taken from the static context,
     * and defaults to the current working directory.
     * <p>Note that this interface makes the caller responsible for decoding the query and
     * presenting it as a string of characters. This means it is likely that any encoding
     * specified in the query prolog will be ignored.</p>
     *
     * @param query The XQuery library module to be compiled, supplied as a string.
     * @throws net.sf.saxon.trans.XPathException
     *          if the syntax of the expression is wrong,
     *          or if it references namespaces, variables, or functions that have not been declared,
     *          or contains other static errors.
     * @since 9.2 (changed in 9.3 to return void)
     */

    @Override
    public void compileLibrary(String query) throws XPathException {
        try {
            Configuration config = getConfiguration();
            Executable executable = makeExecutable();
            FunctionLibraryList libList = new FunctionLibraryList();
            libList.addFunctionLibrary(new ExecutableFunctionLibrary(config));
            executable.setFunctionLibrary(libList);
            XQueryParser qp = (XQueryParser) config.newExpressionParser("XQ", isUpdating());
            if (getCodeInjector() != null) {
                qp.setCodeInjector(getCodeInjector());
            }
            QueryLibrary topModule = new QueryLibraryImpl(this);
            executable.setTopLevelPackage(topModule.getPackageData());
            topModule.setExecutable(executable);
            topModule.setIsMainModule(false);
            qp.parseLibraryModule(query, topModule);
            topModule.bindUnboundFunctionCalls();
            topModule.bindUnboundVariables();
            compiledLibraries.put(topModule.getModuleNamespace(), topModule);
            executable.fixupQueryModules(topModule);
            executable.addQueryLibraryModule(topModule);
        } catch (UncheckedXPathException e) {
            throw e.getXPathException();
        }
    }

    /**
     * Prepare an XQuery library module for subsequent evaluation. The Query is supplied
     * in the form of a Reader. The base URI of the query is taken from the static context,
     * and defaults to the current working directory.
     * The effect of the method is that subsequent query compilations using this static
     * context can import the module URI without specifying a location hint; the import then takes effect
     * without requiring the module to be compiled each time it is imported.
     * <p>Note that this interface makes the Reader responsible for decoding the query and
     * presenting it as a stream of characters. This means it is likely that any encoding
     * specified in the query prolog will be ignored. Also, some implementations of Reader
     * cannot handle a byte order mark.</p>
     *
     * @param source A Reader giving access to the text of the XQuery query to be compiled.
     * @throws net.sf.saxon.trans.XPathException
     *                             if the syntax of the expression is wrong, or if it references namespaces,
     *                             variables, or functions that have not been declared, or any other static error is reported.
     * @throws java.io.IOException if a failure occurs reading the supplied input.
     * @since 9.2 (changed in 9.3 to return void)
     */

    @Override
    public void compileLibrary(Reader source)
            throws XPathException, IOException {
        char[] buffer = new char[4096];
        StringBuilder sb = new StringBuilder(4096);
        while (true) {
            int n = source.read(buffer);
            if (isFinished(n)) {
                break;
            } else {
                sb.append(buffer, 0, n);
            }
        }
        compileLibrary(sb.toString());
    }

    @CSharpReplaceBody(code = "return indicator == 0;")
    private boolean isFinished(int indicator) {
        return indicator < 0;
    }

    /**
     * Prepare an XQuery library module for subsequent evaluation. The Query is supplied
     * in the form of a InputStream, with an optional encoding. If the encoding is not specified,
     * the query parser attempts to obtain the encoding by inspecting the input stream: it looks specifically
     * for a byte order mark, and for the encoding option in the version declaration of an XQuery prolog.
     * The effect of the method is that subsequent query compilations using this static
     * context can import the module URI without specifying a location hint; the import then takes effect
     * without requiring the module to be compiled each time it is imported.
     * The encoding defaults to UTF-8.
     * The base URI of the query is taken from the static context,
     * and defaults to the current working directory.
     *
     * @param source   An InputStream giving access to the text of the XQuery query to be compiled, as a stream
     *                 of octets
     * @param encoding The encoding used to translate characters to octets in the query source. The parameter
     *                 may be null: in this case the query parser attempts to infer the encoding by inspecting the source,
     *                 and if that fails, it assumes UTF-8 encoding
     * @throws net.sf.saxon.trans.XPathException
     *                             if the syntax of the expression is wrong, or if it references namespaces,
     *                             variables, or functions that have not been declared, or any other static error is reported.
     * @throws java.io.IOException if a failure occurs reading the supplied input.
     * @since 9.2 (changed in 9.3 to return void)
     */

    @Override
    public void compileLibrary(InputStream source, /*@Nullable*/ String encoding)
            throws XPathException, IOException {
        String query = QueryReader.readInputStream(source, encoding, getConfiguration().getValidCharacterChecker());
        compileLibrary(query);
    }

    /**
     * Get a previously compiled library module
     *
     * @param namespace the module namespace
     * @return the QueryLibrary if a module with this namespace has been compiled as a library module;
     *         otherwise null.
     * @since 9.3
     */

    @Override
    public QueryLibrary getCompiledLibrary(NamespaceUri namespace) {
        return compiledLibraries.get(namespace);
    }

    @Override
    public Iterable<QueryLibrary> getCompiledLibraries() {
        return compiledLibraries.values();
    }

}

// Copyright (c) 2009-2023 Saxonica Limited