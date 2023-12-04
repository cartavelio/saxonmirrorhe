package internaltests;

import net.sf.saxon.Configuration;
import net.sf.saxon.Controller;
import net.sf.saxon.Transform;
import net.sf.saxon.event.*;
import net.sf.saxon.expr.parser.Loc;
import net.sf.saxon.lib.AugmentedSource;
import net.sf.saxon.om.FingerprintedQName;
//import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.s9api.*;
import net.sf.saxon.serialize.SerializationProperties;
import net.sf.saxon.type.BuiltInAtomicType;
import org.junit.jupiter.api.Test;
import org.xml.sax.InputSource;

import javax.xml.transform.sax.SAXSource;
import javax.xml.transform.stream.StreamSource;
import java.io.File;
import java.net.URI;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.fail;

public class CheckSumTests {
    /*
    @Test void commandLineTest() {
        //  -xsl:src/test/resources/identity.xsl -export:out.sef.json -nogo "-ns:##html5" -target:JS3
        Processor processor = new Processor(false);
        Transform transformer = new Transform();
        transformer.doTransform(new String[] {"-xsl:src/test/resources/identity.xsl", "-export:out.sef.json", "-nogo", "-ns:##html5", "-target:JS"});
    }

    @Test
    public void testCheckSumFilter() {
        try {
            Processor processor = new Processor(false);
            Configuration config = processor.getUnderlyingConfiguration();

            XsltCompiler compiler = processor.newXsltCompiler();
            XsltExecutable exec = compiler.compile(new File("src/test/resources/identity.xsl"));
            Xslt30Transformer transformer = exec.load30();

            XdmDestination destination = new XdmDestination();

            PipelineConfiguration pipe = config.makePipelineConfiguration();
            Receiver out = destination.getReceiver(pipe, config.obtainDefaultSerializationProperties());

            AugmentedSource source = AugmentedSource.makeAugmentedSource(new StreamSource(new File("src/test/resources/identity.sef.json")));
            source.setSystemId("http://example.com/");
            source.addFilter(CheckSumFilter::new);

            transformer.transform(source, destination);

            System.err.println(destination.getXdmNode());

        } catch (Exception e) {
            fail();
        }

    }

     */
}
