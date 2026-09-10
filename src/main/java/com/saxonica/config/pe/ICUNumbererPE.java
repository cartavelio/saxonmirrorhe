package com.saxonica.config.pe;

import com.ibm.icu.text.DateFormat;
import com.ibm.icu.text.RuleBasedCollator;
import com.ibm.icu.text.RuleBasedNumberFormat;
import com.ibm.icu.text.SimpleDateFormat;
import com.ibm.icu.util.Calendar;
import com.ibm.icu.util.ULocale;
import com.ibm.icu.util.VersionInfo;
import com.saxonica.config.ICULibrary;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.number.AbstractNumberer;

import java.util.*;
import java.util.concurrent.ConcurrentHashMap;

/**
 * A numberer using the ICU features, used principally for formatting numbers as words
 * and day/month names
 * <p>Note that in the case that the ICU libraries have not been loaded,
 * then this class is not instantiated. However, the public static functions,
 * e.g. getLocales() could in theory be accessed via an XSLT stylesheet
 * (such as the documentation generators for example) and these invoke ICU features
 * The guard function com.saxonica.config.ICUNumbererPE.hasICU() can be used to check.</p>
 */
public class ICUNumbererPE extends AbstractNumberer {
    private final static String spelloutCardinalPattern = "%spellout-cardinal";
    private final static String spelloutOrdinalPattern = "%spellout-ordinal";
    private final static String spelloutNumberingPattern = "%spellout-numbering";
    private final static String numberOrdinalPattern = "%digits-ordinal";
    private String defaultCardinalRule;
    private String defaultOrdinalRule;
    private String defaultNumericOrdinalRule;
    private final Map<String, String> ordinalParamIndex = new ConcurrentHashMap<>();
    private final ArrayList<String> spelloutCardinalRulesets = new ArrayList<>();
    private final ArrayList<String> spelloutOrdinalRulesets = new ArrayList<>();
    private final ArrayList<String> spelloutNumberingRulesets = new ArrayList<>();
    private final ArrayList<String> numberOrdinalRulesets = new ArrayList<>();
    private Locale locale;
    private Locale defaultedLocale;

    private final static String DAY_FULL = "EEEE",
        DAY_SHORT = "EEE",
        DAY_NARROW = "EEEEE",
        DAY_SHORT2 = "EEEEEE";
    private final static String MONTH_FULL = "MMMM",
        MONTH_SHORT = "MMM",
        MONTH_NARROW = "MMMMM";

    private static final String[] preferences = {"-verbose", "", "-native", "-neuter", "-feminine", "-masculine"};

    private RuleBasedNumberFormat rbnf = null;
    private RuleBasedNumberFormat rbnfNumber = null;
    private final ThreadLocal<HashMap<String, SimpleDateFormat>> simpleDateFormats = new ThreadLocal<>();

    private static final HashMap<String, String> schemeCodes = new HashMap<>();

