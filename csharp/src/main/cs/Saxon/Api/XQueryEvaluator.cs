using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Saxon.Api.Support;
using Saxon.Hej.lib;
using Saxon.Hej.query;
using Saxon.Callbacks;
using Saxon.Ejavax.xml.transform.stream;

namespace Saxon.Api {
    /// <summary inherits="IEnumerable">
    /// An <c>XQueryEvaluator</c> represents a compiled and loaded query ready for execution.
    /// The <c>XQueryEvaluator</c> holds details of the dynamic evaluation context for the query.
    /// </summary>
    /// <remarks>
    /// <para>An <c>XQueryEvaluator</c> must not be used concurrently in multiple threads. It is safe,
    /// however, to reuse the object within a single thread to run the same query several times.
    /// Running the query does not change the context that has been established.</para>
    /// <para>An <c>XQueryEvaluator</c> is always constructed by running the <c>Load</c> method of
    /// an <c>XQueryExecutable</c>.</para>
    /// </remarks>     
    [Serializable]
    public class XQueryEvaluator : IEnumerable<XdmItem> {
        private Saxon.Hej.s9api.XQueryEvaluator evaluator;
        private TextWriter traceFunctionDestination;
        private ResourceResolver textResolver;
        private ResourceResolver xmlDocumentResolver;
        private ErrorReporter errorReporter;
        private System.Action closeAction;


        // internal constructor

        internal XQueryEvaluator(Processor processor, Saxon.Hej.s9api.XQueryEvaluator eval) {
            this.evaluator = eval;
            this.Processor = processor;
        }
        
        /// <summary>
        /// Get the <c>Processor</c> from which this <c>XQueryCompiler</c> was constructed
        /// </summary>

        public Processor Processor { get; }

        /// <summary>
        /// The context item for the query.
        /// </summary>
        /// <remarks> This may be a node, an atomic value, or a function item such as a map or array.
        /// Most commonly it will be a document node, which might be constructed
        /// using a <c>DocumentBuilder</c> created from the <c>Processor</c> object.
        /// </remarks>

        public XdmItem ContextItem {
            get => evaluator.getContextItem();
            set => evaluator.setContextItem(value);
        }

        /// <summary>
        /// The <c>SchemaValidationMode</c> to be used in this query, especially for documents
        /// loaded using the <c>doc()</c>, <c>document()</c>, or <c>collection()</c> functions.
        /// </summary>
        /// <remarks>
        /// This does not affect any document supplied as the context item for the query, or as the values
        /// of external variables.
        /// </remarks>

        public SchemaValidationMode SchemaValidationMode {
            get {
                switch (evaluator.getUnderlyingQueryContext().getSchemaValidationMode()) {
                    case Validation.STRICT:
                        return SchemaValidationMode.Strict;
                    case Validation.LAX:
                        return SchemaValidationMode.Lax;
                    case Validation.STRIP:
                        return SchemaValidationMode.None;
                    case Validation.PRESERVE:
                        return SchemaValidationMode.Preserve;
                    case Validation.DEFAULT:
                    default:
                        return SchemaValidationMode.Unspecified;
                }
            }

            set {
                DynamicQueryContext context = evaluator.getUnderlyingQueryContext();
                switch (value) {
                    case SchemaValidationMode.Strict:
                        context.setSchemaValidationMode(Validation.STRICT);
                        break;
                    case SchemaValidationMode.Lax:
                        context.setSchemaValidationMode(Validation.LAX);
                        break;
                    case SchemaValidationMode.None:
                        context.setSchemaValidationMode(Validation.STRIP);
                        break;
                    case SchemaValidationMode.Preserve:
                        context.setSchemaValidationMode(Validation.PRESERVE);
                        break;
                    case SchemaValidationMode.Unspecified:
                    default:
                        context.setSchemaValidationMode(Validation.DEFAULT);
                        break;
                }
            }
        }

        /// <summary>
        /// The <code>ResourceResolver</code> to be used at run-time to resolve and dereference URIs
        /// supplied to the <c>fn:doc()</c> and <c>fn:doc-available()</c> functions, and other functions
        /// that fetch XML resources such as <c>fn:transform</c>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The URI supplied as the first argument to the resolver is the absolute URI requested,
        /// that is, the result of resolving the requested relative URI against the relevant base URI.
        /// </para>
        /// <para>The requested relative URI and the corresponding base URI are supplied as separate
        /// properties within the <c>ResourceProperties</c> argument, in case they are needed.
        /// </para>
        /// <para>The returned <c>IResource</c> must be one of the following:
        /// <list>
        /// <item>An <c>IResource</c> such as a <c>NodeResource</c> or an <c>XmlReaderResource</c>
        /// whose <c>getItem()</c> method delivers an <c>XdmNode</c>;</item>
        /// <item>A <c>TextResource</c> delivering lexical XML in its <c>Content</c> property; this
        /// will then be parsed to deliver the corresponding XML document.</item>
        /// </list>
        /// </para>
        /// <para>If no <c>XmlDocumentResolver</c> is supplied, or if it returns null, then
        /// the <c>CommonResourceResolver</c> associated with the Saxon Configuration is invoked.
        /// </para>
        /// </remarks>

