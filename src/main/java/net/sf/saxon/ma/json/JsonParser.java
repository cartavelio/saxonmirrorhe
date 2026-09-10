////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.ma.json;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.ma.map.Shape;
import net.sf.saxon.om.*;
import net.sf.saxon.serialize.charcode.UTF16CharacterSet;
import net.sf.saxon.str.*;
import net.sf.saxon.trans.Err;
import net.sf.saxon.trans.UncheckedXPathException;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.transpile.CSharp;
import net.sf.saxon.transpile.CSharpSimpleEnum;
import net.sf.saxon.type.StringToDouble;
import net.sf.saxon.value.BooleanValue;
import net.sf.saxon.value.DoubleValue;
import net.sf.saxon.value.StringValue;
import net.sf.saxon.z.IntIterator;

import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.function.Function;

/**
 * Parser for JSON, which notifies parsing events to a JsonHandler
 */
public class JsonParser {

    public static final int ESCAPE = 1;
    public static final int ALLOW_ANY_TOP_LEVEL = 2;
    public static final int LIBERAL = 4;
    public static final int VALIDATE = 8;
    public static final int DEBUG = 16;
    public static final int DUPLICATES_RETAINED = 32;
    public static final int DUPLICATES_LAST = 64;
    public static final int DUPLICATES_FIRST = 128;
    public static final int DUPLICATES_REJECTED = 256;
    public static final int NUMERIC_FORMAT_RETAINED = 512;
    public static final int ORDER_RETAINED = 1024;

    public static final int DUPLICATES_SPECIFIED = DUPLICATES_FIRST | DUPLICATES_LAST | DUPLICATES_RETAINED | DUPLICATES_REJECTED;

    public static final int NESTING_LIMIT = 10000;

    private static final String ERR_GRAMMAR = "FOJS0001";
    private static final String ERR_DUPLICATE = "FOJS0003";
    private static final String ERR_SCHEMA = "FOJS0004";
    private static final String ERR_OPTIONS = "FOJS0005";
    private static final String ERR_LIMITS = "FOJS0001";  // No specific code in spec

    private FunctionItem numberParser = null;
    private int nesting;
    private Function<UnicodeString, UnicodeString> invalidEscapeHandler;
    private HashMap<List<UnicodeString>, Shape> shapePool;

    /**
     * Create a JSON parser
     */

    public JsonParser() {
        nesting = 0;
    }

    /**
     * Parse the JSON string according to supplied options
     *
     * @param input   JSON input string
     * @param flags   options for the conversion as a map of xs:string : value pairs
     * @param handler event handler to which parsing events are notified
     * @param context XPath evaluation context
     * @throws XPathException if the syntax of the input is incorrect
     */
    public void parse(IntIterator input, int flags, JsonHandler handler, XPathContext context) throws XPathException {
        try {
            if (!input.hasNext()) {
                invalidJSON("An empty string is not valid JSON", ERR_GRAMMAR, 1);
            }

            if ((flags & ESCAPE) != 0) {
                invalidEscapeHandler = esc -> esc;
            } else if (handler.getFallbackFunction() != null) {
                FunctionItem fallback = handler.getFallbackFunction();
                invalidEscapeHandler = esc -> {
                    try {
                        Item result = fallback.call(context, new Sequence[]{new StringValue(esc)}).head();
                        return result == null ? EmptyUnicodeString.getInstance() : result.getUnicodeStringValue();
                    } catch (XPathException e) {
                        throw new UncheckedXPathException(e);
                    }
                };
            } else {
                invalidEscapeHandler = esc -> JsonHandler.REPLACEMENT;
            }

            JsonTokenizer t = new JsonTokenizer(input);
            t.next();

            try {
                parseConstruct(handler, t, flags, context);
            } catch (IllegalStateException e) {
                // e.g. unmatched surrogate pairs
                invalidJSON(e.getMessage(), ERR_GRAMMAR, t.lineNumber);
            }

            if (t.next() != JsonToken.EOF) {
                invalidJSON("Unexpected token beyond end of JSON input", ERR_GRAMMAR, t.lineNumber);
            }
        } catch (UncheckedXPathException e) {
            throw e.getXPathException().maybeWithErrorCode("FOJS0001");
        }

    }

