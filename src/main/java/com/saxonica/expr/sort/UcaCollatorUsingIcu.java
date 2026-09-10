package com.saxonica.expr.sort;

import com.ibm.icu.lang.UScript;
import com.ibm.icu.text.CollationElementIterator;
import com.ibm.icu.text.Collator;
import com.ibm.icu.text.RuleBasedCollator;
import com.ibm.icu.util.ULocale;
import com.ibm.icu.util.VersionInfo;
import net.sf.saxon.expr.sort.AtomicMatchKey;
import net.sf.saxon.expr.sort.CollationMatchKey;
import net.sf.saxon.lib.SubstringMatcher;
import net.sf.saxon.str.EmptyUnicodeString;
import net.sf.saxon.str.StringView;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.transpile.CSharp;
import net.sf.saxon.transpile.CSharpReplaceBody;
import net.sf.saxon.type.StringConverter;
import net.sf.saxon.type.ValidationFailure;
import net.sf.saxon.value.AnyURIValue;

import java.net.URI;
import java.net.URISyntaxException;
import java.text.CollationKey;
import java.util.*;


/**
 * This class represents a collation (for comparing strings) implemented
 * using the ICU library. It is available in Saxon-PE and Saxon-EE only.
 * <p>Note that in the case that the ICU libraries have not been loaded,
 * then this class is not instantiated. However, the public static functions,
 * e.g. getLocales() could in theory be accessed via an XSLT stylesheet
 * (such as the documentation generators for example) and these invoke ICU features
 * The guard function com.saxonica.config.JavaPlatformPE.hasICUCollator() can be used to check.</p>
 */
public class UcaCollatorUsingIcu implements SubstringMatcher {

    private final String uri;
    private com.ibm.icu.text.RuleBasedCollator uca;
    private Strength strengthLevel;
    public int[] maxIgnorable;


    private static final String[] keywords = {"fallback", "lang", "version", "strength",
            "alternate", "backwards", "normalization", "maxVariable",
            "caseLevel", "caseFirst", "numeric", "reorder"}; //, "hiraganaQuaternary"
    private static final Set<String> keys = new HashSet<>(Arrays.asList(keywords));

    /**
     * Return the version of UCA supported
     *
     * @return UCA version as nn.nn.nn.nn
     */
    public static String getUCAVersion() {
        VersionInfo v = com.ibm.icu.text.Collator.getInstance().getUCAVersion();
        return v.getMajor() + "." + v.getMinor()
                + "." + v.getMilli() + "." + v.getMicro();
    }

    /**
     * Construct a collation from a given UCA-based collation URI
     * @param uri the collation URI, conforming to the syntax defined in the W3C
     *            <i>Functions and Operators</i> specification.
     * @throws XPathException if the URI is invalid
     */

    public UcaCollatorUsingIcu(String uri) throws XPathException {
        this.uri = uri;
        uca = (RuleBasedCollator) com.ibm.icu.text.Collator.getInstance();
        setProps(parseProps(uri));
    }

    /**
     * Get the underlying ICU collator represented by this collation object
     * @return the underlying ICU collator
     */

    public RuleBasedCollator getRuleBasedCollator() {
        return uca;
    }

//    /**
//     * Return the locales supported in this version of ICU
//     * Note that with Java 6 this method has been known to throw an array index exception from RuleBasedCollator,
//     * With Java 7 this performs correctly and getAvailableLocales() does not appear to be
//     * invoked from any of the UCA or numbering support.
//     *
//     * @return The language tags and display names separated by semicolon
//     */
//    public static String[] getLocales() {
//        ArrayList<String> locales = new ArrayList<>();
//        Locale[] availableLocales = RuleBasedCollator.getAvailableLocales();
//        for (Locale l : availableLocales) {
//            locales.add(ICULibrary.getLanguageTag(l) + ";" + l.getDisplayName());
//        }
//        return locales.toArray(new String[0]);
//    }