        public ResourceResolver XmlDocumentResolver {
            get => xmlDocumentResolver;
            set {
                xmlDocumentResolver = value;
                evaluator.setResourceResolver(
                    new JResourceResolverWrappingCS(value, evaluator.getUnderlyingQueryContext().getConfiguration()));
            }
        }

        /// <summary>
        /// The <code>TextResolver</code> to be used at run-time to resolve and dereference URIs
        /// supplied to the <c>unparsed-text()</c>, <c>unparsed-text-lines()</c>,
        /// <c>unparsed-text-available()</c>, and <c>json-doc()</c> functions.
        /// </summary>

        public ResourceResolver TextResolver {
            get => textResolver;
            set {
                textResolver = value;
                evaluator.setUnparsedTextResolver(Xslt30Transformer.WrapTextResolver(value));
            }
        }
        
        /// <summary>
        /// The <code>ErrorReporter</code> to be used at run-time to notify any dynamic
        /// errors that occur.
        /// </summary>

        public ErrorReporter ErrorReporter {
            get => errorReporter;
            set {
                errorReporter = value;
                evaluator.setErrorReporter(new ErrorReporterWrapper(value));
            }
        }

        /// <summary>
        /// Set the value of an external variable declared in the query.
        /// </summary>
        /// <param name="name">The name of the external variable, expressed
        /// as a <c>QName</c>. If an external variable of this name has been declared in the
        /// query prolog, the given value will be assigned to the variable. If the
        /// variable has not been declared, calling this method has no effect (it is
        /// not an error).</param>
        /// <param name="value">The value to be given to the external variable.
        /// If the variable declaration defines a required type for the variable, then
        /// this value must match the required type: no conversions are applied.</param>
        
        public void SetExternalVariable(QName name, XdmValue value) {
            evaluator.setExternalVariable(name, value);
        }

        /// <summary>
        /// Destination for output of messages produced using the <c>trace()</c> function. 
        /// </summary>
        /// <remarks>
        /// <para>If no specific destination is supplied by the caller, message information will be written to
        /// the standard error stream.</para>
        /// <para>The supplied destination is ignored if a <c>TraceListener</c> is in use.</para>
        /// <para>The <c>TextWriter</c> should be closed by the caller after use.</para>
        /// </remarks>

		public TextWriter TraceFunctionDestination {
            set {
                traceFunctionDestination = value;
                evaluator.setTraceFunctionDestination(new StandardLogger(value));
            }
            get => traceFunctionDestination;
        }


        /// <summary>
        /// Evaluate the query, returning the result as an <c>XdmValue</c> (that is,
        /// a sequence of nodes and/or atomic values).
        /// </summary>
        /// <returns>
        /// An <c>XdmValue</c> representing the results of the query
        /// </returns>
        /// <exception cref="SaxonApiException">Throws a <c>SaxonApiException</c> if any run-time failure
        /// occurs while evaluating the query.</exception>
        public XdmValue Evaluate() {
            return evaluator.evaluate();
        }

        /// <summary>
        /// Evaluate the query, returning the result as an <c>XdmItem</c> (that is,
        /// a single node or atomic value).
        /// </summary>
        /// <returns>
        /// An <c>XdmItem</c> representing the result of the query, or null if the query
        /// returns an empty sequence. If the query returns a sequence of more than one item,
        /// any items after the first are ignored.
        /// </returns>
        /// <exception cref="SaxonApiException">Throws a <c>DynamicError</c> if any run-time failure
        /// occurs while evaluating the expression.</exception>
        public XdmItem EvaluateSingle() {
            return evaluator.evaluateSingle();
        }

        /// <summary>
        /// Evaluate the query, returning the result as an <c>IEnumerator</c> (that is,
        /// an enumerator over a sequence of nodes and/or atomic values).
        /// </summary>
        /// <returns>
        /// An enumerator over the sequence that represents the results of the query.
        /// Each object in this sequence will be an instance of <c>XdmItem</c>. Note
        /// that the query may be evaluated lazily, which means that a successful response
        /// from this method does not imply that the query has executed successfully: failures
        /// may be reported later while retrieving items from the iterator. 
        /// </returns>
        /// <exception cref="SaxonApiException">Throws a <c>SaxonApiException</c> if any run-time failure
        /// occurs while evaluating the expression.</exception>
        public IEnumerator GetEnumerator() {
            return evaluator.GetEnumerator();
        }
        
