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
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.om.Function;
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

//    /**
//     * Parse an ItemType within a SequenceType
//     *
//     * @return the ItemType after parsing
//     * @throws XPathException if a static error is found
//     */
//
//    @Override
//    public ItemType parseExtendedItemType(XPathParser p) throws XPathException {
//        Tokenizer t = p.getTokenizer();
//        if (t.currentToken == Token.FUNCTION && t.currentTokenValue.equals("type")) {
//            // Saxon extension: type aliases
//            p.checkSyntaxExtensions("type alias");
//            p.nextToken();
//            p.expect(Token.NAME);
//            StructuredQName typeAlias = p.getQNameParser().parse(t.currentTokenValue, "");
//            ItemType realType = p.getStaticContext().resolveTypeAlias(typeAlias);
//            if (realType == null) {
//                p.grumble("Named type(" + typeAlias.getDisplayName() + ") has not been defined");
//            }
//            p.nextToken();
//            p.expect(Token.RPAR);
//            p.nextToken();
//            return realType;
//        } else {
//            return null;
//        }
//    }

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



    public static class UnderscoreFunctionDetails extends XPathParser.InlineFunctionDetails {
        List<UserFunctionParameter> numberedParams = new ArrayList<>();
    }



    @Override
    public Expression parseDotFunction(XPathParser p) throws XPathException {
        p.checkSyntaxExtensions("dot function");
        Tokenizer t = p.getTokenizer();
        int offset = t.currentTokenStartOffset;

        XPathParser.InlineFunctionDetails details = new XPathParser.InlineFunctionDetails();
        details.outerVariables = new IndexedStack<LocalBinding>();
        for (LocalBinding lb : p.getRangeVariables()) {
            details.outerVariables.push(lb);
        }
        details.outerVariablesUsed = new ArrayList<>(4);
        details.implicitParams = new ArrayList<>(4);
        p.inlineFunctionStack.push(details);
        p.setRangeVariables(new IndexedStack<LocalBinding>());
        p.nextToken();
        List<UserFunctionParameter> params = new ArrayList<>(1);
        SequenceType resultType = SequenceType.ANY_SEQUENCE;

        StructuredQName argQName = new StructuredQName("saxon", NamespaceConstant.SAXON, "dot");

        UserFunctionParameter arg = new UserFunctionParameter();
        arg.setRequiredType(SequenceType.SINGLE_ITEM);
        arg.setVariableQName(argQName);
        arg.setSlotNumber(0);
        params.add(arg);

        Expression body = p.parseExpression();
        p.expect(Token.RCURLY);
        t.lookAhead();  // must be done manually after an RCURLY
        p.nextToken();
        body.setRetainedStaticContext(p.getStaticContext().makeRetainedStaticContext());

        LocalVariableReference ref = new LocalVariableReference(arg);
        body = new ForEach(ref, body);
        Expression result = XPathParser.makeInlineFunctionValue(p, AnnotationList.EMPTY, details, params, resultType, body);

        p.setLocation(result, offset);
        // restore the previous stack of range variables
        p.setRangeVariables(details.outerVariables);
        p.inlineFunctionStack.pop();
        return result;
    }

    /**
     * Saxon extension: inline functions of the form <code>_{$1 + $2}</code>
     * @param p the XPath parser
     * @return the compiled expression
     * @throws XPathException if syntax is incorrect
     */

    @Override
    public Expression parseUnderscoreFunction(XPathParser p) throws XPathException {
        p.checkSyntaxExtensions("underscore function");
        Tokenizer t = p.getTokenizer();
        int offset = t.currentTokenStartOffset;

        UnderscoreFunctionDetails details = new UnderscoreFunctionDetails();
        details.outerVariables = new IndexedStack<LocalBinding>();
        for (LocalBinding lb : p.getRangeVariables()) {
            details.outerVariables.push(lb);
        }
        details.outerVariablesUsed = new ArrayList<LocalBinding>(4);
        details.implicitParams = new ArrayList<UserFunctionParameter>(4);
        p.inlineFunctionStack.push(details);
        p.setRangeVariables(new IndexedStack<LocalBinding>());
        p.nextToken();
        SequenceType resultType = SequenceType.ANY_SEQUENCE;

        Expression body = p.parseExpression();
        p.expect(Token.RCURLY);
        t.lookAhead();  // must be done manually after an RCURLY
        p.nextToken();
        body.setRetainedStaticContext(p.getStaticContext().makeRetainedStaticContext());

        List<UserFunctionParameter> params = details.numberedParams;
        Expression result = XPathParser.makeInlineFunctionValue(p, AnnotationList.EMPTY, details, params, resultType, body);

        p.setLocation(result, offset);
        // restore the previous stack of range variables
        p.setRangeVariables(details.outerVariables);
        p.inlineFunctionStack.pop();
        return result;
    }

    @Override
    public Expression bindNumericParameterReference(XPathParser p) throws XPathException {
        p.checkSyntaxExtensions("underscore function");
        Tokenizer t = p.getTokenizer();
        int n = -1;
        try {
            n = Integer.parseInt(t.currentTokenValue);
        } catch (NumberFormatException err) {
            p.grumble("Numeric parameter reference must be an integer");
        }
        if (n <= 0) {
            p.grumble("Numeric parameter reference must be a positive integer");
        }
        if (p.inlineFunctionStack.isEmpty() || !(p.inlineFunctionStack.peek() instanceof UnderscoreFunctionDetails)) {
            p.grumble("Numeric parameter reference must be contained directly in an underscore function");
        }
        UnderscoreFunctionDetails fn = (UnderscoreFunctionDetails)p.inlineFunctionStack.peek();
        if (n > fn.numberedParams.size()) {
            for (int k=fn.numberedParams.size(); k < n; k++) {
                UserFunctionParameter arg = new UserFunctionParameter();
                arg.setRequiredType(SequenceType.ANY_SEQUENCE);
                StructuredQName argQName = new StructuredQName("_", NamespaceConstant.ANONYMOUS, "_" + (k + 1));
                arg.setVariableQName(argQName);
                arg.setSlotNumber(k);
                fn.numberedParams.add(arg);
            }
        }
        p.nextToken();
        return new LocalVariableReference(fn.numberedParams.get(n-1));
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

            v.setVariableQName(p.makeStructuredQName(var, ""));
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