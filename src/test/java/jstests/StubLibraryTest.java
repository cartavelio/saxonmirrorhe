package jstests;

//import com.saxonica.ee.extfn.IxslFunctionLibrary;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.lib.ExtensionFunctionCall;
import net.sf.saxon.lib.ExtensionFunctionDefinition;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.s9api.*;
import net.sf.saxon.trans.CompilerInfo;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.BooleanValue;
import net.sf.saxon.value.SequenceType;
import org.junit.jupiter.api.Test;

import javax.xml.transform.stream.StreamSource;
import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.io.StringReader;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.fail;

/**
 * Created by mike on 27/01/2016.
 */
public class StubLibraryTest {

    @Test
    public void testLoadStubs() {
        String stubs = "{\n"
                + "  \"namespace\": \"http://example.com/\",\n"
                + "  \"signatures\": {\n"
                + "    \"sample\": {\n"
                + "      \"as\": \"xs:boolean\",\n"
                + "      \"param\": [\n"
                + "        \"xs:string\"\n"
                + "      ],\n"
                + "      \"arity\": [1]\n"
                + "    }}}";
        String xsl = "<xsl:transform xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>"
                + "<xsl:template name='xsl:initial-template'><z><xsl:sequence select=\"Q{http://example.com/}sample('data')\"/></z></xsl:template>"
                + "</xsl:transform>";
        try {
            Processor p = new Processor(true);
            XsltCompiler comp = p.newXsltCompiler();
            comp.setJustInTimeCompilation(false);
            CompilerInfo info = comp.getUnderlyingCompilerInfo();
            info.importStubFunctionLibrary(new StreamSource(new StringReader(stubs)));
            XsltExecutable exec = comp.compile(new StreamSource(new StringReader(xsl)));
            ByteArrayOutputStream baos = new ByteArrayOutputStream();
            exec.export(baos);
            ByteArrayInputStream bais = new ByteArrayInputStream(baos.toByteArray());
            p.registerExtensionFunction(new ExtensionFunctionDefinition() {
                @Override
                public StructuredQName getFunctionQName() {
                    return new StructuredQName("", "http://example.com/", "sample");
                }

                @Override
                public SequenceType[] getArgumentTypes() {
                    return new SequenceType[]{SequenceType.SINGLE_STRING};
                }

                @Override
                public SequenceType getResultType(SequenceType[] suppliedArgumentTypes) {
                    return SequenceType.SINGLE_BOOLEAN;
                }

                @Override
                public ExtensionFunctionCall makeCallExpression() {
                    return new ExtensionFunctionCall() {
                        @Override
                        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
                            return BooleanValue.get(arguments[0].head().getStringValue().equals("data"));
                        }
                    };
                }
            });
            XsltExecutable exec2 = comp.loadExecutablePackage(new StreamSource(bais));
            Xslt30Transformer trans = exec2.load30();
            XdmNode result = (XdmNode)trans.callTemplate(null);
            assertEquals("<z>true</z>", result.toString());
        } catch (SaxonApiException|XPathException err) {
            err.printStackTrace();
            fail();
        }
    }

}

