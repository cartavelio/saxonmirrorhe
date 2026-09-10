using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.update {
    public class UpdateParser {
        private UpdateParser()  {
        }
        public static Saxon.Hej.expr.Expression parseTransformExpression(Saxon.Hej.expr.parser.XPathParser p) {
            Saxon.Hej.expr.parser.Tokenizer t = p.getTokenizer();
            int offset = t.currentTokenStartOffset;
            System.Collections.Generic.IList<Saxon.Eej.ee.update.UpdateParser.CopyClause> clauseList = new System.Collections.Generic.List<Saxon.Eej.ee.update.UpdateParser.CopyClause>(4);
            while (true) {
                if ((t.currentToken == Saxon.Hej.expr.parser.Token.COPY)) {
                    parseCopyClause(p, clauseList);
                } else {
                    break ;
                }
            }
            int modifyOffset = t.currentTokenStartOffset;
            p.expect(Saxon.Hej.expr.parser.Token.MODIFY);
            t.setState(Saxon.Hej.expr.parser.Tokenizer.DEFAULT_STATE);
            p.nextToken();
            Saxon.Hej.expr.Expression updateExp = p.parseExprSingle();
            updateExp = p.makeTracer(updateExp, null);
            int returnOffset = t.currentTokenStartOffset;
            p.expect(Saxon.Hej.expr.parser.Token.RETURN);
            t.setState(Saxon.Hej.expr.parser.Tokenizer.DEFAULT_STATE);
            p.nextToken();
            Saxon.Hej.expr.Expression returnExp = p.parseExprSingle();
            returnExp = p.makeTracer(returnExp, null);
            int revalidationMode = ((Saxon.Hej.query.QueryModule)(p.getStaticContext())).getRevalidationMode();
            Saxon.Eej.ee.update.CopyModifyExpression copyExp = new Saxon.Eej.ee.update.CopyModifyExpression(updateExp, returnExp, revalidationMode);
            Saxon.Hej.expr.Expression e = copyExp;
            System.Collections.Generic.IList<Saxon.Hej.expr.Expression> copyBindings = new System.Collections.Generic.List<Saxon.Hej.expr.Expression>(clauseList.Count);
            for (int i = (clauseList.Count-1); (i>=0); i--) {
                Saxon.Eej.ee.update.UpdateParser.CopyClause clause = clauseList[i];
                Saxon.Hej.expr.LetExpression let = clause.binding;
                Saxon.Hej.expr.VariableReference var = new Saxon.Hej.expr.LocalVariableReference(let);
                copyBindings.Add(var);
                let.setAction(e);
                e = let;
            }
            copyExp.setCopyBindings(Saxon.Hej.expr.instruct.Block.makeBlock(copyBindings));
            for (int i = (clauseList.Count-1); (i>=0); i--) {
                p.undeclareRangeVariable();
            }
            p.setLocation(e, offset);
            return p.makeTracer(e, null);
        }
        private static void parseCopyClause(Saxon.Hej.expr.parser.XPathParser p, System.Collections.Generic.IList<Saxon.Eej.ee.update.UpdateParser.CopyClause> clauseList) {
            bool first = true;
            Saxon.Hej.expr.parser.Tokenizer t = p.getTokenizer();
            do {
                Saxon.Eej.ee.update.UpdateParser.CopyClause clause = new Saxon.Eej.ee.update.UpdateParser.CopyClause();
                if (first) {
                    clause.offset = t.currentTokenStartOffset;
                }
                clauseList.Add(clause);
                p.nextToken();
                if (first) {
                    first = false;
                } else {
                    clause.offset = t.currentTokenStartOffset;
                }
                p.expect(Saxon.Hej.expr.parser.Token.DOLLAR);
                p.nextToken();
                p.expect(Saxon.Hej.expr.parser.Token.NAME);
                string var = t.currentTokenValue;
                Saxon.Hej.expr.LetExpression v = new Saxon.Hej.expr.LetExpression();
                Saxon.Hej.om.StructuredQName varQName = p.makeStructuredQName(var, "");
                v.setRequiredType(Saxon.Hej.value.SequenceType.SINGLE_NODE);
                v.setVariableQName(varQName);
                clause.binding = v;
                p.nextToken();
                p.expect(Saxon.Hej.expr.parser.Token.ASSIGN);
                p.nextToken();
                Saxon.Hej.expr.Expression rhs = p.parseExprSingle();
                Saxon.Hej.expr.parser.RoleDiagnostic role = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.UPDATING_EXPR, "copyModify", 0);
                role.setErrorCode("XUTY0013");
                rhs = new Saxon.Hej.expr.ItemChecker(rhs, Saxon.Hej.pattern.AnyNodeTest.getInstance(), role);
                rhs = Saxon.Hej.expr.CardinalityChecker.makeCardinalityChecker(rhs, Saxon.Hej.expr.StaticProperty.EXACTLY_ONE, role);
                bool preserve = ((Saxon.Hej.query.QueryModule)(p.getStaticContext())).isPreserveNamespaces();
                Saxon.Hej.expr.instruct.CopyOf copy = new Saxon.Hej.expr.instruct.CopyOf(rhs, preserve, Saxon.Hej.lib.Validation.STRIP, null, true);
                copy.setCopyForUpdate(true);
                v.setSequence(copy);
                p.declareRangeVariable(v);
            } while ((t.currentToken == Saxon.Hej.expr.parser.Token.COMMA));
        }
        private class CopyClause {
            public Saxon.Hej.expr.LetExpression binding;
            public int offset;
        
        }
        public static Saxon.Hej.expr.Expression parseInsertExpression(Saxon.Hej.expr.parser.XPathParser p) {
            Saxon.Hej.expr.parser.Tokenizer t = p.getTokenizer();
            int offset = t.currentTokenStartOffset;
            p.nextToken();
            Saxon.Hej.expr.Expression source = p.parseExprSingle();
            int op = t.currentToken;
            int position = op;
            if ((op == Saxon.Hej.expr.parser.Token.AS)) {
                p.nextToken();
                op = t.currentToken;
                if (((op == Saxon.Hej.expr.parser.Token.FIRST_INTO) || (op == Saxon.Hej.expr.parser.Token.LAST_INTO))) {
                    position = op;
                } else {
                    p.grumble("Expected 'as first into' or 'as last into'");
                }
            } else {
                if ((((op == Saxon.Hej.expr.parser.Token.AFTER) || (op == Saxon.Hej.expr.parser.Token.BEFORE)) || (op == Saxon.Hej.expr.parser.Token.INTO))) {
                    position = op;
                } else {
                    p.grumble("insert position must be one of: before | after | as first into | as last into | into");
                }
            }
            p.nextToken();
            Saxon.Hej.expr.Expression target = p.parseExprSingle();
            Saxon.Hej.query.QueryModule env = (Saxon.Hej.query.QueryModule)(p.getStaticContext());
            int constructionMode = env.getConstructionMode();
            bool inherit = env.isInheritNamespaces();
            bool preserve = env.isPreserveNamespaces();
            Saxon.Hej.expr.Expression e = new Saxon.Eej.ee.update.InsertExpression(source, target, position, constructionMode, inherit, preserve);
            return p.makeTracer(e, null);
        }
        public static Saxon.Hej.expr.Expression parseDeleteExpression(Saxon.Hej.expr.parser.XPathParser p) {
            int offset = p.getTokenizer().currentTokenStartOffset;
            p.nextToken();
            Saxon.Hej.expr.Expression target = p.parseExprSingle();
            Saxon.Hej.expr.Expression e = new Saxon.Eej.ee.update.DeleteExpression(target);
            return p.makeTracer(e, null);
        }
        public static Saxon.Hej.expr.Expression parseReplaceNodeExpression(Saxon.Hej.expr.parser.XPathParser p) {
            int offset = p.getTokenizer().currentTokenStartOffset;
            p.nextToken();
            Saxon.Hej.expr.Expression target = p.parseExprSingle();
            p.expect(Saxon.Hej.expr.parser.Token.WITH);
            p.nextToken();
            Saxon.Hej.expr.Expression replacement = p.parseExprSingle();
            Saxon.Hej.query.QueryModule env = (Saxon.Hej.query.QueryModule)(p.getStaticContext());
            Saxon.Hej.expr.Expression e = new Saxon.Eej.ee.update.ReplaceNodeExpression(target, replacement, env.getConstructionMode(), env.isInheritNamespaces());
            return p.makeTracer(e, null);
        }
        public static Saxon.Hej.expr.Expression parseReplaceValueExpression(Saxon.Hej.expr.parser.XPathParser p) {
            int offset = p.getTokenizer().currentTokenStartOffset;
            p.nextToken();
            Saxon.Hej.expr.Expression target = p.parseExprSingle();
            p.expect(Saxon.Hej.expr.parser.Token.WITH);
            p.nextToken();
            Saxon.Hej.expr.Expression replacement = p.parseExprSingle();
            Saxon.Hej.expr.Expression e = new Saxon.Eej.ee.update.ReplaceValueExpression(target, replacement);
            return p.makeTracer(e, null);
        }
        public static Saxon.Hej.expr.Expression parseRenameExpression(Saxon.Hej.expr.parser.XPathParser p) {
            int offset = p.getTokenizer().currentTokenStartOffset;
            Saxon.Hej.query.QueryModule env = (Saxon.Hej.query.QueryModule)(p.getStaticContext());
            p.nextToken();
            Saxon.Hej.expr.Expression target = p.parseExprSingle();
            p.expect(Saxon.Hej.expr.parser.Token.AS);
            p.nextToken();
            Saxon.Hej.expr.Expression newName = p.parseExprSingle();
            bool inherit = env.isInheritNamespaces();
            Saxon.Hej.expr.Expression e = new Saxon.Eej.ee.update.RenameExpression(target, newName, env.getNamespaceResolver(), env.getDefaultElementNamespace(), inherit);
            return p.makeTracer(e, null);
        }
    
    }
}