    private void error(String field, String allowed) throws XPathException {
        error("value of " + field + " must be " + allowed);
    }

    private void error(String field, String allowed, String requested) throws XPathException {
        error("value of " + field + " must be " + allowed + ", requested was:" + requested);
    }

    private void error(String message) throws XPathException {
        throw new XPathException("Error in UCA Collation URI " + uri + ": " + message, "FOCH0002");
    }

    //#if CSHARP==false
    /**
     * Display details of this collation
     * @return a string suitable for diagnostic display
     */

    public String show() {
        if (uca != null) {
            return " UCAversion=" + uca.getUCAVersion()
                    + " Version=" + uca.getVersion()
                    + " lang=" + uca.getLocale(ULocale.ACTUAL_LOCALE)
                    + " french=" + uca.isFrenchCollation()
                    + " lowerFirst=" + uca.isLowerCaseFirst()
                    + " upperFirst=" + uca.isUpperCaseFirst()
                    + " strength=" + uca.getStrength();
        } else {
            return "No RuleBasedCollator initialised";
        }
    }
    //#endif

    /**
     * Factory method to generate a collation key for a given string, using this collation
     * @param source the supplied string
     * @return a collation key with the property that two strings that are deemed equal
     * under this collation generate the same collation key (according to the {@link CollationKey#equals(Object)}
     * and {@link CollationKey#compareTo(CollationKey)} methods).
     */

    public CollationKey getCollationKey(String source) {
//        System.err.println("UCA.getCollationKey:" + source);
        return new UcaCollationKeyUsingIcu(source, uca.getCollationKey(source).toByteArray());
    }

    /**
     * Get a hash code for the purpose of comparing whether this is the same collation as another.
     * @return a suitable hash code
     */

    @Override
    public int hashCode() {
        return uca.hashCode();
    }

