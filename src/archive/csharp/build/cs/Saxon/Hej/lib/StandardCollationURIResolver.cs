using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.lib {
    public class StandardCollationURIResolver : Saxon.Hej.lib.CollationURIResolver {
        private static readonly Saxon.Hej.lib.StandardCollationURIResolver theInstance = new Saxon.Hej.lib.StandardCollationURIResolver();
        public StandardCollationURIResolver()  {
        }
        public static Saxon.Hej.lib.StandardCollationURIResolver getInstance() {
            return theInstance;
        }
        public Saxon.Hej.lib.StringCollator resolve(string uri, Saxon.Hej.Configuration config) {
            if (uri.Equals("http://saxon.sf.net/collation")) {
                return Saxon.Hej.Version.platform.makeCollation(config, new Saxon.Ejava.util.Properties(), uri);
            } else {
                if (uri.StartsWith("http://saxon.sf.net/collation?")) {
                    System.Uri uuri;
                    try {
                        uuri = new System.Uri(uri, System.UriKind.RelativeOrAbsolute);
                    } catch (System.UriFormatException err) {
                        throw new Saxon.Hej.trans.XPathException(err);
                    }
                    Saxon.Ejava.util.Properties props = new Saxon.Ejava.util.Properties();
                    string query = uuri.Query;
                    if (query.StartsWith("?")) {
                        query = query.Substring(1);
                    }
                    Saxon.Ejava.util.StringTokenizer queryTokenizer = new Saxon.Ejava.util.StringTokenizer(query, ";&");
                    while (queryTokenizer.hasMoreTokens()) {
                        string param = queryTokenizer.nextToken();
                        int eq = param.IndexOf('=');
                        if (((eq>0) && (eq<(param.Length-1)))) {
                            string kw = param.Substring(0, eq - 0);
                            string val = Saxon.Hej.value.AnyURIValue.decode(param.Substring((eq+1)));
                            props.setProperty(kw, val);
                        }
                    }
                    return Saxon.Hej.Version.platform.makeCollation(config, props, uri);
                } else {
                    if (uri.StartsWith("http://www.w3.org/2013/collation/UCA")) {
                        Saxon.Hej.lib.StringCollator uca = Saxon.Hej.Version.platform.makeUcaCollator(uri, config);
                        if ((uca != null)) {
                            return uca;
                        }
                        if (uri.Contains("fallback=no")) {
                            return null;
                        } else {
                            System.Uri uuri;
                            try {
                                uuri = new System.Uri(uri, System.UriKind.RelativeOrAbsolute);
                            } catch (System.UriFormatException err) {
                                throw new Saxon.Hej.trans.XPathException(err);
                            }
                            Saxon.Ejava.util.Properties props = new Saxon.Ejava.util.Properties();
                            string query = Saxon.Hej.value.AnyURIValue.decode(uuri.Query);
                            foreach (string param in System.Text.RegularExpressions.Regex.Split(query, ";")) {
                                string[] tokens = System.Text.RegularExpressions.Regex.Split(param, "=");
                                if ((tokens.Length == 2)) {
                                    string kw = tokens[0];
                                    string val = tokens[1];
                                    if (kw.Equals("fallback")) {
                                        if (val.Equals("no")) {
                                            return null;
                                        } else {
                                            if (!(val.Equals("yes"))) {
                                                return null;
                                            }
                                        }
                                    }
                                    switch (kw) {
                                      case "strength":
                                        switch (val) {
                                          case "1":
                                            val = "primary";
                                            break ;
                                          case "2":
                                            val = "secondary";
                                            break ;
                                          case "3":
                                            val = "tertiary";
                                            break ;
                                          case "quaternary":
                                          case "4":
                                          case "5":
                                            val = "identical";
                                            break ;
                                        }
                                        break ;
                                      case "caseFirst":
                                        kw = "case-order";
                                        val += "-first";
                                        break ;
                                      case "numeric":
                                        kw = "alphanumeric";
                                        break ;
                                    }
                                    props.setProperty(kw, val);
                                }
                            }
                            return Saxon.Hej.Version.platform.makeCollation(config, props, uri);
                        }
                    } else {
                        return null;
                    }
                }
            }
        }
    
    }
}