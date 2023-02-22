using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej {
    public class PreparedStylesheet : Saxon.Hej.expr.instruct.Executable {
        private System.Collections.Generic.Dictionary<System.Uri,Saxon.Hej.PreparedStylesheet> nextStylesheetCache;
        private Saxon.Hej.trans.rules.RuleManager ruleManager;
        private System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.expr.instruct.NamedTemplate> namedTemplateTable;
        private System.Collections.Generic.IDictionary<Saxon.Hej.trans.SymbolicName,Saxon.Hej.expr.Component> componentIndex;
        private readonly Saxon.Hej.om.StructuredQName defaultInitialTemplate;
        private readonly Saxon.Hej.om.StructuredQName defaultInitialMode;
        private readonly string messageReceiverClassName;
        private readonly Saxon.Hej.expr.instruct.GlobalParameterSet compileTimeParams;
        public PreparedStylesheet(Saxon.Hej.style.Compilation compilation) : base (compilation.getConfiguration()) {
            Saxon.Hej.trans.CompilerInfo compilerInfo = compilation.getCompilerInfo();
            setHostLanguage(Saxon.Api.HostLanguage.XSLT);
            if (compilerInfo.isSchemaAware()) {
                int localLic = compilation.getPackageData().getLocalLicenseId();
                getConfiguration().checkLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.ENTERPRISE_XSLT, "schema-aware XSLT", localLic);
                schemaAware = true;
            }
            defaultInitialMode = compilerInfo.getDefaultInitialMode();
            defaultInitialTemplate = compilerInfo.getDefaultInitialTemplate();
            compileTimeParams = compilation.getParameters();
        }
        public Saxon.Hej.trans.XsltController newController() {
            Saxon.Hej.Configuration config = getConfiguration();
            Saxon.Hej.trans.XsltController c = new Saxon.Hej.trans.XsltController(config, this);
            if ((defaultInitialMode != null)) {
                try {
                    c.setInitialMode(defaultInitialMode);
                } catch (Saxon.Hej.trans.XPathException e) {
                }
            }
            return c;
        }
        public Saxon.Hej.expr.instruct.GlobalParameterSet getCompileTimeParams() {
            return compileTimeParams;
        }
        public override void checkSuppliedParameters(Saxon.Hej.expr.instruct.GlobalParameterSet @params) {
            foreach (System.Collections.Generic.KeyValuePair<Saxon.Hej.om.StructuredQName,Saxon.Hej.expr.instruct.GlobalParam> entry in getGlobalParameters()) {
                if (entry.Value.isRequiredParam()) {
                    Saxon.Hej.om.StructuredQName req = entry.Key;
                    if (((getCompileTimeParams().get(req) == null) && (((@params == null) || (@params.get(req) == null))))) {
                        Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(("No value supplied for required parameter "+req.getDisplayName()));
                        err.setErrorCode(((getHostLanguage() == Saxon.Api.HostLanguage.XQUERY) ? "XPDY0002" : "XTDE0050"));
                        throw err;
                    }
                }
            }
            foreach (Saxon.Hej.om.StructuredQName name in @params.getKeys()) {
                Saxon.Hej.expr.instruct.GlobalParam decl = getGlobalParameter(name);
                if (((decl != null) && decl.isStatic())) {
                    throw new Saxon.Hej.trans.XPathException((("Parameter $"+name.getDisplayName())+" cannot be supplied dynamically because it is declared as static"));
                }
                if (compileTimeParams.containsKey(name)) {
                    throw new Saxon.Hej.trans.XPathException((("Parameter $"+name.getDisplayName())+" cannot be supplied dynamically because a value was already supplied at compile time"));
                }
            }
            foreach (Saxon.Hej.om.StructuredQName name in compileTimeParams.getKeys()) {
                @params.put(name, compileTimeParams.get(name));
            }
        }
        public override Saxon.Hej.style.StylesheetPackage getTopLevelPackage() {
            return (Saxon.Hej.style.StylesheetPackage)(base.getTopLevelPackage());
        }
        public void setRuleManager(Saxon.Hej.trans.rules.RuleManager rm) {
            ruleManager = rm;
        }
        public Saxon.Hej.trans.rules.RuleManager getRuleManager() {
            return ruleManager;
        }
        public void putNamedTemplate(Saxon.Hej.om.StructuredQName templateName, Saxon.Hej.expr.instruct.NamedTemplate template) {
            if ((namedTemplateTable == null)) {
                namedTemplateTable = new System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName, Saxon.Hej.expr.instruct.NamedTemplate>(32);
            }
            Saxon.Impl.Helpers.MapUtils.Put(namedTemplateTable, templateName, template);
        }
        public Saxon.Hej.om.StructuredQName getDefaultInitialTemplateName() {
            return defaultInitialTemplate;
        }
        public void setComponentIndex(System.Collections.Generic.IDictionary<Saxon.Hej.trans.SymbolicName,Saxon.Hej.expr.Component> index) {
            componentIndex = index;
        }
        public Saxon.Hej.expr.Component getComponent(Saxon.Hej.trans.SymbolicName name) {
            return Saxon.Impl.Helpers.MapUtils.Get(componentIndex, name);
        }
        public bool isEligibleInitialMode(Saxon.Hej.expr.Component.M component) {
            if ((component == null)) {
                return false;
            }
            if (((component.getVisibility() == Saxon.Hej.trans.Visibility.PUBLIC) || (component.getVisibility() == Saxon.Hej.trans.Visibility.FINAL))) {
                return true;
            }
            if (component.getActor().isUnnamedMode()) {
                return true;
            }
            Saxon.Hej.style.StylesheetPackage top = getTopLevelPackage();
            if (component.getActor().getModeName().Equals(top.getDefaultMode())) {
                return true;
            }
            if (((!(top.isDeclaredModes()) && !(component.getActor().isEmpty())) && (((component.getVisibilityProvenance() == Saxon.Hej.trans.VisibilityProvenance.DEFAULTED) || (component.getVisibility() != Saxon.Hej.trans.Visibility.PRIVATE))))) {
                return true;
            }
            return false;
        }
        public void explainNamedTemplates(Saxon.Hej.trace.ExpressionPresenter presenter) {
            presenter.startElement("namedTemplates");
            if ((namedTemplateTable != null)) {
                foreach (Saxon.Hej.expr.instruct.NamedTemplate t in namedTemplateTable.Values) {
                    presenter.startElement("template");
                    presenter.emitAttribute("name", t.getTemplateName().getDisplayName());
                    presenter.emitAttribute("line", (t.getLineNumber()+""));
                    presenter.emitAttribute("module", t.getSystemId());
                    if ((t.getBody() != null)) {
                        t.getBody().export(presenter);
                    }
                    presenter.endElement();
                }
            }
            presenter.endElement();
        }
        public Saxon.Hej.serialize.SerializationProperties getDeclaredSerializationProperties() {
            Saxon.Hej.serialize.SerializationProperties details = getPrimarySerializationProperties();
            return new Saxon.Hej.serialize.SerializationProperties(new Saxon.Ejava.util.Properties(details.getProperties()), getCharacterMapIndex());
        }
        public Saxon.Hej.PreparedStylesheet getCachedStylesheet(string href, string baseURI) {
            System.Uri abs = null;
            try {
                abs = Saxon.Hej.functions.ResolveURI.makeAbsolute(href, baseURI);
            } catch (System.UriFormatException err) {
            }
            Saxon.Hej.PreparedStylesheet result = null;
            if (((abs != null) && (nextStylesheetCache != null))) {
                result = Saxon.Impl.Helpers.MapUtils.Get(nextStylesheetCache, abs);
            }
            return result;
        }
        public void putCachedStylesheet(string href, string baseURI, Saxon.Hej.PreparedStylesheet pss) {
            System.Uri abs = null;
            try {
                abs = Saxon.Hej.functions.ResolveURI.makeAbsolute(href, baseURI);
            } catch (System.UriFormatException err) {
            }
            if ((abs != null)) {
                if ((nextStylesheetCache == null)) {
                    nextStylesheetCache = new System.Collections.Generic.Dictionary<System.Uri, Saxon.Hej.PreparedStylesheet>(4);
                }
                Saxon.Impl.Helpers.MapUtils.Put(nextStylesheetCache, abs, pss);
            }
        }
        public void explain(Saxon.Hej.trace.ExpressionPresenter presenter) {
            presenter.startElement("stylesheet");
            presenter.@namespace("fn", Saxon.Hej.lib.NamespaceConstant.FN);
            presenter.@namespace("xs", Saxon.Hej.lib.NamespaceConstant.SCHEMA);
            explainGlobalVariables(presenter);
            ruleManager.explainTemplateRules(presenter);
            explainNamedTemplates(presenter);
            presenter.startElement("accumulators");
            foreach (Saxon.Hej.expr.accum.Accumulator acc in getTopLevelPackage().getAccumulatorRegistry().getAllAccumulators()) {
                acc.export(presenter);
            }
            presenter.endElement();
            Saxon.Hej.functions.FunctionLibraryList libList = getFunctionLibrary();
            System.Collections.Generic.IList<Saxon.Hej.functions.FunctionLibrary> libraryList = libList.getLibraryList();
            presenter.startElement("functions");
            foreach (Saxon.Hej.functions.FunctionLibrary lib in libraryList) {
                if ((lib is Saxon.Hej.functions.ExecutableFunctionLibrary)) {
                    foreach (Saxon.Hej.expr.instruct.UserFunction func in ((Saxon.Hej.functions.ExecutableFunctionLibrary)(lib)).getAllFunctions()) {
                        func.export(presenter);
                    }
                }
            }
            presenter.endElement();
            presenter.endElement();
            presenter.close();
        }
    
    }
}