package commands;

import com.saxonica.Validate;
import jaxptest.ConfigTest;
import junit.framework.TestCase;
import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.XPathCompiler;
import net.sf.saxon.s9api.XdmAtomicValue;
import net.sf.saxon.s9api.XdmItem;

import java.io.File;

/**
 * JUnit tests on Validate Command Line options
 */
public class ValidateTests extends TestCase {

    public void testSchemaValidation() {

        String args[] = {
                "-xsd:" + ConfigTest.DATA_DIR + "books.xsd",
                "-t"};
        try {
            int rc = new Validate().doValidate(args);
            assertTrue(rc==0);
        } catch (Exception e) {
            e.printStackTrace();
            fail();
        }
    }

    public void testBasicInstanceValidation() {

        String args[] = {
                "-xsd:" + ConfigTest.DATA_DIR + "books.xsd",
                "-s:" + ConfigTest.DATA_DIR + "books.xml",
                "-t"};
        try {
            int rc = new Validate().doValidate(args);
            assertTrue(rc==0);
        } catch (Exception e) {
            e.printStackTrace();
            fail();
        }
    }

    public void testExportSCM() {

        String args[] = {
                "-xsd:" + ConfigTest.DATA_DIR + "books.xsd",
                "-scmout:" + ConfigTest.DATA_DIR + "books.scm",
                "-t"};
        try {
            int rc = new Validate().doValidate(args);
            assertTrue(rc==0);
        } catch (Exception e) {
            e.printStackTrace();
            fail();
        }
    }

    public void testImportSCM() {

        String args[] = {
                "-scmin:" + ConfigTest.DATA_DIR + "books.scm",
                "-s:" + ConfigTest.DATA_DIR + "books.xml",
                "-t"};
        try {
            int rc = new Validate().doValidate(args);
            assertTrue(rc==0);
        } catch (Exception e) {
            e.printStackTrace();
            fail();
        }
    }

    public void testValidationWithParams() {
        String args[] = {
                "-xsd:" + ConfigTest.DATA_DIR + "schema-with-param.xsd",
                "-s:" + ConfigTest.DATA_DIR + "birthdays.xml",
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

    public void testValidationFailureWithParams() {
        String args[] = {
                "-xsd:" + ConfigTest.DATA_DIR + "schema-with-param.xsd",
                "-s:" + ConfigTest.DATA_DIR + "birthdays.xml",
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

    public void testTypeAlternativeWithParams() {
        String args[] = {
                "-xsd:" + ConfigTest.DATA_DIR + "schema-with-param.xsd",
                "-s:" + ConfigTest.DATA_DIR + "stayAtHome.xml",
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

    public void testTypeAlternativeWithParams2() {
        String args[] = {
                "-xsd:" + ConfigTest.DATA_DIR + "schema-with-param.xsd",
                "-s:" + ConfigTest.DATA_DIR + "goToWork.xml",
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

    public void testTypeAlternativeWithParamsFailure() {
        String args[] = {
                "-xsd:" + ConfigTest.DATA_DIR + "schema-with-param.xsd",
                "-s:" + ConfigTest.DATA_DIR + "goToWork.xml",
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

    public void testStatsReporting() {

        new File(ConfigTest.DATA_DIR + "validation-report.xml").delete();

        String args[] = {
                "-xsd:" + ConfigTest.DATA_DIR + "books.xsd",
                "-s:" + ConfigTest.DATA_DIR + "books.xml",
                "-stats:" + ConfigTest.DATA_DIR + "validation-report.xml",
                "-t"};
        try {
            int rc = new Validate().doValidate(args);
            assertTrue(rc == 0);
            Processor proc = new Processor(false);
            XdmItem report = proc.newDocumentBuilder().build(
                    new File(ConfigTest.DATA_DIR + "validation-report.xml"));
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

