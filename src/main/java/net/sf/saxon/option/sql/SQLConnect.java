////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.option.sql;

import net.sf.saxon.Configuration;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.parser.RebindingMap;
import net.sf.saxon.om.*;
import net.sf.saxon.style.Compilation;
import net.sf.saxon.style.ComponentDeclaration;
import net.sf.saxon.style.ExtensionInstruction;
import net.sf.saxon.trans.SaxonErrorCode;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.ObjectValue;

import java.io.PrintWriter;
import java.sql.Connection;
import java.sql.DriverManager;
import java.sql.SQLException;
import java.util.regex.Pattern;

/**
 * An sql:connect element in the stylesheet.
 */

public class SQLConnect extends ExtensionInstruction {

    Expression database;
    Expression driver;
    Expression user;
    Expression password;
    Expression autoCommit = Literal.makeEmptySequence();

    @Override
    protected boolean mayContainSequenceConstructor() {
        return false;
    }

    @Override
    protected void prepareAttributes() {

        // Get mandatory database attribute

        AttributeMap atts = attributes();
        AttributeInfo dbAtt = atts.get(NamespaceUri.NULL, "database");
        String dbName = "";
        if (dbAtt == null) {
            reportAbsence("database");
        } else {
            dbName = dbAtt.getValue();
        }
        database = makeAttributeValueTemplate(dbName, dbAtt);

        // Get driver attribute

        AttributeInfo dbDriver = atts.get(NamespaceUri.NULL, "driver");
        String dbDriverName = null;
        if (dbDriver == null) {
            if (dbName.length() > 9 && dbName.substring(0, 9).equals("jdbc:odbc")) {
                dbDriverName = "sun.jdbc.odbc.JdbcOdbcDriver";
            } else {
                //reportAbsence("driver");
            }
        } else {
            dbDriverName = dbDriver.getValue();
        }
        if (dbDriverName != null) {
            driver = makeAttributeValueTemplate(dbDriverName, dbDriver);
        } else {
            driver = Literal.makeEmptySequence();
        }

        // Get user attribute

        AttributeInfo dbUser = atts.get(NamespaceUri.NULL, "user");
        String dbUserName = null;
        if (dbUser == null) {
            reportAbsence("user");
        } else {
            dbUserName = dbUser.getValue();
        }
        user = makeAttributeValueTemplate(dbUserName, dbUser);

        // Get password attribute

        AttributeInfo dbPass = atts.get(NamespaceUri.NULL, "password");
        String dbPassword = null;
        if (dbPass == null) {
            reportAbsence("password");
        } else {
            dbPassword = dbPass.getValue();
        }
        password = makeAttributeValueTemplate(dbPassword, dbPass);

        // Get auto-commit attribute

        AttributeInfo dbAutoCommit = atts.get(NamespaceUri.NULL, "auto-commit");
        if (dbAutoCommit != null) {
            String dbAuto = dbAutoCommit.getValue();
            autoCommit =  makeAttributeValueTemplate(dbAuto, dbAutoCommit);
        }
    }

    @Override
    public void validate(ComponentDeclaration decl) throws XPathException {
        super.validate(decl);
        getConfiguration().checkLicensedFeature(Configuration.LicenseFeature.PROFESSIONAL_EDITION,
                                                "sql:connect",
                                                getPackageData().getLocalLicenseId());
        database = typeCheck("database", database);
        driver = typeCheck("driver", driver);
        user = typeCheck("user", user);
        password = typeCheck("password", password);
        autoCommit = typeCheck("auto-commit", autoCommit);
    }

    @Override
    public Expression compile(Compilation exec, ComponentDeclaration decl) throws XPathException {
        return new ConnectInstruction(database, driver, user, password, autoCommit);
    }

    public static class ConnectInstruction extends SimpleExpression {

        public static final int DATABASE = 0;
        public static final int DRIVER = 1;
        public static final int USER = 2;
        public static final int PASSWORD = 3;
        public static final int AUTOCOMMIT = 4;

        private ConnectInstruction() {
            // provided for the benefit of copy()
        }

        public ConnectInstruction(Expression database,
                                  Expression driver, Expression user, Expression password, Expression autoCommit) {

            Expression[] subs = {database, driver, user, password, autoCommit};
            setArguments(subs);
        }

        /**
         * A subclass must provide one of the methods evaluateItem(), iterate(), or process().
         * This method indicates which of the three is provided.
         */

        @Override
        public int getImplementationMethod() {
            return Expression.EVALUATE_METHOD;
        }

        @Override
        protected int computeCardinality() {
            return StaticProperty.EXACTLY_ONE;
        }

        @Override
        public String getExpressionType() {
            return "sql:connect";
        }

        @Override
        public ConnectInstruction copy(RebindingMap rebindings) {
            return (ConnectInstruction)new ConnectInstruction().copyOperandsFrom(this);
        }

        @Override
        public ObjectValue<Connection> call(XPathContext context, Sequence[] arguments) throws XPathException {

            // Establish the JDBC connection

            Connection connection = null;      // JDBC Database Connection

            String dbString = str(arguments[DATABASE]);
            String dbDriverString = str(arguments[DRIVER]);
            String userString = str(arguments[USER]);
            String pwdString = str(arguments[PASSWORD]);
            String autoCommitString = str(arguments[AUTOCOMMIT]);

            try {
                // the following hack is necessary to load JDBC drivers
                Class.forName(dbDriverString);
            } catch (ClassNotFoundException e) {
                XPathException err = new XPathException("Failed to load JDBC driver " + dbDriverString, e);
                err.setXPathContext(context);
                err.setErrorCode(SaxonErrorCode.SXSQ0003);
                err.setLocation(getLocation());
                throw err;
            }
            try {
                if (context.getConfiguration().isTiming()) {
                    DriverManager.setLogWriter(new PrintWriter(System.err));
                }
                connection = DriverManager.getConnection(dbString, userString, pwdString);
            } catch (SQLException ex) {
                XPathException err = new XPathException("JDBC Connection Failure", ex);
                err.setXPathContext(context);
                err.setErrorCode(SaxonErrorCode.SXSQ0003);
                err.setLocation(getLocation());
                throw err;
            }

            try {
                if (autoCommitString.length() > 0) {
                    connection.setAutoCommit("yes".equals(autoCommitString));
                }
            } catch (SQLException e) {
                XPathException err = new XPathException("Failed to set autoCommit on JDBC connection " + dbDriverString, e);
                err.setXPathContext(context);
                err.setErrorCode(SaxonErrorCode.SXSQ0003);
                err.setLocation(getLocation());
                throw err;
            }

            return new ObjectValue<>(connection);

        }

        private String str(/*@NotNull*/ Sequence iterator) throws XPathException {
            Item item = iterator.head();
            return item == null ? "" : item.getStringValue();
        }
    }

    /**
     * Utility method to quote a SQL table or column name if it needs quoting.
     *
     * @param name the supplied name
     * @return the supplied name, enclosed in double quotes if it does not satisfy the pattern [A-Za-z_][A-Za-z0-9_]*,
     *         with any double quotes replaced by two double quotes
     */

    public static String quoteSqlName(String name) {
        if (namePattern.matcher(name).matches()) {
            return name;
        }
        // Surround the string with quotes, and replace any embedded '"' with '""'.
        return "\"" + name.replace("\"", "\"\"") + "\"";
    }

    private static final Pattern namePattern = Pattern.compile("\"[^\"]+\"|[A-Za-z_][A-Za-z0-9_]*");
}

