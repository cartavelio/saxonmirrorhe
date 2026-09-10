package com.saxonica.functions.sql;

import net.sf.saxon.expr.*;
import net.sf.saxon.expr.parser.OperatorSymbol;
import net.sf.saxon.functions.CallableFunction;
import net.sf.saxon.functions.NotFn;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.FunctionItem;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.ZeroOrMore;
import net.sf.saxon.trans.SaxonErrorCode;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.FunctionItemType;
import net.sf.saxon.type.SpecificFunctionType;
import net.sf.saxon.value.NumericValue;
import net.sf.saxon.value.SequenceType;
import net.sf.saxon.value.StringValue;

import java.sql.*;
import java.util.ArrayList;
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

    @Override
    public FunctionItem call(XPathContext context, Sequence[] arguments) throws XPathException {
        Connection connection = SQLFunctionSet.expectConnection(arguments[0], context);
        String query = arguments[1].head().getStringValue();

        PreparedStatement ps = null;

        try {

            // -- Prepare the SQL statement
            try {
                ps = connection.prepareStatement(query);
            } catch (SQLException e) {
                throw new XPathException("sql:prepared-query() failed: " + e.getMessage() +
                        " Failed to prepare query {" + query + '}',
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
                    return new ZeroOrMore<>(out);
                } catch (SQLException e) {
                    throw new XPathException("Failure executing prepared SQL query", e);
                }
            };

            ParameterMetaData metaData = ps2.getParameterMetaData();
            int arity = metaData.getParameterCount();
            SequenceType[] parameterTypes = new SequenceType[arity];
            SQLFunctionSet.supplyParameters(metaData, parameterTypes);
            FunctionItemType type = new SpecificFunctionType(parameterTypes, SequenceType.MAP_SEQUENCE);
            return new CallableFunction(arity, callable, type);

        } catch (SQLException ex) {
            throw new XPathException("sql:prepared-query() failed: " + ex.getMessage(), SaxonErrorCode.SXSQ0004, context);
        } 

    }

    public static Expression makePreparedQueryFunction(String tableName, Expression predicate) {
        StringBuilder sb = new StringBuilder();
        List<Expression> params = new ArrayList<>();
        boolean ok = makeWhereClause(predicate, sb, params);
        if (ok) {
            String query = "SELECT * FROM " + tableName + " WHERE " + sb.toString();
            Callable callable = new Callable() {
                @Override
                public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
                    return null;
                }
            };
            return null;
        } else {
            return null;
        }
    }


    public static boolean makeWhereClause(Expression predicate, StringBuilder sb, List<Expression> params) {
        boolean ok = false;
        if (predicate instanceof AndExpression) {
            ok = makeWhereClause(((AndExpression) predicate).getLhsExpression(), sb, params);
            if (ok) {
                sb.append(" AND ");
                ok = makeWhereClause(((AndExpression) predicate).getRhsExpression(), sb, params);
            }
        } else if (predicate instanceof OrExpression) {
            ok = makeWhereClause(((OrExpression) predicate).getLhsExpression(), sb, params);
            if (ok) {
                sb.append(" OR ");
                ok = makeWhereClause(((OrExpression) predicate).getRhsExpression(), sb, params);
            }
        } else if (predicate.isCallOn(NotFn.class)) {
            sb.append(" NOT (");
            ok = makeWhereClause(((SystemFunctionCall) predicate).getArg(0), sb, params);
            sb.append(")");
        } else if (predicate instanceof ValueComparison) {
            ok = makeComparisonOperand(((ValueComparison) predicate).getLhsExpression(), sb, params);
            if (ok) {
                sb.append(makeOperator(((ValueComparison) predicate).getOperator()));
                ok = makeComparisonOperand(((ValueComparison) predicate).getRhsExpression(), sb, params);
            }
        }
        return ok;
    }

    private static String makeOperator(OperatorSymbol op) {
        switch (op) {
            case FEQ:
                return "=";
            case FNE:
                return "<>";
            case FLE:
                return "<=";
            case FLT:
                return "<";
            case FGE:
                return ">=";
            case FGT:
                return ">";
            default:
                return "###";
        }
    }

    private static boolean makeComparisonOperand(Expression exp, StringBuilder sb, List<Expression> params) {
        if (exp instanceof Literal) {
            GroundedValue val = ((Literal) exp).getGroundedValue();
            if (val instanceof StringValue) {
                sb.append("'").append(((StringValue) val).getStringValue().replace("'", "\\'")).append("'");
                return true;
            } else if (val instanceof NumericValue) {
                sb.append(((NumericValue) val).getUnicodeStringValue());
                return true;
            }
        }
        if (exp instanceof LookupExpression) {
            LookupExpression lookup = (LookupExpression) exp;
            if ((lookup.getLhsExpression() instanceof ContextItemExpression) &&
                    lookup.getRhsExpression() instanceof StringLiteral) {
                sb.append((((StringLiteral) lookup.getRhsExpression()).getString()));
                return true;
            }
        }
        sb.append(" ? ");
        params.add(exp);
        return true;
    }

}


// Copyright (c) 2018-2023 Saxonica Limited