    /**
     * Extract the requested JSON parsing options as a set of flags in a bit-significant integer
     * @param options the supplied options map
     * @param allowValidate true if the validate option is permitted
     * @param isSchemaAware true if the processor is schema-aware (only relevant when allowValidate=true)
     * @return the options as a sef of flags
     * @throws XPathException if any options are invalid
     */

    public static int getFlags(Map<String, GroundedValue> options, boolean allowValidate, boolean isSchemaAware) throws XPathException {
        int flags = 0;
        BooleanValue debug = (BooleanValue) options.get("debug");
        if (debug != null && debug.getBooleanValue()) {
            flags |= DEBUG;
        }

        BooleanValue escape = ((BooleanValue) options.get("escape"));
        if (escape != null && escape.getBooleanValue()) {
            flags |= ESCAPE;
            if (options.get("fallback") != null) {
                throw new XPathException("Cannot specify a fallback function when escape=true", "FOJS0005");
            }
        }

        BooleanValue retainOrder = ((BooleanValue) options.get("retain-order"));
        if (retainOrder != null && retainOrder.getBooleanValue()) {
            flags |= ORDER_RETAINED;
        }

        BooleanValue liberal = ((BooleanValue) options.get("liberal"));
        if (liberal != null && liberal.getBooleanValue()) {
            flags |= LIBERAL;
            flags |= ALLOW_ANY_TOP_LEVEL;
        }

        boolean validate = false;
        if (allowValidate) {
            validate = ((BooleanValue) options.get("validate")).getBooleanValue();
            if (validate) {
                if (!isSchemaAware) {
                    error("Requiring validation on non-schema-aware processor", ERR_SCHEMA);
                }
                flags |= VALIDATE;
            }
        }

        if (options.containsKey("duplicates")) {
            String duplicates = ((StringValue) options.get("duplicates")).getStringValue();
            switch (duplicates) {
                case "reject":
                    flags |= DUPLICATES_REJECTED;
                    break;
                case "use-last":
                    flags |= DUPLICATES_LAST;
                    break;
                case "use-first":
                    flags |= DUPLICATES_FIRST;
                    break;
                case "retain":
                    flags |= DUPLICATES_RETAINED;
                    break;
                default:
                    error("Invalid value for 'duplicates' option", ERR_OPTIONS);
                    break;
            }
            if (validate && "retain".equals(duplicates)) {
                error("The options validate:true and duplicates:retain cannot be used together", ERR_OPTIONS);
            }
        }
        return flags;
    }

    /**
     * Parse a JSON construct (top-level or nested)
     *
     * @param handler   the handler to generate the result
     * @param tokenizer the tokenizer, positioned at the first token of the construct to be read
     * @param flags     parsing options
     * @param context   XPath evaluation context
     * @throws net.sf.saxon.trans.XPathException if a dynamic error occurs (for example, invalid JSON input)
     */

