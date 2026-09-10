package s9apitest;

import net.sf.saxon.s9api.*;
import org.junit.jupiter.api.Test;

import javax.xml.transform.stream.StreamSource;
import java.io.StringReader;
import java.util.List;

import static net.sf.saxon.s9api.streams.Steps.atomize;
import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;

/**
 * Test XdmArray class, representing an XPath map item
 */
public class TestXdmArray {

    @Test
    public void testArrayOperations() {
        XdmArray a = new XdmArray();
        assertEquals(0, a.arrayLength());
        assertEquals(1, a.size());

        a = new XdmArray(new XdmValue[]{new XdmAtomicValue(1), new XdmAtomicValue(2), new XdmAtomicValue(3)});
        assertEquals(3, a.arrayLength());
        assertEquals(1, a.size());
        try {
            assertEquals(2, ((XdmAtomicValue) a.get(1)).getLongValue());
        } catch (SaxonApiException e) {
            fail();
        }
        a = a.put(1, new XdmAtomicValue(17));
        try {
            assertEquals(17, ((XdmAtomicValue) a.get(1)).getLongValue());
        } catch (SaxonApiException e) {
            fail();
        }

        List<XdmValue> values = a.asList();
        try {
            assertEquals(17, ((XdmAtomicValue) values.get(1)).getLongValue());
        } catch (SaxonApiException e) {
            fail();
        }

    }


    @Test
    public void testArrayAsFunction() {
        XdmArray a = new XdmArray(new XdmValue[]{new XdmAtomicValue(1), new XdmAtomicValue(2), new XdmAtomicValue(3)});
        Processor proc = new Processor(true);
        try {
            XdmValue result = a.call(proc, new XdmAtomicValue(1));  // 1-based indexing here
            assertTrue(result instanceof XdmAtomicValue);
            assertTrue(((XdmAtomicValue)result).getLongValue() == 1);
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testArrayAsQueryParameter() {
        try {
            XdmArray a = new XdmArray(new XdmValue[]{new XdmAtomicValue(1), new XdmAtomicValue(2), new XdmAtomicValue(3)});
            Processor proc = new Processor(true);
            String query =
                    "declare namespace a='http://www.w3.org/2005/xpath-functions/array';" +
                    "declare variable $a as array(*) external; " +
                    "a:size($a)";
            XQueryCompiler comp = proc.newXQueryCompiler();
            XQueryEvaluator q = comp.compile(query).load();
            q.setExternalVariable(new QName("a"), a);
            XdmAtomicValue result = (XdmAtomicValue)q.evaluateSingle();
            assertEquals(3, result.getLongValue());
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testArrayAsQueryResult() {
        try {
            Processor proc = new Processor(true);
            String query = "[1, 2, 3]";
            XQueryCompiler comp = proc.newXQueryCompiler();
            XQueryEvaluator q = comp.compile(query).load();
            XdmArray result = (XdmArray) q.evaluateSingle();
            assertEquals(3, result.arrayLength());
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testArrayConvertIntegers() {
        int[] in = new int[]{5,6,7,8};
        XdmArray result = XdmArray.makeArray(in);
        assertEquals(4, result.arrayLength());
        assertEquals(new XdmAtomicValue(5), result.get(0));
        assertTrue(ItemType.INT.matches((XdmAtomicValue)result.get(3)));
    }

    @Test
    public void testArrayConvertLongs() {
        long[] in = new long[]{5, 6, 7, 8};
        XdmArray result = XdmArray.makeArray(in);
        assertEquals(4, result.arrayLength());
        assertEquals(new XdmAtomicValue(5), result.get(0));
        assertTrue(ItemType.LONG.matches((XdmAtomicValue) result.get(3)));
    }

    @Test
    public void testArrayConvertStrings() {
        String[] in = new String[]{"a", "b", "c", "d"};
        XdmArray result = XdmArray.makeArray(in);
        assertEquals(4, result.arrayLength());
        assertEquals(new XdmAtomicValue("a"), result.get(0));
        assertTrue(ItemType.ANY_ARRAY.matches(result));
        assertTrue(ItemType.STRING.matches((XdmAtomicValue) result.get(3)));
    }

    @Test
    public void testArrayConvertBooleans() {
        boolean[] in = new boolean[]{true, true, false};
        XdmArray result = XdmArray.makeArray(in);
        assertEquals(3, result.arrayLength());
        assertEquals(XdmValue.makeValue(true), result.get(0));
        assertTrue(ItemType.BOOLEAN.matches((XdmAtomicValue) result.get(2)));
    }

    @Test
    public void testArrayConvertArrays() {
        String[] in0 = new String[]{"a", "b", "c", "d"};
        String[] in1 = new String[]{"w", "x", "y", "z"};
        XdmArray result = XdmArray.makeArray(new String[][]{in0, in1});
        assertEquals(2, result.arrayLength());
        assertEquals(new XdmAtomicValue("a"), ((XdmArray)result.get(0)).get(0));
        assertTrue(ItemType.ANY_ARRAY.matches((XdmItem)result.get(1)));
        assertTrue(ItemType.STRING.matches((XdmItem)((XdmArray) result.get(1)).get(2)));
        Processor proc = new Processor(true);
        ItemTypeFactory factory = new ItemTypeFactory(proc);
        ItemType arrayOfString = factory.getArrayType(SequenceType.makeSequenceType(ItemType.STRING, OccurrenceIndicator.ONE));
        ItemType arrayOfArray = factory.getArrayType(SequenceType.makeSequenceType(arrayOfString, OccurrenceIndicator.ONE));
        assertTrue(arrayOfArray.matches(result));
    }

    @Test
    public void testAtomizeArrayStep() {
        try {
            Processor proc = new Processor(true);
            DocumentBuilder builder = proc.newDocumentBuilder();
            XdmNode e1 = builder.build(new StreamSource(new StringReader("<foo>footext</foo>")));
            XdmNode e2 = builder.build(new StreamSource(new StringReader("<bar>bartext</bar>")));
            XdmArray nodes = XdmArray.makeArray(new XdmNode[]{e1, e2});
            List<XdmAtomicValue> data = nodes.select(atomize()).asList();
            assertEquals(2, data.size());
            assertEquals("footext", data.get(0).getStringValue());
            assertEquals("bartext", data.get(1).getStringValue());
        } catch (SaxonApiException e) {
            e.printStackTrace();
        }
    }

    @Test
    public void testArrayToString() {
        XdmArray a = new XdmArray(new XdmValue[]{new XdmAtomicValue(1), new XdmAtomicValue(2), new XdmAtomicValue("x")});
        String s = a.toString().replace(" ","").replace("\n","");
        assertEquals("[1,2,\"x\"]", s);
    }
}




