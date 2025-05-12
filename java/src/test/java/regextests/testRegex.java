package regextests;

import jaxptest.ConfigTest;
import net.sf.saxon.s9api.*;
import net.sf.saxon.z.IntIterator;
import net.sf.saxon.z.IntRangeSet;
import org.junit.jupiter.api.Assertions;
import org.junit.jupiter.api.Test;

import java.io.File;
import java.io.FileWriter;
import java.io.IOException;
import java.io.PrintWriter;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.TimeUnit;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;

/**
 *
 */
public class testRegex {
    private ConfigTest configTest = new ConfigTest();

    @Test
    public void testCharacterClasses() {

        PrintWriter out;
        try {
            out = new PrintWriter(new FileWriter(new File(configTest.getTempDir() + "JavaSubCategories.xml")));
        } catch (IOException e) {
            e.printStackTrace();
            return;
        }
        // output a table showing which characters belong to which classes
        String[] classes = {"C", "Cc", "Cf", "Co",
                    "L", "Ll", "Lm", "Lo", "Lt", "Lu",
                    "M", "Mc", "Me", "Mn",
                    "N", "Nd", "Nl", "No",
                    "P", "Pc", "Pd", "Pe", "Pf", "Pi", "Po", "Ps",
                    "S", "Sc", "Sk", "Sm", "So",
                    "Z", "Zl", "Zp", "Zs",
                    "Cn"};
        IntRangeSet chars = new IntRangeSet();
        chars.addRange(9, 10);
        chars.add(13);
        chars.addRange(32, 55295);
        chars.addRange(57344, 65533);
        chars.addRange(65536, 1114111);

        out.println("<categories>");

        for (int c=0; c<classes.length; c++) {
            System.err.println(classes[c]);
            if (classes[c].length() == 1) {
                continue;
            }
            IntRangeSet result = new IntRangeSet();
            Pattern pat = Pattern.compile("\\p{" + classes[c] + "}");

            for (IntIterator ii = chars.iterator(); ii.hasNext();) {
                int[] ch = new int[]{ii.next()};
                String s = new String(ch, 0, 1);
                if (pat.matcher(s).find()) {
                    result.add(ch[0]);
                }
            }

            out.println("   <" + classes[c] + ">");
            int[] starts = result.getStartPoints();
            int[] ends = result.getEndPoints();
            for (int r=0; r<result.getNumberOfRanges(); r++) {
                out.println("      <range from=\"" + starts[r] + "\" to=\"" + ends[r] + "\"/>");
            }
            out.println("   </" + classes[c] + ">");
        }

        out.println("</categories>");
        out.close();

    }

    @Test
    public void testNonBmpClassComplement() {
        int[] ch = new int[]{119800, 119801, 119802, 119803, 119804, 119805, 119806, 119807, 119808, 119809, 119810};
        String s = new String(ch, 0, ch.length);
        Pattern pat = Pattern.compile("\\p{Lu}");
        Matcher m1 = pat.matcher(s);
        System.err.println("Finding p{Lu}");
        while (m1.find()) {
            System.err.println("Found at " + m1.start());
        }
        pat = Pattern.compile("\\P{Lu}");
        m1 = pat.matcher(s);
        System.err.println("Finding P{Lu}");
        while (m1.find()) {
            System.err.println("Found at " + m1.start());
        }
    }

    @Test
    public void testJFlagMatches() {
        // Saxon extension; j flag causes use of Java regex engine
        try {
            Processor p = new Processor(true);
            XPathCompiler c = p.newXPathCompiler();
            XdmItem result = c.evaluateSingle("matches(' ', '^\\x20$', ';j')", null);
            assertTrue(((XdmAtomicValue)result).getBooleanValue());
            result = c.evaluateSingle("matches('Z ', '^z\\x20$', 'i;j')", null);
            assertTrue(((XdmAtomicValue) result).getBooleanValue());
        } catch (SaxonApiException e) {
            Assertions.fail();
        }
    }

    @Test
    public void TestJFlagTokenize() {  // bug 5655
        try {
            Processor p = new Processor(false);
            XPathCompiler c = p.newXPathCompiler();
            XdmValue v = c.evaluate("tokenize('dole0021', '\\W+|(?<=\\D)(?=\\d)|(?<=\\d)(?=\\D)', ';j')", null);
            Assertions.assertEquals(2, v.size());
            Assertions.assertEquals(v.itemAt(0).getStringValue(), "dole");
            Assertions.assertEquals(v.itemAt(1).getStringValue(), "0021");
        } catch (Exception e) {
            Assertions.fail();
        }
    }

    @Test
    public void testJFlagReplace() {
        // Saxon extension; j flag causes use of Java regex engine
        try {
            Processor p = new Processor(true);
            XPathCompiler c = p.newXPathCompiler();
            XdmItem result = c.evaluateSingle(
                    "replace('3/20/1986', '(?<mm>[0-9]+)/(?<dd>[0-9]+)/(?<yy>[0-9]+)', '${yy}/${mm}/${dd}\\$', ';j')", null);
            Assertions.assertEquals("1986/3/20$", result.getStringValue());
        } catch (SaxonApiException e) {
            Assertions.fail(e);
        }
    }

    @Test
    public void testParallelResourceInitialization() {   // Bug 6225/6226
        try {
            Processor proc = new Processor(true);
            final XPathCompiler xpath = proc.newXPathCompiler();
            ExecutorService executor = Executors.newFixedThreadPool(10);
            for (int i = 0; i < 300; i++) {
                executor.execute(new Runnable() {

                    @Override
                    public void run() {
                        try {
                            XPathExecutable exec1 = xpath.compile("matches(., 'banana', 'i')");
                            XPathExecutable exec2 = xpath.compile("matches(., '\\p{Lu}+')");
                            XPathExecutable exec3 = xpath.compile("matches(., '\\p{IsLatin1}+')");
                            XPathSelector s1 = exec1.load();
                            s1.setContextItem(new XdmAtomicValue("Banana"));
                            assertTrue(s1.effectiveBooleanValue());
                            XPathSelector s2 = exec2.load();
                            s2.setContextItem(new XdmAtomicValue("BANANA"));
                            assertTrue(s2.effectiveBooleanValue());
                            XPathSelector s3 = exec2.load();
                            s3.setContextItem(new XdmAtomicValue("BAÑANA"));
                            assertTrue(s3.effectiveBooleanValue());
                        } catch (Throwable ex) {
                            System.err.println("thread-# " + Thread.currentThread().getId() + " ERROR");
                            ex.printStackTrace();
                            fail();
                        }
                    }
                });
            }
            executor.shutdown();
            executor.awaitTermination(300, TimeUnit.SECONDS);
        } catch (InterruptedException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }


}
