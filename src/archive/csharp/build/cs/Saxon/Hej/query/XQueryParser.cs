using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.query {
    public class XQueryParser : Saxon.Hej.expr.parser.XPathParser {
        public const string XQUERY10 = "1.0";
        public const string XQUERY30 = "3.0";
        public const string XQUERY31 = "3.1";
        public const string XQUERY40 = "4.0";
        private bool memoFunction = false;
        private bool streaming = false;
        private int errorCount = 0;
        private Saxon.Hej.trans.XPathException firstError = null;
        protected internal Saxon.Hej.expr.instruct.Executable executable;
        private bool foundCopyNamespaces = false;
        private bool foundBoundarySpaceDeclaration = false;
        private bool foundOrderingDeclaration = false;
        private bool foundEmptyOrderingDeclaration = false;
        private bool foundDefaultCollation = false;
        private bool foundConstructionDeclaration = false;
        private bool foundDefaultFunctionNamespace = false;
        private bool foundDefaultElementNamespace = false;
        private bool foundBaseURIDeclaration = false;
        private bool foundContextItemDeclaration = false;
        private bool foundDefaultDecimalFormat = false;
        private bool preambleProcessed = false;
        public readonly System.Collections.Generic.ISet<string> importedModules = new System.Collections.Generic.HashSet<string>(5);
        readonly System.Collections.Generic.IList<string> namespacesToBeSealed = new System.Collections.Generic.List<string>(10);
        readonly System.Collections.Generic.IList<Saxon.Hej.query.XQueryParser.Import> schemaImports = new System.Collections.Generic.List<Saxon.Hej.query.XQueryParser.Import>(5);
        readonly System.Collections.Generic.IList<Saxon.Hej.query.XQueryParser.Import> moduleImports = new System.Collections.Generic.List<Saxon.Hej.query.XQueryParser.Import>(5);
        private readonly System.Collections.Generic.ISet<Saxon.Hej.om.StructuredQName> outputPropertiesSeen = new System.Collections.Generic.HashSet<Saxon.Hej.om.StructuredQName>(4);
        private Saxon.Ejava.util.Properties parameterDocProperties;
        public XQueryParser(int languageVersion)  {
            this.languageVersion = languageVersion;
            setLanguage(Saxon.Hej.expr.parser.XPathParser.ParsedLanguage.XQUERY, languageVersion);
        }
        private Saxon.Hej.query.XQueryParser newParser() {
            Saxon.Hej.query.XQueryParser qp = new Saxon.Hej.query.XQueryParser(languageVersion);
            qp.setLanguage(language, languageVersion);
            qp.setParserExtension(parserExtension);
            return qp;
        }
        public Saxon.Hej.query.XQueryExpression makeXQueryExpression(string query, Saxon.Hej.query.QueryModule mainModule, Saxon.Hej.Configuration config) {
            try {
                setLanguage(Saxon.Hej.expr.parser.XPathParser.ParsedLanguage.XQUERY, languageVersion);
                if ((config.getXMLVersion() == Saxon.Hej.Configuration.XML10)) {
                    query = normalizeLineEndings10(query);
                } else {
                    query = normalizeLineEndings11(query);
                }
                Saxon.Hej.expr.instruct.Executable exec = mainModule.getExecutable();
                if ((exec == null)) {
                    exec = new Saxon.Hej.expr.instruct.Executable(config);
                    exec.setHostLanguage(Saxon.Api.HostLanguage.XQUERY);
                    exec.setTopLevelPackage(mainModule.getPackageData());
                    setExecutable(exec);
                }
                Saxon.Hej.expr.instruct.GlobalContextRequirement requirement = exec.getGlobalContextRequirement();
                if ((requirement != null)) {
                    requirement.addRequiredItemType(mainModule.getRequiredContextItemType());
                } else {
                    if (((mainModule.getRequiredContextItemType() != null) && (mainModule.getRequiredContextItemType() != Saxon.Hej.type.AnyItemType.getInstance()))) {
                        Saxon.Hej.expr.instruct.GlobalContextRequirement req = new Saxon.Hej.expr.instruct.GlobalContextRequirement();
                        req.setExternal(true);
                        req.addRequiredItemType(mainModule.getRequiredContextItemType());
                        exec.setGlobalContextRequirement(req);
                    }
                }
                Saxon.Ejava.util.Properties outputProps = new Saxon.Ejava.util.Properties(config.getDefaultSerializationProperties());
                if ((outputProps.getProperty(Saxon.Ejavax.xml.transform.OutputKeys.METHOD) == null)) {
                    outputProps.setProperty(Saxon.Ejavax.xml.transform.OutputKeys.METHOD, "xml");
                }
                parameterDocProperties = new Saxon.Ejava.util.Properties(outputProps);
                exec.setDefaultOutputProperties(new Saxon.Ejava.util.Properties(parameterDocProperties));
                Saxon.Hej.functions.FunctionLibraryList libList = new Saxon.Hej.functions.FunctionLibraryList();
                libList.addFunctionLibrary(new Saxon.Hej.functions.ExecutableFunctionLibrary(config));
                exec.setFunctionLibrary(libList);
                setExecutable(exec);
                setCodeInjector(mainModule.getCodeInjector());
                Saxon.Hej.expr.Expression exp = parseQuery(query, mainModule);
                if (streaming) {
                    env.getConfiguration().checkLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.ENTERPRISE_XQUERY, "streaming", -1);
                }
                exec.fixupQueryModules(mainModule);
                Saxon.Hej.query.XQueryExpression queryExp = config.makeXQueryExpression(exp, mainModule, streaming);
                Saxon.Hej.functions.FunctionLibrary userlib = exec.getFunctionLibrary();
                Saxon.Hej.functions.FunctionLibraryList lib = new Saxon.Hej.functions.FunctionLibraryList();
                lib.addFunctionLibrary(mainModule.getBuiltInFunctionSet());
                lib.addFunctionLibrary(config.getBuiltInExtensionLibraryList());
                lib.addFunctionLibrary(new Saxon.Hej.functions.registry.ConstructorFunctionLibrary(config));
                lib.addFunctionLibrary(config.getIntegratedFunctionLibrary());
                lib.addFunctionLibrary(mainModule.getGlobalFunctionLibrary());
                config.addExtensionBinders(lib);
                lib.addFunctionLibrary(userlib);
                exec.setFunctionLibrary(lib);
                return queryExp;
            } catch (Saxon.Hej.trans.XPathException e) {
                if (!(e.hasBeenReported())) {
                    reportError(e);
                }
                throw;
            }
        }
        private static string normalizeLineEndings11(string @in) {
            if ((((@in.IndexOf((char)(unchecked((int)0xd)))<0) && (@in.IndexOf((char)(unchecked((int)0x85)))<0)) && (@in.IndexOf((char)(0x2028))<0))) {
                return @in;
            }
            Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder(@in.Length);
            for (int i = 0; (i<@in.Length); i++) {
                char ch = @in[i];
                switch (ch) {
                  case (char)(unchecked((int)0x85)):
                  case (char)(0x2028):
                    sb.append((char)(unchecked((int)0xa)));
                    break ;
                  case (char)(unchecked((int)0xd)):
                    if (((i<(@in.Length-1)) && (((@in[(i+1)] == (char)(unchecked((int)0xa))) || (@in[(i+1)] == (char)(unchecked((int)0x85))))))) {
                        sb.append((char)(unchecked((int)0xa)));
                        i++;
                    } else {
                        sb.append((char)(unchecked((int)0xa)));
                    }
                    break ;
                  default:
                    sb.append(ch);
                    break ;
                }
            }
            return sb.ToString();
        }
        private static string normalizeLineEndings10(string @in) {
            if ((@in.IndexOf((char)(unchecked((int)0xd)))<0)) {
                return @in;
            }
            Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder(@in.Length);
            for (int i = 0; (i<@in.Length); i++) {
                char ch = @in[i];
                if ((ch == unchecked((int)0xd))) {
                    if (((i<(@in.Length-1)) && (@in[(i+1)] == (char)(unchecked((int)0xa))))) {
                        sb.append((char)(unchecked((int)0xa)));
                        i++;
                    } else {
                        sb.append((char)(unchecked((int)0xa)));
                    }
                } else {
                    sb.append(ch);
                }
            }
            return sb.ToString();
        }
        public Saxon.Hej.expr.instruct.Executable getExecutable() {
            return executable;
        }
        public void setExecutable(Saxon.Hej.expr.instruct.Executable exec) {
            executable = exec;
        }
        protected internal override void customizeTokenizer(Saxon.Hej.expr.parser.Tokenizer t) {
            t.isXQuery = true;
        }
        public void setStreaming(bool option) {
            streaming = option;
        }
        public bool isStreaming() {
            return streaming;
        }
        private Saxon.Hej.expr.Expression parseQuery(string queryString, Saxon.Hej.query.QueryModule env) {
            this.env = Saxon.Ejava.util.Objects.requireNonNull(env);
            charChecker = env.getConfiguration().getValidCharacterChecker();
            language = Saxon.Hej.expr.parser.XPathParser.ParsedLanguage.XQUERY;
            t = new Saxon.Hej.expr.parser.Tokenizer();
            t.languageLevel = languageVersion;
            t.isXQuery = true;
            try {
                t.tokenize(Saxon.Ejava.util.Objects.requireNonNull(queryString), 0, -1);
            } catch (Saxon.Hej.trans.XPathException err) {
                grumble(err.Message);
            }
            parseVersionDeclaration();
            allowXPath40Syntax = t.allowSaxonExtensions = (allowXPath40Syntax || env.getConfiguration().getBooleanProperty(Saxon.Hej.lib.Feature<bool>.ALLOW_SYNTAX_EXTENSIONS));
            Saxon.Hej.om.QNameParser qp = new Saxon.Hej.om.QNameParser(env.getLiveNamespaceResolver()).withAcceptEQName(true).withUnescaper(new Saxon.Hej.query.XQueryParser.Unescaper(env.getConfiguration().getValidCharacterChecker()));
            setQNameParser(qp);
            parseProlog();
            processPreamble();
            Saxon.Hej.expr.Expression exp = parseExpression();
            exp = makeTracer(exp, null);
            if ((t.currentToken != Saxon.Hej.expr.parser.Token.EOF)) {
                grumble((("Unexpected token "+currentTokenDisplay())+" beyond end of query"));
            }
            setLocation(exp);
            Saxon.Hej.expr.parser.ExpressionTool.setDeepRetainedStaticContext(exp, env.makeRetainedStaticContext());
            if ((errorCount == 0)) {
                return exp;
            } else {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("One or more static errors were reported during query analysis");
                err.setHasBeenReported(true);
                err.setErrorCodeQName(firstError.getErrorCodeQName());
                throw err;
            }
        }
        public void parseLibraryModule(string queryString, Saxon.Hej.query.QueryModule env) {
            this.env = env;
            Saxon.Hej.Configuration config = env.getConfiguration();
            charChecker = config.getValidCharacterChecker();
            if ((config.getXMLVersion() == Saxon.Hej.Configuration.XML10)) {
                queryString = normalizeLineEndings10(queryString);
            } else {
                queryString = normalizeLineEndings11(queryString);
            }
            Saxon.Hej.expr.instruct.Executable exec = env.getExecutable();
            if ((exec == null)) {
                throw new Saxon.Ejava.lang.IllegalStateException("Query library module has no associated Executable");
            }
            executable = exec;
            t = new Saxon.Hej.expr.parser.Tokenizer();
            t.languageLevel = languageVersion;
            t.isXQuery = true;
            Saxon.Hej.om.QNameParser qp = new Saxon.Hej.om.QNameParser(env.getLiveNamespaceResolver()).withAcceptEQName(true).withUnescaper(new Saxon.Hej.query.XQueryParser.Unescaper(config.getValidCharacterChecker()));
            setQNameParser(qp);
            try {
                t.tokenize(queryString, 0, -1);
            } catch (Saxon.Hej.trans.XPathException err) {
                grumble(err.Message);
            }
            parseVersionDeclaration();
            allowXPath40Syntax = t.allowSaxonExtensions = (allowXPath40Syntax || env.getConfiguration().getBooleanProperty(Saxon.Hej.lib.Feature<bool>.ALLOW_SYNTAX_EXTENSIONS));
            parseModuleDeclaration();
            parseProlog();
            processPreamble();
            if ((t.currentToken != Saxon.Hej.expr.parser.Token.EOF)) {
                grumble("Unrecognized content found after the variable and function declarations in a library module");
            }
            if ((errorCount != 0)) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("Static errors were reported in the imported library module");
                err.setErrorCodeQName(firstError.getErrorCodeQName());
                throw err;
            }
        }
        private void reportError(Saxon.Hej.trans.XPathException exception) {
            errorCount++;
            if ((firstError == null)) {
                firstError = exception;
            }
            ((Saxon.Hej.query.QueryModule)(env)).reportStaticError(exception);
            throw exception;
        }
        private static readonly Saxon.Ejava.util.regex.Pattern encNamePattern = Saxon.Ejava.util.regex.Pattern.compile("^[A-Za-z]([A-Za-z0-9._\\x2D])*$");
        private void parseVersionDeclaration() {
            if ((t.currentToken == Saxon.Hej.expr.parser.Token.XQUERY_VERSION)) {
                nextToken();
                expect(Saxon.Hej.expr.parser.Token.STRING_LITERAL);
                string queryVersion = unescape(t.currentTokenValue).ToString();
                string[] allowedVersions = new string[]{"1.0", "3.0", "3.1", "4.0"};
                if ((Saxon.Ejava.util.Arrays.binarySearch(allowedVersions, queryVersion)<0)) {
                    grumble(("Invalid XQuery version "+queryVersion), "XQST0031");
                }
                if (queryVersion.Equals("4.0")) {
                    allowXPath40Syntax = true;
                }
                nextToken();
                if ("encoding".Equals(t.currentTokenValue)) {
                    nextToken();
                    expect(Saxon.Hej.expr.parser.Token.STRING_LITERAL);
                    if (!(encNamePattern.matcher(unescape(t.currentTokenValue)).matches())) {
                        grumble("Encoding name contains invalid characters", "XQST0087");
                    }
                    nextToken();
                }
                expect(Saxon.Hej.expr.parser.Token.SEMICOLON);
                nextToken();
            } else {
                if ((t.currentToken == Saxon.Hej.expr.parser.Token.XQUERY_ENCODING)) {
                    nextToken();
                    expect(Saxon.Hej.expr.parser.Token.STRING_LITERAL);
                    if (!(encNamePattern.matcher(t.currentTokenValue).matches())) {
                        grumble("Encoding name contains invalid characters", "XQST0087");
                    }
                    nextToken();
                    expect(Saxon.Hej.expr.parser.Token.SEMICOLON);
                    nextToken();
                }
            }
        }
        private void parseModuleDeclaration() {
            expect(Saxon.Hej.expr.parser.Token.MODULE_NAMESPACE);
            nextToken();
            expect(Saxon.Hej.expr.parser.Token.NAME);
            string prefix = t.currentTokenValue;
            nextToken();
            expect(Saxon.Hej.expr.parser.Token.EQUALS);
            nextToken();
            expect(Saxon.Hej.expr.parser.Token.STRING_LITERAL);
            string uri = uriLiteral(t.currentTokenValue);
            checkProhibitedPrefixes(prefix, uri);
            if (uri.Length==0) {
                grumble("Module namespace cannot be \"\"", "XQST0088");
                uri = "http://saxon.fallback.namespace/";
            }
            nextToken();
            expect(Saxon.Hej.expr.parser.Token.SEMICOLON);
            nextToken();
            try {
                ((Saxon.Hej.query.QueryModule)(env)).setModuleNamespace(uri);
                ((Saxon.Hej.query.QueryModule)(env)).declarePrologNamespace(prefix, uri);
                executable.addQueryLibraryModule((Saxon.Hej.query.QueryModule)(env));
            } catch (Saxon.Hej.trans.XPathException err) {
                err.setLocator(makeLocation());
                reportError(err);
            }
        }
        private void parseProlog() {
            bool allowModuleDecl = true;
            bool allowDeclarations = true;
            while (true) {
                try {
                    if ((t.currentToken == Saxon.Hej.expr.parser.Token.MODULE_NAMESPACE)) {
                        string uri = ((Saxon.Hej.query.QueryModule)(env)).getModuleNamespace();
                        if ((uri == null)) {
                            grumble("Module declaration must not be used in a main module");
                        } else {
                            grumble("Module declaration appears more than once");
                        }
                        if (!(allowModuleDecl)) {
                            grumble("Module declaration must precede other declarations in the query prolog");
                        }
                    }
                    allowModuleDecl = false;
                    switch (t.currentToken) {
                      case Saxon.Hej.expr.parser.Token.DECLARE_NAMESPACE:
                        if (!(allowDeclarations)) {
                            grumble("Namespace declarations cannot follow variables, functions, or options");
                        }
                        parseNamespaceDeclaration();
                        break ;
                      case Saxon.Hej.expr.parser.Token.DECLARE_ANNOTATED:
                        processPreamble();
                        if (allowDeclarations) {
                            sealNamespaces(namespacesToBeSealed, env.getConfiguration());
                            allowDeclarations = false;
                        }
                        nextToken();
                        expect(Saxon.Hej.expr.parser.Token.PERCENT);
                        Saxon.Hej.query.AnnotationList annotationList = parseAnnotationsList();
                        if (isKeyword("function")) {
                            annotationList.check(env.getConfiguration(), "DF");
                            parseFunctionDeclaration(annotationList);
                        } else {
                            if (isKeyword("variable")) {
                                annotationList.check(env.getConfiguration(), "DV");
                                parseVariableDeclaration(annotationList);
                            } else {
                                grumble("Annotations can appear only in 'declare variable' and 'declare function'");
                            }
                        }
                        break ;
                      case Saxon.Hej.expr.parser.Token.DECLARE_DEFAULT:
                        nextToken();
                        expect(Saxon.Hej.expr.parser.Token.NAME);
                        switch (t.currentTokenValue) {
                          case "element":
                            if (!(allowDeclarations)) {
                                grumble("Namespace declarations cannot follow variables, functions, or options");
                            }
                            parseDefaultElementNamespace();
                            break ;
                          case "function":
                            if (!(allowDeclarations)) {
                                grumble("Namespace declarations cannot follow variables, functions, or options");
                            }
                            parseDefaultFunctionNamespace();
                            break ;
                          case "collation":
                            if (!(allowDeclarations)) {
                                grumble("Collation declarations must appear earlier in the prolog");
                            }
                            parseDefaultCollation();
                            break ;
                          case "order":
                            if (!(allowDeclarations)) {
                                grumble("Order declarations must appear earlier in the prolog");
                            }
                            parseDefaultOrder();
                            break ;
                          case "decimal-format":
                            nextToken();
                            parseDefaultDecimalFormat();
                            break ;
                          default:
                            grumble("After 'declare default', expected 'element', 'function', or 'collation'");
                            break ;
                        }
                        break ;
                      case Saxon.Hej.expr.parser.Token.DECLARE_BOUNDARY_SPACE:
                        if (!(allowDeclarations)) {
                            grumble("'declare boundary-space' must appear earlier in the query prolog");
                        }
                        parseBoundarySpaceDeclaration();
                        break ;
                      case Saxon.Hej.expr.parser.Token.DECLARE_ORDERING:
                        if (!(allowDeclarations)) {
                            grumble("'declare ordering' must appear earlier in the query prolog");
                        }
                        parseOrderingDeclaration();
                        break ;
                      case Saxon.Hej.expr.parser.Token.DECLARE_COPY_NAMESPACES:
                        if (!(allowDeclarations)) {
                            grumble("'declare copy-namespaces' must appear earlier in the query prolog");
                        }
                        parseCopyNamespacesDeclaration();
                        break ;
                      case Saxon.Hej.expr.parser.Token.DECLARE_BASEURI:
                        if (!(allowDeclarations)) {
                            grumble("'declare base-uri' must appear earlier in the query prolog");
                        }
                        parseBaseURIDeclaration();
                        break ;
                      case Saxon.Hej.expr.parser.Token.DECLARE_DECIMAL_FORMAT:
                        if (!(allowDeclarations)) {
                            grumble("'declare decimal-format' must appear earlier in the query prolog");
                        }
                        parseDecimalFormatDeclaration();
                        break ;
                      case Saxon.Hej.expr.parser.Token.IMPORT_SCHEMA:
                        if (!(allowDeclarations)) {
                            grumble("Import schema must appear earlier in the prolog");
                        }
                        parseSchemaImport();
                        break ;
                      case Saxon.Hej.expr.parser.Token.IMPORT_MODULE:
                        if (!(allowDeclarations)) {
                            grumble("Import module must appear earlier in the prolog");
                        }
                        parseModuleImport();
                        break ;
                      case Saxon.Hej.expr.parser.Token.DECLARE_VARIABLE:
                        if (allowDeclarations) {
                            sealNamespaces(namespacesToBeSealed, env.getConfiguration());
                            allowDeclarations = false;
                        }
                        processPreamble();
                        parseVariableDeclaration(Saxon.Hej.query.AnnotationList.EMPTY);
                        break ;
                      case Saxon.Hej.expr.parser.Token.DECLARE_CONTEXT:
                        if (allowDeclarations) {
                            sealNamespaces(namespacesToBeSealed, env.getConfiguration());
                            allowDeclarations = false;
                        }
                        processPreamble();
                        parseContextItemDeclaration();
                        break ;
                      case Saxon.Hej.expr.parser.Token.DECLARE_FUNCTION:
                        if (allowDeclarations) {
                            sealNamespaces(namespacesToBeSealed, env.getConfiguration());
                            allowDeclarations = false;
                        }
                        processPreamble();
                        parseFunctionDeclaration(Saxon.Hej.query.AnnotationList.EMPTY);
                        break ;
                      case Saxon.Hej.expr.parser.Token.DECLARE_UPDATING:
                        nextToken();
                        if (!(isKeyword("function"))) {
                            grumble("expected 'function' after 'declare updating");
                        }
                        if (allowDeclarations) {
                            sealNamespaces(namespacesToBeSealed, env.getConfiguration());
                            allowDeclarations = false;
                        }
                        processPreamble();
                        parserExtension.parseUpdatingFunctionDeclaration(this);
                        break ;
                      case Saxon.Hej.expr.parser.Token.DECLARE_OPTION:
                        if (allowDeclarations) {
                            sealNamespaces(namespacesToBeSealed, env.getConfiguration());
                            allowDeclarations = false;
                        }
                        parseOptionDeclaration();
                        break ;
                      case Saxon.Hej.expr.parser.Token.DECLARE_TYPE:
                        checkSyntaxExtensions("declare type");
                        if (allowDeclarations) {
                            sealNamespaces(namespacesToBeSealed, env.getConfiguration());
                            allowDeclarations = false;
                        }
                        parseTypeAliasDeclaration();
                        break ;
                      case Saxon.Hej.expr.parser.Token.DECLARE_CONSTRUCTION:
                        if (!(allowDeclarations)) {
                            grumble("'declare construction' must appear earlier in the query prolog");
                        }
                        parseConstructionDeclaration();
                        break ;
                      case Saxon.Hej.expr.parser.Token.DECLARE_REVALIDATION:
                        if (!(allowDeclarations)) {
                            grumble("'declare revalidation' must appear earlier in the query prolog");
                        }
                        parserExtension.parseRevalidationDeclaration(this);
                        break ;
                      case Saxon.Hej.expr.parser.Token.EOF:
                        string uri = ((Saxon.Hej.query.QueryModule)(env)).getModuleNamespace();
                        if ((uri == null)) {
                            grumble("The main module must contain a query expression after any declarations in the prolog");
                        } else {
                            return ;
                        }
                        break ;
                      default:
                        return ;
                    }
                    expect(Saxon.Hej.expr.parser.Token.SEMICOLON);
                    nextToken();
                } catch (Saxon.Hej.trans.XPathException err) {
                    if ((err.getLocator() == null)) {
                        err.setLocator(makeLocation());
                    }
                    if (!(err.hasBeenReported())) {
                        errorCount++;
                        if ((firstError == null)) {
                            firstError = err;
                        }
                        reportError(err);
                    }
                    while ((t.currentToken != Saxon.Hej.expr.parser.Token.SEMICOLON)) {
                        nextToken();
                        if ((t.currentToken == Saxon.Hej.expr.parser.Token.EOF)) {
                            return ;
                        } else {
                            if ((t.currentToken == Saxon.Hej.expr.parser.Token.RCURLY)) {
                                t.lookAhead();
                            } else {
                                if ((t.currentToken == Saxon.Hej.expr.parser.Token.TAG)) {
                                    parsePseudoXML(true);
                                }
                            }
                        }
                    }
                    nextToken();
                }
            }
        }
        protected internal override Saxon.Hej.query.AnnotationList parseAnnotationsList() {
            System.Collections.Generic.List<Saxon.Hej.query.Annotation> annotations = new System.Collections.Generic.List<Saxon.Hej.query.Annotation>();
            int options = 0;
            while (true) {
                t.setState(Saxon.Hej.expr.parser.Tokenizer.BARE_NAME_STATE);
                nextToken();
                expect(Saxon.Hej.expr.parser.Token.NAME);
                t.setState(Saxon.Hej.expr.parser.Tokenizer.DEFAULT_STATE);
                Saxon.Hej.om.StructuredQName qName;
                string uri;
                if ((t.currentTokenValue.IndexOf(':')<0)) {
                    uri = Saxon.Hej.lib.NamespaceConstant.XQUERY;
                    qName = new Saxon.Hej.om.StructuredQName("", uri, t.currentTokenValue);
                } else {
                    qName = makeStructuredQName(t.currentTokenValue, "");
                    System.Diagnostics.Debug.Assert((qName != null));
                    uri = qName.getURI();
                }
                Saxon.Hej.query.Annotation annotation = new Saxon.Hej.query.Annotation(qName);
                if (uri.Equals(Saxon.Hej.lib.NamespaceConstant.XQUERY)) {
                    if ((((!(qName.Equals(Saxon.Hej.query.Annotation.PRIVATE)) && !(qName.Equals(Saxon.Hej.query.Annotation.PUBLIC))) && !(qName.Equals(Saxon.Hej.query.Annotation.UPDATING))) && !(qName.Equals(Saxon.Hej.query.Annotation.SIMPLE)))) {
                        grumble(("Unrecognized variable or function annotation "+qName.getDisplayName()), "XQST0045");
                    }
                    annotation.addAnnotationParameter(new Saxon.Hej.value.Int64Value(options));
                } else {
                    if (isReservedInQuery(uri)) {
                        grumble((("The annotation "+t.currentTokenValue)+" is in a reserved namespace"), "XQST0045");
                    } else {
                        if (uri.Equals("")) {
                            grumble((("The annotation "+t.currentTokenValue)+" is in no namespace"), "XQST0045");
                        } else {
                        }
                    }
                }
                nextToken();
                if ((t.currentToken == Saxon.Hej.expr.parser.Token.LPAR)) {
                    nextToken();
                    if ((t.currentToken == Saxon.Hej.expr.parser.Token.RPAR)) {
                        grumble("Annotation parameter list cannot be empty");
                    }
                    while (true) {
                        Saxon.Hej.expr.Literal arg;
                        switch (t.currentToken) {
                          case Saxon.Hej.expr.parser.Token.STRING_LITERAL:
                            arg = (Saxon.Hej.expr.Literal)(parseStringLiteral(false));
                            break ;
                          case Saxon.Hej.expr.parser.Token.NUMBER:
                            arg = (Saxon.Hej.expr.Literal)(parseNumericLiteral(false));
                            break ;
                          default:
                            grumble("Annotation parameter must be a literal");
                            return null;
                        }
                        Saxon.Hej.om.GroundedValue val = arg.getGroundedValue();
                        if (((val is Saxon.Hej.value.StringValue) || (val is Saxon.Hej.value.NumericValue))) {
                            annotation.addAnnotationParameter((Saxon.Hej.value.AtomicValue)(val));
                        } else {
                            grumble("Annotation parameter must be a string or number");
                        }
                        if ((t.currentToken == Saxon.Hej.expr.parser.Token.RPAR)) {
                            nextToken();
                            break ;
                        }
                        expect(Saxon.Hej.expr.parser.Token.COMMA);
                        nextToken();
                    }
                }
                annotations.Add(annotation);
                if ((t.currentToken != Saxon.Hej.expr.parser.Token.PERCENT)) {
                    return new Saxon.Hej.query.AnnotationList(annotations);
                }
            }
        }
        private void sealNamespaces(System.Collections.Generic.IList<string> namespacesToBeSealed, Saxon.Hej.Configuration config) {
            foreach (string ns in namespacesToBeSealed) {
                config.sealNamespace(ns);
            }
        }
        private void processPreamble() {
            if (preambleProcessed) {
                return ;
            }
            preambleProcessed = true;
            if (foundDefaultCollation) {
                string collationName = env.getDefaultCollationName();
                System.Uri collationURI;
                try {
                    collationURI = new System.Uri(collationName, System.UriKind.RelativeOrAbsolute);
                    if (!(collationURI.IsAbsoluteUri)) {
                        System.Uri @base = new System.Uri(env.getStaticBaseURI(), System.UriKind.RelativeOrAbsolute);
                        collationURI = new System.Uri(@base, collationURI);
                        collationName = collationURI.ToString();
                    }
                } catch (System.UriFormatException err) {
                    grumble((("Default collation name '"+collationName)+"' is not a valid URI"), "XQST0046");
                    collationName = Saxon.Hej.lib.NamespaceConstant.CODEPOINT_COLLATION_URI;
                }
                if ((env.getConfiguration().getCollation(collationName) == null)) {
                    grumble((("Default collation name '"+collationName)+"' is not a recognized collation"), "XQST0038");
                    collationName = Saxon.Hej.lib.NamespaceConstant.CODEPOINT_COLLATION_URI;
                }
                ((Saxon.Hej.query.QueryModule)(env)).setDefaultCollationName(collationName);
            }
            foreach (Saxon.Hej.query.XQueryParser.Import imp in schemaImports) {
                try {
                    applySchemaImport(imp);
                } catch (Saxon.Hej.trans.XPathException err) {
                    if (!(err.hasBeenReported())) {
                        err.maybeSetLocation(makeLocation(imp.offset));
                        throw;
                    }
                }
            }
            foreach (Saxon.Hej.query.XQueryParser.Import imp in moduleImports) {
                try {
                    applyModuleImport(imp);
                } catch (Saxon.Hej.trans.XPathException err) {
                    if (!(err.hasBeenReported())) {
                        err.maybeSetLocation(makeLocation(imp.offset));
                        throw;
                    }
                }
            }
        }
        private void parseDefaultCollation() {
            if (foundDefaultCollation) {
                grumble("default collation appears more than once", "XQST0038");
            }
            foundDefaultCollation = true;
            nextToken();
            expect(Saxon.Hej.expr.parser.Token.STRING_LITERAL);
            string uri = uriLiteral(t.currentTokenValue);
            ((Saxon.Hej.query.QueryModule)(env)).setDefaultCollationName(uri);
            nextToken();
        }
        private void parseDefaultOrder() {
            if (foundEmptyOrderingDeclaration) {
                grumble("empty ordering declaration appears more than once", "XQST0069");
            }
            foundEmptyOrderingDeclaration = true;
            nextToken();
            if (!(isKeyword("empty"))) {
                grumble("After 'declare default order', expected keyword 'empty'");
            }
            nextToken();
            if (isKeyword("least")) {
                ((Saxon.Hej.query.QueryModule)(env)).setEmptyLeast(true);
            } else {
                if (isKeyword("greatest")) {
                    ((Saxon.Hej.query.QueryModule)(env)).setEmptyLeast(false);
                } else {
                    grumble("After 'declare default order empty', expected keyword 'least' or 'greatest'");
                }
            }
            nextToken();
        }
        private void parseBoundarySpaceDeclaration() {
            if (foundBoundarySpaceDeclaration) {
                grumble("'declare boundary-space' appears more than once", "XQST0068");
            }
            foundBoundarySpaceDeclaration = true;
            nextToken();
            expect(Saxon.Hej.expr.parser.Token.NAME);
            if ("preserve".Equals(t.currentTokenValue)) {
                ((Saxon.Hej.query.QueryModule)(env)).setPreserveBoundarySpace(true);
            } else {
                if ("strip".Equals(t.currentTokenValue)) {
                    ((Saxon.Hej.query.QueryModule)(env)).setPreserveBoundarySpace(false);
                } else {
                    grumble("boundary-space must be 'preserve' or 'strip'");
                }
            }
            nextToken();
        }
        private void parseOrderingDeclaration() {
            if (foundOrderingDeclaration) {
                grumble("ordering mode declaration appears more than once", "XQST0065");
            }
            foundOrderingDeclaration = true;
            nextToken();
            expect(Saxon.Hej.expr.parser.Token.NAME);
            if ("ordered".Equals(t.currentTokenValue)) {
            } else {
                if ("unordered".Equals(t.currentTokenValue)) {
                } else {
                    grumble("ordering mode must be 'ordered' or 'unordered'");
                }
            }
            nextToken();
        }
        private void parseCopyNamespacesDeclaration() {
            if (foundCopyNamespaces) {
                grumble("declare copy-namespaces appears more than once", "XQST0055");
            }
            foundCopyNamespaces = true;
            nextToken();
            expect(Saxon.Hej.expr.parser.Token.NAME);
            if ("preserve".Equals(t.currentTokenValue)) {
                ((Saxon.Hej.query.QueryModule)(env)).setPreserveNamespaces(true);
            } else {
                if ("no-preserve".Equals(t.currentTokenValue)) {
                    ((Saxon.Hej.query.QueryModule)(env)).setPreserveNamespaces(false);
                } else {
                    grumble("copy-namespaces must be followed by 'preserve' or 'no-preserve'");
                }
            }
            nextToken();
            expect(Saxon.Hej.expr.parser.Token.COMMA);
            nextToken();
            expect(Saxon.Hej.expr.parser.Token.NAME);
            if ("inherit".Equals(t.currentTokenValue)) {
                ((Saxon.Hej.query.QueryModule)(env)).setInheritNamespaces(true);
            } else {
                if ("no-inherit".Equals(t.currentTokenValue)) {
                    ((Saxon.Hej.query.QueryModule)(env)).setInheritNamespaces(false);
                } else {
                    grumble("After the comma in the copy-namespaces declaration, expected 'inherit' or 'no-inherit'");
                }
            }
            nextToken();
        }
        private void parseConstructionDeclaration() {
            if (foundConstructionDeclaration) {
                grumble("declare construction appears more than once", "XQST0067");
            }
            foundConstructionDeclaration = true;
            nextToken();
            expect(Saxon.Hej.expr.parser.Token.NAME);
            int val;
            if ("preserve".Equals(t.currentTokenValue)) {
                val = Saxon.Hej.lib.Validation.PRESERVE;
            } else {
                if ("strip".Equals(t.currentTokenValue)) {
                    val = Saxon.Hej.lib.Validation.STRIP;
                } else {
                    grumble("construction mode must be 'preserve' or 'strip'");
                    val = Saxon.Hej.lib.Validation.STRIP;
                }
            }
            ((Saxon.Hej.query.QueryModule)(env)).setConstructionMode(val);
            nextToken();
        }
        protected internal void parseRevalidationDeclaration() {
            grumble("declare revalidation is allowed only in XQuery Update");
        }
        private void parseSchemaImport() {
            ensureSchemaAware("import schema");
            Saxon.Hej.query.XQueryParser.Import sImport = new Saxon.Hej.query.XQueryParser.Import();
            string prefix = null;
            sImport.namespaceURI = null;
            sImport.locationURIs = new System.Collections.Generic.List<string>(5);
            sImport.offset = t.currentTokenStartOffset;
            nextToken();
            if (isKeyword("namespace")) {
                prefix = readNamespaceBinding();
            } else {
                if (isKeyword("default")) {
                    nextToken();
                    if (!(isKeyword("element"))) {
                        grumble("In 'import schema', expected 'element namespace'");
                    }
                    nextToken();
                    if (!(isKeyword("namespace"))) {
                        grumble("In 'import schema', expected keyword 'namespace'");
                    }
                    nextToken();
                    prefix = "";
                }
            }
            if ((t.currentToken == Saxon.Hej.expr.parser.Token.STRING_LITERAL)) {
                string uri = uriLiteral(t.currentTokenValue);
                checkProhibitedPrefixes(prefix, uri);
                sImport.namespaceURI = uri;
                nextToken();
                if (isKeyword("at")) {
                    nextToken();
                    expect(Saxon.Hej.expr.parser.Token.STRING_LITERAL);
                    sImport.locationURIs.Add(uriLiteral(t.currentTokenValue));
                    nextToken();
                    while ((t.currentToken == Saxon.Hej.expr.parser.Token.COMMA)) {
                        nextToken();
                        expect(Saxon.Hej.expr.parser.Token.STRING_LITERAL);
                        sImport.locationURIs.Add(uriLiteral(t.currentTokenValue));
                        nextToken();
                    }
                } else {
                    if ((t.currentToken != Saxon.Hej.expr.parser.Token.SEMICOLON)) {
                        grumble("After the target namespace URI, expected 'at' or ';'");
                    }
                }
            } else {
                grumble("After 'import schema', expected 'namespace', 'default', or a string-literal");
            }
            if ((prefix != null)) {
                try {
                    if (prefix.Length==0) {
                        ((Saxon.Hej.query.QueryModule)(env)).setDefaultElementNamespace(sImport.namespaceURI);
                    } else {
                        if (((sImport.namespaceURI == null) || "".Equals(sImport.namespaceURI))) {
                            grumble("A prefix cannot be bound to the null namespace", "XQST0057");
                        }
                        ((Saxon.Hej.query.QueryModule)(env)).declarePrologNamespace(prefix, sImport.namespaceURI);
                    }
                } catch (Saxon.Hej.trans.XPathException err) {
                    err.setLocator(makeLocation());
                    reportError(err);
                }
            }
            foreach (System.Object schemaImport in schemaImports) {
                Saxon.Hej.query.XQueryParser.Import imp = (Saxon.Hej.query.XQueryParser.Import)(schemaImport);
                if (imp.namespaceURI.Equals(sImport.namespaceURI)) {
                    grumble((("Schema namespace '"+sImport.namespaceURI)+"' is imported more than once"), "XQST0058");
                    break ;
                }
            }
            schemaImports.Add(sImport);
        }
        private string readNamespaceBinding() {
            t.setState(Saxon.Hej.expr.parser.Tokenizer.DEFAULT_STATE);
            nextToken();
            expect(Saxon.Hej.expr.parser.Token.NAME);
            string prefix = t.currentTokenValue;
            nextToken();
            expect(Saxon.Hej.expr.parser.Token.EQUALS);
            nextToken();
            return prefix;
        }
        protected internal void ensureSchemaAware(string featureName) {
            if (!(env.getConfiguration().isLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.ENTERPRISE_XQUERY))) {
                throw new Saxon.Hej.trans.XPathException((("This Saxon version and license does not allow use of '"+featureName)+"'"), "XQST0009");
            }
            env.getConfiguration().checkLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.ENTERPRISE_XQUERY, featureName, -1);
            getExecutable().setSchemaAware(true);
            getStaticContext().getPackageData().setSchemaAware(true);
        }
        private void applySchemaImport(Saxon.Hej.query.XQueryParser.Import sImport) {
            Saxon.Hej.Configuration config = env.getConfiguration();
            lock (config) {
                if (!(config.isSchemaAvailable(sImport.namespaceURI))) {
                    if (!(sImport.locationURIs.Count==0)) {
                        try {
                            Saxon.Hej.@event.PipelineConfiguration pipe = config.makePipelineConfiguration();
                            config.readMultipleSchemas(pipe, env.getStaticBaseURI(), sImport.locationURIs, sImport.namespaceURI);
                            namespacesToBeSealed.Add(sImport.namespaceURI);
                        } catch (Saxon.Hej.type.SchemaException err) {
                            grumble(((("Error in schema "+sImport.namespaceURI)+": ")+err.Message), "XQST0059", sImport.offset);
                        }
                    } else {
                        if (((sImport.namespaceURI.Equals(Saxon.Hej.lib.NamespaceConstant.XML) || sImport.namespaceURI.Equals(Saxon.Hej.lib.NamespaceConstant.FN)) || sImport.namespaceURI.Equals(Saxon.Hej.lib.NamespaceConstant.SCHEMA_INSTANCE))) {
                            config.addSchemaForBuiltInNamespace(sImport.namespaceURI);
                        } else {
                            grumble(("Unable to locate requested schema "+sImport.namespaceURI), "XQST0059", sImport.offset);
                        }
                    }
                }
                ((Saxon.Hej.query.QueryModule)(env)).addImportedSchema(sImport.namespaceURI, env.getStaticBaseURI(), sImport.locationURIs);
            }
        }
        private void parseModuleImport() {
            Saxon.Hej.query.QueryModule thisModule = (Saxon.Hej.query.QueryModule)(env);
            Saxon.Hej.query.XQueryParser.Import mImport = new Saxon.Hej.query.XQueryParser.Import();
            string prefix = null;
            mImport.namespaceURI = null;
            mImport.locationURIs = new System.Collections.Generic.List<string>(5);
            mImport.offset = t.currentTokenStartOffset;
            nextToken();
            if (((t.currentToken == Saxon.Hej.expr.parser.Token.NAME) && t.currentTokenValue.Equals("namespace"))) {
                prefix = readNamespaceBinding();
            }
            if ((t.currentToken == Saxon.Hej.expr.parser.Token.STRING_LITERAL)) {
                string uri = uriLiteral(t.currentTokenValue);
                checkProhibitedPrefixes(prefix, uri);
                mImport.namespaceURI = uri;
                if (mImport.namespaceURI.Length==0) {
                    grumble("Imported module namespace cannot be \"\"", "XQST0088");
                    mImport.namespaceURI = ("http://saxon.fallback.namespace/line"+t.getLineNumber());
                }
                if (importedModules.Contains(mImport.namespaceURI)) {
                    grumble("Two 'import module' declarations specify the same module namespace", "XQST0047");
                }
                importedModules.Add(mImport.namespaceURI);
                ((Saxon.Hej.query.QueryModule)(env)).addImportedNamespace(mImport.namespaceURI);
                nextToken();
                if (isKeyword("at")) {
                    do {
                        nextToken();
                        expect(Saxon.Hej.expr.parser.Token.STRING_LITERAL);
                        mImport.locationURIs.Add(uriLiteral(t.currentTokenValue));
                        nextToken();
                    } while ((t.currentToken == Saxon.Hej.expr.parser.Token.COMMA));
                }
            } else {
                grumble("After 'import module', expected 'namespace' or a string-literal");
            }
            if ((prefix != null)) {
                try {
                    if ((mImport.namespaceURI.Equals(thisModule.getModuleNamespace()) && mImport.namespaceURI.Equals(thisModule.checkURIForPrefix(prefix)))) {
                    } else {
                        thisModule.declarePrologNamespace(prefix, mImport.namespaceURI);
                    }
                } catch (Saxon.Hej.trans.XPathException err) {
                    err.setLocator(makeLocation());
                    reportError(err);
                }
            }
            moduleImports.Add(mImport);
        }
        private void applyModuleImport(Saxon.Hej.query.XQueryParser.Import mImport) {
            System.Collections.Generic.IList<Saxon.Hej.query.QueryModule> existingModules;
            for (int i = 0; (i<mImport.locationURIs.Count); i++) {
                try {
                    string uri = mImport.locationURIs[i];
                    System.Uri abs = Saxon.Hej.functions.ResolveURI.makeAbsolute(uri, env.getStaticBaseURI());
                    mImport.locationURIs[i] = abs.ToString();
                } catch (System.UriFormatException e) {
                    grumble(((("Invalid URI "+mImport.locationURIs[i])+": ")+e.Message), "XQST0046", mImport.offset);
                }
            }
            Saxon.Hej.query.QueryLibrary lib = ((Saxon.Hej.query.QueryModule)(env)).getUserQueryContext().getCompiledLibrary(mImport.namespaceURI);
            if ((lib != null)) {
                executable.addQueryLibraryModule(lib);
                existingModules = new System.Collections.Generic.List<Saxon.Hej.query.QueryModule>();
                existingModules.Add(lib);
                lib.link((Saxon.Hej.query.QueryModule)(env));
            } else {
                if (!(env.getConfiguration().getBooleanProperty(Saxon.Hej.lib.Feature<bool>.XQUERY_MULTIPLE_MODULE_IMPORTS))) {
                    System.Collections.Generic.IList<Saxon.Hej.query.QueryModule> list = executable.getQueryLibraryModules(mImport.namespaceURI);
                    if (((list != null) && !(list.Count==0))) {
                        return ;
                    }
                } else {
                    for (int h = (mImport.locationURIs.Count-1); (h>=0); h--) {
                        if (executable.isQueryLocationHintProcessed(mImport.locationURIs[h])) {
                            Saxon.Impl.Helpers.ListUtils.Remove(mImport.locationURIs, h);
                        }
                    }
                }
            }
            if (mImport.locationURIs.Count==0) {
                System.Collections.Generic.IList<Saxon.Hej.query.QueryModule> list = executable.getQueryLibraryModules(mImport.namespaceURI);
                if (((list != null) && !(list.Count==0))) {
                    return ;
                }
            }
            Saxon.Hej.lib.ModuleURIResolver resolver = ((Saxon.Hej.query.QueryModule)(env)).getUserQueryContext().getModuleURIResolver();
            string[] hints = new string[mImport.locationURIs.Count];
            for (int h = 0; (h<hints.Length); h++) {
                hints[h] = mImport.locationURIs[h];
            }
            Saxon.Ejavax.xml.transform.stream.StreamSource[] sources = null;
            if ((resolver != null)) {
                try {
                    sources = resolver.resolve(mImport.namespaceURI, env.getStaticBaseURI(), hints);
                } catch (Saxon.Hej.trans.XPathException err) {
                    grumble(("Failed to resolve URI of imported module: "+err.Message), "XQST0059", mImport.offset);
                }
            }
            if ((sources == null)) {
                if ((hints.Length == 0)) {
                    grumble(("Cannot locate module for namespace "+mImport.namespaceURI), "XQST0059", mImport.offset);
                }
                resolver = env.getConfiguration().getStandardModuleURIResolver();
                sources = resolver.resolve(mImport.namespaceURI, env.getStaticBaseURI(), hints);
            }
            foreach (string hint in mImport.locationURIs) {
                executable.addQueryLocationHintProcessed(hint);
            }
            for (int m = 0; (m<sources.Length); m++) {
                Saxon.Ejavax.xml.transform.stream.StreamSource ss = sources[m];
                string baseURI = ss.getSystemId();
                if ((baseURI == null)) {
                    if ((m<hints.Length)) {
                        baseURI = hints[m];
                    } else {
                        baseURI = env.getStaticBaseURI();
                    }
                    ss.setSystemId(baseURI);
                }
                existingModules = executable.getQueryLibraryModules(mImport.namespaceURI);
                bool loaded = false;
                if (((existingModules != null) && (m<hints.Length))) {
                    foreach (Saxon.Hej.query.QueryModule existingModule in existingModules) {
                        System.Uri uri = existingModule.getLocationURI();
                        if (((uri != null) && uri.ToString().Equals(mImport.locationURIs[m]))) {
                            loaded = true;
                            break ;
                        }
                    }
                }
                if (loaded) {
                    break ;
                }
                try {
                    string queryText = Saxon.Hej.query.QueryReader.readSourceQuery(ss, charChecker);
                    try {
                        if ((ss.getInputStream() != null)) {
                            ss.getInputStream().Close();
                        } else {
                            if ((ss.getReader() != null)) {
                                ss.getReader().Close();
                            }
                        }
                    } catch (System.IO.IOException e) {
                        throw new Saxon.Hej.trans.XPathException("Failure while closing file for imported query module");
                    }
                    Saxon.Hej.query.QueryModule.makeQueryModule(baseURI, executable, (Saxon.Hej.query.QueryModule)(env), queryText, mImport.namespaceURI);
                } catch (Saxon.Hej.trans.XPathException err) {
                    err.maybeSetLocation(makeLocation());
                    reportError(err);
                }
            }
        }
        private void parseBaseURIDeclaration() {
            if (foundBaseURIDeclaration) {
                grumble("Base URI Declaration may only appear once", "XQST0032");
            }
            foundBaseURIDeclaration = true;
            nextToken();
            expect(Saxon.Hej.expr.parser.Token.STRING_LITERAL);
            string uri = uriLiteral(t.currentTokenValue);
            try {
                System.Uri baseURI = new System.Uri(uri, System.UriKind.RelativeOrAbsolute);
                if (!(baseURI.IsAbsoluteUri)) {
                    string oldBase = env.getStaticBaseURI();
                    uri = Saxon.Hej.functions.ResolveURI.makeAbsolute(uri, oldBase).ToString();
                }
                ((Saxon.Hej.query.QueryModule)(env)).setBaseURI(uri);
            } catch (System.UriFormatException err) {
                ((Saxon.Hej.query.QueryModule)(env)).setBaseURI(uri);
            }
            nextToken();
        }
        private void parseDecimalFormatDeclaration() {
            nextToken();
            expect(Saxon.Hej.expr.parser.Token.NAME);
            Saxon.Hej.om.StructuredQName formatName = makeStructuredQName(t.currentTokenValue, "");
            if ((env.getDecimalFormatManager().getNamedDecimalFormat(formatName) != null)) {
                grumble(("Duplicate declaration of decimal-format "+formatName.getDisplayName()), "XQST0111");
            }
            nextToken();
            parseDecimalFormatProperties(formatName);
        }
        private void parseDefaultDecimalFormat() {
            if (foundDefaultDecimalFormat) {
                grumble("Duplicate declaration of default decimal-format", "XQST0111");
            }
            foundDefaultDecimalFormat = true;
            parseDecimalFormatProperties(null);
        }
        private void parseDecimalFormatProperties(Saxon.Hej.om.StructuredQName formatName) {
            int outerOffset = t.currentTokenStartOffset;
            Saxon.Hej.trans.DecimalFormatManager dfm = env.getDecimalFormatManager();
            Saxon.Hej.trans.DecimalSymbols dfs = ((formatName == null) ? dfm.getDefaultDecimalFormat() : dfm.obtainNamedDecimalFormat(formatName));
            dfs.setHostLanguage(Saxon.Api.HostLanguage.XQUERY, 31);
            System.Collections.Generic.ISet<string> propertyNames = new System.Collections.Generic.HashSet<string>(10);
            while ((t.currentToken != Saxon.Hej.expr.parser.Token.SEMICOLON)) {
                int offset = t.currentTokenStartOffset;
                string propertyName = t.currentTokenValue;
                if (propertyNames.Contains(propertyName)) {
                    grumble((("Property name "+propertyName)+" is defined more than once"), "XQST0114", offset);
                }
                nextToken();
                expect(Saxon.Hej.expr.parser.Token.EQUALS);
                nextToken();
                expect(Saxon.Hej.expr.parser.Token.STRING_LITERAL);
                string propertyValue = unescape(t.currentTokenValue).ToString();
                nextToken();
                propertyNames.Add(propertyName);
                switch (propertyName) {
                  case "decimal-separator":
                    dfs.setDecimalSeparator(propertyValue);
                    break ;
                  case "grouping-separator":
                    dfs.setGroupingSeparator(propertyValue);
                    break ;
                  case "infinity":
                    dfs.setInfinity(propertyValue);
                    break ;
                  case "minus-sign":
                    dfs.setMinusSign(propertyValue);
                    break ;
                  case "NaN":
                    dfs.setNaN(propertyValue);
                    break ;
                  case "percent":
                    dfs.setPercent(propertyValue);
                    break ;
                  case "per-mille":
                    dfs.setPerMille(propertyValue);
                    break ;
                  case "zero-digit":
                    try {
                        dfs.setZeroDigit(propertyValue);
                    } catch (Saxon.Hej.trans.XPathException err) {
                        err.setErrorCode("XQST0097");
                        throw;
                    }
                    break ;
                  case "digit":
                    dfs.setDigit(propertyValue);
                    break ;
                  case "pattern-separator":
                    dfs.setPatternSeparator(propertyValue);
                    break ;
                  case "exponent-separator":
                    dfs.setExponentSeparator(propertyValue);
                    break ;
                  default:
                    grumble(("Unknown decimal-format property: "+propertyName), "XPST0003", offset);
                    break ;
                }
            }
            try {
                dfs.checkConsistency(formatName);
            } catch (Saxon.Hej.trans.XPathException err) {
                grumble(err.Message, "XQST0098", outerOffset);
            }
        }
        private void parseDefaultFunctionNamespace() {
            if (foundDefaultFunctionNamespace) {
                grumble("default function namespace appears more than once", "XQST0066");
            }
            foundDefaultFunctionNamespace = true;
            nextToken();
            expect(Saxon.Hej.expr.parser.Token.NAME);
            if (!("namespace".Equals(t.currentTokenValue))) {
                grumble("After 'declare default function', expected 'namespace'");
            }
            nextToken();
            expect(Saxon.Hej.expr.parser.Token.STRING_LITERAL);
            string uri = uriLiteral(t.currentTokenValue);
            if ((uri.Equals(Saxon.Hej.lib.NamespaceConstant.XML) || uri.Equals(Saxon.Hej.lib.NamespaceConstant.XMLNS))) {
                grumble("Reserved namespace used as default element/type namespace", "XQST0070");
            }
            ((Saxon.Hej.query.QueryModule)(env)).setDefaultFunctionNamespace(uri);
            nextToken();
        }
        private void parseDefaultElementNamespace() {
            if (foundDefaultElementNamespace) {
                grumble("default element namespace appears more than once", "XQST0066");
            }
            foundDefaultElementNamespace = true;
            nextToken();
            expect(Saxon.Hej.expr.parser.Token.NAME);
            if (!("namespace".Equals(t.currentTokenValue))) {
                grumble("After 'declare default element', expected 'namespace'");
            }
            nextToken();
            expect(Saxon.Hej.expr.parser.Token.STRING_LITERAL);
            string uri = uriLiteral(t.currentTokenValue);
            if ((uri.Equals(Saxon.Hej.lib.NamespaceConstant.XML) || uri.Equals(Saxon.Hej.lib.NamespaceConstant.XMLNS))) {
                grumble("Reserved namespace used as default element/type namespace", "XQST0070");
            }
            ((Saxon.Hej.query.QueryModule)(env)).setDefaultElementNamespace(uri);
            nextToken();
        }
        private void parseNamespaceDeclaration() {
            nextToken();
            expect(Saxon.Hej.expr.parser.Token.NAME);
            string prefix = t.currentTokenValue;
            if (!(Saxon.Hej.om.NameChecker.isValidNCName(prefix))) {
                grumble(("Invalid namespace prefix "+Saxon.Hej.trans.Err.wrap(prefix)));
            }
            nextToken();
            expect(Saxon.Hej.expr.parser.Token.EQUALS);
            nextToken();
            expect(Saxon.Hej.expr.parser.Token.STRING_LITERAL);
            string uri = uriLiteral(t.currentTokenValue);
            checkProhibitedPrefixes(prefix, uri);
            if ("xml".Equals(prefix)) {
                grumble("Namespace prefix 'xml' cannot be declared", "XQST0070");
            }
            try {
                ((Saxon.Hej.query.QueryModule)(env)).declarePrologNamespace(prefix, uri);
            } catch (Saxon.Hej.trans.XPathException err) {
                err.setLocator(makeLocation());
                reportError(err);
            }
            nextToken();
        }
        private void checkProhibitedPrefixes(string prefix, string uri) {
            if ((((prefix != null) && !(prefix.Length==0)) && !(Saxon.Hej.om.NameChecker.isValidNCName(prefix)))) {
                grumble((("The namespace prefix "+Saxon.Hej.trans.Err.wrap(prefix))+" is not a valid NCName"));
            }
            if ((prefix == null)) {
                prefix = "";
            }
            if ((uri == null)) {
                uri = "";
            }
            if ("xmlns".Equals(prefix)) {
                grumble("The namespace prefix 'xmlns' cannot be redeclared", "XQST0070");
            }
            if (uri.Equals(Saxon.Hej.lib.NamespaceConstant.XMLNS)) {
                grumble("The xmlns namespace URI is reserved", "XQST0070");
            }
            if ((uri.Equals(Saxon.Hej.lib.NamespaceConstant.XML) && !(prefix.Equals("xml")))) {
                grumble("The XML namespace cannot be bound to any prefix other than 'xml'", "XQST0070");
            }
            if ((prefix.Equals("xml") && !(uri.Equals(Saxon.Hej.lib.NamespaceConstant.XML)))) {
                grumble(("The prefix 'xml' cannot be bound to any namespace other than "+Saxon.Hej.lib.NamespaceConstant.XML), "XQST0070");
            }
        }
        private void parseVariableDeclaration(Saxon.Hej.query.AnnotationList annotations) {
            int offset = t.currentTokenStartOffset;
            Saxon.Hej.expr.instruct.GlobalVariable var = new Saxon.Hej.expr.instruct.GlobalVariable();
            var.setPackageData(env.getPackageData());
            var.setLineNumber((t.getLineNumber()+1));
            var.setSystemId(env.getSystemId());
            if ((annotations != null)) {
                var.setPrivate(annotations.includes(Saxon.Hej.query.Annotation.PRIVATE));
            }
            nextToken();
            expect(Saxon.Hej.expr.parser.Token.DOLLAR);
            t.setState(Saxon.Hej.expr.parser.Tokenizer.BARE_NAME_STATE);
            nextToken();
            expect(Saxon.Hej.expr.parser.Token.NAME);
            string varName = t.currentTokenValue;
            Saxon.Hej.om.StructuredQName varQName = makeStructuredQName(t.currentTokenValue, "");
            System.Diagnostics.Debug.Assert((varQName != null));
            var.setVariableQName(varQName);
            string uri = varQName.getURI();
            string moduleURI = ((Saxon.Hej.query.QueryModule)(env)).getModuleNamespace();
            if (((moduleURI != null) && !(moduleURI.Equals(uri)))) {
                grumble("A variable declared in a library module must be in the module namespace", "XQST0048", offset);
            }
            nextToken();
            Saxon.Hej.value.SequenceType requiredType = Saxon.Hej.value.SequenceType.ANY_SEQUENCE;
            if ((t.currentToken == Saxon.Hej.expr.parser.Token.AS)) {
                t.setState(Saxon.Hej.expr.parser.Tokenizer.SEQUENCE_TYPE_STATE);
                nextToken();
                requiredType = parseSequenceType();
            }
            var.setRequiredType(requiredType);
            if ((t.currentToken == Saxon.Hej.expr.parser.Token.ASSIGN)) {
                t.setState(Saxon.Hej.expr.parser.Tokenizer.DEFAULT_STATE);
                nextToken();
                Saxon.Hej.expr.Expression exp = parseExprSingle();
                var.setBody(makeTracer(exp, varQName));
            } else {
                if ((t.currentToken == Saxon.Hej.expr.parser.Token.NAME)) {
                    if ("external".Equals(t.currentTokenValue)) {
                        Saxon.Hej.expr.instruct.GlobalParam par = new Saxon.Hej.expr.instruct.GlobalParam();
                        par.setPackageData(env.getPackageData());
                        par.setLineNumber(var.getLineNumber());
                        par.setSystemId(var.getSystemId());
                        par.setVariableQName(var.getVariableQName());
                        par.setRequiredType(var.getRequiredType());
                        var = par;
                        nextToken();
                        if ((t.currentToken == Saxon.Hej.expr.parser.Token.ASSIGN)) {
                            t.setState(Saxon.Hej.expr.parser.Tokenizer.DEFAULT_STATE);
                            nextToken();
                            Saxon.Hej.expr.Expression exp = parseExprSingle();
                            var.setBody(makeTracer(exp, varQName));
                        }
                    } else {
                        grumble("Variable must either be initialized or be declared as external");
                    }
                } else {
                    grumble("Expected ':=' or 'external' in variable declaration");
                }
            }
            Saxon.Hej.query.QueryModule qenv = (Saxon.Hej.query.QueryModule)(env);
            Saxon.Hej.expr.parser.RetainedStaticContext rsc = env.makeRetainedStaticContext();
            var.setRetainedStaticContext(rsc);
            if ((var.getBody() != null)) {
                Saxon.Hej.expr.parser.ExpressionTool.setDeepRetainedStaticContext(var.getBody(), rsc);
            }
            if (((qenv.getModuleNamespace() != null) && !(uri.Equals(qenv.getModuleNamespace())))) {
                grumble((("Variable "+Saxon.Hej.trans.Err.wrap(varName, Saxon.Hej.trans.Err.VARIABLE))+" is not defined in the module namespace"));
            }
            try {
                qenv.declareVariable(var);
            } catch (Saxon.Hej.trans.XPathException e) {
                grumble(e.Message, e.getErrorCodeQName(), -1);
            }
        }
        private void parseContextItemDeclaration() {
            int offset = t.currentTokenStartOffset;
            nextToken();
            if (!(isKeyword("item"))) {
                grumble("After 'declare context', expected 'item'");
            }
            if (foundContextItemDeclaration) {
                grumble("More than one context item declaration found", "XQST0099", offset);
            }
            foundContextItemDeclaration = true;
            Saxon.Hej.expr.instruct.GlobalContextRequirement req = new Saxon.Hej.expr.instruct.GlobalContextRequirement();
            req.setAbsentFocus(false);
            t.setState(Saxon.Hej.expr.parser.Tokenizer.BARE_NAME_STATE);
            nextToken();
            Saxon.Hej.type.ItemType requiredType = Saxon.Hej.type.AnyItemType.getInstance();
            if ((t.currentToken == Saxon.Hej.expr.parser.Token.AS)) {
                t.setState(Saxon.Hej.expr.parser.Tokenizer.SEQUENCE_TYPE_STATE);
                nextToken();
                requiredType = parseItemType();
            }
            req.addRequiredItemType(requiredType);
            if ((t.currentToken == Saxon.Hej.expr.parser.Token.ASSIGN)) {
                if (!(((Saxon.Hej.query.QueryModule)(env)).isMainModule())) {
                    grumble("The context item must not be initialized in a library module", "XQST0113");
                }
                t.setState(Saxon.Hej.expr.parser.Tokenizer.DEFAULT_STATE);
                nextToken();
                Saxon.Hej.expr.Expression exp = parseExprSingle();
                exp.setRetainedStaticContext(env.makeRetainedStaticContext());
                Saxon.Hej.expr.parser.RoleDiagnostic role = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.CONTEXT_ITEM, "context item declaration", 0);
                exp = Saxon.Hej.expr.CardinalityChecker.makeCardinalityChecker(exp, Saxon.Hej.expr.StaticProperty.EXACTLY_ONE, role);
                Saxon.Hej.expr.parser.ExpressionVisitor visitor = Saxon.Hej.expr.parser.ExpressionVisitor.make(env);
                exp = exp.simplify();
                Saxon.Hej.expr.parser.ContextItemStaticInfo info = env.getConfiguration().makeContextItemStaticInfo(Saxon.Hej.type.AnyItemType.getInstance(), true);
                exp.setRetainedStaticContext(env.makeRetainedStaticContext());
                exp = exp.typeCheck(visitor, info);
                req.setDefaultValue(exp);
                req.setExternal(false);
            } else {
                if (((t.currentToken == Saxon.Hej.expr.parser.Token.NAME) && "external".Equals(t.currentTokenValue))) {
                    req.setAbsentFocus(false);
                    req.setExternal(true);
                    nextToken();
                    if ((t.currentToken == Saxon.Hej.expr.parser.Token.ASSIGN)) {
                        if (!(((Saxon.Hej.query.QueryModule)(env)).isMainModule())) {
                            grumble("The context item must not be initialized in a library module", "XQST0113");
                        }
                        t.setState(Saxon.Hej.expr.parser.Tokenizer.DEFAULT_STATE);
                        nextToken();
                        Saxon.Hej.expr.Expression exp = parseExprSingle();
                        Saxon.Hej.expr.parser.RoleDiagnostic role = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.CONTEXT_ITEM, "context item declaration", 0);
                        exp = Saxon.Hej.expr.CardinalityChecker.makeCardinalityChecker(exp, Saxon.Hej.expr.StaticProperty.EXACTLY_ONE, role);
                        exp.setRetainedStaticContext(env.makeRetainedStaticContext());
                        req.setDefaultValue(exp);
                    }
                } else {
                    grumble("Expected ':=' or 'external' in context item declaration");
                }
            }
            Saxon.Hej.expr.instruct.Executable exec = getExecutable();
            if ((exec.getGlobalContextRequirement() != null)) {
                Saxon.Hej.expr.instruct.GlobalContextRequirement gcr = exec.getGlobalContextRequirement();
                if (((gcr.getDefaultValue() == null) && (req.getDefaultValue() != null))) {
                    gcr.setDefaultValue(req.getDefaultValue());
                }
                foreach (Saxon.Hej.type.ItemType otherType in gcr.getRequiredItemTypes()) {
                    if ((otherType != Saxon.Hej.type.AnyItemType.getInstance())) {
                        Saxon.Hej.type.TypeHierarchy th = env.getConfiguration().getTypeHierarchy();
                        Saxon.Hej.type.Affinity rel = th.relationship(requiredType, otherType);
                        if ((rel == Saxon.Hej.type.Affinity.DISJOINT)) {
                            grumble("Different modules specify incompatible requirements for the type of the initial context item", "XPTY0004");
                        }
                    }
                }
                gcr.addRequiredItemType(requiredType);
            } else {
                exec.setGlobalContextRequirement(req);
            }
        }
        public void parseFunctionDeclaration(Saxon.Hej.query.AnnotationList annotations) {
            if (annotations.includes(SAXON_MEMO_FUNCTION)) {
                if (env.getConfiguration().getEditionCode().Equals("HE")) {
                    warning("saxon:memo-function option is ignored under Saxon-HE");
                } else {
                    memoFunction = true;
                }
            }
            int offset = t.currentTokenStartOffset;
            t.setState(Saxon.Hej.expr.parser.Tokenizer.DEFAULT_STATE);
            nextToken();
            expect(Saxon.Hej.expr.parser.Token.FUNCTION);
            string uri;
            Saxon.Hej.om.StructuredQName qName;
            if ((t.currentTokenValue.IndexOf(':')<0)) {
                uri = env.getDefaultFunctionNamespace();
                qName = new Saxon.Hej.om.StructuredQName("", uri, t.currentTokenValue);
            } else {
                qName = makeStructuredQName(t.currentTokenValue, "");
                uri = qName.getURI();
            }
            if (uri.Length==0) {
                grumble("The function must be in a namespace", "XQST0060");
            }
            string moduleURI = ((Saxon.Hej.query.QueryModule)(env)).getModuleNamespace();
            if (((moduleURI != null) && !(moduleURI.Equals(uri)))) {
                grumble("A function in a library module must be in the module namespace", "XQST0048");
            }
            if (isReservedInQuery(uri)) {
                grumble((("The function name "+t.currentTokenValue)+" is in a reserved namespace"), "XQST0045");
            }
            Saxon.Hej.query.XQueryFunction func = new Saxon.Hej.query.XQueryFunction();
            func.setFunctionName(qName);
            func.setResultType(Saxon.Hej.value.SequenceType.ANY_SEQUENCE);
            func.setBody(null);
            Saxon.Hej.s9api.Location loc = makeNestedLocation(env.getContainingLocation(), t.getLineNumber(offset), t.getColumnNumber(offset), null);
            func.setLocation(loc);
            func.setStaticContext((Saxon.Hej.query.QueryModule)(env));
            func.setMemoFunction(memoFunction);
            func.setUpdating(annotations.includes(Saxon.Hej.query.Annotation.UPDATING));
            func.setAnnotations(annotations);
            nextToken();
            System.Collections.Generic.HashSet<Saxon.Hej.om.StructuredQName> paramNames = new System.Collections.Generic.HashSet<Saxon.Hej.om.StructuredQName>(8);
            bool external = false;
            if ((t.currentToken != Saxon.Hej.expr.parser.Token.RPAR)) {
                while (true) {
                    expect(Saxon.Hej.expr.parser.Token.DOLLAR);
                    nextToken();
                    expect(Saxon.Hej.expr.parser.Token.NAME);
                    Saxon.Hej.om.StructuredQName argQName = makeStructuredQName(t.currentTokenValue, "");
                    if (paramNames.Contains(argQName)) {
                        grumble(("Duplicate parameter name "+Saxon.Hej.trans.Err.wrap(t.currentTokenValue, Saxon.Hej.trans.Err.VARIABLE)), "XQST0039");
                    }
                    paramNames.Add(argQName);
                    Saxon.Hej.value.SequenceType paramType = Saxon.Hej.value.SequenceType.ANY_SEQUENCE;
                    nextToken();
                    if ((t.currentToken == Saxon.Hej.expr.parser.Token.AS)) {
                        nextToken();
                        paramType = parseSequenceType();
                    }
                    Saxon.Hej.expr.instruct.UserFunctionParameter arg = new Saxon.Hej.expr.instruct.UserFunctionParameter();
                    arg.setRequiredType(paramType);
                    arg.setVariableQName(argQName);
                    func.addArgument(arg);
                    declareRangeVariable(arg);
                    if ((t.currentToken == Saxon.Hej.expr.parser.Token.RPAR)) {
                        break ;
                    } else {
                        if ((t.currentToken == Saxon.Hej.expr.parser.Token.COMMA)) {
                            nextToken();
                        } else {
                            grumble((("Expected ',' or ')' after function argument, found '"+Saxon.Hej.expr.parser.Token.tokens[t.currentToken])+'\''));
                        }
                    }
                }
            }
            t.setState(Saxon.Hej.expr.parser.Tokenizer.BARE_NAME_STATE);
            nextToken();
            if ((t.currentToken == Saxon.Hej.expr.parser.Token.AS)) {
                if (func.isUpdating()) {
                    grumble("Cannot specify a return type for an updating function", "XUST0028");
                }
                t.setState(Saxon.Hej.expr.parser.Tokenizer.SEQUENCE_TYPE_STATE);
                nextToken();
                func.setResultType(parseSequenceType());
            }
            if (isKeyword("external")) {
                external = true;
            } else {
                expect(Saxon.Hej.expr.parser.Token.LCURLY);
                t.setState(Saxon.Hej.expr.parser.Tokenizer.DEFAULT_STATE);
                nextToken();
                if ((t.currentToken == Saxon.Hej.expr.parser.Token.RCURLY)) {
                    Saxon.Hej.expr.Expression body = Saxon.Hej.expr.Literal.makeEmptySequence();
                    body.setRetainedStaticContext(env.makeRetainedStaticContext());
                    setLocation(body);
                    func.setBody(body);
                } else {
                    Saxon.Hej.expr.Expression body = parseExpression();
                    func.setBody(body);
                    Saxon.Hej.expr.parser.ExpressionTool.setDeepRetainedStaticContext(body, env.makeRetainedStaticContext());
                }
                expect(Saxon.Hej.expr.parser.Token.RCURLY);
                lookAhead();
            }
            Saxon.Hej.expr.instruct.UserFunctionParameter[] @params = func.getParameterDefinitions();
            foreach (Saxon.Hej.expr.instruct.UserFunctionParameter param in @params) {
                undeclareRangeVariable();
            }
            t.setState(Saxon.Hej.expr.parser.Tokenizer.DEFAULT_STATE);
            nextToken();
            Saxon.Hej.query.QueryModule qenv = (Saxon.Hej.query.QueryModule)(env);
            if (external) {
                parserExtension.handleExternalFunctionDeclaration(this, func);
            } else {
                try {
                    qenv.declareFunction(func);
                } catch (Saxon.Hej.trans.XPathException e) {
                    grumble(e.Message, e.getErrorCodeQName(), -1);
                }
            }
            memoFunction = false;
        }
        public static readonly Saxon.Hej.om.StructuredQName SAXON_MEMO_FUNCTION = new Saxon.Hej.om.StructuredQName("saxon", Saxon.Hej.lib.NamespaceConstant.SAXON, "memo-function");
        protected internal void parseTypeAliasDeclaration() {
            parserExtension.parseTypeAliasDeclaration(this);
        }
        private void parseOptionDeclaration() {
            nextToken();
            expect(Saxon.Hej.expr.parser.Token.NAME);
            string defaultUri = Saxon.Hej.lib.NamespaceConstant.XQUERY;
            Saxon.Hej.om.StructuredQName varName = makeStructuredQName(t.currentTokenValue, defaultUri);
            System.Diagnostics.Debug.Assert((varName != null));
            string uri = varName.getURI();
            if (uri.Length==0) {
                grumble("The QName identifying an option declaration must be prefixed", "XPST0081");
                return ;
            }
            nextToken();
            expect(Saxon.Hej.expr.parser.Token.STRING_LITERAL);
            string value = unescape(t.currentTokenValue).ToString();
            if (uri.Equals(Saxon.Hej.lib.NamespaceConstant.OUTPUT)) {
                parseOutputDeclaration(varName, value);
            } else {
                if (uri.Equals(Saxon.Hej.lib.NamespaceConstant.SAXON)) {
                    string localName = varName.getLocalPart();
                    switch (localName) {
                      case "output":
                        setOutputProperty(value);
                        break ;
                      case "memo-function":
                        value = value.Trim();
                        switch (value) {
                          case "true":
                            memoFunction = true;
                            if (env.getConfiguration().getEditionCode().Equals("HE")) {
                                warning("saxon:memo-function option is ignored under Saxon-HE");
                            }
                            break ;
                          case "false":
                            memoFunction = false;
                            break ;
                          default:
                            warning("Value of saxon:memo-function must be 'true' or 'false'");
                            break ;
                        }
                        break ;
                      case "allow-cycles":
                        warning("Value of saxon:allow-cycles is ignored");
                        break ;
                      default:
                        warning(("Unknown Saxon option declaration: "+varName.getDisplayName()));
                        break ;
                    }
                }
            }
            nextToken();
        }
        protected internal void parseOutputDeclaration(Saxon.Hej.om.StructuredQName varName, string value) {
            if (!(((Saxon.Hej.query.QueryModule)(env)).isMainModule())) {
                grumble("Output declarations must not appear in a library module", "XQST0108");
            }
            string localName = varName.getLocalPart();
            if (outputPropertiesSeen.Contains(varName)) {
                grumble((("Duplicate output declaration ("+varName)+")"), "XQST0110");
            }
            outputPropertiesSeen.Add(varName);
            switch (localName) {
              case "parameter-document":
                {
                    Saxon.Ejavax.xml.transform.Source source;
                    try {
                        source = env.getConfiguration().getURIResolver().resolve(value, env.getStaticBaseURI());
                    } catch (Saxon.Ejavax.xml.transform.TransformerException e) {
                        throw Saxon.Hej.trans.XPathException.makeXPathException(e);
                    }
                    Saxon.Hej.lib.ParseOptions options = new Saxon.Hej.lib.ParseOptions();
                    options.setSchemaValidationMode(Saxon.Hej.lib.Validation.LAX);
                    options.setDTDValidationMode(Saxon.Hej.lib.Validation.SKIP);
                    Saxon.Hej.om.TreeInfo doc = env.getConfiguration().buildDocumentTree(source);
                    Saxon.Hej.serialize.SerializationParamsHandler ph = new Saxon.Hej.serialize.SerializationParamsHandler(parameterDocProperties);
                    ph.setSerializationParams(doc.getRootNode());
                    Saxon.Hej.serialize.CharacterMap characterMap = ph.getCharacterMap();
                    if ((characterMap != null)) {
                        Saxon.Hej.serialize.CharacterMapIndex index = new Saxon.Hej.serialize.CharacterMapIndex();
                        index.putCharacterMap(characterMap.getName(), characterMap);
                        getExecutable().setCharacterMapIndex(index);
                        parameterDocProperties.setProperty(Saxon.Hej.lib.SaxonOutputKeys.USE_CHARACTER_MAPS, characterMap.getName().getClarkName());
                    }
                    break ;
                }
              case "use-character-maps":
                grumble("Output declaration use-character-maps cannot appear except in a parameter file", "XQST0109");
                break ;
              default:
                {
                    Saxon.Ejava.util.Properties props = getExecutable().getPrimarySerializationProperties().getProperties();
                    Saxon.Hej.expr.instruct.ResultDocument.setSerializationProperty(props, "", localName, value, env.getNamespaceResolver(), false, env.getConfiguration());
                    break ;
                }
            }
        }
        private void setOutputProperty(string property) {
            int Equals = property.IndexOf("=");
            if ((Equals<0)) {
                badOutputProperty("no equals sign");
            } else {
                if ((Equals == 0)) {
                    badOutputProperty("starts with '=");
                }
            }
            string keyword = Saxon.Hej.value.Whitespace.trim(property.Substring(0, Equals - 0));
            string value = ((Equals == (property.Length-1)) ? "" : Saxon.Hej.value.Whitespace.trim(property.Substring((Equals+1))));
            Saxon.Ejava.util.Properties props = getExecutable().getPrimarySerializationProperties().getProperties();
            try {
                Saxon.Hej.om.StructuredQName name = makeStructuredQName(keyword, "");
                string lname = name.getLocalPart();
                string uri = name.getURI();
                Saxon.Hej.expr.instruct.ResultDocument.setSerializationProperty(props, uri, lname, value, env.getNamespaceResolver(), false, env.getConfiguration());
            } catch (Saxon.Hej.trans.XPathException e) {
                badOutputProperty(e.Message);
            }
        }
        private void badOutputProperty(string s) {
            warning((("Invalid serialization property ("+s)+")"));
        }
        protected internal override Saxon.Hej.expr.Expression parseFLWORExpression() {
            Saxon.Hej.expr.flwor.FLWORExpression flwor = new Saxon.Hej.expr.flwor.FLWORExpression();
            int exprOffset = t.currentTokenStartOffset;
            System.Collections.Generic.IList<Saxon.Hej.expr.flwor.Clause> clauseList = new System.Collections.Generic.List<Saxon.Hej.expr.flwor.Clause>(4);
            while (true) {
                int offset = t.currentTokenStartOffset;
                if (((t.currentToken == Saxon.Hej.expr.parser.Token.FOR) || (t.currentToken == Saxon.Hej.expr.parser.Token.FOR_MEMBER))) {
                    parseForClause(flwor, clauseList);
                } else {
                    if ((t.currentToken == Saxon.Hej.expr.parser.Token.LET)) {
                        parseLetClause(flwor, clauseList);
                    } else {
                        if ((t.currentToken == Saxon.Hej.expr.parser.Token.COUNT)) {
                            parseCountClause(clauseList);
                        } else {
                            if ((t.currentToken == Saxon.Hej.expr.parser.Token.GROUP_BY)) {
                                parseGroupByClause(flwor, clauseList);
                            } else {
                                if (((t.currentToken == Saxon.Hej.expr.parser.Token.FOR_TUMBLING) || (t.currentToken == Saxon.Hej.expr.parser.Token.FOR_SLIDING))) {
                                    parseWindowClause(flwor, clauseList);
                                } else {
                                    if (((t.currentToken == Saxon.Hej.expr.parser.Token.WHERE) || isKeyword("where"))) {
                                        nextToken();
                                        Saxon.Hej.expr.Expression condition = parseExprSingle();
                                        Saxon.Hej.expr.flwor.WhereClause clause = new Saxon.Hej.expr.flwor.WhereClause(flwor, condition);
                                        clause.setRepeated(containsLoopingClause(clauseList));
                                        clauseList.Add(clause);
                                    } else {
                                        if ((isKeyword("stable") || isKeyword("order"))) {
                                            if (isKeyword("stable")) {
                                                nextToken();
                                                if (!(isKeyword("order"))) {
                                                    grumble("'stable' must be followed by 'order by'");
                                                }
                                            }
                                            Saxon.Hej.expr.flwor.TupleExpression tupleExpression = new Saxon.Hej.expr.flwor.TupleExpression();
                                            System.Collections.Generic.IList<Saxon.Hej.expr.LocalVariableReference> vars = new System.Collections.Generic.List<Saxon.Hej.expr.LocalVariableReference>();
                                            foreach (Saxon.Hej.expr.flwor.Clause c in clauseList) {
                                                foreach (Saxon.Hej.expr.flwor.LocalVariableBinding b in c.getRangeVariables()) {
                                                    vars.Add(new Saxon.Hej.expr.LocalVariableReference(b));
                                                }
                                            }
                                            tupleExpression.setVariables(vars);
                                            System.Collections.Generic.IList<Saxon.Hej.query.XQueryParser.SortSpec> sortSpecList;
                                            t.setState(Saxon.Hej.expr.parser.Tokenizer.BARE_NAME_STATE);
                                            nextToken();
                                            if (!((((t.currentToken == Saxon.Hej.expr.parser.Token.BY) || isKeyword("by"))))) {
                                                grumble("'order' must be followed by 'by'");
                                            }
                                            t.setState(Saxon.Hej.expr.parser.Tokenizer.DEFAULT_STATE);
                                            nextToken();
                                            sortSpecList = parseSortDefinition();
                                            Saxon.Hej.expr.sort.SortKeyDefinition[] keys = new Saxon.Hej.expr.sort.SortKeyDefinition[sortSpecList.Count];
                                            for (int i = 0; (i<keys.Length); i++) {
                                                Saxon.Hej.query.XQueryParser.SortSpec spec = sortSpecList[i];
                                                Saxon.Hej.expr.sort.SortKeyDefinition key = new Saxon.Hej.expr.sort.SortKeyDefinition();
                                                key.setSortKey(sortSpecList[i].sortKey, false);
                                                string str = (spec.ascending ? "ascending" : "descending");
                                                key.setOrder(new Saxon.Hej.expr.StringLiteral(Saxon.Hej.str.BMPString.of(str)));
                                                key.setEmptyLeast(spec.emptyLeast);
                                                if ((spec.collation != null)) {
                                                    Saxon.Hej.lib.StringCollator comparator = env.getConfiguration().getCollation(spec.collation);
                                                    if ((comparator == null)) {
                                                        grumble((("Unknown collation '"+spec.collation)+'\''), "XQST0076");
                                                    }
                                                    key.setCollation(comparator);
                                                }
                                                keys[i] = key;
                                            }
                                            Saxon.Hej.expr.flwor.OrderByClause clause = new Saxon.Hej.expr.flwor.OrderByClause(flwor, keys, tupleExpression);
                                            clause.setRepeated(containsLoopingClause(clauseList));
                                            clauseList.Add(clause);
                                        } else {
                                            break ;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                setLocation(clauseList[(clauseList.Count-1)], offset);
            }
            int returnOffset = t.currentTokenStartOffset;
            expect(Saxon.Hej.expr.parser.Token.RETURN);
            t.setState(Saxon.Hej.expr.parser.Tokenizer.DEFAULT_STATE);
            nextToken();
            Saxon.Hej.expr.Expression returnExpression = parseExprSingle();
            returnExpression = makeTracer(returnExpression, null);
            for (int i = (clauseList.Count-1); (i>=0); i--) {
                Saxon.Hej.expr.flwor.Clause clause = clauseList[i];
                for (int n = 0; (n<clause.getRangeVariables().Length); n++) {
                    undeclareRangeVariable();
                }
            }
            flwor.init(clauseList, returnExpression);
            setLocation(flwor, exprOffset);
            return flwor;
        }
        protected internal Saxon.Hej.expr.LetExpression makeLetExpression() {
            if (((Saxon.Hej.query.QueryModule)(env)).getUserQueryContext().isCompileWithTracing()) {
                return new Saxon.Hej.expr.EagerLetExpression();
            } else {
                return new Saxon.Hej.expr.LetExpression();
            }
        }
        protected internal static bool containsLoopingClause(System.Collections.Generic.IList<Saxon.Hej.expr.flwor.Clause> clauseList) {
            foreach (Saxon.Hej.expr.flwor.Clause c in clauseList) {
                if (Saxon.Hej.expr.flwor.FLWORExpression.isLoopingClause(c)) {
                    return true;
                }
            }
            return false;
        }
        private void parseForClause(Saxon.Hej.expr.flwor.FLWORExpression flwor, System.Collections.Generic.IList<Saxon.Hej.expr.flwor.Clause> clauseList) {
            bool first = true;
            bool forMember = (t.currentToken == Saxon.Hej.expr.parser.Token.FOR_MEMBER);
            if ((forMember && !(allowXPath40Syntax))) {
                grumble("The 'for member' syntax requires XQuery 4.0 to be enabled");
            }
            do {
                nextToken();
                int offset = t.currentTokenStartOffset;
                Saxon.Hej.expr.flwor.ForClause clause = (forMember ? new Saxon.Hej.expr.flwor.ForMemberClause() : new Saxon.Hej.expr.flwor.ForClause());
                clause.setRepeated((!(first) || containsLoopingClause(clauseList)));
                if (first) {
                    first = false;
                }
                setLocation(clause, offset);
                clauseList.Add(clause);
                expect(Saxon.Hej.expr.parser.Token.DOLLAR);
                nextToken();
                expect(Saxon.Hej.expr.parser.Token.NAME);
                Saxon.Hej.om.StructuredQName varQName = makeStructuredQName(t.currentTokenValue, "");
                Saxon.Hej.value.SequenceType type = (forMember ? Saxon.Hej.value.SequenceType.ANY_SEQUENCE : Saxon.Hej.value.SequenceType.SINGLE_ITEM);
                nextToken();
                bool explicitType = false;
                if ((t.currentToken == Saxon.Hej.expr.parser.Token.AS)) {
                    explicitType = true;
                    nextToken();
                    type = parseSequenceType();
                }
                bool allowingEmpty = false;
                if (isKeyword("allowing")) {
                    if (forMember) {
                        grumble("'allowing empty' cannot appear in a 'for member' clause");
                    }
                    allowingEmpty = true;
                    clause.setAllowingEmpty(true);
                    if (!(explicitType)) {
                        type = (forMember ? Saxon.Hej.value.SequenceType.ANY_SEQUENCE : Saxon.Hej.value.SequenceType.OPTIONAL_ITEM);
                    }
                    nextToken();
                    if (!(isKeyword("empty"))) {
                        grumble("After 'allowing', expected 'empty'");
                    }
                    nextToken();
                }
                if ((((explicitType && !(allowingEmpty)) && !(forMember)) && (type.getCardinality() != Saxon.Hej.expr.StaticProperty.EXACTLY_ONE))) {
                    warning("Occurrence indicator on singleton range variable has no effect");
                    type = Saxon.Hej.value.SequenceType.makeSequenceType(type.getPrimaryType(), Saxon.Hej.expr.StaticProperty.EXACTLY_ONE);
                }
                Saxon.Hej.expr.flwor.LocalVariableBinding binding = new Saxon.Hej.expr.flwor.LocalVariableBinding(varQName, type);
                clause.setRangeVariable(binding);
                if (isKeyword("at")) {
                    nextToken();
                    expect(Saxon.Hej.expr.parser.Token.DOLLAR);
                    nextToken();
                    expect(Saxon.Hej.expr.parser.Token.NAME);
                    Saxon.Hej.om.StructuredQName posQName = makeStructuredQName(t.currentTokenValue, "");
                    if ((!(scanOnly) && posQName.Equals(varQName))) {
                        grumble("The two variables declared in a single 'for' clause must have different names", "XQST0089");
                    }
                    Saxon.Hej.expr.flwor.LocalVariableBinding pos = new Saxon.Hej.expr.flwor.LocalVariableBinding(posQName, Saxon.Hej.value.SequenceType.SINGLE_INTEGER);
                    clause.setPositionVariable(pos);
                    nextToken();
                }
                expect(Saxon.Hej.expr.parser.Token.IN);
                nextToken();
                clause.initSequence(flwor, parseExprSingle());
                declareRangeVariable(clause.getRangeVariable());
                if ((clause.getPositionVariable() != null)) {
                    declareRangeVariable(clause.getPositionVariable());
                }
                if (allowingEmpty) {
                    checkForClauseAllowingEmpty(flwor, clause);
                }
            } while ((t.currentToken == Saxon.Hej.expr.parser.Token.COMMA));
        }
        private void checkForClauseAllowingEmpty(Saxon.Hej.expr.flwor.FLWORExpression flwor, Saxon.Hej.expr.flwor.ForClause clause) {
            if (!(allowXPath30Syntax)) {
                grumble("The 'allowing empty' option requires XQuery 3.0");
            }
            Saxon.Hej.value.SequenceType type = clause.getRangeVariable().getRequiredType();
            if (!(Saxon.Hej.value.Cardinality.allowsZero(type.getCardinality()))) {
                warning("When 'allowing empty' is specified, the occurrence indicator on the range variable type should be '?'");
            }
        }
        private void parseLetClause(Saxon.Hej.expr.flwor.FLWORExpression flwor, System.Collections.Generic.IList<Saxon.Hej.expr.flwor.Clause> clauseList) {
            bool first = true;
            do {
                Saxon.Hej.expr.flwor.LetClause clause = new Saxon.Hej.expr.flwor.LetClause();
                setLocation(clause, t.currentTokenStartOffset);
                clause.setRepeated(containsLoopingClause(clauseList));
                if (first) {
                }
                clauseList.Add(clause);
                nextToken();
                if (first) {
                    first = false;
                } else {
                }
                expect(Saxon.Hej.expr.parser.Token.DOLLAR);
                nextToken();
                expect(Saxon.Hej.expr.parser.Token.NAME);
                string var = t.currentTokenValue;
                Saxon.Hej.om.StructuredQName varQName = makeStructuredQName(var, "");
                Saxon.Hej.value.SequenceType type = Saxon.Hej.value.SequenceType.ANY_SEQUENCE;
                nextToken();
                if ((t.currentToken == Saxon.Hej.expr.parser.Token.AS)) {
                    nextToken();
                    type = parseSequenceType();
                }
                Saxon.Hej.expr.flwor.LocalVariableBinding v = new Saxon.Hej.expr.flwor.LocalVariableBinding(varQName, type);
                expect(Saxon.Hej.expr.parser.Token.ASSIGN);
                nextToken();
                clause.initSequence(flwor, parseExprSingle());
                clause.setRangeVariable(v);
                declareRangeVariable(v);
            } while ((t.currentToken == Saxon.Hej.expr.parser.Token.COMMA));
        }
        private void parseCountClause(System.Collections.Generic.IList<Saxon.Hej.expr.flwor.Clause> clauseList) {
            do {
                Saxon.Hej.expr.flwor.CountClause clause = new Saxon.Hej.expr.flwor.CountClause();
                setLocation(clause, t.currentTokenStartOffset);
                clause.setRepeated(containsLoopingClause(clauseList));
                clauseList.Add(clause);
                nextToken();
                expect(Saxon.Hej.expr.parser.Token.DOLLAR);
                nextToken();
                expect(Saxon.Hej.expr.parser.Token.NAME);
                string var = t.currentTokenValue;
                Saxon.Hej.om.StructuredQName varQName = makeStructuredQName(var, "");
                Saxon.Hej.value.SequenceType type = Saxon.Hej.value.SequenceType.ANY_SEQUENCE;
                nextToken();
                Saxon.Hej.expr.flwor.LocalVariableBinding v = new Saxon.Hej.expr.flwor.LocalVariableBinding(varQName, type);
                clause.setRangeVariable(v);
                declareRangeVariable(v);
            } while ((t.currentToken == Saxon.Hej.expr.parser.Token.COMMA));
        }
        private void parseGroupByClause(Saxon.Hej.expr.flwor.FLWORExpression flwor, System.Collections.Generic.IList<Saxon.Hej.expr.flwor.Clause> clauseList) {
            Saxon.Hej.expr.flwor.GroupByClause clause = new Saxon.Hej.expr.flwor.GroupByClause(env.getConfiguration());
            setLocation(clause, t.currentTokenStartOffset);
            clause.setRepeated(containsLoopingClause(clauseList));
            System.Collections.Generic.IList<Saxon.Hej.om.StructuredQName> variableNames = new System.Collections.Generic.List<Saxon.Hej.om.StructuredQName>();
            System.Collections.Generic.IList<string> collations = new System.Collections.Generic.List<string>();
            nextToken();
            while (true) {
                Saxon.Hej.value.SequenceType type = Saxon.Hej.value.SequenceType.ANY_SEQUENCE;
                Saxon.Hej.om.StructuredQName varQName = readVariableName();
                if ((t.currentToken == Saxon.Hej.expr.parser.Token.AS)) {
                    nextToken();
                    type = parseSequenceType();
                    if ((t.currentToken != Saxon.Hej.expr.parser.Token.ASSIGN)) {
                        grumble("In group by, if the type is declared then it must be followed by ':= value'");
                    }
                }
                if ((t.currentToken == Saxon.Hej.expr.parser.Token.ASSIGN)) {
                    Saxon.Hej.expr.flwor.LetClause letClause = new Saxon.Hej.expr.flwor.LetClause();
                    clauseList.Add(letClause);
                    nextToken();
                    Saxon.Hej.expr.flwor.LocalVariableBinding v = new Saxon.Hej.expr.flwor.LocalVariableBinding(varQName, type);
                    Saxon.Hej.expr.Expression value = parseExprSingle();
                    Saxon.Hej.expr.parser.RoleDiagnostic role = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.MISC, "grouping key", 0);
                    Saxon.Hej.expr.Expression atomizedValue = Saxon.Hej.expr.Atomizer.makeAtomizer(value, role);
                    letClause.initSequence(flwor, atomizedValue);
                    letClause.setRangeVariable(v);
                    declareRangeVariable(v);
                }
                variableNames.Add(varQName);
                if (isKeyword("collation")) {
                    nextToken();
                    expect(Saxon.Hej.expr.parser.Token.STRING_LITERAL);
                    collations.Add(t.currentTokenValue);
                    nextToken();
                } else {
                    collations.Add(env.getDefaultCollationName());
                }
                if ((t.currentToken == Saxon.Hej.expr.parser.Token.COMMA)) {
                    nextToken();
                } else {
                    break ;
                }
            }
            Saxon.Hej.expr.flwor.TupleExpression groupingTupleExpr = new Saxon.Hej.expr.flwor.TupleExpression();
            Saxon.Hej.expr.flwor.TupleExpression retainedTupleExpr = new Saxon.Hej.expr.flwor.TupleExpression();
            System.Collections.Generic.IList<Saxon.Hej.expr.LocalVariableReference> groupingRefs = new System.Collections.Generic.List<Saxon.Hej.expr.LocalVariableReference>();
            System.Collections.Generic.IList<Saxon.Hej.expr.LocalVariableReference> retainedRefs = new System.Collections.Generic.List<Saxon.Hej.expr.LocalVariableReference>();
            System.Collections.Generic.IList<Saxon.Hej.expr.flwor.LocalVariableBinding> groupedBindings = new System.Collections.Generic.List<Saxon.Hej.expr.flwor.LocalVariableBinding>();
            foreach (Saxon.Hej.om.StructuredQName q in variableNames) {
                bool found = locateDeclaration(clauseList, groupingRefs, groupedBindings, q);
                if (!(found)) {
                    grumble((("The grouping variable "+q.getDisplayName())+" must be the name of a variable bound earlier in the FLWOR expression"), "XQST0094");
                }
            }
            groupingTupleExpr.setVariables(groupingRefs);
            clause.initGroupingTupleExpression(flwor, groupingTupleExpr);
            System.Collections.Generic.IList<Saxon.Hej.expr.flwor.LocalVariableBinding> ungroupedBindings = new System.Collections.Generic.List<Saxon.Hej.expr.flwor.LocalVariableBinding>();
            for (int i = (clauseList.Count-1); (i>=0); i--) {
                foreach (Saxon.Hej.expr.flwor.LocalVariableBinding b in clauseList[i].getRangeVariables()) {
                    if (!(groupedBindings.Contains(b))) {
                        ungroupedBindings.Add(b);
                        retainedRefs.Add(new Saxon.Hej.expr.LocalVariableReference(b));
                    }
                }
            }
            retainedTupleExpr.setVariables(retainedRefs);
            clause.initRetainedTupleExpression(flwor, retainedTupleExpr);
            Saxon.Hej.expr.flwor.LocalVariableBinding[] bindings = new Saxon.Hej.expr.flwor.LocalVariableBinding[(groupedBindings.Count+ungroupedBindings.Count)];
            int k = 0;
            foreach (Saxon.Hej.expr.flwor.LocalVariableBinding b in groupedBindings) {
                bindings[k] = new Saxon.Hej.expr.flwor.LocalVariableBinding(b.getVariableQName(), b.getRequiredType());
                k++;
            }
            foreach (Saxon.Hej.expr.flwor.LocalVariableBinding b in ungroupedBindings) {
                Saxon.Hej.type.ItemType itemType = b.getRequiredType().getPrimaryType();
                bindings[k] = new Saxon.Hej.expr.flwor.LocalVariableBinding(b.getVariableQName(), Saxon.Hej.value.SequenceType.makeSequenceType(itemType, Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE));
                k++;
            }
            for (int z = groupedBindings.Count; (z<bindings.Length); z++) {
                declareRangeVariable(bindings[z]);
            }
            for (int z = 0; (z<groupedBindings.Count); z++) {
                declareRangeVariable(bindings[z]);
            }
            clause.setVariableBindings(bindings);
            Saxon.Hej.expr.sort.GenericAtomicComparer[] comparers = new Saxon.Hej.expr.sort.GenericAtomicComparer[collations.Count];
            Saxon.Hej.expr.XPathContext context = env.makeEarlyEvaluationContext();
            for (int i = 0; (i<comparers.Length); i++) {
                Saxon.Hej.lib.StringCollator coll = env.getConfiguration().getCollation(collations[i]);
                comparers[i] = (Saxon.Hej.expr.sort.GenericAtomicComparer)(Saxon.Hej.expr.sort.GenericAtomicComparer.makeAtomicComparer(Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC, Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC, coll, context));
            }
            clause.setComparers(comparers);
            clauseList.Add(clause);
        }
        private bool locateDeclaration(System.Collections.Generic.IList<Saxon.Hej.expr.flwor.Clause> clauseList, System.Collections.Generic.IList<Saxon.Hej.expr.LocalVariableReference> groupingRefs, System.Collections.Generic.IList<Saxon.Hej.expr.flwor.LocalVariableBinding> groupedBindings, Saxon.Hej.om.StructuredQName q) {
            for (int i = (clauseList.Count-1); (i>=0); i--) {
                foreach (Saxon.Hej.expr.flwor.LocalVariableBinding b in clauseList[i].getRangeVariables()) {
                    if (q.Equals(b.getVariableQName())) {
                        groupedBindings.Add(b);
                        groupingRefs.Add(new Saxon.Hej.expr.LocalVariableReference(b));
                        return true;
                    }
                }
            }
            return false;
        }
        private Saxon.Hej.om.StructuredQName readVariableName() {
            expect(Saxon.Hej.expr.parser.Token.DOLLAR);
            nextToken();
            expect(Saxon.Hej.expr.parser.Token.NAME);
            string name = t.currentTokenValue;
            nextToken();
            return makeStructuredQName(name, "");
        }
        private void parseWindowClause(Saxon.Hej.expr.flwor.FLWORExpression flwor, System.Collections.Generic.IList<Saxon.Hej.expr.flwor.Clause> clauseList) {
            Saxon.Hej.expr.flwor.WindowClause clause = new Saxon.Hej.expr.flwor.WindowClause();
            setLocation(clause, t.currentTokenStartOffset);
            clause.setRepeated(containsLoopingClause(clauseList));
            clause.setIsSlidingWindow((t.currentToken == Saxon.Hej.expr.parser.Token.FOR_SLIDING));
            nextToken();
            if (!(isKeyword("window"))) {
                grumble(("after 'sliding' or 'tumbling', expected 'window', but found "+currentTokenDisplay()));
            }
            nextToken();
            Saxon.Hej.om.StructuredQName windowVarName = readVariableName();
            Saxon.Hej.value.SequenceType windowType = Saxon.Hej.value.SequenceType.ANY_SEQUENCE;
            if ((t.currentToken == Saxon.Hej.expr.parser.Token.AS)) {
                nextToken();
                windowType = parseSequenceType();
            }
            Saxon.Hej.expr.flwor.LocalVariableBinding windowVar = new Saxon.Hej.expr.flwor.LocalVariableBinding(windowVarName, windowType);
            clause.setVariableBinding(Saxon.Hej.expr.flwor.WindowClause.WINDOW_VAR, windowVar);
            Saxon.Hej.value.SequenceType windowItemTypeMandatory = Saxon.Hej.value.SequenceType.SINGLE_ITEM;
            Saxon.Hej.value.SequenceType windowItemTypeOptional = Saxon.Hej.value.SequenceType.OPTIONAL_ITEM;
            expect(Saxon.Hej.expr.parser.Token.IN);
            nextToken();
            clause.initSequence(flwor, parseExprSingle());
            if (!(isKeyword("start"))) {
                grumble(("in window clause, expected 'start', but found "+currentTokenDisplay()));
            }
            t.setState(Saxon.Hej.expr.parser.Tokenizer.BARE_NAME_STATE);
            nextToken();
            if ((t.currentToken == Saxon.Hej.expr.parser.Token.DOLLAR)) {
                Saxon.Hej.expr.flwor.LocalVariableBinding startItemVar = new Saxon.Hej.expr.flwor.LocalVariableBinding(readVariableName(), windowItemTypeMandatory);
                clause.setVariableBinding(Saxon.Hej.expr.flwor.WindowClause.START_ITEM, startItemVar);
                declareRangeVariable(startItemVar);
            }
            if (isKeyword("at")) {
                nextToken();
                Saxon.Hej.expr.flwor.LocalVariableBinding startPositionVar = new Saxon.Hej.expr.flwor.LocalVariableBinding(readVariableName(), Saxon.Hej.value.SequenceType.SINGLE_INTEGER);
                clause.setVariableBinding(Saxon.Hej.expr.flwor.WindowClause.START_ITEM_POSITION, startPositionVar);
                declareRangeVariable(startPositionVar);
            }
            if (isKeyword("previous")) {
                nextToken();
                Saxon.Hej.expr.flwor.LocalVariableBinding startPreviousItemVar = new Saxon.Hej.expr.flwor.LocalVariableBinding(readVariableName(), windowItemTypeOptional);
                clause.setVariableBinding(Saxon.Hej.expr.flwor.WindowClause.START_PREVIOUS_ITEM, startPreviousItemVar);
                declareRangeVariable(startPreviousItemVar);
            }
            if (isKeyword("next")) {
                nextToken();
                Saxon.Hej.expr.flwor.LocalVariableBinding startNextItemVar = new Saxon.Hej.expr.flwor.LocalVariableBinding(readVariableName(), windowItemTypeOptional);
                clause.setVariableBinding(Saxon.Hej.expr.flwor.WindowClause.START_NEXT_ITEM, startNextItemVar);
                declareRangeVariable(startNextItemVar);
            }
            if (!(isKeyword("when"))) {
                grumble(("Expected 'when' condition for window start, but found "+currentTokenDisplay()));
            }
            t.setState(Saxon.Hej.expr.parser.Tokenizer.DEFAULT_STATE);
            nextToken();
            clause.initStartCondition(flwor, parseExprSingle());
            if (isKeyword("only")) {
                clause.setIncludeUnclosedWindows(false);
                nextToken();
            }
            if (isKeyword("end")) {
                t.setState(Saxon.Hej.expr.parser.Tokenizer.BARE_NAME_STATE);
                nextToken();
                if ((t.currentToken == Saxon.Hej.expr.parser.Token.DOLLAR)) {
                    Saxon.Hej.expr.flwor.LocalVariableBinding endItemVar = new Saxon.Hej.expr.flwor.LocalVariableBinding(readVariableName(), windowItemTypeMandatory);
                    clause.setVariableBinding(Saxon.Hej.expr.flwor.WindowClause.END_ITEM, endItemVar);
                    declareRangeVariable(endItemVar);
                }
                if (isKeyword("at")) {
                    nextToken();
                    Saxon.Hej.expr.flwor.LocalVariableBinding endPositionVar = new Saxon.Hej.expr.flwor.LocalVariableBinding(readVariableName(), Saxon.Hej.value.SequenceType.SINGLE_INTEGER);
                    clause.setVariableBinding(Saxon.Hej.expr.flwor.WindowClause.END_ITEM_POSITION, endPositionVar);
                    declareRangeVariable(endPositionVar);
                }
                if (isKeyword("previous")) {
                    nextToken();
                    Saxon.Hej.expr.flwor.LocalVariableBinding endPreviousItemVar = new Saxon.Hej.expr.flwor.LocalVariableBinding(readVariableName(), windowItemTypeOptional);
                    clause.setVariableBinding(Saxon.Hej.expr.flwor.WindowClause.END_PREVIOUS_ITEM, endPreviousItemVar);
                    declareRangeVariable(endPreviousItemVar);
                }
                if (isKeyword("next")) {
                    nextToken();
                    Saxon.Hej.expr.flwor.LocalVariableBinding endNextItemVar = new Saxon.Hej.expr.flwor.LocalVariableBinding(readVariableName(), windowItemTypeOptional);
                    clause.setVariableBinding(Saxon.Hej.expr.flwor.WindowClause.END_NEXT_ITEM, endNextItemVar);
                    declareRangeVariable(endNextItemVar);
                }
                if (!(isKeyword("when"))) {
                    grumble(("Expected 'when' condition for window end, but found "+currentTokenDisplay()));
                }
                t.setState(Saxon.Hej.expr.parser.Tokenizer.DEFAULT_STATE);
                nextToken();
                clause.initEndCondition(flwor, parseExprSingle());
            } else {
                if (clause.isSlidingWindow()) {
                    grumble("A sliding window requires an end condition");
                }
            }
            declareRangeVariable(windowVar);
            clauseList.Add(clause);
        }
        public static Saxon.Hej.expr.Expression makeStringJoin(Saxon.Hej.expr.Expression exp, Saxon.Hej.expr.StaticContext env) {
            exp = Saxon.Hej.expr.Atomizer.makeAtomizer(exp, null);
            Saxon.Hej.type.ItemType t = exp.getItemType();
            if ((!(t.Equals(Saxon.Hej.type.BuiltInAtomicType.STRING)) && !(t.Equals(Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC)))) {
                exp = new Saxon.Hej.expr.AtomicSequenceConverter(exp, Saxon.Hej.type.BuiltInAtomicType.STRING);
                ((Saxon.Hej.expr.AtomicSequenceConverter)(exp)).allocateConverterStatically(env.getConfiguration(), false);
            }
            if ((exp.getCardinality() == Saxon.Hej.expr.StaticProperty.EXACTLY_ONE)) {
                return exp;
            } else {
                Saxon.Hej.expr.parser.RetainedStaticContext rsc = new Saxon.Hej.expr.parser.RetainedStaticContext(env);
                Saxon.Hej.expr.Expression fn = Saxon.Hej.functions.SystemFunction.makeCall("string-join", rsc, exp, new Saxon.Hej.expr.StringLiteral(Saxon.Hej.value.StringValue.SINGLE_SPACE));
                Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(exp, fn);
                return fn;
            }
        }
        private System.Collections.Generic.IList<Saxon.Hej.query.XQueryParser.SortSpec> parseSortDefinition() {
            System.Collections.Generic.IList<Saxon.Hej.query.XQueryParser.SortSpec> sortSpecList = new System.Collections.Generic.List<Saxon.Hej.query.XQueryParser.SortSpec>(5);
            while (true) {
                Saxon.Hej.query.XQueryParser.SortSpec sortSpec = new Saxon.Hej.query.XQueryParser.SortSpec();
                sortSpec.sortKey = parseExprSingle();
                sortSpec.ascending = true;
                sortSpec.emptyLeast = ((Saxon.Hej.query.QueryModule)(env)).isEmptyLeast();
                sortSpec.collation = env.getDefaultCollationName();
                if (isKeyword("ascending")) {
                    nextToken();
                } else {
                    if (isKeyword("descending")) {
                        sortSpec.ascending = false;
                        nextToken();
                    }
                }
                if (isKeyword("empty")) {
                    nextToken();
                    if (isKeyword("greatest")) {
                        sortSpec.emptyLeast = false;
                        nextToken();
                    } else {
                        if (isKeyword("least")) {
                            sortSpec.emptyLeast = true;
                            nextToken();
                        } else {
                            grumble("'empty' must be followed by 'greatest' or 'least'");
                        }
                    }
                }
                if (isKeyword("collation")) {
                    sortSpec.collation = readCollationName();
                }
                sortSpecList.Add(sortSpec);
                if ((t.currentToken == Saxon.Hej.expr.parser.Token.COMMA)) {
                    nextToken();
                } else {
                    break ;
                }
            }
            return sortSpecList;
        }
        protected internal string readCollationName() {
            nextToken();
            expect(Saxon.Hej.expr.parser.Token.STRING_LITERAL);
            string collationName = uriLiteral(t.currentTokenValue);
            System.Uri collationURI;
            try {
                collationURI = new System.Uri(collationName, System.UriKind.RelativeOrAbsolute);
                if (!(collationURI.IsAbsoluteUri)) {
                    System.Uri @base = new System.Uri(env.getStaticBaseURI(), System.UriKind.RelativeOrAbsolute);
                    collationURI = new System.Uri(@base, collationURI);
                    collationName = collationURI.ToString();
                }
            } catch (System.UriFormatException err) {
                grumble((("Collation name '"+collationName)+"' is not a valid URI"), "XQST0046");
                collationName = Saxon.Hej.lib.NamespaceConstant.CODEPOINT_COLLATION_URI;
            }
            nextToken();
            return collationName;
        }
        private class SortSpec {
            public Saxon.Hej.expr.Expression sortKey;
            public bool ascending;
            public bool emptyLeast;
            public string collation;
        
        }
        protected internal override Saxon.Hej.expr.Expression parseTypeswitchExpression() {
            int offset = t.currentTokenStartOffset;
            nextToken();
            Saxon.Hej.expr.Expression operand = parseExpression();
            System.Collections.Generic.IList<System.Collections.Generic.IList<Saxon.Hej.value.SequenceType>> types = new System.Collections.Generic.List<System.Collections.Generic.IList<Saxon.Hej.value.SequenceType>>(10);
            System.Collections.Generic.IList<Saxon.Hej.expr.Expression> actions = new System.Collections.Generic.List<Saxon.Hej.expr.Expression>(10);
            expect(Saxon.Hej.expr.parser.Token.RPAR);
            nextToken();
            Saxon.Hej.expr.LetExpression outerLet = makeLetExpression();
            outerLet.setRequiredType(Saxon.Hej.value.SequenceType.ANY_SEQUENCE);
            outerLet.setVariableQName(new Saxon.Hej.om.StructuredQName("zz", Saxon.Hej.lib.NamespaceConstant.SAXON, "zz_typeswitchVar"));
            outerLet.setSequence(operand);
            while ((t.currentToken == Saxon.Hej.expr.parser.Token.CASE)) {
                int caseOffset = t.currentTokenStartOffset;
                System.Collections.Generic.IList<Saxon.Hej.value.SequenceType> typeList;
                Saxon.Hej.expr.Expression action;
                nextToken();
                if ((t.currentToken == Saxon.Hej.expr.parser.Token.DOLLAR)) {
                    nextToken();
                    expect(Saxon.Hej.expr.parser.Token.NAME);
                    string var = t.currentTokenValue;
                    Saxon.Hej.om.StructuredQName varQName = makeStructuredQName(var, "");
                    nextToken();
                    expect(Saxon.Hej.expr.parser.Token.AS);
                    nextToken();
                    typeList = parseSequenceTypeList();
                    action = makeTracer(parseTypeswitchReturnClause(varQName, outerLet), varQName);
                    if ((action is Saxon.Hej.expr.instruct.TraceExpression)) {
                        ((Saxon.Hej.expr.instruct.TraceExpression)(action)).setProperty("type", typeList[0].ToString());
                    }
                } else {
                    typeList = parseSequenceTypeList();
                    action = makeTracer(parseExprSingle(), null);
                    if ((action is Saxon.Hej.expr.instruct.TraceExpression)) {
                        ((Saxon.Hej.expr.instruct.TraceExpression)(action)).setProperty("type", typeList[0].ToString());
                    }
                }
                types.Add(typeList);
                actions.Add(action);
            }
            if (types.Count==0) {
                grumble("At least one case clause is required in a typeswitch");
            }
            expect(Saxon.Hej.expr.parser.Token.DEFAULT);
            int defaultOffset = t.currentTokenStartOffset;
            nextToken();
            Saxon.Hej.expr.Expression defaultAction;
            if ((t.currentToken == Saxon.Hej.expr.parser.Token.DOLLAR)) {
                nextToken();
                expect(Saxon.Hej.expr.parser.Token.NAME);
                string var = t.currentTokenValue;
                Saxon.Hej.om.StructuredQName varQName = makeStructuredQName(var, "");
                nextToken();
                expect(Saxon.Hej.expr.parser.Token.RETURN);
                nextToken();
                defaultAction = makeTracer(parseTypeswitchReturnClause(varQName, outerLet), varQName);
            } else {
                t.treatCurrentAsOperator();
                expect(Saxon.Hej.expr.parser.Token.RETURN);
                nextToken();
                defaultAction = makeTracer(parseExprSingle(), null);
            }
            Saxon.Hej.expr.Expression lastAction = defaultAction;
            for (int i = (types.Count-1); (i>=0); i--) {
                Saxon.Hej.expr.LocalVariableReference var = new Saxon.Hej.expr.LocalVariableReference(outerLet);
                setLocation(var);
                Saxon.Hej.expr.Expression ioe = new Saxon.Hej.expr.InstanceOfExpression(var, types[i][0]);
                for (int j = 1; (j<types[i].Count); j++) {
                    ioe = new Saxon.Hej.expr.OrExpression(ioe, new Saxon.Hej.expr.InstanceOfExpression(var.copy(new Saxon.Hej.expr.parser.RebindingMap()), types[i][j]));
                }
                setLocation(ioe);
                Saxon.Hej.expr.Expression ife = Saxon.Hej.expr.instruct.Choose.makeConditional(ioe, actions[i], lastAction);
                setLocation(ife);
                lastAction = ife;
            }
            outerLet.setAction(lastAction);
            return makeTracer(outerLet, null);
        }
        private System.Collections.Generic.IList<Saxon.Hej.value.SequenceType> parseSequenceTypeList() {
            System.Collections.Generic.IList<Saxon.Hej.value.SequenceType> typeList = new System.Collections.Generic.List<Saxon.Hej.value.SequenceType>();
            while (true) {
                Saxon.Hej.value.SequenceType type = parseSequenceType();
                typeList.Add(type);
                t.treatCurrentAsOperator();
                if ((t.currentToken == Saxon.Hej.expr.parser.Token.UNION)) {
                    nextToken();
                } else {
                    break ;
                }
            }
            expect(Saxon.Hej.expr.parser.Token.RETURN);
            nextToken();
            return typeList;
        }
        private Saxon.Hej.expr.Expression parseTypeswitchReturnClause(Saxon.Hej.om.StructuredQName varQName, Saxon.Hej.expr.LetExpression outerLet) {
            Saxon.Hej.expr.Expression action;
            Saxon.Hej.expr.LetExpression innerLet = makeLetExpression();
            innerLet.setRequiredType(Saxon.Hej.value.SequenceType.ANY_SEQUENCE);
            innerLet.setVariableQName(varQName);
            innerLet.setSequence(new Saxon.Hej.expr.LocalVariableReference(outerLet));
            declareRangeVariable(innerLet);
            action = parseExprSingle();
            undeclareRangeVariable();
            innerLet.setAction(action);
            return innerLet;
        }
        protected internal override Saxon.Hej.expr.Expression parseSwitchExpression() {
            int offset = t.currentTokenStartOffset;
            nextToken();
            Saxon.Hej.expr.Expression operand = parseExpression();
            expect(Saxon.Hej.expr.parser.Token.RPAR);
            nextToken();
            System.Collections.Generic.IList<Saxon.Hej.expr.Expression> conditions = new System.Collections.Generic.List<Saxon.Hej.expr.Expression>(10);
            System.Collections.Generic.IList<Saxon.Hej.expr.Expression> actions = new System.Collections.Generic.List<Saxon.Hej.expr.Expression>(10);
            Saxon.Hej.expr.LetExpression outerLet = makeLetExpression();
            outerLet.setRequiredType(Saxon.Hej.value.SequenceType.OPTIONAL_ATOMIC);
            outerLet.setVariableQName(new Saxon.Hej.om.StructuredQName("zz", Saxon.Hej.lib.NamespaceConstant.SAXON, "zz_switchVar"));
            outerLet.setSequence(Saxon.Hej.expr.Atomizer.makeAtomizer(operand, null));
            do {
                System.Collections.Generic.IList<Saxon.Hej.expr.Expression> caseExpressions = new System.Collections.Generic.List<Saxon.Hej.expr.Expression>(4);
                expect(Saxon.Hej.expr.parser.Token.CASE);
                do {
                    nextToken();
                    Saxon.Hej.expr.Expression c = parseExprSingle();
                    caseExpressions.Add(c);
                } while ((t.currentToken == Saxon.Hej.expr.parser.Token.CASE));
                expect(Saxon.Hej.expr.parser.Token.RETURN);
                nextToken();
                Saxon.Hej.expr.Expression action = parseExprSingle();
                for (int i = 0; (i<caseExpressions.Count); i++) {
                    Saxon.Hej.expr.EquivalenceComparison vc = new Saxon.Hej.expr.EquivalenceComparison(new Saxon.Hej.expr.LocalVariableReference(outerLet), Saxon.Hej.expr.parser.Token.FEQ, caseExpressions[i]);
                    if ((i == 0)) {
                        conditions.Add(vc);
                        actions.Add(action);
                    } else {
                        Saxon.Hej.expr.OrExpression orExpr = new Saxon.Hej.expr.OrExpression(Saxon.Impl.Helpers.ListUtils.Remove(conditions, (conditions.Count-1)), vc);
                        conditions.Add(orExpr);
                    }
                }
            } while ((t.currentToken == Saxon.Hej.expr.parser.Token.CASE));
            expect(Saxon.Hej.expr.parser.Token.DEFAULT);
            nextToken();
            expect(Saxon.Hej.expr.parser.Token.RETURN);
            nextToken();
            Saxon.Hej.expr.Expression defaultExpr = parseExprSingle();
            conditions.Add(Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.BooleanValue.TRUE));
            actions.Add(defaultExpr);
            Saxon.Hej.expr.instruct.Choose choice = new Saxon.Hej.expr.instruct.Choose(conditions.ToArray(), actions.ToArray());
            outerLet.setAction(choice);
            return makeTracer(outerLet, null);
        }
        protected internal override Saxon.Hej.expr.Expression parseValidateExpression() {
            int offset = t.currentTokenStartOffset;
            int mode = Saxon.Hej.lib.Validation.STRICT;
            bool foundCurly = false;
            Saxon.Hej.type.SchemaType requiredType = null;
            ensureSchemaAware("validate expression");
            switch (t.currentToken) {
              case Saxon.Hej.expr.parser.Token.VALIDATE_STRICT:
                mode = Saxon.Hej.lib.Validation.STRICT;
                nextToken();
                break ;
              case Saxon.Hej.expr.parser.Token.VALIDATE_LAX:
                mode = Saxon.Hej.lib.Validation.LAX;
                nextToken();
                break ;
              case Saxon.Hej.expr.parser.Token.VALIDATE_TYPE:
                mode = Saxon.Hej.lib.Validation.BY_TYPE;
                nextToken();
                expect(Saxon.Hej.expr.parser.Token.KEYWORD_CURLY);
                if (!(Saxon.Hej.om.NameChecker.isQName(Saxon.Hej.str.StringTool.codePoints(t.currentTokenValue)))) {
                    grumble("Schema type name expected after 'validate type");
                }
                requiredType = env.getConfiguration().getSchemaType(makeStructuredQName(t.currentTokenValue, env.getDefaultElementNamespace()));
                if ((requiredType == null)) {
                    grumble(("Unknown schema type "+t.currentTokenValue), "XQST0104");
                }
                foundCurly = true;
                break ;
              case Saxon.Hej.expr.parser.Token.KEYWORD_CURLY:
                if (t.currentTokenValue.Equals("validate")) {
                    mode = Saxon.Hej.lib.Validation.STRICT;
                } else {
                    throw new Saxon.Ejava.lang.AssertionError("shouldn't be parsing a validate expression");
                }
                foundCurly = true;
                break ;
            }
            if (!(foundCurly)) {
                expect(Saxon.Hej.expr.parser.Token.LCURLY);
            }
            nextToken();
            Saxon.Hej.expr.Expression exp = parseExpression();
            if ((exp is Saxon.Hej.expr.instruct.ParentNodeConstructor)) {
                ((Saxon.Hej.expr.instruct.ParentNodeConstructor)(exp)).setValidationAction(mode, ((mode == Saxon.Hej.lib.Validation.BY_TYPE) ? requiredType : null));
            } else {
                exp = new Saxon.Hej.expr.instruct.CopyOf(exp, true, mode, requiredType, true);
                setLocation(exp);
                ((Saxon.Hej.expr.instruct.CopyOf)(exp)).setRequireDocumentOrElement(true);
            }
            expect(Saxon.Hej.expr.parser.Token.RCURLY);
            t.lookAhead();
            nextToken();
            return makeTracer(exp, null);
        }
        protected internal override Saxon.Hej.expr.Expression parseExtensionExpression() {
            Saxon.Hej.type.SchemaType requiredType = null;
            string trimmed = Saxon.Hej.value.Whitespace.trim(t.currentTokenValue);
            int c = 0;
            int len = trimmed.Length;
            while (((c<len) && (" \t\r\n".IndexOf(trimmed[c])<0))) {
                c++;
            }
            string qname = trimmed.Substring(0, c - 0);
            string pragmaContents = "";
            while (((c<len) && (" \t\r\n".IndexOf(trimmed[c])>=0))) {
                c++;
            }
            if ((c<len)) {
                pragmaContents = trimmed.Substring(c, len - c);
            }
            bool validateType = false;
            Saxon.Hej.om.StructuredQName pragmaName = makeStructuredQName(qname, "");
            System.Diagnostics.Debug.Assert((pragmaName != null));
            string uri = pragmaName.getURI();
            string localName = pragmaName.getLocalPart();
            if (uri.Equals(Saxon.Hej.lib.NamespaceConstant.SAXON)) {
                if ("validate-type".Equals(localName)) {
                    if (!(env.getConfiguration().isLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.ENTERPRISE_XQUERY))) {
                        warning(("Ignoring saxon:validate-type. To use this feature "+"you need the Saxon-EE processor from http://www.saxonica.com/"));
                    } else {
                        string typeName = Saxon.Hej.value.Whitespace.trim(pragmaContents);
                        if (!(Saxon.Hej.om.NameChecker.isQName(Saxon.Hej.str.StringTool.codePoints(typeName)))) {
                            grumble(("Schema type name expected in saxon:validate-type pragma: found "+Saxon.Hej.trans.Err.wrap(typeName)));
                        }
                        requiredType = env.getConfiguration().getSchemaType(makeStructuredQName(typeName, env.getDefaultElementNamespace()));
                        if ((requiredType == null)) {
                            grumble(("Unknown schema type "+typeName));
                        }
                        validateType = true;
                    }
                } else {
                    warning((("Ignored pragma "+qname)+" (unrecognized Saxon pragma)"));
                }
            }
            nextToken();
            Saxon.Hej.expr.Expression expr;
            if ((t.currentToken == Saxon.Hej.expr.parser.Token.PRAGMA)) {
                expr = parseExtensionExpression();
            } else {
                expect(Saxon.Hej.expr.parser.Token.LCURLY);
                nextToken();
                if ((t.currentToken == Saxon.Hej.expr.parser.Token.RCURLY)) {
                    t.lookAhead();
                    nextToken();
                    grumble("Unrecognized pragma, with no fallback expression", "XQST0079");
                }
                expr = parseExpression();
                expect(Saxon.Hej.expr.parser.Token.RCURLY);
                t.lookAhead();
                nextToken();
            }
            if (validateType) {
                if ((expr is Saxon.Hej.expr.instruct.ParentNodeConstructor)) {
                    ((Saxon.Hej.expr.instruct.ParentNodeConstructor)(expr)).setValidationAction(Saxon.Hej.lib.Validation.BY_TYPE, requiredType);
                    return expr;
                } else {
                    if ((expr is Saxon.Hej.expr.instruct.AttributeCreator)) {
                        if (!(((requiredType is Saxon.Hej.type.SimpleType)))) {
                            grumble("The type used for validating an attribute must be a simple type");
                        }
                        ((Saxon.Hej.expr.instruct.AttributeCreator)(expr)).setSchemaType((Saxon.Hej.type.SimpleType)(requiredType));
                        ((Saxon.Hej.expr.instruct.AttributeCreator)(expr)).setValidationAction(Saxon.Hej.lib.Validation.BY_TYPE);
                        return expr;
                    } else {
                        Saxon.Hej.expr.instruct.CopyOf copy = new Saxon.Hej.expr.instruct.CopyOf(expr, true, Saxon.Hej.lib.Validation.BY_TYPE, requiredType, true);
                        copy.setLocation(makeLocation());
                        return copy;
                    }
                }
            } else {
                return expr;
            }
        }
        protected internal override Saxon.Hej.expr.Expression parseConstructor() {
            int offset = t.currentTokenStartOffset;
            switch (t.currentToken) {
              case Saxon.Hej.expr.parser.Token.TAG:
                Saxon.Hej.expr.Expression tag = parsePseudoXML(false);
                lookAhead();
                t.setState(Saxon.Hej.expr.parser.Tokenizer.OPERATOR_STATE);
                nextToken();
                return tag;
              case Saxon.Hej.expr.parser.Token.KEYWORD_CURLY:
                string nodeKind = t.currentTokenValue;
                switch (nodeKind) {
                  case "validate":
                    grumble("A validate expression is not allowed within a path expression");
                    break ;
                  case "ordered":
                  case "unordered":
                    nextToken();
                    Saxon.Hej.expr.Expression content;
                    if (((t.currentToken == Saxon.Hej.expr.parser.Token.RCURLY) && allowXPath31Syntax)) {
                        content = Saxon.Hej.expr.Literal.makeEmptySequence();
                    } else {
                        content = parseExpression();
                    }
                    expect(Saxon.Hej.expr.parser.Token.RCURLY);
                    lookAhead();
                    nextToken();
                    return content;
                  case "document":
                    return parseDocumentConstructor(offset);
                  case "element":
                    return parseComputedElementConstructor(offset);
                  case "attribute":
                    return parseComputedAttributeConstructor(offset);
                  case "text":
                    return parseTextNodeConstructor(offset);
                  case "comment":
                    return parseCommentConstructor(offset);
                  case "processing-instruction":
                    return parseProcessingInstructionConstructor(offset);
                  case "namespace":
                    return parseNamespaceConstructor(offset);
                  default:
                    grumble((("Unrecognized node constructor "+t.currentTokenValue)+"{}"));
                    break ;
                }
                break ;
              case Saxon.Hej.expr.parser.Token.ELEMENT_QNAME:
                return parseNamedElementConstructor(offset);
              case Saxon.Hej.expr.parser.Token.ATTRIBUTE_QNAME:
                return parseNamedAttributeConstructor(offset);
              case Saxon.Hej.expr.parser.Token.NAMESPACE_QNAME:
                return parseNamedNamespaceConstructor(offset);
              case Saxon.Hej.expr.parser.Token.PI_QNAME:
                return parseNamedProcessingInstructionConstructor(offset);
            }
            return new Saxon.Hej.expr.ErrorExpression();
        }
        private Saxon.Hej.expr.Expression parseDocumentConstructor(int offset) {
            nextToken();
            Saxon.Hej.expr.Expression content;
            if (((t.currentToken == Saxon.Hej.expr.parser.Token.RCURLY) && allowXPath31Syntax)) {
                content = Saxon.Hej.expr.Literal.makeEmptySequence();
            } else {
                content = parseExpression();
            }
            expect(Saxon.Hej.expr.parser.Token.RCURLY);
            lookAhead();
            nextToken();
            Saxon.Hej.expr.instruct.DocumentInstr doc = new Saxon.Hej.expr.instruct.DocumentInstr(false, null);
            if (!(((Saxon.Hej.query.QueryModule)(env)).isPreserveNamespaces())) {
                content = new Saxon.Hej.expr.instruct.CopyOf(content, false, Saxon.Hej.lib.Validation.PRESERVE, null, true);
            }
            doc.setValidationAction(((Saxon.Hej.query.QueryModule)(env)).getConstructionMode(), null);
            doc.setContentExpression(content);
            setLocation(doc, offset);
            return doc;
        }
        private Saxon.Hej.expr.Expression parseComputedElementConstructor(int offset) {
            nextToken();
            Saxon.Hej.expr.Expression name = parseExpression();
            expect(Saxon.Hej.expr.parser.Token.RCURLY);
            lookAhead();
            nextToken();
            expect(Saxon.Hej.expr.parser.Token.LCURLY);
            t.setState(Saxon.Hej.expr.parser.Tokenizer.DEFAULT_STATE);
            nextToken();
            Saxon.Hej.expr.Expression content = null;
            if ((t.currentToken != Saxon.Hej.expr.parser.Token.RCURLY)) {
                content = parseExpression();
                if (((content is Saxon.Hej.expr.instruct.ElementCreator) && (((Saxon.Hej.expr.instruct.ElementCreator)(content)).getSchemaType() == null))) {
                    ((Saxon.Hej.expr.instruct.ElementCreator)(content)).setValidationAction(Saxon.Hej.lib.Validation.PRESERVE, null);
                }
                expect(Saxon.Hej.expr.parser.Token.RCURLY);
            }
            lookAhead();
            nextToken();
            Saxon.Hej.expr.instruct.Instruction inst;
            if ((name is Saxon.Hej.expr.Literal)) {
                Saxon.Hej.om.GroundedValue vName = ((Saxon.Hej.expr.Literal)(name)).getGroundedValue();
                Saxon.Hej.om.NodeName elemName;
                if (((vName is Saxon.Hej.value.StringValue) && !(((vName is Saxon.Hej.value.AnyURIValue))))) {
                    string lex = ((Saxon.Hej.value.StringValue)(vName)).getStringValue();
                    try {
                        Saxon.Hej.om.QNameParser oldQP = getQNameParser();
                        setQNameParser(oldQP.withUnescaper(null));
                        elemName = makeNodeName(lex, true);
                        setQNameParser(oldQP);
                        elemName.obtainFingerprint(env.getConfiguration().getNamePool());
                    } catch (Saxon.Hej.trans.XPathException staticError) {
                        string code = staticError.getErrorCodeLocalPart();
                        if (("XPST0008".Equals(code) || "XPST0081".Equals(code))) {
                            staticError.setErrorCode("XQDY0074");
                        } else {
                            if ("XPST0003".Equals(code)) {
                                grumble(("Invalid QName in element constructor: "+lex), "XQDY0074", offset);
                                return new Saxon.Hej.expr.ErrorExpression();
                            }
                        }
                        staticError.setLocator(makeLocation());
                        staticError.setIsStaticError(false);
                        return new Saxon.Hej.expr.ErrorExpression(new Saxon.Hej.trans.XmlProcessingException(staticError));
                    }
                } else {
                    if ((vName is Saxon.Hej.value.QualifiedNameValue)) {
                        string uri = ((Saxon.Hej.value.QualifiedNameValue)(vName)).getNamespaceURI();
                        elemName = new Saxon.Hej.om.FingerprintedQName("", uri, ((Saxon.Hej.value.QualifiedNameValue)(vName)).getLocalName());
                        elemName.obtainFingerprint(env.getConfiguration().getNamePool());
                    } else {
                        grumble("Element name must be either a string or a QName", "XPTY0004", offset);
                        return new Saxon.Hej.expr.ErrorExpression();
                    }
                }
                inst = new Saxon.Hej.expr.instruct.FixedElement(elemName, ((Saxon.Hej.query.QueryModule)(env)).getActiveNamespaceBindings(), ((Saxon.Hej.query.QueryModule)(env)).isInheritNamespaces(), true, null, ((Saxon.Hej.query.QueryModule)(env)).getConstructionMode());
                if ((content == null)) {
                    content = Saxon.Hej.expr.Literal.makeEmptySequence();
                }
                if (!(((Saxon.Hej.query.QueryModule)(env)).isPreserveNamespaces())) {
                    content = new Saxon.Hej.expr.instruct.CopyOf(content, false, Saxon.Hej.lib.Validation.PRESERVE, null, true);
                }
                ((Saxon.Hej.expr.instruct.FixedElement)(inst)).setContentExpression(content);
                setLocation(inst, offset);
                return makeTracer(inst, elemName.getStructuredQName());
            } else {
                Saxon.Hej.om.NamespaceResolver ns = new Saxon.Hej.tree.util.NamespaceResolverWithDefault(env.getNamespaceResolver(), env.getDefaultElementNamespace());
                inst = new Saxon.Hej.expr.instruct.ComputedElement(name, null, null, ((Saxon.Hej.query.QueryModule)(env)).getConstructionMode(), ((Saxon.Hej.query.QueryModule)(env)).isInheritNamespaces(), true);
                setLocation(inst);
                if ((content == null)) {
                    content = Saxon.Hej.expr.Literal.makeEmptySequence();
                }
                if (!(((Saxon.Hej.query.QueryModule)(env)).isPreserveNamespaces())) {
                    content = new Saxon.Hej.expr.instruct.CopyOf(content, false, Saxon.Hej.lib.Validation.PRESERVE, null, true);
                }
                ((Saxon.Hej.expr.instruct.ComputedElement)(inst)).setContentExpression(content);
                setLocation(inst, offset);
                return makeTracer(inst, null);
            }
        }
        private Saxon.Hej.expr.Expression parseNamedElementConstructor(int offset) {
            Saxon.Hej.om.NodeName nodeName = makeNodeName(t.currentTokenValue, true);
            Saxon.Hej.expr.Expression content = null;
            nextToken();
            if ((t.currentToken != Saxon.Hej.expr.parser.Token.RCURLY)) {
                content = parseExpression();
                expect(Saxon.Hej.expr.parser.Token.RCURLY);
            }
            lookAhead();
            nextToken();
            Saxon.Hej.expr.instruct.FixedElement el2 = new Saxon.Hej.expr.instruct.FixedElement(nodeName, ((Saxon.Hej.query.QueryModule)(env)).getActiveNamespaceBindings(), ((Saxon.Hej.query.QueryModule)(env)).isInheritNamespaces(), true, null, ((Saxon.Hej.query.QueryModule)(env)).getConstructionMode());
            setLocation(el2, offset);
            if ((content == null)) {
                content = Saxon.Hej.expr.Literal.makeEmptySequence();
            }
            if (!(((Saxon.Hej.query.QueryModule)(env)).isPreserveNamespaces())) {
                content = new Saxon.Hej.expr.instruct.CopyOf(content, false, Saxon.Hej.lib.Validation.PRESERVE, null, true);
            }
            el2.setContentExpression(content);
            return makeTracer(el2, nodeName.getStructuredQName());
        }
        private Saxon.Hej.expr.Expression parseComputedAttributeConstructor(int offset) {
            nextToken();
            Saxon.Hej.expr.Expression name = parseExpression();
            expect(Saxon.Hej.expr.parser.Token.RCURLY);
            lookAhead();
            nextToken();
            expect(Saxon.Hej.expr.parser.Token.LCURLY);
            t.setState(Saxon.Hej.expr.parser.Tokenizer.DEFAULT_STATE);
            nextToken();
            Saxon.Hej.expr.Expression content = null;
            if ((t.currentToken != Saxon.Hej.expr.parser.Token.RCURLY)) {
                content = parseExpression();
                expect(Saxon.Hej.expr.parser.Token.RCURLY);
            }
            lookAhead();
            nextToken();
            if ((name is Saxon.Hej.expr.Literal)) {
                Saxon.Hej.om.GroundedValue vName = ((Saxon.Hej.expr.Literal)(name)).getGroundedValue();
                if (((vName is Saxon.Hej.value.StringValue) && !(((vName is Saxon.Hej.value.AnyURIValue))))) {
                    string lex = ((Saxon.Hej.value.StringValue)(vName)).getStringValue();
                    if ((lex.Equals("xmlns") || lex.StartsWith("xmlns:"))) {
                        grumble("Cannot create a namespace using an attribute constructor", "XQDY0044", offset);
                    }
                    Saxon.Hej.om.NodeName attributeName;
                    try {
                        Saxon.Hej.om.QNameParser oldQP = getQNameParser();
                        setQNameParser(oldQP.withUnescaper(null));
                        attributeName = makeNodeName(lex, false);
                        setQNameParser(oldQP);
                    } catch (Saxon.Hej.trans.XPathException staticError) {
                        string code = staticError.getErrorCodeLocalPart();
                        staticError.setLocator(makeLocation());
                        if (("XPST0008".Equals(code) || "XPST0081".Equals(code))) {
                            staticError.setErrorCode("XQDY0074");
                        } else {
                            if ("XPST0003".Equals(code)) {
                                grumble(("Invalid QName in attribute constructor: "+lex), "XQDY0074", offset);
                                return new Saxon.Hej.expr.ErrorExpression();
                            }
                        }
                        throw;
                    }
                    if ((attributeName.getPrefix().Length==0 && !(attributeName.hasURI("")))) {
                        attributeName = new Saxon.Hej.om.FingerprintedQName("_", attributeName.getURI(), attributeName.getLocalPart(), attributeName.getFingerprint());
                    }
                    Saxon.Hej.expr.instruct.FixedAttribute fatt = new Saxon.Hej.expr.instruct.FixedAttribute(attributeName, Saxon.Hej.lib.Validation.STRIP, null);
                    fatt.setRejectDuplicates();
                    makeSimpleContent(content, fatt, offset);
                    return makeTracer(fatt, null);
                } else {
                    if ((vName is Saxon.Hej.value.QNameValue)) {
                        Saxon.Hej.value.QNameValue qnv = (Saxon.Hej.value.QNameValue)(vName);
                        Saxon.Hej.om.NodeName attributeName = new Saxon.Hej.om.FingerprintedQName(qnv.getPrefix(), qnv.getNamespaceURI(), qnv.getLocalName());
                        attributeName.obtainFingerprint(env.getConfiguration().getNamePool());
                        Saxon.Hej.expr.instruct.FixedAttribute fatt = new Saxon.Hej.expr.instruct.FixedAttribute(attributeName, Saxon.Hej.lib.Validation.STRIP, null);
                        fatt.setRejectDuplicates();
                        makeSimpleContent(content, fatt, offset);
                        return makeTracer(fatt, null);
                    }
                }
            }
            Saxon.Hej.expr.instruct.ComputedAttribute att = new Saxon.Hej.expr.instruct.ComputedAttribute(name, null, env.getNamespaceResolver(), Saxon.Hej.lib.Validation.STRIP, null, true);
            att.setRejectDuplicates();
            makeSimpleContent(content, att, offset);
            return makeTracer(att, null);
        }
        private Saxon.Hej.expr.Expression parseNamedAttributeConstructor(int offset) {
            string warningMessage = null;
            if ((t.currentTokenValue.Equals("xmlns") || t.currentTokenValue.StartsWith("xmlns:"))) {
                warningMessage = "Cannot create a namespace declaration using an attribute constructor";
            }
            Saxon.Hej.om.NodeName attributeName = makeNodeName(t.currentTokenValue, false);
            if ((!(attributeName.getURI().Equals("")) && attributeName.getPrefix().Equals(""))) {
                attributeName = new Saxon.Hej.om.FingerprintedQName("_", attributeName.getURI(), attributeName.getLocalPart());
            }
            Saxon.Hej.expr.Expression attContent = null;
            nextToken();
            if ((t.currentToken != Saxon.Hej.expr.parser.Token.RCURLY)) {
                attContent = parseExpression();
                expect(Saxon.Hej.expr.parser.Token.RCURLY);
            }
            lookAhead();
            nextToken();
            if ((warningMessage == null)) {
                Saxon.Hej.expr.instruct.FixedAttribute att2 = new Saxon.Hej.expr.instruct.FixedAttribute(attributeName, Saxon.Hej.lib.Validation.STRIP, null);
                att2.setRejectDuplicates();
                att2.setRetainedStaticContext(env.makeRetainedStaticContext());
                makeSimpleContent(attContent, att2, offset);
                return makeTracer(att2, attributeName.getStructuredQName());
            } else {
                warning(warningMessage);
                return new Saxon.Hej.expr.ErrorExpression(warningMessage, "XQDY0044", false);
            }
        }
        private Saxon.Hej.expr.Expression parseTextNodeConstructor(int offset) {
            nextToken();
            Saxon.Hej.expr.Expression value;
            if (((t.currentToken == Saxon.Hej.expr.parser.Token.RCURLY) && allowXPath31Syntax)) {
                value = Saxon.Hej.expr.Literal.makeEmptySequence();
            } else {
                value = parseExpression();
            }
            expect(Saxon.Hej.expr.parser.Token.RCURLY);
            lookAhead();
            nextToken();
            Saxon.Hej.expr.Expression select = stringify(value, true, env);
            Saxon.Hej.expr.instruct.ValueOf vof = new Saxon.Hej.expr.instruct.ValueOf(select, false, true);
            setLocation(vof, offset);
            return makeTracer(vof, null);
        }
        private Saxon.Hej.expr.Expression parseCommentConstructor(int offset) {
            nextToken();
            Saxon.Hej.expr.Expression value;
            if (((t.currentToken == Saxon.Hej.expr.parser.Token.RCURLY) && allowXPath31Syntax)) {
                value = Saxon.Hej.expr.Literal.makeEmptySequence();
            } else {
                value = parseExpression();
            }
            expect(Saxon.Hej.expr.parser.Token.RCURLY);
            lookAhead();
            nextToken();
            Saxon.Hej.expr.instruct.Comment com = new Saxon.Hej.expr.instruct.Comment();
            makeSimpleContent(value, com, offset);
            return makeTracer(com, null);
        }
        private Saxon.Hej.expr.Expression parseProcessingInstructionConstructor(int offset) {
            nextToken();
            Saxon.Hej.expr.Expression name = parseExpression();
            expect(Saxon.Hej.expr.parser.Token.RCURLY);
            lookAhead();
            nextToken();
            expect(Saxon.Hej.expr.parser.Token.LCURLY);
            t.setState(Saxon.Hej.expr.parser.Tokenizer.DEFAULT_STATE);
            nextToken();
            Saxon.Hej.expr.Expression content = null;
            if ((t.currentToken != Saxon.Hej.expr.parser.Token.RCURLY)) {
                content = parseExpression();
                expect(Saxon.Hej.expr.parser.Token.RCURLY);
            }
            lookAhead();
            nextToken();
            Saxon.Hej.expr.instruct.ProcessingInstruction pi = new Saxon.Hej.expr.instruct.ProcessingInstruction(name);
            makeSimpleContent(content, pi, offset);
            return makeTracer(pi, null);
        }
        private Saxon.Hej.expr.Expression parseNamedProcessingInstructionConstructor(int offset) {
            string target = t.currentTokenValue;
            string warningMessage = null;
            if (Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase(target, "xml")) {
                warningMessage = "A processing instruction must not be named 'xml' in any combination of upper and lower case";
            }
            if (!(Saxon.Hej.om.NameChecker.isValidNCName(target))) {
                grumble(("Invalid processing instruction name "+Saxon.Hej.trans.Err.wrap(target)));
            }
            Saxon.Hej.expr.Expression piName = new Saxon.Hej.expr.StringLiteral(target);
            Saxon.Hej.expr.Expression piContent = null;
            nextToken();
            if ((t.currentToken != Saxon.Hej.expr.parser.Token.RCURLY)) {
                piContent = parseExpression();
                expect(Saxon.Hej.expr.parser.Token.RCURLY);
            }
            lookAhead();
            nextToken();
            if ((warningMessage == null)) {
                Saxon.Hej.expr.instruct.ProcessingInstruction pi2 = new Saxon.Hej.expr.instruct.ProcessingInstruction(piName);
                makeSimpleContent(piContent, pi2, offset);
                return makeTracer(pi2, null);
            } else {
                warning(warningMessage);
                return new Saxon.Hej.expr.ErrorExpression(warningMessage, "XQDY0064", false);
            }
        }
        protected internal override Saxon.Hej.expr.Expression parseTryCatchExpression() {
            if (!(allowXPath30Syntax)) {
                grumble("try/catch requires XQuery 3.0");
            }
            int offset = t.currentTokenStartOffset;
            nextToken();
            Saxon.Hej.expr.Expression tryExpr;
            if (((t.currentToken == Saxon.Hej.expr.parser.Token.RCURLY) && allowXPath31Syntax)) {
                tryExpr = Saxon.Hej.expr.Literal.makeEmptySequence();
            } else {
                tryExpr = parseExpression();
            }
            Saxon.Hej.expr.TryCatch tryCatch = new Saxon.Hej.expr.TryCatch(tryExpr);
            setLocation(tryCatch, offset);
            expect(Saxon.Hej.expr.parser.Token.RCURLY);
            lookAhead();
            nextToken();
            bool foundOneCatch = false;
            System.Collections.Generic.IList<Saxon.Hej.pattern.QNameTest> tests = new System.Collections.Generic.List<Saxon.Hej.pattern.QNameTest>();
            while (isKeyword("catch")) {
                tests.Clear();
                foundOneCatch = true;
                bool seenCurly = false;
                do {
                    nextToken();
                    string tokv = t.currentTokenValue;
                    switch (t.currentToken) {
                      case Saxon.Hej.expr.parser.Token.NAME:
                        nextToken();
                        tests.Add(makeQNameTest(Saxon.Hej.type.Type.ELEMENT, tokv));
                        break ;
                      case Saxon.Hej.expr.parser.Token.KEYWORD_CURLY:
                        nextToken();
                        tests.Add(makeQNameTest(Saxon.Hej.type.Type.ELEMENT, tokv));
                        seenCurly = true;
                        break ;
                      case Saxon.Hej.expr.parser.Token.PREFIX:
                        nextToken();
                        tests.Add(makeNamespaceTest(Saxon.Hej.type.Type.ELEMENT, tokv));
                        break ;
                      case Saxon.Hej.expr.parser.Token.SUFFIX:
                        nextToken();
                        tokv = t.currentTokenValue;
                        if ((t.currentToken == Saxon.Hej.expr.parser.Token.NAME)) {
                        } else {
                            if ((t.currentToken == Saxon.Hej.expr.parser.Token.KEYWORD_CURLY)) {
                                seenCurly = true;
                            } else {
                                grumble("Expected name after '*:'");
                            }
                        }
                        nextToken();
                        tests.Add(makeLocalNameTest(Saxon.Hej.type.Type.ELEMENT, tokv));
                        break ;
                      case Saxon.Hej.expr.parser.Token.STAR:
                      case Saxon.Hej.expr.parser.Token.MULT:
                        nextToken();
                        tests.Add(Saxon.Hej.pattern.AnyNodeTest.getInstance());
                        break ;
                      default:
                        grumble("Unrecognized name test");
                        return null;
                    }
                } while (((t.currentToken == Saxon.Hej.expr.parser.Token.UNION) && !(t.currentTokenValue.Equals("union"))));
                if (!(seenCurly)) {
                    expect(Saxon.Hej.expr.parser.Token.LCURLY);
                    nextToken();
                }
                Saxon.Hej.pattern.QNameTest test;
                if ((tests.Count == 1)) {
                    test = tests[0];
                } else {
                    test = new Saxon.Hej.pattern.UnionQNameTest(tests);
                }
                catchDepth++;
                Saxon.Hej.expr.Expression catchExpr;
                if (((t.currentToken == Saxon.Hej.expr.parser.Token.RCURLY) && allowXPath31Syntax)) {
                    catchExpr = Saxon.Hej.expr.Literal.makeEmptySequence();
                } else {
                    catchExpr = parseExpression();
                }
                tryCatch.addCatchExpression(test, catchExpr);
                expect(Saxon.Hej.expr.parser.Token.RCURLY);
                lookAhead();
                nextToken();
                catchDepth--;
            }
            if (!(foundOneCatch)) {
                grumble("After try{}, expected 'catch'");
            }
            return tryCatch;
        }
        private Saxon.Hej.expr.Expression parseNamespaceConstructor(int offset) {
            if (!(allowXPath30Syntax)) {
                grumble("Namespace node constructors require XQuery 3.0");
            }
            nextToken();
            Saxon.Hej.expr.Expression nameExpr = parseExpression();
            expect(Saxon.Hej.expr.parser.Token.RCURLY);
            lookAhead();
            nextToken();
            expect(Saxon.Hej.expr.parser.Token.LCURLY);
            t.setState(Saxon.Hej.expr.parser.Tokenizer.DEFAULT_STATE);
            nextToken();
            Saxon.Hej.expr.Expression content = null;
            if ((t.currentToken != Saxon.Hej.expr.parser.Token.RCURLY)) {
                content = parseExpression();
                expect(Saxon.Hej.expr.parser.Token.RCURLY);
            }
            lookAhead();
            nextToken();
            Saxon.Hej.expr.instruct.NamespaceConstructor instr = new Saxon.Hej.expr.instruct.NamespaceConstructor(nameExpr);
            setLocation(instr);
            makeSimpleContent(content, instr, offset);
            return makeTracer(instr, null);
        }
        private Saxon.Hej.expr.Expression parseNamedNamespaceConstructor(int offset) {
            if (!(allowXPath30Syntax)) {
                grumble("Namespace node constructors require XQuery 3.0");
            }
            string target = t.currentTokenValue;
            if (!(Saxon.Hej.om.NameChecker.isValidNCName(target))) {
                grumble(("Invalid namespace prefix "+Saxon.Hej.trans.Err.wrap(target)));
            }
            Saxon.Hej.expr.Expression nsName = new Saxon.Hej.expr.StringLiteral(target);
            Saxon.Hej.expr.Expression nsContent = null;
            nextToken();
            if ((t.currentToken != Saxon.Hej.expr.parser.Token.RCURLY)) {
                nsContent = parseExpression();
                expect(Saxon.Hej.expr.parser.Token.RCURLY);
            }
            lookAhead();
            nextToken();
            Saxon.Hej.expr.instruct.NamespaceConstructor instr = new Saxon.Hej.expr.instruct.NamespaceConstructor(nsName);
            makeSimpleContent(nsContent, instr, offset);
            return makeTracer(instr, null);
        }
        protected internal void makeSimpleContent(Saxon.Hej.expr.Expression content, Saxon.Hej.expr.instruct.SimpleNodeConstructor inst, int offset) {
            if ((content == null)) {
                inst.setSelect(new Saxon.Hej.expr.StringLiteral(Saxon.Hej.value.StringValue.EMPTY_STRING));
            } else {
                inst.setSelect(stringify(content, false, env));
            }
            setLocation(inst, offset);
        }
        private Saxon.Hej.expr.Expression parsePseudoXML(bool allowEndTag) {
            try {
                Saxon.Hej.expr.Expression exp;
                int offset = t.inputOffset;
                char c = t.nextChar();
                switch (c) {
                  case '!':
                    c = t.nextChar();
                    if ((c == '-')) {
                        exp = parseCommentConstructor();
                    } else {
                        if ((c == '[')) {
                            grumble("A CDATA section is allowed only in element content");
                            return null;
                        } else {
                            grumble("Expected '--' or '[CDATA[' after '<!'");
                            return null;
                        }
                    }
                    break ;
                  case '?':
                    exp = parsePIConstructor();
                    break ;
                  case '/':
                    if (allowEndTag) {
                        Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder(16);
                        while (true) {
                            c = t.nextChar();
                            if ((c == '>')) {
                                break ;
                            }
                            sb.append(c);
                        }
                        return new Saxon.Hej.expr.StringLiteral(sb.ToString());
                    }
                    grumble("Unmatched XML end tag");
                    return new Saxon.Hej.expr.ErrorExpression();
                  default:
                    t.unreadChar();
                    exp = parseDirectElementConstructor(allowEndTag);
                    break ;
                }
                setLocation(exp, offset);
                return exp;
            } catch (System.IndexOutOfRangeException e) {
                grumble("End of input encountered while parsing direct constructor");
                return new Saxon.Hej.expr.ErrorExpression();
            }
        }
        private Saxon.Hej.expr.Expression parseDirectElementConstructor(bool isNested) {
            Saxon.Hej.om.NamePool pool = env.getConfiguration().getNamePool();
            bool changesContext = false;
            int offset = (t.inputOffset-1);
            char c;
            Saxon.Ejava.lang.StringBuilder buff = new Saxon.Ejava.lang.StringBuilder(64);
            int namespaceCount = 0;
            while (true) {
                c = t.nextChar();
                if (((((((c == ' ') || (c == '\n')) || (c == '\r')) || (c == '\t')) || (c == '/')) || (c == '>'))) {
                    break ;
                }
                buff.append(c);
            }
            string elname = buff.ToString();
            if (elname.Length==0) {
                grumble("Expected element name after '<'");
            }
            Saxon.Impl.Helpers.OrderedDictionary<string,Saxon.Hej.query.XQueryParser.AttributeDetails> attributes = new Saxon.Impl.Helpers.OrderedDictionary<string, Saxon.Hej.query.XQueryParser.AttributeDetails>(10);
            while (true) {
                c = skipSpaces(c);
                if (((c == '/') || (c == '>'))) {
                    break ;
                }
                int attOffset = (t.inputOffset-1);
                buff.setLength(0);
                do {
                    buff.append(c);
                    c = t.nextChar();
                } while ((((((c != ' ') && (c != '\n')) && (c != '\r')) && (c != '\t')) && (c != '=')));
                string attName = buff.ToString();
                if (!(Saxon.Hej.om.NameChecker.isQName(Saxon.Hej.str.StringTool.codePoints(attName)))) {
                    grumble(("Invalid attribute name "+Saxon.Hej.trans.Err.wrap(attName, Saxon.Hej.trans.Err.ATTRIBUTE)));
                }
                c = skipSpaces(c);
                expectChar(c, '=');
                c = t.nextChar();
                c = skipSpaces(c);
                char delim = c;
                bool isNamespace = ("xmlns".Equals(attName) || attName.StartsWith("xmlns:"));
                int end;
                if (isNamespace) {
                    end = makeNamespaceContent(t.input, t.inputOffset, delim);
                    changesContext = true;
                } else {
                    Saxon.Hej.expr.Expression avt;
                    try {
                        avt = makeAttributeContent(t.input, t.inputOffset, delim, true);
                    } catch (Saxon.Hej.trans.XPathException err) {
                        if (!(err.hasBeenReported())) {
                            grumble(err.Message);
                        }
                        throw;
                    }
                    end = (int)(((Saxon.Hej.value.Int64Value)(((Saxon.Hej.expr.Literal)(avt)).getGroundedValue())).longValue());
                }
                string val = t.input.Substring((t.inputOffset-1), (end+1) - (t.inputOffset-1));
                string rval = t.input.Substring(t.inputOffset, end - t.inputOffset);
                string tail = val;
                int pos;
                while (((pos = tail.IndexOf('\n'))>=0)) {
                    t.incrementLineNumber(((t.inputOffset-1)+pos));
                    tail = tail.Substring((pos+1));
                }
                t.inputOffset = (end+1);
                if (isNamespace) {
                    Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder(rval.Length);
                    bool prevDelim = false;
                    bool prevOpenCurly = false;
                    bool prevCloseCurly = false;
                    for (int i = 0; (i<rval.Length); i++) {
                        char n = rval[i];
                        if ((n == delim)) {
                            prevDelim = !(prevDelim);
                            if (prevDelim) {
                                continue ;
                            }
                        }
                        if ((n == '{')) {
                            prevOpenCurly = !(prevOpenCurly);
                            if (prevOpenCurly) {
                                continue ;
                            }
                        } else {
                            if (prevOpenCurly) {
                                grumble("Namespace must not contain an unescaped opening brace", "XQST0022");
                            }
                        }
                        if ((n == '}')) {
                            prevCloseCurly = !(prevCloseCurly);
                            if (prevCloseCurly) {
                                continue ;
                            }
                        } else {
                            if (prevCloseCurly) {
                                grumble("Namespace must not contain an unescaped closing brace", "XPST0003");
                            }
                        }
                        sb.append(n);
                    }
                    if (prevOpenCurly) {
                        grumble("Namespace must not contain an unescaped opening brace", "XQST0022");
                    }
                    if (prevCloseCurly) {
                        grumble("Namespace must not contain an unescaped closing brace", "XPST0003");
                    }
                    rval = sb.ToString();
                    string uri = uriLiteral(rval);
                    if (!(Saxon.Hej.lib.StandardURIChecker.getInstance().isValidURI(uri))) {
                        grumble("Namespace must be a valid URI value", "XQST0046");
                    }
                    string prefix;
                    if ("xmlns".Equals(attName)) {
                        prefix = "";
                        if (uri.Equals(Saxon.Hej.lib.NamespaceConstant.XML)) {
                            grumble("Cannot have the XML namespace as the default namespace", "XQST0070");
                        }
                    } else {
                        prefix = attName.Substring(6);
                        if ((prefix.Equals("xml") && !(uri.Equals(Saxon.Hej.lib.NamespaceConstant.XML)))) {
                            grumble("Cannot bind the prefix 'xml' to a namespace other than the XML namespace", "XQST0070");
                        } else {
                            if ((uri.Equals(Saxon.Hej.lib.NamespaceConstant.XML) && !(prefix.Equals("xml")))) {
                                grumble("Cannot bind a prefix other than 'xml' to the XML namespace", "XQST0070");
                            } else {
                                if (prefix.Equals("xmlns")) {
                                    grumble("Cannot use xmlns as a namespace prefix", "XQST0070");
                                }
                            }
                        }
                        if (uri.Length==0) {
                            if ((env.getConfiguration().getXMLVersion() == Saxon.Hej.Configuration.XML10)) {
                                grumble("Namespace URI must not be empty", "XQST0085");
                            }
                        }
                    }
                    namespaceCount++;
                    ((Saxon.Hej.query.QueryModule)(env)).declareActiveNamespace(prefix, uri);
                }
                if ((Saxon.Impl.Helpers.MapUtils.Get(attributes, attName) != null)) {
                    if (isNamespace) {
                        grumble(("Duplicate namespace declaration "+attName), "XQST0071", attOffset);
                    } else {
                        grumble(("Duplicate attribute name "+attName), "XQST0040", attOffset);
                    }
                }
                Saxon.Hej.query.XQueryParser.AttributeDetails a = new Saxon.Hej.query.XQueryParser.AttributeDetails();
                a.value = val;
                a.startOffset = attOffset;
                Saxon.Impl.Helpers.MapUtils.Put(attributes, attName, a);
                c = t.nextChar();
                if (!((((((((c == ' ') || (c == '\n')) || (c == '\r')) || (c == '\t')) || (c == '/')) || (c == '>'))))) {
                    grumble("There must be whitespace after every attribute except the last");
                }
            }
            Saxon.Hej.om.StructuredQName qName = null;
            if (scanOnly) {
                qName = Saxon.Hej.om.StandardNames.getStructuredQName(Saxon.Hej.om.StandardNames.XSL_ELEMENT);
            } else {
                try {
                    string[] parts = Saxon.Hej.om.NameChecker.getQNameParts(elname);
                    string @namespace = ((Saxon.Hej.query.QueryModule)(env)).checkURIForPrefix(parts[0]);
                    if ((@namespace == null)) {
                        grumble(("Undeclared prefix in element name "+Saxon.Hej.trans.Err.wrap(elname, Saxon.Hej.trans.Err.ELEMENT)), "XPST0081", offset);
                    }
                    qName = new Saxon.Hej.om.StructuredQName(parts[0], @namespace, parts[1]);
                } catch (Saxon.Hej.om.QNameException e) {
                    grumble(("Invalid element name "+Saxon.Hej.trans.Err.wrap(elname, Saxon.Hej.trans.Err.ELEMENT)), "XPST0003", offset);
                    qName = Saxon.Hej.om.StandardNames.getStructuredQName(Saxon.Hej.om.StandardNames.XSL_ELEMENT);
                }
            }
            int validationMode = ((Saxon.Hej.query.QueryModule)(env)).getConstructionMode();
            Saxon.Hej.om.FingerprintedQName fqn = new Saxon.Hej.om.FingerprintedQName(qName.getPrefix(), qName.getURI(), qName.getLocalPart(), pool.allocateFingerprint(qName.getURI(), qName.getLocalPart()));
            Saxon.Hej.expr.instruct.FixedElement elInst = new Saxon.Hej.expr.instruct.FixedElement(fqn, ((Saxon.Hej.query.QueryModule)(env)).getActiveNamespaceBindings(), ((Saxon.Hej.query.QueryModule)(env)).isInheritNamespaces(), !(isNested), null, validationMode);
            setLocation(elInst, offset);
            System.Collections.Generic.IList<Saxon.Hej.expr.Expression> contents = new System.Collections.Generic.List<Saxon.Hej.expr.Expression>(10);
            Saxon.Hej.z.IntHashSet attFingerprints = new Saxon.Hej.z.IntHashSet(attributes.Count);
            foreach (System.Collections.Generic.KeyValuePair<string,Saxon.Hej.query.XQueryParser.AttributeDetails> entry in attributes) {
                string attName = entry.Key;
                Saxon.Hej.query.XQueryParser.AttributeDetails a = entry.Value;
                string attValue = a.value;
                int attOffset = a.startOffset;
                if (("xmlns".Equals(attName) || attName.StartsWith("xmlns:"))) {
                } else {
                    if (scanOnly) {
                    } else {
                        Saxon.Hej.om.NodeName attributeName = null;
                        string attNamespace;
                        try {
                            string[] parts = Saxon.Hej.om.NameChecker.getQNameParts(attName);
                            if (parts[0].Length==0) {
                                attNamespace = "";
                            } else {
                                attNamespace = ((Saxon.Hej.query.QueryModule)(env)).checkURIForPrefix(parts[0]);
                            }
                            if ((attNamespace == null)) {
                                grumble(("Undeclared prefix in attribute name "+Saxon.Hej.trans.Err.wrap(attName, Saxon.Hej.trans.Err.ATTRIBUTE)), "XPST0081", attOffset);
                            }
                            attributeName = new Saxon.Hej.om.FingerprintedQName(parts[0], attNamespace, parts[1]);
                            int key = attributeName.obtainFingerprint(pool);
                            if (attFingerprints.contains(key)) {
                                grumble(("Duplicate expanded attribute name "+attName), "XQST0040", attOffset);
                            }
                            attFingerprints.add(key);
                        } catch (Saxon.Hej.om.QNameException e) {
                            grumble(("Invalid attribute name "+Saxon.Hej.trans.Err.wrap(attName, Saxon.Hej.trans.Err.ATTRIBUTE)), "XPST0003", attOffset);
                        }
                        System.Diagnostics.Debug.Assert((attributeName != null));
                        Saxon.Hej.expr.instruct.FixedAttribute attInst = new Saxon.Hej.expr.instruct.FixedAttribute(attributeName, Saxon.Hej.lib.Validation.STRIP, null);
                        setLocation(attInst);
                        Saxon.Hej.expr.Expression select;
                        try {
                            select = makeAttributeContent(attValue, 1, attValue[0], false);
                        } catch (Saxon.Hej.trans.XPathException err) {
                            err.setIsStaticError(true);
                            throw;
                        }
                        attInst.setRetainedStaticContext(env.makeRetainedStaticContext());
                        attInst.setSelect(select);
                        attInst.setRejectDuplicates();
                        setLocation(attInst);
                        contents.Add(makeTracer(attInst, attributeName.getStructuredQName()));
                    }
                }
            }
            if ((c == '/')) {
                expectChar(t.nextChar(), '>');
            } else {
                readElementContent(elname, contents);
            }
            Saxon.Hej.expr.Expression[] elk = new Saxon.Hej.expr.Expression[contents.Count];
            for (int i = 0; (i<contents.Count); i++) {
                if ((validationMode != Saxon.Hej.lib.Validation.STRIP)) {
                    contents[i].suppressValidation(validationMode);
                }
                elk[i] = contents[i];
            }
            Saxon.Hej.expr.instruct.Block block = new Saxon.Hej.expr.instruct.Block(elk);
            if (changesContext) {
                block.setRetainedStaticContext(env.makeRetainedStaticContext());
            }
            elInst.setContentExpression(block);
            for (int n = 0; (n<namespaceCount); n++) {
                ((Saxon.Hej.query.QueryModule)(env)).undeclareNamespace();
            }
            return makeTracer(elInst, qName);
        }
        private Saxon.Hej.expr.Expression makeAttributeContent(string avt, int start, char terminator, bool scanOnly) {
            Saxon.Hej.s9api.Location loc = makeLocation();
            System.Collections.Generic.IList<Saxon.Hej.expr.Expression> components = new System.Collections.Generic.List<Saxon.Hej.expr.Expression>(10);
            int i0, i1, i2, i8, i9, len, last;
            last = start;
            len = avt.Length;
            while ((last<len)) {
                i2 = avt.IndexOf(terminator, last);
                if ((i2<0)) {
                    Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException("Attribute constructor is not properly terminated");
                    e.setIsStaticError(true);
                    throw e;
                }
                i0 = avt.IndexOf("{", last);
                i1 = avt.IndexOf("{{", last);
                i8 = avt.IndexOf("}", last);
                i9 = avt.IndexOf("}}", last);
                if (((((i0<0) || (i2<i0))) && (((i8<0) || (i2<i8))))) {
                    addStringComponent(components, avt, last, i2);
                    if ((((i2+1)<avt.Length) && (avt[(i2+1)] == terminator))) {
                        components.Add(new Saxon.Hej.expr.StringLiteral((terminator+"")));
                        last = (i2+2);
                    } else {
                        last = i2;
                        break ;
                    }
                } else {
                    if (((i8>=0) && (((i0<0) || (i8<i0))))) {
                        if ((i8 != i9)) {
                            Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException((("Closing curly brace in attribute value template \""+avt)+"\" must be doubled"));
                            e.setIsStaticError(true);
                            throw e;
                        }
                        addStringComponent(components, avt, last, (i8+1));
                        last = (i8+2);
                    } else {
                        if (((i1>=0) && (i1 == i0))) {
                            addStringComponent(components, avt, last, (i1+1));
                            last = (i1+2);
                        } else {
                            if ((i0>=0)) {
                                if ((i0>last)) {
                                    addStringComponent(components, avt, last, i0);
                                }
                                Saxon.Hej.expr.Expression exp;
                                Saxon.Hej.expr.parser.XPathParser parser = newParser();
                                ((Saxon.Hej.query.XQueryParser)(parser)).executable = executable;
                                parser.setAllowAbsentExpression(allowXPath31Syntax);
                                parser.setScanOnly(scanOnly);
                                parser.setRangeVariableStack(rangeVariables);
                                parser.setCatchDepth(catchDepth);
                                exp = parser.parse(avt, (i0+1), Saxon.Hej.expr.parser.Token.RCURLY, env);
                                if (!(scanOnly)) {
                                    exp = exp.simplify();
                                }
                                last = (parser.getTokenizer().currentTokenStartOffset+1);
                                components.Add(makeStringJoin(exp, env));
                            } else {
                                throw new Saxon.Ejava.lang.IllegalStateException("Internal error parsing direct attribute constructor");
                            }
                        }
                    }
                }
            }
            if (scanOnly) {
                return Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.Int64Value.makeIntegerValue(last));
            }
            if (components.Count==0) {
                return new Saxon.Hej.expr.StringLiteral(Saxon.Hej.value.StringValue.EMPTY_STRING);
            }
            if ((components.Count == 1)) {
                return components[0];
            }
            Saxon.Hej.expr.Expression[] args = new Saxon.Hej.expr.Expression[components.Count];
            args = components.ToArray();
            Saxon.Hej.expr.parser.RetainedStaticContext rsc = new Saxon.Hej.expr.parser.RetainedStaticContext(env);
            Saxon.Hej.expr.Expression fn = Saxon.Hej.functions.SystemFunction.makeCall("concat", rsc, args);
            System.Diagnostics.Debug.Assert((fn != null));
            fn.setLocation(loc);
            return fn;
        }
        private void addStringComponent(System.Collections.Generic.IList<Saxon.Hej.expr.Expression> components, string avt, int start, int end) {
            if ((start<end)) {
                Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder((end-start));
                for (int i = start; (i<end); i++) {
                    char c = avt[i];
                    switch (c) {
                      case '&':
                        {
                            int semic = avt.IndexOf(';', i);
                            if ((semic<0)) {
                                grumble("No closing ';' found for entity or character reference");
                            } else {
                                string entity = avt.Substring((i+1), semic - (i+1));
                                sb.append(new Saxon.Hej.query.XQueryParser.Unescaper(env.getConfiguration().getValidCharacterChecker()).analyzeEntityReference(entity));
                                i = semic;
                            }
                            break ;
                        }
                      case '<':
                        grumble("The < character must not appear in attribute content");
                        break ;
                      case '\n':
                      case '\t':
                        sb.append(' ');
                        break ;
                      case '\r':
                        sb.append(' ');
                        if ((((i+1)<end) && (avt[(i+1)] == '\n'))) {
                            i++;
                        }
                        break ;
                      default:
                        sb.append(c);
                        break ;
                    }
                }
                components.Add(new Saxon.Hej.expr.StringLiteral(sb.ToString()));
            }
        }
        private int makeNamespaceContent(string avt, int start, char terminator) {
            int i2, len, last;
            last = start;
            len = avt.Length;
            while ((last<len)) {
                i2 = avt.IndexOf(terminator, last);
                if ((i2<0)) {
                    Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException("Namespace declaration is not properly terminated");
                    e.setIsStaticError(true);
                    throw e;
                }
                if ((((i2+1)<avt.Length) && (avt[(i2+1)] == terminator))) {
                    last = (i2+2);
                } else {
                    last = i2;
                    break ;
                }
            }
            return last;
        }
        private void readElementContent(string startTag, System.Collections.Generic.IList<Saxon.Hej.expr.Expression> components) {
            try {
                bool afterEnclosedExpr = false;
                while (true) {
                    Saxon.Ejava.lang.StringBuilder text = new Saxon.Ejava.lang.StringBuilder(64);
                    char c;
                    bool containsEntities = false;
                    while (true) {
                        c = t.nextChar();
                        if ((c == '<')) {
                            if ((t.nextChar() == '!')) {
                                if ((t.nextChar() == '[')) {
                                    readCDATASection(text);
                                    containsEntities = true;
                                    continue ;
                                } else {
                                    t.unreadChar();
                                    t.unreadChar();
                                }
                            } else {
                                t.unreadChar();
                            }
                            break ;
                        } else {
                            if ((c == '&')) {
                                text.append(readEntityReference());
                                containsEntities = true;
                            } else {
                                if ((c == '}')) {
                                    c = t.nextChar();
                                    if ((c != '}')) {
                                        grumble("'}' must be written as '}}' within element content");
                                    }
                                    text.append(c);
                                } else {
                                    if ((c == '{')) {
                                        c = t.nextChar();
                                        if ((c != '{')) {
                                            c = '{';
                                            break ;
                                        }
                                        text.append(c);
                                    } else {
                                        if ((!(charChecker.test(c)) && !(Saxon.Hej.serialize.charcode.UTF16CharacterSet.isSurrogate(c)))) {
                                            grumble((("Character code "+c)+" is not a valid XML character"));
                                        }
                                        text.append(c);
                                    }
                                }
                            }
                        }
                    }
                    string textStr = text.ToString();
                    if ((!(textStr.Length==0) && (((containsEntities|((Saxon.Hej.query.QueryModule)(env)).isPreserveBoundarySpace()) || !(Saxon.Hej.value.Whitespace.isAllWhite(Saxon.Hej.str.StringView.of(textStr))))))) {
                        Saxon.Hej.expr.instruct.ValueOf inst = new Saxon.Hej.expr.instruct.ValueOf(new Saxon.Hej.expr.StringLiteral(new Saxon.Hej.value.StringValue(textStr)), false, false);
                        setLocation(inst);
                        components.Add(inst);
                        afterEnclosedExpr = false;
                    }
                    if ((c == '<')) {
                        Saxon.Hej.expr.Expression exp = parsePseudoXML(true);
                        if ((exp is Saxon.Hej.expr.StringLiteral)) {
                            string endTag = ((Saxon.Hej.expr.StringLiteral)(exp)).getString().ToString();
                            if (Saxon.Hej.value.Whitespace.isWhite(endTag[0])) {
                                grumble("End tag contains whitespace before the name");
                            }
                            endTag = Saxon.Hej.value.Whitespace.trim(endTag);
                            if (endTag.Equals(startTag)) {
                                return ;
                            } else {
                                grumble((((("End tag </"+endTag)+"> does not match start tag <")+startTag)+'>'), "XQST0118");
                            }
                        } else {
                            components.Add(exp);
                        }
                    } else {
                        if (afterEnclosedExpr) {
                            Saxon.Hej.expr.Expression previousComponent = components[(components.Count-1)];
                            bool previousComponentIsNodeTest = true;
                            Saxon.Hej.type.UType previousItemType = previousComponent.getStaticUType(Saxon.Hej.type.UType.ANY);
                            previousComponentIsNodeTest = Saxon.Hej.type.UType.ANY_NODE.subsumes(previousItemType);
                            if (!(previousComponentIsNodeTest)) {
                                Saxon.Hej.expr.instruct.ValueOf inst = new Saxon.Hej.expr.instruct.ValueOf(new Saxon.Hej.expr.StringLiteral(Saxon.Hej.value.StringValue.EMPTY_STRING), false, false);
                                setLocation(inst);
                                components.Add(inst);
                            }
                        }
                        t.unreadChar();
                        t.setState(Saxon.Hej.expr.parser.Tokenizer.DEFAULT_STATE);
                        lookAhead();
                        nextToken();
                        if (((t.currentToken == Saxon.Hej.expr.parser.Token.RCURLY) && allowXPath31Syntax)) {
                            components.Add(Saxon.Hej.expr.Literal.makeEmptySequence());
                        } else {
                            Saxon.Hej.expr.Expression exp = parseExpression();
                            if (!(((Saxon.Hej.query.QueryModule)(env)).isPreserveNamespaces())) {
                                exp = new Saxon.Hej.expr.instruct.CopyOf(exp, false, Saxon.Hej.lib.Validation.PRESERVE, null, true);
                            }
                            components.Add(exp);
                            expect(Saxon.Hej.expr.parser.Token.RCURLY);
                        }
                        afterEnclosedExpr = true;
                    }
                }
            } catch (System.IndexOutOfRangeException err) {
                grumble("No closing end tag found for direct element constructor");
            }
        }
        private Saxon.Hej.expr.Expression parsePIConstructor() {
            try {
                Saxon.Ejava.lang.StringBuilder pi = new Saxon.Ejava.lang.StringBuilder(64);
                int firstSpace = -1;
                while (!(pi.ToString().EndsWith("?>"))) {
                    char c = t.nextChar();
                    if (((firstSpace<0) && (" \t\r\n".IndexOf(c)>=0))) {
                        firstSpace = pi.length();
                    }
                    pi.append(c);
                }
                pi.setLength((pi.length()-2));
                string target;
                string data = "";
                if ((firstSpace<0)) {
                    target = pi.ToString();
                } else {
                    target = pi.ToString().Substring(0, firstSpace - 0);
                    firstSpace++;
                    while (((firstSpace<pi.length()) && (" \t\r\n".IndexOf(pi.charAt(firstSpace))>=0))) {
                        firstSpace++;
                    }
                    data = pi.ToString().Substring(firstSpace);
                }
                if (!(Saxon.Hej.om.NameChecker.isValidNCName(target))) {
                    grumble(("Invalid processing instruction name "+Saxon.Hej.trans.Err.wrap(target)));
                }
                if (Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase(target, "xml")) {
                    grumble("A processing instruction must not be named 'xml' in any combination of upper and lower case");
                }
                Saxon.Hej.expr.instruct.ProcessingInstruction instruction = new Saxon.Hej.expr.instruct.ProcessingInstruction(new Saxon.Hej.expr.StringLiteral(target));
                instruction.setSelect(new Saxon.Hej.expr.StringLiteral(data));
                setLocation(instruction);
                return instruction;
            } catch (System.IndexOutOfRangeException err) {
                grumble("No closing '?>' found for processing instruction");
                return null;
            }
        }
        private void readCDATASection(Saxon.Ejava.lang.StringBuilder cdata) {
            try {
                char c;
                c = t.nextChar();
                expectChar(c, 'C');
                c = t.nextChar();
                expectChar(c, 'D');
                c = t.nextChar();
                expectChar(c, 'A');
                c = t.nextChar();
                expectChar(c, 'T');
                c = t.nextChar();
                expectChar(c, 'A');
                c = t.nextChar();
                expectChar(c, '[');
                while (!(cdata.ToString().EndsWith("]]>"))) {
                    cdata.append(t.nextChar());
                }
                cdata.setLength((cdata.length()-3));
            } catch (System.IndexOutOfRangeException err) {
                grumble("No closing ']]>' found for CDATA section");
            }
        }
        private Saxon.Hej.expr.Expression parseCommentConstructor() {
            try {
                char c = t.nextChar();
                expectChar(c, '-');
                Saxon.Ejava.lang.StringBuilder comment = new Saxon.Ejava.lang.StringBuilder(256);
                while (!(comment.ToString().EndsWith("--"))) {
                    comment.append(t.nextChar());
                }
                if ((t.nextChar() != '>')) {
                    grumble("'--' is not permitted in an XML comment");
                }
                string commentText = comment.substring(0, (comment.length()-2));
                Saxon.Hej.expr.instruct.Comment instruction = new Saxon.Hej.expr.instruct.Comment();
                instruction.setSelect(new Saxon.Hej.expr.StringLiteral(new Saxon.Hej.value.StringValue(commentText.ToString())));
                setLocation(instruction);
                return instruction;
            } catch (System.IndexOutOfRangeException err) {
                grumble("No closing '-->' found for comment constructor");
                return null;
            }
        }
        public static Saxon.Hej.expr.Expression stringify(Saxon.Hej.expr.Expression exp, bool noNodeIfEmpty, Saxon.Hej.expr.StaticContext env) {
            if ((exp is Saxon.Hej.expr.StringLiteral)) {
                return exp;
            }
            if ((exp.getLocalRetainedStaticContext() == null)) {
                exp.setRetainedStaticContext(env.makeRetainedStaticContext());
            }
            exp = Saxon.Hej.expr.Atomizer.makeAtomizer(exp, null);
            exp = new Saxon.Hej.expr.AtomicSequenceConverter(exp, Saxon.Hej.type.BuiltInAtomicType.STRING);
            exp = Saxon.Hej.functions.SystemFunction.makeCall("string-join", exp.getRetainedStaticContext(), exp, new Saxon.Hej.expr.StringLiteral(Saxon.Hej.value.StringValue.SINGLE_SPACE));
            System.Diagnostics.Debug.Assert((exp != null));
            if (noNodeIfEmpty) {
                ((Saxon.Hej.functions.StringJoin)(((Saxon.Hej.expr.SystemFunctionCall)(exp)).getTargetFunction())).setReturnEmptyIfEmpty(true);
            }
            return exp;
        }
        protected internal override Saxon.Hej.expr.Literal makeStringLiteral(string token) {
            Saxon.Hej.expr.StringLiteral lit;
            if ((token.IndexOf('&') == -1)) {
                lit = new Saxon.Hej.expr.StringLiteral(token);
            } else {
                string sb = unescape(token);
                lit = new Saxon.Hej.expr.StringLiteral(Saxon.Hej.value.StringValue.makeStringValue(sb));
            }
            setLocation(lit);
            return lit;
        }
        protected internal override string unescape(string token) {
            return new Saxon.Hej.query.XQueryParser.Unescaper(env.getConfiguration().getValidCharacterChecker()).unescape(token);
        }
        public class Unescaper {
            private readonly Saxon.Hej.z.IntPredicateProxy characterChecker;
            public Unescaper(Saxon.Hej.z.IntPredicateProxy characterChecker)  {
                this.characterChecker = characterChecker;
            }
            public string unescape(string token) {
                Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder(token.Length);
                for (int i = 0; (i<token.Length); i++) {
                    char c = token[i];
                    if ((c == '&')) {
                        int semic = token.IndexOf(';', i);
                        if ((semic<0)) {
                            throw new Saxon.Hej.trans.XPathException("No closing ';' found for entity or character reference", "XPST0003");
                        } else {
                            string entity = token.Substring((i+1), semic - (i+1));
                            sb.append(analyzeEntityReference(entity));
                            i = semic;
                        }
                    } else {
                        sb.append(c);
                    }
                }
                return sb.ToString();
            }
            public string analyzeEntityReference(string entity) {
                if ("lt".Equals(entity)) {
                    return "<";
                } else {
                    if ("gt".Equals(entity)) {
                        return ">";
                    } else {
                        if ("amp".Equals(entity)) {
                            return "&";
                        } else {
                            if ("quot".Equals(entity)) {
                                return "\"";
                            } else {
                                if ("apos".Equals(entity)) {
                                    return "'";
                                } else {
                                    if (((entity.Length<2) || (entity[0] != '#'))) {
                                        throw new Saxon.Hej.trans.XPathException((("invalid character reference &"+entity)+';'), "XPST0003");
                                    } else {
                                        return parseCharacterReference(entity);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            private string parseCharacterReference(string entity) {
                int value = 0;
                if ((entity[1] == 'x')) {
                    if ((entity.Length<3)) {
                        throw new Saxon.Hej.trans.XPathException("No hex digits in hexadecimal character reference", "XPST0003");
                    }
                    entity = entity.ToLower();
                    for (int i = 2; (i<entity.Length); i++) {
                        int digit = "0123456789abcdef".IndexOf(entity[i]);
                        if ((digit<0)) {
                            throw new Saxon.Hej.trans.XPathException((("Invalid hex digit '"+entity[i])+"' in character reference"), "XPST0003");
                        }
                        value = (((value*16))+digit);
                        if ((value>Saxon.Hej.serialize.charcode.UTF16CharacterSet.NONBMP_MAX)) {
                            throw new Saxon.Hej.trans.XPathException("Character reference exceeds Unicode codepoint limit", "XQST0090");
                        }
                    }
                } else {
                    for (int i = 1; (i<entity.Length); i++) {
                        int digit = "0123456789".IndexOf(entity[i]);
                        if ((digit<0)) {
                            throw new Saxon.Hej.trans.XPathException((("Invalid digit '"+entity[i])+"' in decimal character reference"), "XPST0003");
                        }
                        value = (((value*10))+digit);
                        if ((value>Saxon.Hej.serialize.charcode.UTF16CharacterSet.NONBMP_MAX)) {
                            throw new Saxon.Hej.trans.XPathException("Character reference exceeds Unicode codepoint limit", "XQST0090");
                        }
                    }
                }
                if (!(characterChecker.test(value))) {
                    throw new Saxon.Hej.trans.XPathException(("Invalid XML character reference x"+Saxon.Impl.Helpers.Utils.intToHexString(value)), "XQST0090");
                }
                if ((value<=0x0000ffff)) {
                    return (""+(char)(value));
                } else {
                    System.Diagnostics.Debug.Assert((value<=0x0010ffff));
                    value -= 0x10000;
                    return ((""+(char)(((unchecked((int)0xd800)|((value>>10))))))+(char)(((unchecked((int)0xdc00)|((value&0x0003ff))))));
                }
            }
        
        }
        private string readEntityReference() {
            try {
                Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder(64);
                while (true) {
                    char c = t.nextChar();
                    if ((c == ';')) {
                        break ;
                    }
                    sb.append(c);
                }
                string entity = sb.ToString();
                return new Saxon.Hej.query.XQueryParser.Unescaper(env.getConfiguration().getValidCharacterChecker()).analyzeEntityReference(entity);
            } catch (System.IndexOutOfRangeException err) {
                grumble("No closing ';' found for entity or character reference");
                return "";
            }
        }
        protected internal override Saxon.Hej.expr.Expression parseStringConstructor() {
            int offset = t.currentTokenStartOffset;
            if (!(allowXPath31Syntax)) {
                throw new Saxon.Hej.trans.XPathException("String constructor expressions require XQuery 3.1");
            }
            System.Collections.Generic.IList<Saxon.Hej.expr.Expression> components = new System.Collections.Generic.List<Saxon.Hej.expr.Expression>();
            components.Add(new Saxon.Hej.expr.StringLiteral(t.currentTokenValue));
            t.next();
            while (true) {
                bool emptyExpression = (t.currentToken == Saxon.Hej.expr.parser.Token.RCURLY);
                if (emptyExpression) {
                    components.Add(new Saxon.Hej.expr.StringLiteral(Saxon.Hej.value.StringValue.EMPTY_STRING));
                } else {
                    Saxon.Hej.expr.Expression enclosed = parseExpression();
                    Saxon.Hej.expr.Expression stringJoin = Saxon.Hej.functions.SystemFunction.makeCall("string-join", env.makeRetainedStaticContext(), enclosed, new Saxon.Hej.expr.StringLiteral(" "));
                    components.Add(stringJoin);
                }
                if ((t.currentToken != Saxon.Hej.expr.parser.Token.RCURLY)) {
                    grumble("Expected '}' after enclosed expression in string constructor");
                }
                Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder(256);
                char c = t.nextChar();
                if ((c != '`')) {
                    grumble("Expected '}`' after enclosed expression in string constructor");
                }
                char prior = (char)(0);
                char penult = (char)(0);
                try {
                    bool continueOuter = false;
                    while (true) {
                        c = t.nextChar();
                        if (((prior == '`') && (c == '{'))) {
                            sb.setLength((sb.length()-1));
                            components.Add(new Saxon.Hej.expr.StringLiteral(sb.ToString()));
                            t.lookAhead();
                            t.next();
                            if ((t.currentToken == Saxon.Hej.expr.parser.Token.RCURLY)) {
                                components.Add(Saxon.Hej.expr.Literal.makeEmptySequence());
                                sb.setLength(0);
                                continue ;
                            } else {
                                continueOuter = true;
                                break ;
                            }
                        } else {
                            if ((((penult == ']') && (prior == '`')) && (c == '`'))) {
                                sb.setLength((sb.length()-2));
                                components.Add(new Saxon.Hej.expr.StringLiteral(sb.ToString()));
                                t.lookAhead();
                                t.next();
                                continueOuter = false;
                                break ;
                            }
                        }
                        sb.append(c);
                        penult = prior;
                        prior = c;
                    }
                    if (!(continueOuter)) {
                        break ;
                    }
                } catch (System.IndexOutOfRangeException e) {
                    grumble("String constructor is missing ]`` terminator ");
                }
            }
            Saxon.Hej.expr.Expression[] args = components.ToArray();
            Saxon.Hej.expr.Expression result = Saxon.Hej.functions.SystemFunction.makeCall("concat", env.makeRetainedStaticContext(), args);
            setLocation(result, offset);
            return result;
        }
        public string uriLiteral(string @in) {
            return Saxon.Hej.value.Whitespace.collapse(unescape(@in)).ToString();
        }
        protected internal void lookAhead() {
            try {
                t.lookAhead();
            } catch (Saxon.Hej.trans.XPathException err) {
                grumble(err.Message);
            }
        }
        protected internal override bool atStartOfRelativePath() {
            return ((t.currentToken == Saxon.Hej.expr.parser.Token.TAG) || base.atStartOfRelativePath());
        }
        protected internal override void testPermittedAxis(int axis, string errorCode) {
            base.testPermittedAxis(axis, errorCode);
            if (((axis == Saxon.Hej.om.AxisInfo.NAMESPACE) && (language == Saxon.Hej.expr.parser.XPathParser.ParsedLanguage.XQUERY))) {
                grumble("The namespace axis is not available in XQuery", errorCode);
            }
        }
        private char skipSpaces(char c) {
            while (((((c == ' ') || (c == '\n')) || (c == '\r')) || (c == '\t'))) {
                c = t.nextChar();
            }
            return c;
        }
        private void expectChar(char actual, char expected) {
            if ((actual != expected)) {
                grumble((((("Expected '"+expected)+"', found '")+actual)+'\''));
            }
        }
        protected internal override string getLanguage() {
            return "XQuery";
        }
        private class AttributeDetails {
            internal string value;
            internal int startOffset;
        
        }
        private class Import {
            internal string namespaceURI;
            internal System.Collections.Generic.IList<string> locationURIs;
            internal int offset;
        
        }
    
    }
}