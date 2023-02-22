using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.@event {
    public class PipelineConfiguration {
        private Saxon.Hej.Configuration config;
        private Saxon.Hej.Controller controller;
        private Saxon.Hej.lib.ParseOptions parseOptions;
        private Saxon.Api.HostLanguage hostLanguage = Saxon.Api.HostLanguage.UNKNOWN;
        private System.Collections.Generic.IDictionary<string,System.Object> components;
        private Saxon.Hej.expr.XPathContext context;
        private System.Func<Saxon.Hej.om.NodeInfo,System.Object> copyInformee;
        public PipelineConfiguration(Saxon.Hej.Configuration config)  {
            this.config = config;
            parseOptions = new Saxon.Hej.lib.ParseOptions();
        }
        public PipelineConfiguration(Saxon.Hej.Configuration config, Saxon.Hej.lib.ParseOptions parseOptions)  {
            this.config = config;
            this.parseOptions = parseOptions;
        }
        public PipelineConfiguration(Saxon.Hej.@event.PipelineConfiguration p)  {
            config = p.config;
            controller = p.controller;
            parseOptions = p.parseOptions;
            hostLanguage = p.hostLanguage;
            if ((p.components != null)) {
                components = new System.Collections.Generic.Dictionary<string, System.Object>(p.components);
            }
            context = p.context;
            copyInformee = null;
        }
        public Saxon.Hej.Configuration getConfiguration() {
            return config;
        }
        public void setConfiguration(Saxon.Hej.Configuration config) {
            this.config = config;
        }
        public Saxon.Hej.lib.ErrorReporter getErrorReporter() {
            Saxon.Hej.lib.ErrorReporter reporter = getParseOptions().getErrorReporter();
            if ((reporter == null)) {
                reporter = ((controller == null) ? config.makeErrorReporter() : controller.getErrorReporter());
            }
            return reporter;
        }
        public void setErrorReporter(Saxon.Hej.lib.ErrorReporter errorReporter) {
            parseOptions = getParseOptions().withErrorReporter(errorReporter);
        }
        public void setParseOptions(Saxon.Hej.lib.ParseOptions options) {
            parseOptions = options;
        }
        public Saxon.Hej.lib.ParseOptions getParseOptions() {
            if ((parseOptions == null)) {
                parseOptions = config.getParseOptions();
            }
            return parseOptions;
        }
        public void setUseXsiSchemaLocation(bool recognize) {
            parseOptions = getParseOptions().withUseXsiSchemaLocation(recognize);
        }
        public void setRecoverFromValidationErrors(bool recover) {
            parseOptions = getParseOptions().withContinueAfterValidationErrors(recover);
        }
        public bool isRecoverFromValidationErrors() {
            return getParseOptions().isContinueAfterValidationErrors();
        }
        public Saxon.Hej.Controller getController() {
            return controller;
        }
        public void setController(Saxon.Hej.Controller controller) {
            this.controller = controller;
        }
        public Saxon.Api.HostLanguage getHostLanguage() {
            if ((hostLanguage == Saxon.Api.HostLanguage.UNKNOWN)) {
                hostLanguage = ((controller == null) ? Saxon.Api.HostLanguage.UNKNOWN : controller.getExecutable().getHostLanguage());
            }
            return hostLanguage;
        }
        public bool isXSLT() {
            return (getHostLanguage() == Saxon.Api.HostLanguage.XSLT);
        }
        public void setHostLanguage(Saxon.Api.HostLanguage language) {
            hostLanguage = language;
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