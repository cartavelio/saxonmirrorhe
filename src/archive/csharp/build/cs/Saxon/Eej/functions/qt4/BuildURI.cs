using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.qt4 {
    public class BuildURI : Saxon.Eej.functions.qt4.URIFunctions {
        private static Saxon.Hej.ma.map.RecordTest.Field f(string name, Saxon.Hej.value.SequenceType type, bool optional) {
            return new Saxon.Hej.ma.map.RecordTest.Field(name, type, optional);
        }
        private static readonly Saxon.Hej.ma.map.MapType STRING_MAP_TYPE = new Saxon.Hej.ma.map.MapType(Saxon.Hej.type.BuiltInAtomicType.STRING, Saxon.Hej.value.SequenceType.STRING_SEQUENCE);
        public static Saxon.Hej.ma.map.RecordTest uriStructureRecord = Saxon.Hej.ma.map.RecordTest.extensible(f("uri", Saxon.Hej.value.SequenceType.SINGLE_STRING, true), f("scheme", Saxon.Hej.value.SequenceType.SINGLE_STRING, true), f("authority", Saxon.Hej.value.SequenceType.SINGLE_STRING, true), f("userinfo", Saxon.Hej.value.SequenceType.SINGLE_STRING, true), f("host", Saxon.Hej.value.SequenceType.SINGLE_STRING, true), f("port", Saxon.Hej.value.SequenceType.SINGLE_STRING, true), f("path", Saxon.Hej.value.SequenceType.SINGLE_STRING, true), f("query", Saxon.Hej.value.SequenceType.SINGLE_STRING, true), f("fragment", Saxon.Hej.value.SequenceType.SINGLE_STRING, true), f("path-segments", Saxon.Hej.value.SequenceType.STRING_SEQUENCE, true), f("query-parameters", Saxon.Hej.value.SequenceType.one(STRING_MAP_TYPE), true));
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
                Saxon.Hej.om.GroundedValue value = parts.get(PATH_SEGMENTS);
                uri.append((hierarchical ? encodePathSegment(value.itemAt(0)) : value.itemAt(0).getStringValue()));
                for (int pos = 1; (pos<value.getLength()); pos++) {
                    uri.append(pathSeparator);
                    uri.append(encodePathSegment(value.itemAt(pos)));
                }
            } else {
                if ((parts.get(PATH) != null)) {
                    uri.append(parts.get(PATH).getStringValue());
                }
            }
            if ((parts.get(QUERY_PARAMETERS) != null)) {
                char sep = '?';
                foreach (Saxon.Hej.ma.map.KeyValuePair pair in ((Saxon.Hej.ma.map.MapItem)(parts.get(QUERY_PARAMETERS))).keyValuePairs()) {
                    if (((pair.key != null) || (pair.value != null))) {
                        uri.append(sep);
                        sep = querySeparator;
                        if (((pair.key != null) && !(pair.key.getStringValue().Length==0))) {
                            if (((pair.value != null) && (pair.value.getLength()>0))) {
                                for (int idx = 0; (idx<pair.value.getLength()); idx++) {
                                    if ((idx>0)) {
                                        uri.append(sep);
                                    }
                                    uri.append(encodeQueryPart(pair.key));
                                    uri.append("=");
                                    uri.append(encodeQueryPart(pair.value.itemAt(idx)));
                                }
                            } else {
                                uri.append(encodeQueryPart(pair.key));
                            }
                        } else {
                            uri.append(encodeQueryPart(pair.value));
                        }
                    }
                }
            } else {
                if ((parts.get(QUERY) != null)) {
                    uri.append("?").append(parts.get(QUERY).getStringValue());
                }
            }
            if ((parts.get(FRAGMENT) != null)) {
                uri.append("#").append(encodeFragmentIdentifier(parts.get(FRAGMENT)));
            }
            return new Saxon.Hej.value.StringValue(uri.ToString());
        }
    
    }
}