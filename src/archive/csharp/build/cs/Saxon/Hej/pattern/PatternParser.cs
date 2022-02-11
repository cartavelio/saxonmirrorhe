using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.pattern {
    public class PatternParser : Saxon.Hej.expr.parser.XPathParser {
        internal int inPredicate = 0;
        public Saxon.Hej.pattern.Pattern parsePattern(string pattern, Saxon.Hej.expr.StaticContext env) {
            this.env = env;
            charChecker = env.getConfiguration().getValidCharacterChecker();
            language = Saxon.Hej.expr.parser.XPathParser.ParsedLanguage.XSLT_PATTERN;
            string trimmed = pattern.Trim();
            if (trimmed.StartsWith("(:")) {
                t = new Saxon.Hej.expr.parser.Tokenizer();
                t.languageLevel = 30;
                t.tokenize(trimmed, 0, -1);
                int start = t.currentTokenStartOffset;
                trimmed = trimmed.Substring(start);
            }
            allowXPath40Syntax = (env.getConfiguration().getBooleanProperty(Saxon.Hej.lib.Feature<bool>.ALLOW_SYNTAX_EXTENSIONS) || (env.getXPathVersion() == 40));
            if (isSelectionPattern(trimmed)) {
                Saxon.Hej.expr.Expression e = parse(pattern, 0, Saxon.Hej.expr.parser.Token.EOF, env);
                if ((e is Saxon.Hej.pattern.Pattern)) {
                    return (Saxon.Hej.pattern.Pattern)(e);
                } else {
                    if ((e is Saxon.Hej.expr.ContextItemExpression)) {
                        return new Saxon.Hej.pattern.UniversalPattern();
                    } else {
                        if ((e is Saxon.Hej.expr.FilterExpression)) {
                            Saxon.Hej.expr.Expression predicate = null;
                            while ((e is Saxon.Hej.expr.FilterExpression)) {
                                Saxon.Hej.expr.Expression filter = ((Saxon.Hej.expr.FilterExpression)(e)).getActionExpression();
                                e = ((Saxon.Hej.expr.FilterExpression)(e)).getSelectExpression();
                                Saxon.Hej.type.ItemType filterType = filter.getItemType();
                                Saxon.Hej.type.TypeHierarchy th = env.getConfiguration().getTypeHierarchy();
                                Saxon.Hej.type.Affinity rel = th.relationship(filterType, Saxon.Hej.type.NumericType.getInstance());
                                if ((rel != Saxon.Hej.type.Affinity.DISJOINT)) {
                                    if (((rel == Saxon.Hej.type.Affinity.SAME_TYPE) || (rel == Saxon.Hej.type.Affinity.SUBSUMED_BY))) {
                                        filter = new Saxon.Hej.expr.ValueComparison(filter, Saxon.Hej.expr.parser.Token.FEQ, Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.Int64Value.PLUS_ONE));
                                    } else {
                                        Saxon.Hej.expr.LetExpression let = new Saxon.Hej.expr.LetExpression();
                                        Saxon.Hej.om.StructuredQName varName = new Saxon.Hej.om.StructuredQName("vv", Saxon.Hej.lib.NamespaceConstant.SAXON_GENERATED_VARIABLE, ("v"+filter.GetHashCode()));
                                        let.setVariableQName(varName);
                                        Saxon.Hej.expr.InstanceOfExpression condition = new Saxon.Hej.expr.InstanceOfExpression(new Saxon.Hej.expr.LocalVariableReference(let), Saxon.Hej.value.SequenceType.SINGLE_NUMERIC);
                                        Saxon.Hej.expr.LocalVariableReference @ref = new Saxon.Hej.expr.LocalVariableReference(let);
                                        @ref.setStaticType(Saxon.Hej.value.SequenceType.SINGLE_NUMERIC, null, 0);
                                        Saxon.Hej.expr.ValueComparison comparison = new Saxon.Hej.expr.ValueComparison(@ref, Saxon.Hej.expr.parser.Token.FEQ, Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.Int64Value.PLUS_ONE));
                                        Saxon.Hej.expr.instruct.Choose choice = new Saxon.Hej.expr.instruct.Choose(new Saxon.Hej.expr.Expression[]{condition, Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.BooleanValue.TRUE)}, new Saxon.Hej.expr.Expression[]{comparison, new Saxon.Hej.expr.LocalVariableReference(let)});
                                        let.setSequence(filter);
                                        let.setAction(choice);
                                        let.setRequiredType(Saxon.Hej.value.SequenceType.ANY_SEQUENCE);
                                        let.setRetainedStaticContext(env.makeRetainedStaticContext());
                                        filter = let;
                                    }
                                }
                                if ((predicate == null)) {
                                    predicate = filter;
                                } else {
                                    predicate = new Saxon.Hej.expr.AndExpression(filter, predicate);
                                }
                            }
                            if ((e is Saxon.Hej.expr.ContextItemExpression)) {
                                return new Saxon.Hej.pattern.BooleanExpressionPattern(predicate);
                            }
                        }
                    }
                }
                grumble("Pattern starting with '.' must be followed by a sequence of predicates");
                return null;
            } else {
                if (isTypePattern(pattern)) {
                    this.env = env;
                    if ((qNameParser == null)) {
                        qNameParser = new Saxon.Hej.om.QNameParser(env.getNamespaceResolver());
                        if ((languageVersion>=30)) {
                            qNameParser = qNameParser.withAcceptEQName(true);
                        }
                    }
                    language = Saxon.Hej.expr.parser.XPathParser.ParsedLanguage.XSLT_PATTERN;
                    t = new Saxon.Hej.expr.parser.Tokenizer();
                    t.languageLevel = env.getXPathVersion();
                    allowXPath40Syntax = t.allowSaxonExtensions = (env.getConfiguration().getBooleanProperty(Saxon.Hej.lib.Feature<bool>.ALLOW_SYNTAX_EXTENSIONS) || (t.languageLevel == 40));
                    try {
                        t.tokenize(pattern, 0, -1);
                    } catch (Saxon.Hej.trans.XPathException err) {
                        grumble(err.Message);
                    }
                    Saxon.Hej.type.ItemType req = parseItemType();
                    Saxon.Hej.pattern.Pattern result = new Saxon.Hej.pattern.ItemTypePattern(req);
                    while ((t.currentToken == Saxon.Hej.expr.parser.Token.LSQB)) {
                        nextToken();
                        Saxon.Hej.expr.Expression predicate = parsePredicate();
                        expect(Saxon.Hej.expr.parser.Token.RSQB);
                        nextToken();
                        result = new Saxon.Hej.pattern.BasePatternWithPredicate(result, predicate);
                    }
                    expect(Saxon.Hej.expr.parser.Token.EOF);
                    return result;
                } else {
                    Saxon.Hej.expr.Expression exp = parse(pattern, 0, Saxon.Hej.expr.parser.Token.EOF, env);
                    exp.setRetainedStaticContext(env.makeRetainedStaticContext());
                    if ((exp is Saxon.Hej.expr.VennExpression)) {
                        checkNoPredicatePattern(((Saxon.Hej.expr.VennExpression)(exp)).getLhsExpression());
                        checkNoPredicatePattern(((Saxon.Hej.expr.VennExpression)(exp)).getRhsExpression());
                    }
                    Saxon.Hej.expr.parser.ExpressionVisitor visitor = Saxon.Hej.expr.parser.ExpressionVisitor.make(env);
                    visitor.setOptimizeForPatternMatching(true);
                    Saxon.Hej.expr.parser.ContextItemStaticInfo cit = visitor.getConfiguration().makeContextItemStaticInfo(Saxon.Hej.pattern.AnyNodeTest.getInstance(), true);
                    Saxon.Hej.pattern.Pattern pat;
                    try {
                        pat = Saxon.Hej.pattern.PatternMaker.fromExpression(exp.simplify().typeCheck(visitor, cit), env.getConfiguration(), true);
                    } catch (Saxon.Hej.trans.XPathException e) {
                        pat = Saxon.Hej.pattern.PatternMaker.fromExpression(exp.simplify(), env.getConfiguration(), true);
                    }
                    pat.setOriginalText(pattern);
                    if ((pat is Saxon.Hej.pattern.UnionPattern)) {
                        string[] parts = System.Text.RegularExpressions.Regex.Split(pattern, "\\|");
                        if ((parts.Length == 2)) {
                            ((Saxon.Hej.pattern.UnionPattern)(pat)).p1.setOriginalText(parts[0]);
                            ((Saxon.Hej.pattern.UnionPattern)(pat)).p2.setOriginalText(parts[1]);
                        }
                    }
                    if (((exp is Saxon.Hej.expr.FilterExpression) && (((Saxon.Hej.expr.FilterExpression)(exp)).getBase() is Saxon.Hej.expr.ContextItemExpression))) {
                        if ((allowXPath40Syntax && ((((pattern.StartsWith("tuple") || pattern.StartsWith("map")) || pattern.StartsWith("array")) || pattern.StartsWith("union"))))) {
                        } else {
                            grumble("A predicatePattern can appear only at the outermost level (parentheses not allowed)");
                        }
                    }
                    if (((exp is Saxon.Hej.expr.FilterExpression) && (pat is Saxon.Hej.pattern.NodeTestPattern))) {
                        pat.setPriority(0.5);
                    }
                    return pat;
                }
            }
        }
        private bool isSelectionPattern(string pattern) {
            return pattern.StartsWith(".");
        }
        private bool isTypePattern(string pattern) {
            if (System.Text.RegularExpressions.Regex.IsMatch(pattern, "^(type|record|map|array|union|atomic)\\s*\\(.+")) {
                checkLanguageVersion40();
                return true;
            }
            return false;
        }
        private void checkNoPredicatePattern(Saxon.Hej.expr.Expression exp) {
            if ((exp is Saxon.Hej.expr.ContextItemExpression)) {
                grumble("A predicatePattern can appear only at the outermost level (union operator not allowed)");
            }
            if ((exp is Saxon.Hej.expr.FilterExpression)) {
                checkNoPredicatePattern(((Saxon.Hej.expr.FilterExpression)(exp)).getBase());
            }
            if ((exp is Saxon.Hej.expr.VennExpression)) {
                checkNoPredicatePattern(((Saxon.Hej.expr.VennExpression)(exp)).getLhsExpression());
                checkNoPredicatePattern(((Saxon.Hej.expr.VennExpression)(exp)).getRhsExpression());
            }
        }
        protected internal override void customizeTokenizer(Saxon.Hej.expr.parser.Tokenizer t) {
        }
        public override Saxon.Hej.expr.Expression parseExpression() {
            Saxon.Hej.expr.parser.Tokenizer t = getTokenizer();
            if ((inPredicate>0)) {
                return base.parseExpression();
            } else {
                if (((allowXPath40Syntax && (t.currentToken == Saxon.Hej.expr.parser.Token.NODEKIND)) && ((((t.currentTokenValue.Equals("record") || t.currentTokenValue.Equals("type")) || t.currentTokenValue.Equals("map")) || t.currentTokenValue.Equals("array"))))) {
                    Saxon.Hej.type.ItemType type = parseItemType();
                    Saxon.Hej.expr.Expression expr = new Saxon.Hej.pattern.ItemTypePattern(type);
                    expr.setRetainedStaticContext(env.makeRetainedStaticContext());
                    setLocation(expr);
                    while ((t.currentToken == Saxon.Hej.expr.parser.Token.LSQB)) {
                        expr = parsePredicate(expr).toPattern(env.getConfiguration());
                    }
                    return expr;
                } else {
                    if (((allowXPath40Syntax && (t.currentToken == Saxon.Hej.expr.parser.Token.NODEKIND)) && (t.currentTokenValue.Equals("atomic")))) {
                        nextToken();
                        expect(Saxon.Hej.expr.parser.Token.NAME);
                        Saxon.Hej.om.StructuredQName typeName = makeStructuredQName(t.currentTokenValue, env.getDefaultElementNamespace());
                        nextToken();
                        expect(Saxon.Hej.expr.parser.Token.RPAR);
                        nextToken();
                        Saxon.Hej.type.SchemaType type = env.getConfiguration().getSchemaType(typeName);
                        if (((type == null) || !(type.isAtomicType()))) {
                            grumble(("Unknown atomic type "+typeName));
                        }
                        Saxon.Hej.type.AtomicType at = (Saxon.Hej.type.AtomicType)(type);
                        Saxon.Hej.expr.Expression expr = new Saxon.Hej.pattern.ItemTypePattern(at);
                        setLocation(expr);
                        while ((t.currentToken == Saxon.Hej.expr.parser.Token.LSQB)) {
                            expr = parsePredicate(expr);
                        }
                        return expr;
                    } else {
                        return parseBinaryExpression(parsePathExpression(), 10);
                    }
                }
            }
        }
        protected internal override Saxon.Hej.expr.Expression parseBasicStep(bool firstInPattern) {
            if ((inPredicate>0)) {
                return base.parseBasicStep(firstInPattern);
            } else {
                switch (t.currentToken) {
                  case Saxon.Hej.expr.parser.Token.DOLLAR:
                    if (!(firstInPattern)) {
                        grumble("In an XSLT 3.0 pattern, a variable reference is allowed only as the first step in a path");
                        return null;
                    } else {
                        return base.parseBasicStep(firstInPattern);
                    }
                  case Saxon.Hej.expr.parser.Token.STRING_LITERAL:
                  case Saxon.Hej.expr.parser.Token.NUMBER:
                  case Saxon.Hej.expr.parser.Token.KEYWORD_CURLY:
                  case Saxon.Hej.expr.parser.Token.ELEMENT_QNAME:
                  case Saxon.Hej.expr.parser.Token.ATTRIBUTE_QNAME:
                  case Saxon.Hej.expr.parser.Token.NAMESPACE_QNAME:
                  case Saxon.Hej.expr.parser.Token.PI_QNAME:
                  case Saxon.Hej.expr.parser.Token.TAG:
                  case Saxon.Hej.expr.parser.Token.NAMED_FUNCTION_REF:
                  case Saxon.Hej.expr.parser.Token.DOTDOT:
                    grumble((("Token "+currentTokenDisplay())+" not allowed here in an XSLT pattern"));
                    return null;
                  case Saxon.Hej.expr.parser.Token.FUNCTION:
                    if (!(firstInPattern)) {
                        grumble("In an XSLT pattern, a function call is allowed only as the first step in a path");
                    }
                    return base.parseBasicStep(firstInPattern);
                  case Saxon.Hej.expr.parser.Token.NODEKIND:
                    switch (t.currentTokenValue) {
                      case "type":
                      case "tuple":
                      case "union":
                      case "map":
                      case "array":
                      case "atomic":
                        return parserExtension.parseTypePattern(this);
                      default:
                        return base.parseBasicStep(firstInPattern);
                    }
                  default:
                    return base.parseBasicStep(firstInPattern);
                }
            }
        }
        protected internal override void testPermittedAxis(int axis, string errorCode) {
            base.testPermittedAxis(axis, errorCode);
            if ((inPredicate == 0)) {
                if (!(Saxon.Hej.om.AxisInfo.isSubtreeAxis[axis])) {
                    grumble((("The "+Saxon.Hej.om.AxisInfo.axisName[axis])+" is not allowed in a pattern"));
                }
            }
        }
        protected internal override Saxon.Hej.expr.Expression parsePredicate() {
            bool disallow = t.disallowUnionKeyword;
            t.disallowUnionKeyword = false;
            ++inPredicate;
            Saxon.Hej.expr.Expression exp = parseExpression();
            --inPredicate;
            t.disallowUnionKeyword = disallow;
            return exp;
        }
        public override Saxon.Hej.expr.Expression parseFunctionCall(Saxon.Hej.expr.Expression prefixArgument) {
            Saxon.Hej.expr.Expression fn = base.parseFunctionCall(prefixArgument);
            if ((((((inPredicate<=0) && !(fn.isCallOn(typeof(Saxon.Hej.functions.SuperId)))) && !(fn.isCallOn(typeof(Saxon.Hej.functions.KeyFn)))) && !(fn.isCallOn(typeof(Saxon.Hej.functions.Doc)))) && !(fn.isCallOn(typeof(Saxon.Hej.functions.Root_1))))) {
                grumble((("The "+fn)+" function is not allowed at the head of a pattern"));
            }
            return fn;
        }
        public override Saxon.Hej.expr.Expression parseFunctionArgument() {
            if ((inPredicate>0)) {
                return base.parseFunctionArgument();
            } else {
                switch (t.currentToken) {
                  case Saxon.Hej.expr.parser.Token.DOLLAR:
                    return parseVariableReference();
                  case Saxon.Hej.expr.parser.Token.STRING_LITERAL:
                    return parseStringLiteral(true);
                  case Saxon.Hej.expr.parser.Token.NUMBER:
                    return parseNumericLiteral(true);
                  default:
                    grumble("A function argument in an XSLT pattern must be a variable reference or literal");
                    return null;
                }
            }
        }
        public override Saxon.Hej.expr.Expression makeTracer(Saxon.Hej.expr.Expression exp, Saxon.Hej.om.StructuredQName qName) {
            return exp;
        }
    
    }
}