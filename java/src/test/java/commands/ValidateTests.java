package commands;

import com.saxonica.Validate;
import net.sf.saxon.junit.Testdata;
import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.XPathCompiler;
import net.sf.saxon.s9api.XdmAtomicValue;
import net.sf.saxon.s9api.XdmItem;
import org.junit.jupiter.api.Test;

import java.nio.file.Path;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;

/**
 * JUnit tests on Validate Command Line options
 */
public class ValidateTests {
    private final Path TEMPDIR = Testdata.path("temp");

    public void setUp() {
        System.err.println("TEMP:" + TEMPDIR);
    }

    @Test
    public void testSchemaValidation() {

        String[] args = {
                "-xsd:" + Testdata.file("books.xsd"),
                "-t"};
        try {
            int rc = new Validate().doValidate(args);
            assertEquals(0, rc);
        } catch (Exception e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testBasicInstanceValidation() {

        String[] args = {
                "-xsd:" + Testdata.file("books.xsd"),
                "-s:" + Testdata.file("books.xml"),
                "-t"};
        try {
            int rc = new Validate().doValidate(args);
            assertEquals(0, rc);
        } catch (Exception e) {
            e.printStackTrace();
            fail();
        }
    }

    private void testExportSCM() {

        String[] args = {
                "-xsd:" + Testdata.file("books.xsd"),
                "-export:" + TEMPDIR.resolve("books.scm"),
                "-t"};
        try {
            int rc = new Validate().doValidate(args);
            assertEquals(0, rc);
        } catch (Exception e) {
            e.printStackTrace();
            fail();
        }
    }


    private void testImportSCM() {

        String[] args = {
                "-xsd:" + TEMPDIR.resolve("books.scm"),
                "-xsiloc:off",
                "-s:" + Testdata.file("books.xml"),
                "-t"};
        try {
            int rc = new Validate().doValidate(args);
            assertEquals(0, rc);
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
                "-xsd:" + Testdata.file("schema-with-param.xsd"),
                "-s:" + Testdata.file("birthdays.xml"),
                "-xsdversion:1.1",
                "-t",
                "baseLine=2012-01-01",
                "dayOfWeek=Monday"};
        try {
            int rc = new Validate().doValidate(args);
            assertEquals(0, rc);
        } catch (Exception e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testValidationFailureWithParams() {
        String[] args = {
                "-xsd:" + Testdata.file("schema-with-param.xsd"),
                "-s:" + Testdata.file("birthdays.xml"),
                "-xsdversion:1.1",
                "-t",
                "-quit:off",
                "baseLine=1980-01-01"};
        try {
            int rc = new Validate().doValidate(args);
            assertEquals(1, rc);
        } catch (Exception e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testTypeAlternativeWithParams() {
        String[] args = {
                "-xsd:" + Testdata.file("schema-with-param.xsd"),
                "-s:" + Testdata.file("stayAtHome.xml"),
                "-xsdversion:1.1",
                "-t",
                "dayOfWeek=Sunday"};
        try {
            int rc = new Validate().doValidate(args);
            assertEquals(0, rc);
        } catch (Exception e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testTypeAlternativeWithParams2() {
        String[] args = {
                "-xsd:" + Testdata.file("schema-with-param.xsd"),
                "-s:" + Testdata.file("goToWork.xml"),
                "-xsdversion:1.1",
                "-t",
                "dayOfWeek=Monday"};
        try {
            int rc = new Validate().doValidate(args);
            assertEquals(0, rc);
        } catch (Exception e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testTypeAlternativeWithParamsFailure() {
        String[] args = {
                "-xsd:" + Testdata.file("schema-with-param.xsd"),
                "-s:" + Testdata.file("goToWork.xml"),
                "-xsdversion:1.1",
                "-t",
                "dayOfWeek=Sunday"};
        try {
            int rc = new Validate().doValidate(args);
            assertEquals(1, rc);
        } catch (Exception e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testStatsReporting() {

        //noinspection ResultOfMethodCallIgnored
        TEMPDIR.resolve("validation-report.xml").toFile().delete();

        String[] args = {
                "-xsd:" + Testdata.file("books.xsd"),
                "-s:" + Testdata.file("books.xml"),
                "-stats:" + TEMPDIR.resolve("validation-report.xml"),
                "-t"};
        try {
            int rc = new Validate().doValidate(args);
            assertEquals(0, rc);
            Processor proc = new Processor(false);
            XdmItem report = proc.newDocumentBuilder().build(
                    TEMPDIR.resolve("validation-report.xml").toFile());
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

