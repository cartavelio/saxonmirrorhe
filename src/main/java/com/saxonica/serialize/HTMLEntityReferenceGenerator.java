package com.saxonica.serialize;

import net.sf.saxon.serialize.CharacterReferenceGenerator;
import net.sf.saxon.serialize.HexCharacterReferenceGenerator;
import net.sf.saxon.str.UnicodeWriter;

/**
 * A class that represents a character as a decimal character reference and writes
 * the result to a supplied Writer.
 */
public class HTMLEntityReferenceGenerator implements CharacterReferenceGenerator {

    private static final String[] latin1Entities = {

            "nbsp",   // "&#160;" -- no-break space = non-breaking space,
            //                        U+00A0 ISOnum -->
            "iexcl",  // "&#161;" -- inverted exclamation mark, U+00A1 ISOnum -->
            "cent",   // "&#162;" -- cent sign, U+00A2 ISOnum -->
            "pound",  // "&#163;" -- pound sign, U+00A3 ISOnum -->
            "curren", // "&#164;" -- currency sign, U+00A4 ISOnum -->
            "yen",    // "&#165;" -- yen sign = yuan sign, U+00A5 ISOnum -->
            "brvbar", // "&#166;" -- broken bar = broken vertical bar,
            //                        U+00A6 ISOnum -->
            "sect",   // "&#167;" -- section sign, U+00A7 ISOnum -->
            "uml",    // "&#168;" -- diaeresis = spacing diaeresis,
            //                        U+00A8 ISOdia -->
            "copy",   // "&#169;" -- copyright sign, U+00A9 ISOnum -->
            "ordf",   // "&#170;" -- feminine ordinal indicator, U+00AA ISOnum -->
            "laquo",  // "&#171;" -- left-pointing double angle quotation mark
            //                        = left pointing guillemet, U+00AB ISOnum -->
            "not",    // "&#172;" -- not sign, U+00AC ISOnum -->
            "shy",    // "&#173;" -- soft hyphen = discretionary hyphen,
            //                        U+00AD ISOnum -->
            "reg",    // "&#174;" -- registered sign = registered trade mark sign,
            //                        U+00AE ISOnum -->
            "macr",   // "&#175;" -- macron = spacing macron = overline
            //                        = APL overbar, U+00AF ISOdia -->
            "deg",    // "&#176;" -- degree sign, U+00B0 ISOnum -->
            "plusmn", // "&#177;" -- plus-minus sign = plus-or-minus sign,
            //                        U+00B1 ISOnum -->
            "sup2",   // "&#178;" -- superscript two = superscript digit two
            //                        = squared, U+00B2 ISOnum -->
            "sup3",   // "&#179;" -- superscript three = superscript digit three
            //                        = cubed, U+00B3 ISOnum -->
            "acute",  // "&#180;" -- acute accent = spacing acute,
            //                       U+00B4 ISOdia -->
            "micro",  // "&#181;" -- micro sign, U+00B5 ISOnum -->
            "para",   // "&#182;" -- pilcrow sign = paragraph sign,
            //                        U+00B6 ISOnum -->
            "middot", // "&#183;" -- middle dot = Georgian comma
            //                        = Greek middle dot, U+00B7 ISOnum -->
            "cedil",  // "&#184;" -- cedilla = spacing cedilla, U+00B8 ISOdia -->
            "sup1",   // "&#185;" -- superscript one = superscript digit one,
            //                        U+00B9 ISOnum -->
            "ordm",   // "&#186;" -- masculine ordinal indicator,
            //                        U+00BA ISOnum -->
            "raquo",  // "&#187;" -- right-pointing double angle quotation mark
            //                        = right pointing guillemet, U+00BB ISOnum -->
            "frac14", // "&#188;" -- vulgar fraction one quarter
            //                        = fraction one quarter, U+00BC ISOnum -->
            "frac12", // "&#189;" -- vulgar fraction one half
            //                        = fraction one half, U+00BD ISOnum -->
            "frac34", // "&#190;" -- vulgar fraction three quarters
            //                        = fraction three quarters, U+00BE ISOnum -->
            "iquest", // "&#191;" -- inverted question mark
            //                        = turned question mark, U+00BF ISOnum -->
            "Agrave", // "&#192;" -- latin capital letter A with grave
            //                        = latin capital letter A grave,
            //                        U+00C0 ISOlat1 -->
            "Aacute", // "&#193;" -- latin capital letter A with acute,
            //                        U+00C1 ISOlat1 -->
            "Acirc",  // "&#194;" -- latin capital letter A with circumflex,
            //                        U+00C2 ISOlat1 -->
            "Atilde", // "&#195;" -- latin capital letter A with tilde,
            //                        U+00C3 ISOlat1 -->
            "Auml",   // "&#196;" -- latin capital letter A with diaeresis,
            //                        U+00C4 ISOlat1 -->
            "Aring",  // "&#197;" -- latin capital letter A with ring above
            //                        = latin capital letter A ring,
            //                        U+00C5 ISOlat1 -->
            "AElig",  // "&#198;" -- latin capital letter AE
            //                        = latin capital ligature AE,
            //                        U+00C6 ISOlat1 -->
            "Ccedil", // "&#199;" -- latin capital letter C with cedilla,
            //                        U+00C7 ISOlat1 -->
            "Egrave", // "&#200;" -- latin capital letter E with grave,
            //                        U+00C8 ISOlat1 -->
            "Eacute", // "&#201;" -- latin capital letter E with acute,
            //                        U+00C9 ISOlat1 -->
            "Ecirc",  // "&#202;" -- latin capital letter E with circumflex,
            //                        U+00CA ISOlat1 -->
            "Euml",   // "&#203;" -- latin capital letter E with diaeresis,
            //                        U+00CB ISOlat1 -->
            "Igrave", // "&#204;" -- latin capital letter I with grave,
            //                        U+00CC ISOlat1 -->
            "Iacute", // "&#205;" -- latin capital letter I with acute,
            //                        U+00CD ISOlat1 -->
            "Icirc",  // "&#206;" -- latin capital letter I with circumflex,
            //                        U+00CE ISOlat1 -->
            "Iuml",   // "&#207;" -- latin capital letter I with diaeresis,
            //                        U+00CF ISOlat1 -->
            "ETH",    // "&#208;" -- latin capital letter ETH, U+00D0 ISOlat1 -->
            "Ntilde", // "&#209;" -- latin capital letter N with tilde,
            //                        U+00D1 ISOlat1 -->
            "Ograve", // "&#210;" -- latin capital letter O with grave,
            //                        U+00D2 ISOlat1 -->
            "Oacute", // "&#211;" -- latin capital letter O with acute,
            //                        U+00D3 ISOlat1 -->
            "Ocirc",  // "&#212;" -- latin capital letter O with circumflex,
            //                        U+00D4 ISOlat1 -->
            "Otilde", // "&#213;" -- latin capital letter O with tilde,
            //                        U+00D5 ISOlat1 -->
            "Ouml",   // "&#214;" -- latin capital letter O with diaeresis,
            //                        U+00D6 ISOlat1 -->
            "times",  // "&#215;" -- multiplication sign, U+00D7 ISOnum -->
            "Oslash", // "&#216;" -- latin capital letter O with stroke
            //                        = latin capital letter O slash,
            //                        U+00D8 ISOlat1 -->
            "Ugrave", // "&#217;" -- latin capital letter U with grave,
            //                        U+00D9 ISOlat1 -->
            "Uacute", // "&#218;" -- latin capital letter U with acute,
            //                        U+00DA ISOlat1 -->
            "Ucirc",  // "&#219;" -- latin capital letter U with circumflex,
            //                        U+00DB ISOlat1 -->
            "Uuml",   // "&#220;" -- latin capital letter U with diaeresis,
            //                        U+00DC ISOlat1 -->
            "Yacute", // "&#221;" -- latin capital letter Y with acute,
            //                        U+00DD ISOlat1 -->
            "THORN",  // "&#222;" -- latin capital letter THORN,
            //                        U+00DE ISOlat1 -->
            "szlig",  // "&#223;" -- latin small letter sharp s = ess-zed,
            //                        U+00DF ISOlat1 -->
            "agrave", // "&#224;" -- latin small letter a with grave
            //                        = latin small letter a grave,
            //                        U+00E0 ISOlat1 -->
            "aacute", // "&#225;" -- latin small letter a with acute,
            //                        U+00E1 ISOlat1 -->
            "acirc",  // "&#226;" -- latin small letter a with circumflex,
            //                        U+00E2 ISOlat1 -->
            "atilde", // "&#227;" -- latin small letter a with tilde,
            //                        U+00E3 ISOlat1 -->
            "auml",   // "&#228;" -- latin small letter a with diaeresis,
            //                        U+00E4 ISOlat1 -->
            "aring",  // "&#229;" -- latin small letter a with ring above
            //                        = latin small letter a ring,
            //                        U+00E5 ISOlat1 -->
            "aelig",  // "&#230;" -- latin small letter ae
            //                        = latin small ligature ae, U+00E6 ISOlat1 -->
            "ccedil", // "&#231;" -- latin small letter c with cedilla,
            //                        U+00E7 ISOlat1 -->
            "egrave", // "&#232;" -- latin small letter e with grave,
            //                        U+00E8 ISOlat1 -->
            "eacute", // "&#233;" -- latin small letter e with acute,
            //                        U+00E9 ISOlat1 -->
            "ecirc",  // "&#234;" -- latin small letter e with circumflex,
            //                        U+00EA ISOlat1 -->
            "euml",   // "&#235;" -- latin small letter e with diaeresis,
            //                        U+00EB ISOlat1 -->
            "igrave", // "&#236;" -- latin small letter i with grave,
            //                        U+00EC ISOlat1 -->
            "iacute", // "&#237;" -- latin small letter i with acute,
            //                        U+00ED ISOlat1 -->
            "icirc",  // "&#238;" -- latin small letter i with circumflex,
            //                        U+00EE ISOlat1 -->
            "iuml",   // "&#239;" -- latin small letter i with diaeresis,
            //                        U+00EF ISOlat1 -->
            "eth",    // "&#240;" -- latin small letter eth, U+00F0 ISOlat1 -->
            "ntilde", // "&#241;" -- latin small letter n with tilde,
            //                        U+00F1 ISOlat1 -->
            "ograve", // "&#242;" -- latin small letter o with grave,
            //                        U+00F2 ISOlat1 -->
            "oacute", // "&#243;" -- latin small letter o with acute,
            //                        U+00F3 ISOlat1 -->
            "ocirc",  // "&#244;" -- latin small letter o with circumflex,
            //                        U+00F4 ISOlat1 -->
            "otilde", // "&#245;" -- latin small letter o with tilde,
            //                        U+00F5 ISOlat1 -->
            "ouml",   // "&#246;" -- latin small letter o with diaeresis,
            //                        U+00F6 ISOlat1 -->
            "divide", // "&#247;" -- division sign, U+00F7 ISOnum -->
            "oslash", // "&#248;" -- latin small letter o with stroke,
            //                        = latin small letter o slash,
            //                        U+00F8 ISOlat1 -->
            "ugrave", // "&#249;" -- latin small letter u with grave,
            //                        U+00F9 ISOlat1 -->
            "uacute", // "&#250;" -- latin small letter u with acute,
            //                        U+00FA ISOlat1 -->
            "ucirc",  // "&#251;" -- latin small letter u with circumflex,
            //                        U+00FB ISOlat1 -->
            "uuml",   // "&#252;" -- latin small letter u with diaeresis,
            //                        U+00FC ISOlat1 -->
            "yacute", // "&#253;" -- latin small letter y with acute,
            //                        U+00FD ISOlat1 -->
            "thorn",  // "&#254;" -- latin small letter thorn,
            //                        U+00FE ISOlat1 -->
            "yuml"    // "&#255;" -- latin small letter y with diaeresis,
            //                        U+00FF ISOlat1 -->
    };

    public final static HTMLEntityReferenceGenerator HEX =
            new HTMLEntityReferenceGenerator(HexCharacterReferenceGenerator.THE_INSTANCE);

    public final static HTMLEntityReferenceGenerator DECIMAL =
            new HTMLEntityReferenceGenerator(DecimalCharacterReferenceGenerator.THE_INSTANCE);

    private final CharacterReferenceGenerator fallback;

    /**
     * Create an instance of this class with a defined fallback for use when no named entity
     * is known
     *
     * @param fallback the generator to be used when no named entity is available: typically either
     *                 a hex character reference generator, or a decimal character reference generator
     */

    private HTMLEntityReferenceGenerator(CharacterReferenceGenerator fallback) {
        this.fallback = fallback;
    }

    @Override
    public void outputCharacterReference(int c, UnicodeWriter writer) throws java.io.IOException {
        if (c > 160 && c <= 255) {
            // if chararacter in iso-8859-1, use an entity reference
            writer.writeCodePoint('&');
            writer.write(latin1Entities[c - 160]);
            writer.writeCodePoint(';');
        } else {
            fallback.outputCharacterReference(c, writer);
        }
    }
}

// Copyright (c) 2011-2022 Saxonica Limited