        /// <summary>
        /// Evaluate the query, returning the result as an <c>IEnumerator</c> (that is,
        /// an enumerator over a sequence of nodes and/or atomic values).
        /// </summary>
        /// <returns>
        /// An enumerator over the sequence that represents the results of the query.
        /// Each object in this sequence will be an instance of <c>XdmItem</c>. Note
        /// that the query may be evaluated lazily, which means that a successful response
        /// from this method does not imply that the query has executed successfully: failures
        /// may be reported later while retrieving items from the iterator. 
        /// </returns>
        /// <exception cref="Saxon.Api.SaxonApiException">Throws a <c>SaxonApiException</c> if any run-time failure
        /// occurs while evaluating the expression.</exception>

        IEnumerator<XdmItem> IEnumerable<XdmItem>.GetEnumerator() {
            return evaluator.GetEnumerator();
        }


        /// <summary>
        /// Evaluate the query, sending the result to a specified destination.
        /// </summary>
        /// <param name="destination">
        /// The destination for the results of the query. The class <c>XmlDestination</c>
        /// is an abstraction that allows a number of different kinds of destination
        /// to be specified.
        /// </param>
        /// <exception cref="SaxonApiException">Throws a <c>DynamicError</c> if any run-time failure
        /// occurs while evaluating the expression.</exception>
        public void Run(IDestination destination) {
            evaluator.setDestination(destination.UnderlyingDestination);
            evaluator.run();
            closeAction?.Invoke();
        }

        /// <summary>
        /// Execute an updating query.
        /// </summary>
        /// <returns>An array containing the root nodes of documents that have been
        /// updated by the query.</returns>
        /// <exception cref="SaxonApiException">Throws a <c>SaxonApiException</c> if any run-time failure
        /// occurs while evaluating the expression, or if the expression is not an
        /// updating query.</exception>
        public XdmNode[] RunUpdate() {
            evaluator.run();
            IEnumerator<Saxon.Api.XdmNode> updatedDocsIter = evaluator.getUpdatedDocuments();
            List<XdmNode> resultList = new List<XdmNode>();

            for (; updatedDocsIter.MoveNext();) {
                resultList.Add(updatedDocsIter.Current);
            }

            XdmNode[] result = resultList.ToArray();
            return result;
        }

        /// <summary>
        /// Execute a streamed query.
        /// </summary>
        /// <param name="input">The input stream, which must contain a well-formed XML document</param>
        /// <param name="baseUri">The base URI of the document supplied on the input stream</param>
        /// <param name="destination">The destination to which the query result will be sent</param>
        /// <exception cref="SaxonApiException">Throws a <c>SaxonApiException</c> if any run-time failure
        /// occurs while evaluating the query, or if the query execution cannot be streamed.</exception>

        public void RunStreamed(Stream input, Uri baseUri, IDestination destination) {
            StreamSource source = new (input, baseUri.ToString());
            Implementation.runStreamed(source, destination.UnderlyingDestination);
        }

        /// <summary>
        /// Call a global user-defined function in the compiled query.
        /// </summary>
        /// <remarks>
        /// If this is called more than once (to evaluate the same function repeatedly with different arguments,
        /// or to evaluate different functions) then the sequence of evaluations uses the same values of global
        /// variables including external variables (query parameters); the effect of any changes made to query parameters
        /// between calls is undefined.
        /// </remarks>
        /// <param name="function">
        /// The name of the function to be called
        /// </param>
        /// <param name="arguments">
        /// The values of the arguments to be supplied to the function. These
        /// must be of the correct type as defined in the function signature (there is no automatic
        /// conversion to the required type).
        /// </param>
        /// <exception cref="ArgumentException">If no function has been defined with the given name and arity
        /// or if any of the arguments does not match its required type according to the function
        /// signature.</exception>
        /// <exception cref="SaxonApiException">If a dynamic error occurs in evaluating the function.
        /// </exception>
        public XdmValue CallFunction(QName function, params XdmValue[] arguments) {
            return evaluator.callFunction(function, arguments);
        }


        /// <summary>
        /// Escape hatch to the <c>Saxon.Hej.s9api.XQueryEvaluator</c> object in the underlying implementation
        /// </summary>

        private Saxon.Hej.s9api.XQueryEvaluator Implementation => evaluator;
    }
}

// Copyright (c) 2021 - 2023 Saxonica Limited