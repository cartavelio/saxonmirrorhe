using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    #nullable enable
    public class URIQueryParameters {
        internal Saxon.Ejava.util.Optional<Saxon.Ejava.io.FilenameFilter> filter = Saxon.Ejava.util.Optional<Saxon.Ejava.io.FilenameFilter>.empty();
        internal Saxon.Ejava.util.Optional<bool> recurse = Saxon.Ejava.util.Optional<bool>.empty();
        internal Saxon.Ejava.util.Optional<int> validation = Saxon.Ejava.util.Optional<int>.empty();
        internal Saxon.Ejava.util.Optional<Saxon.Hej.om.SpaceStrippingRule> strippingRule = Saxon.Ejava.util.Optional<Saxon.Hej.om.SpaceStrippingRule>.empty();
        internal Saxon.Ejava.util.Optional<int> onError = Saxon.Ejava.util.Optional<int>.empty();
        internal Saxon.Ejava.util.Optional<bool> xinclude = Saxon.Ejava.util.Optional<bool>.empty();
        internal Saxon.Ejava.util.Optional<bool> stable = Saxon.Ejava.util.Optional<bool>.empty();
        internal Saxon.Ejava.util.Optional<bool> metadata = Saxon.Ejava.util.Optional<bool>.empty();
        internal Saxon.Ejava.util.Optional<string> contentType = Saxon.Ejava.util.Optional<string>.empty();
        public const int ON_ERROR_FAIL = 1;
        public const int ON_ERROR_WARNING = 2;
        public const int ON_ERROR_IGNORE = 3;
        public URIQueryParameters(string query, Saxon.Hej.Configuration config)  {
            if ((query != null)) {
                Saxon.Ejava.util.StringTokenizer t = new Saxon.Ejava.util.StringTokenizer(query, ";&");
                while (t.hasMoreTokens()) {
                    string tok = t.nextToken();
                    int eq = tok.IndexOf('=');
                    if (((eq>0) && (eq<((tok.Length-1))))) {
                        string keyword = tok.Substring(0, eq - 0);
                        string value = tok.Substring((eq+1));
                        processParameter(config, keyword, value);
                    }
                }
            }
        }
        private void processParameter(Saxon.Hej.Configuration config, string keyword, string value) {
            if (keyword.Equals("select")) {
                filter = Saxon.Ejava.util.Optional<Saxon.Ejava.io.FilenameFilter>.of(makeGlobFilter(value));
            } else {
                if (keyword.Equals("match")) {
                    Saxon.Hej.regex.ARegularExpression regex = new Saxon.Hej.regex.ARegularExpression(Saxon.Hej.str.StringView.of(value).tidy(), "", "XP", new System.Collections.Generic.List<string>(), config);
                    filter = Saxon.Ejava.util.Optional<Saxon.Ejava.io.FilenameFilter>.of(new Saxon.Hej.functions.URIQueryParameters.RegexFilter(regex));
                } else {
                    if (keyword.Equals("recurse")) {
                        recurse = Saxon.Ejava.util.Optional<bool>.of("yes".Equals(value));
                    } else {
                        if (keyword.Equals("validation")) {
                            int v = Saxon.Hej.lib.Validation.getCode(value);
                            if ((v != Saxon.Hej.lib.Validation.INVALID)) {
                                validation = Saxon.Ejava.util.Optional<int>.of(v);
                            }
                        } else {
                            if (keyword.Equals("strip-space")) {
                                switch (value) {
                                  case "yes":
                                    strippingRule = Saxon.Ejava.util.Optional<Saxon.Hej.om.SpaceStrippingRule>.of(Saxon.Hej.om.AllElementsSpaceStrippingRule.getInstance());
                                    break ;
                                  case "ignorable":
                                    strippingRule = Saxon.Ejava.util.Optional<Saxon.Hej.om.SpaceStrippingRule>.of(Saxon.Hej.om.IgnorableSpaceStrippingRule.getInstance());
                                    break ;
                                  case "no":
                                    strippingRule = Saxon.Ejava.util.Optional<Saxon.Hej.om.SpaceStrippingRule>.of(Saxon.Hej.om.NoElementsSpaceStrippingRule.getInstance());
                                    break ;
                                }
                            } else {
                                if (keyword.Equals("stable")) {
                                    if (value.Equals("yes")) {
                                        stable = Saxon.Ejava.util.Optional<bool>.of(true);
                                    } else {
                                        if (value.Equals("no")) {
                                            stable = Saxon.Ejava.util.Optional<bool>.of(false);
                                        }
                                    }
                                } else {
                                    if (keyword.Equals("metadata")) {
                                        if (value.Equals("yes")) {
                                            metadata = Saxon.Ejava.util.Optional<bool>.of(true);
                                        } else {
                                            if (value.Equals("no")) {
                                                metadata = Saxon.Ejava.util.Optional<bool>.of(false);
                                            }
                                        }
                                    } else {
                                        if (keyword.Equals("xinclude")) {
                                            if (value.Equals("yes")) {
                                                xinclude = Saxon.Ejava.util.Optional<bool>.of(true);
                                            } else {
                                                if (value.Equals("no")) {
                                                    xinclude = Saxon.Ejava.util.Optional<bool>.of(false);
                                                }
                                            }
                                        } else {
                                            if (keyword.Equals("content-type")) {
                                                contentType = Saxon.Ejava.util.Optional<string>.of(value);
                                            } else {
                                                if (keyword.Equals("on-error")) {
                                                    switch (value) {
                                                      case "warning":
                                                        onError = Saxon.Ejava.util.Optional<int>.of(ON_ERROR_WARNING);
                                                        break ;
                                                      case "ignore":
                                                        onError = Saxon.Ejava.util.Optional<int>.of(ON_ERROR_IGNORE);
                                                        break ;
                                                      case "fail":
                                                        onError = Saxon.Ejava.util.Optional<int>.of(ON_ERROR_FAIL);
                                                        break ;
                                                    }
                                                } else {
                                                    if ((keyword.Equals("parser") && (config != null))) {
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        public static Saxon.Ejava.io.FilenameFilter makeGlobFilter(string value) {
            Saxon.Hej.str.UnicodeBuilder sb = new Saxon.Hej.str.UnicodeBuilder();
            sb.append('^');
            for (int i = 0; (i<value.Length); i++) {
                char c = value[i];
                if ((c == '.')) {
                    sb.appendLatin("\\.");
                } else {
                    if ((c == '*')) {
                        sb.appendLatin(".*");
                    } else {
                        if ((c == '?')) {
                            sb.appendLatin(".?");
                        } else {
                            sb.append(c);
                        }
                    }
                }
            }
            sb.append('$');
            try {
                return new Saxon.Hej.functions.URIQueryParameters.RegexFilter(new Saxon.Hej.regex.JavaRegularExpression(sb.toUnicodeString(), ""));
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Hej.trans.XPathException((("Invalid glob "+value)+" in collection URI"), "FODC0004");
            }
        }
        public Saxon.Ejava.util.Optional<Saxon.Hej.om.SpaceStrippingRule> getSpaceStrippingRule() {
            return strippingRule;
        }
        public Saxon.Ejava.util.Optional<int> getValidationMode() {
            return validation;
        }
        public Saxon.Ejava.util.Optional<Saxon.Ejava.io.FilenameFilter> getFilenameFilter() {
            return filter;
        }
        public Saxon.Ejava.util.Optional<bool> getRecurse() {
            return recurse;
        }
        public Saxon.Ejava.util.Optional<int> getOnError() {
            return onError;
        }
        public Saxon.Ejava.util.Optional<bool> getXInclude() {
            return xinclude;
        }
        public Saxon.Ejava.util.Optional<bool> getMetaData() {
            return metadata;
        }
        public Saxon.Ejava.util.Optional<string> getContentType() {
            return contentType;
        }
        public Saxon.Ejava.util.Optional<bool> getStable() {
            return stable;
        }
        public Saxon.Hej.lib.ParseOptions makeParseOptions(Saxon.Hej.Configuration config) {
            Saxon.Hej.lib.ParseOptions options = new Saxon.Hej.lib.ParseOptions();
            Saxon.Ejava.util.Optional<Saxon.Hej.om.SpaceStrippingRule> stripSpace = getSpaceStrippingRule();
            if (stripSpace.isPresent()) {
                options.setSpaceStrippingRule(stripSpace.get());
            }
            Saxon.Ejava.util.Optional<int> validation = getValidationMode();
            if (validation.isPresent()) {
                options.setSchemaValidationMode(validation.get());
            }
            Saxon.Ejava.util.Optional<bool> xinclude = getXInclude();
            if (xinclude.isPresent()) {
                options.setXIncludeAware(xinclude.get());
            }
            return options;
        }
        public class RegexFilter : Saxon.Ejava.io.FilenameFilter {
            private readonly Saxon.Hej.regex.RegularExpression pattern;
            public RegexFilter(Saxon.Hej.regex.RegularExpression regex)  {
                this.pattern = regex;
            }
            public bool accept(Saxon.Ejava.io.File dir, string name) {
                return (new Saxon.Ejava.io.File(dir, name).isDirectory() || pattern.matches(Saxon.Hej.str.StringView.of(name).tidy()));
            }
            public bool matches(string name) {
                return pattern.matches(Saxon.Hej.str.StringView.of(name).tidy());
            }
        
        }
    
    }
}