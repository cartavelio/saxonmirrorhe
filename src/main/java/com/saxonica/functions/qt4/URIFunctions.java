package com.saxonica.functions.qt4;

import net.sf.saxon.expr.DefaultedArgumentExpression;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.Literal;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.ma.map.DictionaryMap;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.transpile.CSharpReplaceBody;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.StringValue;

import java.util.*;

/**
 * Common declarations for BuildURI and ParseURI.
 */

abstract public class URIFunctions extends SystemFunction {
    protected static HashSet<String> hierarchicalSchemes = new HashSet<>(Arrays.asList("http", "https", "ftp", "file"));
    protected static HashSet<String> nonHierarchicalSchemes = new HashSet<>(Arrays.asList("urn", "mailto", "news", "tel", "tag", "jar"));
    protected static HashMap<String,String> defaultPorts = new HashMap<>();
    static {
        defaultPorts.put("http", "80");
        defaultPorts.put("https", "443");
        defaultPorts.put("ssh", "22");
        defaultPorts.put("ftp", "21");
    }

    protected static final AtomicValue URI = new StringValue("uri");
    protected static final AtomicValue PATH_SEPARATOR = new StringValue("path-separator");
    protected static final AtomicValue QUERY_SEPARATOR = new StringValue("query-separator");
    protected static final AtomicValue ALLOW_DEPRECATED_FEATURES = new StringValue("allow-deprecated-features");
    protected static final AtomicValue OMIT_DEFAULT_PORTS = new StringValue("omit-default-ports");
    protected static final AtomicValue UNC_PATH = new StringValue("unc-path");
    protected static final AtomicValue SCHEME = new StringValue("scheme");
    protected static final AtomicValue AUTHORITY = new StringValue("authority");
    protected static final AtomicValue USERINFO = new StringValue("userinfo");
    protected static final AtomicValue HOST = new StringValue("host");
    protected static final AtomicValue PORT = new StringValue("port");
    protected static final AtomicValue PATH = new StringValue("path");
    protected static final AtomicValue FILEPATH = new StringValue("filepath");
    protected static final AtomicValue PATH_SEGMENTS = new StringValue("path-segments");
    protected static final AtomicValue QUERY_PARAMETERS = new StringValue("query-parameters");
    protected static final AtomicValue QUERY = new StringValue("query");
    protected static final AtomicValue KEY = new StringValue("key");
    protected static final AtomicValue VALUE = new StringValue("value");
    protected static final AtomicValue FRAGMENT = new StringValue("fragment");
    protected static final AtomicValue HIERARCHICAL = new StringValue("hierarchical");

    protected static final Set<Character> unescapedUriCharacters;
    static {
        HashSet<Character> chars = new HashSet<>();
        for (char ch = 'A'; ch <= 'Z'; ch++) {
            chars.add(ch);
        }
        for (char ch = 'a'; ch <= 'z'; ch++) {
            chars.add(ch);
        }
        for (char ch = '0'; ch <= '9'; ch++) {
            chars.add(ch);
        }
        chars.add('-');
        chars.add('.');
        chars.add('_');
        chars.add('~');
        chars.add('!');
        chars.add('$');
        chars.add('&');
        chars.add('\'');
        chars.add('(');
        chars.add(')');
        chars.add('*');
        chars.add('+');
        chars.add(',');
        chars.add(';');
        chars.add('=');
        chars.add(':');
        chars.add('@');

        unescapedUriCharacters = Collections.unmodifiableSet(chars);
    }

    protected char pathSeparator = '/';
    protected char querySeparator = '&';
    protected boolean allowDeprecatedFeatures = false;
    protected boolean omitDefaultPorts = false;
    protected boolean hierarchical = true;
    protected boolean uncPath = false;

    @Override
    public Expression makeFunctionCall(Expression... arguments) {
        Expression[] newArgs = new Expression[2];
        newArgs[0] = arguments[0];
        if (arguments.length < 2 || arguments[1] instanceof DefaultedArgumentExpression) {
            newArgs[1] = Literal.makeLiteral(new DictionaryMap());
        } else {
            newArgs[1] = arguments[1];
        }
        setArity(2);
        return super.makeFunctionCall(newArgs);
    }

    protected void checkOptions(Sequence[] arguments) throws XPathException {
        if (getArity() == 2) {
            MapItem options = (MapItem) arguments[1].head();
            if (options.get(PATH_SEPARATOR) != null) {
                String str = options.get(PATH_SEPARATOR).getStringValue();
                if (str.length() != 1) {
                    throw new IllegalArgumentException("Path separator must be a single character");
                }
                pathSeparator = str.charAt(0);
            }
            if (options.get(QUERY_SEPARATOR) != null) {
                String str = options.get(QUERY_SEPARATOR).getStringValue();
                if (str.length() != 1) {
                    throw new IllegalArgumentException("Path separator must be a single character");
                }
                querySeparator = str.charAt(0);
            }
            allowDeprecatedFeatures = options.get(ALLOW_DEPRECATED_FEATURES) != null && options.get(ALLOW_DEPRECATED_FEATURES).effectiveBooleanValue();
            omitDefaultPorts = options.get(OMIT_DEFAULT_PORTS) != null && options.get(OMIT_DEFAULT_PORTS).effectiveBooleanValue();
            uncPath = options.get(UNC_PATH) != null && options.get(UNC_PATH).effectiveBooleanValue();
        }
    }

    protected String encodePathSegment(GroundedValue value) throws XPathException {
        String input = value.getStringValue();
        StringBuilder output = new StringBuilder();
        for (int pos = 0; pos < input.length(); pos++) {
            char ch = input.charAt(pos);
            if (ch > 127 || unescapedUriCharacters.contains(ch)) {
                output.append(ch);
            } else {
                output.append("%");
                output.append(toTwoHexDigits(ch));
            }
        }
        return output.toString();
    }

    protected String encodeQueryPart(GroundedValue value) throws XPathException {
        // Like encodePathSegment but allow '/' and '?' unencoded.
        // Force '=' and '&' to be percent encoded.
        String input = value.getStringValue();
        StringBuilder output = new StringBuilder();
        for (int pos = 0; pos < input.length(); pos++) {
            char ch = input.charAt(pos);

            if (ch == querySeparator) {
                output.append("%");
                output.append(toTwoHexDigits(querySeparator));
                continue;
            }

            switch (ch) {
                case '=':
                    output.append("%3D");
                    break;
                case '&':
                    output.append("%26");
                    break;
                case '/':
                case '?':
                    output.append(ch);
                    break;
                default:
                    if (ch > 127 || unescapedUriCharacters.contains(ch)) {
                        output.append(ch);
                    } else {
                        output.append("%");
                        output.append(toTwoHexDigits(ch));
                    }
                    break;
            }
        }
        return output.toString();
    }

    protected String encodeFragmentIdentifier(GroundedValue value) throws XPathException {
        // Like encodePathSegment but allow '/' and '?' unencoded.
        String input = value.getStringValue();
        StringBuilder output = new StringBuilder();
        for (int pos = 0; pos < input.length(); pos++) {
            char ch = input.charAt(pos);
            if (ch > 127 || unescapedUriCharacters.contains(ch)) {
                output.append(ch);
            } else {
                switch (ch) {
                    case '/':
                    case '?':
                        output.append(ch);
                        break;
                    default:
                        output.append("%");
                        output.append(toTwoHexDigits(ch));
                        break;
                }
            }
        }
        return output.toString();
    }

    @CSharpReplaceBody(code="return String.Format(\"{0:X2}\", ch);")
    private String toTwoHexDigits(int ch) {
        return String.format("%02X", ch);
    }
}
