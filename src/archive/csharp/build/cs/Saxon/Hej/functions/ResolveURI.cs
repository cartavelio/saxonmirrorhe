using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class ResolveURI : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.value.AtomicValue arg0 = (Saxon.Hej.value.AtomicValue)(arguments[0].head());
            if ((arg0 == null)) {
                return Saxon.Hej.value.EmptySequence.getInstance();
            }
            string relative = arg0.getStringValue();
            string @base;
            if ((getArity() == 2)) {
                @base = arguments[1].head().getStringValue();
            } else {
                @base = getStaticBaseUriString();
                if ((@base == null)) {
                    throw new Saxon.Hej.trans.XPathException("Base URI in static context of resolve-uri() is unknown", "FONS0005", context);
                }
            }
            return resolve(@base, relative, context);
        }
        private Saxon.Hej.value.AnyURIValue resolve(string @base, string relative, Saxon.Hej.expr.XPathContext context) {
            bool escaped = false;
            if (relative.Contains(" ")) {
                relative = escapeSpaces(relative);
                escaped = true;
            }
            if (@base.Contains(" ")) {
                @base = escapeSpaces(@base);
                escaped = true;
            }
            System.Uri relativeURI = null;
            try {
                relativeURI = absoluteOrRelativeURI(relative);
            } catch (System.UriFormatException e) {
                throw new Saxon.Hej.trans.XPathException(((("Relative URI "+Saxon.Hej.trans.Err.wrap(relative))+" is invalid: ")+e.Message), "FORG0002", context);
            }
            if (relativeURI.IsAbsoluteUri) {
                return new Saxon.Hej.value.AnyURIValue(relative);
            }
            System.Uri absoluteURI = null;
            try {
                absoluteURI = new System.Uri(@base, System.UriKind.RelativeOrAbsolute);
            } catch (System.UriFormatException e) {
                throw new Saxon.Hej.trans.XPathException(((("Base URI "+Saxon.Hej.trans.Err.wrap(@base))+" is invalid: ")+e.Message), "FORG0002", context);
            }
            if (!(absoluteURI.IsAbsoluteUri)) {
                throw new Saxon.Hej.trans.XPathException((("Base URI "+Saxon.Hej.trans.Err.wrap(@base))+" is not an absolute URI"), "FORG0002", context);
            }
            if ((Saxon.Impl.Helpers.UriUtils.IsOpaque(absoluteURI) && !(@base.StartsWith("jar:")))) {
                throw new Saxon.Hej.trans.XPathException((("Base URI "+Saxon.Hej.trans.Err.wrap(@base))+" is a non-hierarchic URI"), "FORG0002", context);
            }
            string fragment = absoluteURI.Fragment;
            if (((fragment != null) && !(fragment.Length==0))) {
                throw new Saxon.Hej.trans.XPathException((("Base URI "+Saxon.Hej.trans.Err.wrap(@base))+" contains a fragment identifier"), "FORG0002", context);
            }
            if (((!(@base.StartsWith("jar:")) && (absoluteURI.LocalPath != null)) && absoluteURI.LocalPath.Length==0)) {
                try {
                    absoluteURI = Saxon.Impl.Helpers.UriUtils.Build(absoluteURI.Scheme, absoluteURI.UserInfo, absoluteURI.Host, absoluteURI.Port, "/", Saxon.Impl.Helpers.UriUtils.GetQuery(absoluteURI), Saxon.Impl.Helpers.StringUtils.nullIfEmpty(absoluteURI.Fragment));
                } catch (System.UriFormatException e) {
                    throw new Saxon.Hej.trans.XPathException(("Failed to parse JAR scheme URI "+Saxon.Hej.trans.Err.wrap(absoluteURI.OriginalString)), "FORG0002", context);
                }
                @base = absoluteURI.ToString();
            }
            System.Uri resolved = null;
            try {
                resolved = makeAbsolute(relative, @base);
            } catch (System.UriFormatException e) {
                throw new Saxon.Hej.trans.XPathException(e.Message, "FORG0002");
            }
            if (!(resolved.OriginalString.StartsWith("file:////"))) {
                resolved = Saxon.Impl.Helpers.UriUtils.Normalize(resolved);
            }
            bool inputIsPercentEncoded = (@base.Contains("%") || relative.Contains("%"));
            string resolvedString = (inputIsPercentEncoded ? resolved.OriginalString : resolved.ToString());
            string result = (escaped ? unescapeSpaces(resolvedString) : resolvedString);
            while (result.EndsWith("..")) {
                result = result.Substring(0, (result.Length-2) - 0);
            }
            while (result.EndsWith("../")) {
                result = result.Substring(0, (result.Length-3) - 0);
            }
            return new Saxon.Hej.value.AnyURIValue(result);
        }
        public static System.Uri absoluteOrRelativeURI(string href) {
            return new System.Uri(href, System.UriKind.RelativeOrAbsolute);
        }
        public static string tryToExpand(string systemId) {
            if (((systemId == null) || systemId.Length==0)) {
                return resolveAgainstCurrentDirectory("");
            }
            try {
                new Saxon.Ejava.net.URL(systemId);
                return systemId;
            } catch (Saxon.Ejava.net.MalformedURLException err) {
                return resolveAgainstCurrentDirectory(systemId);
            }
        }
        private static string resolveAgainstCurrentDirectory(string systemId) {
            string dir;
            try {
                dir = Saxon.Ejava.lang.System.getProperty("user.dir");
            } catch (System.Exception geterr) {
                return systemId;
            }
            if (!(((dir.EndsWith("/") || systemId.StartsWith("/"))))) {
                dir = (dir+'/');
            }
            try {
                System.Uri currentDirectoryURI = new Saxon.Ejava.io.File(dir).toURI();
                System.Uri baseURI = new System.Uri(currentDirectoryURI, systemId);
                return baseURI.ToString();
            } catch (System.Exception e) {
                return systemId;
            }
        }
        public static System.Uri makeAbsolute(string relativeURI, string @base) {
            System.Uri absoluteURI;
            Saxon.Hej.lib.StandardURIChecker checker = Saxon.Hej.lib.StandardURIChecker.getInstance();
            if ((relativeURI == null)) {
                if ((@base == null)) {
                    throw failure("", "Relative and Base URI must not both be null");
                }
                absoluteURI = new System.Uri(Saxon.Hej.functions.ResolveURI.escapeSpaces(@base), System.UriKind.RelativeOrAbsolute);
                checker.checkThoroughly(absoluteURI);
                if (!(absoluteURI.IsAbsoluteUri)) {
                    throw failure(@base, "Relative URI not supplied, so base URI must be absolute");
                } else {
                    return absoluteURI;
                }
            }
            try {
                if (((@base == null) || @base.Length==0)) {
                    absoluteURI = new System.Uri(relativeURI, System.UriKind.RelativeOrAbsolute);
                    if (!(absoluteURI.IsAbsoluteUri)) {
                        string expandedBase = Saxon.Hej.functions.ResolveURI.tryToExpand(@base);
                        if (!(expandedBase.Equals(@base))) {
                            return makeAbsolute(relativeURI, expandedBase);
                        }
                    }
                } else {
                    if ((@base.StartsWith("jar:") || @base.StartsWith("file:////"))) {
                        try {
                            Saxon.Ejava.net.URL baseURL = new Saxon.Ejava.net.URL(@base);
                            Saxon.Ejava.net.URL absoluteURL = new Saxon.Ejava.net.URL(baseURL, relativeURI);
                            absoluteURI = absoluteURL.toURI();
                        } catch (Saxon.Ejava.net.MalformedURLException err) {
                            throw failure(((@base+" ")+relativeURI), err.Message);
                        }
                    } else {
                        if (@base.StartsWith("classpath:")) {
                            absoluteURI = new System.Uri(relativeURI, System.UriKind.RelativeOrAbsolute);
                            if (!(absoluteURI.IsAbsoluteUri)) {
                                absoluteURI = new System.Uri(("classpath:"+relativeURI), System.UriKind.RelativeOrAbsolute);
                            }
                        } else {
                            System.Uri baseURI;
                            try {
                                baseURI = new System.Uri(@base, System.UriKind.RelativeOrAbsolute);
                            } catch (System.UriFormatException e) {
                                throw failure(@base, ("Invalid base URI: "+e.Message));
                            }
                            int hash = @base.IndexOf('#');
                            if ((hash>=0)) {
                                @base = @base.Substring(0, hash - 0);
                                try {
                                    baseURI = new System.Uri(@base, System.UriKind.RelativeOrAbsolute);
                                    checker.checkThoroughly(baseURI);
                                } catch (System.UriFormatException e) {
                                    throw failure(@base, ("Invalid base URI: "+e.Message));
                                }
                            }
                            System.Uri absOrRel;
                            try {
                                absOrRel = absoluteOrRelativeURI(relativeURI);
                                checker.checkThoroughly(absOrRel);
                            } catch (System.UriFormatException e) {
                                throw failure(@base, ("Invalid relative URI: "+e.Message));
                            }
                            if (absOrRel.IsAbsoluteUri) {
                                absoluteURI = absOrRel;
                            } else {
                                absoluteURI = (relativeURI.Length==0 ? baseURI : new System.Uri(baseURI, relativeURI));
                            }
                        }
                    }
                }
            } catch (Saxon.Ejava.lang.IllegalArgumentException err0) {
                throw failure(relativeURI, ("Cannot resolve URI against base "+Saxon.Hej.trans.Err.wrap(@base)));
            }
            return absoluteURI;
        }
        private static System.UriFormatException failure(string input, string reason) {
            return new System.UriFormatException("Failed to resolve " + input + ": " + reason);
        }
        public static string escapeSpaces(string s) {
            int i = s.IndexOf(' ');
            if ((i<0)) {
                return s;
            }
            return (((((i == 0) ? "" : s.Substring(0, i - 0)))+"%20")+(((i == (s.Length-1)) ? "" : escapeSpaces(s.Substring((i+1))))));
        }
        public static string unescapeSpaces(string uri) {
            return uri.Replace("%20", " ");
        }
    
    }
}