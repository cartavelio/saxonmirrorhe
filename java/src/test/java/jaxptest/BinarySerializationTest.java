package jaxptest;

import com.saxonica.config.ProfessionalTransformerFactory;
import net.sf.saxon.lib.OutputURIResolver;
import org.junit.jupiter.api.Test;

import javax.xml.transform.*;
import javax.xml.transform.stream.StreamResult;
import javax.xml.transform.stream.StreamSource;
import java.io.*;

import static org.junit.jupiter.api.Assertions.assertTrue;


public class BinarySerializationTest {
    private final ConfigTest configTest = new ConfigTest();
    public final String TESTPDF = configTest.getDataDir() + "temp/testfile.pdf";

    @Test
    public void testBase64OutputMethod() throws TransformerException,
            FileNotFoundException {


        new File(TESTPDF).delete();
        TransformerFactory tfactory = new ProfessionalTransformerFactory();
        tfactory.setAttribute("http://saxon.sf.net/feature/outputURIResolver",
                new MyOutputURIResolver());

        OutputStream stdout = new ByteArrayOutputStream();
        StreamResult transResult = new StreamResult(stdout);

        String xsls = "<xsl:stylesheet version=\"2.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" xmlns:saxon=\"http://saxon.sf.net/\">\n" +
                "\t\n" +
                "\t<xsl:template match=\"/*\">\n" +
                "\t\t\n" +
                "\t\t<xsl:result-document method=\"saxon:base64Binary\" href=\"result.pdf\">\n" +
                "\t\t\t<xsl:value-of select=\".\"/>\n" +
                "\t\t</xsl:result-document>\t\t\n" +
                "\t\t\n" +
                "\t</xsl:template>\n" +
                "</xsl:stylesheet>";

        StreamSource xslSource = new StreamSource(new StringReader(xsls));
        Templates xsl = tfactory.newTemplates(xslSource);
        Transformer transformer = xsl.newTransformer();

        StreamSource xmlSource = new StreamSource(new File(configTest.getDataDir() + "base64data.xml"));

        transformer.transform(xmlSource, transResult);

        assertTrue(new File(TESTPDF).exists());

    }

    public static class MyOutputURIResolver implements OutputURIResolver {

        FileOutputStream resdoc;

        /**
         * Konstruktor
         */
        public MyOutputURIResolver() {
            ConfigTest ctest = new ConfigTest();
            String TESTPDF = ctest.getDataDir() + "temp/testfile.pdf";

            System.out.println("MyOutputURIResolver");

            try {
                resdoc = new FileOutputStream(TESTPDF);
            } catch (FileNotFoundException e) {
                throw new AssertionError(e);
            }

        }

        @Override
        public OutputURIResolver newInstance() {
            return new MyOutputURIResolver();
        }

        @Override
        public Result resolve(String href, String base)
                throws TransformerException {

            System.out.println("resolve");

            StreamResult res = new StreamResult(resdoc);
            res.setSystemId(href);
            return res;

        }

        @Override
        public void close(Result result) {

            System.out.println("close");
            try {
                resdoc.close();
            } catch (IOException e) {
                e.printStackTrace();
            }

        }

    }


}



