package com.saxonica.functions.sql;

import net.sf.saxon.Configuration;
import net.sf.saxon.expr.Atomizer;
import net.sf.saxon.expr.PJConverter;
import net.sf.saxon.expr.StaticProperty;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.registry.BuiltInFunctionSet;
import net.sf.saxon.ma.map.DictionaryMap;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.ma.map.MapType;
import net.sf.saxon.om.AtomicSequence;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.SaxonErrorCode;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.AnyFunctionType;
import net.sf.saxon.type.AnyItemType;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.JavaExternalObjectType;
import net.sf.saxon.value.AnyExternalObject;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.SequenceType;
import net.sf.saxon.value.StringValue;

import java.sql.*;
import java.util.ArrayList;
import java.util.List;

/**
 * The SQLFunctionSet is a set of Saxon vendor-supplied
 * extension functions designed to enable access to SQL databases
 */

public class SQLFunctionSet extends BuiltInFunctionSet {

    protected Configuration config;

    public SQLFunctionSet() {
    }

    @Override
    public void setConfiguration(Configuration config) {
        this.config = config;
        init();
    }

    public static List<MapItem> sqlQueryResult(ResultSet rs) throws SQLException {
        // -- Get the column names
        ResultSetMetaData metaData = rs.getMetaData();

        // -- Create a sequence of maps containing the result
        List<MapItem> out = new ArrayList<>();
        int icol = rs.getMetaData().getColumnCount();
        while (rs.next()) {                            // next row
            //System.out.print("<- SQL : "+ rowStart);
            DictionaryMap map = new DictionaryMap();
            for (int col = 1; col <= icol; col++) {     // next column
                // Read result from RS only once, because
                // of JDBC-Specifications
                String result = rs.getString(col);
                String sqlName = metaData.getColumnName(col);
                map.initialPut(sqlName, new StringValue(result));
                // TODO: use SQL data type
            }
            out.add(map);
            //System.out.println(rowEnd);
        }
        return out;
    }

    public static void setSqlStatementParam(PreparedStatement ps, int index, Sequence value, XPathContext context) throws XPathException {

        try {
            AtomicSequence as = Atomizer.atomize(value);
            ParameterMetaData metaData = ps.getParameterMetaData();

            if (as.getLength() == 0) {
                ps.setNull(index, JDBCType.NULL.ordinal());
            } else if (as.getLength() == 1) {
                AtomicValue av = as.head();
                String parameterClassName = null;
                try {
                    parameterClassName = metaData.getParameterClassName(index);
                } catch (SQLException ex) {
                    parameterClassName = "java.lang.String";
                }
                Object obj = null;
                switch (parameterClassName) {
                    case "java.lang.String":
                        ps.setString(index, av.getStringValue());
                        return;
//                        obj = av.getStringValue();
//                        break;
                    case "java.sql.Date":
                        obj = Date.valueOf(av.getStringValue());
                        break;
                    default:
                        try {
                            Class targetClass = Class.forName(parameterClassName);
                            PJConverter converter = PJConverter.allocate(context.getConfiguration(), av.getPrimitiveType(), StaticProperty.ALLOWS_ONE, targetClass);
                            obj = converter.convert(av, targetClass, context);
                        } catch (ClassNotFoundException err) {
                            throw new XPathException("Cannot convert value to required class " + parameterClassName);
                        }
                        break;
                }

                ps.setObject(index, obj);

            } else {
                throw new XPathException("Cannot substitute sequence of length " + as.getLength()
                                                 + " into prepared SQL statement");
            }
        } catch (SQLException e) {
            throw new XPathException("Failure supplying argument to prepared SQL statement", e);
        }
    }

    public static void supplyParameters(ParameterMetaData metaData, SequenceType[] parameterTypes) throws SQLException {
        int arity = metaData.getParameterCount();
        for (int i=0; i<arity; i++) {
            String parameterClassName = null;
            try {
                parameterClassName = metaData.getParameterClassName(i+1);
            } catch (SQLException ex) {
                parameterClassName = "java.lang.String";
            }
            switch (parameterClassName) {
                case "java.lang.String":
                default:
                    parameterTypes[i] = SequenceType.SINGLE_STRING;
                    break;
                case "java.lang.Boolean":
                    parameterTypes[i] = SequenceType.SINGLE_BOOLEAN;
                    break;
                case "java.lang.Integer":
                case "java.lang.Long":
                case "java.lang.Short":
                case "java.lang.Byte":
                    parameterTypes[i] = SequenceType.SINGLE_INTEGER;
                    break;
                case "java.lang.Double":
                    parameterTypes[i] = SequenceType.SINGLE_DOUBLE;
                    break;
                case "java.lang.Float":
                    parameterTypes[i] = SequenceType.SINGLE_FLOAT;
                    break;
                case "java.math.BigDecimal":
                    parameterTypes[i] = SequenceType.SINGLE_DECIMAL;
                    break;
            }
        }
    }

