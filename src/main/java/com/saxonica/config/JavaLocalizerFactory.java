package com.saxonica.config;

import com.saxonica.config.pe.ICUNumbererPE;
import net.sf.saxon.Configuration;
import net.sf.saxon.Version;
import net.sf.saxon.expr.number.Numberer_en;
import net.sf.saxon.lib.LocalizerFactory;
import net.sf.saxon.lib.Numberer;
import net.sf.saxon.str.StringView;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.ConversionResult;
import net.sf.saxon.type.StringConverter;
import net.sf.saxon.type.ValidationFailure;

import java.util.HashMap;
import java.util.Locale;
import java.util.Properties;


/**
 * Default factory for number/date localizers for the Java platform
 */
public class JavaLocalizerFactory extends LocalizerFactory {

    private final Configuration config;

    private HashMap<String, String> numbererClassNames = new HashMap<>(10);
    private HashMap<String, Numberer> numberers = new HashMap<>(10);


    public JavaLocalizerFactory(Configuration config) {
        this.config = config;
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
        String className = properties.getProperty("class");
        if (className != null) {
            setLocalizationClass(lang, className);
        }
    }


    /**
     * Set the name of the Localization class to be used for a particular language
     *
     * @param language  the language code, in ISO format, for example "fr" or "fr-CA"
     * @param className the name of the localization class. This must be a class that implements
     *                  the interface {@link net.sf.saxon.lib.Numberer}; however, no error is reported if
     *                  the class cannot be loaded or does not implement this interface; the system simply
     *                  falls back to using the default localization class.
     * @since 9.2
     */

    public void setLocalizationClass(String language, String className) {
        numbererClassNames.put(language.toLowerCase(), className);
    }

    /**
     * Get the numberer for a given language
     *
     * @param language the language code (for example "de" or "en-GB"
     * @param country  the country, as used in format-date(). This is not the country associated
     *                 with the language, but the one associated with the date to be formatted. It is primarily
     *                 used to determine a civil time zone name.
     * @return the appropriate numberer, or null if none is available (in which case the English
     * numberer will be used)
     */

    @Override
    public Numberer getNumberer(/*@Nullable*/ String language, String country) {

        String key;
        if (language != null) {
            ConversionResult cr = StringConverter.StringToLanguage.INSTANCE.convertString(StringView.of(language).tidy());
            if (cr instanceof ValidationFailure) {
                // See W3C bug 20663
                language = null;
            }
        }
        if (language == null) {
            language = config.getDefaultLanguage();
        }
        if (country == null) {
            country = config.getDefaultCountry();
        }
        key = (language + "|" + country).toLowerCase();
        Numberer numberer = numberers.get(key);


        if ((numberer == null) &&
                Version.platform.hasICUNumberer() &&
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

            numberers.put(key, numberer);
        }


        if (numberer == null) {
            /*Look through the loaded classes for a language match
            (In -HE you'll have to do this by hand)    */
            language = language.replaceAll("-x-.+$", "");// Remove private keys
            String[] candidates = {language, config.getDefaultLanguage()};
            String className = null;
            outer:
            for (int i = 0; i < 2; i++) {
                String lang = candidates[i].toLowerCase();
                boolean breakOuter = false;
                while (true) {
                    className = numbererClassNames.get(lang);
                    if (className != null) {
                        breakOuter = true;
                        break;
                    }
                    if ("en".equals(lang)) {
                        className = "net.sf.saxon.expr.number.Numberer_en";
                        break outer;
                    }
                    int hyphen = lang.indexOf("-");
                    if (hyphen < 0) {
                        break;
                    }
                    lang = lang.substring(0, hyphen);
                }
                if (breakOuter) {
                    break;
                }
            }
            if (className == null) {
                className = "net.sf.saxon.expr.number.Numberer_en";
            }
            try {
                numberer = (Numberer) config.getInstance(className);
            } catch (XPathException err) {
                numberer = new Numberer_en();
            }
            numberer.setCountry(country);
            numberers.put(key, numberer);
        }

        return numberer;
    }

    /**
     * Copy the state of this factory to create a new LocalizerFactory
     */

    @Override
    public JavaLocalizerFactory copy() {
        JavaLocalizerFactory f = new JavaLocalizerFactory(config);
        f.numbererClassNames = new HashMap<>(numbererClassNames);
        f.numberers = new HashMap<>(10);
        return f;
    }
}

// Copyright (c) 2009-2022 Saxonica Limited



