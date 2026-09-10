using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema.sdoc {
    public class SelectionParser : Saxon.Hej.expr.parser.XPathParser {
        internal bool isFieldSubset;
        internal Saxon.Hej.expr.Expression optimizedExpression;
        public Saxon.Hej.pattern.Pattern parseSelector(string expression, Saxon.Hej.expr.StaticContext env, bool isField) {
            this.env = env;
            this.charChecker = env.getConfiguration().getValidCharacterChecker();
            this.isFieldSubset = isField;
            t = new Saxon.Hej.expr.parser.Tokenizer();
            t.languageLevel = 20;
            Saxon.Hej.om.QNameParser qp = new Saxon.Hej.om.QNameParser(env.getNamespaceResolver());
            setQNameParser(qp);
            try {
                t.tokenize(expression, 0, -1);
            } catch (Saxon.Hej.trans.XPathException err) {
                grumble(err.Message);
            }
            Saxon.Hej.expr.Expression exp = parseUnionExpression();
            if ((t.currentToken != Saxon.Hej.expr.parser.Token.EOF)) {
                grumble((("Unexpected token "+currentTokenDisplay())+" beyond end of expression"));
            }
            exp.setLocation(makeLocation());
            exp.setRetainedStaticContext(env.makeRetainedStaticContext());
            Saxon.Hej.expr.parser.ExpressionVisitor visitor = Saxon.Hej.expr.parser.ExpressionVisitor.make(env);
            exp = exp.simplify().typeCheck(visitor, env.getConfiguration().makeContextItemStaticInfo(Saxon.Hej.pattern.AnyNodeTest.getInstance(), true));
            optimizedExpression = exp;
            System.Collections.Generic.IList<string> reasonsForFailure = new System.Collections.Generic.List<string>();
            Saxon.Hej.pattern.Pattern pattern = /*134*/Saxon.Eej.ee.stream.StreamingPatternMaker.makeStreamingPattern(exp, env.getConfiguration(), reasonsForFailure);
            if (!(reasonsForFailure.Count==0)) {
                throw new Saxon.Hej.trans.XPathException(reasonsForFailure[0]);
            }
            return pattern;
        }
        protected internal Saxon.Hej.expr.Expression parseUnionExpression() {
            Saxon.Hej.expr.Expression exp = parseRelativePath();
            while ((t.currentToken == Saxon.Hej.expr.parser.Token.UNION)) {
                if (t.currentTokenValue.Equals("union")) {
                    grumble("Union operator in the selector/field XPath subset must be written as '|'");
                }
                nextToken();
                exp = new Saxon.Hej.expr.VennExpression(exp, Saxon.Hej.expr.parser.Token.UNION, parseRelativePath());
                setLocation(exp);
            }
            return exp;
        }
        protected internal override Saxon.Hej.expr.Expression parseRelativePath() {
            bool first = true;
            Saxon.Hej.expr.Expression exp = parseStepExpression();
            while (((t.currentToken == Saxon.Hej.expr.parser.Token.SLASH) || (t.currentToken == Saxon.Hej.expr.parser.Token.SLASH_SLASH))) {
                if ((t.currentToken == Saxon.Hej.expr.parser.Token.SLASH_SLASH)) {
                    if (!(((first && (exp is Saxon.Hej.expr.ContextItemExpression))))) {
                        grumble("The // operator can be used only at the start of the path, preceded by '.'");
                    }
                }
                first = false;
                int op = t.currentToken;
                nextToken();
                Saxon.Hej.expr.Expression next = parseStepExpression();
                if ((op == Saxon.Hej.expr.parser.Token.SLASH)) {
                    exp = new Saxon.Hej.expr.SlashExpression(exp, next);
                } else {
                    Saxon.Hej.expr.AxisExpression ae = new Saxon.Hej.expr.AxisExpression(Saxon.Hej.om.AxisInfo.DESCENDANT_OR_SELF, null);
                    setLocation(ae);
                    Saxon.Hej.expr.SlashExpression se = new Saxon.Hej.expr.SlashExpression(ae, next);
                    setLocation(se);
                    if ((exp is Saxon.Hej.expr.ContextItemExpression)) {
                        exp = se;
                    } else {
                        exp = new Saxon.Hej.expr.SlashExpression(exp, se);
                    }
                }
                setLocation(exp);
            }
            return exp;
        }
        protected internal Saxon.Hej.expr.Expression parseStepExpression() {
            switch (t.currentToken) {
              case Saxon.Hej.expr.parser.Token.DOT:
                nextToken();
                Saxon.Hej.expr.Expression cie = new Saxon.Hej.expr.ContextItemExpression();
                setLocation(cie);
                return cie;
              case Saxon.Hej.expr.parser.Token.NAME:
              case Saxon.Hej.expr.parser.Token.PREFIX:
              case Saxon.Hej.expr.parser.Token.STAR:
                Saxon.Hej.expr.Expression ae = new Saxon.Hej.expr.AxisExpression(Saxon.Hej.om.AxisInfo.CHILD, parseNodeTest(Saxon.Hej.type.Type.ELEMENT));
                setLocation(ae);
                return ae;
              case Saxon.Hej.expr.parser.Token.AT:
                if (!(isFieldSubset)) {
                    grumble("The attribute axis cannot be used in a selector XPath");
                }
                nextToken();
                switch (t.currentToken) {
                  case Saxon.Hej.expr.parser.Token.NAME:
                  case Saxon.Hej.expr.parser.Token.PREFIX:
                  case Saxon.Hej.expr.parser.Token.STAR:
                    Saxon.Hej.expr.Expression ae2 = new Saxon.Hej.expr.AxisExpression(Saxon.Hej.om.AxisInfo.ATTRIBUTE, parseNodeTest(Saxon.Hej.type.Type.ATTRIBUTE));
                    setLocation(ae2);
                    return ae2;
                  default:
                    grumble((("Unexpected token "+currentTokenDisplay())+" after axis name"));
                    break ;
                }
                break ;
              case Saxon.Hej.expr.parser.Token.AXIS:
                int axis = Saxon.Hej.om.AxisInfo.getAxisNumber(t.currentTokenValue);
                if (((axis != Saxon.Hej.om.AxisInfo.CHILD) && (axis != Saxon.Hej.om.AxisInfo.ATTRIBUTE))) {
                    grumble("Only the child and attribute axes are permitted");
                }
                if (((axis == Saxon.Hej.om.AxisInfo.ATTRIBUTE) && !(isFieldSubset))) {
                    grumble("The attribute axis cannot be used in a selector XPath");
                }
                short principalNodeType = Saxon.Hej.om.AxisInfo.principalNodeType[axis];
                nextToken();
                switch (t.currentToken) {
                  case Saxon.Hej.expr.parser.Token.NAME:
                  case Saxon.Hej.expr.parser.Token.PREFIX:
                  case Saxon.Hej.expr.parser.Token.STAR:
                    Saxon.Hej.expr.Expression ae3 = new Saxon.Hej.expr.AxisExpression(axis, parseNodeTest(principalNodeType));
                    setLocation(ae3);
                    return ae3;
                  default:
                    grumble((("Unexpected token "+currentTokenDisplay())+" after axis name"));
                    break ;
                }
                break ;
              default:
                grumble((("Unexpected token "+currentTokenDisplay())+" in path expression"));
                break ;
            }
            return null;
        }
        public Saxon.Hej.expr.Expression getOptimizedExpression() {
            return optimizedExpression;
        }
    
    }
}