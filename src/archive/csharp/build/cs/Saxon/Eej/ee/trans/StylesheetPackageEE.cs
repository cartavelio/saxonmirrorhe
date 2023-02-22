using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.trans {
    public class StylesheetPackageEE : Saxon.Hej.style.StylesheetPackage {
        private bool fallbackToNonStreaming = false;
        private bool justInTimeCompilation = false;
        private string errorMessageOnExport;
        private string errorCodeOnExport;
        public StylesheetPackageEE(Saxon.Hej.Configuration config) : base (config) {
        }
        public override void setFallbackToNonStreaming() {
            foreach (Saxon.Hej.trans.Mode m in getRuleManager().getAllNamedModes()) {
                m.setStreamable(false);
            }
            getRuleManager().getUnnamedMode().setStreamable(false);
            foreach (Saxon.Hej.expr.accum.Accumulator acc in getAccumulatorRegistry().getAllAccumulators()) {
                acc.setDeclaredStreamable(false);
            }
            fallbackToNonStreaming = true;
        }
        public override bool isFallbackToNonStreaming() {
            return fallbackToNonStreaming;
        }
        public override bool isJustInTimeCompilation() {
            return justInTimeCompilation;
        }
        public override void setJustInTimeCompilation(bool justInTimeCompilation) {
            this.justInTimeCompilation = justInTimeCompilation;
        }
        public override void markNonExportable(string message, string errorCode) {
            errorMessageOnExport = message;
            errorCodeOnExport = errorCode;
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter presenter) {
            if (!(config.isLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.ENTERPRISE_XSLT))) {
                throw new Saxon.Hej.trans.XPathException("Exporting a package requires a Saxon-EE license");
            }
            presenter.setDefaultNamespace(Saxon.Hej.om.NamespaceUri.SAXON_XSLT_EXPORT);
            Saxon.Hej.trace.ExpressionPresenter.ExportOptions options = presenter.getOptions();
            options.componentMap = new System.Collections.Generic.Dictionary<Saxon.Hej.expr.Component, int>();
            options.packageMap = new System.Collections.Generic.Dictionary<Saxon.Hej.style.StylesheetPackage, int>();
            Saxon.Impl.Helpers.MapUtils.Put(options.packageMap, this, 0);
            export(presenter, true);
            presenter.close();
        }
        protected internal override void addIxslFunctionLibrary(Saxon.Hej.functions.FunctionLibraryList functionLibrary) {
            functionLibrary.addFunctionLibrary(/*134*/Saxon.Eej.ee.extfn.js.IXSLFunctionSet.getInstance());
            functionLibrary.addFunctionLibrary(new Saxon.Eej.ee.extfn.js.JSFunctionLibrary());
        }
        private void export(Saxon.Hej.trace.ExpressionPresenter presenter, bool topLevel) {
            if ((errorMessageOnExport != null)) {
                throw new Saxon.Hej.trans.XPathException(("Stylesheet package cannot be exported: "+errorMessageOnExport), errorCodeOnExport);
            }
            Saxon.Hej.trace.ExpressionPresenter.ExportOptions options = presenter.getOptions();
            System.Collections.Generic.IDictionary<Saxon.Hej.expr.Component,int> componentIdMap = options.componentMap;
            System.Collections.Generic.IDictionary<Saxon.Hej.style.StylesheetPackage,int> packageIdMap = options.packageMap;
            presenter.startElement("package");
            int packageId = Saxon.Impl.Helpers.MapUtils.Get(packageIdMap, this);
            if (topLevel) {
                presenter.emitAttribute("when", Saxon.Hej.value.DateTimeValue.now().getStringValue());
                presenter.@namespace("fn", Saxon.Hej.om.NamespaceUri.FN);
                presenter.@namespace("xs", Saxon.Hej.om.NamespaceUri.SCHEMA);
                presenter.@namespace("vv", Saxon.Hej.om.NamespaceUri.SAXON_GENERATED_VARIABLE);
                presenter.@namespace("java-type", Saxon.Hej.om.NamespaceUri.JAVA_TYPE);
                string dmk = getConfiguration().getLicenseFeature("DMK");
                if ((dmk != null)) {
                    presenter.emitAttribute("dmk", dmk);
                }
                string expiry = getConfiguration().getLicenseFeature("Expiration");
                if (((expiry != null) && !(expiry.Equals("never")))) {
                    presenter.emitAttribute("expiry", expiry);
                }
            }
            presenter.emitAttribute("id", (packageId+""));
            presenter.emitAttribute("name", getPackageName());
            presenter.emitAttribute("version", (getHostLanguageVersion()+""));
            if (((getDefaultMode() != null) && !(getDefaultMode().getStructuredQName().Equals(Saxon.Hej.trans.Mode.UNNAMED_MODE_NAME)))) {
                presenter.emitAttribute("defaultMode", getDefaultMode().getStructuredQName());
            }
            if (isDeclaredModes()) {
                presenter.emitAttribute("declaredModes", "1");
            }
            if (isStripsTypeAnnotations()) {
                presenter.emitAttribute("stripType", "1");
            }
            if ((getPackageVersion() != null)) {
                presenter.emitAttribute("packageVersion", getPackageVersion().ToString());
            }
            if (isSchemaAware()) {
                presenter.emitAttribute("schemaAware", "1");
            }
            presenter.emitAttribute("saxonProduct", ((Saxon.Hej.Version.getProductName()+'-')+config.getEditionCode()));
            presenter.emitAttribute("saxonVersion", Saxon.Hej.Version.getProductVersion());
            presenter.emitAttribute("buildDateTime", Saxon.Hej.value.DateTimeValue.now().getStringValue());
            presenter.emitAttribute("target", presenter.getOptions().target);
            presenter.emitAttribute("targetVersion", (presenter.getOptions().targetVersion+""));
            presenter.emitAttribute("relocatable", (isRelocatable() ? "true" : "false"));
            presenter.emitAttribute("implicit", (isImplicitPackage() ? "true" : "false"));
            foreach (Saxon.Hej.style.StylesheetPackage used in getUsedPackages()) {
                if (!(packageIdMap.ContainsKey(used))) {
                    Saxon.Impl.Helpers.MapUtils.Put(packageIdMap, used, packageIdMap.Count);
                    ((Saxon.Eej.ee.trans.StylesheetPackageEE)(used)).export(presenter, false);
                }
            }
            if ((topLevel && (globalContextRequirement != null))) {
                globalContextRequirement.export(presenter);
            }
            exportSchemaNamespaces(presenter);
            foreach (Saxon.Hej.expr.Component c in getComponentIndex().Values) {
                if (needToExport(c)) {
                    c.export(presenter, componentIdMap, packageIdMap);
                }
            }
            foreach (Saxon.Hej.expr.Component c in hiddenComponents) {
                if (needToExport(c)) {
                    c.export(presenter, componentIdMap, packageIdMap);
                }
            }
            presenter.startElement("overridden");
            foreach (Saxon.Hej.expr.Component c in overriddenComponents.Values) {
                c.export(presenter, componentIdMap, packageIdMap);
            }
            presenter.endElement();
            keyManager.exportKeys(presenter, componentIdMap);
            exportAccumulators(presenter, componentIdMap);
            exportOutputProperties(presenter);
            exportCharacterMaps(presenter);
            if ((stripperRules != null)) {
                stripperRules.export(presenter);
            }
            exportDecimalFormats(presenter);
            presenter.endElement();
        }
        private bool needToExport(Saxon.Hej.expr.Component c) {
            if ((c.getVisibility() == Saxon.Hej.trans.Visibility.HIDDEN)) {
                return ((c.getBaseComponent() != null) && needToExport(c.getBaseComponent()));
            } else {
                if (overriddenComponents.ContainsKey(c.getActor().getSymbolicName())) {
                    return true;
                } else {
                    if ((c.getVisibility() == Saxon.Hej.trans.Visibility.PRIVATE)) {
                        return c.getActor().isExportable();
                    } else {
                        return true;
                    }
                }
            }
        }
        private void exportSchemaNamespaces(Saxon.Hej.trace.ExpressionPresenter presenter) {
            foreach (Saxon.Hej.om.NamespaceUri ns in schemaIndex) {
                presenter.startElement("schema");
                presenter.emitAttribute("ns", ns.ToString());
                presenter.endElement();
            }
        }
        private void exportOutputProperties(Saxon.Hej.trace.ExpressionPresenter presenter) {
            presenter.startElement("output");
            exportOutputDefinition(defaultOutputProperties, presenter);
            presenter.endElement();
            foreach (System.Collections.Generic.KeyValuePair<Saxon.Hej.om.StructuredQName,Saxon.Ejava.util.Properties> namedProps in namedOutputProperties) {
                Saxon.Hej.om.StructuredQName defName = namedProps.Key;
                presenter.startElement("output");
                presenter.emitAttribute("name", defName);
                exportOutputDefinition(namedProps.Value, presenter);
                presenter.endElement();
            }
        }
        private void exportCharacterMaps(Saxon.Hej.trace.ExpressionPresenter presenter) {
            foreach (Saxon.Hej.serialize.CharacterMap cm in getCharacterMapIndex()) {
                cm.export(presenter);
            }
        }
        private void exportOutputDefinition(Saxon.Ejava.util.Properties properties, Saxon.Hej.trace.ExpressionPresenter presenter) {
            foreach (string name in properties.stringPropertyNames()) {
                presenter.startElement("property");
                string key = (name.StartsWith("{") ? ("Q"+name) : name);
                presenter.emitAttribute("name", key);
                string val = properties.getProperty(name);
                if (key.Equals("use-character-maps")) {
                    val = val.Replace("{", "Q{");
                }
                presenter.emitAttribute("value", val);
                presenter.endElement();
            }
        }
        private void exportDecimalFormats(Saxon.Hej.trace.ExpressionPresenter presenter) {
            Saxon.Hej.trans.DecimalFormatManager dfm = getDecimalFormatManager();
            dfm.getDefaultDecimalFormat().export(null, presenter);
            foreach (Saxon.Hej.om.StructuredQName qn in dfm.getDecimalFormatNames()) {
                Saxon.Hej.trans.DecimalSymbols symbols = dfm.getNamedDecimalFormat(qn);
                symbols.export(qn, presenter);
            }
        }
        private void exportAccumulators(Saxon.Hej.trace.ExpressionPresenter presenter, System.Collections.Generic.IDictionary<Saxon.Hej.expr.Component,int> componentIdMap) {
            foreach (Saxon.Hej.expr.accum.Accumulator acc in getAccumulatorRegistry().getAllAccumulators()) {
                acc.export(presenter, componentIdMap);
            }
        }
    
    }
}