package failing;

import jaxptest.ConfigTest;
import net.sf.saxon.Configuration;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.lib.SchemaURIResolver;
import net.sf.saxon.s9api.*;
import org.junit.jupiter.api.Disabled;
import org.junit.jupiter.api.Test;

import javax.xml.transform.Source;
import javax.xml.transform.stream.StreamSource;
import java.io.File;
import java.io.StringReader;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.fail;

/**
 * This module is a place for parking tests that we are currently failing for a known reason. Failures in these
 * tests are known problems and should not inhibit progress.
 */
public class FailingTests {
    private final ConfigTest configTest = new ConfigTest();
    public final String DATA_DIR = configTest.getDataDir();



    @Test
    @Disabled("Known to fail. There are spec problems and implementation problems loading a package multiple times with different static parameter values")
    public void testPackageRenaming() {
        try {
            String configFile = DATA_DIR + "packages/config-120.xml";
            Processor processor = new Processor(new StreamSource(new File(configFile)));
            XsltCompiler compiler = processor.newXsltCompiler();
            XsltExecutable top = compiler.compile(
                    new StreamSource(new File(DATA_DIR + "packages/pack-120-top.xsl")));
            Xslt30Transformer trans = top.load30();
            XdmNode result = (XdmNode) trans.callTemplate(null);
            XPathCompiler xp = processor.newXPathCompiler();
            String sum = xp.evaluateSingle("/out/add", result).getStringValue();
            String difference = xp.evaluateSingle("/out/subtract", result).getStringValue();
            String product = xp.evaluateSingle("/out/multiply", result).getStringValue();
            assertEquals("12", sum);
            assertEquals("8", difference);
            assertEquals("20", product);
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    @Disabled("Known to fail: see bug 4220")
    public void testPackageExportWithParams() {
        // See bug 4220, currently still open (2020-01-23)
        try {
            String configFile = DATA_DIR + "packages/config-120.xml";
            Processor processor = new Processor(new StreamSource(new File(configFile)));
            XsltCompiler compiler = processor.newXsltCompiler();
            XsltExecutable top = compiler.compile(
                    new StreamSource(new File(DATA_DIR + "packages/pack-120-top.xsl")));
            XsltPackage add = compiler.obtainPackage("http://package-add/", "1.0");
            add.save(new File(DATA_DIR + "packages/pack-120-add.xsl"));
            XsltPackage subtract = compiler.obtainPackage("http://package-subtract/", "1.0");
            subtract.save(new File(DATA_DIR + "packages/pack-120-subtract.xsl"));
            XsltPackage multiply = compiler.obtainPackage("http://package-multiply/", "1.0");
            multiply.save(new File(DATA_DIR + "packages/pack-120-multiply.xsl"));

        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    @Disabled("Known to fail: see bug 4220")
    public void testPackageRenamingExported() {
        // This uses the sef packages created by test testPackageExportWithParams
        // See bug 4220, currently still open (2020-01-23)
        try {
            String configFile = DATA_DIR + "packages/config-121.xml";
            Processor processor = new Processor(new StreamSource(new File(configFile)));
            XsltCompiler compiler = processor.newXsltCompiler();
            XsltExecutable top = compiler.compile(
                    new StreamSource(new File(DATA_DIR + "packages/pack-120-top.xsl")));
            Xslt30Transformer trans = top.load30();
            XdmNode result = (XdmNode) trans.callTemplate(null);
            XPathCompiler xp = processor.newXPathCompiler();
            String sum = xp.evaluateSingle("/out/add", result).getStringValue();
            String difference = xp.evaluateSingle("/out/subtract", result).getStringValue();
            String product = xp.evaluateSingle("/out/multiply", result).getStringValue();
            assertEquals("12", sum);
            assertEquals("8", difference);
            assertEquals("20", product);
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }


}

