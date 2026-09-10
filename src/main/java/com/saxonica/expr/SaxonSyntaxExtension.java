package com.saxonica.expr;

import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.ForMemberExpression;
import net.sf.saxon.expr.LocalBinding;
import net.sf.saxon.expr.LocalVariableReference;
import net.sf.saxon.expr.instruct.ForEach;
import net.sf.saxon.expr.instruct.UserFunctionParameter;
import net.sf.saxon.expr.parser.ParserExtension;
import net.sf.saxon.expr.parser.Token;
import net.sf.saxon.expr.parser.Tokenizer;
import net.sf.saxon.expr.parser.XPathParser;
import net.sf.saxon.om.Function;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.pattern.ItemTypePattern;
import net.sf.saxon.query.AnnotationList;
import net.sf.saxon.query.QueryModule;
import net.sf.saxon.query.XQueryFunction;
import net.sf.saxon.query.XQueryParser;
import net.sf.saxon.trans.SymbolicName;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.util.IndexedStack;
import net.sf.saxon.type.ItemType;
import net.sf.saxon.type.TypeHierarchy;
import net.sf.saxon.value.SequenceType;

import java.util.ArrayList;
import java.util.List;

/**
 * Parser extension for syntax in XPath that is accepted only in Saxon-PE and -EE. This means
 * Saxon-defined syntax extensions.
 */
public class SaxonSyntaxExtension extends ParserExtension {


    public SaxonSyntaxExtension() {
    }


    /**
     * Parse an extended XSLT pattern in the form item-type (predicate)*  (but excluding the
     * predicates)
     *
     * @return the equivalent expression in the form .[. instance of type] (predicate)*
     */

    @Override
    public Expression parseTypePattern(XPathParser p) throws XPathException {
        //p.nextToken();
        ItemType type = p.parseItemType();
//        Expression expr = new InstanceOfExpression(
//                new ContextItemExpression(), SequenceType.makeSequenceType(type, StaticProperty.EXACTLY_ONE));
//        expr = new FilterExpression(new ContextItemExpression(), expr);
        return new ItemTypePattern(type);
        //return expr;
    }




    @Override
    public void handleExternalFunctionDeclaration(XQueryParser p, XQueryFunction func) throws XPathException {
        QueryModule qenv = (QueryModule)p.getStaticContext();
        SymbolicName.F fName = new SymbolicName.F(func.getFunctionName(), func.getNumberOfArguments());
        if (!qenv.getFunctionLibrary().isAvailable(fName, qenv.getXPathVersion())) {
            p.grumble("External function " + fName + " is not available", "XPST0017");
        }
        Function ext = qenv.getFunctionLibrary().getFunctionItem(fName, qenv);
        if (ext == null) {
            p.grumble("External function " + fName + " is not available", "XPST0017");
        } else {
            TypeHierarchy th = qenv.getConfiguration().getTypeHierarchy();
            if (!ext.getFunctionItemType().getResultType().isSameType(func.getResultType(), th)) {
                p.grumble("The declared return type of the external function (" +
                                ext.getFunctionItemType().getResultType() +
                                ") does not match the return type of the actual function (" +
                                func.getResultType());
            }

            for (int i = 0; i < ext.getArity(); i++) {
                if (!ext.getFunctionItemType().getArgumentTypes()[i].isSameType(func.getArgumentTypes()[i], th)) {
                    p.grumble("The declared type of argument " + i + " of the external function (" +
                                    ext.getFunctionItemType().getArgumentTypes()[i] +
                                    ") does not match the argument type of the actual function (" +
                                    func.getArgumentTypes()[i]);
                }
            }
        }
    }

    /**
     * Parse a for-member expression (Saxon extension):
     * for member $x in expr return expr
     *
     * @return the resulting subexpression
     * @throws XPathException if any error is encountered
     */
    
    @Override
    protected Expression parseForMemberExpression(XPathParser p) throws XPathException {
        Tokenizer t = p.getTokenizer();
        p.checkSyntaxExtensions("'for member'");
        int clauses = 0;
        int operator = t.currentToken;
        ForMemberExpression first = null;
        ForMemberExpression previous = null;
        do {
            int offset = t.currentTokenStartOffset;
            p.nextToken();
            p.expect(Token.DOLLAR);
            p.nextToken();
            p.expect(Token.NAME);
            String var = t.currentTokenValue;
            clauses++;

            // declare the range variable
            ForMemberExpression v = new ForMemberExpression();
            p.setLocation(v, offset);

            v.setVariableQName(p.makeStructuredQName(var, NamespaceUri.NULL));
            p.nextToken();

            // process the "in" clause
            p.expect(Token.IN);
            p.nextToken();
            v.setSequence(p.parseExprSingle());
            p.declareRangeVariable(v);
            if (previous != null) {
                previous.setAction(v);
            } else {
                first = v;
            }
            previous = v;

        } while (t.currentToken == Token.COMMA);

        // process the "return" expression (called the "action")
        p.expect(Token.RETURN);
        p.nextToken();
        previous.setAction(p.parseExprSingle());


        // undeclare all the range variables

        for (int i = 0; i < clauses; i++) {
            p.undeclareRangeVariable();
        }
        return p.makeTracer(first, first.getVariableQName());

    }

}

// Copyright (c) 2010-2022 Saxonica Limited