    /**
     * Set the properties for the UCA collation
     *
     * @param props the set of properties parsed from the UCA parameters
     * @throws XPathException if the properties are invalid
     */
    public void setProps(Properties props) throws XPathException {
        boolean fallbackError = false;
        String fallback = props.getProperty("fallback");
        if (fallback != null) {
            switch (fallback) {
                case "yes":
                    break;
                case "no":
                    fallbackError = true;
                    break;
                default:
                    error("fallback", "yes|no");
                    break;
            }
        }

        String lang = props.getProperty("lang");
        if (lang != null && !lang.isEmpty()) {
            ValidationFailure vf = StringConverter.StringToLanguage.INSTANCE.validate(StringView.tidy(lang));
            if (vf != null) {
                error("lang", "a valid language code");
            }
            String language, country = "", variant = "";
            String[] parts = lang.split("-");
            language = parts[0];
            if (parts.length > 1) {
                country = parts[1];
            }
            if (parts.length > 2) {
                variant = parts[2];
            }
            Locale loc = makeLocale(language, country, variant);
            uca = (RuleBasedCollator) com.ibm.icu.text.Collator.getInstance(loc);
        }

        String version = props.getProperty("version");
        if (version != null) {
            try {
                VersionInfo v = VersionInfo.getInstance(version);
                VersionInfo current = uca.getUCAVersion();
                if (v.compareTo(current) > 0 && fallbackError) {
                    error("requesting UCA version " + v.toString() + ", implementation supports UCA version " + current.toString());
                }
            } catch (Exception e) {
                if (fallbackError) {
                    error("version number format incorrect:" + version);
                }
            }
        }

        String strength = props.getProperty("strength");
        if (strength != null) {
            switch (strength) {
                case "primary":
                case "1":
                    setStrength(com.ibm.icu.text.Collator.PRIMARY);
                    break;
                case "secondary":
                case "2":
                    setStrength(com.ibm.icu.text.Collator.SECONDARY);
                    break;
                case "tertiary":
                case "3":
                    setStrength(com.ibm.icu.text.Collator.TERTIARY);
                    break;
                case "quaternary":
                case "4":
                    setStrength(com.ibm.icu.text.Collator.QUATERNARY);
                    break;
                case "identical":
                case "5":
                    setStrength(com.ibm.icu.text.Collator.IDENTICAL);
                    break;
                default:
                    if (fallbackError) {
                        error("strength", "primary|secondary|tertiary|quaternary|identical|1|2|3|4|5", strength);
                    }
                    break;
            }
        }

        String maxVariable = props.getProperty("maxVariable");
        if (maxVariable != null) {
            if (maxVariable.equals("space")) {
                uca.setMaxVariable(com.ibm.icu.text.Collator.ReorderCodes.SPACE);
            } else if (maxVariable.equals("punct")) {
                uca.setMaxVariable(com.ibm.icu.text.Collator.ReorderCodes.PUNCTUATION);
            } else if (maxVariable.equals("symbol")) {
                uca.setMaxVariable(com.ibm.icu.text.Collator.ReorderCodes.SYMBOL);
            } else if (maxVariable.equals("currency")) {
                uca.setMaxVariable(com.ibm.icu.text.Collator.ReorderCodes.CURRENCY);
            } else if (fallbackError) {
                error("maxVariable", "space|punct|symbol|currency");
            }
        }

        String alternate = props.getProperty("alternate");
        if (alternate != null) {
            switch (alternate) {
                case "non-ignorable":
                    uca.setAlternateHandlingShifted(false);
                    break;
                case "shifted":
                    uca.setAlternateHandlingShifted(true);
                    break;
                case "blanked":
                /* According to http://slackware.uk/slacky/slackware-12.0/libraries/icu/3.6/src/icu-3_6-userguide.pdf
                 - The UCA option Blanked is expressed with Strength set to 3, and Alternate set to Shifted.
                 But see the notes on test case compare-042: if strength=identical is combined with alternate=blanked,
                 strength=identical wins.
                 Haven't been able to get this to work on .NET with ICU4N.
                */
                    uca.setAlternateHandlingShifted(true);
                    if ("quaternary".equals(props.getProperty("strength"))) {
                        setStrength(com.ibm.icu.text.Collator.TERTIARY);
                    }
                    break;
                default:
                    if (fallbackError) {
                        error("alternate", "non-ignorable|shifted|blanked", alternate);
                    }
                    break;
            }
        }

        String backwards = props.getProperty("backwards");
        if (backwards != null) {
            switch (backwards) {
                case "yes":
                    uca.setFrenchCollation(true);
                    break;
                case "no":
                    uca.setFrenchCollation(false);
                    break;
                default:
                    if (fallbackError) {
                        error("backwards", "yes|no");
                    }
                    break;
            }
        }

        String normalization = props.getProperty("normalization");
        if (normalization != null) {
            switch (normalization) {
                case "yes":
                    uca.setDecomposition(com.ibm.icu.text.Collator.CANONICAL_DECOMPOSITION);
                    break;
                case "no":
                    uca.setDecomposition(com.ibm.icu.text.Collator.NO_DECOMPOSITION);
                    break;
                default:
                    if (fallbackError) {
                        error("normalization", "yes|no");
                    }
                    break;
            }
        }

        String caseLevel = props.getProperty("caseLevel");
        if (caseLevel != null) {
            switch (caseLevel) {
                case "yes":
                    uca.setCaseLevel(true);
                    break;
                case "no":
                    uca.setCaseLevel(false);
                    break;
                default:
                    if (fallbackError) {
                        error("caseLevel", "yes|no");
                    }
                    break;
            }
        }

        String caseFirst = props.getProperty("caseFirst");
        if (caseFirst != null) {
            switch (caseFirst) {
                case "upper":
                    uca.setUpperCaseFirst(true);
                    break;
                case "lower":
                    uca.setLowerCaseFirst(true);
                    break;
                default:
                    if (fallbackError) {
                        error("caseFirst", "upper|lower");
                    }
                    break;
            }
        }

        String numeric = props.getProperty("numeric");
        if (numeric != null) {
            switch (numeric) {
                case "yes":
                    uca.setNumericCollation(true);
                    break;
                case "no":
                    uca.setNumericCollation(false);
                    break;
                default:
                    if (fallbackError) {
                        error("numeric", "yes|no");
                    }
                    break;
            }
        }

        String reorder = props.getProperty("reorder");
        if (reorder != null) {
            ArrayList<Integer> codesInt = new ArrayList<>();
            for (String param : reorder.split(",")) {
                if (param.equals("space")) {
                    codesInt.add(Collator.ReorderCodes.SPACE);
                } else if (param.equals("punct")) {
                    codesInt.add(Collator.ReorderCodes.PUNCTUATION);
                } else if (param.equals("symbol")) {
                    codesInt.add(Collator.ReorderCodes.SYMBOL);
                } else if (param.equals("currency")) {
                    codesInt.add(Collator.ReorderCodes.CURRENCY);
                } else if (param.equals("digit")) {
                    codesInt.add(Collator.ReorderCodes.DIGIT);
                } else if (param.length() == 4) {
                    int[] script = UScript.getCode(param);
                    if (script != null) {
                        codesInt.add(script[0]);
                    } else if (fallbackError) {
                        error("reorder-code", "space|punct|symbol|currency|digit or 4-character script");
                    }
                } else if (fallbackError) {
                    error("reorder-code", "space|punct|symbol|currency|digit or 4-character script");
                }
            }
            int[] codes = new int[codesInt.size()];
            int i = 0;
            for (Integer n : codesInt) {
                codes[i++] = n;
            }
            uca.setReorderCodes(codes);
        }

        // VariableTop is deprecated in C#, we should use MaxVariable, but it doesn't seem to work (MHK 2021-09-11)
        CSharp.emitCode("#pragma warning disable 618");
        int m = uca.getVariableTop();
        CSharp.emitCode("#pragma warning restore 618");
        maxIgnorable = new int[]{m, m, m, m, 0};
    }

