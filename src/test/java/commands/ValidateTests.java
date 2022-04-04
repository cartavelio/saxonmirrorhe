package commands;

import com.saxonica.Validate;
import jaxptest.ConfigTest;
import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.XPathCompiler;
import net.sf.saxon.s9api.XdmAtomicValue;
import net.sf.saxon.s9api.XdmItem;
import org.junit.jupiter.api.Test;

import java.io.File;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;

/**
 * JUnit tests on Validate Command Line options
 */
public class ValidateTests {
    private final ConfigTest configTest = new ConfigTest();

    private String TEMPDIR = configTest.getDataDir() + "temp/";

    public void setUp() {
        System.err.println("TEMP:" + TEMPDIR);
    }

    @Test
    public void testSchemaValidation() {

        String[] args = {
                "-xsd:" + configTest.getDataDir() + "books.xsd",
                "-t"};
        try {
            int rc = new Validate().doValidate(args);
            assertTrue(rc==0);
        } catch (Exception e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testBasicInstanceValidation() {

        String[] args = {
                "-xsd:" + configTest.getDataDir() + "books.xsd",
                "-s:" + configTest.getDataDir() + "books.xml",
                "-t"};
        try {
            int rc = new Validate().doValidate(args);
            assertTrue(rc==0);
        } catch (Exception e) {
            e.printStackTrace();
            fail();
        }
    }

    private void testExportSCM() {

        String[] args = {
                "-xsd:" + configTest.getDataDir() + "books.xsd",
                "-scmout:" + TEMPDIR + "books.scm",
                "-t"};
        try {
            int rc = new Validate().doValidate(args);
            assertTrue(rc==0);
        } catch (Exception e) {
            e.printStackTrace();
            fail();
        }
    }


    private void testImportSCM() {

        String[] args = {
                "-scmin:" + TEMPDIR + "books.scm",
                "-s:" + configTest.getDataDir() + "books.xml",
                "-t"};
        try {
            int rc = new Validate().doValidate(args);
            assertTrue(rc==0);
        } catch (Exception e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void exportThenImport() {
        testExportSCM();
        testImportSCM();
    }

    @Test
    public void testValidationWithParams() {
        String[] args = {
                "-xsd:" + configTest.getDataDir() + "schema-with-param.xsd",
                "-s:" + configTest.getDataDir() + "birthdays.xml",
                "-xsdversion:1.1",
                "-t",
                "baseLine=2012-01-01",
                "dayOfWeek=Monday"};
        try {
            int rc = new Validate().doValidate(args);
            assertTrue(rc==0);
        } catch (Exception e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testValidationFailureWithParams() {
        String[] args = {
                "-xsd:" + configTest.getDataDir() + "schema-with-param.xsd",
                "-s:" + configTest.getDataDir() + "birthdays.xml",
                "-xsdversion:1.1",
                "-t",
                "-quit:off",
                "baseLine=1980-01-01"};
        try {
            int rc = new Validate().doValidate(args);
            assertTrue(rc==1);
        } catch (Exception e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testTypeAlternativeWithParams() {
        String[] args = {
                "-xsd:" + configTest.getDataDir() + "schema-with-param.xsd",
                "-s:" + configTest.getDataDir() + "stayAtHome.xml",
                "-xsdversion:1.1",
                "-t",
                "dayOfWeek=Sunday"};
        try {
            int rc = new Validate().doValidate(args);
            assertTrue(rc==0);
        } catch (Exception e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testTypeAlternativeWithParams2() {
        String[] args = {
                "-xsd:" + configTest.getDataDir() + "schema-with-param.xsd",
                "-s:" + configTest.getDataDir() + "goToWork.xml",
                "-xsdversion:1.1",
                "-t",
                "dayOfWeek=Monday"};
        try {
            int rc = new Validate().doValidate(args);
            assertTrue(rc==0);
        } catch (Exception e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testTypeAlternativeWithParamsFailure() {
        String[] args = {
                "-xsd:" + configTest.getDataDir() + "schema-with-param.xsd",
                "-s:" + configTest.getDataDir() + "goToWork.xml",
                "-xsdversion:1.1",
                "-t",
                "dayOfWeek=Sunday"};
        try {
            int rc = new Validate().doValidate(args);
            assertTrue(rc==1);
        } catch (Exception e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testStatsReporting() {

        new File(TEMPDIR + "validation-report.xml").delete();

        String[] args = {
                "-xsd:" + configTest.getDataDir() + "books.xsd",
                "-s:" + configTest.getDataDir() + "books.xml",
                "-stats:" + TEMPDIR + "validation-report.xml",
                "-t"};
        try {
            int rc = new Validate().doValidate(args);
            assertEquals(0, rc);
            Processor proc = new Processor(false);
            XdmItem report = proc.newDocumentBuilder().build(
                    new File(TEMPDIR + "validation-report.xml"));
            XPathCompiler compiler = proc.newXPathCompiler();
            XdmAtomicValue result = (XdmAtomicValue) compiler.evaluateSingle(
                    "count(/schemaCoverage/component)", report);
            assertTrue(result.getLongValue() > 20);
        } catch (Exception e) {
            e.printStackTrace();
            fail();
        }
    }
}

