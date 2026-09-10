package com.saxonica.functions.sql;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.om.ZeroOrMore;
import net.sf.saxon.trans.SaxonErrorCode;
import net.sf.saxon.trans.XPathException;

import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.util.ArrayList;
import java.util.List;


/**
 * This class implements the sql:query() extension function. Arguments:
 * <ol>
 *     <li>The connection</li>
 *     <li>The table name</li>
 *     <li>A sequence of column names</li>
 *     <li>(optional) A SQL predicate selecting the rows to be selected</li>
 * </ol>
 *
 * <p>The result is a sequence of maps, one for each selected row: the maps hold one entry
 * for each requested column, with the corresponding value from the table.</p>
 */

public class SQLQueryFn extends SystemFunction {

    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        Connection connection = SQLFunctionSet.expectConnection(arguments[0], context);
        String tableName = arguments[1].head().getStringValue();
        List<String> columnNames = new ArrayList<>();
        Item it;
        SequenceIterator colIter = arguments[2].iterate();
        while ((it = colIter.next()) != null) {
            columnNames.add(it.getStringValue());
        }
        String predicate = null;
        if (arguments.length >= 4) {
            predicate = arguments[3].head().getStringValue();
        }

        PreparedStatement ps = null;

        try {
            StringBuilder statement = new StringBuilder();
            statement.append("SELECT ");

            boolean first = true;
            for (String c : columnNames) {
                if (!first) {
                    statement.append(", ");
                }
                statement.append(c);
                first = false;
            }

            statement.append(" FROM ").append(tableName);
            if (predicate != null && !"".equals(predicate)) {
                statement.append(" WHERE ").append(predicate);
            }
            //System.err.println("-> SQL: " + statement.toString());

            // -- Prepare the SQL statement
            try {
                ps = connection.prepareStatement(statement.toString());
            } catch (SQLException e) {
                throw new XPathException("sql:query failed: " + e.getMessage() +
                        " Failed to prepare query {" + statement + '}',
                                         SaxonErrorCode.SXSQ0004, context);

            }
            // -- Execute Statement
            ResultSet rs = ps.executeQuery();

            List<MapItem> out = SQLFunctionSet.sqlQueryResult(rs);

            if (!connection.getAutoCommit()) {
                connection.commit();
            }
            return new ZeroOrMore<>(out);
        } catch (SQLException ex) {
            throw new XPathException("sql:query() failed: " + ex.getMessage(), SaxonErrorCode.SXSQ0004, context);
        } finally {
            if (ps != null) {
                try {
                    ps.close();
                } catch (SQLException ignore) {
                }
            }
        }

    }

}


// Copyright (c) 2018-2023 Saxonica Limited