    static {
        schemeCodes.put("do", "%digits-ordinal");
        schemeCodes.put("dof", "%digits-ordinal-feminine");
        schemeCodes.put("dom", "%digits-ordinal-masculine");
        schemeCodes.put("doma", "%digits-ordinal-masculine-adjective");
        schemeCodes.put("don", "%digits-ordinal-neuter");
        schemeCodes.put("dor", "%digits-ordinal-reale");
        schemeCodes.put("sc", "%spellout-cardinal");
        schemeCodes.put("sca", "%spellout-cardinal-alternate2");
        schemeCodes.put("scf", "%spellout-cardinal-feminine");
        schemeCodes.put("scfbc", "%spellout-cardinal-feminine-before-consonant");
        schemeCodes.put("scfi", "%spellout-cardinal-financial");
        schemeCodes.put("scm", "%spellout-cardinal-masculine");
        schemeCodes.put("scmbc", "%spellout-cardinal-masculine-before-consonant");
        schemeCodes.put("scn", "%spellout-cardinal-native");
        schemeCodes.put("scna", "%spellout-cardinal-native-attributive");
        schemeCodes.put("scne", "%spellout-cardinal-neuter");
        schemeCodes.put("scr", "%spellout-cardinal-reale");
        schemeCodes.put("scs", "%spellout-cardinal-sinokorean");
        schemeCodes.put("scv", "%spellout-cardinal-verbose");
        schemeCodes.put("sn", "%spellout-numbering");
        schemeCodes.put("snd", "%spellout-numbering-days");
        schemeCodes.put("snv", "%spellout-numbering-verbose");
        schemeCodes.put("sny", "%spellout-numbering-year");
        schemeCodes.put("so", "%spellout-ordinal");
        schemeCodes.put("sof", "%spellout-ordinal-feminine");
        schemeCodes.put("sofp", "%spellout-ordinal-feminine-plural");
        schemeCodes.put("som", "%spellout-ordinal-masculine");
        schemeCodes.put("soma", "%spellout-ordinal-masculine-adjective");
        schemeCodes.put("somp", "%spellout-ordinal-masculine-plural");
        schemeCodes.put("son", "%spellout-ordinal-native");
        schemeCodes.put("sonc", "%spellout-ordinal-native-count");
        schemeCodes.put("sone", "%spellout-ordinal-neuter");
        schemeCodes.put("sor", "%spellout-ordinal-reale");
        schemeCodes.put("sos", "%spellout-ordinal-sinokorean");
        schemeCodes.put("sosc", "%spellout-ordinal-sinokorean-count");
        schemeCodes.put("sov", "%spellout-ordinal-verbose");
    }

    private synchronized DateFormat getDateFormat(String pattern) {
        // SimpleDateFormat is not thread-safe, so we need a separate cache of SimpleDateFormat objects in each thread.
        // See bug 2436.
        HashMap<String, SimpleDateFormat> map = simpleDateFormats.get();
        if (map == null) {
            map = new HashMap<>();
            simpleDateFormats.set(map);
        }
        SimpleDateFormat s = map.get(pattern);
        if (s == null) {
            s = new SimpleDateFormat(pattern, locale);
            map.put(pattern, s);
        }
        return s;
    }

    ICUNumbererPE() { }

    /**
     * Returns a suitable configured ICU numberer or a specialist subclass thereof.
     *
     * @param locale              the locale to use
     * @param privateUseExtension the language code extension used to denote required numbering
     * @param config              the system Configuration
     * @return a suitable Numberer.
     */
    public static synchronized ICUNumbererPE getNumberer(Locale locale, String privateUseExtension, Configuration config) {
        ICUNumbererPE n = new ICUNumbererPE();
        String[] extensions = privateUseExtension == null ? new String[0] : privateUseExtension.split("-");
        List<String> extensionList = new ArrayList<>(Arrays.asList(extensions));
        boolean withHyphen = extensionList.contains("hyphen");
        if (withHyphen) {
            extensionList.remove("hyphen");
        }
        boolean sansHyphen = extensionList.contains("nohyphen");
        if (sansHyphen) {
            extensionList.remove("nohyphen");
        }
        String ext = String.join("-", extensionList);
        if (locale.getLanguage().equals("en") && !withHyphen) {
            n = new ICUNumbererEnUnhyphenated();
        } else if (locale.getLanguage().equals("de")) {
            n = new ICUNumberer_de();
        }
        n.config(locale, ext, config);
        return n;
    }

