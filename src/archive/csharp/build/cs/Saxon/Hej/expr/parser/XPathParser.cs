using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.parser {
    public class XPathParser {
        protected internal Saxon.Hej.expr.parser.Tokenizer t;
        protected internal Saxon.Hej.expr.StaticContext env;
        protected internal Saxon.Hej.tree.util.IndexedStack<Saxon.Hej.expr.LocalBinding> rangeVariables = new Saxon.Hej.tree.util.IndexedStack<Saxon.Hej.expr.LocalBinding>();
        public Saxon.Hej.tree.util.IndexedStack<Saxon.Hej.expr.parser.XPathParser.InlineFunctionDetails> inlineFunctionStack = new Saxon.Hej.tree.util.IndexedStack<Saxon.Hej.expr.parser.XPathParser.InlineFunctionDetails>();
        protected internal Saxon.Hej.om.QNameParser qNameParser;
        protected internal Saxon.Hej.expr.parser.ParserExtension parserExtension = new Saxon.Hej.expr.parser.ParserExtension();
        protected internal Saxon.Hej.z.IntPredicateProxy charChecker;
        protected internal bool allowXPath30Syntax = false;
        protected internal bool allowXPath30XSLTExtensions = false;
        protected internal bool allowXPath31Syntax = false;
        protected internal bool allowXPath40Syntax = false;
        protected internal bool allowSaxonExtensions = false;
        protected internal bool scanOnly = false;
        private bool allowAbsentExpression = false;
        protected internal Saxon.Hej.expr.parser.CodeInjector codeInjector = null;
        private Saxon.Hej.expr.parser.XPathParser.Accelerator accelerator = null;
        public enum ParsedLanguage {
            XPATH,
            XSLT_PATTERN,
            SEQUENCE_TYPE,
            XQUERY,
            EXTENDED_ITEM_TYPE,
        }
        protected internal Saxon.Hej.expr.parser.XPathParser.ParsedLanguage language = Saxon.Hej.expr.parser.XPathParser.ParsedLanguage.XPATH;
        protected internal int languageVersion = 20;
        protected internal int catchDepth = 0;
        public class InlineFunctionDetails {
            public Saxon.Hej.tree.util.IndexedStack<Saxon.Hej.expr.LocalBinding> outerVariables;
            public System.Collections.Generic.IList<Saxon.Hej.expr.LocalBinding> outerVariablesUsed;
            public System.Collections.Generic.IList<Saxon.Hej.expr.instruct.UserFunctionParameter> implicitParams;
        
        }
        public interface Accelerator {
            Saxon.Hej.expr.Expression parse(Saxon.Hej.expr.parser.Tokenizer t, Saxon.Hej.expr.StaticContext env, string expression, int start, int terminator);
        
        }
        public XPathParser()  {
        }
        public void setCodeInjector(Saxon.Hej.expr.parser.CodeInjector injector) {
            this.codeInjector = injector;
        }
        public Saxon.Hej.expr.parser.CodeInjector getCodeInjector() {
            return codeInjector;
        }
        public void setAccelerator(Saxon.Hej.expr.parser.XPathParser.Accelerator accelerator) {
            this.accelerator = accelerator;
        }
        public Saxon.Hej.expr.parser.Tokenizer getTokenizer() {
            return t;
        }
        public Saxon.Hej.expr.StaticContext getStaticContext() {
            return env;
        }
        public void setParserExtension(Saxon.Hej.expr.parser.ParserExtension extension) {
            this.parserExtension = extension;
        }
        public void setCatchDepth(int depth) {
            catchDepth = depth;
        }
        public void nextToken() {
            try {
                t.next();
            } catch (Saxon.Hej.trans.XPathException e) {
                grumble(e.Message);
            }
        }
        public void expect(int token) {
            if ((t.currentToken != token)) {
                grumble(((("expected \""+Saxon.Hej.expr.parser.Token.tokens[token])+"\", found ")+currentTokenDisplay()));
            }
        }
        public void grumble(string message) {
            grumble(message, ((language == Saxon.Hej.expr.parser.XPathParser.ParsedLanguage.XSLT_PATTERN) ? "XTSE0340" : "XPST0003"));
        }
        public void grumble(string message, string errorCode) {
            grumble(message, new Saxon.Hej.om.StructuredQName("", Saxon.Hej.lib.NamespaceConstant.ERR, errorCode), -1);
        }
        public void grumble(string message, string errorCode, int offset) {
            grumble(message, new Saxon.Hej.om.StructuredQName("", Saxon.Hej.lib.NamespaceConstant.ERR, errorCode), offset);
        }
        protected internal void grumble(string message, Saxon.Hej.om.StructuredQName errorCode, int offset) {
            if ((errorCode == null)) {
                errorCode = new Saxon.Hej.om.StructuredQName("err", Saxon.Hej.lib.NamespaceConstant.ERR, "XPST0003");
            }
            string nearbyText = null;
            int line = -1;
            int column = -1;
            if ((t != null)) {
                nearbyText = t.recentText(-1);
                if ((offset == -1)) {
                    line = t.getLineNumber();
                    column = t.getColumnNumber();
                } else {
                    line = t.getLineNumber(offset);
                    column = t.getColumnNumber(offset);
                }
            }
            Saxon.Hej.s9api.Location loc = makeNestedLocation(env.getContainingLocation(), line, column, nearbyText);
            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(message);
            err.setLocation(loc);
            err.setIsSyntaxError("XPST0003".Equals(errorCode.getLocalPart()));
            err.setIsStaticError(true);
            err.setHostLanguage(getLanguage());
            err.setErrorCodeQName(errorCode);
            throw err;
        }
        protected internal void warning(string message) {
            if (!(env.getConfiguration().getBooleanProperty(Saxon.Hej.lib.Feature<bool>.SUPPRESS_XPATH_WARNINGS))) {
                string s = t.recentText(-1);
                string prefix = (((((message.StartsWith("...") ? "near" : "in"))+' ')+Saxon.Hej.trans.Err.wrap(s))+":\n    ");
                env.issueWarning((prefix+message), makeLocation());
            }
        }
        public void setLanguage(Saxon.Hej.expr.parser.XPathParser.ParsedLanguage language, int version) {
            if ((version == 0)) {
                version = 30;
            }
            if ((version == 305)) {
                version = 30;
                allowXPath30XSLTExtensions = true;
            }
            switch (language) {
              case Saxon.Hej.expr.parser.XPathParser.ParsedLanguage.XPATH:
                if (!((((((version == 20) || (version == 30)) || (version == 31)) || (version == 40))))) {
                    throw new Saxon.Ejava.lang.IllegalArgumentException(("Unsupported language version "+version));
                }
                break ;
              case Saxon.Hej.expr.parser.XPathParser.ParsedLanguage.XSLT_PATTERN:
              case Saxon.Hej.expr.parser.XPathParser.ParsedLanguage.SEQUENCE_TYPE:
                if (!((((((version == 20) || (version == 30)) || (version == 31)) || (version == 40))))) {
                    throw new Saxon.Ejava.lang.IllegalArgumentException(("Unsupported language version "+version));
                }
                break ;
              case Saxon.Hej.expr.parser.XPathParser.ParsedLanguage.XQUERY:
                if (!((((((version == 10) || (version == 30)) || (version == 31)) || (version == 40))))) {
                    throw new Saxon.Ejava.lang.IllegalArgumentException(("Unsupported language version "+version));
                }
                break ;
              default:
                throw new Saxon.Ejava.lang.IllegalArgumentException(("Unknown language "+language));
            }
            this.language = language;
            this.languageVersion = version;
            this.allowXPath30Syntax = (languageVersion>=30);
            this.allowXPath31Syntax = (languageVersion>=31);
            this.allowXPath40Syntax = (languageVersion>=40);
        }
        protected internal virtual string getLanguage() {
            switch (language) {
              case Saxon.Hej.expr.parser.XPathParser.ParsedLanguage.XPATH:
                return "XPath";
              case Saxon.Hej.expr.parser.XPathParser.ParsedLanguage.XSLT_PATTERN:
                return "XSLT Pattern";
              case Saxon.Hej.expr.parser.XPathParser.ParsedLanguage.SEQUENCE_TYPE:
                return "SequenceType";
              case Saxon.Hej.expr.parser.XPathParser.ParsedLanguage.XQUERY:
                return "XQuery";
              case Saxon.Hej.expr.parser.XPathParser.ParsedLanguage.EXTENDED_ITEM_TYPE:
                return "Extended ItemType";
              default:
                return "XPath";
            }
        }
        public bool isAllowXPath31Syntax() {
            return allowXPath31Syntax;
        }
        public void setQNameParser(Saxon.Hej.om.QNameParser qp) {
            this.qNameParser = qp;
        }
        public Saxon.Hej.om.QNameParser getQNameParser() {
            return qNameParser;
        }
        protected internal string currentTokenDisplay() {
            if ((t.currentToken == Saxon.Hej.expr.parser.Token.NAME)) {
                return (("name \""+t.currentTokenValue)+'\"');
            } else {
                if ((t.currentToken == Saxon.Hej.expr.parser.Token.UNKNOWN)) {
                    return "(unknown token)";
                } else {
                    return (('\"'+Saxon.Hej.expr.parser.Token.tokens[t.currentToken])+'\"');
                }
            }
        }
        public Saxon.Hej.expr.Expression parse(string expression, int start, int terminator, Saxon.Hej.expr.StaticContext env) {
            this.env = env;
            int languageVersion = env.getXPathVersion();
            if (((languageVersion == 20) && (language == Saxon.Hej.expr.parser.XPathParser.ParsedLanguage.XQUERY))) {
                languageVersion = 10;
            }
            setLanguage(language, languageVersion);
            Saxon.Hej.expr.Expression exp = null;
            int offset = start;
            if (((((accelerator != null) && (env.getUnprefixedElementMatchingPolicy() == Saxon.Api.UnprefixedElementMatchingPolicy.DefaultNamespace)) && (terminator != Saxon.Hej.expr.parser.Token.IMPLICIT_EOF)) && ((((expression.Length-start)<30) || (terminator == Saxon.Hej.expr.parser.Token.RCURLY))))) {
                t = new Saxon.Hej.expr.parser.Tokenizer();
                t.languageLevel = env.getXPathVersion();
                exp = accelerator.parse(t, env, expression, start, terminator);
            }
            if ((exp == null)) {
                qNameParser = new Saxon.Hej.om.QNameParser(env.getNamespaceResolver()).withAcceptEQName(allowXPath30Syntax).withErrorOnBadSyntax(((language == Saxon.Hej.expr.parser.XPathParser.ParsedLanguage.XSLT_PATTERN) ? "XTSE0340" : "XPST0003")).withErrorOnUnresolvedPrefix("XPST0081");
                charChecker = env.getConfiguration().getValidCharacterChecker();
                t = new Saxon.Hej.expr.parser.Tokenizer();
                t.languageLevel = env.getXPathVersion();
                allowXPath40Syntax = t.allowSaxonExtensions = (env.getConfiguration().getBooleanProperty(Saxon.Hej.lib.Feature<bool>.ALLOW_SYNTAX_EXTENSIONS) || (t.languageLevel == 40));
                offset = t.currentTokenStartOffset;
                customizeTokenizer(t);
                try {
                    t.tokenize(expression, start, -1);
                } catch (Saxon.Hej.trans.XPathException err) {
                    grumble(err.Message);
                }
                if ((t.currentToken == terminator)) {
                    if (allowAbsentExpression) {
                        Saxon.Hej.expr.Expression result = Saxon.Hej.expr.Literal.makeEmptySequence();
                        result.setRetainedStaticContext(env.makeRetainedStaticContext());
                        setLocation(result);
                        return result;
                    } else {
                        grumble("The expression is empty");
                    }
                }
                exp = parseExpression();
                if (((t.currentToken != terminator) && (terminator != Saxon.Hej.expr.parser.Token.IMPLICIT_EOF))) {
                    if (((t.currentToken == Saxon.Hej.expr.parser.Token.EOF) && (terminator == Saxon.Hej.expr.parser.Token.RCURLY))) {
                        grumble("Missing curly brace after expression in value template", "XTSE0350");
                    } else {
                        grumble((("Unexpected token "+currentTokenDisplay())+" beyond end of expression"));
                    }
                }
                setLocation(exp, offset);
            }
            exp.setRetainedStaticContextThoroughly(env.makeRetainedStaticContext());
            return exp;
        }
        protected internal virtual void customizeTokenizer(Saxon.Hej.expr.parser.Tokenizer t) {
        }
        public Saxon.Hej.value.SequenceType parseSequenceType(string input, Saxon.Hej.expr.StaticContext env) {
            this.env = env;
            if ((qNameParser == null)) {
                qNameParser = new Saxon.Hej.om.QNameParser(env.getNamespaceResolver());
                if ((languageVersion>=30)) {
                    qNameParser = qNameParser.withAcceptEQName(true);
                }
            }
            language = Saxon.Hej.expr.parser.XPathParser.ParsedLanguage.SEQUENCE_TYPE;
            t = new Saxon.Hej.expr.parser.Tokenizer();
            t.languageLevel = env.getXPathVersion();
            allowXPath40Syntax = t.allowSaxonExtensions = (env.getConfiguration().getBooleanProperty(Saxon.Hej.lib.Feature<bool>.ALLOW_SYNTAX_EXTENSIONS) || (t.languageLevel == 40));
            try {
                t.tokenize(input, 0, -1);
            } catch (Saxon.Hej.trans.XPathException err) {
                grumble(err.Message);
            }
            Saxon.Hej.value.SequenceType req = parseSequenceType();
            if ((t.currentToken != Saxon.Hej.expr.parser.Token.EOF)) {
                grumble((("Unexpected token "+currentTokenDisplay())+" beyond end of SequenceType"));
            }
            return req;
        }
        public Saxon.Hej.type.ItemType parseExtendedItemType(string input, Saxon.Hej.expr.StaticContext env) {
            this.env = env;
            language = Saxon.Hej.expr.parser.XPathParser.ParsedLanguage.EXTENDED_ITEM_TYPE;
            t = new Saxon.Hej.expr.parser.Tokenizer();
            t.languageLevel = env.getXPathVersion();
            allowSaxonExtensions = t.allowSaxonExtensions = true;
            try {
                t.tokenize(input, 0, -1);
            } catch (Saxon.Hej.trans.XPathException err) {
                grumble(err.Message);
            }
            Saxon.Hej.type.ItemType req = parseItemType();
            if ((t.currentToken != Saxon.Hej.expr.parser.Token.EOF)) {
                grumble((("Unexpected token "+currentTokenDisplay())+" beyond end of ItemType"));
            }
            return req;
        }
        public Saxon.Hej.value.SequenceType parseExtendedSequenceType(string input, Saxon.Hej.expr.StaticContext env) {
            this.env = env;
            language = Saxon.Hej.expr.parser.XPathParser.ParsedLanguage.EXTENDED_ITEM_TYPE;
            t = new Saxon.Hej.expr.parser.Tokenizer();
            t.languageLevel = languageVersion = env.getXPathVersion();
            allowSaxonExtensions = t.allowSaxonExtensions = true;
            allowXPath30Syntax = (languageVersion>=30);
            allowXPath31Syntax = (languageVersion>=31);
            allowXPath40Syntax = (languageVersion>=40);
            try {
                t.tokenize(input, 0, -1);
            } catch (Saxon.Hej.trans.XPathException err) {
                grumble(err.Message);
            }
            Saxon.Hej.value.SequenceType req = parseSequenceType();
            if ((t.currentToken != Saxon.Hej.expr.parser.Token.EOF)) {
                grumble((("Unexpected token "+currentTokenDisplay())+" beyond end of SequenceType"));
            }
            return req;
        }
        public virtual Saxon.Hej.expr.Expression parseExpression() {
            int offset = t.currentTokenStartOffset;
            Saxon.Hej.expr.Expression exp = parseExprSingle();
            System.Collections.Generic.List<Saxon.Hej.expr.Expression> list = null;
            while ((t.currentToken == Saxon.Hej.expr.parser.Token.COMMA)) {
                if ((list == null)) {
                    list = new System.Collections.Generic.List<Saxon.Hej.expr.Expression>(10);
                    list.Add(exp);
                }
                nextToken();
                Saxon.Hej.expr.Expression next = parseExprSingle();
                setLocation(next);
                list.Add(next);
            }
            if ((list != null)) {
                exp = Saxon.Hej.expr.instruct.Block.makeBlock(list);
                setLocation(exp, offset);
            }
            return exp;
        }
        public Saxon.Hej.expr.Expression parseExprSingle() {
            Saxon.Hej.expr.Expression e = parserExtension.parseExtendedExprSingle(this);
            if ((e != null)) {
                return e;
            }
            int peek = t.peekAhead();
            if (((((peek == Saxon.Hej.expr.parser.Token.EOF) || (peek == Saxon.Hej.expr.parser.Token.COMMA)) || (peek == Saxon.Hej.expr.parser.Token.RPAR)) || (peek == Saxon.Hej.expr.parser.Token.RSQB))) {
                switch (t.currentToken) {
                  case Saxon.Hej.expr.parser.Token.STRING_LITERAL:
                    return parseStringLiteral(true);
                  case Saxon.Hej.expr.parser.Token.NUMBER:
                    return parseNumericLiteral(true);
                  case Saxon.Hej.expr.parser.Token.NAME:
                  case Saxon.Hej.expr.parser.Token.PREFIX:
                  case Saxon.Hej.expr.parser.Token.SUFFIX:
                  case Saxon.Hej.expr.parser.Token.STAR:
                    return parseBasicStep(true);
                  case Saxon.Hej.expr.parser.Token.DOT:
                    nextToken();
                    Saxon.Hej.expr.Expression cie = new Saxon.Hej.expr.ContextItemExpression();
                    setLocation(cie);
                    return cie;
                  case Saxon.Hej.expr.parser.Token.DOTDOT:
                    nextToken();
                    Saxon.Hej.expr.Expression pne = new Saxon.Hej.expr.AxisExpression(Saxon.Hej.om.AxisInfo.PARENT, null);
                    setLocation(pne);
                    return pne;
                  case Saxon.Hej.expr.parser.Token.EOF:
                  default:
                    break ;
                }
            }
            switch (t.currentToken) {
              case Saxon.Hej.expr.parser.Token.EOF:
                grumble("Expected an expression, but reached the end of the input");
                return null;
              case Saxon.Hej.expr.parser.Token.FOR:
              case Saxon.Hej.expr.parser.Token.LET:
              case Saxon.Hej.expr.parser.Token.FOR_MEMBER:
              case Saxon.Hej.expr.parser.Token.FOR_SLIDING:
              case Saxon.Hej.expr.parser.Token.FOR_TUMBLING:
                return parseFLWORExpression();
              case Saxon.Hej.expr.parser.Token.SOME:
              case Saxon.Hej.expr.parser.Token.EVERY:
                return parseQuantifiedExpression();
              case Saxon.Hej.expr.parser.Token.IF:
                return parseIfExpression();
              case Saxon.Hej.expr.parser.Token.SWITCH:
                return parseSwitchExpression();
              case Saxon.Hej.expr.parser.Token.TYPESWITCH:
                return parseTypeswitchExpression();
              case Saxon.Hej.expr.parser.Token.VALIDATE:
              case Saxon.Hej.expr.parser.Token.VALIDATE_STRICT:
              case Saxon.Hej.expr.parser.Token.VALIDATE_LAX:
              case Saxon.Hej.expr.parser.Token.VALIDATE_TYPE:
                return parseValidateExpression();
              case Saxon.Hej.expr.parser.Token.PRAGMA:
                return parseExtensionExpression();
              case Saxon.Hej.expr.parser.Token.KEYWORD_CURLY:
                if (t.currentTokenValue.Equals("try")) {
                    return parseTryCatchExpression();
                }
                goto default;
              default:
                Saxon.Hej.expr.Expression e1 = parseBinaryExpression(parseUnaryExpression(), 4);
                if ((t.currentToken == Saxon.Hej.expr.parser.Token.QMARK_QMARK)) {
                    if (!(allowXPath40Syntax)) {
                        grumble("Ternary conditionals (A ?? B !! C) require XPath 4.0 to be enabled");
                    }
                    return parseTernaryExpression(e1);
                }
                return e1;
            }
        }
        private Saxon.Hej.expr.Expression parseTernaryExpression(Saxon.Hej.expr.Expression condition) {
            nextToken();
            Saxon.Hej.expr.Expression e2 = parseExprSingle();
            expect(Saxon.Hej.expr.parser.Token.BANG_BANG);
            nextToken();
            Saxon.Hej.expr.Expression e3 = parseExprSingle();
            return Saxon.Hej.expr.instruct.Choose.makeConditional(condition, e2, e3);
        }
        public Saxon.Hej.expr.Expression parseBinaryExpression(Saxon.Hej.expr.Expression lhs, int minPrecedence) {
            while ((getCurrentOperatorPrecedence()>=minPrecedence)) {
                int offset = t.currentTokenStartOffset;
                int @operator = t.currentToken;
                int prec = getCurrentOperatorPrecedence();
                switch (@operator) {
                  case Saxon.Hej.expr.parser.Token.INSTANCE_OF:
                  case Saxon.Hej.expr.parser.Token.TREAT_AS:
                    nextToken();
                    Saxon.Hej.value.SequenceType seq = parseSequenceType();
                    lhs = makeSequenceTypeExpression(lhs, @operator, seq);
                    setLocation(lhs, offset);
                    if ((getCurrentOperatorPrecedence()>=prec)) {
                        grumble((("Left operand of '"+Saxon.Hej.expr.parser.Token.tokens[t.currentToken])+"' needs parentheses"));
                    }
                    break ;
                  case Saxon.Hej.expr.parser.Token.CAST_AS:
                  case Saxon.Hej.expr.parser.Token.CASTABLE_AS:
                    nextToken();
                    Saxon.Hej.type.CastingTarget at;
                    if ((((languageVersion>=40) && (t.currentToken == Saxon.Hej.expr.parser.Token.NODEKIND)) && t.currentTokenValue.Equals("union"))) {
                        at = (Saxon.Hej.type.CastingTarget)(parseItemType());
                    } else {
                        expect(Saxon.Hej.expr.parser.Token.NAME);
                        at = getSimpleType(t.currentTokenValue);
                        nextToken();
                    }
                    if ((at == Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC)) {
                        grumble("No value is castable to xs:anyAtomicType", "XPST0080");
                    }
                    if ((at == Saxon.Hej.type.BuiltInAtomicType.NOTATION)) {
                        grumble("No value is castable to xs:NOTATION", "XPST0080");
                    }
                    bool allowEmpty = (t.currentToken == Saxon.Hej.expr.parser.Token.QMARK);
                    if (allowEmpty) {
                        nextToken();
                    }
                    lhs = makeSingleTypeExpression(lhs, @operator, at, allowEmpty);
                    setLocation(lhs, offset);
                    if ((getCurrentOperatorPrecedence()>=prec)) {
                        grumble((("Left operand of '"+Saxon.Hej.expr.parser.Token.tokens[t.currentToken])+"' needs parentheses"));
                    }
                    break ;
                  case Saxon.Hej.expr.parser.Token.FAT_ARROW:
                    lhs = parseArrowPostfix(lhs);
                    break ;
                  default:
                    nextToken();
                    Saxon.Hej.expr.Expression rhs = parseUnaryExpression();
                    while ((getCurrentOperatorPrecedence()>prec)) {
                        rhs = parseBinaryExpression(rhs, getCurrentOperatorPrecedence());
                    }
                    if (((getCurrentOperatorPrecedence() == prec) && !(allowMultipleOperators()))) {
                        string tok = Saxon.Hej.expr.parser.Token.tokens[t.currentToken];
                        string message = (("Left operand of '"+Saxon.Hej.expr.parser.Token.tokens[t.currentToken])+"' needs parentheses");
                        if ((tok.Equals("<") || tok.Equals(">"))) {
                            message += ". Or perhaps an XQuery element constructor appears where it is not allowed";
                        }
                        grumble(message);
                    }
                    lhs = makeBinaryExpression(lhs, @operator, rhs);
                    setLocation(lhs, offset);
                    break ;
                }
            }
            return lhs;
        }
        private bool allowMultipleOperators() {
            switch (t.currentToken) {
              case Saxon.Hej.expr.parser.Token.FEQ:
              case Saxon.Hej.expr.parser.Token.FNE:
              case Saxon.Hej.expr.parser.Token.FLE:
              case Saxon.Hej.expr.parser.Token.FLT:
              case Saxon.Hej.expr.parser.Token.FGE:
              case Saxon.Hej.expr.parser.Token.FGT:
              case Saxon.Hej.expr.parser.Token.EQUALS:
              case Saxon.Hej.expr.parser.Token.NE:
              case Saxon.Hej.expr.parser.Token.LE:
              case Saxon.Hej.expr.parser.Token.LT:
              case Saxon.Hej.expr.parser.Token.GE:
              case Saxon.Hej.expr.parser.Token.GT:
              case Saxon.Hej.expr.parser.Token.IS:
              case Saxon.Hej.expr.parser.Token.PRECEDES:
              case Saxon.Hej.expr.parser.Token.FOLLOWS:
              case Saxon.Hej.expr.parser.Token.TO:
                return false;
              default:
                return true;
            }
        }
        private int getCurrentOperatorPrecedence() {
            return operatorPrecedence(t.currentToken);
        }
        public static int operatorPrecedence(int @operator) {
            switch (@operator) {
              case Saxon.Hej.expr.parser.Token.QMARK_QMARK:
              case Saxon.Hej.expr.parser.Token.BANG_BANG:
                return 3;
              case Saxon.Hej.expr.parser.Token.OR:
              case Saxon.Hej.expr.parser.Token.OR_ELSE:
                return 4;
              case Saxon.Hej.expr.parser.Token.AND:
              case Saxon.Hej.expr.parser.Token.AND_ALSO:
                return 5;
              case Saxon.Hej.expr.parser.Token.FEQ:
              case Saxon.Hej.expr.parser.Token.FNE:
              case Saxon.Hej.expr.parser.Token.FLE:
              case Saxon.Hej.expr.parser.Token.FLT:
              case Saxon.Hej.expr.parser.Token.FGE:
              case Saxon.Hej.expr.parser.Token.FGT:
              case Saxon.Hej.expr.parser.Token.EQUALS:
              case Saxon.Hej.expr.parser.Token.NE:
              case Saxon.Hej.expr.parser.Token.LE:
              case Saxon.Hej.expr.parser.Token.LT:
              case Saxon.Hej.expr.parser.Token.GE:
              case Saxon.Hej.expr.parser.Token.GT:
              case Saxon.Hej.expr.parser.Token.IS:
              case Saxon.Hej.expr.parser.Token.PRECEDES:
              case Saxon.Hej.expr.parser.Token.FOLLOWS:
                return 6;
              case Saxon.Hej.expr.parser.Token.CONCAT:
                return 7;
              case Saxon.Hej.expr.parser.Token.BY:
                return 8;
              case Saxon.Hej.expr.parser.Token.TO:
                return 9;
              case Saxon.Hej.expr.parser.Token.PLUS:
              case Saxon.Hej.expr.parser.Token.MINUS:
                return 10;
              case Saxon.Hej.expr.parser.Token.MULT:
              case Saxon.Hej.expr.parser.Token.DIV:
              case Saxon.Hej.expr.parser.Token.IDIV:
              case Saxon.Hej.expr.parser.Token.MOD:
                return 11;
              case Saxon.Hej.expr.parser.Token.OTHERWISE:
                return 12;
              case Saxon.Hej.expr.parser.Token.UNION:
                return 13;
              case Saxon.Hej.expr.parser.Token.INTERSECT:
              case Saxon.Hej.expr.parser.Token.EXCEPT:
                return 14;
              case Saxon.Hej.expr.parser.Token.INSTANCE_OF:
                return 15;
              case Saxon.Hej.expr.parser.Token.TREAT_AS:
                return 16;
              case Saxon.Hej.expr.parser.Token.CASTABLE_AS:
                return 17;
              case Saxon.Hej.expr.parser.Token.CAST_AS:
                return 18;
              case Saxon.Hej.expr.parser.Token.FAT_ARROW:
                return 19;
              default:
                return -1;
            }
        }
        private Saxon.Hej.expr.Expression makeBinaryExpression(Saxon.Hej.expr.Expression lhs, int @operator, Saxon.Hej.expr.Expression rhs) {
            switch (@operator) {
              case Saxon.Hej.expr.parser.Token.OR:
                return new Saxon.Hej.expr.OrExpression(lhs, rhs);
              case Saxon.Hej.expr.parser.Token.AND:
                return new Saxon.Hej.expr.AndExpression(lhs, rhs);
              case Saxon.Hej.expr.parser.Token.FEQ:
              case Saxon.Hej.expr.parser.Token.FNE:
              case Saxon.Hej.expr.parser.Token.FLE:
              case Saxon.Hej.expr.parser.Token.FLT:
              case Saxon.Hej.expr.parser.Token.FGE:
              case Saxon.Hej.expr.parser.Token.FGT:
                return new Saxon.Hej.expr.ValueComparison(lhs, @operator, rhs);
              case Saxon.Hej.expr.parser.Token.EQUALS:
              case Saxon.Hej.expr.parser.Token.NE:
              case Saxon.Hej.expr.parser.Token.LE:
              case Saxon.Hej.expr.parser.Token.LT:
              case Saxon.Hej.expr.parser.Token.GE:
              case Saxon.Hej.expr.parser.Token.GT:
                return env.getConfiguration().getTypeChecker(env.isInBackwardsCompatibleMode()).makeGeneralComparison(lhs, @operator, rhs);
              case Saxon.Hej.expr.parser.Token.IS:
              case Saxon.Hej.expr.parser.Token.PRECEDES:
              case Saxon.Hej.expr.parser.Token.FOLLOWS:
                return new Saxon.Hej.expr.IdentityComparison(lhs, @operator, rhs);
              case Saxon.Hej.expr.parser.Token.BY:
                if (!(allowXPath40Syntax)) {
                    grumble("Range operator 'by' requires XPath 4.0 to be enabled");
                }
                return Saxon.Hej.expr.RangeExpression.makeSteppingExpression(lhs, rhs);
              case Saxon.Hej.expr.parser.Token.TO:
                return new Saxon.Hej.expr.RangeExpression(lhs, rhs);
              case Saxon.Hej.expr.parser.Token.CONCAT:
                {
                    if (!(allowXPath30Syntax)) {
                        grumble("Concatenation operator ('||') requires XPath 3.0 to be enabled");
                    }
                    Saxon.Hej.expr.parser.RetainedStaticContext rsc = new Saxon.Hej.expr.parser.RetainedStaticContext(env);
                    if (lhs.isCallOn(typeof(Saxon.Hej.functions.Concat))) {
                        Saxon.Hej.expr.Expression[] args = ((Saxon.Hej.expr.SystemFunctionCall)(lhs)).getArguments();
                        Saxon.Hej.expr.Expression[] newArgs = new Saxon.Hej.expr.Expression[(args.Length+1)];
                        Saxon.Ejava.lang.System.arraycopy(args, 0, newArgs, 0, args.Length);
                        newArgs[args.Length] = rhs;
                        return Saxon.Hej.functions.SystemFunction.makeCall("concat", rsc, newArgs);
                    } else {
                        return Saxon.Hej.functions.SystemFunction.makeCall("concat", rsc, lhs, rhs);
                    }
                }
              case Saxon.Hej.expr.parser.Token.PLUS:
              case Saxon.Hej.expr.parser.Token.MINUS:
              case Saxon.Hej.expr.parser.Token.MULT:
              case Saxon.Hej.expr.parser.Token.DIV:
              case Saxon.Hej.expr.parser.Token.IDIV:
              case Saxon.Hej.expr.parser.Token.MOD:
                return env.getConfiguration().getTypeChecker(env.isInBackwardsCompatibleMode()).makeArithmeticExpression(lhs, @operator, rhs);
              case Saxon.Hej.expr.parser.Token.OTHERWISE:
                return makeOtherwiseExpression(lhs, rhs);
              case Saxon.Hej.expr.parser.Token.UNION:
              case Saxon.Hej.expr.parser.Token.INTERSECT:
              case Saxon.Hej.expr.parser.Token.EXCEPT:
                return new Saxon.Hej.expr.VennExpression(lhs, @operator, rhs);
              case Saxon.Hej.expr.parser.Token.OR_ELSE:
                {
                    Saxon.Hej.expr.parser.RetainedStaticContext rsc = new Saxon.Hej.expr.parser.RetainedStaticContext(env);
                    rhs = Saxon.Hej.functions.SystemFunction.makeCall("boolean", rsc, rhs);
                    return Saxon.Hej.expr.instruct.Choose.makeConditional(lhs, Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.BooleanValue.TRUE), rhs);
                }
              case Saxon.Hej.expr.parser.Token.AND_ALSO:
                {
                    Saxon.Hej.expr.parser.RetainedStaticContext rsc = new Saxon.Hej.expr.parser.RetainedStaticContext(env);
                    rhs = Saxon.Hej.functions.SystemFunction.makeCall("boolean", rsc, rhs);
                    return Saxon.Hej.expr.instruct.Choose.makeConditional(lhs, rhs, Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.BooleanValue.FALSE));
                }
              default:
                throw new Saxon.Ejava.lang.IllegalArgumentException(Saxon.Hej.expr.parser.Token.tokens[@operator]);
            }
        }
        private Saxon.Hej.expr.Expression makeOtherwiseExpression(Saxon.Hej.expr.Expression lhs, Saxon.Hej.expr.Expression rhs) {
            Saxon.Hej.expr.LetExpression let = new Saxon.Hej.expr.LetExpression();
            let.setVariableQName(new Saxon.Hej.om.StructuredQName("vv", Saxon.Hej.lib.NamespaceConstant.ANONYMOUS, ("n"+lhs.GetHashCode())));
            let.setSequence(lhs);
            let.setRequiredType(Saxon.Hej.value.SequenceType.ANY_SEQUENCE);
            Saxon.Hej.expr.LocalVariableReference v1 = new Saxon.Hej.expr.LocalVariableReference(let.getVariableQName());
            v1.setBinding(let);
            let.addReference(v1, false);
            Saxon.Hej.expr.LocalVariableReference v2 = new Saxon.Hej.expr.LocalVariableReference(let.getVariableQName());
            v2.setBinding(let);
            let.addReference(v2, false);
            Saxon.Hej.expr.parser.RetainedStaticContext rsc = new Saxon.Hej.expr.parser.RetainedStaticContext(env);
            Saxon.Hej.expr.Expression[] conditions = new Saxon.Hej.expr.Expression[]{Saxon.Hej.functions.SystemFunction.makeCall("exists", rsc, v1), Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.BooleanValue.TRUE, lhs)};
            Saxon.Hej.expr.Expression[] actions = new Saxon.Hej.expr.Expression[]{v2, rhs};
            let.setAction(new Saxon.Hej.expr.instruct.Choose(conditions, actions));
            return let;
        }
        private Saxon.Hej.expr.Expression makeSequenceTypeExpression(Saxon.Hej.expr.Expression lhs, int @operator, Saxon.Hej.value.SequenceType type) {
            switch (@operator) {
              case Saxon.Hej.expr.parser.Token.INSTANCE_OF:
                return new Saxon.Hej.expr.InstanceOfExpression(lhs, type);
              case Saxon.Hej.expr.parser.Token.TREAT_AS:
                return Saxon.Hej.expr.TreatExpression.make(lhs, type);
              default:
                throw new Saxon.Ejava.lang.IllegalArgumentException();
            }
        }
        private Saxon.Hej.expr.Expression makeSingleTypeExpression(Saxon.Hej.expr.Expression lhs, int @operator, Saxon.Hej.type.CastingTarget type, bool allowEmpty) {
            if (((type is Saxon.Hej.type.AtomicType) && !(((type == Saxon.Hej.type.ErrorType.getInstance()))))) {
                switch (@operator) {
                  case Saxon.Hej.expr.parser.Token.CASTABLE_AS:
                    Saxon.Hej.expr.CastableExpression castable = new Saxon.Hej.expr.CastableExpression(lhs, (Saxon.Hej.type.AtomicType)(type), allowEmpty);
                    if ((lhs is Saxon.Hej.expr.StringLiteral)) {
                        castable.setOperandIsStringLiteral(true);
                    }
                    return castable;
                  case Saxon.Hej.expr.parser.Token.CAST_AS:
                    Saxon.Hej.expr.CastExpression cast = new Saxon.Hej.expr.CastExpression(lhs, (Saxon.Hej.type.AtomicType)(type), allowEmpty);
                    if ((lhs is Saxon.Hej.expr.StringLiteral)) {
                        cast.setOperandIsStringLiteral(true);
                    }
                    return cast;
                  default:
                    throw new Saxon.Ejava.lang.IllegalArgumentException();
                }
            } else {
                if (allowXPath30Syntax) {
                    switch (@operator) {
                      case Saxon.Hej.expr.parser.Token.CASTABLE_AS:
                        if ((type is Saxon.Hej.type.UnionType)) {
                            Saxon.Hej.om.NamespaceResolver resolver = env.getNamespaceResolver();
                            Saxon.Hej.expr.UnionCastableFunction ucf = new Saxon.Hej.expr.UnionCastableFunction((Saxon.Hej.type.UnionType)(type), resolver, allowEmpty);
                            return new Saxon.Hej.expr.StaticFunctionCall(ucf, new Saxon.Hej.expr.Expression[]{lhs});
                        } else {
                            if ((type is Saxon.Hej.type.ListType)) {
                                Saxon.Hej.om.NamespaceResolver resolver = env.getNamespaceResolver();
                                Saxon.Hej.expr.ListCastableFunction lcf = new Saxon.Hej.expr.ListCastableFunction((Saxon.Hej.type.ListType)(type), resolver, allowEmpty);
                                return new Saxon.Hej.expr.StaticFunctionCall(lcf, new Saxon.Hej.expr.Expression[]{lhs});
                            }
                        }
                        break ;
                      case Saxon.Hej.expr.parser.Token.CAST_AS:
                        if ((type is Saxon.Hej.type.UnionType)) {
                            Saxon.Hej.om.NamespaceResolver resolver = env.getNamespaceResolver();
                            Saxon.Hej.expr.UnionConstructorFunction ucf = new Saxon.Hej.expr.UnionConstructorFunction((Saxon.Hej.type.UnionType)(type), resolver, allowEmpty);
                            return new Saxon.Hej.expr.StaticFunctionCall(ucf, new Saxon.Hej.expr.Expression[]{lhs});
                        } else {
                            if ((type is Saxon.Hej.type.ListType)) {
                                Saxon.Hej.om.NamespaceResolver resolver = env.getNamespaceResolver();
                                Saxon.Hej.expr.ListConstructorFunction lcf = new Saxon.Hej.expr.ListConstructorFunction((Saxon.Hej.type.ListType)(type), resolver, allowEmpty);
                                return new Saxon.Hej.expr.StaticFunctionCall(lcf, new Saxon.Hej.expr.Expression[]{lhs});
                            }
                        }
                        break ;
                      default:
                        throw new Saxon.Ejava.lang.IllegalArgumentException();
                    }
                    throw new Saxon.Hej.trans.XPathException(("Cannot cast to "+type.GetType()), "XPST0051");
                } else {
                    throw new Saxon.Hej.trans.XPathException("Casting to list or union types requires XPath 3.0 to be enabled", "XPST0051");
                }
            }
        }
        protected internal virtual Saxon.Hej.expr.Expression parseTypeswitchExpression() {
            grumble("typeswitch is not allowed in XPath");
            return new Saxon.Hej.expr.ErrorExpression();
        }
        protected internal virtual Saxon.Hej.expr.Expression parseSwitchExpression() {
            grumble("switch is not allowed in XPath");
            return new Saxon.Hej.expr.ErrorExpression();
        }
        protected internal virtual Saxon.Hej.expr.Expression parseValidateExpression() {
            grumble("validate{} expressions are not allowed in XPath");
            return new Saxon.Hej.expr.ErrorExpression();
        }
        protected internal virtual Saxon.Hej.expr.Expression parseExtensionExpression() {
            grumble("extension expressions (#...#) are not allowed in XPath");
            return new Saxon.Hej.expr.ErrorExpression();
        }
        protected internal virtual Saxon.Hej.expr.Expression parseTryCatchExpression() {
            grumble("try/catch expressions are not allowed in XPath");
            return new Saxon.Hej.expr.ErrorExpression();
        }
        protected internal virtual Saxon.Hej.expr.Expression parseFLWORExpression() {
            if (((t.currentToken == Saxon.Hej.expr.parser.Token.LET) && !(allowXPath30Syntax))) {
                grumble("'let' is not permitted in XPath 2.0");
            }
            if (((t.currentToken == Saxon.Hej.expr.parser.Token.FOR_SLIDING) || (t.currentToken == Saxon.Hej.expr.parser.Token.FOR_TUMBLING))) {
                grumble("sliding/tumbling windows can only be used in XQuery");
            }
            if (((t.currentToken == Saxon.Hej.expr.parser.Token.FOR_MEMBER) && !(allowXPath40Syntax))) {
                grumble("'for member' requires XPath 4.0 to be enabled");
            }
            int clauses = 0;
            int offset;
            int @operator = t.currentToken;
            Saxon.Hej.expr.Assignation first = null;
            Saxon.Hej.expr.Assignation previous = null;
            do {
                offset = t.currentTokenStartOffset;
                nextToken();
                expect(Saxon.Hej.expr.parser.Token.DOLLAR);
                nextToken();
                expect(Saxon.Hej.expr.parser.Token.NAME);
                string var = t.currentTokenValue;
                Saxon.Hej.expr.Assignation v;
                if ((@operator == Saxon.Hej.expr.parser.Token.FOR)) {
                    v = new Saxon.Hej.expr.ForExpression();
                    v.setRequiredType(Saxon.Hej.value.SequenceType.SINGLE_ITEM);
                } else {
                    if ((@operator == Saxon.Hej.expr.parser.Token.FOR_MEMBER)) {
                        v = new Saxon.Hej.expr.ForMemberExpression();
                        v.setRequiredType(Saxon.Hej.value.SequenceType.ANY_SEQUENCE);
                    } else {
                        v = new Saxon.Hej.expr.LetExpression();
                        v.setRequiredType(Saxon.Hej.value.SequenceType.ANY_SEQUENCE);
                    }
                }
                clauses++;
                setLocation(v, offset);
                v.setVariableQName(makeStructuredQName(var, ""));
                nextToken();
                expect(((@operator == Saxon.Hej.expr.parser.Token.LET) ? Saxon.Hej.expr.parser.Token.ASSIGN : Saxon.Hej.expr.parser.Token.IN));
                nextToken();
                v.setSequence(parseExprSingle());
                declareRangeVariable(v);
                if ((previous == null)) {
                    first = v;
                } else {
                    previous.setAction(v);
                }
                previous = v;
            } while ((t.currentToken == Saxon.Hej.expr.parser.Token.COMMA));
            expect(Saxon.Hej.expr.parser.Token.RETURN);
            nextToken();
            previous.setAction(parseExprSingle());
            for (int i = 0; (i<clauses); i++) {
                undeclareRangeVariable();
            }
            return makeTracer(first, first.getVariableQName());
        }
        private Saxon.Hej.expr.Expression parseQuantifiedExpression() {
            int clauses = 0;
            int @operator = t.currentToken;
            Saxon.Hej.expr.QuantifiedExpression first = null;
            Saxon.Hej.expr.QuantifiedExpression previous = null;
            do {
                int offset = t.currentTokenStartOffset;
                nextToken();
                expect(Saxon.Hej.expr.parser.Token.DOLLAR);
                nextToken();
                expect(Saxon.Hej.expr.parser.Token.NAME);
                string var = t.currentTokenValue;
                clauses++;
                Saxon.Hej.expr.QuantifiedExpression v = new Saxon.Hej.expr.QuantifiedExpression();
                v.setRequiredType(Saxon.Hej.value.SequenceType.SINGLE_ITEM);
                v.setOperator(@operator);
                setLocation(v, offset);
                v.setVariableQName(makeStructuredQName(var, ""));
                nextToken();
                if (((t.currentToken == Saxon.Hej.expr.parser.Token.AS) && (language == Saxon.Hej.expr.parser.XPathParser.ParsedLanguage.XQUERY))) {
                    nextToken();
                    Saxon.Hej.value.SequenceType type = parseSequenceType();
                    if ((type.getCardinality() != Saxon.Hej.expr.StaticProperty.EXACTLY_ONE)) {
                        warning("Occurrence indicator on singleton range variable has no effect");
                        type = Saxon.Hej.value.SequenceType.makeSequenceType(type.getPrimaryType(), Saxon.Hej.expr.StaticProperty.EXACTLY_ONE);
                    }
                    v.setRequiredType(type);
                }
                expect(Saxon.Hej.expr.parser.Token.IN);
                nextToken();
                v.setSequence(parseExprSingle());
                declareRangeVariable(v);
                if ((previous != null)) {
                    previous.setAction(v);
                } else {
                    first = v;
                }
                previous = v;
            } while ((t.currentToken == Saxon.Hej.expr.parser.Token.COMMA));
            expect(Saxon.Hej.expr.parser.Token.SATISFIES);
            nextToken();
            previous.setAction(parseExprSingle());
            for (int i = 0; (i<clauses); i++) {
                undeclareRangeVariable();
            }
            return makeTracer(first, first.getVariableQName());
        }
        private Saxon.Hej.expr.Expression parseIfExpression() {
            int ifoffset = t.currentTokenStartOffset;
            nextToken();
            Saxon.Hej.expr.Expression condition = parseExpression();
            expect(Saxon.Hej.expr.parser.Token.RPAR);
            nextToken();
            int thenoffset = t.currentTokenStartOffset;
            expect(Saxon.Hej.expr.parser.Token.THEN);
            nextToken();
            Saxon.Hej.expr.Expression thenExp = makeTracer(parseExprSingle(), null);
            int elseoffset = t.currentTokenStartOffset;
            expect(Saxon.Hej.expr.parser.Token.ELSE);
            nextToken();
            Saxon.Hej.expr.Expression elseExp = makeTracer(parseExprSingle(), null);
            Saxon.Hej.expr.Expression ifExp = Saxon.Hej.expr.instruct.Choose.makeConditional(condition, thenExp, elseExp);
            setLocation(ifExp, ifoffset);
            return makeTracer(ifExp, null);
        }
        private Saxon.Hej.type.ItemType getPlainType(string qname) {
            if (scanOnly) {
                return Saxon.Hej.type.BuiltInAtomicType.STRING;
            }
            Saxon.Hej.om.StructuredQName sq = null;
            try {
                sq = qNameParser.parse(qname, env.getDefaultElementNamespace());
            } catch (Saxon.Hej.trans.XPathException e) {
                grumble(e.Message, e.getErrorCodeLocalPart());
                return null;
            }
            return getPlainType(sq);
        }
        public Saxon.Hej.type.ItemType getPlainType(Saxon.Hej.om.StructuredQName sq) {
            Saxon.Hej.Configuration config = env.getConfiguration();
            string uri = sq.getURI();
            if (uri.Length==0) {
                uri = env.getDefaultElementNamespace();
            }
            string local = sq.getLocalPart();
            string qname = sq.getDisplayName();
            bool builtInNamespace = uri.Equals(Saxon.Hej.lib.NamespaceConstant.SCHEMA);
            if (builtInNamespace) {
                Saxon.Hej.type.ItemType t = Saxon.Hej.type.Type.getBuiltInItemType(uri, local);
                if ((t == null)) {
                    grumble(("Unknown atomic type "+qname), "XPST0051");
                    System.Diagnostics.Debug.Assert(false);
                }
                if ((t is Saxon.Hej.type.BuiltInAtomicType)) {
                    checkAllowedType(env, (Saxon.Hej.type.BuiltInAtomicType)(t));
                    return t;
                } else {
                    if (t.isPlainType()) {
                        return t;
                    } else {
                        grumble((("The type "+qname)+" is not atomic"), "XPST0051");
                        System.Diagnostics.Debug.Assert(false);
                    }
                }
            } else {
                if (uri.Equals(Saxon.Hej.lib.NamespaceConstant.DOT_NET_TYPE)) {
                    return Saxon.Hej.Version.platform.getExternalObjectType(config, uri, local);
                } else {
                    Saxon.Hej.type.SchemaType st = config.getSchemaType(sq);
                    if ((st == null)) {
                        grumble(("Unknown simple type "+qname), "XPST0051");
                    } else {
                        if (st.isAtomicType()) {
                            if (!(env.isImportedSchema(uri))) {
                                grumble((("Atomic type "+qname)+" exists, but its schema definition has not been imported"), "XPST0051");
                            }
                            return (Saxon.Hej.type.AtomicType)(st);
                        } else {
                            if ((((st is Saxon.Hej.type.ItemType) && ((Saxon.Hej.type.ItemType)(st)).isPlainType()) && allowXPath30Syntax)) {
                                if (!(env.isImportedSchema(uri))) {
                                    grumble((("Type "+qname)+" exists, but its schema definition has not been imported"), "XPST0051");
                                }
                                return (Saxon.Hej.type.ItemType)(st);
                            } else {
                                if (st.isComplexType()) {
                                    grumble((("Type ("+qname)+") is a complex type"), "XPST0051");
                                    return Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC;
                                } else {
                                    if (((Saxon.Hej.type.SimpleType)(st)).isListType()) {
                                        grumble((("Type ("+qname)+") is a list type"), "XPST0051");
                                        return Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC;
                                    } else {
                                        if (allowXPath30Syntax) {
                                            grumble((("Type ("+qname)+") is a union type that cannot be used as an item type"), "XPST0051");
                                            return Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC;
                                        } else {
                                            grumble((("The union type ("+qname)+") cannot be used as an item type unless XPath 3.0 is enabled"), "XPST0051");
                                            return Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            grumble(("Unknown atomic type "+qname), "XPST0051");
            return Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC;
        }
        private void checkAllowedType(Saxon.Hej.expr.StaticContext env, Saxon.Hej.type.BuiltInAtomicType type) {
            string s = whyDisallowedType(env.getPackageData(), type);
            if ((s != null)) {
                grumble(s, "XPST0080");
            }
        }
        public static string whyDisallowedType(Saxon.Hej.expr.PackageData pack, Saxon.Hej.type.BuiltInAtomicType type) {
            if ((!(type.isAllowedInXSD10()) && (pack.getConfiguration().getXsdVersion() == Saxon.Hej.Configuration.XSD10))) {
                return (("The built-in atomic type "+type.getDisplayName())+" is not recognized unless XSD 1.1 is enabled");
            }
            return null;
        }
        private Saxon.Hej.type.CastingTarget getSimpleType(string qname) {
            if (scanOnly) {
                return Saxon.Hej.type.BuiltInAtomicType.STRING;
            }
            Saxon.Hej.om.StructuredQName sq = null;
            try {
                sq = qNameParser.parse(qname, env.getDefaultElementNamespace());
            } catch (Saxon.Hej.trans.XPathException e) {
                grumble(e.Message, e.getErrorCodeLocalPart());
                System.Diagnostics.Debug.Assert(false);
            }
            string uri = sq.getURI();
            string local = sq.getLocalPart();
            bool builtInNamespace = uri.Equals(Saxon.Hej.lib.NamespaceConstant.SCHEMA);
            if (builtInNamespace) {
                Saxon.Hej.type.SimpleType target = Saxon.Hej.type.Type.getBuiltInSimpleType(uri, local);
                if ((target == null)) {
                    grumble(("Unknown simple type "+qname), (allowXPath30Syntax ? "XQST0052" : "XPST0051"));
                } else {
                    if (!(((target is Saxon.Hej.type.CastingTarget)))) {
                        grumble(("Unsuitable type for cast: "+target.getDescription()), "XPST0080");
                    }
                }
                Saxon.Hej.type.CastingTarget t = (Saxon.Hej.type.CastingTarget)(target);
                if ((t is Saxon.Hej.type.BuiltInAtomicType)) {
                    checkAllowedType(env, (Saxon.Hej.type.BuiltInAtomicType)(t));
                }
                return t;
            } else {
                if (uri.Equals(Saxon.Hej.lib.NamespaceConstant.DOT_NET_TYPE)) {
                    return (Saxon.Hej.type.AtomicType)(Saxon.Hej.Version.platform.getExternalObjectType(env.getConfiguration(), uri, local));
                } else {
                    Saxon.Hej.type.SchemaType st = env.getConfiguration().getSchemaType(new Saxon.Hej.om.StructuredQName("", uri, local));
                    if ((st == null)) {
                        if (allowXPath30Syntax) {
                            grumble(("Unknown simple type "+qname), "XQST0052");
                        } else {
                            grumble(("Unknown simple type "+qname), "XPST0051");
                        }
                        return Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC;
                    }
                    if (allowXPath30Syntax) {
                        if (!(env.isImportedSchema(uri))) {
                            grumble((("Simple type "+qname)+" exists, but its target namespace has not been imported in the static context"));
                        }
                        return (Saxon.Hej.type.CastingTarget)(st);
                    } else {
                        if (st.isAtomicType()) {
                            if (!(env.isImportedSchema(uri))) {
                                grumble((("Atomic type "+qname)+" exists, but its target namespace has not been imported in the static context"));
                            }
                            return (Saxon.Hej.type.AtomicType)(st);
                        } else {
                            if (st.isComplexType()) {
                                grumble((("Cannot cast to a complex type ("+qname)+")"), "XPST0051");
                                return Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC;
                            } else {
                                if (((Saxon.Hej.type.SimpleType)(st)).isListType()) {
                                    grumble((("Casting to a list type ("+qname)+") requires XPath 3.0"), "XPST0051");
                                    return Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC;
                                } else {
                                    grumble((("casting to a union type ("+qname)+") requires XPath 3.0"), "XPST0051");
                                    return Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC;
                                }
                            }
                        }
                    }
                }
            }
        }
        public Saxon.Hej.value.SequenceType parseSequenceType() {
            bool disallowIndicator = t.currentTokenValue.Equals("empty-sequence");
            Saxon.Hej.type.ItemType primaryType = parseItemType();
            if (disallowIndicator) {
                return Saxon.Hej.value.SequenceType.makeSequenceType(primaryType, Saxon.Hej.expr.StaticProperty.EMPTY);
            }
            int occurrenceFlag = parseOccurrenceIndicator();
            return Saxon.Hej.value.SequenceType.makeSequenceType(primaryType, occurrenceFlag);
        }
        public int parseOccurrenceIndicator() {
            int occurrenceFlag;
            switch (t.currentToken) {
              case Saxon.Hej.expr.parser.Token.STAR:
              case Saxon.Hej.expr.parser.Token.MULT:
                occurrenceFlag = Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE;
                t.currentToken = Saxon.Hej.expr.parser.Token.RPAR;
                nextToken();
                break ;
              case Saxon.Hej.expr.parser.Token.PLUS:
                occurrenceFlag = Saxon.Hej.expr.StaticProperty.ALLOWS_ONE_OR_MORE;
                t.currentToken = Saxon.Hej.expr.parser.Token.RPAR;
                nextToken();
                break ;
              case Saxon.Hej.expr.parser.Token.QMARK:
                occurrenceFlag = Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_ONE;
                t.currentToken = Saxon.Hej.expr.parser.Token.RPAR;
                nextToken();
                break ;
              default:
                occurrenceFlag = Saxon.Hej.expr.StaticProperty.EXACTLY_ONE;
                break ;
            }
            return occurrenceFlag;
        }
        public Saxon.Hej.type.ItemType parseItemType() {
            Saxon.Hej.type.ItemType extended = parserExtension.parseExtendedItemType(this);
            return ((extended == null) ? parseSimpleItemType() : extended);
        }
        private Saxon.Hej.type.ItemType parseSimpleItemType() {
            Saxon.Hej.type.ItemType primaryType;
            if ((t.currentToken == Saxon.Hej.expr.parser.Token.LPAR)) {
                primaryType = parseParenthesizedItemType();
            } else {
                if ((t.currentToken == Saxon.Hej.expr.parser.Token.NAME)) {
                    primaryType = getPlainType(t.currentTokenValue);
                    nextToken();
                } else {
                    if ((t.currentToken == Saxon.Hej.expr.parser.Token.NODEKIND)) {
                        switch (t.currentTokenValue) {
                          case "item":
                            nextToken();
                            expect(Saxon.Hej.expr.parser.Token.RPAR);
                            nextToken();
                            primaryType = Saxon.Hej.type.AnyItemType.getInstance();
                            break ;
                          case "function":
                            checkLanguageVersion30();
                            Saxon.Hej.query.AnnotationList annotations = Saxon.Hej.query.AnnotationList.EMPTY;
                            primaryType = parseFunctionItemType(annotations);
                            break ;
                          case "map":
                            primaryType = parseMapItemType();
                            break ;
                          case "array":
                            primaryType = parseArrayItemType();
                            break ;
                          case "record":
                          case "tuple":
                            primaryType = parseRecordTest(this);
                            break ;
                          case "atomic":
                            nextToken();
                            expect(Saxon.Hej.expr.parser.Token.NAME);
                            Saxon.Hej.om.StructuredQName typeName = getQNameParser().parse(t.currentTokenValue, Saxon.Hej.lib.NamespaceConstant.SCHEMA);
                            primaryType = getPlainType(typeName);
                            if (!(((primaryType is Saxon.Hej.type.AtomicType)))) {
                                grumble((("Type "+t.currentTokenValue)+" exists, but is not atomic"));
                            }
                            nextToken();
                            expect(Saxon.Hej.expr.parser.Token.RPAR);
                            nextToken();
                            break ;
                          case "union":
                            primaryType = parseUnionType();
                            break ;
                          case "enum":
                            primaryType = parseEnumType();
                            break ;
                          case "empty-sequence":
                            nextToken();
                            expect(Saxon.Hej.expr.parser.Token.RPAR);
                            nextToken();
                            primaryType = Saxon.Hej.type.ErrorType.getInstance();
                            break ;
                          case "type":
                            checkLanguageVersion40();
                            nextToken();
                            expect(Saxon.Hej.expr.parser.Token.NAME);
                            Saxon.Hej.om.StructuredQName typeAlias = getQNameParser().parse(t.currentTokenValue, "");
                            Saxon.Hej.type.ItemType realType = getStaticContext().resolveTypeAlias(typeAlias);
                            if ((realType == null)) {
                                grumble((("Named type ("+typeAlias.getDisplayName())+") has not been defined"));
                            }
                            nextToken();
                            expect(Saxon.Hej.expr.parser.Token.RPAR);
                            nextToken();
                            return realType;
                          default:
                            primaryType = parseKindTest();
                            break ;
                        }
                    } else {
                        if ((t.currentToken == Saxon.Hej.expr.parser.Token.PERCENT)) {
                            Saxon.Hej.query.AnnotationList annotations = parseAnnotationsList();
                            if (t.currentTokenValue.Equals("function")) {
                                primaryType = parseFunctionItemType(annotations);
                            } else {
                                grumble(("Expected 'function' to follow annotation assertions, found "+Saxon.Hej.expr.parser.Token.tokens[t.currentToken]));
                                return null;
                            }
                        } else {
                            if (((language == Saxon.Hej.expr.parser.XPathParser.ParsedLanguage.EXTENDED_ITEM_TYPE) && (t.currentToken == Saxon.Hej.expr.parser.Token.PREFIX))) {
                                string tokv = t.currentTokenValue;
                                nextToken();
                                return makeNamespaceTest(Saxon.Hej.type.Type.ELEMENT, tokv);
                            } else {
                                if (((language == Saxon.Hej.expr.parser.XPathParser.ParsedLanguage.EXTENDED_ITEM_TYPE) && (t.currentToken == Saxon.Hej.expr.parser.Token.SUFFIX))) {
                                    nextToken();
                                    expect(Saxon.Hej.expr.parser.Token.NAME);
                                    string tokv = t.currentTokenValue;
                                    nextToken();
                                    return makeLocalNameTest(Saxon.Hej.type.Type.ELEMENT, tokv);
                                } else {
                                    if (((language == Saxon.Hej.expr.parser.XPathParser.ParsedLanguage.EXTENDED_ITEM_TYPE) && (t.currentToken == Saxon.Hej.expr.parser.Token.AT))) {
                                        nextToken();
                                        if ((t.currentToken == Saxon.Hej.expr.parser.Token.PREFIX)) {
                                            string tokv = t.currentTokenValue;
                                            nextToken();
                                            return makeNamespaceTest(Saxon.Hej.type.Type.ATTRIBUTE, tokv);
                                        } else {
                                            if ((t.currentToken == Saxon.Hej.expr.parser.Token.SUFFIX)) {
                                                nextToken();
                                                expect(Saxon.Hej.expr.parser.Token.NAME);
                                                string tokv = t.currentTokenValue;
                                                nextToken();
                                                return makeLocalNameTest(Saxon.Hej.type.Type.ATTRIBUTE, tokv);
                                            } else {
                                                grumble("Expected NodeTest after '@'");
                                                return Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC;
                                            }
                                        }
                                    } else {
                                        grumble(("Expected type name in SequenceType, found "+Saxon.Hej.expr.parser.Token.tokens[t.currentToken]));
                                        return Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return primaryType;
        }
        private Saxon.Hej.type.ItemType parseRecordTest(Saxon.Hej.expr.parser.XPathParser p) {
            checkLanguageVersion40();
            Saxon.Hej.expr.parser.Tokenizer t = p.getTokenizer();
            p.nextToken();
            System.Collections.Generic.IList<string> fieldNames = new System.Collections.Generic.List<string>(6);
            System.Collections.Generic.IList<string> optionalFieldNames = new System.Collections.Generic.List<string>(6);
            System.Collections.Generic.IList<Saxon.Hej.value.SequenceType> fieldTypes = new System.Collections.Generic.List<Saxon.Hej.value.SequenceType>(6);
            bool extensible = false;
            Saxon.Hej.ma.map.RecordTest recordTest = new Saxon.Hej.ma.map.RecordTest();
            while (true) {
                string name;
                if ((t.currentToken == Saxon.Hej.expr.parser.Token.NAME)) {
                    name = t.currentTokenValue;
                    if (!(Saxon.Hej.om.NameChecker.isValidNCName(name))) {
                        p.grumble((Saxon.Hej.trans.Err.wrap(name)+" is not a valid NCName"));
                    }
                } else {
                    if ((t.currentToken == Saxon.Hej.expr.parser.Token.STRING_LITERAL)) {
                        name = t.currentTokenValue;
                    } else {
                        p.grumble("Name of field in tuple must be either an NCName or a quoted string literal");
                        name = "dummy";
                    }
                }
                if (fieldNames.Contains(name)) {
                    p.grumble((("Duplicate field name ("+name)+")"));
                    name = "dummy";
                }
                fieldNames.Add(name);
                p.nextToken();
                if ((t.currentToken == Saxon.Hej.expr.parser.Token.QMARK)) {
                    optionalFieldNames.Add(name);
                    p.nextToken();
                }
                Saxon.Hej.value.SequenceType arg = Saxon.Hej.value.SequenceType.NON_EMPTY_SEQUENCE;
                if ((t.currentToken == Saxon.Hej.expr.parser.Token.AS)) {
                    p.nextToken();
                    if ((t.currentToken == Saxon.Hej.expr.parser.Token.DOTDOT)) {
                        p.nextToken();
                        int occ = parseOccurrenceIndicator();
                        arg = Saxon.Hej.value.SequenceType.makeSequenceType(recordTest, occ);
                        if ((!(Saxon.Hej.value.Cardinality.allowsZero(occ)) && !(optionalFieldNames.Contains(name)))) {
                            throw new Saxon.Hej.trans.XPathException("A self-referencing field in a record type must be emptiable or optional", "XPST0140");
                        }
                    } else {
                        arg = p.parseSequenceType();
                    }
                }
                fieldTypes.Add(arg);
                if ((t.currentToken == Saxon.Hej.expr.parser.Token.RPAR)) {
                    break ;
                } else {
                    if ((t.currentToken == Saxon.Hej.expr.parser.Token.COMMA)) {
                        p.nextToken();
                        if (((t.currentToken == Saxon.Hej.expr.parser.Token.STAR) || (t.currentToken == Saxon.Hej.expr.parser.Token.MULT))) {
                            extensible = true;
                            p.nextToken();
                            p.expect(Saxon.Hej.expr.parser.Token.RPAR);
                            break ;
                        }
                    } else {
                        p.grumble((("Expected ',' or ')' after field in RecordTest, found '"+Saxon.Hej.expr.parser.Token.tokens[t.currentToken])+'\''));
                    }
                }
            }
            p.nextToken();
            if (fieldNames.Count==0) {
                p.grumble("RecordTest must contain at least one field");
                return null;
            }
            recordTest.setDetails(fieldNames, fieldTypes, optionalFieldNames, extensible);
            return recordTest;
        }
        public Saxon.Hej.type.ItemType parseUnionType() {
            checkLanguageVersion40();
            nextToken();
            System.Collections.Generic.IList<Saxon.Hej.type.AtomicType> memberTypes = new System.Collections.Generic.List<Saxon.Hej.type.AtomicType>(6);
            while (true) {
                if (((t.currentToken == Saxon.Hej.expr.parser.Token.NODEKIND) && t.currentTokenValue.Equals("enum"))) {
                    Saxon.Hej.type.EnumerationType type = parseEnumType();
                    memberTypes.Add(type);
                } else {
                    expect(Saxon.Hej.expr.parser.Token.NAME);
                    Saxon.Hej.om.StructuredQName member = getQNameParser().parse(t.currentTokenValue, getStaticContext().getDefaultElementNamespace());
                    Saxon.Hej.type.ItemType type = getPlainType(member);
                    if ((type is Saxon.Hej.type.AtomicType)) {
                        memberTypes.Add((Saxon.Hej.type.AtomicType)(type));
                    } else {
                        if ((type is Saxon.Hej.type.PlainType)) {
                            foreach (Saxon.Hej.type.PlainType pt in ((Saxon.Hej.type.UnionType)(type)).getPlainMemberTypes()) {
                                if ((pt is Saxon.Hej.type.AtomicType)) {
                                    memberTypes.Add((Saxon.Hej.type.AtomicType)(pt));
                                } else {
                                    grumble(((("Union type "+type)+" has a non-atomic member type ")+pt));
                                }
                            }
                        } else {
                            grumble((("Type "+t.currentTokenValue)+" exists, but is not atomic"));
                        }
                    }
                    nextToken();
                }
                if ((t.currentToken == Saxon.Hej.expr.parser.Token.RPAR)) {
                    break ;
                } else {
                    if ((t.currentToken == Saxon.Hej.expr.parser.Token.COMMA)) {
                        nextToken();
                    } else {
                        grumble((("Expected ',' or ')' after member name in union type, found '"+Saxon.Hej.expr.parser.Token.tokens[t.currentToken])+'\''));
                    }
                }
            }
            nextToken();
            return new Saxon.Hej.type.LocalUnionType(memberTypes);
        }
        public Saxon.Hej.type.EnumerationType parseEnumType() {
            checkLanguageVersion40();
            nextToken();
            System.Collections.Generic.ISet<string> values = new System.Collections.Generic.HashSet<string>(6);
            while (true) {
                expect(Saxon.Hej.expr.parser.Token.STRING_LITERAL);
                values.Add(t.currentTokenValue);
                nextToken();
                if ((t.currentToken == Saxon.Hej.expr.parser.Token.RPAR)) {
                    break ;
                } else {
                    if ((t.currentToken == Saxon.Hej.expr.parser.Token.COMMA)) {
                        nextToken();
                    } else {
                        grumble((("Expected ',' or ')' after string literal in enum type, found '"+Saxon.Hej.expr.parser.Token.tokens[t.currentToken])+'\''));
                    }
                }
            }
            nextToken();
            return new Saxon.Hej.type.EnumerationType(values);
        }
        protected internal Saxon.Hej.type.ItemType parseFunctionItemType(Saxon.Hej.query.AnnotationList annotations) {
            return parserExtension.parseFunctionItemType(this, annotations);
        }
        protected internal Saxon.Hej.type.ItemType parseMapItemType() {
            checkMapExtensions();
            Saxon.Hej.expr.parser.Tokenizer t = getTokenizer();
            nextToken();
            if (((t.currentToken == Saxon.Hej.expr.parser.Token.STAR) || (t.currentToken == Saxon.Hej.expr.parser.Token.MULT))) {
                nextToken();
                expect(Saxon.Hej.expr.parser.Token.RPAR);
                nextToken();
                return Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE;
            } else {
                Saxon.Hej.type.ItemType keyType = parseItemType();
                expect(Saxon.Hej.expr.parser.Token.COMMA);
                nextToken();
                Saxon.Hej.value.SequenceType valueType = parseSequenceType();
                expect(Saxon.Hej.expr.parser.Token.RPAR);
                nextToken();
                if (!(((keyType is Saxon.Hej.type.AtomicType)))) {
                    grumble("Key type of a map must be atomic");
                    return null;
                }
                return new Saxon.Hej.ma.map.MapType((Saxon.Hej.type.AtomicType)(keyType), valueType);
            }
        }
        protected internal Saxon.Hej.type.ItemType parseArrayItemType() {
            checkLanguageVersion31();
            Saxon.Hej.expr.parser.Tokenizer t = getTokenizer();
            nextToken();
            if (((t.currentToken == Saxon.Hej.expr.parser.Token.STAR) || (t.currentToken == Saxon.Hej.expr.parser.Token.MULT))) {
                nextToken();
                expect(Saxon.Hej.expr.parser.Token.RPAR);
                nextToken();
                return Saxon.Hej.ma.arrays.ArrayItemType.ANY_ARRAY_TYPE;
            } else {
                Saxon.Hej.value.SequenceType memberType = parseSequenceType();
                expect(Saxon.Hej.expr.parser.Token.RPAR);
                nextToken();
                return new Saxon.Hej.ma.arrays.ArrayItemType(memberType);
            }
        }
        private Saxon.Hej.type.ItemType parseParenthesizedItemType() {
            if (!(allowXPath30Syntax)) {
                grumble("Parenthesized item types require 3.0 to be enabled");
            }
            nextToken();
            Saxon.Hej.type.ItemType primaryType = parseItemType();
            while ((((primaryType is Saxon.Hej.pattern.NodeTest) && (language == Saxon.Hej.expr.parser.XPathParser.ParsedLanguage.EXTENDED_ITEM_TYPE)) && (t.currentToken != Saxon.Hej.expr.parser.Token.RPAR))) {
                switch (t.currentToken) {
                  case Saxon.Hej.expr.parser.Token.UNION:
                  case Saxon.Hej.expr.parser.Token.EXCEPT:
                  case Saxon.Hej.expr.parser.Token.INTERSECT:
                    int op = t.currentToken;
                    nextToken();
                    primaryType = new Saxon.Hej.pattern.CombinedNodeTest((Saxon.Hej.pattern.NodeTest)(primaryType), op, (Saxon.Hej.pattern.NodeTest)(parseItemType()));
                    break ;
                }
            }
            expect(Saxon.Hej.expr.parser.Token.RPAR);
            nextToken();
            return primaryType;
        }
        private Saxon.Hej.expr.Expression parseUnaryExpression() {
            Saxon.Hej.expr.Expression exp;
            switch (t.currentToken) {
              case Saxon.Hej.expr.parser.Token.MINUS:
                {
                    nextToken();
                    Saxon.Hej.expr.Expression operand = parseUnaryExpression();
                    exp = makeUnaryExpression(Saxon.Hej.expr.parser.Token.NEGATE, operand);
                    break ;
                }
              case Saxon.Hej.expr.parser.Token.PLUS:
                {
                    nextToken();
                    Saxon.Hej.expr.Expression operand = parseUnaryExpression();
                    exp = makeUnaryExpression(Saxon.Hej.expr.parser.Token.PLUS, operand);
                    break ;
                }
              case Saxon.Hej.expr.parser.Token.VALIDATE:
              case Saxon.Hej.expr.parser.Token.VALIDATE_STRICT:
              case Saxon.Hej.expr.parser.Token.VALIDATE_LAX:
              case Saxon.Hej.expr.parser.Token.VALIDATE_TYPE:
                exp = parseValidateExpression();
                break ;
              case Saxon.Hej.expr.parser.Token.PRAGMA:
                exp = parseExtensionExpression();
                break ;
              case Saxon.Hej.expr.parser.Token.KEYWORD_CURLY:
                if (t.currentTokenValue.Equals("validate")) {
                    exp = parseValidateExpression();
                    break ;
                }
                goto default;
              default:
                exp = parseSimpleMappingExpression();
                break ;
            }
            setLocation(exp);
            return exp;
        }
        private Saxon.Hej.expr.Expression makeUnaryExpression(int @operator, Saxon.Hej.expr.Expression operand) {
            if (Saxon.Hej.expr.Literal.isAtomic(operand)) {
                Saxon.Hej.value.AtomicValue val = (Saxon.Hej.value.AtomicValue)(((Saxon.Hej.expr.Literal)(operand)).getGroundedValue());
                if ((val is Saxon.Hej.value.NumericValue)) {
                    if (env.isInBackwardsCompatibleMode()) {
                        val = new Saxon.Hej.value.DoubleValue(((Saxon.Hej.value.NumericValue)(val)).getDoubleValue());
                    }
                    Saxon.Hej.value.AtomicValue value = ((@operator == Saxon.Hej.expr.parser.Token.NEGATE) ? ((Saxon.Hej.value.NumericValue)(val)).negate() : (Saxon.Hej.value.NumericValue)(val));
                    return Saxon.Hej.expr.Literal.makeLiteral(value);
                }
            }
            return env.getConfiguration().getTypeChecker(env.isInBackwardsCompatibleMode()).makeArithmeticExpression(Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.Int64Value.ZERO), @operator, operand);
        }
        protected internal virtual bool atStartOfRelativePath() {
            switch (t.currentToken) {
              case Saxon.Hej.expr.parser.Token.AXIS:
              case Saxon.Hej.expr.parser.Token.AT:
              case Saxon.Hej.expr.parser.Token.NAME:
              case Saxon.Hej.expr.parser.Token.PREFIX:
              case Saxon.Hej.expr.parser.Token.SUFFIX:
              case Saxon.Hej.expr.parser.Token.STAR:
              case Saxon.Hej.expr.parser.Token.NODEKIND:
              case Saxon.Hej.expr.parser.Token.DOT:
              case Saxon.Hej.expr.parser.Token.DOTDOT:
              case Saxon.Hej.expr.parser.Token.FUNCTION:
              case Saxon.Hej.expr.parser.Token.STRING_LITERAL:
              case Saxon.Hej.expr.parser.Token.NUMBER:
              case Saxon.Hej.expr.parser.Token.LPAR:
              case Saxon.Hej.expr.parser.Token.DOLLAR:
              case Saxon.Hej.expr.parser.Token.PRAGMA:
              case Saxon.Hej.expr.parser.Token.ELEMENT_QNAME:
              case Saxon.Hej.expr.parser.Token.ATTRIBUTE_QNAME:
              case Saxon.Hej.expr.parser.Token.PI_QNAME:
              case Saxon.Hej.expr.parser.Token.NAMESPACE_QNAME:
              case Saxon.Hej.expr.parser.Token.NAMED_FUNCTION_REF:
                return true;
              case Saxon.Hej.expr.parser.Token.KEYWORD_CURLY:
                return (t.currentTokenValue.Equals("ordered") || t.currentTokenValue.Equals("unordered"));
              default:
                return false;
            }
        }
        protected internal bool disallowedAtStartOfRelativePath() {
            switch (t.currentToken) {
              case Saxon.Hej.expr.parser.Token.CAST_AS:
              case Saxon.Hej.expr.parser.Token.CASTABLE_AS:
              case Saxon.Hej.expr.parser.Token.INSTANCE_OF:
              case Saxon.Hej.expr.parser.Token.TREAT_AS:
                return true;
              default:
                return false;
            }
        }
        protected internal Saxon.Hej.expr.Expression parsePathExpression() {
            int offset = t.currentTokenStartOffset;
            switch (t.currentToken) {
              case Saxon.Hej.expr.parser.Token.SLASH:
                nextToken();
                Saxon.Hej.expr.RootExpression start = new Saxon.Hej.expr.RootExpression();
                setLocation(start);
                if (disallowedAtStartOfRelativePath()) {
                    grumble((("Operator '"+Saxon.Hej.expr.parser.Token.tokens[t.currentToken])+"' is not allowed after '/'"));
                }
                if (atStartOfRelativePath()) {
                    Saxon.Hej.expr.Expression path = parseRemainingPath(start);
                    setLocation(path, offset);
                    return path;
                } else {
                    return start;
                }
              case Saxon.Hej.expr.parser.Token.SLASH_SLASH:
                nextToken();
                Saxon.Hej.expr.RootExpression start2 = new Saxon.Hej.expr.RootExpression();
                setLocation(start2, offset);
                Saxon.Hej.expr.AxisExpression axisExp = new Saxon.Hej.expr.AxisExpression(Saxon.Hej.om.AxisInfo.DESCENDANT_OR_SELF, null);
                setLocation(axisExp, offset);
                Saxon.Hej.expr.Expression slashExp = Saxon.Hej.expr.parser.ExpressionTool.makePathExpression(start2, axisExp);
                setLocation(slashExp, offset);
                Saxon.Hej.expr.Expression exp = parseRemainingPath(slashExp);
                setLocation(exp, offset);
                return exp;
              default:
                if (((t.currentToken == Saxon.Hej.expr.parser.Token.NAME) && ((t.currentTokenValue.Equals("true") || t.currentTokenValue.Equals("false"))))) {
                    warning((((((((("The expression is looking for a child element named '"+t.currentTokenValue)+"' - perhaps ")+t.currentTokenValue)+"() was intended? To avoid this warning, use child::")+t.currentTokenValue)+" or ./")+t.currentTokenValue)+"."));
                }
                if (((((t.currentToken == Saxon.Hej.expr.parser.Token.NAME) && (t.getBinaryOp(t.currentTokenValue) != Saxon.Hej.expr.parser.Token.UNKNOWN)) && (language != Saxon.Hej.expr.parser.XPathParser.ParsedLanguage.XSLT_PATTERN)) && (((offset>0) || (t.peekAhead() != Saxon.Hej.expr.parser.Token.EOF))))) {
                    string s = t.currentTokenValue;
                    warning((((((((("The keyword '"+s)+"' in this context means 'child::")+s)+"'. If this was intended, use 'child::")+s)+"' or './")+s)+"' to avoid this warning."));
                }
                return parseRelativePath();
            }
        }
        protected internal Saxon.Hej.expr.Expression parseSimpleMappingExpression() {
            int offset = t.currentTokenStartOffset;
            Saxon.Hej.expr.Expression exp = parsePathExpression();
            while ((t.currentToken == Saxon.Hej.expr.parser.Token.BANG)) {
                if (!(allowXPath30Syntax)) {
                    grumble("XPath '!' operator requires XPath 3.0 to be enabled");
                }
                nextToken();
                Saxon.Hej.expr.Expression next = parsePathExpression();
                exp = new Saxon.Hej.expr.instruct.ForEach(exp, next);
                setLocation(exp, offset);
            }
            return exp;
        }
        protected internal virtual Saxon.Hej.expr.Expression parseRelativePath() {
            int offset = t.currentTokenStartOffset;
            Saxon.Hej.expr.Expression exp = parseStepExpression((language == Saxon.Hej.expr.parser.XPathParser.ParsedLanguage.XSLT_PATTERN));
            while (((t.currentToken == Saxon.Hej.expr.parser.Token.SLASH) || (t.currentToken == Saxon.Hej.expr.parser.Token.SLASH_SLASH))) {
                int op = t.currentToken;
                nextToken();
                Saxon.Hej.expr.Expression next = parseStepExpression(false);
                if ((op == Saxon.Hej.expr.parser.Token.SLASH)) {
                    exp = new Saxon.Hej.expr.HomogeneityChecker(new Saxon.Hej.expr.SlashExpression(exp, next));
                } else {
                    Saxon.Hej.expr.AxisExpression ae = new Saxon.Hej.expr.AxisExpression(Saxon.Hej.om.AxisInfo.DESCENDANT_OR_SELF, null);
                    setLocation(ae, offset);
                    Saxon.Hej.expr.Expression one = Saxon.Hej.expr.parser.ExpressionTool.makePathExpression(exp, ae);
                    setLocation(one, offset);
                    exp = Saxon.Hej.expr.parser.ExpressionTool.makePathExpression(one, next);
                    exp = new Saxon.Hej.expr.HomogeneityChecker(exp);
                }
                setLocation(exp, offset);
            }
            return exp;
        }
        protected internal Saxon.Hej.expr.Expression parseRemainingPath(Saxon.Hej.expr.Expression start) {
            int offset = t.currentTokenStartOffset;
            Saxon.Hej.expr.Expression exp = start;
            int op = Saxon.Hej.expr.parser.Token.SLASH;
            while (true) {
                Saxon.Hej.expr.Expression next = parseStepExpression(false);
                if ((op == Saxon.Hej.expr.parser.Token.SLASH)) {
                    exp = new Saxon.Hej.expr.HomogeneityChecker(new Saxon.Hej.expr.SlashExpression(exp, next));
                } else {
                    if ((op == Saxon.Hej.expr.parser.Token.SLASH_SLASH)) {
                        Saxon.Hej.expr.AxisExpression descOrSelf = new Saxon.Hej.expr.AxisExpression(Saxon.Hej.om.AxisInfo.DESCENDANT_OR_SELF, null);
                        setLocation(descOrSelf);
                        Saxon.Hej.expr.Expression step = Saxon.Hej.expr.parser.ExpressionTool.makePathExpression(descOrSelf, next);
                        setLocation(step);
                        exp = Saxon.Hej.expr.parser.ExpressionTool.makePathExpression(exp, step);
                        exp = new Saxon.Hej.expr.HomogeneityChecker(exp);
                    } else {
                        if (!(allowXPath30Syntax)) {
                            grumble("XPath '!' operator requires XPath 3.0 to be enabled");
                        }
                        exp = new Saxon.Hej.expr.instruct.ForEach(exp, next);
                    }
                }
                setLocation(exp, offset);
                op = t.currentToken;
                if ((((op != Saxon.Hej.expr.parser.Token.SLASH) && (op != Saxon.Hej.expr.parser.Token.SLASH_SLASH)) && (op != Saxon.Hej.expr.parser.Token.BANG))) {
                    break ;
                }
                nextToken();
            }
            return exp;
        }
        protected internal Saxon.Hej.expr.Expression parseStepExpression(bool firstInPattern) {
            Saxon.Hej.expr.Expression step = parseBasicStep(firstInPattern);
            bool reverse = (((step is Saxon.Hej.expr.AxisExpression)) && !(Saxon.Hej.om.AxisInfo.isForwards[((Saxon.Hej.expr.AxisExpression)(step)).getAxis()]));
            while (true) {
                if ((t.currentToken == Saxon.Hej.expr.parser.Token.LSQB)) {
                    step = parsePredicate(step);
                } else {
                    if ((t.currentToken == Saxon.Hej.expr.parser.Token.LPAR)) {
                        step = parseDynamicFunctionCall(step, null);
                        setLocation(step);
                    } else {
                        if ((t.currentToken == Saxon.Hej.expr.parser.Token.QMARK)) {
                            step = parseLookup(step);
                            setLocation(step);
                        } else {
                            break ;
                        }
                    }
                }
            }
            if (reverse) {
                Saxon.Hej.expr.parser.RetainedStaticContext rsc = env.makeRetainedStaticContext();
                step = Saxon.Hej.functions.SystemFunction.makeCall("reverse", rsc, step);
                System.Diagnostics.Debug.Assert((step != null));
                return step;
            } else {
                return step;
            }
        }
        protected internal Saxon.Hej.expr.Expression parsePredicate(Saxon.Hej.expr.Expression step) {
            nextToken();
            Saxon.Hej.expr.Expression predicate = parsePredicate();
            expect(Saxon.Hej.expr.parser.Token.RSQB);
            nextToken();
            step = new Saxon.Hej.expr.FilterExpression(step, predicate);
            setLocation(step);
            return step;
        }
        protected internal Saxon.Hej.expr.Expression parseArrowPostfix(Saxon.Hej.expr.Expression lhs) {
            checkLanguageVersion31();
            nextToken();
            int token = getTokenizer().currentToken;
            if (((token == Saxon.Hej.expr.parser.Token.NAME) || (token == Saxon.Hej.expr.parser.Token.FUNCTION))) {
                return parseFunctionCall(lhs);
            } else {
                if ((token == Saxon.Hej.expr.parser.Token.DOLLAR)) {
                    Saxon.Hej.expr.Expression var = parseVariableReference();
                    expect(Saxon.Hej.expr.parser.Token.LPAR);
                    return parseDynamicFunctionCall(var, lhs);
                } else {
                    if ((token == Saxon.Hej.expr.parser.Token.LPAR)) {
                        Saxon.Hej.expr.Expression var = parseParenthesizedExpression();
                        expect(Saxon.Hej.expr.parser.Token.LPAR);
                        return parseDynamicFunctionCall(var, lhs);
                    } else {
                        grumble((("Unexpected "+Saxon.Hej.expr.parser.Token.tokens[token])+" after '=>'"));
                        return null;
                    }
                }
            }
        }
        protected internal virtual Saxon.Hej.expr.Expression parsePredicate() {
            return parseExpression();
        }
        protected internal bool isReservedInQuery(string uri) {
            return Saxon.Hej.lib.NamespaceConstant.isReservedInQuery31(uri);
        }
        protected internal virtual Saxon.Hej.expr.Expression parseBasicStep(bool firstInPattern) {
            switch (t.currentToken) {
              case Saxon.Hej.expr.parser.Token.DOLLAR:
                return parseVariableReference();
              case Saxon.Hej.expr.parser.Token.LPAR:
                return parseParenthesizedExpression();
              case Saxon.Hej.expr.parser.Token.LSQB:
                return parseArraySquareConstructor();
              case Saxon.Hej.expr.parser.Token.STRING_LITERAL:
                return parseStringLiteral(true);
              case Saxon.Hej.expr.parser.Token.STRING_LITERAL_BACKTICKED:
                return parseStringLiteral(true);
              case Saxon.Hej.expr.parser.Token.STRING_CONSTRUCTOR_INITIAL:
                return parseStringConstructor();
              case Saxon.Hej.expr.parser.Token.NUMBER:
                return parseNumericLiteral(true);
              case Saxon.Hej.expr.parser.Token.FUNCTION:
                return parseFunctionCall(null);
              case Saxon.Hej.expr.parser.Token.QMARK:
                return parseLookup(new Saxon.Hej.expr.ContextItemExpression());
              case Saxon.Hej.expr.parser.Token.DOT:
                nextToken();
                Saxon.Hej.expr.Expression cie = new Saxon.Hej.expr.ContextItemExpression();
                setLocation(cie);
                return cie;
              case Saxon.Hej.expr.parser.Token.DOTDOT:
                nextToken();
                Saxon.Hej.expr.Expression pne = new Saxon.Hej.expr.AxisExpression(Saxon.Hej.om.AxisInfo.PARENT, null);
                setLocation(pne);
                return pne;
              case Saxon.Hej.expr.parser.Token.PERCENT:
                {
                    Saxon.Hej.query.AnnotationList annotations = parseAnnotationsList();
                    if ((t.currentToken == Saxon.Hej.expr.parser.Token.THIN_ARROW)) {
                        checkLanguageVersion40();
                        nextToken();
                        annotations.check(env.getConfiguration(), "IF");
                        if ((t.currentToken == Saxon.Hej.expr.parser.Token.LPAR)) {
                            return parseInlineFunction(annotations);
                        } else {
                            if ((t.currentToken == Saxon.Hej.expr.parser.Token.LCURLY)) {
                                return parseFocusFunction(annotations);
                            } else {
                                grumble("Expected '(' or '{' after '->'");
                            }
                        }
                    }
                    if (!(t.currentTokenValue.Equals("function"))) {
                        grumble("Expected 'function' to follow the annotation assertion");
                    }
                    annotations.check(env.getConfiguration(), "IF");
                    return parseInlineFunction(annotations);
                }
              case Saxon.Hej.expr.parser.Token.THIN_ARROW:
                {
                    checkLanguageVersion40();
                    nextToken();
                    if ((t.currentToken == Saxon.Hej.expr.parser.Token.LPAR)) {
                        Saxon.Hej.query.AnnotationList annotations = Saxon.Hej.query.AnnotationList.EMPTY;
                        return parseInlineFunction(annotations);
                    } else {
                        if ((t.currentToken == Saxon.Hej.expr.parser.Token.LCURLY)) {
                            Saxon.Hej.query.AnnotationList annotations = Saxon.Hej.query.AnnotationList.EMPTY;
                            return parseFocusFunction(annotations);
                        } else {
                            grumble("Expected '(' or '{' after '->'");
                        }
                    }
                    break ;
                }
              case Saxon.Hej.expr.parser.Token.NODEKIND:
                if (t.currentTokenValue.Equals("function")) {
                    Saxon.Hej.query.AnnotationList annotations = Saxon.Hej.query.AnnotationList.EMPTY;
                    return parseInlineFunction(annotations);
                }
                goto case Saxon.Hej.expr.parser.Token.NAME;
              case Saxon.Hej.expr.parser.Token.NAME:
              case Saxon.Hej.expr.parser.Token.PREFIX:
              case Saxon.Hej.expr.parser.Token.SUFFIX:
              case Saxon.Hej.expr.parser.Token.STAR:
                byte defaultAxis = Saxon.Hej.om.AxisInfo.CHILD;
                if (((t.currentToken == Saxon.Hej.expr.parser.Token.NODEKIND) && ((t.currentTokenValue.Equals("attribute") || t.currentTokenValue.Equals("schema-attribute"))))) {
                    defaultAxis = Saxon.Hej.om.AxisInfo.ATTRIBUTE;
                } else {
                    if (((t.currentToken == Saxon.Hej.expr.parser.Token.NODEKIND) && t.currentTokenValue.Equals("namespace-node"))) {
                        defaultAxis = Saxon.Hej.om.AxisInfo.NAMESPACE;
                        testPermittedAxis(Saxon.Hej.om.AxisInfo.NAMESPACE, "XQST0134");
                    } else {
                        if (((firstInPattern && (t.currentToken == Saxon.Hej.expr.parser.Token.NODEKIND)) && t.currentTokenValue.Equals("document-node"))) {
                            defaultAxis = Saxon.Hej.om.AxisInfo.SELF;
                        }
                    }
                }
                Saxon.Hej.pattern.NodeTest test = parseNodeTest(Saxon.Hej.type.Type.ELEMENT);
                if ((test is Saxon.Hej.pattern.AnyNodeTest)) {
                    if ((defaultAxis == Saxon.Hej.om.AxisInfo.CHILD)) {
                        test = Saxon.Hej.pattern.MultipleNodeKindTest.CHILD_NODE;
                    } else {
                        test = Saxon.Hej.pattern.NodeKindTest.ATTRIBUTE;
                    }
                }
                Saxon.Hej.expr.AxisExpression ae = new Saxon.Hej.expr.AxisExpression(defaultAxis, test);
                setLocation(ae);
                return ae;
              case Saxon.Hej.expr.parser.Token.AT:
                nextToken();
                switch (t.currentToken) {
                  case Saxon.Hej.expr.parser.Token.NAME:
                  case Saxon.Hej.expr.parser.Token.PREFIX:
                  case Saxon.Hej.expr.parser.Token.SUFFIX:
                  case Saxon.Hej.expr.parser.Token.STAR:
                  case Saxon.Hej.expr.parser.Token.NODEKIND:
                    Saxon.Hej.expr.AxisExpression ae2 = new Saxon.Hej.expr.AxisExpression(Saxon.Hej.om.AxisInfo.ATTRIBUTE, parseNodeTest(Saxon.Hej.type.Type.ATTRIBUTE));
                    setLocation(ae2);
                    return ae2;
                  default:
                    grumble("@ must be followed by a NodeTest");
                    break ;
                }
                break ;
              case Saxon.Hej.expr.parser.Token.AXIS:
                int axis;
                try {
                    axis = Saxon.Hej.om.AxisInfo.getAxisNumber(t.currentTokenValue);
                } catch (Saxon.Hej.trans.XPathException err) {
                    grumble(err.Message);
                    axis = Saxon.Hej.om.AxisInfo.CHILD;
                }
                testPermittedAxis(axis, "XPST0003");
                short principalNodeType = Saxon.Hej.om.AxisInfo.principalNodeType[axis];
                nextToken();
                switch (t.currentToken) {
                  case Saxon.Hej.expr.parser.Token.NAME:
                  case Saxon.Hej.expr.parser.Token.PREFIX:
                  case Saxon.Hej.expr.parser.Token.SUFFIX:
                  case Saxon.Hej.expr.parser.Token.STAR:
                  case Saxon.Hej.expr.parser.Token.NODEKIND:
                    Saxon.Hej.expr.Expression ax = new Saxon.Hej.expr.AxisExpression(axis, parseNodeTest(principalNodeType));
                    setLocation(ax);
                    return ax;
                  default:
                    grumble((("Unexpected token "+currentTokenDisplay())+" after axis name"));
                    break ;
                }
                break ;
              case Saxon.Hej.expr.parser.Token.KEYWORD_CURLY:
                switch (t.currentTokenValue) {
                  case "map":
                    return parseMapExpression();
                  case "array":
                    return parseArrayCurlyConstructor();
                  case "fn":
                  case ".":
                    warning("The experimental syntax '.{expr}' has changed to '->{expr}");
                    return parserExtension.parseDotFunction(this);
                  case "_":
                    return parserExtension.parseUnderscoreFunction(this);
                }
                goto case Saxon.Hej.expr.parser.Token.ELEMENT_QNAME;
              case Saxon.Hej.expr.parser.Token.ELEMENT_QNAME:
              case Saxon.Hej.expr.parser.Token.ATTRIBUTE_QNAME:
              case Saxon.Hej.expr.parser.Token.NAMESPACE_QNAME:
              case Saxon.Hej.expr.parser.Token.PI_QNAME:
              case Saxon.Hej.expr.parser.Token.TAG:
                return parseConstructor();
              case Saxon.Hej.expr.parser.Token.NAMED_FUNCTION_REF:
                return parseNamedFunctionReference();
              default:
                grumble((("Unexpected token "+currentTokenDisplay())+" at start of expression"));
                break ;
            }
            return new Saxon.Hej.expr.ErrorExpression();
        }
        public Saxon.Hej.expr.Expression parseParenthesizedExpression() {
            nextToken();
            if ((t.currentToken == Saxon.Hej.expr.parser.Token.RPAR)) {
                nextToken();
                return makeTracer(Saxon.Hej.expr.Literal.makeEmptySequence(), null);
            }
            Saxon.Hej.expr.Expression seq = parseExpression();
            expect(Saxon.Hej.expr.parser.Token.RPAR);
            nextToken();
            return seq;
        }
        protected internal virtual void testPermittedAxis(int axis, string errorCode) {
            if ((axis == Saxon.Hej.om.AxisInfo.PRECEDING_OR_ANCESTOR)) {
                grumble("The preceding-or-ancestor axis is for internal use only", errorCode);
            }
        }
        public Saxon.Hej.expr.Expression parseNumericLiteral(bool traceable) {
            int offset = t.currentTokenStartOffset;
            Saxon.Hej.value.NumericValue number = Saxon.Hej.value.NumericValue.parseNumber(t.currentTokenValue);
            if (number.isNaN()) {
                grumble(("Invalid numeric literal "+Saxon.Hej.trans.Err.wrap(t.currentTokenValue, Saxon.Hej.trans.Err.VALUE)));
            }
            nextToken();
            Saxon.Hej.expr.Literal lit = Saxon.Hej.expr.Literal.makeLiteral(number);
            setLocation(lit, offset);
            return (traceable ? makeTracer(lit, null) : lit);
        }
        protected internal Saxon.Hej.expr.Expression parseStringLiteral(bool traceable) {
            Saxon.Hej.expr.Literal literal = makeStringLiteral(t.currentTokenValue);
            nextToken();
            return (traceable ? makeTracer(literal, null) : literal);
        }
        protected internal virtual Saxon.Hej.expr.Expression parseStringConstructor() {
            grumble("String constructor expressions are allowed only in XQuery");
            return null;
        }
        public Saxon.Hej.expr.Expression parseVariableReference() {
            int offset = t.currentTokenStartOffset;
            nextToken();
            if ((t.currentToken == Saxon.Hej.expr.parser.Token.NUMBER)) {
                return parserExtension.bindNumericParameterReference(this);
            }
            expect(Saxon.Hej.expr.parser.Token.NAME);
            string var = t.currentTokenValue;
            nextToken();
            if (scanOnly) {
                return new Saxon.Hej.expr.ContextItemExpression();
            }
            Saxon.Hej.om.StructuredQName vtest = makeStructuredQName(var, "");
            System.Diagnostics.Debug.Assert((vtest != null));
            Saxon.Hej.expr.LocalBinding b = findRangeVariable(vtest);
            Saxon.Hej.expr.Expression @ref;
            if ((b != null)) {
                @ref = new Saxon.Hej.expr.LocalVariableReference(b);
            } else {
                if ((catchDepth>0)) {
                    foreach (Saxon.Hej.om.StructuredQName errorVariable in Saxon.Hej.om.StandardNames.errorVariables) {
                        if (errorVariable.getLocalPart().Equals(vtest.getLocalPart())) {
                            Saxon.Hej.om.StructuredQName functionName = new Saxon.Hej.om.StructuredQName("saxon", Saxon.Hej.lib.NamespaceConstant.SAXON, "dynamic-error-info");
                            Saxon.Hej.trans.SymbolicName.F sn = new Saxon.Hej.trans.SymbolicName.F(functionName, 1);
                            Saxon.Hej.expr.Expression[] args = new Saxon.Hej.expr.Expression[]{new Saxon.Hej.expr.StringLiteral(vtest.getLocalPart())};
                            return Saxon.Hej.functions.registry.VendorFunctionSetHE.getInstance().bind(sn, args, env, new System.Collections.Generic.List<string>());
                        }
                    }
                }
                try {
                    @ref = env.bindVariable(vtest);
                } catch (Saxon.Hej.trans.XPathException err) {
                    err.maybeSetLocation(makeLocation());
                    throw;
                }
            }
            setLocation(@ref, offset);
            return @ref;
        }
        protected internal virtual Saxon.Hej.expr.Literal makeStringLiteral(string currentTokenValue) {
            Saxon.Hej.expr.StringLiteral literal = new Saxon.Hej.expr.StringLiteral(currentTokenValue);
            setLocation(literal);
            return literal;
        }
        protected internal virtual string unescape(string token) {
            return token;
        }
        protected internal virtual Saxon.Hej.expr.Expression parseConstructor() {
            grumble("Node constructor expressions are allowed only in XQuery, not in XPath");
            return new Saxon.Hej.expr.ErrorExpression();
        }
        public Saxon.Hej.expr.Expression parseDynamicFunctionCall(Saxon.Hej.expr.Expression functionItem, Saxon.Hej.expr.Expression prefixArgument) {
            checkLanguageVersion30();
            System.Collections.Generic.List<Saxon.Hej.expr.Expression> args = new System.Collections.Generic.List<Saxon.Hej.expr.Expression>(10);
            if ((prefixArgument != null)) {
                args.Add(prefixArgument);
            }
            Saxon.Hej.z.IntSet placeMarkers = null;
            nextToken();
            if ((t.currentToken != Saxon.Hej.expr.parser.Token.RPAR)) {
                while (true) {
                    Saxon.Hej.expr.Expression arg;
                    int peek = t.peekAhead();
                    if (((t.currentToken == Saxon.Hej.expr.parser.Token.QMARK) && (((peek == Saxon.Hej.expr.parser.Token.COMMA) || (peek == Saxon.Hej.expr.parser.Token.RPAR))))) {
                        nextToken();
                        if ((placeMarkers == null)) {
                            placeMarkers = new Saxon.Hej.z.IntArraySet();
                        }
                        placeMarkers.add(args.Count);
                        arg = Saxon.Hej.expr.Literal.makeEmptySequence();
                    } else {
                        arg = parseFunctionArgument();
                    }
                    args.Add(arg);
                    if ((t.currentToken == Saxon.Hej.expr.parser.Token.COMMA)) {
                        nextToken();
                    } else {
                        if ((t.currentToken == Saxon.Hej.expr.parser.Token.COLON)) {
                            grumble("Keyword arguments are not allowed in a dynamic function call");
                        } else {
                            break ;
                        }
                    }
                }
                expect(Saxon.Hej.expr.parser.Token.RPAR);
            }
            nextToken();
            if ((placeMarkers == null)) {
                return generateApplyCall(functionItem, args);
            } else {
                return parserExtension.createDynamicCurriedFunction(this, functionItem, args, placeMarkers);
            }
        }
        protected internal Saxon.Hej.expr.Expression generateApplyCall(Saxon.Hej.expr.Expression functionItem, System.Collections.Generic.List<Saxon.Hej.expr.Expression> args) {
            Saxon.Hej.ma.arrays.SquareArrayConstructor block = new Saxon.Hej.ma.arrays.SquareArrayConstructor(args);
            Saxon.Hej.expr.parser.RetainedStaticContext rsc = new Saxon.Hej.expr.parser.RetainedStaticContext(getStaticContext());
            Saxon.Hej.functions.SystemFunction fn = Saxon.Hej.functions.registry.VendorFunctionSetHE.getInstance().makeFunction("apply", 2);
            fn.setRetainedStaticContext(rsc);
            Saxon.Hej.expr.Expression call = fn.makeFunctionCall(functionItem, block);
            ((Saxon.Hej.functions.ApplyFn)(fn)).setDynamicFunctionCall(functionItem.toShortString());
            setLocation(call, t.currentTokenStartOffset);
            return call;
        }
        protected internal Saxon.Hej.expr.Expression parseLookup(Saxon.Hej.expr.Expression lhs) {
            checkLanguageVersion31();
            Saxon.Hej.expr.parser.Tokenizer t = getTokenizer();
            int offset = t.currentTokenStartOffset;
            t.setState(Saxon.Hej.expr.parser.Tokenizer.BARE_NAME_STATE);
            t.currentToken = Saxon.Hej.expr.parser.Token.LPAR;
            nextToken();
            int token = t.currentToken;
            t.setState(Saxon.Hej.expr.parser.Tokenizer.OPERATOR_STATE);
            Saxon.Hej.expr.Expression result;
            if ((token == Saxon.Hej.expr.parser.Token.NAME)) {
                string name = t.currentTokenValue;
                if (!(Saxon.Hej.om.NameChecker.isValidNCName(Saxon.Hej.str.StringTool.codePoints(name)))) {
                    grumble("The name following '?' must be a valid NCName");
                }
                nextToken();
                result = lookupName(lhs, name);
            } else {
                if ((token == Saxon.Hej.expr.parser.Token.NUMBER)) {
                    Saxon.Hej.value.NumericValue number = Saxon.Hej.value.NumericValue.parseNumber(t.currentTokenValue);
                    if (!(((number is Saxon.Hej.value.IntegerValue)))) {
                        grumble("Number following '?' must be an integer");
                    }
                    nextToken();
                    result = lookup(this, lhs, Saxon.Hej.expr.Literal.makeLiteral(number));
                } else {
                    if (((token == Saxon.Hej.expr.parser.Token.MULT) || (token == Saxon.Hej.expr.parser.Token.STAR))) {
                        nextToken();
                        result = lookupStar(lhs);
                    } else {
                        if ((token == Saxon.Hej.expr.parser.Token.LPAR)) {
                            t.setState(Saxon.Hej.expr.parser.Tokenizer.DEFAULT_STATE);
                            result = lookup(this, lhs, parseParenthesizedExpression());
                        } else {
                            if ((token == Saxon.Hej.expr.parser.Token.STRING_LITERAL)) {
                                checkLanguageVersion40();
                                result = lookupName(lhs, t.currentTokenValue);
                                nextToken();
                            } else {
                                if ((token == Saxon.Hej.expr.parser.Token.DOLLAR)) {
                                    checkLanguageVersion40();
                                    result = lookup(this, lhs, parseVariableReference());
                                } else {
                                    grumble((("Unexpected "+Saxon.Hej.expr.parser.Token.tokens[token])+" after '?'"));
                                    return null;
                                }
                            }
                        }
                    }
                }
            }
            setLocation(result, offset);
            return result;
        }
        private static Saxon.Hej.expr.Expression lookup(Saxon.Hej.expr.parser.XPathParser parser, Saxon.Hej.expr.Expression lhs, Saxon.Hej.expr.Expression rhs) {
            return new Saxon.Hej.expr.LookupExpression(lhs, rhs);
        }
        private Saxon.Hej.expr.Expression lookupName(Saxon.Hej.expr.Expression lhs, string rhs) {
            return new Saxon.Hej.expr.LookupExpression(lhs, new Saxon.Hej.expr.StringLiteral(rhs));
        }
        private static Saxon.Hej.expr.Expression lookupStar(Saxon.Hej.expr.Expression lhs) {
            return new Saxon.Hej.expr.LookupAllExpression(lhs);
        }
        protected internal Saxon.Hej.pattern.NodeTest parseNodeTest(short nodeType) {
            int tok = t.currentToken;
            string tokv = t.currentTokenValue;
            switch (tok) {
              case Saxon.Hej.expr.parser.Token.NAME:
                nextToken();
                return makeNameTest(nodeType, tokv, (nodeType == Saxon.Hej.type.Type.ELEMENT));
              case Saxon.Hej.expr.parser.Token.PREFIX:
                nextToken();
                return makeNamespaceTest(nodeType, tokv);
              case Saxon.Hej.expr.parser.Token.SUFFIX:
                nextToken();
                tokv = t.currentTokenValue;
                expect(Saxon.Hej.expr.parser.Token.NAME);
                nextToken();
                return makeLocalNameTest(nodeType, tokv);
              case Saxon.Hej.expr.parser.Token.STAR:
                nextToken();
                return Saxon.Hej.pattern.NodeKindTest.makeNodeKindTest(nodeType);
              case Saxon.Hej.expr.parser.Token.NODEKIND:
                return parseKindTest();
              default:
                grumble("Unrecognized node test");
                throw new Saxon.Hej.trans.XPathException("");
            }
        }
        private Saxon.Hej.pattern.NodeTest parseKindTest() {
            Saxon.Hej.om.NamePool pool = env.getConfiguration().getNamePool();
            string typeName = t.currentTokenValue;
            bool schemaDeclaration = typeName.StartsWith("schema-");
            int primaryType = getSystemType(typeName);
            int fp = -1;
            bool empty = false;
            nextToken();
            if ((t.currentToken == Saxon.Hej.expr.parser.Token.RPAR)) {
                if (schemaDeclaration) {
                    grumble("schema-element() and schema-attribute() require a name to be supplied");
                    return null;
                }
                empty = true;
                nextToken();
            }
            switch (primaryType) {
              case Saxon.Hej.type.Type.ITEM:
                grumble("item() is not allowed in a path expression");
                return null;
              case Saxon.Hej.type.Type.NODE:
                if (empty) {
                    return Saxon.Hej.pattern.AnyNodeTest.getInstance();
                } else {
                    grumble("Expected ')': no arguments are allowed in node()");
                    return null;
                }
              case Saxon.Hej.type.Type.TEXT:
                if (empty) {
                    return Saxon.Hej.pattern.NodeKindTest.TEXT;
                } else {
                    grumble("Expected ')': no arguments are allowed in text()");
                    return null;
                }
              case Saxon.Hej.type.Type.COMMENT:
                if (empty) {
                    return Saxon.Hej.pattern.NodeKindTest.COMMENT;
                } else {
                    grumble("Expected ')': no arguments are allowed in comment()");
                    return null;
                }
              case Saxon.Hej.type.Type.NAMESPACE:
                if (empty) {
                    if (!(isNamespaceTestAllowed())) {
                        grumble("namespace-node() test is not allowed in XPath 2.0/XQuery 1.0");
                    }
                    return Saxon.Hej.pattern.NodeKindTest.NAMESPACE;
                } else {
                    if (((language == Saxon.Hej.expr.parser.XPathParser.ParsedLanguage.EXTENDED_ITEM_TYPE) && (t.currentToken == Saxon.Hej.expr.parser.Token.NAME))) {
                        string nsName = t.currentTokenValue;
                        nextToken();
                        expect(Saxon.Hej.expr.parser.Token.RPAR);
                        nextToken();
                        return new Saxon.Hej.pattern.NameTest(Saxon.Hej.type.Type.NAMESPACE, "", nsName, pool);
                    } else {
                        grumble("No arguments are allowed in namespace-node()");
                        return null;
                    }
                }
              case Saxon.Hej.type.Type.DOCUMENT:
                if (empty) {
                    return Saxon.Hej.pattern.NodeKindTest.DOCUMENT;
                } else {
                    int innerType;
                    try {
                        innerType = getSystemType(t.currentTokenValue);
                    } catch (Saxon.Hej.trans.XPathException err) {
                        innerType = Saxon.Hej.type.Type.ITEM;
                    }
                    if ((innerType != Saxon.Hej.type.Type.ELEMENT)) {
                        grumble("Argument to document-node() must be an element type descriptor");
                        return null;
                    }
                    Saxon.Hej.pattern.NodeTest inner = parseKindTest();
                    expect(Saxon.Hej.expr.parser.Token.RPAR);
                    nextToken();
                    return new Saxon.Hej.pattern.DocumentNodeTest(inner);
                }
              case Saxon.Hej.type.Type.PROCESSING_INSTRUCTION:
                if (empty) {
                    return Saxon.Hej.pattern.NodeKindTest.PROCESSING_INSTRUCTION;
                } else {
                    if ((t.currentToken == Saxon.Hej.expr.parser.Token.STRING_LITERAL)) {
                        string piName = Saxon.Hej.value.Whitespace.trim(unescape(t.currentTokenValue));
                        if (!(Saxon.Hej.om.NameChecker.isValidNCName(Saxon.Hej.str.StringTool.codePoints(piName)))) {
                            grumble("Processing instruction name must be a valid NCName", "XPTY0004");
                        } else {
                            fp = pool.allocateFingerprint("", piName);
                        }
                    } else {
                        if ((t.currentToken == Saxon.Hej.expr.parser.Token.NAME)) {
                            try {
                                string[] parts = Saxon.Hej.om.NameChecker.getQNameParts(t.currentTokenValue);
                                if (parts[0].Length==0) {
                                    fp = pool.allocateFingerprint("", parts[1]);
                                } else {
                                    grumble("Processing instruction name must not contain a colon");
                                }
                            } catch (Saxon.Hej.om.QNameException e) {
                                grumble(("Invalid processing instruction name. "+e.Message));
                            }
                        } else {
                            grumble("Processing instruction name must be a QName or a string literal");
                        }
                    }
                }
                nextToken();
                expect(Saxon.Hej.expr.parser.Token.RPAR);
                nextToken();
                return new Saxon.Hej.pattern.NameTest(Saxon.Hej.type.Type.PROCESSING_INSTRUCTION, fp, pool);
              case Saxon.Hej.type.Type.ATTRIBUTE:
              case Saxon.Hej.type.Type.ELEMENT:
                string nodeName = "";
                Saxon.Hej.pattern.NodeTest nodeTest = null;
                if (empty) {
                    return Saxon.Hej.pattern.NodeKindTest.makeNodeKindTest(primaryType);
                } else {
                    if (((t.currentToken == Saxon.Hej.expr.parser.Token.STAR) || (t.currentToken == Saxon.Hej.expr.parser.Token.MULT))) {
                        if (schemaDeclaration) {
                            grumble("schema-element() and schema-attribute() must specify an actual name, not '*'");
                            return null;
                        }
                        nodeTest = Saxon.Hej.pattern.NodeKindTest.makeNodeKindTest(primaryType);
                        nextToken();
                    } else {
                        if ((t.currentToken == Saxon.Hej.expr.parser.Token.NAME)) {
                            nodeName = t.currentTokenValue;
                            fp = makeFingerprint(t.currentTokenValue, (primaryType == Saxon.Hej.type.Type.ELEMENT));
                            nextToken();
                        } else {
                            if (((((t.currentToken == Saxon.Hej.expr.parser.Token.PREFIX) || (t.currentToken == Saxon.Hej.expr.parser.Token.SUFFIX))) && allowXPath40Syntax)) {
                                nodeTest = parseNodeTest((short)(primaryType));
                            } else {
                                grumble((("Unexpected "+Saxon.Hej.expr.parser.Token.tokens[t.currentToken])+" after '(' in SequenceType"));
                            }
                        }
                    }
                }
                string suri = null;
                if ((fp != -1)) {
                    suri = pool.getURI(fp);
                }
                if ((t.currentToken == Saxon.Hej.expr.parser.Token.RPAR)) {
                    nextToken();
                    if ((fp == -1)) {
                        return nodeTest;
                    } else {
                        if ((primaryType == Saxon.Hej.type.Type.ATTRIBUTE)) {
                            if (schemaDeclaration) {
                                Saxon.Hej.type.SchemaDeclaration attributeDecl = env.getConfiguration().getAttributeDeclaration((fp&unchecked((int)0xfffff)));
                                if (!(env.isImportedSchema(suri))) {
                                    grumble((("No schema has been imported for namespace '"+suri)+'\''), "XPST0008");
                                }
                                if ((attributeDecl == null)) {
                                    grumble((("There is no declaration for attribute @"+nodeName)+" in an imported schema"), "XPST0008");
                                    return null;
                                } else {
                                    return attributeDecl.makeSchemaNodeTest();
                                }
                            } else {
                                return new Saxon.Hej.pattern.NameTest(Saxon.Hej.type.Type.ATTRIBUTE, fp, pool);
                            }
                        } else {
                            if (schemaDeclaration) {
                                if (!(env.isImportedSchema(suri))) {
                                    grumble((("No schema has been imported for namespace '"+suri)+'\''), "XPST0008");
                                }
                                Saxon.Hej.type.SchemaDeclaration elementDecl = env.getConfiguration().getElementDeclaration((fp&unchecked((int)0xfffff)));
                                if ((elementDecl == null)) {
                                    grumble((("There is no declaration for element <"+nodeName)+"> in an imported schema"), "XPST0008");
                                    return null;
                                } else {
                                    return elementDecl.makeSchemaNodeTest();
                                }
                            } else {
                                return makeNameTest(Saxon.Hej.type.Type.ELEMENT, nodeName, true);
                            }
                        }
                    }
                } else {
                    if ((t.currentToken == Saxon.Hej.expr.parser.Token.COMMA)) {
                        if (schemaDeclaration) {
                            grumble("schema-element() and schema-attribute() must have one argument only");
                            return null;
                        }
                        nextToken();
                        Saxon.Hej.pattern.NodeTest result;
                        if ((t.currentToken == Saxon.Hej.expr.parser.Token.STAR)) {
                            grumble("'*' is no longer permitted as the second argument of element() and attribute()");
                            return null;
                        } else {
                            if ((t.currentToken == Saxon.Hej.expr.parser.Token.NAME)) {
                                Saxon.Hej.type.SchemaType schemaType;
                                Saxon.Hej.om.StructuredQName contentType = makeStructuredQName(t.currentTokenValue, env.getDefaultElementNamespace());
                                System.Diagnostics.Debug.Assert((contentType != null));
                                string uri = contentType.getURI();
                                string lname = contentType.getLocalPart();
                                if (uri.Equals(Saxon.Hej.lib.NamespaceConstant.SCHEMA)) {
                                    schemaType = env.getConfiguration().getSchemaType(contentType);
                                } else {
                                    if (!(env.isImportedSchema(uri))) {
                                        grumble((("No schema has been imported for namespace '"+uri)+'\''), "XPST0008");
                                    }
                                    schemaType = env.getConfiguration().getSchemaType(contentType);
                                }
                                if ((schemaType == null)) {
                                    grumble(("Unknown type name "+contentType.getEQName()), "XPST0008");
                                    return null;
                                }
                                if (((primaryType == Saxon.Hej.type.Type.ATTRIBUTE) && schemaType.isComplexType())) {
                                    warning("An attribute cannot have a complex type");
                                }
                                Saxon.Hej.pattern.ContentTypeTest typeTest = new Saxon.Hej.pattern.ContentTypeTest(primaryType, schemaType, env.getConfiguration(), false);
                                if (((fp == -1) && (((nodeTest == null) || (nodeTest is Saxon.Hej.pattern.NodeKindTest))))) {
                                    result = typeTest;
                                    if ((primaryType == Saxon.Hej.type.Type.ATTRIBUTE)) {
                                        nextToken();
                                    } else {
                                        nextToken();
                                        if ((t.currentToken == Saxon.Hej.expr.parser.Token.QMARK)) {
                                            typeTest.setNillable(true);
                                            nextToken();
                                        }
                                    }
                                } else {
                                    if ((primaryType == Saxon.Hej.type.Type.ATTRIBUTE)) {
                                        if ((nodeTest == null)) {
                                            nodeTest = new Saxon.Hej.pattern.NameTest(Saxon.Hej.type.Type.ATTRIBUTE, fp, pool);
                                        }
                                        if (((schemaType == Saxon.Hej.type.AnyType.getInstance()) || (schemaType == Saxon.Hej.type.AnySimpleType.getInstance()))) {
                                            result = nodeTest;
                                        } else {
                                            result = new Saxon.Hej.pattern.CombinedNodeTest(nodeTest, Saxon.Hej.expr.parser.Token.INTERSECT, typeTest);
                                        }
                                        nextToken();
                                    } else {
                                        if ((nodeTest == null)) {
                                            nodeTest = new Saxon.Hej.pattern.NameTest(Saxon.Hej.type.Type.ELEMENT, fp, pool);
                                        }
                                        result = new Saxon.Hej.pattern.CombinedNodeTest(nodeTest, Saxon.Hej.expr.parser.Token.INTERSECT, typeTest);
                                        nextToken();
                                        if ((t.currentToken == Saxon.Hej.expr.parser.Token.QMARK)) {
                                            typeTest.setNillable(true);
                                            nextToken();
                                        }
                                    }
                                }
                            } else {
                                grumble((("Unexpected "+Saxon.Hej.expr.parser.Token.tokens[t.currentToken])+" after ',' in SequenceType"));
                                return null;
                            }
                        }
                        expect(Saxon.Hej.expr.parser.Token.RPAR);
                        nextToken();
                        return result;
                    } else {
                        grumble("Expected ')' or ',' in SequenceType");
                    }
                }
                return null;
              default:
                grumble("Unknown node kind");
                return null;
            }
        }
        protected internal bool isNamespaceTestAllowed() {
            return allowXPath30Syntax;
        }
        private int getSystemType(string name) {
            if ("item".Equals(name)) {
                return Saxon.Hej.type.Type.ITEM;
            } else {
                if ("document-node".Equals(name)) {
                    return Saxon.Hej.type.Type.DOCUMENT;
                } else {
                    if ("element".Equals(name)) {
                        return Saxon.Hej.type.Type.ELEMENT;
                    } else {
                        if ("schema-element".Equals(name)) {
                            return Saxon.Hej.type.Type.ELEMENT;
                        } else {
                            if ("attribute".Equals(name)) {
                                return Saxon.Hej.type.Type.ATTRIBUTE;
                            } else {
                                if ("schema-attribute".Equals(name)) {
                                    return Saxon.Hej.type.Type.ATTRIBUTE;
                                } else {
                                    if ("text".Equals(name)) {
                                        return Saxon.Hej.type.Type.TEXT;
                                    } else {
                                        if ("comment".Equals(name)) {
                                            return Saxon.Hej.type.Type.COMMENT;
                                        } else {
                                            if ("processing-instruction".Equals(name)) {
                                                return Saxon.Hej.type.Type.PROCESSING_INSTRUCTION;
                                            } else {
                                                if ("namespace-node".Equals(name)) {
                                                    return Saxon.Hej.type.Type.NAMESPACE;
                                                } else {
                                                    if ("node".Equals(name)) {
                                                        return Saxon.Hej.type.Type.NODE;
                                                    } else {
                                                        grumble(("Unknown type "+name));
                                                        return -1;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        protected internal void checkLanguageVersion30() {
            if (!(allowXPath30Syntax)) {
                grumble("To use XPath 3.0 syntax, you must configure the XPath parser to handle it");
            }
        }
        protected internal void checkLanguageVersion31() {
            if (!(allowXPath31Syntax)) {
                grumble("The XPath parser is not configured to allow use of XPath 3.1 syntax");
            }
        }
        protected internal void checkLanguageVersion40() {
            if (!(allowXPath40Syntax)) {
                grumble("The XPath parser is not configured to allow use of XPath 4.0 syntax");
            }
        }
        protected internal void checkMapExtensions() {
            if (!(((allowXPath31Syntax || allowXPath30XSLTExtensions)))) {
                grumble("The XPath parser is not configured to allow use of the map syntax from XSLT 3.0 or XPath 3.1");
            }
        }
        public void checkSyntaxExtensions(string construct) {
            if (!(allowXPath40Syntax)) {
                grumble((("Saxon XPath syntax extensions have not been enabled: "+construct)+" is not allowed"));
            }
        }
        protected internal Saxon.Hej.expr.Expression parseMapExpression() {
            checkMapExtensions();
            Saxon.Hej.expr.parser.Tokenizer t = getTokenizer();
            int offset = t.currentTokenStartOffset;
            System.Collections.Generic.IList<Saxon.Hej.expr.Expression> entries = new System.Collections.Generic.List<Saxon.Hej.expr.Expression>();
            nextToken();
            if ((t.currentToken != Saxon.Hej.expr.parser.Token.RCURLY)) {
                while (true) {
                    Saxon.Hej.expr.Expression key = parseExprSingle();
                    if ((t.currentToken == Saxon.Hej.expr.parser.Token.ASSIGN)) {
                        grumble("The ':=' notation is no longer accepted in map expressions: use ':' instead");
                    }
                    expect(Saxon.Hej.expr.parser.Token.COLON);
                    nextToken();
                    Saxon.Hej.expr.Expression value = parseExprSingle();
                    Saxon.Hej.expr.Expression entry;
                    if ((((key is Saxon.Hej.expr.Literal) && (((Saxon.Hej.expr.Literal)(key)).getGroundedValue() is Saxon.Hej.value.AtomicValue)) && (value is Saxon.Hej.expr.Literal))) {
                        entry = Saxon.Hej.expr.Literal.makeLiteral(new Saxon.Hej.ma.map.SingleEntryMap((Saxon.Hej.value.AtomicValue)(((Saxon.Hej.expr.Literal)(key)).getGroundedValue()), ((Saxon.Hej.expr.Literal)(value)).getGroundedValue()));
                    } else {
                        entry = Saxon.Hej.ma.map.MapFunctionSet.getInstance().makeFunction("entry", 2).makeFunctionCall(key, value);
                    }
                    entries.Add(entry);
                    if ((t.currentToken == Saxon.Hej.expr.parser.Token.RCURLY)) {
                        break ;
                    } else {
                        expect(Saxon.Hej.expr.parser.Token.COMMA);
                        nextToken();
                    }
                }
            }
            t.lookAhead();
            nextToken();
            Saxon.Hej.expr.Expression result;
            switch (entries.Count) {
              case 0:
                result = Saxon.Hej.expr.Literal.makeLiteral(new Saxon.Impl.DataModel.ImmMap());
                break ;
              case 1:
                result = entries[0];
                break ;
              default:
                Saxon.Hej.expr.Expression[] entriesArray = new Saxon.Hej.expr.Expression[entries.Count];
                Saxon.Hej.expr.instruct.Block block = new Saxon.Hej.expr.instruct.Block(entries.ToArray());
                Saxon.Hej.ma.map.DictionaryMap options = new Saxon.Hej.ma.map.DictionaryMap();
                options.initialPut("duplicates", Saxon.Hej.value.StringValue.bmp("reject"));
                options.initialPut("duplicates-error-code", Saxon.Hej.value.StringValue.bmp("XQDY0137"));
                result = Saxon.Hej.ma.map.MapFunctionSet.getInstance().makeFunction("merge", 2).makeFunctionCall(block, Saxon.Hej.expr.Literal.makeLiteral(options));
                break ;
            }
            setLocation(result, offset);
            return result;
        }
        protected internal Saxon.Hej.expr.Expression parseArraySquareConstructor() {
            checkLanguageVersion31();
            Saxon.Hej.expr.parser.Tokenizer t = getTokenizer();
            int offset = t.currentTokenStartOffset;
            System.Collections.Generic.IList<Saxon.Hej.expr.Expression> members = new System.Collections.Generic.List<Saxon.Hej.expr.Expression>();
            nextToken();
            if ((t.currentToken == Saxon.Hej.expr.parser.Token.RSQB)) {
                nextToken();
                Saxon.Hej.ma.arrays.SquareArrayConstructor arrayBlock = new Saxon.Hej.ma.arrays.SquareArrayConstructor(members);
                setLocation(arrayBlock, offset);
                return arrayBlock;
            }
            while (true) {
                Saxon.Hej.expr.Expression member = parseExprSingle();
                members.Add(member);
                if ((t.currentToken == Saxon.Hej.expr.parser.Token.COMMA)) {
                    nextToken();
                    continue ;
                } else {
                    if ((t.currentToken == Saxon.Hej.expr.parser.Token.RSQB)) {
                        nextToken();
                        break ;
                    }
                }
                grumble((("Expected ',' or ']', "+"found ")+Saxon.Hej.expr.parser.Token.tokens[t.currentToken]));
                return new Saxon.Hej.expr.ErrorExpression();
            }
            Saxon.Hej.ma.arrays.SquareArrayConstructor block = new Saxon.Hej.ma.arrays.SquareArrayConstructor(members);
            setLocation(block, offset);
            return block;
        }
        protected internal Saxon.Hej.expr.Expression parseArrayCurlyConstructor() {
            checkLanguageVersion31();
            Saxon.Hej.expr.parser.Tokenizer t = getTokenizer();
            int offset = t.currentTokenStartOffset;
            nextToken();
            if ((t.currentToken == Saxon.Hej.expr.parser.Token.RCURLY)) {
                t.lookAhead();
                nextToken();
                return Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.ma.arrays.SimpleArrayItem.EMPTY_ARRAY);
            }
            Saxon.Hej.expr.Expression body = parseExpression();
            expect(Saxon.Hej.expr.parser.Token.RCURLY);
            t.lookAhead();
            nextToken();
            Saxon.Hej.functions.SystemFunction sf = Saxon.Hej.ma.arrays.ArrayFunctionSet.getInstance().makeFunction("_from-sequence", 1);
            Saxon.Hej.expr.Expression result = sf.makeFunctionCall(body);
            setLocation(result, offset);
            return result;
        }
        public virtual Saxon.Hej.expr.Expression parseFunctionCall(Saxon.Hej.expr.Expression prefixArgument) {
            string fname = t.currentTokenValue;
            int offset = t.currentTokenStartOffset;
            System.Collections.Generic.List<Saxon.Hej.expr.Expression> args = new System.Collections.Generic.List<Saxon.Hej.expr.Expression>(10);
            if ((prefixArgument != null)) {
                args.Add(prefixArgument);
            }
            Saxon.Hej.om.StructuredQName functionName = resolveFunctionName(fname);
            Saxon.Hej.z.IntSet placeMarkers = null;
            System.Collections.Generic.IDictionary<string,Saxon.Hej.expr.Expression> keywordArgs = null;
            nextToken();
            if ((t.currentToken != Saxon.Hej.expr.parser.Token.RPAR)) {
                while (true) {
                    int peek = t.peekAhead();
                    Saxon.Hej.expr.Expression arg;
                    if (((t.currentToken == Saxon.Hej.expr.parser.Token.NAME) && (((peek == Saxon.Hej.expr.parser.Token.COLON) || (peek == Saxon.Hej.expr.parser.Token.ASSIGN))))) {
                        if (!(Saxon.Hej.om.NameChecker.isValidNCName(t.currentTokenValue))) {
                            grumble("Name used as argument keyword must be a valid NCName (a space may be missing after ':')");
                        }
                        string keyword = t.currentTokenValue;
                        nextToken();
                        nextToken();
                        arg = parseExprSingle();
                        if ((keywordArgs == null)) {
                            keywordArgs = new System.Collections.Generic.Dictionary<string, Saxon.Hej.expr.Expression>();
                        } else {
                            if (keywordArgs.ContainsKey(keyword)) {
                                grumble((("Duplicate keyword '"+keyword)+"'in function arguments"));
                            }
                        }
                        Saxon.Impl.Helpers.MapUtils.Put(keywordArgs, keyword, arg);
                    } else {
                        if ((keywordArgs != null)) {
                            grumble("Keyword arguments must not be followed by positional arguments in a function call");
                        }
                        if (((t.currentToken == Saxon.Hej.expr.parser.Token.QMARK) && (((peek == Saxon.Hej.expr.parser.Token.COMMA) || (peek == Saxon.Hej.expr.parser.Token.RPAR))))) {
                            nextToken();
                            if ((placeMarkers == null)) {
                                placeMarkers = new Saxon.Hej.z.IntArraySet();
                            }
                            placeMarkers.add(args.Count);
                            arg = Saxon.Hej.expr.Literal.makeEmptySequence();
                        } else {
                            arg = parseExprSingle();
                        }
                        args.Add(arg);
                    }
                    if ((t.currentToken == Saxon.Hej.expr.parser.Token.COMMA)) {
                        nextToken();
                    } else {
                        break ;
                    }
                }
                expect(Saxon.Hej.expr.parser.Token.RPAR);
                if ((keywordArgs != null)) {
                    args.Add(makeMapExpression(keywordArgs));
                }
            }
            nextToken();
            if (scanOnly) {
                return new Saxon.Hej.expr.StringLiteral(Saxon.Hej.value.StringValue.EMPTY_STRING);
            }
            Saxon.Hej.expr.Expression[] arguments = new Saxon.Hej.expr.Expression[args.Count];
            arguments = args.ToArray();
            if ((placeMarkers != null)) {
                return parserExtension.makeCurriedFunction(this, offset, functionName, arguments, placeMarkers);
            }
            Saxon.Hej.expr.Expression fcall;
            Saxon.Hej.trans.SymbolicName.F sn = new Saxon.Hej.trans.SymbolicName.F(functionName, args.Count);
            System.Collections.Generic.IList<string> reasons = new System.Collections.Generic.List<string>();
            fcall = env.getFunctionLibrary().bind(sn, arguments, env, reasons);
            if ((fcall == null)) {
                return reportMissingFunction(offset, functionName, arguments, reasons);
            }
            if ((language == Saxon.Hej.expr.parser.XPathParser.ParsedLanguage.XSLT_PATTERN)) {
                if (fcall.isCallOn(typeof(Saxon.Hej.functions.RegexGroup))) {
                    return Saxon.Hej.expr.Literal.makeEmptySequence();
                } else {
                    if ((fcall is Saxon.Hej.functions.CurrentGroupCall)) {
                        grumble("The current-group() function cannot be used in a pattern", "XTSE1060", offset);
                        return new Saxon.Hej.expr.ErrorExpression();
                    } else {
                        if ((fcall is Saxon.Hej.functions.CurrentGroupingKeyCall)) {
                            grumble("The current-grouping-key() function cannot be used in a pattern", "XTSE1070", offset);
                            return new Saxon.Hej.expr.ErrorExpression();
                        } else {
                            if (fcall.isCallOn(typeof(Saxon.Hej.functions.CurrentMergeGroup))) {
                                grumble("The current-merge-group() function cannot be used in a pattern", "XTSE3470", offset);
                                return new Saxon.Hej.expr.ErrorExpression();
                            } else {
                                if (fcall.isCallOn(typeof(Saxon.Hej.functions.CurrentMergeKey))) {
                                    grumble("The current-merge-key() function cannot be used in a pattern", "XTSE3500", offset);
                                    return new Saxon.Hej.expr.ErrorExpression();
                                }
                            }
                        }
                    }
                }
            }
            setLocation(fcall, offset);
            foreach (Saxon.Hej.expr.Expression argument in arguments) {
                if ((((fcall != argument) && (argument.getParentExpression() == null)) && !(functionName.hasURI(Saxon.Hej.lib.NamespaceConstant.GLOBAL_JS)))) {
                    fcall.adoptChildExpression(argument);
                }
            }
            return makeTracer(fcall, functionName);
        }
        private Saxon.Hej.expr.Expression makeMapExpression(System.Collections.Generic.IDictionary<string,Saxon.Hej.expr.Expression> keywordArgs) {
            Saxon.Hej.expr.Expression[] block = new Saxon.Hej.expr.Expression[keywordArgs.Count];
            int i = 0;
            foreach (System.Collections.Generic.KeyValuePair<string,Saxon.Hej.expr.Expression> entry in keywordArgs) {
                Saxon.Hej.expr.StringLiteral key = new Saxon.Hej.expr.StringLiteral(entry.Key);
                block[i++] = Saxon.Hej.ma.map.MapFunctionSet.getInstance().makeFunction("entry", 2).makeFunctionCall(key, entry.Value);
            }
            Saxon.Hej.expr.instruct.Block entries = new Saxon.Hej.expr.instruct.Block(block);
            return Saxon.Hej.ma.map.MapFunctionSet.getInstance().makeFunction("merge", 1).makeFunctionCall(entries);
        }
        public Saxon.Hej.expr.Expression reportMissingFunction(int offset, Saxon.Hej.om.StructuredQName functionName, Saxon.Hej.expr.Expression[] arguments, System.Collections.Generic.IList<string> reasons) {
            Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder();
            sb.append("Cannot find a ").append(arguments.Length).append("-argument function named ").append(functionName.getEQName()).append("()");
            Saxon.Hej.Configuration config = env.getConfiguration();
            foreach (string reason in reasons) {
                sb.append(". ").append(reason);
            }
            if (config.getBooleanProperty(Saxon.Hej.lib.Feature<bool>.ALLOW_EXTERNAL_FUNCTIONS)) {
                bool existsWithDifferentArity = false;
                for (int i = 0; (i<(arguments.Length+5)); i++) {
                    if ((i != arguments.Length)) {
                        Saxon.Hej.trans.SymbolicName.F sn = new Saxon.Hej.trans.SymbolicName.F(functionName, i);
                        if (env.getFunctionLibrary().isAvailable(sn)) {
                            existsWithDifferentArity = true;
                            break ;
                        }
                    }
                }
                if (existsWithDifferentArity) {
                    sb.append(". The namespace URI and local name are recognized, but the number of arguments is wrong");
                } else {
                    string supplementary = getMissingFunctionExplanation(functionName, config);
                    if ((supplementary != null)) {
                        sb.append(". ").append(supplementary);
                    }
                }
            } else {
                sb.append(". External function calls have been disabled");
            }
            if (env.isInBackwardsCompatibleMode()) {
                return new Saxon.Hej.expr.ErrorExpression(sb.ToString(), "XTDE1425", false);
            } else {
                grumble(sb.ToString(), "XPST0017", offset);
                return null;
            }
        }
        public static string getMissingFunctionExplanation(Saxon.Hej.om.StructuredQName functionName, Saxon.Hej.Configuration config) {
            string actualURI = functionName.getURI();
            string similarNamespace = Saxon.Hej.lib.NamespaceConstant.findSimilarNamespace(actualURI);
            if ((similarNamespace != null)) {
                if (similarNamespace.Equals(actualURI)) {
                    switch (similarNamespace) {
                      case Saxon.Hej.lib.NamespaceConstant.FN:
                        return null;
                      case Saxon.Hej.lib.NamespaceConstant.SAXON:
                        if (config.getEditionCode().Equals("HE")) {
                            return "Saxon extension functions are not available under Saxon-HE";
                        } else {
                            if (!(config.isLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.PROFESSIONAL_EDITION))) {
                                return "Saxon extension functions require a Saxon-PE or Saxon-EE license";
                            }
                        }
                        break ;
                      case Saxon.Hej.lib.NamespaceConstant.XSLT:
                        if (functionName.getLocalPart().Equals("original")) {
                            return "Function name xsl:original is only available within an overriding function";
                        } else {
                            return "There are no functions defined in the XSLT namespace";
                        }
                    }
                } else {
                    return (("Perhaps the intended namespace was '"+similarNamespace)+"'");
                }
            } else {
                if (actualURI.Contains("java")) {
                    return diagnoseCallToJavaMethod(config);
                } else {
                    if (actualURI.StartsWith("clitype:")) {
                        return diagnoseCallToCliMethod(config);
                    }
                }
            }
            return null;
        }
        private static string diagnoseCallToJavaMethod(Saxon.Hej.Configuration config) {
            return "Reflexive calls to external Java methods are not available under Saxon-CS";
        }
        private static string diagnoseCallToCliMethod(Saxon.Hej.Configuration config) {
            return "Reflexive calls to external .NET methods are not available under Saxon-CS";
        }
        protected internal Saxon.Hej.om.StructuredQName resolveFunctionName(string fname) {
            if (scanOnly) {
                return new Saxon.Hej.om.StructuredQName("", Saxon.Hej.lib.NamespaceConstant.SAXON, "dummy");
            }
            Saxon.Hej.om.StructuredQName functionName = null;
            try {
                functionName = qNameParser.parse(fname, env.getDefaultFunctionNamespace());
            } catch (Saxon.Hej.trans.XPathException e) {
                grumble(e.Message, e.getErrorCodeLocalPart());
                System.Diagnostics.Debug.Assert(false);
            }
            if (functionName.hasURI(Saxon.Hej.lib.NamespaceConstant.SCHEMA)) {
                Saxon.Hej.type.ItemType t = Saxon.Hej.type.Type.getBuiltInItemType(functionName.getURI(), functionName.getLocalPart());
                if ((t is Saxon.Hej.type.BuiltInAtomicType)) {
                    checkAllowedType(env, (Saxon.Hej.type.BuiltInAtomicType)(t));
                }
            }
            return functionName;
        }
        public virtual Saxon.Hej.expr.Expression parseFunctionArgument() {
            return parseExprSingle();
        }
        protected internal Saxon.Hej.expr.Expression parseNamedFunctionReference() {
            return parserExtension.parseNamedFunctionReference(this);
        }
        protected internal virtual Saxon.Hej.query.AnnotationList parseAnnotationsList() {
            grumble("Function annotations are not allowed in XPath");
            return null;
        }
        protected internal Saxon.Hej.expr.Expression parseInlineFunction(Saxon.Hej.query.AnnotationList annotations) {
            int offset = t.currentTokenStartOffset;
            Saxon.Hej.expr.parser.XPathParser.InlineFunctionDetails details = new Saxon.Hej.expr.parser.XPathParser.InlineFunctionDetails();
            details.outerVariables = new Saxon.Hej.tree.util.IndexedStack<Saxon.Hej.expr.LocalBinding>();
            foreach (Saxon.Hej.expr.LocalBinding lb in getRangeVariables()) {
                details.outerVariables.push(lb);
            }
            details.outerVariablesUsed = new System.Collections.Generic.List<Saxon.Hej.expr.LocalBinding>(4);
            details.implicitParams = new System.Collections.Generic.List<Saxon.Hej.expr.instruct.UserFunctionParameter>(4);
            inlineFunctionStack.push(details);
            setRangeVariables(new Saxon.Hej.tree.util.IndexedStack<Saxon.Hej.expr.LocalBinding>());
            nextToken();
            System.Collections.Generic.HashSet<Saxon.Hej.om.StructuredQName> paramNames = new System.Collections.Generic.HashSet<Saxon.Hej.om.StructuredQName>(8);
            System.Collections.Generic.IList<Saxon.Hej.expr.instruct.UserFunctionParameter> @params = new System.Collections.Generic.List<Saxon.Hej.expr.instruct.UserFunctionParameter>(8);
            Saxon.Hej.value.SequenceType resultType = Saxon.Hej.value.SequenceType.ANY_SEQUENCE;
            int paramSlot = 0;
            while ((t.currentToken != Saxon.Hej.expr.parser.Token.RPAR)) {
                expect(Saxon.Hej.expr.parser.Token.DOLLAR);
                nextToken();
                expect(Saxon.Hej.expr.parser.Token.NAME);
                string argName = t.currentTokenValue;
                Saxon.Hej.om.StructuredQName argQName = makeStructuredQName(argName, "");
                if (paramNames.Contains(argQName)) {
                    grumble(("Duplicate parameter name "+Saxon.Hej.trans.Err.wrap(t.currentTokenValue, Saxon.Hej.trans.Err.VARIABLE)), "XQST0039");
                }
                paramNames.Add(argQName);
                Saxon.Hej.value.SequenceType paramType = Saxon.Hej.value.SequenceType.ANY_SEQUENCE;
                nextToken();
                if ((t.currentToken == Saxon.Hej.expr.parser.Token.AS)) {
                    nextToken();
                    paramType = parseSequenceType();
                }
                Saxon.Hej.expr.instruct.UserFunctionParameter arg = new Saxon.Hej.expr.instruct.UserFunctionParameter();
                arg.setRequiredType(paramType);
                arg.setVariableQName(argQName);
                arg.setSlotNumber(paramSlot++);
                @params.Add(arg);
                declareRangeVariable(arg);
                if ((t.currentToken == Saxon.Hej.expr.parser.Token.RPAR)) {
                    break ;
                } else {
                    if ((t.currentToken == Saxon.Hej.expr.parser.Token.COMMA)) {
                        nextToken();
                    } else {
                        grumble((("Expected ',' or ')' after function argument, found '"+Saxon.Hej.expr.parser.Token.tokens[t.currentToken])+'\''));
                    }
                }
            }
            t.setState(Saxon.Hej.expr.parser.Tokenizer.BARE_NAME_STATE);
            nextToken();
            if ((t.currentToken == Saxon.Hej.expr.parser.Token.AS)) {
                t.setState(Saxon.Hej.expr.parser.Tokenizer.SEQUENCE_TYPE_STATE);
                nextToken();
                resultType = parseSequenceType();
            }
            expect(Saxon.Hej.expr.parser.Token.LCURLY);
            t.setState(Saxon.Hej.expr.parser.Tokenizer.DEFAULT_STATE);
            nextToken();
            Saxon.Hej.expr.Expression body;
            if (((t.currentToken == Saxon.Hej.expr.parser.Token.RCURLY) && isAllowXPath31Syntax())) {
                t.lookAhead();
                nextToken();
                body = Saxon.Hej.expr.Literal.makeEmptySequence();
            } else {
                body = parseExpression();
                expect(Saxon.Hej.expr.parser.Token.RCURLY);
                t.lookAhead();
                nextToken();
            }
            Saxon.Hej.expr.parser.ExpressionTool.setDeepRetainedStaticContext(body, getStaticContext().makeRetainedStaticContext());
            int arity = paramNames.Count;
            for (int i = 0; (i<arity); i++) {
                undeclareRangeVariable();
            }
            Saxon.Hej.expr.Expression result = makeInlineFunctionValue(this, annotations, details, @params, resultType, body);
            setLocation(result, offset);
            setRangeVariables(details.outerVariables);
            inlineFunctionStack.pop();
            return result;
        }
        public static Saxon.Hej.expr.Expression makeInlineFunctionValue(Saxon.Hej.expr.parser.XPathParser p, Saxon.Hej.query.AnnotationList annotations, Saxon.Hej.expr.parser.XPathParser.InlineFunctionDetails details, System.Collections.Generic.IList<Saxon.Hej.expr.instruct.UserFunctionParameter> @params, Saxon.Hej.value.SequenceType resultType, Saxon.Hej.expr.Expression body) {
            int arity = @params.Count;
            Saxon.Hej.expr.instruct.UserFunction uf = new Saxon.Hej.expr.instruct.UserFunction();
            uf.setFunctionName(new Saxon.Hej.om.StructuredQName("anon", Saxon.Hej.lib.NamespaceConstant.ANONYMOUS, ("f_"+uf.GetHashCode())));
            uf.setPackageData(p.getStaticContext().getPackageData());
            uf.setBody(body);
            uf.setAnnotations(annotations);
            uf.setResultType(resultType);
            uf.incrementReferenceCount();
            if ((uf.getPackageData() is Saxon.Hej.style.StylesheetPackage)) {
                Saxon.Hej.style.StylesheetPackage pack = (Saxon.Hej.style.StylesheetPackage)(uf.getPackageData());
                Saxon.Hej.expr.Component comp = Saxon.Hej.expr.Component.makeComponent(uf, Saxon.Hej.trans.Visibility.PRIVATE, Saxon.Hej.trans.VisibilityProvenance.DEFAULTED, pack, pack);
                uf.setDeclaringComponent(comp);
            }
            Saxon.Hej.expr.Expression result;
            System.Collections.Generic.IList<Saxon.Hej.expr.instruct.UserFunctionParameter> implicitParams = details.implicitParams;
            if (!(implicitParams.Count==0)) {
                int extraParams = implicitParams.Count;
                int expandedArity = (@params.Count+extraParams);
                Saxon.Hej.expr.instruct.UserFunctionParameter[] paramArray = new Saxon.Hej.expr.instruct.UserFunctionParameter[expandedArity];
                for (int i = 0; (i<@params.Count); i++) {
                    paramArray[i] = @params[i];
                }
                int k = @params.Count;
                foreach (Saxon.Hej.expr.instruct.UserFunctionParameter implicitParam in implicitParams) {
                    paramArray[k++] = implicitParam;
                }
                uf.setParameterDefinitions(paramArray);
                Saxon.Hej.expr.instruct.SlotManager stackFrame = p.getStaticContext().getConfiguration().makeSlotManager();
                for (int i = 0; (i<expandedArity); i++) {
                    int slot = stackFrame.allocateSlotNumber(paramArray[i].getVariableQName());
                    paramArray[i].setSlotNumber(slot);
                }
                Saxon.Hej.expr.parser.ExpressionTool.allocateSlots(body, expandedArity, stackFrame);
                uf.setStackFrameMap(stackFrame);
                result = new Saxon.Hej.functions.hof.UserFunctionReference(uf);
                Saxon.Hej.expr.Expression[] partialArgs = new Saxon.Hej.expr.Expression[expandedArity];
                for (int i = 0; (i<arity); i++) {
                    partialArgs[i] = null;
                }
                for (int ip = 0; (ip<implicitParams.Count); ip++) {
                    Saxon.Hej.expr.instruct.UserFunctionParameter ufp = implicitParams[ip];
                    Saxon.Hej.expr.LocalBinding binding = details.outerVariablesUsed[ip];
                    Saxon.Hej.expr.VariableReference var;
                    if ((binding is Saxon.Hej.expr.parser.ParserExtension.TemporaryXSLTVariableBinding)) {
                        var = new Saxon.Hej.expr.LocalVariableReference(binding);
                        ((Saxon.Hej.expr.parser.ParserExtension.TemporaryXSLTVariableBinding)(binding)).declaration.registerReference(var);
                    } else {
                        var = new Saxon.Hej.expr.LocalVariableReference(binding);
                    }
                    var.setStaticType(binding.getRequiredType(), null, 0);
                    ufp.setRequiredType(binding.getRequiredType());
                    partialArgs[(ip+arity)] = var;
                }
                result = new Saxon.Hej.functions.hof.PartialApply(result, partialArgs);
            } else {
                Saxon.Hej.expr.instruct.UserFunctionParameter[] paramArray = @params.ToArray();
                uf.setParameterDefinitions(paramArray);
                Saxon.Hej.expr.instruct.SlotManager stackFrame = p.getStaticContext().getConfiguration().makeSlotManager();
                foreach (Saxon.Hej.expr.instruct.UserFunctionParameter param in paramArray) {
                    stackFrame.allocateSlotNumber(param.getVariableQName());
                }
                Saxon.Hej.expr.parser.ExpressionTool.allocateSlots(body, @params.Count, stackFrame);
                uf.setStackFrameMap(stackFrame);
                result = new Saxon.Hej.functions.hof.UserFunctionReference(uf);
            }
            if ((uf.getPackageData() is Saxon.Hej.style.StylesheetPackage)) {
                ((Saxon.Hej.style.StylesheetPackage)(uf.getPackageData())).addComponent(uf.getDeclaringComponent());
            }
            return result;
        }
        public Saxon.Hej.expr.LocalBinding findOuterRangeVariable(Saxon.Hej.om.StructuredQName qName) {
            return findOuterRangeVariable(qName, inlineFunctionStack, getStaticContext());
        }
        public static Saxon.Hej.expr.LocalBinding findOuterRangeVariable(Saxon.Hej.om.StructuredQName qName, Saxon.Hej.tree.util.IndexedStack<Saxon.Hej.expr.parser.XPathParser.InlineFunctionDetails> inlineFunctionStack, Saxon.Hej.expr.StaticContext env) {
            Saxon.Hej.expr.LocalBinding b2 = findOuterXPathRangeVariable(qName, inlineFunctionStack);
            if ((b2 != null)) {
                return b2;
            }
            if (((env is Saxon.Hej.style.ExpressionContext) && !(inlineFunctionStack.isEmpty()))) {
                b2 = findOuterXSLTVariable(qName, inlineFunctionStack, env);
            }
            return b2;
        }
        private static Saxon.Hej.expr.LocalBinding findOuterXPathRangeVariable(Saxon.Hej.om.StructuredQName qName, Saxon.Hej.tree.util.IndexedStack<Saxon.Hej.expr.parser.XPathParser.InlineFunctionDetails> inlineFunctionStack) {
            for (int s = (inlineFunctionStack.size()-1); (s>=0); s--) {
                Saxon.Hej.expr.parser.XPathParser.InlineFunctionDetails details = inlineFunctionStack.get(s);
                Saxon.Hej.tree.util.IndexedStack<Saxon.Hej.expr.LocalBinding> outerVariables = details.outerVariables;
                for (int v = (outerVariables.size()-1); (v>=0); v--) {
                    Saxon.Hej.expr.LocalBinding b2 = outerVariables.get(v);
                    if (b2.getVariableQName().Equals(qName)) {
                        for (int bs = s; (bs<=(inlineFunctionStack.size()-1)); bs++) {
                            details = inlineFunctionStack.get(bs);
                            bool found = false;
                            for (int p = 0; (p<(details.outerVariablesUsed.Count-1)); p++) {
                                if ((details.outerVariablesUsed[p] == b2)) {
                                    b2 = details.implicitParams[p];
                                    found = true;
                                    break ;
                                }
                            }
                            if (!(found)) {
                                details.outerVariablesUsed.Add(b2);
                                Saxon.Hej.expr.instruct.UserFunctionParameter ufp = new Saxon.Hej.expr.instruct.UserFunctionParameter();
                                ufp.setVariableQName(qName);
                                ufp.setRequiredType(b2.getRequiredType());
                                details.implicitParams.Add(ufp);
                                b2 = ufp;
                            }
                        }
                        return b2;
                    }
                }
                Saxon.Hej.expr.LocalBinding b3 = bindParametersInNestedFunctions(qName, inlineFunctionStack, s);
                if ((b3 != null)) {
                    return b3;
                }
            }
            return null;
        }
        private static Saxon.Hej.expr.LocalBinding findOuterXSLTVariable(Saxon.Hej.om.StructuredQName qName, Saxon.Hej.tree.util.IndexedStack<Saxon.Hej.expr.parser.XPathParser.InlineFunctionDetails> inlineFunctionStack, Saxon.Hej.expr.StaticContext env) {
            Saxon.Hej.style.SourceBinding decl = ((Saxon.Hej.style.ExpressionContext)(env)).getStyleElement().bindLocalVariable(qName);
            if ((decl != null)) {
                Saxon.Hej.expr.parser.XPathParser.InlineFunctionDetails details = inlineFunctionStack.get(0);
                Saxon.Hej.expr.LocalBinding innermostBinding;
                bool found = false;
                for (int p = 0; (p<details.outerVariablesUsed.Count); p++) {
                    if (details.outerVariablesUsed[p].getVariableQName().Equals(qName)) {
                        found = true;
                        break ;
                    }
                }
                if (!(found)) {
                    details.outerVariablesUsed.Add(new Saxon.Hej.expr.parser.ParserExtension.TemporaryXSLTVariableBinding(decl));
                    Saxon.Hej.expr.instruct.UserFunctionParameter ufp = new Saxon.Hej.expr.instruct.UserFunctionParameter();
                    ufp.setVariableQName(qName);
                    ufp.setRequiredType(decl.getInferredType(true));
                    details.implicitParams.Add(ufp);
                }
                innermostBinding = bindParametersInNestedFunctions(qName, inlineFunctionStack, 0);
                return innermostBinding;
            }
            return null;
        }
        private static Saxon.Hej.expr.LocalBinding bindParametersInNestedFunctions(Saxon.Hej.om.StructuredQName qName, Saxon.Hej.tree.util.IndexedStack<Saxon.Hej.expr.parser.XPathParser.InlineFunctionDetails> inlineFunctionStack, int start) {
            Saxon.Hej.expr.parser.XPathParser.InlineFunctionDetails details = inlineFunctionStack.get(start);
            System.Collections.Generic.IList<Saxon.Hej.expr.instruct.UserFunctionParameter> @params = details.implicitParams;
            foreach (Saxon.Hej.expr.instruct.UserFunctionParameter param in @params) {
                if (param.getVariableQName().Equals(qName)) {
                    Saxon.Hej.expr.LocalBinding b2 = param;
                    for (int bs = (start+1); (bs<=(inlineFunctionStack.size()-1)); bs++) {
                        details = inlineFunctionStack.get(bs);
                        bool found = false;
                        for (int p = 0; (p<(details.outerVariablesUsed.Count-1)); p++) {
                            if ((details.outerVariablesUsed[p] == param)) {
                                b2 = details.implicitParams[p];
                                found = true;
                                break ;
                            }
                        }
                        if (!(found)) {
                            details.outerVariablesUsed.Add(param);
                            Saxon.Hej.expr.instruct.UserFunctionParameter ufp = new Saxon.Hej.expr.instruct.UserFunctionParameter();
                            ufp.setVariableQName(qName);
                            ufp.setRequiredType(param.getRequiredType());
                            details.implicitParams.Add(ufp);
                            b2 = ufp;
                        }
                    }
                    if ((b2 != null)) {
                        return b2;
                    }
                }
            }
            return null;
        }
        public Saxon.Hej.expr.Expression parseFocusFunction(Saxon.Hej.query.AnnotationList annotations) {
            checkLanguageVersion40();
            int offset = t.currentTokenStartOffset;
            Saxon.Hej.expr.parser.XPathParser.InlineFunctionDetails details = new Saxon.Hej.expr.parser.XPathParser.InlineFunctionDetails();
            details.outerVariables = new Saxon.Hej.tree.util.IndexedStack<Saxon.Hej.expr.LocalBinding>();
            foreach (Saxon.Hej.expr.LocalBinding lb in getRangeVariables()) {
                details.outerVariables.push(lb);
            }
            details.outerVariablesUsed = new System.Collections.Generic.List<Saxon.Hej.expr.LocalBinding>(4);
            details.implicitParams = new System.Collections.Generic.List<Saxon.Hej.expr.instruct.UserFunctionParameter>(4);
            inlineFunctionStack.push(details);
            setRangeVariables(new Saxon.Hej.tree.util.IndexedStack<Saxon.Hej.expr.LocalBinding>());
            nextToken();
            System.Collections.Generic.IList<Saxon.Hej.expr.instruct.UserFunctionParameter> @params = new System.Collections.Generic.List<Saxon.Hej.expr.instruct.UserFunctionParameter>(1);
            Saxon.Hej.value.SequenceType resultType = Saxon.Hej.value.SequenceType.ANY_SEQUENCE;
            Saxon.Hej.om.StructuredQName argQName = new Saxon.Hej.om.StructuredQName("saxon", Saxon.Hej.lib.NamespaceConstant.SAXON, "dot");
            Saxon.Hej.expr.instruct.UserFunctionParameter arg = new Saxon.Hej.expr.instruct.UserFunctionParameter();
            arg.setRequiredType(Saxon.Hej.value.SequenceType.SINGLE_ITEM);
            arg.setVariableQName(argQName);
            arg.setSlotNumber(0);
            @params.Add(arg);
            Saxon.Hej.expr.Expression body;
            if ((t.currentToken == Saxon.Hej.expr.parser.Token.RCURLY)) {
                t.lookAhead();
                nextToken();
                body = Saxon.Hej.expr.Literal.makeEmptySequence();
            } else {
                body = parseExpression();
                expect(Saxon.Hej.expr.parser.Token.RCURLY);
                t.lookAhead();
                nextToken();
                body.setRetainedStaticContext(getStaticContext().makeRetainedStaticContext());
                Saxon.Hej.expr.LocalVariableReference @ref = new Saxon.Hej.expr.LocalVariableReference(arg);
                body = new Saxon.Hej.expr.instruct.ForEach(@ref, body);
            }
            Saxon.Hej.expr.Expression result = makeInlineFunctionValue(this, Saxon.Hej.query.AnnotationList.EMPTY, details, @params, resultType, body);
            setLocation(result, offset);
            setRangeVariables(details.outerVariables);
            inlineFunctionStack.pop();
            return result;
        }
        protected internal Saxon.Hej.expr.Expression makeCurriedFunction(int offset, Saxon.Hej.om.StructuredQName name, Saxon.Hej.expr.Expression[] args, Saxon.Hej.z.IntSet placeMarkers) {
            grumble("Partial function application is not allowed in Saxon-HE");
            return new Saxon.Hej.expr.ErrorExpression();
        }
        private readonly static string[] reservedFunctionNames30 = new string[]{"attribute", "comment", "document-node", "element", "empty-sequence", "function", "if", "item", "namespace-node", "node", "processing-instruction", "schema-attribute", "schema-element", "switch", "text", "typeswitch"};
        protected internal static bool isReservedFunctionName30(string name) {
            int x = Saxon.Ejava.util.Arrays.binarySearch(reservedFunctionNames30, name);
            return (x>=0);
        }
        private readonly static string[] reservedFunctionNames31 = new string[]{"array", "attribute", "comment", "document-node", "element", "empty-sequence", "function", "if", "item", "map", "namespace-node", "node", "processing-instruction", "schema-attribute", "schema-element", "switch", "text", "typeswitch"};
        public static bool isReservedFunctionName31(string name) {
            int x = Saxon.Ejava.util.Arrays.binarySearch(reservedFunctionNames31, name);
            return (x>=0);
        }
        public Saxon.Hej.tree.util.IndexedStack<Saxon.Hej.expr.LocalBinding> getRangeVariables() {
            return rangeVariables;
        }
        public void setRangeVariables(Saxon.Hej.tree.util.IndexedStack<Saxon.Hej.expr.LocalBinding> variables) {
            this.rangeVariables = variables;
        }
        public void declareRangeVariable(Saxon.Hej.expr.LocalBinding declaration) {
            rangeVariables.push(declaration);
        }
        public void undeclareRangeVariable() {
            rangeVariables.pop();
        }
        protected internal Saxon.Hej.expr.LocalBinding findRangeVariable(Saxon.Hej.om.StructuredQName qName) {
            for (int v = (rangeVariables.size()-1); (v>=0); v--) {
                Saxon.Hej.expr.LocalBinding b = rangeVariables.get(v);
                if (b.getVariableQName().Equals(qName)) {
                    return b;
                }
            }
            return findOuterRangeVariable(qName);
        }
        public void setRangeVariableStack(Saxon.Hej.tree.util.IndexedStack<Saxon.Hej.expr.LocalBinding> stack) {
            rangeVariables = stack;
        }
        public int makeFingerprint(string qname, bool useDefault) {
            if (scanOnly) {
                return Saxon.Hej.om.StandardNames.XML_SPACE;
            }
            try {
                string defaultNS = (useDefault ? env.getDefaultElementNamespace() : "");
                Saxon.Hej.om.StructuredQName sq = qNameParser.parse(qname, defaultNS);
                return env.getConfiguration().getNamePool().allocateFingerprint(sq.getURI(), sq.getLocalPart());
            } catch (Saxon.Hej.trans.XPathException e) {
                grumble(e.Message, e.getErrorCodeLocalPart());
                return -1;
            }
        }
        public Saxon.Hej.om.StructuredQName makeStructuredQNameSilently(string qname, string defaultUri) {
            if (scanOnly) {
                return new Saxon.Hej.om.StructuredQName("", Saxon.Hej.lib.NamespaceConstant.SAXON, "dummy");
            }
            return qNameParser.parse(qname, defaultUri);
        }
        public Saxon.Hej.om.StructuredQName makeStructuredQName(string qname, string defaultUri) {
            try {
                return makeStructuredQNameSilently(qname, defaultUri);
            } catch (Saxon.Hej.trans.XPathException err) {
                grumble(err.Message, err.getErrorCodeLocalPart());
                return new Saxon.Hej.om.StructuredQName("", "", "error");
            }
        }
        public Saxon.Hej.om.NodeName makeNodeName(string qname, bool useDefault) {
            Saxon.Hej.om.StructuredQName sq = makeStructuredQNameSilently(qname, (useDefault ? env.getDefaultElementNamespace() : ""));
            string prefix = sq.getPrefix();
            string uri = sq.getURI();
            string local = sq.getLocalPart();
            if (uri.Length==0) {
                int fp = env.getConfiguration().getNamePool().allocateFingerprint("", local);
                return new Saxon.Hej.om.NoNamespaceName(local, fp);
            } else {
                int fp = env.getConfiguration().getNamePool().allocateFingerprint(uri, local);
                return new Saxon.Hej.om.FingerprintedQName(prefix, uri, local, fp);
            }
        }
        public Saxon.Hej.pattern.NodeTest makeNameTest(short nodeType, string qname, bool useDefault) {
            Saxon.Hej.om.NamePool pool = env.getConfiguration().getNamePool();
            string defaultNS = "";
            if ((((useDefault && (nodeType == Saxon.Hej.type.Type.ELEMENT)) && !(qname.StartsWith("Q{"))) && !(qname.Contains(":")))) {
                Saxon.Api.UnprefixedElementMatchingPolicy policy = env.getUnprefixedElementMatchingPolicy();
                switch (policy) {
                  case Saxon.Api.UnprefixedElementMatchingPolicy.DefaultNamespace:
                    defaultNS = env.getDefaultElementNamespace();
                    break ;
                  case Saxon.Api.UnprefixedElementMatchingPolicy.DefaultNamespaceOrNone:
                    defaultNS = env.getDefaultElementNamespace();
                    Saxon.Hej.om.StructuredQName q = makeStructuredQName(qname, defaultNS);
                    int fp1 = pool.allocateFingerprint(q.getURI(), q.getLocalPart());
                    Saxon.Hej.pattern.NameTest test1 = new Saxon.Hej.pattern.NameTest(nodeType, fp1, pool);
                    int fp2 = pool.allocateFingerprint("", q.getLocalPart());
                    Saxon.Hej.pattern.NameTest test2 = new Saxon.Hej.pattern.NameTest(nodeType, fp2, pool);
                    return new Saxon.Hej.pattern.CombinedNodeTest(test1, Saxon.Hej.expr.parser.Token.UNION, test2);
                  case Saxon.Api.UnprefixedElementMatchingPolicy.AnyNamespace:
                    if (!(Saxon.Hej.om.NameChecker.isValidNCName(Saxon.Hej.str.StringTool.codePoints(qname)))) {
                        grumble((("Invalid name '"+qname)+"'"));
                    }
                    return new Saxon.Hej.pattern.LocalNameTest(pool, nodeType, qname);
                }
            }
            Saxon.Hej.om.StructuredQName qName = makeStructuredQName(qname, defaultNS);
            int fp = pool.allocateFingerprint(qName.getURI(), qName.getLocalPart());
            return new Saxon.Hej.pattern.NameTest(nodeType, fp, pool);
        }
        public Saxon.Hej.pattern.QNameTest makeQNameTest(short nodeType, string qname) {
            Saxon.Hej.om.NamePool pool = env.getConfiguration().getNamePool();
            Saxon.Hej.om.StructuredQName q = makeStructuredQName(qname, "");
            System.Diagnostics.Debug.Assert((q != null));
            int fp = pool.allocateFingerprint(q.getURI(), q.getLocalPart());
            return new Saxon.Hej.pattern.NameTest(nodeType, fp, pool);
        }
        public Saxon.Hej.pattern.NamespaceTest makeNamespaceTest(short nodeType, string prefix) {
            Saxon.Hej.om.NamePool pool = env.getConfiguration().getNamePool();
            if (scanOnly) {
                return new Saxon.Hej.pattern.NamespaceTest(pool, nodeType, Saxon.Hej.lib.NamespaceConstant.SAXON);
            }
            if (prefix.StartsWith("Q{")) {
                string uri = prefix.Substring(2, (prefix.Length-2) - 2);
                return new Saxon.Hej.pattern.NamespaceTest(pool, nodeType, uri);
            }
            try {
                Saxon.Hej.om.StructuredQName sq = qNameParser.parse((prefix+":dummy"), "");
                return new Saxon.Hej.pattern.NamespaceTest(pool, nodeType, sq.getURI());
            } catch (Saxon.Hej.trans.XPathException err) {
                grumble(err.Message, err.getErrorCodeLocalPart());
                return null;
            }
        }
        public Saxon.Hej.pattern.LocalNameTest makeLocalNameTest(short nodeType, string localName) {
            if (!(Saxon.Hej.om.NameChecker.isValidNCName(Saxon.Hej.str.StringTool.codePoints(localName)))) {
                grumble((("Local name ["+localName)+"] contains invalid characters"));
            }
            return new Saxon.Hej.pattern.LocalNameTest(env.getConfiguration().getNamePool(), nodeType, localName);
        }
        protected internal void setLocation(Saxon.Hej.expr.Expression exp) {
            setLocation(exp, t.currentTokenStartOffset);
        }
        public void setLocation(Saxon.Hej.expr.Expression exp, int offset) {
            if ((exp != null)) {
                if (((exp.getLocation() == null) || (exp.getLocation() == Saxon.Hej.expr.parser.Loc.NONE))) {
                    exp.setLocation(makeLocation(offset));
                }
            }
        }
        public Saxon.Hej.s9api.Location makeLocation(int offset) {
            int line = t.getLineNumber(offset);
            int column = t.getColumnNumber(offset);
            return makeNestedLocation(env.getContainingLocation(), line, column, null);
        }
        public void setLocation(Saxon.Hej.expr.flwor.Clause clause, int offset) {
            int line = t.getLineNumber(offset);
            int column = t.getColumnNumber(offset);
            Saxon.Hej.s9api.Location loc = makeNestedLocation(env.getContainingLocation(), line, column, null);
            clause.setLocation(loc);
            clause.setPackageData(env.getPackageData());
        }
        private Saxon.Hej.s9api.Location mostRecentLocation = Saxon.Hej.expr.parser.Loc.NONE;
        public Saxon.Hej.s9api.Location makeLocation() {
            if ((((t.getLineNumber() == mostRecentLocation.getLineNumber()) && (t.getColumnNumber() == mostRecentLocation.getColumnNumber())) && (((((env.getSystemId() == null) && (mostRecentLocation.getSystemId() == null))) || env.getSystemId().Equals(mostRecentLocation.getSystemId()))))) {
                return mostRecentLocation;
            } else {
                int line = t.getLineNumber();
                int column = t.getColumnNumber();
                mostRecentLocation = makeNestedLocation(env.getContainingLocation(), line, column, null);
                return mostRecentLocation;
            }
        }
        public Saxon.Hej.s9api.Location makeNestedLocation(Saxon.Hej.s9api.Location containingLoc, int line, int column, string nearbyText) {
            if (((((containingLoc is Saxon.Hej.expr.parser.Loc) && (containingLoc.getLineNumber()<=1)) && (containingLoc.getColumnNumber() == -1)) && (nearbyText == null))) {
                return new Saxon.Hej.expr.parser.Loc(env.getSystemId(), (line+1), (column+1));
            } else {
                return new Saxon.Hej.expr.parser.XPathParser.NestedLocation(containingLoc, line, column, nearbyText);
            }
        }
        public virtual Saxon.Hej.expr.Expression makeTracer(Saxon.Hej.expr.Expression exp, Saxon.Hej.om.StructuredQName qName) {
            exp.setRetainedStaticContextLocally(env.makeRetainedStaticContext());
            if ((codeInjector != null)) {
                return codeInjector.inject(exp);
            } else {
                return exp;
            }
        }
        protected internal bool isKeyword(string s) {
            return ((t.currentToken == Saxon.Hej.expr.parser.Token.NAME) && t.currentTokenValue.Equals(s));
        }
        public void setScanOnly(bool scanOnly) {
            this.scanOnly = scanOnly;
        }
        public void setAllowAbsentExpression(bool allowEmpty) {
            this.allowAbsentExpression = allowEmpty;
        }
        public bool isAllowAbsentExpression() {
            return this.allowAbsentExpression;
        }
        public class NestedLocation : Saxon.Hej.s9api.Location {
            private readonly Saxon.Hej.s9api.Location containingLocation;
            private readonly int localLineNumber;
            private readonly int localColumnNumber;
            private string nearbyText;
            public NestedLocation(Saxon.Hej.s9api.Location containingLocation, int localLineNumber, int localColumnNumber)  {
                this.containingLocation = containingLocation.saveLocation();
                this.localLineNumber = localLineNumber;
                this.localColumnNumber = localColumnNumber;
            }
            public NestedLocation(Saxon.Hej.s9api.Location containingLocation, int localLineNumber, int localColumnNumber, string nearbyText)  {
                this.containingLocation = containingLocation.saveLocation();
                this.localLineNumber = localLineNumber;
                this.localColumnNumber = localColumnNumber;
                this.nearbyText = nearbyText;
            }
            public Saxon.Hej.s9api.Location getContainingLocation() {
                return containingLocation;
            }
            public int getColumnNumber() {
                return localColumnNumber;
            }
            public string getSystemId() {
                return containingLocation.getSystemId();
            }
            public string getPublicId() {
                return containingLocation.getPublicId();
            }
            public int getLocalLineNumber() {
                return localLineNumber;
            }
            public int getLineNumber() {
                return (containingLocation.getLineNumber()+localLineNumber);
            }
            public string getNearbyText() {
                return nearbyText;
            }
            public Saxon.Hej.s9api.Location saveLocation() {
                return this;
            }
        
        }
    
    }
}