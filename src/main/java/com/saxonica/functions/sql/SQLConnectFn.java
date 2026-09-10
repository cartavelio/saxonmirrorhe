package com.saxonica.functions.sql;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.SaxonErrorCode;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.trans.XmlProcessingIncident;
import net.sf.saxon.value.ObjectValue;
import net.sf.saxon.value.StringValue;

import javax.naming.InitialContext;
import javax.naming.NamingException;
import javax.sql.DataSource;
import java.io.PrintWriter;
import java.sql.Connection;
import java.sql.DriverManager;
import java.sql.SQLException;


/**
 * This class implements the sql:connect() extension function.
 */

public class SQLConnectFn extends SystemFunction {

    @Override
    public ObjectValue<Connection> call(XPathContext context, Sequence[] arguments) throws XPathException {

        if (context.getConfiguration().getBooleanProperty(Feature.ALLOW_MULTITHREADING)) {
            context.getController().getErrorReporter().report(
                    new XmlProcessingIncident("SQL JDBC Connections are not thread-safe, "
                                         + "so Saxon multithreading should be disabled by setting "
                                         + "Feature.ALLOW_MULTITHREADING to false",
                                              SaxonErrorCode.SXSQ0005).asWarning());

        }


        // Establish the JDBC connection

        Connection connection = null;      // JDBC Database Connection

        MapItem options = (MapItem) arguments[0].head();

        String dataSource = str(options.get(new StringValue("dataSource")));
        String dbString = str(options.get(new StringValue("database")));
        String dbDriverString = str(options.get(new StringValue("driver")));
        String userString = str(options.get(new StringValue("user")));
        String pwdString = str(options.get(new StringValue("password")));
        String autoCommitString = str(options.get(new StringValue("autoCommit")));

//        try {
//            // the following hack is necessary to load JDBC drivers
//            Class.forName(dbDriverString);
//        } catch (ClassNotFoundException e) {
//            XPathException err = new XPathException("Failed to load JDBC driver " + dbDriverString, e);
//            err.setXPathContext(context);
//            err.setErrorCode(SaxonErrorCode.SXSQ0003);
//            throw err;
//        }
        try {
            if (context.getConfiguration().isTiming()) {
                DriverManager.setLogWriter(new PrintWriter(System.err));
            }
            if (dataSource != null) {
                InitialContext namingContext = new InitialContext();
                DataSource ds = (DataSource) namingContext.lookup(dataSource);
                if (userString != null && pwdString != null) {
                    connection = ds.getConnection(userString, pwdString);
                } else {
                    connection = ds.getConnection();
                }
            } else {
                if (userString == null && pwdString == null) {
                    connection = DriverManager.getConnection(dbString);
                } else {
                    connection = DriverManager.getConnection(dbString, userString, pwdString);
                }
            }
        } catch (NamingException nex) {
            if (context.getConfiguration().isTiming()) {
                nex.printStackTrace();
            }
            throw new XPathException("JDBC Connection Failure: failure from naming service", nex)
                    .withXPathContext(context)
                    .withErrorCode(SaxonErrorCode.SXSQ0003);
        } catch (SQLException ex) {
            if (context.getConfiguration().isTiming()) {
                ex.printStackTrace();
            }
            throw new XPathException("JDBC Connection Failure", ex)
                    .withXPathContext(context)
                    .withErrorCode(SaxonErrorCode.SXSQ0003);
        }

        try {
            if (!autoCommitString.isEmpty()) {
                connection.setAutoCommit("yes".equals(autoCommitString));
            }
        } catch (SQLException e) {
            throw new XPathException("Failed to set autoCommit on JDBC connection " + dbDriverString, e)
                    .withXPathContext(context)
                    .withErrorCode(SaxonErrorCode.SXSQ0003);
        }

        return new ObjectValue<>(connection);

    }

    private String str(Sequence seq) throws XPathException {
        if (seq == null) {
            return null;
        }
        Item item = seq.head();
        return item == null ? "" : item.getStringValue();
    }


}



// Copyright (c) 2018-2023 Saxonica Limited
