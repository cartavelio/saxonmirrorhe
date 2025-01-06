package internaltests;

import com.saxonica.config.ProfessionalConfiguration;
import net.sf.saxon.s9api.Processor;
import java.io.BufferedReader;
import java.io.StringReader;

public class ActivateSaxon {

    private static final String oldKey =
            "j0k0q0fbbbj0f0fbbbkb009bbbq09b9ba0bbb0j0bb9b0030v0b0v030bbfb" +
                    "0b90kbk000bbbbq03bf0b0f0k0a0b0a0a0j0j0j0j0q090a0v0fbf00bf03b" +
                    "3bkbb0bb009bb0k0q030j0j0v030k0j00bk0a03b3ba0k0k0b0f0k0a00bk0" +
                    "a000r093kj9jbjf3w3j3q309zaza303000r00jqjf3bb9303w3f3w393bjw3" +
                    "q3f3rbza303000r00jqjf3bb93b3f3kjj3aj99zakj933j93w3r0w3d3q3bj" +
                    "f3kjq3ajvj9bzad3w3r0w3d3q3bjf39j$3f33j9bzaf0a0a0a0a9r0kj930j" +
                    "99zab0k0a0a0f0a039r0$3f3q3kj9309za39r00j93q3kj9309za30f0rkf0" +
                    "a0rkv0f0a0k0r0b3939j0j0jqbzad3w3r039fb09zad3w3r0f9fb09zad3w3" +
                    "r0b9fb09za9ba9r0w3d3q3bjq3b39bzar3d303wkf303q3w3d3vjf30jab93" +
                    "q3k3k393b3r0$3q3f3r39bza0jj3j3d3$3kbakrb9bdbr0qjw3f3ajr3d30b" +
                    "za0jj3j3d3$3kbakr3d3b9r093930jw39303q3$bzaf303q3w3d3vjf309r0" +
                    "kjd30jw39303q3$b";

    private static String key =
            "0bkb90kbq0f0a0f03bv0j09bj0v0fb0b00q0v0fbb0309b3b90q09b3030bb" +
                    "v09000j00bk0bbk0b000b0f0k0a0309b0b3b3b9bfbk0a000fbb0fbfbkba0" +
                    "b090j0b0fb90b0bbkb30q0q0b0fbq0v0b030v0bbk0b0kbk0b0f0k0a00bk0" +
                    "a000r093kj9jbjf3w3j3q309zaza303000r00jqjf3bb9303w3f3w393bjw3" +
                    "q3f3rbza303000r00jqjf3bb93b3f3kjj3aj99zakj933j93w3r0w3d3q3bj" +
                    "f3kjq3ajvj9bzad3w3r0w3d3q3bjf39j$3f33j9bzaf0a0a0a0a9r0kj930j" +
                    "99zaf0f000k0f0a039r0$3f3q3kj9309za39r00j93q3kj9309zak0f0rk90" +
                    "a0rkb0k0a0k0r0b3939j0j0jqbza0j93qjr039fb09za0j93qjr0f9fb09za" +
                    "0j93qjr0b9fb09za9b9br0w3d3q3bjq3b39bzar3d303wkf303q3w3d3vjf3" +
                    "0jab93;3q3r3r0$3q3f3r39bzaf303q3w3d3vjf309akrb9bdbr0qjw3f3aj" +
                    "r3d30bzaqjf3;bak$393f3v303q3rbr093930jw39303q3$bzaf303q3w3d3" +
                    "vjf309r0kjd30jw39303q3$b";


    private static String deobfuscate(String x) {
        String digits = "afk0b93jvqz;$rwd";
        StringBuffer buff = new StringBuffer(x.length());
        for (int i = x.length() - 1; i >= 0; i -= 2) {
            int c1 = digits.indexOf(x.charAt(i));
            int c2 = digits.indexOf(x.charAt(i - 1));
            buff.append((char) (c1 << 4 | c2));
        }
        return buff.toString();
    }

    // Activate a ProfessionalConfiguration or EnterpriseConfiguration
    public static void activate(ProfessionalConfiguration config) {
        config.supplyLicenseKey(new BufferedReader(new StringReader(deobfuscate(key))));
    }


    // Activate Saxon at the level of a s9api Processor
    public static void activate(Processor processor) {
        processor.setConfigurationProperty("http://saxonica.com/oem-data", deobfuscate(key));
    }

    // Activate Saxon at the level of a s9api Processor, on the underlying configuration, to avoid using deprecated API
    public static void activate2(Processor processor) {
        processor.getUnderlyingConfiguration().setConfigurationProperty(
                "http://saxonica.com/oem-data", deobfuscate(key));
    }

    // Activate Saxon at the level of a s9api Processor using an expired key
    public static void activateExpired(Processor processor) {
        processor.setConfigurationProperty("http://saxonica.com/oem-data", deobfuscate(oldKey));
    }

    // Activate Saxon at the level of a s9api Processor using an expired key,
    // on the underlying configuration, to avoid using deprecated API
    public static void activateExpired2(Processor processor) {
        processor.getUnderlyingConfiguration().setConfigurationProperty(
                "http://saxonica.com/oem-data", deobfuscate(oldKey));
    }

}