    private void parseConstruct(JsonHandler handler, JsonTokenizer tokenizer, int flags, XPathContext context) throws XPathException {
        boolean debug = (flags & DEBUG) != 0;
        if (debug) {
            System.err.println("token:" + tokenizer.currentToken + " :" + tokenizer.currentTokenValue);
        }
        if (nesting > NESTING_LIMIT) {
            // Needed for C#, because we can't rely on catching StackOverflow
            invalidJSON("Objects are too deeply nested", ERR_LIMITS, tokenizer.lineNumber);
        }
        JsonToken tok = tokenizer.currentToken;
        switch (tok) {
            case LCURLY:
                nesting++;
                parseObject(handler, tokenizer, flags, context);
                nesting--;
                break;

            case LSQB:
                nesting++;
                parseArray(handler, tokenizer, flags, context);
                nesting--;
                break;

            case NUMERIC_LITERAL:
                String lexical = tokenizer.currentTokenValue.toString();
                Item d = parseNumericLiteral(lexical, flags, tokenizer.lineNumber, context);
                if (d == null) {
                    handler.writeNull();
                } else {
                    handler.writeNumeric(lexical, d);
                }
                break;

            case TRUE:
                handler.writeBoolean(true);
                break;

            case FALSE:
                handler.writeBoolean(false);
                break;

            case NULL:
                handler.writeNull();
                break;

            case STRING_LITERAL:
                UnicodeString literal = tokenizer.currentTokenValue;
                handler.writeString(unescape(literal, flags, ERR_GRAMMAR, invalidEscapeHandler));
                break;

            default:
                invalidJSON("Unexpected symbol: " + tokenizer.currentTokenValue, ERR_GRAMMAR, tokenizer.lineNumber);
                break;
        }
    }

    /**
     * Parse a JSON object (or map), i.e. construct delimited by curly braces
     *
     * @param handler   the handler to generate the result
     * @param tokenizer the tokenizer, positioned at the object to be read
     * @param flags     parsing options as a set of flags
     * @param context   XPath evaluation context
     * @throws net.sf.saxon.trans.XPathException if a dynamic error occurs (such as invalid JSON input)
     */

    private void parseObject(JsonHandler handler, JsonTokenizer tokenizer, int flags, XPathContext context) throws XPathException {
        boolean liberal = (flags & LIBERAL) != 0;
        handler.startMap();
        JsonToken tok = tokenizer.next();
        while (tok != JsonToken.RCURLY) {
            if (tok != JsonToken.STRING_LITERAL && !(tok == JsonToken.UNQUOTED_STRING && liberal)) {
                invalidJSON("Property name must be a string literal (found " + showToken(tok, tokenizer.currentTokenValue.toString() + ")"),
                            ERR_GRAMMAR, tokenizer.lineNumber);
            }
            UnicodeString key = tokenizer.currentTokenValue;
            key = unescape(key, flags, ERR_GRAMMAR, invalidEscapeHandler);
            UnicodeString reEscaped = handler.reEscape(key);
            tok = tokenizer.next();
            if (tok != JsonToken.COLON) {
                invalidJSON("Missing colon after \"" + Err.wrap(key) + "\"", ERR_GRAMMAR, tokenizer.lineNumber);
            }
            tokenizer.next();
            boolean duplicate = handler.setKey(key, reEscaped);
            if (duplicate && ((flags & DUPLICATES_REJECTED) != 0)) {
                invalidJSON("Duplicate key value \"" + Err.wrap(key) + "\"", ERR_DUPLICATE, tokenizer.lineNumber);
            }
            try {
                if (!duplicate || ((flags & (DUPLICATES_LAST | DUPLICATES_RETAINED)) != 0)) {
                    parseConstruct(handler, tokenizer, flags, context);
                } else {
                    // retain first: parse the duplicate value but discard it
                    JsonHandler h2 = new JsonHandler();
                    h2.setContext(context);
                    parseConstruct(h2, tokenizer, flags, context);
                }
            } catch (StackOverflowError e) {
                invalidJSON("Objects are too deeply nested", ERR_LIMITS, tokenizer.lineNumber);
            }
            tok = tokenizer.next();
            if (tok == JsonToken.COMMA) {
                tok = tokenizer.next();
                if (tok == JsonToken.RCURLY) {
                    if (liberal) {
                        break;  // tolerate the trailing comma
                    } else {
                        invalidJSON("Trailing comma after entry in object", ERR_GRAMMAR, tokenizer.lineNumber);
                    }
                }
            } else if (tok == JsonToken.RCURLY) {
                break;
            } else {
                invalidJSON("Unexpected token after value of \"" + Err.wrap(key) + "\" property", ERR_GRAMMAR, tokenizer.lineNumber);
            }
        }
        handler.endMap();
    }

