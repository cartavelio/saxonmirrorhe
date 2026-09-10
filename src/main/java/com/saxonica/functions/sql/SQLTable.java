package com.saxonica.functions.sql;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.om.ZeroOrMore;
import net.sf.saxon.trans.SaxonErrorCode;
import net.sf.saxon.trans.UncheckedXPathException;
import net.sf.saxon.trans.XPathException;

import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.util.List;

public class SQLTable implements Sequence /* of MapItem */ {

    private final Connection connection;
    private final String tableName;
    private final XPathContext context;

    SQLTable(Connection connection, String tableName, XPathContext context) {
        this.connection = connection;
        this.tableName = tableName;
        this.context = context;
    }

    @Override
    public MapItem head() throws XPathException {
        return (MapItem)iterate().next();
    }

    @Override
    public SequenceIterator /* of MapItem */ iterate() {
        PreparedStatement ps = null;

        try {
            StringBuilder statement = new StringBuilder();
            statement.append("SELECT * FROM ").append(tableName);

            // -- Prepare the SQL statement
            try {
                ps = connection.prepareStatement(statement.toString());
            } catch (SQLException e) {
                XPathException xpe = new XPathException("sql:table() failed: " + e.getMessage() +
                                                 " Failed to prepare query {" + statement + '}',
                                         SaxonErrorCode.SXSQ0004, context);
                throw new UncheckedXPathException(xpe);

            }
            // -- Execute Statement
            ResultSet rs = ps.executeQuery();

            List<MapItem> out = SQLFunctionSet.sqlQueryResult(rs);

            if (!connection.getAutoCommit()) {
                connection.commit();
            }
            return new ZeroOrMore<>(out).iterate();
        } catch (SQLException ex) {
            XPathException xpe2 = new XPathException(
                    "Failed to materialize sql:table(): " + ex.getMessage(), SaxonErrorCode.SXSQ0004, context);
            throw new UncheckedXPathException(xpe2);
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

// Copyright (c) 2020-2023 Saxonica Limited

