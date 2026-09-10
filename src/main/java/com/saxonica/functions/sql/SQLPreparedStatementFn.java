package com.saxonica.functions.sql;

import net.sf.saxon.expr.Callable;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.CallableFunction;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.om.FunctionItem;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.SaxonErrorCode;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.FunctionItemType;
import net.sf.saxon.type.SpecificFunctionType;
import net.sf.saxon.value.EmptySequence;
import net.sf.saxon.value.SequenceType;

import java.sql.Connection;
import java.sql.ParameterMetaData;
import java.sql.PreparedStatement;
import java.sql.SQLException;


/**
 * This class implements the sql:prepared-statement() extension function. Arguments:
 * <ol>
 *     <li>The connection</li>
 *     <li>A SQL statement, typically containing "?" placeholders</li>
 * </ol>
 *
 * <p>The result is a function, with arity equal to the number of placeholders. When calling
 * this function, the supplied arguments substitute for the placeholders in the statement, and the
 * result is an empty sequence.</p>
 */

public class SQLPreparedStatementFn extends SystemFunction {

    @Override
    public FunctionItem call(XPathContext context, Sequence[] arguments) throws XPathException {
        Connection connection = SQLFunctionSet.expectConnection(arguments[0], context);
        String statement = arguments[1].head().getStringValue();

        PreparedStatement ps = null;
        try {
            // -- Prepare the SQL statement
            try {
                ps = connection.prepareStatement(statement);
            } catch (SQLException e) {
                throw new XPathException("sql:prepared-statement() failed: " + e.getMessage() +
                        " Failed to prepare statement {" + statement + '}',
                                         SaxonErrorCode.SXSQ0004, context);
            }

            final PreparedStatement ps2 = ps;
            Callable callable = (context1, arguments1) -> {
                for (int i = 0; i< arguments1.length; i++) {
                    SQLFunctionSet.setSqlStatementParam(ps2, i+1, arguments1[i], context1);
                }
                try {
                    ps2.execute();
                    return EmptySequence.getInstance();
                } catch (SQLException e) {
                    throw new XPathException("Failure executing prepared SQL statement", e);
                }
            };

            ParameterMetaData metaData = ps2.getParameterMetaData();
            int arity = metaData.getParameterCount();
            SequenceType[] parameterTypes = new SequenceType[arity];
            SQLFunctionSet.supplyParameters(metaData, parameterTypes);
            FunctionItemType type = new SpecificFunctionType(parameterTypes, SequenceType.OPTIONAL_ITEM);
            return new CallableFunction(arity, callable, type);

        } catch (SQLException ex) {
            throw new XPathException("sql:prepared-statement() failed: " + ex.getMessage(), SaxonErrorCode.SXSQ0004, context);
        } 

    }

}


// Copyright (c) 2018-2022 Saxonica Limited