    @CSharpReplaceBody(code="return makeCSharpLocale(language, country, variant);")
    private static Locale makeLocale(String language, String country, String variant) {
        return new Locale(language, country, variant);
    }

    //#if CSHARP==true
    private static Locale makeCSharpLocale(String language, String country, String variant) {
        StringBuilder sb = new StringBuilder(language);
        if (country != null && !country.isEmpty()) {
            sb.append("-").append(country);
        }
        if (variant != null && !variant.isEmpty()) {
            sb.append("-").append(variant);
        }
        return new Locale(sb.toString());
    }
    //#endif

    private Properties parseProps(String uri) throws XPathException {
        URI uuri;
        try {
            uuri = new URI(uri);
        } catch (URISyntaxException err) {
            throw new XPathException(err);
        }
        ArrayList<String> unknownKeys = new ArrayList<String>();
        Properties props = new Properties();
        String query = AnyURIValue.decode(uuri.getRawQuery());
        if (query != null && !query.isEmpty()) {
            if (query.startsWith("?")) { // Happens on .NET
                query = query.substring(1);
            }
            for (String s : query.split(";")) {
                String[] tokens = s.split("=");
                if (!keys.contains(tokens[0])) {
                    unknownKeys.add(tokens[0]);
                }
                props.setProperty(tokens[0], tokens[1]);
            }
        }
        String fallback = props.getProperty("fallback");
        if (fallback != null && fallback.equals("no") && !unknownKeys.isEmpty()) {
            String message = unknownKeys.size() > 1 ? "unknown parameters:" : "unknown parameter:";
            for (String u : unknownKeys) {
                message += u + " ";
            }
            error(message);
        }
        return props;
    }

    public void setStrength(int newStrength) {
        uca.setStrength(newStrength);
    }

    public int getStrength() {
        return uca.getStrength();
    }

//    public ULocale getLocale() {
//        return uca.getLocale(ULocale.ACTUAL_LOCALE);
//    }

