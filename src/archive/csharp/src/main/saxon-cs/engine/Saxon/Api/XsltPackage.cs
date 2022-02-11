using System;
using System.IO;
using Saxon.Hej.trace;

namespace Saxon.Api {
    /// <summary> An <c>XsltPackage</c> object represents the result of compiling an XSLT 3.0 package, as
    /// represented by an XML document containing an <c>xsl:package</c> element.</summary>

    [Serializable]
    public class XsltPackage
    {
        private Saxon.Hej.s9api.XsltPackage package;

        // internal constructor: Initializes a new instance of the <see cref="Saxon.Api.XsltPackage"/> class.
        internal XsltPackage(Processor p, Saxon.Hej.s9api.XsltPackage pp)
        {
            package = pp;
            Processor = p;

        }

        /// <summary>
        /// The Processor from which this <c>XsltPackage</c> was constructed
        /// </summary>

        public Processor Processor { get; }


        /// <summary>
        /// Get the name of the package (the URI appearing as the value of <code>xsl:package/@name</code>)
        /// </summary>
        /// <returns>The package name</returns>

        public string PackageName => package.getName();


        /// <summary>Get the version number of the package (the value of the attribute 
        /// <code>xsl:package/@package-version</code></summary>
        /// <returns>The package version number</returns>

        public string Version => package.getVersion();


        /// <summary>Link this package with the packages it uses to form an executable stylesheet. This process fixes
        /// up any cross-package references to files, templates, and other components, and checks to ensure
        /// that all such references are consistent.</summary>
        /// <returns> the resulting <c>XsltExecutable</c></returns>

        public XsltExecutable Link()
        {
            try
            {
                Saxon.Hej.s9api.XsltExecutable jexecutable = package.link();
                XsltExecutable executable = new XsltExecutable(jexecutable);
                executable.InternalProcessor = Processor;
                return executable;
            }
            catch (Saxon.Hej.trans.XPathException e)
            {
                throw new Saxon.Api.SaxonApiException(e);
            }
        }

        /// <summary>Save this compiled package to filestore.</summary>
        /// <param name="stream"> the stream to which the compiled package should be saved</param>

        public void Save(Stream stream)
        {
            ExpressionPresenter outp = new ExpressionPresenter(package.getProcessor().getUnderlyingConfiguration(), new Saxon.Ejavax.xml.transform.stream.StreamResult(stream), true);
            try
            {
                package.getUnderlyingPreparedPackage().export(outp);
            }
            catch (Saxon.Hej.trans.XPathException e)
            {
                throw new Saxon.Api.SaxonApiException(e);
            }
        }


        /// <summary>Escape-hatch interface to the underlying implementation class.</summary>
        /// <returns>the underlying <c>StylesheetPackage</c>. The interface to <c>StylesheetPackage</c>
        /// is not a stable part of the s9api API definition.</returns>

		public Saxon.Hej.style.StylesheetPackage getUnderlyingPreparedPackage()
        {
            return package.getUnderlyingPreparedPackage();
        }

        internal Saxon.Hej.s9api.XsltPackage GetUnderlyingXsltPackage() {
            return package;
        }
        
    }
}

// Copyright (c) 2021 Saxonica Limited