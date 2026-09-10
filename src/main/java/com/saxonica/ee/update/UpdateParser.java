package com.saxonica.ee.update;

import net.sf.saxon.expr.*;
import net.sf.saxon.expr.instruct.Block;
import net.sf.saxon.expr.instruct.CopyOf;
import net.sf.saxon.expr.parser.XPathParser;
import net.sf.saxon.expr.parser.RoleDiagnostic;
import net.sf.saxon.expr.parser.Token;
import net.sf.saxon.expr.parser.Tokenizer;
import net.sf.saxon.lib.Validation;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.pattern.AnyNodeTest;
import net.sf.saxon.query.QueryModule;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.SequenceType;

import java.util.ArrayList;
import java.util.List;

/**
 * Parser utility methods for the XQuery Update extensions
 */
public class UpdateParser {

    // Disallow instantiation
    private UpdateParser() {
    }

    /**
     * Parse the "transform" (that is, copy/modify/return) expression
     * Syntax: "copy" "$" VarName ":=" ExprSingle ("," "$" VarName ":=" ExprSingle)*
     * "modify" ExprSingle "return" ExprSingle
     *
     * @param p the calling expression parser
     * @return the resulting expression
     * @throws XPathException in the event of a syntax error
     */

    public static Expression parseTransformExpression(XPathParser p) throws XPathException {
        Tokenizer t = p.getTokenizer();
        int offset = t.currentTokenStartOffset;
        List<CopyClause> clauseList = new ArrayList<CopyClause>(4);
        while (true) {
            if (t.currentToken == Token.COPY) {
                parseCopyClause(p, clauseList);
            } else {
                break;
            }
        }

        int modifyOffset = t.currentTokenStartOffset;
        p.expect(Token.MODIFY);
        t.setState(Tokenizer.DEFAULT_STATE);
        p.nextToken();
        Expression updateExp = p.parseExprSingle();
        updateExp = p.makeTracer(updateExp, null);

        int returnOffset = t.currentTokenStartOffset;
        p.expect(Token.RETURN);
        t.setState(Tokenizer.DEFAULT_STATE);
        p.nextToken();
        Expression returnExp = p.parseExprSingle();
        returnExp = p.makeTracer(returnExp, null);

        int revalidationMode = ((QueryModule) p.getStaticContext()).getRevalidationMode();
        CopyModifyExpression copyExp = new CopyModifyExpression(updateExp, returnExp, revalidationMode);
        Expression e = copyExp;
        List<Expression> copyBindings = new ArrayList<Expression>(clauseList.size());
        for (int i = clauseList.size() - 1; i >= 0; i--) {
            CopyClause clause = clauseList.get(i);
            LetExpression let = clause.binding;
            VariableReference var = new LocalVariableReference(let);
            copyBindings.add(var);
            let.setAction(e);
            e = let;
        }
        copyExp.setCopyBindings(Block.makeBlock(copyBindings));

        // undeclare all the range variables

        for (int i = clauseList.size() - 1; i >= 0; i--) {
            // undeclare the primary variable
            p.undeclareRangeVariable();
        }

        p.setLocation(e, offset);
        return p.makeTracer(e, null);
    }

    /**
     * Parse a CopyClause.
     * <p>
     * [44] CopyClause ::= <"copy" "$"> VarName ":=" ExprSingle
     * ("," "$" VarName ":=" ExprSingle)*
     * </p>
     *
     * @param p          the calling expression parser
     * @param clauseList - the components of the parsed LetClause are appended to the
     *                   supplied list
     * @throws XPathException in the event of a syntax error
     */
    private static void parseCopyClause(XPathParser p, List<CopyClause> clauseList) throws XPathException {
        boolean first = true;
        Tokenizer t = p.getTokenizer();
        do {
            CopyClause clause = new CopyClause();
            if (first) {
                clause.offset = t.currentTokenStartOffset;
            }
            clauseList.add(clause);
            p.nextToken();
            if (first) {
                first = false;
            } else {
                clause.offset = t.currentTokenStartOffset;
            }
            p.expect(Token.DOLLAR);
            p.nextToken();
            p.expect(Token.NAME);
            String var = t.currentTokenValue;

            LetExpression v = new LetExpression();
            StructuredQName varQName = p.makeStructuredQName(var, "");
            v.setRequiredType(SequenceType.SINGLE_NODE);
            v.setVariableQName(varQName);
            clause.binding = v;
            p.nextToken();

            p.expect(Token.ASSIGN);
            p.nextToken();
            Expression rhs = p.parseExprSingle();
            RoleDiagnostic role = new RoleDiagnostic(RoleDiagnostic.UPDATING_EXPR, "copyModify", 0);
            role.setErrorCode("XUTY0013");
            rhs = new ItemChecker(rhs, AnyNodeTest.getInstance(), role);
            rhs = CardinalityChecker.makeCardinalityChecker(rhs, StaticProperty.EXACTLY_ONE, role);
            boolean preserve = ((QueryModule) p.getStaticContext()).isPreserveNamespaces();
            CopyOf copy = new CopyOf(rhs, preserve, Validation.STRIP, null, true);
            copy.setCopyForUpdate(true);
            v.setSequence(copy);
            p.declareRangeVariable(v);
        } while (t.currentToken == Token.COMMA);
    }