    @Override
    public boolean comparesEqual(UnicodeString s1, UnicodeString s2) {
        return uca.compare(s1.toString(), s2.toString()) == 0;
    }

    @Override
    public String getCollationURI() {
        return uri;
    }

    @Override
    public int compareStrings(UnicodeString o1, UnicodeString o2) {
        return uca.compare(o1.toString(), o2.toString());
    }

    @Override
    public AtomicMatchKey getCollationKey(UnicodeString s) {
        CollationKey ck = getCollationKey(s.toString());
        return new CollationMatchKey(ck);
    }


    /**
     * Test whether one string contains another, according to the rules
     * of the XPath contains() function
     *
     * @param s1 the containing string
     * @param s2 the contained string
     * @return true iff s1 contains s2
     */

    @Override
    public boolean contains(UnicodeString s1, UnicodeString s2) {
        RuleBasedCollator collator = getRuleBasedCollator();
        CollationElementIterator iter1 = collator.getCollationElementIterator(s1.toString());
        CollationElementIterator iter2 = collator.getCollationElementIterator(s2.toString());
        return collationContains(iter1, iter2, null, false);
    }

    /**
     * Test whether one string ends with another, according to the rules
     * of the XPath ends-with() function
     *
     * @param s1 the containing string
     * @param s2 the contained string
     * @return true iff s1 ends with s2
     */

    @Override
    public boolean endsWith(UnicodeString s1, UnicodeString s2) {
        RuleBasedCollator collator = getRuleBasedCollator();
        CollationElementIterator iter1 = collator.getCollationElementIterator(s1.toString());
        CollationElementIterator iter2 = collator.getCollationElementIterator(s2.toString());
        return collationContains(iter1, iter2, null, true);
    }

    /**
     * Test whether one string starts with another, according to the rules
     * of the XPath starts-with() function
     *
     * @param s1 the containing string
     * @param s2 the contained string
     * @return true iff s1 starts with s2
     */

    @Override
    public boolean startsWith(UnicodeString s1, UnicodeString s2) {
        RuleBasedCollator collator = getRuleBasedCollator();
        CollationElementIterator iter1 = collator.getCollationElementIterator(s1.toString());
        CollationElementIterator iter2 = collator.getCollationElementIterator(s2.toString());
        return collationStartsWith(iter1, iter2);
    }

    /**
     * Return the part of a string after a given substring, according to the rules
     * of the XPath substring-after() function
     *
     * @param s1 the containing string
     * @param s2 the contained string
     * @return the part of s1 that follows the first occurrence of s2
     */

    @Override
    public UnicodeString substringAfter(UnicodeString s1, UnicodeString s2) {
        RuleBasedCollator collator = getRuleBasedCollator();
        final String g1 = s1.toString();
        CollationElementIterator iter1 = collator.getCollationElementIterator(g1);
        CollationElementIterator iter2 = collator.getCollationElementIterator(s2.toString());
        int[] ia = new int[2];
        boolean ba = collationContains(iter1, iter2, ia, false);
        if (ba) {
            return s1.substring(ia[1]);
        } else {
            return EmptyUnicodeString.getInstance();
        }
    }

    /**
     * Return the part of a string before a given substring, according to the rules
     * of the XPath substring-before() function
     *
     * @param s1 the containing string
     * @param s2 the contained string
     * @return the part of s1 that precedes the first occurrence of s2
     */

    @Override
    public UnicodeString substringBefore(UnicodeString s1, UnicodeString s2) {
        RuleBasedCollator collator = getRuleBasedCollator();
        final String g1 = s1.toString();
        CollationElementIterator iter1 = collator.getCollationElementIterator(g1);
        CollationElementIterator iter2 = collator.getCollationElementIterator(s2.toString());
        int[] ib = new int[2];
        boolean bb = collationContains(iter1, iter2, ib, false);
        if (bb) {
            return s1.substring(0, ib[0]);
        } else {
            return EmptyUnicodeString.getInstance();
        }
    }

