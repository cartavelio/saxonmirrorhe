using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.qt4 {
    public class BuildURI : Saxon.Eej.functions.qt4.URIFunctions {
        public override Saxon.Hej.value.StringValue call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.ma.map.MapItem parts = (Saxon.Hej.ma.map.MapItem)(arguments[0].head());
            checkOptions(arguments);
            Saxon.Ejava.lang.StringBuilder uri = new Saxon.Ejava.lang.StringBuilder();
            string scheme = "";
            if ((parts.get(SCHEME) != null)) {
                scheme = parts.get(SCHEME).getStringValue();
                uri.append(scheme);
                if (nonHierarchicalSchemes.Contains(parts.get(SCHEME).getStringValue())) {
                    hierarchical = false;
                }
            }
            if ((parts.get(HIERARCHICAL) != null)) {
                hierarchical = parts.get(HIERARCHICAL).effectiveBooleanValue();
            }
            if (!("".Equals(scheme))) {
                if (hierarchical) {
                    uri.append("://");
                } else {
                    uri.append(":");
                }
            }
            if ((((parts.get(USERINFO) != null) || (parts.get(HOST) != null)) || (parts.get(PORT) != null))) {
                string userinfo = ((parts.get(USERINFO) == null) ? "" : parts.get(USERINFO).getStringValue());
                string host = ((parts.get(HOST) == null) ? "" : parts.get(HOST).getStringValue());
                string port = ((parts.get(PORT) == null) ? "" : parts.get(PORT).getStringValue());
                if (!(allowDeprecatedFeatures)) {
                    int pos = userinfo.IndexOf(":");
                    if (((pos>=0) && (pos != (userinfo.Length-1)))) {
                        userinfo = "";
                    }
                }
                if (!("".Equals(userinfo))) {
                    uri.append(userinfo).append("@");
                }
                uri.append(host);
                if (!("".Equals(port))) {
                    if (((!(omitDefaultPorts) || !(defaultPorts.ContainsKey(scheme))) || !(port.Equals(Saxon.Impl.Helpers.MapUtils.Get(defaultPorts, scheme))))) {
                        uri.append(":").append(port);
                    }
                }
            } else {
                uri.append(((parts.get(AUTHORITY) == null) ? "" : parts.get(AUTHORITY).getStringValue()));
            }
            if ((parts.get(PATH_SEGMENTS) != null)) {
                Saxon.Hej.ma.arrays.ArrayItem value = (Saxon.Hej.ma.arrays.ArrayItem)(parts.get(PATH_SEGMENTS));
                uri.append((hierarchical ? encodePathSegment(value.get(0)) : value.get(0).getStringValue()));
                for (int pos = 1; (pos<value.arrayLength()); pos++) {
                    uri.append(pathSeparator);
                    uri.append(encodePathSegment(value.get(pos)));
                }
            } else {
                if ((parts.get(PATH) != null)) {
                    uri.append(parts.get(PATH).getStringValue());
                }
            }
            if ((parts.get(QUERY_SEGMENTS) != null)) {
                string sep = "?";
                Saxon.Hej.ma.arrays.ArrayItem value = (Saxon.Hej.ma.arrays.ArrayItem)(parts.get(QUERY_SEGMENTS));
                for (int pos = 0; (pos<value.arrayLength()); pos++) {
                    Saxon.Hej.ma.map.MapItem param = (Saxon.Hej.ma.map.MapItem)(value.get(pos));
                    if (((param.get(KEY) != null) || (param.get(VALUE) != null))) {
                        uri.append(sep);
                        sep = querySeparator;
                        if ((param.get(KEY) != null)) {
                            if ((param.get(VALUE) != null)) {
                                uri.append(encodeOtherSegment(param.get(KEY)));
                                uri.append("=");
                                uri.append(encodeOtherSegment(param.get(VALUE)));
                            } else {
                                uri.append(encodeOtherSegment(param.get(KEY)));
                            }
                        } else {
                            uri.append(encodeOtherSegment(param.get(VALUE)));
                        }
                    }
                }
            } else {
                if ((parts.get(QUERY) != null)) {
                    uri.append("?").append(parts.get(QUERY).getStringValue());
                }
            }
            if ((parts.get(FRAGMENT) != null)) {
                uri.append("#").append(encodeOtherSegment(parts.get(FRAGMENT)));
            }
            return new Saxon.Hej.value.StringValue(uri.ToString());
        }
    
    }
}