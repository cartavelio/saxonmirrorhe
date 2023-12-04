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
            System.Collections.Generic.HashSet<System.Char> chars = new System.Collections.Generic.HashSet<System.Char>();
            for (char ch = 'A'; (ch<='Z'); ch++) {
                chars.Add(ch);
            }
            for (char ch = 'a'; (ch<='z'); ch++) {
                chars.Add(ch);
            }
            for (char ch = '0'; (ch<='9'); ch++) {
                chars.Add(ch);
            }
            chars.Add('-');
            chars.Add('.');
            chars.Add('_');
            chars.Add('~');
            chars.Add('!');
            chars.Add('$');
            chars.Add('&');
            chars.Add('\'');
            chars.Add('(');
            chars.Add(')');
            chars.Add('*');
            chars.Add('+');
            chars.Add(',');
            chars.Add(';');
            chars.Add('=');
            chars.Add(':');
            chars.Add('@');
            unescapedUriCharacters = unmodifiable(chars);
        }
        protected internal static System.Collections.Generic.HashSet<string> hierarchicalSchemes = new System.Collections.Generic.HashSet<string>(Saxon.Ejava.util.Arrays.asList("http", "https", "ftp", "file"));
        protected internal static System.Collections.Generic.HashSet<string> nonHierarchicalSchemes = new System.Collections.Generic.HashSet<string>(Saxon.Ejava.util.Arrays.asList("urn", "mailto", "news", "tel", "tag", "jar"));
        protected internal static System.Collections.Generic.Dictionary<string,string> defaultPorts = new System.Collections.Generic.Dictionary<string, string>();
        protected internal static readonly Saxon.Hej.value.AtomicValue URI = new Saxon.Hej.value.StringValue("uri");
        protected internal static readonly Saxon.Hej.value.AtomicValue PATH_SEPARATOR = new Saxon.Hej.value.StringValue("path-separator");
        protected internal static readonly Saxon.Hej.value.AtomicValue QUERY_SEPARATOR = new Saxon.Hej.value.StringValue("query-separator");
        protected internal static readonly Saxon.Hej.value.AtomicValue ALLOW_DEPRECATED_FEATURES = new Saxon.Hej.value.StringValue("allow-deprecated-features");
        protected internal static readonly Saxon.Hej.value.AtomicValue OMIT_DEFAULT_PORTS = new Saxon.Hej.value.StringValue("omit-default-ports");
        protected internal static readonly Saxon.Hej.value.AtomicValue UNC_PATH = new Saxon.Hej.value.StringValue("unc-path");
        protected internal static readonly Saxon.Hej.value.AtomicValue SCHEME = new Saxon.Hej.value.StringValue("scheme");
        protected internal static readonly Saxon.Hej.value.AtomicValue AUTHORITY = new Saxon.Hej.value.StringValue("authority");
        protected internal static readonly Saxon.Hej.value.AtomicValue USERINFO = new Saxon.Hej.value.StringValue("userinfo");
        protected internal static readonly Saxon.Hej.value.AtomicValue HOST = new Saxon.Hej.value.StringValue("host");
        protected internal static readonly Saxon.Hej.value.AtomicValue PORT = new Saxon.Hej.value.StringValue("port");
        protected internal static readonly Saxon.Hej.value.AtomicValue PATH = new Saxon.Hej.value.StringValue("path");
        protected internal static readonly Saxon.Hej.value.AtomicValue FILEPATH = new Saxon.Hej.value.StringValue("filepath");
        protected internal static readonly Saxon.Hej.value.AtomicValue PATH_SEGMENTS = new Saxon.Hej.value.StringValue("path-segments");
        protected internal static readonly Saxon.Hej.value.AtomicValue QUERY_PARAMETERS = new Saxon.Hej.value.StringValue("query-parameters");
        protected internal static readonly Saxon.Hej.value.AtomicValue QUERY = new Saxon.Hej.value.StringValue("query");
        protected internal static readonly Saxon.Hej.value.AtomicValue KEY = new Saxon.Hej.value.StringValue("key");
        protected internal static readonly Saxon.Hej.value.AtomicValue VALUE = new Saxon.Hej.value.StringValue("value");
        protected internal static readonly Saxon.Hej.value.AtomicValue FRAGMENT = new Saxon.Hej.value.StringValue("fragment");
        protected internal static readonly Saxon.Hej.value.AtomicValue HIERARCHICAL = new Saxon.Hej.value.StringValue("hierarchical");
        protected internal static readonly System.Collections.Generic.ISet<System.Char> unescapedUriCharacters;
        private static System.Collections.Generic.ISet<System.Char> unmodifiable(System.Collections.Generic.ISet<System.Char> chars) {
            return chars;
        }
        protected internal char pathSeparator = '/';
        protected internal char querySeparator = '&';
        protected internal bool allowDeprecatedFeatures = false;
        protected internal bool omitDefaultPorts = false;
        protected internal bool hierarchical = true;
        protected internal bool uncPath = false;
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
        protected internal void checkOptions(Saxon.Hej.om.Sequence[] arguments) {
            if ((getArity() == 2)) {
                Saxon.Hej.ma.map.MapItem options = (Saxon.Hej.ma.map.MapItem)(arguments[1].head());
                if ((options.get(PATH_SEPARATOR) != null)) {
                    string str = options.get(PATH_SEPARATOR).getStringValue();
                    if ((str.Length != 1)) {
                        throw new Saxon.Hej.trans.XPathException("The path separator must be a single character", "FOXX0000");
                    }
                    pathSeparator = str[0];
                }
                if ((options.get(QUERY_SEPARATOR) != null)) {
                    string str = options.get(QUERY_SEPARATOR).getStringValue();
                    if ((str.Length != 1)) {
                        throw new Saxon.Hej.trans.XPathException("The query separator must be a single character", "FOXX0000");
                    }
                    querySeparator = str[0];
                }
                allowDeprecatedFeatures = ((options.get(ALLOW_DEPRECATED_FEATURES) != null) && options.get(ALLOW_DEPRECATED_FEATURES).effectiveBooleanValue());
                omitDefaultPorts = ((options.get(OMIT_DEFAULT_PORTS) != null) && options.get(OMIT_DEFAULT_PORTS).effectiveBooleanValue());
                uncPath = ((options.get(UNC_PATH) != null) && options.get(UNC_PATH).effectiveBooleanValue());
            }
        }
        protected internal string encodePathSegment(Saxon.Hej.om.GroundedValue value) {
            string input = value.getStringValue();
            Saxon.Ejava.lang.StringBuilder output = new Saxon.Ejava.lang.StringBuilder();
            for (int pos = 0; (pos<input.Length); pos++) {
                char ch = input[pos];
                if (((ch>127) || unescapedUriCharacters.Contains(ch))) {
                    output.append(ch);
                } else {
                    output.append("%");
                    output.append(toTwoHexDigits(ch));
                }
            }
            return output.ToString();
        }
        protected internal string encodeQueryPart(Saxon.Hej.om.GroundedValue value) {
            string input = value.getStringValue();
            Saxon.Ejava.lang.StringBuilder output = new Saxon.Ejava.lang.StringBuilder();
            for (int pos = 0; (pos<input.Length); pos++) {
                char ch = input[pos];
                if ((ch == querySeparator)) {
                    output.append("%");
                    output.append(toTwoHexDigits(querySeparator));
                    continue ;
                }
                switch (ch) {
                  case '=':
                    output.append("%3D");
                    break ;
                  case '&':
                    output.append("%26");
                    break ;
                  case '/':
                  case '?':
                    output.append(ch);
                    break ;
                  default:
                    if (((ch>127) || unescapedUriCharacters.Contains(ch))) {
                        output.append(ch);
                    } else {
                        output.append("%");
                        output.append(toTwoHexDigits(ch));
                    }
                    break ;
                }
            }
            return output.ToString();
        }
        protected internal string encodeFragmentIdentifier(Saxon.Hej.om.GroundedValue value) {
            string input = value.getStringValue();
            Saxon.Ejava.lang.StringBuilder output = new Saxon.Ejava.lang.StringBuilder();
            for (int pos = 0; (pos<input.Length); pos++) {
                char ch = input[pos];
                if (((ch>127) || unescapedUriCharacters.Contains(ch))) {
                    output.append(ch);
                } else {
                    switch (ch) {
                      case '/':
                      case '?':
                        output.append(ch);
                        break ;
                      default:
                        output.append("%");
                        output.append(toTwoHexDigits(ch));
                        break ;
                    }
                }
            }
            return output.ToString();
        }
        private string toTwoHexDigits(int ch) {
            return String.Format("{0:X2}", ch);
        }
    
    }
}