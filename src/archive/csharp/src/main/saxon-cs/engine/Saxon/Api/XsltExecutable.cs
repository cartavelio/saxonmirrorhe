using System;
using System.Collections.Generic;
using Saxon.Hej.value;

namespace Saxon.Api {
    /// <summary>
    /// An <c>XsltExecutable</c> represents the compiled form of a stylesheet. To execute the stylesheet,
    /// it must first be loaded to form an <see cref="XsltTransformer"/> or <see cref="Xslt30Transformer"/>.
    /// </summary>
    /// <remarks>
    /// <para>An <c>XsltExecutable</c> is immutable, and therefore thread-safe. It is simplest to
    /// load a new <c>XsltEvaluator</c> each time the stylesheet is to be run. However, the 
    /// <c>XsltEvaluator</c> is serially reusable within a single thread.</para>
    /// <para>An <c>XsltExecutable</c> is created by using one of the <c>Compile</c>
    /// methods on the <see cref="XsltCompiler"/> class.</para>
    /// </remarks>    

    [Serializable]
    public class XsltExecutable
    {

        private Processor processor;
        private Saxon.Hej.s9api.XsltExecutable jExecutable;

        // internal constructor

        internal XsltExecutable(Saxon.Hej.s9api.XsltExecutable executable)
        {
            jExecutable = executable;
        }

        internal Processor InternalProcessor
        { 
            set => processor = value;
        }

        /// <summary>
        /// Get the Processor that was used to create this XsltExecutable
        /// </summary>
        public Processor Processor => processor;

        /// <summary>
        /// Load the stylesheet to prepare it for execution.
        /// </summary>
        /// <returns>
        /// An <c>XsltTransformer</c>. The returned <c>XsltTransformer</c> can be used to
        /// set up the dynamic context for stylesheet evaluation, and to run the stylesheet.
        /// </returns>

        public XsltTransformer Load()
        {
            return new XsltTransformer(jExecutable.load());
        }



        /// <summary>
        /// Load the stylesheet to prepare it for execution. This version of the <c>load()</c> method
        /// creates an <code>Xslt30Transformer</code> which offers interfaces for stylesheet
        /// invocation corresponding to those described in the XSLT 3.0 specification. It can be used
        /// with XSLT 2.0 or XSLT 3.0 stylesheets, and in both cases it offers new XSLT 3.0 functionality such
        /// as the ability to supply parameters to the initial template, or the ability to invoke
        /// stylesheet-defined functions, or the ability to return an arbitrary sequence as a result
        /// without wrapping it in a document node.
        /// </summary>
        /// <returns>
        /// An <c>Xslt30Transformer</c>. The returned <c>Xslt30Transformer</c> can be used to
        /// set up the dynamic context for stylesheet evaluation, and to run the stylesheet.
        /// </returns>

        public Xslt30Transformer Load30()
        {
            return new Xslt30Transformer(jExecutable.load30());
        }

        /// <summary>
        /// Output an XML representation of the compiled code of the stylesheet, for purposes of 
        /// diagnostics and instrumentation.
        /// </summary>
        /// <param name="destination">The destination for the diagnostic output</param>

        public void Explain(IDestination destination)
        {
            jExecutable.explain(destination.UnderlyingDestination);
        }

        /// <summary>
        /// Escape hatch to the underlying Java implementation object.
        /// </summary>

        private Saxon.Hej.s9api.XsltExecutable Implementation => jExecutable;

        /// <summary>
        /// Get the whitespace stripping policy defined by this stylesheet, that is, the policy
        /// defined by the <c>xsl:strip-space</c> and <c>xsl:preserve-space</c> elements in the stylesheet.
        /// </summary>
        /// <returns> a newly constructed <c>WhitespacePolicy</c> based on the declarations in this
        /// stylesheet. This policy can be used as input to a <see cref="DocumentBuilder"/>.</returns>
        
        public WhitespacePolicy getWhitespaceStrippingPolicy() {
            throw new NotImplementedException();
            // TODO: unclear what to do here. Whitespace-stripping policy can vary for each package.
            //return new WhitespacePolicy();
        }


        /// <summary>
        /// Get the names of the <c>xsl:param</c> elements defined in this stylesheet, with details
        /// of each parameter including its required type, and whether it is required or optional.
        /// </summary>
        /// <returns>
        /// a <c>Dictionary</c> whose keys are the names of global parameters in the stylesheet,
        /// and whose values are <see cref="Saxon.Api.ParameterDetails"/> objects giving information about the
        /// corresponding parameter.
        /// </returns>
        public Dictionary<QName, ParameterDetails> GetGlobalParameters()
        {
            Dictionary<QName, Saxon.Hej.s9api.XsltExecutable.ParameterDetails> globals = jExecutable.getGlobalParameters();
            Dictionary<QName, ParameterDetails> params1 = new ();
            foreach (KeyValuePair<QName, Saxon.Hej.s9api.XsltExecutable.ParameterDetails> pair in globals) {
                SequenceType st = pair.Value.getUnderlyingDeclaredType();
                XdmItemType xit = XdmItemType.MakeXdmItemType(st.getPrimaryType(), processor.Implementation);
                char indicator = (Cardinality.getOccurrenceIndicator(st.getCardinality()) + " ")[0];
                XdmSequenceType sType = new (xit, indicator);
                params1.Add(pair.Key, new ParameterDetails(sType, false));
            }
            return params1;
        }
    }
}

// Copyright (c) 2021 Saxonica Limited