    /**
     * Parse a JSON array, i.e. construct delimited by square brackets
     *
     * @param handler   the handler to generate the result
     * @param tokenizer the tokenizer, positioned at the object to be read
     * @param flags     parsing options
     * @param context   XPath evaluation context
     * @throws net.sf.saxon.trans.XPathException if a dynamic error occurs (such as invalid JSON input)
     */

    private void parseArray(JsonHandler handler, JsonTokenizer tokenizer, int flags, XPathContext context) throws XPathException {
        boolean liberal = (flags & LIBERAL) != 0;
        handler.startArray();
        JsonToken tok = tokenizer.next();
        if (tok == JsonToken.RSQB) {
            handler.endArray();
            return;
        }
        while (true) {
            try {
                parseConstruct(handler, tokenizer, flags, context);
            } catch (StackOverflowError e) {
                invalidJSON("Arrays are too deeply nested", ERR_LIMITS, tokenizer.lineNumber);
            }
            tok = tokenizer.next();
            if (tok == JsonToken.COMMA) {
                tok = tokenizer.next();
                if (tok == JsonToken.RSQB) {
                    if (liberal) {
                        break;// tolerate the trailing comma
                    } else {
                        invalidJSON("Trailing comma after entry in array", ERR_GRAMMAR, tokenizer.lineNumber);
                    }
                }
            } else if (tok == JsonToken.RSQB) {
                break;
            } else {
                invalidJSON("Unexpected token (" + showToken(tok, tokenizer.currentTokenValue.toString()) +
                                    ") after entry in array", ERR_GRAMMAR, tokenizer.lineNumber);
            }
        }
        handler.endArray();
    }

    /**
     * Parse a JSON numeric literal,
     *
     * @param token the numeric literal to be parsed and converted
     * @param flags parsing options
     * @return the result of parsing and conversion to XDM
     * @throws net.sf.saxon.trans.XPathException if a dynamic error occurs (such as invalid JSON input)
     */

    private Item parseNumericLiteral(String token, int flags, int lineNumber, XPathContext context) throws XPathException {
        try {
            if ((flags & LIBERAL) == 0) {
                // extra checks on the number disabled by choosing spec="liberal"
                if (token.startsWith("+")) {
                    invalidJSON("Leading + sign not allowed: " + token, ERR_GRAMMAR, lineNumber);
                } else {
                    String t = token;
                    if (t.startsWith("-")) {
                        t = t.substring(1);
                    }
                    if (t.startsWith("0") &&
                            !(t.equals("0") || t.startsWith("0.") || t.startsWith("0e") || t.startsWith("0E"))) {
                        invalidJSON("Redundant leading zeroes not allowed: " + token, ERR_GRAMMAR, lineNumber);
                    }
                    if (t.endsWith(".") || t.contains(".e") || t.contains(".E")) {
                        invalidJSON("Empty fractional part not allowed", ERR_GRAMMAR, lineNumber);
                    }
                    if (t.startsWith(".")) {
                        invalidJSON("Empty integer part not allowed", ERR_GRAMMAR, lineNumber);
                    }
                }
            }
            if (numberParser != null) {
                Sequence[] args = new Sequence[1];
                args[0] = StringValue.makeUntypedAtomic(StringTool.fromCharSequence(token));
                Item parserResult = SystemFunction.dynamicCall(numberParser, context, args).head();
                return (parserResult == null && ((flags & NUMERIC_FORMAT_RETAINED) != 0)) ? StringValue.EMPTY_STRING : parserResult;
            } else if ((flags & NUMERIC_FORMAT_RETAINED) != 0) {
                return new StringValue(token);
            } else {
                return new DoubleValue(StringToDouble.getInstance().stringToNumber(StringView.tidy(token)));
            }
        } catch (NumberFormatException e) {
            invalidJSON("Invalid numeric literal: " + e.getMessage(), ERR_GRAMMAR, lineNumber);
            return DoubleValue.NaN;
        }
    }

