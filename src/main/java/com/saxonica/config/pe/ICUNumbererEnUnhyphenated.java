
package com.saxonica.config.pe;

/**
 * A numberer for English, using the ICU features, which produces space-separated word numbers,
 * for example "Twenty Two" rather than the ICU default, which is hyphen-separated: "Twenty-Two".
 * Used by default, but not when the language code is "en-x-hyphen".
 */
public class ICUNumbererEnUnhyphenated extends ICUNumbererPE {

    /**
     * Replace '-' in compound numeric words in English by ' '
     *
     * @param s the word string to process
     * @return processed string
     */
    @Override
    public String postProcess(String s) {
        return s.replaceAll("-", " ");
    }
}

// Copyright (c) 2018-2022 Saxonica Limited
