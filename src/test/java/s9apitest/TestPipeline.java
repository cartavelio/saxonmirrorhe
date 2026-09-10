package s9apitest;

import jaxptest.ConfigTest;
import net.sf.saxon.s9api.*;
import net.sf.saxon.trace.AbstractTraceListener;
import net.sf.saxon.trace.Traceable;
import org.junit.jupiter.api.Test;

import javax.xml.transform.stream.StreamSource;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.StringReader;

import static org.junit.jupiter.api.Assertions.fail;

public class TestPipeline {
    private final ConfigTest configTest = new ConfigTest();
	private XsltExecutable xslt_executable1 = null;
	private XsltExecutable xslt_executable2 = null;
	private final File xslt1 = new File(configTest.getDataDir() + "echo_given_sourcefile.xsl");
	private final File xslt2 = new File(configTest.getDataDir() + "print_elements.xsl");

    @Test
	public void testPipeline() {
		// common processor
		Processor processor = new Processor(false);

		// transformer1
		XsltCompiler xslt_compiler1 = processor.newXsltCompiler();

		try {
			xslt_executable1 = xslt_compiler1.compile(new StreamSource(new FileInputStream(xslt1)));
		} catch (FileNotFoundException | SaxonApiException e) {
		    fail();
			e.printStackTrace();
		}

        XsltTransformer xslt_transformer1 = xslt_executable1.load();

		// set source file
		try {
			xslt_transformer1.setSource(new StreamSource(new FileInputStream(xslt1))); // itself
		} catch (FileNotFoundException e) {
			e.printStackTrace();
		}


		// transformer2
		XsltCompiler xslt_compiler2 = processor.newXsltCompiler();
		try {
			xslt_executable2 = xslt_compiler2.compile(new StreamSource(new FileInputStream(xslt2)));
		} catch (FileNotFoundException | SaxonApiException e) {
		    fail();
			e.printStackTrace();
		}

        XsltTransformer xslt_transformer2 = xslt_executable2.load();

		// transformer2's destination
		Serializer destination2 = processor.newSerializer(System.out);
		xslt_transformer2.setDestination(destination2);

		// transformer1's destination
		xslt_transformer1.setDestination(xslt_transformer2);

		/*
		// omit xml declaration in transformer2
		xslt_transformer2.getUnderlyingController().setOutputProperty("omit-xml-declaration", "yes");
		*/
        //xslt_transformer1.getUnderlyingController().setOutputProperty("omit-xml-declaration", "yes");
		// run transformation
		try {
			xslt_transformer1.transform();
		} catch (SaxonApiException e) {
			e.printStackTrace();
			fail();
		}
	}


    @Test
    public void testPipelineWithGlobalContextItem() {
        String xsl = "<xsl:stylesheet exclude-result-prefixes=\"#all\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" version=\"2.0\">\n"
                + "<xsl:variable name=\"supp\" as=\"xs:string\" select=\"root()/*/name()\"/>\n"
                + "\n"
                + "<xsl:template match=\"/\">\n"
                + "    <xsl:comment select=\"$supp\"/>\n"
                + "    <file/>\n"
                + "</xsl:template>\n"
                + "</xsl:stylesheet>\n";

        try {
            // common processor
            Processor processor = new Processor(false);

            // transformer1
            XsltCompiler xslt_compiler1 = processor.newXsltCompiler();
            xslt_compiler1.setCompileWithTracing(true);

            XsltExecutable exec = xslt_compiler1.compile(new StreamSource(new StringReader(xsl)));

            XsltTransformer xslt_transformer1 = exec.load();
            xslt_transformer1.setSource(new StreamSource(new StringReader(xsl))); // itself

            // transformer2
            XsltTransformer xslt_transformer2 = exec.load();

            // transformer2's destination
            Serializer destination2 = processor.newSerializer(System.out);
            xslt_transformer2.setDestination(destination2);

            // transformer1's destination
            xslt_transformer1.setDestination(xslt_transformer2);

            xslt_transformer1.transform();
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }


    @Test
    public void testPipelineWithGlobalContextItemWithTraceListener() {
        // bug 4114: stylesheet in pipeline has global variable that uses the context item
        String xsl = "<xsl:stylesheet exclude-result-prefixes=\"#all\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" version=\"2.0\">\n"
                + "<xsl:variable name=\"supp\" as=\"xs:string\" select=\"root()/*/name()\"/>\n"
                + "\n"
                + "<xsl:template match=\"/\">\n"
                + "    <xsl:comment select=\"$supp\"/>\n"
                + "    <file/>\n"
                + "</xsl:template>\n"
                + "</xsl:stylesheet>\n";

        try {
            // common processor
            Processor processor = new Processor(false);

            // transformer1
            XsltCompiler xslt_compiler1 = processor.newXsltCompiler();
            xslt_compiler1.setCompileWithTracing(true);

            XsltExecutable exec = xslt_compiler1.compile(new StreamSource(new StringReader(xsl)));

            XsltTransformer xslt_transformer1 = exec.load();
            xslt_transformer1.setSource(new StreamSource(new StringReader(xsl))); // itself
            xslt_transformer1.setTraceListener(new AbstractTraceListener() {
                @Override
                protected String getOpeningAttributes() {
                    return "";
                }

                @Override
                protected String tag(Traceable construct) {
                    return "E" + construct.getClass();
                }
            });


            // transformer2
            XsltTransformer xslt_transformer2 = exec.load();

            // transformer2's destination
            Serializer destination2 = processor.newSerializer(System.out);
            xslt_transformer2.setDestination(destination2);
            xslt_transformer2.setTraceListener(new AbstractTraceListener() {
                @Override
                protected String getOpeningAttributes() {
                    return "";
                }

                @Override
                protected String tag(Traceable construct) {
                    return "F" + construct.getClass();
                }
            });

            // transformer1's destination
            xslt_transformer1.setDestination(xslt_transformer2);

            xslt_transformer1.transform();
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }


}

