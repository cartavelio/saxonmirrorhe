using System;
using Saxon.Hej.trans;

namespace Saxon.Api {
    /// <summary>
    /// The class <c>StaticContext</c> provides information about the static context of an expression.
    /// </summary>
    /// <remarks>
    /// <para>The class is used to supply information about the static context of an extension function call,
    /// allowing the result of an extension function to depend on aspects of the static context
    /// such as the default collation.</para>
    /// </remarks>

    public class StaticContext {
        internal StaticContext(Saxon.Hej.expr.StaticContext jsc) {
            Implementation = jsc;
        }


        /// <summary>
        /// The URI of the module where an expression appears, suitable for use in diagnostics
        /// </summary>

        public Uri ModuleUri => new Uri(Implementation.getSystemId());


        /// <summary>
        /// The static base URI of the expression. Often the same as the URI of the containing module,
        /// but not necessarily so, for example in a stylesheet that uses external XML entities or the
        /// <c>xml:base</c> attribute
        /// </summary>

        public Uri BaseUri => new(Implementation.getStaticBaseURI());

        /// <summary>
        /// Resolve an in-scope namespace prefix to obtain the corresponding namespace URI. If the prefix
        /// is a zero-length string, the default namespace for elements and types is returned.
        /// </summary>
        /// <param name="Prefix">The namespace prefix</param>
        /// <returns>The corresponding namespace URI if there is one, or null otherwise</returns>

        public string GetNamespaceForPrefix(string Prefix) {
            if (Prefix == "") {
                return Implementation.getDefaultElementNamespace().ToString();
            }
            try {
                return Implementation.getNamespaceResolver().getURIForPrefix(Prefix, false).ToString();
            } catch (XPathException) {
                return null;
            }
        }

        /// <summary>
        /// The <c>Processor</c> that was used to create the query or stylesheet from which this extension
        /// function was invoked.
        /// </summary>
        /// <remarks>
        /// <para>This property is useful if the extension function wishes to create new nodes (the <code>Processor</code>
        /// can be used to obtain a <code>DocumentBuilder</code>), or to execute XPath expressions or queries.</para>
        /// <para>There may be circumstances in which the <c>Processor</c> is not available, in which case this method
        /// may return null, or may return a different <c>Processor</c>. This will happen only if low-level interfaces
        /// have been used to cause a <c>Configuration</c> to be shared between several <c>Processor</c> instances,
        /// or between a <c>Processor</c> and other applications.</para>
        /// </remarks>

        public Processor Processor => (object)Implementation.getConfiguration().getProcessor() is Processor processor ? processor : null;

        /// <summary>
        /// The underlying object in the Saxon implementation, an instance of class
        /// <code>Saxon.Hej.expr.StaticContext</code>
        /// </summary>
        /// <remarks>
        /// <para>This property provides access to internal methods in the Saxon engine that are
        /// not specifically exposed in the .NET API. In general these methods should be
        /// considered to be less stable than the classes in the Saxon.Api namespace.</para> 
        /// <para>The internal methods follow
        /// Java naming conventions rather than .NET conventions.</para>
        /// <para>Information about the returned object (and the objects it provides access to)
        /// is included in the Saxon JavaDoc documentation, see <see cref="Saxon.Hej.expr.StaticContext"/>.
        /// </para>
        /// </remarks>

        public Saxon.Hej.expr.StaticContext Implementation { get; }
    }
}

// Copyright (c) 2021 Saxonica Limited