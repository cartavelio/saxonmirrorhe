package com.saxonica.ee.trans;

import com.saxonica.ee.extfn.js.IXSLFunctionSet;
import com.saxonica.ee.extfn.js.JSFunctionLibrary;
import net.sf.saxon.Configuration;
import net.sf.saxon.Version;
import net.sf.saxon.expr.Component;
import net.sf.saxon.expr.accum.Accumulator;
import net.sf.saxon.functions.FunctionLibraryList;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.serialize.CharacterMap;
import net.sf.saxon.style.StylesheetPackage;
import net.sf.saxon.trace.ExpressionPresenter;
import net.sf.saxon.trans.*;
import net.sf.saxon.value.DateTimeValue;

import java.util.HashMap;
import java.util.Map;
import java.util.Properties;

/**
 * Subclass of StylesheetPackage for Saxon-EE
 */
public class StylesheetPackageEE extends StylesheetPackage {

    private boolean fallbackToNonStreaming = false;
    private boolean justInTimeCompilation = false;
    private String errorMessageOnExport;
    private String errorCodeOnExport;

    public StylesheetPackageEE(Configuration config) {
        super(config);
    }

    @Override
    public void setFallbackToNonStreaming() {
        for (Mode m : getRuleManager().getAllNamedModes()) {
            m.setStreamable(false);
        }
        getRuleManager().getUnnamedMode().setStreamable(false);
        for (Accumulator acc : getAccumulatorRegistry().getAllAccumulators()) {
            acc.setDeclaredStreamable(false);
        }
        fallbackToNonStreaming = true;
    }

    /**
     * Ask whether a non-streamable construct has been found, forcing the entire stylesheet
     * to fall back to unstreamed processing
     *
     * @return true if the stylesheet must fall back to unstreamed processing
     */

    @Override
    public boolean isFallbackToNonStreaming() {
        return fallbackToNonStreaming;
    }

    /**
     * Ask whether the package was compiled with just-in-time compilation of template rules enabled
     * @return true if just-in-time compilation was enabled when the package was compiled
     */

    @Override
    public boolean isJustInTimeCompilation() {
        return justInTimeCompilation;
    }

    /**
     * Say whether the package was compiled with just-in-time compilation of template rules enabled.
     * This method is called by the compiler itself and should not be called by users.
     *
     * @param justInTimeCompilation set to true if just-in-time compilation was enabled when the package was compiled
     */

    @Override
    public void setJustInTimeCompilation(boolean justInTimeCompilation) {
        this.justInTimeCompilation = justInTimeCompilation;
    }

    /**
     * Mark the package as non-exportable, supplying an error message to be reported if export is attempted
     *
     * @param message   the error message to report indicating why export is not possible
     * @param errorCode the error code to be used if export is attempted and fails
     */

    public void markNonExportable(String message, String errorCode) {
        errorMessageOnExport = message;
        errorCodeOnExport = errorCode;
    }

    /**
     * Output the abstract expression tree to the supplied destination.
     *
     * @param presenter the expression presenter used to display the structure
     */

    @Override
    public void export(ExpressionPresenter presenter) throws XPathException {
        if (!config.isLicensedFeature(Configuration.LicenseFeature.ENTERPRISE_XSLT)) {
            throw new XPathException("Exporting a package requires a Saxon-EE license");
        }
        presenter.setDefaultNamespace(NamespaceUri.SAXON_XSLT_EXPORT);
        ExpressionPresenter.ExportOptions options = presenter.getOptions();
        options.componentMap = new HashMap<>();
        options.packageMap = new HashMap<>();
        options.packageMap.put(this, 0);
        export(presenter, true);
        presenter.close();
    }

    @Override
    protected void addIxslFunctionLibrary(FunctionLibraryList functionLibrary) {
        //functionLibrary.addFunctionLibrary(new IxslFunctionLibrary(config));
        functionLibrary.addFunctionLibrary(IXSLFunctionSet.getInstance());
        functionLibrary.addFunctionLibrary(new JSFunctionLibrary());
    }

