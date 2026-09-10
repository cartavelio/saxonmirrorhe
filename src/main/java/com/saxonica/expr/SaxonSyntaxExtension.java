package com.saxonica.expr;

import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.parser.ParserExtension;
import net.sf.saxon.expr.parser.XPathParser;
import net.sf.saxon.om.FunctionItem;
import net.sf.saxon.pattern.ItemTypePattern;
import net.sf.saxon.query.QueryModule;
import net.sf.saxon.query.XQueryFunction;
import net.sf.saxon.query.XQueryParser;
import net.sf.saxon.trans.SymbolicName;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.ItemType;
import net.sf.saxon.type.TypeHierarchy;

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
        SymbolicName.F fName = new SymbolicName.F(func.getFunctionName(), func.getNumberOfParameters());
        if (!qenv.getFunctionLibrary().isAvailable(fName, qenv.getPackageData().getImportedSchema(), qenv.getXPathVersion())) {
            p.grumble("External function " + fName + " is not available", "XPST0017");
        }

        FunctionItem ext = qenv.getFunctionLibrary().getFunctionItem(fName, qenv);
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


}

// Copyright (c) 2010-2023 Saxonica Limited