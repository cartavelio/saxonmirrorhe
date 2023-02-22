
namespace Saxon.Api {
    /// <summary>
    /// An instance of this class will be created by the compiler for each function call to this extension function
    /// that is found in the source code. The class is always instantiated by calling the method <c>MakeFunctionCall()</c>
    /// of the corresponding <c>ExtensionFunctionDefinition</c>. 
    /// The implementation may therefore retain information about the static context of the
    /// call. Once compiled, however, the instance object must be immutable.
    /// </summary>

    public abstract class ExtensionFunctionCall
    {

        /// <summary>
        /// Method called by the compiler (at compile time) to provide information about the static context of the
        /// function call. The implementation may retain this information for use at run-time, if the result of the
        /// function depends on information in the static context.
        /// </summary>
        /// <remarks>
        /// For efficiency, the implementation should only retain copies of the information that it actually needs. It
        /// is not a good idea to hold a reference to the static context itself, since that can result in a great deal of
        /// compile-time information being locked into memory during run-time execution.
        /// </remarks>
        /// <param name="context">Information about the static context in which the function is called</param>

        public virtual void SupplyStaticContext(StaticContext context)
        {
            // default: no action
        }

        /// <summary>
        /// This method is defined in the interface for backwards compatibility. It is never called, and will be
        /// dropped in a future release.
        /// </summary>
        /// <param name="destination">The new extension function object. This will always be an instance of the same
        /// class as the existing object.</param>

        public virtual void CopyLocalData(ExtensionFunctionCall destination) { }

        /// <summary>
        /// Method called at run time to evaluate the function.
        /// </summary>
        /// <param name="arguments">The values of the arguments to the function, supplied as iterators over XPath
        /// sequence values.</param>
        /// <param name="context">The dynamic context for evaluation of the function. This provides access
        /// to the context item, position, and size, and if required to internal data maintained by the Saxon
        /// engine.</param>
        /// <returns>An iterator over a sequence, representing the result of the extension function.
        /// Note that Saxon does not guarantee to read this sequence to completion, so calls on the iterator
        /// must have no side-effects. In rare circumstances (for example, when <code>last()</code> is
        /// used) Saxon may clone the returned iterator by calling its <c>GetAnother()</c> method, 
        /// allowing the function results to be read more than once.</returns>

        public abstract XdmValue Call(XdmValue[] arguments, DynamicContext context);
        
    }
}

// Copyright (c) 2021 Saxonica Limited