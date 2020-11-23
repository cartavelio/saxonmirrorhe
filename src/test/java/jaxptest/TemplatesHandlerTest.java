package jaxptest;

import com.saxonica.config.ProfessionalTransformerFactory;
import junit.framework.TestCase;
import net.sf.saxon.TransformerFactoryImpl;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.StaticContext;
import net.sf.saxon.expr.StringLiteral;
import net.sf.saxon.functions.FunctionLibrary;
import net.sf.saxon.lib.FeatureKeys;
import net.sf.saxon.om.Function;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.trans.CompilerInfo;
import net.sf.saxon.trans.SymbolicName;
import org.xml.sax.SAXException;
import org.xml.sax.XMLReader;

import javax.xml.parsers.ParserConfigurationException;
import javax.xml.parsers.SAXParserFactory;
import javax.xml.transform.Templates;
import javax.xml.transform.Transformer;
import javax.xml.transform.TransformerException;
import javax.xml.transform.TransformerFactory;
import javax.xml.transform.sax.SAXTransformerFactory;
import javax.xml.transform.sax.TemplatesHandler;
import javax.xml.transform.stream.StreamResult;
import javax.xml.transform.stream.StreamSource;
import java.io.*;
import java.util.List;

/**
 *
 */
public class TemplatesHandlerTest extends TestCase {

    public static void testTemplatesHandler() throws SAXException,
            ParserConfigurationException,
            IOException, TransformerException {

        String xsl = ConfigTest.DATA_DIR + "books.xsl";
        String xml = ConfigTest.DATA_DIR + "books.xml";

        SAXParserFactory factory = SAXParserFactory.newInstance();
        factory.setNamespaceAware(true);
        factory.setValidating(false);
        XMLReader reader = factory.newSAXParser().getXMLReader();

        SAXTransformerFactory transformerFactory = new net.sf.saxon.TransformerFactoryImpl();
        //((TransformerFactoryImpl)transformerFactory).getConfiguration().setConfigurationProperty(FeatureKeys.GENERATE_BYTE_CODE, false);
//(SAXTransformerFactory) TransformerFactory.newInstance();

        TemplatesHandler templatesHandler =
                transformerFactory.newTemplatesHandler();
        reader.setContentHandler(templatesHandler);
        reader.parse(new File(xsl).toURI().toASCIIString());

        Templates templates = templatesHandler.getTemplates();
        Transformer transformer = templates.newTransformer();
        transformer.transform(new StreamSource(new File(xml)), new StreamResult(System.out));

    }

    public void testAddExtensionLibrary() {
        try {
            final String xsl
                    = "<?xml version=\"1.0\"?> \n" +
                    "<xsl:stylesheet version=\"2.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\"\n" +
                    " xmlns:hello='http://my.hello.com/'>" +
                    "<xsl:template match='/'>\n" +
                    "<xsl:value-of select='hello:world()'/>\n" +
                    "</xsl:template>\n" +
                    "</xsl:stylesheet>";

            TransformerFactoryImpl tFactory = new ProfessionalTransformerFactory();
            CompilerInfo info = tFactory.getConfiguration().makeCompilerInfo();
            info.setExtensionFunctionLibrary(new FunctionLibrary() {

                StructuredQName fnName = new StructuredQName("hello", "http://my.hello.com/", "world");
                SymbolicName.F fn = new SymbolicName.F(fnName, 0);

                public Function getFunctionItem(SymbolicName.F functionName, StaticContext env) {
                    if (functionName.equals(fn) && functionName.getArity() == 0) {
                        return null;
                    } else {
                        return null;
                    }
                }

                public boolean isAvailable(SymbolicName.F functionName) {
                    return functionName.equals(fn) && functionName.getArity() == 0;
                }

                public Expression bind(SymbolicName.F functionName, Expression[] staticArgs, StaticContext env, List<String> reasons) {
                    if (functionName.equals(fn) && staticArgs.length == 0) {
                        return new StringLiteral("hello world");
                    } else {
                        return null;
                    }
                }

                public FunctionLibrary copy() {
                    return this;
                }
            });

            Templates templates = tFactory.newTemplates(new StreamSource(new StringReader(xsl)), info);
            Transformer transformer = templates.newTransformer();
            transformer.setOutputProperty("method", "text");
            StringWriter sw = new StringWriter();

            transformer.transform(new StreamSource(new StringReader(xsl)),
                    new StreamResult(sw));
            assertEquals(sw.toString(), "hello world");
        } catch (TransformerException e) {
            fail();
            e.printStackTrace();
        }
    }

    /**
     * Test use of a per-stylesheet extension library
     */

    public void testExtensionNotFound() {
        try {
            String inputUri = "file:///Users/Mike/bugs/2011/Pedruzzi/base64/books.xml";
            String xsltUrl = "file:///Users/Mike/bugs/2011/Pedruzzi/base64/TextTobase64.xsl";

            TransformerFactory tFactory = new net.sf.saxon.TransformerFactoryImpl();
            tFactory.setAttribute(FeatureKeys.RECOGNIZE_URI_QUERY_PARAMETERS, Boolean.TRUE);
            tFactory.setAttribute(FeatureKeys.STRIP_WHITESPACE, "ignorable");
            tFactory.setAttribute(FeatureKeys.VERSION_WARNING, Boolean.FALSE);

            Transformer transformer = tFactory.newTransformer(new StreamSource(xsltUrl));


            System.out.println();
            System.out.println("XSLT starting.");
            transformer.transform(new StreamSource(inputUri),
                    new StreamResult(new OutputStreamWriter(System.out)));
            System.out.println();
            System.out.println("XSLT finished.");
            assertFalse(true); // should not succeed
        } catch (TransformerException e) {
            //e.printStackTrace();
        }
    }


}

//
// The contents of this file are subject to the Mozilla Public License Version
// 1.0 (the "License");
// you may not use this file except in compliance with the License. You may
// obtain a copy of the
// License at http://www.mozilla.org/MPL/
//
// Software distributed under the License is distributed on an "AS IS" basis,
// WITHOUT WARRANTY OF ANY KIND, either express or implied.
// See the License for the specific language governing rights and limitations
// under the License.
//
// The Original Code is: all this file.
//
// The Initial Developer of the Original Code is Michael Kay,
//
// Portions created by (your name) are Copyright (C) (your legal entity). All
// Rights Reserved.
//
// Contributor(s): none.
//