    /**
     * Determine whether one string starts with another, under the terms of a given
     * collating sequence.
     *
     * @param s0 iterator over the collation elements of the containing string
     * @param s1 iterator over the collation elements of the contained string
     * @return true if the first string starts with the second
     */

    private boolean collationStartsWith(CollationElementIterator s0,
                                        CollationElementIterator s1) {
        makeStrengthObject();
        while (true) {
            int e0, e1;
            do {
                e1 = s1.next();
                //System.err.println("Got e1 = " + show(e1));
            } while (strengthLevel.isIgnored(e1));
            if (e1 == CollationElementIterator.NULLORDER) {
                return true;
            }
            do {
                e0 = s0.next();
                //System.err.println("Got e0 = " + show(e1));
            } while (strengthLevel.isIgnored(e0));
            if (e0 == CollationElementIterator.NULLORDER) {
                return false;
            }
            //System.err.println("Compare " + show(e0) + " ~ " + show(e1) + " = " + strengthLevel.compare(e0, e1));
            if (strengthLevel.compareCollationElements(e0, e1) != 0) {
                return false;
            }
        }
    }

    private String show(int ce) {
        return "" + CollationElementIterator.primaryOrder(ce) + "/" +
                CollationElementIterator.secondaryOrder(ce) + "/" +
                CollationElementIterator.tertiaryOrder(ce);
    }

    private void makeStrengthObject() {
        if (strengthLevel == null) {
            switch (getStrength()) {
                case com.ibm.icu.text.Collator.PRIMARY:
                    strengthLevel = new Primary(this);
                    break;
                case com.ibm.icu.text.Collator.SECONDARY:
                    strengthLevel = new Secondary(this);
                    break;
                case com.ibm.icu.text.Collator.TERTIARY:
                    strengthLevel = new Tertiary(this);
                    break;
                default:
                    strengthLevel = new Identical(this);
                    break;
            }
        }
    }

    /**
     * Determine whether one string contains another, under the terms of a given
     * collating sequence. If matchAtEnd=true, the match must be at the end of the first
     * string.
     *
     * @param s0         iterator over the collation elements of the containing string
     * @param s1         iterator over the collation elements of the contained string
     * @param offsets    may be null, but if it is supplied, it must be an array of two
     *                   integers which, if the function returns true, will contain the start position of the
     *                   first matching substring, and the offset of the first character after the first
     *                   matching substring. This is not available for matchAtEnd=true
     * @param matchAtEnd true if the match is required to be at the end of the string
     * @return true if the first string contains the second
     */

    private boolean collationContains(CollationElementIterator s0,
                                      CollationElementIterator s1,
                                      /*@Nullable*/ int[] offsets,
                                      boolean matchAtEnd) {
        makeStrengthObject();
        int e0, e1;
        do {
            e1 = s1.next();
        } while (strengthLevel.isIgnored(e1));
        if (e1 == CollationElementIterator.NULLORDER) {
            return true;
        }
        e0 = CollationElementIterator.NULLORDER;
        while (true) {
            // scan the first string to find a matching character
            while (strengthLevel.compareCollationElements(e0, e1) != 0) {
                do {
                    e0 = s0.next();
                } while (strengthLevel.isIgnored(e0));
                if (e0 == CollationElementIterator.NULLORDER) {
                    // hit the end, no match
                    return false;
                }
            }
            // matched first character, note the position of the possible match
            int start = s0.getOffset();
            if (collationStartsWith(s0, s1)) {
                if (matchAtEnd) {
                    do {
                        e0 = s0.next();
                    } while (strengthLevel.isIgnored(e0));
                    if (e0 == CollationElementIterator.NULLORDER) {
                        // the match is at the end
                        return true;
                    }
                    // else ignore this match and keep looking
                } else {
                    if (offsets != null) {
                        offsets[0] = start - 1;
                        offsets[1] = s0.getOffset();
                    }
                    return true;
                }
            }
            // reset the position and try again
            s0.setOffset(start);

            // workaround for a difference between JDK 1.4.0 and JDK 1.4.1
            if (s0.getOffset() != start) {
                // JDK 1.4.0 takes this path
                s0.next();
            }
            s1.reset();
            e0 = -1;
            do {
                e1 = s1.next();
            } while (strengthLevel.isIgnored(e1));
            // loop round to try again
        }
    }