    /**
     * Unescape a JSON string literal
     *
     * @param literal    the string literal to be processed
     * @param flags      parsing options
     * @param errorCode  Error code
     * @param badEscapeHandler callback to handle invalid escape sequences
     * @return the result of parsing and conversion to XDM
     * @throws net.sf.saxon.trans.XPathException if a dynamic error occurs (such as invalid JSON input)
     */

    public static UnicodeString unescape(UnicodeString literal,
                                         int flags,
                                         String errorCode,
                                         Function<UnicodeString, UnicodeString> badEscapeHandler) throws XPathException {
        if (literal.indexOf('\\') < 0) {
            return literal;
        }
        boolean liberal = (flags & LIBERAL) != 0;
        // We use a StringBuilder rather than a UnicodeBuilder because the string might contain escape
        // sequences corresponding to unpaired surrogates
        StringBuilder buffer = new StringBuilder();
        for (int i = 0; i < literal.length(); i++) {
            int c = literal.codePointAt(i);
            if (c == '\\') {
                if (i++ == literal.length() - 1) {
                    throw new XPathException("Invalid JSON escape: String " + Err.wrap(literal) + " ends in backslash", errorCode);
                }
                switch (literal.codePointAt(i)) {
                    case '"':
                        buffer.append('"');
                        break;
                    case '\\':
                        buffer.append('\\');
                        break;
                    case '/':
                        buffer.append('/');
                        break;
                    case 'b':
                        buffer.append('\b');
                        break;
                    case 'f':
                        buffer.append('\f');
                        break;
                    case 'n':
                        buffer.append('\n');
                        break;
                    case 'r':
                        buffer.append('\r');
                        break;
                    case 't':
                        buffer.append('\t');
                        break;
                    case 'u':
                        try {
                            String hex = literal.substring(i + 1, i + 5).toString();
                            int code = Integer.parseInt(hex, 16);
                            buffer.append((char) code);
                            i += 4;
                        } catch (Exception e) {
                            if (liberal) {
                                buffer.append("\\u");
                            } else {
                                throw new XPathException("Invalid JSON escape: \\u must be followed by four hex characters", errorCode);
                            }
                        }
                        break;
                    default:
                        if (liberal) {
                            buffer.appendCodePoint(literal.codePointAt(i));
                        } else {
                            int next = literal.codePointAt(i);
                            UnicodeString xx = next < 256 ? new UnicodeChar(next) : StringView.of("x" + Integer.toHexString(next));
                            throw new XPathException("Unknown escape sequence \\" + xx, errorCode);
                        }
                        break;
                }
            } else {
                buffer.appendCodePoint(c);
            }
        }
        // Now handle surrogates (matching pairs, or unmatched)
        TwineBuilder tb = TwineBuilder.make(buffer.length());
        for (int i=0; i<buffer.length(); i++) {
            char c = buffer.charAt(i);
            if (UTF16CharacterSet.isSurrogate(c)) {
                if (UTF16CharacterSet.isHighSurrogate(c)) {
                    if (i+1 < buffer.length() && UTF16CharacterSet.isLowSurrogate(buffer.charAt(i+1))) {
                        tb = tb.append(UTF16CharacterSet.combinePair(c, buffer.charAt(i+1)));
                        i++;
                    } else {
                        tb = tb.append(badEscapeHandler.apply(StringView.of("\\u" + JsonReceiver.hex4(c))));
                    }
                } else {
                    tb = tb.append(badEscapeHandler.apply(StringView.of("\\u" + JsonReceiver.hex4(c))));
                }
            } else {
                tb = tb.append(c);
            }
        }
        return tb.toUnicodeString();
    }

    /**
     * Throw an error
     *
     * @param message the error message
     * @param code    the error code to be used
     * @throws net.sf.saxon.trans.XPathException always
     */

