using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.s9api {
    internal class XQueryCompiler {
        private readonly Saxon.Hej.s9api.Processor processor;
        private readonly Saxon.Hej.query.StaticQueryContext staticQueryContext;
        private Saxon.Api.XdmItemType requiredContextItemType;
        private string encoding;
        private int languageVersion;
        protected internal XQueryCompiler(Saxon.Hej.s9api.Processor processor)  {
            this.processor = processor;
            this.staticQueryContext = processor.getUnderlyingConfiguration().newStaticQueryContext();
        }
        public Saxon.Hej.s9api.Processor getProcessor() {
            return processor;
        }
        public void setBaseURI(System.Uri baseURI) {
            if ((baseURI == null)) {
                staticQueryContext.setBaseURI(null);
            } else {
                if (!(baseURI.IsAbsoluteUri)) {
                    throw new Saxon.Ejava.lang.IllegalArgumentException(("Base URI must be an absolute URI: "+baseURI));
                }
                staticQueryContext.setBaseURI(baseURI.ToString());
            }
        }
        public System.Uri getBaseURI() {
            if ((staticQueryContext.getBaseURI() == null)) {
                return null;
            }
            try {
                return new System.Uri(staticQueryContext.getBaseURI(), System.UriKind.RelativeOrAbsolute);
            } catch (System.UriFormatException err) {
                throw new Saxon.Ejava.lang.IllegalStateException(("Invalid base URI for query: "+staticQueryContext.getBaseURI()));
            }
        }
        public void setErrorReporter(Saxon.Hej.lib.ErrorReporter reporter) {
            staticQueryContext.setErrorReporter(reporter);
        }
        public Saxon.Hej.lib.ErrorReporter getErrorReporter() {
            return staticQueryContext.getErrorReporter();
        }
        public void setCompileWithTracing(bool option) {
            staticQueryContext.setCompileWithTracing(option);
        }
        public bool isCompileWithTracing() {
            return staticQueryContext.isCompileWithTracing();
        }
        public void setModuleURIResolver(Saxon.Hej.lib.ModuleURIResolver resolver) {
            staticQueryContext.setModuleURIResolver(resolver);
        }
        public Saxon.Hej.lib.ModuleURIResolver getModuleURIResolver() {
            return staticQueryContext.getModuleURIResolver();
        }
        public void setEncoding(string encoding) {
            this.encoding = encoding;
        }
        public string getEncoding() {
            return encoding;
        }
        public void setUpdatingEnabled(bool updating) {
            if ((updating && !(staticQueryContext.getConfiguration().isLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.ENTERPRISE_XQUERY)))) {
                throw new Saxon.Ejava.lang.UnsupportedOperationException("XQuery Update is not supported in this Saxon Configuration");
            }
            staticQueryContext.setUpdatingEnabled(updating);
        }
        public bool isUpdatingEnabled() {
            return staticQueryContext.isUpdatingEnabled();
        }
        public void setSchemaAware(bool schemaAware) {
            if ((schemaAware && !(processor.getUnderlyingConfiguration().isLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.ENTERPRISE_XQUERY)))) {
                throw new Saxon.Ejava.lang.UnsupportedOperationException("Schema-awareness requires a Saxon-EE license");
            }
            staticQueryContext.setSchemaAware(schemaAware);
        }
        public bool isSchemaAware() {
            return staticQueryContext.isSchemaAware();
        }
        public void setStreaming(bool option) {
            staticQueryContext.setStreaming(option);
            if ((option && !(processor.getUnderlyingConfiguration().isLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.ENTERPRISE_XQUERY)))) {
                throw new Saxon.Ejava.lang.UnsupportedOperationException("Streaming requires a Saxon-EE license");
            }
            if (option) {
                setRequiredContextItemType(Saxon.Api.XdmNodeKind.Document);
            }
        }
        public bool isStreaming() {
            return staticQueryContext.isStreaming();
        }
        public void setLanguageVersion(string version) {
            switch (version) {
              case "3.1":
                languageVersion = 31;
                break ;
              case "4.0":
                languageVersion = 40;
                break ;
              default:
                throw new Saxon.Ejava.lang.IllegalArgumentException((("XQuery version must be 3.1 or 4.0 (not "+version)+")"));
            }
            staticQueryContext.setLanguageVersion(languageVersion);
        }
        public string getLanguageVersion() {
            return ((languageVersion == 40) ? "4.0" : "3.1");
        }
        public void declareNamespace(string prefix, string uri) {
            staticQueryContext.declareNamespace(prefix, Saxon.Hej.om.NamespaceUri.of(uri));
        }
        public void declareDefaultCollation(string uri) {
            staticQueryContext.declareDefaultCollation(uri);
        }
        public string getDefaultCollationName() {
            return staticQueryContext.getDefaultCollationName();
        }
        public void setRequiredContextItemType(Saxon.Api.XdmItemType type) {
            requiredContextItemType = type;
            staticQueryContext.setRequiredContextItemType(((Saxon.Hej.type.ItemType)type.Unwrap()));
        }
        public Saxon.Api.XdmItemType getRequiredContextItemType() {
            return requiredContextItemType;
        }
        public void setFastCompilation(bool fast) {
            if (fast) {
                staticQueryContext.setOptimizerOptions(new Saxon.Hej.expr.parser.OptimizerOptions(0));
            } else {
                staticQueryContext.setOptimizerOptions(getProcessor().getUnderlyingConfiguration().getOptimizerOptions());
            }
        }
        public bool isFastCompilation() {
            return (staticQueryContext.getOptimizerOptions().getOptions() == 0);
        }
        public void compileLibrary(string query) {
            try {
                staticQueryContext.compileLibrary(query);
            } catch (Exception e) when (e is Saxon.Hej.trans.XPathException || e is Saxon.Hej.trans.UncheckedXPathException || e is Saxon.Hej.trans.XmlProcessingAbort) {
                throw new Saxon.Api.SaxonApiException(e);
            }
        }
        public void compileLibrary(Saxon.Ejava.io.File query) {
            using (System.IO.FileStream stream = System.IO.File.OpenRead(query.getPath())) {                try {
                    string savedBaseUri = staticQueryContext.getBaseURI();
                    staticQueryContext.setBaseURI(query.toURI().ToString());
                    staticQueryContext.compileLibrary(stream, encoding);
                    staticQueryContext.setBaseURI(savedBaseUri);
                } catch (Exception e) when (e is Saxon.Hej.trans.XPathException || e is Saxon.Hej.trans.UncheckedXPathException || e is Saxon.Hej.trans.XmlProcessingAbort) {
                    throw new Saxon.Api.SaxonApiException(e);
                }
            }        }
        public void compileLibrary(System.IO.TextReader query) {
            try {
                staticQueryContext.compileLibrary(query);
            } catch (Exception e) when (e is Saxon.Hej.trans.XPathException || e is System.IO.IOException || e is Saxon.Hej.trans.UncheckedXPathException || e is Saxon.Hej.trans.XmlProcessingAbort) {
                throw new Saxon.Api.SaxonApiException(e);
            }
        }
        public void compileLibrary(System.IO.Stream query) {
            try {
                staticQueryContext.compileLibrary(query, encoding);
            } catch (Exception e) when (e is Saxon.Hej.trans.XPathException || e is System.IO.IOException || e is Saxon.Hej.trans.UncheckedXPathException || e is Saxon.Hej.trans.XmlProcessingAbort) {
                throw new Saxon.Api.SaxonApiException(e);
            }
        }
        public Saxon.Hej.s9api.XQueryExecutable compile(string query) {
            try {
                return new Saxon.Hej.s9api.XQueryExecutable(processor, staticQueryContext.compileQuery(query));
            } catch (Saxon.Hej.trans.UncheckedXPathException e) {
                throw new Saxon.Api.SaxonApiException(e.getXPathException());
            } catch (Exception e) when (e is Saxon.Hej.trans.XPathException || e is Saxon.Hej.trans.XmlProcessingAbort) {
                throw new Saxon.Api.SaxonApiException(e);
            }
        }
        public Saxon.Hej.s9api.XQueryExecutable compile(Saxon.Ejava.io.File query) {
            using (System.IO.FileStream stream = System.IO.File.OpenRead(query.getPath())) {                try {
                    string savedBaseUri = staticQueryContext.getBaseURI();
                    staticQueryContext.setBaseURI(query.toURI().ToString());
                    Saxon.Hej.s9api.XQueryExecutable exec = new Saxon.Hej.s9api.XQueryExecutable(processor, staticQueryContext.compileQuery(stream, encoding));
                    staticQueryContext.setBaseURI(savedBaseUri);
                    return exec;
                } catch (Saxon.Hej.trans.UncheckedXPathException e) {
                    throw new Saxon.Api.SaxonApiException(e.getXPathException());
                } catch (Exception e) when (e is Saxon.Hej.trans.XPathException || e is Saxon.Hej.trans.XmlProcessingAbort) {
                    throw new Saxon.Api.SaxonApiException(e);
                }
            }        }
        public Saxon.Hej.s9api.XQueryExecutable compile(System.IO.Stream query) {
            try {
                return new Saxon.Hej.s9api.XQueryExecutable(processor, staticQueryContext.compileQuery(query, encoding));
            } catch (Saxon.Hej.trans.UncheckedXPathException e) {
                throw new Saxon.Api.SaxonApiException(e.getXPathException());
            } catch (Exception e) when (e is Saxon.Hej.trans.XPathException || e is Saxon.Hej.trans.XmlProcessingAbort) {
                throw new Saxon.Api.SaxonApiException(e);
            }
        }
        public Saxon.Hej.s9api.XQueryExecutable compile(System.IO.TextReader query) {
            try {
                return new Saxon.Hej.s9api.XQueryExecutable(processor, staticQueryContext.compileQuery(query));
            } catch (Saxon.Hej.trans.UncheckedXPathException e) {
                throw new Saxon.Api.SaxonApiException(e.getXPathException());
            } catch (Exception e) when (e is Saxon.Hej.trans.XPathException || e is Saxon.Hej.trans.XmlProcessingAbort) {
                throw new Saxon.Api.SaxonApiException(e);
            }
        }
        public Saxon.Hej.query.StaticQueryContext getUnderlyingStaticContext() {
            return staticQueryContext;
        }
    
    }
}