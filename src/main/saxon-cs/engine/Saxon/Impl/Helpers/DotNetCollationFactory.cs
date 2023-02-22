using System;
using System.Globalization;
using Saxon.Eej.expr.sort;
using Saxon.Ejava.util;
using Saxon.Hej;
using Saxon.Hej.expr.sort;
using Saxon.Hej.lib;
using Saxon.Hej.trans;

namespace Saxon.Impl.Helpers {
    public static class DotNetCollationFactory {
        /**
         * Make a collator with given properties
         *
         * @param config the Configuration
         * @param uri the collation URI
         * @param props  the desired properties of the collation
         * @return a collation with these properties
         * @throws Saxon.Hej.trans.XPathException
         *          if it is not possible to create a collation with the
         *          requested properties
         */

        /*@Nullable*/
        public static StringCollator makeCollation(Configuration config, string uri, Properties props) {

            if (uri.StartsWith("http://www.w3.org/2013/collation/UCA")) {
                return new UcaCollatorUsingIcu(uri);
            }

            StringCollator stringCollator = null;
            
            // rules attribute works only on Java platform

            if (props.getProperty("rules") != null) {
                throw new XPathException("Rules parameter is not supported in collation URIs on Saxon-CS");
            }

            // Start with the lang attribute

            CompareInfo info;
            CompareOptions options = CompareOptions.None;

            string langAtt = props.getProperty("lang");
            if (langAtt != null) {
                info = CultureInfo.CreateSpecificCulture(langAtt).CompareInfo;
            } else {
                info = CultureInfo.CurrentCulture.CompareInfo;  // use default locale
            }

            // See if there is a strength attribute
            string strengthAtt = props.getProperty("strength");
            if (strengthAtt != null && options == CompareOptions.None) {
                options = strengthAtt switch {
                    "primary" => System.Globalization.CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace |
                                 CompareOptions.IgnoreWidth,
                    "secondary" => CompareOptions.IgnoreCase | CompareOptions.IgnoreWidth,
                    "tertiary" => CompareOptions.IgnoreWidth,
                    "identical" => 0,
                    _ => throw new XPathException("strength must be primary, secondary, tertiary, or identical")
                };
            }

            // Look for the properties ignore-case, ignore-modifiers, ignore-width

            string ignore = props.getProperty("ignore-case");
            if (ignore != null) {
                if (ignore == "yes") {
                    options |= CompareOptions.IgnoreCase;
                } else if (ignore == "no") {
                    // no-op
                } else {
                    throw new XPathException("ignore-case must be yes or no");
                }
            }

            ignore = props.getProperty("ignore-modifiers");
            if (ignore != null) {
                if (ignore == "yes") {
                    options |= CompareOptions.IgnoreNonSpace;
                } else if (ignore == "no") {
                    // no-op
                } else {
                    throw new XPathException("ignore-modifiers must be yes or no");
                }
            }

            ignore = props.getProperty("ignore-symbols");
            if (ignore != null) {
                if (ignore == "yes") {
                    options |= CompareOptions.IgnoreSymbols;
                } else if (ignore == "no") {
                    // no-op
                } else {
                    throw new XPathException("ignore-symbols must be yes or no");
                }
            }

            ignore = props.getProperty("ignore-width");
            if (ignore != null) {
                if (ignore == "yes") {
                    options |= CompareOptions.IgnoreWidth;
                } else if (ignore == "no") {
                    // no-op
                } else {
                    throw new XPathException("ignore-width must be yes or no");
                }
            }

            if (stringCollator == null) {
                stringCollator = new DotNetComparator(uri, info, options);
            }

            // See if there is a case-order property
            string caseOrder = props.getProperty("case-order");
            if (caseOrder != null && "#default" != caseOrder) {
                // force the base collation to ignore case
                options |= CompareOptions.IgnoreCase;
                stringCollator = new DotNetComparator(uri, info, options);
                stringCollator = CaseFirstCollator.makeCaseOrderedCollator(uri, stringCollator, caseOrder);
            }

            // See if there is an alphanumeric property
            string alphanumeric = props.getProperty("alphanumeric");
            if (alphanumeric != null && "no" != alphanumeric) {
                if (alphanumeric == "yes") {
                    stringCollator = new AlphanumericCollator(stringCollator);
                } else if (alphanumeric == "codepoint") {
                    stringCollator = new AlphanumericCollator(CodepointCollator.getInstance());
                } else {
                    throw new XPathException("alphanumeric must be yes, no, or codepoint");
                }
            }

            return stringCollator;
        }        
    }
}

// Copyright (c) 2021 Saxonica Limited