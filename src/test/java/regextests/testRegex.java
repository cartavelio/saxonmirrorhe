package regextests;

import jaxptest.ConfigTest;
import net.sf.saxon.z.IntIterator;
import net.sf.saxon.z.IntRangeSet;
import org.junit.jupiter.api.Test;

import java.io.File;
import java.io.FileWriter;
import java.io.IOException;
import java.io.PrintWriter;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

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


}
