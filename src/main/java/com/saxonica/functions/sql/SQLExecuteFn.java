package com.saxonica.functions.sql;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.EmptySequence;

import java.sql.Connection;
import java.sql.SQLException;
import java.sql.Statement;


/**
 * This class implements the sql:execute() extension function.
 */

public class SQLExecuteFn extends SystemFunction {

    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        Connection connection = SQLFunctionSet.expectConnection(arguments[0], context);
        String statementText = arguments[1].head().getStringValue();

        try {
            if ("COMMIT WORK".equals(statementText)) {
                connection.commit();
            } else if ("ROLLBACK WORK".equals(statementText)) {
                connection.rollback();
            } else {
                Statement s = connection.createStatement();
                s.execute(statementText);
                s.close();
            }

        } catch (SQLException ex) {
            XPathException de = new XPathException("sql:execute() failed: " + ex.getMessage());
            de.setXPathContext(context);
            throw de;
        }

        return EmptySequence.getInstance();
    }
}



// Copyright (c) 2018-2022 Saxonica Limited
