using System;
using System.Collections.Generic;
using Saxon.Hej.expr.instruct;
using Saxon.Hej.om;
using Saxon.Hej.value;

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

        internal XQueryExecutable(Processor processor, Saxon.Hej.s9api.XQueryExecutable exec) {
            this.Processor = processor;
            this.executable = exec;
        }
        
        /// <summary>
        /// Get the <c>Processor</c> from which this <c>XQueryExecutable</c> was constructed
        /// </summary>

        public Processor Processor { get; }

        /// <summary>Ask whether this is an updating query (that is, one that returns a pending
        /// update list rather than a conventional value).</summary>

        public bool IsUpdateQuery => executable.isUpdateQuery();
        
        /// <summary>
        /// Get information about the external variables declared in the query
        /// </summary>

        public IDictionary<QName, ParameterDetails> GetExternalVariables() {
            Saxon.Hej.query.XQueryExpression expression = executable.getUnderlyingCompiledQuery();
            IEnumerable<GlobalVariable> globalVariables = expression.getMainModule().getAllGlobalVariables();
            Dictionary<QName, ParameterDetails> result = new();
            foreach (GlobalVariable globalVariable in globalVariables) 
            {
                if (globalVariable is GlobalParam) {
                    StructuredQName name = globalVariable.getVariableQName();
                    bool isRequired = globalVariable.isRequiredParam();
                    SequenceType type = globalVariable.getRequiredType();
                    ParameterDetails parameterDetails = new (XdmSequenceType.FromSequenceType(type), isRequired) {
                        Location = new Location(globalVariable.getLocation())
                    };
                    result[new QName(name)] = parameterDetails;
                }
            }

            return result;
        }


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
            return new XQueryEvaluator(Processor, executable.load());
        }
    }
}

// Copyright (c) 2021 Saxonica Limited