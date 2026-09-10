package com.saxonica.testdriver;

import net.sf.saxon.s9api.*;
import net.sf.saxon.testdriver.Environment;
import net.sf.saxon.testdriver.TestOutcome;

import javax.xml.transform.Source;
import javax.xml.transform.stream.StreamSource;
import java.io.File;
import java.net.URI;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

public class Xslt30TestSuiteDriverJS extends Xslt30TestSuiteDriverEE {

    File exportFile;
    private final Map<String, XdmValue> globalParams = new HashMap<String, XdmValue>();
    private final List<XdmValue> initialFunctionArgs = new ArrayList<XdmValue>();
    private String targetVersion = "JS";
    private int targetVersionNo = 2;

    public static void main(String[] args) throws Exception {
        if (args.length == 0 || args[0].equals("-?")) {
            usage();
        }
        new Xslt30TestSuiteDriverJS().go(args);
    }

    @Override
    public void go(String[] args) throws Exception {
        boolean jsOptionIncluded = false;
        for (int i = 2; i < args.length; i++) {
            int colon = args[i].indexOf(':');
            String argValue = colon < 0 ? "" : args[i].substring(colon + 1);
            if (args[i].startsWith("-js:")) {
                targetVersion = argValue;
                String targetV = argValue.replace("JS", "");
                targetVersionNo = targetV.equals("") ? 2: Integer.parseInt(targetV);
                jsOptionIncluded = true;
            }
        }
        if (!jsOptionIncluded) {
            String[] argsExtra = new String[args.length + 1];
            System.arraycopy(args, 0, argsExtra, 0, args.length);
            argsExtra[args.length] = "-js";
            super.go(argsExtra);
        } else {
            super.go(args);
        }
    }

    @Override
    public boolean hasEECapability() {
        return false;
    }

    @Override
    protected void setDependencyData() {
        // TODO: revise this list
        alwaysOff.add("feature/disabling_output_escaping");
        alwaysOn.add("feature/serialization"); // off for JS1, on for JS2
        alwaysOn.add("feature/namespace_axis");
        alwaysOn.add("feature/backwards_compatibility");
        alwaysOn.add("feature/built_in_derived_types");
        alwaysOff.add("feature/xsl-stylesheet-processing-instruction");
        alwaysOn.add("available_documents");
        alwaysOff.add("ordinal_scheme_name");
        alwaysOn.add("default_calendar_in_date_formatting_functions");
        alwaysOn.add("supported_calendars_in_date_formatting_functions");
        alwaysOn.add("maximum_number_of_decimal_digits");
        alwaysOn.add("default_output_encoding");
        alwaysOn.add("unparsed_text_encoding");
        alwaysOn.add("feature/XSD_1.1"); // TODO try with this on & off
        alwaysOff.add("recognize_id_as_uri_fragment");

        alwaysOn.add("feature/higher_order_functions"); // off for JS1, on for JS2
        alwaysOff.add("feature/XML_1.1");
        alwaysOff.add("feature/dtd");
        alwaysOff.add("feature/HTML4");
        alwaysOn.add("feature/HTML5");

        // simple-uca-fallback and advanced-uca-fallback not used in XSLT30 test suite
        // but may be used in converted QT3 test suite
        alwaysOff.add("feature/simple-uca-fallback"); // off for JS1, on for JS2
        alwaysOff.add("feature/advanced-uca-fallback");

        //needsEE.add("languages_for_numbering");
        alwaysOff.add("feature/streaming");
        alwaysOff.add("feature/schema_aware");
        alwaysOn.add("feature/Saxon-PE");
        alwaysOn.add("feature/Saxon-EE");
        //needsEE.add("feature/XSD_1.1");
        alwaysOn.add("feature/XPath_3.1");
        alwaysOn.add("feature/dynamic_evaluation");
        alwaysOff.add("feature/xquery_invocation");

        alwaysOff.add("detect_accumulator_cycles");
    }