    /**
     * Configure an ICU numberer. This includes possibly substituting a default locale
     * if that requested isn't supported.
     *
     * @param locale              the locale to use
     * @param privateUseExtension the language code extension used to denote required numbering
     * @param config              the system Configuration
     */
    protected synchronized void config(Locale locale, String privateUseExtension, Configuration config) {
        rbnf = new RuleBasedNumberFormat(locale, RuleBasedNumberFormat.SPELLOUT);
        // If the locale isn't the one requested
        // switch to the configuration default locale.
        if (!rbnf.getLocale(ULocale.ACTUAL_LOCALE).getLanguage().equals(locale.getLanguage())) {
            locale = new Locale(config.getDefaultLanguage());
            defaultedLocale = locale;
            rbnf = new RuleBasedNumberFormat(locale, RuleBasedNumberFormat.SPELLOUT);
        }
        rbnfNumber = new RuleBasedNumberFormat(locale, RuleBasedNumberFormat.ORDINAL);
        this.locale = locale;

        String[] rules = rbnf.getRuleSetNames();
        for (String s : rules) {
            if (s.startsWith(spelloutCardinalPattern)) {
                spelloutCardinalRulesets.add(s);
            }
            if (s.startsWith(spelloutOrdinalPattern)) {
                spelloutOrdinalRulesets.add(s);
                ordinalParamIndex.put(s, s);
            }
            if (s.startsWith(spelloutNumberingPattern)) {
                spelloutNumberingRulesets.add(s);
            }
        }
        rules = rbnfNumber.getRuleSetNames();
        for (String s : rules) {
            if (s.startsWith(numberOrdinalPattern)) {
                numberOrdinalRulesets.add(s);
            }
        }

        // We iterate through the preference list to find the 'default'
        // that should be used for appropriate spellout
        // If there is none of the preferences available,
        // then choose any (the first) of the suitable class as default.
        for (String p : preferences) {
            String n = spelloutOrdinalPattern + p;
            if (spelloutOrdinalRulesets.contains(n)) {
                defaultOrdinalRule = n;
                break;
            }
        }
        if (defaultOrdinalRule == null && spelloutOrdinalRulesets.size() > 0) {
            defaultOrdinalRule = spelloutOrdinalRulesets.get(0);
        }
        for (String p : preferences) {
            String n = spelloutCardinalPattern + p;
            if (spelloutCardinalRulesets.contains(n)) {
                defaultCardinalRule = n;
                break;
            }
        }
        if (defaultCardinalRule == null && spelloutCardinalRulesets.size() > 0) {
            defaultCardinalRule = spelloutCardinalRulesets.get(0);
        }
        for (String p : preferences) {
            String n = numberOrdinalPattern + p;
            if (numberOrdinalRulesets.contains(n)) {
                defaultNumericOrdinalRule = n;
                break;
            }
        }
        if (defaultNumericOrdinalRule == null && numberOrdinalRulesets.size() > 0) {
            defaultNumericOrdinalRule = numberOrdinalRulesets.get(0);
        }
        // If there is no ordinal at all, then use the default cardinal
        if (defaultOrdinalRule == null) {
            defaultOrdinalRule = defaultCardinalRule;
        }
        if (!spelloutOrdinalPattern.equals(defaultOrdinalRule)) {
//            if (config.isTiming()) {
//                config.getLogger().info("No plain ordinal:" + locale.getDisplayLanguage() + " using:" + useOrdinal);
//            }
        }
        /* The following code needs JDK 7, which is avoided by decoding the private keys in the caller
        String privateTag = locale.getExtension(Locale.PRIVATE_USE_EXTENSION);
        */
        if (privateUseExtension != null) {
            String scheme = schemeCodes.get(privateUseExtension);
            if (scheme != null) {
                if (spelloutCardinalRulesets.contains(scheme) ||
                    spelloutOrdinalRulesets.contains(scheme) ||
                    spelloutNumberingRulesets.contains(scheme) ||
                    numberOrdinalRulesets.contains(scheme)) {
                    setScheme(scheme);
                } else if (config.isTiming()) {
                    config.getLogger().info("No ICU ruleset" + scheme + " for " + locale.getLanguage());
                }
            }
        }
    }

    /**
     * Returns the locale that was used by default, rather than that requested.
     *
     * @return the locale actually used, null if no default was required.
     */
    @Override
    public Locale defaultedLocale() {
        return defaultedLocale;
    }

