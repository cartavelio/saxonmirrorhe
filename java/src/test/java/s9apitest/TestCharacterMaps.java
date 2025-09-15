package s9apitest;

import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.s9api.*;
import net.sf.saxon.serialize.CharacterMap;
import net.sf.saxon.serialize.CharacterMapIndex;
import net.sf.saxon.z.IntHashMap;
import org.junit.jupiter.api.Assertions;
import org.junit.jupiter.api.Test;
import org.xml.sax.InputSource;

import javax.xml.transform.sax.SAXSource;
import java.io.ByteArrayOutputStream;
import java.io.File;
import java.io.StringReader;
import java.util.ArrayList;
import java.util.List;

import static org.junit.jupiter.api.Assertions.fail;

public class TestCharacterMaps {
    private void runTest(String method, List<QName> maps) {
        try {
            Processor processor = new Processor(false);
            DocumentBuilder builder = processor.newDocumentBuilder();

            StringReader reader = new StringReader("<doc>Hello!</doc>");
            SAXSource source = new SAXSource(new InputSource(reader));
            XdmDestination destination = new XdmDestination();
            builder.parse(source, destination);
            XdmNode node = destination.getXdmNode();

            ByteArrayOutputStream baos = new ByteArrayOutputStream();

            IntHashMap<String> cpmap = new IntHashMap<>();
            cpmap.put('!', "?");

            CharacterMapIndex index = new CharacterMapIndex();

            StringBuilder sb = new StringBuilder();
            for (QName map : maps) {
                StructuredQName name = new StructuredQName(map.getPrefix(), map.getNamespace(), map.getLocalName());
                CharacterMap cmap = new CharacterMap(name, cpmap);
                index.putCharacterMap(name, cmap);

                sb.append(map.getClarkName()).append(" ");
            }

            Serializer serializer = processor.newSerializer(baos);
            serializer.setOutputProperty(new QName("use-character-maps"), sb.toString());

            serializer.setOutputProperty(new QName("method"), method);

            serializer.setCharacterMap(index);
            serializer.serializeNode(node);

            String result = baos.toString();
            Assertions.assertTrue(result.contains("<doc>Hello?</doc>"));
        } catch (Exception ex) {
            ex.printStackTrace();
            fail();
        }

    }

    @Test
    public void testCharacterMapsXmlSingleMap() {
        List<QName> maps = new ArrayList<>();
        maps.add(new QName("", "http://example.com/", "map"));
        // With the xml method, the character map was ignored
        runTest("xml", maps);
    }

    @Test
    public void testCharacterMapsXmlMultipleMaps() {
        List<QName> maps = new ArrayList<>();
        maps.add(new QName("", "http://example.com/", "map"));
        maps.add(new QName("", "http://example.com/", "map2"));
        // With the xml method, the character map was ignored
        runTest("xml", maps);
    }

    @Test
    public void testCharacterMapsAdaptiveSingleMap() {
        List<QName> maps = new ArrayList<>();
        maps.add(new QName("", "http://example.com/", "map"));
        // With the adaptive method, the attempt threw an XPathException
        runTest("adaptive", maps);
    }

    @Test
    public void testCharacterMapsAdaptiveMultipleMaps() {
        List<QName> maps = new ArrayList<>();
        maps.add(new QName("", "http://example.com/", "map"));
        maps.add(new QName("", "http://example.com/", "map2"));
        // With the adaptive method, the attempt threw an XPathException
        runTest("adaptive", maps);
    }

    @Test
    public void testXsltCharacterMapsXml() {
        try {
            Processor processor = new Processor(false);
            DocumentBuilder builder = processor.newDocumentBuilder();

            File sample = new File("src/test/resources/sample.xsl");
            XdmDestination destination = new XdmDestination();
            builder.parse(sample, destination);
            XdmNode node = destination.getXdmNode();

            XsltCompiler compiler = processor.newXsltCompiler();
            XsltExecutable exec = compiler.compile(sample);
            Xslt30Transformer transformer = exec.load30();

            ByteArrayOutputStream baos = new ByteArrayOutputStream();

            Serializer serializer = processor.newSerializer(baos);

            transformer.setGlobalContextItem(node);
            transformer.applyTemplates(node, serializer);

            Assertions.assertTrue(baos.toString().contains(">Hello, world?</doc>"));
        } catch (Exception ex) {
            ex.printStackTrace();
            fail();
        }
    }

    @Test
    public void testXsltCharacterMapsAdaptive() {
        try {
            Processor processor = new Processor(false);
            DocumentBuilder builder = processor.newDocumentBuilder();

            File sample = new File("src/test/resources/sample-two.xsl");
            XdmDestination destination = new XdmDestination();
            builder.parse(sample, destination);
            XdmNode node = destination.getXdmNode();

            XsltCompiler compiler = processor.newXsltCompiler();
            XsltExecutable exec = compiler.compile(sample);
            Xslt30Transformer transformer = exec.load30();

            ByteArrayOutputStream baos = new ByteArrayOutputStream();

            Serializer serializer = processor.newSerializer(baos);

            transformer.setGlobalContextItem(node);
            transformer.applyTemplates(node, serializer);

            Assertions.assertTrue(baos.toString().contains(">Hello, world?</doc>"));
            Assertions.assertTrue(baos.toString().contains("\"greeting\":\"Hello, world?\"}"));
        } catch (Exception ex) {
            ex.printStackTrace();
            fail();
        }
    }

    @Test
    public void testXsltCharacterMapsWitCustomSerializer() {
        try {
            Processor processor = new Processor(false);
            DocumentBuilder builder = processor.newDocumentBuilder();

            File sample = new File("src/test/resources/sample.xsl");
            XdmDestination destination = new XdmDestination();
            builder.parse(sample, destination);
            XdmNode node = destination.getXdmNode();

            XsltCompiler compiler = processor.newXsltCompiler();
            XsltExecutable exec = compiler.compile(sample);
            Xslt30Transformer transformer = exec.load30();

            ByteArrayOutputStream baos = new ByteArrayOutputStream();

            Serializer serializer = processor.newSerializer(baos);
            serializer.setOutputProperty(new QName("use-character-maps"), "Q{http://example.com/ns/examples}map");

            serializer.setOutputProperty(new QName("method"), "adaptive");

            IntHashMap<String> cpmap = new IntHashMap<>();
            cpmap.put('!', "?");

            StructuredQName name = new StructuredQName("", "http://example.com/ns/examples", "map");
            CharacterMap cmap = new CharacterMap(name, cpmap);

            CharacterMapIndex index = new CharacterMapIndex();
            index.putCharacterMap(name, cmap);

            serializer.setCharacterMap(index);

            transformer.setGlobalContextItem(node);
            transformer.applyTemplates(node, serializer);

            Assertions.assertTrue(baos.toString().contains(">Hello, world?</doc>"));
        } catch (Exception ex) {
            ex.printStackTrace();
            fail();
        }
    }
}
