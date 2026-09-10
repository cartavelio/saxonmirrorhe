package com.saxonica.functions.sql;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.SaxonErrorCode;
import net.sf.saxon.trans.XPathException;

import java.sql.Connection;
import java.sql.DatabaseMetaData;
import java.sql.ResultSet;
import java.sql.SQLException;


/**
 * This class implements the sql:table() extension function. Arguments:
 * <ol>
 *     <li>The connection</li>
 *     <li>The table name</li>
 * </ol>
 *
 * <p>The result is conceptually a sequence of maps, one for each selected row: the maps hold one entry
 * for each requested column, with the corresponding value from the table.</p>
 *
 * <p>However, the result is virtual; predicates applied to this sequence of maps are
 * (wherever possible) converted to SQL queries, so that only the required rows are
 * fetched from the database.</p>
 */

public class SQLTableFn extends SystemFunction {

    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        Connection connection = SQLFunctionSet.expectConnection(arguments[0], context);
        String tableName = arguments[1].head().getStringValue();

        try {
            DatabaseMetaData dbm = connection.getMetaData();
            ResultSet rs = dbm.getTables(null, null, tableName, null);
            if (!rs.next()) {
                throw new XPathException("sql:table() failed: table " + tableName + " does not exist", SaxonErrorCode.SXSQ0004, context);
            }

            return new SQLTable(connection, tableName, context);
        } catch (SQLException ex) {
            throw new XPathException("sql:query() failed: " + ex.getMessage(), SaxonErrorCode.SXSQ0004, context);
        }

    }
}


// Copyright (c) 2018-2022 Saxonica Limited
