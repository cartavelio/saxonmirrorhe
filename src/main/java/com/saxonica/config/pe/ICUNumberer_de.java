
package com.saxonica.config.pe;

import net.sf.saxon.Configuration;

import java.util.Locale;

/**
 * A numberer for German using the ICU features, which supports variable ordinal suffixing, for example '-en', '-es'
 */
public class ICUNumberer_de extends ICUNumbererPE {

    @Override
    public void config(Locale locale, String privateUseExtension, Configuration config) {
        super.config(locale, privateUseExtension, config);
    }

    /**
     * Replace trailing 'e' on German ordinals with the post-hyphen suffix, e.g. '-en'
     *
     * @param result the raw result as delivered by ICU
     * @param ordinalParam the requested ordinal form, e.g. "-er" or "-es"
     * @return New ordinal as words, with appropriate suffix
     */
    @Override
    public String postProcessOrdinal(String result, String ordinalParam) {
        if (ordinalParam.startsWith("-")) {
            if (result.endsWith("e")) {
                return result.substring(0, result.length() - 1) + ordinalParam.toLowerCase().substring(1);
            } else if (result.endsWith("E")) {
                return result.substring(0, result.length() - 1) + ordinalParam.toUpperCase().substring(1);
            } else {
                return result;
            }
        } else {
            return result;
        }
    }
}

// Copyright (c) 2018-2022 Saxonica Limited