    private static void error(String message, String code)
            throws XPathException {
        throw new XPathException(message, code);
    }

    /**
     * Throw an error
     *
     * @param message the error message
     * @param code    the error code to be used
     * @throws net.sf.saxon.trans.XPathException always
     */

    private static void invalidJSON(String message, String code, int lineNumber)
            throws XPathException {
        error("Invalid JSON input on line " + lineNumber + ": " + message, code);
    }

    @CSharpSimpleEnum
    public enum JsonToken {
        LSQB, RSQB, LCURLY, RCURLY, STRING_LITERAL, NUMERIC_LITERAL, TRUE,
        FALSE, NULL, COLON, COMMA, UNQUOTED_STRING, EOF
    }

    /**
     * Inner class to do the tokenization
     */

    private static class JsonTokenizer {

        //public final UnicodeString input;
        //public int position;
        private final IntIterator codepoints;
        private int ch;
        public int lineNumber = 1;
        public JsonToken currentToken;
        public UnicodeString currentTokenValue;

        JsonTokenizer(IntIterator input) {
            codepoints = input;
            readCh();
            // Ignore a leading BOM
            if (ch == 65279) {
                readCh();
            }
        }

        public JsonToken next() throws XPathException {
            currentToken = readToken();
            return currentToken;
        }

        private void readCh() {
            if (codepoints.hasNext()) {
                ch = codepoints.next();
            } else {
                ch = -1;
            }
        }

