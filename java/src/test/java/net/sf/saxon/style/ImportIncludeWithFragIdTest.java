package net.sf.saxon.style;

import net.sf.saxon.junit.Testdata;
import net.sf.saxon.s9api.SaxonApiException;

import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.XsltCompiler;
import net.sf.saxon.s9api.XsltExecutable;
import net.sf.saxon.Transform;

import org.junit.jupiter.api.*;
import org.xml.sax.InputSource;

import javax.xml.transform.sax.SAXSource;
import java.io.*;
import java.nio.file.Path;
import java.nio.file.Files;

import static org.junit.jupiter.api.Assertions.*;


public class ImportIncludeWithFragIdTest {

	private final Transform transform = new Transform();

	private static class SimpleTransform extends Transform {

		public SimpleTransform() {
		}

		public String transform(String[] args) throws Exception {

			ByteArrayOutputStream baos = new ByteArrayOutputStream();
			// Capture System.out
			System.setOut(new PrintStream(baos));
			super.doTransform(args);
			// Revert change to System.out
			baos.close();
			System.setOut(new PrintStream(new FileOutputStream(FileDescriptor.out)));
			return baos.toString("utf-8");

		}

	}

	@Test
	@DisplayName("xsl:include without a fragment ID resolves to a stylesheet and the transformation completes successfully")
	public void test_includeWithoutFragId() {

		Path pathToStylesheet = Testdata.path("include-import/happy-path/happy_path_include.xsl");
		String[] args = {
				"-t",
				"-s:" + pathToStylesheet,
				"-xsl:" + pathToStylesheet
		};

		try {
			SimpleTransform transformer = new SimpleTransform();
			String result = transformer.transform(args);

			// Check result
			Assertions.assertEquals("<out>Transformation completed successfully using happy-path and happy-path-dep stylesheets.</out>", result.toString());
		} catch (Exception e) {
			fail();
		}
	}

	@Test
	@DisplayName("xsl:import without a fragment ID resolves to a stylesheet and the transformation completes successfully")
	public void test_importWithoutFragId() {

		Path pathToStylesheet = Testdata.path("include-import/happy-path/happy_path_import.xsl");
		String[] args = {
				"-t",
				"-s:" + pathToStylesheet,
				"-xsl:" + pathToStylesheet
		};

		try {
			SimpleTransform transformer = new SimpleTransform();
			String result = transformer.transform(args);

			// Check result
			Assertions.assertEquals("<out>Transformation completed successfully using happy-path and happy-path-dep stylesheets.</out>", result.toString());
		} catch (Exception e) {
			fail();
		}
	}

	@Test
	@DisplayName("xsl:include with a fragment ID resolves to a stylesheet and the transformation completes successfully")
	public void test_includeWithValidFragIdOnStylesheet() {

		Path pathToStylesheet = Testdata.path("include-import/valid-frag-id/on-stylesheet/on_stylesheet_include.xsl");
		String[] args = {
				"-t",
				"-s:" + pathToStylesheet,
				"-xsl:" + pathToStylesheet
		};

		try {
			SimpleTransform transformer = new SimpleTransform();
			String result = transformer.transform(args);

			// Check result
			Assertions.assertEquals("<out>Transformation completed successfully using principal stylesheet and dependency stylesheet.</out>", result.toString());
		} catch (Exception e) {
			Assertions.fail("An error was thrown but none was expected.");
		}
	}

	@Test
	@DisplayName("xsl:import with a fragment ID resolves to a stylesheet and the transformation completes successfully")
	public void test_importWithValidFragIdOnStylesheet() {

		Path pathToStylesheet = Testdata.path("include-import/valid-frag-id/on-stylesheet/on_stylesheet_import.xsl");
		String[] args = {
				"-t",
				"-s:" + pathToStylesheet,
				"-xsl:" + pathToStylesheet
		};

		try {
			SimpleTransform transformer = new SimpleTransform();
			String result = transformer.transform(args);

			// Check result
			Assertions.assertEquals("<out>Transformation completed successfully using principal stylesheet and dependency stylesheet.</out>", result.toString());
		} catch (Exception e) {
			Assertions.fail("An error was thrown but none was expected.");
		}
	}

	@Test
	@DisplayName("xsl:include with a fragment ID resolves to an element that's not a stylesheet so an exception is thrown (XTSE0165)")
	public void test_includeWithValidFragIdNotOnStylesheet() {

		try {
			Processor processor = new Processor(false);

			// Prepare the stylesheet
			Path pathToStylesheet = Testdata.path("include-import/valid-frag-id/not-on-stylesheet/not_on_stylesheet_include.xsl");
			SAXSource stylesheet = new SAXSource(new InputSource(Files.newInputStream(pathToStylesheet)));
			stylesheet.setSystemId(String.valueOf(pathToStylesheet));
			XsltCompiler compiler = processor.newXsltCompiler();
			compiler.setSchemaAware(false);

			compiler.setErrorReporter(error -> {
				Assertions.assertEquals("XTSE0165", error.getErrorCode().getLocalName());
				Assertions.assertTrue(error.getMessage().endsWith(" is not a stylesheet"));
			});

			XsltExecutable exec = compiler.compile(stylesheet);

			Assertions.fail("Expected an error to be thrown but it wasn't.");

		} catch (SaxonApiException error) {

			// Confirm that the error is a compilation error. If so, specifics already checked (above).  If not, something else has gone wrong.
			Assertions.assertEquals("Errors were reported during stylesheet compilation", error.getMessage());

		} catch (Exception e) {

			Assertions.fail("An error was thrown but it wasn't the one expected.");

		}
	}

