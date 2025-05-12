package internaltests;

import org.apache.axiom.om.OMAbstractFactory;
import org.apache.axiom.om.OMDocument;
import org.apache.axiom.om.OMElement;
import org.apache.axiom.om.OMFactory;
import org.junit.jupiter.api.Test;

import java.util.Iterator;

import static org.junit.jupiter.api.Assertions.assertEquals;

/**
 * Tests for the Axiom object model
 */
public class AxiomTests {

    @Test
    public void testDescendantAxis() {

        OMFactory factory = OMAbstractFactory.getOMFactory();
        OMDocument document = factory.createOMDocument();
        OMElement element = factory.createOMElement("test", null);
        document.setOMDocumentElement(element);
        Iterator iter = document.getDescendants(true);
        int count = 0;
        while (iter.hasNext()) {
            Object o = iter.next();
            count++;
        }
        assertEquals(2, count, "Two nodes");
        System.err.println(count);
    }
}


