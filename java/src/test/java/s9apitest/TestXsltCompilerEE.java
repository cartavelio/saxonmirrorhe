////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2013-2026 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is “Incompatible With Secondary Licenses”, as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package s9apitest;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.config.ProfessionalConfiguration;
import net.sf.saxon.Configuration;
import net.sf.saxon.junit.Testdata;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.s9api.*;
import org.junit.jupiter.api.Test;

import javax.xml.transform.stream.StreamSource;
import java.io.*;

import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;


/**
 * JUnit tests for the s9api XsltCompiler
 */
public class TestXsltCompilerEE {
    @Test
    public void testJitEEOn() {
        testJit(new EnterpriseConfiguration(), "10", false);
    }

    @Test
    public void testJitEEOnJ() {
        testJit(new EnterpriseConfiguration(), "j", false);
    }

    @Test
    public void testJitEEOff() {
        testJit(new EnterpriseConfiguration(), "0", true);
    }

    @Test
    public void testJitEEOffJ() {
        testJit(new EnterpriseConfiguration(), "-j", true);
    }

    @Test
    public void testJitPEOn() {
        testJit(new ProfessionalConfiguration(), "10", true);
    }

    @Test
    public void testJitPEOff() {
        testJit(new ProfessionalConfiguration(), "0", true);
    }

    @Test
    public void testJitHEOn() {
        testJit(new Configuration(), "10", true);
    }

    @Test
    public void testJitHEOff() {
        testJit(new Configuration(), "0", true);
    }

    private void testJit(Configuration config, String opt, boolean failEarly) {
        String xsl =
                "<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>\n" +
                        "<xsl:template match='/'>\n" +
                        "  <xsl:apply-templates/>\n" +
                        "</xsl:template>\n" +
                        "<xsl:template match='doc'>\n" +
                        "  <xsl:apply-templates select='$x'/>\n" +    // deliberate syntax error
                        "</xsl:template>\n" +
                        "</xsl:stylesheet>";
        String xml = "<doc/>";
        Processor p = new Processor(config);
        p.setConfigurationProperty(Feature.OPTIMIZATION_LEVEL, opt);
        XsltCompiler c = p.newXsltCompiler();
        XsltExecutable q = null;
        try {
            q = c.compile(new StreamSource(new StringReader(xsl)));
            if (failEarly) {
                fail("Compilation should fail");
            }
        } catch (Exception err) {
            if (!failEarly) {
                fail("Compilation should not fail");
                err.printStackTrace();
            }
            return;
        }
        try {
            Xslt30Transformer t = q.load30();
            t.applyTemplates(new StreamSource(new StringReader(xml)));
            fail("Execution should not succeed");
        } catch (SaxonApiException err) {
            // OK
        }
    }

