package com.saxonica.functions.sql;

import net.sf.saxon.expr.Callable;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.CallableFunction;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.ma.map.MapType;
import net.sf.saxon.om.Function;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.SaxonErrorCode;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.FunctionItemType;
import net.sf.saxon.type.SpecificFunctionType;
import net.sf.saxon.value.SequenceExtent;
import net.sf.saxon.value.SequenceType;

import java.sql.*;
import java.util.List;


/**
 * This class implements the sql:prepared-query() extension function. Arguments:
 * <ol>
 *     <li>The connection</li>
 *     <li>A SQL select statement, typically containing "?" placeholders</li>
 * </ol>
 *
 * <p>The result is a function, with arity equal to the number of placeholders. When calling
 * this function, the supplied arguments substitute for the placeholders in the query, and the
 * result is a sequence of maps containing the rows selected by the query.</p>
 */

public class SQLPreparedQueryFn extends SystemFunction {

    public Function call(XPathContext context, Sequence[] arguments) throws XPathException {
        Connection connection = SQLFunctionSet.expectConnection(arguments[0], context);
        String query = arguments[1].head().getStringValue();

        PreparedStatement ps = null;

        try {
            StringBuilder statement = new StringBuilder();
            statement.append("SELECT ");

            // -- Prepare the SQL statement
            try {
                ps = connection.prepareStatement(query);
            } catch (SQLException e) {
                throw new XPathException("sql:prepared-query() failed: " + e.getMessage() +
                        " Failed to prepare query {" + statement + '}',
                                         SaxonErrorCode.SXSQ0004, context);

            }

            final PreparedStatement ps2 = ps;
            Callable callable = (context1, arguments1) -> {
                for (int i = 0; i< arguments1.length; i++) {
                    SQLFunctionSet.setSqlStatementParam(ps2, i+1, arguments1[i], context1);
                }
                try {
                    ResultSet rs = ps2.executeQuery();
                    List<MapItem> out = SQLFunctionSet.sqlQueryResult(rs);
                    return SequenceExtent.makeSequenceExtent(out);
                } catch (SQLException e) {
                    throw new XPathException("Failure executing prepared SQL query", e);
                }
            };

            ParameterMetaData metaData = ps2.getParameterMetaData();
            int arity = metaData.getParameterCount();
            SequenceType[] parameterTypes = new SequenceType[arity];
            SQLFunctionSet.supplyParameters(metaData, parameterTypes);
            FunctionItemType type = new SpecificFunctionType(parameterTypes, MapType.SEQUENCE_OF_MAPS);
            return new CallableFunction(arity, callable, type);

        } catch (SQLException ex) {
            throw new XPathException("sql:prepared-query() failed: " + ex.getMessage(), SaxonErrorCode.SXSQ0004, context);
        } 

    }

}


// Copyright (c) 2018 Saxonica Limited.
