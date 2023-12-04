package s9apitest;

import com.saxonica.config.EnterpriseConfiguration;
import jaxptest.ConfigTest;
import net.sf.saxon.Configuration;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.s9api.*;
import net.sf.saxon.style.Compilation;
import net.sf.saxon.style.PrincipalStylesheetModule;
import net.sf.saxon.trans.CompilerInfo;
import net.sf.saxon.trans.LicenseException;
import net.sf.saxon.trans.XPathException;
import org.junit.jupiter.api.Tag;
import org.junit.jupiter.api.Test;

import javax.xml.transform.stream.StreamSource;
import java.io.*;

import static net.sf.saxon.s9api.streams.Steps.child;
import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;


public class TestPackage {
    private ConfigTest configTest = new ConfigTest();
    
    /**
     * Basic test: compile a package, save it to filestore, load it as a library package that overrides it,
     * then execute
     */

    @Test
    public void testPackageExport() {
        try {
            Processor proc = new Processor(true);
            XsltCompiler xc = proc.newXsltCompiler();
            XsltPackage pack = xc.compilePackage(new StreamSource(new File(configTest.getDataDir() + "packages/override-base-f-001.xsl")));
            pack.save(new File(configTest.getDataDir()+"sandpit/"+"test-package-001.pack"));

            Processor proc2 = new Processor(true);
            XsltCompiler xc2 = proc2.newXsltCompiler();
            XsltPackage xp = xc2.loadLibraryPackage(new File(configTest.getDataDir()+"sandpit/" + "test-package-001.pack").toURI());
            xc2.importPackage(xp);
            XsltExecutable xe = xc2.compile(new StreamSource(new File(configTest.getDataDir() + "packages/override-f-001.xsl")));
            Xslt30Transformer xt = xe.load30();
            XdmValue result = xt.callTemplate(new QName("main"));
            System.err.println(result);
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }


    /**
     * Test to check picking up of correct version
     */
    @Test
    public void testPackageVersion() {

        Configuration config = new EnterpriseConfiguration();
        //config.setHostLanguage(Configuration.XSLT);
        CompilerInfo info = config.getDefaultXsltCompilerInfo();
        Compilation compilation = new Compilation(config, info);
        String xsl = "<xsl:package\n" +
                "    name = \"package-002.xsl\"\n" +
                "    package-version = \"2.1.0.5.a\"\n" +
                "    version = \"3.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">\n" +
                "    <xsl:stylesheet  version=\"3.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">\n" +
                "        <xsl:template match=\"value\">\n" +
                "          <out>" +
                "            <xsl:for-each select=\".\">\n" +
                "                <xsl:value-of select=\".\" />\n" +
                "            </xsl:for-each>\n" +
                "          </out>" +
                "        </xsl:template>\n" +
                "    </xsl:stylesheet>\n" +
                "</xsl:package>";
        try {
            PrincipalStylesheetModule pp2 = compilation.compilePackage(new StreamSource(new StringReader(xsl)));
            //config.addPackage(pp2);
        } catch (XPathException e) {
            e.printStackTrace();
            fail();
        }

    }

    @Test
    public void testSavePackage01() {
        try {
            Processor processor = new Processor(true);
            XsltCompiler compiler = processor.newXsltCompiler();
            String xsl = "<xsl:package\n" +
                    "    name = \"package-002.xsl\"\n" +
                    "    package-version = \"2.1.0.5\"\n" +
                    "    version = \"3.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">\n" +
                    "        <xsl:param name='c' select='2'/>" +
                    "        <xsl:template name='go'>\n" +
                    "          <xsl:param name='p1'/>\n" +
                    "          <out><xsl:value-of select='for $i in 1 to 5 return $i+$c+$p1'/></out>\n" +
                    "        </xsl:template>\n" +
                    "        <xsl:template name='main' visibility='public'>\n" +
                    "          <xsl:call-template name='go'>\n" +
                    "          <xsl:with-param name='p1' select='22'/>\n" +
                    "          </xsl:call-template>\n" +
                    "        </xsl:template>\n" +
                    "</xsl:package>";
            XsltPackage pack = compiler.compilePackage(new StreamSource(new StringReader(xsl)));
            File file = new File(configTest.getDataDir()+"sandpit/"+"package01.xsltpack");
            pack.save(file);
            System.err.println("Package written to " + file.getAbsolutePath());
        } catch (SaxonApiException e) {
            fail();
            e.printStackTrace();
        }

        try {
            Processor processor = new Processor(true);
            XsltCompiler compiler = processor.newXsltCompiler();
            File file = new File(configTest.getDataDir()+"sandpit/"+"package01.xsltpack");
            XsltExecutable pack = compiler.loadExecutablePackage(file.toURI());
            System.err.println("Package loaded from " + file.getAbsolutePath());
            XsltTransformer trans = pack.load();
            trans.setInitialTemplate(new QName("main"));
            StringWriter out = new StringWriter();
            Serializer ser = processor.newSerializer(out);
            trans.setDestination(ser);
            trans.transform();
            assertTrue(out.toString().contains("<out>25 26 27 28 29</out>"), "result");
        } catch (SaxonApiException e) {
            fail();
            e.printStackTrace();
        }
    }

    @Test
    public void testSaveAndRestorePackage02() {
        try {
            Processor processor = new Processor(true);
            XsltCompiler compiler = processor.newXsltCompiler();
            String xsl = "<xsl:package\n" +
                    "    name = \"package-002.xsl\"\n" +
                    "    package-version = \"2.1.0.5\"\n" +
                    "    version = \"3.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">\n" +
                    "        <xsl:param name='c' select='2'/>" +
                    "        <xsl:mode/>" +
                    "        <xsl:template match='/'>\n" +
                    "          <out><xsl:apply-templates/></out>\n" +
                    "        </xsl:template>\n" +
                    "        <xsl:template match='/*'>\n" +
                    "          <in><xsl:value-of select='name()'/></in>\n" +
                    "        </xsl:template>\n" +
                    "</xsl:package>";
            XsltPackage pack = compiler.compilePackage(new StreamSource(new StringReader(xsl)));
            File file = new File(configTest.getDataDir()+"sandpit/"+"package02.xsltpack");
            pack.save(file);
            System.err.println("Package written to " + file.getAbsolutePath());
        } catch (SaxonApiException e) {
            fail();
            e.printStackTrace();
        }

        try {
            Processor processor = new Processor(true);
            XsltCompiler compiler = processor.newXsltCompiler();
            File file = new File(configTest.getDataDir()+"sandpit/"+"package02.xsltpack");
            XsltExecutable pack = compiler.loadExecutablePackage(file.toURI());
            System.err.println("Package loaded from " + file.getAbsolutePath());
            DocumentBuilder db = processor.newDocumentBuilder();
            XdmNode root = db.build(new File(configTest.getDataDir() + "books.xml"));
            XsltTransformer trans = pack.load();
            trans.setInitialContextNode(root);
            StringWriter out = new StringWriter();
            Serializer ser = processor.newSerializer(out);
            trans.setDestination(ser);
            trans.transform();
            System.err.println(out.toString());
            assertTrue(out.toString().contains("<out><in>BOOKLIST</in></out>"), "result");
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testExportedPackage01() {
        try {
            Processor processor = new Processor(true);
            // MHK 2016-08-27 - currently crashes if bytecode enabled
            //processor.setConfigurationProperty(FeatureKeys.GENERATE_BYTE_CODE, false);
            XsltCompiler compiler = processor.newXsltCompiler();
            // Note: if necessary, the .sef file can be regenerated from the .xsl file of the same name
            // by using Transform on the command line. The current .sef file was generated using 9.7.0.7, and
            // leaving it in this form is deliberate as it provides a backwards compatibility test
            File file = new File(configTest.getDataDir() + "override-m-021-base.sef");
            XsltPackage pack = compiler.loadLibraryPackage(file.toURI());
            fail("Failed to detect that license has expired");
            System.err.println("Package loaded from " + file.getAbsolutePath());
            compiler.importPackage(pack);
            XsltExecutable exec = compiler.compile(new StreamSource(new File(configTest.getDataDir() + "override-m-021.xsl")));
            DocumentBuilder db = processor.newDocumentBuilder();
            XdmNode root = db.build(new File(configTest.getDataDir() + "override-m-021.xml"));
            Xslt30Transformer trans = exec.load30();
            StringWriter out = new StringWriter();
            Serializer ser = processor.newSerializer(out);
            trans.applyTemplates(root, ser);
            //System.err.println(out.toString());
            assertTrue(out.toString().contains("<root><list><item><bar>foo 1</bar><list><item><bar>foo 2</bar></item></list></item></list></root>"), "result");
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        } catch (LicenseException le) {
            assertTrue(le.getMessage().contains("does not cover upgrade"));
        }
    }

    @Test
    public void testConfig01() {
        // using package declared first in configuration file
        runConfigFileTest("packages/config-100.xml", "http://package-one/", true);
    }

    @Test
    public void testConfig02() {
        // using package declared last in configuration file
        runConfigFileTest("packages/config-101.xml", "http://package-one/", true);
    }

    @Test
    public void testConfig03() {
        // cyclic dependencies in configuration file
        runConfigFileTest("packages/config-103.xml", "http://package-103/", false);
    }

    public void runConfigFileTest(String configurationFile, String startPack, boolean expectSuccess) {
        try {
            String configFile = configTest.getDataDir() + configurationFile;
            Processor processor = new Processor(new StreamSource(new File(configFile)));
            XsltCompiler compiler = processor.newXsltCompiler();
            XsltPackage pack1 = compiler.obtainPackage(startPack, "*");
            XsltExecutable pack = pack1.link();
            DocumentBuilder db = processor.newDocumentBuilder();
            XdmNode root = db.build(new File(configTest.getDataDir() + "books.xml"));
            XsltTransformer trans = pack.load();
            trans.setInitialContextNode(root);
            StringWriter out = new StringWriter();
            Serializer ser = processor.newSerializer(out);
            trans.setDestination(ser);
            trans.setInitialTemplate(new QName("main"));
            trans.transform();
            System.err.println(out.toString());
            if (expectSuccess) {
                assertTrue(out.toString().contains("<out>42</out>"), "result");
            } else {
                fail("Unexpected success");
            }
        } catch (SaxonApiException e) {
            if (expectSuccess) {
                e.printStackTrace();
                fail();
            }
        }
    }

    @Test
    public void testImportingWithoutLicense() {
        try {
            Processor processor1 = new Processor(true);
            XsltCompiler compiler = processor1.newXsltCompiler();
            compiler.setJustInTimeCompilation(false);
            XsltExecutable top = compiler.compile(
                    new StreamSource(new File(configTest.getDataDir() + "bills.xsl")));
            ByteArrayOutputStream baos = new ByteArrayOutputStream();
            top.export(baos);
            Processor processor2 = new Processor(false);
            processor2.getUnderlyingConfiguration().disableLicensing();
            XsltCompiler compiler2 = processor2.newXsltCompiler();
            compiler2.setJustInTimeCompilation(false);
            compiler2.compile(new StreamSource(new ByteArrayInputStream(baos.toByteArray())));
            Xslt30Transformer trans = top.load30();
            XdmValue result = trans.applyTemplates(new StreamSource(new File(configTest.getDataDir() + "bills.xml")));
            System.err.println(result.toString());
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    @Tag("bytecode")
    public void testImportingWithoutLicenseOptimizedCode() {
        try {
            Processor processor1 = new Processor(true);
            XsltCompiler compiler = processor1.newXsltCompiler();
            compiler.setTargetEdition("HE");
            // Choose a stylesheet that Saxon-EE will optimize
            XsltExecutable top = compiler.compile(
                    new StreamSource(new File(configTest.getXMarkDir() + "q8.xsl")));
            ByteArrayOutputStream baos = new ByteArrayOutputStream();
            top.export(baos);
            Processor processor2 = new Processor(false);
            processor2.getUnderlyingConfiguration().disableLicensing();
            XsltCompiler compiler2 = processor2.newXsltCompiler();
            compiler2.compile(new StreamSource(new ByteArrayInputStream(baos.toByteArray())));
            Xslt30Transformer trans = top.load30();
            XdmValue result = trans.applyTemplates(new StreamSource(new File(configTest.getXMarkDir() + "xmark1.xml")));
            System.err.println(result.toString());
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testExportToHEworking() {
        // Export now working in 10.0 because HE does support higher-order-functions
        try {
            Processor processor = new Processor(true);
            XsltCompiler compiler = processor.newXsltCompiler();
            compiler.setTargetEdition("JS");
            String xsl = "<xsl:package\n" +
                    "    name = \"package-002.xsl\"\n" +
                    "    package-version = \"2.1.0.5\"\n" +
                    "    version = \"3.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">\n" +
                    "        <xsl:param name='c' select='2'/>" +
                    "        <xsl:template name='go'>\n" +
                    "          <xsl:param name='p1'/>\n" +
                    "          <out><xsl:value-of select='sort(1 to 10, (), function($x){-$x})'/></out>\n" +
                    "        </xsl:template>\n" +
                    "        <xsl:template name='main'>\n" +
                    "          <xsl:call-template name='go'>\n" +
                    "          <xsl:with-param name='p1' select='22'/>\n" +
                    "          </xsl:call-template>\n" +
                    "        </xsl:template>\n" +
                    "</xsl:package>";
            XsltPackage pack = compiler.compilePackage(new StreamSource(new StringReader(xsl)));
            File file = new File(configTest.getDataDir() + "sandpit/" + "package01.xsltpack");
            pack.save(file);
        } catch (SaxonApiException e) {
            fail("Unexpected failure");
        }
    }


    @Test
    public void testExportToHEfailing() {
        // Export fails because HE does not support streaming
        try {
            Processor processor = new Processor(true);
            XsltCompiler compiler = processor.newXsltCompiler();
            compiler.setTargetEdition("JS");
            String xsl = "<xsl:package\n" +
                    "    name = \"package-002.xsl\"\n" +
                    "    package-version = \"2.1.0.5\"\n" +
                    "    version = \"3.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">\n" +
                    "        <xsl:param name='c' select='2'/>" +
                    "        <xsl:mode streamable='yes'/>\n" +
                    "        <xsl:template match='/'>\n" +
                    "          <xsl:param name='p1'/>\n" +
                    "          <out><xsl:apply-templates/></out>\n" +
                    "        </xsl:template>\n" +
                    "        <xsl:template name='main'>\n" +
                    "          <xsl:call-template name='go'>\n" +
                    "          <xsl:with-param name='p1' select='22'/>\n" +
                    "          </xsl:call-template>\n" +
                    "        </xsl:template>\n" +
                    "</xsl:package>";
            XsltPackage pack = compiler.compilePackage(new StreamSource(new StringReader(xsl)));
            File file = new File(configTest.getDataDir() + "sandpit/" + "package01.xsltpack");
            pack.save(file);
            fail("Unexpected success");
        } catch (SaxonApiException e) {
            // OK
        }
    }


    @Test
    public void testBug4117() {

        // Bugs 4035 and 4117 - exporting a package containing node-valued static variables

        String p =
         "<xsl:package name='http://www.ctk.cz/keywords/' " +

        "  xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:xs='http://www.w3.org/2001/XMLSchema' version='3.0' " +
        "  xmlns:kw='http://www.ctk.cz/keywords/'>" +
        "  <xsl:variable name='keywords' as='document-node()' static='yes'"
                 + " select=\"parse-xml('&lt;Keywords&gt;&lt;/Keywords&gt;')\"/>" +
        "  <xsl:function name='kw:get-keyword-value' as='xs:string?' visibility='final'>" +
        "     <xsl:param name = 'qcode' as= 'xs:string'/>" +
        "     <xsl:sequence select='$keywords/Keywords//Keyword[@qcode eq $qcode]/Value'/>" +
        "  </xsl:function>" +
        "</xsl:package>";


        String s =
        "<xsl:transform version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:xs='http://www.w3.org/2001/XMLSchema' xmlns:kw='http://www.ctk.cz/keywords/'> " +
        "  <xsl:use-package name='http://www.ctk.cz/keywords/'/>" +
        "  <xsl:template name='xsl:initial-template'> " +
        "      <out x='{kw:get-keyword-value(\"abc\")}'/>" +
        "  </xsl:template>" +
        "</xsl:transform>";

        try {
            Processor processor = new Processor(true);
            //processor.setConfigurationProperty(Feature.OPTIMIZATION_LEVEL, "-j");
            XsltCompiler xsltCompiler = processor.newXsltCompiler();
            XsltPackage pp = xsltCompiler.compilePackage(new StreamSource(new StringReader(p)));
            xsltCompiler.importPackage(pp);
            XsltExecutable exec = xsltCompiler.compile(new StreamSource(new StringReader(s)));
            Xslt30Transformer transf = exec.load30();
            Serializer dest = processor.newSerializer(System.out);
            transf.callTemplate(null, dest);
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }



    }

    @Test
    public void testDualUse() {  // Bug 4153

        // The idea here is to show that a compiled package can be used both as a top-level package
        // and as a library package

        String base =
                "<xsl:package name='http://www.ctk.cz/keywords/' " +
                        "  xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:xs='http://www.w3.org/2001/XMLSchema' version='3.0' >" +
                        "  <xsl:variable name='top' as='xs:string' visibility='public'><xsl:try select='./x'><xsl:catch errors='*'>caught</xsl:catch></xsl:try></xsl:variable>" +
                        "  <xsl:mode streamable='yes' visibility='public' name='m'/>" +
                        "  <xsl:template name='go' visibility='final'>" +
                        "     <xsl:apply-templates select='.' mode='m'/>" +
                        "  </xsl:template>" +
                        "  <xsl:template mode='m' match='x'><foundX val='{string(.)}' top='{$top}'/></xsl:template>" +
                        "</xsl:package>";


        String overlay =

                "<xsl:transform version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:xs='http://www.w3.org/2001/XMLSchema'> " +
                        "  <xsl:use-package name='http://www.ctk.cz/keywords/'>" +
                        "    <xsl:override><xsl:template match='x' mode='m'><foundOverride top='{$top}'/></xsl:template></xsl:override>" +
                        "  </xsl:use-package>" +
                        "  <xsl:template match='/'> " +
                        "      <xsl:apply-templates mode='m'/>" +
                        "  </xsl:template>" +
                        "</xsl:transform>";

        String doc = "<x>42</x>";

        try {
            Processor processor = new Processor(true);
            processor.setConfigurationProperty(Feature.OPTIMIZATION_LEVEL, "-j");
            DocumentBuilder builder = processor.newDocumentBuilder();
            XdmNode root = builder.build(new StreamSource(new StringReader(doc)));
            XsltCompiler xsltCompiler = processor.newXsltCompiler();
            StringReader ms = new StringReader(base);
            XsltPackage pp = xsltCompiler.compilePackage(new StreamSource(ms));
            xsltCompiler.importPackage(pp);
            XsltExecutable exec = xsltCompiler.compile(new StreamSource(new StringReader(overlay)));

            // Run overlay stylesheet
            Xslt30Transformer transf = exec.load30();
            XdmDestination dest = new XdmDestination();
            transf.applyTemplates(root, dest);
            System.err.println(dest.getXdmNode());
            XdmNode rootElement = dest.getXdmNode().select(child().first()).asNode();
            assertEquals("foundOverride", rootElement.getNodeName().getLocalName());
            assertEquals("caught", rootElement.attribute("top"));

            // Run base stylesheet
            exec = pp.link();
            transf = exec.load30();
            dest = new XdmDestination();
            transf.setGlobalContextItem(root);
            transf.callTemplate(new QName("go"), dest);
            System.err.println(dest.getXdmNode());
            rootElement = dest.getXdmNode().select(child().first()).asNode();
            assertEquals("foundX", rootElement.getNodeName().getLocalName());
            assertEquals("42", rootElement.attribute("top"));
            assertEquals("42", rootElement.attribute("val"));
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testBug4727a() {
        String p = "<xsl:package xmlns:xsl='http://www.w3.org/1999/XSL/Transform'\n" +
                "             xmlns:xs='http://www.w3.org/2001/XMLSchema'\n" +
                "             xmlns:t='http://example.com/templates'\n" +
                "             package-version='1.0.0'\n" +
                "             exclude-result-prefixes='#all'\n" +
                "             version='3.0'>\n" +
                "<xsl:template name='t:funcsynopsis' visibility='public'>\n" +
                "  <irrelevant/>\n" +
                "</xsl:template>\n" +
                "</xsl:package>\n";

        String s = "<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'\n" +
                "                xmlns:xs='http://www.w3.org/2001/XMLSchema'\n" +
                "                xmlns:t='http://example.com/templates'\n" +
                "                exclude-result-prefixes='#all'\n" +
                "                version='3.0'>\n" +
                "<xsl:output omit-xml-declaration='yes'/>\n" +
                "<xsl:use-package name='http://example.com/funcsynopsis'\n" +
                "                 package-version=\"'1.0.0'\"/>\n" +
                "<xsl:template match='funcsynopsis'>\n" +
                "  <xsl:call-template name='t:funcsynopsis'/>\n" +
                "</xsl:template>\n" +
                "</xsl:stylesheet>\n";

        try {
            Processor processor = new Processor(true);
            XsltCompiler xsltCompiler = processor.newXsltCompiler();
            XsltPackage pp = xsltCompiler.compilePackage(new StreamSource(new StringReader(p)));
            xsltCompiler.importPackage(pp);
            XsltExecutable exec = xsltCompiler.compile(new StreamSource(new StringReader(s)));
            fail("Package compile should not have succeeded");
        } catch (SaxonApiException e) {
            // that's expected
        } catch (Exception e) {
            e.printStackTrace();
            fail("Unexpected exception attempting to use unavailable package");
        }
    }

    @Test
    public void testBug4727b() {
        String p = "<xsl:package xmlns:xsl='http://www.w3.org/1999/XSL/Transform'\n" +
                "             xmlns:xs='http://www.w3.org/2001/XMLSchema'\n" +
                "             xmlns:t='http://example.com/templates'\n" +
                "             package-version='1.0.0'\n" +
                "             exclude-result-prefixes='#all'\n" +
                "             version='3.0'>\n" +
                "<xsl:template name='t:funcsynopsis' visibility='public'>\n" +
                "  <irrelevant/>\n" +
                "</xsl:template>\n" +
                "</xsl:package>\n";

        String s = "<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'\n" +
                "                xmlns:xs='http://www.w3.org/2001/XMLSchema'\n" +
                "                xmlns:t='http://example.com/templates'\n" +
                "                exclude-result-prefixes='#all'\n" +
                "                version='3.0'>\n" +
                "<xsl:output omit-xml-declaration='yes'/>\n" +
                "<xsl:use-package name='http://example.com/funcsynopsis'\n" +
                "                 package-version=\"2.0.0\"/>\n" +
                "<xsl:template match='funcsynopsis'>\n" +
                "  <xsl:call-template name='t:funcsynopsis'/>\n" +
                "</xsl:template>\n" +
                "</xsl:stylesheet>\n";

        try {
            Processor processor = new Processor(true);
            XsltCompiler xsltCompiler = processor.newXsltCompiler();
            XsltPackage pp = xsltCompiler.compilePackage(new StreamSource(new StringReader(p)));
            xsltCompiler.importPackage(pp);
            XsltExecutable exec = xsltCompiler.compile(new StreamSource(new StringReader(s)));
            fail("Package compile should not have succeeded");
        } catch (SaxonApiException e) {
            // that's expected
        } catch (Exception e) {
            e.printStackTrace();
            fail("Unexpected exception attempting to use unavailable package");
        }
    }

    @Test
    public void testBug6186() {
        // The essence of this bug was the dependency graph (TOP->WIF, TOP->IR, IR->WIF), where TOP overrides
        // components in both WIF and IR, including overriding a mode in WIF that is not directly referenced
        // in IR, but exists implicitly in IR as a hidden component.
        String pack_wif = "<xsl:package name=\"my:pack-wif\" version=\"3.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">\n"
                + "   <xsl:variable name=\"data\" visibility=\"public\">\n"
                + "      <data/>\n"
                + "   </xsl:variable>\n"
                + "   <xsl:param name=\"size\" select='42'/>\n"
                + "   <xsl:mode name=\"m\" on-no-match=\"fail\" visibility='public'/>\n"
                + "   <xsl:template match=\"data\" mode=\"m\">\n"
                + "      <p1data size='{$size}'/>\n"
                + "   </xsl:template>\n"
                + "</xsl:package>";
        String pack_ir = "<xsl:package name=\"my:pack-ir\" version=\"3.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">\n"
                + "   <xsl:use-package name=\"my:pack-wif\">\n"
                + "      <xsl:override>\n"
                + "         <xsl:variable name=\"data\" visibility=\"public\">\n"
                + "            <data>d2</data>\n"
                + "         </xsl:variable>\n"
                + "      </xsl:override>\n"
                + "   </xsl:use-package>\n"
                + "   <xsl:variable name=\"thing\" select=\"'thang'\"/>\n"
                + "</xsl:package>";
        String xsl = "<xsl:stylesheet version=\"3.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">\n"
                + "   <xsl:use-package name=\"my:pack-wif\">\n"
                + "      <xsl:override>\n"
                + "         <xsl:template match=\"data\" mode=\"m\">\n"
                + "            <top><xsl:next-match/></top>\n"
                + "         </xsl:template>\n"
                + "      </xsl:override>\n"
                + "   </xsl:use-package>\n"
                + "   <xsl:use-package name=\"my:pack-ir\">\n"
                + "      <xsl:override>\n"
                + "         <xsl:variable name=\"data\" visibility=\"private\">\n"
                + "            <data>d2</data>\n"
                + "         </xsl:variable>\n"
                + "      </xsl:override>\n"
                + "   </xsl:use-package>\n"
                + "   <xsl:template name=\"xsl:initial-template\" visibility='public'>\n"
                + "      <xsl:apply-templates select=\"$data/data\" mode=\"m\"/>\n"
                + "   </xsl:template>\n"
                + "</xsl:stylesheet>";
        try {
            Processor processor = new Processor(true);
            XsltCompiler xsltCompiler = processor.newXsltCompiler();
            XsltPackage p1 = xsltCompiler.compilePackage(new StreamSource(new StringReader(pack_wif), "pack_wif"));
            xsltCompiler.importPackage(p1);
            XsltPackage p2 = xsltCompiler.compilePackage(new StreamSource(new StringReader(pack_ir), "p2ack_ir"));
            xsltCompiler.importPackage(p2);
            XsltExecutable exec = xsltCompiler.compile(new StreamSource(new StringReader(xsl), "top-xsl"));
            Xslt30Transformer trans = exec.load30();
            StringWriter sw = new StringWriter();
            trans.callTemplate(null, processor.newSerializer(sw));
            assert(sw.toString().contains("<p1data size=\"42\"/>"));
        } catch (SaxonApiException e) {
            fail(e);
        }
    }
}
