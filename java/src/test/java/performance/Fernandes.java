package performance;

import net.sf.saxon.junit.Testdata;
import net.sf.saxon.s9api.*;
import org.junit.jupiter.api.Tag;
import org.junit.jupiter.api.Test;

import javax.xml.transform.stream.StreamSource;
import java.io.File;
import java.util.ArrayList;
import java.util.List;

import static org.junit.jupiter.api.Assertions.fail;

public class Fernandes extends PerformanceTestCase {

    @Test
    @Tag("performance")
    public void testFernandesPerf() {
        try {
            Processor proc = new Processor(false);
            XsltCompiler comp = proc.newXsltCompiler();
            XsltExecutable t1 = comp.compile(new StreamSource(Testdata.file("fernandes/t1.xsl")));
            XsltExecutable t2 = comp.compile(new StreamSource(Testdata.file("fernandes/t2.xsl")));
            final XdmNode initial = proc.newDocumentBuilder().build(Testdata.file("fernandes/test.xml"));
            final List<XdmNode> wrapper = new ArrayList<>();
            wrapper.add(initial);
            measure(new Measurable() {
                @Override
                public void run() {
                    try {
                        XdmNode xml = wrapper.get(0);
                        XdmDestination dest1 = new XdmDestination();
                        t1.load30().applyTemplates(xml, dest1);
                        XdmDestination dest2 = new XdmDestination();
                        t2.load30().applyTemplates(dest1.getXdmNode(), dest2);
                        wrapper.set(0, dest2.getXdmNode());
                    } catch (SaxonApiException e) {
                        fail();
                    }
                }

                @Override
                public int expectedTimeMicroseconds() {
                    return 7500;
                }

                @Override
                public String name() {
                    return "Fernandes";
                }
            });

        } catch (Exception err) {
            err.printStackTrace();
            fail();
        }
    }

}

