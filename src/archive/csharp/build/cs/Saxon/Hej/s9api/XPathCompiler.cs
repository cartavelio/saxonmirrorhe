using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.s9api {
    internal class XPathCompiler {
        private readonly Saxon.Hej.s9api.Processor processor;
        private readonly Saxon.Hej.sxpath.XPathEvaluator evaluator;
        private readonly Saxon.Hej.sxpath.IndependentContext env;
        private Saxon.Api.XdmItemType requiredContextItemType;
        private Saxon.Hej.expr.sort.LFUCache<string,Saxon.Hej.s9api.XPathExecutable> cache = null;
        protected internal XPathCompiler(Saxon.Hej.s9api.Processor processor)  {
            this.processor = processor;
            this.evaluator = new Saxon.Hej.sxpath.XPathEvaluator(processor.getUnderlyingConfiguration());
            env = (Saxon.Hej.sxpath.IndependentContext)(this.evaluator.getStaticContext());
        }
        public Saxon.Hej.s9api.Processor getProcessor() {
            return processor;
        }
        public void setBackwardsCompatible(bool option) {
            if ((cache != null)) {
                cache.clear();
            }
            env.setBackwardsCompatibilityMode(option);
        }
        public bool isBackwardsCompatible() {
            return env.isInBackwardsCompatibleMode();
        }
        public void setSchemaAware(bool schemaAware) {
            if ((schemaAware && !(processor.getUnderlyingConfiguration().isLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.SCHEMA_VALIDATION)))) {
                throw new Saxon.Ejava.lang.UnsupportedOperationException("Schema processing requires a licensed Saxon-EE configuration");
            }
            env.setSchemaAware(schemaAware);
        }
        public bool isSchemaAware() {
            return env.getPackageData().isSchemaAware();
        }
        public void setLanguageVersion(string value) {
            if ((cache != null)) {
                cache.clear();
            }
            int version;
            if ("1.0".Equals(value)) {
                version = 20;
                env.setBackwardsCompatibilityMode(true);
            } else {
                if ("2.0".Equals(value)) {
                    version = 20;
                } else {
                    if (("3.0".Equals(value) || "3.05".Equals(value))) {
                        version = 30;
                    } else {
                        if ("3.1".Equals(value)) {
                            version = 31;
                        } else {
                            if ("4.0".Equals(value)) {
                                version = 40;
                            } else {
                                throw new Saxon.Ejava.lang.IllegalArgumentException("XPath version");
                            }
                        }
                    }
                }
            }
            env.setXPathLanguageLevel(version);
            env.setDefaultFunctionLibrary(version);
        }
        public string getLanguageVersion() {
            if ((env.getXPathVersion() == 20)) {
                return "2.0";
            } else {
                if ((env.getXPathVersion() == 30)) {
                    return "3.0";
                } else {
                    if ((env.getXPathVersion() == 31)) {
                        return "3.1";
                    } else {
                        if ((env.getXPathVersion() == 40)) {
                            return "4.0";
                        } else {
                            throw new Saxon.Ejava.lang.IllegalStateException(("Unknown XPath version "+env.getXPathVersion()));
                        }
                    }
                }
            }
        }
        public void setBaseURI(System.Uri uri) {
            if ((cache != null)) {
                cache.clear();
            }
            if ((uri == null)) {
                env.setBaseURI(null);
            } else {
                if (!(uri.IsAbsoluteUri)) {
                    throw new Saxon.Ejava.lang.IllegalArgumentException("Supplied base URI must be absolute");
                }
                env.setBaseURI(uri.ToString());
            }
        }
        public System.Uri getBaseURI() {
            try {
                return new System.Uri(env.getStaticBaseURI(), System.UriKind.RelativeOrAbsolute);
            } catch (System.UriFormatException err) {
                throw new Saxon.Ejava.lang.IllegalStateException(("Invalid base URI for XPath: "+env.getStaticBaseURI()));
            }
        }
        public void setUnprefixedElementMatchingPolicy(Saxon.Api.UnprefixedElementMatchingPolicy policy) {
            env.setUnprefixedElementMatchingPolicy(policy);
        }
        public Saxon.Api.UnprefixedElementMatchingPolicy getUnprefixedElementMatchingPolicy() {
            return env.getUnprefixedElementMatchingPolicy();
        }
        public void setWarningHandler(Saxon.Hej.lib.ErrorReporter reporter) {
            env.setWarningHandler((message, code, location) => {
                reporter.report(((/*1430*/Saxon.Hej.trans.XmlProcessingIncident)new Saxon.Hej.trans.XmlProcessingIncident(message, code, location).asWarning()));
            } );
        }
        public void declareNamespace(string prefix, string uri) {
            if ((cache != null)) {
                cache.clear();
            }
            env.declareNamespace(prefix, Saxon.Hej.om.NamespaceUri.of(uri));
        }
        public void importSchemaNamespace(string uri) {
            if ((cache != null)) {
                cache.clear();
            }
            env.getImportedSchemaNamespaces().Add(Saxon.Hej.om.NamespaceUri.of(uri));
            env.setSchemaAware(true);
        }
        public void setAllowUndeclaredVariables(bool allow) {
            if ((cache != null)) {
                cache.clear();
            }
            env.setAllowUndeclaredVariables(allow);
        }
        public bool isAllowUndeclaredVariables() {
            return env.isAllowUndeclaredVariables();
        }
        public void declareVariable(Saxon.Api.QName qname) {
            if ((cache != null)) {
                cache.clear();
            }
            env.declareVariable(qname.NamespaceUri, qname.LocalName);
        }
        public void declareVariable(Saxon.Api.QName qname, Saxon.Api.XdmItemType itemType, Saxon.Hej.s9api.OccurrenceIndicator occurrences) {
            if ((cache != null)) {
                cache.clear();
            }
            Saxon.Hej.sxpath.XPathVariable var = env.declareVariable(qname.NamespaceUri, qname.LocalName);
            var.setRequiredType(Saxon.Hej.value.SequenceType.makeSequenceType(((Saxon.Hej.type.ItemType)itemType.Unwrap()), occurrences.getCardinality()));
        }
        public void addXsltFunctionLibrary(Saxon.Hej.s9api.XsltPackage libraryPackage) {
            ((Saxon.Hej.functions.FunctionLibraryList)(env.getFunctionLibrary())).addFunctionLibrary(libraryPackage.getUnderlyingPreparedPackage().getPublicFunctions());
        }
        public void setRequiredContextItemType(Saxon.Api.XdmItemType type) {
            requiredContextItemType = type;
            env.setRequiredContextItemType(((Saxon.Hej.type.ItemType)type.Unwrap()));
        }
        public Saxon.Api.XdmItemType getRequiredContextItemType() {
            return requiredContextItemType;
        }
        public void declareDefaultCollation(string uri) {
            Saxon.Hej.lib.StringCollator c;
            try {
                c = getProcessor().getUnderlyingConfiguration().getCollation(uri);
            } catch (Saxon.Hej.trans.XPathException e) {
                c = null;
            }
            if ((c == null)) {
                throw new Saxon.Ejava.lang.IllegalStateException(("Unknown collation "+uri));
            }
            env.setDefaultCollationName(uri);
        }
        public void setCaching(bool caching) {
            if (caching) {
                if ((cache == null)) {
                    cache = new Saxon.Hej.expr.sort.LFUCache<string, Saxon.Hej.s9api.XPathExecutable>(100, true);
                }
            } else {
                cache = null;
            }
        }
        public bool isCaching() {
            return (cache != null);
        }
        public void setFastCompilation(bool fast) {
            if (fast) {
                env.setOptimizerOptions(new Saxon.Hej.expr.parser.OptimizerOptions(0));
            } else {
                env.setOptimizerOptions(getProcessor().getUnderlyingConfiguration().getOptimizerOptions());
            }
        }
        public bool isFastCompilation() {
            return (env.getOptimizerOptions().getOptions() == 0);
        }
        public Saxon.Hej.s9api.XPathExecutable compile(string source) {
            Saxon.Ejava.util.Objects.requireNonNull(source);
            if ((cache != null)) {
                Saxon.Hej.s9api.XPathExecutable expr = cache.get(source);
                if ((expr == null)) {
                    expr = internalCompile(source);
                    cache.put(source, expr);
                }
                return expr;
            } else {
                return internalCompile(source);
            }
        }
        private Saxon.Hej.s9api.XPathExecutable internalCompile(string source) {
            try {
                env.getDecimalFormatManager().checkConsistency();
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Api.SaxonApiException(e);
            }
            Saxon.Hej.sxpath.XPathEvaluator eval = evaluator;
            Saxon.Hej.sxpath.IndependentContext ic = env;
            if (ic.isAllowUndeclaredVariables()) {
                eval = new Saxon.Hej.sxpath.XPathEvaluator(processor.getUnderlyingConfiguration());
                ic = new Saxon.Hej.sxpath.IndependentContext(env);
                eval.setStaticContext(ic);
                foreach (Saxon.Hej.sxpath.XPathVariable var in env.getExternalVariables()) {
                    Saxon.Hej.sxpath.XPathVariable var2 = ic.declareVariable(var.getVariableQName());
                    var2.setRequiredType(var.getRequiredType());
                }
            }
            try {
                Saxon.Hej.sxpath.XPathExpression cexp = eval.createExpression(source);
                return new Saxon.Hej.s9api.XPathExecutable(cexp, processor, ic);
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Api.SaxonApiException(e);
            }
        }
        public Saxon.Api.XdmValue evaluate(string expression, Saxon.Api.XdmItem contextItem) {
            Saxon.Ejava.util.Objects.requireNonNull(expression);
            bool oldFastCompileOption = isFastCompilation();
            if (!(isCaching())) {
                setFastCompilation(true);
            }
            Saxon.Hej.s9api.XPathSelector selector = compile(expression).load();
            if (!(isCaching())) {
                setFastCompilation(oldFastCompileOption);
            }
            if ((contextItem != null)) {
                selector.setContextItem(contextItem);
            }
            return selector.evaluate();
        }
        public Saxon.Api.XdmItem evaluateSingle(string expression, Saxon.Api.XdmItem contextItem) {
            Saxon.Ejava.util.Objects.requireNonNull(expression);
            bool oldFastCompileOption = isFastCompilation();
            if (!(isCaching())) {
                setFastCompilation(true);
            }
            Saxon.Hej.s9api.XPathSelector selector = compile(expression).load();
            if (!(isCaching())) {
                setFastCompilation(oldFastCompileOption);
            }
            if ((contextItem != null)) {
                selector.setContextItem(contextItem);
            }
            return selector.evaluateSingle();
        }
        public Saxon.Hej.s9api.XPathExecutable compilePattern(string source) {
            Saxon.Ejava.util.Objects.requireNonNull(source);
            try {
                env.getDecimalFormatManager().checkConsistency();
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Api.SaxonApiException(e);
            }
            try {
                Saxon.Hej.sxpath.XPathExpression cexp = evaluator.createPattern(source);
                return new Saxon.Hej.s9api.XPathExecutable(cexp, processor, env);
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Api.SaxonApiException(e);
            }
        }
        public void setDecimalFormatProperty(Saxon.Api.QName format, string property, string value) {
            Saxon.Ejava.util.Objects.requireNonNull(format);
            Saxon.Ejava.util.Objects.requireNonNull(property);
            Saxon.Ejava.util.Objects.requireNonNull(value);
            Saxon.Hej.trans.DecimalFormatManager dfm = env.getDecimalFormatManager();
            if ((dfm == null)) {
                dfm = new Saxon.Hej.trans.DecimalFormatManager(Saxon.Api.HostLanguage.XPATH, env.getXPathVersion());
                env.setDecimalFormatManager(dfm);
            }
            Saxon.Hej.trans.DecimalSymbols symbols = dfm.obtainNamedDecimalFormat(format.GetStructuredQName());
            try {
                switch (property) {
                  case "decimal-separator":
                    symbols.setDecimalSeparator(value);
                    break ;
                  case "grouping-separator":
                    symbols.setGroupingSeparator(value);
                    break ;
                  case "exponent-separator":
                    symbols.setExponentSeparator(value);
                    break ;
                  case "infinity":
                    symbols.setInfinity(value);
                    break ;
                  case "NaN":
                    symbols.setNaN(value);
                    break ;
                  case "minus-sign":
                    symbols.setMinusSign(value);
                    break ;
                  case "percent":
                    symbols.setPercent(value);
                    break ;
                  case "per-mille":
                    symbols.setPerMille(value);
                    break ;
                  case "zero-digit":
                    symbols.setZeroDigit(value);
                    break ;
                  case "digit":
                    symbols.setDigit(value);
                    break ;
                  case "pattern-separator":
                    symbols.setPatternSeparator(value);
                    break ;
                  default:
                    throw new Saxon.Ejava.lang.IllegalArgumentException(("Unknown decimal format attribute "+property));
                }
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Api.SaxonApiException(e);
            }
        }
        public Saxon.Hej.expr.StaticContext getUnderlyingStaticContext() {
            return env;
        }
    
    }
}