    /**
     * Set a common numbering scheme for cardinal and ordinal use
     *
     * @param s the scheme name, e.g. '%spellout-cardinal'
     */
    private void setScheme(String s) {
        defaultCardinalRule = s;
        defaultOrdinalRule = s;
    }

    /**
     * Return all the locales available for collation
     *
     * @return The set of locales as 'code';'display name' pairs
     */
    public static String[] getLocales() {
        ArrayList<String> locales = new ArrayList<>();
        for (Locale l : RuleBasedCollator.getAvailableLocales()) {
            locales.add(ICULibrary.getLanguageTag(l) + ";" + l.getDisplayName());
        }
        return locales.toArray(new String[0]);
    }

    public static String[] getPreferences() {
        return preferences;
    }

    /**
     * Return the available spellout numbering schemes available for a given locale
     *
     * @param loc the locale code
     * @return the numbering scheme names, e.g. '%spellout-verbose'
     */
    public static String[] availableSpelloutNumberings(String loc) {
        RuleBasedNumberFormat temp = new RuleBasedNumberFormat(new Locale(loc), RuleBasedNumberFormat.SPELLOUT);
        return temp.getRuleSetNames();
    }

    /**
     * Return the available ordinal numbering schemes available for a given locale
     *
     * @param loc the locale code
     * @return the numbering scheme names, e.g. '%digits-ordinal'
     */
    public static String[] availableOrdinalNumberings(String loc) {
        RuleBasedNumberFormat temp = new RuleBasedNumberFormat(new Locale(loc), RuleBasedNumberFormat.ORDINAL);
        return temp.getRuleSetNames();
    }

    /**
     * Return the real language to be used for a given locale
     *
     * @param loc the locale code
     * @return the language code actually used
     */
    public static String realLocale(String loc) {
        RuleBasedNumberFormat temp = new RuleBasedNumberFormat(new Locale(loc), RuleBasedNumberFormat.SPELLOUT);
        return temp.getLocale(ULocale.ACTUAL_LOCALE).getLanguage();
    }

    /**
     * Return the version of the ICU supported in this implementation
     *
     * @return the version code as a 'major.minor.milli.micro' string
     */
    public static String getICUVersion() {
        VersionInfo icu = VersionInfo.ICU_VERSION;
        return icu.getMajor() + "." + icu.getMinor()
            + "." + icu.getMilli() + "." + icu.getMicro();
    }

    @Override
    public String ordinalSuffix(String ordinalParam, long number) {
        /* ICU formats the complete number, but the abstract numberer is only looking for the suffix
        (which also makes assumptions that it is indeed just a suffix... )
        The approach is to remove the leading digit/',' sequence, ending in a digit from the ICU result
         */
        if (rbnfNumber != null) {
            String r = defaultNumericOrdinalRule != null ? rbnfNumber.format(number, defaultNumericOrdinalRule) : "";
            return r.replaceAll("^[,\\d]*\\d", "");
        }
        return "UNKNOWN";
    }

    @Override
    public String toWords(String cardinal, long number) {
        if (rbnf != null) {
            String rule = cardinal.startsWith("%") ? cardinal : defaultCardinalRule;
            String s;
            try {
                s = rule != null ? rbnf.format(number, rule) : rbnf.format(number);
            } catch (IllegalArgumentException e) {
                s = rbnf.format(number, defaultCardinalRule);
            }
            return postProcess(s);
        }
        return null;
    }

    @Override
    public String toWords(String cardinal, long number, int wordCase) {
        return postProcess(wordCase(toWords(cardinal, number), wordCase));
    }

