using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.qt4 {
    public class ParseURI : Saxon.Eej.functions.qt4.URIFunctions {
        public override Saxon.Hej.ma.map.MapItem call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            string uristr = arguments[0].head().getStringValue();
            Saxon.Hej.ma.map.MapItem map = new Saxon.Impl.DataModel.ImmMap();
            map = map.addEntry(URI, new Saxon.Hej.value.StringValue(uristr));
            uristr = System.Text.RegularExpressions.Regex.Replace(uristr, "\\\\", "/");
            try {
                checkOptions(arguments);
            } catch (Saxon.Ejava.lang.IllegalArgumentException e) {
                throw new Saxon.Hej.trans.XPathException(e);
            }
            int pos = uristr.IndexOf("#");
            if ((pos>=0)) {
                map = map.addEntry(FRAGMENT, new Saxon.Hej.value.StringValue(Saxon.Eej.functions.qt4.DecodeFromURI.decode(uristr.Substring((pos+1)))));
                uristr = uristr.Substring(0, pos - 0);
            }
            string query;
            pos = uristr.IndexOf("?");
            if ((pos>=0)) {
                query = uristr.Substring((pos+1));
                map = map.addEntry(QUERY, new Saxon.Hej.value.StringValue(query));
                uristr = uristr.Substring(0, pos - 0);
            } else {
                query = null;
            }
            string scheme = null;
            Saxon.Ejava.util.regex.Pattern pattern = Saxon.Ejava.util.regex.Pattern.compile("^([A-Za-z][-+.A-Za-z0-9]+):(.*)$");
            Saxon.Ejava.util.regex.Matcher matcher = pattern.matcher(uristr);
            if (matcher.matches()) {
                scheme = matcher.group(1).ToLower();
                uristr = matcher.group(2);
            }
            if (((scheme == null) || "file".Equals(scheme))) {
                pattern = Saxon.Ejava.util.regex.Pattern.compile("^/*([A-Za-z])[:|](.*)$");
                matcher = pattern.matcher(uristr);
                if (matcher.find()) {
                    scheme = "file";
                    uristr = ((("/"+matcher.group(1))+":")+matcher.group(2));
                } else {
                    if (uncPath) {
                        scheme = "file";
                    }
                }
            }
            if ((scheme != null)) {
                map = map.addEntry(SCHEME, new Saxon.Hej.value.StringValue(scheme));
            }
            if (((scheme != null) && hierarchicalSchemes.Contains(scheme))) {
                hierarchical = true;
                map = map.addEntry(HIERARCHICAL, Saxon.Hej.value.BooleanValue.get(hierarchical));
            } else {
                if (((scheme != null) && nonHierarchicalSchemes.Contains(scheme))) {
                    hierarchical = false;
                    map = map.addEntry(HIERARCHICAL, Saxon.Hej.value.BooleanValue.get(hierarchical));
                } else {
                    hierarchical = uristr.StartsWith("/");
                    if (!(uristr.Length==0)) {
                        map = map.addEntry(HIERARCHICAL, Saxon.Hej.value.BooleanValue.get(hierarchical));
                    }
                }
            }
            string filepath = null;
            string authority;
            if ("file".Equals(scheme)) {
                authority = null;
                pattern = Saxon.Ejava.util.regex.Pattern.compile("^/*(//[^/].*)$");
                matcher = pattern.matcher(uristr);
                if ((uncPath && matcher.find())) {
                    uristr = matcher.group(1);
                    filepath = uristr;
                } else {
                    uristr = System.Text.RegularExpressions.Regex.Replace(uristr, "^/+", "/");
                    if (System.Text.RegularExpressions.Regex.IsMatch(uristr, "^/[A-Za-z]:.*$")) {
                        filepath = uristr.Substring(1);
                    } else {
                        filepath = uristr;
                    }
                }
            } else {
                if (hierarchical) {
                    pattern = Saxon.Ejava.util.regex.Pattern.compile("^//([^/]+)$");
                    matcher = pattern.matcher(uristr);
                    if (matcher.find()) {
                        authority = matcher.group(1);
                        uristr = "";
                    } else {
                        pattern = Saxon.Ejava.util.regex.Pattern.compile("^//([^/]*)(/.*)$");
                        matcher = pattern.matcher(uristr);
                        if (matcher.find()) {
                            if (((matcher.group(1) != null) && !(matcher.group(1).Length==0))) {
                                authority = matcher.group(1);
                            } else {
                                authority = null;
                            }
                            uristr = matcher.group(2);
                        } else {
                            authority = null;
                        }
                    }
                } else {
                    authority = null;
                }
            }
            if ((authority != null)) {
                map = map.addEntry(AUTHORITY, new Saxon.Hej.value.StringValue(authority));
            }
            string port = null;
            if ((authority != null)) {
                string userinfo;
                pos = authority.IndexOf("@");
                if (((pos>=0) && ((allowDeprecatedFeatures || !(authority.Substring(0, pos - 0).Contains(":")))))) {
                    userinfo = authority.Substring(0, pos - 0);
                } else {
                    userinfo = null;
                }
                if ((userinfo != null)) {
                    map = map.addEntry(USERINFO, new Saxon.Hej.value.StringValue(userinfo));
                }
                pattern = Saxon.Ejava.util.regex.Pattern.compile("^([^@]*@)?(\\[[^]]*])(:([^:]*))?$");
                matcher = pattern.matcher(authority);
                if (matcher.find()) {
                    map = map.addEntry(HOST, new Saxon.Hej.value.StringValue(matcher.group(2)));
                    port = ("".Equals(matcher.group(4)) ? null : matcher.group(4));
                } else {
                    pattern = Saxon.Ejava.util.regex.Pattern.compile("^([^@]*@)?\\[.*$");
                    matcher = pattern.matcher(authority);
                    if (matcher.find()) {
                        throw new System.Exception("Unparsable authority component");
                    } else {
                        pattern = Saxon.Ejava.util.regex.Pattern.compile("^([^@]*@)?([^:]+)(:([^:]*))?$");
                        matcher = pattern.matcher(authority);
                        if (matcher.find()) {
                            map = map.addEntry(HOST, new Saxon.Hej.value.StringValue(matcher.group(2)));
                            port = ("".Equals(matcher.group(4)) ? null : matcher.group(4));
                        }
                    }
                }
            }
            if ((((port != null) && omitDefaultPorts) && port.Equals(Saxon.Impl.Helpers.MapUtils.Get(defaultPorts, scheme)))) {
                port = null;
            }
            if ((port != null)) {
                map = map.addEntry(PORT, new Saxon.Hej.value.StringValue(port));
            }
            if (!("".Equals(uristr))) {
                if (((filepath == null) && (((scheme == null) || "file".Equals(scheme))))) {
                    filepath = uristr;
                }
                map = map.addEntry(PATH, new Saxon.Hej.value.StringValue(uristr));
                Saxon.Hej.om.GroundedValue pathSegments = Saxon.Hej.value.EmptySequence.getInstance();
                pos = uristr.IndexOf(pathSeparator);
                while ((pos>=0)) {
                    string seg = Saxon.Eej.functions.qt4.DecodeFromURI.decode(uristr.Substring(0, pos - 0));
                    if ((((pathSegments.getLength() == 0) && "tel".Equals(scheme)) && seg.StartsWith(" "))) {
                        seg = ("+"+seg.Substring(1));
                    }
                    pathSegments = pathSegments.concatenate(new Saxon.Hej.value.StringValue(seg));
                    uristr = uristr.Substring((pos+1));
                    pos = uristr.IndexOf(pathSeparator);
                }
                pathSegments = pathSegments.concatenate(new Saxon.Hej.value.StringValue(Saxon.Eej.functions.qt4.DecodeFromURI.decode(uristr)));
                map = map.addEntry(PATH_SEGMENTS, pathSegments);
            }
            if ((filepath != null)) {
                string decoded = Saxon.Eej.functions.qt4.DecodeFromURI.decode(filepath);
                map = map.addEntry(FILEPATH, new Saxon.Hej.value.StringValue(decoded));
            }
            if ((query != null)) {
                Saxon.Impl.DataModel.ImmMap querySegments = new Saxon.Impl.DataModel.ImmMap();
                foreach (string str in System.Text.RegularExpressions.Regex.Split(query, (""+querySeparator))) {
                    pos = str.IndexOf("=");
                    Saxon.Hej.value.StringValue key;
                    Saxon.Hej.value.StringValue value;
                    if ((pos>=0)) {
                        key = new Saxon.Hej.value.StringValue(Saxon.Eej.functions.qt4.DecodeFromURI.decode(str.Substring(0, pos - 0)));
                        value = new Saxon.Hej.value.StringValue(Saxon.Eej.functions.qt4.DecodeFromURI.decode(str.Substring((pos+1))));
                    } else {
                        key = new Saxon.Hej.value.StringValue("");
                        value = new Saxon.Hej.value.StringValue(Saxon.Eej.functions.qt4.DecodeFromURI.decode(str));
                    }
                    Saxon.Hej.om.GroundedValue currentValue = querySegments.get(key);
                    if ((currentValue == null)) {
                        querySegments = (Saxon.Impl.DataModel.ImmMap)(querySegments.addEntry(key, value));
                    } else {
                        querySegments = (Saxon.Impl.DataModel.ImmMap)(querySegments.addEntry(key, currentValue.concatenate(value)));
                    }
                }
                map = map.addEntry(QUERY_PARAMETERS, querySegments);
            }
            return map;
        }
    
    }
}