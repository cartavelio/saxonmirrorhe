package s9apitest;

import net.sf.saxon.Configuration;
import net.sf.saxon.dom.NodeOverNodeInfo;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.One;
import net.sf.saxon.om.OneOrMore;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.ListIterator;
import net.sf.saxon.value.BigDecimalValue;
import net.sf.saxon.value.DecimalValue;
import net.sf.saxon.value.DoubleValue;
import net.sf.saxon.value.IntegerValue;
import org.w3c.dom.Node;

import javax.xml.transform.stream.StreamSource;
import java.io.StringReader;
import java.math.BigDecimal;
import java.time.*;
import java.time.temporal.ChronoField;
import java.util.*;


public class TestReflexionHelper {

    int intValue;
    String stringValue;

    public TestReflexionHelper() {
        // instantiated by extension function calls
    }

    public TestReflexionHelper(int i) {
        this.intValue = i;
    }

    public TestReflexionHelper(String s) {
        this.stringValue = s;
    }

    public int getIntValue() {
        return intValue;
    }

    public String getStringValue() {
        return stringValue;
    }

    public void setIntValue(int i) {
        this.intValue = i;
    }

    public void setStringValue(String s) {
        this.stringValue = s;
    }

    public static String process(String x) {
        return "String " + x;
    }

    public static String process(byte[] x) {
        return "byte[] " + x;
    }

    public static byte[] toEvenByteArray(byte[] x) {
        for (int i=0; i<x.length; i++) {
            x[i] = (byte)(x[i] &~ (byte)1);
        }
        return x;
    }

    public static String processDouble(Double x) {
        return "Double " + x;
    }

    public static String processInt(int x) {
        return "int " + x;
    }

    public static String processDoubleValue(DoubleValue x) {
        return "DoubleValue " + x;
    }

    public static String processIntegerValue(IntegerValue x) {
        return "IntegerValue " + x;
    }

    public static List<String> getList() {
        final ArrayList<String> theList = new ArrayList<String>();
        theList.add("The Wonderful Wizard of Oz");
        theList.add("Encyclopedia Brown");
        return theList;
    }

    public static List<String> getEmptyList() {
        return new ArrayList<String>();
    }

    public static String maxi(String label, DoubleValue... doubles) {
        double max = Double.MIN_VALUE;
        for (DoubleValue val : doubles) {
            max = val.getDoubleValue() > max ? val.getDoubleValue() : max;
        }
        return label + ": " + max;
    }

    public static String maxi2(String label, DoubleValue... doubles) {
        double max = Double.MIN_VALUE;
        for (DoubleValue val : doubles) {
            max = val.getDoubleValue() > max ? val.getDoubleValue() : max;
        }
        return label + " (s) : " + max;
    }

    public static String maxi2(java.net.URI label, DoubleValue... doubles) {
        double max = Double.MIN_VALUE;
        for (DoubleValue val : doubles) {
            max = val.getDoubleValue() > max ? val.getDoubleValue() : max;
        }
        return label + " (u) : " + max;
    }

    public static NodeInfo makeDocGood(XPathContext context) throws XPathException {
        return context.getConfiguration().buildDocumentTree(new StreamSource(new StringReader("<doc/>"))).getRootNode();
    }

    public static NodeInfo makeDocBad() throws XPathException {
        return new Configuration().buildDocumentTree(new StreamSource(new StringReader("<doc/>"))).getRootNode();
    }

    public static SequenceIterator makeDocsGood(XPathContext context) throws XPathException {
        Configuration config = context.getConfiguration();
        List<NodeInfo> list = new ArrayList<>();
        list.add(config.buildDocumentTree(new StreamSource(new StringReader("<doc1/>"))).getRootNode());
        list.add(config.buildDocumentTree(new StreamSource(new StringReader("<doc2/>"))).getRootNode());
        return new ListIterator.Of<>(list);
    }

    public static SequenceIterator makeDocsBad() throws XPathException {
        Configuration config = new Configuration();
        List<NodeInfo> list = new ArrayList<>();
        list.add(config.buildDocumentTree(new StreamSource(new StringReader("<doc1/>"))).getRootNode());
        list.add(config.buildDocumentTree(new StreamSource(new StringReader("<doc2/>"))).getRootNode());
        return new ListIterator.Of<>(list);
    }

    public static One addDecimals(OneOrMore<DecimalValue> values) throws XPathException {
        BigDecimal total = BigDecimal.ZERO;
        SequenceIterator iter = values.iterate();
        DecimalValue val;
        while ((val = (DecimalValue) iter.next()) != null) {
            total = total.add(val.getDecimalValue());
        }
        return new One(new BigDecimalValue(total));
    }

    public static boolean is2008d(Date value) {
        //System.err.println("YEAR=" + value.getYear());
        Calendar cal = Calendar.getInstance();
        cal.setTime(value);
        return cal.get(Calendar.YEAR) == 2008;
    }

    public static boolean is2008i(Instant value) {
        //System.err.println("YEAR=" + value.getYear());
        return value.atOffset(ZoneOffset.UTC).getLong(ChronoField.YEAR) == 2008;
    }

    public static boolean is2008z(ZonedDateTime value) {
        //System.err.println("YEAR=" + value.getYear());
        return value.getYear() == 2008;
    }

    public static boolean is2008localDateTime(LocalDateTime value) {
        return value.getYear() == 2008;
    }

    public static boolean is2008localDate(LocalDate value) {
        return value.getYear() == 2008;
    }

    public static int mapSize(Map<?, ?> map) {
        return map.size();
    }

    public static boolean isDomNode(Node supplied) {
        return supplied instanceof NodeOverNodeInfo
                && supplied.getNodeName().equals("a")
                && supplied.getAttributes().getNamedItem("id").getNodeValue().equals("23");
    }




}