    @Override
    public String toOrdinalWords(String ordinalParam, long number, int wordCase) {
        if (rbnf != null) {
            String word;
            if (ordinalParam == null) {
                word = rbnf.format(number);
            } else {
                String rule = ordinalParamIndex.get(ordinalParam);
                if (rule == null) {
                    synchronized (this) {
                        if (ordinalParam.startsWith("-") && ordinalParam.length() >= 2) {
                            // If the ordinalParam starts with "-" we treat it as a required suffix. We
                            // try formatting the number "2" with each possible ruleset, searching for a ruleset
                            // where the result ends with the required suffix. This tells us which ruleset to
                            // use. Once we've worked this out, we remember the association of suffix to ruleset
                            // for future use.
                            String suffix = ordinalParam.substring(1);
                            for (String ruleSet : spelloutOrdinalRulesets) {
                                String two = rbnf.format(2, ruleSet);
                                if (two.endsWith(suffix)) {
                                    rule = ruleSet;
                                    ordinalParamIndex.put(ordinalParam, rule);
                                    break;
                                }
                            }
                        } else if (ordinalParam.startsWith("2=") && ordinalParam.length() >= 3) {
                            // If the ordinalParam starts with "2=" then we treat it as containing the
                            // desired representation of the number 2, and again we use this information
                            // to find the appropriate ruleset which we can then use for all numbers
                            for (String ruleSet : spelloutOrdinalRulesets) {
                                String required = ordinalParam.substring(2);
                                String two = rbnf.format(2, ruleSet);
                                if (two.equalsIgnoreCase(required)) {
                                    rule = ruleSet;
                                    ordinalParamIndex.put(ordinalParam, rule);
                                    break;
                                }
                            }
                        } else if (ordinalParam.startsWith("%")) {
                            rule = ordinalParam;
                        }
                    }
                }
                if (rule == null) {
                    rule = defaultOrdinalRule;
                }
                try {
                    word = rbnf.format(number, rule);
                } catch (IllegalArgumentException e) {
                    word = rbnf.format(number, defaultOrdinalRule);
                }
            }
            return postProcessOrdinal(postProcess(wordCase(word, wordCase)), ordinalParam);
        }
        return null;
    }

    /**
     * Post-process a spelled-out number. Overriding this can add additional effects
     * such as replacing '-' with a space.
     *
     * @param result The spelled-out number generated by ICU
     * @return processed string
     */
    public String postProcess(String result) {
        return result;
    }

    /**
     * Post-process an ordinal spelled-out number. Overriding this can add additional effects, such as
     * gender/case variable suffices in German
     *
     * @param result       The spelled-out ordinal generated by ICU
     * @param ordinalParam The ordinal parameter, if any
     * @return processed string
     */
    public String postProcessOrdinal(String result, String ordinalParam) {
        return result;
    }