        private JsonToken readToken() throws XPathException {
            if (ch == -1) {
                return JsonToken.EOF;
            }
            boolean breakLoop = false;
            do {
                switch (ch) {
                    case '\n':
                        lineNumber++;
                        // drop through
                        CSharp.emitCode("goto case ' ';");
                    case '\r':
                    case ' ':
                    case '\t':
                        readCh();
                        if (ch == -1) {
                            return JsonToken.EOF;
                        }
                        break;
                    default:
                        breakLoop = true;
                        break;
                }
            } while (!breakLoop);

            switch (ch) {
                case '[':
                    readCh();
                    return JsonToken.LSQB;
                case '{':
                    readCh();
                    return JsonToken.LCURLY;
                case ']':
                    readCh();
                    return JsonToken.RSQB;
                case '}':
                    readCh();
                    return JsonToken.RCURLY;
                case '"': {
                    // String literal. We check that the literal conforms to the JSON grammar, but
                    // return it exactly as written, with no expansion of escape sequences
                    TwineBuilder tb = TwineBuilder.make(64);
                    readCh();
                    while (true) {
                        if (ch == -1) {
                            invalidJSON("Unclosed quotes in string literal", ERR_GRAMMAR, lineNumber);
                        } else if (ch < 32) {
                            invalidJSON("Unescaped control character (x" + Integer.toHexString(ch) + ")", ERR_GRAMMAR, lineNumber);
                        } else if (ch == '\\') {
                            tb = tb.append(ch);
                            readCh();
                            if (ch == 'u') {
                                tb = tb.append(ch);
                                readCh();
                                tb = tb.append(expectHex(ch, lineNumber));
                                readCh();
                                tb = tb.append(expectHex(ch, lineNumber));
                                readCh();
                                tb = tb.append(expectHex(ch, lineNumber));
                                readCh();
                                tb = tb.append(expectHex(ch, lineNumber));
                                readCh();
                            } else if (ch == -1) {
                                invalidJSON("Backslash in string literal at end of input", ERR_GRAMMAR, lineNumber);
                            } else {
                                tb = tb.append(ch);
                                readCh();
                            }
                        } else if (ch == '"') {
                            readCh();
                            currentTokenValue = tb.toUnicodeString();
                            return JsonToken.STRING_LITERAL;
                        } else {
                            tb = tb.append(ch);
                            readCh();
                        }
                    }
                }
                case ':':
                    readCh();
                    return JsonToken.COLON;
                case ',':
                    readCh();
                    return JsonToken.COMMA;
                case '-':
                case '+': // for liberal parsing
                case '.': // for liberal parsing
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9': {
                    TwineBuilder tb = TwineBuilder.make(16);
                    tb = tb.append(ch);
                    readCh();
                    while (ch != -1) {
                        if ((ch >= '0' && ch <= '9') || ch == '-' || ch == '+' || ch == '.' || ch == 'e' || ch == 'E') {
                            tb = tb.append(ch);
                            readCh();
                        } else {
                            break;
                        }
                    }
                    currentTokenValue = tb.toUnicodeString();
                    return JsonToken.NUMERIC_LITERAL;
                }
                default: {
                    // Allow unquoted strings in liberal mode
                    TwineBuilder tb = TwineBuilder.make(32);
                    if (NameChecker.isNCNameChar(ch)) {
                        tb = tb.append(ch);
                        readCh();
                        while (ch != -1) {
                            if (NameChecker.isNCNameChar(ch)) {
                                tb = tb.append(ch);
                                readCh();
                            } else {
                                break;
                            }
                        }
                        currentTokenValue = tb.toUnicodeString();
                        long len = currentTokenValue.length();
                        if (len == 4) {
                            if (currentTokenValue.equals(StringConstants.TRUE)) {
                                return JsonToken.TRUE;
                            }
                            if (currentTokenValue.equals(StringConstants.NULL)) {
                                return JsonToken.NULL;
                            }
                        } else if (len == 5) {
                            if (currentTokenValue.equals(StringConstants.FALSE)) {
                                return JsonToken.FALSE;
                            }
                        }
                        return JsonToken.UNQUOTED_STRING;

                    } else {
//                        int c = input.codePointAt(--position);
//                        String s = UTF16CharacterSet.isSurrogate(c) ? "" : " '" + c + "'";
                        invalidJSON("Unexpected character " + ch + " (\\u" +
                                            Integer.toHexString(ch) + ")", ERR_GRAMMAR, lineNumber);
                        return JsonToken.EOF;
                    }
                }
            }
        }
    }

    private static int expectHex(int ch, int lineNumber) throws XPathException {
        if ((ch >= '0' && ch <= '9') || (ch >= 'a' && ch <= 'f') || (ch >= 'A' && ch <= 'F')) {
            return ch;
        } else {
            invalidJSON("Four hex digits required after \\u ", ERR_GRAMMAR, lineNumber);
            return ch;
        }

    }


    public static String showToken(JsonToken token, String currentTokenValue) {
        switch (token) {
            case LSQB:
                return "[";
            case RSQB:
                return "]";
            case LCURLY:
                return "{";
            case RCURLY:
                return "}";
            case STRING_LITERAL:
                return "string (\"" + currentTokenValue + "\")";
            case NUMERIC_LITERAL:
                return "number (" + currentTokenValue + ")";
            case TRUE:
                return "true";
            case FALSE:
                return "false";
            case NULL:
                return "null";
            case COLON:
                return ":";
            case COMMA:
                return ",";
            case EOF:
                return "<eof>";
            default:
                return "<" + token + ">";
        }
    }


    public void setNumberParser(Map<String, GroundedValue> options, XPathContext context) throws XPathException {
        Sequence val = options.get("number-parser");
        if (val != null) {
            Item fn = val.head();
            if (fn instanceof FunctionItem) {
                numberParser = (FunctionItem) fn;
            } else {
                throw new XPathException("Value of option 'number-parser' is not a function", "XPTY0004");
            }
        }
    }

    public Shape obtainShape(List<UnicodeString> keyList) {
        if (shapePool == null) {
            shapePool = new HashMap<>();
        }
        return shapePool.computeIfAbsent(keyList, keys -> new Shape(keyList.toArray(new UnicodeString[]{})));
    }


}

// Copyright (c) 2018-2023 Saxonica Limited