    private static class CopyClause {
        public LetExpression binding;
        public int offset;
    }


    /**
     * Parse the insert expression
     * Syntax: insert_node Expr choice Expr
     * choice: "as first into" | "as last into" | "into" | "after" | "before"
     *
     * @param p the calling expression parser
     * @return the InsertExpression
     * @throws XPathException in the event of a syntax error
     */

    public static Expression parseInsertExpression(XPathParser p) throws XPathException {
        Tokenizer t = p.getTokenizer();
        int offset = t.currentTokenStartOffset;
        p.nextToken();
        Expression source = p.parseExprSingle();

        int op = t.currentToken;
        int position = op;
        if (op == Token.AS) {
            p.nextToken();
            op = t.currentToken;
            if (op == Token.FIRST_INTO || op == Token.LAST_INTO) {
                position = op;
            } else {
                p.grumble("Expected 'as first into' or 'as last into'");
            }
        } else if (op == Token.AFTER || op == Token.BEFORE || op == Token.INTO) {
            position = op;
        } else {
            p.grumble("insert position must be one of: before | after | as first into | as last into | into");
        }
        p.nextToken();
        Expression target = p.parseExprSingle();
        QueryModule env = (QueryModule) p.getStaticContext();
        int constructionMode = env.getConstructionMode();
        boolean inherit = env.isInheritNamespaces();
        boolean preserve = env.isPreserveNamespaces();
        Expression e = new InsertExpression(source, target, position, constructionMode, inherit, preserve);
        return p.makeTracer(e, null);
    }

    /**
     * Parse a delete expression.
     * Syntax: delete_node TargetExpr
     *
     * @param p the calling expression parser
     * @return the expression
     * @throws XPathException in the event of a syntax error
     */

    public static Expression parseDeleteExpression(XPathParser p) throws XPathException {
        int offset = p.getTokenizer().currentTokenStartOffset;
        p.nextToken();
        Expression target = p.parseExprSingle();
        Expression e = new DeleteExpression(target);
        return p.makeTracer(e, null);
    }

    /**
     * Parse a "replace node" expression.
     * Syntax: replace_node TargetExpr with ExprSingle
     *
     * @param p the calling expression parser
     * @return the expression
     * @throws XPathException in the event of a syntax error
     */

    public static Expression parseReplaceNodeExpression(XPathParser p) throws XPathException {
        int offset = p.getTokenizer().currentTokenStartOffset;
        p.nextToken();
        Expression target = p.parseExprSingle();
        p.expect(Token.WITH);
        p.nextToken();
        Expression replacement = p.parseExprSingle();
        final QueryModule env = (QueryModule) p.getStaticContext();
        Expression e = new ReplaceNodeExpression(target, replacement, env.getConstructionMode(), env.isInheritNamespaces());
        return p.makeTracer(e, null);
    }

    /**
     * Parse a "replace value" expression.
     * Syntax: replace_value "of" "node" TargetExpr with ExprSingle
     *
     * @param p the calling expression parser
     * @return the expression
     * @throws XPathException in the event of a syntax error
     */

    public static Expression parseReplaceValueExpression(XPathParser p) throws XPathException {
        int offset = p.getTokenizer().currentTokenStartOffset;
        p.nextToken();
        // The tokenizer swallows the "of node"
        Expression target = p.parseExprSingle();
        p.expect(Token.WITH);
        p.nextToken();
        Expression replacement = p.parseExprSingle();
        Expression e = new ReplaceValueExpression(target, replacement);
        return p.makeTracer(e, null);
    }

    /**
     * Parse a "rename" expression.
     * Syntax: rename_node TargetExpr as ExprSingle
     *
     * @param p the calling expression parser
     * @return the expression
     * @throws XPathException in the event of a syntax error
     */

    public static Expression parseRenameExpression(XPathParser p) throws XPathException {
        int offset = p.getTokenizer().currentTokenStartOffset;
        QueryModule env = (QueryModule) p.getStaticContext();
        p.nextToken();
        Expression target = p.parseExprSingle();
        p.expect(Token.AS);
        p.nextToken();
        Expression newName = p.parseExprSingle();
        boolean inherit = env.isInheritNamespaces();
        Expression e = new RenameExpression(target, newName, env.getNamespaceResolver(),
                                            env.getDefaultElementNamespace(), inherit);
        return p.makeTracer(e, null);
    }
}

// Copyright (c) 2008-2022 Saxonica Limited
