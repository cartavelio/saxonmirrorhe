package bugs;


import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.ee.config.StandardSchemaResolver;
import net.sf.saxon.s9api.*;
import net.sf.saxon.s9api.streams.Steps;
import net.sf.saxon.trace.Instrumentation;
import net.sf.saxon.trans.XPathException;
import org.junit.Ignore;
import org.junit.jupiter.api.Assertions;

import javax.xml.transform.Source;
import javax.xml.transform.stream.StreamSource;
import java.io.File;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

public class Bug6547 {

    private final static String xsdFiles =
            "/Users/mike/bugs/2024/6547-Walters/kvk-groot-xsd/META-INF/xsd-files.xml";
    private final static String catalog =
            "file:/Users/mike/bugs/2024/6547-Walters/kvk-groot-xsd/META-INF/catalog2.xml";

    @Ignore("Depends on MHK filestore")
    public void bug0() {
        try {
            Processor processor = new Processor(true);
            DocumentBuilder doc = processor.newDocumentBuilder();
            XdmNode xsdFileList = doc.build(new File(xsdFiles));
            Map<String, String> locationIndex = new HashMap<>(50);
            xsdFileList.select(Steps.descendant("s")).forEach(
                    node -> locationIndex.put(node.attribute("ns"), node.attribute("loc").substring(5)));

            SchemaManager sm = processor.getSchemaManager();
            sm.setSchemaURIResolver(new StandardSchemaResolver(
                    (EnterpriseConfiguration) processor.getUnderlyingConfiguration()) {
                @Override
                public Source[] resolve(String targetNamespace, String baseURI, String[] locations) throws XPathException {
                    //System.err.println("Resolving " + targetNamespace);
                    String loc = locationIndex.get(targetNamespace);
                    if (loc == null) {
                        System.err.println("Cannot resolve " + targetNamespace);
                        return new Source[0];
                    }
                    Source[] sources = new Source[1];
                    sources[0] = new StreamSource(new File(locationIndex.get(targetNamespace)));
                    return sources;
                }
            });
            Instrumentation.reset();
            long start = System.currentTimeMillis();
            for (String xsdDoc : locationIndex.values()) {
                if (!xsdDoc.contains("xbrl-specification-2024-02-28") && !xsdDoc.contains("xbrl-syntax-extension")) {
                    //System.err.println("Loading " + xsdDoc);
                    sm.load(new File(xsdDoc));
                }
            }
            long end = System.currentTimeMillis();
            System.err.println((end - start) + " milliseconds");
            Instrumentation.report();

        } catch (Exception ex) {
            Assertions.fail(ex);
        }
    }

    @Ignore("Depends on MHK filestore")
    public void bug1() {
        try {
            Processor processor = new Processor(true);
            DocumentBuilder doc = processor.newDocumentBuilder();
            XdmNode xsdFileList = doc.build(new File(xsdFiles));
            Map<String, String> locationIndex = new HashMap<>(50);
            xsdFileList.select(Steps.descendant("s")).forEach(
                    node -> locationIndex.put(node.attribute("ns"), node.attribute("loc").substring(5)));

            XsdCompiler sm = processor.newXsdCompiler();
            sm.setSchemaURIResolver(new StandardSchemaResolver(
                    (EnterpriseConfiguration) processor.getUnderlyingConfiguration()) {
                @Override
                public Source[] resolve(String targetNamespace, String baseURI, String[] locations) throws XPathException {
                    //System.err.println("Resolving " + targetNamespace);
                    String loc = locationIndex.get(targetNamespace);
                    if (loc == null) {
                        System.err.println("Cannot resolve " + targetNamespace);
                        return new Source[0];
                    }
                    Source[] sources = new Source[1];
                    sources[0] = new StreamSource(new File(locationIndex.get(targetNamespace)));
                    return sources;
                }
            });
            Instrumentation.reset();
            long start = System.currentTimeMillis();
            List<File> files = new ArrayList<>();
            for (String xsdDoc : locationIndex.values()) {
                if (!xsdDoc.contains("xbrl-specification-2024-02-28") && !xsdDoc.contains("xbrl-syntax-extension")) {
                    //System.err.println("Loading " + xsdDoc);
                    files.add(new File(xsdDoc));
                }
            }
            sm.compile(files.toArray(new File[0]));
            long end = System.currentTimeMillis();
            System.err.println((end - start) + " milliseconds");
            Instrumentation.report();

        } catch (Exception ex) {
            Assertions.fail(ex);
        }
    }
}



