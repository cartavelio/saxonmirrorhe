package jstests;

//import com.saxonica.ee.extfn.IxslFunctionLibrary;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.config.ProfessionalConfiguration;
import com.saxonica.ee.extfn.js.IXSLFunctionSet;
import net.sf.saxon.functions.FunctionLibrary;
import net.sf.saxon.functions.FunctionLibraryList;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.s9api.*;
import org.junit.jupiter.api.Test;

import javax.xml.transform.stream.StreamSource;
import java.io.StringReader;

import static net.sf.saxon.s9api.streams.Steps.child;
import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertFalse;
import static org.junit.jupiter.api.Assertions.fail;

/**
 * Created by mike on 27/01/2016.
 */
public class IxslExtensionTest {

    @Test
    public void testIxslEval() {
        try {
            Processor p = new Processor(true);
            ProfessionalConfiguration config = (ProfessionalConfiguration)p.getUnderlyingConfiguration();
            FunctionLibrary lib = IXSLFunctionSet.getInstance();
            XPathCompiler comp = p.newXPathCompiler();
            FunctionLibraryList list = config.getBuiltInExtensionLibraryList(31);
            list.addFunctionLibrary(lib);
            comp.declareNamespace("ixsl", NamespaceConstant.IXSL);
            XPathExecutable exec = comp.compile("ixsl:eval('2+2')");
            XdmItem result = exec.load().evaluateSingle();
            assertEquals("4", result.getStringValue());
        } catch (SaxonApiException err) {
            err.printStackTrace();
            fail();
        }
    }

    @Test
    public void testIxslFunctionAvailableJS() {
        XdmNode node = testIxslFunctionAvailable("JS");
        assertFalse(node.select(child("out").then(child("*"))).exists());
    }

    @Test
    public void testIxslFunctionAvailableEE() {
        XdmNode node = testIxslFunctionAvailable("EE");
        assertFalse(node.select(child("out").then(child("*"))).exists());
    }

    private XdmNode testIxslFunctionAvailable(String target) {
        try {

            String xsl = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n"
                    + "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\"\n"
                    + "    xmlns:xs=\"http://www.w3.org/2001/XMLSchema\"\n"
                    + "    xmlns:ixsl=\"http://saxonica.com/ns/interactiveXSLT\"\n"
                    + "    exclude-result-prefixes=\"xs\"\n"
                    + "    version=\"3.0\">\n"
                    + "    \n"
                    + "    <xsl:template name=\"xsl:initial-template\">\n"
                    + "        <out>\n"
                    + "            <in xsl:use-when=\"function-available('ixsl:eval', 1)\">\n"
                    + "                <result>\n"
                    + "                    <xsl:sequence select=\"ixsl:eval('2+2')\"/>\n"
                    + "                </result>\n"
                    + "            </in>\n"
                    + "        </out> \n"
                    + "    </xsl:template>\n"
                    + "    \n"
                    + "</xsl:stylesheet>";
            Processor p = new Processor(true);
            EnterpriseConfiguration config = (EnterpriseConfiguration) p.getUnderlyingConfiguration();
            FunctionLibrary lib = IXSLFunctionSet.getInstance();
            XsltCompiler comp = p.newXsltCompiler();
            comp.setTargetEdition(target);
            XsltExecutable exec = comp.compile(new StreamSource(new StringReader(xsl)));
            try {
                final XdmDestination destination = new XdmDestination();
                exec.load30().callTemplate(null, destination);
                return destination.getXdmNode();
            } catch (SaxonApiException e) {
                fail(e.getMessage());
            }
        } catch (SaxonApiException err) {
            err.printStackTrace();
            fail();
        }
        return null;
    }
}

