package com.saxonica.config;

import com.saxonica.config.pe.ICUNumbererPE;
import net.sf.saxon.Configuration;
import net.sf.saxon.Version;
import net.sf.saxon.expr.number.Numberer_en;
import net.sf.saxon.lib.LocalizerFactory;
import net.sf.saxon.lib.Numberer;
import net.sf.saxon.option.local.*;

import java.util.HashMap;
import java.util.Locale;
import java.util.Properties;

/**
 * Default factory for number/date localizers for the .NET platform.
 * This has built-in knowledge of the available localizations, it does not attempt dynamic loading.
 * Note that these are available only in Saxon-PE and Saxon-EE.
 */
public class DotNetLocalizerFactory extends LocalizerFactory {

    private final Configuration config;

    private HashMap<String, Class<? extends Numberer>> numbererClasses =
            new HashMap<String, Class<? extends Numberer>>(10);
    private final HashMap<String, Numberer> numbererInstances =
            new HashMap<String, Numberer>(10);

    public DotNetLocalizerFactory(Configuration config) {
        this.config = config;
        numbererClasses.put("en", Numberer_en.class);
        numbererClasses.put("da", Numberer_da.class);
        numbererClasses.put("de", Numberer_de.class);
        numbererClasses.put("fr", Numberer_fr.class);
        numbererClasses.put("frbe", Numberer_frBE.class);
        numbererClasses.put("it", Numberer_it.class);
        numbererClasses.put("nl", Numberer_nl.class);
        numbererClasses.put("nlBE", Numberer_nlBE.class);
        numbererClasses.put("sv", Numberer_sv.class);
    }

    /**
     * Set properties for a particular language. The properties available are specific to the
     * LocalizerFactory in use. Default implementation does nothing.
     *
     * @param lang       the language
     * @param properties properties of this language
     * @since 9.2
     */

    @Override
    public void setLanguageProperties(String lang, Properties properties) {
        // no action
    }

    /**
     * Get the numberer for a given language
     *
     * @param language the language code (for example "de" or "en-GB")
     * @param country  the country, as used in format-date(). This is not the country associated
     *                 with the language, but the one associated with the date to be formatted. It is primarily
     *                 used to determine a civil time zone name.
     * @return the appropriate numberer, or null if none is available (in which case the English
     *         numberer will be used)
     */

    /*@Nullable*/
    @Override
    public Numberer getNumberer(String language, String country) {

        String defaultLanguage = config.getDefaultLanguage();
        if (language == null) {
            language = defaultLanguage;
        }
        String defaultCountry = config.getDefaultCountry();
        if (country == null) {
            country = defaultCountry;
        }
        String classKey = language.toLowerCase();
        String instanceKey = classKey + "|" + country.toLowerCase();
        Numberer numberer = numbererInstances.get(instanceKey);
        if (numberer != null) {
            return numberer;
        }

        if (Version.platform.hasICUNumberer() &&
                config.isLicensedFeature(Configuration.LicenseFeature.PROFESSIONAL_EDITION)) {
            // We emulate some of the JDK7 features for handling private extension keys.
            // and pass the private keys separately from the Locale
            // We also have to deconstruct the language-country pairing.
            String[] parts = language.split("-x-");
            String[] langRegion = parts[0].split("-");
            String privateKeys = null;
            if (parts.length == 2) {
                privateKeys = parts[1];
            }
            String baseLang = langRegion[0].toLowerCase();
            Locale loc;
            if (langRegion.length == 2) {
                loc = new Locale(baseLang, langRegion[1].toUpperCase());
            } else {
                loc = new Locale(baseLang);
            }

            numberer = ICUNumbererPE.getNumberer(loc, privateKeys, config);

            numbererInstances.put(instanceKey, numberer);

            return numberer;
        }

        Class numbererClass = numbererClasses.get(classKey);
        if (numbererClass == null) {
            if (country.equals(defaultCountry) && language.equals(defaultLanguage)) {
                return null;
            } else {
                return getNumberer(defaultLanguage, defaultCountry);
            }
        }
        try {
            numberer = (Numberer) numbererClass.newInstance();
            numberer.setCountry(country);
            numbererInstances.put(instanceKey, numberer);
            return numberer;
        } catch (InstantiationException e) {
            return new Numberer_en();
        } catch (IllegalAccessException e) {
            return new Numberer_en();
        }
    }

    /**
     * Copy the state of this factory to create a new LocalizerFactory
     */

    @Override
    public DotNetLocalizerFactory copy() {
        DotNetLocalizerFactory f = new DotNetLocalizerFactory(config);
        f.numbererClasses = numbererClasses;
        return f;
    }
}


// Copyright (c) 2009-2023 Saxonica Limited