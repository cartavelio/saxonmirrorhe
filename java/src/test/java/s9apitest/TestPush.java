package s9apitest;

import net.sf.saxon.s9api.*;
import net.sf.saxon.s9api.push.Document;
import net.sf.saxon.s9api.push.Element;
import org.junit.jupiter.api.Test;

import java.io.StringWriter;
import java.util.ArrayList;
import java.util.List;

import static net.sf.saxon.s9api.streams.Steps.child;
import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.fail;

/**
 * test new s9api Push interface
 */
public class TestPush {

    class Employee {
        private final String ssn;
        private final String name;

        private Employee(String s, String n) {
            ssn = s;
            name = n;
        }
    }

    private List<Employee> getData() {
        List<Employee> employees = new ArrayList<>();
        employees.add(new Employee("s22", "John Doe"));
        employees.add(new Employee("s33", "Jane Doe"));
        employees.add(new Employee("s44", "Samir Patel"));
        return employees;
    }

    @Test
    public void testSimple() {

        try {
            Processor processor = new Processor(false);
            StringWriter sw = new StringWriter();
            Serializer destination = processor.newSerializer(sw);
            destination.setOutputProperty(Serializer.Property.INDENT, "no");
            Document doc = processor.newPush(destination).document(true);
            doc.setDefaultNamespace("http://www.example.org/ns");
            Element top = doc.element("root");
            top.attribute("version", "1.5");
            for (Employee emp : getData()) {
                top.element("emp").attribute("ssn", emp.ssn)
                        .text(emp.name);
            }
            doc.close();
            assertEquals("<?xml version=\"1.0\" encoding=\"UTF-8\"?><root xmlns=\"http://www.example.org/ns\" version=\"1.5\"><emp ssn=\"s22\">John Doe</emp><emp ssn=\"s33\">Jane Doe</emp><emp ssn=\"s44\">Samir Patel</emp></root>", sw.toString(), "result");
        } catch (SaxonApiException e) {
            fail();
        }
    }

    @Test
    public void testNullHandling() {  // test null attributes and comments; and QNames
        try {
            Processor processor = new Processor(false);
            StringWriter sw = new StringWriter();
            Serializer destination = processor.newSerializer(sw);
            destination.setOutputProperty(Serializer.Property.INDENT, "no");
            Document doc = processor.newPush(destination).document(true);
            doc.setDefaultNamespace("http://www.example.org/ns");
            Element top = doc.element(new QName("top", "http://www.example.org/ns", "root"));
            top.attribute("version", "1.5");
            for (Employee emp : getData()) {
                top.element(new QName("top", "http://www.example.org/ns", "emp"))
                        .attribute("ssn", emp.ssn)
                        .attribute("other", null)
                        .text(emp.name)
                        .comment(null);
            }
            doc.close();
            assertEquals("<?xml version=\"1.0\" encoding=\"UTF-8\"?><top:root xmlns:top=\"http://www.example.org/ns\" version=\"1.5\"><top:emp ssn=\"s22\">John Doe</top:emp><top:emp ssn=\"s33\">Jane Doe</top:emp><top:emp ssn=\"s44\">Samir Patel</top:emp></top:root>", sw.toString(), "result");
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testFragment() {  // test null attributes and comments; and QNames
        try {
            Processor processor = new Processor(false);
            XdmDestination destination = new XdmDestination();
            Document doc = processor.newPush(destination).document(false);
            doc.setDefaultNamespace("http://www.example.org/ns");
            for (Employee emp : getData()) {
                doc.element(new QName("top", "http://www.example.org/ns", "emp"))
                        .attribute("ssn", emp.ssn)
                        .attribute("other", null)
                        .text(emp.name)
                        .comment(null);
            }
            doc.close();
            XdmNode root = destination.getXdmNode();
            assertEquals(3, root.select(child()).count());
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }


    @Test
    public void testIllformedError() {
        try {
            Processor processor = new Processor(false);
            XdmDestination destination = new XdmDestination();
            Document doc = processor.newPush(destination).document(true);
            for (Employee emp : getData()) {
                doc.element(new QName("emp"))
                        .attribute("ssn", emp.ssn)
                        .attribute("other", null)
                        .text(emp.name)
                        .comment(null);
            }
            doc.close();
            XdmNode root = destination.getXdmNode();
            fail("unexpected success");
        } catch (SaxonApiException e) {
            //System.err.println(e.getMessage());
            // OK
        }
    }

    @Test
    public void testAttributesFirst() {
        try {
            Processor processor = new Processor(false);
            StringWriter sw = new StringWriter();
            Serializer destination = processor.newSerializer(sw);
            destination.setOutputProperty(Serializer.Property.INDENT, "no");
            Document doc = processor.newPush(destination).document(true);
            doc.setDefaultNamespace("http://www.example.org/ns");
            Element top = doc.element("root");
            top.attribute("version", "1.5");
            for (Employee emp : getData()) {
                top.element("emp").text(emp.name).attribute("ssn", emp.ssn);
            }
            doc.close();
            fail("Unexpected success");
        } catch (SaxonApiException e) {
            System.err.println(e.getMessage());
            // OK
        }
    }

}

