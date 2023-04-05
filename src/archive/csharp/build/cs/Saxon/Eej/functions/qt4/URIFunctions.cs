using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.qt4 {
    abstract public class URIFunctions : Saxon.Hej.functions.SystemFunction {

        static URIFunctions() {
            Saxon.Impl.Helpers.MapUtils.Put(defaultPorts, "http", "80");
            Saxon.Impl.Helpers.MapUtils.Put(defaultPorts, "https", "443");
            Saxon.Impl.Helpers.MapUtils.Put(defaultPorts, "ssh", "22");
            Saxon.Impl.Helpers.MapUtils.Put(defaultPorts, "ftp", "21");
        }
        protected internal static System.Collections.Generic.HashSet<string> hierarchicalSchemes = new System.Collections.Generic.HashSet<string>(Saxon.Ejava.util.Arrays.asList("http", "https", "ftp", "file"));
        protected internal static System.Collections.Generic.HashSet<string> nonHierarchicalSchemes = new System.Collections.Generic.HashSet<string>(Saxon.Ejava.util.Arrays.asList("urn", "mailto", "news", "tel", "tag", "jar"));
        protected internal static System.Collections.Generic.Dictionary<string,string> defaultPorts = new System.Collections.Generic.Dictionary<string, string>();
        protected internal static readonly Saxon.Hej.value.AtomicValue URI = new Saxon.Hej.value.StringValue("uri");
        protected internal static readonly Saxon.Hej.value.AtomicValue PATH_SEPARATOR = new Saxon.Hej.value.StringValue("path-separator");
        protected internal static readonly Saxon.Hej.value.AtomicValue QUERY_SEPARATOR = new Saxon.Hej.value.StringValue("query-separator");
        protected internal static readonly Saxon.Hej.value.AtomicValue ALLOW_DEPRECATED_FEATURES = new Saxon.Hej.value.StringValue("allow-deprecated-features");
        protected internal static readonly Saxon.Hej.value.AtomicValue OMIT_DEFAULT_PORTS = new Saxon.Hej.value.StringValue("omit-default-ports");
        protected internal static readonly Saxon.Hej.value.AtomicValue SCHEME = new Saxon.Hej.value.StringValue("scheme");
        protected internal static readonly Saxon.Hej.value.AtomicValue AUTHORITY = new Saxon.Hej.value.StringValue("authority");
        protected internal static readonly Saxon.Hej.value.AtomicValue USERINFO = new Saxon.Hej.value.StringValue("userinfo");
        protected internal static readonly Saxon.Hej.value.AtomicValue HOST = new Saxon.Hej.value.StringValue("host");
        protected internal static readonly Saxon.Hej.value.AtomicValue PORT = new Saxon.Hej.value.StringValue("port");
        protected internal static readonly Saxon.Hej.value.AtomicValue PATH = new Saxon.Hej.value.StringValue("path");
        protected internal static readonly Saxon.Hej.value.AtomicValue PATH_SEGMENTS = new Saxon.Hej.value.StringValue("path-segments");
        protected internal static readonly Saxon.Hej.value.AtomicValue QUERY_SEGMENTS = new Saxon.Hej.value.StringValue("query-segments");
        protected internal static readonly Saxon.Hej.value.AtomicValue QUERY = new Saxon.Hej.value.StringValue("query");
        protected internal static readonly Saxon.Hej.value.AtomicValue KEY = new Saxon.Hej.value.StringValue("key");
        protected internal static readonly Saxon.Hej.value.AtomicValue VALUE = new Saxon.Hej.value.StringValue("value");
        protected internal static readonly Saxon.Hej.value.AtomicValue FRAGMENT = new Saxon.Hej.value.StringValue("fragment");
        protected internal static readonly Saxon.Hej.value.AtomicValue HIERARCHICAL = new Saxon.Hej.value.StringValue("hierarchical");
        protected internal string pathSeparator = "/";
        protected internal string querySeparator = "&";
        protected internal bool allowDeprecatedFeatures = false;
        protected internal bool omitDefaultPorts = false;
        protected internal bool hierarchical = true;
        protected internal void checkOptions(Saxon.Hej.om.Sequence[] arguments) {
            if ((getArity() == 2)) {
                Saxon.Hej.ma.map.MapItem options = (Saxon.Hej.ma.map.MapItem)(arguments[1].head());
                if ((options.get(PATH_SEPARATOR) != null)) {
                    pathSeparator = options.get(PATH_SEPARATOR).getStringValue();
                }
                if ((options.get(QUERY_SEPARATOR) != null)) {
                    querySeparator = options.get(QUERY_SEPARATOR).getStringValue();
                }
                allowDeprecatedFeatures = ((options.get(ALLOW_DEPRECATED_FEATURES) != null) && options.get(ALLOW_DEPRECATED_FEATURES).effectiveBooleanValue());
                omitDefaultPorts = ((options.get(OMIT_DEFAULT_PORTS) != null) && options.get(OMIT_DEFAULT_PORTS).effectiveBooleanValue());
            }
        }
        protected internal string encodePathSegment(Saxon.Hej.om.GroundedValue value) {
            string input = value.getStringValue();
            Saxon.Ejava.lang.StringBuilder output = new Saxon.Ejava.lang.StringBuilder();
            for (int pos = 0; (pos<input.Length); pos++) {
                char ch = input[pos];
                if ((ch == ' ')) {
                    output.append("+");
                } else {
                    if ((ch == '/')) {
                        output.append("%2F");
                    } else {
                        output.append(ch);
                    }
                }
            }
            return output.ToString();
        }
        protected internal string encodeOtherSegment(Saxon.Hej.om.GroundedValue value) {
            string input = value.getStringValue();
            Saxon.Ejava.lang.StringBuilder output = new Saxon.Ejava.lang.StringBuilder();
            for (int pos = 0; (pos<input.Length); pos++) {
                char ch = input[pos];
                if ((ch == ' ')) {
                    output.append("+");
                } else {
                    if (((((((ch == '=') || (ch == '&')) || (ch == ';')) || (ch == '?')) || (ch == '#')) || (ch == '\''))) {
                        output.append("%").append(toTwoHexDigits((int)(ch)));
                    } else {
                        output.append(ch);
                    }
                }
            }
            return output.ToString();
        }
        private string toTwoHexDigits(int ch) {
            return String.Format("{0:X2}", ch);
        }
        protected internal string decodeString(string value) {
            Saxon.Ejava.util.regex.Pattern pattern = Saxon.Ejava.util.regex.Pattern.compile("^(.*?)(\\+|%[0-9a-fA-F]{2})(.*)$");
            string input = value;
            Saxon.Ejava.util.regex.Matcher matcher = pattern.matcher(input);
            Saxon.Ejava.lang.StringBuilder output = new Saxon.Ejava.lang.StringBuilder();
            while (matcher.find()) {
                output.append(matcher.group(1));
                string esc = matcher.group(2);
                if ("+".Equals(esc)) {
                    output.append(" ");
                } else {
                    int hex = Saxon.Impl.Helpers.Utils.ParseInt(esc.Substring(1), 16);
                    output.append((char)(hex));
                }
                input = matcher.group(3);
                matcher = pattern.matcher(input);
            }
            output.append(input);
            return output.ToString();
        }
    
    }
}