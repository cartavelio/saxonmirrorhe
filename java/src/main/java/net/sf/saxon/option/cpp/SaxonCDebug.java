package net.sf.saxon.option.cpp;

/**
 * A Static helper that we can check to see if we should be doing debug-mode
 * things. Separate from SaxonCAPI to avoid the problems of running low-level
 * native-image code in a Java process (e.g. unit testing)
 */
public class SaxonCDebug {
    public static boolean debug = false;

    /**
     * Write the supplied message string to process stderr
     *
     * @param message
     */
    public static void err(String message) {
        System.err.println(message);
    }

    /**
     * Write the supplied message string to process stdout
     *
     * @param message
     */
    public static void out(String message) {
        System.out.println(message);
    }
}
