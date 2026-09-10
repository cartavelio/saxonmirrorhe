using System;
using System.Collections;
using System.Xml;
using Saxon.Hej.trans;
using Saxon.Callbacks;

namespace Saxon.Api {
    /// <summary inherits="IEnumerable">
    /// An <c>XPathSelector</c> represents a compiled and loaded XPath expression ready for execution.
    /// The <c>XPathSelector</c> holds details of the dynamic evaluation context for the XPath expression.
    /// </summary>
    /// <remarks>
    /// <para>An <c>XPathSelector</c> should not be used concurrently in multiple threads. It is safe,
    /// however, to reuse the object within a single thread to evaluate the same XPath expression several times.
    /// Evaluating the expression does not change the context that has been established.</para>
    /// <para>An <c>XPathSelector</c> is always constructed by running the <c>Load</c> method of
    /// an <c>XPathExecutable</c>.</para>
    /// <para>The class <c>XPathSelector</c> implements <c>IEnumerable</c>, so it is possible to
    /// enumerate the results in a <c>for</c> expression.</para>
    /// </remarks>     
    [Serializable]
    public class XPathSelector : IEnumerable {
        Saxon.Hej.s9api.XPathSelector selector;

        // internal constructor

        internal XPathSelector(Saxon.Hej.s9api.XPathSelector selector) {
            this.selector = selector;
        }

        /// <summary>
        /// The context item for the XPath expression evaluation.
        /// </summary>
        /// <remarks> This may be either a node or an atomic
        /// value. Most commonly it will be a document node, which might be constructed
        /// using the <c>Build</c> method of the <c>DocumentBuilder</c> object.
        /// </remarks>

        public XdmItem ContextItem {
            get => selector.getContextItem();
            set {
                if (value == null) {
                    throw new ArgumentException("contextItem is null");
                }

                try {
                    selector.setContextItem(value);
                }
                catch (Saxon.Hej.trans.XPathException err) {
                    throw new Saxon.Api.SaxonApiException(err);
                }
            }
        }

        /// <summary>
        /// Escape hatch to the <c>Saxon.Hej.s9api.XPathSelector</c> object in the underlying Java implementation
        /// </summary>

        public Saxon.Hej.sxpath.XPathDynamicContext Implementation => selector.getUnderlyingXPathContext();

        /// <summary>
        /// Set the value of a variable
        /// </summary>
        /// <param name="name">The name of the variable. This must match the name of a variable
        /// that was declared to the <c>XPathCompiler</c>. No error occurs if the expression does not
        /// actually reference a variable with this name.</param>
        /// <param name="value">The value to be given to the variable.</param>
        public void SetVariable(QName name, XdmValue value) {
            try {
                selector.setVariable(name, value);
            }
            catch (XPathException err) {
                throw new Saxon.Api.SaxonApiException(err);
            }
        }

        /// <summary>
        /// The <code>XmlResolver</code> to be used at run-time to resolve and dereference URIs
        /// supplied to the <c>doc()</c> function.
        /// </summary>

        public XmlResolver InputXmlResolver {
            get => ((StandardURIResolver) selector.getURIResolver()).getXmlResolver();
            set => selector.setURIResolver(
                new StandardURIResolver(
                    selector.getUnderlyingXPathContext().getXPathContextObject().getConfiguration(), 
                    value));
        }

        /// <summary>
        /// Evaluate the expression, returning the result as an <c>XdmValue</c> (that is,
        /// a sequence of nodes, atomic values, and possibly function items such as maps and arrays).
        /// </summary>
        /// <remarks>
        /// Although a singleton result <i>may</i> be represented as an <c>XdmItem</c>, there is
        /// no guarantee that this will always be the case. If you know that the expression will return at
        /// most one node or atomic value, it is best to use the <c>EvaluateSingle</c> method, which 
        /// does guarantee that an <c>XdmItem</c> (or null) will be returned.
        /// </remarks>
        /// <returns>
        /// An <c>XdmValue</c> representing the results of the expression. 
        /// </returns>
        /// <exception cref="SaxonApiException">
        /// Throws <c>Saxon.Api.SaxonApiException</c> if the evaluation of the XPath expression fails
        /// with a dynamic error.
        /// </exception>
        public XdmValue Evaluate() {
            return selector.evaluate();
        }

        /// <summary>
        /// Evaluate the XPath expression, returning the result as an <c>XdmItem</c> (that is,
        /// a single node or atomic value).
        /// </summary>
        /// <returns>
        /// An <c>XdmItem</c> representing the result of the expression, or null if the expression
        /// returns an empty sequence. If the expression returns a sequence of more than one item,
        /// any items after the first are ignored.
        /// </returns>
        /// <exception cref="SaxonApiException">
        /// Throws <c>Saxon.Api.DynamicError</c> if the evaluation of the XPath expression fails
        /// with a dynamic error.
        /// </exception>
        public XdmItem EvaluateSingle() {
            return selector.evaluateSingle();
        }

        /// <summary>
        /// Evaluate the effective boolean value of the XPath expression, returning the result as a <c>Boolean</c>
        /// </summary>
        /// <returns>
        /// A <c>Boolean</c> representing the result of the expression, converted to its
        /// effective boolean value as if by applying the XPath <c>boolean()</c> function
        /// </returns>
        /// <exception cref="SaxonApiException">
        /// Throws <c>Saxon.Api.DynamicError</c> if the evaluation of the XPath expression fails
        /// with a dynamic error.
        /// </exception>
        public bool EffectiveBooleanValue() {
            return selector.effectiveBooleanValue();
        }


        /// <summary>
        /// Evaluate the expression, returning the result as an <c>IEnumerator</c> (that is,
        /// an enumerator over a sequence of nodes and/or atomic values).
        /// </summary>
        /// <returns>
        /// An enumerator over the sequence that represents the results of the expression.
        /// Each object in this sequence will be an instance of <c>XdmItem</c>. Note
        /// that the expression may be evaluated lazily, which means that a successful response
        /// from this method does not imply that the expression has executed successfully: failures
        /// may be reported later while retrieving items from the iterator. 
        /// </returns>
        /// <exception cref="SaxonApiException">
        /// May throw a <c>Saxon.Api.DynamicError</c> if the evaluation of the XPath expression fails
        /// with a dynamic error. However, some errors will not be detected during the invocation of this
        /// method, but only when stepping through the returned <c>SequenceEnumerator</c>.
        /// </exception>
        public IEnumerator GetEnumerator() {
            return selector.GetEnumerator();
        }
    }
}

// Copyright (c) 2021 Saxonica Limited