    private void export(ExpressionPresenter presenter, boolean topLevel) throws XPathException {
        if (errorMessageOnExport != null) {
            throw new XPathException("Stylesheet package cannot be exported: " + errorMessageOnExport, errorCodeOnExport);
        }
        ExpressionPresenter.ExportOptions options = presenter.getOptions();
        Map<Component, Integer> componentIdMap = options.componentMap;
        Map<StylesheetPackage, Integer> packageIdMap = options.packageMap;
        presenter.startElement("package");
        int packageId = packageIdMap.get(this);
        if (topLevel) {
            presenter.emitAttribute("when", DateTimeValue.now().getStringValue());
            presenter.namespace("fn", NamespaceUri.FN);
            presenter.namespace("xs", NamespaceUri.SCHEMA);
            presenter.namespace("vv", NamespaceUri.SAXON_GENERATED_VARIABLE);
            presenter.namespace("java-type", NamespaceUri.JAVA_TYPE);
            String dmk = getConfiguration().getLicenseFeature("DMK");
            if (dmk != null) {
                presenter.emitAttribute("dmk", dmk);
            }
            String expiry = getConfiguration().getLicenseFeature("Expiration");
            if (expiry != null && !expiry.equals("never")) {
                presenter.emitAttribute("expiry", expiry);
            }
        }
        presenter.emitAttribute("id", packageId+"");
        presenter.emitAttribute("name", getPackageName());
        presenter.emitAttribute("version", getHostLanguageVersion() + "");
        if (getDefaultMode() != null && !getDefaultMode().getStructuredQName().equals(Mode.UNNAMED_MODE_NAME)) {
            presenter.emitAttribute("defaultMode", getDefaultMode().getStructuredQName());
        }
        if (isDeclaredModes()) {
            presenter.emitAttribute("declaredModes", "1");
        }
        if (isStripsTypeAnnotations()) {
            presenter.emitAttribute("stripType", "1");
        }
        if (getPackageVersion() != null) {
            presenter.emitAttribute("packageVersion", getPackageVersion().toString());
        }
        if (isSchemaAware()) {
            presenter.emitAttribute("schemaAware", "1");
        }
        presenter.emitAttribute("saxonProduct", Version.getProductName() + '-' + config.getEditionCode());
        presenter.emitAttribute("saxonVersion", Version.getProductVersion());
        presenter.emitAttribute("buildDateTime", DateTimeValue.now().getStringValue());
        presenter.emitAttribute("target", presenter.getOptions().target);
        presenter.emitAttribute("targetVersion", presenter.getOptions().targetVersion+"");
        presenter.emitAttribute("relocatable", isRelocatable() ? "true" : "false");
        presenter.emitAttribute("implicit", isImplicitPackage() ? "true" : "false");
        for (StylesheetPackage used : getUsedPackages()) {
            if (!packageIdMap.containsKey(used)) {
                packageIdMap.put(used, packageIdMap.size());
                ((StylesheetPackageEE) used).export(presenter, false);
            }
        }
        if (topLevel && globalContextRequirement != null) {
            globalContextRequirement.export(presenter);
        }
        exportSchemaNamespaces(presenter);
        for (Component c : getComponentIndex().values()) {
            if (needToExport(c)) {
                c.export(presenter, componentIdMap, packageIdMap);
            }
        }
        for (Component c : hiddenComponents) {
            if (needToExport(c)) {
                c.export(presenter, componentIdMap, packageIdMap);
            }
        }
        presenter.startElement("overridden");
        for (Component c : overriddenComponents.values()) {
            c.export(presenter, componentIdMap, packageIdMap);
        }
        presenter.endElement();
        keyManager.exportKeys(presenter, componentIdMap);
        exportAccumulators(presenter, componentIdMap);
        exportOutputProperties(presenter);
        exportCharacterMaps(presenter);
        if (stripperRules != null) {
            stripperRules.export(presenter);
        }
        exportDecimalFormats(presenter);
        presenter.endElement();
    }

    private boolean needToExport(Component c) {
        if (c.getVisibility() == Visibility.HIDDEN) {
            return c.getBaseComponent() != null && needToExport(c.getBaseComponent());
        } else if (overriddenComponents.containsKey(c.getActor().getSymbolicName())) {
            return true;
        } else if (c.getVisibility() == Visibility.PRIVATE) {
            return c.getActor().isExportable();
        } else {
            return true;
        }
    }

    private void exportSchemaNamespaces(ExpressionPresenter presenter) {
        for (NamespaceUri ns : schemaIndex) {
            presenter.startElement("schema");
            presenter.emitAttribute("ns", ns.toString());
            presenter.endElement();
        }
    }

    private void exportOutputProperties(ExpressionPresenter presenter) {
        presenter.startElement("output");
        exportOutputDefinition(defaultOutputProperties, presenter);
        presenter.endElement();
        for (Map.Entry<StructuredQName, Properties> namedProps : namedOutputProperties.entrySet()) {
            StructuredQName defName = namedProps.getKey();
            presenter.startElement("output");
            presenter.emitAttribute("name", defName);
            exportOutputDefinition(namedProps.getValue(), presenter);
            presenter.endElement();
        }
    }

    private void exportCharacterMaps(ExpressionPresenter presenter) {
        for (CharacterMap cm : getCharacterMapIndex()) {
            cm.export(presenter);
        }
    }

    private void exportOutputDefinition(Properties properties, ExpressionPresenter presenter) {
        for (String name : properties.stringPropertyNames()) {
            presenter.startElement("property");
            String key = name.startsWith("{") ? "Q" + name : name;
            presenter.emitAttribute("name", key);
            String val = properties.getProperty(name);
            if (key.equals("use-character-maps")) {
                val = val.replace("{", "Q{");
            }
            presenter.emitAttribute("value", val);
            presenter.endElement();
        }
    }

    private void exportDecimalFormats(ExpressionPresenter presenter) {
        DecimalFormatManager dfm = getDecimalFormatManager();
        dfm.getDefaultDecimalFormat().export(null, presenter);
        for (StructuredQName qn : dfm.getDecimalFormatNames()) {
            DecimalSymbols symbols = dfm.getNamedDecimalFormat(qn);
            symbols.export(qn, presenter);
        }
    }

    private void exportAccumulators(ExpressionPresenter presenter, Map<Component, Integer> componentIdMap) throws XPathException {
        for (Accumulator acc : getAccumulatorRegistry().getAllAccumulators()) {
            acc.export(presenter, componentIdMap);

        }
    }

}

// Copyright (c) 2018-2022 Saxonica Limited