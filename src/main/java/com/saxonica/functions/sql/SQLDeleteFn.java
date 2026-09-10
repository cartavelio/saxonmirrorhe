package com.saxonica.functions.sql;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.SaxonErrorCode;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.EmptySequence;

import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.SQLException;


/**
 * This class implements the sql:delete() extension function. Arguments:
 * <ol>
 *     <li>The connection</li>
 *     <li>The table name</li>
 *     <li>A SQL predicate selecting the rows to be deleted</li>
 * </ol>
 */

public class SQLDeleteFn extends SystemFunction {

    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        Connection connection = SQLFunctionSet.expectConnection(arguments[0], context);
        String tableName = arguments[1].head().getStringValue();
        String predicate = arguments[2].head().getStringValue();

        String statement = "DELETE FROM " + tableName;

        if (!predicate.equals("")) {
            statement += " WHERE " + predicate;
        }

        try (PreparedStatement ps = connection.prepareStatement(statement)) {
            ps.executeUpdate();
        } catch (SQLException ex) {
            throw new XPathException("sql:delete() failed: " + ex.getMessage(), SaxonErrorCode.SXSQ0004, context);
        }

        return EmptySequence.getInstance();

    }
}


// Copyright (c) 2018-2022 Saxonica Limited
