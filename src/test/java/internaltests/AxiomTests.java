package internaltests;

import junit.framework.TestCase;
import org.apache.axiom.om.OMAbstractFactory;
import org.apache.axiom.om.OMDocument;
import org.apache.axiom.om.OMElement;
import org.apache.axiom.om.OMFactory;

import java.util.Iterator;

/**
 * Created by mike on 07/12/15.
 */
public class AxiomTests extends TestCase {

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
        assertEquals("Two nodes", 2, count);
        System.err.println(count);
    }
}