    private static abstract class Strength {

        protected UcaCollatorUsingIcu collator;

        public Strength(UcaCollatorUsingIcu thisCollator) {
            this.collator = thisCollator;
        }
        /**
         * Compare two collation elements
         * @param ce1 the first collation element
         * @param ce2 the second collation element
         * @return -1, 0, or +1 depending on the ordering relation between the two collation elements
         */

        abstract public int compareCollationElements(int ce1, int ce2);

        /**
         * Ask whether a collation element should be ignored
         * @param ce the collation element
         * @return true if the collation element should be ignored
         */
        abstract public boolean isIgnored(int ce);
    }

    private static class Primary extends Strength {

        public Primary(UcaCollatorUsingIcu collator) {
            super(collator);
        }
        @Override
        public int compareCollationElements(int ce1, int ce2) {
            return Integer.compare(CollationElementIterator.primaryOrder(ce1), CollationElementIterator.primaryOrder(ce2));
        }

        @Override
        public boolean isIgnored(int ce) {
            return ce != CollationElementIterator.NULLORDER &&
                    (CollationElementIterator.primaryOrder(ce) == CollationElementIterator.IGNORABLE
                             || ce < collator.maxIgnorable[0]);
        }
    }

    private static class Secondary extends Strength {

        public Secondary(UcaCollatorUsingIcu collator) {
            super(collator);
        }
        @Override
        public int compareCollationElements(int ce1, int ce2) {
            int c1 = Integer.compare(CollationElementIterator.primaryOrder(ce1), CollationElementIterator.primaryOrder(ce2));
            if (c1 == 0) {
                return Integer.compare(CollationElementIterator.secondaryOrder(ce1), CollationElementIterator.secondaryOrder(ce2));
            } else {
                return c1;
            }
        }

        @Override
        public boolean isIgnored(int ce) {
            return false;
        }
    }

    private static class Tertiary extends Strength {

        public Tertiary(UcaCollatorUsingIcu collator) {
            super(collator);
        }
        @Override
        public int compareCollationElements(int ce1, int ce2) {
            int c1 = Integer.compare(CollationElementIterator.primaryOrder(ce1), CollationElementIterator.primaryOrder(ce2));
            if (c1 == 0) {
                int c2 = Integer.compare(CollationElementIterator.secondaryOrder(ce1), CollationElementIterator.secondaryOrder(ce2));
                if (c2 == 0) {
                    return Integer.compare(CollationElementIterator.tertiaryOrder(ce1), CollationElementIterator.tertiaryOrder(ce2));
                } else {
                    return c2;
                }
            } else {
                return c1;
            }
        }

        @Override
        public boolean isIgnored(int ce) {
            //System.err.println("isIgnored " + show(ce) + "?");
            return ce != CollationElementIterator.NULLORDER &&
                    CollationElementIterator.secondaryOrder(ce) < 6 &&
                    CollationElementIterator.primaryOrder(ce) < CollationElementIterator.primaryOrder(collator.maxIgnorable[0]);
        }
    }

    private static class Identical extends Strength {

        public Identical(UcaCollatorUsingIcu collator) {
            super(collator);
        }

        @Override
        public int compareCollationElements(int ce1, int ce2) {
            return Integer.compare(ce1, ce2);
        }

        @Override
        public boolean isIgnored(int ce) {
            return false;
        }
    }


}

// Copyright (c) 2015-2023 Saxonica Limited
