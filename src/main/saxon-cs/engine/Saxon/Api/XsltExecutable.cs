using System;
using System.Collections.Generic;

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

        // private JPreparedStylesheet pss;
        private Processor processor;
        private net.sf.saxon.s9api.XsltExecutable executable;

        // internal constructor

        internal XsltExecutable(net.sf.saxon.s9api.XsltExecutable executable)
        {
            //this.processor = proc;
            this.executable = executable;
        }

        internal Processor InternalProcessor
        { 
            set { processor = value;}

        }

        /// <summary>
        /// Get the Processor that was used to create this XsltExecutable
        /// </summary>
        public Processor Processor
        {
            get { return processor; }

        }

        /// <summary>
        /// Load the stylesheet to prepare it for execution.
        /// </summary>
        /// <returns>
        /// An <c>XsltTransformer</c>. The returned <c>XsltTransformer</c> can be used to
        /// set up the dynamic context for stylesheet evaluation, and to run the stylesheet.
        /// </returns>

        public XsltTransformer Load()
        {
            // JXsltController c = pss.newController();
            return new XsltTransformer(executable.load());
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
            return new Xslt30Transformer(executable.load30());
        }

        /// <summary>
        /// Output an XML representation of the compiled code of the stylesheet, for purposes of 
        /// diagnostics and instrumentation.
        /// </summary>
        /// <param name="destination">The destination for the diagnostic output</param>

        public void Explain(XmlDestination destination)
        {
            executable.explain(destination.GetUnderlyingDestination());
        }

        /// <summary>
        /// Escape hatch to the underlying Java implementation object.
        /// </summary>

        public net.sf.saxon.s9api.XsltExecutable Implementation
        {
            get
            {
                return executable;
            }
        }

        /// <summary>
        /// Get the whitespace stripping policy defined by this stylesheet, that is, the policy
        /// defined by the <c>xsl:strip-space</c> and <c>xsl:preserve-space</c> elements in the stylesheet.
        /// </summary>
        /// <returns> a newly constructed <c>WhitespacePolicy</c> based on the declarations in this
        /// stylesheet. This policy can be used as input to a <see cref="DocumentBuilder"/>.</returns>
        
        public WhitespacePolicy getWhitespaceStrippingPolicy() {
            return new WhitespacePolicy(executable);
        }


        /// <summary>
        /// Get the names of the <c>xsl:param</c> elements defined in this stylesheet, with details
        /// of each parameter including its required type, and whether it is required or optional.
        /// </summary>
        /// <returns>
        /// a <c>Dictionary</c> whose keys are the names of global parameters in the stylesheet,
        /// and whose values are <see cref="Saxon.Api.XsltExecutable+ParameterDetails"/> objects giving information about the
        /// corresponding parameter.
        /// </returns>
        public Dictionary<QName, ParameterDetails> GetGlobalParameters()
        {
            java.util.HashMap globals = executable.getGlobalParameters();
            Dictionary<QName, ParameterDetails> params1 = new Dictionary<QName, ParameterDetails>();
            int count = globals.size();
            object[] names = globals.keySet().toArray();
            for (int i = 0; i < count; i++) {
                net.sf.saxon.s9api.XsltExecutable.ParameterDetails details = (net.sf.saxon.s9api.XsltExecutable.ParameterDetails)globals.get(names[i]);
                XdmSequenceType sType = XdmSequenceType.FromSequenceType(details.getUnderlyingDeclaredType());
                sType.itemType.SetJXdmItemType(details.getDeclaredItemType());
                params1.Add(new QName((net.sf.saxon.s9api.QName)names[i]), new ParameterDetails(sType, false));

            }
            return params1;
        }

        /// <summary>
        /// Information about a global parameter to a stylesheet.
        /// </summary>

        public class ParameterDetails
        {
            private XdmSequenceType type;
            private bool isRequired;

            /// <summary>
            /// Create parameter details.
            /// </summary>
            /// <param name="type">The declared type of the parameter.</param>
            /// <param name="isRequired">Indicates whether the parameter is required or optional.</param>
            
            public ParameterDetails(XdmSequenceType type, bool isRequired)
            {
                this.type = type;
                this.isRequired = isRequired;
            }

            /// <summary>
            /// Gets the declared item type of the parameter.
            /// </summary>
            /// <returns>The type defined in the <code>as</code> attribute of the <code>xsl:param</code> element,
            /// without its occurrence indicator</returns>
            public XdmItemType GetDeclaredItemType()
            {
                return type.itemType;
            }


            /// <summary>
            /// Gets the declared cardinality of the parameter.
            /// </summary>
            /// <returns>The occurrence indicator from the type appearing in the <code>as</code> attribute
            /// of the <code>xsl:param</code> element</returns>

            public char GetDeclaredCardinality()
            {
                return type.occurrenceIn;
            }

            /// <summary>
            /// Gets the underlying declared type of the parameter.
            /// </summary>
            /// <returns>The underlying declared type.</returns>

            public XdmSequenceType GetUnderlyingDeclaredType()
            {
                return type;
            }


            /// <summary>
            /// Ask whether the parameter is required (mandatory) or optional
            /// </summary>
            /// <returns><c>true</c> if the parameter is mandatory (<code>required="yes"</code>), false
            /// if it is optional.</returns>

            public bool IsRequired
            {
                set { this.isRequired = value; }
                get { return this.isRequired; }

            }
        }




    }
}