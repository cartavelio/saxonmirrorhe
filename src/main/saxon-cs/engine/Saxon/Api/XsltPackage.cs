using System;
using System.IO;
using net.sf.saxon.style;
using net.sf.saxon.trace;

namespace Saxon.Api {
    /// <summary> An <c>XsltPackage</c> object represents the result of compiling an XSLT 3.0 package, as
    /// represented by an XML document containing an <c>xsl:package</c> element.</summary>

    [Serializable]
    public class XsltPackage
    {
        private net.sf.saxon.s9api.XsltPackage package;
        private Processor processor;
        // internal constructor: Initializes a new instance of the <see cref="Saxon.Api.XsltPackage"/> class.
        internal XsltPackage(Processor p, net.sf.saxon.s9api.XsltPackage pp)
        {
            this.package = pp;
            this.processor = p;

        }

        /// <summary>
        /// Get the Processor from which this <c>XsltPackage</c> was constructed
        /// </summary>

        public Processor Processor
        {
            get { return processor; }
        }


        /// <summary>
        /// Get the name of the package (the URI appearing as the value of <code>xsl:package/@name</code>)
        /// </summary>
        /// <returns>The package name</returns>

        public String PackageName
        {
            get { return package.getName(); }
        }


        /// <summary>Get the version number of the package (the value of the attribute 
        /// <code>xsl:package/@package-version</code></summary>
        /// <returns>The package version number</returns>

        public String Version
        {
            get { return package.getVersion(); }
        }


        /// <summary>Link this package with the packages it uses to form an executable stylesheet. This process fixes
        /// up any cross-package references to files, templates, and other components, and checks to ensure
        /// that all such references are consistent.</summary>
        /// <returns> the resulting <c>XsltExecutable</c></returns>

        public XsltExecutable Link()
        {
            try
            {
                net.sf.saxon.s9api.XsltExecutable jexecutable = package.link();
                XsltExecutable executable = new XsltExecutable(jexecutable);
                executable.InternalProcessor = processor;
                return executable;
            }
            catch (net.sf.saxon.trans.XPathException e)
            {
                throw new StaticError(e);
            }
        }

        /// <summary>Save this compiled package to filestore.</summary>
        /// <param name="stream"> the stream to which the compiled package should be saved</param>

        public void Save(Stream stream)
        {
            ExpressionPresenter outp = new ExpressionPresenter(package.getProcessor().getUnderlyingConfiguration(), new javax.xml.transform.stream.StreamResult(stream), true);
            try
            {
                package.getUnderlyingPreparedPackage().export(outp);
            }
            catch (net.sf.saxon.trans.XPathException e)
            {
                throw new StaticError(e);
            }
        }


        /// <summary>Escape-hatch interface to the underlying implementation class.</summary>
        /// <returns>the underlying <c>StylesheetPackage</c>. The interface to <c>StylesheetPackage</c>
        /// is not a stable part of the s9api API definition.</returns>

        public StylesheetPackage getUnderlyingPreparedPackage()
        {
            return package.getUnderlyingPreparedPackage();
        }

        internal net.sf.saxon.s9api.XsltPackage GetUnderlyingXsltPackage() {
            return package;
        }
        
    }
}