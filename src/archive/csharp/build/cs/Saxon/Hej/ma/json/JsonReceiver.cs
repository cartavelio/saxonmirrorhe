using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.ma.json {
    public class JsonReceiver : Saxon.Hej.@event.Receiver {
        private Saxon.Hej.expr.XPathContext context;
        private Saxon.Hej.@event.PipelineConfiguration pipe;
        private Saxon.Hej.str.UniStringConsumer output;
        private readonly Saxon.Ejava.lang.StringBuilder textBuffer = new Saxon.Ejava.lang.StringBuilder(128);
        private readonly System.Collections.Generic.Stack<Saxon.Hej.om.NodeName> stack = new System.Collections.Generic.Stack<Saxon.Hej.om.NodeName>();
        private bool atStart = true;
        private bool indenting = false;
        private bool escaped = false;
        private readonly System.Collections.Generic.Stack<System.Collections.Generic.ISet<string>> keyChecker = new System.Collections.Generic.Stack<System.Collections.Generic.ISet<string>>();
        private Saxon.Hej.om.Function numberFormatter = null;
        private const string ERR_INPUT = "FOJS0006";
        public JsonReceiver(Saxon.Hej.@event.PipelineConfiguration pipe, Saxon.Hej.expr.XPathContext context, Saxon.Hej.str.UniStringConsumer output)  {
            Saxon.Ejava.util.Objects.requireNonNull(pipe);
            Saxon.Ejava.util.Objects.requireNonNull(output);
            setPipelineConfiguration(pipe);
            this.output = output;
            this.context = context;
        }
        public void setPipelineConfiguration(Saxon.Hej.@event.PipelineConfiguration pipe) {
            this.pipe = pipe;
        }
        public Saxon.Hej.@event.PipelineConfiguration getPipelineConfiguration() {
            return pipe;
        }
        public void setSystemId(string systemId) {
        }
        public void setIndenting(bool indenting) {
            this.indenting = indenting;
        }
        public bool isIndenting() {
            return indenting;
        }
        public void setNumberFormatter(Saxon.Hej.om.Function formatter) {
            System.Diagnostics.Debug.Assert((formatter.getArity() == 1));
            this.numberFormatter = formatter;
        }
        public Saxon.Hej.om.Function getNumberFormatter() {
            return this.numberFormatter;
        }
        public void open() {
            output.open();
        }
        public void startDocument(int properties) {
        }
        public void endDocument() {
        }
        public void setUnparsedEntity(string name, string systemID, string publicID) {
        }
        public void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            string parent = ((stack.Count==0) ? null : stack.Peek().getLocalPart());
            bool inMap = ("map".Equals(parent) || (stack.Count==0));
            stack.Push(elemName);
            if (!(elemName.hasURI(Saxon.Hej.lib.NamespaceConstant.FN))) {
                throw new Saxon.Hej.trans.XPathException(("xml-to-json: element found in wrong namespace: "+elemName.getStructuredQName().getEQName()), ERR_INPUT);
            }
            string key = null;
            string escapedAtt = null;
            string escapedKey = null;
            foreach (Saxon.Hej.om.AttributeInfo att in attributes) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                if (attName.hasURI("")) {
                    switch (attName.getLocalPart()) {
                      case "key":
                        if (!(inMap)) {
                            throw new Saxon.Hej.trans.XPathException("xml-to-json: The key attribute is allowed only on elements within a map", ERR_INPUT);
                        }
                        key = att.getValue();
                        break ;
                      case "escaped-key":
                        if (!(inMap)) {
                            throw new Saxon.Hej.trans.XPathException("xml-to-json: The escaped-key attribute is allowed only on elements within a map", ERR_INPUT);
                        }
                        escapedKey = att.getValue();
                        break ;
                      case "escaped":
                        bool allowed = ((stack.Count == 1) || elemName.getLocalPart().Equals("string"));
                        if (!(allowed)) {
                            throw new Saxon.Hej.trans.XPathException("xml-to-json: The escaped attribute is allowed only on the <string> element", ERR_INPUT);
                        }
                        escapedAtt = att.getValue();
                        break ;
                      default:
                        throw new Saxon.Hej.trans.XPathException(("xml-to-json: Disallowed attribute in input: "+attName.getDisplayName()), ERR_INPUT);
                    }
                } else {
                    if (attName.hasURI(Saxon.Hej.lib.NamespaceConstant.FN)) {
                        throw new Saxon.Hej.trans.XPathException(("xml-to-json: Disallowed attribute in input: "+attName.getDisplayName()), ERR_INPUT);
                    }
                }
            }
            if (!(atStart)) {
                output.accept(Saxon.Hej.str.BMPString.of(","));
                if (indenting) {
                    indent(stack.Count);
                }
            }
            if ((inMap && !((keyChecker.Count==0)))) {
                if ((key == null)) {
                    throw new Saxon.Hej.trans.XPathException("xml-to-json: Child elements of <map> must have a key attribute", ERR_INPUT);
                }
                bool alreadyEscaped = false;
                if ((escapedKey != null)) {
                    try {
                        alreadyEscaped = Saxon.Hej.type.StringConverter.StringToBoolean.INSTANCE.convertString(Saxon.Hej.str.StringView.tidy(escapedKey)).asAtomic().effectiveBooleanValue();
                    } catch (Saxon.Hej.trans.XPathException e) {
                        throw new Saxon.Hej.trans.XPathException((("xml-to-json: Value of escaped-key attribute '"+Saxon.Hej.trans.Err.wrap(escapedKey))+"' is not a valid xs:boolean"), ERR_INPUT);
                    }
                }
                key = ((alreadyEscaped ? handleEscapedString(key) : escape(key, false, isControlChar)));
                string normalizedKey = (alreadyEscaped ? unescape(key) : key);
                bool added = keyChecker.Peek().Add(normalizedKey);
                if (!(added)) {
                    throw new Saxon.Hej.trans.XPathException(("xml-to-json: duplicate key value "+Saxon.Hej.trans.Err.wrap(key)), ERR_INPUT);
                }
                string @base = (indenting ? " : " : ":");
                output.accept(Saxon.Hej.str.BMPString.of("\"")).accept(Saxon.Hej.str.StringView.of(key)).accept(Saxon.Hej.str.BMPString.of("\"")).accept(Saxon.Hej.str.BMPString.of(@base));
            }
            string local = elemName.getLocalPart();
            switch (local) {
              case "array":
                if (indenting) {
                    indent(stack.Count);
                    output.accept(Saxon.Hej.str.BMPString.of("[ "));
                } else {
                    output.accept(Saxon.Hej.str.BMPString.of("["));
                }
                atStart = true;
                break ;
              case "map":
                if (indenting) {
                    indent(stack.Count);
                    output.accept(Saxon.Hej.str.BMPString.of("{ "));
                } else {
                    output.accept(Saxon.Hej.str.BMPString.of("{"));
                }
                atStart = true;
                keyChecker.Push(new System.Collections.Generic.HashSet<string>());
                break ;
              case "null":
                checkParent(local, parent);
                output.accept(Saxon.Hej.str.BMPString.of("null"));
                atStart = false;
                break ;
              case "string":
                if ((escapedAtt != null)) {
                    try {
                        escaped = Saxon.Hej.type.StringConverter.StringToBoolean.INSTANCE.convertString(Saxon.Hej.str.StringView.tidy(escapedAtt)).asAtomic().effectiveBooleanValue();
                    } catch (Saxon.Hej.trans.XPathException e) {
                        throw new Saxon.Hej.trans.XPathException((("xml-to-json: value of escaped attribute ("+escaped)+") is not a valid xs:boolean"), ERR_INPUT);
                    }
                }
                checkParent(local, parent);
                atStart = false;
                break ;
              case "boolean":
              case "number":
                checkParent(local, parent);
                atStart = false;
                break ;
              default:
                throw new Saxon.Hej.trans.XPathException((("xml-to-json: unknown element <"+local)+">"), ERR_INPUT);
            }
            textBuffer.setLength(0);
        }
        private void checkParent(string child, string parent) {
            if (((("null".Equals(parent) || "string".Equals(parent)) || "number".Equals(parent)) || "boolean".Equals(parent))) {
                throw new Saxon.Hej.trans.XPathException(((("xml-to-json: A "+Saxon.Hej.trans.Err.wrap(child, Saxon.Hej.trans.Err.ELEMENT))+" element cannot appear as a child of ")+Saxon.Hej.trans.Err.wrap(parent, Saxon.Hej.trans.Err.ELEMENT)), ERR_INPUT);
            }
        }
        public void endElement() {
            Saxon.Hej.om.NodeName name = stack.Pop();
            string local = name.getLocalPart();
            string content = textBuffer.ToString();
            Saxon.Hej.str.UnicodeString uContent = Saxon.Hej.str.StringView.tidy(content);
            if (local.Equals("boolean")) {
                try {
                    bool b = Saxon.Hej.type.StringConverter.StringToBoolean.INSTANCE.convertString(uContent).asAtomic().effectiveBooleanValue();
                    string @base = (b ? "true" : "false");
                    output.accept(Saxon.Hej.str.BMPString.of(@base));
                } catch (Saxon.Hej.trans.XPathException e) {
                    throw new Saxon.Hej.trans.XPathException("xml-to-json: Value of <boolean> element is not a valid xs:boolean", ERR_INPUT);
                }
            } else {
                if (local.Equals("number")) {
                    if ((numberFormatter == null)) {
                        try {
                            double d = Saxon.Hej.value.StringToDouble11.getInstance().stringToNumber(uContent);
                            if ((System.Double.IsNaN(d) || System.Double.IsInfinity(d))) {
                                throw new Saxon.Hej.trans.XPathException("xml-to-json: Infinity and NaN are not allowed", ERR_INPUT);
                            }
                            output.accept(new Saxon.Hej.value.DoubleValue(d).getUnicodeStringValue());
                        } catch (Saxon.Ejava.lang.NumberFormatException e) {
                            throw new Saxon.Hej.trans.XPathException(("xml-to-json: Invalid number: "+textBuffer), ERR_INPUT);
                        }
                    } else {
                        Saxon.Hej.om.Sequence result = Saxon.Hej.functions.SystemFunction.dynamicCall(numberFormatter, context, new Saxon.Hej.om.Sequence[]{new Saxon.Hej.value.StringValue(uContent)});
                        output.accept(((Saxon.Hej.value.StringValue)(result)).getUnicodeStringValue());
                    }
                } else {
                    if (local.Equals("string")) {
                        output.accept(Saxon.Hej.str.BMPString.of("\""));
                        string str = textBuffer.ToString();
                        if (escaped) {
                            output.accept(Saxon.Hej.str.StringView.of(handleEscapedString(content)));
                        } else {
                            output.accept(Saxon.Hej.str.StringView.of(escape(content, false, isControlChar)));
                        }
                        output.accept(Saxon.Hej.str.BMPString.of("\""));
                    } else {
                        if (!(Saxon.Hej.value.Whitespace.isAllWhite(uContent))) {
                            throw new Saxon.Hej.trans.XPathException((("xml-to-json: Element "+name.getDisplayName())+" must have no text content"), ERR_INPUT);
                        }
                    }
                }
            }
            textBuffer.setLength(0);
            escaped = false;
            if (local.Equals("array")) {
                string @base = (indenting ? " ]" : "]");
                output.accept(Saxon.Hej.str.BMPString.of(@base));
            } else {
                if (local.Equals("map")) {
                    keyChecker.Pop();
                    string @base = (indenting ? " }" : "}");
                    output.accept(Saxon.Hej.str.BMPString.of(@base));
                }
            }
            atStart = false;
        }
        private static string handleEscapedString(string str) {
            unescape(str);
            Saxon.Ejava.lang.StringBuilder @out = new Saxon.Ejava.lang.StringBuilder((str.Length*2));
            bool afterEscapeChar = false;
            for (int i = 0; (i<str.Length); i++) {
                char c = str[i];
                if (((c == '"') && !(afterEscapeChar))) {
                    @out.append("\\\"");
                } else {
                    if (((c<32) || (((c>=127) && (c<160))))) {
                        if ((c == '\b')) {
                            @out.append("\\b");
                        } else {
                            if ((c == '\f')) {
                                @out.append("\\f");
                            } else {
                                if ((c == '\n')) {
                                    @out.append("\\n");
                                } else {
                                    if ((c == '\r')) {
                                        @out.append("\\r");
                                    } else {
                                        if ((c == '\t')) {
                                            @out.append("\\t");
                                        } else {
                                            @out.append("\\u");
                                            @out.append(hex4(c));
                                        }
                                    }
                                }
                            }
                        }
                    } else {
                        if (((c == '/') && !(afterEscapeChar))) {
                            @out.append("\\/");
                        } else {
                            @out.appendCodePoint(c);
                        }
                    }
                }
                afterEscapeChar = ((c == '\\') && !(afterEscapeChar));
            }
            return @out.ToString();
        }
        public static string escape(string @in, bool forXml, Saxon.Ejava.util.function.IntPredicate hexEscapes) {
            Saxon.Ejava.lang.StringBuilder @out = new Saxon.Ejava.lang.StringBuilder(@in.Length);
            for (int i = 0; (i<@in.Length); i++) {
                int c = @in[i];
                switch (c) {
                  case '"':
                    @out.append((forXml ? "\"" : "\\\""));
                    break ;
                  case '\b':
                    @out.append("\\b");
                    break ;
                  case '\f':
                    @out.append("\\f");
                    break ;
                  case '\n':
                    @out.append("\\n");
                    break ;
                  case '\r':
                    @out.append("\\r");
                    break ;
                  case '\t':
                    @out.append("\\t");
                    break ;
                  case '/':
                    @out.append((forXml ? "/" : "\\/"));
                    break ;
                  case '\\':
                    @out.append("\\\\");
                    break ;
                  default:
                    if (hexEscapes(c)) {
                        @out.append("\\u");
                        @out.append(hex4(c));
                    } else {
                        @out.appendCodePoint(c);
                    }
                    break ;
                }
            }
            return @out.ToString();
        }
        private static Saxon.Ejava.lang.StringBuilder hex4(int c) {
            Saxon.Ejava.lang.StringBuilder hex = new Saxon.Ejava.lang.StringBuilder(Saxon.Impl.Helpers.Utils.intToHexString(c).ToUpper());
            while ((hex.length()<4)) {
                hex.insert(0, "0");
            }
            return hex;
        }
        private Saxon.Ejava.util.function.IntPredicate isControlChar = c => ((c<31) || (((c>=127) && (c<=159))));
        public void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            if ((!((stack.Count==0)) && !(Saxon.Hej.value.Whitespace.isAllWhite(chars)))) {
                Saxon.Hej.om.NodeName element = stack.Peek();
                string local = element.getLocalPart();
                if ((local.Equals("map") || local.Equals("array"))) {
                    throw new Saxon.Hej.trans.XPathException((("xml-to-json: Element "+local)+" must have no text content"), ERR_INPUT);
                }
            }
            textBuffer.append(chars);
        }
        public void processingInstruction(string name, Saxon.Hej.str.UnicodeString data, Saxon.Hej.s9api.Location locationId, int properties) {
        }
        public void comment(Saxon.Hej.str.UnicodeString content, Saxon.Hej.s9api.Location locationId, int properties) {
        }
        public void close() {
            if ((output != null)) {
                output.close();
                output = null;
            }
        }
        public bool usesTypeAnnotations() {
            return false;
        }
        public string getSystemId() {
            return null;
        }
        private void indent(int depth) {
            output.accept(Saxon.Hej.str.BMPString.of("\n"));
            for (int i = 0; (i<depth); i++) {
                output.accept(Saxon.Hej.str.StringConstants.SINGLE_SPACE);
            }
        }
        private static string unescape(string literal) {
            if ((literal.IndexOf('\\')<0)) {
                return literal;
            }
            Saxon.Ejava.lang.StringBuilder buffer = new Saxon.Ejava.lang.StringBuilder(literal.Length);
            for (int i = 0; (i<literal.Length); i++) {
                char c = literal[i];
                if ((c == '\\')) {
                    if ((i++ == (literal.Length-1))) {
                        throw new Saxon.Hej.trans.XPathException((("String '"+Saxon.Hej.trans.Err.wrap(literal))+"' ends in backslash "), "FOJS0007");
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
                            throw new Saxon.Hej.trans.XPathException((("Invalid hex escape sequence in string '"+Saxon.Hej.trans.Err.wrap(literal))+"'"), "FOJS0007");
                        }
                        break ;
                      default:
                        int next = literal[i];
                        string xx = ((next<256) ? (next+"") : ("x"+Saxon.Impl.Helpers.Utils.intToHexString(next)));
                        throw new Saxon.Hej.trans.XPathException(("Unknown escape sequence \\"+xx), "FOJS0007");
                    }
                } else {
                    buffer.append(c);
                }
            }
            return buffer.ToString();
        }
    
        /* Expanding default method append from interface Receiver */
        public virtual void append(Saxon.Hej.om.Item item, Saxon.Hej.s9api.Location locationId, int properties) {
            throw new Saxon.Ejava.lang.UnsupportedOperationException();
        }

        /* Expanding default method append from interface Receiver */
        public virtual void append(Saxon.Hej.om.Item item) {
            append(item, Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.ALL_NAMESPACES);
        }

        /* Expanding default method handlesAppend from interface Receiver */
        public virtual bool handlesAppend() {
            return false;
        }

    }
}