package s9apitest;

import net.sf.saxon.junit.Testdata;
import net.sf.saxon.s9api.*;
import net.sf.saxon.value.AtomicValue;
import org.junit.jupiter.api.Test;

import javax.xml.transform.stream.StreamSource;
import java.io.IOException;
import java.io.StringReader;
import java.io.StringWriter;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.fail;

/**
 * Test XSD processing
 */
public class TestSchemaProcessing {
    @Test
    public void testBug6050() {
        try {
            Processor processor = new Processor(true);
            XsdCompiler mgr = processor.newXsdCompiler();
            XsdSchema schema = mgr.compile(Testdata.file("bug6050/person.xsd"));
            //mgr.load(new File("src/test/java/bugs/bug6050/simple.xsd"));

            XQueryCompiler compiler = processor.newXQueryCompiler();
            compiler.useSchema(schema);
            XQueryExecutable exec = compiler.compile(Testdata.file("bug6050/common.xq"));
            XQueryEvaluator eval = exec.load();
            eval.setExternalVariable(new QName("phoneNumber"), new XdmAtomicValue("1189482766"));
            StringWriter sw = new StringWriter();
            eval.setDestination(processor.newSerializer(sw));
            eval.run();
            assertEquals("<?xml version=\"1.0\" encoding=\"UTF-8\"?>1189482766", sw.toString());
        } catch (SaxonApiException | IOException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testBug7079() {
        try {
            String xsd = "<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>\n" +
                    "  <xs:element name='e'>\n" +
                    "    <xs:simpleType>\n" +
                    "      <xs:union memberTypes = 'custom xs:gYear xs:integer xs:token xs:string'/>\n" +
                    "    </xs:simpleType>\n" +
                    "  </xs:element>\n" +
                    "  <xs:simpleType name='custom'><xs:restriction base='xs:integer'>" +
                    "<xs:maxInclusive value='9999'/></xs:restriction></xs:simpleType>\n" +
                    "</xs:schema>";
            Processor processor = new Processor(true);
            SchemaManager mgr = processor.getSchemaManager();
            mgr.load(new StreamSource(new StringReader(xsd)));
            SchemaValidator val = mgr.newSchemaValidator();
            XdmDestination dest = new XdmDestination();
            val.setDestination(dest);
            val.validate(new StreamSource(new StringReader("<e>1976</e>")));
            XdmNode e = dest.getXdmNode().getOutermostElement();
            AtomicValue av = (AtomicValue) e.getTypedValue().getUnderlyingValue();
            assertEquals("custom", av.getItemType().getTypeName().getLocalPart());

            XdmDestination scm = new XdmDestination();
            mgr.exportComponents(scm);
            Processor processor2 = new Processor(true);
            SchemaManager mgr2 = processor2.getSchemaManager();
            mgr2.importComponents(scm.getXdmNode().asSource());
            SchemaValidator val2 = mgr2.newSchemaValidator();
            XdmDestination dest2 = new XdmDestination();
            val2.setDestination(dest2);
            val2.validate(new StreamSource(new StringReader("<e>1976</e>")));
            XdmNode e2 = dest2.getXdmNode().getOutermostElement();
            AtomicValue av2 = (AtomicValue) e2.getTypedValue().getUnderlyingValue();
            assertEquals("custom", av2.getItemType().getTypeName().getLocalPart());

        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

}

