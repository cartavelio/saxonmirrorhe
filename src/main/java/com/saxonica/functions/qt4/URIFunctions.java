package com.saxonica.functions.qt4;

import net.sf.saxon.expr.DefaultedArgumentExpression;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.Literal;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.ma.map.EmptyMap;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.transpile.CSharpReplaceBody;
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

    protected static final StringValue URI = new StringValue("uri");
    protected static final StringValue ALLOW_DEPRECATED_FEATURES = new StringValue("allow-deprecated-features");
    protected static final StringValue OMIT_DEFAULT_PORTS = new StringValue("omit-default-ports");
    protected static final StringValue UNC_PATH = new StringValue("unc-path");
    protected static final StringValue SCHEME = new StringValue("scheme");
    protected static final StringValue AUTHORITY = new StringValue("authority");
    protected static final StringValue USERINFO = new StringValue("userinfo");
    protected static final StringValue HOST = new StringValue("host");
    protected static final StringValue PORT = new StringValue("port");
    protected static final StringValue PATH = new StringValue("path");
    protected static final StringValue FILEPATH = new StringValue("filepath");
    protected static final StringValue PATH_SEGMENTS = new StringValue("path-segments");
    protected static final StringValue QUERY_PARAMETERS = new StringValue("query-parameters");
    protected static final StringValue QUERY = new StringValue("query");
    protected static final StringValue KEY = new StringValue("key");
    protected static final StringValue VALUE = new StringValue("value");
    protected static final StringValue FRAGMENT = new StringValue("fragment");
    protected static final StringValue HIERARCHICAL = new StringValue("hierarchical");
    protected static final StringValue ABSOLUTE = new StringValue("absolute");

    protected static final Set<Character> escapedPathCharacters;
    static {
        HashSet<Character> pchars = new HashSet<>();
        pchars.add(' ');
        pchars.add('%');
        pchars.add('/');
        pchars.add('?');
        pchars.add('#');
        pchars.add('+');
        pchars.add('[');
        pchars.add(']');
        escapedPathCharacters = unmodifiable(pchars);
    }

    protected static final Set<Character> escapedQueryCharacters;
    static {
        HashSet<Character> qchars = new HashSet<>(escapedPathCharacters);
        qchars.remove('/');
        qchars.remove('?');
        qchars.add('=');
        qchars.add('&');
        escapedQueryCharacters = unmodifiable(qchars);
    }

    protected static final Set<Character> escapedFragidCharacters;
    static {
        HashSet<Character> fchars = new HashSet<>(escapedPathCharacters);
        fchars.remove('/');
        fchars.remove('?');
        escapedFragidCharacters = unmodifiable(fchars);
    }

    @CSharpReplaceBody(code="return chars;")
    private static Set<Character> unmodifiable(Set<Character> chars) {
        return Collections.unmodifiableSet(chars);
    }

    protected boolean allowDeprecatedFeatures = false;
    protected boolean omitDefaultPorts = false;
    protected boolean hierarchical = true;
    protected boolean uncPath = false;

    @Override
    public Expression makeFunctionCall(Expression... arguments) {
        Expression[] newArgs = new Expression[2];
        newArgs[0] = arguments[0];
        if (arguments.length < 2 || arguments[1] instanceof DefaultedArgumentExpression) {
            newArgs[1] = Literal.makeLiteral(EmptyMap.getInstance());
        } else {
            newArgs[1] = arguments[1];
        }
        setArity(2);
        return super.makeFunctionCall(newArgs);
    }

    protected void checkOptions(Sequence[] arguments) throws XPathException {
        if (getArity() == 2) {
            MapItem options = (MapItem) arguments[1].head();
            if (options == null) {
                options = EmptyMap.getInstance();
            }
            allowDeprecatedFeatures = options.get(ALLOW_DEPRECATED_FEATURES) != null && options.get(ALLOW_DEPRECATED_FEATURES).effectiveBooleanValue();
            omitDefaultPorts = options.get(OMIT_DEFAULT_PORTS) != null && options.get(OMIT_DEFAULT_PORTS).effectiveBooleanValue();
            uncPath = options.get(UNC_PATH) != null && options.get(UNC_PATH).effectiveBooleanValue();
        }
    }

    private String encodeSomeCharacters(GroundedValue value, Collection<Character> escaped) throws XPathException {
        String input = value.getStringValue();
        StringBuilder output = new StringBuilder();
        for (int pos = 0; pos < input.length(); pos++) {
            char ch = input.charAt(pos);
            if (ch < ' ' || escaped.contains(ch)) {
                output.append("%");
                output.append(toTwoHexDigits(ch));
            } else {
                output.append(ch);
            }
        }
        return output.toString();
    }

    protected String encodePathSegment(GroundedValue value) throws XPathException {
        return encodeSomeCharacters(value, escapedPathCharacters);
    }

    protected String encodeQueryPart(GroundedValue value) throws XPathException {
        return encodeSomeCharacters(value, escapedQueryCharacters);
    }

    protected String encodeFragmentIdentifier(GroundedValue value) throws XPathException {
        return encodeSomeCharacters(value, escapedFragidCharacters);
    }

    @CSharpReplaceBody(code="return String.Format(\"{0:X2}\", ch);")
    private String toTwoHexDigits(int ch) {
        return String.format("%02X", ch);
    }
}
