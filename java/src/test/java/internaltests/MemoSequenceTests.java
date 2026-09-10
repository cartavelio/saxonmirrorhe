package internaltests;

import net.sf.saxon.expr.AscendingRangeIterator;
import net.sf.saxon.om.MemoSequence;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.om.SequenceTool;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.GroundedIterator;
import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertNull;
import static org.junit.jupiter.api.Assertions.fail;

public class MemoSequenceTests {

    @Test
    public void testItemAt() {
        try {
            SequenceIterator iter = new AscendingRangeIterator(10, 1, 20);
            MemoSequence ms = new MemoSequence(iter);
            assertEquals("15", ms.itemAt(5).getStringValue(), "itemAtA");
            SequenceIterator prog = ms.iterate();
            assertEquals("10", prog.next().getStringValue(), "next1");
            assertEquals("11", prog.next().getStringValue(), "next2");
            assertEquals("12", prog.next().getStringValue(), "next3");
            assertEquals("15", ms.itemAt(5).getStringValue(), "itemAtZ");
        } catch (XPathException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testMaterialize() {
        try {
            SequenceIterator iter = new AscendingRangeIterator(10, 1, 20);
            MemoSequence ms = new MemoSequence(iter);
            SequenceIterator prog = ms.iterate();

            assertEquals("10 11 12 13 14 15 16 17 18 19 20", SequenceTool.toGroundedValue(prog).getStringValue(), "materializeA");
            assertEquals("10", prog.next().getStringValue(), "next1");
            assertEquals("11", prog.next().getStringValue(), "next2");
            assertEquals("12", prog.next().getStringValue(), "next3");
            assertEquals("10 11 12 13 14 15 16 17 18 19 20", SequenceTool.toGroundedValue(prog).getStringValue(), "materializeB");
            assertEquals("13", prog.next().getStringValue(), "next4");
            assertEquals("14", prog.next().getStringValue(), "next5");
            assertEquals("15", prog.next().getStringValue(), "next6");
            assertEquals("16", prog.next().getStringValue(), "next7");
            assertEquals("17", prog.next().getStringValue(), "next8");
            assertEquals("18", prog.next().getStringValue(), "next9");
            assertEquals("19", prog.next().getStringValue(), "next10");
            assertEquals("20", prog.next().getStringValue(), "next11");
            assertNull(prog.next(), "next12");
            assertEquals("10 11 12 13 14 15 16 17 18 19 20", SequenceTool.toGroundedValue(((GroundedIterator) prog)).getStringValue(), "materializeC");

        } catch (XPathException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testMaterialize2() {
        try {
            SequenceIterator iter = new AscendingRangeIterator(10, 1, 20);
            MemoSequence ms = new MemoSequence(iter);
            SequenceIterator prog = ms.iterate();

            assertEquals("10", prog.next().getStringValue(), "next1");
            assertEquals("11", prog.next().getStringValue(), "next2");
            assertEquals("12", prog.next().getStringValue(), "next3");
            assertEquals("10 11 12 13 14 15 16 17 18 19 20", SequenceTool.toGroundedValue(((GroundedIterator) prog)).getStringValue(), "materializeB");
            assertEquals("13", prog.next().getStringValue(), "next4");
            assertEquals("14", prog.next().getStringValue(), "next5");
            assertEquals("15", prog.next().getStringValue(), "next6");
            assertEquals("16", prog.next().getStringValue(), "next7");
            assertEquals("17", prog.next().getStringValue(), "next8");
            assertEquals("18", prog.next().getStringValue(), "next9");
            assertEquals("19", prog.next().getStringValue(), "next10");
            assertEquals("20", prog.next().getStringValue(), "next11");
            assertNull(prog.next(), "next12");
            assertEquals("10 11 12 13 14 15 16 17 18 19 20", SequenceTool.toGroundedValue(((GroundedIterator) prog)).getStringValue(), "materializeC");

        } catch (XPathException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testResidue() {
        try {
            SequenceIterator iter = new AscendingRangeIterator(10, 1, 20);
            MemoSequence ms = new MemoSequence(iter);
            SequenceIterator prog = ms.iterate();

            assertEquals("10 11 12 13 14 15 16 17 18 19 20", ((GroundedIterator) prog).getResidue().getStringValue(), "residueA");
            assertEquals("10", prog.next().getStringValue(), "next1");
            assertEquals("11", prog.next().getStringValue(), "next2");
            assertEquals("12", prog.next().getStringValue(), "next3");
            assertEquals("13 14 15 16 17 18 19 20", ((GroundedIterator) prog).getResidue().getStringValue(), "residueB");
            assertEquals("13", prog.next().getStringValue(), "next4");
            assertEquals("14", prog.next().getStringValue(), "next5");
            assertEquals("15", prog.next().getStringValue(), "next6");
            assertEquals("16", prog.next().getStringValue(), "next7");
            assertEquals("17", prog.next().getStringValue(), "next8");
            assertEquals("18", prog.next().getStringValue(), "next9");
            assertEquals("19", prog.next().getStringValue(), "next10");
            assertEquals("20", prog.next().getStringValue(), "next11");
            assertNull(prog.next(), "next12");
            assertEquals("", ((GroundedIterator) prog).getResidue().getStringValue(), "residueC");

        } catch (XPathException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testResidue2() {
        try {
            SequenceIterator iter = new AscendingRangeIterator(10, 1, 20);
            MemoSequence ms = new MemoSequence(iter);
            SequenceIterator prog = ms.iterate();

            assertEquals("10", prog.next().getStringValue(), "next1");
            assertEquals("11", prog.next().getStringValue(), "next2");
            assertEquals("12", prog.next().getStringValue(), "next3");
            assertEquals("13 14 15 16 17 18 19 20", ((GroundedIterator) prog).getResidue().getStringValue(), "residueB");
            assertEquals("13", prog.next().getStringValue(), "next4");
            assertEquals("14", prog.next().getStringValue(), "next5");
            assertEquals("15", prog.next().getStringValue(), "next6");
            assertEquals("16", prog.next().getStringValue(), "next7");
            assertEquals("17", prog.next().getStringValue(), "next8");
            assertEquals("18", prog.next().getStringValue(), "next9");
            assertEquals("19", prog.next().getStringValue(), "next10");
            assertEquals("20", prog.next().getStringValue(), "next11");
            assertNull(prog.next(), "next12");
            assertEquals("", ((GroundedIterator) prog).getResidue().getStringValue(), "residueC");

        } catch (XPathException e) {
            e.printStackTrace();
            fail();
        }
    }
}