    /**
     * Convert words to appropriate word case, using a heuristic, and some additional heuristics regarding joiner words.
     *
     * @param s        the string in words
     * @param wordCase the case to convert to - UPPER, LOWER or TITLE
     * @return processed words
     */
    private String wordCase(String s, int wordCase) {
        if (wordCase == UPPER_CASE) {
            return s.toUpperCase();
        } else if (wordCase == LOWER_CASE) {
            return s.toLowerCase();
        } else if (wordCase == TITLE_CASE) {
            if (s.length() > 1
                && Character.isUpperCase(s.charAt(0))
                && Character.isLowerCase(s.charAt(1))) {
                return s;
            }
            StringBuilder result = new StringBuilder();
            boolean precedingLetter = false;
            for (char c : s.toCharArray()) {
                boolean isLetter = Character.isLetter(c);
                result.append(isLetter ? precedingLetter ? Character.toLowerCase(c) : Character.toUpperCase(c) : c);
                precedingLetter = isLetter || c == 0xAD; // soft hyphen
            }
            /* Lower case all the 'and' words - note that this may not be correct for all languages,
             * and the technique relies on none of the number/day/month words starting with the 'and'
             * character sequence, which might have been a particular problem in Croatian, Catalan,
             * Portugese and Maltese, where the joiner is a single character, but that appears
             * not to be the case from the ICU rules, at least in the common cases
             *
             * With some others, notably Irish Gaelic, there can be conflict (e.g. 'Aibreán' (April)),
             * so the replacement includes a trailing space
             **/
            String lang = locale.getLanguage();
            switch (lang) {
                case "en":
                    result = new StringBuilder(result.toString().replace("And", "and"));
                    break;
                case "de":
                    result = new StringBuilder(result.toString().replace("Und", "und"));
                    break;
                case "nl":
                    result = new StringBuilder(result.toString().replace("Ën", "ën"));
                    break;
                case "af":
                    result = new StringBuilder(result.toString().replace("En", "en"));
                    break;
                case "da":
                case "fo":
                case "is":
                case "nb":
                case "nn":
                    result = new StringBuilder(result.toString().replace("Og", "og"));
                    break;
                case "ga":
                    result = new StringBuilder(result.toString().replace("A ", "a "));
                    break;
                case "hr":
                case "ca":
                    result = new StringBuilder(result.toString().replace("I", "i"));
                    break;
                case "bg":
                case "mk":
                case "sr":
                    result = new StringBuilder(result.toString().replace("И ", "и "));
                    break;
                case "mt":
                    result = new StringBuilder(result.toString().replace("U", "u"));
                    break;
                case "pt":
                case "sq":
                    result = new StringBuilder(result.toString().replace("E", "e"));
                    break;
                case "ro":
                    result = new StringBuilder(result.toString().replace("Şi", "şi"));
                    break;
                case "ee":
                    result = new StringBuilder(result.toString().replace("Vɔ", "vɔ"));
                    break;
                case "fi":
                    result = new StringBuilder(result.toString().replace("At", "at"));
                    break;
            }
            return result.toString();
        } else {
            return s;
        }
    }

    /**
     * Return an appropriate month name, using ICU data
     * The approach is to try each of the forms available in descending order of 'size'
     * until we have the first that fits the width constraints.
     *
     * @param month    The month number (1=January, 12=December)
     * @param minWidth The minimum number of characters
     * @param maxWidth The maximum number of characters
     * @return The month name in TITLE case
     */
    @Override
    public String monthName(int month, int minWidth, int maxWidth) {
        com.ibm.icu.util.GregorianCalendar cal = new com.ibm.icu.util.GregorianCalendar(locale);
        cal.set(1900, month - 1, 1);
        Date d = cal.getTime();
        String result = getDateFormat(MONTH_FULL).format(d);
        if (result.length() > maxWidth) {
            result = getDateFormat(MONTH_SHORT).format(d);
        }
        if (result.length() > maxWidth) {
            //result = getDateFormat(MONTH_NARROW).format(d);
            result = result.substring(0, maxWidth);
        }
        return wordCase(result, TITLE_CASE);
    }

    /**
     * Return an appropriate day name, using ICU data
     * The approach is to try each of the forms available in descending order of 'size'
     * until we have the first that fits the width constraints.
     *
     * @param day      The day of the week (1=Monday, 7=Sunday)
     * @param minWidth The minimum number of characters
     * @param maxWidth The maximum number of characters
     * @return The day name in TITLE case
     */
    @Override
    public String dayName(int day, int minWidth, int maxWidth) {
        com.ibm.icu.util.GregorianCalendar cal = new com.ibm.icu.util.GregorianCalendar(locale);
        cal.set(1900, Calendar.JANUARY, day);
        Date d = cal.getTime();
        String result = getDateFormat(DAY_FULL).format(d);
        if (result.length() > maxWidth) {
            result = getDateFormat(DAY_SHORT).format(d);
        }
        if (result.length() > maxWidth) {
            result = result.substring(0, maxWidth);
//            result = getDateFormat(DAY_SHORT2).format(d);
        }
//        if (result.length() > maxWidth) {
//            result = getDateFormat(DAY_NARROW).format(d);
//        }
        return wordCase(result, TITLE_CASE);
    }
}

// Copyright (c) 2018-2022 Saxonica Limited
