package s9apitest;

import jaxptest.ConfigTest;
import net.sf.saxon.s9api.*;
import org.junit.Assert;
import org.junit.Test;
import org.xml.sax.InputSource;

import javax.xml.transform.Source;
import javax.xml.transform.sax.SAXSource;
import javax.xml.transform.stream.StreamSource;
import java.io.FileOutputStream;
import java.io.StringReader;
import java.util.HashMap;
import java.util.Map;

import static org.junit.jupiter.api.Assertions.fail;

public class TestChecksums {
    private static final String patchPI =
            "<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'\n" +
                    "                xmlns:xs='http://www.w3.org/2001/XMLSchema'\n" +
                    "                exclude-result-prefixes='xs'\n" +
                    "                version='3.0'>\n" +
                    "<xsl:output method='xml' encoding='utf-8' indent='no'/>\n" +
                    "<xsl:param name='target' select='()'/>\n" +
                    "<xsl:param name='value' select='()'/>\n" +
                    "<xsl:mode on-no-match='shallow-copy'/>\n" +
                    "<xsl:template match='/' name='xsl:initial-template'>\n" +
                    "  <xsl:apply-templates/>\n" +
                    "</xsl:template>\n" +
                    "<xsl:template match='processing-instruction()'>\n" +
                    "  <xsl:choose>\n" +
                    "    <xsl:when test='$target = name()'>\n" +
                    "      <xsl:if test='exists($value)'>\n" +
                    "        <xsl:processing-instruction name='{$target}' select='$value'/>\n" +
                    "      </xsl:if>\n" +
                    "    </xsl:when>\n" +
                    "    <xsl:otherwise>\n" +
                    "      <xsl:copy/>\n" +
                    "    </xsl:otherwise>\n" +
                    "  </xsl:choose>\n" +
                    "</xsl:template>\n" +
                    "</xsl:stylesheet>";

    private static final String readPI =
            "<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'\n" +
                    "                xmlns:xs='http://www.w3.org/2001/XMLSchema'\n" +
                    "                exclude-result-prefixes='xs'\n" +
                    "                version='3.0'>\n" +
                    "<xsl:output method='text' encoding='utf-8'/>\n" +
                    "<xsl:param name='target' select='()'/>\n" +
                    "<xsl:template match='/' name='xsl:initial-template'>\n" +
                    "  <xsl:apply-templates/>\n" +
                    "</xsl:template>\n" +
                    "<xsl:template match='*'>\n" +
                    "  <xsl:apply-templates/>\n" +
                    "</xsl:template>\n" +
                    "<xsl:template match='text()'/>\n" +
                    "<xsl:template match='processing-instruction()'>\n" +
                    "  <xsl:if test='$target = name()'>\n" +
                    "    <xsl:sequence select='string(.)'/>\n" +
                    "  </xsl:if>\n" +
                    "</xsl:template>\n" +
                    "</xsl:stylesheet>";

