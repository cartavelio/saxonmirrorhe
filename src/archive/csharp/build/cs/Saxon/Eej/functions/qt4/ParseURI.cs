using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.qt4 {
    public class ParseURI : Saxon.Eej.functions.qt4.URIFunctions {
        public override Saxon.Hej.expr.Expression makeFunctionCall(params Saxon.Hej.expr.Expression[]  arguments) {
            Saxon.Hej.expr.Expression[] newArgs = new Saxon.Hej.expr.Expression[2];
            newArgs[0] = arguments[0];
            if (((arguments.Length<2) || (arguments[1] is Saxon.Hej.expr.DefaultedArgumentExpression))) {
                newArgs[1] = Saxon.Hej.expr.Literal.makeLiteral(new Saxon.Hej.ma.map.DictionaryMap());
            } else {
                newArgs[1] = arguments[1];
            }
            setArity(2);
            return base.makeFunctionCall(newArgs);
        }
        public override Saxon.Hej.ma.map.MapItem call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            string uristr = arguments[0].head().getStringValue();
            Saxon.Hej.ma.map.MapItem map = new Saxon.Impl.DataModel.ImmMap();
            map = map.addEntry(URI, new Saxon.Hej.value.StringValue(uristr));
            uristr = System.Text.RegularExpressions.Regex.Replace(uristr, "\\\\", "/");
            checkOptions(arguments);
            int pos = uristr.IndexOf("#");
            if ((pos>=0)) {
                map = map.addEntry(FRAGMENT, new Saxon.Hej.value.StringValue(uristr.Substring((pos+1))));
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
            string scheme;
            if (System.Text.RegularExpressions.Regex.IsMatch(uristr, "^[A-Za-z]:.*$")) {
                scheme = "file";
                map = map.addEntry(SCHEME, new Saxon.Hej.value.StringValue(scheme));
                uristr = ("/"+uristr);
            } else {
                if (System.Text.RegularExpressions.Regex.IsMatch(uristr, "^[A-Za-z]+:.*$")) {
                    pos = uristr.IndexOf(":");
                    scheme = uristr.Substring(0, pos - 0);
                    map = map.addEntry(SCHEME, new Saxon.Hej.value.StringValue(scheme));
                    uristr = uristr.Substring((pos+1));
                } else {
                    scheme = null;
                }
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
                    if (!("".Equals(uristr))) {
                        map = map.addEntry(HIERARCHICAL, Saxon.Hej.value.BooleanValue.get(hierarchical));
                    }
                }
            }
            string authority;
            if (System.Text.RegularExpressions.Regex.IsMatch(uristr, "^//*[a-zA-Z]:.*$")) {
                authority = null;
                uristr = System.Text.RegularExpressions.Regex.Replace(uristr, "^/+", "/");
            } else {
                Saxon.Ejava.util.regex.Pattern pattern = Saxon.Ejava.util.regex.Pattern.compile("^(///*)([^/]+)(/.*)?$");
                Saxon.Ejava.util.regex.Matcher matcher = pattern.matcher(uristr);
                if (matcher.find()) {
                    if (((matcher.group(3) == null) || matcher.group(3).Length==0)) {
                        if ((matcher.group(1).Length == 2)) {
                            authority = matcher.group(2);
                            map = map.addEntry(AUTHORITY, new Saxon.Hej.value.StringValue(authority));
                            uristr = "";
                        } else {
                            authority = null;
                            uristr = ("/"+matcher.group(2));
                        }
                    } else {
                        authority = matcher.group(2);
                        map = map.addEntry(AUTHORITY, new Saxon.Hej.value.StringValue(authority));
                        uristr = matcher.group(3);
                    }
                } else {
                    authority = null;
                }
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
                Saxon.Ejava.util.regex.Pattern pattern = Saxon.Ejava.util.regex.Pattern.compile("^([^@]*@)?(\\[[^]]*])(:([^:]*))?$");
                Saxon.Ejava.util.regex.Matcher matcher = pattern.matcher(authority);
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
                map = map.addEntry(PATH, new Saxon.Hej.value.StringValue(uristr));
                System.Collections.Generic.List<Saxon.Hej.om.GroundedValue> pathSegments = new System.Collections.Generic.List<Saxon.Hej.om.GroundedValue>();
                pos = uristr.IndexOf(pathSeparator);
                while ((pos>=0)) {
                    string seg = decodeString(uristr.Substring(0, pos - 0));
                    if ((((pathSegments.Count == 0) && "tel".Equals(scheme)) && seg.StartsWith(" "))) {
                        seg = ("+"+seg.Substring(1));
                    }
                    pathSegments.Add(new Saxon.Hej.value.StringValue(seg));
                    uristr = uristr.Substring((pos+pathSeparator.Length));
                    pos = uristr.IndexOf(pathSeparator);
                }
                pathSegments.Add(new Saxon.Hej.value.StringValue(decodeString(uristr)));
                map = map.addEntry(PATH_SEGMENTS, new Saxon.Hej.ma.arrays.SimpleArrayItem(pathSegments));
            }
            if ((query != null)) {
                System.Collections.Generic.List<Saxon.Hej.om.GroundedValue> querySegments = new System.Collections.Generic.List<Saxon.Hej.om.GroundedValue>();
                foreach (string str in System.Text.RegularExpressions.Regex.Split(query, querySeparator)) {
                    Saxon.Hej.ma.map.MapItem seg = new Saxon.Impl.DataModel.ImmMap();
                    pos = str.IndexOf("=");
                    if ((pos>=0)) {
                        string key = str.Substring(0, pos - 0);
                        string value = str.Substring((pos+1));
                        seg = seg.addEntry(KEY, new Saxon.Hej.value.StringValue(decodeString(key)));
                        seg = seg.addEntry(VALUE, new Saxon.Hej.value.StringValue(decodeString(value)));
                    } else {
                        seg = seg.addEntry(VALUE, new Saxon.Hej.value.StringValue(decodeString(str)));
                    }
                    querySegments.Add(seg);
                }
                map = map.addEntry(QUERY_SEGMENTS, new Saxon.Hej.ma.arrays.SimpleArrayItem(querySegments));
            }
            return map;
        }
    
    }
}