	@Test
	@DisplayName("xsl:import with a fragment ID resolves to an element that's not a stylesheet so an exception is thrown (XTSE0165)")
	public void test_importWithValidFragIdNotOnStylesheet() {

		try {
			Processor processor = new Processor(false);

			// Prepare the stylesheet
			Path pathToStylesheet = Testdata.path("include-import/valid-frag-id/not-on-stylesheet/not_on_stylesheet_import.xsl");
			SAXSource stylesheet = new SAXSource(new InputSource(Files.newInputStream(pathToStylesheet)));
			stylesheet.setSystemId(String.valueOf(pathToStylesheet));
			XsltCompiler compiler = processor.newXsltCompiler();
			compiler.setSchemaAware(false);

			compiler.setErrorReporter(error -> {
				Assertions.assertEquals("XTSE0165", error.getErrorCode().getLocalName());
				Assertions.assertTrue(error.getMessage().endsWith(" is not a stylesheet"));
			});

			XsltExecutable exec = compiler.compile(stylesheet);

			Assertions.fail("Expected an error to be thrown but it wasn't.");

		} catch (SaxonApiException error) {

			// Confirm that the error is a compilation error. If so, specifics already checked (above).  If not, something else has gone wrong.
			Assertions.assertEquals("Errors were reported during stylesheet compilation", error.getMessage());

		} catch (Exception e) {

			Assertions.fail("An error was thrown but it wasn't the one expected.");

		}
	}

	@Test
	@DisplayName("xsl:include with a fragment ID that doesn't match an ID in the included stylesheet so an exception is thrown (XTSE0165)")
	public void test_includeWithMissingFragId() {

		try {
			Processor processor = new Processor(false);

			// Prepare the stylesheet
			Path pathToStylesheet = Testdata.path("include-import/missing-frag-id/missing_frag_id_include.xsl");
			SAXSource stylesheet = new SAXSource(new InputSource(Files.newInputStream(pathToStylesheet)));
			stylesheet.setSystemId(String.valueOf(pathToStylesheet));
			XsltCompiler compiler = processor.newXsltCompiler();
			compiler.setSchemaAware(false);

			compiler.setErrorReporter(error -> {
				Assertions.assertEquals("XTSE0165", error.getErrorCode().getLocalName());
				Assertions.assertTrue(error.getMessage().endsWith(" is not a stylesheet"));
			});

			XsltExecutable exec = compiler.compile(stylesheet);

			Assertions.fail("Expected an error to be thrown but it wasn't.");

		} catch (SaxonApiException error) {

			// Confirm that the error is a compilation error. If so, specifics already checked (above).  If not, something else has gone wrong.
			// The error message for an xsl:include in this context doesn't get tidied up like the others.
			Assertions.assertEquals("Included document missing_frag_id.xsl#does-not-exist is not a stylesheet", error.getMessage());

		} catch (Exception e) {

			Assertions.fail("An error was thrown but it wasn't the one expected.");

		}
	}

	@Test
	@DisplayName("xsl:import with a fragment ID that doesn't match an ID in the imported stylesheet so an exception is thrown (XTSE0165)")
	public void test_importWithMissingFragId() {

		try {
			Processor processor = new Processor(false);

			// Prepare the stylesheet
			Path pathToStylesheet = Testdata.path("include-import/missing-frag-id/missing_frag_id_import.xsl");
			SAXSource stylesheet = new SAXSource(new InputSource(Files.newInputStream(pathToStylesheet)));
			stylesheet.setSystemId(String.valueOf(pathToStylesheet));
			XsltCompiler compiler = processor.newXsltCompiler();
			compiler.setSchemaAware(false);

			compiler.setErrorReporter(error -> {
				Assertions.assertEquals("XTSE0165", error.getErrorCode().getLocalName());
				Assertions.assertTrue(error.getMessage().endsWith(" is not a stylesheet"));
			});

			XsltExecutable exec = compiler.compile(stylesheet);

			Assertions.fail("Expected an error to be thrown but it wasn't.");

		} catch (SaxonApiException error) {

			// Confirm that the error is a compilation error. If so, specifics already checked (above).  If not, something else has gone wrong.
			Assertions.assertEquals("Errors were reported during stylesheet compilation", error.getMessage());

		} catch (Exception e) {

			Assertions.fail("An error was thrown but it wasn't the one expected.");

		}
	}

}