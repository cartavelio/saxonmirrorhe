using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.parser {
    public sealed class Tokenizer {
        public const char NUL = (char)(0);
        private int state = DEFAULT_STATE;
        public const int DEFAULT_STATE = 0;
        public const int BARE_NAME_STATE = 1;
        public const int SEQUENCE_TYPE_STATE = 2;
        public const int OPERATOR_STATE = 3;
        public int currentToken = Saxon.Hej.expr.parser.Token.EOF;
        public string currentTokenValue = null;
        public int currentTokenStartOffset = 0;
        private int nextToken = Saxon.Hej.expr.parser.Token.EOF;
        private string nextTokenValue = null;
        private int nextTokenStartOffset = 0;
        public string input;
        public int inputOffset = 0;
        private int inputLength;
        private int lineNumber = 1;
        private int nextLineNumber = 1;
        private System.Collections.Generic.IList<int> newlineOffsets = null;
        private int precedingToken = Saxon.Hej.expr.parser.Token.UNKNOWN;
        private string precedingTokenValue = "";
        public bool disallowUnionKeyword;
        public bool isXQuery = false;
        public int languageLevel = 20;
        public bool allowSaxonExtensions = false;
        public Tokenizer()  {
        }
        public int getState() {
            return state;
        }
        public void setState(int state) {
            this.state = state;
            if ((state == DEFAULT_STATE)) {
                precedingToken = Saxon.Hej.expr.parser.Token.UNKNOWN;
                precedingTokenValue = "";
                currentToken = Saxon.Hej.expr.parser.Token.UNKNOWN;
            } else {
                if ((state == OPERATOR_STATE)) {
                    precedingToken = Saxon.Hej.expr.parser.Token.RPAR;
                    precedingTokenValue = ")";
                    currentToken = Saxon.Hej.expr.parser.Token.RPAR;
                }
            }
        }
        public void tokenize(string input, int start, int end) {
            nextToken = Saxon.Hej.expr.parser.Token.EOF;
            nextTokenValue = null;
            nextTokenStartOffset = 0;
            inputOffset = start;
            this.input = input;
            this.lineNumber = 0;
            nextLineNumber = 0;
            if ((end == -1)) {
                inputLength = input.Length;
            } else {
                inputLength = end;
            }
            lookAhead();
            next();
        }
        public void next() {
            precedingToken = currentToken;
            precedingTokenValue = currentTokenValue;
            currentToken = nextToken;
            currentTokenValue = nextTokenValue;
            if ((currentTokenValue == null)) {
                currentTokenValue = "";
            }
            currentTokenStartOffset = nextTokenStartOffset;
            lineNumber = nextLineNumber;
            switch (currentToken) {
              case Saxon.Hej.expr.parser.Token.NAME:
                int optype = getBinaryOp(currentTokenValue);
                if (((optype != Saxon.Hej.expr.parser.Token.UNKNOWN) && !(followsOperator(precedingToken)))) {
                    currentToken = optype;
                }
                break ;
              case Saxon.Hej.expr.parser.Token.LT:
                if ((isXQuery && followsOperator(precedingToken))) {
                    currentToken = Saxon.Hej.expr.parser.Token.TAG;
                }
                break ;
              case Saxon.Hej.expr.parser.Token.STAR:
                if (!(followsOperator(precedingToken))) {
                    currentToken = Saxon.Hej.expr.parser.Token.MULT;
                }
                break ;
            }
            if (((currentToken == Saxon.Hej.expr.parser.Token.TAG) || (currentToken == Saxon.Hej.expr.parser.Token.RCURLY))) {
                return ;
            }
            int oldPrecedingToken = precedingToken;
            lookAhead();
            if ((currentToken == Saxon.Hej.expr.parser.Token.NAME)) {
                if ((state == BARE_NAME_STATE)) {
                    return ;
                }
                if ((oldPrecedingToken == Saxon.Hej.expr.parser.Token.DOLLAR)) {
                    return ;
                }
                handleNextToken(oldPrecedingToken);
            }
        }
        private void handleNextToken(int oldPrecedingToken) {
            switch (nextToken) {
              case Saxon.Hej.expr.parser.Token.LPAR:
                handleLPAR(oldPrecedingToken);
                break ;
              case Saxon.Hej.expr.parser.Token.LCURLY:
                handleLCURLY();
                break ;
              case Saxon.Hej.expr.parser.Token.COLONCOLON:
                handleCOLONCOLON();
                break ;
              case Saxon.Hej.expr.parser.Token.HASH:
                handleHASH();
                break ;
              case Saxon.Hej.expr.parser.Token.COLONSTAR:
                handleCOLONSTAR();
                break ;
              case Saxon.Hej.expr.parser.Token.DOLLAR:
                handleDOLLAR();
                break ;
              case Saxon.Hej.expr.parser.Token.PERCENT:
                handlePERCENT();
                break ;
              case Saxon.Hej.expr.parser.Token.NAME:
                int candidate = getCandidate();
                if ((candidate != -1)) {
                    string qname = nextTokenValue;
                    string saveTokenValue = currentTokenValue;
                    int savePosition = inputOffset;
                    lookAhead();
                    if ((nextToken == Saxon.Hej.expr.parser.Token.LCURLY)) {
                        currentToken = candidate;
                        currentTokenValue = qname;
                        lookAhead();
                        return ;
                    } else {
                        currentToken = Saxon.Hej.expr.parser.Token.NAME;
                        currentTokenValue = saveTokenValue;
                        inputOffset = savePosition;
                        nextToken = Saxon.Hej.expr.parser.Token.NAME;
                        nextTokenValue = qname;
                    }
                }
                string composite = ((currentTokenValue+' ')+nextTokenValue);
                int possibleToken = Saxon.Impl.Helpers.MapUtils.GetOrDefault(Saxon.Hej.expr.parser.Token.doubleKeywords, composite, Saxon.Hej.expr.parser.Token.UNKNOWN);
                if ((possibleToken == Saxon.Hej.expr.parser.Token.UNKNOWN)) {
                    break ;
                } else {
                    handleNotUnknown(composite, possibleToken);
                    return ;
                }
              default:
                break;
            }
        }
        private void handleLPAR(int oldPrecedingToken) {
            int op = getBinaryOp(currentTokenValue);
            if (((op == Saxon.Hej.expr.parser.Token.UNKNOWN) || followsOperator(oldPrecedingToken))) {
                currentToken = getFunctionType(currentTokenValue);
                lookAhead();
            } else {
                currentToken = op;
            }
        }
        private void handleLCURLY() {
            if (!(((state == SEQUENCE_TYPE_STATE)))) {
                currentToken = Saxon.Hej.expr.parser.Token.KEYWORD_CURLY;
                lookAhead();
            }
        }
        private void handleCOLONCOLON() {
            lookAhead();
            currentToken = Saxon.Hej.expr.parser.Token.AXIS;
        }
        private void handleHASH() {
            lookAhead();
            currentToken = Saxon.Hej.expr.parser.Token.NAMED_FUNCTION_REF;
        }
        private void handleCOLONSTAR() {
            lookAhead();
            currentToken = Saxon.Hej.expr.parser.Token.PREFIX;
        }
        private void handleDOLLAR() {
            switch (currentTokenValue) {
              case "for":
                currentToken = Saxon.Hej.expr.parser.Token.FOR;
                break ;
              case "some":
                currentToken = Saxon.Hej.expr.parser.Token.SOME;
                break ;
              case "every":
                currentToken = Saxon.Hej.expr.parser.Token.EVERY;
                break ;
              case "let":
                currentToken = Saxon.Hej.expr.parser.Token.LET;
                break ;
              case "count":
                currentToken = Saxon.Hej.expr.parser.Token.COUNT;
                break ;
              case "copy":
                currentToken = Saxon.Hej.expr.parser.Token.COPY;
                break ;
            }
        }
        private void handlePERCENT() {
            if (currentTokenValue.Equals("declare")) {
                currentToken = Saxon.Hej.expr.parser.Token.DECLARE_ANNOTATED;
            }
        }
        private int getCandidate() {
            int candidate = -1;
            switch (currentTokenValue) {
              case "element":
                candidate = Saxon.Hej.expr.parser.Token.ELEMENT_QNAME;
                break ;
              case "attribute":
                candidate = Saxon.Hej.expr.parser.Token.ATTRIBUTE_QNAME;
                break ;
              case "processing-instruction":
                candidate = Saxon.Hej.expr.parser.Token.PI_QNAME;
                break ;
              case "namespace":
                candidate = Saxon.Hej.expr.parser.Token.NAMESPACE_QNAME;
                break ;
            }
            return candidate;
        }
        private void handleNotUnknown(string composite, int possibleToken) {
            currentToken = possibleToken;
            currentTokenValue = composite;
            if ((currentToken == Saxon.Hej.expr.parser.Token.REPLACE_VALUE)) {
                lookAhead();
                if (((nextToken != Saxon.Hej.expr.parser.Token.NAME) || !(nextTokenValue.Equals("of")))) {
                    throw new Saxon.Hej.trans.XPathException((("After '"+composite)+"', expected 'of'"));
                }
                lookAhead();
                if (((nextToken != Saxon.Hej.expr.parser.Token.NAME) || !(nextTokenValue.Equals("node")))) {
                    throw new Saxon.Hej.trans.XPathException("After 'replace value of', expected 'node'");
                }
                nextToken = currentToken;
            }
            lookAhead();
        }
        internal int peekAhead() {
            return nextToken;
        }
        public void treatCurrentAsOperator() {
            switch (currentToken) {
              case Saxon.Hej.expr.parser.Token.NAME:
                int optype = getBinaryOp(currentTokenValue);
                if ((optype != Saxon.Hej.expr.parser.Token.UNKNOWN)) {
                    currentToken = optype;
                }
                break ;
              case Saxon.Hej.expr.parser.Token.STAR:
                currentToken = Saxon.Hej.expr.parser.Token.MULT;
                break ;
            }
        }
        public void lookAhead() {
            precedingToken = nextToken;
            precedingTokenValue = nextTokenValue;
            nextTokenValue = null;
            nextTokenStartOffset = inputOffset;
            for (; ; ) {
                if ((inputOffset>=inputLength)) {
                    nextToken = Saxon.Hej.expr.parser.Token.EOF;
                    return ;
                }
                char c = input[inputOffset++];
                switch (c) {
                  case '/':
                    if (((inputOffset<inputLength) && (input[inputOffset] == '/'))) {
                        inputOffset++;
                        nextToken = Saxon.Hej.expr.parser.Token.SLASH_SLASH;
                        return ;
                    }
                    nextToken = Saxon.Hej.expr.parser.Token.SLASH;
                    return ;
                  case ':':
                    if ((inputOffset<inputLength)) {
                        if ((input[inputOffset] == ':')) {
                            inputOffset++;
                            nextToken = Saxon.Hej.expr.parser.Token.COLONCOLON;
                            return ;
                        } else {
                            if ((input[inputOffset] == '=')) {
                                nextToken = Saxon.Hej.expr.parser.Token.ASSIGN;
                                inputOffset++;
                                return ;
                            } else {
                                nextToken = Saxon.Hej.expr.parser.Token.COLON;
                                return ;
                            }
                        }
                    }
                    throw new Saxon.Hej.trans.XPathException("Unexpected colon at start of token");
                  case '@':
                    nextToken = Saxon.Hej.expr.parser.Token.AT;
                    return ;
                  case '?':
                    if ((inputOffset<inputLength)) {
                        if ((input[inputOffset] == '?')) {
                            inputOffset++;
                            nextToken = Saxon.Hej.expr.parser.Token.QMARK_QMARK;
                            return ;
                        }
                    }
                    nextToken = Saxon.Hej.expr.parser.Token.QMARK;
                    return ;
                  case '[':
                    nextToken = Saxon.Hej.expr.parser.Token.LSQB;
                    return ;
                  case ']':
                    nextToken = Saxon.Hej.expr.parser.Token.RSQB;
                    return ;
                  case '{':
                    nextToken = Saxon.Hej.expr.parser.Token.LCURLY;
                    return ;
                  case '}':
                    nextToken = Saxon.Hej.expr.parser.Token.RCURLY;
                    return ;
                  case ';':
                    nextToken = Saxon.Hej.expr.parser.Token.SEMICOLON;
                    state = DEFAULT_STATE;
                    return ;
                  case '%':
                    nextToken = Saxon.Hej.expr.parser.Token.PERCENT;
                    return ;
                  case '(':
                    if (((inputOffset<inputLength) && (input[inputOffset] == '#'))) {
                        inputOffset++;
                        int pragmaStart = inputOffset;
                        int nestingDepth = 1;
                        while (((nestingDepth>0) && (inputOffset<((inputLength-1))))) {
                            if ((input[inputOffset] == '\n')) {
                                incrementLineNumber();
                            } else {
                                if (((input[inputOffset] == '#') && (input[(inputOffset+1)] == ')'))) {
                                    nestingDepth--;
                                    inputOffset++;
                                } else {
                                    if (((input[inputOffset] == '(') && (input[(inputOffset+1)] == '#'))) {
                                        nestingDepth++;
                                        inputOffset++;
                                    }
                                }
                            }
                            inputOffset++;
                        }
                        if ((nestingDepth>0)) {
                            throw new Saxon.Hej.trans.XPathException("Unclosed XQuery pragma");
                        }
                        nextToken = Saxon.Hej.expr.parser.Token.PRAGMA;
                        nextTokenValue = input.Substring(pragmaStart, (inputOffset-2) - pragmaStart);
                        return ;
                    }
                    if (((inputOffset<inputLength) && (input[inputOffset] == ':'))) {
                        inputOffset++;
                        int nestingDepth = 1;
                        while (((nestingDepth>0) && (inputOffset<((inputLength-1))))) {
                            if ((input[inputOffset] == '\n')) {
                                incrementLineNumber();
                            } else {
                                if (((input[inputOffset] == ':') && (input[(inputOffset+1)] == ')'))) {
                                    nestingDepth--;
                                    inputOffset++;
                                } else {
                                    if (((input[inputOffset] == '(') && (input[(inputOffset+1)] == ':'))) {
                                        nestingDepth++;
                                        inputOffset++;
                                    }
                                }
                            }
                            inputOffset++;
                        }
                        if ((nestingDepth>0)) {
                            throw new Saxon.Hej.trans.XPathException("Unclosed XPath comment");
                        }
                        lookAhead();
                    } else {
                        nextToken = Saxon.Hej.expr.parser.Token.LPAR;
                    }
                    return ;
                  case ')':
                    nextToken = Saxon.Hej.expr.parser.Token.RPAR;
                    return ;
                  case '+':
                    nextToken = Saxon.Hej.expr.parser.Token.PLUS;
                    return ;
                  case '-':
                    if (((inputOffset<inputLength) && (input[inputOffset] == '>'))) {
                        inputOffset++;
                        nextToken = Saxon.Hej.expr.parser.Token.THIN_ARROW;
                        return ;
                    }
                    nextToken = Saxon.Hej.expr.parser.Token.MINUS;
                    return ;
                  case '=':
                    if (((inputOffset<inputLength) && (input[inputOffset] == '>'))) {
                        inputOffset++;
                        nextToken = Saxon.Hej.expr.parser.Token.FAT_ARROW;
                        return ;
                    }
                    nextToken = Saxon.Hej.expr.parser.Token.EQUALS;
                    return ;
                  case '!':
                    if ((inputOffset<inputLength)) {
                        if ((input[inputOffset] == '=')) {
                            inputOffset++;
                            nextToken = Saxon.Hej.expr.parser.Token.NE;
                            return ;
                        } else {
                            if ((input[inputOffset] == '!')) {
                                inputOffset++;
                                nextToken = Saxon.Hej.expr.parser.Token.BANG_BANG;
                                return ;
                            }
                        }
                    }
                    nextToken = Saxon.Hej.expr.parser.Token.BANG;
                    return ;
                  case '*':
                    if (((((inputOffset<inputLength) && (input[inputOffset] == ':')) && ((inputOffset+1)<inputLength)) && (((input[(inputOffset+1)]>127) || Saxon.Hej.om.NameChecker.isNCNameStartChar(input[(inputOffset+1)]))))) {
                        inputOffset++;
                        nextToken = Saxon.Hej.expr.parser.Token.SUFFIX;
                        return ;
                    }
                    nextToken = Saxon.Hej.expr.parser.Token.STAR;
                    return ;
                  case ',':
                    nextToken = Saxon.Hej.expr.parser.Token.COMMA;
                    return ;
                  case '$':
                    nextToken = Saxon.Hej.expr.parser.Token.DOLLAR;
                    return ;
                  case '|':
                    if (((inputOffset<inputLength) && (input[inputOffset] == '|'))) {
                        inputOffset++;
                        nextToken = Saxon.Hej.expr.parser.Token.CONCAT;
                        return ;
                    }
                    nextToken = Saxon.Hej.expr.parser.Token.UNION;
                    return ;
                  case '#':
                    nextToken = Saxon.Hej.expr.parser.Token.HASH;
                    return ;
                  case '<':
                    if (((inputOffset<inputLength) && (input[inputOffset] == '='))) {
                        inputOffset++;
                        nextToken = Saxon.Hej.expr.parser.Token.LE;
                        return ;
                    }
                    if (((inputOffset<inputLength) && (input[inputOffset] == '<'))) {
                        inputOffset++;
                        nextToken = Saxon.Hej.expr.parser.Token.PRECEDES;
                        return ;
                    }
                    nextToken = Saxon.Hej.expr.parser.Token.LT;
                    return ;
                  case '>':
                    if (((inputOffset<inputLength) && (input[inputOffset] == '='))) {
                        inputOffset++;
                        nextToken = Saxon.Hej.expr.parser.Token.GE;
                        return ;
                    }
                    if (((inputOffset<inputLength) && (input[inputOffset] == '>'))) {
                        inputOffset++;
                        nextToken = Saxon.Hej.expr.parser.Token.FOLLOWS;
                        return ;
                    }
                    nextToken = Saxon.Hej.expr.parser.Token.GT;
                    return ;
                  case '.':
                    if (((inputOffset<inputLength) && (input[inputOffset] == '.'))) {
                        inputOffset++;
                        nextToken = Saxon.Hej.expr.parser.Token.DOTDOT;
                        return ;
                    }
                    if (((inputOffset<inputLength) && (input[inputOffset] == '{'))) {
                        inputOffset++;
                        nextTokenValue = ".";
                        nextToken = Saxon.Hej.expr.parser.Token.KEYWORD_CURLY;
                        return ;
                    }
                    if ((((inputOffset == inputLength) || (input[inputOffset]<'0')) || (input[inputOffset]>'9'))) {
                        nextToken = Saxon.Hej.expr.parser.Token.DOT;
                        return ;
                    }
                    goto case '0';
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
                    bool allowE = true;
                    bool allowSign = false;
                    bool allowDot = true;
                    bool keepGoing = true;
                    while (true) {
                        switch (c) {
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
                            allowSign = false;
                            break ;
                          case '.':
                            if (allowDot) {
                                allowDot = false;
                                allowSign = false;
                            } else {
                                inputOffset--;
                                keepGoing = false;
                            }
                            break ;
                          case 'E':
                          case 'e':
                            if (allowE) {
                                allowSign = true;
                                allowE = false;
                            } else {
                                inputOffset--;
                                keepGoing = false;
                            }
                            break ;
                          case '+':
                          case '-':
                            if (allowSign) {
                                allowSign = false;
                            } else {
                                inputOffset--;
                                keepGoing = false;
                            }
                            break ;
                          default:
                            if ((((('a'<=c) && (c<='z'))) || (c>127))) {
                                throw new Saxon.Hej.trans.XPathException("Separator needed after numeric literal");
                            }
                            inputOffset--;
                            keepGoing = false;
                            break ;
                        }
                        if ((!(keepGoing) || (inputOffset>=inputLength))) {
                            break ;
                        }
                        c = input[inputOffset++];
                    }
                    nextTokenValue = input.Substring(nextTokenStartOffset, inputOffset - nextTokenStartOffset);
                    nextToken = Saxon.Hej.expr.parser.Token.NUMBER;
                    return ;
                  case '"':
                  case '\'':
                    nextTokenValue = "";
                    while (true) {
                        inputOffset = input.IndexOf(c, inputOffset);
                        if ((inputOffset<0)) {
                            inputOffset = (nextTokenStartOffset+1);
                            throw new Saxon.Hej.trans.XPathException("Unmatched quote in expression");
                        }
                        nextTokenValue += input.Substring((nextTokenStartOffset+1), inputOffset++ - (nextTokenStartOffset+1));
                        if ((inputOffset<inputLength)) {
                            char n = input[inputOffset];
                            if ((n == c)) {
                                nextTokenValue += c;
                                nextTokenStartOffset = inputOffset;
                                inputOffset++;
                            } else {
                                break ;
                            }
                        } else {
                            break ;
                        }
                    }
                    if ((nextTokenValue.IndexOf('\n')>=0)) {
                        for (int i = 0; (i<nextTokenValue.Length); i++) {
                            if ((nextTokenValue[i] == '\n')) {
                                incrementLineNumber(((nextTokenStartOffset+i)+1));
                            }
                        }
                    }
                    nextToken = Saxon.Hej.expr.parser.Token.STRING_LITERAL;
                    return ;
                  case '`':
                    if ((((isXQuery && (inputOffset<(inputLength-1))) && (input[inputOffset] == '`')) && (input[(inputOffset+1)] == '['))) {
                        inputOffset += 2;
                        int j = inputOffset;
                        int newlines = 0;
                        while (true) {
                            if ((j>=inputLength)) {
                                throw new Saxon.Hej.trans.XPathException("Unclosed string template in expression");
                            }
                            if ((input[j] == '\n')) {
                                newlines++;
                            } else {
                                if ((((input[j] == '`') && ((j+1)<inputLength)) && (input[(j+1)] == '{'))) {
                                    nextToken = Saxon.Hej.expr.parser.Token.STRING_CONSTRUCTOR_INITIAL;
                                    nextTokenValue = input.Substring(inputOffset, j - inputOffset);
                                    inputOffset = (j+2);
                                    incrementLineNumber(newlines);
                                    return ;
                                } else {
                                    if (((((input[j] == ']') && ((j+2)<inputLength)) && (input[(j+1)] == '`')) && (input[(j+2)] == '`'))) {
                                        nextToken = Saxon.Hej.expr.parser.Token.STRING_LITERAL_BACKTICKED;
                                        nextTokenValue = input.Substring(inputOffset, j - inputOffset);
                                        inputOffset = (j+3);
                                        incrementLineNumber(newlines);
                                        return ;
                                    }
                                }
                            }
                            j++;
                        }
                    } else {
                        throw new Saxon.Hej.trans.XPathException("Invalid character '`' (backtick) in expression");
                    }
                  case '\n':
                    incrementLineNumber();
                    goto case ' ';
                  case ' ':
                  case '\t':
                  case '\r':
                    nextTokenStartOffset = inputOffset;
                    break ;
                  case '\u00B6':
                  case 'Q':
                    if (((inputOffset<inputLength) && (input[inputOffset] == '{'))) {
                        int close = input.IndexOf('}', inputOffset++);
                        if ((close<inputOffset)) {
                            throw new Saxon.Hej.trans.XPathException("Missing closing brace in EQName");
                        }
                        string uri = input.Substring(inputOffset, close - inputOffset);
                        uri = Saxon.Hej.value.Whitespace.collapseWhitespace(uri);
                        if (uri.Contains("{")) {
                            throw new Saxon.Hej.trans.XPathException("EQName must not contain opening brace");
                        }
                        inputOffset = (close+1);
                        int start = inputOffset;
                        bool isStar = false;
                        while ((inputOffset<inputLength)) {
                            char c2 = input[inputOffset];
                            if ((((((c2>unchecked((int)0x80)) || System.Char.IsLetterOrDigit(c2)) || (c2 == '_')) || (c2 == '.')) || (c2 == '-'))) {
                                inputOffset++;
                            } else {
                                if (((c2 == '*') && ((start == inputOffset)))) {
                                    inputOffset++;
                                    isStar = true;
                                    break ;
                                } else {
                                    break ;
                                }
                            }
                        }
                        string localName = input.Substring(start, inputOffset - start);
                        nextTokenValue = ((("Q{"+uri)+"}")+localName);
                        nextToken = (isStar ? Saxon.Hej.expr.parser.Token.PREFIX : Saxon.Hej.expr.parser.Token.NAME);
                        return ;
                    }
                    goto default;
                  default:
                    if (((c<unchecked((int)0x80)) && !(System.Char.IsLetter(c)))) {
                        throw new Saxon.Hej.trans.XPathException((((("Invalid character '"+c)+"' (x")+Saxon.Impl.Helpers.Utils.intToHexString((int)(c)))+") in expression"));
                    }
                    goto case '_';
                  case '_':
                    bool foundColon = false;
                    bool breakLoop = false;
                    for (; (inputOffset<inputLength); inputOffset++) {
                        c = input[inputOffset];
                        switch (c) {
                          case ':':
                            if (!(foundColon)) {
                                if (((precedingToken == Saxon.Hej.expr.parser.Token.QMARK) || (precedingToken == Saxon.Hej.expr.parser.Token.SUFFIX))) {
                                    nextTokenValue = input.Substring(nextTokenStartOffset, inputOffset - nextTokenStartOffset);
                                    nextToken = Saxon.Hej.expr.parser.Token.NAME;
                                    return ;
                                }
                                if (((inputOffset+1)<inputLength)) {
                                    char nc = input[(inputOffset+1)];
                                    if ((nc == ':')) {
                                        nextTokenValue = input.Substring(nextTokenStartOffset, inputOffset - nextTokenStartOffset);
                                        nextToken = Saxon.Hej.expr.parser.Token.AXIS;
                                        inputOffset += 2;
                                        return ;
                                    } else {
                                        if ((nc == '*')) {
                                            nextTokenValue = input.Substring(nextTokenStartOffset, inputOffset - nextTokenStartOffset);
                                            nextToken = Saxon.Hej.expr.parser.Token.PREFIX;
                                            inputOffset += 2;
                                            return ;
                                        } else {
                                            if (!(((((nc == '_') || (nc>127)) || System.Char.IsLetter(nc))))) {
                                                nextTokenValue = input.Substring(nextTokenStartOffset, inputOffset - nextTokenStartOffset);
                                                nextToken = Saxon.Hej.expr.parser.Token.NAME;
                                                return ;
                                            }
                                        }
                                    }
                                }
                                foundColon = true;
                            } else {
                                breakLoop = true;
                            }
                            break ;
                          case '.':
                          case '-':
                            if (((((precedingToken>Saxon.Hej.expr.parser.Token.LAST_OPERATOR) && !((((precedingToken == Saxon.Hej.expr.parser.Token.QMARK) || (precedingToken == Saxon.Hej.expr.parser.Token.SUFFIX))))) && (getBinaryOp(input.Substring(nextTokenStartOffset, inputOffset - nextTokenStartOffset)) != Saxon.Hej.expr.parser.Token.UNKNOWN)) && !((((precedingToken == Saxon.Hej.expr.parser.Token.NAME) && (getBinaryOp(precedingTokenValue) != Saxon.Hej.expr.parser.Token.UNKNOWN)))))) {
                                nextToken = getBinaryOp(input.Substring(nextTokenStartOffset, inputOffset - nextTokenStartOffset));
                                return ;
                            }
                            goto case '_';
                          case '_':
                            break ;
                          default:
                            if (((c<unchecked((int)0x80)) && !(System.Char.IsLetterOrDigit(c)))) {
                                breakLoop = true;
                            }
                            break ;
                        }
                        if (breakLoop) {
                            break ;
                        }
                    }
                    nextTokenValue = input.Substring(nextTokenStartOffset, inputOffset - nextTokenStartOffset);
                    nextToken = Saxon.Hej.expr.parser.Token.NAME;
                    return ;
                }
            }
        }
        internal int getBinaryOp(string s) {
            switch (s) {
              case "after":
                return Saxon.Hej.expr.parser.Token.AFTER;
              case "and":
                return Saxon.Hej.expr.parser.Token.AND;
              case "as":
                return Saxon.Hej.expr.parser.Token.AS;
              case "before":
                return Saxon.Hej.expr.parser.Token.BEFORE;
              case "by":
                return Saxon.Hej.expr.parser.Token.BY;
              case "case":
                return Saxon.Hej.expr.parser.Token.CASE;
              case "default":
                return Saxon.Hej.expr.parser.Token.DEFAULT;
              case "div":
                return Saxon.Hej.expr.parser.Token.DIV;
              case "else":
                return Saxon.Hej.expr.parser.Token.ELSE;
              case "eq":
                return Saxon.Hej.expr.parser.Token.FEQ;
              case "except":
                return Saxon.Hej.expr.parser.Token.EXCEPT;
              case "ge":
                return Saxon.Hej.expr.parser.Token.FGE;
              case "gt":
                return Saxon.Hej.expr.parser.Token.FGT;
              case "idiv":
                return Saxon.Hej.expr.parser.Token.IDIV;
              case "in":
                return Saxon.Hej.expr.parser.Token.IN;
              case "intersect":
                return Saxon.Hej.expr.parser.Token.INTERSECT;
              case "into":
                return Saxon.Hej.expr.parser.Token.INTO;
              case "is":
                return Saxon.Hej.expr.parser.Token.IS;
              case "le":
                return Saxon.Hej.expr.parser.Token.FLE;
              case "lt":
                return Saxon.Hej.expr.parser.Token.FLT;
              case "mod":
                return Saxon.Hej.expr.parser.Token.MOD;
              case "modify":
                return Saxon.Hej.expr.parser.Token.MODIFY;
              case "ne":
                return Saxon.Hej.expr.parser.Token.FNE;
              case "or":
                return Saxon.Hej.expr.parser.Token.OR;
              case "otherwise":
                return Saxon.Hej.expr.parser.Token.OTHERWISE;
              case "return":
                return Saxon.Hej.expr.parser.Token.RETURN;
              case "satisfies":
                return Saxon.Hej.expr.parser.Token.SATISFIES;
              case "then":
                return Saxon.Hej.expr.parser.Token.THEN;
              case "to":
                return Saxon.Hej.expr.parser.Token.TO;
              case "union":
                return Saxon.Hej.expr.parser.Token.UNION;
              case "where":
                return Saxon.Hej.expr.parser.Token.WHERE;
              case "with":
                return Saxon.Hej.expr.parser.Token.WITH;
              case "orElse":
                return (allowSaxonExtensions ? Saxon.Hej.expr.parser.Token.OR_ELSE : Saxon.Hej.expr.parser.Token.UNKNOWN);
              case "andAlso":
                return (allowSaxonExtensions ? Saxon.Hej.expr.parser.Token.AND_ALSO : Saxon.Hej.expr.parser.Token.UNKNOWN);
              default:
                return Saxon.Hej.expr.parser.Token.UNKNOWN;
            }
        }
        private int getFunctionType(string s) {
            switch (s) {
              case "if":
                return Saxon.Hej.expr.parser.Token.IF;
              case "map":
              case "namespace-node":
              case "array":
              case "function":
                return ((languageLevel == 20) ? Saxon.Hej.expr.parser.Token.FUNCTION : Saxon.Hej.expr.parser.Token.NODEKIND);
              case "node":
              case "schema-attribute":
              case "schema-element":
              case "processing-instruction":
              case "empty-sequence":
              case "document-node":
              case "comment":
              case "element":
              case "item":
              case "text":
              case "attribute":
                return Saxon.Hej.expr.parser.Token.NODEKIND;
              case "atomic":
              case "tuple":
              case "record":
              case "type":
              case "union":
              case "enum":
                return (allowSaxonExtensions ? Saxon.Hej.expr.parser.Token.NODEKIND : Saxon.Hej.expr.parser.Token.FUNCTION);
              case "switch":
                return ((languageLevel == 20) ? Saxon.Hej.expr.parser.Token.FUNCTION : Saxon.Hej.expr.parser.Token.SWITCH);
              case "otherwise":
                return Saxon.Hej.expr.parser.Token.OTHERWISE;
              case "typeswitch":
                return Saxon.Hej.expr.parser.Token.TYPESWITCH;
              default:
                return Saxon.Hej.expr.parser.Token.FUNCTION;
            }
        }
        private bool followsOperator(int precedingToken) {
            return (precedingToken<=Saxon.Hej.expr.parser.Token.LAST_OPERATOR);
        }
        public char nextChar() {
            if ((inputOffset<inputLength)) {
                char c = input[inputOffset++];
                if ((c == '\n')) {
                    incrementLineNumber();
                    lineNumber++;
                }
                return c;
            } else {
                return NUL;
            }
        }
        private void incrementLineNumber() {
            nextLineNumber++;
            if ((newlineOffsets == null)) {
                newlineOffsets = new System.Collections.Generic.List<int>(20);
            }
            newlineOffsets.Add((inputOffset-1));
        }
        public void incrementLineNumber(int offset) {
            nextLineNumber++;
            if ((newlineOffsets == null)) {
                newlineOffsets = new System.Collections.Generic.List<int>(20);
            }
            newlineOffsets.Add(offset);
        }
        public void unreadChar() {
            if ((input[--inputOffset] == '\n')) {
                nextLineNumber--;
                lineNumber--;
                if ((newlineOffsets != null)) {
                    Saxon.Impl.Helpers.ListUtils.Remove(newlineOffsets, (newlineOffsets.Count-1));
                }
            }
        }
        internal string recentText(int offset) {
            if ((offset == -1)) {
                if ((inputOffset>inputLength)) {
                    inputOffset = inputLength;
                }
                if ((inputOffset<34)) {
                    return input.Substring(0, inputOffset - 0);
                } else {
                    return Saxon.Hej.value.Whitespace.collapseWhitespace(("..."+input.Substring((inputOffset-30), inputOffset - (inputOffset-30))));
                }
            } else {
                int end = (offset+30);
                if ((end>inputLength)) {
                    end = inputLength;
                }
                return Saxon.Hej.value.Whitespace.collapseWhitespace(((((offset>0) ? "..." : ""))+input.Substring(offset, end - offset)));
            }
        }
        public int getLineNumber() {
            return lineNumber;
        }
        public int getColumnNumber() {
            return (int)(((getLineAndColumn(currentTokenStartOffset)&0x7fffffff)));
        }
        private long getLineAndColumn(int offset) {
            if ((newlineOffsets == null)) {
                return offset;
            }
            for (int line = (newlineOffsets.Count-1); (line>=0); line--) {
                int nloffset = newlineOffsets[line];
                if ((offset>nloffset)) {
                    return ((((long)(((line+1)))<<32))|(long)(((offset-nloffset))));
                }
            }
            return offset;
        }
        public int getLineNumber(int offset) {
            return (int)(((getLineAndColumn(offset)>>32)));
        }
        public int getColumnNumber(int offset) {
            return (int)(((getLineAndColumn(offset)&0x7fffffff)));
        }
    
    }
}