namespace Saxon.Api {
    /// <summary>
    /// This is an interface for simple external/extension functions.
    /// Users can implement this interface and register the implementation with the <see cref="Saxon.Api.Processor"/>;
    /// the function will then be available for calling from all queries, stylesheets, and XPath expressions compiled
    /// under this Processor.
    /// </summary>
    /// <remarks>
    /// <para>Extension functions implemented using this interface are expected to be free of side-effects,
    /// and to have no dependencies on the static or dynamic context. A richer interface for extension
    /// functions is provided via the <see cref="Saxon.Api.ExtensionFunctionDefinition"/> class.</para>
    /// </remarks>

    public interface ExtensionFunction
    {

        /// <summary>
        /// Return the name of the external function
        /// </summary>
        /// <returns>the name of the function, as a <c>QName</c></returns>

        /**public**/ 
        QName GetName();


        /// <summary>
        /// Declare the result type of the external function
        /// </summary>
        /// <returns>the result type of the external function</returns>

        /**public**/ 
        XdmSequenceType GetResultType();

        /// <summary>
        /// Declare the type of the arguments
        /// </summary>
        /// <returns>an array of <c>XdmSequenceType</c> objects, one for each argument to the function,
        /// representing the expected types of the arguments</returns>

        /**public**/ 
        XdmSequenceType[] GetArgumentTypes();


        /// <summary>
        /// Method called at run time to evaluate the function.
        /// </summary>
        /// <param name="arguments">The values of the arguments supplied in the XPath function call.</param>
        /// <returns>An <c>XdmValue</c>, representing the result of the extension function. 
        /// (Note: in many cases it will be convenient to return an <c>XdmAtomicValue</c> or <c>XdmNode</c>, 
        /// both of which are instances of <c>XdmValue</c>).</returns>

        /**public**/ 
        XdmValue Call(XdmValue[] arguments);
    }
}