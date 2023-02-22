using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.@event {
    public class PipelineConfiguration {
        private Saxon.Hej.Configuration config;
        private Saxon.Ejavax.xml.transform.URIResolver uriResolver;
        private Saxon.Hej.lib.SchemaURIResolver schemaURIResolver;
        private Saxon.Hej.Controller controller;
        private Saxon.Hej.lib.ParseOptions parseOptions;
        private Saxon.Api.HostLanguage hostLanguage = Saxon.Api.HostLanguage.XSLT;
        private System.Collections.Generic.IDictionary<string,System.Object> components;
        private Saxon.Hej.expr.XPathContext context;
        private System.Func<Saxon.Hej.om.NodeInfo,System.Object> copyInformee;
        public PipelineConfiguration(Saxon.Hej.Configuration config)  {
            this.config = config;
            parseOptions = new Saxon.Hej.lib.ParseOptions();
        }
        public PipelineConfiguration(Saxon.Hej.@event.PipelineConfiguration p)  {
            config = p.config;
            uriResolver = p.uriResolver;
            schemaURIResolver = p.schemaURIResolver;
            controller = p.controller;
            parseOptions = new Saxon.Hej.lib.ParseOptions(p.parseOptions);
            hostLanguage = p.hostLanguage;
            if ((p.components != null)) {
                components = new System.Collections.Generic.Dictionary<string, System.Object>(p.components);
            }
            context = p.context;
        }
        public Saxon.Hej.Configuration getConfiguration() {
            return config;
        }
        public void setConfiguration(Saxon.Hej.Configuration config) {
            this.config = config;
        }
        public Saxon.Hej.lib.ErrorReporter getErrorReporter() {
            Saxon.Hej.lib.ErrorReporter reporter = parseOptions.getErrorReporter();
            if ((reporter == null)) {
                reporter = config.makeErrorReporter();
            }
            return reporter;
        }
        public void setErrorReporter(Saxon.Hej.lib.ErrorReporter errorReporter) {
            parseOptions.setErrorReporter(errorReporter);
        }
        public Saxon.Ejavax.xml.transform.URIResolver getURIResolver() {
            return uriResolver;
        }
        public void setURIResolver(Saxon.Ejavax.xml.transform.URIResolver uriResolver) {
            this.uriResolver = uriResolver;
        }
        public Saxon.Hej.lib.SchemaURIResolver getSchemaURIResolver() {
            return schemaURIResolver;
        }
        public void setParseOptions(Saxon.Hej.lib.ParseOptions options) {
            parseOptions = options;
        }
        public Saxon.Hej.lib.ParseOptions getParseOptions() {
            return parseOptions;
        }
        public void setUseXsiSchemaLocation(bool recognize) {
            parseOptions.setUseXsiSchemaLocation(recognize);
        }
        public void setRecoverFromValidationErrors(bool recover) {
            parseOptions.setContinueAfterValidationErrors(recover);
        }
        public bool isRecoverFromValidationErrors() {
            return parseOptions.isContinueAfterValidationErrors();
        }
        public void setSchemaURIResolver(Saxon.Hej.lib.SchemaURIResolver resolver) {
            schemaURIResolver = resolver;
        }
        public Saxon.Hej.Controller getController() {
            return controller;
        }
        public void setController(Saxon.Hej.Controller controller) {
            this.controller = controller;
        }
        public Saxon.Api.HostLanguage getHostLanguage() {
            return hostLanguage;
        }
        public bool isXSLT() {
            return (hostLanguage == Saxon.Api.HostLanguage.XSLT);
        }
        public void setHostLanguage(Saxon.Api.HostLanguage language) {
            hostLanguage = language;
        }
        public void setExpandAttributeDefaults(bool expand) {
            parseOptions.setExpandAttributeDefaults(expand);
        }
        public void setComponent(string name, System.Object value) {
            if ((components == null)) {
                components = new System.Collections.Generic.Dictionary<string, System.Object>();
            }
            Saxon.Impl.Helpers.MapUtils.Put(components, name, value);
        }
        public void setCopyInformee(System.Func<Saxon.Hej.om.NodeInfo,System.Object> informee) {
            this.copyInformee = informee;
        }
        public System.Func<Saxon.Hej.om.NodeInfo,System.Object> getCopyInformee() {
            return this.copyInformee;
        }
        public System.Object getComponent(string name) {
            if ((components == null)) {
                return null;
            } else {
                return Saxon.Impl.Helpers.MapUtils.Get(components, name);
            }
        }
        public void setXPathContext(Saxon.Hej.expr.XPathContext context) {
            this.context = context;
        }
        public Saxon.Hej.expr.XPathContext getXPathContext() {
            return context;
        }
    
    }
}