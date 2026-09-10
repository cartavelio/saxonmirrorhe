package regextests;

import java.util.regex.Matcher;
import java.util.regex.Pattern;


public class RegexComplementBug {


    public static void testNonBmpClassComplement() {
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
        pat = Pattern.compile("[^\\p{Lu}]");
        m1 = pat.matcher(s);
        System.err.println("Finding [^\\p{Lu}]");
        while (m1.find()) {
            System.err.println("Found at " + m1.start());
        }
        pat = Pattern.compile("[\u0001-\uDBFF\uDFE1]");
        m1 = pat.matcher(s);
        System.err.println("Finding [\\u0001-\\uDBFF\\uDFE1&&[P{Lu}]]");
        while (m1.find()) {
            System.err.println("Found at " + m1.start());
        }
        pat = Pattern.compile("[^\\p{Lu}\uD801\uDC00-\uD801\uDC27\uD835\uDC00-\uD835\uDC19\uD835\uDC34-\uD835\uDC4D\uD835\uDC68-\uD835\uDC81\uD835\uDC9C-\uD835\uDC9C\uD835\uDC9E-\uD835\uDC9F\uD835\uDCA2-\uD835\uDCA2\uD835\uDCA5-\uD835\uDCA6\uD835\uDCA9-\uD835\uDCAC\uD835\uDCAE-\uD835\uDCB5\uD835\uDCD0-\uD835\uDCE9\uD835\uDD04-\uD835\uDD05\uD835\uDD07-\uD835\uDD0A\uD835\uDD0D-\uD835\uDD14\uD835\uDD16-\uD835\uDD1C\uD835\uDD38-\uD835\uDD39\uD835\uDD3B-\uD835\uDD3E\uD835\uDD40-\uD835\uDD44\uD835\uDD46-\uD835\uDD46\uD835\uDD4A-\uD835\uDD50\uD835\uDD6C-\uD835\uDD85\uD835\uDDA0-\uD835\uDDB9\uD835\uDDD4-\uD835\uDDED\uD835\uDE08-\uD835\uDE21\uD835\uDE3C-\uD835\uDE55\uD835\uDE70-\uD835\uDE89\uD835\uDEA8-\uD835\uDEC0\uD835\uDEE2-\uD835\uDEFA\uD835\uDF1C-\uD835\uDF34\uD835\uDF56-\uD835\uDF6E\uD835\uDF90-\uD835\uDFA8\u03F4]");
        m1 = pat.matcher(s);
        System.err.println("Find using Clark regex");
        while (m1.find()) {
            System.err.println("Found at " + m1.start());
        }



    }

    public static void main(String[] args) throws Exception {
//        int[] nonbmp = new int[1114111 - 65535];
//        for (int i=65536; i<1114111; i++) {
//            nonbmp[i - 65536] = i;
//        }
//        PrintWriter out = new PrintWriter(new FileWriter("e:/temp/utf16.txt"));
//        String omni = new String(nonbmp, 0, nonbmp.length);
//        for (int i=0; i<omni.length()-2; i+=2) {
//            int u = 65536 + (i/2);
//            out.println(u + " = " + Integer.toHexString((int)omni.charAt(i)) + " " + Integer.toHexString((int)omni.charAt(i+1)));
//        }

        testNonBmpClassComplement();
    }
}
