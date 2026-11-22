package s9apitest;

import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.SaxonApiException;
import net.sf.saxon.s9api.XsltCompiler;
import net.sf.saxon.s9api.XsltExecutable;
import net.sf.saxon.trans.CompilerInfo;
import net.sf.saxon.trans.XPathException;
import org.junit.jupiter.api.Tag;
import org.junit.jupiter.api.Test;

import javax.xml.transform.stream.StreamSource;
import java.io.ByteArrayOutputStream;
import java.io.StringReader;

import static org.junit.jupiter.api.Assertions.fail;


/**
 * Tests SEF generation of a stylesheet with calls to extension functions defined by stubs
 */
@Tag("requires-ee")
public class StubLoaderTest {

    @Test
    public void testSingleNamespace() {

        String lib = "{\n"
                + "    \"namespace\": \"http://sjs/\",\n"
                + "    \"signatures\": {\n"
                + "      \"one\": {\n"
                + "        \"as\": \"xs:boolean\",\n"
                + "        \"param\": [\n"
                + "          \"xs:string\"\n"
                + "        ],\n"
                + "        \"arity\": [1]\n"
                + "      },\n"
                + "      \"two\": {\n"
                + "        \"as\": \"element(*)\",\n"
                + "        \"param\": [\n"
                + "          \"xs:string\",\n"
                + "          \"xs:string\"\n"
                + "        ],\n"
                + "        \"arity\": [1, 2]\n"
                + "      }\n"
                + "    }\n"
                + "  }";

        String xsl = "<xsl:transform xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>" +
                "<xsl:template name='xsl:initial-template'>" +
                "<xsl:value-of select='Q{http://sjs/}one(\"thing\")'/>" +
                "<xsl:value-of select='Q{http://sjs/}two(\"thing\")'/>" +
                "<xsl:value-of select='Q{http://sjs/}two(\"thing\", \"thong\")'/>" +
                "</xsl:template>" +
                "</xsl:transform>";
        try {
            Processor proc = new Processor(true);
            XsltCompiler comp = proc.newXsltCompiler();
            comp.setJustInTimeCompilation(false);
            CompilerInfo info = comp.getUnderlyingCompilerInfo();
            info.importStubFunctionLibrary(new StreamSource(new StringReader(lib)));
            XsltExecutable exec = comp.compile(new StreamSource(new StringReader(xsl)));
            ByteArrayOutputStream sef = new ByteArrayOutputStream();
            exec.export(sef, "JS3");

        } catch (XPathException | SaxonApiException e) {
            fail(e);
        }


    }

    @Test
    public void testMultipleNamespace() {

        String lib = "[{\n"
                + "    \"namespace\": \"http://sjs/\",\n"
                + "    \"signatures\": {\n"
                + "      \"one\": {\n"
                + "        \"as\": \"xs:boolean\",\n"
                + "        \"param\": [\n"
                + "          \"xs:string\"\n"
                + "        ],\n"
                + "        \"arity\": [1]\n"
                + "      },\n"
                + "      \"two\": {\n"
                + "        \"as\": \"element(*)\",\n"
                + "        \"param\": [\n"
                + "          \"xs:string\",\n"
                + "          \"xs:string\"\n"
                + "        ],\n"
                + "        \"arity\": [1, 2]\n"
                + "      }\n"
                + "    }\n"
                + "  },"
                + "  {\n "
                + "    \"namespace\": \"http://sjs2/\",\n"
                + "    \"signatures\": {\n"
                + "      \"one\": {\n"
                + "        \"as\": \"xs:boolean\",\n"
                + "        \"param\": [\n"
                + "          \"xs:string\"\n"
                + "        ],\n"
                + "        \"arity\": [1]\n"
                + "      },\n"
                + "      \"two\": {\n"
                + "        \"as\": \"element(*)\",\n"
                + "        \"param\": [\n"
                + "          \"xs:string\",\n"
                + "          \"xs:string\"\n"
                + "        ],\n"
                + "        \"arity\": [1, 2]\n"
                + "      }\n"
                + "    }\n"
                + "  }]";

        String xsl = "<xsl:transform xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>" +
                "<xsl:template name='xsl:initial-template'>" +
                "<xsl:value-of select='Q{http://sjs/}one(\"thing\")'/>" +
                "<xsl:value-of select='Q{http://sjs/}two(\"thing\")'/>" +
                "<xsl:value-of select='Q{http://sjs/}two(\"thing\", \"thong\")'/>" +
                "<xsl:value-of select='Q{http://sjs2/}one(\"thing\")'/>" +
                "<xsl:value-of select='Q{http://sjs2/}two(\"thing\")'/>" +
                "<xsl:value-of select='Q{http://sjs2/}two(\"thing\", \"thong\")'/>" +
                "</xsl:template>" +
                "</xsl:transform>";
        try {
            Processor proc = new Processor(true);
            XsltCompiler comp = proc.newXsltCompiler();
            comp.setJustInTimeCompilation(false);
            CompilerInfo info = comp.getUnderlyingCompilerInfo();
            info.importStubFunctionLibrary(new StreamSource(new StringReader(lib)));
            XsltExecutable exec = comp.compile(new StreamSource(new StringReader(xsl)));
            ByteArrayOutputStream sef = new ByteArrayOutputStream();
            exec.export(sef, "JS3");

        } catch (XPathException | SaxonApiException e) {
            fail(e);
        }


    }


}

