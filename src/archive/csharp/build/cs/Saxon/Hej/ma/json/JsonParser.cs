using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.ma.json {
    public class JsonParser {
        public const int ESCAPE = 1;
        public const int ALLOW_ANY_TOP_LEVEL = 2;
        public const int LIBERAL = 4;
        public const int VALIDATE = 8;
        public const int DEBUG = 16;
        public const int DUPLICATES_RETAINED = 32;
        public const int DUPLICATES_LAST = 64;
        public const int DUPLICATES_FIRST = 128;
        public const int DUPLICATES_REJECTED = 256;
        public const int DUPLICATES_SPECIFIED = (((DUPLICATES_FIRST|DUPLICATES_LAST)|DUPLICATES_RETAINED)|DUPLICATES_REJECTED);
        public const int NESTING_LIMIT = 10000;
        private const string ERR_GRAMMAR = "FOJS0001";
        private const string ERR_DUPLICATE = "FOJS0003";
        private const string ERR_SCHEMA = "FOJS0004";
        private const string ERR_OPTIONS = "FOJS0005";
        private const string ERR_LIMITS = "FOJS0001";
        private Saxon.Hej.om.Function numberParser = null;
        private int nesting;
        public JsonParser()  {
            nesting = 0;
        }
        public void parse(string input, int flags, Saxon.Hej.ma.json.JsonHandler handler, Saxon.Hej.expr.XPathContext context) {
            if (input.Length==0) {
                invalidJSON("An empty string is not valid JSON", ERR_GRAMMAR, 1);
            }
            Saxon.Hej.ma.json.JsonParser.JsonTokenizer t = new Saxon.Hej.ma.json.JsonParser.JsonTokenizer(input);
            t.next();
            try {
                parseConstruct(handler, t, flags, context);
            } catch (Saxon.Ejava.lang.IllegalStateException e) {
                invalidJSON(e.Message, ERR_GRAMMAR, t.lineNumber);
            }
            if ((t.next() != Saxon.Hej.ma.json.JsonParser.JsonToken.EOF)) {
                invalidJSON("Unexpected token beyond end of JSON input", ERR_GRAMMAR, t.lineNumber);
            }
        }
        public static int getFlags(System.Collections.Generic.IDictionary<string,Saxon.Hej.om.Sequence> options, bool allowValidate, bool isSchemaAware) {
            int flags = 0;
            Saxon.Hej.value.BooleanValue debug = (Saxon.Hej.value.BooleanValue)(Saxon.Impl.Helpers.MapUtils.Get(options, "debug"));
            if (((debug != null) && debug.getBooleanValue())) {
                flags |= DEBUG;
            }
            Saxon.Hej.value.BooleanValue escape = ((Saxon.Hej.value.BooleanValue)(Saxon.Impl.Helpers.MapUtils.Get(options, "escape")));
            if (((escape != null) && escape.getBooleanValue())) {
                flags |= ESCAPE;
                if ((Saxon.Impl.Helpers.MapUtils.Get(options, "fallback") != null)) {
                    throw new Saxon.Hej.trans.XPathException("Cannot specify a fallback function when escape=true", "FOJS0005");
                }
            }
            Saxon.Hej.value.BooleanValue liberal = ((Saxon.Hej.value.BooleanValue)(Saxon.Impl.Helpers.MapUtils.Get(options, "liberal")));
            if (((liberal != null) && liberal.getBooleanValue())) {
                flags |= LIBERAL;
                flags |= ALLOW_ANY_TOP_LEVEL;
            }
            bool validate = false;
            if (allowValidate) {
                validate = ((Saxon.Hej.value.BooleanValue)(Saxon.Impl.Helpers.MapUtils.Get(options, "validate"))).getBooleanValue();
                if (validate) {
                    if (!(isSchemaAware)) {
                        error("Requiring validation on non-schema-aware processor", ERR_SCHEMA);
                    }
                    flags |= VALIDATE;
                }
            }
            if (options.ContainsKey("duplicates")) {
                string duplicates = ((Saxon.Hej.value.StringValue)(Saxon.Impl.Helpers.MapUtils.Get(options, "duplicates"))).getStringValue();
                switch (duplicates) {
                  case "reject":
                    flags |= DUPLICATES_REJECTED;
                    break ;
                  case "use-last":
                    flags |= DUPLICATES_LAST;
                    break ;
                  case "use-first":
                    flags |= DUPLICATES_FIRST;
                    break ;
                  case "retain":
                    flags |= DUPLICATES_RETAINED;
                    break ;
                  default:
                    error("Invalid value for 'duplicates' option", ERR_OPTIONS);
                    break ;
                }
                if ((validate && "retain".Equals(duplicates))) {
                    error("The options validate:true and duplicates:retain cannot be used together", ERR_OPTIONS);
                }
            }
            return flags;
        }
        private void parseConstruct(Saxon.Hej.ma.json.JsonHandler handler, Saxon.Hej.ma.json.JsonParser.JsonTokenizer tokenizer, int flags, Saxon.Hej.expr.XPathContext context) {
            bool debug = (((flags&DEBUG)) != 0);
            if (debug) {
                System.Console.Error.WriteLine(((("token:"+tokenizer.currentToken)+" :")+tokenizer.currentTokenValue));
            }
            if ((nesting>NESTING_LIMIT)) {
                invalidJSON("Objects are too deeply nested", ERR_LIMITS, tokenizer.lineNumber);
            }
            Saxon.Hej.ma.json.JsonParser.JsonToken tok = tokenizer.currentToken;
            switch (tok) {
              case Saxon.Hej.ma.json.JsonParser.JsonToken.LCURLY:
                nesting++;
                parseObject(handler, tokenizer, flags, context);
                nesting--;
                break ;
              case Saxon.Hej.ma.json.JsonParser.JsonToken.LSQB:
                nesting++;
                parseArray(handler, tokenizer, flags, context);
                nesting--;
                break ;
              case Saxon.Hej.ma.json.JsonParser.JsonToken.NUMERIC_LITERAL:
                string lexical = tokenizer.currentTokenValue.ToString();
                Saxon.Hej.value.AtomicValue d = parseNumericLiteral(lexical, flags, tokenizer.lineNumber, context);
                handler.writeNumeric(lexical, d);
                break ;
              case Saxon.Hej.ma.json.JsonParser.JsonToken.TRUE:
                handler.writeBoolean(true);
                break ;
              case Saxon.Hej.ma.json.JsonParser.JsonToken.FALSE:
                handler.writeBoolean(false);
                break ;
              case Saxon.Hej.ma.json.JsonParser.JsonToken.NULL:
                handler.writeNull();
                break ;
              case Saxon.Hej.ma.json.JsonParser.JsonToken.STRING_LITERAL:
                string literal = tokenizer.currentTokenValue.ToString();
                handler.writeString(unescape(literal, flags, ERR_GRAMMAR, tokenizer.lineNumber));
                break ;
              default:
                invalidJSON(("Unexpected symbol: "+tokenizer.currentTokenValue), ERR_GRAMMAR, tokenizer.lineNumber);
                break ;
            }
        }
        private void parseObject(Saxon.Hej.ma.json.JsonHandler handler, Saxon.Hej.ma.json.JsonParser.JsonTokenizer tokenizer, int flags, Saxon.Hej.expr.XPathContext context) {
            bool liberal = (((flags&LIBERAL)) != 0);
            handler.startMap();
            Saxon.Hej.ma.json.JsonParser.JsonToken tok = tokenizer.next();
            while ((tok != Saxon.Hej.ma.json.JsonParser.JsonToken.RCURLY)) {
                if (((tok != Saxon.Hej.ma.json.JsonParser.JsonToken.STRING_LITERAL) && !((((tok == Saxon.Hej.ma.json.JsonParser.JsonToken.UNQUOTED_STRING) && liberal))))) {
                    invalidJSON(("Property name must be a string literal (found "+showToken(tok, (tokenizer.currentTokenValue.ToString()+")"))), ERR_GRAMMAR, tokenizer.lineNumber);
                }
                string key = tokenizer.currentTokenValue.ToString();
                key = unescape(key, flags, ERR_GRAMMAR, tokenizer.lineNumber);
                string reEscaped = handler.reEscape(key);
                tok = tokenizer.next();
                if ((tok != Saxon.Hej.ma.json.JsonParser.JsonToken.COLON)) {
                    invalidJSON((("Missing colon after \""+Saxon.Hej.trans.Err.wrap(key))+"\""), ERR_GRAMMAR, tokenizer.lineNumber);
                }
                tokenizer.next();
                bool duplicate = handler.setKey(key, reEscaped);
                if ((duplicate && ((((flags&DUPLICATES_REJECTED)) != 0)))) {
                    invalidJSON((("Duplicate key value \""+Saxon.Hej.trans.Err.wrap(key))+"\""), ERR_DUPLICATE, tokenizer.lineNumber);
                }
                try {
                    if ((!(duplicate) || ((((flags&((DUPLICATES_LAST|DUPLICATES_RETAINED)))) != 0)))) {
                        parseConstruct(handler, tokenizer, flags, context);
                    } else {
                        Saxon.Hej.ma.json.JsonHandler h2 = new Saxon.Hej.ma.json.JsonHandler();
                        h2.setContext(context);
                        parseConstruct(h2, tokenizer, flags, context);
                    }
                } catch (System.StackOverflowException e) {
                    invalidJSON("Objects are too deeply nested", ERR_LIMITS, tokenizer.lineNumber);
                }
                tok = tokenizer.next();
                if ((tok == Saxon.Hej.ma.json.JsonParser.JsonToken.COMMA)) {
                    tok = tokenizer.next();
                    if ((tok == Saxon.Hej.ma.json.JsonParser.JsonToken.RCURLY)) {
                        if (liberal) {
                            break ;
                        } else {
                            invalidJSON("Trailing comma after entry in object", ERR_GRAMMAR, tokenizer.lineNumber);
                        }
                    }
                } else {
                    if ((tok == Saxon.Hej.ma.json.JsonParser.JsonToken.RCURLY)) {
                        break ;
                    } else {
                        invalidJSON((("Unexpected token after value of \""+Saxon.Hej.trans.Err.wrap(key))+"\" property"), ERR_GRAMMAR, tokenizer.lineNumber);
                    }
                }
            }
            handler.endMap();
        }
        private void parseArray(Saxon.Hej.ma.json.JsonHandler handler, Saxon.Hej.ma.json.JsonParser.JsonTokenizer tokenizer, int flags, Saxon.Hej.expr.XPathContext context) {
            bool liberal = (((flags&LIBERAL)) != 0);
            handler.startArray();
            Saxon.Hej.ma.json.JsonParser.JsonToken tok = tokenizer.next();
            if ((tok == Saxon.Hej.ma.json.JsonParser.JsonToken.RSQB)) {
                handler.endArray();
                return ;
            }
            while (true) {
                try {
                    parseConstruct(handler, tokenizer, flags, context);
                } catch (System.StackOverflowException e) {
                    invalidJSON("Arrays are too deeply nested", ERR_LIMITS, tokenizer.lineNumber);
                }
                tok = tokenizer.next();
                if ((tok == Saxon.Hej.ma.json.JsonParser.JsonToken.COMMA)) {
                    tok = tokenizer.next();
                    if ((tok == Saxon.Hej.ma.json.JsonParser.JsonToken.RSQB)) {
                        if (liberal) {
                            break ;
                        } else {
                            invalidJSON("Trailing comma after entry in array", ERR_GRAMMAR, tokenizer.lineNumber);
                        }
                    }
                } else {
                    if ((tok == Saxon.Hej.ma.json.JsonParser.JsonToken.RSQB)) {
                        break ;
                    } else {
                        invalidJSON((("Unexpected token ("+showToken(tok, tokenizer.currentTokenValue.ToString()))+") after entry in array"), ERR_GRAMMAR, tokenizer.lineNumber);
                    }
                }
            }
            handler.endArray();
        }
        private Saxon.Hej.value.AtomicValue parseNumericLiteral(string token, int flags, int lineNumber, Saxon.Hej.expr.XPathContext context) {
            try {
                if ((((flags&LIBERAL)) == 0)) {
                    if (token.StartsWith("+")) {
                        invalidJSON(("Leading + sign not allowed: "+token), ERR_GRAMMAR, lineNumber);
                    } else {
                        string t = token;
                        if (t.StartsWith("-")) {
                            t = t.Substring(1);
                        }
                        if ((t.StartsWith("0") && !(((((t.Equals("0") || t.StartsWith("0.")) || t.StartsWith("0e")) || t.StartsWith("0E")))))) {
                            invalidJSON(("Redundant leading zeroes not allowed: "+token), ERR_GRAMMAR, lineNumber);
                        }
                        if (((t.EndsWith(".") || t.Contains(".e")) || t.Contains(".E"))) {
                            invalidJSON("Empty fractional part not allowed", ERR_GRAMMAR, lineNumber);
                        }
                        if (t.StartsWith(".")) {
                            invalidJSON("Empty integer part not allowed", ERR_GRAMMAR, lineNumber);
                        }
                    }
                }
                if ((numberParser != null)) {
                    Saxon.Hej.om.Sequence[] args = new Saxon.Hej.om.Sequence[1];
                    args[0] = new Saxon.Hej.value.StringValue(token);
                    Saxon.Hej.om.Sequence result = Saxon.Hej.functions.SystemFunction.dynamicCall(numberParser, context, args).head();
                    return (Saxon.Hej.value.AtomicValue)(result.head());
                } else {
                    return new Saxon.Hej.value.DoubleValue(Saxon.Hej.type.StringToDouble.getInstance().stringToNumber(Saxon.Hej.str.StringView.tidy(token)));
                }
            } catch (Saxon.Ejava.lang.NumberFormatException e) {
                invalidJSON(("Invalid numeric literal: "+e.Message), ERR_GRAMMAR, lineNumber);
                return Saxon.Hej.value.DoubleValue.NaN;
            }
        }
        public static string unescape(string literal, int flags, string errorCode, int lineNumber) {
            if ((literal.IndexOf('\\')<0)) {
                return literal;
            }
            bool liberal = (((flags&LIBERAL)) != 0);
            Saxon.Ejava.lang.StringBuilder buffer = new Saxon.Ejava.lang.StringBuilder(literal.Length);
            for (int i = 0; (i<literal.Length); i++) {
                char c = literal[i];
                if ((c == '\\')) {
                    if ((i++ == (literal.Length-1))) {
                        throw new Saxon.Hej.trans.XPathException((("Invalid JSON escape: String "+Saxon.Hej.trans.Err.wrap(literal))+" ends in backslash"), errorCode);
                    }
                    switch (literal[i]) {
                      case '"':
                        buffer.append('"');
                        break ;
                      case '\\':
                        buffer.append('\\');
                        break ;
                      case '/':
                        buffer.append('/');
                        break ;
                      case 'b':
                        buffer.append('\b');
                        break ;
                      case 'f':
                        buffer.append('\f');
                        break ;
                      case 'n':
                        buffer.append('\n');
                        break ;
                      case 'r':
                        buffer.append('\r');
                        break ;
                      case 't':
                        buffer.append('\t');
                        break ;
                      case 'u':
                        try {
                            string hex = literal.Substring((i+1), (i+5) - (i+1));
                            int code = Saxon.Impl.Helpers.Utils.ParseInt(hex, 16);
                            buffer.append((char)(code));
                            i += 4;
                        } catch (System.Exception e) {
                            if (liberal) {
                                buffer.append("\\u");
                            } else {
                                throw new Saxon.Hej.trans.XPathException("Invalid JSON escape: \\u must be followed by four hex characters", errorCode);
                            }
                        }
                        break ;
                      default:
                        if (liberal) {
                            buffer.append(literal[i]);
                        } else {
                            char next = literal[i];
                            string xx = ((next<256) ? (next+"") : ("x"+Saxon.Impl.Helpers.Utils.intToHexString(next)));
                            throw new Saxon.Hej.trans.XPathException(("Unknown escape sequence \\"+xx), errorCode);
                        }
                        break ;
                    }
                } else {
                    buffer.append(c);
                }
            }
            return buffer.ToString();
        }
        private static void error(string message, string code) {
            throw new Saxon.Hej.trans.XPathException(message, code);
        }
        private static void invalidJSON(string message, string code, int lineNumber) {
            error(((("Invalid JSON input on line "+lineNumber)+": ")+message), code);
        }
        public enum JsonToken {
            LSQB,
            RSQB,
            LCURLY,
            RCURLY,
            STRING_LITERAL,
            NUMERIC_LITERAL,
            TRUE,
            FALSE,
            NULL,
            COLON,
            COMMA,
            UNQUOTED_STRING,
            EOF,
        }
        private class JsonTokenizer {
            public readonly string input;
            public int position;
            public int lineNumber = 1;
            public Saxon.Hej.ma.json.JsonParser.JsonToken currentToken;
            public Saxon.Ejava.lang.StringBuilder currentTokenValue = new Saxon.Ejava.lang.StringBuilder(64);
            internal JsonTokenizer(string input)  {
                this.input = input;
                this.position = 0;
                if ((!(input.Length==0) && (input[0] == 65279))) {
                    position++;
                }
            }
            public Saxon.Hej.ma.json.JsonParser.JsonToken next() {
                currentToken = readToken();
                return currentToken;
            }
            private Saxon.Hej.ma.json.JsonParser.JsonToken readToken() {
                if ((position>=input.Length)) {
                    return Saxon.Hej.ma.json.JsonParser.JsonToken.EOF;
                }
                bool breakLoop = false;
                do {
                    char c = input[position];
                    switch (c) {
                      case '\n':
                      case '\r':
                        if (!(((((c == '\n') && (position>0)) && (input[position] == '\n'))))) {
                            lineNumber++;
                        }
                        goto case ' ';
                      case ' ':
                      case '\t':
                        if ((++position>=input.Length)) {
                            return Saxon.Hej.ma.json.JsonParser.JsonToken.EOF;
                        }
                        break ;
                      default:
                        breakLoop = true;
                        break ;
                    }
                } while (!(breakLoop));
                char ch = input[position++];
                switch (ch) {
                  case '[':
                    return Saxon.Hej.ma.json.JsonParser.JsonToken.LSQB;
                  case '{':
                    return Saxon.Hej.ma.json.JsonParser.JsonToken.LCURLY;
                  case ']':
                    return Saxon.Hej.ma.json.JsonParser.JsonToken.RSQB;
                  case '}':
                    return Saxon.Hej.ma.json.JsonParser.JsonToken.RCURLY;
                  case '"':
                    currentTokenValue.setLength(0);
                    bool afterBackslash = false;
                    while (true) {
                        if ((position>=input.Length)) {
                            invalidJSON("Unclosed quotes in string literal", ERR_GRAMMAR, lineNumber);
                        }
                        char c = input[position++];
                        if ((c<32)) {
                            invalidJSON((("Unescaped control character (x"+Saxon.Impl.Helpers.Utils.intToHexString(c))+")"), ERR_GRAMMAR, lineNumber);
                        }
                        if ((afterBackslash && (c == 'u'))) {
                            try {
                                string hex = input.Substring(position, (position+4) - position);
                                Saxon.Impl.Helpers.Utils.ParseInt(hex, 16);
                            } catch (System.Exception e) {
                                invalidJSON("\\u must be followed by four hex characters", ERR_GRAMMAR, lineNumber);
                            }
                        }
                        if (((c == '"') && !(afterBackslash))) {
                            break ;
                        } else {
                            currentTokenValue.append(c);
                            afterBackslash = ((c == '\\') && !(afterBackslash));
                        }
                    }
                    return Saxon.Hej.ma.json.JsonParser.JsonToken.STRING_LITERAL;
                  case ':':
                    return Saxon.Hej.ma.json.JsonParser.JsonToken.COLON;
                  case ',':
                    return Saxon.Hej.ma.json.JsonParser.JsonToken.COMMA;
                  case '-':
                  case '+':
                  case '.':
                  case '0':
                  case '1':
                  case '2':
                  case '3':
                  case '4':
                  case '5':
                  case '6':
                  case '7':
                  case '8':
                  case '9':
                    currentTokenValue.setLength(0);
                    currentTokenValue.append(ch);
                    if ((position<input.Length)) {
                        while (true) {
                            char c = input[position];
                            if (((((((((c>='0') && (c<='9'))) || (c == '-')) || (c == '+')) || (c == '.')) || (c == 'e')) || (c == 'E'))) {
                                currentTokenValue.append(c);
                                if ((++position>=input.Length)) {
                                    break ;
                                }
                            } else {
                                break ;
                            }
                        }
                    }
                    return Saxon.Hej.ma.json.JsonParser.JsonToken.NUMERIC_LITERAL;
                  default:
                    {
                        if (Saxon.Hej.om.NameChecker.isNCNameChar(ch)) {
                            currentTokenValue.setLength(0);
                            currentTokenValue.append(ch);
                            while ((position<input.Length)) {
                                char c = input[position];
                                if (Saxon.Hej.om.NameChecker.isNCNameChar(c)) {
                                    currentTokenValue.append(c);
                                    position++;
                                } else {
                                    break ;
                                }
                            }
                            string val = currentTokenValue.ToString();
                            switch (val) {
                              case "true":
                                return Saxon.Hej.ma.json.JsonParser.JsonToken.TRUE;
                              case "false":
                                return Saxon.Hej.ma.json.JsonParser.JsonToken.FALSE;
                              case "null":
                                return Saxon.Hej.ma.json.JsonParser.JsonToken.NULL;
                              default:
                                return Saxon.Hej.ma.json.JsonParser.JsonToken.UNQUOTED_STRING;
                            }
                        } else {
                            char c = input[--position];
                            string s = (Saxon.Hej.serialize.charcode.UTF16CharacterSet.isSurrogate(c) ? "" : ((" '"+c)+"'"));
                            invalidJSON(((((("Unexpected character"+s)+" (\\u")+Saxon.Impl.Helpers.Utils.intToHexString(c))+") at position ")+position), ERR_GRAMMAR, lineNumber);
                            return Saxon.Hej.ma.json.JsonParser.JsonToken.EOF;
                        }
                    }
                }
            }
        
        }
        public static string showToken(Saxon.Hej.ma.json.JsonParser.JsonToken token, string currentTokenValue) {
            switch (token) {
              case Saxon.Hej.ma.json.JsonParser.JsonToken.LSQB:
                return "[";
              case Saxon.Hej.ma.json.JsonParser.JsonToken.RSQB:
                return "]";
              case Saxon.Hej.ma.json.JsonParser.JsonToken.LCURLY:
                return "{";
              case Saxon.Hej.ma.json.JsonParser.JsonToken.RCURLY:
                return "}";
              case Saxon.Hej.ma.json.JsonParser.JsonToken.STRING_LITERAL:
                return (("string (\""+currentTokenValue)+"\")");
              case Saxon.Hej.ma.json.JsonParser.JsonToken.NUMERIC_LITERAL:
                return (("number ("+currentTokenValue)+")");
              case Saxon.Hej.ma.json.JsonParser.JsonToken.TRUE:
                return "true";
              case Saxon.Hej.ma.json.JsonParser.JsonToken.FALSE:
                return "false";
              case Saxon.Hej.ma.json.JsonParser.JsonToken.NULL:
                return "null";
              case Saxon.Hej.ma.json.JsonParser.JsonToken.COLON:
                return ":";
              case Saxon.Hej.ma.json.JsonParser.JsonToken.COMMA:
                return ",";
              case Saxon.Hej.ma.json.JsonParser.JsonToken.EOF:
                return "<eof>";
              default:
                return (("<"+token)+">");
            }
        }
        public void setNumberParser(System.Collections.Generic.IDictionary<string,Saxon.Hej.om.Sequence> options, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.om.Sequence val = Saxon.Impl.Helpers.MapUtils.Get(options, "number-parser");
            if ((val != null)) {
                Saxon.Hej.om.Item fn = val.head();
                if ((fn is Saxon.Hej.om.Function)) {
                    numberParser = (Saxon.Hej.om.Function)(fn);
                    if ((numberParser.getArity() != 1)) {
                        throw new Saxon.Hej.trans.XPathException("Number-parser function must have arity=1", "FOJS0005");
                    }
                    Saxon.Hej.type.SpecificFunctionType required = new Saxon.Hej.type.SpecificFunctionType(new Saxon.Hej.value.SequenceType[]{Saxon.Hej.value.SequenceType.SINGLE_STRING}, Saxon.Hej.value.SequenceType.SINGLE_ATOMIC);
                    if (!(required.matches(numberParser, context.getConfiguration().getTypeHierarchy()))) {
                        throw new Saxon.Hej.trans.XPathException("Number-parser function does not match the required type", "FOJS0005");
                    }
                } else {
                    throw new Saxon.Hej.trans.XPathException("Value of option 'number-parser' is not a function", "FOJS0005");
                }
            }
        }
    
    }
}