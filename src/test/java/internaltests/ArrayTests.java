package internaltests;

import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.ma.arrays.ImmutableArrayItem;
import net.sf.saxon.ma.arrays.SimpleArrayItem;
import net.sf.saxon.s9api.*;
import org.junit.jupiter.api.Assertions;
import org.junit.jupiter.api.Test;

public class ArrayTests {


    @Test
    public void testSquareImplementationClass() {
        // Test "learning from experience" of the array implementation class
        try {
            Processor proc = new Processor(true);
            XPathCompiler xpc = proc.newXPathCompiler();
            xpc.declareNamespace("array", NamespaceConstant.ARRAY_FUNCTIONS);
            XPathExecutable exec = xpc.compile("[1, 2, 3, 4, 5, current-date()]");
            XdmArray result = null;
            for (int i=0; i < 50; i++) {
                result = (XdmArray) exec.load().evaluate();
            }
            Assertions.assertTrue(result.getUnderlyingValue() instanceof SimpleArrayItem);
            XPathExecutable exec2 = xpc.compile("let $x := [1, 2, 3, 4, 5, current-date()] return ($x => array:remove(1), $x)[2]");
            XdmArray result2 = null;
            for (int i = 0; i < 50; i++) {
                result2 = (XdmArray) exec2.load().evaluate();
            }
            Assertions.assertTrue(result2.getUnderlyingValue() instanceof ImmutableArrayItem);

        } catch (SaxonApiException e) {
            Assertions.fail(e);
        }
    }

    @Test
    public void testCurlyImplementationClass() {
        // Test "learning from experience" of the array implementation class
        try {
            Processor proc = new Processor(true);
            XPathCompiler xpc = proc.newXPathCompiler();
            xpc.declareNamespace("array", NamespaceConstant.ARRAY_FUNCTIONS);
            XPathExecutable exec = xpc.compile("array{1, 2, 3, 4, 5, current-date()}");
            XdmArray result = null;
            for (int i = 0; i < 50; i++) {
                result = (XdmArray) exec.load().evaluate();
            }
            Assertions.assertTrue(result.getUnderlyingValue() instanceof SimpleArrayItem);
            XPathExecutable exec2 = xpc.compile("let $x := array{1, 2, 3, 4, 5, current-date()} return ($x => array:append(10), $x)[2]");
            XdmArray result2 = null;
            for (int i = 0; i < 50; i++) {
                result2 = (XdmArray) exec2.load().evaluate();
            }
            Assertions.assertTrue(result2.getUnderlyingValue() instanceof ImmutableArrayItem);

        } catch (SaxonApiException e) {
            Assertions.fail(e);
        }
    }

}

