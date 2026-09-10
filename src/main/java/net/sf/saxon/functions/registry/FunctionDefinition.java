package net.sf.saxon.functions.registry;

import net.sf.saxon.expr.Expression;
import net.sf.saxon.om.StructuredQName;

public interface FunctionDefinition {

    /**
     * Get the name of the function
     * @return the function name
     */
    StructuredQName getFunctionName();

    /**
     * Get the number of declared parameters (the upper bound of the arity range)
     * @return the number of declared parameters
     */

    int getNumberOfParameters();

    /**
     * Get the number of mandatory arguments (the lower bound of the arity range)
     * @return the number of mandatory arguments
     */

    int getMinimumArity();

    /**
     * Get the name (keyword) of the Nth parameter
     *
     * @return the expression for computing the value of the Nth parameter
     */
    StructuredQName getParameterName(int i);

    /**
     * Get the default value expression of the Nth parameter, if any
     *
     * @return the expression for computing the value of the Nth parameter, or null if there is none
     */

    Expression getDefaultValueExpression(int i);

    /**
     * Get the position in the parameter list of a given parameter name
     *
     * @param name the name of the required parameter
     * @return the position of the parameter in the parameter list, or -1 if absent
     */

    int getPositionOfParameter(StructuredQName name);

}