    @Test
    public void testBug4969() {
        // The essence of this bug is that a SEF file generated for a schema-aware stylesheet
        // contained a reference to an anonymous type, which could not be found when the stylesheet
        // was reloaded from the SEF file
        try {
            Processor proc = new Processor(true);
            XsltCompiler comp = proc.newXsltCompiler();
            comp.setSchemaAware(true);
            String xsl1 = "<xsl:stylesheet exclude-result-prefixes=\"#all\" version=\"3.0\"\n"
                    + "  xmlns:test=\"http://www.example.com/test\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\"\n"
                    + "  xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">\n"
                    + "<xsl:template name=\"xsl:initial-template\">\n"
                    + "  <xsl:variable as=\"element(test:e)\" name=\"e\">\n"
                    + "    <test:e />\n"
                    + "  </xsl:variable>\n"
                    + "  <xsl:variable as=\"element(test:e)\" name=\"ve\">\n"
                    + "    <xsl:copy-of select=\"$e\" validation=\"strict\" />\n"
                    + "  </xsl:variable>\n"
                    + "  <xsl:sequence select=\"$ve, $ve\" />\n"
                    + "</xsl:template>"
                    + "</xsl:stylesheet>";

            String xsd = "<xs:schema elementFormDefault=\"qualified\" targetNamespace=\"http://www.example.com/test\"\n"
                    + "  xmlns=\"http://www.example.com/test\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">\n"
                    + "  <xs:element name=\"e\">\n"
                    + "    <xs:complexType />\n"
                    + "  </xs:element>\n"
                    + "</xs:schema>";
            SchemaManager sm = proc.getSchemaManager();
            sm.load(new StreamSource(new StringReader(xsd)));
            StringWriter scm = new StringWriter();
            Serializer scmOut = proc.newSerializer(scm);
            sm.exportComponents(scmOut);

            comp.useSchema(sm.getXsdSchema());
            XsltPackage pack = comp.compilePackage(new StreamSource(new StringReader(xsl1)));
            File sefFile = Testdata.tempFile("6969test.sef");
            pack.save(sefFile);

            Processor proc2 = new Processor(true);
            SchemaManager sm2 = proc2.getSchemaManager();
            sm2.importComponents(new StreamSource(new StringReader(scm.toString())));
            XsltCompiler comp2 = proc2.newXsltCompiler();
            comp2.setSchemaAware(true);
            XsltExecutable exec = comp2.loadExecutablePackage(sefFile.toURI());
            Xslt30Transformer trans = exec.load30();
            StringWriter out = new StringWriter();
            trans.callTemplate(null, proc2.newSerializer(out));
            assertTrue(out.toString().contains("<test:e xmlns:test=\"http://www.example.com/test\"/>"));
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }

    @Test
    public void testReloadFromSource() {
        // Variant of testBug4969 that reloads the SEF file from a string
        // Changes in Saxon 13 because schemas are now included in compiled SEF files
        try {
            Processor proc = new Processor(true);
            XsltCompiler comp = proc.newXsltCompiler();
            comp.setSchemaAware(true);
            comp.setJustInTimeCompilation(false);
            String xsl1 = "<xsl:stylesheet exclude-result-prefixes=\"#all\" version=\"3.0\"\n"
                    + "  xmlns:test=\"http://www.example.com/test\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\"\n"
                    + "  xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">\n"
                    + "<xsl:template name=\"xsl:initial-template\">\n"
                    + "  <xsl:variable as=\"element(test:e)\" name=\"e\">\n"
                    + "    <test:e />\n"
                    + "  </xsl:variable>\n"
                    + "  <xsl:variable as=\"element(test:e)\" name=\"ve\">\n"
                    + "    <xsl:copy-of select=\"$e\" validation=\"strict\" />\n"
                    + "  </xsl:variable>\n"
                    + "  <xsl:sequence select=\"$ve, $ve\" />\n"
                    + "</xsl:template>"
                    + "</xsl:stylesheet>";

            String xsd = "<xs:schema elementFormDefault=\"qualified\" targetNamespace=\"http://www.example.com/test\"\n"
                    + "  xmlns=\"http://www.example.com/test\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">\n"
                    + "  <xs:element name=\"e\">\n"
                    + "    <xs:complexType />\n"
                    + "  </xs:element>\n"
                    + "</xs:schema>";
            XsdCompiler sm = proc.newXsdCompiler();
            XsdSchema schema = sm.compile(new StreamSource(new StringReader(xsd)));
            StringWriter scm = new StringWriter();
            Serializer scmOut = proc.newSerializer(scm);
            schema.exportComponents(scmOut);

            comp.useSchema(schema);
            XsltExecutable pack = comp.compile(new StreamSource(new StringReader(xsl1)));
            ByteArrayOutputStream baos = new ByteArrayOutputStream();
            pack.export(baos);

            Processor proc2 = new Processor(true);
//            SchemaManager sm2 = proc2.getSchemaManager();
//            sm2.importComponents(new StreamSource(new StringReader(scm.toString())));
            XsltCompiler comp2 = proc2.newXsltCompiler();
            comp2.setSchemaAware(true);
            XsltExecutable exec = comp2.loadExecutablePackage(
                    new StreamSource(new ByteArrayInputStream(baos.toByteArray())));
            Xslt30Transformer trans = exec.load30();
            StringWriter out = new StringWriter();
            trans.callTemplate(null, proc2.newSerializer(out));
            assertTrue(out.toString().contains("<test:e xmlns:test=\"http://www.example.com/test\"/>"));
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }
}