    public static Connection expectConnection(Sequence arg, XPathContext context) throws XPathException {
        Item conn = arg.head();
        if (!(conn instanceof AnyExternalObject && ((AnyExternalObject) conn).getWrappedObject() instanceof Connection)) {
            throw new XPathException("Value of connection argument is not a JDBC Connection", SaxonErrorCode.SXSQ0001, context);
        }
        return(Connection) ((AnyExternalObject) conn).getWrappedObject();
    }

    private void init() {

        JavaExternalObjectType result;
        synchronized(config) {
            result = JavaExternalObjectType.of(Connection.class);
        }
        JavaExternalObjectType connectionType = result;

        register("connect", 1, e -> e.populate(SQLConnectFn::new, connectionType, ONE, LATE)
                .arg(0, MapType.ANY_MAP_TYPE, ONE, null));

        register("delete", 3, e -> e.populate(SQLDeleteFn::new, AnyItemType.getInstance(), OPT, SIDE|LATE)
                .arg(0, connectionType, ONE, null)
                .arg(1, BuiltInAtomicType.STRING, ONE, null)
                .arg(2, BuiltInAtomicType.STRING, ONE, null));

        register("execute", 2, e -> e.populate(SQLExecuteFn::new, AnyItemType.getInstance(), OPT, SIDE|LATE)
                .arg(0, connectionType, ONE, null)
                .arg(1, BuiltInAtomicType.STRING, ONE, null));

        register("insert", 3, e -> e.populate(SQLInsertFn::new, AnyItemType.getInstance(), OPT, SIDE|LATE)
                .arg(0, connectionType, ONE, null)
                .arg(1, BuiltInAtomicType.STRING, ONE, null)
                .arg(2, MapType.ANY_MAP_TYPE, ONE, null));

        register("prepared-statement", 2, e -> e.populate(SQLPreparedStatementFn::new, AnyItemType.getInstance(), OPT, LATE)
                .arg(0, connectionType, ONE, null)
                .arg(1, BuiltInAtomicType.STRING, ONE, null));

        register("prepared-query", 2, e -> e.populate(SQLPreparedQueryFn::new, AnyFunctionType.getInstance(), ONE, LATE)
                .arg(0, connectionType, ONE, null)
                .arg(1, BuiltInAtomicType.STRING, ONE, null));

        register("query", 3, e -> e.populate(SQLQueryFn::new, MapType.ANY_MAP_TYPE, STAR, LATE)
                .arg(0, connectionType, ONE, null)
                .arg(1, BuiltInAtomicType.STRING, ONE, null)
                .arg(2, BuiltInAtomicType.STRING, STAR, null));

        register("query", 4, e -> e.populate(SQLQueryFn::new, MapType.ANY_MAP_TYPE, STAR, LATE)
                .arg(0, connectionType, ONE, null)
                .arg(1, BuiltInAtomicType.STRING, ONE, null)
                .arg(2, BuiltInAtomicType.STRING, STAR, null)
                .arg(3, BuiltInAtomicType.STRING, ONE, null));

        register("update", 4, e -> e.populate(SQLUpdateFn::new, AnyItemType.getInstance(), OPT, SIDE|LATE)
                .arg(0, connectionType, ONE, null)
                .arg(1, BuiltInAtomicType.STRING, ONE, null)
                .arg(2, BuiltInAtomicType.STRING, ONE, null)
                .arg(3, MapType.ANY_MAP_TYPE, ONE, null));

    }

    /**
     * Return the namespace URI for the functions local to this function set.
     *
     * @return the namespace URI of the functions local to this function set.
     * Note that functions imported from another function set may have a different
     * namespace URI.
     */
    @Override
    public NamespaceUri getNamespace() {
        return NamespaceUri.SQL;
    }

    @Override
    public String getConventionalPrefix() {
        return "sql";
    }
}

// Copyright (c) 2018-2023 Saxonica Limited