    @Override
    public boolean ensureDependencySatisfied(XdmNode dependency, Environment env) {
        String type = dependency.getNodeName().getLocalName();
        String value = dependency.attribute("value");
        boolean inverse = "false".equals(dependency.attribute("satisfied"));

        String tv = type + "/" + (value == null ? "*" : value);

        boolean needed = !"false".equals(dependency.attribute("satisfied"));

        if (alwaysOn.contains(type) || alwaysOn.contains(tv)) {
            return needed;
        }
        if (alwaysOff.contains(type) || alwaysOff.contains(tv)) {
            return !needed;
        }
        if (needsEE.contains(type) || needsEE.contains(tv)) {
            return !needed;
        }

        if ("collation_uri".equals(type)) {
            //TODO should this check against some list of recognized collations?
            return true;
        }
        if ("combinations_for_numbering".equals(type)) {
            if (value.equals("COPTIC EPACT DIGIT ONE") || value.equals("SINHALA ARCHAIC DIGIT ONE") || value.equals("MENDE KIKAKUI DIGIT ONE")
                    || value.equals("AEGEAN NUMBER ONE") || value.equals("RUMI DIGIT ONE") || value.equals("BRAHMI NUMBER ONE")
                    || value.equals("COUNTING ROD UNIT DIGIT ONE") || value.equals("DIGIT ONE COMMA")) {
                return false;
            }
            return !inverse;
        }
        if ("languages_for_numbering".equals(type) && !"en".equals(value)) {
            return false;
        }
        if ("XSD_1.1".equals(value)) {
            return !inverse;
        }
        if ("default_HTML_version".equals(type) && !"5".equals(value)) {
            return false;
        }
        if ("js-version".equals(type)) {
            for (String p : value.split(" ")) {
                if (p.equals(targetVersionNo + "")) {
                    return !inverse;
                }
                if (p.endsWith("+") && Integer.parseInt(p.replace("+", "")) <= targetVersionNo) {
                    return !inverse;
                }
            }
            return inverse;
        }
        return super.ensureDependencySatisfied(dependency, env);
    }

    @Override
    protected XsltExecutable reloadExportedStylesheet(XsltCompiler compiler, File exportFile) throws SaxonApiException {
        this.exportFile = exportFile;
        globalParams.clear();
        return null;
    }

    @Override
    public File exportStylesheet(XsltCompiler compiler, String filename) throws SaxonApiException {
        // Used when the stylesheet is named as part of the environment
        try {
            File sourceFile = new File(filename);
            String localName = sourceFile.getName();
            File exportLoc = new File(resultsDir + "/export/" + localName + ".sef.json");
            System.err.println("Exporting to " + exportLoc);
            compiler.setTargetEdition(targetVersion);
            XsltPackage compiledPack = compiler.compilePackage(new StreamSource(filename));
            compiledPack.save(exportLoc);
            return exportLoc;
        } catch (SaxonApiException e) {
            System.err.println("*** Failed to export " + filename + " - " + e.getMessage());
            throw e;
        }
    }

    @Override
    protected XsltExecutable exportStylesheet(String testName, String testSetName, XsltCompiler compiler, XsltExecutable sheet, Source styleSource) throws SaxonApiException {
        try {
            File exportFile = new File(resultsDir + "/export/" + testSetName + "/" + testName + ".sef.json");
            System.err.println("Exporting to " + exportFile);
            compiler.setTargetEdition(targetVersion);
            XsltPackage compiledPack = compiler.compilePackage(styleSource);
            compiledPack.save(exportFile);
            sheet = reloadExportedStylesheet(compiler, exportFile);
        } catch (SaxonApiException e) {
            System.err.println(e.getMessage());
            //e.printStackTrace();  //temporary, for debugging
            throw e;
        }
        return sheet;
    }

    @Override
    protected void clearGlobalParameters() {
        globalParams.clear();
    }

    @Override
    protected void setGlobalParameter(QName qName, XdmValue value) {
        globalParams.put("Q{" + qName.getNamespaceUri() + "}" + qName.getLocalName(), value);
    }

    @Override
    protected void clearInitialFunctionArguments() {
        initialFunctionArgs.clear();
    }

    @Override
    protected void addInitialFunctionArgument(XdmValue value) {
        initialFunctionArgs.add(value);
    }

    @Override
    public void runJSTransform(Environment env, TestOutcome outcome, QName initialTemplateName,
                               QName initialModeName, QName initialFunctionName, URI baseOutputURI) {
        outcome.setWarningsReported(true); // force assert-warnings to pass
        //System.err.println("This test driver can only be used to export tests for SaxonJS, not run them");
        return;
    }

}
