package internaltests;

import junit.framework.TestCase;
import net.sf.saxon.expr.RangeIterator;
import net.sf.saxon.om.MemoSequence;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.GroundedIterator;
import net.sf.saxon.value.AtomicValue;

public class MemoSequenceTests extends TestCase {

    public void testItemAt() {
        try {
            SequenceIterator<AtomicValue> iter = new RangeIterator(10, 20);
            MemoSequence<AtomicValue> ms = new MemoSequence<>(iter);
            assertEquals("itemAtA", "15", ms.itemAt(5).getStringValue());
            SequenceIterator prog = ms.iterate();
            assertEquals("next1", "10", prog.next().getStringValue());
            assertEquals("next2", "11", prog.next().getStringValue());
            assertEquals("next3", "12", prog.next().getStringValue());
            assertEquals("itemAtZ", "15", ms.itemAt(5).getStringValue());
        } catch (XPathException e) {
            e.printStackTrace();
            fail();
        }
    }

    public void testMaterialize() {
        try {
            SequenceIterator<AtomicValue> iter = new RangeIterator(10, 20);
            MemoSequence<AtomicValue> ms = new MemoSequence<>(iter);
            SequenceIterator prog = ms.iterate();

            assertEquals("materializeA", "10 11 12 13 14 15 16 17 18 19 20", ((GroundedIterator) prog).materialize().getStringValue());
            assertEquals("next1", "10", prog.next().getStringValue());
            assertEquals("next2", "11", prog.next().getStringValue());
            assertEquals("next3", "12", prog.next().getStringValue());
            assertEquals("materializeB", "10 11 12 13 14 15 16 17 18 19 20", ((GroundedIterator) prog).materialize().getStringValue());
            assertEquals("next4", "13", prog.next().getStringValue());
            assertEquals("next5", "14", prog.next().getStringValue());
            assertEquals("next6", "15", prog.next().getStringValue());
            assertEquals("next7", "16", prog.next().getStringValue());
            assertEquals("next8", "17", prog.next().getStringValue());
            assertEquals("next9", "18", prog.next().getStringValue());
            assertEquals("next10", "19", prog.next().getStringValue());
            assertEquals("next11", "20", prog.next().getStringValue());
            assertNull("next12", prog.next());
            assertEquals("materializeC", "10 11 12 13 14 15 16 17 18 19 20", ((GroundedIterator) prog).materialize().getStringValue());

        } catch (XPathException e) {
            e.printStackTrace();
            fail();
        }
    }

    public void testMaterialize2() {
        try {
            SequenceIterator<AtomicValue> iter = new RangeIterator(10, 20);
            MemoSequence<AtomicValue> ms = new MemoSequence<>(iter);
            SequenceIterator prog = ms.iterate();

            assertEquals("next1", "10", prog.next().getStringValue());
            assertEquals("next2", "11", prog.next().getStringValue());
            assertEquals("next3", "12", prog.next().getStringValue());
            assertEquals("materializeB", "10 11 12 13 14 15 16 17 18 19 20", ((GroundedIterator) prog).materialize().getStringValue());
            assertEquals("next4", "13", prog.next().getStringValue());
            assertEquals("next5", "14", prog.next().getStringValue());
            assertEquals("next6", "15", prog.next().getStringValue());
            assertEquals("next7", "16", prog.next().getStringValue());
            assertEquals("next8", "17", prog.next().getStringValue());
            assertEquals("next9", "18", prog.next().getStringValue());
            assertEquals("next10", "19", prog.next().getStringValue());
            assertEquals("next11", "20", prog.next().getStringValue());
            assertNull("next12", prog.next());
            assertEquals("materializeC", "10 11 12 13 14 15 16 17 18 19 20", ((GroundedIterator) prog).materialize().getStringValue());

        } catch (XPathException e) {
            e.printStackTrace();
            fail();
        }
    }

    public void testResidue() {
        try {
            SequenceIterator iter = new RangeIterator(10, 20);
            MemoSequence ms = new MemoSequence(iter);
            SequenceIterator prog = ms.iterate();

            assertEquals("residueA", "10 11 12 13 14 15 16 17 18 19 20", ((GroundedIterator) prog).getResidue().getStringValue());
            assertEquals("next1", "10", prog.next().getStringValue());
            assertEquals("next2", "11", prog.next().getStringValue());
            assertEquals("next3", "12", prog.next().getStringValue());
            assertEquals("residueB", "13 14 15 16 17 18 19 20", ((GroundedIterator) prog).getResidue().getStringValue());
            assertEquals("next4", "13", prog.next().getStringValue());
            assertEquals("next5", "14", prog.next().getStringValue());
            assertEquals("next6", "15", prog.next().getStringValue());
            assertEquals("next7", "16", prog.next().getStringValue());
            assertEquals("next8", "17", prog.next().getStringValue());
            assertEquals("next9", "18", prog.next().getStringValue());
            assertEquals("next10", "19", prog.next().getStringValue());
            assertEquals("next11", "20", prog.next().getStringValue());
            assertNull("next12", prog.next());
            assertEquals("residueC", "", ((GroundedIterator) prog).getResidue().getStringValue());

        } catch (XPathException e) {
            e.printStackTrace();
            fail();
        }
    }

    public void testResidue2() {
        try {
            SequenceIterator iter = new RangeIterator(10, 20);
            MemoSequence ms = new MemoSequence(iter);
            SequenceIterator prog = ms.iterate();

            assertEquals("next1", "10", prog.next().getStringValue());
            assertEquals("next2", "11", prog.next().getStringValue());
            assertEquals("next3", "12", prog.next().getStringValue());
            assertEquals("residueB", "13 14 15 16 17 18 19 20", ((GroundedIterator) prog).getResidue().getStringValue());
            assertEquals("next4", "13", prog.next().getStringValue());
            assertEquals("next5", "14", prog.next().getStringValue());
            assertEquals("next6", "15", prog.next().getStringValue());
            assertEquals("next7", "16", prog.next().getStringValue());
            assertEquals("next8", "17", prog.next().getStringValue());
            assertEquals("next9", "18", prog.next().getStringValue());
            assertEquals("next10", "19", prog.next().getStringValue());
            assertEquals("next11", "20", prog.next().getStringValue());
            assertNull("next12", prog.next());
            assertEquals("residueC", "", ((GroundedIterator) prog).getResidue().getStringValue());

        } catch (XPathException e) {
            e.printStackTrace();
            fail();
        }
    }
}


