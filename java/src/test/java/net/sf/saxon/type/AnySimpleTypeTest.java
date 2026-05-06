package net.sf.saxon.type;

import net.sf.saxon.junit.Testdata;
import net.sf.saxon.s9api.*;
import org.junit.jupiter.api.Assertions;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;
import org.xml.sax.InputSource;

import javax.xml.transform.sax.SAXSource;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.ArrayList;


public class AnySimpleTypeTest {

	private ArrayList<XmlProcessingError> reportedErrors;

	@BeforeEach
	public void setup() {
		reportedErrors = new ArrayList<XmlProcessingError>();
	}

	@Test
	@DisplayName("Error raised when attempt to construct an instance of xs:anySimpleType from a string (XPST0017).")
	public void testConstructor_fromString() {

		try {
			Processor processor = new Processor(false);

			// Prepare the stylesheet
			Path pathToStylesheet = Testdata.path("constructor-functions/built-in-types/anySimpleType/construct_from_string.xsl");
			SAXSource stylesheet = new SAXSource(new InputSource(Files.newInputStream(pathToStylesheet)));
			stylesheet.setSystemId(String.valueOf(pathToStylesheet));
			XsltCompiler compiler = processor.newXsltCompiler();
			compiler.setSchemaAware(false);

			compiler.setErrorReporter(error -> {
				this.reportedErrors.add(error);
			});

			XsltExecutable exec = compiler.compile(stylesheet);


		} catch (SaxonApiException error) {

			// Confirm that the error is a compilation error. If so, specifics already checked (above).  If not, something else has gone wrong.
			Assertions.assertEquals("Errors were reported during stylesheet compilation", error.getMessage());

			// Check that only one error was reported.
			Assertions.assertEquals(1, reportedErrors.size(), "More than one error was reported but was only expecting XPST0017.");

			// Validate the error.
			XmlProcessingError firstError = reportedErrors.get(0);
			Assertions.assertEquals("XPST0017", firstError.getErrorCode().getLocalName());
			Assertions.assertEquals("Cannot find a 1-argument function named Q{http://www.w3.org/2001/XMLSchema}anySimpleType(). Abstract type used in constructor function: {http://www.w3.org/2001/XMLSchema}anySimpleType", firstError.getMessage());

		} catch (Exception e) {

			Assertions.fail("Expected XPST0017 but got " + e.getClass() + ", " + e.getMessage());

		}

	}


	@Test
	@DisplayName("Error raised when attempt to construct an instance of xs:anySimpleType from an integer (XPST0017).")
	public void testConstructor_fromInteger() {

		try {
			Processor processor = new Processor(false);

			// Prepare the stylesheet
			Path pathToStylesheet = Testdata.path("constructor-functions/built-in-types/anySimpleType/construct_from_integer.xsl");
			SAXSource stylesheet = new SAXSource(new InputSource(Files.newInputStream(pathToStylesheet)));
			stylesheet.setSystemId(String.valueOf(pathToStylesheet));
			XsltCompiler compiler = processor.newXsltCompiler();
			compiler.setSchemaAware(false);

			compiler.setErrorReporter(error -> {
				this.reportedErrors.add(error);
			});

			XsltExecutable exec = compiler.compile(stylesheet);


		} catch (SaxonApiException error) {

			// Confirm that the error is a compilation error. If so, specifics already checked (above).  If not, something else has gone wrong.
			Assertions.assertEquals("Errors were reported during stylesheet compilation", error.getMessage());

			// Check that only one error was reported.
			Assertions.assertEquals(1, reportedErrors.size(), "More than one error was reported but was only expecting XPST0017.");

			// Validate the error.
			XmlProcessingError firstError = reportedErrors.get(0);
			Assertions.assertEquals("XPST0017", firstError.getErrorCode().getLocalName());
			Assertions.assertEquals("Cannot find a 1-argument function named Q{http://www.w3.org/2001/XMLSchema}anySimpleType(). Abstract type used in constructor function: {http://www.w3.org/2001/XMLSchema}anySimpleType", firstError.getMessage());

		} catch (Exception e) {

			Assertions.fail("Expected XPST0017 but got " + e.getClass() + ", " + e.getMessage());

		}

	}

}