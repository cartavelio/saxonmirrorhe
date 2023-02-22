using System;

namespace Saxon.Api {
    /// <summary>
    /// An <c>XQueryExecutable</c> represents the compiled form of a query. To execute the query,
    /// it must first be loaded to form an <c>XQueryEvaluator</c>.
    /// </summary>
    /// <remarks>
    /// <para>An <c>XQueryExecutable</c> is immutable, and therefore thread-safe. It is simplest to
    /// load a new <c>XQueryEvaluator</c> each time the query is to be run. However, the 
    /// <c>XQueryEvaluator</c> is serially reusable within a single thread.</para>
    /// <para>An <c>XQueryExecutable</c> is created by using one of the <c>Compile</c>
    /// methods on the <c>XQueryCompiler</c> class.</para>
    /// </remarks>    
    [Serializable]
    public class XQueryExecutable {
        private Saxon.Hej.s9api.XQueryExecutable executable;

        // internal constructor

        internal XQueryExecutable(Saxon.Hej.s9api.XQueryExecutable exec) {
            this.executable = exec;
        }

        /// <summary>Ask whether this is an updating query (that is, one that returns a pending
        /// update list rather than a conventional value).</summary>

        public bool IsUpdateQuery => executable.isUpdateQuery();

        /// <summary>Escape-hatch method to get the underlying Saxon implementation object if required.
        /// This provides access to methods that may not be stable from release to release.</summary>
        internal Saxon.Hej.s9api.XQueryExecutable getUnderlyingCompiledQuery() {
            return executable;
        }

        /// <summary>
        /// Load the query to prepare it for execution.
        /// </summary>
        /// <returns>
        /// An <c>XQueryEvaluator</c>. The returned <c>XQueryEvaluator</c> can be used to
        /// set up the dynamic context for query evaluation, and to run the query.
        /// </returns>
        public XQueryEvaluator Load() {
            return new XQueryEvaluator(executable.load());
        }
    }
}