    @Test
    public void testExportChecksum() {
        try {
            ConfigTest ctest = new ConfigTest();
            Processor proc = new Processor(true);

            // Compile the stylesheet into an SEF
            XsltCompiler comp = proc.newXsltCompiler();
            comp.setJustInTimeCompilation(false);
            StringReader sr = new StringReader(patchPI);
            XsltExecutable exec = comp.compile(new StreamSource(sr));
            String exportFn = ctest.getDataDir() + "sandpit/export.sef";
            exec.export(new FileOutputStream(exportFn), "HE");

            // N.B. It would be nice to check if the checksum and hash are correct,
            // but the values contain hashed timestamps, so there's no way to precompute
            // what the correct values are...

            // Transform the SEF, breaking its Σ checksum
            comp = proc.newXsltCompiler();
            sr = new StringReader(patchPI);
            exec = comp.compile(new StreamSource(sr));

            XdmDestination destination = new XdmDestination();

            Source source = new SAXSource(new InputSource(exportFn));
            Xslt30Transformer transformer = exec.load30();

            Map<QName, XdmValue> params = new HashMap<>();
            params.put(new QName("target"), new XdmAtomicValue("Σ"));
            params.put(new QName("value"), new XdmAtomicValue("deadbeef"));

            transformer.setStylesheetParameters(params);
            transformer.transform(source, destination);

            XdmNode badChecksum = destination.getXdmNode();

            try {
                comp = proc.newXsltCompiler();
                exec = comp.compile(badChecksum.asSource());
                fail();
            } catch (SaxonApiException ex) {
                Assert.assertTrue(ex.getMessage().contains("incorrect checksum"));
            }
        } catch (Exception e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }

    @Test
    public void testExportDigest() {
        try {
            ConfigTest ctest = new ConfigTest();
            Processor proc = new Processor(true);

            // Compile the stylesheet into an SEF
            XsltCompiler comp = proc.newXsltCompiler();
            comp.setJustInTimeCompilation(false);
            StringReader sr = new StringReader(patchPI);
            XsltExecutable exec = comp.compile(new StreamSource(sr));
            String exportFn = ctest.getDataDir() + "sandpit/export.sef";
            exec.export(new FileOutputStream(exportFn), "HE");

            // N.B. It would be nice to check if the checksum and hash are correct,
            // but the values contain hashed timestamps, so there's no way to precompute
            // what the correct values are...

            // Transform the SEF, breaking its Σ2 checksum
            comp = proc.newXsltCompiler();
            sr = new StringReader(patchPI);
            exec = comp.compile(new StreamSource(sr));

            XdmDestination destination = new XdmDestination();

            Source source = new SAXSource(new InputSource(exportFn));
            Xslt30Transformer transformer = exec.load30();

            Map<QName, XdmValue> params = new HashMap<>();
            params.put(new QName("target"), new XdmAtomicValue("Σ2"));
            params.put(new QName("value"), new XdmAtomicValue("deadbeef"));

            transformer.setStylesheetParameters(params);
            transformer.transform(source, destination);

            XdmNode badChecksum = destination.getXdmNode();

            try {
                comp = proc.newXsltCompiler();
                exec = comp.compile(badChecksum.asSource());
                fail();
            } catch (SaxonApiException ex) {
                Assert.assertTrue(ex.getMessage().contains("incorrect checksum"));
            }
        } catch (Exception e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }

    @Test
    public void testExportDigestRequired() {
        try {
            ConfigTest ctest = new ConfigTest();
            Processor proc = new Processor(true);

            // Compile the stylesheet into an SEF
            XsltCompiler comp = proc.newXsltCompiler();
            comp.setJustInTimeCompilation(false);
            StringReader sr = new StringReader(patchPI);
            XsltExecutable exec = comp.compile(new StreamSource(sr));
            String exportFn = ctest.getDataDir() + "sandpit/export.sef";
            exec.export(new FileOutputStream(exportFn), "HE");

            // N.B. It would be nice to check if the checksum and hash are correct,
            // but the values contain hashed timestamps, so there's no way to precompute
            // what the correct values are...

            // Remove the Σ2 checksum, breaking the SEF
            comp = proc.newXsltCompiler();
            sr = new StringReader(patchPI);
            exec = comp.compile(new StreamSource(sr));

            XdmDestination destination = new XdmDestination();

            Source source = new SAXSource(new InputSource(exportFn));
            Xslt30Transformer transformer = exec.load30();

            Map<QName, XdmValue> params = new HashMap<>();
            params.put(new QName("target"), new XdmAtomicValue("Σ2"));

            transformer.setStylesheetParameters(params);
            transformer.transform(source, destination);

            XdmNode removedDigest = destination.getXdmNode();

            try {
                comp = proc.newXsltCompiler();
                exec = comp.compile(removedDigest.asSource());
                fail();
            } catch (SaxonApiException ex) {
                Assert.assertTrue(ex.getMessage().contains("incorrect checksum"));
            }
        } catch (Exception e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }

    @Test
    public void testExportDigestReplace() {
        try {
            ConfigTest ctest = new ConfigTest();
            Processor proc = new Processor(true);

            // Compile the stylesheet into an SEF
            XsltCompiler comp = proc.newXsltCompiler();
            comp.setJustInTimeCompilation(false);
            StringReader sr = new StringReader(patchPI);
            XsltExecutable exec = comp.compile(new StreamSource(sr));
            String exportFn = ctest.getDataDir() + "sandpit/export.sef";
            exec.export(new FileOutputStream(exportFn), "HE");

            // N.B. It would be nice to check if the checksum and hash are correct,
            // but the values contain hashed timestamps, so there's no way to precompute
            // what the correct values are...

            // Read the Σ2 checksum...
            comp = proc.newXsltCompiler();
            sr = new StringReader(readPI);
            exec = comp.compile(new StreamSource(sr));
            XdmDestination destination = new XdmDestination();
            Source source = new SAXSource(new InputSource(exportFn));
            Xslt30Transformer transformer = exec.load30();
            Map<QName, XdmValue> params = new HashMap<>();
            params.put(new QName("target"), new XdmAtomicValue("Σ2"));
            transformer.setStylesheetParameters(params);
            transformer.transform(source, destination);
            XdmNode E2 = destination.getXdmNode();
            String digest = E2.getStringValue();

            // Remove the Σ2 checksum...
            comp = proc.newXsltCompiler();
            sr = new StringReader(patchPI);
            exec = comp.compile(new StreamSource(sr));
            destination = new XdmDestination();
            source = new SAXSource(new InputSource(exportFn));
            transformer = exec.load30();
            params = new HashMap<>();
            params.put(new QName("target"), new XdmAtomicValue("Σ2"));
            transformer.setStylesheetParameters(params);
            transformer.transform(source, destination);
            XdmNode singleSum = destination.getXdmNode();

            // Replace the Σ checksum...
            comp = proc.newXsltCompiler();
            sr = new StringReader(patchPI);
            exec = comp.compile(new StreamSource(sr));
            destination = new XdmDestination();
            transformer = exec.load30();
            params = new HashMap<>();
            params.put(new QName("target"), new XdmAtomicValue("Σ"));
            params.put(new QName("value"), new XdmAtomicValue(digest));
            transformer.setStylesheetParameters(params);
            transformer.transform(singleSum.asSource(), destination);
            XdmNode patchedSum = destination.getXdmNode();

            try {
                comp = proc.newXsltCompiler();
                exec = comp.compile(patchedSum.asSource());
            } catch (SaxonApiException ex) {
                fail();
            }
        } catch (Exception e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }

}
