package net.sf.saxon.functions.registry;

import net.sf.saxon.expr.Expression;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.trans.XPathException;

import java.util.Arrays;
import java.util.Map;

public abstract class DeclaredFunction {

    /**
     * Get the name of the function
     * @return the function name
     */
    public abstract StructuredQName getFunctionName();

    public abstract int getNumberOfArguments();

    /**
     * Get the name (keyword) of the Nth parameter
     *
     * @return the expression for computing the value of the Nth parameter
     */
    public abstract StructuredQName getParameterName(int i);

    /**
     * Get the default value expression of the Nth parameter, if any
     *
     * @return the expression for computing the value of the Nth parameter, or null if there is none
     */

    public abstract Expression getDefaultValueExpression(int i);

    /**
     * Get the position in the parameter list of a given parameter name
     *
     * @param name the name of the required parameter
     * @return the position of the parameter in the parameter list, or -1 if absent
     */

    public abstract int getPositionOfParameter(StructuredQName name);

    /**
     * Combine supplied positional and keyword arguments into a single array of positional arguments,
     * completing the array using the default value expression for any unsupplied arguments
     *
     * @param arguments the expressions supplied in the function call, including both positional and keyword arguments,
     *                  in the order supplied
     * @param keywords  a list of keywords used in the function call, with the 0-based position at which each keyword was used;
     *                  or null if no keywords were used
     * @param fd        the XQuery function definition, used to identify the position in the list of declared parameters
     *                  at which a particular parameter name appears
     * @return the expanded list of positional parameters
     * @throws XPathException in the event of duplicate or undeclared keywords, or keywords that duplicate positional
     *                                 arguments
     */

    public static Expression[] makeExpandedArgumentArray(
            Expression[] arguments, Map<StructuredQName, Integer> keywords, DeclaredFunction fd) throws XPathException {
        // 4.0: handle keyword arguments and default arguments
        Expression[] expandedArgs;
        int maxArity = fd.getNumberOfArguments();
        // If there are keyword arguments, reposition them to the correct position in the argument sequence
        if (keywords != null) {
            expandedArgs = new Expression[maxArity];
            int positionalArgs = arguments.length - keywords.size();
            System.arraycopy(arguments, 0, expandedArgs, 0, positionalArgs);
            for (Map.Entry<StructuredQName, Integer> entry : keywords.entrySet()) {
                StructuredQName key = entry.getKey();
                int argPos = entry.getValue();
                int paramPos = fd.getPositionOfParameter(key);
                if (paramPos < 0) {
                    throw new XPathException("Keyword " + key +
                                                     " does not match the name of any declared parameter of function " + fd.getFunctionName());
                }
                if (paramPos < positionalArgs) {
                    throw new XPathException("Parameter " + key + " of function " + fd.getFunctionName() +
                                                     " is supplied both by position and by keyword");
                }
                Expression supplied = arguments[argPos];
                expandedArgs[paramPos] = supplied;
            }
        } else {
            expandedArgs = Arrays.copyOf(arguments, maxArity);
        }
        for (int a = 0; a < maxArity; a++) {
            if (expandedArgs[a] == null) {
                Expression defaultValueExpression = fd.getDefaultValueExpression(a);
                if (defaultValueExpression == null) {
                    throw new XPathException("No value supplied for mandatory function parameter " + fd.getParameterName(a)
                    + " of function " + fd.getFunctionName());
                }
                expandedArgs[a] = defaultValueExpression;
            }
        }
        return expandedArgs;
    }

}

