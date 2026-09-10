using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.optim {
    public class QuickXPathParser : Saxon.Hej.expr.parser.XPathParser.Accelerator {

        static QuickXPathParser() {
            Saxon.Impl.Helpers.MapUtils.Put(patterns, key(Saxon.Hej.expr.parser.Token.LPAR, Saxon.Hej.expr.parser.Token.RPAR), (values, env) => Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.EmptySequence.getInstance()));
            Saxon.Impl.Helpers.MapUtils.Put(patterns, key(Saxon.Hej.expr.parser.Token.LSQB, Saxon.Hej.expr.parser.Token.RSQB), (values, env) => Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.ma.arrays.SimpleArrayItem.EMPTY_ARRAY));
            Saxon.Impl.Helpers.MapUtils.Put(patterns, key(Saxon.Hej.expr.parser.Token.DOT), (values, env) => new Saxon.Hej.expr.ContextItemExpression());
            Saxon.Impl.Helpers.MapUtils.Put(patterns, key(Saxon.Hej.expr.parser.Token.DOTDOT), (values, env) => new Saxon.Hej.expr.AxisExpression(Saxon.Hej.om.AxisInfo.PARENT, Saxon.Hej.pattern.AnyNodeTest.getInstance()));
            Saxon.Impl.Helpers.MapUtils.Put(patterns, key(Saxon.Hej.expr.parser.Token.STAR), (values, env) => new Saxon.Hej.expr.AxisExpression(Saxon.Hej.om.AxisInfo.CHILD, Saxon.Hej.pattern.NodeKindTest.ELEMENT));
            Saxon.Impl.Helpers.MapUtils.Put(patterns, key(Saxon.Hej.expr.parser.Token.STRING_LITERAL), (values, env) => Saxon.Hej.expr.StringLiteral.makeLiteral(Saxon.Hej.value.StringValue.makeStringValue(values[0])));
            Saxon.Impl.Helpers.MapUtils.Put(patterns, key(Saxon.Hej.expr.parser.Token.NUMBER), (values, env) => toNumericLiteral(values[0]));
            Saxon.Impl.Helpers.MapUtils.Put(patterns, key(Saxon.Hej.expr.parser.Token.SLASH), (values, env) => new Saxon.Hej.expr.RootExpression());
            Saxon.Impl.Helpers.MapUtils.Put(patterns, key(Saxon.Hej.expr.parser.Token.NODEKIND, Saxon.Hej.expr.parser.Token.RPAR), (values, env) => {
    string kind = values[0];
    Saxon.Hej.pattern.NodeTest test = null;
    switch (kind) {
      case "node":
        test = Saxon.Hej.pattern.AnyNodeTest.getInstance();
        break ;
      case "element":
        test = Saxon.Hej.pattern.NodeKindTest.ELEMENT;
        break ;
      case "text":
        test = Saxon.Hej.pattern.NodeKindTest.TEXT;
        break ;
      case "comment":
        test = Saxon.Hej.pattern.NodeKindTest.COMMENT;
        break ;
      case "processing-instruction":
        test = Saxon.Hej.pattern.NodeKindTest.PROCESSING_INSTRUCTION;
        break ;
      default:
        return null;
    }
    return new Saxon.Hej.expr.AxisExpression(Saxon.Hej.om.AxisInfo.CHILD, test);
} );
            Saxon.Impl.Helpers.MapUtils.Put(patterns, key(Saxon.Hej.expr.parser.Token.DOLLAR, Saxon.Hej.expr.parser.Token.NAME), (values, env) => {
    Saxon.Hej.om.StructuredQName name = verifyQName(values[1], env);
    return env.bindVariable(name);
} );
            Saxon.Impl.Helpers.MapUtils.Put(patterns, key(Saxon.Hej.expr.parser.Token.AT, Saxon.Hej.expr.parser.Token.NAME), (values, env) => {
    Saxon.Hej.om.StructuredQName name = verifyQName(values[1], env);
    return new Saxon.Hej.expr.AxisExpression(Saxon.Hej.om.AxisInfo.ATTRIBUTE, new Saxon.Hej.pattern.NameTest(Saxon.Hej.type.Type.ATTRIBUTE, name.getURI(), name.getLocalPart(), env.getConfiguration().getNamePool()));
} );
            Saxon.Impl.Helpers.MapUtils.Put(patterns, key(Saxon.Hej.expr.parser.Token.AT, Saxon.Hej.expr.parser.Token.STAR), (values, env) => new Saxon.Hej.expr.AxisExpression(Saxon.Hej.om.AxisInfo.ATTRIBUTE, Saxon.Hej.pattern.NodeKindTest.ATTRIBUTE));
            Saxon.Impl.Helpers.MapUtils.Put(patterns, key(Saxon.Hej.expr.parser.Token.NAME), (values, env) => {
    if ((env.getUnprefixedElementMatchingPolicy() != Saxon.Api.UnprefixedElementMatchingPolicy.DefaultNamespace)) {
        return null;
    }
    Saxon.Hej.om.StructuredQName name = verifyElementQName(values[0], env);
    return new Saxon.Hej.expr.AxisExpression(Saxon.Hej.om.AxisInfo.CHILD, new Saxon.Hej.pattern.NameTest(Saxon.Hej.type.Type.ELEMENT, name.getURI(), name.getLocalPart(), env.getConfiguration().getNamePool()));
} );
            Saxon.Impl.Helpers.MapUtils.Put(patterns, key(Saxon.Hej.expr.parser.Token.KEYWORD_CURLY, Saxon.Hej.expr.parser.Token.RCURLY), (values, env) => {
    string name = values[0];
    if (name.Equals("map")) {
        return Saxon.Hej.expr.Literal.makeLiteral(new Saxon.Impl.DataModel.ImmMap());
    } else {
        if (name.Equals("array")) {
            return Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.ma.arrays.SimpleArrayItem.EMPTY_ARRAY);
        } else {
            return null;
        }
    }
} );
            Saxon.Impl.Helpers.MapUtils.Put(patterns, key(Saxon.Hej.expr.parser.Token.FUNCTION, Saxon.Hej.expr.parser.Token.RPAR), (values, env) => {
    Saxon.Hej.om.StructuredQName name = verifyFunctionQName(values[0], env);
    System.Collections.Generic.IList<string> reasons = new System.Collections.Generic.List<string>();
    return env.getFunctionLibrary().bind(new Saxon.Hej.trans.SymbolicName.F(name, 0), new Saxon.Hej.expr.Expression[]{}, env, reasons);
} );
            Saxon.Impl.Helpers.MapUtils.Put(patterns, key(Saxon.Hej.expr.parser.Token.FUNCTION, Saxon.Hej.expr.parser.Token.DOT, Saxon.Hej.expr.parser.Token.RPAR), (values, env) => {
    Saxon.Hej.om.StructuredQName name = verifyFunctionQName(values[0], env);
    System.Collections.Generic.IList<string> reasons = new System.Collections.Generic.List<string>();
    Saxon.Hej.expr.Expression cie = new Saxon.Hej.expr.ContextItemExpression();
    cie.setLocation(env.getContainingLocation());
    return env.getFunctionLibrary().bind(new Saxon.Hej.trans.SymbolicName.F(name, 1), new Saxon.Hej.expr.Expression[]{cie}, env, reasons);
} );
            Saxon.Impl.Helpers.MapUtils.Put(patterns, key(Saxon.Hej.expr.parser.Token.FUNCTION, Saxon.Hej.expr.parser.Token.NAME, Saxon.Hej.expr.parser.Token.RPAR), (values, env) => {
    Saxon.Hej.om.StructuredQName fnName = verifyFunctionQName(values[0], env);
    Saxon.Hej.om.StructuredQName elName = verifyElementQName(values[1], env);
    System.Collections.Generic.IList<string> reasons = new System.Collections.Generic.List<string>();
    Saxon.Hej.expr.Expression child = new Saxon.Hej.expr.AxisExpression(Saxon.Hej.om.AxisInfo.CHILD, new Saxon.Hej.pattern.NameTest(Saxon.Hej.type.Type.ELEMENT, elName.getURI(), elName.getLocalPart(), env.getConfiguration().getNamePool()));
    child.setLocation(env.getContainingLocation());
    return env.getFunctionLibrary().bind(new Saxon.Hej.trans.SymbolicName.F(fnName, 1), new Saxon.Hej.expr.Expression[]{child}, env, reasons);
} );
            Saxon.Impl.Helpers.MapUtils.Put(patterns, key(Saxon.Hej.expr.parser.Token.FUNCTION, Saxon.Hej.expr.parser.Token.DOLLAR, Saxon.Hej.expr.parser.Token.NAME, Saxon.Hej.expr.parser.Token.RPAR), (values, env) => {
    Saxon.Hej.om.StructuredQName varName = verifyQName(values[2], env);
    Saxon.Hej.expr.Expression varRef = env.bindVariable(varName);
    varRef.setLocation(env.getContainingLocation());
    Saxon.Hej.om.StructuredQName name = verifyFunctionQName(values[0], env);
    System.Collections.Generic.IList<string> reasons = new System.Collections.Generic.List<string>();
    return env.getFunctionLibrary().bind(new Saxon.Hej.trans.SymbolicName.F(name, 1), new Saxon.Hej.expr.Expression[]{varRef}, env, reasons);
} );
            Saxon.Impl.Helpers.MapUtils.Put(patterns, key(Saxon.Hej.expr.parser.Token.FUNCTION, Saxon.Hej.expr.parser.Token.AT, Saxon.Hej.expr.parser.Token.NAME, Saxon.Hej.expr.parser.Token.RPAR), (values, env) => {
    Saxon.Hej.om.StructuredQName attName = verifyQName(values[2], env);
    Saxon.Hej.expr.AxisExpression arg = new Saxon.Hej.expr.AxisExpression(Saxon.Hej.om.AxisInfo.ATTRIBUTE, new Saxon.Hej.pattern.NameTest(Saxon.Hej.type.Type.ATTRIBUTE, attName.getURI(), attName.getLocalPart(), env.getConfiguration().getNamePool()));
    arg.setLocation(env.getContainingLocation());
    Saxon.Hej.om.StructuredQName name = verifyFunctionQName(values[0], env);
    System.Collections.Generic.IList<string> reasons = new System.Collections.Generic.List<string>();
    return env.getFunctionLibrary().bind(new Saxon.Hej.trans.SymbolicName.F(name, 1), new Saxon.Hej.expr.Expression[]{arg}, env, reasons);
} );
            Saxon.Impl.Helpers.MapUtils.Put(patterns, key(Saxon.Hej.expr.parser.Token.FUNCTION, Saxon.Hej.expr.parser.Token.STRING_LITERAL, Saxon.Hej.expr.parser.Token.RPAR), (values, env) => {
    Saxon.Hej.expr.Expression arg = Saxon.Hej.expr.StringLiteral.makeLiteral(Saxon.Hej.value.StringValue.makeStringValue(values[1]));
    arg.setLocation(env.getContainingLocation());
    Saxon.Hej.om.StructuredQName name = verifyFunctionQName(values[0], env);
    System.Collections.Generic.IList<string> reasons = new System.Collections.Generic.List<string>();
    return env.getFunctionLibrary().bind(new Saxon.Hej.trans.SymbolicName.F(name, 1), new Saxon.Hej.expr.Expression[]{arg}, env, reasons);
} );
            Saxon.Impl.Helpers.MapUtils.Put(patterns, key(Saxon.Hej.expr.parser.Token.NAME, Saxon.Hej.expr.parser.Token.SLASH, Saxon.Hej.expr.parser.Token.NAME), (values, env) => {
    if ((env.getUnprefixedElementMatchingPolicy() != Saxon.Api.UnprefixedElementMatchingPolicy.DefaultNamespace)) {
        return null;
    }
    Saxon.Hej.om.StructuredQName first = verifyElementQName(values[0], env);
    Saxon.Hej.expr.AxisExpression start = new Saxon.Hej.expr.AxisExpression(Saxon.Hej.om.AxisInfo.CHILD, new Saxon.Hej.pattern.NameTest(Saxon.Hej.type.Type.ELEMENT, first.getURI(), first.getLocalPart(), env.getConfiguration().getNamePool()));
    start.setLocation(env.getContainingLocation());
    Saxon.Hej.om.StructuredQName second = verifyElementQName(values[2], env);
    Saxon.Hej.expr.AxisExpression step = new Saxon.Hej.expr.AxisExpression(Saxon.Hej.om.AxisInfo.CHILD, new Saxon.Hej.pattern.NameTest(Saxon.Hej.type.Type.ELEMENT, second.getURI(), second.getLocalPart(), env.getConfiguration().getNamePool()));
    step.setLocation(env.getContainingLocation());
    return new Saxon.Hej.expr.SlashExpression(start, step);
} );
            Saxon.Impl.Helpers.MapUtils.Put(patterns, key(Saxon.Hej.expr.parser.Token.NAME, Saxon.Hej.expr.parser.Token.UNION, Saxon.Hej.expr.parser.Token.NAME), (values, env) => {
    if ((env.getUnprefixedElementMatchingPolicy() != Saxon.Api.UnprefixedElementMatchingPolicy.DefaultNamespace)) {
        return null;
    }
    Saxon.Hej.om.StructuredQName first = verifyElementQName(values[0], env);
    Saxon.Hej.expr.AxisExpression one = new Saxon.Hej.expr.AxisExpression(Saxon.Hej.om.AxisInfo.CHILD, new Saxon.Hej.pattern.NameTest(Saxon.Hej.type.Type.ELEMENT, first.getURI(), first.getLocalPart(), env.getConfiguration().getNamePool()));
    one.setLocation(env.getContainingLocation());
    Saxon.Hej.om.StructuredQName second = verifyElementQName(values[2], env);
    Saxon.Hej.expr.AxisExpression two = new Saxon.Hej.expr.AxisExpression(Saxon.Hej.om.AxisInfo.CHILD, new Saxon.Hej.pattern.NameTest(Saxon.Hej.type.Type.ELEMENT, second.getURI(), second.getLocalPart(), env.getConfiguration().getNamePool()));
    two.setLocation(env.getContainingLocation());
    return new Saxon.Hej.expr.VennExpression(one, Saxon.Hej.expr.parser.Token.UNION, two);
} );
            Saxon.Impl.Helpers.MapUtils.Put(patterns, key(Saxon.Hej.expr.parser.Token.NAME, Saxon.Hej.expr.parser.Token.SLASH, Saxon.Hej.expr.parser.Token.AT, Saxon.Hej.expr.parser.Token.NAME), (values, env) => {
    if ((env.getUnprefixedElementMatchingPolicy() != Saxon.Api.UnprefixedElementMatchingPolicy.DefaultNamespace)) {
        return null;
    }
    Saxon.Hej.om.StructuredQName first = verifyElementQName(values[0], env);
    Saxon.Hej.expr.AxisExpression start = new Saxon.Hej.expr.AxisExpression(Saxon.Hej.om.AxisInfo.CHILD, new Saxon.Hej.pattern.NameTest(Saxon.Hej.type.Type.ELEMENT, first.getURI(), first.getLocalPart(), env.getConfiguration().getNamePool()));
    start.setLocation(env.getContainingLocation());
    Saxon.Hej.om.StructuredQName second = verifyQName(values[3], env);
    Saxon.Hej.expr.AxisExpression step = new Saxon.Hej.expr.AxisExpression(Saxon.Hej.om.AxisInfo.ATTRIBUTE, new Saxon.Hej.pattern.NameTest(Saxon.Hej.type.Type.ATTRIBUTE, second.getURI(), second.getLocalPart(), env.getConfiguration().getNamePool()));
    step.setLocation(env.getContainingLocation());
    return new Saxon.Hej.expr.SlashExpression(start, step);
} );
            Saxon.Impl.Helpers.MapUtils.Put(patterns, key(Saxon.Hej.expr.parser.Token.DOLLAR, Saxon.Hej.expr.parser.Token.NAME, Saxon.Hej.expr.parser.Token.SLASH, Saxon.Hej.expr.parser.Token.AT, Saxon.Hej.expr.parser.Token.NAME), (values, env) => {
    Saxon.Hej.om.StructuredQName first = verifyQName(values[1], env);
    Saxon.Hej.expr.Expression varRef = env.bindVariable(first);
    varRef.setLocation(env.getContainingLocation());
    Saxon.Hej.om.StructuredQName second = verifyQName(values[4], env);
    Saxon.Hej.expr.AxisExpression step = new Saxon.Hej.expr.AxisExpression(Saxon.Hej.om.AxisInfo.ATTRIBUTE, new Saxon.Hej.pattern.NameTest(Saxon.Hej.type.Type.ATTRIBUTE, second.getURI(), second.getLocalPart(), env.getConfiguration().getNamePool()));
    step.setLocation(env.getContainingLocation());
    return new Saxon.Hej.expr.SlashExpression(varRef, step);
} );
            Saxon.Impl.Helpers.MapUtils.Put(patterns, key(Saxon.Hej.expr.parser.Token.DOLLAR, Saxon.Hej.expr.parser.Token.NAME, Saxon.Hej.expr.parser.Token.LSQB, Saxon.Hej.expr.parser.Token.NUMBER, Saxon.Hej.expr.parser.Token.RSQB), (values, env) => {
    Saxon.Hej.om.StructuredQName first = verifyQName(values[1], env);
    Saxon.Hej.expr.Expression varRef = env.bindVariable(first);
    varRef.setLocation(env.getContainingLocation());
    if (values[3].Equals("1")) {
        return Saxon.Hej.expr.FirstItemExpression.makeFirstItemExpression(varRef);
    } else {
        Saxon.Hej.expr.Expression subscript = toNumericLiteral(values[3]);
        subscript.setLocation(env.getContainingLocation());
        return new Saxon.Hej.expr.FilterExpression(varRef, subscript);
    }
} );
            Saxon.Impl.Helpers.MapUtils.Put(patterns, key(Saxon.Hej.expr.parser.Token.DOLLAR, Saxon.Hej.expr.parser.Token.NAME, Saxon.Hej.expr.parser.Token.QMARK, Saxon.Hej.expr.parser.Token.NAME), (values, env) => {
    Saxon.Hej.om.StructuredQName first = verifyQName(values[1], env);
    Saxon.Hej.expr.Expression varRef = env.bindVariable(first);
    varRef.setLocation(env.getContainingLocation());
    if (!(Saxon.Hej.om.NameChecker.isValidNCName(values[3]))) {
        return null;
    }
    return new Saxon.Hej.expr.LookupExpression(varRef, new Saxon.Hej.expr.StringLiteral(values[3]));
} );
            Saxon.Impl.Helpers.MapUtils.Put(patterns, key(Saxon.Hej.expr.parser.Token.DOLLAR, Saxon.Hej.expr.parser.Token.NAME, Saxon.Hej.expr.parser.Token.EQUALS, Saxon.Hej.expr.parser.Token.STRING_LITERAL), (values, env) => {
    if (env.isInBackwardsCompatibleMode()) {
        return null;
    }
    Saxon.Hej.om.StructuredQName name = verifyQName(values[1], env);
    Saxon.Hej.expr.Expression varRef = env.bindVariable(name);
    varRef.setLocation(env.getContainingLocation());
    Saxon.Hej.expr.Expression comparand = new Saxon.Hej.expr.StringLiteral(values[3]);
    comparand.setLocation(env.getContainingLocation());
    return new Saxon.Hej.expr.GeneralComparison20(varRef, Saxon.Hej.expr.parser.Token.EQUALS, comparand);
} );
            Saxon.Impl.Helpers.MapUtils.Put(patterns, key(Saxon.Hej.expr.parser.Token.DOLLAR, Saxon.Hej.expr.parser.Token.NAME, Saxon.Hej.expr.parser.Token.NE, Saxon.Hej.expr.parser.Token.STRING_LITERAL), (values, env) => {
    if (env.isInBackwardsCompatibleMode()) {
        return null;
    }
    Saxon.Hej.om.StructuredQName name = verifyQName(values[1], env);
    Saxon.Hej.expr.Expression varRef = env.bindVariable(name);
    varRef.setLocation(env.getContainingLocation());
    Saxon.Hej.expr.Expression comparand = new Saxon.Hej.expr.StringLiteral(values[3]);
    comparand.setLocation(env.getContainingLocation());
    return new Saxon.Hej.expr.GeneralComparison20(varRef, Saxon.Hej.expr.parser.Token.NE, comparand);
} );
            Saxon.Impl.Helpers.MapUtils.Put(patterns, key(Saxon.Hej.expr.parser.Token.DOLLAR, Saxon.Hej.expr.parser.Token.NAME, Saxon.Hej.expr.parser.Token.EQUALS, Saxon.Hej.expr.parser.Token.NUMBER), (values, env) => {
    if (env.isInBackwardsCompatibleMode()) {
        return null;
    }
    Saxon.Hej.om.StructuredQName name = verifyQName(values[1], env);
    Saxon.Hej.expr.Expression varRef = env.bindVariable(name);
    varRef.setLocation(env.getContainingLocation());
    Saxon.Hej.expr.Expression comparand = toNumericLiteral(values[3]);
    comparand.setLocation(env.getContainingLocation());
    return new Saxon.Hej.expr.GeneralComparison20(varRef, Saxon.Hej.expr.parser.Token.EQUALS, comparand);
} );
            Saxon.Impl.Helpers.MapUtils.Put(patterns, key(Saxon.Hej.expr.parser.Token.DOLLAR, Saxon.Hej.expr.parser.Token.NAME, Saxon.Hej.expr.parser.Token.NE, Saxon.Hej.expr.parser.Token.NUMBER), (values, env) => {
    if (env.isInBackwardsCompatibleMode()) {
        return null;
    }
    Saxon.Hej.om.StructuredQName name = verifyQName(values[1], env);
    Saxon.Hej.expr.Expression varRef = env.bindVariable(name);
    varRef.setLocation(env.getContainingLocation());
    Saxon.Hej.expr.Expression comparand = toNumericLiteral(values[3]);
    comparand.setLocation(env.getContainingLocation());
    return new Saxon.Hej.expr.GeneralComparison20(varRef, Saxon.Hej.expr.parser.Token.NE, comparand);
} );
            Saxon.Impl.Helpers.MapUtils.Put(patterns, key(Saxon.Hej.expr.parser.Token.DOLLAR, Saxon.Hej.expr.parser.Token.NAME, Saxon.Hej.expr.parser.Token.PLUS, Saxon.Hej.expr.parser.Token.NUMBER), (values, env) => {
    if (env.isInBackwardsCompatibleMode()) {
        return null;
    }
    Saxon.Hej.om.StructuredQName name = verifyQName(values[1], env);
    Saxon.Hej.expr.Expression varRef = env.bindVariable(name);
    varRef.setLocation(env.getContainingLocation());
    Saxon.Hej.expr.Expression addend = toNumericLiteral(values[3]);
    addend.setLocation(env.getContainingLocation());
    return new Saxon.Hej.expr.ArithmeticExpression(varRef, Saxon.Hej.expr.parser.Token.PLUS, addend);
} );
            Saxon.Impl.Helpers.MapUtils.Put(patterns, key(Saxon.Hej.expr.parser.Token.AXIS, Saxon.Hej.expr.parser.Token.NAME), (values, env) => {
    int axis = Saxon.Hej.om.AxisInfo.getAxisNumber(values[0]);
    bool reverse = !(Saxon.Hej.om.AxisInfo.isForwards[axis]);
    if (((reverse || (axis == Saxon.Hej.om.AxisInfo.ATTRIBUTE)) || (axis == Saxon.Hej.om.AxisInfo.NAMESPACE))) {
        return null;
    }
    Saxon.Hej.om.StructuredQName name = verifyElementQName(values[1], env);
    return new Saxon.Hej.expr.AxisExpression(axis, new Saxon.Hej.pattern.NameTest(Saxon.Hej.type.Type.ELEMENT, name.getURI(), name.getLocalPart(), env.getConfiguration().getNamePool()));
} );
        }
        private static readonly System.Collections.Generic.IDictionary<System.Int64,Saxon.Eej.ee.optim.QuickXPathParser.ExpressionMaker> patterns = new System.Collections.Generic.Dictionary<System.Int64, Saxon.Eej.ee.optim.QuickXPathParser.ExpressionMaker>();
        public QuickXPathParser()  {
        }
        public Saxon.Hej.expr.Expression parse(Saxon.Hej.expr.parser.Tokenizer t, Saxon.Hej.expr.StaticContext env, string expression, int start, int terminator) {
            if ((((expression.Length-start)>30) && (terminator != Saxon.Hej.expr.parser.Token.RCURLY))) {
                return null;
            }
            try {
                t.tokenize(expression, start, -1);
                System.Collections.Generic.IList<int> tokenTypes = new System.Collections.Generic.List<int>();
                System.Collections.Generic.IList<string> tokenValues = new System.Collections.Generic.List<string>();
                while (((t.currentToken != Saxon.Hej.expr.parser.Token.EOF) && (t.currentToken != terminator))) {
                    if ((tokenTypes.Count>6)) {
                        return null;
                    }
                    tokenTypes.Add(t.currentToken);
                    tokenValues.Add(t.currentTokenValue);
                    t.next();
                    if ((t.currentToken == Saxon.Hej.expr.parser.Token.RCURLY)) {
                        t.lookAhead();
                    }
                }
                if ((t.currentToken != terminator)) {
                    return null;
                }
                long keyValue = key(tokenTypes);
                Saxon.Eej.ee.optim.QuickXPathParser.ExpressionMaker maker = Saxon.Impl.Helpers.MapUtils.Get(patterns, keyValue);
                if ((maker == null)) {
                    return null;
                }
                Saxon.Hej.expr.Expression result = /*Delegate.make()*/maker(tokenValues, env);
                if ((result != null)) {
                    result.setLocation(env.getContainingLocation());
                }
                return result;
            } catch (Saxon.Hej.trans.XPathException e) {
                return null;
            }
        }
        private static long key(System.Collections.Generic.IList<int> tokens) {
            long key = 0L;
            foreach (int token in tokens) {
                key = (((key<<8))+token);
            }
            return key;
        }
        private static long key(params int[]  tokens) {
            long key = 0L;
            foreach (int token in tokens) {
                key = (((key<<8))+token);
            }
            return key;
        }
        private delegate Saxon.Hej.expr.Expression ExpressionMaker (System.Collections.Generic.IList<string> tokens, Saxon.Hej.expr.StaticContext env);
        static Saxon.Hej.om.StructuredQName verifyQName(string name, Saxon.Hej.expr.StaticContext env) {
            return new Saxon.Hej.om.QNameParser(env.getNamespaceResolver()).parse(name, "");
        }
        static Saxon.Hej.om.StructuredQName verifyElementQName(string name, Saxon.Hej.expr.StaticContext env) {
            Saxon.Hej.om.QNameParser qNameParser = new Saxon.Hej.om.QNameParser(env.getNamespaceResolver());
            return qNameParser.parse(name, env.getDefaultElementNamespace());
        }
        static Saxon.Hej.om.StructuredQName verifyFunctionQName(string name, Saxon.Hej.expr.StaticContext env) {
            Saxon.Hej.om.QNameParser qNameParser = new Saxon.Hej.om.QNameParser(env.getNamespaceResolver());
            return qNameParser.parse(name, env.getDefaultFunctionNamespace());
        }
        private static Saxon.Hej.expr.Expression toNumericLiteral(string value) {
            Saxon.Hej.value.NumericValue number = Saxon.Hej.value.NumericValue.parseNumber(value);
            if (number.isNaN()) {
                return null;
            }
            return Saxon.Hej.expr.Literal.makeLiteral(number);
        }
    
    }
}