package com.saxonica.functions.sql;

import net.sf.saxon.expr.Atomizer;
import net.sf.saxon.expr.PJConverter;
import net.sf.saxon.expr.StaticProperty;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.ma.map.KeyValuePair;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.AtomicSequence;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.SaxonErrorCode;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.EmptySequence;

import java.sql.Connection;
import java.sql.ParameterMetaData;
import java.sql.PreparedStatement;
import java.sql.SQLException;
import java.util.ArrayList;
import java.util.List;


/**
 * This class implements the {@code sql:update()} extension function. Arguments:
 * <ol>
 *     <li>The connection</li>
 *     <li>The table name</li>
 *     <li>A predicate selecting the rows to be updated</li>
 *     <li>A map whose keys are column names (as strings) and whose values are the corresponding values to be updated</li>
 * </ol>
 */

public class SQLUpdateFn extends SystemFunction {

    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        Connection connection = SQLFunctionSet.expectConnection(arguments[0], context);
        String tableName = arguments[1].head().getStringValue();
        String predicate = arguments[2].head().getStringValue();
        MapItem columns = (MapItem) arguments[3].head();


        StringBuilder statement = new StringBuilder(120);
        statement.append("UPDATE ").append(tableName).append(" SET ");

        List<AtomicValue> values = new ArrayList<>();

        int cols = 0;
        for (KeyValuePair kvp : columns.keyValuePairs()) {
            if (cols++ > 0) {
                statement.append(',');
            }
            String colname = kvp.key.getStringValue();
            statement.append(colname);
            statement.append("=?");
            AtomicSequence as = Atomizer.atomize(kvp.value);
            if (as.getLength() != 1) {
                throw new XPathException("sql:update - new value must be a singleton - actual length = " + as.getLength());
            }
            values.add(as.head());
        }

        try (PreparedStatement ps = connection.prepareStatement(statement.toString())) {

            ParameterMetaData metaData = ps.getParameterMetaData();

            for (int c = 0; c < values.size(); c++) {
                AtomicValue v = values.get(c);
                String parameterClassName = metaData.getParameterClassName(c + 1);
                Object value;
                switch (parameterClassName) {
                    case "java.lang.String":
                        value = v.getStringValue();
                        break;
                    case "java.sql.Date":
                        value = java.sql.Date.valueOf(v.getStringValue());
                        break;
                    default:
                        try {
                            Class targetClass = Class.forName(parameterClassName);
                            PJConverter converter = PJConverter.allocate(context.getConfiguration(), v.getPrimitiveType(), StaticProperty.ALLOWS_ONE, targetClass);
                            value = converter.convert(v, targetClass, context);
                        } catch (ClassNotFoundException err) {
                            throw new XPathException("sql:update() - cannot convert value to required class " + parameterClassName);
                        }
                        break;
                }
                //System.err.println("Set statement parameter " + i + " to " + val);
                ps.setObject(c + 1, value);

            }

            ps.executeUpdate();
            if (!connection.getAutoCommit()) {
                connection.commit();
            }

        } catch (SQLException ex) {
            throw new XPathException("sql:update() failed: " + ex.getMessage(), SaxonErrorCode.SXSQ0004, context);
        }

        return EmptySequence.getInstance();

    }
}


// Copyright (c) 2018-2022 Saxonica Limited
