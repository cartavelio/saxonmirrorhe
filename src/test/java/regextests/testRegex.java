package regextests;

import jaxptest.ConfigTest;
import junit.framework.TestCase;
import net.sf.saxon.value.StringValue;
import net.sf.saxon.z.IntIterator;
import net.sf.saxon.z.IntRangeSet;

import java.io.File;
import java.io.FileWriter;
import java.io.IOException;
import java.io.PrintWriter;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

/**
 *
 */
public class testRegex extends TestCase {

    public void testCharacterClasses() {

        PrintWriter out;
        try {
            out = new PrintWriter(new FileWriter(new File(ConfigTest.DATA_DIR + "JavaSubCategories.xml")));
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

    public static String diagnosticDisplay(String s) {
        return StringValue.diagnosticDisplay(s);
    }
}

//
// The contents of this file are subject to the Mozilla Public License Version
// 1.0 (the "License");
// you may not use this file except in compliance with the License. You may
// obtain a copy of the
// License at http://www.mozilla.org/MPL/
//
// Software distributed under the License is distributed on an "AS IS" basis,
// WITHOUT WARRANTY OF ANY KIND, either express or implied.
// See the License for the specific language governing rights and limitations
// under the License.
//
// The Original Code is: all this file.
//
// The Initial Developer of the Original Code is Michael Kay,
//
// Portions created by (your name) are Copyright (C) (your legal entity). All
// Rights Reserved.
//
// Contributor(s): none.
//



