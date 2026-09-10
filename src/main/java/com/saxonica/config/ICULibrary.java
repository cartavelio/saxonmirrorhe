package com.saxonica.config;

import com.saxonica.expr.sort.UcaCollatorUsingIcu;
import net.sf.saxon.Configuration;
import net.sf.saxon.lib.StringCollator;
import net.sf.saxon.trans.XPathException;

import java.lang.reflect.InvocationTargetException;
import java.lang.reflect.Method;
import java.util.Locale;

/**
 * Class to report whether ICU features are loaded.
 * This will be used by the PE Platform to check ICU presence
 */
public class ICULibrary {
    // Facilities to report whether the ICU facilities are available
    private static boolean ICUCollatorAvailable = false;
    private static boolean ICUNumbererAvailable = false;

    static {
        try {
            Class t = Class.forName("com.ibm.icu.text.RuleBasedCollator");
            ICUCollatorAvailable = true;
        } catch (ClassNotFoundException e) {
            ICUCollatorAvailable = false;
        }
        try {
            Class t = Class.forName("com.ibm.icu.text.RuleBasedNumberFormat");
            ICUNumbererAvailable = true;
        } catch (ClassNotFoundException e) {
            ICUNumbererAvailable = false;
        }

    }

    /**
     * Is an ICU collator loaded?
     * @return true if so
     */
    public static boolean isICUCollatorAvailable() {
        return ICUCollatorAvailable;
    }
    /**
     * Is an ICU numberer loaded?
     * @return true if so
     */
    public static boolean hasNumberer() {
        return ICUNumbererAvailable;
    }


    /**
     * If available, make a collation using the ICU-J Library
     *
     * @param uri    the collation URI (which will always be a UCA collation URI as defined in XSLT 3.0)
     * @param config the Saxon configuration
     * @return the collation, or null if not available
     * @throws net.sf.saxon.trans.XPathException if the URI is malformed in some way
     */
    public static StringCollator makeICUCollator(String uri, Configuration config) throws XPathException {
        if (config.isLicensedFeature(Configuration.LicenseFeature.PROFESSIONAL_EDITION)
                && isICUCollatorAvailable()) {
            return new UcaCollatorUsingIcu(uri);
        } else {
            return null;
        }
    }

    public static String getLanguageTag(Locale locale) {
        try {
            Method m = Locale.class.getMethod("toLanguageTag");  // JDK 1.7 method
            return (String) m.invoke(locale);
        } catch (NoSuchMethodException e) {
            //
        } catch (IllegalAccessException e) {
            //
        } catch (InvocationTargetException e) {
            //
        }
        // JDK 1.6 fallback
        String rep = locale.toString();
        int firstUnderscore = rep.indexOf('_');
        if(firstUnderscore < 0 ){
            return rep;
        }
        int secondUnderscore = rep.indexOf('_', firstUnderscore + 1);
        if(secondUnderscore < 0) {
            return rep.replace('_','-');
        }
        return rep.substring(0, secondUnderscore).replace('_', '-');

    }
}

// Copyright (c) 2018-2023 Saxonica Limited
