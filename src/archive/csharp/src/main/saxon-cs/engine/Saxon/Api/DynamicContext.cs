using Saxon.Hej.expr;

namespace Saxon.Api {
    /// <summary>
    /// The class <c>DynamicContext</c> provides information about the dynamic context of an expression
    /// </summary>

    public class DynamicContext {

        internal readonly XPathContext context;

        internal DynamicContext(XPathContext context) {
            this.context = context;
        }

        /// <summary>
        /// The context item. May be null if no context item is defined
        /// </summary>

        public XdmItem ContextItem => context.getContextItem() == null ? null : (XdmItem)XdmValue.Wrap(context.getContextItem());

        /// <summary>
        /// The context position (equivalent to the XPath <c>position()</c> function).
        /// </summary>
        /// <remarks>Calling this method throws an exception if the context item is undefined.</remarks>

        public int ContextPosition => context.getCurrentIterator().position();

        /// <summary>
        /// The context size (equivalent to the XPath <c>last()</c> function).
        /// </summary>
        /// <remarks>Calling this method throws an exception if the context item is undefined.</remarks>

        public int ContextSize => context.getLast();

        /// <summary>
        /// The underlying object in the Saxon implementation, an instance of class
        /// <code>Saxon.Hej.expr.XPathContext</code>
        /// </summary>
        /// <remarks>
        /// <para>This property provides access to internal methods in the Saxon engine that are
        /// not specifically exposed in the .NET API. In general these methods should be
        /// considered to be less stable than the classes in the Saxon.Api namespace.</para> 
        /// <para>The internal methods follow
        /// Java naming conventions rather than .NET conventions.</para>
        /// <para>Information about the returned object (and the objects it provides access to)
        /// is included in the Saxon JavaDoc documentation, see <see cref="Saxon.Hej.expr.XPathContext"/>
        /// </para>
        /// </remarks>

		public Saxon.Hej.expr.XPathContext Implementation => context;
    }
}

// Copyright (c) 2021 Saxonica Limited